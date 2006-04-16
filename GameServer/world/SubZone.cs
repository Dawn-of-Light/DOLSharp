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
using System.Threading;

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de SubZone.
	/// </summary>
	public class SubZone
	{
		#region Declarations

		/// <summary>
		/// This is the doubly linked list of elements
		/// </summary>
		GeometryEngineNode m_elementsList = null;

		/// <summary>
		/// This is the ReaderWriterLock used to allow multiple read and single write
		/// </summary>
		ReaderWriterLock lockObject = new ReaderWriterLock();

		/// <summary>
		/// Returns the first element of the list
		/// </summary>
		public GeometryEngineNode ElementsList
		{
			get { return m_elementsList; }
			set { m_elementsList = value; }
		}

		/// <summary>
		/// Returns the object used to locked the subZone
		/// </summary>
		public ReaderWriterLock LockObject
		{
			get { return lockObject; }
		}

		#endregion
	}
}
