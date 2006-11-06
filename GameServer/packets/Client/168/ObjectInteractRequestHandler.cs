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

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xD2^168,"Handles right click on mobs or objects!")]
	public class ObjectInteractRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
//			packet.Skip(10);
			uint playerX = packet.ReadInt();
			uint playerY = packet.ReadInt();
			int sessionId = packet.ReadShort();
			ushort targetOid = packet.ReadShort();

			new InteractActionHandler(client.Player, targetOid).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles player interact actions
		/// </summary>
		protected class InteractActionHandler : RegionAction
		{
			/// <summary>
			/// The interact target OID
			/// </summary>
			protected readonly int m_targetOid;

			/// <summary>
			/// Constructs a new InterractActionHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="targetOid">The interact target OID</param>
			public InteractActionHandler(GamePlayer actionSource, int targetOid) : base(actionSource)
			{
				m_targetOid = targetOid;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				Region region = player.CurrentRegion;
				if (region == null) return;
				GameObject obj = region.GetObject((ushort)m_targetOid);
				if(obj == null) return;
				obj.Interact(player);
			}
		}
	}
}