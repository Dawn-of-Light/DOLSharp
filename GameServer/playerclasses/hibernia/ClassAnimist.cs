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
	[PlayerClassAttribute((int)eCharacterClass.Animist, "Animist", "Forester")]
	public class ClassAnimist : ClassForester
	{
		public ClassAnimist() : base() 
		{
			m_profession = "Path of Affinity";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Force of Nature";
			if (level>=45) return "Arboreal Champion";
			if (level>=40) return "Arboreal Mage";
			if (level>=35) return "Forestmage";
			if (level>=30) return "Servant of Nature";
			if (level>=25) return "Arboreal Adept";
			if (level>=20) return "Plantfriend";
			if (level>=15) return "Friend of Gaia";
			if (level>=10) return "Arboreal Apprentice"; 
			if (level>=5) return "Grove Initiate"; 
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
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Creeping_Path));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Verdant_Path));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Creeping Path")); //base
			player.AddSpellLine(SkillBase.GetSpellLine("Verdant Path")); //base
			player.AddSpellLine(SkillBase.GetSpellLine("Arborial Mastery")); //spec
			player.AddSpellLine(SkillBase.GetSpellLine("Creeping Path Spec")); //spec
			player.AddSpellLine(SkillBase.GetSpellLine("Verdant Path Spec")); //spec

			if (player.Level >= 5) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}
	}
}
