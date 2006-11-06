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
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xD1^168,"Handles player selling")]
	public class PlayerSellRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort item_slot = packet.ReadShort();

			if (client.Player.TargetObject == null)
			{
				client.Out.SendMessage("You must select an NPC to sell to.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return 0;
			}

			lock (client.Player.Inventory)
			{
				InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)item_slot);
				if (item == null)
					return 0;

				int itemCount = Math.Max(1, item.Count);
				int packSize = Math.Max(1, item.PackSize);

				if (client.Player.TargetObject is GameMerchant)
				{
					//Let the merchant choos how to handle the trade.
					((GameMerchant)client.Player.TargetObject).OnPlayerSell(client.Player, item);

				}
				else if (client.Player.TargetObject is GameLotMarker)
				{
					((GameLotMarker)client.Player.TargetObject).OnPlayerSell(client.Player, item);
				}
			}
			return 0;
		}
	}
}