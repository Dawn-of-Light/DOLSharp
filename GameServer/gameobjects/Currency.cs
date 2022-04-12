using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS.Finance
{
    public abstract class Currency
    {
        private enum eCurrency : byte
        {
            Copper = 1,
            ItemTemplate = 2,
            BountyPoints = 3,
            Mithril = 4
        }

        private static Dictionary<eCurrency, Currency> cachedTypes = new Dictionary<eCurrency, Currency>(){
            {eCurrency.Copper, new Copper()},
            {eCurrency.BountyPoints, new BountyPoints()},
            {eCurrency.Mithril, new Mithril()},
        };

        protected Currency() { }

        private static Currency Create(eCurrency currencyId)
        {
            if (cachedTypes.TryGetValue(currencyId, out var currencyType))
            {
                return currencyType;
            }
            throw new System.NotImplementedException($"Currency with id {currencyId} is not implemented.");
        }

        public static Currency Create(byte currencyId)
            => Create((eCurrency)currencyId);

        public static byte ItemCurrencyId => (byte)eCurrency.ItemTemplate;

        public static Currency Copper => Create(eCurrency.Copper);
        public static Currency BountyPoints => Create(eCurrency.BountyPoints);
        public static Currency Mithril => Create(eCurrency.Mithril);
        public static Currency Item(ItemTemplate item) => ItemCurrency.Create(item);

        public Money Mint(long value) => Money.Mint(value, this);

        public abstract string Name { get; }

        public override bool Equals(object obj)
        {
            return obj.GetType().Equals(this.GetType());
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public class Copper : Currency
    {
        public override string Name => "copper";
    }

    public class BountyPoints : Currency
    {
        public override string Name => "bounty points";
    }

    public class Mithril : Currency
    {
        public override string Name => "mithril";
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