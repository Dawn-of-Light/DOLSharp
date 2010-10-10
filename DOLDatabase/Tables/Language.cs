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
	/// The language table
	/// </summary>
	[DataTable(TableName = "Language")]
	public class DBLanguage : DataObject
	{
		protected string m_translationid; // the global translation string
		protected string m_EN = ""; // EN native translation of global translation string
		protected string m_DE = ""; // DE translation of global translation string
		protected string m_FR = ""; // FR translation of global translation string
		protected string m_IT = ""; // IT translation of global translation string
		protected string m_CU = ""; // Custom language translation of global translation string
		protected string m_packageID;

		/// <summary>
		/// Create language
		/// </summary>
		public DBLanguage()
		{
		}

		/// <summary>
		/// The translation key
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true)]
		public string TranslationID
		{
			get { return m_translationid; }
			set { Dirty = true; m_translationid = value; }
		}

		/// <summary>
		/// English
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string EN
		{
			get { return m_EN; }
			set { Dirty = true; m_EN = value; }
		}

		/// <summary>
		/// German
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string DE
		{
			get { return m_DE; }
			set { Dirty = true; m_DE = value; }
		}

		/// <summary>
		/// French
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string FR
		{
			get { return m_FR; }
			set { Dirty = true; m_FR = value; }
		}

		/// <summary>
		/// Italian
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string IT
		{
			get { return m_IT; }
			set { Dirty = true; m_IT = value; }
		}

		/// <summary>
		/// Custom language (other)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string CU
		{
			get { return m_CU; }
			set { Dirty = true; m_CU = value; }
		}
		
		/// <summary>
		/// Package ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string PackageID
		{
			get { return m_packageID; }
			set { Dirty = true; m_packageID = value; }
		}
	}
}