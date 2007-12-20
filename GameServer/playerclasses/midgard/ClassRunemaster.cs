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
	[PlayerClassAttribute((int)eCharacterClass.Runemaster, "Runemaster", "Mystic")]
	public class ClassRunemaster : ClassMystic
	{
		public ClassRunemaster() : base()
		{
			m_profession = "House of Odin";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Hand of Odin";
			if (level>=45) return "Runic Sage";
			if (level>=40) return "Runic Adept";
			if (level>=35) return "Runemancer";
			if (level>=30) return "Elder Runecarver";
			if (level>=25) return "Odin's Runecarver";
			if (level>=20) return "Stoneteller";
			if (level>=15) return "Runecarver";
			if (level>=10) return "Runic Practitioner"; 
			if (level>=5) return "Runic Eleve"; 
			return "None"; 
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			
			base.OnLevelUp(player);

		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Runecarving));			

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Runecarving"));
			player.AddSpellLine(SkillBase.GetSpellLine("Runes of Darkness"));
			player.AddSpellLine(SkillBase.GetSpellLine("Runes of Suppression"));
			player.AddSpellLine(SkillBase.GetSpellLine("Runes of Destruction"));

			if(player.Level >= 5)
			{
				// Abilities
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}
	}
}
