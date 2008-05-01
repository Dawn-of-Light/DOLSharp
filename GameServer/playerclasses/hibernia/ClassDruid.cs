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
	[PlayerClassAttribute((int)eCharacterClass.Druid, "Druid", "Naturalist")]
	public class ClassDruid : ClassNaturalist
	{
		public ClassDruid() : base() 
		{
			m_profession = "Path of Harmony";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.EMP;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.EMP;
			m_wsbase = 320;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Grove Priest";
			if (level>=45) return "Grove Protector";
			if (level>=40) return "Master Ovate";
			if (level>=35) return "Grove Oracle";
			if (level>=30) return "Grove Protector";
			if (level>=25) return "Grove Healer";
			if (level>=20) return "Ovate";
			if (level>=15) return "Apprentice Druid";
			if (level>=10) return "Student"; 
			if (level>=5) return "Novice"; 
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
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Nature));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Druid Nature Magic"));
			player.AddSpellLine(SkillBase.GetSpellLine("Druid Nurture Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Regrowth Druid Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Druid Nature Spec"));

			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Reinforced));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Scale));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
