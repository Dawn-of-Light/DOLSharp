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
	public class TripPath : Path
	{	
		#region Declaration
		
		/// <summary>
		/// The model to use when a stable master give a horse for the path
		/// </summary>
		protected string m_steedName;	
	
		/// <summary>
		/// Get / set the name to use when a stable master give a horse for the path
		/// </summary>
		public string SteedName
		{
			get { return m_steedName; }
			set { m_steedName = value; }
		}

		/// <summary>
		/// The model to use when a stable master give a horse for the path
		/// </summary>
		protected int m_steedModel;	
	
		/// <summary>
		/// Get / set the model to use when a stable master give a horse for the path
		/// </summary>
		public int SteedModel
		{
			get { return m_steedModel; }
			set { m_steedModel = value; }
		}
		#endregion
	}
}
