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
	[PlayerClassAttribute((int)eCharacterClass.AlbionRogue, "Rogue", "Rogue")]
	public class ClassAlbionRogue : DOL.GS.CharacterClassSpec
	{
		public ClassAlbionRogue() : base() {
			m_specializationMultiplier = 10;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override string GetTitle(int level) {
			return "None"; 
		}

		public override eClassType ClassType
		{
			get { return eClassType.PureTank; }
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Stealth));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Slash));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Thrust));

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
			player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Leather));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Slashing));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrusting));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
