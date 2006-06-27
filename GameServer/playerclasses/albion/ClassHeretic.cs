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
	[PlayerClassAttribute((int)eCharacterClass.Heretic, "Heretic", "Acolyte")]
	public class ClassHeretic : ClassAcolyte
	{
		public ClassHeretic() : base()
		{
			m_profession = "Temple of Arawn";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override string GetTitle(int level)
		{
			if (level>=50) return "Arawn's Own Reanimator";
			if (level>=45) return "Priest of Arawn";
			if (level>=40) return "Apostle of Arawn";
			if (level>=35) return "Servant of Arawn";
			if (level>=30) return "Advocate of Arawn";
			if (level>=25) return "Devotee of Arawn";
			if (level>=20) return "Proselyte";
			if (level>=15) return "Initiate";
			if (level>=10) return "Apprentice";
			if (level>=5) return "Aspirant";
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
				player.RemoveAbility(Abilities.AlbArmor);
				player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, (int)eArmorLevel.VeryLow));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Flexible));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Flexible));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Crush));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpellLine(SkillBase.GetSpellLine("Heretic Rejuvenation Spec"));
				player.AddSpellLine(SkillBase.GetSpellLine("Heretic Enhancement Spec"));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
		}
	}
}
