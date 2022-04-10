using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS.Finance
{
    public enum eCurrency : byte
    {
        Copper = 1,
        ItemTemplate = 2,
        BountyPoints = 3,
        Mithril = 4
    }

    public class Currency
    {
        private static Dictionary<eCurrency, Currency> cachedTypes = new Dictionary<eCurrency, Currency>(){
            {eCurrency.Copper, new Currency(){Id = eCurrency.Copper}},
            {eCurrency.BountyPoints, new Currency(){Id = eCurrency.BountyPoints}},
            {eCurrency.Mithril, new Currency(){Id = eCurrency.Mithril}},
        };

        public eCurrency Id { get; private set; }

        protected Currency() { }

        internal static Currency Create(eCurrency currencyId)
        {
            if (cachedTypes.TryGetValue(currencyId, out var currencyType))
            {
                return currencyType;
            }
            throw new System.NotImplementedException($"Currency with id {currencyId} is not implemented.");
        }

        public Money Create(long value)
        {
            return Money.Create(value, this);
        }

        public virtual string Name
        {
            get
            {
                switch (Id)
                {
                    case eCurrency.Copper: return "copper";
                    case eCurrency.BountyPoints: return "bounty points";
                    case eCurrency.Mithril: return "mithril";
                }
                return $"<{Id}>";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Currency currencyType) return currencyType.Id == Id;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    internal class ItemCurrency : Currency
    {
        private static Dictionary<string, ItemCurrency> cachedCurrencyItems = new Dictionary<string, ItemCurrency>();

        public ItemTemplate Item { get; private set; }

        internal static ItemCurrency Create(ItemTemplate item)
        {
            if (cachedCurrencyItems.TryGetValue(item.Id_nb, out var cachedItemCurrency))
            {
                return cachedItemCurrency;
            }
            var newItemCurrency = new ItemCurrency() { Item = item };
            cachedCurrencyItems[item.Id_nb] = newItemCurrency;
            return newItemCurrency;
        }

        public static ItemCurrency CreateFromItemTemplateId(string itemTemplateId)
        {
            var itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplateId);
            if (itemTemplate == null)
            {
                itemTemplate = new ItemTemplate() { Id_nb = itemTemplateId, Name = itemTemplateId };
            }
            return Create(itemTemplate);
        }

        public override string Name
            => Item.Name;

        public override bool Equals(object obj)
        {
            if (obj is ItemCurrency itemCurrency) return itemCurrency.Item.Id_nb.ToLower() == Item.Id_nb.ToLower();
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}