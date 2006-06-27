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
using System.ComponentModel;
using DOL.Database.Attributes;
using DOL.Database.UniqueID;

namespace DOL.Database
{
	/// <summary>
	/// Abstract Baseclass for all DataObject's. All Classes that are derived from this class
	/// are stored in a Datastore
	/// </summary>
	public abstract class DataObject : ICloneable
	{
		private string objectId;

		private bool dirty;

		private bool valid;

		/// <summary>
		/// Default-Construktor that generates a new Object-ID and set
		/// Dirty and Valid to <c>false</c>
		/// </summary>
		public DataObject()
		{
			objectId = IdGenerator.generateId();
			dirty = false;
			valid = false;
		}

		/// <summary>
		/// Returns the Tablename for an Objecttype. 
		/// Reads the DataTable-Attribute or if
		/// not defined returns the Classname
		/// </summary>
		/// <param name="myType">get the Tablename for this DataObject</param>
		/// <returns>The </returns>
		public static string GetTableName(Type myType)
		{
			object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);

			if((attri.Length >= 1) && (attri[0] is DataTable))
			{
				DataTable tab = attri[0] as DataTable;					
				string name = tab.TableName;
				if(name != null)
					return name;
			}

			return myType.Name;
		}

		/// <summary>
		/// Returns the Tablename for an Objecttype. 
		/// Reads the DataTable-Attribute or if
		/// not defined returns the Classname
		/// </summary>
		/// <param name="myType">get the Tablename for this DataObject</param>
		/// <returns>The </returns>
		public static bool GetPreCachedFlag(Type myType)
		{
			object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);
			if((attri.Length >= 1) && (attri[0] is DataTable))
			{
				DataTable tab = attri[0] as DataTable;					
				return tab.PreCache;
			}
			return false;
		}

		/// <summary>
		/// The table name which own he object 
		/// </summary>
		[Browsable(false)]
		public virtual string TableName
		{
			get
			{
				Type myType = this.GetType();
				return DataObject.GetTableName(myType);
			}
		}

		/// <summary>
		/// Load object in cache or not?
		/// </summary>
		[Browsable(false)]
		public virtual bool UsesPreCaching
		{
			get
			{
				Type myType = this.GetType();
				return DataObject.GetPreCachedFlag(myType);
			}
		}

		/// <summary>
		/// Is object valid
		/// </summary>
		[Browsable(false)]
		public bool IsValid
		{
			get
			{
				return valid;
			}
			set
			{
				valid = value;
			}
		}

		/// <summary>
		/// Auto save object or not?
		/// </summary>
		[Browsable(false)]
		abstract public bool AutoSave
		{
			get;
			set;
		}

		/// <summary>
		/// Index of the object in his table
		/// </summary>
		[Browsable(false)]
		public string ObjectId
		{
			get
			{
				return objectId;
			}
			set
			{
				objectId = value;
			}
		}

		/// <summary>
		/// Is object different of object in DB
		/// </summary>
		[Browsable(false)]
		public bool Dirty
		{
			set
			{
				dirty = value;
			}
			get
			{
				return dirty;
			}
		}
		#region ICloneable Member

		/// <summary>
		/// Clone the current object and return the copy
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			DataObject obj = (DataObject)MemberwiseClone();
			obj.ObjectId = IdGenerator.generateId();
			return obj;
		}

		#endregion
	}
}
