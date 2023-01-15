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
using System.Collections.Generic;
using DOL.GS.Realm;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Thane, "Thane", "Viking")]
	public class ClassThane : ClassViking
	{
		public ClassThane()
			: base()
		{
			m_profession = "PlayerClass.Profession.HouseofThor";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.PIE;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		public override List<PlayerRace> EligibleRaces => new List<PlayerRace>()
		{
			 PlayerRace.Dwarf, PlayerRace.Frostalf, PlayerRace.Deifrang, PlayerRace.Norseman, PlayerRace.Troll, PlayerRace.Valkyn,
		};
	}
}
