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
	/// Denotes a class as a property calculator. Must also implement IPropertyCalculator.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public class PropertyCalculatorAttribute : Attribute
	{
		/// <summary>
		/// Defines lowest property of calculator properties range
		/// </summary>
		private readonly eProperty m_min;
		/// <summary>
		/// Defines highest property of calculator properties range
		/// </summary>
		private readonly eProperty m_max;

		/// <summary>
		/// Gets the lowest property of calculator properties range
		/// </summary>
		public eProperty Min
		{
			get { return m_min; }
		}

		/// <summary>
		/// Gets the highest property of calculator properties range
		/// </summary>
		public eProperty Max
		{
			get { return m_max; }
		}

		/// <summary>
		/// Constructs a new calculator attribute for just one property
		/// </summary>
		/// <param name="prop">The property calculator is assigned to</param>
		public PropertyCalculatorAttribute(eProperty prop) : this(prop, prop)
		{
		}

		/// <summary>
		/// Constructs a new calculator attribute for range of properties
		/// </summary>
		/// <param name="min">The lowest property in range</param>
		/// <param name="max">The highest property in range</param>
		public PropertyCalculatorAttribute(eProperty min, eProperty max)
		{
			if (min > max)
				throw new ArgumentException("min property is higher than max (min=" + (int)min + " max=" + (int)max + ")");
			if (min < 0 || max > eProperty.MaxProperty)
				throw new ArgumentOutOfRangeException("max", (int)max, "property must be in 0 .. eProperty.MaxProperty range");
			m_min = min;
			m_max = max;
		}
	}
}
