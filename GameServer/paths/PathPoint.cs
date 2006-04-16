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
	/// This class represent a pth point
	/// </summary>
	public class PathPoint
	{	
		#region Declaration
		
		/// <summary>
		/// The unique ID of this route point
		/// </summary>
		protected int m_id;	
	
		/// <summary>
		/// Returns the unique ID of this path point
		/// </summary>
		public int PathPointID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The position of this path point
		/// </summary>
		protected Point m_position;

		/// <summary>
		/// Gets or sets the position of this path point
		/// </summary>
		public virtual Point Position
		{
			get { return m_position; }
			set { m_position = value; }
		}
		
		/// <summary>
		/// The speed to use to go to this route point
		/// </summary>
		protected int m_speed;	
	
		/// <summary>
		/// Get or set the speed to use to go to this route point
		/// </summary>
		public int Speed
		{
			get { return m_speed; }
			set { m_speed = value; }
		}

		/// <summary>
		/// The next path point in the path
		/// </summary>
		protected PathPoint m_nextPoint;	
	
		/// <summary>
		/// Get or set the next path point in the path
		/// </summary>
		public PathPoint NextPoint
		{
			get { return m_nextPoint; }
			set { m_nextPoint = value; }
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
		#endregion
	}
}
