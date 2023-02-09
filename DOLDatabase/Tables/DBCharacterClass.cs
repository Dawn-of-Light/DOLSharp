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
using DOL.Database.Attributes;

namespace DOL.Database
{
    [DataTable(TableName = "CharacterClass")]
    public class DBCharacterClass : DataObject
    {
        private byte id;
        private byte baseClassID;
        private byte specPointMultiplier;
        private short baseHP;
        private short baseWeaponSkill;
        private string autoTrainableSkills = "";
        private string eligibleRaces = "";
        private byte classType;
        private string name;
        private string femaleName = "";
        private string professionTranslationID;
        private byte primaryStat;
        private byte secondaryStat;
        private byte tertiaryStat;
        private byte manaStat;
        private bool canUseLeftHandedWeapon;
        private byte maxPulsingSpells;

        [PrimaryKey]
        public byte ID
        {
            get => id;
            set
            {
                Dirty = true;
                id = value;
            }
        }

        [DataElement]
        public byte BaseClassID
        {
            get => baseClassID;
            set
            {
                Dirty = true;
                baseClassID = value;
            }
        }

        [DataElement]
        public byte ClassType
        {
            get => classType;
            set
            {
                Dirty = true;
                classType = value;
            }
        }

        [DataElement]
        public string Name
        {
            get => name;
            set
            {
                Dirty = true;
                name = value;
            }
        }

        [DataElement]
        public string FemaleName
        {
            get => femaleName;
            set
            {
                Dirty = true;
                femaleName = value;
            }
        }

        [DataElement]
        public byte SpecPointMultiplier
        {
            get => specPointMultiplier;
            set
            {
                Dirty = true;
                specPointMultiplier = value;
            }
        }

        [DataElement]
        public string AutoTrainSkills
        {
            get => autoTrainableSkills;
            set
            {
                Dirty = true;
                autoTrainableSkills = value;
            }
        }


        [DataElement]
        public byte PrimaryStat
        {
            get => primaryStat;
            set
            {
                Dirty = true;
                primaryStat = value;
            }
        }

        [DataElement]
        public byte SecondaryStat
        {
            get => secondaryStat;
            set
            {
                Dirty = true;
                secondaryStat = value;
            }
        }

        [DataElement]
        public byte TertiaryStat
        {
            get => tertiaryStat;
            set
            {
                Dirty = true;
                tertiaryStat = value;
            }
        }

        [DataElement]
        public byte ManaStat
        {
            get => manaStat;
            set
            {
                Dirty = true;
                manaStat = value;
            }
        }

        [DataElement]
        public short BaseHP
        {
            get => baseHP;
            set
            {
                Dirty = true;
                baseHP = value;
            }
        }

        [DataElement]
        public short BaseWeaponSkill
        {
            get => baseWeaponSkill;
            set
            {
                Dirty = true;
                baseWeaponSkill = value;
            }
        }

        [DataElement]
        public string EligibleRaces
        {
            get => eligibleRaces;
            set
            {
                Dirty = true;
                eligibleRaces = value;
            }
        }

        [DataElement]
        public bool CanUseLeftHandedWeapon
        {
            get => canUseLeftHandedWeapon;
            set
            {
                Dirty = true;
                canUseLeftHandedWeapon = value;
            }
        }

        [DataElement]
        public string ProfessionTranslationID
        {
            get => professionTranslationID;
            set
            {
                Dirty = true;
                professionTranslationID = value;
            }
        }

        //No DataElement on purpose
        public byte MaxPulsingSpells
        {
            get => maxPulsingSpells;
            set
            {
                Dirty = true;
                maxPulsingSpells = value;
            }
        }
    }
}
