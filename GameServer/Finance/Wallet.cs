using System;
using System.Collections.Generic;

namespace DOL.GS.Finance
{
    public class Wallet
    {
        private readonly Dictionary<Currency, long> balances = new Dictionary<Currency, long>();
        private GamePlayer owner;

        public Wallet() { }

        public Wallet(GamePlayer owner) 
        { 
            this.owner = owner;
        }

        public long GetBalance(Currency currency)
        {
            if(currency is ItemCurrency itemCurrency) 
            {
                return owner.Inventory.CountItemTemplate(itemCurrency.Item.Id_nb,eInventorySlot.FirstBackpack,eInventorySlot.LastBackpack);
            }
            balances.TryGetValue(currency, out long balance);
            return balance;
        }

        public void AddMoney(Money money)
        {
            if(money.Currency is ItemCurrency)
            {
                throw new ArgumentException("You cannot add money of type ItemCurrency.");
            }
            lock (balances)
            {
                var oldBalance = GetBalance(money.Currency);
                var newBalance = oldBalance + money.Amount;
                SetBalance(money.Currency.Mint(newBalance));
                SaveToDatabase();
            }
        }

        public bool RemoveMoney(Money money)
        {
            if(money.Currency is ItemCurrency itemCurrency)
            {
                var inventoryCurrencyItem = new GameInventoryItem(itemCurrency.Item);
                return owner.Inventory.RemoveTemplate(inventoryCurrencyItem.Id_nb, (int)money.Amount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
            }
            lock (balances)
            {
                var oldBalance = GetBalance(money.Currency);
                if (oldBalance < money.Amount) return false;
                var newBalance = oldBalance - money.Amount;
                SetBalance(money.Currency.Mint(newBalance));
                SaveToDatabase();
                return true;
            }
        }

        public void SetBalance(Money money)
        {
            lock (balances)
            {
                if (money.Amount == 0) balances.Remove(money.Currency);
                else balances[money.Currency] = money.Amount;
                UpdateCurrencyStatus(money.Currency);
            }
        }

        public void InitializeFromDatabase()
        {
            var dbCharacter = owner.DBCharacter;
            var initialCopperBalance = DOL.GS.Money.GetMoney(dbCharacter.Mithril,dbCharacter.Platinum,dbCharacter.Gold,dbCharacter.Silver,dbCharacter.Copper);
			SetBalance(Currency.Copper.Mint(initialCopperBalance));
            var initialBountyPoints = dbCharacter.BountyPoints;
            SetBalance(Currency.BountyPoints.Mint(initialBountyPoints));
        }

        public void SaveToDatabase()
        {
            if(owner == null || owner.DBCharacter == null) return;
            var dbCharacter = owner.DBCharacter;

			dbCharacter.Copper = owner.Copper;
			dbCharacter.Silver = owner.Silver;
			dbCharacter.Gold = owner.Gold;
			dbCharacter.Platinum = owner.Platinum;
			dbCharacter.Mithril = owner.Mithril;
            dbCharacter.BountyPoints = GetBalance(Currency.BountyPoints);
        }

        private void UpdateCurrencyStatus(Currency currency)
        {
            if(owner != null && owner.Out != null)
            {
                if(currency.Equals(Currency.Copper)) owner.Out.SendUpdateMoney();
                else if(currency.Equals(Currency.BountyPoints)) owner.Out.SendUpdatePoints();
            }
        }
    }
}