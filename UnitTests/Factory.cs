using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using NSubstitute;

namespace DOL.UnitTests.Gameserver
{
    public static class Create
    {
        public static Spell Spell => new Spell(new DBSpell(), 0);

        public static FakePlayer GenericFakePlayer => FakePlayer.CreateGeneric();

        public static FakePlayer FakePlayerAnimist
        {
            get
            {
                var player = GenericFakePlayer;
                player.characterClass = new CharacterClassAnimist();
                return player;
            }
        }

        public static FakeNPC GenericFakeNPC => FakeNPC.CreateGeneric();

        public static GamePet GenericRealPet(GameLiving owner)
        {
            var brain = Substitute.For<ABrain, IControlledBrain>();
            var npcTemplate = Substitute.For<INpcTemplate>();
            (brain as IControlledBrain).Owner.Returns(owner);
            GamePet pet = new GamePet(brain);
            return pet;
        }
    }
}
