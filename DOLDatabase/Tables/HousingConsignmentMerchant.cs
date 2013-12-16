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
    /// Contains all Consignment Merchants and owners
    /// </summary>
    [DataTable(TableName = "HouseConsignmentMerchant")]
    public class HouseConsignmentMerchant : DataObject
    {
		long m_ID;
		private string m_ownerID;
        private int m_houseNumber;
        private long m_money;


        public HouseConsignmentMerchant()
        {
			m_ownerID = "";
			m_houseNumber = 0;
			m_money = 0;
        }

		[PrimaryKey(AutoIncrement = true)]
		public long ID
		{
			get { return m_ID; }
			set
			{
				Dirty = true;
				m_ID = value;
			}
		}

		/// <summary>
		/// The owner id of this merchant.  Can be player or guild.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar=128, Index=true)]
		public string OwnerID
		{
			get
			{
				return m_ownerID;
			}
			set
			{
				Dirty = true;
				m_ownerID = value;
			}
		}

        /// <summary>
        /// The Housenumber of the Merchant
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public int HouseNumber
        {
            get
            {
                return m_houseNumber;
            }
            set
            {
                Dirty = true;
                m_houseNumber = value;
            }
        }


        /// <summary>
        /// The value of the money/bp the merchant currently holds
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public long Money
        {
            get
            {
                return m_money;
            }
            set
            {
                Dirty = true;
                m_money = value;
            }
        }
    }
}
