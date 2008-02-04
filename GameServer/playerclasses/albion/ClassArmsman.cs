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
using System.Collections;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Armsman, "Armsman", "Fighter", "Armswoman")]
	public class ClassArmsman : ClassFighter
	{
		public ClassArmsman() : base() 
		{
			m_profession = "Defenders of Albion";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_baseHP = 880;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "General";
			if (level>=45) return "Captain";
			if (level>=40) return "Centurian";
			if (level>=35) return "Lieutenant";
			if (level>=30) return "Sergeant";
			if (level>=25) return "Legionnaire";
			if (level>=20) return "Soldier";
			if (level>=15) return "Infantry";
			if (level>=10) return "Footsoldier"; 
			if (level>=5) return "Enlistee"; 
			return "None"; 
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Slash);
			skills.Add(Specs.Thrust);
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Polearms));
				player.AddAbility(SkillBase.GetAbility(Abilities.TauntingShout));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Polearms));
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
			}
			if (player.Level >= 10)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Two_Handed));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_TwoHanded));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
			}
			if (player.Level >= 11)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 12)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 15)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Crossbow));
				player.AddAbility(SkillBase.GetAbility(Abilities.MetalGuard));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Crossbow));
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Plate));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 27)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.BolsteringRoar));
			}
			if (player.Level >= 35)
			{                              
                player.AddAbility(SkillBase.GetAbility(Abilities.MemoriesOfWar));
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
			if (player.Level >= 40)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Rampage));
			}
			if (player.Level >= 50)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Fury));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
