using DOL.Database;
using DOL.GS;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GamePlayer
    {
        [OneTimeSetUp]
        public void LoadCalculators()
        {
            GameLiving.LoadCalculators();
        }

        [Test]
        public void Constitution_Level50PlayerWith100ConstBaseBuff_Return62()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.BaseBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(50 * 1.25);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Constitution_Level50PlayerWith100ConstSpecBuff_Return93()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.SpecBuffBonusCategory[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(50 * 1.875);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Constitution_Level50Player100ConstFromItems_Return75()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.Constitution] = 100;

            int actual = player.Constitution;

            int expected = (int)(1.5 * 50);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Intelligence_Level50IntCasterWith50AcuityFromItems_Return50()
        {
            var player = CreatePlayer(IntCaster);
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 50;

            int actual = player.Intelligence;

            Assert.That(actual, Is.EqualTo(50));
        }

        [Test]
        public void Constitution_Level50Player150ConAnd100MythicalConCap_Return127()
        {
            var player = CreatePlayer();
            player.Level = 50;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 100;
            player.ItemBonus[eProperty.Constitution] = 150;

            int actual = player.Constitution;

            Assert.That(actual, Is.EqualTo(127));
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

            Assert.That(actual, Is.EqualTo(106));
        }

        [Test]
        public void CalcValue_GetIntelligenceFromLevel50IntCasterWith50Acuity_Return50()
        {
            var player = CreatePlayer(IntCaster);
            player.Level = 50;
            player.BaseBuffBonusCategory[(int)eProperty.Acuity] = 50;

            int actual = player.Intelligence;

            Assert.That(actual, Is.EqualTo(50));
        }

        [Test]
        public void Intelligence_Level50IntCasterWith200AcuityAnd30AcuCapEachFromItems_Return127()
        {
            var player = CreatePlayer(IntCaster);
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 200;
            player.ItemBonus[eProperty.AcuCapBonus] = 30;
            player.ItemBonus[eProperty.MythicalAcuCapBonus] = 30;

            int actual = player.Intelligence;

            Assert.That(actual, Is.EqualTo(127));
        }

        [Test]
        public void Intelligence_Level50IntCasterWith30AcuityAnd30IntelligenceFromItems_Return60()
        {
            var player = CreatePlayer(IntCaster);
            player.Level = 50;
            player.ItemBonus[eProperty.Acuity] = 30;
            player.ItemBonus[eProperty.Intelligence] = 30;

            int actual = player.Intelligence;

            Assert.That(actual, Is.EqualTo(60));
        }

        [Test]
        public void Constitution_Level30With200ConAnd20ConCapEachViaItems_Return81()
        {
            var player = CreatePlayer(IntCaster);
            player.Level = 30;
            player.ItemBonus[eProperty.Constitution] = 200;
            player.ItemBonus[eProperty.ConCapBonus] = 20;
            player.ItemBonus[eProperty.MythicalConCapBonus] = 20;

            int actual = player.Constitution;

            Assert.That(actual, Is.EqualTo(81));
        }

        private static GamePlayer CreatePlayer()
        {
            return GamePlayer.CreateDummy();
        }

        private static GamePlayer CreatePlayer(CharacterClass charClass)
        {
            var player = CreatePlayer();
            player.SetCharacterClass(charClass);
            return player;
        }

        private CharacterClass IntCaster
            => CharacterClass.Create(new DBCharacterClass() { ID=1, ManaStat = (byte)eStat.INT });
    }
}
