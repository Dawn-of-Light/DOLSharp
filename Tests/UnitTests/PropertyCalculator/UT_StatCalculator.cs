using NUnit.Framework;
using DOL.GS;
using DOL.GS.PropertyCalc;
using DOL.Database;

namespace DOL.UnitTests.Gameserver.PropertyCalc
{
    [TestFixture]
    public class UT_StatCalculator
    {
        [Test]
        public void CalcValueFromBuffs_GameNPCWith100ConstBaseBuff_100()
        {
            var npc = NewNPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = StatCalculator.CalcValueFromBuffs(npc, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(100));
        }

        [Test]
        public void CalcValueFromBuffs_Level50PlayerWith100ConstBaseBuff_62()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.BaseBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = StatCalculator.CalcValueFromBuffs(player, eProperty.Constitution);

            int expected = (int)(50 * 1.25);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValueFromBuffs_Level50PlayerWith100ConstSpecBuff_93()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = StatCalculator.CalcValueFromBuffs(player, eProperty.Constitution);

            int expected = (int)(50 * 1.875);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValueFromBuffs_BaseBuff3AndSpecBuff4_7()
        {
            var npc = NewNPC();
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 3;
            npc.SpecBuffBonusCategory[eProperty.Constitution] = 4;

            int actual = StatCalculator.CalcValueFromBuffs(npc, eProperty.Constitution);

            int expected = 7;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValueFromBuffs_LivingIsNull_Zero()
        {
            int actual = StatCalculator.CalcValueFromBuffs(null, eProperty.Constitution);
            
            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void CalcValueFromItems_LivingIsNull_Zero()
        {
            int actual = StatCalculator.CalcValueFromItems(null, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void CalcValueFromItems_Level50Player100ConstFromItems_75()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.Constitution] = 100;

            int actual = StatCalculator.CalcValueFromItems(player, eProperty.Constitution);

            int expected = (int)(1.5 * 50);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValueFromItems_Level50NPC100ConstFromItems_75()
        {
            var stat = eProperty.Constitution;
            var npc = NewNPC();
            npc.Level = 50;
            npc.ItemBonus[stat] = 100;

            int actual = StatCalculator.CalcValueFromItems(npc, stat);

            int expected = (int)(1.5 * 50);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValueFromItems_IntelligenceOfLevel50IntCasterWith50AcuityFromItems_50()
        {
            var player = NewPlayer();
            player.fakeCharacterClass = IntCaster;
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 50;

            int actual = StatCalculator.CalcValueFromItems(player, eProperty.Intelligence);
            
            Assert.That(actual, Is.EqualTo(50));
        }

        [Test]
        public void CalcValueFromItems_Level50Player150ConAnd100MythicalConCap_127()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;

            int actual = StatCalculator.CalcValueFromItems(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(127));
        }

        [Test]
        public void CalcValueFromItems_Level50PlayerWith5MythicalConCap100ConCap_106()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 5;
            player.ItemBonus[eProperty.ConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;

            int actual = StatCalculator.CalcValueFromItems(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(106));
        }

        [Test]
        public void GetItemBonusCapIncrease_Level50Player100ConstCap_26()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.ConCapBonus] = 100;

            int actual = StatCalculator.GetItemBonusCapIncrease(player, eProperty.Constitution);

            int expected = (int)(50 / 2.0 + 1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItemBonusCapIncrease_Level50Player10ConstCap_10()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.ConCapBonus] = 10;

            int actual = StatCalculator.GetItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(10));
        }

        [Test]
        public void GetMythicalItemBonusCapIncrease_PlayerWith100MythicalConCap_52()
        {
            var player = NewPlayer();
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;

            int actual = StatCalculator.GetMythicalItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(52));
        }

        [Test]
        public void GetMythicalItemBonusCapIncrease_PlayerWith10MythicalConCap_10()
        {
            var player = NewPlayer();
            player.ItemBonus[eProperty.MythicalConCapBonus] = 10;

            int actual = StatCalculator.GetMythicalItemBonusCapIncrease(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(10));
        }

        [Test]
        public void CalcValue_NPCWith100Constitution_100()
        {
            var npc = NewNPC();
            npc.Constitution = 100;

            int actual = StatCalculator.CalcValue(npc, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(100));
        }

        [Test]
        public void CalcValue_NPCWith100Intelligence_100()
        {
            var npc = NewNPC();
            npc.Intelligence = 100;

            int actual = StatCalculator.CalcValue(npc, eProperty.Intelligence);

            Assert.That(actual, Is.EqualTo(100));
        }

        [Test]
        public void CalcValue_GetIntelligenceFromLevel50IntCasterWith50Acuity_50()
        {
            var player = NewPlayer();
            player.fakeCharacterClass = IntCaster;
            player.Level = 50;
            player.BaseBuffBonusCategory[(int)eProperty.Acuity] = 50;

            int actual = StatCalculator.CalcValue(player, eProperty.Intelligence);

            Assert.That(actual, Is.EqualTo(50));
        }

        [Test]
        public void CalcValue_200ConstitutionAbilityBonus_200()
        {
            var player = NewPlayer();
            player.AbilityBonus[eProperty.Constitution] = 200;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(200));
        }

        [Test]
        public void CalcValue_200ConstitutionDebuff_1()
        {
            var player = NewPlayer();
            player.DebuffCategory[eProperty.Constitution] = 200;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void CalcValue_200ConAbilityBonusAnd50ConDebuff_200()
        {
            var player = NewPlayer();
            player.AbilityBonus[eProperty.Constitution] = 200;
            player.DebuffCategory[eProperty.Constitution] = 50;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(200 - (50/2)));
        }

        [Test]
        public void CalcValue_70ConBuffBonusAnd50ConDebuff_20()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            Assert.That(actual, Is.EqualTo(20));
        }

        [Test]
        public void CalcValue_70ConItemBonusAnd50ConDebuff_45()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.Constitution] = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            int expected = 70 - (50 / 2);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValue_70ConBaseStatAnd50ConDebuff_45()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.baseStat = 70;
            player.DebuffCategory[eProperty.Constitution] = 50;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);

            int expected = 70 - (50 / 2);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalcValue_70ConBaseStatAnd3ConLostOnDeath_67()
        {
            var player = NewPlayer();
            player.Level = 50;
            player.baseStat = 70;
            player.TotalConstitutionLostAtDeath = 3;

            int actual = StatCalculator.CalcValue(player, eProperty.Constitution);
            
            Assert.That(actual, Is.EqualTo(67));
        }

        public static StatCalculator StatCalculator => new StatCalculator();

        private static FakePlayer NewPlayer() => new FakePlayer();
        private static CharacterClass IntCaster
            => CharacterClass.Create(new DBCharacterClass() { ManaStat = (byte)eStat.INT });
        private static FakeNPC NewNPC() => new FakeNPC();
    }
}
