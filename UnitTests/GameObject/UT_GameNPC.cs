using NUnit.Framework;
using DOL.GS;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GameNPC
    {
        [TestFixtureSetUp]
        public void Init()
        {
            GameLiving.LoadCalculators();
        }

        [Test]
        public void GetModified_Constitution_GameNPCWith75Constitution_75()
        {
            var npc = NewRealNPC();
            npc.Constitution = 75;

            int actual = npc.GetModified(eProperty.Constitution);
            
            Assert.AreEqual(75, actual);
        }

        private static GameNPC NewRealNPC() => new FakeNPC(new FakeControlledBrain());
    }
}
