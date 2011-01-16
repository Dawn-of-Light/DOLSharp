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
	public class WorldInventoryItem : GameStaticItemTimed
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The InventoryItem that is contained within
		/// </summary>
		private InventoryItem m_item;

		/// <summary>
		/// Has this item been removed from the world?
		/// </summary>
		private bool m_isRemoved = false;


		/// <summary>
		/// Constructs an empty GameInventoryItem
		/// that will disappear from the world after a certain amount of time
		/// </summary>
		public WorldInventoryItem() : base(ServerProperties.Properties.WORLD_ITEM_DECAY_TIME)
		{
		}

		/// <summary>
		/// Constructs a GameInventoryItem based on an
		/// InventoryItem. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="item">the InventoryItem to put into this class</param>
		public WorldInventoryItem(InventoryItem item) : this()
		{
			m_item = item;
			m_item.SlotPosition = 0;
			m_item.OwnerID = null;
			this.Level = (byte)item.Level;
			this.Model = (ushort)item.Model;
			this.Emblem = item.Emblem;
			this.Name = item.Name;
			if (item.Template is ItemUnique)
			{
				ItemUnique unique = new ItemUnique(item.Template);
				GameServer.Database.AddObject(unique);
				m_item.Template = unique;
				m_item.UTemplate_Id = unique.Id_nb;
			}
			else
			{
				m_item.Template = item.Template;
			}

			m_item.AllowAdd = true;
		}

		/// <summary>
		/// Creates a new GameInventoryItem based on an
		/// InventoryTemplateID. Will disappear after 3 minutes if
		/// added to the world
		/// </summary>
		/// <param name="item">The InventoryItem to load and create an item from</param>
		/// <returns>Found item or null</returns>
		public static WorldInventoryItem CreateFromTemplate(InventoryItem item)
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
		public static WorldInventoryItem CreateFromTemplate(string templateID)
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
		public static WorldInventoryItem CreateUniqueFromTemplate(string templateID)
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
		public static WorldInventoryItem CreateFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			WorldInventoryItem invItem = new WorldInventoryItem();

			invItem.m_item = GameInventoryItem.Create<ItemTemplate>(template);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Emblem = template.Emblem;
			invItem.Name = template.Name;

			return invItem;
		}

		public static WorldInventoryItem CreateUniqueFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			WorldInventoryItem invItem = new WorldInventoryItem();
			ItemUnique item = new ItemUnique(template);
			GameServer.Database.AddObject(item);

			invItem.m_item = GameInventoryItem.Create<ItemUnique>(item);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Emblem = template.Emblem;
			invItem.Name = template.Name;

			return invItem;
		}

		public override bool RemoveFromWorld()
		{
			if (base.RemoveFromWorld())
			{
				if (m_item is IGameInventoryItem)
				{
					(m_item as IGameInventoryItem).OnRemoveFromWorld();
				}

				m_isRemoved = true;
				return true;
			}

			return false;
		}

		public override void Delete()
		{
			if (m_item != null && m_isRemoved == false && m_item.Template is ItemUnique)
			{
				// for world items that expire we need to delete the associated ItemUnique
				GameServer.Database.DeleteObject(m_item.Template as ItemUnique);
			}

			base.Delete();
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
