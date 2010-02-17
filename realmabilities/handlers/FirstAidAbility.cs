using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// First Aid, healing
	/// </summary>
	public class FirstAidAbility : TimedRealmAbility
	{
		public FirstAidAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED | INCOMBAT)) return;

			int heal = 0;
			switch (Level)
			{
				case 1: heal = 25; break;
				case 2: heal = 60; break;
				case 3: heal = 100; break;
			}
			int healed = living.ChangeHealth(living, GameLiving.eHealthChangeType.Spell, living.MaxHealth * heal / 100);

			SendCasterSpellEffectAndCastMessage(living, 7001, healed > 0);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (healed > 0) player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "FirstAidAbility.Execute.HealYourself", healed), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (heal > healed)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "FirstAidAbility.Execute.FullyHealed"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			if (healed > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 180;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info1"));
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info2"));
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info3"));
			list.Add("");
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info4"));
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info5"));
			list.Add("");
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "FirstAidAbility.AddEffectsInfo.Info6"));
		}
	}
}