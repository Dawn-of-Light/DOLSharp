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

namespace DOL.GS.Scripts
{
	/// <summary>
	///
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Mercenary, "Mercenary", "Fighter")]
	public class ClassMercenary : ClassFighter
	{
		public ClassMercenary() : base()
		{
			m_profession = "Guild of Shadows";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_baseHP = 880;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Mercenary Master";
			if (level>=45) return "Master Warrior";
			if (level>=40) return "Warrior";
			if (level>=35) return "Veteran";
			if (level>=30) return "Blackguard";
			if (level>=25) return "Soldier of Fortune";
			if (level>=20) return "Swashbuckler";
			if (level>=15) return "Escaladar";
			if (level>=10) return "Pugilist";
			if (level>=5) return "Strong-Arm";
			return "None";
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Dual_Wield));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));

			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 17)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
			}
			if (player.Level >= 19)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.DirtyTricks));
			}
			if (player.Level >= 23)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
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
		}
	}
}
