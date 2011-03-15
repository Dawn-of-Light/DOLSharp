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
	[CharacterClassAttribute((int)eCharacterClass.Guardian, "Guardian", "Guardian")]
	public class ClassGuardian : DOL.GS.CharacterClassBase
	{
		public ClassGuardian() : base() {
			m_specializationMultiplier = 10;
			m_wsbase = 400;
			m_baseHP = 880;
		}

		public override string GetTitle(int level)
		{
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		public override eClassType ClassType
		{
			get { return eClassType.PureTank; }
		}

		public override GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.Guardian;
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blades));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blunt));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Piercing));

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Reinforced));
			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blades));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blunt));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Piercing));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));

			if (player.Level >= 2)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
