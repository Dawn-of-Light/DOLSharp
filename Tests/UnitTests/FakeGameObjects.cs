using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.UnitTests.Gameserver
{
    public class FakePlayer : GamePlayer
    {
        public CharacterClass fakeCharacterClass = GS.CharacterClass.None;
        public int modifiedSpecLevel;
        public int modifiedIntelligence;
        public int modifiedToHitBonus;
        public int modifiedSpellLevel;
        public int modifiedEffectiveLevel;
        public int modifiedSpellDamage = 0;
        public int baseStat;
        private int totalConLostOnDeath;
        public int LastDamageDealt { get; private set; } = -1;
        public FakeRegion fakeRegion = new FakeRegion();

        public FakePlayer() : base(null, null)
        {
            this.ObjectState = eObjectState.Active;
            this.m_invulnerabilityTick = -1;
        }

        public override CharacterClass CharacterClass { get { return fakeCharacterClass; } }
        public override byte Level { get; set; }
        public override Region CurrentRegion { get { return fakeRegion; } set { } }
        public override IPacketLib Out => new FakePacketLib();
        public override GameClient Client => new GameClient(GameServer.Instance) { Account = new Account() };
        public override int GetBaseStat(eStat stat) => baseStat;
        public override int GetModifiedSpecLevel(string keyName) => modifiedSpecLevel;

        public override int GetModified(eProperty property)
        {
            switch (property)
            {
                case eProperty.Intelligence:
                    return modifiedIntelligence;
                case eProperty.SpellLevel:
                    return modifiedSpellLevel;
                case eProperty.ToHitBonus:
                    return modifiedToHitBonus;
                case eProperty.LivingEffectiveLevel:
                    return modifiedEffectiveLevel;
                case eProperty.SpellDamage:
                    return modifiedSpellDamage;
                default:
                    return base.GetModified(property);
            }
        }

        public override void LoadFromDatabase(DataObject obj) { }

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
        public override void StartHealthRegeneration() { }
        public override void StartEnduranceRegeneration() { }
        public override void MessageToSelf(string message, DOL.GS.PacketHandler.eChatType chatType) { }

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

        public FakeNPC(ABrain defaultBrain) : base(defaultBrain)
        {
            this.ObjectState = eObjectState.Active;
        }

        public FakeNPC() : this(new FakeBrain()) { }

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

    public class FakeLiving : GameLiving
    {
        public bool fakeIsAlive = true;
        public eObjectState fakeObjectState = eObjectState.Active;

        public override bool IsAlive => fakeIsAlive;
        public override eObjectState ObjectState => fakeObjectState;
    }

    public class FakeControlledBrain : ABrain, IControlledBrain
    {
        public GameLiving fakeOwner;
        public bool receivedUpdatePetWindow = false;

        public GameLiving Owner => fakeOwner;
        public void UpdatePetWindow() { receivedUpdatePetWindow = true; }

        public eWalkState WalkState { get; }
        public eAggressionState AggressionState { get; set; }
        public bool IsMainPet { get; set; }
        public void Attack(GameObject target) { }
        public void ComeHere() { }
        public void Follow(GameObject target) { }
        public void FollowOwner() { }
        public GameLiving GetLivingOwner() { return null; }
        public GameNPC GetNPCOwner() { return null; }
        public GamePlayer GetPlayerOwner() { return null; }
        public void Goto(GameObject target) { }
        public void SetAggressionState(eAggressionState state) { }
        public void Stay() { }
        public override void Think() { }
    }

    public class FakeBrain : ABrain
    {
        public override void Think() { }
    }
}
