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
using System.Collections;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Warrior, "Warrior", "Viking")]
	public class ClassWarrior : ClassViking
	{
		public ClassWarrior() : base() 
		{
			m_profession = "House of Tyr";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_wsbase = 460;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Hand of Tyr";
			if (level>=45) return "Warlord";
			if (level>=40) return "Warmonger";
			if (level>=35) return "Marauder";
			if (level>=30) return "Elite Skirmisher";
			if (level>=25) return "Myrmidon of Tyr";
			if (level>=20) return "Veteran";
			if (level>=15) return "Footman";
			if (level>=10) return "Yeoman Fighter"; 
			if (level>=5) return "Initiate Fighter"; 
			return "None"; 
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Axe);
			skills.Add(Specs.Hammer);
			skills.Add(Specs.Sword);
			return skills;
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
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrown));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));

				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Thrown_Weapons));
			}
			if (player.Level >= 7) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Chain));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
			}
			if (player.Level >= 12) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 13) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 18) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 35)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
		}
	}
}
