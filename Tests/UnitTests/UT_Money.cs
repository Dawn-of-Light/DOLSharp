using DOL.GS.Finance;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_Money
    {
        [Test]
        public void ToText_ZeroCopper_0copper()
        {
            var actual = Currency.Copper.Mint(0).ToText();

            var expected = "0 copper";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToText_2Copper_2copper()
        {
            var actual = Currency.Copper.Mint(2).ToText();

            var expected = "2 copper";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToText_202Copper_2silverAnd2copper()
        {
            var actual = Currency.Copper.Mint(202).ToText();

            var expected = "2 silver and 2 copper";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToText_20202_2gold2silverAnd2copper()
        {
            var actual = Currency.Copper.Mint(20202).ToText();

            var expected = "2 gold, 2 silver and 2 copper";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}