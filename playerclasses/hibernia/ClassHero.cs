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

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[CharacterClassAttribute((int)eCharacterClass.Hero, "Hero", "Guardian", "Heroine")]
	public class ClassHero : ClassGuardian
	{
		public ClassHero() : base() 
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofFocus");
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_wsbase = 440;
		}

		public override string GetTitle(int level) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Hero.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{		
			base.OnLevelUp(player);

			player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));

			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_CelticSpear));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Celtic_Spear));

			if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.TauntingShout));
			}
			if (player.Level >= 10) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_LargeWeapons));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Large_Weapons));

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
				player.AddAbility(SkillBase.GetAbility(Abilities.MetalGuard));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
				player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Scale));
				player.AddAbility(SkillBase.GetAbility(Abilities.Stag, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 25) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Stag, 2));
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
				player.AddAbility(SkillBase.GetAbility(Abilities.Stag, 3));
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
			if (player.Level >= 45) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Stag, 4));
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
