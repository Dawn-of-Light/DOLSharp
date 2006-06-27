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

namespace DOL.Database.Attributes
{
	/// <summary>
	/// Attribute to indicate an Relationship to another DatabaseObject.
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Relation : Attribute
	{
		private string localField;
		private string remoteField;
		private bool autoLoad;
		private bool autoDelete;

		/// <summary>
		/// Constructor for the Relation-Attribute.
		/// Standartsettings are:
		///		AutoLoad = true
		///		AutoDelete = false
		/// </summary>
		public Relation()
		{
			localField = null;
			remoteField = null;
			autoLoad=true;
			autoDelete = false;
		}

		/// <summary>
		/// Property to set/get the Name of the LocalField for the Relation
		/// </summary>
		/// <value>Name of the Local Field</value>
		public string LocalField
		{
			get
			{
				return localField;
			}
			set
			{
				localField = value;
			}
		}

		/// <summary>
		/// Property to set/get the RemoteField of the Relation
		/// </summary>
		/// <value>Name of the Remote Field</value>
		public string RemoteField
		{
			get
			{
				return remoteField;
			}
			set
			{
				remoteField = value;
			}
		}

		/// <summary>
		/// Property to set/get Autoload
		/// If Autoload is true the Releation is filled on Object load/reload
		/// If false you have to fill the Relation with Database.FillObjectRelations(DataObject)
		/// </summary>
		/// <value><c>true</c> if Relation sould be filled automatical</value>
		public bool AutoLoad
		{
			get
			{
				return autoLoad;
			}
			set
			{
				autoLoad = value;
			}
		}

		/// <summary>
		/// AutoDelete-Property to set/get AutoDelete
		/// If set to true, all related Objects are deleted from Database when the Object is deleted
		/// If set to false, the related Objects are NOT deleted.
		/// </summary>
		/// <value><c>true</c> if related objects are deleted as well</value>
		public bool AutoDelete
		{
			get
			{
				return autoDelete;
			}
			set
			{
				autoDelete = value;
			}
		}

	}
}
