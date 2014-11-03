/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using DOL.GS;
using DOL.Language;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[CharacterClassAttribute((int)eCharacterClass.Ranger, "Ranger", "Stalker")]
	public class ClassRanger : ClassStalker
	{
		private static readonly string[] AutotrainableSkills = new[] { Specs.Archery, Specs.RecurveBow };

		public ClassRanger()
			: base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofFocus");
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.DEX;
		}

		public override string GetTitle(GamePlayer player, int level)
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Ranger.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

		public override bool CanUseLefthandedWeapon
		{
			get { return true; }
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		/// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
        /// FIXME : this should be in database.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skill"></param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			switch (skill.KeyName)
			{
				case Specs.RecurveBow:
					if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
					{
						if (skill.Level < 3)
						{
							// do nothing
						}
						else if (skill.Level < 6)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 1));
						}
						else if (skill.Level < 9)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 2));
						}
						else if (skill.Level < 12)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 3));
						}
						else if (skill.Level < 15)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 4));
						}
						else if (skill.Level < 18)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 5));
						}
						else if (skill.Level < 21)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 6));
						}
						else if (skill.Level < 24)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 7));
						}
						else if (skill.Level < 27)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 8));
						}
						else if (skill.Level >= 27)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 9));
						}

						if (skill.Level >= 45)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 2));
						}
						else if (skill.Level >= 35)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 1));
						}

						if (skill.Level >= 45)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.SureShot));
						}

						if (skill.Level >= 50)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 3));
						}
						else if (skill.Level >= 40)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 2));
						}
						else if (skill.Level >= 30)
						{
							player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 1));
						}
					}
					break;
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
