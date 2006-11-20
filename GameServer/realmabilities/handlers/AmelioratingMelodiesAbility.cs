using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ameliorating Melodies realm ability
	/// </summary>
	public class AmelioratingMelodiesAbility : TimedRealmAbility
	{
		/// <summary>
		/// Constructs the Ameliorating Melodies handler
		/// </summary>
		public AmelioratingMelodiesAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			GamePlayer player = living as GamePlayer;
			if (player == null) return;
			if (CheckPreconditions(living, DEAD | SITTING | STUNNED | MEZZED | NOTINGROUP)) return;
			AmelioratingMelodiesEffect ameffect = (AmelioratingMelodiesEffect)player.EffectList.GetOfType(typeof(AmelioratingMelodiesEffect));
			if (ameffect != null)
			{
				ameffect.Cancel(false);
			}

			SendCasterSpellEffectAndCastMessage(living, 3021, true);

			int heal = 0;

			switch (Level)
			{
				case 1:
					heal = 100;
					break;
				case 2:
					heal = 250;
					break;
				case 3:
					heal = 400;
					break;
			}

			AmelioratingMelodiesEffect am = new AmelioratingMelodiesEffect(heal);
			am.Start(player);

			DisableSkill(living);
		}

		/// <summary>
		/// Returns the re-use delay of the ability
		/// </summary>
		/// <param name="level">Level of the ability</param>
		/// <returns>Delay in seconds</returns>
		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Level 1: Heals 100 / tick");
			list.Add("Level 2: Heals 250 / tick");
			list.Add("Level 3: Heals 400 / tick");
			list.Add("");
			list.Add("Target: Group, except the user");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: instant");
		}
	}
}