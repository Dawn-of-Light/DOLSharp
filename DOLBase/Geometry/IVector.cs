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

namespace DOL.Geometry
{
	public enum eVectorDefinition : byte
	{
		Coordinates	 = 0, // (X,Y,Z,Q)
		Goniometric	 = 1, // (size, Alpha, Beta, Gamma)   
	}

	
	public interface IVector
	{
		/// <summary>
		/// X coordinate
		/// </summary>
		int XInt
		{
			get;
			set;
		}

		/// <summary>
		/// Y coordinate
		/// </summary>
		int YInt
		{
			get;
			set;
		}

		/// <summary>
		/// X coordinate (as float)
		/// </summary>
		double X
		{
			get;
			set;
		}

		/// <summary>
		/// Y coordinate (as float)
		/// </summary>
		double Y
		{
			get;
			set;
		}

		/// <summary>
		/// vector size
		/// </summary>
		double Size
		{
			get;
			set;
		}

		/// <summary>
		/// vector yaw angle
		/// </summary>
		double Yaw
		{
			get;
			set;
		}
	
	}
}
