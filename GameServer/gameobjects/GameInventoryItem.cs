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
using System.Reflection;
using DOL.GS.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents an inventory item when it is
	/// laying on the floor in the world! It is just a wraper
	/// class around InventoryItem
	/// </summary>
	public class GameInventoryItem : GameStaticItemTimed
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The GenericItem that is contained within
		/// </summary>
		private GenericItem m_item;

		/// <summary>
		/// Constructs a GameInventoryItem based on an
		/// GenericItem. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="item">the InventoryItem to put into this class</param>
		public GameInventoryItem(GenericItem item) : base(180000)
		{
			m_item = item;
			m_item.SlotPosition = 0;
			m_item.Owner = null;
			m_Level = (byte)item.Level;
			m_Model = (ushort)item.Model;
			m_Name = item.Name;
		}

		/// <summary>
		/// Gets the GenericItem contained within this class
		/// </summary>
		public GenericItem Item
		{
			get { return m_item; }
		}
	}
}
