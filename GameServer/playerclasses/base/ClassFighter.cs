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

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Fighter, "Fighter", "Fighter")]
	public class ClassFighter : DOL.GS.CharacterClassSpec
	{
		public ClassFighter() : base() {
			m_specializationMultiplier = 10;
			m_wsbase = 440;
			m_baseHP = 880;
		}

		public override eClassType ClassType
		{
			get { return eClassType.PureTank; }
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Slash));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Thrust));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Crush));

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, (int)eArmorLevel.Medium));
			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, (int)eShieldSize.Medium));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Slashing));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrusting));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Crushing));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
		}
	}
}
