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
	[PlayerClassAttribute((int)eCharacterClass.Paladin, "Paladin", "Fighter")]
	public class ClassPaladin : ClassFighter
	{
		public ClassPaladin() : base()
		{
			m_profession = "Church of Albion";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.CON;
			m_secondaryStat = eStat.PIE;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
			m_wsbase = 380;
			m_baseHP = 760;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Templar";
			if (level>=45) return "Champion";
			if (level>=40) return "Justicator";
			if (level>=35) return "Crusader";
			if (level>=30) return "Benefactor";
			if (level>=25) return "Guardian";
			if (level>=20) return "Defender";
			if (level>=15) return "Protector";
			if (level>=10) return "Tyro";
			if (level>=5) return "Neophyte";
			return "None";
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Slash);
			skills.Add(Specs.Chants);
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Chants));
				player.AddSpellLine(SkillBase.GetSpellLine("Chants"));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Two_Handed));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_TwoHanded));
			}
			if (player.Level >=9)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 14)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 15)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 19)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Plate));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
