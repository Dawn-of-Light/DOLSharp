using System.Collections;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Helper for charge realm ability
	/// </summary>
	public class ArmsLengthEffect : TimedEffect
	{
		/// <summary>
		/// Creates a new effect
		/// </summary>
		public ArmsLengthEffect() : base(10000) { }

		/// <summary>
		/// Start the effect on player
		/// </summary>
		/// <param name="target">The effect target</param>
		public override void Start(GameLiving target)
		{
			base.Start(target);
			target.TempProperties.setProperty("Charging", true);
			target.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 2.5);
			if (target is GamePlayer)
			{
				((GamePlayer)target).Out.SendUpdateMaxSpeed();
			}
		}

		public override void Stop()
		{
			base.Stop();
			Owner.TempProperties.removeProperty("Charging");
			Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			if (Owner is GamePlayer)
			{
				((GamePlayer)Owner).Out.SendUpdateMaxSpeed();
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return "Arms Length"; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 3057; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Grants unbreakable extreme speed for 15 seconds.");
				list.Add(base.DelveInfo);
				return list;
			}
		}
	}
}