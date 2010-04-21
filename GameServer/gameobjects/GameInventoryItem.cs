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
using DOL.GS.PacketHandler;
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
		/// that will disappear from the world after a certain amount of time
		/// </summary>
		public GameInventoryItem() : base(ServerProperties.Properties.WORLD_ITEM_DECAY_TIME)
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
			m_item.OwnerID = null;
			this.Level = (byte)item.Level;
			this.Model = (ushort)item.Model;
			this.Name = item.Name;
			m_item.Template = item.Template ;
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
			if (item.Template is ItemUnique)
				return null;
			
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
			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Item Creation: Template not found!\n"+Environment.StackTrace);
				return null;
			}

			return CreateFromTemplate(template);
		}

		/// <summary>
		/// Creates a new GII handling an UniqueItem in the InventoryItem attached
		/// </summary>
		/// <param name="templateID"></param>
		/// <returns></returns>
		public static GameInventoryItem CreateUniqueFromTemplate(string templateID)
		{
			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);

			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Item Creation: Template not found!\n" + Environment.StackTrace);
				return null;
			}
			
			return CreateUniqueFromTemplate(template);
		}
		
		/// <summary>
		/// Creates a new GameInventoryItem based on an ItemTemplate. Will disappear
		/// after 3 minutes after being added to the world.
		/// </summary>
		/// <param name="template">The template to load and create an item from.</param>
		/// <returns>Item reference or null.</returns>
		public static GameInventoryItem CreateFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			GameInventoryItem invItem = new GameInventoryItem();

			invItem.m_item = new InventoryItem(template);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Name = template.Name;

			return invItem;
		}
		public static GameInventoryItem CreateUniqueFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			GameInventoryItem invItem = new GameInventoryItem();
			ItemUnique item = new ItemUnique(template);
			GameServer.Database.AddObject(item);
			
			invItem.m_item = new InventoryItem(item);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Name = template.Name;

			return invItem;
		}

		#region PickUpTimer
		private RegionTimer m_pickup;
		/// <summary>
		/// Starts a new pickuptimer with the given time (in seconds)
		/// </summary>
		/// <param name="time"></param>
		public void StartPickupTimer(int time)
		{
			if (m_pickup != null)
			{
				m_pickup.Stop();
				m_pickup = null;
			}
			m_pickup = new RegionTimer(this, new RegionTimerCallback(CallBack), time * 1000);
		}
		private int CallBack(RegionTimer timer)
		{
			m_pickup.Stop();
			m_pickup = null;
			return 0;
		}
		public void StopPickupTimer()
		{
			foreach (GamePlayer player in Owners)
			{
				if (player.ObjectState == eObjectState.Active)
				{
					player.Out.SendMessage("You may now pick up " + Name + "!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
				}
			}
			m_pickup.Stop();
			m_pickup = null;
		}
		public int GetPickupTime
		{
			get
			{
				if (m_pickup == null)
					return 0;
				return m_pickup.TimeUntilElapsed;
			}
		}
		#endregion

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
