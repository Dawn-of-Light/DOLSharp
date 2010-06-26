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
	[CharacterClassAttribute((int)eCharacterClass.Shadowblade, "Shadowblade", "MidgardRogue")]
	public class ClassShadowblade : ClassMidgardRogue
	{
		private static readonly string[] AutotrainableSkills = new[] { Specs.Stealth };

		public ClassShadowblade() : base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.Loki");
			m_specializationMultiplier = 22;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_baseHP = 760;
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Shadowblade.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Checks whether player has ability to use lefthanded weapons
		/// </summary>
		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		public override void OnLevelUp(GamePlayer player)
		{

			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Critical_Strike));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Axe));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Left_Axe));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Envenom));

			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Axes));
			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrown));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_LeftAxes));

			if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 4));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 5));
			}
			if (player.Level >= 40)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 6));
			}
			if (player.Level >= 45)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.BloodRage));
			}
			if (player.Level >= 50)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 7));
				player.AddAbility(SkillBase.GetAbility(Abilities.BloodRage));
			}
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="skill">The skill to train</param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			switch(skill.KeyName)
			{
				case Specs.Stealth:
					if(skill.Level >= 5) player.AddAbility(SkillBase.GetAbility(Abilities.Distraction));
					if (skill.Level >= 8) player.AddAbility(SkillBase.GetAbility(Abilities.DangerSense));
					if (skill.Level >= 10) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 1));
					if(skill.Level >= 16) player.AddAbility(SkillBase.GetAbility(Abilities.DetectHidden));
					if (skill.Level >= 20) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 2));
					if (skill.Level >= 25) player.AddAbility(SkillBase.GetAbility(Abilities.Climbing));
					if (skill.Level >= 30) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 3));
					if (skill.Level >= 40) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 4));
					if (skill.Level >= 50) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 5));
					break;
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
