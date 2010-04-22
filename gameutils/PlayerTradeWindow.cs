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
using DOL.GS;
using DOL.Database;
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
		/// <summary>
		/// Stores the begin changes count
		/// </summary>
		protected int m_changesCount;

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

				InventoryItem itemToRepair = (InventoryItem) m_partnerWindow.TradeItems[0];
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

				InventoryItem itemToCombine = (InventoryItem) m_partnerWindow.TradeItems[0];
				if(itemToCombine == null)
				{
					m_partnerWindow.m_combine = false;
					m_combine = false;
					return;
				}

                // --------------------------------------------------------------
                // Luhz Crafting Update:
                // Players may now have any, and all, "primary" crafting skills.
                AbstractCraftingSkill skill = null;
                lock (m_owner.TradeWindow.Sync)
                {
                    foreach (InventoryItem i in (ArrayList)m_owner.TradeWindow.TradeItems.Clone())
                    {
                        if (i.Object_Type == (int)eObjectType.AlchemyTincture)
                        {
                            if (m_owner.GetCraftingSkillValue(eCraftingSkill.Alchemy) > 0)
                            {
                                skill = CraftingMgr.getSkillbyEnum(eCraftingSkill.Alchemy);
                                break;
                            }
                        }
                        else if (i.Object_Type == (int)eObjectType.SpellcraftGem)
                        {
                            if (m_owner.GetCraftingSkillValue(eCraftingSkill.SpellCrafting) > 0)
                            {
                                skill = CraftingMgr.getSkillbyEnum(eCraftingSkill.SpellCrafting);
                                break;
                            }
                        }
                    }
                }
                // --------------------------------------------------------------
				if(skill != null && skill is AdvancedCraftingSkill)
				{
					if(((AdvancedCraftingSkill)skill).IsAllowedToCombine(m_owner, itemToCombine))
					{
						if(skill is SpellCrafting)
							((SpellCrafting)skill).ShowSpellCraftingInfos(m_owner, itemToCombine);

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
		public bool AddItemToTrade(InventoryItem itemForTrade)
		{
			lock(Sync)
			{
				// allow admin and gm account opened windows to trade any item
				if (this.m_owner.Client.Account.PrivLevel == 1)
				{
					if (!itemForTrade.IsDropable || !itemForTrade.IsPickable || itemForTrade.IsNotLosingDur)
						return false;
				}
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
				TradeMoney += money;
				TradeUpdate();
			}
		}

		/// <summary>
		/// Removes an item from the tradewindow
		/// </summary>
		/// <param name="itemToRemove"></param>
		public void RemoveItemToTrade(InventoryItem itemToRemove)
		{
			if (itemToRemove == null)
				return;

			lock(Sync)
			{
				TradeItems.Remove(itemToRemove);
				if(!m_tradeAccept || !m_partnerWindow.m_tradeAccept) TradeUpdate();
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

				if (m_changesCount > 0) return;
				if (m_changesCount < 0)
				{
					m_changesCount = 0;
					if (log.IsErrorEnabled)
						log.Error("Changes count is less than 0, forgot to add m_changesCount++?\n\n" + Environment.StackTrace);
				}

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

				bool logTrade = ServerProperties.Properties.LOG_TRADES;
				if (m_owner.Client.Account.PrivLevel > 1 || partner.Client.Account.PrivLevel > 1)
					logTrade = true;

				//Test if we and our partner have enough money
				bool enoughMoney        = m_owner.RemoveMoney(TradeMoney);
				bool partnerEnoughMoney = partner.RemoveMoney(m_partnerWindow.TradeMoney);

				//Check the preconditions
				if (!enoughMoney || !partnerEnoughMoney)
				{
					if (!enoughMoney)
					{
						//Reset the money if we don't have enough
						TradeMoney = 0;
						if (partnerEnoughMoney)
							partner.AddMoney(m_partnerWindow.TradeMoney);

						m_owner.Out.SendMessage("You don't have enough money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						partner.Out.SendMessage(m_owner.Name + " doesn't have enough money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					}
					if (!partnerEnoughMoney)
					{
						//Reset the money if our partner doesn't have enough
						m_partnerWindow.TradeMoney = 0;
						if (enoughMoney)
							m_owner.AddMoney(TradeMoney);

						partner.Out.SendMessage("You don't have enough money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						m_owner.Out.SendMessage(partner.Name + " doesn't have enough money.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					}

					//Update our tradewindow and return
					TradeUpdate();
					return false;
				}

				if(m_combine == true)
				{
					GamePlayer crafter = (m_recipiant == true ? m_owner : partner);					
                    // --------------------------------------------------------------
                    // Luhz Crafting Update:
                    // Players may now have any, and all, "primary" crafting skills.
                    // AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(crafter.CraftingPrimarySkill);
                    AbstractCraftingSkill skill = null;
                    lock (crafter.TradeWindow.Sync)
                    {
                        foreach (InventoryItem i in (ArrayList)crafter.TradeWindow.TradeItems.Clone())
                        {
                            if (i.Object_Type == (int)eObjectType.AlchemyTincture)
                            {
                                if (m_owner.GetCraftingSkillValue(eCraftingSkill.Alchemy) > 0)
                                {
                                    skill = CraftingMgr.getSkillbyEnum(eCraftingSkill.Alchemy);
                                    break;
                                }
                            }
                            else if (i.Object_Type == (int)eObjectType.SpellcraftGem)
                            {
                                if (crafter.GetCraftingSkillValue(eCraftingSkill.SpellCrafting) > 0)
                                {
                                    skill = CraftingMgr.getSkillbyEnum(eCraftingSkill.SpellCrafting);
                                    break;
                                }
                            }
                        }
                    }
                    // --------------------------------------------------------------
					if(skill != null && skill is AdvancedCraftingSkill)
					{
						((AdvancedCraftingSkill)skill).CombineItems(crafter);
					}
				}
				else if(m_repair == true)
				{
					GamePlayer crafter = (m_recipiant == true ? m_owner : partner);
					InventoryItem itemToRepair = (InventoryItem)(m_recipiant == true ? m_partnerWindow.TradeItems[0] : TradeItems[0]);
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

					//Check the preconditions
					if (!enoughSpace || !partnerEnoughSpace)
					{
						if (!enoughSpace)
						{
							m_owner.Out.SendMessage("You don't have enough space in your inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							partner.Out.SendMessage(m_owner.Name + " doesn't have enough space in his inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}
						if (!partnerEnoughSpace)
						{
							partner.Out.SendMessage("You don't have enough space in your inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							m_owner.Out.SendMessage(partner.Name + " doesn't have enough space in his inventory.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						}

						//Update our tradewindow and return
						TradeUpdate();

                        //This was already removed above, needs to be returned to the players on trade failure.
                        m_owner.AddMoney(TradeMoney);
                        partner.AddMoney(m_partnerWindow.TradeMoney);

						return false;
					}

					//Now transfer everything
					m_owner.Inventory.BeginChanges();
					partner.Inventory.BeginChanges();
					m_changesCount++;
					m_partnerWindow.m_changesCount++;

					// must be cloned because Inventory.RemoveItem removes it from trade window
					ArrayList ownerTradeItems = (ArrayList) TradeItems.Clone();
					ArrayList partnerTradeItems = (ArrayList) m_partnerWindow.TradeItems.Clone();

					// remove all items first to make sure there is enough space
					// if inventory is full but removed items count >= received count
                    foreach (InventoryItem item in ownerTradeItems)
                    {
                        lock (m_owner.Inventory)
                        {
							if (!m_owner.Inventory.RemoveTradeItem(item))
							{
								if (logTrade)
									GameServer.Instance.LogGMAction("   NOTItem: " + m_owner.Name + "(" + m_owner.Client.Account.Name + ") -> " + partner.Name + "(" + partner.Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb + ")");

								//BOT.Ban(m_owner, "Trade Hack");
								//BOT.Ban(partner, "Trade Hack");

								return false;
							}
                        }
                    }
                    foreach (InventoryItem item in partnerTradeItems)
                    {
                        lock (partner.Inventory)
                        {
							if (!partner.Inventory.RemoveTradeItem(item))
							{
								if (logTrade)
									GameServer.Instance.LogGMAction("   NOTItem: " + m_owner.Name + "(" + m_owner.Client.Account.Name + ") -> " + partner.Name + "(" + partner.Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb + ")");

								//BOT.Ban(m_owner, "Trade Hack");
								//BOT.Ban(partner, "Trade Hack");

								return false;
							}
                        }
                    }

					foreach(InventoryItem item in ownerTradeItems)
					{
						if (m_owner.Guild != partner.Guild)
						{
							item.Emblem = 0;
						}

						if (!partner.Inventory.AddTradeItem(eInventorySlot.FirstEmptyBackpack, item))
						{
							if (log.IsWarnEnabled)
							{
								log.Warn("Item was not added to first free slot. Player=" + partner.Name + "; Item=" + item.Id_nb);
							}
						}

						if (logTrade)
						{
							GameServer.Instance.LogGMAction("   Item: " + m_owner.Name + "(" + m_owner.Client.Account.Name + ") -> " + partner.Name + "(" + partner.Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb + ")");
						}
					}

					foreach(InventoryItem item in partnerTradeItems)
					{
						if (m_owner.Guild != partner.Guild)
						{
							item.Emblem = 0;
						}

						if (!m_owner.Inventory.AddTradeItem(eInventorySlot.FirstEmptyBackpack, item))
						{
							if (log.IsWarnEnabled)
							{
								log.Warn("Item was not added to first free slot. Player=" + m_owner.Name + "; Item=" + item.Id_nb);
							}

							if (logTrade)
							{
								GameServer.Instance.LogGMAction("   Item: " + partner.Name + "(" + partner.Client.Account.Name + ") -> " + m_owner.Name + "(" + m_owner.Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb + ")");
							}
						}
					}

					m_owner.Inventory.CommitChanges();
					partner.Inventory.CommitChanges();
					m_changesCount--;
					m_partnerWindow.m_changesCount--;

					m_owner.Out.SendMessage("Trade Completed. " + myTradeItemsCount + " items for " + partnerTradeItemsCount + " items.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					partner.Out.SendMessage("Trade Completed. " + partnerTradeItemsCount + " items for " + myTradeItemsCount + " items.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					m_owner.Inventory.SaveIntoDatabase(m_owner.InternalID);
					partner.Inventory.SaveIntoDatabase(partner.InternalID);
				}

				if(logTrade)
				{
					if(m_partnerWindow.TradeMoney > 0)
						GameServer.Instance.LogGMAction("  Money: "+partner.Name+"("+partner.Client.Account.Name+") -> "+m_owner.Name+"("+m_owner.Client.Account.Name+") : "+m_partnerWindow.TradeMoney+"coppers");
					if(TradeMoney > 0)
						GameServer.Instance.LogGMAction("  Money: "+m_owner.Name+"("+m_owner.Client.Account.Name+") -> "+partner.Name+"("+partner.Client.Account.Name+") : "+TradeMoney+"coppers");
				}

				if (TradeMoney > 0 || m_partnerWindow.TradeMoney > 0)
				{
					//Now add the money
					m_owner.AddMoney(m_partnerWindow.TradeMoney, "You get {0}.");
					partner.AddMoney(TradeMoney, "You get {0}.");
					m_owner.SaveIntoDatabase();
					partner.SaveIntoDatabase();
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
