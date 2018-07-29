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
        [TestCase]
        public void GetModified_GameNPCWith75Constitution_Return75()
        {
            GameLiving.LoadCalculators(); //temporal coupling and global state
            var brain = Substitute.For<ABrain>();
            var npc = new GameNPC(brain);
            npc.Constitution = 75;

            int actual = npc.GetModified(eProperty.Constitution);
            
            Assert.AreEqual(75, actual);
        }
    }
}
