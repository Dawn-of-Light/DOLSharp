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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	public class GameNpcInventoryTemplate : GameLivingInventory
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds inventory item instances already used in inventory templates
		/// </summary>
		protected static readonly Hashtable m_usedInventoryItems = new Hashtable(1024);

		/// <summary>
		/// Holds already used inventory template instances
		/// </summary>
		protected static readonly Hashtable m_usedInventoryTemplates = new Hashtable(256);

		/// <summary>
		/// Holds an empty invenotory template instance
		/// </summary>
		public static readonly GameNpcInventoryTemplate EmptyTemplate;

		/// <summary>
		/// Static constructor
		/// </summary>
		static GameNpcInventoryTemplate()
		{
			GameNpcInventoryTemplate temp = new GameNpcInventoryTemplate().CloseTemplate();
			Thread.MemoryBarrier();
			EmptyTemplate = temp;
		}

		/// <summary>
		/// Holds the closed flag, if true template cannot be modified
		/// </summary>
		protected bool m_isClosed;

		/// <summary>
		/// Gets the closed flag
		/// </summary>
		public bool IsClosed
		{
			get { return m_isClosed; }
		}

		/// <summary>
		/// Check if the slot is valid in the inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected override eInventorySlot GetValidInventorySlot(eInventorySlot slot)
		{
			foreach (eInventorySlot visibleSlot in VISIBLE_SLOTS)
				if (visibleSlot == slot)
					return slot;
			return eInventorySlot.Invalid;
		}

		#region AddNPCEquipment/RemoveNPCEquipment/CloseTemplate/CloneTemplate

		/// <summary>
		/// Adds item to template reusing inventory item instances from other templates.
		/// </summary>
		/// <param name="slot">The equipment slot</param>
		/// <param name="model">The equipment model</param>
		/// <returns>true if added</returns>
		public bool AddNPCEquipment(eInventorySlot slot, int model)
		{
			return AddNPCEquipment(slot, model, 0, 0, 0);
		}

		/// <summary>
		/// Adds item to template reusing inventory item instances from other templates.
		/// </summary>
		/// <param name="slot">The equipment slot</param>
		/// <param name="model">The equipment model</param>
		/// <param name="color">The equipment color</param>
		/// <returns>true if added</returns>
		public bool AddNPCEquipment(eInventorySlot slot, int model, int color)
		{
			return AddNPCEquipment(slot, model, color, 0, 0);
		}

		/// <summary>
		/// Adds item to template reusing inventory item instances from other templates.
		/// </summary>
		/// <param name="slot">The equipment slot</param>
		/// <param name="model">The equipment model</param>
		/// <param name="color">The equipment color</param>
		/// <param name="effect">The equipment effect</param>
		/// <returns>true if added</returns>
		public bool AddNPCEquipment(eInventorySlot slot, int model, int color, int effect)
		{
			return AddNPCEquipment(slot, model, color, effect, 0);
		}

		/// <summary>
		/// Adds item to template reusing iventory  item instances from other templates.
		/// </summary>
		/// <param name="slot">The equipment slot</param>
		/// <param name="model">The equipment model</param>
		/// <param name="color">The equipment color</param>
		/// <param name="effect">The equipment effect</param>
		/// <param name="extension">The equipment extension</param>
		/// <returns>true if added</returns>
		public bool AddNPCEquipment(eInventorySlot slot, int model, int color, int effect, int extension)
		{
			lock (m_items)
			{
				lock (m_usedInventoryItems.SyncRoot)
				{
					if (m_isClosed)
						return false;
					slot = GetValidInventorySlot(slot);
					if (slot == eInventorySlot.Invalid)
						return false;
					//Changed to support randomization of slots - if we try to load a weapon in the same spot with a different model,
					//let's make it random 50% chance to either overwrite the item or leave it be
					if (m_items.ContainsKey(slot))
					{
						//50% chance to keep the item we have
						if (Util.Chance(50))
							return false;
						//Let's remove the old item!
						m_items.Remove(slot);
					}
					string itemID = string.Format("{0}:{1},{2},{3}", slot, model, color, effect, extension);
					InventoryItem item = null;

					if (!m_usedInventoryItems.ContainsKey(itemID))
					{
							item = new InventoryItem();
							item.Id_nb = itemID;
							item.Model = model;
							item.Color = color;
							item.Effect = effect;
							item.Extension = (byte)extension;
							item.SlotPosition = (int)slot;
					}
					else
						return false;

					m_items.Add(slot, item);
				}
			}
			return true;
		}

		/// <summary>
		/// Removes item from slot if template is not closed.
		/// </summary>
		/// <param name="slot">The slot to remove</param>
		/// <returns>true if removed</returns>
		public bool RemoveNPCEquipment(eInventorySlot slot)
		{
			lock (m_items)
			{
				slot = GetValidInventorySlot(slot);

				if (slot == eInventorySlot.Invalid) 
					return false;

				if (m_isClosed) 
					return false;

				if (!m_items.ContainsKey(slot))
					return false;

				m_items.Remove(slot);

				return true;
			}
		}

		/// <summary>
		/// Closes this template and searches for other identical templates.
		/// Template cannot be modified after it was closed, clone it instead.
		/// </summary>
		/// <returns>Invetory template instance that should be used</returns>
		public GameNpcInventoryTemplate CloseTemplate()
		{
			lock (m_items)
			{
				lock (m_usedInventoryTemplates.SyncRoot)
				{
					lock (m_usedInventoryItems.SyncRoot)
					{
						m_isClosed = true;
						StringBuilder templateID = new StringBuilder(m_items.Count * 16);
						foreach (InventoryItem item in new SortedList(m_items).Values)
						{
							if (templateID.Length > 0)
								templateID.Append(";");
							templateID.Append(item.Id_nb);
						}

						GameNpcInventoryTemplate finalTemplate = m_usedInventoryTemplates[templateID.ToString()] as GameNpcInventoryTemplate;
						if (finalTemplate == null)
						{
							finalTemplate = this;
							m_usedInventoryTemplates[templateID.ToString()] = this;
							foreach (var de in m_items)
							{
								if (!m_usedInventoryItems.Contains(de.Key))
									m_usedInventoryItems.Add(de.Key, de.Value);
							}
						}

						return finalTemplate;
					}
				}
			}
		}

		/// <summary>
		/// Creates a shallow copy of the GameNpcInventoryTemplate.
		/// </summary>
		/// <returns>Open copy of this template</returns>
		public GameNpcInventoryTemplate CloneTemplate()
		{
			lock (m_items)
			{
				var clone = new GameNpcInventoryTemplate();
				clone.m_changedSlots = new List<eInventorySlot>(m_changedSlots);
				clone.m_changesCounter = m_changesCounter;

				foreach (var de in m_items)
					clone.m_items.Add(de.Key, de.Value);

				clone.m_isClosed = false;

				return clone;
			}
		}

		#endregion

		#region LoadFromDatabase/SaveIntoDatabase

		/// <summary>
		/// Cache for fast loading of npc equipment
		/// </summary>
		protected static Dictionary<string, List<NPCEquipment>> m_npcEquipmentCache = null;

		/// <summary>
		/// Loads the inventory template from the Database
		/// </summary>
		/// <returns>success</returns>
		public override bool LoadFromDatabase(string templateID)
		{
			if (Util.IsEmpty(templateID) || templateID == "\r\n" || templateID == "0")
			{
				//if (log.IsWarnEnabled)
					//log.Warn("Null or empty string template reference");

				return false;
			}

			lock (m_items)
			{
				if (m_npcEquipmentCache.ContainsKey(templateID))
				{
					List<NPCEquipment> npcEquip = m_npcEquipmentCache[templateID];

					foreach (NPCEquipment npcItem in npcEquip)
					{
						if (!AddNPCEquipment((eInventorySlot)npcItem.Slot, npcItem.Model, npcItem.Color, npcItem.Effect, npcItem.Extension))
						{
							if (log.IsWarnEnabled)
								log.Warn("Error adding NPC equipment, ObjectId=" + npcItem.ObjectId);
						}
					}
					return true;
				}
			}

			if (log.IsWarnEnabled)
				log.Warn(string.Format("Failed loading NPC inventory template: {0}", templateID));
			return false;
		}

		/// <summary>
		/// Create the hash table
		/// </summary>
		public static bool Init()
		{
			try
			{
				m_npcEquipmentCache = new Dictionary<string, List<NPCEquipment>>(1000);
				foreach (NPCEquipment equip in GameServer.Database.SelectAllObjects<NPCEquipment>())
				{
					List<NPCEquipment> list;
					if (m_npcEquipmentCache.ContainsKey(equip.TemplateID))
					{
						list = m_npcEquipmentCache[equip.TemplateID];
					}
					else
					{
						list = new List<NPCEquipment>();
						m_npcEquipmentCache[equip.TemplateID] = list;
					}

					list.Add(equip);
				}
				return true;
			}
			catch (Exception e)
			{
				log.Error(e);
			}
			return false;
		}

		/// <summary>
		/// Save the inventory template to Database
		/// </summary>
		/// <returns>success</returns>
		public override bool SaveIntoDatabase(string templateID)
		{
			lock (m_items)
			{
				try
				{
					if (templateID == null)
						throw new ArgumentNullException("templateID");

					var npcEquipment = GameServer.Database.SelectObjects<NPCEquipment>("TemplateID = '" + GameServer.Database.Escape(templateID) + "'");

					// delete removed item templates
					foreach (NPCEquipment npcItem in npcEquipment)
					{
						if (!m_items.ContainsKey((eInventorySlot)npcItem.Slot))
							GameServer.Database.DeleteObject(npcItem);
					}

					// save changed item templates
					foreach (InventoryItem item in m_items.Values)
					{
						bool foundInDB = false;
						foreach (NPCEquipment npcItem in npcEquipment)
						{
							if (item.SlotPosition != npcItem.Slot) 
								continue;

							if (item.Model != npcItem.Model || item.Color != npcItem.Color || item.Effect != npcItem.Effect)
							{
								npcItem.Model = item.Model;
								npcItem.Color = item.Color;
								npcItem.Effect = item.Effect;
								npcItem.Extension = item.Extension;
								GameServer.Database.SaveObject(npcItem);
							}

							foundInDB = true;

							break;
						}

						if (!foundInDB)
						{
							NPCEquipment npcItem = new NPCEquipment();
							npcItem.Slot = item.SlotPosition;
							npcItem.Model = item.Model;
							npcItem.Color = item.Color;
							npcItem.Effect = item.Effect;
							npcItem.TemplateID = templateID;
							npcItem.Extension = item.Extension;
							GameServer.Database.AddObject(npcItem);
						}
					}

					return true;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error saving NPC inventory template, templateID=" + templateID, e);

					return false;
				}
			}
		}

		#endregion

		#region methods not allowed in inventory template

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <returns>false</returns>
		public override bool AddItem(eInventorySlot slot, InventoryItem item)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>false</returns>
		public override bool RemoveItem(InventoryItem item)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="count"></param>
		/// <returns>false</returns>
		public override bool AddCountToStack(InventoryItem item, int count)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <param name="count">the count of items to be removed from the stack</param>
		/// <returns>false</returns>
		public override bool RemoveCountFromStack(InventoryItem item, int count)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <param name="itemCount"></param>
		/// <returns></returns>
		public override bool MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="fromItem">First Item</param>
		/// <param name="toItem">Second Item</param>
		/// <returns>false</returns>
		protected override bool CombineItems(InventoryItem fromItem, InventoryItem toItem)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <param name="itemCount">How many items to move</param>
		/// <returns>false</returns>
		protected override bool StackItems(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <returns>false</returns>
		protected override bool ExchangeItems(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="template">The ItemTemplate</param>
		/// <param name="count">The count of items to add</param>
		/// <param name="minSlot">The first slot</param>
		/// <param name="maxSlot">The last slot</param>
		/// <returns>false</returns>
		public override bool AddTemplate(ItemTemplate template, int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			return false;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="templateID">The ItemTemplate ID</param>
		/// <param name="count">The count of items to add</param>
		/// <param name="minSlot">The first slot</param>
		/// <param name="maxSlot">The last slot</param>
		/// <returns>false</returns>
		public override bool RemoveTemplate(string templateID, int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			return false;
		}

		#endregion
	}
}
