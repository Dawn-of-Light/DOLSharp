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
	[PlayerClassAttribute((int)eCharacterClass.Champion, "Champion", "Guardian")]
	public class ClassChampion : ClassGuardian
	{
		public ClassChampion() : base() 
		{
			m_profession = "Path of Essence";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.INT;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.INT; //TODO: not sure
			m_wsbase = 380;
			m_baseHP = 760;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Archon";
			if (level>=45) return "Hero";
			if (level>=40) return "Consul";
			if (level>=35) return "Prodigy";
			if (level>=30) return "Defender";
			if (level>=25) return "Brigadier";
			if (level>=20) return "Valiant";
			if (level>=15) return "Propugner";
			if (level>=10) return "Charger"; 
			if (level>=5) return "Page"; 
			return "None"; 
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
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
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, (int)eShieldSize.Medium));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));


				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_LargeWeapons));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Large_Weapons));

				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Valor));
				player.AddSpellLine(SkillBase.GetSpellLine("Valor"));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 18) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, (int)eArmorLevel.High));
			}
			if (player.Level >= 25) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
		}
	}
}
