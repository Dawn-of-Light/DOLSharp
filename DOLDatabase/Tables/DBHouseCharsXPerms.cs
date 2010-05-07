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
	/// Table that holds the different characters and guilds that have been given permissions to a house.
	/// </summary>
	[DataTable(TableName = "DBHouseCharsXPerms")]
	public class DBHouseCharsXPerms : DataObject
	{
		//important data
		private int _houseNumber;
		private string _targetName;
		private string _displayName;
		private int _permissionLevel;
		private int _slot;
		private int _permissionType;

		public DBHouseCharsXPerms()
		{}

		public DBHouseCharsXPerms(string targetName, string displayName, int permissionLevel, int permissionType)
		{
			_targetName = targetName;
			_displayName = displayName;
			_permissionLevel = permissionLevel;
			_permissionType = permissionType;
		}

		/// <summary>
		/// Gets or sets the house number this permission is associated with.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int HouseNumber
		{
			get { return _houseNumber; }
			set
			{
				Dirty = true;
				_houseNumber = value;
			}
		}

		/// <summary>
		/// Gets or sets the type of permission for this mapping
		/// </summary>
		/// <remarks>Type includes things like character, account, guild, class, etc.</remarks>
		[DataElement(AllowDbNull = false)]
		public int PermissionType
		{
			get { return _permissionType; }
			set
			{
				Dirty = true;
				_permissionType = value;
			}
		}

		/// <summary>
		/// Gets or sets the target name of the character, account, guild, etc, that is tied to this mapping.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string TargetName
		{
			get { return _targetName; }
			set
			{
				Dirty = true;
				_targetName = value;
			}
		}

		/// <summary>
		/// Gets or sets the display name for the permission.
		/// </summary>
		/// <remarks>In cases of giving account-wide permissions to a player, this will be the character name
		/// at the time the permission was added, not the account name.</remarks>
		[DataElement(AllowDbNull = false)]
		public string DisplayName
		{
			get { return _displayName; }
			set
			{
				Dirty = true;
				_displayName = value;
			}
		}

		/// <summary>
		/// Gets or sets level of the permission.
		/// </summary>
		/// <remarks>Since permission levels are hard-coded, this value should never be anything other than 1 - 9.</remarks>
		[DataElement(AllowDbNull = false)]
		public int PermissionLevel
		{
			get { return _permissionLevel; }
			set
			{
				Dirty = true;
				_permissionLevel = value;
			}
		}

		/// <summary>
		/// Gets or sets the slot that this mapping is displayed in when the house owner sees the list of all mappings.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Slot
		{
			get { return _slot; }
			set
			{
				Dirty = true;
				_slot = value;
			}
		}
	}
}