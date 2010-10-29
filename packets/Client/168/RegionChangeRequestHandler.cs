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
using DOL.GS.Keeps;
using DOL.GS.Quests;
using DOL.GS.ServerRules;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerRegionChangeRequest, ClientStatus.PlayerInGame)]
	public class PlayerRegionChangeRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds jump point types
		/// </summary>
		protected readonly Hashtable m_instanceByName = new Hashtable();

		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort jumpSpotID = packet.ReadShort();

			eRealm targetRealm = client.Player.Realm;

			if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
			{
				// if we are in TrialsOfAtlantis then base the target jump on the current region realm instead of the players realm
				targetRealm = client.Player.CurrentZone.GetRealm();
			}

			var zonePoint =	GameServer.Database.SelectObject<ZonePoint>("`Id` = '" + jumpSpotID + "' AND (`Realm` = '" + (byte)targetRealm +
																		"' OR `Realm` = '0' OR `Realm` = NULL)");

			if (zonePoint == null)
			{
				if (client.Account.PrivLevel > 1)
				{
					client.Out.SendMessage("Invalid Jump : [" + jumpSpotID + "]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				zonePoint = new ZonePoint();
				zonePoint.Id = jumpSpotID;
			}

			//tutorial zone
			if (client.Player.CurrentRegionID == 27)
			{
				zonePoint = new ZonePoint();
				switch (client.Player.Realm)
				{
					case eRealm.Albion:
						zonePoint.Region = 1;
						break;
					case eRealm.Midgard:
						zonePoint.Region = 100;
						break;
					case eRealm.Hibernia:
						zonePoint.Region = 200;
						break;
				}

				zonePoint.ClassType = "DOL.GS.GameEvents.TutorialJumpPointHandler";
			}

			if (client.Account.PrivLevel > 1)
			{
				client.Out.SendMessage("JumpSpotID = " + jumpSpotID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("ZonePoint Region = " + zonePoint.Region + ", ClassType = '" + zonePoint.ClassType + "'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			//Dinberg: Fix - some jump points are handled code side, such as instances.
			//As such, region MAY be zero in the database, so this causes an issue.

			if (zonePoint.Region != 0)
			{
				Region reg = WorldMgr.GetRegion(zonePoint.Region);
				if (reg != null)
				{
					// check for target region disabled if player is in a standard region
					// otherwise the custom region should handle OnZonePoint for this check
					if (client.Player.CurrentRegion.IsCustom == false && reg.IsDisabled)
					{
						if ((client.Player.Mission is TaskDungeonMission &&
							 (client.Player.Mission as TaskDungeonMission).TaskRegion.Skin == reg.Skin) == false)
						{
							client.Out.SendMessage("This region has been disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							if (client.Account.PrivLevel == 1)
							{
								return;
							}
						}
					}
				}
			}

			// Allow the region to either deny exit or handle the zonepoint in a custom way
			if (client.Player.CurrentRegion.OnZonePoint(client.Player, zonePoint) == false)
			{
				return;
			}

			//check caps for battleground
			Battleground bg = KeepMgr.GetBattleground(zonePoint.Region);
			if (bg != null)
			{
				if (client.Player.Level < bg.MinLevel && client.Player.Level > bg.MaxLevel &&
					client.Player.RealmLevel >= bg.MaxRealmLevel)
					return;
			}

			IJumpPointHandler customHandler = null;
			if (string.IsNullOrEmpty(zonePoint.ClassType) == false)
			{
				customHandler = (IJumpPointHandler)m_instanceByName[zonePoint.ClassType];

				// check for db change to update cached handler
				if (customHandler != null && customHandler.GetType().FullName != zonePoint.ClassType)
				{
					customHandler = null;
				}

				if (customHandler == null)
				{
					//Dinberg - Instances need to use a special handler. This is because some instances will result
					//in duplicated zonepoints, such as if Tir Na Nog were to be instanced for a quest.
					string type = (client.Player.CurrentRegion.IsInstance)
									? "DOL.GS.ServerRules.InstanceDoorJumpPoint"
									: zonePoint.ClassType;
					Type t = ScriptMgr.GetType(type);

					if (t == null)
					{
						Log.ErrorFormat("jump point {0}: class {1} not found!", zonePoint.Id, zonePoint.ClassType);
					}
					else if (!typeof(IJumpPointHandler).IsAssignableFrom(t))
					{
						Log.ErrorFormat("jump point {0}: class {1} must implement IJumpPointHandler interface!", zonePoint.Id,
										zonePoint.ClassType);
					}
					else
					{
						try
						{
							customHandler = (IJumpPointHandler)Activator.CreateInstance(t);
						}
						catch (Exception e)
						{
							customHandler = null;
							Log.Error(
								string.Format("jump point {0}: error creating a new instance of jump point handler {1}", zonePoint.Id,
											  zonePoint.ClassType), e);
						}
					}
				}

				if (customHandler != null)
				{
					m_instanceByName[zonePoint.ClassType] = customHandler;
				}
			}

			new RegionChangeRequestHandler(client.Player, zonePoint, customHandler).Start(1);
		}

		#endregion

		#region Nested type: RegionChangeRequestHandler

		/// <summary>
		/// Handles player region change requests
		/// </summary>
		protected class RegionChangeRequestHandler : RegionAction
		{
			/// <summary>
			/// Checks whether player is allowed to jump
			/// </summary>
			protected readonly IJumpPointHandler m_checkHandler;

			/// <summary>
			/// The target zone point
			/// </summary>
			protected readonly ZonePoint m_zonePoint;

			/// <summary>
			/// Constructs a new RegionChangeRequestHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="zonePoint">The target zone point</param>
			/// <param name="checker">The jump point checker instance</param>
			public RegionChangeRequestHandler(GamePlayer actionSource, ZonePoint zonePoint, IJumpPointHandler checkHandler)
				: base(actionSource)
			{
				if (zonePoint == null)
					throw new ArgumentNullException("zonePoint");

				m_zonePoint = zonePoint;
				m_checkHandler = checkHandler;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer)m_actionSource;

				Region reg = WorldMgr.GetRegion(m_zonePoint.Region);
				if (reg != null && reg.Expansion > (int)player.Client.ClientType)
				{
					player.Out.SendMessage("Destination region (" + reg.Description + ") is not supported by your client type.",
										   eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (m_checkHandler != null)
				{
					try
					{
						if (!m_checkHandler.IsAllowedToJump(m_zonePoint, player))
							return;
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Jump point handler (" + m_zonePoint.ClassType + ")", e);

						player.Out.SendMessage("exception in jump point (" + m_zonePoint.Id + ") handler...", eChatType.CT_System,
											   eChatLoc.CL_SystemWindow);
						return;
					}
				}

				player.MoveTo(m_zonePoint.Region, m_zonePoint.X, m_zonePoint.Y, m_zonePoint.Z, m_zonePoint.Heading);
			}
		}

		#endregion
	}
}