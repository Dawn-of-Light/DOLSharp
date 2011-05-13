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
    [DataTable(TableName = "Regions")]
    public class DBRegions : DataObject
    {
        /// <summary>
        /// The region id.
        /// </summary>
        private ushort m_regionID;

        /// <summary>
        /// The region name.
        /// </summary>
        private string m_name;

        /// <summary>
        /// The region description.
        /// </summary>
        private string m_description;

        /// <summary>
        /// The region ip.
        /// </summary>
        private string m_ip;

        /// <summary>
        /// The region port.
        /// </summary>
        private ushort m_port;

        /// <summary>
        /// The region expansion.
        /// </summary>
        private int m_expansion;

        /// <summary>
        /// The region housing flag.
        /// </summary>
        private bool m_housingEnabled;

        /// <summary>
        /// The region diving flag.
        /// </summary>
        private bool m_divingEnabled;

        /// <summary>
        /// The region water level.
        /// </summary>
        private int m_waterLevel;

        public DBRegions()
        {
            m_regionID = 0;
            m_name = string.Empty;
            m_description = string.Empty;
            m_ip = "127.0.0.1";
            m_port = 10400;
            m_expansion = 0;
            m_housingEnabled = false;
            m_divingEnabled = false;
            m_waterLevel = 0;
        }

        /// <summary>
        /// Gets or sets the region id.
        /// </summary>
        [PrimaryKey]
        public ushort RegionID
        {
            get { return m_regionID; }
            set
            {
                Dirty = true;
                m_regionID = value;
            }
        }

        /// <summary>
        /// Gets or sets the region name.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Name
        {
            get { return m_name; }
            set
            {
                Dirty = true;
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the region description.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Description
        {
            get { return m_description; }
            set
            {
                Dirty = true;
                m_description = value;
            }
        }

        /// <summary>
        /// Gets or sets the region ip.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string IP
        {
            get { return m_ip; }
            set
            {
                Dirty = true;
                m_ip = value;
            }
        }

        /// <summary>
        /// Gets or sets the region port.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort Port
        {
            get { return m_port; }
            set
            {
                Dirty = true;
                m_port = value;
            }
        }

        /// <summary>
        /// Gets or sets the region expansion.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Expansion
        {
            get { return m_expansion; }
            set
            {
                Dirty = true;
                m_expansion = value;
            }
        }

        /// <summary>
        /// Gets or sets the region housing flag.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool HousingEnabled
        {
            get { return m_housingEnabled; }
            set
            {
                Dirty = true;
                m_housingEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the region diving flag.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool DivingEnabled
        {
            get { return m_divingEnabled; }
            set
            {
                Dirty = true;
                m_divingEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the region water level.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int WaterLevel
        {
            get { return m_waterLevel; }
            set
            {
                Dirty = true;
                m_waterLevel = value;
            }
        }
    }
}
