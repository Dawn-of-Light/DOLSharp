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
	[PlayerClassAttribute((int)eCharacterClass.Berserker, "Berserker", "Viking")]
	public class ClassBerserker : ClassViking
	{
		public ClassBerserker() : base() 
		{
			m_profession = "House of Modi";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_wsbase = 440;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Hand of Modi";
			if (level>=45) return "Frenzien Reaver";
			if (level>=40) return "Frenzied Mauler";
			if (level>=35) return "Frenzied Pillager";
			if (level>=30) return "Ursine Reaver";
			if (level>=25) return "Ursine Mauler";
			if (level>=20) return "Ursine Pillager";
			if (level>=15) return "Ursine Fervent";
			if (level>=10) return "Ursine Seeker"; 
			if (level>=5) return "Ursine Initiate"; 
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
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_LeftAxes));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Left_Axe));

				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrown));
				player.AddAbility(SkillBase.GetAbility(Abilities.Berserk, 1));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Berserk, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));

			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Berserk, 3));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}		
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
				player.AddAbility(SkillBase.GetAbility(Abilities.Berserk, 4));
			}	
			if (player.Level >= 35)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Advanced_Evade));
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
			if (player.Level >= 40)
			{
				//player.AddAbility(SkillBase.GetAbility(Abilities.Charge));
			}
		}
	}
}
