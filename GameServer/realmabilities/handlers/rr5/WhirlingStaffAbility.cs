using System;
using DOL.Database2;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Whirling Staff Ability
	/// </summary>
	public class WhirlingStaffAbility : RR5RealmAbility
	{
		public WhirlingStaffAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7043, true);

			bool deactivate = false;
			foreach (GamePlayer player in living.GetPlayersInRadius(false, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(living, player, true))
				{
					DamageTarget(player, living);
					deactivate = true;
				}
			}

			foreach (GameNPC npc in living.GetNPCsInRadius(false, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(living, npc, true))
				{
					DamageTarget(npc, living);
					deactivate = true;
				}
			}
			if (deactivate)
				DisableSkill(living);
		}

		private void DamageTarget(GameLiving target, GameLiving caster)
		{
			int resist = 251 * target.GetResist(eDamageType.Crush) / -100;
			int damage = 251 + resist;

			GamePlayer player = caster as GamePlayer;
			if (player != null)
				player.Out.SendMessage("You hit " + target.Name + " for " + damage + "(" + resist + ") points of damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

			GamePlayer targetPlayer = target as GamePlayer;
			if (targetPlayer != null)
			{
				if (targetPlayer.IsStealthed)
					targetPlayer.Stealth(false);
			}

			foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(caster, target, 7043, 0, false, 1);
				p.Out.SendCombatAnimation(caster, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
			}

			//target.TakeDamage(caster, eDamageType.Spirit, damage, 0);
			AttackData ad = new AttackData();
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
			ad.Attacker = caster;
			ad.Target = target;
			ad.DamageType = eDamageType.Crush;
			ad.Damage = damage;
			target.OnAttackedByEnemy(ad);
			caster.DealDamage(ad);

			if ((WhirlingStaffEffect)target.EffectList.GetOfType(typeof(WhirlingStaffEffect)) == null)
			{
				WhirlingStaffEffect effect = new WhirlingStaffEffect();
				effect.Start(target);
			}

		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("A 350 radius PBAE attack that deals medium crushing damage and disarms your opponents for 6 seconds");
			list.Add("");
			list.Add("Radius: 350");
			list.Add("Target: Enemy");
			list.Add("Duration: 6 sec");
			list.Add("Casting time: instant");
		}

	}
}