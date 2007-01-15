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
	[PlayerClassAttribute((int)eCharacterClass.Minstrel, "Minstrel", "Rogue")]
	public class ClassMinstrel : ClassAlbionRogue
	{
		
		public ClassMinstrel() : base()
		{
			m_profession = "Academy";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CHR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.CHR;
			m_wsbase = 380;
			m_baseHP = 720;
		}

		public override string GetTitle(int level) {
			if (level>=50) return "Virtuoso";
			if (level>=45) return "Master Soloist";
			if (level>=40) return "Soloist";
			if (level>=35) return "Elegist";
			if (level>=30) return "Rhapsodist";
			if (level>=25) return "Troubadour";
			if (level>=20) return "Lyricist";
			if (level>=15) return "Versesmith";
			if (level>=10) return "Sonneteer"; 
			if (level>=5) return "Balladeer"; 
			return "None"; 
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Instruments);
			return skills;
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Instruments));
				player.AddSpellLine(SkillBase.GetSpellLine("Instruments"));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Instruments));
			}
			if (player.Level >= 10)
			{				
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Studded));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
			}
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			switch(skill.KeyName)
			{
				case Specs.Stealth:
                    if (skill.Level >= 5) player.AddAbility(SkillBase.GetAbility(Abilities.Distraction));
                    if (skill.Level >= 8) player.AddAbility(SkillBase.GetAbility(Abilities.DangerSense));
                    if (skill.Level >= 10) player.AddAbility(SkillBase.GetAbility(Abilities.SafeFall, 1));
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
