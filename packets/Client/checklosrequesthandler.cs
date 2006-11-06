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

namespace DOL.GS.PacketHandler.Client
{
	[PacketHandler(PacketHandlerType.TCP, 0xD0, "handle Check LOS")]
	public class CheckLOSResponseHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort checker_oid = packet.ReadShort();
			ushort target_oid = packet.ReadShort();
			ushort response = packet.ReadShort();
			ushort unknow = packet.ReadShort();

			if (client.Account.PrivLevel > 1)
				client.Out.SendMessage(string.Format("CheckLOSResponse: oid1:0x{0:X4} oid2:0x{1:X4} response:0x{2:X4} unk:0x{3:X4}",
				checker_oid, target_oid, response, unknow), eChatType.CT_System,eChatLoc.CL_SystemWindow);

			HandleCheckAction action = new HandleCheckAction(
				client.Player,
				checker_oid,
				target_oid,
				response);

			action.Start(1);

			return 1;
		}

		/// <summary>
		/// Handles the LOS check response
		/// </summary>
		protected class HandleCheckAction : RegionAction
		{
			/// <summary>
			/// The LOS source OID
			/// </summary>
			protected readonly int m_checkerOid;
			/// <summary>
			/// The LOS target OID
			/// </summary>
			protected readonly int m_targetOid;
			/// <summary>
			/// The request response
			/// </summary>
			protected readonly int m_response;

			/// <summary>
			/// Constructs a new HandleCheckAction
			/// </summary>
			/// <param name="actionSource">The player received the packet</param>
			/// <param name="checkerOid">The LOS source OID</param>
			/// <param name="targetOid">The LOS target OID</param>
			/// <param name="response">The request response</param>
			public HandleCheckAction(GamePlayer actionSource, int checkerOid, int targetOid, int response) : base(actionSource)
			{
				m_checkerOid = checkerOid;
				m_targetOid = targetOid;
				m_response = response;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				string key = string.Format("LOS C:0x{0} T:0x{1}",m_checkerOid,m_targetOid);
				GamePlayer player = (GamePlayer)m_actionSource;
				CheckLOSResponse callback = (CheckLOSResponse)player.TempProperties.getObjectProperty(key, null);
				if(callback == null) return;
				callback(player, (ushort)m_response, (ushort)m_targetOid);
				player.TempProperties.removeProperty(key);
			}
		}
	}
}