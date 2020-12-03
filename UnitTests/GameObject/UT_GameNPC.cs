using NUnit.Framework;
using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GameNPC
    {
        [OneTimeSetUp]
        public void Init()
        {
            GameLiving.LoadCalculators();
        }

        [Test]
        public void GetModified_Constitution_GameNPCWith75Constitution_75Percent()
        {
            var npc = NewNPC();
            npc.Constitution = 75;

            int actual = npc.GetModified(eProperty.Constitution);
            
            Assert.AreEqual(75, actual);
        }

        [Test]
        public void GetArmorAbsorb_LevelZeroNPCHasOneConstitution_Circa5Percent()
        {
            var npc = NewNPC();

            var actual = npc.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(-0.05, actual, 0.001);
        }

        [Test]
        public void GetArmorAbsorb_LevelZeroNPCwith60BaseAnd120BaseBuffConstitution_30Percent()
        {
            var npc = NewNPC();
            npc.Constitution = 60;
            npc.BaseBuffBonusCategory[eProperty.Constitution] = 120;

            var actual = npc.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(0.30, actual, 0.001);
        }

        [Test]
        public void GetArmorAbsorb_L30NPCWith60Constitution_27Percent()
        {
            var npc = NewNPC();
            npc.Level = 30;
            npc.Constitution = 60;

            var actual = npc.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(0.27, actual, 0.001);
        }

        [Test]
        public void GetArmorAbsorb_LevelZeroNPCWith50ConDebuff_Minus25Percent()
        {
            var npc = NewNPC();
            npc.Constitution = 60;
            npc.DebuffCategory[eProperty.Constitution] = 50;

            var actual = npc.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(-0.25, actual, 0.001);
        }

        private static GameNPC NewNPC() => new GameNPC(new FakeControlledBrain());
    }

    [TestFixture]
    class UT_GameKeepGuard
    {
        [SetUp]
        public void Init()
        {
            GameLiving.LoadCalculators();
        }

        [Test]
        public void GetArmorAbsorb_AnySlot_L30Guard_27Percent()
        {
            var guard = new GameKeepGuard();
            guard.Level = 30;
            guard.Constitution = 60;

            var actual = guard.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(0.27, actual, 0.001);
        }

        [Test]
        public void GetArmorAbsorb_AnySlot_L30GuardLord_32Percent()
        {
            var guard = new GuardLord();
            guard.Level = 30;
            guard.Constitution = 60;

            var actual = guard.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(0.32, actual, 0.001);
        }

        [Test]
        public void GetArmorAbsorb_AnySlot_L30GuardCaster_22Percent()
        {
            var guard = new GuardCaster();
            guard.Level = 30;
            guard.Constitution = 60;

            var actual = guard.GetArmorAbsorb(eArmorSlot.NOTSET);

            Assert.AreEqual(0.22, actual, 0.001);
        }
    }
}
