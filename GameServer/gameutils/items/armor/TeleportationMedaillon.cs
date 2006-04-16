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
	/// Summary description for a TeleportationMedaillon
	/// </summary> 
	public class TeleportationMedaillon : Necklace
	{	
		#region Declaraction
		/// <summary>
		/// The x coohor of the teleportation medaillon
		/// </summary>
		private int m_x;

		/// <summary>
		/// The y coohor of the teleportation medaillon
		/// </summary>
		private int m_y;

		/// <summary>
		/// The z coohor of the teleportation medaillon
		/// </summary>
		private int m_z;

		/// <summary>
		/// The heading of the teleportation medaillon
		/// </summary>
		private int m_heading;

		/// <summary>
		/// The region of the teleportation medaillon
		/// </summary>
		private int m_region;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the x coohor of the teleportation medaillon
		/// </summary>
		public int X
		{
			get { return m_x; }
			set	{ m_x = value; }
		}

		/// <summary>
		/// Gets or sets the y coohor of the teleportation medaillon
		/// </summary>
		public int Y
		{
			get { return m_y; }
			set	{ m_y = value; }
		}

		/// <summary>
		/// Gets or sets the z coohor of the teleportation medaillon
		/// </summary>
		public int Z
		{
			get { return m_z; }
			set	{ m_z = value; }
		}

		/// <summary>
		/// Gets or sets the heading of the teleportation medaillon
		/// </summary>
		public int Heading
		{
			get { return m_heading; }
			set	{ m_heading = value; }
		}

		/// <summary>
		/// Gets or sets the region of the teleportation medaillon
		/// </summary>
		public int Region
		{
			get { return m_region; }
			set	{ m_region = value; }
		}

		#endregion
	}
}	