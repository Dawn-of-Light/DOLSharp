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
using DOL.GS.Database;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x44^168,"Handles player ground-target")]
	public class PlayerGroundTargetHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int groundX = (int)packet.ReadInt();
			int groundY = (int)packet.ReadInt();
			int groundZ = (int)packet.ReadInt();
			ushort flag = packet.ReadShort();
//			ushort unk2 = packet.ReadShort();

			new ChangeGroundTargetHandler(client.Player, new Point(groundX, groundY, groundZ), flag)
				.Start(1);

			return 1;
		}

		/// <summary>
		/// Handles ground target changes.
		/// </summary>
		protected class ChangeGroundTargetHandler : RegionAction
		{
			/// <summary>
			/// The new ground target position.
			/// </summary>
			protected readonly Point m_newPosition;

			/// <summary>
			/// Various flags.
			/// </summary>
			protected readonly ushort m_flag;

			/// <summary>
			/// Constructs a new ChangeGroundTargetHandler.
			/// </summary>
			/// <param name="actionSource">The action source.</param>
			/// <param name="position">The new ground target position.</param>
			public ChangeGroundTargetHandler(GamePlayer actionSource, Point position, ushort flag) : base(actionSource)
			{
				m_newPosition = position;
				m_flag = flag;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				player.GroundTargetInView = ((m_flag & 0x100) != 0);
				player.GroundTarget = m_newPosition;
				if(!player.GroundTargetInView)
					player.Out.SendMessage("Your ground target is not visible!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}
	}
}