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

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Seer, "Seer", "Seer")]
	public class ClassSeer : DOL.GS.CharacterClassSpec
	{
		public ClassSeer() : base() {
			m_specializationMultiplier = 10;
			m_wsbase = 360;
			m_baseHP = 720;
			m_manaStat = eStat.PIE;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mending));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Augmentation));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Mending"));
			player.AddSpellLine(SkillBase.GetSpellLine("Augmentation"));

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Leather));
			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Hammers));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
