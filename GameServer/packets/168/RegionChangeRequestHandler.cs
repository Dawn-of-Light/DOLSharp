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
using DOL.GS.Database;
using DOL.GS.JumpPoints;
using DOL.GS.Scripts;
using DOL.GS.ServerRules;
using NHibernate.Expression;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x38^168,"Handles the player region change")]
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
			AbstractJumpPoint jumpPoint = JumpPointMgr.GetJumpPoint((int)JumpSpotID);
			if (jumpPoint == null)
			{
				client.Out.SendMessage("Invalid Jump : [" + JumpSpotID + "]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			new RegionChangeRequestHandler(client.Player, jumpPoint).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles player region change requests
		/// </summary>
		protected class RegionChangeRequestHandler : RegionAction
		{
			/// <summary>
			/// The target jump point
			/// </summary>
			protected readonly AbstractJumpPoint m_jumpPoint;

			/// <summary>
			/// Constructs a new RegionChangeRequestHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="jumpPoint">The target jump point</param>
			public RegionChangeRequestHandler(GamePlayer actionSource, AbstractJumpPoint jumpPoint) : base(actionSource)
			{
				if (jumpPoint == null)
					throw new ArgumentNullException("zonePoint");
				m_jumpPoint = jumpPoint;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				JumpPointTargetLocation targetLoc = m_jumpPoint.IsAllowedToJump(player);
				if (targetLoc == null)
					return;
					
				player.MoveTo((ushort)targetLoc.Region, new Point(targetLoc.X, targetLoc.Y, targetLoc.Z), (ushort)targetLoc.Heading);
			}
		}
	}
}
