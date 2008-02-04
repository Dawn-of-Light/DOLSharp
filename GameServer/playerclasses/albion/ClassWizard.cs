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
	[PlayerClassAttribute((int)eCharacterClass.Wizard, "Wizard", "Elementalist")]
	public class ClassWizard : ClassElementalist
	{
		public ClassWizard() : base() 
		{
			m_profession = "Academy";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
			m_wsbase = 240; // yes, lower that for other casters for some reason
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Magus Prime";
			if (level>=45) return "Master Spellbinder";
			if (level>=40) return "Master Elementalist";
			if (level>=35) return "Thaumaturge";
			if (level>=30) return "Invoker";
			if (level>=25) return "Magus";
			if (level>=20) return "Spellbinder";
			if (level>=15) return "Elementalist";
			if (level>=10) return "Wizard Adept"; 
			if (level>=5) return "Apprentice Wizard"; 
			return "None"; 
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Fire_Magic));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Path of Fire"));
			player.AddSpellLine(SkillBase.GetSpellLine("Calefaction"));
			player.AddSpellLine(SkillBase.GetSpellLine("Liquifaction"));
			player.AddSpellLine(SkillBase.GetSpellLine("Pyromancy"));

			if (player.Level >= 5) 
			{				
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
