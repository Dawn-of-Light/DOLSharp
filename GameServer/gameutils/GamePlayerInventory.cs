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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a full player inventory
	/// and contains functions that can be used to
	/// add and remove items from the player
	/// </summary>
	public class GamePlayerInventory : GameLivingInventory
	{
		#region Constructor/Declaration/LoadDatabase/SaveDatabase

		/// <summary>
		/// Holds the player that owns
		/// this inventory
		/// </summary>
		protected readonly GamePlayer m_player;

		/// <summary>
		/// Constructs a new empty inventory for player
		/// </summary>
		/// <param name="player">GamePlayer to create the inventory for</param>
		public GamePlayerInventory(GamePlayer player)
		{
			m_player = player;
		}
		/// <summary>
		/// Loads the inventory from the DataBase
		/// </summary>
		/// <param name="inventoryID">The inventory ID</param>
		/// <returns>success</returns>
		public override bool LoadFromDatabase(string inventoryID)
		{
			//ArtifactManager.LoadArtifacts(); //loading artifacts
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				try
				{
					// We only want to cache items in the players personal inventory and personal vault.
					// If we cache ALL items them all vault code must make sure to update cache, which is not ideal
					// in addition, a player with a housing vault may still have an item in cache that may have been
					// removed by another player with the appropriate house permission.  - Tolakram
					var items = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + GameServer.Database.Escape(inventoryID) + 
																					"' AND (SlotPosition <= " + (int)eInventorySlot.LastVault + 
																					" OR (SlotPosition >= 500 AND SlotPosition < 600))");

					foreach (InventoryItem item in items)
					{
						try
						{
							var itemSlot = (eInventorySlot)item.SlotPosition;

							if (item.CanUseEvery > 0)
							{
								item.SetCooldown();
							}

							if (GetValidInventorySlot((eInventorySlot)item.SlotPosition) == eInventorySlot.Invalid)
							{
								if (Log.IsErrorEnabled)
									Log.Error("Tried to load an item in invalid slot, ignored. Item id=" + item.ObjectId);

								continue;
							}

							if (m_items.ContainsKey(itemSlot))
							{
								if (Log.IsErrorEnabled)
								{
									Log.ErrorFormat("Error loading {0}'s ({1}) inventory!\nDuplicate item {2} found in slot {3}; Skipping!",
										m_player.Name, inventoryID, item.Name, itemSlot);
								}

								continue;
							}

							// Depending on whether or not the item is an artifact we will
							// create different types of inventory items. That way we can speed
							// up item type checks and implement item delve information in
							// a natural way, i.e. through inheritance.

							if (ArtifactMgr.IsArtifact(item))
							{
								m_items.Add(itemSlot, new InventoryArtifact(item));
							}
							else
							{
								m_items.Add(itemSlot, item);
							}

							if (Log.IsWarnEnabled)
							{
								// bows don't use damage type - no warning needed
								if (GlobalConstants.IsWeapon(item.Object_Type)
									&& item.Type_Damage == 0
									&& item.Object_Type != (int)eObjectType.CompositeBow
									&& item.Object_Type != (int)eObjectType.Crossbow
									&& item.Object_Type != (int)eObjectType.Longbow
									&& item.Object_Type != (int)eObjectType.Fired
									&& item.Object_Type != (int)eObjectType.RecurvedBow)
								{
									Log.Warn(m_player.Name + ": weapon with damage type 0 is loaded \"" + item.Name + "\" (" + item.ObjectId + ")");
								}
							}
						}
						catch (Exception ex)
						{
							Log.Error("Error loading player inventory (" + inventoryID + "), Inventory_ID: " + 
										item.ObjectId + 
										" (" + (item.ITemplate_Id == null ? "" : item.ITemplate_Id) + 
										", " + (item.UTemplate_Id == null ? "" : item.UTemplate_Id) + 
										"), slot: " + item.SlotPosition, ex);
						}
					}

					// notify handlers that the item was just equipped
					foreach (eInventorySlot slot in EQUIP_SLOTS)
					{
						// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
						if (slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.DistanceWeapon)
							continue;

						InventoryItem item;

						if (m_items.TryGetValue(slot, out item))
						{
							m_player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(item, slot));
						}
					}

					return true;
				}
				catch (Exception e)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Error loading player inventory (" + inventoryID + ").  Load aborted!", e);

					return false;
				}
			}
		}

		/// <summary>
		/// Saves all dirty items to database
		/// </summary>
		/// <param name="inventoryID">The inventory ID</param>
		/// <returns>success</returns>
		public override bool SaveIntoDatabase(string inventoryID)
		{
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				try
				{
					foreach (var item in m_items)
					{
						try
						{
							InventoryItem currentItem = item.Value;

							if (currentItem == null)
								continue;

							if (GetValidInventorySlot((eInventorySlot) currentItem.SlotPosition) == eInventorySlot.Invalid)
							{
								if (Log.IsErrorEnabled)
									Log.Error("item's slot position is invalid. item slot=" + currentItem.SlotPosition + " id=" +
									          currentItem.ObjectId);

								continue;
							}

							if (currentItem.OwnerID != m_player.InternalID)
							{
								string itemOwner = currentItem.OwnerID ?? "(null)";

								if (Log.IsErrorEnabled)
									Log.Error("item owner id (" + itemOwner + ") not equals player ID (" + m_player.InternalID + "); item ID=" +
									          currentItem.ObjectId);

								continue;
							}

							if (currentItem.Dirty)
							{
								var realSlot = (int) item.Key;

								if (currentItem.SlotPosition != realSlot)
								{
									if (Log.IsErrorEnabled)
										Log.Error("Item slot and real slot position are different. Item slot=" + currentItem.SlotPosition +
										          " real slot=" + realSlot + " item ID=" + currentItem.ObjectId);
									currentItem.SlotPosition = realSlot; // just to be sure
								}

								// Check database to make sure player still owns this item before saving

								InventoryItem checkItem = GameServer.Database.SelectObject<InventoryItem>("Inventory_ID = '" + currentItem.ObjectId + "'");

								if (checkItem == null || checkItem.OwnerID != m_player.InternalID)
								{
									if (checkItem != null)
									{
										Log.ErrorFormat("Item '{0}' : '{1}' does not have same owner id on save inventory.  Game Owner = '{2}' : '{3}', DB Owner = '{4}'", currentItem.Name, currentItem.ObjectId, m_player.Name, m_player.InternalID, checkItem.OwnerID);
									}
									else
									{
										Log.ErrorFormat("Item '{0}' : '{1}' not found in DBInventory for player '{2}'", currentItem.Name, currentItem.Id_nb, m_player.Name);
									}

									continue;
								}

								GameServer.Database.SaveObject(currentItem);
							}
						}
						catch (Exception e)
						{
							if (Log.IsErrorEnabled)
								Log.Error("Error saving inventory item: player=" + m_player.Name, e);
						}
					}

					return true;
				}
				catch (Exception e)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Saving player inventory (" + m_player.Name + ")", e);

					return false;
				}
			}
		}

		#endregion Constructor/Declaration/LoadDatabase/SaveDatabase

		#region Add/Remove

		/// <summary>
		/// Adds an item to the inventory and DB
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool AddItem(eInventorySlot slot, InventoryItem item)
		{
			return AddItem(slot, item, true);
		}

		public override bool AddTradeItem(eInventorySlot slot, InventoryItem item)
		{
			return AddItem(slot, item, false);
		}

		protected bool AddItem(eInventorySlot slot, InventoryItem item, bool addObject)
		{
			if (!base.AddItem(slot, item))
				return false;

			// guild banner code here? 
			switch (item.Model)
			{
				case 3223:
					item.Model = 3359;
					break;
				case 3224:
					item.Model = 3361;
					break;
				case 3225:
					item.Model = 3360;
					break;
			}

			item.OwnerID = m_player.InternalID;

			if (addObject)
			{
				GameServer.Database.AddObject(item);
			}
			else
			{
				GameServer.Database.SaveObject(item);
			}


			if (IsEquippedSlot((eInventorySlot)item.SlotPosition))
				m_player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(item, eInventorySlot.Invalid));

			return true;
		}



		public override bool RemoveItem(InventoryItem item)
		{
			return RemoveItem(item, true);
		}

		public override bool RemoveTradeItem(InventoryItem item)
		{
			return RemoveItem(item, false);
		}

		/// <summary>
		/// Removes an item from the inventory and DB
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		protected bool RemoveItem(InventoryItem item, bool deleteObject)
		{
			if (item == null)
				return false;

			if (item.OwnerID != m_player.InternalID)
			{
				if (Log.IsErrorEnabled)
					Log.Error(m_player.Name + ": PlayerInventory -> tried to remove item with wrong owner (" + (item.OwnerID ?? "null") +
					          ")\n\n" + Environment.StackTrace);
				return false;
			}

			var oldSlot = (eInventorySlot) item.SlotPosition;

			if (!base.RemoveItem(item))
				return false;

			if (deleteObject)
			{
				GameServer.Database.DeleteObject(item);
			}
			else
			{
				GameServer.Database.SaveObject(item);
			}

			ITradeWindow window = m_player.TradeWindow;
			if (window != null)
				window.RemoveItemToTrade(item);

			if (oldSlot >= eInventorySlot.RightHandWeapon && oldSlot <= eInventorySlot.DistanceWeapon)
			{
				// if active weapon was destroyed
				if (m_player.AttackWeapon == null)
				{
					m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
				}
				else
				{
					m_player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(item, oldSlot));
				}
			}
			else if (oldSlot >= eInventorySlot.FirstQuiver && oldSlot <= eInventorySlot.FourthQuiver)
			{
				m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.None, true);
			}
			else if (IsEquippedSlot(oldSlot))
			{
				m_player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(item, oldSlot));
			}

			return true;
		}

		/// <summary>
		/// Adds count of items to the inventory item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public override bool AddCountToStack(InventoryItem item, int count)
		{
			if (item != null && item.OwnerID != m_player.InternalID)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Item owner not equals inventory owner.\n" + Environment.StackTrace);

				return false;
			}

			return base.AddCountToStack(item, count);
		}

		/// <summary>
		/// Removes one item from the inventory item
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <param name="count">the count of items to be removed from the stack</param>
		/// <returns>true one item removed</returns>
		public override bool RemoveCountFromStack(InventoryItem item, int count)
		{
			if (item != null && item.OwnerID != m_player.InternalID)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Item owner not equals inventory owner.\n\n" + Environment.StackTrace);

				return false;
			}

			return base.RemoveCountFromStack(item, count);
		}

		#endregion Add/Remove

		#region Get Inventory Informations

		/// <summary>
		/// Check if the slot is valid in this inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected override eInventorySlot GetValidInventorySlot(eInventorySlot slot)
		{
			switch (slot)
			{
				case eInventorySlot.LastEmptyQuiver:
					slot = FindLastEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver);
					break;
				case eInventorySlot.FirstEmptyQuiver:
					slot = FindFirstEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver);
					break;
				case eInventorySlot.LastEmptyVault:
					slot = FindLastEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault);
					break;
				case eInventorySlot.FirstEmptyVault:
					slot = FindFirstEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault);
					break;
				case eInventorySlot.LastEmptyBackpack:
					slot = FindLastEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					break;
				case eInventorySlot.FirstEmptyBackpack:
					slot = FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					break;
			}

			if ((slot >= eInventorySlot.FirstBackpack && slot <= eInventorySlot.LastBackpack)
			    //				|| ( slot >= eInventorySlot.Mithril && slot <= eInventorySlot.Copper ) // can't place items in money slots, is it?
			    || (slot >= eInventorySlot.HorseArmor && slot <= eInventorySlot.Horse)
			    || (slot >= eInventorySlot.FirstVault && slot <= eInventorySlot.LastVault)
			    || (slot >= eInventorySlot.HouseVault_First && slot <= eInventorySlot.HouseVault_Last)
			    || (slot >= eInventorySlot.Consignment_First && slot <= eInventorySlot.Consignment_Last)
			    || (slot == eInventorySlot.PlayerPaperDoll)
			    || (slot == eInventorySlot.Mythical))
				return slot;


			return base.GetValidInventorySlot(slot);
		}

		#endregion Get Inventory Informations

		#region Move Item

		/// <summary>
		/// Moves an item from one slot to another
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <param name="itemCount">How many items to move</param>
		/// <returns>true if items switched successfully</returns>
		public override bool MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			if (!m_player.IsAlive)
			{
				m_player.Out.SendMessage("You can't change your inventory when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_player.Out.SendInventorySlotsUpdate(null);

				return false;
			}

			bool valid = true;
			InventoryItem fromItem, toItem;
			eInventorySlot[] updatedSlots;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				fromSlot = GetValidInventorySlot(fromSlot);
				toSlot = GetValidInventorySlot(toSlot);
				if (fromSlot == eInventorySlot.Invalid || toSlot == eInventorySlot.Invalid)
				{
					if (m_player.Client.Account.PrivLevel > 1)
					{
						m_player.Out.SendMessage("Invalid slot.", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
					}

					return false;
				}

				// just change active weapon if placed in same slot
				if (fromSlot == toSlot)
				{
					switch (toSlot)
					{
						case eInventorySlot.RightHandWeapon:
						case eInventorySlot.LeftHandWeapon:
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
							return false;
						case eInventorySlot.TwoHandWeapon:
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
							return false;
						case eInventorySlot.DistanceWeapon:
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
							return false;
					}
				}

				m_items.TryGetValue(fromSlot, out fromItem);
				m_items.TryGetValue(toSlot, out toItem);

				updatedSlots = new eInventorySlot[2];
				updatedSlots[0] = fromSlot;
				updatedSlots[1] = toSlot;

				if (fromItem == toItem || fromItem == null)
					valid = false;

				if (valid && toItem != null && fromItem.Object_Type == (int) eObjectType.Poison &&
				    GlobalConstants.IsWeapon(toItem.Object_Type))
				{
					m_player.ApplyPoison(fromItem, toItem);
					m_player.Out.SendInventorySlotsUpdate(null);

					return false;
				}

				// graveen = fix for allowedclasses is empty or null
				if (fromItem != null && Util.IsEmpty(fromItem.AllowedClasses))
				{
					fromItem.AllowedClasses = "0";
				}

				if (toItem != null && Util.IsEmpty(toItem.AllowedClasses))
				{
					toItem.AllowedClasses = "0";
				}

				//Andraste - Vico / fixing a bugexploit : when player switch from his char slot to an inventory slot, allowedclasses were not checked
				if (valid && fromItem.AllowedClasses != "0")
				{
					if (!(toSlot >= eInventorySlot.FirstBackpack && toSlot <= eInventorySlot.LastBackpack))
						// but we allow the player to switch the item inside his inventory (check only char slots)
					{
						valid = false;
						string[] allowedclasses = fromItem.AllowedClasses.Split(';');
						foreach (string allowed in allowedclasses)
						{
							if (m_player.CharacterClass.ID.ToString() == allowed || m_player.Client.Account.PrivLevel > 1)
							{
								valid = true;
								break;
							}
						}

						if (!valid)
						{
							m_player.Out.SendMessage("Your class cannot use this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
				}

				if (valid && toItem != null && toItem.AllowedClasses != "0")
				{
					if (!(fromSlot >= eInventorySlot.FirstBackpack && fromSlot <= eInventorySlot.LastBackpack))
						// but we allow the player to switch the item inside his inventory (check only char slots)
					{
						valid = false;
						string[] allowedclasses = toItem.AllowedClasses.Split(';');
						foreach (string allowed in allowedclasses)
						{
							if (m_player.CharacterClass.ID.ToString() == allowed || m_player.Client.Account.PrivLevel > 1)
							{
								valid = true;
								break;
							}
						}

						if (!valid)
						{
							m_player.Out.SendMessage("Your class cannot use this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
				}

				if (valid)
				{
					switch (toSlot)
					{
							//Andraste - Vico : Mythical
						case eInventorySlot.Mythical:
							if (fromItem.Item_Type != (int) eInventorySlot.Mythical)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}

							if (valid && fromItem.Type_Damage > m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage(
									"You can't use " + fromItem.GetName(0, true) + " , you should increase your champion level.",
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
							//horse slots
						case eInventorySlot.HorseBarding:
							if (fromItem.Item_Type != (int) eInventorySlot.HorseBarding)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active barding slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.HorseArmor:
							if (fromItem.Item_Type != (int) eInventorySlot.HorseArmor)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active horse armor slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.Horse:
							if (fromItem.Item_Type != (int) eInventorySlot.Horse)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active mount slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
							//weapon slots
						case eInventorySlot.RightHandWeapon:
							if (fromItem.Object_Type == (int) eObjectType.Shield //shield can't be used in right hand slot
							    ||
							    (fromItem.Item_Type != (int) eInventorySlot.RightHandWeapon
							     //right hand weapons can be used in right hand slot
							     && fromItem.Item_Type != (int) eInventorySlot.LeftHandWeapon))
								//left hand weapons can be used in right hand slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(fromItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.TwoHandWeapon:
							if (fromItem.Object_Type == (int) eObjectType.Shield //shield can't be used in 2h slot
							    || (fromItem.Item_Type != (int) eInventorySlot.RightHandWeapon //right hand weapons can be used in 2h slot
							        && fromItem.Item_Type != (int) eInventorySlot.LeftHandWeapon //left hand weapons can be used in 2h slot
							        && fromItem.Item_Type != (int) eInventorySlot.TwoHandWeapon //2h weapons can be used in 2h slot
							        && fromItem.Object_Type != (int) eObjectType.Instrument)) //instruments can be used in 2h slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(fromItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftHandWeapon:
							if (fromItem.Item_Type != (int) toSlot ||
							    (fromItem.Object_Type != (int) eObjectType.Shield && !m_player.CanUseLefthandedWeapon))
								//shield can be used only in left hand slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(fromItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.DistanceWeapon:
							//m_player.Out.SendDebugMessage("From: {0} to {1} ItemType={2}",fromSlot,toSlot,fromItem.Item_Type);
							if (fromItem.Item_Type != (int) toSlot && fromItem.Object_Type != (int) eObjectType.Instrument)
								//instruments can be used in ranged slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(fromItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;

							//armor slots
						case eInventorySlot.HeadArmor:
						case eInventorySlot.HandsArmor:
						case eInventorySlot.FeetArmor:
						case eInventorySlot.TorsoArmor:
						case eInventorySlot.LegsArmor:
						case eInventorySlot.ArmsArmor:
							if (fromItem.Item_Type != (int) toSlot)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(fromItem.Template ))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;

						case eInventorySlot.Jewellery:
						case eInventorySlot.Cloak:
						case eInventorySlot.Neck:
						case eInventorySlot.Waist:
							if (fromItem.Item_Type != (int) toSlot)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftBracer:
						case eInventorySlot.RightBracer:
							if (fromItem.Item_Type != Slot.RIGHTWRIST && fromItem.Item_Type != Slot.LEFTWRIST)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftRing:
						case eInventorySlot.RightRing:
							if (fromItem.Item_Type != Slot.LEFTRING && fromItem.Item_Type != Slot.RIGHTRING)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.FirstQuiver:
						case eInventorySlot.SecondQuiver:
						case eInventorySlot.ThirdQuiver:
						case eInventorySlot.FourthQuiver:
							if (fromItem.Object_Type != (int) eObjectType.Arrow && fromItem.Object_Type != (int) eObjectType.Bolt)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put your " + fromItem.Name + " in your quiver!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
					}
					//"The Lute of the Initiate must be readied in the 2-handed slot!"
				}

				if (valid && (fromItem.Realm > 0 && (int) m_player.Realm != fromItem.Realm) &&
				    (toSlot >= eInventorySlot.HorseArmor && toSlot <= eInventorySlot.HorseBarding))
				{
					if (m_player.Client.Account.PrivLevel == 1)
					{
						valid = false;
					}

					m_player.Out.SendMessage("You cannot put an item from this realm!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				if (valid && toItem != null)
				{
					switch (fromSlot)
					{
							//Andraste
						case eInventorySlot.Mythical:
							if (toItem.Item_Type != (int) eInventorySlot.Mythical)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}

							if (valid && toItem.Type_Damage > m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage(
									"You can't use " + toItem.GetName(0, true) + " , you should increase your champion level.", eChatType.CT_System,
									eChatLoc.CL_SystemWindow);
							}
							break;
							//horse slots
						case eInventorySlot.HorseBarding:
							if (toItem.Item_Type != (int) eInventorySlot.HorseBarding)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active barding slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.HorseArmor:
							if (toItem.Item_Type != (int) eInventorySlot.HorseArmor)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active horse armor slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.Horse:
							if (toItem.Item_Type != (int) eInventorySlot.Horse)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active mount slot!",
								                         eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
							//weapon slots
						case eInventorySlot.RightHandWeapon:
							if (toItem.Object_Type == (int) eObjectType.Shield //shield can't be used in right hand slot
							    ||
							    (toItem.Item_Type != (int) eInventorySlot.RightHandWeapon //right hand weapons can be used in right hand slot
							     && toItem.Item_Type != (int) eInventorySlot.LeftHandWeapon))
								//left hand weapons can be used in right hand slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(toItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.TwoHandWeapon:
							if (toItem.Object_Type == (int) eObjectType.Shield //shield can't be used in 2h slot
							    || (toItem.Item_Type != (int) eInventorySlot.RightHandWeapon //right hand weapons can be used in 2h slot
							        && toItem.Item_Type != (int) eInventorySlot.LeftHandWeapon //left hand weapons can be used in 2h slot
							        && toItem.Item_Type != (int) eInventorySlot.TwoHandWeapon //2h weapons can be used in 2h slot
							        && toItem.Object_Type != (int) eObjectType.Instrument)) //instruments can be used in 2h slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(toItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftHandWeapon:
							if (toItem.Item_Type != (int) fromSlot ||
							    (toItem.Object_Type != (int) eObjectType.Shield && !m_player.CanUseLefthandedWeapon))
								//shield can be used only in left hand slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(toItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.DistanceWeapon:
							if (toItem.Item_Type != (int) fromSlot && toItem.Object_Type != (int) eObjectType.Instrument)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(toItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;

							//armor slots
						case eInventorySlot.HeadArmor:
						case eInventorySlot.HandsArmor:
						case eInventorySlot.FeetArmor:
						case eInventorySlot.TorsoArmor:
						case eInventorySlot.LegsArmor:
						case eInventorySlot.ArmsArmor:
							if (toItem.Item_Type != (int) fromSlot)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							else if (!m_player.HasAbilityToUseItem(toItem.Template))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;

						case eInventorySlot.Jewellery:
						case eInventorySlot.Cloak:
						case eInventorySlot.Neck:
						case eInventorySlot.Waist:
							if (toItem.Item_Type != (int) fromSlot)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftBracer:
						case eInventorySlot.RightBracer:
							if (toItem.Item_Type != Slot.RIGHTWRIST && toItem.Item_Type != Slot.LEFTWRIST)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftRing:
						case eInventorySlot.RightRing:
							if (toItem.Item_Type != Slot.LEFTRING && toItem.Item_Type != Slot.RIGHTRING)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.FirstQuiver:
						case eInventorySlot.SecondQuiver:
						case eInventorySlot.ThirdQuiver:
						case eInventorySlot.FourthQuiver:
							if (toItem.Object_Type != (int) eObjectType.Arrow && toItem.Object_Type != (int) eObjectType.Bolt)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put your " + toItem.Name + " in your quiver!", eChatType.CT_System,
								                         eChatLoc.CL_SystemWindow);
							}
							break;
					}
				}

				if (valid)
				{
					base.MoveItem(fromSlot, toSlot, itemCount);
				}
			}

			if (valid)
			{
				foreach (eInventorySlot updatedSlot in updatedSlots)
				{
					if ((updatedSlot >= eInventorySlot.RightHandWeapon && updatedSlot <= eInventorySlot.DistanceWeapon)
					    || (updatedSlot >= eInventorySlot.FirstQuiver && updatedSlot <= eInventorySlot.FourthQuiver))
					{
						m_player.StopAttack();
						break;
					}
				}

				ITradeWindow window = m_player.TradeWindow;
				if (window != null)
				{
					if (toItem != null)
						window.RemoveItemToTrade(toItem);
						window.RemoveItemToTrade(fromItem);
				}


				// activate weapon slot if moved to it
				switch (toSlot)
				{
					case eInventorySlot.RightHandWeapon:
						m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case eInventorySlot.TwoHandWeapon:
						m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						break;
					case eInventorySlot.DistanceWeapon:
						m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
						break;
					case eInventorySlot.LeftHandWeapon:
						if (m_player.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
							m_player.SwitchWeapon(m_player.ActiveWeaponSlot);
						else m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case eInventorySlot.FirstQuiver:
						m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.First, true);
						break;
					case eInventorySlot.SecondQuiver:
						m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.Second, true);
						break;
					case eInventorySlot.ThirdQuiver:
						m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.Third, true);
						break;
					case eInventorySlot.FourthQuiver:
						m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.Fourth, true);
						break;


					default:
						// change active weapon if moved from active slot
						if (fromSlot == eInventorySlot.RightHandWeapon &&
						    m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard)
						{
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						}
						else if (fromSlot == eInventorySlot.TwoHandWeapon &&
						         m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded)
						{
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						}
						else if (fromSlot == eInventorySlot.DistanceWeapon &&
						         m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
						{
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						}
						else if (fromSlot == eInventorySlot.LeftHandWeapon &&
						         (m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded ||
						          m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard))
						{
							m_player.SwitchWeapon(m_player.ActiveWeaponSlot);
						}

						if (fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver)
						{
							m_player.SwitchQuiver(GameLiving.eActiveQuiverSlot.None, true);
						}

						break;
				}
			}

			m_player.Out.SendInventorySlotsUpdate(null);

			return valid;
		}

		#endregion Move Item

		#region Combine/Exchange/Stack Items

		/// <summary>
		/// Combine 2 items together if possible
		/// </summary>
		/// <param name="fromItem">First Item</param>
		/// <param name="toItem">Second Item</param>
		/// <returns>true if items combined successfully</returns>
		protected override bool CombineItems(InventoryItem fromItem, InventoryItem toItem)
		{
			if (toItem == null ||
			    fromItem.SlotPosition < (int) eInventorySlot.FirstBackpack ||
			    fromItem.SlotPosition > (int) eInventorySlot.LastBackpack)
				return false;

			//Is the fromItem a dye or dyepack?
			//TODO shouldn't be done with model check
			switch (fromItem.Model)
			{
				case 229:
				case 494:
				case 495:
				case 538:
					return DyeItem(fromItem, toItem);
			}

			return false;
		}

		/// <summary>
		/// Stack an item with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <param name="itemCount">How many items to move</param>
		/// <returns>true if items stacked successfully</returns>
		protected override bool StackItems(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			InventoryItem fromItem;
			InventoryItem toItem;

			m_items.TryGetValue(fromSlot, out fromItem);
			m_items.TryGetValue(toSlot, out toItem);

			if (toSlot < eInventorySlot.FirstQuiver
			    || (toSlot > eInventorySlot.FourthQuiver && toSlot < eInventorySlot.FirstBackpack)
			    || (toSlot > eInventorySlot.LastBackpack && toSlot < eInventorySlot.FirstVault)
			    || toSlot > eInventorySlot.LastVault)
				return false;

			if (itemCount == 0)
			{
				itemCount = fromItem.Count > 0 ? fromItem.Count : 1;
			}

			if (toItem != null && toItem.Id_nb.Equals(fromItem.Id_nb) && toItem.IsStackable)
			{
				if (fromItem.Count + toItem.Count > fromItem.MaxCount)
				{
					fromItem.Count -= (toItem.MaxCount - toItem.Count);
					toItem.Count = toItem.MaxCount;
				}
				else
				{
					toItem.Count += fromItem.Count;
					RemoveItem(fromItem);
				}

				return true;
			}

			if (toItem == null && fromItem.Count > itemCount)
			{
				var newItem = (InventoryItem) fromItem.Clone();
				m_items[toSlot] = newItem;
				newItem.Count = itemCount;
				newItem.SlotPosition = (int) toSlot;
				fromItem.Count -= itemCount;
				newItem.AllowAdd = fromItem.Template.AllowAdd;
				GameServer.Database.AddObject(newItem);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Exchange one item position with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <returns>true if items exchanged successfully</returns>
		protected override bool ExchangeItems(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			InventoryItem fromItem;
			InventoryItem toItem;

			m_items.TryGetValue(fromSlot, out fromItem);
			m_items.TryGetValue(toSlot, out toItem);

			bool fromSlotEquipped = IsEquippedSlot(fromSlot);
			bool toSlotEquipped = IsEquippedSlot(toSlot);

			base.ExchangeItems(fromSlot, toSlot);

			if (fromItem != null)
			{
				GameServer.Database.SaveObject(fromItem);
			}
			if (toItem != null && toItem != fromItem)
			{
				GameServer.Database.SaveObject(toItem);
			}

			// notify handlers if items changing state
			if (fromSlotEquipped != toSlotEquipped)
			{
				if (toItem != null)
				{
					if (toSlotEquipped) // item was equipped
					{
						m_player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(toItem, toSlot));
					}
					else
					{
						m_player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(toItem, toSlot));
					}
				}

				if (fromItem != null)
				{
					if (fromSlotEquipped) // item was equipped
					{
						m_player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(fromItem, fromSlot));
					}
					else
					{
						m_player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(fromItem, fromSlot));
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if slot is equipped on player
		/// </summary>
		/// <param name="slot">The slot to check</param>
		/// <returns>true if slot is one of equipment slots and should add magical bonuses</returns>
		public virtual bool IsEquippedSlot(eInventorySlot slot)
		{
			// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
			if (slot > eInventorySlot.DistanceWeapon || slot < eInventorySlot.RightHandWeapon)
			{
				foreach (eInventorySlot staticSlot in EQUIP_SLOTS)
				{
					if (slot == staticSlot)
						return true;
				}

				return false;
			}

			switch (slot)
			{
				case eInventorySlot.RightHandWeapon:
					return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x00;

				case eInventorySlot.LeftHandWeapon:
					return (m_player.VisibleActiveWeaponSlots & 0xF0) == 0x10;

				case eInventorySlot.TwoHandWeapon:
					return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x02;

				case eInventorySlot.DistanceWeapon:
					return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x03;
			}

			return false;
		}

		#endregion Combine/Exchange/Stack Items

		#region Encumberance

		/// <summary>
		/// Gets the inventory weight
		/// </summary>
		public override int InventoryWeight
		{
			get
			{
				int weight = 0;

				lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					InventoryItem item;

					for (eInventorySlot slot = eInventorySlot.FirstBackpack; slot <= eInventorySlot.LastBackpack; slot++)
					{
						if (m_items.TryGetValue(slot, out item))
						{
							weight += item.Weight;
						}
					}

					return weight/10 + base.InventoryWeight;
				}
			}
		}

		#endregion

		#region Dyes

		protected virtual bool DyeItem(InventoryItem dye, InventoryItem objectToDye)
		{
			bool canApply = false;
			//TODO should not be tested via model

			int itemObjType = objectToDye.Object_Type;
			int itemItemType = objectToDye.Item_Type;

			switch (dye.Model)
			{
				case 229: //Dyes
					if (itemObjType == 32) //Cloth
					{
						canApply = true;
					}
					if (itemObjType == 41 && itemItemType == 26) // magical cloaks
					{
						canApply = true;
					}
					if (itemObjType == 41 && itemItemType == 8) // horse barding
					{
						canApply = true;
					}

					break;
				case 494: //Dye pack
					if (itemObjType == 33) //Leather
					{
						canApply = true;
					}
					break;
				case 495: //Dye pack
					if ((itemObjType == 42) // Shield
					    || (itemObjType == 34) // Studded
					    || (itemObjType == 35) // Chain
					    || (itemObjType == 36) // Plate
					    || (itemObjType == 37) // Reinforced
					    || (itemObjType == 38) // Scale
					    || ((itemObjType == 41) && (itemItemType == 7))) // horse saddle 
					{
						canApply = true;
					}
					break;
				case 538: //Dye pot
					if (itemObjType >= 1 && itemObjType <= 26)
					{
						canApply = true;
					}
					break;
			}

			if (canApply)
			{
				objectToDye.Color = dye.Color;
				objectToDye.Emblem = 0;
				RemoveCountFromStack(dye, 1);
			}

			return canApply;
		}

		#endregion

		#region UpdateChangedSlots

		/// <summary>
		/// Updates changed slots, inventory is already locked.
		/// Inventory must be locked before invoking this method.
		/// </summary>
		protected override void UpdateChangedSlots()
		{
			m_player.Out.SendInventorySlotsUpdate(new List<int>(m_changedSlots.Cast<int>()));

			bool statsUpdated = false;
			bool appearanceUpdated = false;
			bool encumberanceUpdated = false;

			foreach (eInventorySlot updatedSlot in m_changedSlots)
			{
				// update appearance if one of changed slots is visible
				if (!appearanceUpdated)
				{
					foreach (eInventorySlot visibleSlot in VISIBLE_SLOTS)
					{
						if (updatedSlot != visibleSlot)
							continue;

						m_player.UpdateEquipmentAppearance();
						appearanceUpdated = true;
						break;
					}
				}

				// update stats if equipped item has changed
				if (!statsUpdated && updatedSlot <= eInventorySlot.RightRing && updatedSlot >= eInventorySlot.RightHandWeapon)
				{
					m_player.Out.SendUpdateWeaponAndArmorStats();
					statsUpdated = true;
				}

				// update encumberance if changed slot was in inventory or equipped
				if (!encumberanceUpdated &&
				    //					(updatedSlot >=(int)eInventorySlot.FirstVault && updatedSlot<=(int)eInventorySlot.LastVault) ||
				    (updatedSlot >= eInventorySlot.RightHandWeapon && updatedSlot <= eInventorySlot.RightRing) ||
				    (updatedSlot >= eInventorySlot.FirstBackpack && updatedSlot <= eInventorySlot.LastBackpack))
				{
					m_player.UpdateEncumberance();
					encumberanceUpdated = true;
				}
			}

			base.UpdateChangedSlots();
		}

		#endregion
	}
}