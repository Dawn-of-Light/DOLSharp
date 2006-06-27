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
	/// Summary description for a Shield
	/// </summary> 
	public class Shield : Weapon
	{
		#region Declaraction
		/// <summary>
		/// The size of the shield
		/// </summary>
		private eShieldSize m_size;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the size of the template
		/// </summary>
		public eShieldSize Size
		{
			get { return m_size; }
			set	{ m_size = value; }
		}

		#endregion

		/// <summary>
		/// Gets or sets the damage type of this weapon
		/// </summary>
		public override eDamageType DamageType 
		{
			get
			{
				// Scythe can make damage of type : 
				// - legendary weapon : No legendary
				// - classic weapon : Crush
				if(m_damageType != eDamageType.Crush) m_damageType = eDamageType.Crush;
				return m_damageType;
			}
			set
			{
				if (DamageType == eDamageType.Crush)
					m_damageType = DamageType;
					
			}
		}

		/// <summary>
		/// Gets how much hands are needed to use this weapon
		/// </summary>
		public override eHandNeeded HandNeeded 
		{
			// Shields can only be left hand
			get	{ return eHandNeeded.LeftHand; }
			set { m_handNeeded = eHandNeeded.LeftHand; }
		}

		/// <summary>
		/// Gets all inventory slots where the item can be equipped
		/// </summary>
		public override eInventorySlot[] EquipableSlot 
		{
			get  // Shield can only be euipped in lef hand slot
			{
				return new eInventorySlot[] {eInventorySlot.LeftHandWeapon};
			}
		}

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Shield; }
		}
	}
}	
