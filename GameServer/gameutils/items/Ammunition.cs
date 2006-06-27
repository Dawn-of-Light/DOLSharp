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
	/// Summary description for a Ammunition
	/// </summary> 
	public abstract class Ammunition : StackableItem
	{
		#region Declaraction
		/// <summary>
		/// The precision of the ammunition
		/// </summary>
		private ePrecision m_precision;

		/// <summary>
		/// The damage level of the ammunition
		/// </summary>
		private eDamageLevel m_damage;

		/// <summary>
		/// The range of the ammunition
		/// </summary>
		private eRange m_range;

		/// <summary>
		/// The damage type of the ammunition
		/// </summary>
		private eDamageType m_damageType;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the precision of the ammunition
		/// </summary>
		public ePrecision Precision
		{
			get { return m_precision; }
			set	{ m_precision = value; }
		}

		/// <summary>
		/// Gets or sets the damage level of the ammunition
		/// </summary>
		public eDamageLevel Damage
		{
			get { return m_damage; }
			set	{ m_damage = value; }
		}

		/// <summary>
		/// Gets or sets the range of the ammunition
		/// </summary>
		public eRange Range
		{
			get { return m_range; }
			set	{ m_range = value; }
		}

		/// <summary>
		/// Gets or sets the damage type of the ammunition
		/// </summary>
		public eDamageType DamageType
		{
			get { return m_damageType; }
			set	{ m_damageType = value; }
		}

		#endregion

		#region Function
	    ///	 <summary>
        ///	Checks if the object can stack with the param  
        /// </summary>
        public override bool CanStackWith(StackableItem item)
        {
            Ammunition ammo = item as Ammunition;
            if (ammo == null) return false;
            return ( base.CanStackWith(item)
                && Precision == ammo.Precision
                && Damage == ammo.Damage
                && Range == ammo.Range
                && DamageType == ammo.DamageType);
        }
        
        
        /// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public override bool OnItemUsed(byte type)
		{
			if(! base.OnItemUsed(type)) return false;
			
			if(type == 0)
			{
				switch(SlotPosition)
				{
					case (int)eInventorySlot.FirstQuiver : Owner.SwitchQuiver(GameLiving.eActiveQuiverSlot.First, false);break;
					case (int)eInventorySlot.SecondQuiver : Owner.SwitchQuiver(GameLiving.eActiveQuiverSlot.Second, false);break;
					case (int)eInventorySlot.ThirdQuiver : Owner.SwitchQuiver(GameLiving.eActiveQuiverSlot.Third, false);break;
					case (int)eInventorySlot.FourthQuiver : Owner.SwitchQuiver(GameLiving.eActiveQuiverSlot.Fourth, false);break;
				}
			}

			return true;
		}

		#endregion
	}
}	