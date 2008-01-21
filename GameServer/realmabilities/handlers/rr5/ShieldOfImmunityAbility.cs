using System;
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class ShieldOfImmunityAbility : RR5RealmAbility
	{
		public ShieldOfImmunityAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;
			
			// Check for MoC on the Sorceror: he cannot cast RA5L when the other is up
			MasteryofConcentrationEffect ra5l = null;
			lock (player.EffectList)
			{
				foreach (object effect in player.EffectList)
				{
					if (effect is MasteryofConcentrationEffect)
					{
						ra5l = effect as MasteryofConcentrationEffect;
						break;
					}
				}
			}
			if (ra5l != null)
			{
				player.Out.SendMessage("You cannot currently use this ability", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			
			SendCasterSpellEffectAndCastMessage(player, 7048, true);
			ShieldOfImmunityEffect raEffect = new ShieldOfImmunityEffect();
			raEffect.Start(player);
			
			DisableSkill(living);
		}


		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Shield that absorbs 90% melee/archer damage for 20 seconds.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 20 sec");
			list.Add("Casting time: instant");
		}

	}
}