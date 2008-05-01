using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Blade Barrier
	/// </summary>
	public class BladeBarrierEffect : TimedEffect
	{
		public BladeBarrierEffect()
			: base(30000)
		{
			;
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);

			foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(target, target, 7055, 0, false, 1);
			}
			m_owner.BuffBonusCategory4[(int)eProperty.ParryChance] += 90;
			GameEventMgr.AddHandler(target, GameLivingEvent.AttackFinished, new DOLEventHandler(attackEventHandler));

		}

		protected void attackEventHandler(DOLEvent e, object sender, EventArgs args)
		{
			if (args == null) return;
			AttackFinishedEventArgs ag = args as AttackFinishedEventArgs;
			if (ag == null) return;
			if (ag.AttackData == null) return;
			if (ag.AttackData.Attacker != Owner) return;
			switch (ag.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					Stop(); break;
			}

		}

		public override string Name { get { return "Blade Barrier"; } }

		public override ushort Icon { get { return 3054; } }

		public override void Stop()
		{
			m_owner.BuffBonusCategory4[(int)eProperty.ParryChance] -= 90;
			base.Stop();
		}

		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Grants 90% Parry chance which is broken by an attack");
				return list;
			}
		}
	}
}