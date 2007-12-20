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
* Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, 
USA.
*
*/
using System;
using DOL.GS;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	///
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Valewalker, "Valewalker", 
"Forester")]
	public class ClassValewalker : ClassForester
	{
		public ClassValewalker() : base()
		{
			m_profession = "Path of Affinity";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.INT;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.INT;
			m_wsbase = 400;
			m_baseHP = 720;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Vale Guardian";
			if (level>=45) return "Scythemaster";
			if (level>=40) return "Vale Warior";
			if (level>=35) return "Ridgewalker";
			if (level>=30) return "Grovewalker";
			if (level>=25) return "Forestfriend";
			if (level>=20) return "Vale Protector";
			if (level>=15) return "Forestwalker";
			if (level>=10) return "Scythewielder";
			if (level>=5) return "Grove Apprentice";
			return "None";
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player)
		{
			base.OnLevelUp(player);

			// Specializations
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Scythe));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Scythe));

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Valewalker Arb Path Spec"));

			if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if(player.Level >= 19)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			if(player.Level >= 23)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 4));
			}
			if(player.Level >= 32)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
		}
	}
}

