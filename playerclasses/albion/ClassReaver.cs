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
	[PlayerClassAttribute((int)eCharacterClass.Reaver, "Reaver", "Fighter")]
	public class ClassReaver : ClassFighter
	{
		public ClassReaver() : base()
		{
			m_profession = "Temple of Arawn";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.PIE;
			m_manaStat = eStat.PIE;
			m_wsbase = 380;
			m_baseHP = 760;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Dark Knight";
			if (level>=45) return "Hero of Arawn";
			if (level>=40) return "Guardian of Arawn";
			if (level>=35) return "Underworld Knight";
			if (level>=30) return "Defender of Arawn";
			if (level>=25) return "Gray Knight";
			if (level>=20) return "Soul Protector";
			if (level>=15) return "Protector of Arawn";
			if (level>=10) return "Strongarm of Arawn";
			if (level>=5) return "Dark Squire";
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
			skills.Add(Specs.Flexible);
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
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Flexible));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Flexible));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));

				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Soulrending));
				player.AddSpellLine(SkillBase.GetSpellLine("Soulrending"));
			}
			if (player.Level >= 9)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 17)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
		}
	}
}
