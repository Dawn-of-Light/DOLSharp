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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a Instrument
	/// </summary> 
	public class Instrument : VisibleEquipment
	{
		#region Declaraction
		/// <summary>
		/// The type of instrument
		/// </summary>
		private eInstrumentType m_type;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the type of this instrument
		/// </summary>
		public eInstrumentType Type
		{
			get { return m_type; }
			set	{ m_type = value; }
		}

		#endregion

		/// <summary>
		/// Gets all inventory slots where the item can be equipped
		/// </summary>
		public override eInventorySlot[] EquipableSlot
		{
			get
			{
				return new eInventorySlot[]{ eInventorySlot.TwoHandWeapon, eInventorySlot.DistanceWeapon };
			}
		}

		/// <summary>
		/// Gets the object type of the instrument (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Instrument; }
		}
	}
}	