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
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using log4net;

namespace DOL.GS.PacketHandler.Client
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

					if (player.Level > 1 && Properties.MOTD != "")
					{
						player.Out.SendMessage(Properties.MOTD, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else if (player.Level == 1 && Properties.STARTING_MSG != "")
					{
						player.Out.SendMessage(Properties.STARTING_MSG, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
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
				if (player.PlayerGroup != null)
				{
					player.PlayerGroup.UpdateGroupWindow();
					player.PlayerGroup.UpdateAllToMember(player, true, false);
					player.PlayerGroup.UpdateMember(player, true, true);
				}

				player.Out.SendPlayerInitFinished((byte)mobs);
				player.TargetObject = null;
//				player.Out.SendChangeTarget(null); // don't work like expected - prints "can't assist..."

				//Start the healing if we are not at full health/mana/endurance
				player.StartHealthRegeneration();
				player.StartPowerRegeneration();
				player.StartEnduranceRegeneration();

				player.SetPvPInvulnerability(10*1000, null);

				AssemblyName an = Assembly.GetExecutingAssembly().GetName();
				player.Out.SendMessage("Dawn of Light " + an.Name + " Version: " + an.Version, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				//we check here if we are near any enemy keeps
				AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(player.CurrentRegionID, player, WorldMgr.VISIBILITY_DISTANCE);
				if (keep != null && player.Client.Account.PrivLevel == 1 && keep.Realm != player.Realm)
				{
					int keepid = 1, x = 0, y = 0, z = 0;
					ushort heading = 0;
					switch (player.Realm)
					{
						case 1: keepid = 1; break;
						case 2: keepid = 3; break;
						case 3: keepid = 5; break;
					}
					KeepMgr.GetBorderKeepLocation(keepid, out x, out y, out z, out heading);
					if (x != 0)
					{
						player.Out.SendMessage("This area is unsafe, moving you to a safe location!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
						player.MoveTo(163, x, y, z, heading);
					}
				}

				if (player.IsUnderwater)
					player.IsDiving = true;

				player.Client.ClientState = GameClient.eClientState.Playing;
			}
		}
	}
}
