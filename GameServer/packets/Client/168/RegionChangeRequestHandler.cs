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
using DOL.Events;
using DOL.Database;
using DOL.GS.ServerRules;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x38 ^ 168, "Handles the player region change")]
	public class PlayerRegionChangeRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		/// <summary>
		/// Holds jump point types
		/// </summary>
		protected readonly Hashtable m_instanceByName = new Hashtable();

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort JumpSpotID = packet.ReadShort();
			ZonePoint zonePoint = GameServer.Database.SelectObject<ZonePoint>("`Id` = '" + JumpSpotID + "' AND (`Realm` = '" + (byte)client.Player.Realm + "' OR `Realm` = '0' OR `Realm` = NULL)");

			if (zonePoint == null)
			{
				client.Out.SendMessage("Invalid Jump : [" + JumpSpotID + "]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			//tutorial zone
			if (client.Player.CurrentRegionID == 27)
			{
				zonePoint = new ZonePoint();
				switch (client.Player.Realm)
				{
					case eRealm.Albion: zonePoint.Region = 1; break;
					case eRealm.Midgard: zonePoint.Region = 100; break;
					case eRealm.Hibernia: zonePoint.Region = 200; break;
				}
				zonePoint.ClassType = "DOL.GS.GameEvents.TutorialJumpPointHandler";
			}

			if (client.Account.PrivLevel > 1)
				client.Out.SendMessage("JumpSpotID = " + JumpSpotID, eChatType.CT_System, eChatLoc.CL_SystemWindow);

            //Dinberg: Fix - some jump points are handled code side, such as instances.
            //As such, region MAY be zero in the database, so this causes an issue.
            if (zonePoint.Region != 0)
            {
                Region reg = WorldMgr.GetRegion(zonePoint.Region);
                if (reg != null)
                {
                    if (reg.IsDisabled)
                    {
                        if ((client.Player.Mission is Quests.TaskDungeonMission && (client.Player.Mission as Quests.TaskDungeonMission).TaskRegion.Skin == reg.Skin) == false)
                        {
                            client.Out.SendMessage("This region has been disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                }
            }

			//check caps for battleground
			Battleground bg = Keeps.KeepMgr.GetBattleground(zonePoint.Region);
			if (bg != null)
			{
				if (client.Player.Level < bg.MinLevel &&
					client.Player.Level > bg.MaxLevel &&
					client.Player.RealmLevel >= bg.MaxRealmLevel)
					return 1;
			}

			IJumpPointHandler check = null;
			if (zonePoint.ClassType != null && zonePoint.ClassType != "")
			{
				check = (IJumpPointHandler)m_instanceByName[zonePoint.ClassType];

				if (check == null)
				{
                    
                    //Dinberg - Instances need to use a special handler. This is because some instances will result
                    //in duplicated zonepoints, such as if Tir Na Nog were to be instanced for a quest.
                    string type = (client.Player.CurrentRegion.IsInstance) ? "DOL.GS.ServerRules.InstanceDoorJumpPoint" : zonePoint.ClassType;
					Type t = ScriptMgr.GetType(type);

					if (t == null)
					{
						log.ErrorFormat("jump point {0}: class {1} not found!", zonePoint.Id, zonePoint.ClassType);
					}
					else if (!typeof(IJumpPointHandler).IsAssignableFrom(t))
					{
						log.ErrorFormat("jump point {0}: class {1} must implement IJumpPointHandler interface!", zonePoint.Id, zonePoint.ClassType);
					}
					else
					{
						try
						{
							check = (IJumpPointHandler)Activator.CreateInstance(t);
						}
						catch (Exception e)
						{
							check = null;
							log.Error(string.Format("jump point {0}: error creating a new instance of jump point handler {1}", zonePoint.Id, zonePoint.ClassType), e);
						}
					}
				}

				if (check != null)
					m_instanceByName[zonePoint.ClassType] = check;
			}

			new RegionChangeRequestHandler(client.Player, zonePoint, check).Start(1);
			return 1;
		}

		/// <summary>
		/// Handles player region change requests
		/// </summary>
		protected class RegionChangeRequestHandler : RegionAction
		{
			/// <summary>
			/// The target zone point
			/// </summary>
			protected readonly ZonePoint m_zonePoint;

			/// <summary>
			/// Checks whether player is allowed to jump
			/// </summary>
			protected readonly IJumpPointHandler m_checkHandler;

			/// <summary>
			/// Constructs a new RegionChangeRequestHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="zonePoint">The target zone point</param>
			/// <param name="checker">The jump point checker instance</param>
			public RegionChangeRequestHandler(GamePlayer actionSource, ZonePoint zonePoint, IJumpPointHandler checker)
				: base(actionSource)
			{
				if (zonePoint == null)
					throw new ArgumentNullException("zonePoint");
				m_zonePoint = zonePoint;
				m_checkHandler = checker;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				Region reg = WorldMgr.GetRegion(m_zonePoint.Region);
				if (reg != null && reg.Expansion > (int)player.Client.ClientType)
				{
					player.Out.SendMessage("Destination region (" + reg.Description + ") is not supported by your client type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
						if (log.IsErrorEnabled)
							log.Error("Jump point handler (" + m_zonePoint.ClassType + ")", e);
						player.Out.SendMessage("exception in jump point (" + m_zonePoint.Id + ") handler...", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}
				player.MoveTo(m_zonePoint.Region, m_zonePoint.X, m_zonePoint.Y, m_zonePoint.Z, m_zonePoint.Heading);
			}
		}
	}
}
