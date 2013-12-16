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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holfs the arguments for AreaEvents, this one can be used for either player, npcs or monster Enter/Leave
	/// </summary>
	public class AreaEventArgs : EventArgs
	{
		/// <summary>
		/// Area
		/// </summary>
		IArea m_area;

		/// <summary>
		///  Object either entering or leaving area
		/// </summary>
		GameObject m_object;

		public AreaEventArgs(IArea area, GameObject obj)
		{
			m_area = area;
			m_object = obj;
		}

		/// <summary>
		/// Gets the area
		/// </summary>
		public IArea Area
		{
			get {return m_area;}
		}


		/// <summary>
		/// Gets the gameobject
		/// </summary>
		public GameObject GameObject
		{
			get{return m_object;}
		}
	}
}
