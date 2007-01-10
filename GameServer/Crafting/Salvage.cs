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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The class holding all salvage functions
	/// </summary>
	public class Salvage
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaration

		/// <summary>
		/// The player currently crafting
		/// </summary>
		protected const string PLAYER_CRAFTER = "PLAYER_CRAFTER";

		/// <summary>
		/// The item to salvage
		/// </summary
		protected const string ITEM_CRAFTER = "ITEM_CRAFTER";

		/// <summary>
		/// The material to give
		/// </summary
		protected const string MATERIAL_CRAFTER = "MATERIAL_CRAFTER";

		/// <summary>
		/// The material to give
		/// </summary
		protected const string MATERIAL_COUNT_CRAFTER = "MATERIAL_COUNT_CRAFTER";

		#endregion

		#region First call function and callback

		/// <summary>
		/// Called when player try to use a secondary crafting skill
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, InventoryItem item)
		{
			if (!IsAllowedToBeginWork(player, item))
			{
				return 0;
			}

			int salvageLevel = CraftingMgr.GetItemCraftLevel(item) / 100;
			if(salvageLevel > 9) salvageLevel = 9; // max 9
			
			DBSalvage material = (DBSalvage) GameServer.Database.SelectObject(typeof(DBSalvage),"ObjectType ='"+item.Object_Type+"' AND SalvageLevel ='"+salvageLevel+"'");
			if (material == null || material.RawMaterial == null)
			{
				player.Out.SendMessage("Salvage material for object type ("+item.Object_Type+") not implemented yet.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}

			if (player.IsMoving || player.IsStrafing)
			{
				player.Out.SendMessage("You move and interrupt your salvage.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}
			
			player.Out.SendMessage("You begin salvaging the "+item.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			
			int count = CalculateMaterialCount(player, item, material);
			player.Out.SendTimerWindow("Salvaging: "+item.Name, count);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
			player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER, player);
			player.CraftTimer.Properties.setProperty(ITEM_CRAFTER, item);
			player.CraftTimer.Properties.setProperty(MATERIAL_CRAFTER, material);
			player.CraftTimer.Properties.setProperty(MATERIAL_COUNT_CRAFTER, count);
			
			player.CraftTimer.Start(count * 1000);
			return 1;
		}

		/// <summary>
		/// Called when player try to use a secondary crafting skill
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, GameSiegeWeapon siegeWeapon)
		{
			// Galenas
			siegeWeapon.ReleaseControl();
			siegeWeapon.RemoveFromWorld();
			bool error = false;
			DBCraftedItem craftItemData = (DBCraftedItem)GameServer.Database.SelectObject(typeof(DBCraftedItem), "Id_nb ='" + siegeWeapon.ItemId + "'");
			InventoryItem item;
			ItemTemplate template;
			foreach (DBCraftedXItem rawmaterial in craftItemData.RawMaterials)
			{
				template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), rawmaterial.IngredientId_nb);
				item = new InventoryItem(template);
				item.Count = rawmaterial.Count;
				if (!player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
				{
					error = true;
					break;
				}
			}
			if (error)
				player.Out.SendMessage("You don't have enough room in your inventory...", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}

		/// <summary>
		/// Called when craft time is finished 
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		protected static int Proceed(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty(PLAYER_CRAFTER, null);
			InventoryItem itemToSalvage = (InventoryItem)timer.Properties.getObjectProperty(ITEM_CRAFTER, null);
			DBSalvage material = (DBSalvage)timer.Properties.getObjectProperty(MATERIAL_CRAFTER, null);
			int materialCount = (int)timer.Properties.getObjectProperty(MATERIAL_COUNT_CRAFTER, 0);


			if (player == null || itemToSalvage == null || material == null || materialCount == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("There was a problem getting back the item to the player in the secondary craft system.");
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			player.Inventory.RemoveItem(itemToSalvage); // clean the free of the item to salvage
			
			Hashtable changedSlots = new Hashtable(5); // value: < 0 = new item count; > 0 = add to old
			lock(player.Inventory)
			{
				int count = materialCount;
				foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					if (item == null) continue;
					if (item.Id_nb != material.RawMaterial.Id_nb) continue;
					if (item.Count >= item.MaxCount) continue;

					int countFree = item.MaxCount - item.Count;
					if (count > countFree)
					{
						changedSlots.Add(item.SlotPosition, countFree); // existing item should be changed
						count -= countFree;
					}
					else
					{
						changedSlots.Add(item.SlotPosition, count); // existing item should be changed
						count = 0;
						break;
					}
				}

				if(count > 0) // Add new object
				{
					eInventorySlot firstEmptySlot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					changedSlots.Add((int)firstEmptySlot, -count); // Create the item in the free slot (always at least one)
					count = 0;
				}
			
			}

			InventoryItem newItem = null;

			player.Inventory.BeginChanges();
			foreach(DictionaryEntry de in changedSlots)
			{
				int countToAdd = (int) de.Value;
				if(countToAdd > 0)	// Add to exiting item
				{
					newItem = player.Inventory.GetItem((eInventorySlot)de.Key);
					player.Inventory.AddCountToStack(newItem, countToAdd);
				}
				else
				{
					newItem = new InventoryItem(material.RawMaterial);
					newItem.Count = -countToAdd;
					newItem.Weight *= -countToAdd;
					player.Inventory.AddItem((eInventorySlot)de.Key, newItem);
				}
			}
			player.Inventory.CommitChanges();

			player.Out.SendMessage("You get back " +materialCount+ " "+ material.RawMaterial.Name+ " after salvaging the " +itemToSalvage.Name+ ".",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
			
			return 0;
		}
		
		#endregion
			
		#region Requirement check

		/// <summary>
		/// Check if the player own can enchant the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="percentNeeded">min 50 max 100</param>
		/// <returns></returns>
		public static bool IsAllowedToBeginWork(GamePlayer player, InventoryItem item)
		{
			if(item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
			{
				player.Out.SendMessage("You can only salvage items in your backpack!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			eCraftingSkill skill = CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item);
			if(skill == eCraftingSkill.NoCrafting)
			{
				player.Out.SendMessage("You can't salvage "+item.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.IsCrafting)
			{
				player.Out.SendMessage("You must end your current action before you salvage anything!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.GetCraftingSkillValue(skill) < (0.75 * CraftingMgr.GetItemCraftLevel(item)))
			{
				player.Out.SendMessage("You don't have enough skill to salvage the "+item.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}
		
		#endregion

		#region Calcul functions

		/// <summary>
		/// Calculate the chance of sucess
		/// </summary>
		protected static int CalculateMaterialCount(GamePlayer player, InventoryItem item, DBSalvage material)
		{
			int maxCount = (int)Math.Floor(Money.GetMoney(0, 0, item.Gold, item.Silver ,item.Copper) * 0.45 / Money.GetMoney(0, 0, material.RawMaterial.Gold, material.RawMaterial.Silver ,material.RawMaterial.Copper)); // crafted item return max 45% of the item value in material
			if(item.CrafterName == null || item.CrafterName == "") maxCount = (int)Math.Ceiling((double)maxCount / 2); // merchand item return max the number of material of the same item if it was crafted crafted / 2 and Ceiling (it give max 30% of the base value)
	
			int playerPercent = player.GetCraftingSkillValue(CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item)) * 100 / CraftingMgr.GetItemCraftLevel(item);
			if(playerPercent > 100) playerPercent = 100;
			else if(playerPercent < 75) playerPercent = 75;

			int minCount = (int)(((maxCount - 1) / 25f) * playerPercent) - ((3 * maxCount) - 4); //75% => min = 1; 100% => min = maxCount;
			
			return Util.Random(minCount, maxCount);
		}

		#endregion
	}
}
