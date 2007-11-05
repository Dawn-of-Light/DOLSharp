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

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// Purpose of a property calculator is to serve
	/// as a formula plugin that calcs the correct property value
	/// ready for further calculations considering all bonuses/buffs 
	/// and possible caps on it
	/// it is a capsulation of the calculation logic behind each property
	/// 
	/// to reach that goal it makes use of the itembonus and buff category fields
	/// on the living that will be filled through equip actions and 
	/// buff/debuff effects
	/// 
	/// further it has access to all other calculators and properties
	/// on a living to fulfil its task
	/// </summary>
	public class PropertyCalculator : IPropertyCalculator
	{
		public PropertyCalculator()
		{
		}

		/// <summary>
		/// calculates the final property value
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual int CalcValue(GameLiving living, eProperty property) 
		{
			return 0;
		}

        /// <summary>
        /// Calculates the property value for this living's buff bonuses only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual int CalcValueFromBuffs(GameLiving living, eProperty property)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the property value for this living's item bonuses only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual int CalcValueFromItems(GameLiving living, eProperty property)
        {
            return 0;
        }
	}
}
