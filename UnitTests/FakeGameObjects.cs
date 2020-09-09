using DOL.AI;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using NSubstitute;

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
        public int LastDamageDealt { get; private set; } = -1;

        public static FakePlayer CreateGeneric()
        {
            var player = new FakePlayer();
            player.characterClass = new DefaultCharacterClass();
            return player;
        }

        public FakePlayer() : base(null, null)
        {
            this.ObjectState = eObjectState.Active;
        }

        public override ICharacterClass CharacterClass { get { return characterClass; } }

        public override byte Level { get; set; }

        public override Region CurrentRegion { get { return new FakeRegion(); } set { } }

        public override void LoadFromDatabase(DataObject obj)
        {
        }

        public override IPacketLib Out => new FakePacketLib();

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

        public override void DealDamage(AttackData ad)
        {
            base.DealDamage(ad);
            LastDamageDealt = ad.Damage;
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

        public override bool TargetInView { get; set; } = true;
    }

    public class FakeNPC : GameNPC
    {
        public int modifiedEffectiveLevel;

        public static FakeNPC CreateGeneric()
        {
            var brain = Substitute.For<ABrain>();
            var npc = new FakeNPC(brain);
            return npc;
        }

        public FakeNPC(ABrain defaultBrain) : base(defaultBrain)
        {
            this.ObjectState = eObjectState.Active;
        }

        public override Region CurrentRegion { get { return new FakeRegion(); } set { } }

        public override bool IsAlive => true;

        public override int GetModified(eProperty property)
        {
            switch (property)
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
}
