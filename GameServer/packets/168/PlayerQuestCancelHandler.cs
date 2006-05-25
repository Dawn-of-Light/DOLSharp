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
using DOL.Events;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.Quests;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xE7^168,"Handles Quest Cancel button request")]
	public class PlayerQuestCancelHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			byte unk1 = (byte) packet.ReadByte(); // Always 0 ?
			byte unk2 = (byte) packet.ReadByte(); // Always 1 ?
			byte unk3 = (byte) packet.ReadByte(); // Always 0 ?
			byte questIndex = (byte) packet.ReadByte(); // quest number, begin from 0 to xx. 0 = first quest in the journal
		/*	byte unk5 = (byte) packet.ReadByte(); // Always 0 ?
			byte unk6 = (byte) packet.ReadByte(); // Always 0 ?
			byte unk7 = (byte) packet.ReadByte(); // Always 0 ?
			byte unk8 = (byte) packet.ReadByte(); // Always 0 ?*/

			if (client.Player.ActiveQuests == null)
			{
				client.Out.SendMessage("You have not quest currently active!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			lock(client.Player.ActiveQuests)
			{
				int i = 0;
				foreach(AbstractQuest quest in client.Player.ActiveQuests)
				{
					if(i == questIndex)
					{
						client.Player.Notify(GamePlayerEvent.AbortQuest, client.Player, new QuestCancelEventArgs(client.Player, quest));
						return 1;
					}
					i++;
				}
			}
			
			client.Out.SendMessage("The position "+questIndex+" in your quest journal is empty!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}
