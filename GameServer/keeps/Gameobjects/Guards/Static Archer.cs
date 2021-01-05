using DOL.AI.Brain;

namespace DOL.GS.Keeps
{
	public class GuardStaticArcher : GuardArcher
	{
		protected override void SetAggression()
		{
			(Brain as KeepGuardBrain).SetAggression(99, 2100);
		}

		protected override void SetSpeed()
		{
			base.SetSpeed();
			MaxSpeedBase = 0;
		}
	}
}
