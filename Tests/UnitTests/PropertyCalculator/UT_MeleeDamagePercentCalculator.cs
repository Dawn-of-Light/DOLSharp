using NUnit.Framework;
using DOL.GS;
using DOL.GS.PropertyCalc;

namespace DOL.UnitTests.Gameserver.PropertyCalc
{
    [TestFixture]
    class UT_MeleeDamagePercentCalculator
    {
        [Test]
        public void CalcValue_50StrengthBuff_6()
        {
            var npc = NewNPC();
            npc.BaseBuffBonusCategory[eProperty.Strength] = 50;

            int actual = MeleeDamageBonusCalculator.CalcValue(npc, MeleeDamageProperty);

            Assert.That(actual, Is.EqualTo(6));
        }

        [Test]
        public void CalcValue_NPCWith50StrengthDebuff_Minus6()
        {
            var npc = NewNPC();
            npc.DebuffCategory[eProperty.Strength] = 50;

            int actual = MeleeDamageBonusCalculator.CalcValue(npc, MeleeDamageProperty);

            Assert.That(actual, Is.EqualTo(-6));
        }

        private MeleeDamagePercentCalculator MeleeDamageBonusCalculator => new MeleeDamagePercentCalculator();
        private eProperty MeleeDamageProperty => eProperty.MeleeDamage;
        private FakeNPC NewNPC() => new FakeNPC();
    }
}
