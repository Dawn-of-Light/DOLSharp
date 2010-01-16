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
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x7C^168,"Handles world init replies")]
	public class WorldInitRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			client.UdpConfirm = false;
			new WorldInitAction(client.Player).Start(1);
			return 1;
		}

		/// <summary>
		/// Handles player world init requests
		/// </summary>
		protected class WorldInitAction : RegionAction
		{
			/// <summary>
			/// Constructs a new WorldInitAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public WorldInitAction(GamePlayer actionSource) : base(actionSource)
			{
			}


			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				if(player==null) return;
				//check emblems at world load before any updates
				if(player.Inventory!=null) lock (player.Inventory)
				{
					Guild playerGuild = player.Guild;
					foreach(InventoryItem myitem in player.Inventory.AllItems)
					{
						if (myitem != null && myitem.Emblem != 0)
						{
							if (playerGuild == null || myitem.Emblem != playerGuild.Emblem )
							{
								myitem.Emblem = 0;
							}
							if (player.Level < 20)
							{
								if (player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
								{
									myitem.Emblem = 0;
								}
								else
								{
									if (player.GetCraftingSkillValue(player.CraftingPrimarySkill) < 400)
									{
										myitem.Emblem = 0;
									}
								}
							}
						}
					}
				}

				player.Client.ClientState = GameClient.eClientState.WorldEnter;
				// 0x88 - Position
				// 0x6D - FriendList
				// 0x15 - Encumberance update
				// 0x1E - Speed update
				// 0xDD - Shared Buffs update
				// 0x1E - Speed update
				// 0x05 - Health, Sit update
				// 0xAA - Inventory Update
				// 0xAA - Inventory Update /Vault ?
				// 0xBE - 01 various tabs update (skills/spells...)
				// 0xBE - 08 various tabs update (skills/spells...)
				// 0xBE - 02 spells
				// 0xBE - 03 various tabs update (skills/spells...)
				// 0x52 - Money update
				// 0x53 - stats update
				// 0xD7 - Self Buffs update
				// 0xBE - 05, various tabs ...
				// 0x2B - Quest list
				// 0x05 - health again...?
				// 0x39 - XP update?
				// 0x15 - Encumberance update
				// 0xBE - 06, group
				// 0xE4 - ??  (0 0 5 0 0 0 0 0)
				// 0xBE - 0 1 0 0
				// 0x89 - Debug mode
				// 0x1E - Speed again!?
				// 0x25 - model change

				//Get the objectID for this player
				//IMPORTANT ... this is needed BEFORE
				//sending Packet 0x88!!!

				if (!player.AddToWorld())
				{
					log.ErrorFormat("Failed to add player to the region! {0}", player.ToString());
					player.Client.Out.SendPlayerQuit(true);
					player.Client.Player.SaveIntoDatabase();
					player.Client.Player.Quit(true);
					player.Client.Disconnect();
					return;
				}
				// this is bind stuff
				// make sure that players doesnt start dead when coming in
				// thats important since if client moves the player it requests player creation
				if (player.Health <= 0)
				{
					player.Health = player.MaxHealth;
				}
				player.Out.SendPlayerPositionAndObjectID();
				player.Out.SendEncumberance(); // Send only max encumberance without used
				player.Out.SendUpdateMaxSpeed();
				//TODO 0xDD - Conc Buffs // 0 0 0 0
				//Now find the friends that are online
				player.Out.SendUpdateMaxSpeed(); // Speed after conc buffs
				player.Out.SendStatusUpdate();
				player.Out.SendInventoryItemsUpdate(0x01, player.Inventory.EquippedItems);
				player.Out.SendInventoryItemsUpdate(0x02, player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack));
				player.Out.SendUpdatePlayerSkills();   //TODO Insert 0xBE - 08 Various in SendUpdatePlayerSkills() before send spells
				player.Out.SendUpdateCraftingSkills(); // ^
				player.Out.SendUpdatePlayer();
				player.Out.SendUpdateMoney();
				player.Out.SendCharStatsUpdate();
				ArrayList friends = player.Friends;
				ArrayList onlineFriends = new ArrayList();
				foreach(string friendName in friends)
				{
					GameClient friendClient = WorldMgr.GetClientByPlayerName(friendName, true, false);
					if (friendClient == null) continue;
					if (friendClient.Player != null && friendClient.Player.IsAnonymous) continue;
					onlineFriends.Add(friendName);
				}
				player.Out.SendAddFriends((string[])onlineFriends.ToArray(typeof(string)));
				player.Out.SendCharResistsUpdate();
				int effectsCount = 0;
				player.Out.SendUpdateIcons(null, ref effectsCount);
				player.Out.SendUpdateWeaponAndArmorStats();
				player.Out.SendQuestListUpdate();
				player.Out.SendStatusUpdate();
				player.Out.SendUpdatePoints();
				player.Out.SendEncumberance();
				// Visual 0x4C - Color Name style (0 0 5 0 0 0 0 0) for RvR or (0 0 5 1 0 0 0 0) for PvP
				// 0xBE - 0 1 0 0
				//used only on PvP, sets THIS players ID for nearest friend/enemy buttons and "friendly" name colors
				//if (GameServer.ServerRules.GetColorHandling(player.Client) == 1) // PvP
					player.Out.SendObjectGuildID(player, player.Guild);
				player.Out.SendDebugMode(player.TempProperties.getObjectProperty(GamePlayer.DEBUG_MODE_PROPERTY, null) != null);
				player.Out.SendUpdateMaxSpeed(); // Speed in debug mode ?
				//WARNING: This would change problems if a scripter changed the values for plvl
				//GSMessages.SendDebugMode(client,client.Account.PrivLevel>1);
				player.Stealth(false);
				//check item at world load
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0}({1} PID:{2} OID:{3}) entering Region {4}(ID:{5})", player.Client.Account.Name, player.Name, player.Client.SessionID, player.ObjectID, player.CurrentRegion.Description, player.CurrentRegionID);
			}
		}
	}
}
