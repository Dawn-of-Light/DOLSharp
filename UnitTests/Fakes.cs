using System;
using DOL.AI;
using DOL.Database;
using DOL.GS;

namespace DOL.UnitTests.Gameserver
{
    public class FakePlayer : GamePlayer
    {
        public int modifiedSpecLevel;
        public ICharacterClass characterClass;
        public int modifiedIntelligence;
        public int modiefiedToHitBonus;
        public int modifiedSpellLevel;
        public int modifiedEffectiveLevel;
        public int baseStat;
        private int totalConLostOnDeath;

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
            switch (property)
            {
                case eProperty.Intelligence:
                    return modifiedIntelligence;
                case eProperty.SpellLevel:
                    return modifiedSpellLevel;
                case eProperty.ToHitBonus:
                    return modiefiedToHitBonus;
                case eProperty.LivingEffectiveLevel:
                    return modifiedEffectiveLevel;
                default: throw new ArgumentException("There is no property with that name");
            }
        }

        public override int GetBaseStat(eStat stat)
        {
            return baseStat;
        }

        public override int TotalConstitutionLostAtDeath
        {
            get { return totalConLostOnDeath; }
            set { totalConLostOnDeath = value; }
        }
    }

    public class FakeNPC : GameNPC
    {
        public int modifiedEffectiveLevel;

        public FakeNPC(ABrain defaultBrain) : base(defaultBrain)
        {
        }

        public override int GetModified(eProperty property)
        {
            switch(property)
            {
                case eProperty.LivingEffectiveLevel:
                    return modifiedEffectiveLevel;
                case eProperty.MaxHealth:
                    return 0;
                case eProperty.Intelligence:
                    return Intelligence;
                default:
                    throw new ArgumentException("There is no property with that name");
            }
        }
    }
}
