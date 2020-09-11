using DOL.Database;
using DOL.GS;

namespace DOL.UnitTests.Gameserver
{
    public static class Create
    {
        public static Spell Spell => new Spell(new DBSpell(), 0);

        public static FakePlayer FakePlayer
        {
            get
            {
                var player = new FakePlayer();
                player.characterClass = new DefaultCharacterClass();
                return player;
            }
        }

        public static FakePlayer FakePlayerAnimist
        {
            get
            {
                var player = FakePlayer;
                player.characterClass = new CharacterClassAnimist();
                return player;
            }
        }

        public static FakeNPC FakeNPC
        {
            get
            {
                var brain = new NullBrain();
                var npc = new FakeNPC(brain);
                return npc;
            }
        }

        public static FakeGameLiving FakeLiving => new FakeGameLiving();

        public static GamePet GenericRealPet(GameLiving owner)
        {
            var brain = new NullControlledBrain();
            brain.fakeOwner = owner;
            GamePet pet = new GamePet(brain);
            return pet;
        }
    }
}
