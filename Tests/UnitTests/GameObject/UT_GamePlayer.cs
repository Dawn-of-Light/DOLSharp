using DOL.GS;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GamePlayer
    {
        [Test]
        public void Constitution_Level50PlayerWith100ConstBaseBuff_Return62()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.BaseBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(50 * 1.25);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Constitution_Level50PlayerWith100ConstSpecBuff_Return93()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(50 * 1.875);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Constitution_Level50Player100ConstFromItems_Return75()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(1.5 * 50);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Intelligence_Level50AnimistWith50AcuityFromItems_Return50()
        {
            var player = CreatePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 50;

            int actual = player.Intelligence;

            Assert.AreEqual(50, actual);
        }

        [Test]
        public void Constitution_Level50Player150ConAnd100MythicalConCap_Return127()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;

            int actual = player.Constitution;

            Assert.AreEqual(127, actual);
        }

        [Test]
        public void Constitution_Level50PlayerWith5MythicalConCap100ConCap_Return106()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 5;
            player.ItemBonus[eProperty.ConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;

            int actual = player.Constitution;

            Assert.AreEqual(106, actual);
        }

        [Test]
        public void CalcValue_GetIntelligenceFromLevel50AnimistWith50Acuity_Return50()
        {
            var player = CreatePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.BaseBuffBonusCategory[(int)eProperty.Acuity] = 50;

            int actual = player.Intelligence;

            Assert.AreEqual(50, actual);
        }

        [Test]
        public void Intelligence_Level50AnimistWith200AcuityAnd30AcuCapEachFromItems_Return127()
        {
            var player = CreatePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 200;
            player.ItemBonus[eProperty.AcuCapBonus] = 30;
            player.ItemBonus[eProperty.MythicalAcuCapBonus] = 30;

            int actual = player.Intelligence;

            Assert.AreEqual(127, actual);
        }

        [Test]
        public void Intelligence_Level50AnimistWith30AcuityAnd30IntelligenceFromItems_Return60()
        {
            var player = CreatePlayer(new CharacterClassAnimist());
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 30;
            player.ItemBonus[eProperty.Intelligence] = 30;

            int actual = player.Intelligence;

            Assert.AreEqual(60, actual);
        }

        [Test]
        public void Constitution_Level30AnimistWith200ConAnd20ConCapEachViaItems_Return81()
        {
            var player = CreatePlayer(new CharacterClassAnimist());
            player.Level = 30;
            player.ItemBonus[eProperty.Constitution] = 200;
            player.ItemBonus[eProperty.ConCapBonus] = 20;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 20;

            int actual = player.Constitution;

            Assert.AreEqual(81, actual);
        }

        private static GamePlayer CreatePlayer()
        {
            return GamePlayer.CreateDummy();
        }

        private static GamePlayer CreatePlayer(ICharacterClass charClass)
        {
            var player = CreatePlayer();
            player.SetCharacterClass(charClass.ID);
            return player;
        }
    }
}
