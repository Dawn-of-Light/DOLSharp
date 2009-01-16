using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Anger of the Gods RA
	/// </summary>
	public class RestorativeMindAbility : RR5RealmAbility
	{
		public RestorativeMindAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;



			bool deactivate = false;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (player.Group != null)
				{
					SendCasterSpellEffectAndCastMessage(living, 7071, true);
					foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
					{
						RestorativeMindEffect aog = (RestorativeMindEffect)member.EffectList.GetOfType(typeof(RestorativeMindEffect));
						if (!CheckPreconditions(member, DEAD) && aog == null
							&& living.IsWithinRadius( member, 2000 ))
						{
							RestorativeMindEffect effect = new RestorativeMindEffect();
							effect.Start(member);
							deactivate = true;
						}
					}
				}
				else
				{
					RestorativeMindEffect aog = (RestorativeMindEffect)player.EffectList.GetOfType(typeof(RestorativeMindEffect));
					if (!CheckPreconditions(player, DEAD) && aog == null)
					{
						RestorativeMindEffect effect = new RestorativeMindEffect();
						effect.Start(player);
						deactivate = true;
					}
				}
			}
			if (deactivate)
				DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}


		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Group Frigg that heals health, power, and endurance over 30 seconds for a total of 50%. (5% is granted every 3 seconds regardless of combat state)");
			list.Add("");
			list.Add("Target: Group");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: instant");
		}

	}
}
