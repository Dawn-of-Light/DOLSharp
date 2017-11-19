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

namespace DOL.GS.PlayerClass
{
    /// <summary>
    /// Midgard Bonebancer Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Bonedancer, "Bonedancer", "Mystic")]
	public class ClassBonedancer : CharacterClassBoneDancer
	{
		public ClassBonedancer()
			: base()
		{
			m_specializationMultiplier = 10;
			m_wsbase = 280;
			m_baseHP = 560;
			m_manaStat = eStat.PIE;

			m_profession = "PlayerClass.Profession.HouseofBodgar";
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
