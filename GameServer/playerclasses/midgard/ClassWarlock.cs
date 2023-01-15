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
	[CharacterClass((int)eCharacterClass.Warlock, "Warlock", "Mystic")]
	public class ClassWarlock : ClassMystic
	{
		public ClassWarlock()
			: base()
		{
			m_profession = "PlayerClass.Profession.HouseofHel";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.PIE;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		/// <summary>
		/// FIXME this has nothing to do here !
		/// </summary>
		/// <param name="line"></param>
		/// <param name="spell"></param>
		/// <returns></returns>
		public override bool CanChangeCastingSpeed(SpellLine line, Spell spell)
		{
			if (spell.SpellType == "Chamber")
				return false;

			if ((line.KeyName == "Cursing"
				 || line.KeyName == "Cursing Spec"
				 || line.KeyName == "Hexing"
				 || line.KeyName == "Witchcraft")
				&& (spell.SpellType != "ArmorFactorBuff"
					&& spell.SpellType != "Bladeturn"
					&& spell.SpellType != "ArmorAbsorptionBuff"
					&& spell.SpellType != "MatterResistDebuff"
					&& spell.SpellType != "Uninterruptable"
					&& spell.SpellType != "Powerless"
					&& spell.SpellType != "Range"
					&& spell.Name != "Lesser Twisting Curse"
					&& spell.Name != "Twisting Curse"
					&& spell.Name != "Lesser Winding Curse"
					&& spell.Name != "Winding Curse"
					&& spell.Name != "Lesser Wrenching Curse"
					&& spell.Name != "Wrenching Curse"
					&& spell.Name != "Lesser Warping Curse"
					&& spell.Name != "Warping Curse"))
			{
				return false;
			}

			return true;
		}

		public override List<PlayerRace> EligibleRaces => new List<PlayerRace>()
		{
			 PlayerRace.Frostalf, PlayerRace.Kobold, PlayerRace.Norseman, PlayerRace.Troll,
		};
	}
}
