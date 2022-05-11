using System;
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS.Finance
{
    public abstract class Currency
    {
        protected Currency() { }

        public static Currency Create(byte currencyId, string itemTemplateId = null)
        {
            switch (currencyId)
            {
                case 1: return Copper;
                case 2: return ItemCurrency.CreateFromItemTemplateId(itemTemplateId);
                case 3: return BountyPoints;
                case 4: return Mithril;
                default: throw new System.NotImplementedException($"Currency with id {currencyId} is not implemented.");
            }
        }

        public static Currency Copper { get; } = new Copper();
        public static Currency BountyPoints { get; } = new BountyPoints();
        public static Currency Mithril { get; } = new Mithril();
        public static Currency Item(ItemTemplate itemTemplate) => ItemCurrency.Create(itemTemplate);
        public static Currency Item(string itemTemplateId) => ItemCurrency.CreateFromItemTemplateId(itemTemplateId);

        public Money Mint(long value) => Money.Mint(value, this);

        public abstract string Name { get; }

        public override bool Equals(object obj)
        {
            return obj.GetType().Equals(this.GetType());
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    internal class Copper : Currency
    {
        public override string Name => "copper";
    }

    internal class BountyPoints : Currency
    {
        public override string Name => "bounty points";
    }

    internal class Mithril : Currency
    {
        public override string Name => "mithril";
    }

    internal class ItemCurrency : Currency
    {
        private static Dictionary<string, ItemCurrency> cachedCurrencyItems = new Dictionary<string, ItemCurrency>();

        public new ItemTemplate Item { get; private set; }

        public static ItemCurrency Create(ItemTemplate itemTemplate)
        {
            if (cachedCurrencyItems.TryGetValue(itemTemplate.Id_nb, out var cachedItemCurrency))
            {
                return cachedItemCurrency;
            }
            var newCurrencyItem = new ItemCurrency() { Item = itemTemplate };
            cachedCurrencyItems[itemTemplate.Id_nb] = newCurrencyItem;
            return newCurrencyItem;
        }

        public static ItemCurrency CreateFromItemTemplateId(string itemTemplateId)
        {
            if(string.IsNullOrEmpty(itemTemplateId)) throw new ArgumentException("An ItemCurrency's ItemTemplateID may not be null nor empty.");
            
            if (cachedCurrencyItems.TryGetValue(itemTemplateId, out var cachedItemCurrency))
            {
                return cachedItemCurrency;
            }
            var itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplateId);
            if (itemTemplate == null)
            {
                itemTemplate = new ItemTemplate() { Id_nb = itemTemplateId, Name = itemTemplateId };
            }
            var newCurrencyItem = new ItemCurrency() { Item = itemTemplate };
            cachedCurrencyItems[itemTemplate.Id_nb] = newCurrencyItem;
            return newCurrencyItem;
        }

        public override string Name
            => Item.Name;

        public override bool Equals(object obj)
        {
            if (obj is ItemCurrency itemCurrency) return itemCurrency.Item.Id_nb == Item.Id_nb;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}