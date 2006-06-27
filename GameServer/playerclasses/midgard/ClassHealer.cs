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
	[PlayerClassAttribute((int)eCharacterClass.Healer, "Healer", "Seer")]
	public class ClassHealer : ClassSeer
	{
		public ClassHealer() : base() 
		{
			m_profession = "House of Eir";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Hand of Eir";
			if (level>=45) return "Oracle of Eir";
			if (level>=40) return "Healing Augur";
			if (level>=35) return "Soothsayer";
			if (level>=30) return "Hand of Dawn";
			if (level>=25) return "Seer of Eir";
			if (level>=20) return "Wiseman";
			if (level>=15) return "Journeyman Healer";
			if (level>=10) return "Practitioner"; 
			if (level>=5) return "Healing Eleve"; 
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Pacification));
				
				player.AddSpellLine(SkillBase.GetSpellLine("Pacification"));
				player.AddSpellLine(SkillBase.GetSpellLine("Healer Mending Spec"));
				player.AddSpellLine(SkillBase.GetSpellLine("Healer Augmentation Spec"));
				player.AddSpellLine(SkillBase.GetSpellLine("Pacification Spec"));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, (int)eArmorLevel.Medium));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, (int)eArmorLevel.High));
			}		
		}
	}
}
