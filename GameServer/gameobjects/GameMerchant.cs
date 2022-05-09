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
using System.Linq;
using System.Threading;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.GS.Finance;
using DOL.GS.Profession;

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
			get => Catalog.IsEmpty ? null : Catalog.ConvertToMerchantTradeItems();
			set
			{
				if(value != null) Catalog = value.Catalog; 
			}
		}

		public MerchantCatalog Catalog { get; set; } = MerchantCatalog.Create();

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
                if (currency.Equals(Currency.Copper) || currency.Equals(Currency.BountyPoints) || currency is ItemCurrency)
                {
                    var costToText = CurrencyToText(currency.Mint(cost));
                    if (!HasPlayerEnoughBalance(player, currency.Mint(cost)))
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
                    WithdrawMoneyFromPlayer(player, currency.Mint(cost));
                    player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);

                }
                else if (currency.Equals(Currency.Mithril)) throw new NotImplementedException("Mithril is currently not implemented as a separate currency.");
                else throw new ArgumentException($"{currency} is not implemented.");
            }
        }

        private void WithdrawMoneyFromPlayer(GamePlayer player, Finance.Money price)
        {
			if (!player.Wallet.RemoveMoney(price)) throw new Exception("Money amount changed while adding items.");
        }

        private bool HasPlayerEnoughBalance(GamePlayer player, Finance.Money price)
        {
			return player.Wallet.GetBalance(price.Currency) >= price.Amount;
        }

        private string CurrencyToText(Finance.Money money)
        {
            return money.ToText();
        }

		public static void OnPlayerBuy(GamePlayer player, int item_slot, int number, MerchantTradeItems TradeItems)
		{
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			long totalCurrencyAmount = number * template.Price;

			lock (player.Inventory)
			{
				if (player.GetCurrentMoney() < totalCurrencyAmount)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalCurrencyAmount)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalCurrencyAmount));
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalCurrencyAmount));

				// Check if player has enough money and subtract the money
				if (!player.RemoveMoney(totalCurrencyAmount, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					throw new Exception("Money amount changed while adding items.");
				}
				InventoryLogging.LogInventoryAction(player, "(TRADEITEMS;" + TradeItems.ItemsListID + ")", eInventoryActionType.Merchant, totalCurrencyAmount);
			}
		}

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
		public override void LoadFromDatabase(DataObject merchantobject)
		{
			base.LoadFromDatabase(merchantobject);
			if (!(merchantobject is Mob)) return;
			Mob merchant = (Mob)merchantobject;
			if (string.IsNullOrEmpty(merchant.ItemsListTemplateID) == false)
				Catalog = MerchantCatalog.LoadFromDatabase(merchant.ItemsListTemplateID);
		}

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
			merchant.ItemsListTemplateID = Catalog.ItemListId;

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

    [Obsolete("Use GameMerchant instead. "
        + "Set currency in database. " 
        + "Adjust exchange rates by setting the amount/price for another currency item.")]
	public abstract class GameItemCurrencyMerchant : GameMerchant
	{
        protected virtual Currency Currency {get; private set;}
        private static Dictionary<Currency, int> currencyExchangeRates = null;
		private static Dictionary<Currency, ItemTemplate> defaultCurrencyItems = new Dictionary<Currency, ItemTemplate>();
        
        public override void LoadTemplate(INpcTemplate template)
        {
            base.LoadTemplate(template);
            SetCurrencyForAllPages();
        }

        public override void LoadFromDatabase(DataObject merchantobject)
        {
            base.LoadFromDatabase(merchantobject);
            SetCurrencyForAllPages();
        }

        private void SetCurrencyForAllPages()
        {
            foreach (var page in Catalog.GetAllPages()) page.SetCurrency(Currency);
        }

		static GameItemCurrencyMerchant()
        {
            LoadExchangeRates();
		}

        private static void LoadExchangeRates()
        {
			if (ServerProperties.Properties.CURRENCY_EXCHANGE_ALLOW)
            {
				foreach (string currencyExchangePair in ServerProperties.Properties.CURRENCY_EXCHANGE_VALUES.Split(';'))
				{
					string[] asVal = currencyExchangePair.Split('|');

					if (asVal.Length > 1 && int.TryParse(asVal[1], out int currencyValue) && currencyValue > 0)
					{
						if (currencyExchangeRates == null)
							currencyExchangeRates = new Dictionary<Currency, int>(1);

						currencyExchangeRates[Currency.Item(asVal[0])] = currencyValue;
					}
				}
            }
        }

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			SendInteractMessage(player);
			return true;
		}

		protected virtual void SendInteractMessage(GamePlayer player)
		{
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.BuyItemsFor", this.Name, Currency.ToText()), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
		}

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
			Currency receivedCurrency = null;
            if (item != null && string.IsNullOrEmpty(item.ClassType) == false)
            {
				receivedCurrency = Currency.Item(item.ClassType.ToLower().Replace("currency.", ""));
            }
            if (source is GamePlayer player && receivedCurrency != null && currencyExchangeRates != null
                && currencyExchangeRates.TryGetValue(receivedCurrency, out int exchangeCurrencyValue)
                && currencyExchangeRates.TryGetValue(Currency, out int referenceCurrencyValue))
            {
                int giveCount = item.Count * exchangeCurrencyValue / referenceCurrencyValue;

                if (giveCount > 0)
                {
                    // Create and give new item to player
                    InventoryItem newItem = GameInventoryItem.Create(GetDefaultCurrencyItem(Currency));
                    newItem.OwnerID = player.InternalID;
                    newItem.Count = giveCount;

                    if (!player.Inventory.AddTemplate(newItem, newItem.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                        player.CreateItemOnTheGround(newItem);

                    // Remove received items
                    InventoryItem playerItem = player.Inventory.GetItem((eInventorySlot)item.SlotPosition);
                    playerItem.Count -= giveCount * referenceCurrencyValue;

                    if (playerItem.Count < 1)
                        player.Inventory.RemoveItem(item);

                    return true;
                }
            }
        	return base.ReceiveItem(source, item);
		}

        private ItemTemplate GetDefaultCurrencyItem(Currency currency)
        {
            if (defaultCurrencyItems.TryGetValue(currency, out var currencyItem))
            {
                return currencyItem;
            }
            else
            {
                var allCurrencyItems = DOLDB<ItemTemplate>.SelectObjects(DB.Column("ClassType").IsLike("Currency.%"));
                foreach (var item in allCurrencyItems)
                {
                    currency.Equals(Currency.Item(item.ClassType.ToLower().Replace("currency.", "")));
                	defaultCurrencyItems[currency] = item;
					return item;
                }
                throw new NullReferenceException($"No currency item for <{currency.ToText()}> was found.");
            }
        }
	}

    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameBountyMerchant : GameItemCurrencyMerchant
	{
		protected static Dictionary<string, int> exchangeRatesToBountyPoints { get; private set; } = null;

        protected override Currency Currency => Currency.BountyPoints;

		static GameBountyMerchant()
        {
            LoadExchangeRates();
		}

        private static void LoadExchangeRates()
        {
			if (ServerProperties.Properties.BP_EXCHANGE_ALLOW)
            {
                foreach (string sCurrencyValue in ServerProperties.Properties.BP_EXCHANGE_VALUES.Split(';'))
                {
                    string[] asVal = sCurrencyValue.Split('|');

                    if (asVal.Length > 1 && int.TryParse(asVal[1], out int currencyValue) && currencyValue > 0)
                    {
                        if (exchangeRatesToBountyPoints == null)
                            exchangeRatesToBountyPoints = new Dictionary<string, int>(1);

                        exchangeRatesToBountyPoints[asVal[0]] = currencyValue;
                    }
                }
            }
		}

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
            if (source is GamePlayer player && item != null && exchangeRatesToBountyPoints != null
                && exchangeRatesToBountyPoints.TryGetValue(item.Id_nb, out int value) && value > 0)
            {
                player.GainBountyPoints(item.Count * value);
                player.Inventory.RemoveItem(item);
                return true;
            }
        	return base.ReceiveItem(source, item);
		}
	}

    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameBloodSealsMerchant : GameItemCurrencyMerchant
	{
        protected override Currency Currency 
            => Currency.Item("BloodSeal");
	}

    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameDiamondSealsMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("DiamondSeal");
	}

    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameSapphireSealsMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("SapphireSeal");
	}

	[Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
    public class GameEmeraldSealsMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("EmeraldSeal");
	}

    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameAuruliteMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("aurulite");
	}
	
    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameAtlanteanGlassMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("atlanteanglass");
	}
	
    [Obsolete("This is going to be removed. See GameItemCurrencyMerchant's obsolete message for more details.")]
	public class GameDragonMerchant : GameItemCurrencyMerchant
	{
		protected override Currency Currency 
            => Currency.Item("dragonscales");
	}
}