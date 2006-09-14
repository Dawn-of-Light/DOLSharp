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
using DOL.Database;

namespace DOL.GS.PacketHandler.v168
{
	/// <summary>
	/// EmblemDialogReponseHandler is the response of client wend when we close the emblem selection dialogue.
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x4A^168,"Handles destroy item requests from client")]
	public class EmblemDialogReponseHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if(client.Player.Guild == null)
				return 0;
			if(!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
				return 0;
			int primarycolor   = packet.ReadByte() & 0x0F; //4bits
			int secondarycolor = packet.ReadByte() & 0x07; //3bits
			int pattern        = packet.ReadByte() & 0x03; //2bits
			int logo           = packet.ReadByte() & 0x7F; //7bits

			/*for 1.76+ if logo > 0x7F for some field added 0x80 bit
			 * example
			 * StoC_0x15 item.Slot & 0x80 = Logo & 0x80
			 * StoC_0x4E horse.Barding & 0x80 = Logo 0x80
			 */

			ushort oldemblem = client.Player.Guild.theGuildDB.Emblem;
			ushort newemblem = (ushort)((logo << 9) | (pattern << 7) | (primarycolor << 3) | secondarycolor);
			if (GuildMgr.IsEmblemUsed(newemblem))
			{
				client.Player.Out.SendMessage("This emblem is already in use by another guild, please choose another!", eChatType.CT_System, eChatLoc.CL_SystemWindow );
				return 0;
			}
			GuildMgr.ChangeEmblem(client.Player, oldemblem, newemblem);
			return 1;
		}
	}
}
