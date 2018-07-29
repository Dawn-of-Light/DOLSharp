using DOL.Database;
using DOL.GS;

namespace DOL.UnitTests.Gameserver
{
    public class FakePlayer : GamePlayer
    {
        public int modifiedSpecLevel;
        public ICharacterClass characterClass;
        public int intelligence;

        public override ICharacterClass CharacterClass { get { return characterClass; } }

        public FakePlayer() : base(null, null) { }

        public override void LoadFromDatabase(DataObject obj)
        {
        }

        public override int GetModifiedSpecLevel(string keyName)
        {
            return modifiedSpecLevel;
        }

        public override int GetModified(eProperty property)
        {
            return intelligence;
        }
    }
}
