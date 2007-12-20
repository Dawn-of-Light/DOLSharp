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
	[PlayerClassAttribute((int)eCharacterClass.Spiritmaster, "Spiritmaster", "Mystic")]
	public class ClassSpiritmaster : ClassMystic
	{
		public ClassSpiritmaster() : base()
		{
			m_profession = "House of Hel";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Hand of Hel";
			if (level>=45) return "Spirit Sage";
			if (level>=40) return "Spirit Adept";
			if (level>=35) return "Spiritmancer";
			if (level>=30) return "Elder Summoner";
			if (level>=25) return "Hel's Spiritist";
			if (level>=20) return "Summoner";
			if (level>=15) return "Journeyman";
			if (level>=10) return "Practitioner"; 
			if (level>=5) return "Spiritist Eleve"; 
			return "None"; 
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			
			base.OnLevelUp(player);

		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Summoning));			

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Summoning"));
			player.AddSpellLine(SkillBase.GetSpellLine("Spirit Dimming"));
			player.AddSpellLine(SkillBase.GetSpellLine("Spirit Suppression"));
			player.AddSpellLine(SkillBase.GetSpellLine("Spirit Enhancement"));

			if(player.Level >= 5)
			{
				// Abilities
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}
	}
}
