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
using System.Collections.Specialized;
using System.Reflection;
using DOL.GS.Database;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Events;
using NHibernate.Expression;
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

		#region Declaration/LoadDatabase

		/// <summary>
		/// Holds the player that owns
		/// this inventory
		/// </summary>
		protected GamePlayer m_player;

		/// <summary>
		/// Returns the GamePlayer that owns this inventory
		/// </summary>
		public GamePlayer Player
		{
			get { return m_player; }
			set { m_player = value; }
		}

		/// <summary>
		/// Loads the inventory from the DataBase
		/// </summary>
		/// <param name="inventoryID">The inventory ID</param>
		/// <returns>success</returns>
		public override bool LoadFromDatabase(string inventoryID)
		{
			lock (this)
			{
				try
				{
					// notify handlers that the item was just equipped
					foreach(eInventorySlot slot in EQUIP_SLOTS )
					{
						// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
						if (slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.DistanceWeapon) continue;
						if (m_items[(int)slot] != null)
							Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs((EquipableItem)m_items[(int)slot], (int)slot));
					}
					return true;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Loading player inventory ("+m_player.Name+")",e);
					return false;
				}
			}
		}
		
		#endregion

		#region Add/Remove

		/// <summary>
		/// Adds a new item to the player inventory (only backpack possible)
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool AddItem(eInventorySlot slot, GenericItem item)
		{
			if(item == null) return false;

			lock (this)
			{
				if(item is StackableItem)
				{
					StackableItem fromItem = (StackableItem)item;
					for(int i=(int)eInventorySlot.FirstBackpack ; i<=(int)eInventorySlot.LastBackpack ; i++)
					{
						StackableItem toItem = m_items[i] as StackableItem;
						if (toItem != null && toItem.CanStackWith(fromItem))
						{
							if (fromItem.Count + toItem.Count > toItem.MaxCount)
							{		
								fromItem.Count -= (toItem.MaxCount - toItem.Count);
								toItem.Count = toItem.MaxCount;

								if (!m_changedSlots.Contains(toItem.SlotPosition))
									m_changedSlots.Add(toItem.SlotPosition);
							}
							else
							{
								toItem.Count += fromItem.Count;
								fromItem.Count = 0;

								if (!m_changedSlots.Contains(toItem.SlotPosition))
									m_changedSlots.Add(toItem.SlotPosition);
							}
						}
					}
				}

				if(!(item is StackableItem) || (item is StackableItem && ((StackableItem)item).Count > 0))
				{
					slot = GetValidInventorySlot(slot);
					if (slot >= eInventorySlot.FirstBackpack && slot <= eInventorySlot.LastBackpack && !m_items.Contains((int)slot))
					{
						m_items.Add((int)slot, item);
						item.SlotPosition=(int)slot;
						item.Owner = m_player;

						if (!m_changedSlots.Contains(item.SlotPosition))
							m_changedSlots.Add(item.SlotPosition);
					}
					else
					{
						eInventorySlot firstFree = FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
						if(slot == eInventorySlot.Invalid)
						{
							m_player.CreateItemOnTheGround(item);
							m_player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "Your backpack is full. "+item.Name+" is created on the ground.");
                        }
						else
						{
							m_items.Add((int)firstFree, item);
							item.SlotPosition=(int)firstFree;
							item.Owner = m_player;

							if (!m_changedSlots.Contains(item.SlotPosition))
								m_changedSlots.Add(item.SlotPosition);
						}
					}
				}

				if (m_changesCounter <= 0)
					UpdateChangedSlots();
			}
			return true;
		}

		/// <summary>
		/// Removes an item from the inventory and DB
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		public override bool RemoveItem(GenericItem item)
		{
			if (item == null) return false;
			int oldSlot;

			lock (this)
			{
				if (item.Owner != m_player)
				{
					if (log.IsErrorEnabled)
						log.Error(m_player.Name + ": PlayerInventory -> tried to remove item with wrong owner ("+item.Owner.CharacterID+")\n\n" + Environment.StackTrace);
					return false;
				}

				oldSlot = item.SlotPosition;

				if (!base.RemoveItem(item)) return false;

				if(item.ItemID != 0) GameServer.Database.DeleteObject(item); // delete the item in the database only if it have already been saved
			}

			ITradeWindow window = m_player.TradeWindow;
			if (window != null)
				window.RemoveItemToTrade(item);

			if (oldSlot >= (int)eInventorySlot.RightHandWeapon && oldSlot <= (int)eInventorySlot.DistanceWeapon)
			{
				// if active weapon was destroyed
				if (m_player.AttackWeapon == null)
					m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
				else
					Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs((EquipableItem)item, oldSlot));
			}
			else if (oldSlot >= (int)eInventorySlot.FirstQuiver && oldSlot <= (int)eInventorySlot.FourthQuiver)
			{
				m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
			}
			else if (IsEquippedSlot((eInventorySlot)oldSlot))
			{
				Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs((EquipableItem)item, oldSlot));
			}

			return true;
		}

		/// <summary>
		/// Removes one item from the inventory item
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <param name="count">the count of items to be removed from the stack</param>
		/// <returns>true one item removed</returns>
		public override bool RemoveCountFromStack(StackableItem item, int count)
		{
			if (item != null && item.Owner != m_player)
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
			switch(slot)
			{
				case eInventorySlot.LastEmptyQuiver     : slot = FindLastEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver); break;
				case eInventorySlot.FirstEmptyQuiver    : slot = FindFirstEmptySlot(eInventorySlot.FirstQuiver, eInventorySlot.FourthQuiver); break;
				case eInventorySlot.LastEmptyVault      : slot = FindLastEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault); break;
				case eInventorySlot.FirstEmptyVault     : slot = FindFirstEmptySlot(eInventorySlot.FirstVault, eInventorySlot.LastVault); break;
				case eInventorySlot.LastEmptyBackpack   : slot = FindLastEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack); break;
				case eInventorySlot.FirstEmptyBackpack  : slot = FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack); break;
			}

			if(    ( slot >= eInventorySlot.FirstBackpack && slot <= eInventorySlot.LastBackpack )
//				|| ( slot >= eInventorySlot.Mithril && slot <= eInventorySlot.Copper ) // can't place items in money slots, is it?
				|| ( slot >= eInventorySlot.FirstVault && slot <= eInventorySlot.LastVault )
				|| ( slot == eInventorySlot.PlayerPaperDoll ))
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
			if(! m_player.Alive)
			{
				m_player.Out.SendMessage("You can't change your inventory when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_player.Out.SendInventorySlotsUpdate(null);
				return false;
			}

			bool valid=true;
			GenericItem fromItem, toItem;
			int[] updatedSlots;
	
			lock (this)
			{
				fromSlot = GetValidInventorySlot(fromSlot);
				toSlot = GetValidInventorySlot(toSlot);
				if (fromSlot == eInventorySlot.Invalid || toSlot == eInventorySlot.Invalid)
					return false;

				// just change active weapon if placed in same slot
				if (fromSlot == toSlot)
				{
					switch(toSlot)
					{
						case eInventorySlot.RightHandWeapon:
						case eInventorySlot.LeftHandWeapon:	goto switch_weapon_standard;
						case eInventorySlot.TwoHandWeapon:	goto switch_weapon_twohanded;
						case eInventorySlot.DistanceWeapon:	goto switch_weapon_distance;
					}
				}

				fromItem = (GenericItem)m_items[(int)fromSlot];
				toItem = (GenericItem)m_items[(int)toSlot];

				updatedSlots=new int[2];
				updatedSlots[0]=(int)fromSlot;
				updatedSlots[1]=(int)toSlot;
				if (fromItem == toItem || fromItem == null)
					valid = false;

				if (valid && fromItem is Poison && toItem != null && toItem is Weapon)
				{
					valid = false;
					goto apply_poison;
				}

				if (valid==true)
				{
					// Check if toSlot is a equipment slot
					bool toSlotIsEquipSlot = false;
					foreach(eInventorySlot staticSlot in EQUIP_SLOTS)
					{
						if(toSlot == staticSlot)
						{
							toSlotIsEquipSlot = true;
							break;
						}
					}

					if(toSlotIsEquipSlot)
					{
						// Only Arrow and bolt can be put in the quiver
						if(toSlot >= eInventorySlot.FirstQuiver && toSlot <= eInventorySlot.FourthQuiver && !(fromItem is Arrow) && !(fromItem is Bolt))
						{
							valid = false;
							m_player.Out.SendMessage("You can't put your "+ fromItem.Name+" in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else if(!(fromItem is EquipableItem)) // in all other equipment slot only EquipableItem must be used
						{
							valid = false;
							m_player.Out.SendMessage(fromItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else // now we check if the slot you want to move fromItem to is in the array of all fromItem equipable slot
						{
							bool toSlotInEquipableSlot = false;
							foreach(eInventorySlot slot in ((EquipableItem)fromItem).EquipableSlot)
							{
								if(toSlot == slot)
								{
									toSlotInEquipableSlot = true;
									break;
								}
							}

							if(toSlotInEquipableSlot == false)
							{
								m_player.Out.SendMessage(fromItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								valid = false;
							}
							else if(!Player.HasAbilityToUseItem((EquipableItem)fromItem)) // now we check if the player has the ability to use the item
							{
								if(fromItem is Weapon)
								{
									m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(fromItem is Armor)
								{
									m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(fromItem is Instrument)
								{
									m_player.Out.SendMessage("The "+fromItem.Name+" must be readied in the 2-handed slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									m_player.Out.SendMessage("You have no skill in using this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								valid=false;
							}
						}
					}
				}
					
				if (valid == true && toItem != null)
				{

					// Check if fromSlot is a equipment slot
					bool fromSlotIsEquipSlot = false;
					foreach(eInventorySlot staticSlot in EQUIP_SLOTS)
					{
						if(toSlot == staticSlot)
						{
							fromSlotIsEquipSlot = true;
							break;
						}
					}

					if(fromSlotIsEquipSlot)
					{
						// Only Arrow and bolt can be put in the quiver
						if(fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver && !(toItem is Arrow) && !(toItem is Bolt))
						{
							valid = false;
							m_player.Out.SendMessage("You can't put your "+ toItem.Name+" in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else if(!(toItem is EquipableItem)) // in all other equipment slot only EquipableItem must be used
						{
							valid = false;
							m_player.Out.SendMessage(toItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else // now we check if the slot you want to move fromItem to is in the array of all fromItem equipable slot
						{
							bool fromSlotInEquipableSlot = false;
							foreach(eInventorySlot slot in ((EquipableItem)toItem).EquipableSlot)
							{
								if(toSlot == slot)
								{
									fromSlotInEquipableSlot = true;
									break;
								}
							}

							if(fromSlotInEquipableSlot == false)
							{
								m_player.Out.SendMessage(toItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								valid = false;
							}
							else if(!Player.HasAbilityToUseItem((EquipableItem)toItem)) // now we check if the player has the ability to use the item
							{
								if(toItem is Weapon)
								{
									m_player.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(toItem is Armor)
								{
									m_player.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(toItem is Instrument)
								{
									m_player.Out.SendMessage("The "+fromItem.Name+" must be readied in the 2-handed slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									m_player.Out.SendMessage("You have no skill in using this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								valid = false;
							}
						}
					}
				}

				if (valid == true)
				{
					base.MoveItem(fromSlot, toSlot, itemCount);
					fromItem = (GenericItem)m_items[(int)fromSlot];
					toItem	 = (GenericItem)m_items[(int)toSlot];
				}
			}

			if (valid == true)
			{
				foreach(int updatedSlot in updatedSlots)
				{
					if((updatedSlot >= (int)eInventorySlot.RightHandWeapon && updatedSlot<=(int)eInventorySlot.DistanceWeapon)
						||(updatedSlot >= (int)eInventorySlot.FirstQuiver && updatedSlot <= (int)eInventorySlot.FourthQuiver))
					{
						Player.StopAttack();
						break;
					}
				}

				ITradeWindow window = m_player.TradeWindow;
				if (window != null)
				{
					if (toItem!=null)
						window.RemoveItemToTrade(toItem);
					if (fromItem!=null)
						window.RemoveItemToTrade(fromItem);
				}


				// activate weapon slot if moved to it
				switch(toSlot)
				{
					case eInventorySlot.RightHandWeapon:	m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); break;
					case eInventorySlot.TwoHandWeapon:		m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); break;
					case eInventorySlot.DistanceWeapon:     m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); break;
					case eInventorySlot.LeftHandWeapon:
						if (m_player.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
							m_player.SwitchWeapon(m_player.ActiveWeaponSlot);
						else m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case eInventorySlot.FirstQuiver  : m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.First, true);break;
					case eInventorySlot.SecondQuiver : m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Second, true);break;
					case eInventorySlot.ThirdQuiver  : m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Third, true);break;
					case eInventorySlot.FourthQuiver : m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Fourth, true);break;


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
						if(fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver)
							m_player.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
						break;
				}
			}

			m_player.Out.SendInventorySlotsUpdate(null);

			return valid;

			apply_poison:
				m_player.ApplyPoison((Poison)fromItem, (Weapon)toItem);

			m_player.Out.SendInventorySlotsUpdate(null);

			return valid;

			// moved outside of the lock
			switch_weapon_standard:  m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); return false;
			switch_weapon_twohanded: m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); return false;
			switch_weapon_distance:  m_player.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); return false;
		}
		#endregion Move Item

		#region Combine/Exchange/Stack Items

		/// <summary>
		/// Combine 2 items together if possible
		/// </summary>
		/// <param name="fromSlot">First Item</param>
		/// <param name="toSlot">Second Item</param>
		/// <returns>true if items combined successfully</returns>
		protected override bool CombineItems(int fromSlot,int toSlot)
		{
			VisibleEquipment toItem = m_items[(int)toSlot] as VisibleEquipment;
			Dye fromItem = m_items[(int)fromSlot] as Dye;
			
			if (toItem == null || fromItem == null || fromSlot < (int)eInventorySlot.FirstBackpack || fromSlot > (int)eInventorySlot.LastBackpack )
				return false;

			bool canApplyDye = false;
			if(fromItem is ClothDye)
			{
				if(toItem is Cloak || (toItem is Armor && ((Armor)toItem).ArmorLevel == eArmorLevel.VeryLow))
				{
					canApplyDye = true;
				}
			}
			else if(fromItem is LeatherDye)
			{
				if((toItem is Armor && ((Armor)toItem).ArmorLevel == eArmorLevel.Low))
				{
					canApplyDye = true;
				}
			}
			else if(fromItem is Enamel)
			{
				if(toItem is Shield || toItem is Armor)
				{
					canApplyDye = true;
				}
			}
			else if(fromItem is WeaponLuster)
			{	
				if(toItem is Weapon)
				{
					canApplyDye = true;
				}
			}
			
			if (canApplyDye)
			{
				toItem.Color = fromItem.Color;
				RemoveItem(fromItem);
			}
			
			return canApplyDye;
		}

		/// <summary>
		/// Stack an item with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <param name="itemCount">How many items to move</param>
		/// <returns>true if items stacked successfully</returns>
		protected override bool StackItems(int fromSlot,int toSlot, int itemCount)
		{
			if(  toSlot < (int)eInventorySlot.FirstQuiver 
				||(toSlot > (int)eInventorySlot.FourthQuiver && toSlot < (int)eInventorySlot.FirstBackpack) 
				||(toSlot > (int)eInventorySlot.LastBackpack && toSlot < (int)eInventorySlot.FirstVault) 
				|| toSlot > (int)eInventorySlot.LastVault)
				return false;

			StackableItem fromItem = m_items[fromSlot] as StackableItem;
			if(fromItem == null) return false;

			if(itemCount <= 0) itemCount = Math.Max(fromItem.Count,1);			
		

			StackableItem toItem = m_items[toSlot] as StackableItem;
			if(toItem != null && toItem.CanStackWith(fromItem))
			{
				if (fromItem.Count + toItem.Count > fromItem.MaxCount)
				{		
					fromItem.Count -= (toItem.MaxCount-toItem.Count);
					toItem.Count = toItem.MaxCount;
				}
				else
				{
					toItem.Count+=fromItem.Count;
					RemoveItem(fromItem);
				}
				return true;
			}
			else if (toItem == null && fromItem.Count > itemCount)
			{
				StackableItem newItem = fromItem.ShallowCopy();
				newItem.ItemID = 0; //tag this item as unsaved
				newItem.Count = itemCount;
				newItem.SlotPosition = toSlot;
				m_items[toSlot] = newItem;
				
				fromItem.Count -= itemCount;

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
			GenericItem fromItem = (GenericItem)m_items[fromSlot];
			GenericItem toItem = (GenericItem)m_items[toSlot];

//			log.DebugFormat("exchange slot from:{0} to:{1}; same items? {2}", fromSlot, toSlot, fromItem==toItem&&fromItem!=null);

			bool fromSlotEquipped = IsEquippedSlot((eInventorySlot)fromSlot);
			bool toSlotEquipped = IsEquippedSlot((eInventorySlot)toSlot);

			base.ExchangeItems(fromSlot, toSlot);

	//		if (fromItem != null)
	//			GameServer.Database.SaveObject(fromItem);
	//		if (toItem != null && toItem != fromItem)
	//			GameServer.Database.SaveObject(toItem);

			// notify handlers if items changing state
			if (fromSlotEquipped != toSlotEquipped)
			{
				if (toItem != null)
				{
					if (toSlotEquipped) // item was equipped
						Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs((EquipableItem)toItem, fromSlot));
					else Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs((EquipableItem)toItem, fromSlot));
				}

				if (fromItem != null)
				{
					if (fromSlotEquipped) // item was equipped
						Player.Notify(PlayerInventoryEvent.ItemUnequipped, this, new ItemUnequippedArgs((EquipableItem)fromItem, toSlot));
					else Player.Notify(PlayerInventoryEvent.ItemEquipped, this, new ItemEquippedArgs((EquipableItem)fromItem, toSlot));
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if slot is equipped on player
		/// </summary>
		/// <param name="slot">The slot to check</param>
		/// <returns>true if slot is one of equipment slots and should add magical bonuses</returns>
		protected virtual bool IsEquippedSlot(eInventorySlot slot)
		{
			// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
			if (slot > eInventorySlot.DistanceWeapon || slot < eInventorySlot.RightHandWeapon)
			{
				foreach(eInventorySlot staticSlot in EQUIP_SLOTS)
				{
					if(slot == staticSlot)
						return true;
				}
				return false;
			}

			switch (slot)
			{
				case eInventorySlot.RightHandWeapon : return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x00;
				case eInventorySlot.LeftHandWeapon  : return (m_player.VisibleActiveWeaponSlots & 0xF0) == 0x10;
				case eInventorySlot.TwoHandWeapon   : return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x02;
				case eInventorySlot.DistanceWeapon  : return (m_player.VisibleActiveWeaponSlots & 0x0F) == 0x03;
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
				int weight= 0;
				lock (this)
				{
					for (int slot = (int)eInventorySlot.FirstBackpack; slot <= (int)eInventorySlot.LastBackpack; slot++)
					{
						GenericItem item = m_items[slot] as GenericItem;
						if (item != null)
						{
							weight += item.Weight;
						}
					}
					return weight/10+base.InventoryWeight;
				}
			}
		}
		#endregion
		
		#region UpdateChangedSlots

		/// <summary>
		/// Updates changed slots, inventory is already locked.
		/// Inventory must be locked before invoking this method.
		/// </summary>
		protected override void UpdateChangedSlots()
		{
			if(m_player.ObjectState == GameObject.eObjectState.Active)
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

							m_player.UpdateEquipementAppearance();
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
						(updatedSlot >=(int)eInventorySlot.RightHandWeapon && updatedSlot<=(int)eInventorySlot.RightRing) ||
						(updatedSlot >=(int)eInventorySlot.FirstBackpack && updatedSlot<=(int)eInventorySlot.LastBackpack))
					{
						m_player.UpdateEncumberance();
						encumberanceUpdated = true;
					}
				}
			}

			base.UpdateChangedSlots();
		}

		#endregion
	}
}
