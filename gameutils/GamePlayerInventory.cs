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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Events;
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
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Constructor/Declaration/LoadDatabase/SaveDatabase

		/// <summary>
		/// Holds the player that owns
		/// this inventory
		/// </summary>
		protected readonly GamePlayer m_player;

		/// <summary>
		/// Returns the GamePlayer that owns this inventory
		/// </summary>
		public GamePlayer Player
		{
			get { return m_player; }
		}

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
            lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				try
				{
					DataObject[] items = GameServer.Database.SelectObjects(typeof(InventoryItem), "OwnerID = '" + GameServer.Database.Escape(inventoryID) + "'");
					foreach (InventoryItem item in items)
					{
						if (item.CanUseEvery > 0)
							item.SetCooldown();

						if (GetValidInventorySlot((eInventorySlot)item.SlotPosition) == eInventorySlot.Invalid)
						{
							if (log.IsErrorEnabled)
								log.Error("Tried to load an item in invalid slot, ignored. Item id=" + item.ObjectId);
							continue;
						}

                        if (m_items[item.SlotPosition] != null)
						{
							if (log.IsErrorEnabled)
								log.Error("Error loading " + m_player.Name + "'s inventory OwnerID " + inventoryID + " slot " + item.SlotPosition + " duplicate item found, skipping!");
							continue;
						}

						// Depending on whether or not the item is an artifact we will
						// create different types of inventory items. That way we can speed
						// up item type checks and implement item delve information in
						// a natural way, i.e. through inheritance.

						if (ArtifactMgr.IsArtifact(item))
							m_items.Add(item.SlotPosition, new InventoryArtifact(item));
						else
							m_items.Add(item.SlotPosition, item);

						if (GlobalConstants.IsWeapon(item.Object_Type)
							&& item.Type_Damage == 0
							&& item.Object_Type != (int)eObjectType.CompositeBow
							&& item.Object_Type != (int)eObjectType.Crossbow
							&& item.Object_Type != (int)eObjectType.Longbow
							&& item.Object_Type != (int)eObjectType.Fired
							&& item.Object_Type != (int)eObjectType.RecurvedBow) // bows don't use damage type - no warning needed
							if (log.IsWarnEnabled)
								log.Warn(Player.Name + ": weapon with damage type 0 is loaded \"" + item.Name + "\" (" + item.ObjectId + ")");
					}

					// notify handlers that the item was just equipped
					foreach (eInventorySlot slot in EQUIP_SLOTS)
					{
						// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
						if (slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.DistanceWeapon) continue;
						if (m_items[(int)slot] != null)
							Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs((InventoryItem)m_items[(int)slot], (int)slot));
					}
					return true;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Loading player inventory (" + inventoryID + ")", e);
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
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				try
				{
					foreach (DictionaryEntry item in m_items)
					{
						try
						{
							InventoryItem currentItem = (InventoryItem)item.Value;
							if (currentItem == null) continue;
							if (GetValidInventorySlot((eInventorySlot)currentItem.SlotPosition) == eInventorySlot.Invalid)
							{
								if (log.IsErrorEnabled)
									log.Error("item's slot position is invalid. item slot=" + currentItem.SlotPosition + " id=" + currentItem.ObjectId);
								continue;
							}
							if (currentItem.OwnerID != m_player.InternalID)
							{
								string itemOwner = (currentItem.OwnerID == null ? "(null)" : currentItem.OwnerID);
								if (log.IsErrorEnabled)
									log.Error("item owner id (" + itemOwner + ") not equals player ID (" + m_player.InternalID + "); item ID=" + currentItem.ObjectId);
								continue;
							}
							if (currentItem.Dirty)
							{
								int realSlot = (int)item.Key;
								if (currentItem.SlotPosition != realSlot)
								{
									if (log.IsErrorEnabled)
										log.Error("Item slot and real slot position are different. Item slot=" + currentItem.SlotPosition + " real slot=" + realSlot + " item ID=" + currentItem.ObjectId);
									currentItem.SlotPosition = realSlot; // just to be sure
								}
								GameServer.Database.SaveObject(currentItem);
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("Error saving inventory item: player=" + Player.Name, e);
						}
					}
					return true;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Saving player inventory (" + m_player.Name + ")", e);
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
			if (!base.AddItem(slot, item)) return false;
			item.OwnerID = m_player.InternalID;
			GameServer.Database.AddNewObject(item);
            
            if (IsEquippedSlot((eInventorySlot)item.SlotPosition))
				Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(item, (int)eInventorySlot.Invalid));
			return true;
		}

		/// <summary>
		/// Removes an item from the inventory and DB
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		public override bool RemoveItem(InventoryItem item)
		{
			if (item == null) return false;
			int oldSlot;

			if (item.OwnerID != m_player.InternalID)
			{
				if (log.IsErrorEnabled)
					log.Error(m_player.Name + ": PlayerInventory -> tried to remove item with wrong owner (" + (item.OwnerID ?? "null") + ")\n\n" + Environment.StackTrace);
				return false;
			}

			oldSlot = item.SlotPosition;

			if (!base.RemoveItem(item)) return false;

			GameServer.Database.DeleteObject(item);            
            ITradeWindow window = m_player.TradeWindow;
			if (window != null)
				window.RemoveItemToTrade(item);

			if (oldSlot >= (int)eInventorySlot.RightHandWeapon && oldSlot <= (int)eInventorySlot.DistanceWeapon)
			{
				// if active weapon was destroyed
				if (m_player.AttackWeapon == null)
					m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
				else
					Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(item, oldSlot));
			}
			else if (oldSlot >= (int)eInventorySlot.FirstQuiver && oldSlot <= (int)eInventorySlot.FourthQuiver)
			{
				m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
			}
			else if (IsEquippedSlot((eInventorySlot)oldSlot))
			{
				Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(item, oldSlot));
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
				if (log.IsErrorEnabled)
					log.Error("Item owner not equals inventory owner.\n" + Environment.StackTrace);
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
				if (log.IsErrorEnabled)
					log.Error("Item owner not equals inventory owner.\n\n" + Environment.StackTrace);
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
				case eInventorySlot.LastEmptyQuiver: slot = FindLastEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver); break;
				case eInventorySlot.FirstEmptyQuiver: slot = FindFirstEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver); break;
				case eInventorySlot.LastEmptyVault: slot = FindLastEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault); break;
				case eInventorySlot.FirstEmptyVault: slot = FindFirstEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault); break;
				case eInventorySlot.LastEmptyBackpack: slot = FindLastEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack); break;
				case eInventorySlot.FirstEmptyBackpack: slot = FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack); break;
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
			int[] updatedSlots;

			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				fromSlot = GetValidInventorySlot(fromSlot);
				toSlot = GetValidInventorySlot(toSlot);
				if (fromSlot == eInventorySlot.Invalid || toSlot == eInventorySlot.Invalid)
				{
					if (Player.Client.Account.PrivLevel > 1)
						Player.Out.SendMessage("Invalid slot.",
							eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
					return false;
				}

				// just change active weapon if placed in same slot
				if (fromSlot == toSlot)
				{
					switch (toSlot)
					{
						case eInventorySlot.RightHandWeapon:
						case eInventorySlot.LeftHandWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); return false;
						case eInventorySlot.TwoHandWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); return false;
						case eInventorySlot.DistanceWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); return false;
					}
				}

				fromItem = (InventoryItem)m_items[(int)fromSlot];
				toItem = (InventoryItem)m_items[(int)toSlot];

				updatedSlots = new int[2];
				updatedSlots[0] = (int)fromSlot;
				updatedSlots[1] = (int)toSlot;
				if (fromItem == toItem || fromItem == null)
					valid = false;

				if (valid && toItem != null && fromItem.Object_Type == (int)eObjectType.Poison && GlobalConstants.IsWeapon(toItem.Object_Type))
				{
					valid = false;
					m_player.ApplyPoison(fromItem, toItem);
					m_player.Out.SendInventorySlotsUpdate(null);
					return valid;
				}

                // graveen = fix for allowedclasses is empty or null
                if (Util.IsEmpty(fromItem.AllowedClasses)) fromItem.AllowedClasses = "0";
                if (toItem!=null&&Util.IsEmpty(toItem.AllowedClasses)) toItem.AllowedClasses = "0";

				//Andraste - Vico / fixing a bugexploit : when player switch from his char slot to an inventory slot, allowedclasses were not checked
				if (valid && fromItem.AllowedClasses !="0")
				{
					if (!(toSlot >= eInventorySlot.FirstBackpack && toSlot <= eInventorySlot.LastBackpack)) // but we allow the player to switch the item inside his inventory (check only char slots)
					{
						valid = false;
						string[] allowedclasses = fromItem.AllowedClasses.Split(';');
						foreach (string allowed in allowedclasses) if (m_player.CharacterClass.ID.ToString() == allowed || m_player.Client.Account.PrivLevel > 1) { valid = true; break; }
						if (!valid) m_player.Out.SendMessage("Your class cannot use this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				if (valid && toItem!=null && toItem.AllowedClasses!="0")
				{
					if (!(fromSlot >= eInventorySlot.FirstBackpack && fromSlot <= eInventorySlot.LastBackpack)) // but we allow the player to switch the item inside his inventory (check only char slots)
					{
						valid = false;
						string[] allowedclasses = toItem.AllowedClasses.Split(';');
						foreach (string allowed in allowedclasses) if (m_player.CharacterClass.ID.ToString() == allowed || m_player.Client.Account.PrivLevel > 1) { valid = true; break; }
						if (!valid)	m_player.Out.SendMessage("Your class cannot use this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				
				

				if (valid == true)
				{
					switch (toSlot)
					{
						//Andraste - Vico : Mythical
						case eInventorySlot.Mythical:
							if (fromItem.Item_Type != (int)eInventorySlot.Mythical)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							if (valid && fromItem.Level > 0 && fromItem.Level > m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't use " + fromItem.GetName(0, true) + " , you should increase your champion level.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						//horse slots
						case eInventorySlot.HorseBarding:
							if (fromItem.Item_Type != (int)eInventorySlot.HorseBarding)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							/*
							if (valid && fromItem.Level > 0 && fromItem.Level >= m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							 */
							break;
						case eInventorySlot.HorseArmor:
							if (fromItem.Item_Type != (int)eInventorySlot.HorseArmor)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active horse armor slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							/*
							if (valid && fromItem.Level > 0 && fromItem.Level >= m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							 */
							break;
						case eInventorySlot.Horse:
							if (fromItem.Item_Type != (int)eInventorySlot.Horse)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + fromItem.GetName(0, true) + " in your active mount slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						//weapon slots
						case eInventorySlot.RightHandWeapon:
							if (fromItem.Object_Type == (int)eObjectType.Shield //shield can't be used in right hand slot
								|| (fromItem.Item_Type != (int)eInventorySlot.RightHandWeapon //right hand weapons can be used in right hand slot
								&& fromItem.Item_Type != (int)eInventorySlot.LeftHandWeapon)) //left hand weapons can be used in right hand slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(fromItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.TwoHandWeapon:
							if (fromItem.Object_Type == (int)eObjectType.Shield //shield can't be used in 2h slot
								|| (fromItem.Item_Type != (int)eInventorySlot.RightHandWeapon //right hand weapons can be used in 2h slot
								&& fromItem.Item_Type != (int)eInventorySlot.LeftHandWeapon //left hand weapons can be used in 2h slot
								&& fromItem.Item_Type != (int)eInventorySlot.TwoHandWeapon //2h weapons can be used in 2h slot
								&& fromItem.Object_Type != (int)eObjectType.Instrument)) //instruments can be used in 2h slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(fromItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftHandWeapon:
							if (fromItem.Item_Type != (int)toSlot || (fromItem.Object_Type != (int)eObjectType.Shield && !Player.CanUseLefthandedWeapon)) //shield can be used only in left hand slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(fromItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.DistanceWeapon:
							//Player.Out.SendDebugMessage("From: {0} to {1} ItemType={2}",fromSlot,toSlot,fromItem.Item_Type);
							if (fromItem.Item_Type != (int)toSlot && fromItem.Object_Type != (int)eObjectType.Instrument) //instruments can be used in ranged slot
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(fromItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;

						//armor slots
						case eInventorySlot.HeadArmor:
						case eInventorySlot.HandsArmor:
						case eInventorySlot.FeetArmor:
						case eInventorySlot.TorsoArmor:
						case eInventorySlot.LegsArmor:
						case eInventorySlot.ArmsArmor:
							if (fromItem.Item_Type != (int)toSlot)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(fromItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;

						case eInventorySlot.Jewellery:
						case eInventorySlot.Cloak:
						case eInventorySlot.Neck:
						case eInventorySlot.Waist:
							if (fromItem.Item_Type != (int)toSlot)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftBracer:
						case eInventorySlot.RightBracer:
							if (fromItem.Item_Type != Slot.RIGHTWRIST && fromItem.Item_Type != Slot.LEFTWRIST)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftRing:
						case eInventorySlot.RightRing:
							if (fromItem.Item_Type != Slot.LEFTRING && fromItem.Item_Type != Slot.RIGHTRING)
							{
								valid = false;
								m_player.Out.SendMessage(fromItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.FirstQuiver:
						case eInventorySlot.SecondQuiver:
						case eInventorySlot.ThirdQuiver:
						case eInventorySlot.FourthQuiver:
							if (fromItem.Object_Type != (int)eObjectType.Arrow && fromItem.Object_Type != (int)eObjectType.Bolt)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put your " + fromItem.Name + " in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
					}
					//"The Lute of the Initiate must be readied in the 2-handed slot!"
				}

				if (valid && (fromItem.Realm > 0 && (int)m_player.Realm != fromItem.Realm) && ((int)toSlot >= (int)eInventorySlot.HorseArmor && (int)toSlot <= (int)eInventorySlot.HorseBarding))
				{
					if (m_player.Client.Account.PrivLevel == 1)
						valid = false;
					m_player.Out.SendMessage("You cannot put an item from this realm!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				if (valid && toItem != null)
				{
					switch (fromSlot)
					{
						//Andraste
						case eInventorySlot.Mythical:
							if (toItem.Item_Type != (int)eInventorySlot.Mythical)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							if (valid && toItem.Level > 0 && toItem.Level > m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't use " + toItem.GetName(0, true) + " , you should increase your champion level.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						//horse slots
						case eInventorySlot.HorseBarding:
							if (toItem.Item_Type != (int)eInventorySlot.HorseBarding)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							/*
							if (valid && toItem.Level > 0 && toItem.Level >= m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							 */
							break;
						case eInventorySlot.HorseArmor:
							if (toItem.Item_Type != (int)eInventorySlot.HorseArmor)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active horse armor slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							/*
							if (valid && toItem.Level > 0 && toItem.Level >= m_player.ChampionLevel)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active barding slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							 */
							break;
						case eInventorySlot.Horse:
							if (toItem.Item_Type != (int)eInventorySlot.Horse)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put " + toItem.GetName(0, true) + " in your active mount slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						//weapon slots
						case eInventorySlot.RightHandWeapon:
							if (toItem.Object_Type == (int)eObjectType.Shield //shield can't be used in right hand slot
								|| (toItem.Item_Type != (int)eInventorySlot.RightHandWeapon //right hand weapons can be used in right hand slot
								&& toItem.Item_Type != (int)eInventorySlot.LeftHandWeapon)) //left hand weapons can be used in right hand slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(toItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.TwoHandWeapon:
							if (toItem.Object_Type == (int)eObjectType.Shield //shield can't be used in 2h slot
								|| (toItem.Item_Type != (int)eInventorySlot.RightHandWeapon //right hand weapons can be used in 2h slot
								&& toItem.Item_Type != (int)eInventorySlot.LeftHandWeapon //left hand weapons can be used in 2h slot
								&& toItem.Item_Type != (int)eInventorySlot.TwoHandWeapon //2h weapons can be used in 2h slot
								&& toItem.Object_Type != (int)eObjectType.Instrument)) //instruments can be used in 2h slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(toItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftHandWeapon:
							if (toItem.Item_Type != (int)fromSlot || (toItem.Object_Type != (int)eObjectType.Shield && !Player.CanUseLefthandedWeapon)) //shield can be used only in left hand slot
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(toItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.DistanceWeapon:
							if (toItem.Item_Type != (int)fromSlot && toItem.Object_Type != (int)eObjectType.Instrument)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(toItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;

						//armor slots
						case eInventorySlot.HeadArmor:
						case eInventorySlot.HandsArmor:
						case eInventorySlot.FeetArmor:
						case eInventorySlot.TorsoArmor:
						case eInventorySlot.LegsArmor:
						case eInventorySlot.ArmsArmor:
							if (toItem.Item_Type != (int)fromSlot)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (!Player.HasAbilityToUseItem(toItem))
							{
								valid = false;
								m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;

						case eInventorySlot.Jewellery:
						case eInventorySlot.Cloak:
						case eInventorySlot.Neck:
						case eInventorySlot.Waist:
							if (toItem.Item_Type != (int)fromSlot)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftBracer:
						case eInventorySlot.RightBracer:
							if (toItem.Item_Type != Slot.RIGHTWRIST && toItem.Item_Type != Slot.LEFTWRIST)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.LeftRing:
						case eInventorySlot.RightRing:
							if (toItem.Item_Type != Slot.LEFTRING && toItem.Item_Type != Slot.RIGHTRING)
							{
								valid = false;
								m_player.Out.SendMessage(toItem.GetName(0, true) + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						case eInventorySlot.FirstQuiver:
						case eInventorySlot.SecondQuiver:
						case eInventorySlot.ThirdQuiver:
						case eInventorySlot.FourthQuiver:
							if (toItem.Object_Type != (int)eObjectType.Arrow && toItem.Object_Type != (int)eObjectType.Bolt)
							{
								valid = false;
								m_player.Out.SendMessage("You can't put your " + toItem.Name + " in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
					}
				}

				if (valid)
				{
					base.MoveItem(fromSlot, toSlot, itemCount);
					fromItem = (InventoryItem)m_items[(int)fromSlot];
					toItem = (InventoryItem)m_items[(int)toSlot];
				}
			}

			if (valid)
			{
				foreach (int updatedSlot in updatedSlots)
				{
					if ((updatedSlot >= (int)eInventorySlot.RightHandWeapon && updatedSlot <= (int)eInventorySlot.DistanceWeapon)
						|| (updatedSlot >= (int)eInventorySlot.FirstQuiver && updatedSlot <= (int)eInventorySlot.FourthQuiver))
					{
						Player.StopAttack();
						break;
					}
				}

				ITradeWindow window = m_player.TradeWindow;
				if (window != null)
				{
					if (toItem != null)
						window.RemoveItemToTrade(toItem);
					if (fromItem != null)
						window.RemoveItemToTrade(fromItem);
				}


				// activate weapon slot if moved to it
				switch (toSlot)
				{
					case eInventorySlot.RightHandWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); break;
					case eInventorySlot.TwoHandWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); break;
					case eInventorySlot.DistanceWeapon: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); break;
					case eInventorySlot.LeftHandWeapon:
						if (m_player.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
							m_player.SwitchWeapon(m_player.ActiveWeaponSlot);
						else m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case eInventorySlot.FirstQuiver: m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.First, true); break;
					case eInventorySlot.SecondQuiver: m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Second, true); break;
					case eInventorySlot.ThirdQuiver: m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Third, true); break;
					case eInventorySlot.FourthQuiver: m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Fourth, true); break;


					default:
						// change active weapon if moved from active slot
						if (fromSlot == eInventorySlot.RightHandWeapon && m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard)
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						else if (fromSlot == eInventorySlot.TwoHandWeapon && m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded)
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						else if (fromSlot == eInventorySlot.DistanceWeapon && m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
							m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						else if (fromSlot == eInventorySlot.LeftHandWeapon && (m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded || m_player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard))
							m_player.SwitchWeapon(m_player.ActiveWeaponSlot);
						if (fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver)
							m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
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
				fromItem.SlotPosition < (int)eInventorySlot.FirstBackpack ||
				fromItem.SlotPosition > (int)eInventorySlot.LastBackpack)
				return false;

			//Is the fromItem a dye or dyepack?
			//TODO shouldn't be done with model check
			switch (fromItem.Model)
			{
				case 229:
				case 494:
				case 495:
				case 538: return DyeItem(fromItem, toItem);
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
		protected override bool StackItems(int fromSlot, int toSlot, int itemCount)
		{
			InventoryItem fromItem = m_items[fromSlot] as InventoryItem;
			InventoryItem toItem = m_items[toSlot] as InventoryItem;

			if (toSlot < (int)eInventorySlot.FirstQuiver
				|| (toSlot > (int)eInventorySlot.FourthQuiver && toSlot < (int)eInventorySlot.FirstBackpack)
				|| (toSlot > (int)eInventorySlot.LastBackpack && toSlot < (int)eInventorySlot.FirstVault)
				|| toSlot > (int)eInventorySlot.LastVault)
				return false;

			if (itemCount == 0)
			{
				if (fromItem.Count > 0)
					itemCount = fromItem.Count;
				else
					itemCount = 1;
			}

			if (toItem != null && toItem.Id_nb.Equals(fromItem.Id_nb) && toItem.IsStackable)
			{
				if (fromItem.Count + toItem.Count > fromItem.MaxCount)
				{
					fromItem.Count -= (toItem.MaxCount - toItem.Count);
					fromItem.Weight = fromItem.Count * (toItem.Weight / toItem.Count);
					toItem.Count = toItem.MaxCount;
					toItem.Weight = toItem.Count * (fromItem.Weight / fromItem.Count);
				}
				else
				{
					toItem.Count += fromItem.Count;
					toItem.Weight += fromItem.Weight;
					RemoveItem(fromItem);
				}
				return true;
			}
			else if (toItem == null && fromItem.Count > itemCount)
			{
				InventoryItem newItem = (InventoryItem)fromItem.Clone();
				m_items[toSlot] = newItem;
				newItem.Count = itemCount;
				newItem.Weight = itemCount * (fromItem.Weight / fromItem.Count);
				newItem.SlotPosition = toSlot;
				fromItem.Weight -= itemCount * (fromItem.Weight / fromItem.Count);
				fromItem.Count -= itemCount;
				GameServer.Database.AddNewObject(newItem);
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
		protected override bool ExchangeItems(int fromSlot, int toSlot)
		{
			InventoryItem fromItem = (InventoryItem)m_items[fromSlot];
			InventoryItem toItem = (InventoryItem)m_items[toSlot];

			//			log.DebugFormat("exchange slot from:{0} to:{1}; same items? {2}", fromSlot, toSlot, fromItem==toItem&&fromItem!=null);

			bool fromSlotEquipped = IsEquippedSlot((eInventorySlot)fromSlot);
			bool toSlotEquipped = IsEquippedSlot((eInventorySlot)toSlot);

			base.ExchangeItems(fromSlot, toSlot);

			if (fromItem != null)
				GameServer.Database.SaveObject(fromItem);
			if (toItem != null && toItem != fromItem)
				GameServer.Database.SaveObject(toItem);

			// notify handlers if items changing state
			if (fromSlotEquipped != toSlotEquipped)
			{
				if (toItem != null)
				{
					if (toSlotEquipped) // item was equipped
						Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(toItem, toSlot));
					else Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(toItem, toSlot));
				}

				if (fromItem != null)
				{
					if (fromSlotEquipped) // item was equipped
						Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs(fromItem, fromSlot));
					else Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs(fromItem, fromSlot));
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
				case eInventorySlot.RightHandWeapon: return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x00;
				case eInventorySlot.LeftHandWeapon: return (m_player.VisibleActiveWeaponSlots & 0xF0) == 0x10;
				case eInventorySlot.TwoHandWeapon: return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x02;
				case eInventorySlot.DistanceWeapon: return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x03;
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
				lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					for (int slot = (int)eInventorySlot.FirstBackpack; slot <= (int)eInventorySlot.LastBackpack; slot++)
					{
						InventoryItem item = m_items[slot] as InventoryItem;
						if (item != null)
						{
							weight += item.Weight;
						}
					}
					return weight / 10 + base.InventoryWeight;
				}
			}
		}
		#endregion

		#region Dyes

		protected virtual bool DyeItem(InventoryItem dye, InventoryItem objectToDye)
		{

			bool canApply = false;
			//TODO should not be tested via model!
			switch (dye.Model)
			{
				case 229: //Dyes
					if (objectToDye.Object_Type == 32) //Cloth
					{
						canApply = true;
					}
					if (objectToDye.Object_Type == 41 && objectToDye.Item_Type == 26) // magical cloaks
					{
						canApply = true;
					}
					if (objectToDye.Object_Type == 41 && objectToDye.Item_Type == 8) // horse barding
					{
						canApply = true;
					} 

					break;
				case 494: //Dye pack
					if (objectToDye.Object_Type == 33) //Leather
						canApply = true;
					break;
				case 495: //Dye pack
					if ((objectToDye.Object_Type == 42) // Shield
						|| (objectToDye.Object_Type == 34) // Studded
						|| (objectToDye.Object_Type == 35) // Chain
						|| (objectToDye.Object_Type == 36) // Plate
						|| (objectToDye.Object_Type == 37) // Reinforced
						|| (objectToDye.Object_Type == 38) // Scale
						|| ((objectToDye.Object_Type == 41) && (objectToDye.Item_Type == 7))) // horse saddle 
						canApply = true;
					break;
				case 538: //Dye pot
					//				        if((objectToDye.Object_Type==1) // generic (weapon)
					//						||(objectToDye.Object_Type==2) // crushing (weapon)
					//						||(objectToDye.Object_Type==3) // slashing (weapon)
					//						||(objectToDye.Object_Type==4) // thrusting (weapon)
					//						||(objectToDye.Object_Type==5) // fired (weapon)
					//						||(objectToDye.Object_Type==6) // twohanded (weapon)
					//						||(objectToDye.Object_Type==7) // polearm (weapon)
					//						||(objectToDye.Object_Type==8) // staff (weapon)
					//						||(objectToDye.Object_Type==9) // longbow (weapon)
					//						||(objectToDye.Object_Type==10) // crossbow (weapon)
					//						||(objectToDye.Object_Type==11) // sword (weapon)
					//						||(objectToDye.Object_Type==12) // hammer (weapon)
					//						||(objectToDye.Object_Type==13) // axe (weapon)
					//						||(objectToDye.Object_Type==14) // spear (weapon)
					//						||(objectToDye.Object_Type==15) // composite bow (weapon)
					//						||(objectToDye.Object_Type==16) // thrown (weapon)
					//						||(objectToDye.Object_Type==17) // left axe (weapon)
					//						||(objectToDye.Object_Type==18) // recurve bow (weapon)
					//						||(objectToDye.Object_Type==19) // blades (weapon)
					//						||(objectToDye.Object_Type==20) // blunt (weapon)
					//						||(objectToDye.Object_Type==21) // piercing (weapon)
					//						||(objectToDye.Object_Type==22) // large (weapon)
					//						||(objectToDye.Object_Type==23) // celtic spear (weapon)
					//						||(objectToDye.Object_Type==24) // flexible (weapon)
					//						||(objectToDye.Object_Type==25) // hand to hand (weapon)
					//						||(objectToDye.Object_Type==26)) // scythe (weapon)
					if (objectToDye.Object_Type >= 1 && objectToDye.Object_Type <= 26)
						canApply = true;
					break;
			}
			if (canApply == true)
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
			m_player.Out.SendInventorySlotsUpdate(m_changedSlots);

			bool statsUpdated = false;
			bool appearanceUpdated = false;
			bool encumberanceUpdated = false;

			foreach (int updatedSlot in m_changedSlots)
			{
				// update appearance if one of changed slots is visible
				if (!appearanceUpdated)
				{
					foreach (eInventorySlot visibleSlot in VISIBLE_SLOTS)
					{
						if (updatedSlot != (int)visibleSlot) continue;

						m_player.UpdateEquipmentAppearance();
						appearanceUpdated = true;
						break;
					}
				}

				// update stats if equipped item has changed
				if (!statsUpdated && updatedSlot <= (int)eInventorySlot.RightRing && updatedSlot >= (int)eInventorySlot.RightHandWeapon)
				{
					m_player.Out.SendUpdateWeaponAndArmorStats();
					statsUpdated = true;
				}

				// update encumberance if changed slot was in inventory or equipped
				if (!encumberanceUpdated &&
					//					(updatedSlot >=(int)eInventorySlot.FirstVault && updatedSlot<=(int)eInventorySlot.LastVault) ||
					(updatedSlot >= (int)eInventorySlot.RightHandWeapon && updatedSlot <= (int)eInventorySlot.RightRing) ||
					(updatedSlot >= (int)eInventorySlot.FirstBackpack && updatedSlot <= (int)eInventorySlot.LastBackpack))
				{
					m_player.UpdateEncumberance();
					encumberanceUpdated = true;
				}
			}

			base.UpdateChangedSlots();
		}

		#endregion
        #region Artifacts

        public ArrayList Artifacts = new ArrayList();

        #endregion
    }
}
