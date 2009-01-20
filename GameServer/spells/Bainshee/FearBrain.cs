using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;

namespace DOL.AI.Brain
{
	public class FearBrain : StandardMobBrain
	{
		public FearBrain()
			: base()
		{
			ThinkInterval = 5000;
		}

		public override void Think()
		{
			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
			{
				CalculateFleeTarget(player);
				break;
			}
		}

		///<summary>Calculate flee target.</summary>
		///<param>The target to flee.</param>
		protected virtual void CalculateFleeTarget(GameLiving target)
		{
			Point3D stalker = new Point3D(target.X, target.Y, target.Z);
			ushort NotTarget = Body.GetHeadingToTarget(stalker);

			ushort TargetAngle = (ushort)((NotTarget + 2048) % 4096);
			Body.Heading = TargetAngle;

			int FleeX, FleeY;
			Body.GetSpotFromHeading(300, out FleeX, out FleeY);
			Body.StopFollow();
			Body.StopAttack();
			Body.WalkTo(FleeX, FleeY, Body.Z, Body.MaxSpeed);
		}
	}
} 
