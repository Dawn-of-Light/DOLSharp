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

namespace DOL.GS.PacketHandler.Client
{
	/// <summary>
	/// Handles the disband group packet
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x37^168,"Handles the disband group packet")]
	public class DisbandFromGroupHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			//DOLConsole.WriteLine("got disband from "+client.Player.Name);

			new PlayerDisbandAction(client.Player).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles players disband actions
		/// </summary>
		protected class PlayerDisbandAction : RegionAction
		{
			/// <summary>
			/// Constructs a new PlayerDisbandAction
			/// </summary>
			/// <param name="actionSource">The disbanding player</param>
			public PlayerDisbandAction(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				if(player.PlayerGroup == null)
				{
					return;
				}
				player.PlayerGroup.RemovePlayer(player);
			}
		}
	}
}
