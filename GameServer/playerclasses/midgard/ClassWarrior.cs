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
    /// Midgard Warrior Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Warrior, "Warrior", "Viking")]
    public class ClassWarrior : ClassViking
    {
        private static readonly string[] AutotrainableSkills = { Specs.Axe, Specs.Hammer, Specs.Sword };

        public ClassWarrior()
        {
            Profession = "PlayerClass.Profession.HouseofTyr";
            SpecPointsMultiplier = 20;
            PrimaryStat = eStat.STR;
            SecondaryStat = eStat.CON;
            TertiaryStat = eStat.DEX;
            WeaponSkillBase = 460;
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
