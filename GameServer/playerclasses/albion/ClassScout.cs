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

namespace DOL.GS.PlayerClass
{
	/// <summary>
	///
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Scout, "Scout", "Rogue")]
	public class ClassScout : ClassAlbionRogue
	{

		public ClassScout()
			: base()
		{
			m_profession = "Defenders of Albion";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_baseHP = 720;
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return "Master Scout";
			if (level >= 45) return "Ranger";
			if (level >= 40) return "Sentinel";
			if (level >= 35) return "Hawkeye";
			if (level >= 30) return "Pathfinder";
			if (level >= 25) return "Reconnoiterer";
			if (level >= 20) return "Watcher";
			if (level >= 15) return "Tracker";
			if (level >= 10) return "Bowman";
			if (level >= 5) return "Apprentice Scout";
			return "None";
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Longbow);
			return skills;
		}

		public override void OnLevelUp(GamePlayer player)
		{
			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Longbow));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Longbows));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));

			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Studded));
			}
			if (player.Level >= 12)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Camouflage));
			}
		}

		/// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skill"></param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			switch (skill.KeyName)
			{
				case Specs.Longbow:
					if (skill.Level < 3)
					{
						// do nothing
					}
					else if (skill.Level < 6)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 1));
					}
					else if (skill.Level < 9)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 2));
					}
					else if (skill.Level < 12)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 3));
					}
					else if (skill.Level < 15)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 4));
					}
					else if (skill.Level < 18)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 5));
					}
					else if (skill.Level < 21)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 6));
					}
					else if (skill.Level < 24)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 7));
					}
					else if (skill.Level < 27)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 8));
					}
					else if (skill.Level >= 27)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 9));
					}

					if (skill.Level >= 45)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 2));
					}
					else if (skill.Level >= 35)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 1));
					}

					if (skill.Level >= 45)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.SureShot));
					}

					if (skill.Level >= 50)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 3));
					}
					else if (skill.Level >= 40)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 2));
					}
					else if (skill.Level >= 30)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 1));
					}
					break;

				case Specs.Stealth:
					if (skill.Level >= 10)
					{
						player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 1));
					}
					break;
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
