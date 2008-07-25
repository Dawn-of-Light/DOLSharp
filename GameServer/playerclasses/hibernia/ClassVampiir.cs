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

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Vampiir, "Vampiir", "Stalker")]
	public class ClassVampiir : ClassStalker
	{
		public ClassVampiir()
			: base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofAffinity");
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CON;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.DEX;
			//Vampiirs do not have a mana stat
			//Special handling is need in the power pool calculator
			//m_manaStat = eStat.STR;
			m_wsbase = 440;
            m_baseHP = 878;
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Vampiir.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player)
		{
			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Piercing));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Dementia));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.ShadowMastery));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.VampiiricEmbrace));
				player.AddSpellLine(SkillBase.GetSpellLine("Dementia"));
				player.AddSpellLine(SkillBase.GetSpellLine("Shadow Mastery"));
				player.AddSpellLine(SkillBase.GetSpellLine("Vampiiric Embrace"));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 0));
			}
			if (player.Level >= 6)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirConstitution));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirDexterity));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirQuickness));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirStrength));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 1));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 2));
			}
			if (player.Level >= 25)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Climbing));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 3));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 3));
			}
			if (player.Level >= 35)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 4));
			}
			if (player.Level >= 40)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 4));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 5));
			}
			if (player.Level >= 45)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 6));
			}
			if (player.Level >= 50)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 5));
				player.AddAbility(SkillBase.GetAbility(Abilities.VampiirBolt, 7));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
