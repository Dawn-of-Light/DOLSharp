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
	/// Summary description for a NPCEquipment
	/// </summary> 
	public class NPCEquipment : GenericItemBase
	{
		#region Constructor
		/// <summary>
		/// No args constructor
		/// </summary>
		public NPCEquipment() 
		{
		}

		/// <summary>
		/// Construtor to use in scripts
		/// </summary>
		public NPCEquipment(int model) 
		{
			Model = model;
			Color = 0;
		}

		/// <summary>
		/// Construtor to use in scripts
		/// </summary>
		public NPCEquipment(int model, int color) 
		{
			Model = model;
			Color = color;
		}

		#endregion

		#region Declaraction
		/// <summary>
		/// The color/item of this item
		/// </summary>
		private int m_color;

		/// <summary>
		/// Gets or sets the color/emblem of this item
		/// </summary>
		public int Color
		{
			get { return m_color; }
			set	{ m_color = value; }
		}

		#endregion
	}
}
