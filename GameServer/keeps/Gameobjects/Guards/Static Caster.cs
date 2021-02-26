using DOL.AI.Brain;

namespace DOL.GS.Keeps
{
	public class GuardStaticCaster : GuardCaster
	{
		protected override void SetAggression()
		{
			(Brain as KeepGuardBrain).SetAggression(99, 1850);
		}

		protected override void SetSpeed()
		{
			base.SetSpeed();
			MaxSpeedBase = 0;
		}
	}
}
