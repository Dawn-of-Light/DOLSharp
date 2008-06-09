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
	[PlayerClassAttribute((int)eCharacterClass.Bard, "Bard", "Naturalist")]
	public class ClassBard : ClassNaturalist
	{
		public ClassBard() : base() 
		{
			m_profession = "Path of Essence";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CHR;
			m_secondaryStat = eStat.EMP;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.CHR;
			m_wsbase = 360;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Master Songster";
			if (level>=45) return "Spellsinger";
			if (level>=40) return "Songster";
			if (level>=35) return "Minstrel";
			if (level>=30) return "Performer";
			if (level>=25) return "Melodist";
			if (level>=20) return "Choralist";
			if (level>=15) return "Caroler";
			if (level>=10) return "Vocalist"; 
			if (level>=5) return "Chanter"; 
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
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blades));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blunt));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Music));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Bard Music"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bard Nurture Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Regrowth Bard Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bard Music Spec"));

			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Instruments));

			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Reinforced));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 25) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
