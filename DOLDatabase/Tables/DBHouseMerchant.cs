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
    /// DBHouseMerchant contains all Consignment Merchants
    /// </summary>
    [DataTable(TableName = "DBHouseMerchant")]
    public class DBHouseMerchant : DataObject
    {
        static int house_number;
        static int quantity;

        public DBHouseMerchant()
        {
            house_number = 0;
            quantity = 0;
        }

        /// <summary>
        /// The Housenumber of the Merchant
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int HouseNumber
        {
            get
            {
                return house_number;
            }
            set
            {
                Dirty = true;
                house_number = value;
            }
        }


        /// <summary>
        /// The value of the money/bp the merchant currently holds
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                Dirty = true;
                quantity = value;
            }
        }
    }
}
