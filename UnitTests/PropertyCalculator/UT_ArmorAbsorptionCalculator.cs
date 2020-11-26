using NUnit.Framework;
using DOL.GS;
using DOL.GS.PropertyCalc;

namespace DOL.UnitTests.Gameserver.PropertyCalc
{
    [TestFixture]
    class UT_ArmorAbsorptionCalculator
    {
        [Test]
        public void CalcValue_FreshNPC_Minus4()
        {
            var absorbCalc = new ArmorAbsorptionCalculator();

            int actual = absorbCalc.CalcValue(NewNPC(), ArmorAbsorptionProperty);

            Assert.AreEqual(-4, actual);
        }

        [Test]
        public void CalcValue_L30NPCwith60BaseAnd120BaseBuffConstitution_30()
        {
            var npc = NewNPC();
            npc.Constitution = 60;
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 120;
            var absorbCalc = new ArmorAbsorptionCalculator();

            int actual = absorbCalc.CalcValue(npc, ArmorAbsorptionProperty);

            Assert.AreEqual(30, actual);
        }

        [Test]
        public void CalcValue_L30NPCWith60Constitution_27()
        {
            var npc = NewNPC();
            npc.Level = 30;
            npc.Constitution = 60;
            var absorbCalc = new ArmorAbsorptionCalculator();

            int actual = absorbCalc.CalcValue(npc, ArmorAbsorptionProperty);

            Assert.AreEqual(27, actual);
        }

        [Test]
        public void CalcValue_FreshNPCWith50ConDebuff_Minus25()
        {
            var npc = NewNPC();
            npc.Constitution = 60;
            npc.DebuffCategory[eProperty.Constitution] = 50;

            int actual = ArmorAbsorptionCalculator.CalcValue(npc, ArmorAbsorptionProperty);

            Assert.AreEqual(-25, actual);
        }

        private ArmorAbsorptionCalculator ArmorAbsorptionCalculator => new ArmorAbsorptionCalculator();
        private FakeNPC NewNPC() => new FakeNPC();
        private eProperty ArmorAbsorptionProperty => eProperty.ArmorAbsorption;
    }
}
