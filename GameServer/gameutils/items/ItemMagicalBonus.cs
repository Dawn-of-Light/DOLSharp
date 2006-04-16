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
	/// Summary description for a ItemMagicalBonus
	/// </summary> 
	public class ItemMagicalBonus
	{
		/// <summary>
		/// Empty Constructor (used internally by NHibernate)
		/// </summary>
		private ItemMagicalBonus()
		{
			
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ItemMagicalBonus(eProperty property, Int16 value)
		{
			m_bonusType = property;
			m_bonus = value;
		}

		#region Declaraction
		
		/// <summary>
		/// The bonus type of the bonus
		/// </summary>
		private eProperty m_bonusType;

		/// <summary>
		/// The amoung of point of this bonus
		/// </summary>
		private Int16 m_bonus;

		#endregion

		#region Get and Set

		/// <summary>
		/// Gets or sets the bonus type of this bonus
		/// </summary>
		public eProperty BonusType
		{
			get { return m_bonusType; }
			set	{ m_bonusType = value; }
		}

		/// <summary>
		/// Gets or sets the amoung of point of this bonus
		/// </summary>
		public Int16 Bonus
		{
			get { return m_bonus; }
			set	{ m_bonus = value; }
		}

		#endregion
	}
}
