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
using System.Reflection;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description résume de ITradeWindow.
	/// </summary>
	public interface ITradeWindow
	{
		ArrayList TradeItems { get; set;}
		ArrayList PartnerTradeItems { get; }

		long TradeMoney { get; set; }
		long PartnerTradeMoney { get; }

		GamePlayer Owner { get; }
		GamePlayer Partner { get; }

		int ItemsCount { get; }
		int PartnerItemsCount { get; }

		bool Repairing { get; set; }
		bool Combine { get; set; }
		
		bool AddItemToTrade(InventoryItem itemForTrade);
		void RemoveItemToTrade(InventoryItem itemToRemove);
		void AddMoneyToTrade(long money);
		
		bool AcceptTrade();
		void TradeUpdate();
	
		object Sync { get; }

		void CloseTrade();
	}
}
