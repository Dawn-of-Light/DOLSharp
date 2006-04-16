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
	[PlayerClassAttribute(46, "Warden", "Naturalist")]
	public class ClassWarden : ClassNaturalist
	{
		public ClassWarden() : base() 
		{
			m_profession = "Path of Focus";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.EMP;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.EMP;
			m_wsbase = 360;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Master Forester";
			if (level>=45) return "Master Hunter";
			if (level>=40) return "Master Woodsman";
			if (level>=35) return "Forester";
			if (level>=30) return "Preserver";
			if (level>=25) return "Protector";
			if (level>=20) return "Hunter";
			if (level>=15) return "Guardian";
			if (level>=10) return "Woodsman"; 
			if (level>=5) return "Watcher"; 
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

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Nurture Warden Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Regrowth Warden Spec"));

			if (player.Level >= 5) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, (int)eShieldSize.Medium));
			}
			if (player.Level >= 7) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, (int)eArmorLevel.Medium));
			}
			if (player.Level >= 15)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, (int)eArmorLevel.High));
			}
		}
	}
}
