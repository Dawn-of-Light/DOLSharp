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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;

using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x40^168,"Handles player init replies")]
	public class PlayerInitRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client == null || client.Player == null) return 1;
			new PlayerInitRequestAction(client.Player).Start(1);
			return 1;
		}

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
				GamePlayer player = (GamePlayer)m_actionSource;

				//TODO send more initialisation
				// 0xD6 - Time
				// 0x3A - ?

				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0}({1}) entering world: pid->{2} oid->{3}", player.Client.Account.Name, player.Name, player.Client.SessionID, player.ObjectID);
				//If put here it works, in 0x7C-World Init alone not ... *sigh*

				player.Out.SendUpdatePoints();

				player.TargetObject = null;
//				player.AddToWorld(); // won't work because player is already in the region

				player.LastNPCUpdate = Environment.TickCount; 

				player.CurrentUpdateArray.SetAll(false);
				//KeepMgr.broadcastKeeps(player.Player);

				player.CurrentRegion.Notify(RegionEvent.PlayerEnter, player.CurrentRegion, new RegionPlayerEventArgs(player));

				//Send npcs in view a 0x72 message
				int mobs = 0;
				foreach (GameNPC npc in player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendNPCCreate(npc);
					mobs++;
					if (npc.Inventory != null)
						player.Out.SendLivingEquipmentUpdate(npc);
					player.CurrentUpdateArray[npc.ObjectID - 1] = true;

					//The following line can cause a racing condition
					//between client and server! Not neccessary
					//player.Out.SendNPCUpdate(npc); <-- BIG NO NO!
				}

				player.Out.SendTime();
				//Update the weather for the player
				WeatherMgr.UpdatePlayerWeather(player);

				if (!player.EnteredGame)
				{
					player.EnteredGame = true;
					player.Notify(GamePlayerEvent.GameEntered, player);

					if (!player.IsAnonymous)
					{
						string[] friendList = new string[] {player.Name};
						foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
						{
							if(pclient.Player.Friends.Contains(player.Name))
								pclient.Out.SendAddFriends(friendList);
						}
					}
					player.EffectList.RestoreAllEffects();
				}
				else
				{
					player.Notify(GamePlayerEvent.RegionChanged, player);
				}

				if (player.TempProperties.getProperty(GamePlayer.RELEASING_PROPERTY, false))
				{
					player.TempProperties.removeProperty(GamePlayer.RELEASING_PROPERTY);
					// fire the player revive event
					player.Notify(GamePlayerEvent.Revive, player);
					player.Notify(GamePlayerEvent.Released, player);
				}

				// important for zoning
				if (player.Group != null)
				{
					player.Group.UpdateGroupWindow();
					player.Group.UpdateAllToMember(player, true, false);
					player.Group.UpdateMember(player, true, true);
				}

				player.Out.SendPlayerInitFinished((byte)mobs);
				player.TargetObject = null;
//				player.Out.SendChangeTarget(null); // don't work like expected - prints "can't assist..."

				//Start the healing if we are not at full health/mana/endurance
				player.StartHealthRegeneration();
				player.StartPowerRegeneration();
				player.StartEnduranceRegeneration();

				player.SetPvPInvulnerability(10*1000, null);

				/*
				 * update points
				 * guild message
				 * officer message
				 * alliance message
				 * housing rent help
				 * server message
				 * you have entered message
				 * freelevel update
				 */

				if (player.Guild != null)
				{
					if (player.GuildRank.GcHear && player.Guild.theGuildDB.Motd != "")
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.GuildMessage"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						player.Out.SendMessage(player.Guild.theGuildDB.Motd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					if (player.GuildRank.OcHear && player.Guild.theGuildDB.oMotd != "")
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.OfficerMessage", player.Guild.theGuildDB.oMotd), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					if (player.Guild.alliance != null && player.GuildRank.AcHear && player.Guild.alliance.Dballiance.Motd != "")
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.AllianceMessage", player.Guild.theGuildDB.oMotd), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}

				House house = HouseMgr.GetRealHouseByPlayer(player);
				if (house != null)
				{
					TimeSpan due = (house.LastPaid.AddDays(7).AddHours(1) - DateTime.Now);
					if (due.Days < 7 && house.KeptMoney < HouseMgr.GetRentByModel(house.Model))
						player.Out.SendRentReminder(house);
				}

                if (player.Guild != null)
                {
                    House ghouse = HouseMgr.GetGuildHouseByPlayer(player);
                    if (ghouse != null)
                    {
                        TimeSpan due = (ghouse.LastPaid.AddDays(7).AddHours(1) - DateTime.Now);
                        if (due.Days < 7 && ghouse.KeptMoney < HouseMgr.GetRentByModel(ghouse.Model))
                            player.Out.SendRentReminder(ghouse);
                    }
                }

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

                player.Out.SendPlayerFreeLevelUpdate();
			    
				if (player.FreeLevelState == 2)
				{
					player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.FreeLevel"));
				}
				
				player.Out.SendMasterLevelWindow(0);
				
				AssemblyName an = Assembly.GetExecutingAssembly().GetName();
				player.Out.SendMessage("Dawn of Light " + an.Name + " Version: " + an.Version, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				//we check here if we are near any enemy keeps
				AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(player.CurrentRegionID, player, WorldMgr.VISIBILITY_DISTANCE);
				if (keep != null && player.Client.Account.PrivLevel == 1 && KeepMgr.IsEnemy(keep, player))
				{
					/*
					int keepid = 1, x = 0, y = 0, z = 0;
					ushort heading = 0;
					switch (player.Realm)
					{
						case 1: keepid = 1; break;
						case 2: keepid = 3; break;
						case 3: keepid = 5; break;
					}
					KeepMgr.GetBorderKeepLocation(keepid, out x, out y, out z, out heading);
					 */

					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.SaferLocation"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.MoveTo((ushort)player.PlayerCharacter.BindRegion, player.PlayerCharacter.BindXpos, player.PlayerCharacter.BindYpos, player.PlayerCharacter.BindZpos, (ushort)player.PlayerCharacter.BindHeading);
				}

				if (player.Client.Account.PrivLevel == 1 && player.CurrentRegion.IsRvR && player.CurrentRegionID != 163)
				{
					ICollection<AbstractGameKeep> list = KeepMgr.GetKeepsOfRegion(player.CurrentRegionID);


					foreach (AbstractGameKeep k in list)
					{
						if (k.BaseLevel >= 50) continue;

						if (player.Level > k.BaseLevel)
						{
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PlayerInitRequestHandler.LevelCap"), eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
							player.MoveTo((ushort)player.PlayerCharacter.BindRegion, player.PlayerCharacter.BindXpos, player.PlayerCharacter.BindYpos, player.PlayerCharacter.BindZpos, (ushort)player.PlayerCharacter.BindHeading);
							break;
						}
					}
				}
				
				if (player.IsUnderwater)
					player.IsDiving = true;

				player.Client.ClientState = GameClient.eClientState.Playing;
			}
		}
	}
}
