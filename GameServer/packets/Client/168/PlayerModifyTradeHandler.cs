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
using System.Collections;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x43^168,"Player Accepts Trade")]
	public class PlayerModifyTradeHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			byte isok =(byte) packet.ReadByte();
			byte repair =(byte) packet.ReadByte();
			byte combine =(byte) packet.ReadByte();
			packet.ReadByte();//unknow

			ITradeWindow trade = client.Player.TradeWindow;
			if (trade == null)
				return 1;
			if (isok==0)
			{
				trade.CloseTrade();
			}
			else if(isok==1)
			{
				if(trade.Repairing != (repair == 1)) trade.Repairing = (repair == 1);
				if(trade.Combine != (combine == 1)) trade.Combine = (combine == 1);
				
				ArrayList tradeSlots = new ArrayList(10);
				for (int i=0;i<10;i++)
				{
					int slotPosition = packet.ReadByte();
					InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slotPosition);
					if(item!=null && item.IsDropable && item.IsTradable )
					{
						tradeSlots.Add(item);
					}
				}
				trade.TradeItems = tradeSlots;

				packet.ReadShort();
				
				int[] tradeMoney = new int[5];
				for(int i=0;i<5;i++)
					tradeMoney[i]=packet.ReadShort();

				long money = Money.GetMoney(tradeMoney[0],tradeMoney[1],tradeMoney[2],tradeMoney[3],tradeMoney[4]);
				trade.TradeMoney = money;
				
				trade.TradeUpdate();
			}
			else if (isok == 2)
			{
				trade.AcceptTrade();
			}
			return 0;
		}
	}
}

