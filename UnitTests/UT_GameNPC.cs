using NUnit.Framework;
using NSubstitute;
using DOL.GS;
using DOL.AI;
using DOL.GS.PropertyCalc;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GameNPC
    {
        [TestFixtureSetUp]
        public void init()
        {
            GameLiving.LoadCalculators();
        }

        [Test]
        public void GetModified_GameNPCWith75Constitution_Return75()
        {
            var brain = Substitute.For<ABrain>();
            var npc = new GameNPC(brain);
            npc.Constitution = 75;

            int actual = npc.GetModified(eProperty.Constitution);
            
            Assert.AreEqual(75, actual);
        }
    }
}
