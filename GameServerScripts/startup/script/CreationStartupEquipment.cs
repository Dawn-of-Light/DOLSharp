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
using System.Collections.Generic;
using System.Linq;

using log4net;

using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.Database;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Give some Default Startup Equipment to newly created Character based on StarterEquipment Table.
	/// </summary>
	public static class CreationStartupEquipment
	{
		#region Properties
		/// <summary>
		/// Enable the Free Starter Equipment Gift.
		/// </summary>
		[ServerProperty("startup", "enable_free_starter_equipment", "Enable Startup Free Equipment gifts imported from StarterEquipment Table", true)]
		public static bool ENABLE_FREE_STARTER_EQUIPMENT;
		#endregion

		/// <summary>
		/// Declare a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Table Cache
		/// </summary>
		private static readonly Dictionary<eCharacterClass, List<ItemTemplate>> m_cachedClassEquipment = new Dictionary<eCharacterClass, List<ItemTemplate>>();
		
		/// <summary>
		/// Register Character Creation Events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			InitStarterEquipment();
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(OnCharacterCreation));
		}
		
		/// <summary>
		/// Init (Or Refresh) Starter Equipment Cache
		/// </summary>
		[RefreshCommand]
		public static void InitStarterEquipment()
		{
			m_cachedClassEquipment.Clear();
			
			// Init Startup Collection.
			foreach (var equipclass in GameServer.Database.SelectAllObjects<StarterEquipment>())
			{
				if (equipclass.Template != null)
				{
					foreach(var classID in equipclass.Class.SplitCSV(true))
					{
						int cId;
						if (int.TryParse(classID, out cId))
						{
							try
							{
								eCharacterClass gameClass = (eCharacterClass)cId;
								if (!m_cachedClassEquipment.ContainsKey(gameClass))
									m_cachedClassEquipment.Add(gameClass, new List<ItemTemplate>());
								
								m_cachedClassEquipment[gameClass].Add(equipclass.Template);
							}
							catch (Exception e)
							{
								if (log.IsWarnEnabled)
									log.WarnFormat("Could not Add Starter Equipement for Record - ID: {0}, ClassID(s): {1}, Itemtemplate: {2}, while parsing {3}\n{4}",
									               equipclass.StarterEquipmentID, equipclass.Class, equipclass.TemplateID, classID, e);
							}
						}
					}
				}
				else
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Cannot Find Item Template for Record - ID: {0}, ClassID(s): {1}, Itemtemplate: {2}", equipclass.StarterEquipmentID, equipclass.Class, equipclass.TemplateID);
				}
			}
		}
		
		/// <summary>
		/// Unregister Character Creation Events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(OnCharacterCreation));
		}
		
		/// <summary>
		/// On Character Creation set up equipment from StarterEquipment Table.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnCharacterCreation(DOLEvent e, object sender, EventArgs args)
		{
			if (!ENABLE_FREE_STARTER_EQUIPMENT)
				return;
			
			// Check Args
			var chArgs = args as CharacterEventArgs;
			
			if (chArgs == null)
				return;
			
			DOLCharacters ch = chArgs.Character;
			
			try
			{
				var usedSlots = new Dictionary<eInventorySlot, bool>();
				
				if (m_cachedClassEquipment.ContainsKey((eCharacterClass)ch.Class))
				{
					// sort for filling righ hand first...
					foreach (var item in m_cachedClassEquipment.Where(k => k.Key == 0 || k.Key == (eCharacterClass)ch.Class).SelectMany(kv => kv.Value).OrderBy(it => it.Item_Type))
					{
						// create Inventory item and set to owner.
						InventoryItem inventoryItem = GameInventoryItem.Create(item);
						inventoryItem.OwnerID = ch.ObjectId;
						inventoryItem.Realm = ch.Realm;
						
						bool itemChoosen = false;
		
						// if equipable item, find equippable slot
						foreach (eInventorySlot currentSlot in GameLivingInventory.EQUIP_SLOTS)
						{
							if ((eInventorySlot)inventoryItem.Item_Type == currentSlot)
							{
								eInventorySlot chosenSlot;
		
								// try to set Left Hand in Right Hand slot if not already used.
								if (currentSlot == eInventorySlot.LeftHandWeapon && (eObjectType)inventoryItem.Object_Type != eObjectType.Shield && !usedSlots.ContainsKey(eInventorySlot.RightHandWeapon))
								{
									chosenSlot = eInventorySlot.RightHandWeapon;
								}
								else
								{
									chosenSlot = currentSlot;
								}
		
								// Slot is occupied, add this to backpack.
								if (usedSlots.ContainsKey(chosenSlot))
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Cannot add Starter Equipment item {0} to class {1} an item is already assigned to this slot! (Added to Backpack...)", item.Id_nb, ch.Class);
									break;
								}
		
								inventoryItem.SlotPosition = (int)chosenSlot;
								usedSlots[chosenSlot] = true;
								if (ch.ActiveWeaponSlot == 0)
								{
									switch (inventoryItem.SlotPosition)
									{
										case Slot.RIGHTHAND:
											ch.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Standard;
											break;
										case Slot.TWOHAND:
											ch.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.TwoHanded;
											break;
										case Slot.RANGED:
											ch.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Distance;
											break;
									}
								}
								
								itemChoosen = true;
								break;
							}
						}
						
						if (!itemChoosen)
						{
							//otherwise stick the item in the backpack
							for (int i = (int)eInventorySlot.FirstBackpack; i < (int)eInventorySlot.LastBackpack; i++)
							{
								if (!usedSlots.ContainsKey((eInventorySlot)i))
								{
									inventoryItem.SlotPosition = i;
									usedSlots[(eInventorySlot)i] = true;
									break;
								}
							}
						}
						
						GameServer.Database.AddObject(inventoryItem);
					}
				}
				
			}
			catch (Exception err)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while adding Startup Equipment to {0} - Exception: {1}", ch.Name, err);
			}
		}
	}
}
