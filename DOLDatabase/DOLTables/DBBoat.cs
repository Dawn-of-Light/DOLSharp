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
using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
    /// <summary>
    /// DBBoat is database of Player Boats
    /// </summary>
    [DataTable(TableName = "PlayerBoats", PreCache = true)]
    public class DBBoat : DataObject
    {
        static bool m_autoSave;
        static string boat_id;
        static string boat_owner;
        static string boat_name;
        static ushort boat_model;
        static int boat_maxspeedbase;

        public DBBoat()
        {
            boat_id = "";
            boat_owner = "";
            boat_name = "";
            boat_model = 0;
            boat_maxspeedbase = 0;
            m_autoSave = true;
        }

        /// <summary>
        /// The ID of the boat
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string BoatID
        {
            get
            {
                return boat_id;
            }
            set
            {
                Dirty = true;
                boat_id = value;
            }
        }

        /// <summary>
        /// The Owner of the boat
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string BoatOwner
        {
            get
            {
                return boat_owner;
            }
            set
            {
                Dirty = true;
                boat_owner = value;
            }
        }

        /// <summary>
        /// The Name of the boat
        /// </summary>
        [PrimaryKey]
        public string BoatName
        {
            get
            {
                return boat_name;
            }
            set
            {
                Dirty = true;
                boat_name = value;
            }
        }
        
        /// <summary>
        /// The Model of the boat
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort BoatModel
        {
            get
            {
                return boat_model;
            }
            set
            {
                Dirty = true;
                boat_model = value;
            }
        }


        /// <summary>
        /// The Max speed base of the boat
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int BoatMaxSpeedBase
        {
            get
            {
                return boat_maxspeedbase;
            }
            set
            {
                Dirty = true;
                boat_maxspeedbase = value;
            }
        }
        
        /// <summary>
        /// autosave boats table
        /// </summary>
        override public bool AutoSave
        {
            get
            {
                return m_autoSave;
            }
            set
            {
                m_autoSave = value;
            }
        }
    }
}
