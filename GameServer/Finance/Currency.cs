using System;
using DOL.Database;

namespace DOL.GS.Finance
{
    public abstract class Currency
    {
        public bool IsItemCurrency => this is ItemCurrency;

        protected Currency() { }

        public static Currency Copper { get; } = new CopperCurrency();
        public static Currency BountyPoints { get; } = new BountyPointsCurrency();
        public static Currency Mithril { get; } = new MithrilCurrency();
        public static Currency Item(string currencyId, string textRepresentation = "") => new ItemCurrency(currencyId, textRepresentation);

        public Money Mint(long value) => Money.Mint(value, this);

        public abstract string ToText();

        public bool IsSameCurrencyItem(ItemTemplate item){
            if (this is ItemCurrency itemCurrency)
            {
                var isEquivalentItem = item.ClassType.ToLower() == $"currency.{itemCurrency.ID.ToLower()}";
                var isOriginalItem = item.Id_nb.ToLower() == itemCurrency.ID.ToLower();
                return isEquivalentItem || isOriginalItem;
            }
            return false;
        }

        public override bool Equals(object obj)
            => obj.GetType().Equals(this.GetType());

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
            private readonly string id;
            private readonly string textRepresentation;

            public string ID => id;

            public ItemCurrency(string id, string textRepresentation)
            {
                if (string.IsNullOrEmpty(id)) throw new ArgumentException("The ID of an ItemCurrency may not be null nor empty.");
                this.id = id.ToLower();
                this.textRepresentation = textRepresentation;
            }

            public override string ToText() => textRepresentation == "" ? id : textRepresentation;

            public override bool Equals(object obj)
            {
                if (obj is ItemCurrency itemCurrency) return itemCurrency.id == id;
                return false;
            }

            public override int GetHashCode() => id.GetHashCode();
        }
        #endregion
    }
}