using DOL.GS;

namespace DOL.AI.Brain
{
	public class ProcPetBrain : StandardMobBrain, IControlledBrain
	{
		private GamePlayer m_owner;
		private GameLiving m_target;

		public ProcPetBrain(GamePlayer owner) { m_owner = owner; m_target=m_owner.TargetObject as GameLiving; AggroLevel = 100; }
		
		public override int ThinkInterval { get { return 1500; } }

		public override void Think() { AttackMostWanted(); }

		public void SetAggressionState(eAggressionState state) { }

		protected override void AttackMostWanted()
		{
			if (!IsActive) return;
			if(m_target==null) return;
			GameLiving target = m_target;
			if(target != null && target.IsAlive) Body.StartAttack(target);
			else
			{
				Body.LastAttackTickPvP=0;
				Body.LastAttackTickPvE=0;
			}
		}

		#region IControlledBrain Members
		public eWalkState WalkState { get { return eWalkState.Stay; } }
		public eAggressionState AggressionState { get { return eAggressionState.Aggressive; } set { } }
		public GameLiving Owner { get { return m_owner; } }
		public void Attack(GameObject target) { }
		public void Follow(GameObject target) { }
		public void FollowOwner() { }
		public void Stay() { }
		public void ComeHere() { }
		public void Goto(GameObject target) { }
		public void UpdatePetWindow() { }
		public GamePlayer GetPlayerOwner() { return m_owner; }
		public bool IsMainPet { get { return false; } set { } }
		#endregion
	}
}
