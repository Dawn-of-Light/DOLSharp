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
using DOL.Database;
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
		/// The InventoryItem that is contained within
		/// </summary>
		private InventoryItem m_item;

		/// <summary>
		/// Constructs an empty GameInventoryItem
		/// that will disappear from the world after 3 minutes
		/// </summary>
		public GameInventoryItem() : base(180000)
		{
		}

		/// <summary>
		/// Constructs a GameInventoryItem based on an
		/// InventoryItem. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="item">the InventoryItem to put into this class</param>
		public GameInventoryItem(InventoryItem item) : this()
		{
			m_item = item;
			m_item.SlotPosition = 0;
			m_item.OwnerID = 0;
			this.Level = (byte)item.Level;
			this.Model = (ushort)item.Model;
			this.Name = item.Name;
			m_item.CopyFrom(item);
		}

		/// <summary>
		/// Creates a new GameInventoryItem based on an
		/// InventoryTemplateID. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="item">The InventoryItem to load and create an item from</param>
		/// <returns>Found item or null</returns>
		public static GameInventoryItem CreateFromTemplate(InventoryItem item)
		{
			return CreateFromTemplate(item.Id_nb);
		}

		/// <summary>
		/// Creates a new GameInventoryItem based on an
		/// InventoryTemplateID. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="templateID">the templateID to load and create an item</param>
		/// <returns>Found item or null</returns>
		public static GameInventoryItem CreateFromTemplate(string templateID)
		{
			ItemTemplate template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), templateID);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Item Creation: Template not found!\n"+Environment.StackTrace);
				return null;
			}

			GameInventoryItem invItem = new GameInventoryItem();

			invItem.m_item = new InventoryItem();
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = 0;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Name = template.Name;

			invItem.m_item.CopyFrom(template);

			return invItem;
		}

		/// <summary>
		/// Gets the InventoryItem contained within this class
		/// </summary>
		public InventoryItem Item
		{
			get
			{
				return m_item;
			}
		}
	}
}
