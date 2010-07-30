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
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[CharacterClassAttribute((int)eCharacterClass.MaulerAlb, "Mauler", "Fighter")]
	public class ClassMaulerAlb : CharacterClassBase
	{
		public ClassMaulerAlb()
			: base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PlayerClass.Profession.TempleofIronFist");
			m_specializationMultiplier = 15;
			m_wsbase = 440;
			m_baseHP = 600;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.QUI;
            m_manaStat = eStat.STR;
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Mauler.GetTitle.5");
			return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player)
		{
			base.OnLevelUp(player);

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_MaulerStaff));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_FistWraps));

			player.RemoveSpecialization(Specs.Slash);
            player.RemoveSpecialization(Specs.Thrust);
            player.RemoveSpecialization(Specs.Parry);
            player.RemoveSpecialization(Specs.Crush);
            player.RemoveAllStyles();
            player.RemoveAbility(Abilities.AlbArmor);
			player.RemoveAbility(Abilities.Shield);
			player.RemoveAbility(Abilities.Weapon_Slashing);
			player.RemoveAbility(Abilities.Weapon_Thrusting);
			player.RemoveAbility(Abilities.Weapon_Crushing);
            player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Leather));
			player.AddAbility(SkillBase.GetAbility(Abilities.DefensiveCombatPowerRegeneration, 1));

			player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));

			player.AddSpellLine(SkillBase.GetSpellLine("Aura Manipulation"));
			player.AddSpellLine(SkillBase.GetSpellLine("Magnetism"));
			player.AddSpellLine(SkillBase.GetSpellLine("Power Strikes"));

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Aura_Manipulation));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Magnetism));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Power_Strikes));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mauler_Staff));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Fist_Wraps));

			if (player.Level >= 7)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 13)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 18)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
