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
    /// <summary>
    /// Specialization Table
    /// </summary>
    [DataTable(TableName="Specialization")]
    public class DBSpecialization : DataObject
    {
        protected int m_SpecializationID;

        protected string m_keyName;
        protected string m_name = "unknown spec";
        protected string m_description = "no description";
        protected ushort m_icon = 0;
        protected string m_implementation;

        /// <summary>
        /// Constructor
        /// </summary>
        public DBSpecialization()
        {
            AllowAdd = false;
        }

        /// <summary>
        /// Primary Key Auto Increment.
        /// </summary>
        [PrimaryKey(AutoIncrement=true)]
        public int SpecializationID {
            get { return m_SpecializationID; }
            set { Dirty = true; m_SpecializationID = value; }
        }

        /// <summary>
        /// Specialization Unique Key Name (Primary Key)
        /// </summary>
        [DataElement(AllowDbNull=false, Unique=true, Varchar=100)]
        public string KeyName {
            get { return m_keyName;   }

            set {
                Dirty = true;
                m_keyName = value;
            }
        }

        /// <summary>
        /// Specizalization Display Name
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
        /// Specialization Icon ID (0 = disabled)
        /// </summary>
        [DataElement(AllowDbNull=false)]
        public ushort Icon
        {
            get { return m_icon;  }

            set {
                Dirty = true;
                m_icon = value;
            }
        }

        /// <summary>
        /// Specialization Description
        /// </summary>
        [DataElement(AllowDbNull=true)]
        public string Description
        {
            get { return m_description;   }

            set {
                Dirty = true;
                m_description = value;
            }
        }

        /// <summary>
        /// Implementation of this Specialization.
        /// </summary>
        [DataElement(AllowDbNull=true, Varchar=255)]
        public string Implementation {
            get { return m_implementation; }
            set { Dirty = true; m_implementation = value; }
        }

        /// <summary>
        /// Styles attached to this Specizalization
        /// </summary>
        [Relation(LocalField = "KeyName", RemoteField = "SpecKeyName", AutoLoad = true, AutoDelete=true)]
        public DBStyle[] Styles;

        /// <summary>
        /// Spell Lines attached to this Specialization
        /// </summary>
        [Relation(LocalField = "KeyName", RemoteField = "Spec", AutoLoad = true, AutoDelete=false)]
        public DBSpellLine[] SpellLines;

        /// <summary>
        /// Ability Lines Constraints attached to this Specialization
        /// </summary>
        [Relation(LocalField = "KeyName", RemoteField = "Spec", AutoLoad = true, AutoDelete=true)]
        public DBSpecXAbility[] AbilityConstraints;
    }
}