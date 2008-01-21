using System;
using DOL.Database2;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Raging power, power heal
	/// </summary>
	public class RagingPowerAbility : TimedRealmAbility
	{
		public RagingPowerAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			int heal = 0;
			switch (Level)
			{
				case 1: heal = 25; break;
				case 2: heal = 60; break;
				case 3: heal = 100; break;
			}
			int healed = living.ChangeMana(living, GameLiving.eManaChangeType.Spell, living.MaxMana * heal / 100);

			SendCasterSpellEffectAndCastMessage(living, 7009, healed > 0);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (healed > 0) player.Out.SendMessage("You gain " + healed + " mana.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (heal > healed)
				{
					player.Out.SendMessage("You have full mana.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			if (healed > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Level 1: Value: 25%");
			list.Add("Level 2: Value: 60%");
			list.Add("Level 3: Value: 100%");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Casting time: instant");
		}
	}
}