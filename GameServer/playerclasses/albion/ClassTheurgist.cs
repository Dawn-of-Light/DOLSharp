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
	[PlayerClassAttribute((int)eCharacterClass.Theurgist, "Theurgist", "Elementalist")]
	public class ClassTheurgist : ClassElementalist
	{
		public ClassTheurgist() : base() 
		{
			m_profession = "Defenders of Albion";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Master Theurgist";
			if (level>=45) return "Master Summoner";
			if (level>=40) return "Combat Engineer";
			if (level>=35) return "Engineer Prime";
			if (level>=30) return "Summoner Prime";
			if (level>=25) return "Engineer";
			if (level>=20) return "Sapper";
			if (level>=15) return "Summoner";
			if (level>=10) return "Journeyman Theurgist"; 
			if (level>=5) return "Theurgist Recruit"; 
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
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Wind_Magic));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Path of Air"));
			player.AddSpellLine(SkillBase.GetSpellLine("Abrasion"));
			player.AddSpellLine(SkillBase.GetSpellLine("Refrigeration"));
			player.AddSpellLine(SkillBase.GetSpellLine("Vapormancy"));

			if (player.Level >= 5) 
			{				
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}
	}
}
