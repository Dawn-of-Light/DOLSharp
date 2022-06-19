using System;
using System.Linq;
using System.Text;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Finance
{
    public class Money
    {
        public long Amount { get; }
        public virtual Currency Currency { get; }

        protected Money(long amount, Currency currency)
        {
            if(amount < 0) throw new ArgumentException("You cannot mint negative Money.");
            Amount = amount;
            Currency = currency;
        }

        internal static Money Mint(long amount, Currency currency)
            => new Money(amount, currency);

        public string ToText()
        {
            if (Currency.Equals(Currency.Copper))
            {
                if (Amount == 0) return $"0 {Translate("Money.GetString.Text6", "copper")}";
                var copperPart = $"{Amount % 100} {Translate("Money.GetString.Text6", "copper")}";
                var silverPart = $"{(Amount / 100) % 100} {Translate("Money.GetString.Text5", "silver")}";
                var goldPart = $"{(Amount / 100 / 100) % 1000} {Translate("Money.GetString.Text4", "gold")}";
                var platinumPart = $"{(Amount / 100 / 100 / 1000)} {Translate("Money.GetString.Text3", "platinum")}";
                var moneyParts = new[] { platinumPart, goldPart, silverPart, copperPart }
                    .Where(p => !p.StartsWith("0")).ToArray();
                if (moneyParts.Length == 1) return moneyParts[0];
                else return string.Join(" and ", new[] { string.Join(", ", moneyParts.Take(moneyParts.Length - 1)), moneyParts.Last() });
            }

            return $"{Amount} {Currency.ToText()}";
        }

        public string ToAbbreviatedText()
        {
            if (Currency.Equals(Currency.Copper))
            {
                if (Amount == 0) return $"0 c";
                var result = new StringBuilder();
                var copperPart = $"{Amount % 100} c";
                var silverPart = $"{(Amount / 100) % 100} s";
                var goldPart = $"{(Amount / 100 / 100) % 1000} g";
                var platinumPart = $"{(Amount / 100 / 100 / 1000)} p";
                var moneyParts = new[] { platinumPart, goldPart, silverPart, copperPart }
                    .Where(p => !p.StartsWith("0")).ToArray();
                return string.Join(" ", moneyParts);
            }
            return ToText();
        }

        private string Translate(string translationId, string defaultTranslation)
        {
            var translation = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, translationId);
            var translationSubId = translationId.Split('.').Last();
            if (translation.Trim() == translationSubId) return defaultTranslation;
            else return translation;
        }

        public override bool Equals(object obj)
        {
            if (obj is Money otherMoney)
            {
                var areOfSameValue = otherMoney.Amount == this.Amount;
                var areOfSameType = otherMoney.Currency.Equals(Currency);
                return areOfSameType && areOfSameValue;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => ToText();
    }
}