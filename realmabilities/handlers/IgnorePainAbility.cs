using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ignore Pain, healing
	/// </summary>
	public class IgnorePainAbility : TimedRealmAbility
	{
		public IgnorePainAbility(DBAbility dba, int level) : base(dba, level) { }

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
				case 1: heal = 20; break;
				case 2: heal = 50; break;
				case 3: heal = 80; break;
			}
			int healed = living.ChangeHealth(living, GameLiving.eHealthChangeType.Spell, living.MaxHealth * heal / 100);

			SendCasterSpellEffectAndCastMessage(living, 7004, healed > 0);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (healed > 0) player.Out.SendMessage("You heal yourself for " + healed + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (heal > healed)
				{
					player.Out.SendMessage("You are fully healed.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			if (healed > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Level 1: Value: 20%");
			list.Add("Level 2: Value: 50%");
			list.Add("Level 3: Value: 80%");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Casting time: instant");
		}
	}
}