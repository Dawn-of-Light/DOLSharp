using DOL.GS.Keeps;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Caster Guards Brain
	/// </summary>
	public class CasterBrain : KeepGuardBrain
	{
		/// <summary>
		/// Brain Think
		/// </summary>
		public override void Think()
		{
			base.Think();

			GameKeepGuard guard = Body as GameKeepGuard;
			/*
			 * Here what we do is check if the guard can used ranged, 
			 * and if so, break melee and start using it
			 */
			if (Body.AttackState && guard.CanUseRanged)
			{
				if (Body.TargetObject != null)
				{
					Body.StopAttack();
					Body.StartAttack(Body.TargetObject);
				}
			}
		}
	}
}
