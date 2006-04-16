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
	[PlayerClassAttribute((int)eCharacterClass.Blademaster, "Blademaster", "Guardian")]
	public class ClassBlademaster : ClassGuardian
	{
		public ClassBlademaster() : base() 
		{
			m_profession = "Path of Harmony";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_wsbase = 440;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Bladesong";
			if (level>=45) return "Armigerant";
			if (level>=40) return "Duelist";
			if (level>=35) return "Blade Dancer";
			if (level>=30) return "Vigilante";
			if (level>=25) return "Gallant";
			if (level>=20) return "Blade Weaver";
			if (level>=15) return "Sabruer";
			if (level>=10) return "Stylist"; 
			if (level>=5) return "Bladist"; 
			return "None"; 
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Celtic_Dual));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, (int)eShieldSize.Medium));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 19) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Triple_Wield));
			}
			if (player.Level >= 23) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 25) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}			
			if (player.Level >= 30) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Flurry));
			}
			if (player.Level >= 32) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 35) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Advanced_Evade));
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
			if (player.Level >= 40) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Charge));
			}
		}
	}
}
