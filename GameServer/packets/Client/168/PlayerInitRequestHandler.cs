/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DOL.Events;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PlayerInitRequest, "Region Entering Init Request", eClientStatus.PlayerInGame)]
	public class PlayerInitRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			new PlayerInitRequestAction(client.Player).Start(1);
		}

		#endregion

		#region Nested type: PlayerInitRequestAction

		/// <summary>
		/// Handles player init requests
		/// </summary>
		protected class PlayerInitRequestAction : RegionAction
		{
			/// <summary>
			/// Constructs a new PlayerInitRequestHandler
			/// </summary>
			/// <param name="actionSource"></param>
			public PlayerInitRequestAction(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				player.Out.SendUpdatePoints();
				player.TargetObject = null;
				// update the region color scheme which may be wrong due to ALLOW_ALL_REALMS support
				player.Out.SendRegionColorScheme();
				if (player.CurrentRegion != null)
				{
					player.CurrentRegion.Notify(RegionEvent.PlayerEnter, player.CurrentRegion, new RegionPlayerEventArgs(player));
				}

				int mobs = SendMobsAndMobEquipmentToPlayer(player);
				player.Out.SendTime();
				WeatherMgr.UpdatePlayerWeather(player);

				bool checkInstanceLogin = false;

				if (!player.EnteredGame)
				{
					player.EnteredGame = true;
					player.Notify(GamePlayerEvent.GameEntered, player);
					NotifyFriendsOfLoginIfNotAnonymous(player);
					player.EffectList.RestoreAllEffects();
					checkInstanceLogin = true;
				}
				else
				{
					player.Notify(GamePlayerEvent.RegionChanged, player);
				}
				if (player.TempProperties.getProperty(GamePlayer.RELEASING_PROPERTY, false))
				{
					player.TempProperties.removeProperty(GamePlayer.RELEASING_PROPERTY);
					player.Notify(GamePlayerEvent.Revive, player);
					player.Notify(GamePlayerEvent.Released, player);
				}
				if (player.Group != null)
				{
					player.Group.UpdateGroupWindow();
					player.Group.UpdateAllToMember(player, true, false);
					player.Group.UpdateMember(player, true, true);
				}
				player.Out.SendPlayerInitFinished(0);
				player.TargetObject = null;
				player.StartHealthRegeneration();
				player.StartPowerRegeneration();
				player.StartEnduranceRegeneration();
				player.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);

				if (player.Guild != null)
				{
					SendGuildMessagesToPlayer(player);
				}
				SendHouseRentRemindersToPlayer(player);
				if (player.Level > 1 && Properties.MOTD != "")
				{
					player.Out.SendMessage(Properties.MOTD, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (player.Level == 1)
				{
					player.Out.SendStarterHelp();
					if (Properties.STARTING_MSG != "")
						player.Out.SendMessage(Properties.STARTING_MSG, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}

				if (Properties.ENABLE_DEBUG)
					player.Out.SendMessage("Server is running in DEBUG mode!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				player.Out.SendPlayerFreeLevelUpdate();
				if (player.FreeLevelState == 2)
				{
					player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true,
					                         LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.FreeLevel"));
				}
				player.Out.SendMasterLevelWindow(0);
				AssemblyName an = Assembly.GetExecutingAssembly().GetName();
				player.Out.SendMessage("Dawn of Light " + an.Name + " Version: " + an.Version, eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);


				if (Properties.TELEPORT_LOGIN_NEAR_ENEMY_KEEP)
				{
					CheckIfPlayerLogsNearEnemyKeepAndMoveIfNecessary(player);
				}

				if (Properties.TELEPORT_LOGIN_BG_LEVEL_EXCEEDED)
				{
					CheckBGLevelCapForPlayerAndMoveIfNecessary(player);
				}

				if (checkInstanceLogin)
				{
					if (WorldMgr.Regions[player.CurrentRegionID] == null || player.CurrentRegion == null || player.CurrentRegion.IsInstance)
					{
						Log.WarnFormat("{0}:{1} logging into instance or CurrentRegion is null, moving to bind!", player.Name, player.Client.Account.Name);
						player.MoveToBind();
					}
				}

				if (player.IsUnderwater)
				{
					player.IsDiving = true;
				}
				player.Client.ClientState = GameClient.eClientState.Playing;
			}

			private static void NotifyFriendsOfLoginIfNotAnonymous(GamePlayer player)
			{
				if (!player.IsAnonymous)
				{
					var friendList = new[] {player.Name};
					foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
					{
						if (pclient.Player.Friends.Contains(player.Name))
							pclient.Out.SendAddFriends(friendList);
					}
				}
			}

			private static void CheckBGLevelCapForPlayerAndMoveIfNecessary(GamePlayer player)
			{
				if (player.Client.Account.PrivLevel == 1 && player.CurrentRegion.IsRvR && player.CurrentRegionID != 163)
				{
					ICollection<AbstractGameKeep> list = GameServer.KeepManager.GetKeepsOfRegion(player.CurrentRegionID);


					foreach (AbstractGameKeep k in list)
					{
						if (k.BaseLevel >= 50)
							continue;

						if (player.Level > k.BaseLevel)
						{
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.LevelCap"),
							                       eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
							player.MoveTo((ushort) player.BindRegion, player.BindXpos,
							              player.BindYpos, player.BindZpos,
							              (ushort) player.BindHeading);
							break;
						}
					}
				}
			}

			private static void CheckIfPlayerLogsNearEnemyKeepAndMoveIfNecessary(GamePlayer player)
			{
				if (player.CurrentRegion.IsInstance)
				{
					if (WorldMgr.RvRLinkDeadPlayers.ContainsKey(player.InternalID))
					{
						WorldMgr.RvRLinkDeadPlayers.Remove(player.InternalID);
					}
					return;
				}

				int gracePeriodInMinutes = 0;
				Int32.TryParse(Properties.RVR_LINK_DEATH_RELOG_GRACE_PERIOD, out gracePeriodInMinutes);
				AbstractGameKeep keep = GameServer.KeepManager.GetKeepCloseToSpot(player.CurrentRegionID, player, WorldMgr.VISIBILITY_DISTANCE);
				if (keep != null && player.Client.Account.PrivLevel == 1 && GameServer.KeepManager.IsEnemy(keep, player))
				{
					if (WorldMgr.RvRLinkDeadPlayers.ContainsKey(player.InternalID))
					{
						if (DateTime.Now.Subtract(new TimeSpan(0, gracePeriodInMinutes, 0)) <= WorldMgr.RvRLinkDeadPlayers[player.InternalID])
						{
							SendMessageAndMoveToSafeLocation(player);
						}
					}
					else
					{
						SendMessageAndMoveToSafeLocation(player);
					}
				}
				var linkDeadPlayerIds = new string[WorldMgr.RvRLinkDeadPlayers.Count];
				WorldMgr.RvRLinkDeadPlayers.Keys.CopyTo(linkDeadPlayerIds, 0);
				foreach (string playerId in linkDeadPlayerIds)
				{
					if (playerId != null &&
					    DateTime.Now.Subtract(new TimeSpan(0, gracePeriodInMinutes, 0)) > WorldMgr.RvRLinkDeadPlayers[playerId])
					{
						WorldMgr.RvRLinkDeadPlayers.Remove(playerId);
					}
				}
			}

			private static void SendMessageAndMoveToSafeLocation(GamePlayer player)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.SaferLocation"),
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.MoveTo((ushort) player.BindRegion, player.BindXpos,
				              player.BindYpos, player.BindZpos,
				              (ushort) player.BindHeading);
			}

			private static void SendHouseRentRemindersToPlayer(GamePlayer player)
			{
				House house = HouseMgr.GetHouseByPlayer(player);

				if (house != null)
				{
					TimeSpan due = (house.LastPaid.AddDays(Properties.RENT_DUE_DAYS).AddHours(1) - DateTime.Now);
					if ((due.Days <= 0 || due.Days < Properties.RENT_DUE_DAYS) && house.KeptMoney < HouseMgr.GetRentByModel(house.Model))
						player.Out.SendRentReminder(house);
				}

				if (player.Guild != null)
				{
					House ghouse = HouseMgr.GetGuildHouseByPlayer(player);
					if (ghouse != null)
					{
						TimeSpan due = (ghouse.LastPaid.AddDays(Properties.RENT_DUE_DAYS).AddHours(1) - DateTime.Now);
						if ((due.Days <= 0 || due.Days < Properties.RENT_DUE_DAYS) && ghouse.KeptMoney < HouseMgr.GetRentByModel(ghouse.Model))
							player.Out.SendRentReminder(ghouse);
					}
				}
			}

			private static void SendGuildMessagesToPlayer(GamePlayer player)
			{
				try
				{
					if (player.GuildRank.GcHear && player.Guild.Motd != "")
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.GuildMessage"),
						                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
						player.Out.SendMessage(player.Guild.Motd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					if (player.GuildRank.OcHear && player.Guild.Omotd != "")
					{
						player.Out.SendMessage(
							LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.OfficerMessage", player.Guild.Omotd),
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					if (player.Guild.alliance != null && player.GuildRank.AcHear && player.Guild.alliance.Dballiance.Motd != "")
					{
						player.Out.SendMessage(
							LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerInitRequestHandler.AllianceMessage",
							                           player.Guild.alliance.Dballiance.Motd), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				catch (Exception ex)
				{
					Log.Error("SendGuildMessageToPlayer exception, missing guild ranks for guild: " + player.Guild.Name + "?", ex);
					if (player != null)
					{
						player.Out.SendMessage(
							"There was an error sending motd for your guild. Guild ranks may be missing or corrupted.",
							eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
			}

			private static int SendMobsAndMobEquipmentToPlayer(GamePlayer player)
			{
				int mobs = 0;

				if (player.CurrentRegion != null)
				{
					var npcs = player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE).Cast<GameNPC>().ToArray();
					foreach (GameNPC npc in npcs)
					{
						player.Out.SendNPCCreate(npc);
						mobs++;
						if (npc.Inventory != null)
							player.Out.SendLivingEquipmentUpdate(npc);
					}
				}

				return mobs;
			}
		}

		#endregion
	}
}