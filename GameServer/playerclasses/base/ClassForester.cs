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
	[CharacterClassAttribute((int)eCharacterClass.Forester, "Forester", "Forester")]
	public class ClassForester : DOL.GS.CharacterClassBase
	{
		public ClassForester()
		{
			m_specializationMultiplier = 10;
			m_wsbase = 280;
			m_baseHP = 560;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level)
		{
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		public override GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.Forester;
		}

		public override void OnLevelUp(GamePlayer player, int previousLevel)
		{
			base.OnLevelUp(player, previousLevel);

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Arboreal_Path));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Arboreal Path"));

			// Abilities
			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Cloth));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
