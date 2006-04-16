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

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Necromancer, "Necromancer", "Disciple")]
	public class ClassNecromancer : ClassDisciple
	{
		public ClassNecromancer() : base() 
		{
			m_profession = "Temple of Arawn";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Master Reanimator";
			if (level>=45) return "Deathbringer";
			if (level>=40) return "Reanimator";
			if (level>=35) return "Deathseeker";
			if (level>=30) return "Priest of Arawn";
			if (level>=25) return "Deathstalker";
			if (level>=20) return "Adept of Arawn";
			if (level>=15) return "Summoner";
			if (level>=10) return "Servant of Arawn"; 
			if (level>=5) return "Apprentice Necromancer"; 
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Death_Servant));

				player.AddSpellLine(SkillBase.GetSpellLine("Painworking"));
				player.AddSpellLine(SkillBase.GetSpellLine("Necro Deathsight"));
				player.AddSpellLine(SkillBase.GetSpellLine("Necro Painworking"));
				player.AddSpellLine(SkillBase.GetSpellLine("Death Servant Spec"));
			}
		}
	}
}
