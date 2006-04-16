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

namespace DOL.GS
{
	/// <summary>
	/// Raw materials for craft item
	/// </summary>
	public class RawMaterial
	{
		#region Declaration
		/// <summary>
		/// The raw material tamplate
		/// </summary>
		private GenericItemTemplate m_materialTemplate;
		
		/// <summary>
		/// How much raw matarial are needed
		/// </summary>
		private byte	m_countNeeded;
		
		#endregion
		
		#region All get and set
		/// <summary>
		/// Get or set the material item template
		/// </summary>
		public GenericItemTemplate MaterialTemplate
		{
			get { return m_materialTemplate; }
			set { m_materialTemplate = value; }
		}

		/// <summary>
		/// Get or set how much raw matarial are needed
		/// </summary>
		public byte CountNeeded
		{
			get { return m_countNeeded; }
			set { m_countNeeded = value; }
		}

		#endregion
	}
}
