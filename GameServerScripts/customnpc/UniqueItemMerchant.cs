/*
 * Created by SharpDevelop.
 * User: Administrateur
 * Date: 24/08/2013
 * Time: 13:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Language;


namespace DOL.GS
{
	/// <summary>
	/// Description of UniqueItemMerchant.
	/// </summary>
	public class UniqueItemMerchant : GameMerchant
	{
		
		public const uint CYCLE_FOR_RENEW_MS = 3600000;
		
		private static Timer m_renewinventory;
		
		private static ulong m_cycle_iteration = 0;
		private ulong m_inventory_iteration;

		private List<eObjectType> m_types;
		
	
		public UniqueItemMerchant()
			:base()
		{
			//launch first gen
			changeInventory();
			m_inventory_iteration = m_cycle_iteration;
		}
		
		public override bool Interact(GamePlayer player)
		{
			if(this.m_inventory_iteration != m_cycle_iteration) {
				changeInventory();		
			}
			
			return base.Interact(player);
		}
		
		protected static void GenerateUniqueInventory(object sender, ElapsedEventArgs args)
		{
			m_cycle_iteration++;
			
			foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
            	if (clients != null)
					clients.Out.SendMessage("RoG Merchants have new Inventories !", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
			
		}
		
		public void changeInventory()
		{
			// Empty current trade list.
			MerchantTradeItems inventory = new MerchantTradeItems("");
			GeneratedUniqueItem item = null;
			int maxitem = (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS*MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS);
			
			//try to get specific object type and level from mob
			this.m_types = new  List<eObjectType>();
			
			
			// arbitrarly set a level range around Merchant Level.
			int minlevel = this.Level - 15;
			int maxlevel = this.Level + 15;
			
			if(minlevel < 1)
			{
				maxlevel += minlevel*(-1);
				minlevel = 1;
			}
			
			if(minlevel >= 50) 
			{
				minlevel = 51;
			}
			
			if(maxlevel >= 50) 
			{
				maxlevel = 51;
			}
			
			int levelrange = 0;
			
			if((maxlevel-minlevel) > 0)
				levelrange = (int)(maxitem/(maxlevel-minlevel)); //level must increase every levelrange steps
			else
				levelrange = maxitem;
				

			// try to parse PackageID for a list of ObjectTypes			
			int typerange = 0;			
			if(this.PackageID != null && this.PackageID.Length > 0) 
			{
				try 
				{
					foreach(string type in Util.SplitCSV(this.PackageID)) 
					{
						eObjectType objtype = (eObjectType)Convert.ToInt32(type);
						
						if((objtype >= eObjectType._FirstArmor && objtype <= eObjectType._LastArmor) || (objtype >= eObjectType._FirstWeapon && objtype <= eObjectType._LastWeapon) || objtype == eObjectType.Magical) 
						{
							this.m_types.Add((eObjectType)objtype);
						}
					}
					
					if(this.m_types.Count > 0)
						typerange = (int)(maxitem/this.m_types.Count);
					
					if((maxlevel-minlevel) > 0)
						levelrange = (int)(typerange/(maxlevel-minlevel));
					else
						levelrange = typerange;
					
				}
				catch 
				{
					this.m_types = null;
					typerange = 0;
				}
			}
			
			
			//initialize counters
			int curitem = 0;
			int curlevel = minlevel;
			int i = 0;
			
			// create randomized inventory
			for(i = 0 ; i < maxitem ; i++) 
			{
				
				
				
				if(i > 0 && typerange > 0 && i%typerange == 0 && this.m_types != null && curitem < (this.m_types.Count - 1)) 
				{
					curitem++;
					curlevel = minlevel;
				}
				else if(i > 0 && i%levelrange == 0 && curlevel < maxlevel) 
				{
					curlevel++;
				}
				
				if(typerange < 1)
					item = new GeneratedUniqueItem(this.Realm, (byte)curlevel);
				else
					item = new GeneratedUniqueItem(this.Realm, (byte)curlevel, this.m_types[curitem]);
				
				inventory.AddTradeItem((byte)(i/MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS), (eMerchantWindowSlot)(i%MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS), item);
			
			}
			
			//assigne inventory to NPC
			this.TradeItems = inventory;
			
			//sync iteration
			this.m_inventory_iteration = m_cycle_iteration;
		}
		
		public override eQuestIndicator GetQuestIndicator(GamePlayer player)
		{
			return eQuestIndicator.Lesson ;
		}
		
		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			if(number != 1)
			{
				player.Out.SendMessage("You can only buy 1 Unique Item at once !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			
			byte pagenumber = (byte)(item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS);
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemUnique template = (ItemUnique)this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Price;

			lock (player.Inventory)
			{

				if (player.GetCurrentMoney() < totalValue)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				
				if(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == eInventorySlot.Invalid)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				
				
				template.AllowAdd = true;
				GameServer.Database.AddObject(template);
				player.Inventory.AddTemplate(GameInventoryItem.Create<ItemUnique>(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				this.TradeItems.RemoveTradeItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

				// Check if player has enough money and subtract the money
				if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					throw new Exception("Money amount changed while adding items.");
				}
				
				InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalValue);
			}
		}

		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			//launch timer
			if (m_renewinventory == null) {
				m_renewinventory = new Timer(CYCLE_FOR_RENEW_MS);
				m_renewinventory.AutoReset = true;
				m_renewinventory.Elapsed += new ElapsedEventHandler(GenerateUniqueInventory);
				m_renewinventory.Start();
			}
			
			m_cycle_iteration++;
		}
		
		//This function is called whenever the event is stopped
		[ScriptUnloadedEvent]
		public static void OnScriptUnload(DOLEvent e, object sender, EventArgs args)
		{
			//We stop our timer ...
			if (m_renewinventory != null) {
				m_renewinventory.Stop();
				m_renewinventory.Close();	
			}				

		}
		
	}
}
