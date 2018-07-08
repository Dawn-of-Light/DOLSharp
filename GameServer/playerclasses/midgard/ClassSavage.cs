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
    /// Midgard Savage Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Savage, "Savage", "Viking")]
    public class ClassSavage : ClassViking
    {

        public ClassSavage()
        {
            Profession = "PlayerClass.Profession.HouseofKelgor";
            SpecPointsMultiplier = 15;
            PrimaryStat = eStat.DEX;
            SecondaryStat = eStat.QUI;
            TertiaryStat = eStat.STR;
            WeaponSkillBase = 400;
        }

        public override bool CanUseLefthandedWeapon => true;

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
