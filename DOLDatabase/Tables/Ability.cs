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
    /*
     *  public static readonly Dictionary<string, ushort> AbilitiesClientHardcodedIDs = new Dictionary<string, ushort>()
        {
            // { "(nothing)", 0 },
            // { "(nothing)", 1 },
            // { "(nothing)", 2 },
            { Abilities.Quickcast, 3 },
            //{Abilities.Sprint, 4},  looks like they have 2 different sprint abilities, the first with +30% speed -5endu/tick, the second +50%speed -10endu/tick
            { Abilities.Sprint, 5 },
            { Abilities.Evade, 6 },
            { Abilities.Protect, 7 },
            { Abilities.Guard, 8 },
            { Abilities.Intercept, 9 },
            { Abilities.Weapon_Crushing, 10 },
            { Abilities.Weapon_Slashing, 11 },
            { Abilities.Weapon_Thrusting, 12 },
            { Abilities.Weapon_Staves, 13 },
            { Abilities.Weapon_TwoHanded, 14 },
            { Abilities.Weapon_Polearms, 15 },
            { Abilities.Weapon_Longbows, 16 },
            { Abilities.Weapon_Shortbows, 17 },
            { Abilities.Weapon_Crossbow, 18 },
            // { "(nothing)", 19 },
            // { "(nothing)", 20 },
            // { "(nothing)", 21 },
            { Abilities.Weapon_Swords, 22 },
            { Abilities.Weapon_Axes, 23 },
            { Abilities.Weapon_Hammers, 24 },
            { Abilities.Weapon_Spears, 25 },
            { Abilities.Weapon_CompositeBows, 26 },
            { Abilities.Weapon_Thrown, 27 },
            { Abilities.Weapon_LeftAxes, 28 },
            { Abilities.Berserk, 29 },
            //{ Abilities.QuarterStaves, 30 }, // quarterstaves?
            { Abilities.Weapon_Blades, 31 },
            { Abilities.Weapon_Blunt, 32 },
            { Abilities.Weapon_Piercing, 33 },
            { Abilities.Weapon_LargeWeapons, 34 },
            { Abilities.Weapon_RecurvedBows, 35 },
            //{ Abilities.Slings, 36 },  what is Slings?
            { Abilities.Weapon_CelticSpear, 37 },
            { Abilities.Engage, 38 },
            // { "(nothing)", 39 },
            { Abilities.Distraction, 40 },
            { Abilities.DangerSense, 41 },
            { Abilities.DetectHidden, 42 },
            { Abilities.SafeFall, 43 },
            // { "(nothing)", 44 },
            { Abilities.Climbing, 45 },
            //{ Abilities.SpiritHunt, 46 }, missing
            //{ Abilities.Concentration, 47 }, missing
            { Abilities.Camouflage, 48 },
            { Abilities.Advanced_Evade, 49 },
            { Abilities.Weapon_Flexible, 50 },
            { Abilities.Weapon_HandToHand, 51 },
            { Abilities.Weapon_Scythe, 52 },
            { Abilities.ChargeAbility, 53 },
            // { "(nothing)", 54 },
            // { "(nothing)", 55 },
            //{ Abilities.PickPocket, 56 }, missing
            //{ Abilities.SiegeMaster, 57 }, missing
            //{ Abilities.UnburdenedWarrior, 58 }, missing
            // { "(nothing)", 59 },
            // { "(nothing)", 60 },
            //{ Abilities.Sabotage, 61 }, missing
            //{ Abilities.Greatness, 62 }, missing
            //{ Abilities.UnduringPoison, 63 }, missing
            { Abilities.Bodyguard, 64 },
            //{ Abilities.Lookout, 65 }, missing
            { Abilities.SureShot, 66 },
            { Abilities.Weapon_FistWraps, 67 },
            { Abilities.Weapon_MaulerStaff, 68 },
            { Abilities.DefensiveCombatPowerRegeneration, 69 },
            //{ Abilities.OffensiveCombatPowerRegeneration, 70 }, missing
        };
    */
    /// <summary>
    /// The ability table
    /// </summary>
    [DataTable(TableName="Ability")]
    public class DBAbility : DataObject
    {
        protected int m_abilityID;

        protected string m_keyName;
        protected int m_iconID = 0;       // 0 if no icon, ability icons start at 0x190
        protected int m_internalID;
        protected string m_name = "unknown";
        protected string m_description = "no description";
        protected string m_implementation = null;

        /// <summary>
        /// Create ability
        /// </summary>
        public DBAbility()
        {
        }

        /// <summary>
        /// Ability Primary Key Auto Increment
        /// </summary>
        [PrimaryKey(AutoIncrement=true)]
        public int AbilityID {
            get { return m_abilityID; }
            set { m_abilityID = value; }
        }

        /// <summary>
        /// The key of this ability
        /// </summary>
        [DataElement(AllowDbNull=false, Unique=true, Varchar=100)]
        public string KeyName
        {
            get { return m_keyName;   }

            set {
                Dirty = true;
                m_keyName = value;
            }
        }

        /// <summary>
        /// Name of this ability
        /// </summary>
        [DataElement(AllowDbNull=false, Varchar=255)]
        public string Name
        {
            get { return m_name;  }

            set {
                Dirty = true;
                m_name = value;
            }
        }

        /// <summary>
        /// Ability ID (new in 1.112)
        /// </summary>
        [DataElement(AllowDbNull=true)]
        public int InternalID
        {
            get { return m_internalID; }

            set
            {
                Dirty = true;
                m_internalID = value;
            }
        }

        /// <summary>
        /// Small description of this ability
        /// </summary>
        [DataElement(AllowDbNull=false)]
        public string Description
        {
            get { return m_description;   }

            set
            {
                Dirty = true;
                m_description = value;
            }
        }

        /// <summary>
        /// Icon of ability
        /// </summary>
        [DataElement(AllowDbNull=false)]
        public int IconID
        {
            get { return m_iconID;    }

            set
            {
                Dirty = true;
                m_iconID = value;
            }
        }

        /// <summary>
        /// Ability Implementation Class
        /// </summary>
        [DataElement(AllowDbNull=true, Varchar=255)]
        public string Implementation
        {
            get { return m_implementation;    }

            set
            {
                Dirty = true;
                m_implementation = value;
            }
        }
    }
}