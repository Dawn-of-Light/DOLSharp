using NUnit.Framework;
using DOL.GS;
using DOL.GS.PropertyCalc;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_StatCalculator
    {
        [Test]
        public void CalcValueFromBuffs_GameNPCWith100ConstBaseBuff_Return100()
        {
            GameNPC npc = Create.NPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(npc, eProperty.Constitution);

            Assert.AreEqual(100, actual);
        }

        [Test]
        public void CalcValueFromBuffs_Level50PlayerWith100ConstBaseBuff_ReturnCapAt62()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.BaseBuffBonusCategory[eProperty.Constitution] = 100;
            var statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(player, eProperty.Constitution);

            int expected = (int)(50 * 1.25);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValueFromBuffs_Level50PlayerWith100ConstSpecBuff_ReturnCapAt93()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 100;
            var statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(player, eProperty.Constitution);

            int expected = (int)(50 * 1.875);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValueFromBuffs_BaseBuff3AndSpecBuff4_Return7()
        {
            GameNPC npc = Create.NPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 3;
            npc.SpecBuffBonusCategory[eProperty.Constitution] = 4;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(npc, eProperty.Constitution);

            int expected = 3 + 4;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValueFromBuffs_LivingIsNull_ReturnZero()
        {
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(null, eProperty.Constitution);
            
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void CalcValueFromItems_LivingIsNull_ReturnZero()
        {
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(null, eProperty.Constitution);

            Assert.AreEqual(0, actual);
        }

        [Test]
        public void CalcValueFromItems_Level50Player100ConstFromItems_ReturnCapAt75()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            //eProperty casted to int, otherwise method calls wrong value
            player.ItemBonus[(int)eProperty.Constitution] = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(player, eProperty.Constitution);

            int expected = (int)(1.5 * 50);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValue_GameNPCWith100Constitution_Return100()
        {
            GameNPC npc = Create.NPC();
            npc.Constitution = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(npc, eProperty.Constitution);

            Assert.AreEqual(100, actual);
        }

        [Test]
        public void CalcValue_GameNPCWith100Intelligence_Return100()
        {
            GameNPC npc = Create.NPC();
            npc.Intelligence = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(npc, eProperty.Intelligence);

            Assert.AreEqual(100, actual);
        }

        public static StatCalculator createStatCalculator()
        {
            return new StatCalculator();
        }
    }
}
