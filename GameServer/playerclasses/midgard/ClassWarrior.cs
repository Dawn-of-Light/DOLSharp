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
using DOL.Language;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Warrior, "Warrior", "Viking")]
	public class ClassWarrior : ClassViking
	{
		private static readonly string[] AutotrainableSkills = new[] { Specs.Axe, Specs.Hammer, Specs.Sword };

		public ClassWarrior() : base() 
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.HouseofTyr");
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_wsbase = 460;
		}

		public override string GetTitle(int level) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Warrior.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		public override IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrown));
			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Thrown_Weapons));

			if(player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.TauntingShout));
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
				player.AddAbility(SkillBase.GetAbility(Abilities.MetalGuard));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 18) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.BolsteringRoar));
			}
			if (player.Level >= 35)
			{                              
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
			if (player.Level >= 40)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Rampage));
			}
			if (player.Level >= 41)
			{
                player.AddAbility(SkillBase.GetAbility(Abilities.MemoriesOfWar));
                player.AddAbility(SkillBase.GetAbility(Abilities.ScarsOfBattle));
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
