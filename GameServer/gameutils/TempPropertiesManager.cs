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
using System.Linq;
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// The TempPropertiesManager hold TempProperties when a player disconnect
    /// </summary>
    public abstract class TempPropertiesManager
    {
        public static ReaderWriterList<TempPropContainer> TempPropContainerList = new ReaderWriterList<TempPropContainer>();

        public class TempPropContainer
        {
            private string m_ownerid;
            public string OwnerID
            {
                get { return m_ownerid; }
            }

            private string m_tempropstring;
            public string TempPropString
            {
                get { return m_tempropstring; }
            }

            private string m_value;
            public string Value
            {
                get { return m_value; }
                set { m_value = value; }
            }
            public TempPropContainer(string ownerid, string tempropstring, string value)
            {
                m_ownerid = ownerid;
                m_tempropstring = tempropstring;
                m_value = value;
            }
        }
    }
}