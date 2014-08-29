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
		
		private long m_cycle_time = 0;

		private List<eObjectType> m_types;
		
	
		public UniqueItemMerchant()
			:base()
		{
			//launch first gen
			//changeInventory();
		}
		
		public override bool Interact(GamePlayer player)
		{
			if(m_cycle_time+CYCLE_FOR_RENEW_MS < GameTimer.GetTickCount()) 
			{
				changeInventory();		
			}
			
			return base.Interact(player);
		}
		
		public void changeInventory()
		{
			// Update timestamp
			m_cycle_time = GameTimer.GetTickCount();
			
			// Empty current trade list.
			MerchantTradeItems inventory = new MerchantTradeItems("");
			int maxitem = (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS*MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS);
			
			//try to get specific object type and level from mob
			m_types = new List<eObjectType>();
			
			
			// arbitrarly set a level range around Merchant Level.
			int minlevel = Level - 15;
			int maxlevel = Level + 15;
			
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
			if(PackageID != null && PackageID.Length > 0) 
			{
				try 
				{
					foreach(string type in Util.SplitCSV(PackageID)) 
					{
						eObjectType objtype = (eObjectType)Convert.ToInt32(type);
						
						if((objtype >= eObjectType._FirstArmor && objtype <= eObjectType._LastArmor) || (objtype >= eObjectType._FirstWeapon && objtype <= eObjectType._LastWeapon) || objtype == eObjectType.Magical) 
						{
							m_types.Add((eObjectType)objtype);
						}
					}
					
					if(m_types.Count > 0)
						typerange = (int)(maxitem/m_types.Count);
					
					if((maxlevel-minlevel) > 0)
						levelrange = (int)(typerange/(maxlevel-minlevel));
					else
						levelrange = typerange;
					
				}
				catch 
				{
					m_types = null;
					typerange = 0;
				}
			}
			
			
			//initialize counters
			int curitem = 0;
			int curlevel = minlevel;
			
			// create randomized inventory
			for(int j = 0 ; j < maxitem ; j++) 
			{
				GeneratedUniqueItem item = null;
				
				int i = j;
				
				if(i > 0 && typerange > 0 && i%typerange == 0 && m_types != null && curitem < (m_types.Count - 1)) 
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
					item = new GeneratedUniqueItem(this.Realm, (byte)curlevel, m_types[curitem]);
				
				inventory.AddTradeItem((byte)(i/MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS), (eMerchantWindowSlot)(i%MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS), item);
			
			}
			
			//assigne inventory to NPC
			TradeItems = inventory;
		}
		
		public override eQuestIndicator GetQuestIndicator(GamePlayer player)
		{
			return eQuestIndicator.Lesson;
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

			ItemUnique template = (ItemUnique)TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
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
				TradeItems.RemoveTradeItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				
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
		
	}
}
