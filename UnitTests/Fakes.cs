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
        public int modifiedSpellDamage = 0;
        public int baseStat;
        private int totalConLostOnDeath;

        public override ICharacterClass CharacterClass { get { return characterClass; } }

        public FakePlayer() : base(null, null)
        {
            this.ObjectState = eObjectState.Active;
        }

        public override byte Level { get; set; }

        public override Region CurrentRegion { get { return new FakeRegion(); } set { } }

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
                case eProperty.SpellDamage:
                    return modifiedSpellDamage;
                default:
                    return base.GetModified(property);
                    //throw new ArgumentException("There is no property: " + property);
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

        public override void MessageToSelf(string message, GS.PacketHandler.eChatType chatType) { }

        public override System.Collections.IEnumerable GetPlayersInRadius(ushort radiusToCheck)
        {
            return new System.Collections.Generic.List<int>();
        }

        protected override void ResetInCombatTimer() { }
    }

    public class FakeNPC : GameNPC
    {
        public int modifiedEffectiveLevel;

        public FakeNPC(ABrain defaultBrain) : base(defaultBrain)
        {
            this.ObjectState = eObjectState.Active;
        }

        public override Region CurrentRegion { get { return new FakeRegion(); } set { } }

        public override bool IsAlive => true;

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
                    return base.GetModified(property);
            }
        }

        public override System.Collections.IEnumerable GetPlayersInRadius(ushort radiusToCheck)
        {
            return new System.Collections.Generic.List<int>();
        }
    }

    public class FakeRegion : Region
    {

        public FakeRegion() : base(null, new RegionData()) { }

        public override long Time => -1;

        public override ushort ID => 0;
    }

    public class FakeRegionData : RegionData
    {
    }
}
