using System;
using System.Collections.Generic;
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
            //Commented out for removal: Parry Chance for BladeBarrier is hardcoded in GameLiving.cs in the CalculateEnemyAttackResult method
			//m_owner.BuffBonusCategory4[(int)eProperty.ParryChance] += 90;
			GameEventMgr.AddHandler(target, GameLivingEvent.AttackFinished, new DOLEventHandler(attackEventHandler));
            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(TakeDamage));

		}

        //[StephenxPimentel]
        //1.108 All Damage Recieved while this effect is active is reduced by 25%.
        public void TakeDamage(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GameLiving)
            {
                GameLiving living = sender as GameLiving;
                AttackedByEnemyEventArgs eDmg = args as AttackedByEnemyEventArgs;

                if (!living.HasEffect(typeof(BladeBarrierEffect)))
                {
                    GameEventMgr.RemoveHandler(GameLivingEvent.AttackedByEnemy, TakeDamage);
                    return;
                }

                eDmg.AttackData.Damage -= ((eDmg.AttackData.Damage * 25) / 100);
                eDmg.AttackData.CriticalDamage -=  ((eDmg.AttackData.CriticalDamage * 25) / 100);
                eDmg.AttackData.StyleDamage -= ((eDmg.AttackData.StyleDamage * 25) / 100);
            }
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
            //Commented out for removal
			//m_owner.BuffBonusCategory4[(int)eProperty.ParryChance] -= 90;
			base.Stop();
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Grants 90% Parry chance which is broken by an attack");
				return list;
			}
		}
	}
}