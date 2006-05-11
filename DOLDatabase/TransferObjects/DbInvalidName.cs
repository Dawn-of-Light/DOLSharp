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
using System.Collections;

namespace DOL.Database.TransferObjects
{
	/// <summary>
	/// Represent a player InvalidName
	/// </summary>
	public class DbInvalidName
	{	
		#region Declaraction
		/// <summary>
		/// The unique InvalidName identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// the invalid name
		/// </summary>
		private string m_name;

		/// <summary>
		/// Gets or sets the unique InvalidName identifier
		/// </summary>
		public int InvalidNameID
		{
			get	{ return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or sets the invalid name
		/// </summary>
		public string Name
		{
			get	{	return m_name; }
			set	{	m_name = value; }
		}

		#endregion
	}
}
