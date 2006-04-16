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
using DOL.Events;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Movement
{
	/// <summary>
	/// This class represent a in game path
	/// </summary>
	public class Path
	{	
		#region Declaration
		
		/// <summary>
		/// The unique ID of this route
		/// </summary>
		protected int m_id;	
	
		/// <summary>
		/// Returns the unique ID of this path
		/// </summary>
		public int PathID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The region where this route is in
		/// </summary>
		protected Region m_region;	
	
		/// <summary>
		/// Returns the region where this route is in
		/// </summary>
		public Region Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		/// <summary>
		/// The starting point of this path
		/// </summary>
		protected PathPoint m_startingPoint;

		/// <summary>
		/// Gets or sets the starting point of this path
		/// </summary>
		public virtual PathPoint StartingPoint
		{
			get { return m_startingPoint; }
			set { m_startingPoint = value; }
		}

		#endregion

		#region NHibernate fix
		/// <summary>
		/// In the db we save the regionID but we must
		/// link the object with the region instance
		/// stored in the regionMgr
		///(it is only used internaly by NHibernate)
		/// </summary>
		/// <value>The region id</value>
		private int RegionID
		{
			get { return m_region.RegionID; }
			set { m_region = WorldMgr.GetRegion(value); }
		}
		#endregion
	}
}
