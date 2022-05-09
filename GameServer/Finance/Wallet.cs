using System;
using System.Collections.Generic;
using System.Linq;

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
            long balance;
            if(currency.IsItemCurrency) 
            {
                lock(owner.Inventory)
                {
                    return owner.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)
                        .Where(i => currency.Equals(Currency.Item(i.ClassType.ToLower().Replace("currency.",""))))
                        .Aggregate(0, (acc,i) => acc + i.Count);
                }
            }
            balances.TryGetValue(currency, out balance);
            return balance;
        }

        public void AddMoney(Money money)
        {
            if(money.Currency.IsItemCurrency)
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
            if (money.Currency.IsItemCurrency)
            {
                lock (owner.Inventory)
                {
                    if(GetBalance(money.Currency) < money.Amount) return false;

                    var validCurrencyItemsInventory = owner.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)
                        .Where(i => money.Currency.Equals(Currency.Item(i.ClassType.ToLower().Replace("currency.",""))));
                    var remainingDue = (int)money.Amount;
                    foreach (var currencyItem in validCurrencyItemsInventory)
                    {
                        if (currencyItem.Count > remainingDue)
                        {
                            owner.Inventory.RemoveCountFromStack(currencyItem, remainingDue);
                            break;
                        }
                        else if (currencyItem.Count == remainingDue)
                        {
                            owner.Inventory.RemoveItem(currencyItem);
                            break;
                        }
                        else
                        {
                            remainingDue -= currencyItem.Count;
                            owner.Inventory.RemoveItem(currencyItem);
                        }
                    }
                    return true;
                }
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