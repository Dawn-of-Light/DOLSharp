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
using DOL.Database2;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x11, "Handles player market search")]
	public class PlayerMarketSearchRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player == null)
				return 0;

			string filter = packet.ReadString(64);
			int slot = (int)packet.ReadInt();
			int skill = (int)packet.ReadInt();
			int resist = (int)packet.ReadInt();
			int bonus = (int)packet.ReadInt();
			int hp = (int)packet.ReadInt();
			int power = (int)packet.ReadInt();
			int proc = (int)packet.ReadInt();
			int qtyMin = (int)packet.ReadInt();
			int qtyMax = (int)packet.ReadInt();
			int levelMin = (int)packet.ReadInt();
			int levelMax = (int)packet.ReadInt();
			int priceMin = (int)packet.ReadInt();
			int priceMax = (int)packet.ReadInt();
			int visual = (int)packet.ReadInt();
			byte page = (byte)packet.ReadByte();
			byte unk1 = (byte)packet.ReadByte();
			short unk2 = (short)packet.ReadShort();

			System.Text.StringBuilder str = new System.Text.StringBuilder();
			str.AppendFormat("PlayerMarketSearchRequestHandler: slot:{0,2} skill:{1,2} resist:{2,2} bonus:{3,2} hp:{4,2} power:{5,2} proc:{6} qtyMin:{7,3} qtyMax:{8,3} levelMin:{9,2} levelMax:{10,2} priceMin:{11,2} priceMax:{12,2} visual:{13} page:{14,2} unk:0x{15:X2}{16:X4} filter:{17}",
				slot, skill, resist, bonus, hp, power, proc, qtyMin, qtyMax, levelMin, levelMax, priceMin, priceMax, visual, page, unk1, unk2, filter);
			log.Debug(str.ToString());
			return 1;
		}
	}
}