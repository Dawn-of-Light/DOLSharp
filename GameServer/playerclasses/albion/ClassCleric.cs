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
using DOL.GS.Realm;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Cleric, "Cleric", "Acolyte")]
	public class ClassCleric : ClassAcolyte
	{
		private static readonly List<PlayerRace> DefaultEligibleRaces = new List<PlayerRace>()
		{
			PlayerRace.Avalonian, PlayerRace.Briton, PlayerRace.Highlander,
		};

		public ClassCleric()
			: base()
		{
			m_profession = "PlayerClass.Profession.ChurchofAlbion";
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.PIE;
			m_eligibleRaces = DefaultEligibleRaces;

			LoadClassOverride(eCharacterClass.Cleric);
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
