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
	[PlayerClassAttribute((int)eCharacterClass.Eldritch, "Eldritch", "Magician")]
	public class ClassEldritch : ClassMagician
	{
		public ClassEldritch() : base() 
		{
			m_profession = "Path of Focus";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Prime Prestidigitist";
			if (level>=45) return "Prestidigitist";
			if (level>=40) return "Force Weaver";
			if (level>=35) return "Master";
			if (level>=30) return "Renderer";
			if (level>=25) return "Adept";
			if (level>=20) return "Magius";
			if (level>=15) return "Conjurer";
			if (level>=10) return "Evoker"; 
			if (level>=5) return "Incanter"; 
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
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Void));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Way of the Eclipse"));
			player.AddSpellLine(SkillBase.GetSpellLine("Shadow Control"));
			player.AddSpellLine(SkillBase.GetSpellLine("Vacuumancy"));
			player.AddSpellLine(SkillBase.GetSpellLine("Void Mastery"));

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
