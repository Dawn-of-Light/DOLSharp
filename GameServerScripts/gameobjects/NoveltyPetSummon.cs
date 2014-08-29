using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.AI.Brain;


namespace GameServerScripts
{
    /// <summary>
    /// This class is for prize type summons - the pet is purely aesthetic.
    /// </summary>
    [SpellHandler("Call")]
    public class NoveltyPetSummon : SummonSpellHandler
    {
        /// <summary>
        /// Constructs the spell handler
        /// </summary>
        public NoveltyPetSummon(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

            if (m_pet != null)
                m_pet.Flags |= GameNPC.eFlags.PEACE; //must be peace!
                        
            //No brain for now, so just follow owner.
            m_pet.Follow(Caster, 100, WorldMgr.VISIBILITY_DISTANCE);
        }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.CurrentRegion.IsRvR)
            {
                MessageToCaster("You cannot cast this spell in an rvr zone!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted);
                return false;
            }

            return base.CheckBeginCast(selectedTarget);
        }

        /// <summary>
        /// These pets aren't controllable!
        /// </summary>
        /// <param name="brain"></param>
        protected override void SetBrainToOwner(IControlledBrain brain)
        {
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            return new NoveltyPetBrain(owner);
        }
    }

    /// <summary>
    /// Eventually says random phrases from the spell's description.
    /// </summary>
    public class NoveltyPetBrain : DOL.AI.ABrain, IControlledBrain
    {
        public NoveltyPetBrain(GameLiving owner)
            : base() {
                m_owner = owner;
            }

        private GameLiving m_owner;

        #region Think

        public override int ThinkInterval
        { get { return 5000; } set { } }

        public override void Think()
        {
            //wander about, unless we are not at the player...
            if (!m_owner.IsWithinRadius(Body, 500))
                Body.Follow(Owner, 150, WorldMgr.VISIBILITY_DISTANCE);

            else
            {
                //have to stop follow or it causes problems!
                Body.StopFollowing();
                int tx = m_owner.X + Util.Random(-300, 300);
                int ty = m_owner.Y + Util.Random(-300, 300);

				Body.WalkTo(tx, ty, m_owner.Z, (short)(Body.MaxSpeed / 3));
            }
        }

        #endregion

        #region IControlledBrain Members
        public void SetAggressionState(eAggressionState state) { }
        public eWalkState WalkState { get { return eWalkState.Follow; } }
        public eAggressionState AggressionState { get { return eAggressionState.Passive; } set { } }
        public GameLiving Owner { get { return m_owner; } }
		public void Attack(GameObject target) { }
        public void Follow(GameObject target) { }
        public void FollowOwner() { }
        public void Stay() { }
        public void ComeHere() { }
        public void Goto(GameObject target) { }
        public void UpdatePetWindow() { }
        public GamePlayer GetPlayerOwner() { return m_owner as GamePlayer; }
		public GameNPC GetNPCOwner() { return m_owner as GameNPC; }
		public GameLiving GetLivingOwner() { return m_owner as GameLiving; }
		public bool IsMainPet { get { return false; } set { } }
        #endregion
    }
}
