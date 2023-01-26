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
	[CharacterClass((int)eCharacterClass.Minstrel, "Minstrel", "Rogue")]
	public class ClassMinstrel : ClassAlbionRogue
	{
		private static readonly List<string> DefaultAutotrainableSkills = new() { Specs.Instruments };
		private static readonly List<PlayerRace> DefaultEligibleRaces = new()
		{
			 PlayerRace.Briton, PlayerRace.Highlander, PlayerRace.Saracen,
		};

		public ClassMinstrel()
			: base()
		{
			m_profession = "PlayerClass.Profession.Academy";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CHR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.CHR;
			m_baseWeaponSkill = 380;
			m_autotrainableSkills = DefaultAutotrainableSkills;
			m_eligibleRaces = DefaultEligibleRaces;
			m_maxPulsingSpells = 2;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
