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
	[CharacterClassAttribute((int)eCharacterClass.Mentalist, "Mentalist", "Magician")]
	public class ClassMentalist : ClassMagician
	{
		public ClassMentalist() : base() 
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofHarmony");
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(GamePlayer player, int level) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client, "PlayerClass.Mentalist.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player, int previousLevel)
		{
			base.OnLevelUp(player, previousLevel);

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mentalism));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Mentalism"));
			player.AddSpellLine(SkillBase.GetSpellLine("Holism"));
			player.AddSpellLine(SkillBase.GetSpellLine("Illusions"));
			player.AddSpellLine(SkillBase.GetSpellLine("Mind Mastery"));

			player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
