using System;

namespace DOL.GS.Finance
{
    public abstract class Currency
    {
        public virtual string Name { get; }
        public bool IsItemCurrency => this is ItemCurrency;

        protected Currency() { }

        public static Currency Copper { get; } = new CopperCurrency();
        public static Currency BountyPoints { get; } = new BountyPointsCurrency();
        public static Currency Mithril { get; } = new MithrilCurrency();
        public static Currency Item(string currencyId) => new ItemCurrency(currencyId);

        public Money Mint(long value) => Money.Mint(value, this);

        public virtual string ToText() => Name;

        public override bool Equals(object obj)
            => obj.GetType().Equals(this.GetType());

        public override int GetHashCode() => base.GetHashCode();

        #region Currency implementations
        private class CopperCurrency : Currency
        {
            public override string Name => "money";
        }

        private class BountyPointsCurrency : Currency
        {
            public override string Name => "bounty points";
        }

        private class MithrilCurrency : Currency
        {
            public override string Name => "Mithril";
        }

        private class ItemCurrency : Currency
        {
            private readonly string id;

            public override string Name => id;

            public ItemCurrency(string id)
            {
                if (string.IsNullOrEmpty(id)) throw new ArgumentException("The ID of an ItemCurrency may not be null nor empty.");
                this.id = id.ToLower();
            }

            public override string ToText() => $"units of {Name}";

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