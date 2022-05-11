namespace DOL.GS.Finance
{
    public class Money
    {
        public long Amount { get; private set; }
        public Currency Currency { get; private set; }

        private Money() { }

        public static Money Mint(long value, Currency type)
            => new Money() { Amount = value, Currency = type };

        public override bool Equals(object obj)
        {
            if (obj is Money otherCurrency)
            {
                var areOfSameValue = otherCurrency.Amount == this.Amount;
                var areOfSameType = otherCurrency.Currency.Equals(Currency);
                return areOfSameType && areOfSameValue;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}