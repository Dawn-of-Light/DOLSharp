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
    /// Albion Armsman Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Armsman, "Armsman", "Fighter", "Armswoman")]
    public class ClassArmsman : ClassFighter
    {
        private static readonly string[] AutotrainableSkills = { Specs.Slash, Specs.Thrust };

        public ClassArmsman()
        {
            Profession = "PlayerClass.Profession.DefendersofAlbion";
            SpecPointsMultiplier = 20;
            PrimaryStat = eStat.STR;
            SecondaryStat = eStat.CON;
            TertiaryStat = eStat.DEX;
            BaseHP = 880;
        }

        public override IList<string> GetAutotrainableSkills()
        {
            return AutotrainableSkills;
        }

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
