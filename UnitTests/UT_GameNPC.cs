using NUnit.Framework;
using DOL.GS;
using DOL.AI;

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
        public void GetModified_GameNPCWith75Constitution_Return75()
        {
            var brain = NewNullBrain();
            var npc = new GameNPC(brain);
            npc.Constitution = 75;

            int actual = npc.GetModified(eProperty.Constitution);
            
            Assert.AreEqual(75, actual);
        }

        private static ABrain NewNullBrain() => new NullBrain();
    }
}
