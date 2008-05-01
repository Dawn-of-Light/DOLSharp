using System;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class EntwiningSnakesAbility : RR5RealmAbility
	{
		public EntwiningSnakesAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7072, true);

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
			if (!target.IsAlive)
				return;


			foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(caster, target, 7072, 0, false, 1);
			}


			if ((EntwiningSnakesEffect)target.EffectList.GetOfType(typeof(EntwiningSnakesEffect)) == null)
			{
				EntwiningSnakesEffect effect = new EntwiningSnakesEffect();
				effect.Start(target);
			}

		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Insta-cast spell that is PBAE 50% snare lasting 20 seconds with a 350 unit radius. Snare breaks on attack.");
			list.Add("");
			list.Add("Radius: 350");
			list.Add("Target: Enemy");
			list.Add("Duration: 20 sec");
			list.Add("Casting time: instant");
		}

	}
}