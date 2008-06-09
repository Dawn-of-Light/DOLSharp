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
	[PlayerClassAttribute((int)eCharacterClass.Cabalist, "Cabalist", "Mage")]
	public class ClassCabalist : ClassMage
	{
		public ClassCabalist() : base()
		{
			m_profession = "Guild of Shadows";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Cabalist Prime";
			if (level>=45) return "Master Imbuer";
			if (level>=40) return "Master Spiritist";
			if (level>=35) return "Animator";
			if (level>=30) return "Spiritist";
			if (level>=25) return "Veteran Cabalist";
			if (level>=20) return "Creator";
			if (level>=15) return "Imbuer";
			if (level>=10) return "Journeyman Cabalist"; 
			if (level>=5) return "Apprentice Cabalist"; 
			return "None"; 
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Spirit_Magic));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Spirit Animation"));
			player.AddSpellLine(SkillBase.GetSpellLine("Matter Manipulation"));
			player.AddSpellLine(SkillBase.GetSpellLine("Essence Manipulation"));
			player.AddSpellLine(SkillBase.GetSpellLine("Vivification"));
			
			if(player.Level >= 5)
			{
				// Abilities
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
