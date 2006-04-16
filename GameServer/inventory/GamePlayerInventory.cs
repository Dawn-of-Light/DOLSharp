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
		protected GamePlayer m_owner;

		/// <summary>
		/// Returns the GamePlayer that owns this inventory
		/// </summary>
		public GamePlayer Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}

		#endregion

		#region Add/Remove

		/// <summary>
		/// Adds a new item to the player inventory (only backpack possible)
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="itemBase"></param>
		/// <returns></returns>
		public override bool AddItem(eInventorySlot slot, GenericItemBase itemBase)
		{
			GenericItem item = itemBase as GenericItem;
			if(item == null) return false;
			
			lock (this)
			{
				if(item is IStackableItem)
				{
					IStackableItem fromItem = (IStackableItem)item;
					for(int i=(int)eInventorySlot.FirstBackpack ; i<=(int)eInventorySlot.LastBackpack ; i++)
					{
						IStackableItem toItem = m_items[i] as IStackableItem;
						if (toItem != null && toItem.CanStackWith(fromItem))
						{
							if (fromItem.Count + toItem.Count > toItem.MaxCount)
							{		
								fromItem.Count -= (toItem.MaxCount - toItem.Count);
								((GenericItem)fromItem).Weight = (int)(fromItem.Count * ((float)((GenericItem)toItem).Weight / toItem.Count));
								((GenericItem)fromItem).Value = (long)(fromItem.Count * ((float)((GenericItem)toItem).Value / toItem.Count));

								toItem.Count = toItem.MaxCount;
								((GenericItem)toItem).Weight = (int)(toItem.Count * ((float)((GenericItem)fromItem).Weight / fromItem.Count));
								((GenericItem)toItem).Value = (long)(toItem.Count * ((float)((GenericItem)fromItem).Value / fromItem.Count));

								if (!m_changedSlots.Contains(((GenericItem)toItem).SlotPosition))
									m_changedSlots.Add(((GenericItem)toItem).SlotPosition);
							}
							else
							{
								toItem.Count += fromItem.Count;
								((GenericItem)toItem).Weight += ((GenericItem)fromItem).Weight;
								((GenericItem)toItem).Value += ((GenericItem)fromItem).Value;

								fromItem.Count = 0;
								((GenericItem)fromItem).Weight = 0;
								((GenericItem)fromItem).Value = 0;

								if (!m_changedSlots.Contains(((GenericItem)toItem).SlotPosition))
									m_changedSlots.Add(((GenericItem)toItem).SlotPosition);
							}
						}
					}
				}

				if(!(item is IStackableItem) || (item is IStackableItem && ((IStackableItem)item).Count > 0))
				{
					slot = GetValidInventorySlot(slot);
					if (slot >= eInventorySlot.FirstBackpack && slot <= eInventorySlot.LastBackpack && !m_items.Contains((int)slot))
					{
						InventoryItems.Add((int)slot, item);
						item.Owner = Owner;
						item.SlotPosition=(int)slot;
						
						if (!m_changedSlots.Contains(item.SlotPosition))
							m_changedSlots.Add(item.SlotPosition);
					}
					else
					{
						eInventorySlot firstFree = FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
						if(slot == eInventorySlot.Invalid)
						{
							m_owner.CreateItemOnTheGround(item);
							m_owner.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "Your backpack is full. "+item.Name+" is created on the ground.");
                        }
						else
						{
							InventoryItems.Add((int)firstFree, item);
							item.Owner = Owner;
							item.SlotPosition=(int)firstFree;
							
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
		/// <param name="itemBase">the item to remove</param>
		/// <returns>true if successfull</returns>
		public override bool RemoveItem(GenericItemBase itemBase)
		{
			GenericItem item = itemBase as GenericItem;
			if (item == null) return false;
			if (item.Owner != Owner)
			{
				if (log.IsErrorEnabled)
					log.Error(m_owner.Name + ": PlayerInventory -> tried to remove item from another inventory\n\n" + Environment.StackTrace);
				return false;
			}
			if(!m_items.Contains(item.SlotPosition))
			{
				if (log.IsErrorEnabled)
					log.Error(m_owner.Name + ": PlayerInventory -> tried to remove a item in a free slot ???\n\n" + Environment.StackTrace);
				return false;
			}

			int oldSlot = item.SlotPosition;
			lock (this)
			{
				InventoryItems.Remove(item.SlotPosition);
				item.Owner = null;
				item.SlotPosition = (int)eInventorySlot.Invalid;
				
				if (!m_changedSlots.Contains(oldSlot))
					m_changedSlots.Add(oldSlot);

				if (m_changesCounter <= 0)
					UpdateChangedSlots();
			}

			ITradeWindow window = m_owner.TradeWindow;
			if (window != null) window.RemoveItemToTrade(item);

			if (oldSlot >= (int)eInventorySlot.RightHandWeapon && oldSlot <= (int)eInventorySlot.DistanceWeapon)
			{
				// if active weapon was destroyed
				if (m_owner.AttackWeapon == null)
					m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
				else
					((EquipableItem)item).OnItemUnequipped(oldSlot);
			}
			else if (oldSlot >= (int)eInventorySlot.FirstQuiver && oldSlot <= (int)eInventorySlot.FourthQuiver)
			{
				m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
			}
			else if (IsEquippedSlot((eInventorySlot)oldSlot))
			{
				((EquipableItem)item).OnItemUnequipped(oldSlot);
			}

			return true;
		}

		/// <summary>
		/// Removes one item from the inventory item
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <param name="count">the count of items to be removed from the stack</param>
		/// <returns>true one item removed</returns>
		public virtual bool RemoveCountFromStack(IStackableItem item, int count)
		{
			if (item == null || ((GenericItem)item).Owner != Owner)
			{
				if (log.IsErrorEnabled)
					log.Error("Item inventory not equals inventory owner.\n\n" + Environment.StackTrace);
				return false;
			}
			if (count <= 0) return false;
			
			lock(this)
			{
				if (m_items.Contains(((GenericItem)item).SlotPosition))
				{
					if(item.Count < count) return false;
					int itemSlot = ((GenericItem)item).SlotPosition;
					if(item.Count == count)
					{
						goto remove_item;
					}
					else
					{
						int oldCount = item.Count;
						item.Count -= count;
						((GenericItem)item).Weight = (int)(item.Count * ((float)((GenericItem)item).Weight / oldCount));
						((GenericItem)item).Value = (long)(item.Count * ((float)((GenericItem)item).Value / oldCount));

						if (!m_changedSlots.Contains(itemSlot))
							m_changedSlots.Add(itemSlot);
						if (m_changesCounter <= 0)
							UpdateChangedSlots();
						return true;
					}
				}
				return false;
			}

			remove_item:
				return RemoveItem((GenericItem)item);
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

			if(	   ( slot == eInventorySlot.Ground)
				|| ( slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.FourthQuiver)
				|| ( slot >= eInventorySlot.HeadArmor && slot <= eInventorySlot.Neck)
				|| ( slot >= eInventorySlot.Waist && slot <= eInventorySlot.RightRing)   
				|| ( slot >= eInventorySlot.FirstBackpack && slot <= eInventorySlot.LastBackpack )
				|| ( slot >= eInventorySlot.FirstVault && slot <= eInventorySlot.LastVault )
				|| ( slot == eInventorySlot.PlayerPaperDoll )
				|| ( slot == eInventorySlot.PlayerPaperDoll178 ))
				return slot;

		
			return eInventorySlot.Invalid;
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
		public virtual bool MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			if(! m_owner.Alive)
			{
				m_owner.Out.SendMessage("You can't change your inventory when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_owner.Out.SendInventorySlotsUpdate(null);
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
						if(toSlot >= eInventorySlot.FirstQuiver && toSlot <= eInventorySlot.FourthQuiver && !(fromItem is Ammunition))
						{
							valid = false;
							m_owner.Out.SendMessage("You can't put your "+ fromItem.Name+" in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else if(!(fromItem is EquipableItem)) // in all other equipment slot only EquipableItem must be used
						{
							valid = false;
							m_owner.Out.SendMessage(fromItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
								m_owner.Out.SendMessage(fromItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								valid = false;
							}
							else if(!Owner.HasAbilityToUseItem((EquipableItem)fromItem)) // now we check if the player has the ability to use the item
							{
								if(fromItem is Weapon)
								{
									m_owner.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(fromItem is Armor)
								{
									m_owner.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(fromItem is Instrument)
								{
									m_owner.Out.SendMessage("The "+fromItem.Name+" must be readied in the 2-handed slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									m_owner.Out.SendMessage("You have no skill in using this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
						if(fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver && !(toItem is Ammunition))
						{
							valid = false;
							m_owner.Out.SendMessage("You can't put your "+ toItem.Name+" in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else if(!(toItem is EquipableItem)) // in all other equipment slot only EquipableItem must be used
						{
							valid = false;
							m_owner.Out.SendMessage(toItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
								m_owner.Out.SendMessage(toItem.Name + " can't go there!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								valid = false;
							}
							else if(!Owner.HasAbilityToUseItem((EquipableItem)toItem)) // now we check if the player has the ability to use the item
							{
								if(toItem is Weapon)
								{
									m_owner.Out.SendMessage("You have no skill in using this weapon type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(toItem is Armor)
								{
									m_owner.Out.SendMessage("You have no skill in wearing this armor type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(toItem is Instrument)
								{
									m_owner.Out.SendMessage("The "+fromItem.Name+" must be readied in the 2-handed slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									m_owner.Out.SendMessage("You have no skill in using this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								valid = false;
							}
						}
					}
				}

				if (valid == true)
				{
					if (!CombineItems(fromSlot, toSlot) && !StackItems(fromSlot, toSlot, itemCount)) 
					{
						ExchangeItems(fromSlot, toSlot);
					}

					if (!m_changedSlots.Contains((int)fromSlot))
						m_changedSlots.Add((int)fromSlot);
					if (!m_changedSlots.Contains((int)toSlot))
						m_changedSlots.Add((int)toSlot);

					if (m_changesCounter <= 0)
						UpdateChangedSlots();

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
						Owner.StopAttack();
						break;
					}
				}

				ITradeWindow window = m_owner.TradeWindow;
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
					case eInventorySlot.RightHandWeapon:	m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); break;
					case eInventorySlot.TwoHandWeapon:		m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); break;
					case eInventorySlot.DistanceWeapon:     m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); break;
					case eInventorySlot.LeftHandWeapon:
						if (m_owner.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
							m_owner.SwitchWeapon(m_owner.ActiveWeaponSlot);
						else m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case eInventorySlot.FirstQuiver  : m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.First, true);break;
					case eInventorySlot.SecondQuiver : m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Second, true);break;
					case eInventorySlot.ThirdQuiver  : m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Third, true);break;
					case eInventorySlot.FourthQuiver : m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.Fourth, true);break;


					default:
						// change active weapon if moved from active slot
						if (fromSlot == eInventorySlot.RightHandWeapon && m_owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard)
							m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						else if (fromSlot == eInventorySlot.TwoHandWeapon && m_owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded)
							m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						else if (fromSlot == eInventorySlot.DistanceWeapon && m_owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
							m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						else if (fromSlot == eInventorySlot.LeftHandWeapon && (m_owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded || m_owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Standard))
							m_owner.SwitchWeapon(m_owner.ActiveWeaponSlot);
						if(fromSlot >= eInventorySlot.FirstQuiver && fromSlot <= eInventorySlot.FourthQuiver)
							m_owner.SwitchQuiver(GamePlayer.eActiveQuiverSlot.None, true);
						break;
				}
			}

			m_owner.Out.SendInventorySlotsUpdate(null);

			return valid;

			apply_poison:
			m_owner.ApplyPoison((Poison)fromItem, (Weapon)toItem);

			m_owner.Out.SendInventorySlotsUpdate(null);

			return valid;

			// moved outside of the lock
			switch_weapon_standard:  m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard); return false;
			switch_weapon_twohanded: m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded); return false;
			switch_weapon_distance:  m_owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance); return false;
		}
		#endregion Move Item

		#region Combine/Exchange/Stack Items

		/// <summary>
		/// Combine 2 items together if possible
		/// </summary>
		/// <param name="fromSlot">First Item</param>
		/// <param name="toSlot">Second Item</param>
		/// <returns>true if items combined successfully</returns>
		protected virtual bool CombineItems(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			VisibleEquipment toItem = m_items[(int)toSlot] as VisibleEquipment;
			Dye fromItem = m_items[(int)fromSlot] as Dye;
			
			if (toItem == null || fromItem == null || fromSlot < eInventorySlot.FirstBackpack || fromSlot > eInventorySlot.LastBackpack)
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
		protected virtual bool StackItems(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			if(  toSlot < eInventorySlot.FirstQuiver 
				||(toSlot > eInventorySlot.FourthQuiver && toSlot < eInventorySlot.FirstBackpack) 
				||(toSlot > eInventorySlot.LastBackpack && toSlot < eInventorySlot.FirstVault) 
				|| toSlot > eInventorySlot.LastVault)
				return false;

			IStackableItem fromItem = m_items[(int)fromSlot] as IStackableItem;
			if(fromItem == null) return false;

			if(itemCount <= 0) itemCount = Math.Max(fromItem.Count, 1);			
		

			IStackableItem toItem = m_items[(int)toSlot] as IStackableItem;
			if(toItem != null && toItem.CanStackWith(fromItem))
			{
				if (fromItem.Count + toItem.Count > fromItem.MaxCount)
				{
					fromItem.Count -= (toItem.MaxCount - toItem.Count);
					((GenericItem)fromItem).Weight = (int)(fromItem.Count * ((float)((GenericItem)toItem).Weight / toItem.Count));
					((GenericItem)fromItem).Value = (long)(fromItem.Count * ((float)((GenericItem)toItem).Value / toItem.Count));

					toItem.Count = toItem.MaxCount;
					((GenericItem)toItem).Weight = (int)(toItem.Count * ((float)((GenericItem)fromItem).Weight / fromItem.Count));
					((GenericItem)toItem).Value = (long)(toItem.Count * ((float)((GenericItem)fromItem).Value / fromItem.Count));
				}
				else
				{
					toItem.Count += fromItem.Count;
					((GenericItem)toItem).Weight += ((GenericItem)fromItem).Weight;
					((GenericItem)toItem).Value += ((GenericItem)fromItem).Value;

					RemoveItem((GenericItem)fromItem);
				}
				return true;
			}
			else if (toItem == null && fromItem.Count > itemCount)
			{
				IStackableItem newItem = fromItem.ShallowCopy();
				newItem.Count = itemCount;
				((GenericItem)newItem).Weight = (int)(newItem.Count * ((float)((GenericItem)fromItem).Weight / fromItem.Count));
				((GenericItem)newItem).Value = (long)(newItem.Count * ((float)((GenericItem)fromItem).Value / fromItem.Count));

				fromItem.Count -= itemCount;
				((GenericItem)fromItem).Weight = (int)(fromItem.Count * ((float)((GenericItem)newItem).Weight / newItem.Count));
				((GenericItem)fromItem).Value = (long)(fromItem.Count * ((float)((GenericItem)newItem).Value / newItem.Count));

				((GenericItem)newItem).SlotPosition = (int)toSlot;
				InventoryItems.Add((int)toSlot, newItem);

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
		protected virtual bool ExchangeItems(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			GenericItem fromItem = (GenericItem)m_items[(int)fromSlot];
			GenericItem toItem = (GenericItem)m_items[(int)toSlot];

//			log.DebugFormat("exchange slot from:{0} to:{1}; same items? {2}", fromSlot, toSlot, fromItem==toItem&&fromItem!=null);

			m_items[(int)fromSlot]=toItem;
			m_items[(int)toSlot]=fromItem;
			
			if (toItem != null)
				toItem.SlotPosition = (int)fromSlot;
			else
				m_items.Remove((int)fromSlot);

			if (fromItem != null)
				fromItem.SlotPosition = (int)toSlot;
			else
				m_items.Remove((int)toSlot);


			bool fromSlotEquipped = IsEquippedSlot(fromSlot);
			bool toSlotEquipped = IsEquippedSlot(toSlot);

			// notify handlers if items changing state
			if (fromSlotEquipped != toSlotEquipped)
			{
				if (toItem != null)
				{
					if (toSlotEquipped) // item was equipped
						((EquipableItem)toItem).OnItemUnequipped((int)fromSlot);
					else
						((EquipableItem)toItem).OnItemEquipped();
				}

				if (fromItem != null)
				{
					if (fromSlotEquipped) // item was equipped
						((EquipableItem)fromItem).OnItemUnequipped((int)toSlot);
					else
						((EquipableItem)fromItem).OnItemEquipped();
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
			switch (slot)
			{
				case eInventorySlot.RightHandWeapon : return (m_owner.VisibleActiveWeaponSlots & 0x0F) == 0x00;
				case eInventorySlot.LeftHandWeapon  : return (m_owner.VisibleActiveWeaponSlots & 0xF0) == 0x10;
				case eInventorySlot.TwoHandWeapon   : return (m_owner.VisibleActiveWeaponSlots & 0x0F) == 0x02;
				case eInventorySlot.DistanceWeapon  : return (m_owner.VisibleActiveWeaponSlots & 0x0F) == 0x03;
				default :
					{
						foreach(eInventorySlot staticSlot in EQUIP_SLOTS)
						{
							if(slot == staticSlot)
								return true;
						}
					}
					break;
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
					return weight/10 + base.InventoryWeight;
				}
			}
		}
		#endregion

		#region IsCloakHoodUp
		/// <summary>
		/// Sets/gets the living's cloak hood state
		/// </summary>
		public override bool IsCloakHoodUp
		{
			get { return base.IsCloakHoodUp; }
			set
			{
				base.IsCloakHoodUp = value;
				
				if(m_owner.ObjectState == eObjectState.Active)
				{
					m_owner.Out.SendInventorySlotsUpdate(null);
					m_owner.UpdateEquipementAppearance();

					if(value)
					{
						m_owner.Out.SendMessage("You will now wear your hood up.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						m_owner.Out.SendMessage("You will no longer wear your hood up.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}
		#endregion
		
		#region BeginChanges/CommitChanges/UpdateChangedSlots

		/// <summary>
		/// Holds the begin changes counter for slot updates
		/// </summary>
		protected int m_changesCounter;

		/// <summary>
		/// Holds all changed slots
		/// </summary>
		protected ArrayList m_changedSlots = new ArrayList(1);

		/// <summary>
		/// Increments changes counter
		/// </summary>
		public void BeginChanges()
		{
			lock (this)
			{
				m_changesCounter++;
			}
		}

		/// <summary>
		/// Commits changes if all started changes are finished
		/// </summary>
		public void CommitChanges()
		{
			lock (this)
			{
				m_changesCounter--;
				if (m_changesCounter < 0)
				{
					if (log.IsErrorEnabled)
						log.Error("Inventory changes counter is bellow zero (forgot to use BeginChanges?)!\n\n" + Environment.StackTrace);
					m_changesCounter = 0;
				}
				if (m_changesCounter <= 0 && m_changedSlots.Count > 0)
				{
					UpdateChangedSlots();
				}
			}
		}

		/// <summary>
		/// Updates changed slots, inventory is already locked.
		/// Inventory must be locked before invoking this method.
		/// </summary>
		protected virtual void UpdateChangedSlots()
		{
			if(m_owner.ObjectState == eObjectState.Active)
			{
				m_owner.Out.SendInventorySlotsUpdate(m_changedSlots);

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

							m_owner.UpdateEquipementAppearance();
							appearanceUpdated = true;
							break;
						}
					}

					// update stats if equipped item has changed
					if (!statsUpdated && updatedSlot <= (int)eInventorySlot.RightRing && updatedSlot >= (int)eInventorySlot.RightHandWeapon)
					{
						m_owner.Out.SendUpdateWeaponAndArmorStats();
						statsUpdated = true;
					}

					// update encumberance if changed slot was in inventory or equipped
					if (!encumberanceUpdated &&
						(updatedSlot >=(int)eInventorySlot.RightHandWeapon && updatedSlot<=(int)eInventorySlot.RightRing) ||
						(updatedSlot >=(int)eInventorySlot.FirstBackpack && updatedSlot<=(int)eInventorySlot.LastBackpack))
					{
						m_owner.UpdateEncumberance();
						encumberanceUpdated = true;
					}
				}
			}

			m_changedSlots.Clear();
		}

		#endregion
	}
}
