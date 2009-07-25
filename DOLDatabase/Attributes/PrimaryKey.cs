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

namespace DOL.Database.Attributes
{
	/// <summary>
	/// Attribute to indicate the PrimaryKey of an DatabaseObject .
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PrimaryKey : Attribute
	{
		private bool m_autoIncrement;
		/// <summary>
		/// Constructor for Attribute
		/// </summary>
		public PrimaryKey()
		{
			m_autoIncrement = false;
		}

		/// <summary>
		/// Indicates if this column will auto increment
		/// </summary>
		/// <value><c>true</c> if <c>null</c> is allowed</value>
		public bool AutoIncrement
		{
			get
			{
				return m_autoIncrement;
			}
			set
			{
				m_autoIncrement = value;
			}
		}


	}
}
