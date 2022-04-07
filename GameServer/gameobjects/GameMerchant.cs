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
using System.Collections.Generic;
using System.Threading;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.GS.Finance;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game merchant
	/// </summary>
	public class GameMerchant : GameNPC
	{
        #region GetExamineMessages / Interact

        /// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.RemoveAt(list.Count - 1);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.YouExamine", 
                                                GetName(0, false, player.Client.Account.Language, this), GetPronoun(0, true, player.Client.Account.Language),
                                                GetAggroLevelString(player, false)));
			list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.RightClick")); 
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
			ThreadPool.QueueUserWorkItem(new WaitCallback(SendMerchantWindowCallback), player);
		}

		/// <summary>
		/// Sends merchant window from threadpool thread
		/// </summary>
		/// <param name="state">The game player to send to</param>
		protected virtual void SendMerchantWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(Catalog, eMerchantWindowType.Normal);
		}
		#endregion

		#region Items List

		[Obsolete("Use .Catalog instead.")]
		public MerchantTradeItems TradeItems
		{
			get => Catalog != null ? Catalog.ConvertToMerchantTradeItems() : null;
			set 
			{
				if(value == null) Catalog = null;
				Catalog = value.Catalog; 
			}
		}

		public MerchantCatalog Catalog { get; set; }

		#endregion

		#region Buy / Sell / Apparaise
		public virtual void OnPlayerBuy(GamePlayer player, int globalSlotPosition, int amountToBuy)
		{
			int pageNumber = globalSlotPosition / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotPosition = globalSlotPosition % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			var page = Catalog.GetPage(pageNumber);
            var articleToBuy = page.GetEntry((byte)slotPosition);
			var itemToBuy = articleToBuy.Item;
            if (itemToBuy == null) return;
			var currency = page.Currency;

			if (itemToBuy.PackSize > 0)
				amountToBuy *= itemToBuy.PackSize;

			if (amountToBuy <= 0) return;

			long cost = amountToBuy * articleToBuy.CurrencyAmount;

            lock (player.Inventory)
            {
                if (currency.Equals(Money.Copper) || currency.Equals(Money.BP) || currency is ItemCurrency)
                {
                    var currencyItem = page.CurrencyItem;
                    var costToText = CurrencyToText(currency.Create(cost));
                    if (!HasPlayerEnoughBalance(player, currency.Create(cost)))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeedGeneric", costToText), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (!player.Inventory.AddTemplate(GameInventoryItem.Create(itemToBuy), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, itemToBuy, amountToBuy);

                    string message;
                    if (amountToBuy == 1)
                        message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtGeneric", itemToBuy.Name, costToText);
                    else
                        message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPiecesGeneric", amountToBuy, itemToBuy.Name, costToText);
                    WithdrawCurrencyFromPlayer(player, currency.Create(cost));
                    player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);

                }
                else if (currency.Equals(Money.Mithril)) throw new NotImplementedException("Mithril is currently not implemented as a separate currency.");
                else throw new ArgumentException($"{currency} is not implemented.");
            }
        }

        private void WithdrawCurrencyFromPlayer(GamePlayer player, Money price)
        {
            if (price.Type.Equals(Money.Copper))
            {
                if (!player.RemoveMoney(price.Amount)) throw new Exception("Money amount changed while adding items.");
            }
            else if (price.Type.Equals(Money.BP)) player.BountyPoints -= price.Amount;
            else if (price.Type is ItemCurrency itemCurrency)
            {
                player.Inventory.RemoveTemplate(itemCurrency.Item.Id_nb, (int)price.Amount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
            }
            else throw new NotImplementedException($"{price.Type} is currently not implemented.");
        }

        private bool HasPlayerEnoughBalance(GamePlayer player, Money price)
        {
            if (price.Type.Equals(Money.Copper)) return player.GetCurrentMoney() > price.Amount;
            else if (price.Type.Equals(Money.BP)) return player.BountyPoints > price.Amount;
            else if (price.Type is ItemCurrency itemCurrency)
            {
                var balance = player.Inventory.CountItemTemplate(itemCurrency.Item.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                return balance > price.Amount;
            }
            else throw new ArgumentException($"ToText for currency {price.Type} does not exist.");
        }

        private string CurrencyToText(Money money)
        {
            if (money.Type.Equals(Money.Copper)) return Money.GetString(money.Amount);
            else if (money.Type.Equals(Money.BP)) return $"{money.Amount} BPs";
            else if (money.Type.Equals(Money.Mithril)) return $"{money.Amount} Mithril";
            else if (money.Type is ItemCurrency itemCurrency) return $"{money.Amount} {itemCurrency.Item.Id_nb}";
            else throw new ArgumentException($"ToText for currency {money.Type} does not exist.");
        }


		/// <summary>
		/// Called when a player buys an item
		/// </summary>
		/// <param name="player">The player making the purchase</param>
		/// <param name="item_slot">slot of the item to be bought</param>
		/// <param name="number">Number to be bought</param>
		/// <param name="TradeItems"></param>
		/// <returns>true if buying is allowed, false if buying should be prevented</returns>
		public static void OnPlayerBuy(GamePlayer player, int item_slot, int number, MerchantTradeItems TradeItems)
		{
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Price;

			lock (player.Inventory)
			{
				if (player.GetCurrentMoney() < totalValue)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				InventoryLogging.LogInventoryAction("(TRADEITEMS;" + TradeItems.ItemsListID + ")", player, eInventoryActionType.Merchant, template, amountToBuy);
				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

				// Check if player has enough money and subtract the money
				if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					throw new Exception("Money amount changed while adding items.");
				}
				InventoryLogging.LogInventoryAction(player, "(TRADEITEMS;" + TradeItems.ItemsListID + ")", eInventoryActionType.Merchant, totalValue);
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
			if(item==null || player==null) return;
			if (!item.IsDropable)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.CantBeSold"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!this.IsWithinRadius(player, GS.ServerProperties.Properties.WORLD_PICKUP_DISTANCE)) // tested
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.TooFarAway", GetName(0, true)), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			long itemValue = OnPlayerAppraise(player, item, true);

			if (itemValue == 0)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.IsntInterested", GetName(0, true), item.GetName(0, false)), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			if (player.Inventory.RemoveItem(item))
			{
				string message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.GivesYou", GetName(0, true), Money.GetString(itemValue), item.GetName(0, false));
				player.AddMoney(itemValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template, item.Count);
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, itemValue);
				return;
			}
			else
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.CantBeSold"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Called to appraise the value of an item
		/// </summary>
		/// <param name="player">The player whose item needs appraising</param>
		/// <param name="item">The item to be appraised</param>
		/// <param name="silent"></param>
		/// <returns>The price this merchant will pay for the offered items</returns>
		public virtual long OnPlayerAppraise(GamePlayer player, InventoryItem item, bool silent)
		{
			if (item == null)
				return 0;

			int itemCount = Math.Max(1, item.Count);
			int packSize = Math.Max(1, item.PackSize);
			
			long val = item.Price * itemCount / packSize * ServerProperties.Properties.ITEM_SELL_RATIO / 100;

			if (!item.IsDropable)
			{
				val = 0;
			}

			if (!silent)
			{
				string message;
				if (val == 0)
				{
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.IsntInterested", GetName(0, true), item.GetName(0, false));
				}
				else
				{
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerAppraise.Offers", GetName(0, true), Money.GetString(val), item.GetName(0, false));
				}
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
			return val;
		}

		#endregion

		#region NPCTemplate
		public override void LoadTemplate(INpcTemplate template)
		{
			base.LoadTemplate(template);

			if (template != null && string.IsNullOrEmpty(template.ItemsListTemplateID) == false)
			{
				Catalog = MerchantCatalog.LoadFromDatabase(template.ItemsListTemplateID);
			}
		}
		#endregion NPCTemplate

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
				Catalog = MerchantCatalog.LoadFromDatabase(merchant.ItemsListTemplateID);
		}

		/// <summary>
		/// Saves a merchant into the DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			Mob merchant = null;
			if (InternalID != null)
				merchant = GameServer.Database.FindObjectByKey<Mob>(InternalID);
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
            merchant.Realm = (byte)Realm;
            merchant.RoamingRange = RoamingRange;
			merchant.Model = Model;
			merchant.Size = Size;
			merchant.Level = Level;
            merchant.Gender = (byte)Gender;
			merchant.Flags = (uint)Flags;
			merchant.PathID = PathID;
			merchant.PackageID = PackageID;
			merchant.OwnerID = OwnerID;

			IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
			if (aggroBrain != null)
			{
				merchant.AggroLevel = aggroBrain.AggroLevel;
				merchant.AggroRange = aggroBrain.AggroRange;
			}
			merchant.ClassType = this.GetType().ToString();
			merchant.EquipmentTemplateID = EquipmentTemplateID;
			if (Catalog == null)
			{
				merchant.ItemsListTemplateID = null;
			}
			else
			{
				merchant.ItemsListTemplateID = Catalog.ItemListId;
			}

			if (InternalID == null)
			{
				GameServer.Database.AddObject(merchant);
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
				Mob merchant = GameServer.Database.FindObjectByKey<Mob>(InternalID);
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
		protected readonly static Dictionary<string, int> m_currencyValues = null;

		/// <summary>
		/// Populate bp conversion rates
		/// </summary>
		static GameBountyMerchant()
        {
			if (ServerProperties.Properties.BP_EXCHANGE_ALLOW && m_currencyValues == null)
				foreach (string sCurrencyValue in ServerProperties.Properties.BP_EXCHANGE_VALUES.Split(';'))
				{
					string[] asVal = sCurrencyValue.Split('|');

					if (asVal.Length > 1 && int.TryParse(asVal[1], out int currencyValue) && currencyValue > 0)
					{
						// Don't create a dictionary until there is at least one valid value
						if (m_currencyValues == null)
							m_currencyValues = new Dictionary<string, int>(1);

						m_currencyValues[asVal[0]] = currencyValue;
					}
				} // foreach
		}

		/// <summary>
		/// Exchange special currency for BPs
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source is GamePlayer player && item != null && m_currencyValues != null
				&& m_currencyValues.TryGetValue(item.Id_nb, out int value) && value > 0)
			{
				player.GainBountyPoints(item.Count * value);
				player.Inventory.RemoveItem(item);
				return true;
			}

			return base.ReceiveItem(source, item);
		}

		protected override void SendMerchantWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(Catalog, eMerchantWindowType.Bp);
		}

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			var template = Catalog.GetEntry(pagenumber, slotnumber).Item;
            if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Price;

			lock (player.Inventory)
			{
				if (player.BountyPoints < totalValue)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeedBP", totalValue), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
				//Generate the buy message
				string message;
				if (number > 1)
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPiecesBP", totalValue, template.GetName(1, false));
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtBP", template.GetName(1, false), totalValue);
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

	/// <summary>
	/// A merchant that uses an item as currency instead of money
	/// </summary>
	public abstract class GameItemCurrencyMerchant : GameMerchant
	{
		public virtual string MoneyKey { get { return null; } }
		protected ItemTemplate m_itemTemplate = null;
		protected WorldInventoryItem m_moneyItem = null;
		protected static readonly Dictionary<String, int> m_currencyValues = null;

		/// <summary>
		/// The item to use as currency
		/// </summary>
		public virtual WorldInventoryItem MoneyItem
		{
			get { return m_moneyItem; }
		}

		/// <summary>
		/// The name of the money item.  Defaults to Item Name
		/// </summary>
		public virtual string MoneyItemName
		{
			get
			{
				if (m_moneyItem != null)
					return m_moneyItem.Name;

				return "not found";
			}
		}

		/// <summary>
		/// Assign templates based on MoneyKey
		/// </summary>
		public GameItemCurrencyMerchant() : base() 
		{
			if (MoneyKey != null)
			{
				m_itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(MoneyKey);

				if (m_itemTemplate != null)
					m_moneyItem = WorldInventoryItem.CreateFromTemplate(m_itemTemplate);

				// Don't waste memory on an item template we won't use.
				if (ServerProperties.Properties.BP_EXCHANGE_ALLOW == false)
					m_itemTemplate = null;
			}
		}

		/// <summary>
		/// Populate the currency exchange table
		/// </summary>
		static GameItemCurrencyMerchant()
        {
			if (ServerProperties.Properties.CURRENCY_EXCHANGE_ALLOW == true)
				foreach (string sCurrencyValue in ServerProperties.Properties.CURRENCY_EXCHANGE_VALUES.Split(';'))
				{
					string[] asVal = sCurrencyValue.Split('|');

					if (asVal.Length > 1 && int.TryParse(asVal[1], out int currencyValue) && currencyValue > 0)
					{
						// Don't create a dictionary until there is at least one valid value
						if (m_currencyValues == null)
							m_currencyValues = new Dictionary<string, int>(1);

						m_currencyValues[asVal[0]] = currencyValue;
					}
				} // foreach
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 10000);
			SendInteractMessage(player);
			return true;
		}

		protected virtual void SendInteractMessage(GamePlayer player)
		{
			string text = "";
			if (m_moneyItem == null || m_moneyItem.Item == null)
			{
				text = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.Nothing");
				ChatUtil.SendDebugMessage(player, "MoneyItem is null!");
			}
			else
			{
				text = MoneyItemName + "s";
			}

			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.BuyItemsFor", this.Name, text), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
		}

		protected override void SendMerchantWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(Catalog, eMerchantWindowType.Count);
		}

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			if (m_moneyItem == null || m_moneyItem.Item == null)
				return;
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			var template = Catalog.GetEntry(pagenumber, slotnumber).Item;
            if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Price;

			lock (player.Inventory)
			{
				var costToText = $"{totalValue} {MoneyItemName}";
				if (player.Inventory.CountItemTemplate(m_moneyItem.Item.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) < totalValue)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeedGeneric", costToText), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					return;
				}
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPiecesGeneric", amountToBuy, template.GetName(1, false), costToText);
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtGeneric", template.GetName(1, false), costToText);

				var items = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				int removed = 0;

				foreach (InventoryItem item in items)
				{
					if (item.Id_nb != m_moneyItem.Item.Id_nb)
						continue;
					int remFromStack = Math.Min(item.Count, (int)(totalValue - removed));
					player.Inventory.RemoveCountFromStack(item, remFromStack);
					InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template, remFromStack);
					removed += remFromStack;
					if (removed == totalValue)
						break;
				}

				player.Out.SendInventoryItemsUpdate(items);
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Exchange special currency for merchant currency type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source is GamePlayer player && item != null && m_currencyValues != null
				&& m_currencyValues.TryGetValue(item.Id_nb, out int receiveCost)
				&& m_currencyValues.TryGetValue(MoneyKey, out int giveCost))
			{
				int giveCount = item.Count * receiveCost / giveCost;

				if (giveCount > 0)
				{
					// Create and give new item to player
					InventoryItem newItem = GameInventoryItem.Create(m_itemTemplate);
					newItem.OwnerID = player.InternalID;
					newItem.Count = giveCount;

					if (!player.Inventory.AddTemplate(newItem, newItem.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
						player.CreateItemOnTheGround(newItem);

					// Remove received items
					InventoryItem playerItem = player.Inventory.GetItem((eInventorySlot)item.SlotPosition);
					playerItem.Count -= giveCount * giveCost;

					if (playerItem.Count < 1)
						player.Inventory.RemoveItem(item);

					return true;
				}
			}

			return base.ReceiveItem(source, item);
		}
	}

	public class GameBloodSealsMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "BloodSeal"; } }
	}

	public class GameDiamondSealsMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "DiamondSeal"; } }
	}

	public class GameSapphireSealsMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "SapphireSeal"; } }
	}

	public class GameEmeraldSealsMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "EmeraldSeal"; } }
	}

	public class GameAuruliteMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "aurulite"; } }
	}
	
	public class GameAtlanteanGlassMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "atlanteanglass"; } }
	}
	
	public class GameDragonMerchant : GameItemCurrencyMerchant
	{
		public override string MoneyKey { get { return "dragonscales"; } }
	}
}