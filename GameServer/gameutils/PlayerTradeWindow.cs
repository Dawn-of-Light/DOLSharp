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
using System.Reflection;
using DOL.Database;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// TradeWindow is the object for tradewindow from the side of one trader 
	/// with all his money and items but nothing of other trader
	/// </summary>
	public class PlayerTradeWindow : ITradeWindow
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public PlayerTradeWindow(GamePlayer owner, bool isRecipiantWindow, object sync)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			
			m_owner = owner;
			m_tradeAccept = false;
			m_tradeItems = new ArrayList(10);
			m_tradeMoney = 0;
			m_combine = false;
			m_repair = false;
			m_recipiant = isRecipiantWindow;
			m_sync = sync;
		}

		#region Fields

		/// <summary>
		/// Holds a list of tradeitems we offer to the other player
		/// </summary>
		protected ArrayList m_tradeItems;
		/// <summary>
		/// Holds money we offer to the other player
		/// </summary>
		protected long	m_tradeMoney;
		/// <summary>
		/// Holds if we have accepted the trade or not
		/// </summary>
		protected bool	m_tradeAccept;
		/// <summary>
		/// Holds the owner of this window and items in it
		/// </summary>
		protected GamePlayer m_owner;
		/// <summary>
		/// Holds our trade partner
		/// </summary>
		protected PlayerTradeWindow	m_partnerWindow;
		/// <summary>
		/// Holds the flag for repair
		/// </summary>
		protected bool m_repair;
		/// <summary>
		/// Holds the flag for combine (spellcraft)
		/// </summary>
		protected bool m_combine;
		/// <summary>
		/// Holds the flag to know the it's a recipiant window
		/// </summary>
		protected bool m_recipiant;
		/// <summary>
		/// Holds the trade windows sync object
		/// </summary>
		protected object m_sync;

		#endregion

		#region Properties

		/// <summary>
		/// Returns the array of items we offer for trade
		/// </summary>
		public ArrayList TradeItems
		{
			get { return m_tradeItems; }
			set { m_tradeItems = value; }
		}

		/// <summary>
		/// Returns the array of items the partner offer for trade
		/// </summary>
		public ArrayList PartnerTradeItems
		{
			get { return m_partnerWindow.TradeItems; }
		}

		/// <summary>
		/// Returns the money we offer for trade
		/// </summary>
		public long TradeMoney
		{
			get { return m_tradeMoney; }
			set { m_tradeMoney = value; }
		}

		/// <summary>
		/// Returns the money the partner offer for trade
		/// </summary>
		public long PartnerTradeMoney
		{
			get { return m_partnerWindow.TradeMoney; }
		}

		/// <summary>
		/// Gets the owner of this window and items in it
		/// </summary>
		public GamePlayer Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Gets the owner of this window and items in it
		/// </summary>
		public GamePlayer Partner
		{
			get { return m_partnerWindow.Owner; }
		}

		/// <summary>
		/// Gets the partner window of this window and items in it
		/// </summary>
		public PlayerTradeWindow PartnerWindow
		{
			set { m_partnerWindow = value; }
		}

		/// <summary>
		/// Gets the access sync object for this and TradePartner windows
		/// </summary>
		public object Sync
		{
			get { return m_sync; }
		}

		/// <summary>
		/// Gets the item count in trade window
		/// </summary>
		public int ItemsCount
		{
			get { return m_tradeItems.Count; }
		}

		/// <summary>
		/// Gets the item count in partner trade window
		/// </summary>
		public int PartnerItemsCount
		{
			get { return m_partnerWindow.TradeItems.Count; }
		}

		/// <summary>
		/// Gets or sets the repair flag is switched
		/// </summary>
		public bool Repairing
		{
			get { return m_repair; }
			set 
			{ 		
				if(value == false)
				{
					m_partnerWindow.m_repair = false;
					m_repair = false;
					return;
				}

				if(!m_recipiant)
				{
					m_owner.Out.SendMessage("Only a recipient of a trade can initiate a repair.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					m_partnerWindow.m_repair = false;
					m_repair = false;
					return;
				}

				if(m_partnerWindow.ItemsCount != 1)
				{
					m_owner.Out.SendMessage("You can only repair one item at a time!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					m_partnerWindow.m_repair = false;
					m_repair = false;
					return;
				}

				if(ItemsCount > 0)
				{
					m_owner.Out.SendMessage("Your trade windows side must be empty to repair!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					m_partnerWindow.m_repair = false;
					m_repair = false;
					return;	
				}

				EquipableItem itemToRepair = (EquipableItem) m_partnerWindow.TradeItems[0];
				if(itemToRepair == null)
				{
					m_partnerWindow.m_repair = false;
					m_repair = false;
					return;
				}

				if(Repair.IsAllowedToBeginWork(m_owner, itemToRepair, 100))
				{
					m_partnerWindow.m_repair = true;
					m_repair = true;
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

				m_partnerWindow.m_repair = false;
				m_repair = false;
				return;
			}
		}

		/// <summary>
		/// Gets or sets the combine flag is switched
		/// </summary>
		public bool Combine
		{
			get { return m_combine; }
			set 
			{ 
				if(value == false)
				{
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

				if(!m_recipiant)
				{
					m_owner.Out.SendMessage("Only a recipient of a trade can initiate a combine.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

				if(m_partnerWindow.ItemsCount != 1)
				{
					m_owner.Out.SendMessage("You can only combine your items into one item!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

				GenericItem itemToCombine = (GenericItem) m_partnerWindow.TradeItems[0];
				if(itemToCombine == null)
				{
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

				AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_owner.CraftingPrimarySkill);
				if(skill != null && skill is AdvancedCraftingSkill)
				{
					if(((AdvancedCraftingSkill)skill).IsAllowedToCombine(m_owner, itemToCombine))
					{
						if(skill is SpellCrafting)
							((SpellCrafting)skill).ShowSpellCraftingInfos(m_owner, (EquipableItem)itemToCombine);

						m_partnerWindow.m_combine = true;
						m_combine = true;
						m_partnerWindow.m_repair = false;
						m_repair = false;
						return;
					}
				}
				else
				{
					m_owner.Out.SendMessage("You don't have enough skill to combine items.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}

				m_partnerWindow.m_combine = false;
				m_combine = false;
				return;
			}
		}

		#endregion

		#region Add/Remove/Update

		/// <summary>
		/// The max amount of items that can be added to tradewindow
		/// </summary>
		public const int MAX_ITEMS = 10;

		/// <summary>
		/// Adds an item to the tradewindow
		/// </summary>
		/// <param name="itemForTrade">InventoryItem to add</param>
		/// <returns>true if added</returns>
		public bool AddItemToTrade(GenericItem itemForTrade)
		{
			lock(Sync)
			{
				if(m_tradeAccept && m_partnerWindow.m_tradeAccept)
					return false;

				if(!itemForTrade.IsTradable)
					return false;

				if (TradeItems.Contains(itemForTrade))
					return false;

				if (TradeItems.Count >= MAX_ITEMS)
				{
					TradeUpdate();
					return false;
				}

				TradeItems.Add(itemForTrade);
				TradeUpdate();
				return true;
			}
		}

		/// <summary>
		/// Adds money to the tradewindow
		/// </summary>
		/// <param name="money">Array of money values to add</param>
		public void AddMoneyToTrade(long money)
		{
			lock(Sync)
			{
				if(!m_tradeAccept || !m_partnerWindow.m_tradeAccept)
				{
					TradeMoney += money;
					TradeUpdate();
				}
			}
		}

		/// <summary>
		/// Removes an item from the tradewindow
		/// </summary>
		/// <param name="itemToRemove"></param>
		public void RemoveItemToTrade(GenericItem itemToRemove)
		{
			if (itemToRemove == null)
				return;

			lock(Sync)
			{
				if(!m_tradeAccept || !m_partnerWindow.m_tradeAccept)
				{
					TradeItems.Remove(itemToRemove);
					TradeUpdate();
				}
			}
		}

		/// <summary>
		/// Updates the trade window
		/// </summary>
		public void TradeUpdate()
		{
			lock (Sync)
			{
				m_tradeAccept = false;
				m_partnerWindow.m_tradeAccept = false;

				m_owner.Out.SendTradeWindow();
				m_partnerWindow.Owner.Out.SendTradeWindow();
			}
		}

		#endregion	

		#region AcceptTrade/CloseTraide
		
		/// <summary>
		/// Called each time a player push the accept button to accept the trade
		/// </summary>
		public bool AcceptTrade()
		{
			lock (Sync)
			{
				m_tradeAccept = true;
				GamePlayer partner = m_partnerWindow.Owner;

				partner.Out.SendMessage(m_owner.Name + " has accepted the trade.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				// Check if the tradepartner has also agreed to the trade
				if (!m_partnerWindow.m_tradeAccept) return false;

				bool logTrade = (m_owner.Client.Account.PrivLevel == ePrivLevel.Player || partner.Client.Account.PrivLevel == ePrivLevel.Player);

				if(m_combine == true)
				{
					GamePlayer crafter = (m_recipiant == true ? m_owner : partner);
					AdvancedCraftingSkill skill = CraftingMgr.getSkillbyEnum(crafter.CraftingPrimarySkill) as AdvancedCraftingSkill;
					if(skill != null)
					{
						skill.CombineItems(crafter);
					}
				}
				else if(m_repair == true)
				{
					GamePlayer crafter = (m_recipiant == true ? m_owner : partner);
					GenericItem itemToRepair = (GenericItem)(m_recipiant == true ? m_partnerWindow.TradeItems[0] : TradeItems[0]);
					if(itemToRepair != null)
					{
						crafter.RepairItem(itemToRepair);
					}
				}
				else
				{
					//Calculate the count of items 
					int myTradeItemsCount = m_tradeItems.Count;
					int partnerTradeItemsCount = m_partnerWindow.TradeItems.Count;

					//Test if we and our partner have enough space in inventory
					int mySpaceNeeded      = Math.Max(0, partnerTradeItemsCount - myTradeItemsCount);
					int partnerSpaceNeeded = Math.Max(0, myTradeItemsCount - partnerTradeItemsCount);
					bool enoughSpace        = m_owner.Inventory.IsSlotsFree(mySpaceNeeded, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					bool partnerEnoughSpace = partner.Inventory.IsSlotsFree(partnerSpaceNeeded, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

					//Test if we and our partner have enough money
					bool enoughMoney        = m_owner.Money >= TradeMoney ? true : false;
					bool partnerEnoughMoney = partner.Money >= m_partnerWindow.TradeMoney ? true : false;

					//Check the preconditions
					if (!enoughSpace || !partnerEnoughSpace || !enoughMoney || !partnerEnoughMoney)
					{
						if (!enoughSpace)
						{
							m_owner.Out.SendMessage("You don't have enought space in your inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							partner.Out.SendMessage(m_owner.Name + " doesn't have enought space in his inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}
						if (!partnerEnoughSpace)
						{
							partner.Out.SendMessage("You don't have enought space in your inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							m_owner.Out.SendMessage(partner.Name + " doesn't have enought space in his inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}
						if (!enoughMoney)
						{
							//Reset the money if we don't have enough
							TradeMoney = 0;

							m_owner.Out.SendMessage("You don't have enought money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							partner.Out.SendMessage(m_owner.Name + " doesn't have enought money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}
						if (!partnerEnoughMoney)
						{
							//Reset the money if our partner doesn't have enough
							m_partnerWindow.TradeMoney = 0;

							partner.Out.SendMessage("You don't have enought money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							m_owner.Out.SendMessage(partner.Name + " doesn't have enought money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}

						//Update our tradewindow and return
						TradeUpdate();
						return false;
					}

					//Now transfer all all items
					((GamePlayerInventory)m_owner.Inventory).BeginChanges();
					((GamePlayerInventory)partner.Inventory).BeginChanges();
					
					// remove all items first to make sure there is enough space
					foreach(GenericItem item in m_tradeItems)
					{
						m_owner.Inventory.RemoveItem(item);
					}

					foreach(GenericItem item in m_partnerWindow.TradeItems)
					{
						partner.Inventory.RemoveItem(item);
					}

					// now add all parter items to the inventory
					foreach(GenericItem item in m_partnerWindow.TradeItems)
					{
						if (!m_owner.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							if (log.IsWarnEnabled)
								log.Warn("Item was not added to first free slot. Player="+m_owner.Name+"; Item="+item.ItemID);
						if(logTrade)
							GameServer.Instance.LogGMAction("   Item: "+partner.Name+"("+partner.Client.Account.AccountName+") -> "+m_owner.Name+"("+m_owner.Client.Account.AccountName+") : "+item.Name+"("+item.ItemID+")");
					}

					foreach(GenericItem item in m_tradeItems)
					{
						if (!partner.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							if (log.IsWarnEnabled)
								log.Warn("Item was not added to first free slot. Player="+partner.Name+"; Item="+item.ItemID);
						if(logTrade)
							GameServer.Instance.LogGMAction("   Item: "+m_owner.Name+"("+m_owner.Client.Account.AccountName+") -> "+partner.Name+"("+partner.Client.Account.AccountName+") : "+item.Name+"("+item.ItemID+")");
					}
					

					((GamePlayerInventory)m_owner.Inventory).CommitChanges();
					((GamePlayerInventory)partner.Inventory).CommitChanges();
					
					m_owner.Out.SendMessage("Trade Completed. " + myTradeItemsCount + " items for " + partnerTradeItemsCount + " items.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					partner.Out.SendMessage("Trade Completed. " + partnerTradeItemsCount + " items for " + myTradeItemsCount + " items.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				//Now add the money
				partner.RemoveMoney(m_partnerWindow.TradeMoney);
				m_owner.AddMoney(m_partnerWindow.TradeMoney, "You get {0}.");

				//Now add the money to the partner
				m_owner.RemoveMoney(TradeMoney);
				partner.AddMoney(TradeMoney, "You get {0}.");

				if(logTrade)
				{
					if(m_partnerWindow.TradeMoney > 0)
						GameServer.Instance.LogGMAction("  Money: "+partner.Name+"("+partner.Client.Account.AccountName+") -> "+m_owner.Name+"("+m_owner.Client.Account.AccountName+") : "+m_partnerWindow.TradeMoney+"coppers");
					if(TradeMoney > 0)
						GameServer.Instance.LogGMAction("  Money: "+m_owner.Name+"("+m_owner.Client.Account.AccountName+") -> "+partner.Name+"("+partner.Client.Account.AccountName+") : "+TradeMoney+"coppers");
				}

				CloseTrade();// Close the Trade Window

				return true;
			}
		}

		/// <summary>
		/// Closes the tradewindow
		/// </summary>
		public void CloseTrade()
		{
			lock(Sync)
			{
				m_owner.Out.SendCloseTradeWindow();
				m_partnerWindow.Owner.Out.SendCloseTradeWindow();
			}
			lock (m_owner)
			{
				m_owner.TradeWindow = null;
			}
			lock (m_partnerWindow.Owner)
			{
				m_partnerWindow.Owner.TradeWindow = null;
			}
		}
		#endregion
	}
}
