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
    /// Albion Mauler Character Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.MaulerAlb, "Mauler", "Fighter")]
    public class ClassMaulerAlb : ClassFighter
    {
        public ClassMaulerAlb()
        {
            Profession = "PlayerClass.Profession.TempleofIronFist";
            SpecPointsMultiplier = 15;
            WeaponSkillBase = 440;
            BaseHP = 600;
            PrimaryStat = eStat.STR;
            SecondaryStat = eStat.CON;
            TertiaryStat = eStat.QUI;
            ManaStat = eStat.STR;
        }

        public override bool CanUseLefthandedWeapon => true;

        public override eClassType ClassType => eClassType.Hybrid;

        public override GameTrainer.eChampionTrainerType ChampionTrainerType()
        {
            return GameTrainer.eChampionTrainerType.Fighter;
        }

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
