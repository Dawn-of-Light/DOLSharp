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
    [CharacterClass((int)eCharacterClass.Vampiir, "Vampiir", "Stalker")]
    public class ClassVampiir : ClassStalker
    {
		private static readonly List<PlayerRace> DefaultEligibleRaces = new()
		{
			 PlayerRace.Celt, PlayerRace.Lurikeen, PlayerRace.Shar,
		};

		public ClassVampiir()
            : base()
        {
            m_profession = "PlayerClass.Profession.PathofAffinity";
            m_specializationMultiplier = 15;
            m_primaryStat = eStat.CON;
            m_secondaryStat = eStat.STR;
            m_tertiaryStat = eStat.DEX;
            //Vampiirs do not have a mana stat
            //Special handling is need in the power pool calculator
            //m_manaStat = eStat.STR;
            m_baseWeaponSkill = 440;
            m_baseHP = 878;
			m_eligibleRaces = DefaultEligibleRaces;

			LoadClassOverride(eCharacterClass.Vampiir);
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
