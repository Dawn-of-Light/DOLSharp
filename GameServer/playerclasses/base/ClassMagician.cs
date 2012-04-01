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
	[CharacterClassAttribute((int)eCharacterClass.Magician, "Magician", "Magician")]
	public class ClassMagician : DOL.GS.CharacterClassBase
	{
		public ClassMagician()
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
			return GameTrainer.eChampionTrainerType.Magician;
		}

		public override void OnLevelUp(GamePlayer player, int previousLevel)
		{
			base.OnLevelUp(player, previousLevel);

			if (this is ClassBainshee == false)
			{
				// Specializations
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Light));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mana));


				// Spell lines
				player.AddSpellLine(SkillBase.GetSpellLine("Way of the Sun"));
				player.AddSpellLine(SkillBase.GetSpellLine("Way of the Moon"));
			}

			// Abilities
			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
			player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Cloth));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
