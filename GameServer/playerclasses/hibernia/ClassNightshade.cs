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
	[PlayerClassAttribute((int)eCharacterClass.Nightshade, "Nightshade", "Stalker")]
	public class ClassNightshade : ClassStalker
	{
		public ClassNightshade() : base() 
		{
			m_profession = "Path of Essence";
			m_specializationMultiplier = 22;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.INT; //TODO: not sure
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Shadowmaster";
			if (level>=45) return "Shadowcaster";
			if (level>=40) return "Shadow Walker";
			if (level>=35) return "Shadow";
			if (level>=30) return "Nightguard";
			if (level>=25) return "Darkguard";
			if (level>=20) return "Darkblade";
			if (level>=15) return "Darkshade";
			if (level>=10) return "Nightwalker"; 
			if (level>=5) return "Trickster"; 
			return "None"; 
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			skills.Add(Specs.Stealth);
			return skills;
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Celtic_Dual));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Critical_Strike));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Envenom));
			player.AddSpellLine(SkillBase.GetSpellLine("Nightshade"));
		
			if (player.Level >= 5) 
			{			
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));	
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
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
