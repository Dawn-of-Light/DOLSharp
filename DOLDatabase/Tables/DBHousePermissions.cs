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
	/// Table that holds the different configurations for the permission levels of different houses.
	/// </summary>
	[DataTable(TableName = "DBHousePermissions")]
	public class DBHousePermissions : DataObject
	{
		//important data
		private bool _banish;
		private bool _bind;
		private bool _changeExternalAppearance;
		private bool _enter;
		private byte _garden;
		private int _housenumber;
		private byte _interior;
		private byte _merchant;
		private bool _payRent;
		private int _permLevel;
		private bool _tools;
		private bool _useMerchant;
		private byte _vault1;
		private byte _vault2;
		private byte _vault3;
		private byte _vault4;

		public DBHousePermissions()
		{
		}

		public DBHousePermissions(int n, int lvl)
		{
			HouseNumber = n;
			PermissionLevel = lvl;
		}

		/// <summary>
		/// Gets or sets level of the permission.
		/// </summary>
		/// <remarks>Since permission levels are hard-coded, this value should never be anything other than 0 thru 9.</remarks>
		[DataElement(AllowDbNull = false)]
		public int PermissionLevel
		{
			get { return _permLevel; }
			set
			{
				Dirty = true;
				_permLevel = value;
			}
		}

		/// <summary>
		/// Gets or sets the house number this permission is associated with.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int HouseNumber
		{
			get { return _housenumber; }
			set
			{
				Dirty = true;
				_housenumber = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can enter the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanEnterHouse
		{
			get { return _enter; }
			set
			{
				Dirty = true;
				_enter = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to the 1st or 5th vault in the home.
		/// </summary>
		/// <remarks>These flags contain view, add, and remove permissions for the vault.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte Vault1
		{
			get { return _vault1; }
			set
			{
				Dirty = true;
				_vault1 = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to the 2nd or 6th vault in the home.
		/// </summary>
		/// <remarks>These flags contain view, add, and remove permissions for the vault.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte Vault2
		{
			get { return _vault2; }
			set
			{
				Dirty = true;
				_vault2 = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to the 3rd or 7th vault in the home.
		/// </summary>
		/// <remarks>These flags contain view, add, and remove permissions for the vault.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte Vault3
		{
			get { return _vault3; }
			set
			{
				Dirty = true;
				_vault3 = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to the 4th or 8th vault in the home.
		/// </summary>
		/// <remarks>These flags contain view, add, and remove permissions for the vault.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte Vault4
		{
			get { return _vault4; }
			set
			{
				Dirty = true;
				_vault4 = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can change the external appearance of the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanChangeExternalAppearance
		{
			get { return _changeExternalAppearance; }
			set
			{
				Dirty = true;
				_changeExternalAppearance = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to changing the interior appearance.
		/// </summary>
		/// <remarks>These flags contain add and remove permissions for the interior appearance.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte ChangeInterior
		{
			get { return _interior; }
			set
			{
				Dirty = true;
				_interior = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to changing the garden layout.
		/// </summary>
		/// <remarks>These flags contain add and remove permissions for the garden layout.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte ChangeGarden
		{
			get { return _garden; }
			set
			{
				Dirty = true;
				_garden = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can banish others from the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanBanish
		{
			get { return _banish; }
			set
			{
				Dirty = true;
				_banish = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can use merchants in the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanUseMerchants
		{
			get { return _useMerchant; }
			set
			{
				Dirty = true;
				_useMerchant = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can the tools in the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanUseTools
		{
			get { return _tools; }
			set
			{
				Dirty = true;
				_tools = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can bind inside the home.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanBindInHouse
		{
			get { return _bind; }
			set
			{
				Dirty = true;
				_bind = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags related to the consignment merchant.
		/// </summary>
		/// <remarks>These flags contain add/remove and withdraw permissions for the consignment merchant.</remarks>
		[DataElement(AllowDbNull = false)]
		public byte ConsignmentMerchant
		{
			get { return _merchant; }
			set
			{
				Dirty = true;
				_merchant = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not players in this level can pay rent.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CanPayRent
		{
			get { return _payRent; }
			set
			{
				Dirty = true;
				_payRent = value;
			}
		}
	}
}