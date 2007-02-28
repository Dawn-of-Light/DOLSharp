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

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Cleric, "Cleric", "Acolyte")]
	public class ClassCleric : ClassAcolyte
	{
		public ClassCleric() : base() 
		{
			m_profession = "Church of Albion";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
			m_baseHP = 720;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Pontifex";
			if (level>=45) return "Cardinal";
			if (level>=40) return "Bishop";
			if (level>=35) return "Abbot";
			if (level>=30) return "Ecclesiastic";
			if (level>=25) return "Deacon";
			if (level>=20) return "Prelate";
			if (level>=15) return "Curate";
			if (level>=10) return "Novice"; 
			if (level>=5) return "Initiate"; 
			return "None"; 
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			if (player.Level >= 5) 
			{				
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Smite));
				
				player.AddSpellLine(SkillBase.GetSpellLine("Smiting"));
				player.AddSpellLine(SkillBase.GetSpellLine("Rebirth (Cleric)"));
				player.AddSpellLine(SkillBase.GetSpellLine("Guardian Angel"));
				player.AddSpellLine(SkillBase.GetSpellLine("Terrible Hammer"));

			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Studded));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
			}		
		}
	}
}
