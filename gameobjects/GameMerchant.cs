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
using System.Threading;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game merchant
	/// </summary>
	public class GameMerchant : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructor
		/// </summary>
		public GameMerchant()
			: base()
		{
		}

		#region GetExamineMessages / Interact

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a merchant.");
			list.Add("[Right click to display a shop window]");
			return list;
		}

		/// <summary>
		/// Called when a player right clicks on the merchant
		/// </summary>
		/// <param name="player">Player that interacted with the merchant</param>
		/// <returns>True if succeeded</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;
			TurnTo(player, 10000);
			SendMerchantWindow(player);
			return true;
		}

		/// <summary>
		/// send the merchants item offer window to a player
		/// </summary>
		/// <param name="player"></param>
		public virtual void SendMerchantWindow(GamePlayer player)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(SendMerchatWindowCallback), player);
		}

		/// <summary>
		/// Sends merchant window from threadpool thread
		/// </summary>
		/// <param name="state">The game player to send to</param>
		protected virtual void SendMerchatWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(m_tradeItems, eMerchantWindowType.Normal);
		}
		#endregion

		#region Items List

		/// <summary>
		/// Items available for sale
		/// </summary>
		protected MerchantTradeItems m_tradeItems;

		/// <summary>
		/// Gets the items available from this merchant
		/// </summary>
		public MerchantTradeItems TradeItems
		{
			get { return m_tradeItems; }
			set { m_tradeItems = value; }
		}

		#endregion

		#region Buy / Sell / Apparaise

		/// <summary>
		/// Called when a player buys an item
		/// </summary>
		/// <param name="player">The player making the purchase</param>
		/// <param name="item_slot">slot of the item to be bought</param>
		/// <param name="number">Number to be bought</param>
		/// <returns>true if buying is allowed, false if buying should be prevented</returns>
		public virtual void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Value;

			lock (player.Inventory)
			{

				if (player.GetCurrentMoney() < totalValue)
				{
					player.Out.SendMessage("You need " + Money.GetString(totalValue) + " to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (!player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = "You just bought " + amountToBuy + " pieces of " + template.GetName(1, false) + " for {0}.";
				else
					message = "You just bought " + template.GetName(1, false) + " for {0}.";

				// Check if player has enough money and subtract the money
				if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					throw new Exception("Money amount changed while adding items.");
				}
			}
		}

		/// <summary>
		/// Called when a player sells something
		/// </summary>
		/// <param name="player">Player making the sale</param>
		/// <param name="item">The InventoryItem to be sold</param>
		/// <returns>true if selling is allowed, false if it should be prevented</returns>
		public virtual void OnPlayerSell(GamePlayer player, InventoryItem item)
		{
			if (!item.IsDropable)
			{
				player.Out.SendMessage("This item can't be sold.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!WorldMgr.CheckDistance(this, player, WorldMgr.PICKUP_DISTANCE)) // tested
			{
				player.Out.SendMessage(GetName(0, true) + " is too far away!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			long itemValue = OnPlayerAppraise(player, item, true);

			if (itemValue == 0)
			{
				player.Out.SendMessage(GetName(0, true) + " isn't interested in " + item.GetName(0, false) + ".", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			if (player.Inventory.RemoveItem(item))
			{
				string message = GetName(0, true) + " gives you " + Money.GetString(itemValue) + " for " + item.GetName(0, false) + ".";
				player.AddMoney(itemValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}
			else
				player.Out.SendMessage("This item can't be sold.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Called to appraise the value of an item
		/// </summary>
		/// <param name="player">The player whose item needs appraising</param>
		/// <param name="item">The item to be appraised</param>
		/// <returns>The price this merchant will pay for the offered items</returns>
		public virtual long OnPlayerAppraise(GamePlayer player, InventoryItem item, bool silent)
		{
			if (item == null)
				return 0;

			int itemCount = Math.Max(1, item.Count);
			int packSize = Math.Max(1, item.PackSize);
			long val = item.Value * itemCount / packSize / 2;

			if (!item.IsDropable) val = 0;

			if (!silent)
			{
				string message;
				if (val == 0)
				{
					message = GetName(0, true) + " isn't interested in " + item.GetName(0, false);
				}
				else
				{
					message = GetName(0, true) + " offers you " + Money.GetString(val) + " for " + item.GetName(0, false);
				}
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
			return val;
		}

		#endregion

		#region Database

		/// <summary>
		/// Loads a merchant from the DB
		/// </summary>
		/// <param name="merchantobject">The merchant DB object</param>
		public override void LoadFromDatabase(DataObject merchantobject)
		{
			base.LoadFromDatabase(merchantobject);
			if (!(merchantobject is Mob)) return;
			Mob merchant = (Mob)merchantobject;
			if (merchant.ItemsListTemplateID != null && merchant.ItemsListTemplateID.Length > 0)
				m_tradeItems = new MerchantTradeItems(merchant.ItemsListTemplateID);
		}

		/// <summary>
		/// Saves a merchant into the DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			Mob merchant = null;
			if (InternalID != null)
				merchant = (Mob)GameServer.Database.FindObjectByKey(typeof(Mob), InternalID);
			if (merchant == null)
				merchant = new Mob();

			merchant.Name = Name;
			merchant.Guild = GuildName;
			merchant.X = X;
			merchant.Y = Y;
			merchant.Z = Z;
			merchant.Heading = Heading;
			merchant.Speed = MaxSpeedBase;
			merchant.Region = CurrentRegionID;
			merchant.Realm = Realm;
			merchant.Model = Model;
			merchant.Size = Size;
			merchant.Level = Level;
			merchant.Flags = Flags;
			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if (aggroBrain != null)
			{
				merchant.AggroLevel = aggroBrain.AggroLevel;
				merchant.AggroRange = aggroBrain.AggroRange;
			}
			merchant.ClassType = this.GetType().ToString();
			merchant.EquipmentTemplateID = EquipmentTemplateID;
			if (m_tradeItems == null)
			{
				merchant.ItemsListTemplateID = null;
			}
			else
			{
				merchant.ItemsListTemplateID = m_tradeItems.ItemsListID;
			}

			if (InternalID == null)
			{
				GameServer.Database.AddNewObject(merchant);
				InternalID = merchant.ObjectId;
			}
			else
			{
				GameServer.Database.SaveObject(merchant);
			}
		}

		/// <summary>
		/// Deletes a merchant from the DB
		/// </summary>
		public override void DeleteFromDatabase()
		{
			if (InternalID != null)
			{
				Mob merchant = (Mob)GameServer.Database.FindObjectByKey(typeof(Mob), InternalID);
				if (merchant != null)
					GameServer.Database.DeleteObject(merchant);
			}
			InternalID = null;
		}

		#endregion
	}

	/* 
 * Author:   Avithan 
 * Date:   22.12.2005 
 * Bounty merchant 
 */

	public class GameBountyMerchant : GameMerchant
	{
		protected override void SendMerchatWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(m_tradeItems, eMerchantWindowType.Bp);
		}

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Value;

			lock (player.Inventory)
			{
				if (player.BountyPoints < totalValue)
				{
					player.Out.SendMessage("You need " + totalValue + " BPs to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if (!player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				//Generate the buy message
				string message;
				if (number > 1)
					message = "You just bought " + totalValue + " pieces of " + template.GetName(1, false) + " for " + totalValue + " BPs.";
				else
					message = "You just bought " + template.GetName(1, false) + " for " + totalValue + " BPs.";
				player.BountyPoints -= totalValue;
				player.Out.SendUpdatePoints();
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}

		}
	}

	public class GameChampionMerchant : GameMerchant
	{
		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			/*
			int page = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			if (player.ChampionLevel >= page + 2)
				return base.OnPlayerBuy(player, item_slot, number);
			else
			{
				player.Out.SendMessage("You must be Champion Level " + (page + 2) + " or higher to be able to buy this horse!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}
			 */
		}

	}

	public abstract class GameCountMerchant : GameMerchant
	{
		protected GameInventoryItem m_moneyItem;

		public GameInventoryItem moneyItem
		{
			get { return m_moneyItem; }
		}

		protected string m_countText;

		public string CountText
		{
			get { return m_countText; }
		}

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a merchant.");
			list.Add("You can buy items here for " + moneyItem.Item.Name + ".");
			list.Add("[Right click to display a shop window]");
			return list;
		}

		protected override void SendMerchatWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(m_tradeItems, eMerchantWindowType.Count);
		}

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{

			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Value;

			lock (player.Inventory)
			{
				if (player.Inventory.CountItemTemplate(m_moneyItem.Item.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) < totalValue)
				{
					player.Out.SendMessage("You need " + totalValue + " " + m_countText + " to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if (!player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = "You just bought " + amountToBuy + " pieces of " + template.GetName(1, false) + " for " + totalValue + " " + m_countText + ".";
				else
					message = "You just bought " + template.GetName(1, false) + " for " + totalValue + " " + m_countText + ".";

				System.Collections.IList items = (System.Collections.IList)player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				int removed = 0;

				foreach (InventoryItem item in items)
				{
					if (item.Id_nb != m_moneyItem.Item.Id_nb)
						continue;
					int remFromStack = Math.Min(item.Count, (int)(totalValue - removed));
					player.Inventory.RemoveCountFromStack(item, remFromStack);
					removed += remFromStack;
					if (removed == totalValue)
						break;
				}
				player.Out.SendInventoryItemsUpdate(items);
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
		}
	}

	public class GameDiamondSealsMerchant : GameCountMerchant
	{
		public GameDiamondSealsMerchant()
			: base()
		{
			m_moneyItem = GameInventoryItem.CreateFromTemplate("dias");
			m_countText = "units of diamond";
		}
	}

	public class GameSapphireSealsMerchant : GameCountMerchant
	{
		public GameSapphireSealsMerchant()
			: base()
		{
			m_moneyItem = GameInventoryItem.CreateFromTemplate("saphir");
			m_countText = "units of sapphire";
		}

	}

	public class GameEmeraldSealsMerchant : GameCountMerchant
	{
		public GameEmeraldSealsMerchant()
			: base()
		{
			m_moneyItem = GameInventoryItem.CreateFromTemplate("smaras");
			m_countText = "units of emerald";
		}
	}

	public class GameAuruliteMerchant : GameCountMerchant
	{
		public GameAuruliteMerchant()
			: base()
		{
			m_moneyItem = GameInventoryItem.CreateFromTemplate("aurulite");
			m_countText = "units of aurulite";
		}
	}
}