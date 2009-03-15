using System;
using System.Collections;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vanish realm ability
	/// </summary>
	public class VanishAbility : TimedRealmAbility
	{		public VanishAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | STEALTHED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7020, true);

			int duration = 0;
			double speedBonus = 1;
			switch (Level)
			{
				case 1: duration = 1000; speedBonus = 1.0; break;
				case 2: duration = 2000; speedBonus = MaxSpeedCalculator.SPEED1; break;
				case 3: duration = 5000; speedBonus = MaxSpeedCalculator.SPEED5; break;
			}

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				VanishEffect vanish = new VanishEffect(duration, speedBonus);
				vanish.Start(player);

				foreach (GameSpellEffect effect in living.EffectList.GetAllOfType(typeof(GameSpellEffect)))
				{
					if (effect.SpellHandler is DoTSpellHandler ||
						effect.SpellHandler is StyleBleeding ||
							effect.SpellHandler is AbstractCCSpellHandler ||
							effect.SpellHandler is SpeedDecreaseSpellHandler)
					{
						effect.Cancel(false);
					}
				}
			}

			ArrayList attackers = new ArrayList();
			attackers.AddRange(living.Attackers);
			foreach (GameLiving attacker in attackers)
			{
				if (attacker.TargetObject == living)
				{
					attacker.TargetObject = null;
					if (attacker is GamePlayer)
					{
						((GamePlayer)attacker).Out.SendChangeTarget(attacker.TargetObject);
					}
					if (attacker is GameNPC)
					{
						GameNPC npc = (GameNPC)attacker;
						if (npc.Brain is IOldAggressiveBrain)
						{
							((IOldAggressiveBrain)npc.Brain).RemoveFromAggroList(living);
						}
						attacker.StopAttack();
					}
				}
			}

			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Level 1: Normal Speed");
			list.Add("Level 2: Speed 1");
			list.Add("Level 3: Speed 5");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Level 1: Duration: 1 sec");
			list.Add("Level 2: Duration: 2 sec");
			list.Add("Level 3: Duration: 5 sec");
			list.Add("Casting time: instant");
		}
	}
}