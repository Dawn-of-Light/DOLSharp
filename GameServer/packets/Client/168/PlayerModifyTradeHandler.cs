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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.ModifyTrade, "Player Modify/Accepts Trade", eClientStatus.PlayerInGame)]
	public class PlayerModifyTradeHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			var status = packet.ReadByte();
			var repair = packet.ReadByte() == 1;
			var combine = packet.ReadByte() == 1;
			var unk1 = packet.ReadByte(); //unknow
			
			var tradeSlots = new int[10];
			for (int i = 0; i < 10; i++)
				tradeSlots[i] = packet.ReadByte();
			
			var unk2 = packet.ReadShort(); //unknow
			long money = Money.GetMoney(
				packet.ReadShort(),
				packet.ReadShort(),
				packet.ReadShort(),
				packet.ReadShort(),
				packet.ReadShort());

			if (client.Player != null)
			{
				new RegionTimerAction<GamePlayer>(client.Player, pl => {
				                                  	ITradeWindow trade = pl.TradeWindow;
				                                  	if (trade == null)
				                                  		return;
				                                  	
				                                  	if (status == 0)
				                                  	{
				                                  		trade.CloseTrade();
				                                  	}
				                                  	else if (status == 1)
				                                  	{
				                                  		trade.Repairing = repair;
				                                  		trade.Combine = combine;
				                                  		
				                                  		var tradeItems = new ArrayList(10);
				                                  		for (int i = 0; i < 10; i++)
				                                  		{
				                                  			var item = pl.Inventory.GetItem((eInventorySlot)tradeSlots[i]);
				                                  			if(item != null && (item.IsTradable || pl.CanTradeAnyItem || pl.TradeWindow.Partner.CanTradeAnyItem))
				                                  				tradeItems.Add(item);
				                                  		}
				                                  		
				                                  		trade.TradeItems = tradeItems;
				                                  		trade.TradeMoney = money;
				                                  		trade.TradeUpdate();
				                                  	}
				                                  	else if (status == 2)
				                                  	{
				                                  		trade.AcceptTrade();
				                                  	}
				                                  }).Start(1);
			}
		}
	}
}

