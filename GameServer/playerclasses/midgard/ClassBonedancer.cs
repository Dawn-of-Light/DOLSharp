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
	[CharacterClassAttribute((int)eCharacterClass.Bonedancer, "Bonedancer", "Mystic")]
	public class ClassBonedancer : CharacterClassBoneDancer
	{
		public ClassBonedancer() : base()
		{
			m_specializationMultiplier = 10;
			m_wsbase = 280;
			m_baseHP = 560;
			m_manaStat = eStat.PIE;

			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.HouseofBodgar");
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		public override string GetTitle(GamePlayer player, int level) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Bonedancer.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

		public override void OnLevelUp(GamePlayer player, int previousLevel)
		{
			base.OnLevelUp(player, previousLevel);

			// Mystic

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Darkness));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Suppression));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Darkness"));
			player.AddSpellLine(SkillBase.GetSpellLine("Suppression"));

			// Abilities
			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Cloth));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));

			// Bonedancer
		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.BoneArmy));			

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Army"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Mystics"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Guardians"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Warriors"));

			// Abilities
			player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
