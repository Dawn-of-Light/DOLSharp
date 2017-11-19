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

namespace DOL.GS.PlayerClass
{
    /// <summary>
    /// Albion Minstrel Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Minstrel, "Minstrel", "Rogue")]
	public class ClassMinstrel : ClassAlbionRogue
	{
		private static readonly string[] AutotrainableSkills = new[] { Specs.Instruments };

		public ClassMinstrel()
			: base()
		{
			m_profession = "PlayerClass.Profession.Academy";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CHR;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.STR;
			m_manaStat = eStat.CHR;
			m_wsbase = 380;
			m_baseHP = 720;
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		public override ushort MaxPulsingSpells
		{
			get { return 2; }
		}
	}
}
