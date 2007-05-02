using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;

namespace DOL.AI.Brain
{
	public class FearBrain : StandardMobBrain
	{
		public FearBrain() : base()
		{
			ThinkInterval = 3000;
		}

		protected override void CheckPlayerAggro()
		{
		}

		protected override void CheckNPCAggro()
		{
		}
	}
}
