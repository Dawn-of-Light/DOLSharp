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
    /// Base Class for Custom Params Table
    /// Implementation for <see cref="ICustomParamsValuable"/>
    /// </summary>
    public abstract class CustomParam : DataObject
	{
		private string m_keyName;
		
		/// <summary>
		/// KeyName for referencing this value.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 100, Index = true)]
		public string KeyName {
			get { return m_keyName; }
			set { Dirty = true; m_keyName = value; }
		}
		
		private string m_value;
		
		/// <summary>
		/// Value, can be converted to numeric from string value.
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string Value {
			get { return m_value; }
			set { Dirty = true; m_value = value; }
		}
		
		private int m_CustomParamID;
		
		/// <summary>
		/// Primary Key Auto Inc
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public int CustomParamID {
			get { return m_CustomParamID; }
			set { Dirty = true; m_CustomParamID = value; }
		}

		/// <summary>
		/// Create new instance of <see cref="CustomParam"/> implementation
		/// Need to be linked with Foreign Key in subclass
		/// </summary>
		/// <param name="KeyName">Key Name</param>
		/// <param name="Value">Value</param>
		protected CustomParam(string KeyName, string Value)
		{
			this.KeyName = KeyName;
			this.Value = Value;
		}
		
		/// <summary>
		/// Default constructor for <see cref="CustomParam"/>
		/// </summary>
		protected CustomParam()
		{
		}
	}
}
