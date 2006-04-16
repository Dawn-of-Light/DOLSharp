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
using System.Reflection;
using System.Text;
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY persistant game object in the game world needs!
	/// </summary>
	public abstract class PersistantGameObject : GameObject
	{
		#region Declaration

		/// <summary>
		/// The unique game object identifier (db unique id)
		/// </summary>
		private int m_id;

		/// <summary>
		/// Gets or sets the unique game object identifier
		/// </summary>
		public int PersistantGameObjectID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		#endregion

		#region NHibernate fix

		/// <summary>
		/// Gets or sets the DB position.
		/// NHibernate does not work with struct so we 
		/// use a warper class to save and load the Point struct ...
		/// </summary>
		/// <value>The DB position.</value>
		private DBPoint DBPosition
		{
			get { return new DBPoint(m_position); }
			set { m_position = new Point(value); }
		}

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

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" dbID=").Append(PersistantGameObjectID)
				.ToString();
		}
	}
}