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
	public enum ePrecision : byte
	{
		Reduced		= 0,
		Normal		= 1,
		Improved	= 2,
		Enhanced	= 3,
	}

	public enum eDamageLevel : byte
	{
		Light		= 0,
		Medium		= 1,
		Heavy		= 2,
		XHeavy		= 3,
	}

	public enum eRange : byte
	{
		Short		= 0,
		Medium		= 1,
		Long		= 2,
		XLong		= 3,
	}

	/// <summary>
	/// Summary description for a AmmunitionTemplate
	/// </summary> 
	public abstract class AmmunitionTemplate : StackableItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The precision of the ammunition template
		/// </summary>
		protected ePrecision m_precision;

		/// <summary>
		/// The damage level of the ammunition template
		/// </summary>
		protected eDamageLevel m_damage;

		/// <summary>
		/// The range of the ammunition template
		/// </summary>
		protected eRange m_range;

		/// <summary>
		/// The damage type of the ammunition template
		/// </summary>
		protected eDamageType m_damageType;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the precision of the ammunition template
		/// </summary>
		public ePrecision Precision
		{
			get { return m_precision; }
			set	{ m_precision = value; }
		}

		/// <summary>
		/// Gets or sets the damage level of the ammunition template
		/// </summary>
		public eDamageLevel Damage
		{
			get { return m_damage; }
			set	{ m_damage = value; }
		}

		/// <summary>
		/// Gets or sets the range of the ammunition template
		/// </summary>
		public eRange Range
		{
			get { return m_range; }
			set	{ m_range = value; }
		}

		/// <summary>
		/// Gets or sets the damage type of the ammunition template
		/// </summary>
		public eDamageType DamageType
		{
			get { return m_damageType; }
			set	{ m_damageType = value; }
		}

		#endregion
	}
}	