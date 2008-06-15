using System;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Soul Quench RA
	/// </summary>
	public class SoulQuenchAbility : RR5RealmAbility
	{
		public SoulQuenchAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 1145, true);

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
			double modifier = 0.5 + (caster.GetModifiedSpecLevel("Soulrending") * 0.01);
			int basedamage = (int)(250 * modifier);
			int resist = basedamage * target.GetResist(eDamageType.Spirit) / -100;
			int damage = basedamage + resist;
			int heal = (int)(damage * 0.75);
			int modheal = caster.MaxHealth - caster.Health;
			if (modheal > heal)
				modheal = heal;
			caster.Health += modheal;

			GamePlayer player = caster as GamePlayer;
			if (player != null)
				player.Out.SendMessage("You hit " + target.Name + " for " + damage + "(" + resist + ") points of damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			if (caster is GamePlayer && modheal > 0)
				((GamePlayer)caster).Out.SendMessage("Your Soul Quench returns " + modheal + " lifepoints to you", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

			GamePlayer targetPlayer = target as GamePlayer;
			if (targetPlayer != null)
			{
				if (targetPlayer.IsStealthed)
					targetPlayer.Stealth(false);
			}

			foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(caster, target, 1145, 0, false, 1);
				p.Out.SendCombatAnimation(caster, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
			}

			//target.TakeDamage(caster, eDamageType.Spirit, damage, 0);
			AttackData ad = new AttackData();
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
			ad.Attacker = caster;
			ad.Target = target;
			ad.DamageType = eDamageType.Spirit;
			ad.Damage = damage;
			target.OnAttackedByEnemy(ad);
			caster.DealDamage(ad);


		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Insta-PBAE attack that drains 250 points (modified up or down by the Reavers SR level) from all nearby enemies and returns 75% to the Reaver.");
			list.Add("");
			list.Add("Radius: 350");
			list.Add("Target: Enemy");
			list.Add("Casting time: instant");
		}

	}
}