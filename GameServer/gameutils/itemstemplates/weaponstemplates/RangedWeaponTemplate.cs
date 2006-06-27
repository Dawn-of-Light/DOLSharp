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
	/// Summary description for a RangedWeaponTemplate
	/// </summary> 
	public abstract class RangedWeaponTemplate : WeaponTemplate
	{
		// - When delving ranged weapons, the range of the weapon is now displayed
		// http://www.camelotherald.com/more/2057.shtml

		#region Declaraction
		/// <summary>
		/// The attack range of this weapon template
		/// </summary>
		protected int m_range;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the range of this template
		/// </summary>
		public int Range
		{
			get { return m_range; }
			set	{ m_range = value; }
		}

		#endregion
	}
}
