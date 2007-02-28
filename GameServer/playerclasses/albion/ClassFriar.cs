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

namespace DOL.GS.Scripts
{
	/// <summary>
	///
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Friar, "Friar", "Acolyte")]
	public class ClassFriar : ClassAcolyte
	{
		public ClassFriar() : base()
		{
			m_profession = "Defenders of Albion";
			m_specializationMultiplier = 18;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Chaplain Primus";
			if (level>=45) return "Master Inquisitor";
			if (level>=40) return "Greater Chaplain";
			if (level>=35) return "Inquisitor";
			if (level>=30) return "Chaplain";
			if (level>=25) return "Ecstatic";
			if (level>=20) return "Zealot";
			if (level>=15) return "Fanatic";
			if (level>=10) return "Lesser Chaplain";
			if (level>=5) return "Friar Acolyte";
			return "None";
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Staff));

				player.AddSpellLine(SkillBase.GetSpellLine("Rebirth"));
				player.AddSpellLine(SkillBase.GetSpellLine("Friar Enhancement Spec"));
			}
			if (player.Level >=10)
			{
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			// EVADE
			if (player.Level >= 33)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 5));
			}
			else if (player.Level >= 22)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 4));
			}
			else if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			else if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			else if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
			}
		}
	}
}
