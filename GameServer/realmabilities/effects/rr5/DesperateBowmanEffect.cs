using System;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class DesperateBowmanDisarmEffect : TimedEffect
	{
		public DesperateBowmanDisarmEffect()
			: base(15000)
		{
			;
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
            target.DisarmedTime = target.CurrentRegion.Time + m_duration;
            target.SilencedTime = target.CurrentRegion.Time + m_duration;
			target.StopAttack();
			target.StopCurrentSpellcast();
		}

		public override string Name { get { return "Desperate Bowman"; } }

		public override ushort Icon { get { return 3060; } }

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Disarms you for 15 seconds!");
				return list;
			}
		}
	}

	public class DesperateBowmanStunEffect : TimedEffect
	{
		public DesperateBowmanStunEffect()
			: base(5000)
		{
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			target.IsStunned = true;
			target.StopAttack();
			target.StopCurrentSpellcast();
			target.DisableTurning(true);
			if (target is GamePlayer)
				(target as GamePlayer).Out.SendUpdateMaxSpeed();
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.IsStunned = false;
			m_owner.DisableTurning(false);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendUpdateMaxSpeed();
		}

		public override string Name { get { return "Desperate Bowman"; } }

		public override ushort Icon { get { return 3060; } }

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Stun Effect");
				return list;
			}
		}
	}

}