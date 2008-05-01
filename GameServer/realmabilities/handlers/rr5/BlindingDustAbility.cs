using System;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class BlindingDustAbility : RR5RealmAbility
	{
		public BlindingDustAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7040, true);

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
			if ((BlindingDustEffect)target.EffectList.GetOfType(typeof(BlindingDustEffect)) == null)
			{
				BlindingDustEffect effect = new BlindingDustEffect();
				effect.Start(target);
			}

		}

		public override int GetReUseDelay(int level)
		{
			return 300;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Insta-cast PBAE Attack that causes the enemy to have a 25% chance to fumble melee/bow attacks for the next 15 seconds.");
			list.Add("");
			list.Add("Radius: 350");
			list.Add("Target: Enemy");
			list.Add("Duration: 15 sec");
			list.Add("Casting time: instant");
		}

	}
}