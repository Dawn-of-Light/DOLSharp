using NUnit.Framework;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PropertyCalc;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_StatCalculator
    {
        [Test]
        public void CalcValueFromBuffs_GameNPCWith100ConstBaseBuff_Return100()
        {
            var npc = Create.FakeNPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(npc, eProperty.Constitution);

            if (Properties.MOB_BUFF_EFFECT_MULTIPLIER > 0)
                Assert.AreEqual(0, actual);
            else
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
            var npc = Create.FakeNPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 3;
            npc.SpecBuffBonusCategory[eProperty.Constitution] = 4;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromBuffs(npc, eProperty.Constitution);

            int expected = 3 + 4;
            if (Properties.MOB_BUFF_EFFECT_MULTIPLIER > 0)
                expected = 0;

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
            player.ItemBonus[eProperty.Constitution] = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(player, eProperty.Constitution);

            int expected = (int)(1.5 * 50);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValueFromItems_Level50NPC100ConstFromItems_ReturnCapAt75()
        {
            var stat = eProperty.Constitution;
            var npc = Create.FakeNPC();
            npc.Level = 50;
            npc.ItemBonus[stat] = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(npc, stat);

            int expected = (int)(1.5 * 50);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValueFromItems_IntelligenceOfLevel50AnimistWith50AcuityFromItems_Return50()
        {
            var player = Create.FakePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(player, eProperty.Intelligence);
            
            Assert.AreEqual(50, actual);
        }

        [Test]
        public void CalcValueFromItems_Level50Player150ConAnd100MythicalConCap_ReturnCapAt127()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(player, eProperty.Constitution);

            Assert.AreEqual(127, actual);
        }

        [Test]
        public void CalcValueFromItems_Level50PlayerWith5MythicalConCap100ConCap_ReturnCapAt106()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 5;
            player.ItemBonus[eProperty.ConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValueFromItems(player, eProperty.Constitution);

            Assert.AreEqual(106, actual);
        }

        [Test]
        public void GetItemBonusCapIncrease_Level50Player100ConstCap_ReturnCapAt26()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.ConCapBonus] = 100;

            int actual = StatCalculator.GetItemBonusCapIncrease(player, eProperty.Constitution);

            int expected = (int)(50 / 2.0 + 1);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetItemBonusCapIncrease_Level50Player10ConstCap_Return10()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.ConCapBonus] = 10;

            int actual = StatCalculator.GetItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.AreEqual(10, actual);
        }

        [Test]
        public void GetMythicalItemBonusCapIncrease_PlayerWith100MythicalConCap_ReturnCapAt52()
        {
            var player = Create.FakePlayer();
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;

            int actual = StatCalculator.GetMythicalItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.AreEqual(52, actual);
        }

        [Test]
        public void GetMythicalItemBonusCapIncrease_PlayerWith10MythicalConCap_Return10()
        {
            var player = Create.FakePlayer();
            player.ItemBonus[eProperty.MythicalConCapBonus] = 10;

            int actual = StatCalculator.GetMythicalItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.AreEqual(10, actual);
        }

        [Test]
        public void CalcValue_NPCWith100Constitution_Return100()
        {
            var npc = Create.FakeNPC();
            npc.Constitution = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(npc, eProperty.Constitution);

            Assert.AreEqual(100, actual);
        }

        [Test]
        public void CalcValue_NPCWith100Intelligence_Return100()
        {
            var npc = Create.FakeNPC();
            npc.Intelligence = 100;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(npc, eProperty.Intelligence);

            Assert.AreEqual(100, actual);
        }

        [Test]
        public void CalcValue_GetIntelligenceFromLevel50AnimistWith50Acuity_Return50()
        {
            var player = Create.FakePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.BaseBuffBonusCategory[(int)eProperty.Acuity] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Intelligence);

            Assert.AreEqual(50, actual);
        }

        [Test]
        public void CalcValue_200ConstitutionAbilityBonus_Return200()
        {
            var player = Create.FakePlayer();
            player.AbilityBonus[eProperty.Constitution] = 200;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            Assert.AreEqual(200, actual);
        }

        [Test]
        public void CalcValue_200ConstitutionDebuff_Return1()
        {
            var player = Create.FakePlayer();
            player.DebuffCategory[eProperty.Constitution] = 200;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void CalcValue_200ConAbilityBonusAnd50ConDebuff_Return200()
        {
            var player = Create.FakePlayer();
            player.AbilityBonus[eProperty.Constitution] = 200;
            player.DebuffCategory[eProperty.Constitution] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            Assert.AreEqual(200 - (50/2), actual);
        }

        [Test]
        public void CalcValue_70ConBuffBonusAnd50ConDebuff_Return20()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            Assert.AreEqual(20, actual);
        }

        [Test]
        public void CalcValue_70ConItemBonusAnd50ConDebuff_Return45()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.Constitution] = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            int expected = 70 - (50 / 2);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValue_70ConBaseStatAnd50ConDebuff_Return45()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.baseStat = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);

            int expected = 70 - (50 / 2);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalcValue_70ConBaseStatAnd3ConLostOnDeath_Return67()
        {
            var player = Create.FakePlayer();
            player.Level = 50;
            player.baseStat = 70;
            player.TotalConstitutionLostAtDeath = 3;
            StatCalculator statCalc = createStatCalculator();

            int actual = statCalc.CalcValue(player, eProperty.Constitution);
            
            Assert.AreEqual(67, actual);
        }

        public static StatCalculator createStatCalculator()
        {
            return new StatCalculator();
        }
    }
}
