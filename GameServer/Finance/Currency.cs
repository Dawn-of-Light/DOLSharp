using System;
using System.Collections.Generic;

namespace DOL.GS.Finance
{
    public abstract class Currency
    {
        protected Currency() { }

        public static Currency Copper { get; } = new CopperCurrency();
        public static Currency BountyPoints { get; } = new BountyPointsCurrency();
        public static Currency Mithril { get; } = new MithrilCurrency();
        public static Currency Item(string currencyId) => ItemCurrency.Create(currencyId);

        public Money Mint(long value) => Money.Mint(value, this);

        public abstract string ToText();

        public bool IsItemCurrency => this is ItemCurrency;

        public override bool Equals(object obj)
        {
            return obj.GetType().Equals(this.GetType());
        }

        public override int GetHashCode() => base.GetHashCode();

        #region Currency implementations
        private class CopperCurrency : Currency
        {
            public override string ToText() => "money";
        }

        private class BountyPointsCurrency : Currency
        {
            public override string ToText() => "bounty points";
        }

        private class MithrilCurrency : Currency
        {
            public override string ToText() => "Mithril";
        }

        private class ItemCurrency : Currency
        {
            private static Dictionary<string, ItemCurrency> cachedCurrencyItems = new Dictionary<string, ItemCurrency>();

            private string Id { get; set; }

            internal static ItemCurrency Create(string currencyId)
            {
                if (string.IsNullOrEmpty(currencyId)) throw new ArgumentException("The ID of an ItemCurrency may not be null nor empty.");

                if (cachedCurrencyItems.TryGetValue(currencyId, out var cachedItemCurrency))
                {
                    return cachedItemCurrency;
                }
                var newCurrencyItem = new ItemCurrency() { Id = currencyId };
                cachedCurrencyItems[currencyId] = newCurrencyItem;
                return newCurrencyItem;
            }

            public override string ToText() => $"units of {Id}";

            public override bool Equals(object obj)
            {
                if (obj is ItemCurrency itemCurrency) return itemCurrency.Id == Id;
                return false;
            }

            public override int GetHashCode() => base.GetHashCode();
        }
        #endregion
    }
}