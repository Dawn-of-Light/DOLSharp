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
	[PlayerClassAttribute((int)eCharacterClass.Bainshee, "Bainshee", "Magician")]
	public class ClassBainshee : ClassMagician
	{
		public ClassBainshee() : base() 
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofAffinity");
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Bainshee.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.EtherealShriek));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.PhantasmalWail));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.SpectralGuard));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.EtherealShriek));
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.PhantasmalWail));
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.SpectralForce));
            player.AddSpellLine(SkillBase.GetSpellLine(Specs.SpectralGuard));

			if (player.Level >= 5) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
