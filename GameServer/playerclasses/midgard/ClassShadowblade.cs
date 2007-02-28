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
	[PlayerClassAttribute((int)eCharacterClass.Shadowblade, "Shadowblade", "MidgardRogue")]
	public class ClassShadowblade : ClassMidgardRogue
	{

		public ClassShadowblade() : base()
		{
			m_profession = "Loki";
			m_specializationMultiplier = 22;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_baseHP = 760;
		}

		public override string GetTitle(int level) {
			if (level>=50) return "Hand of Loki";
			if (level>=45) return "Lord of Mischief";
			if (level>=40) return "Master of Shadows";
			if (level>=35) return "Master of Deception";
			if (level>=30) return "Lurker Adept";
			if (level>=25) return "Loki's Nightblade";
			if (level>=20) return "Shadow Lurker";
			if (level>=15) return "Deceiver";
			if (level>=10) return "Dark Seeker";
			if (level>=5) return "Initiate Stalker";
			return "None";
		}

		/// <summary>
		/// Checks whether player has ability to use lefthanded weapons
		/// </summary>
		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Stealth);
			return skills;
		}

		public override void OnLevelUp(GamePlayer player)
		{

			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Critical_Strike));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Axe));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Left_Axe));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Envenom));

				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Axes));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrown));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_LeftAxes));

			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 4));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 5));
			}
			if (player.Level >= 40)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 6));
			}
			if (player.Level >= 50)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 7));
			}
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="skill">The skill to train</param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			switch(skill.KeyName)
			{
				case Specs.Stealth:
					if(skill.Level >= 5) player.AddAbility(SkillBase.GetAbility(Abilities.Distraction));
                    if (skill.Level >= 8) player.AddAbility(SkillBase.GetAbility(Abilities.DangerSense));
                    if (skill.Level >= 10) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 1));
					if(skill.Level >= 16) player.AddAbility(SkillBase.GetAbility(Abilities.DetectHidden));
                    if (skill.Level >= 20) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 2));
                    if (skill.Level >= 25) player.AddAbility(SkillBase.GetAbility(Abilities.ClimbWalls));
                    if (skill.Level >= 30) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 3));
                    if (skill.Level >= 40) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 4));
                    if (skill.Level >= 50) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 5));
					break;
			}
		}
	}
}
