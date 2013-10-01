/*
 * This NPC was originally written by crazys (kyle).
 * Updated 10-23-08 by BluRaven
 * I have only changed (added) the eMLLine enum to get it to compile
 * as a standalone script with the current SVN of DOL.  Place both scripts into your scripts folder and restart your server.
 * to create in game type: /mob create DOL.GS.Scripts.MLNPC
 */

using System;
using System.Collections;
using System.Reflection;
using System.Timers;

using log4net;
using DOL;

using DOL.GS;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.GS.GameEvents;
using DOL.GS.PacketHandler;



namespace DOL.GS
{


	/// <summary>
	/// Defines the Master Level lines
	/// </summary>
	public enum eMLLine : byte
	{
		/// <summary>
		/// No specific Master Level Line
		/// </summary>
		None = 0,
		/// <summary>
		/// Banelord Master Level Line
		/// </summary>
		Banelord = 1,
		/// <summary>
		/// Battlemaster Master Level Line
		/// </summary>
		BattleMaster = 2,
		/// <summary>
		/// Convoker Master Level Line
		/// </summary>
		Convoker = 3,
		/// <summary>
		/// Perfecter Master Level Line
		/// </summary>
		Perfecter = 4,
		/// <summary>
		/// Sojourner Master Level Line
		/// </summary>
		Sojourner = 5,
		/// <summary>
		/// Spymaster Master Level Line
		/// </summary>
		Spymaster = 6,
		/// <summary>
		/// Stormlord Master Level Line
		/// </summary>
		Stormlord = 7,
		/// <summary>
		/// Stormlord Master Level Line
		/// </summary>
		Warlord = 8,

	}
}



namespace DOL.GS.Scripts
{
	public class MLGlassNPC : GameAtlanteanGlassMerchant
	{
		private static ItemTemplate ml1token = null;
		private static ItemTemplate ml2token = null;
		private static ItemTemplate ml3token = null;
		private static ItemTemplate ml4token = null;
		private static ItemTemplate ml5token = null;
		private static ItemTemplate ml6token = null;
		private static ItemTemplate ml7token = null;
		private static ItemTemplate ml8token = null;
		private static ItemTemplate ml9token = null;
		private static ItemTemplate ml10token = null;

		string mlchoicea = "";
		string mlchoiceb = "";
		
		private const string ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM = "ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM";
		

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MLGlassNPC()
			: base()
		{
			Flags |= GameNPC.eFlags.PEACE;
			Level = 50;


			if(ml1token == null || ml2token == null || ml3token == null || ml4token == null || ml5token == null || ml6token == null || ml7token == null || ml8token == null || ml9token == null || ml10token == null)
			{
				ml1token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml1token");
	
				ml2token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml2token");
	
				ml3token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml3token");
	
				ml4token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml4token");
	
				ml5token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml5token");
	
				ml6token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml6token");
	
				ml7token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml7token");
	
				ml8token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml8token");
	
				ml9token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml9token");
	
				ml10token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml10token");
			}
		
			this.TradeItems = new MerchantTradeItems("");
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml1token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml2token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml3token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml4token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml5token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml6token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml7token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml8token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml9token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml10token);
		}
		
		#region Add To World
		
		public override bool AddToWorld()
		{
			base.AddToWorld();
			return true;
		}
		
		#endregion Add To World
		
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer player = source as GamePlayer;
			
			if (!IsWithinRadius(source, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			
			if(player == null || item == null)
				return false;
			
			switch(item.Id_nb)
			{
				case "ml1token" :
					{
						switch(player.CharacterClass.ID)
						{
							case 1 : //Paladin
							case 2 : //Armsman
							case 32 : //Savage
							case 22 : //Warrior
							case 44 : //Hero
							case 60 :
							case 61 :
							case 62 ://Mauler
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 25 : //Hunter
							case 50 : //Ranger
							case 3 : //Scout
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Sojourner]";
								}
								break;
								
							case 24 : //Skald
							case 4 : //Minstrel
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Sojourner]";
								}
								break;
								
							case 5 : //Theurgist
							case 7 : //Wizard
							case 8 : //Sorcerer
							case 12 : //Necromancer
							case 27 : //Spiritmaster
							case 29 : //Runemaster
							case 41 : //Enchanter
							case 40 : //Eldritch
							case 39 : //Bainshee
							case 55 : //Animist
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Stormlord]";
								}
								break;
								
							case 6 : //Cleric
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 23 : //Shadowblade
							case 49 : //Nightshade
							case 9 : //Infiltrator
								{
									mlchoicea = "[Spymaster]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 46 : //Warden
							case 10 : //Frair
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 19 : //Reaver
							case 11 : //Mercenary
							case 31 : //Berserker
							case 43 : //Blademaster
							case 45 : //Champion
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Banelord]";
								}
								break;
								
							case 13 : //Cabalist
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 56 : //Valewalker
							case 21 : //Thane
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Stormlord]";
								}
								break;
								
							case 48 : //Bard
							case 26 : //Healer
								{
									mlchoicea = "[Sojourner]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 47 : //Druid
							case 28 : //Shaman
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 59 : //Warlock
							case 30 : //Bonedancer
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Banelord]";
								}
								break;
								
							case 33 : //Heretic
								{
									mlchoicea = "[Banelord]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 42 : //Mentalist
							case 34 : //Valkyrie
								{
									mlchoicea = "[Stormlord]";
									mlchoiceb = "[Warlord]";
								}
								break;
								
							case 58 : //Vampiir
								{
									mlchoicea = "[Banelord]";
									mlchoiceb = "[Warlord]";
								}
								break;
						}
						
						if (player.GetSpellLine("ML1 Banelord") == null &&
						    player.GetSpellLine("ML1 Battlemaster") == null &&
						    player.GetSpellLine("ML1 Convoker") == null &&
						    player.GetSpellLine("ML1 Perfecter") == null &&
						    player.GetSpellLine("ML1 Sojourner") == null &&
						    player.GetSpellLine("ML1 Stormlord") == null &&
						    player.GetSpellLine("ML1 Spymaster") == null &&
						    player.GetSpellLine("ML1 Warlord") == null)
						{
							player.Out.SendMessage("I can grant you access to " + mlchoicea + " or " + mlchoiceb, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							player.TempProperties.setProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, item);
							return false;
						}
						else
						{
							player.Out.SendMessage("You already have an ML path.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
					}
					
				case "ml2token":
					{
						if(player.MLLevel == 1 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 2, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml3token":
					{
						if(player.MLLevel == 2 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 3, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml4token":
					{
						if(player.MLLevel == 3 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 4, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml5token":
					{
						if(player.MLLevel == 4 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 5, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml6token":
					{
						if(player.MLLevel == 5 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 6, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml7token":
					{
						if(player.MLLevel == 6 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 7, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml8token":
					{
						if(player.MLLevel == 7 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 8, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml9token":
					{
						if(player.MLLevel == 8 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 9, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml10token":
					{
						if(player.MLLevel == 9 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 10, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
					
				default:
					player.Out.SendMessage("I don't want this item !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return false;
			}

		}

		private static void RaisePlayerMLLevel(GamePlayer player, int level, InventoryItem item)
		{
			if(player == null || level < 2 || level > 10)
				return;
			
			string line = "";
			if (player.GetSpellLine("ML"+(level-1)+" Banelord") != null)
				line = "ML"+level+" Banelord";
			else if (player.GetSpellLine("ML"+(level-1)+" Battlemaster") != null)
				line = "ML"+level+" Battlemaster";
			else if (player.GetSpellLine("ML"+(level-1)+" Convoker") != null)
				line = "ML"+level+" Convoker";
			else if (player.GetSpellLine("ML"+(level-1)+" Perfecter") != null)
				line = "ML"+level+" Perfecter";
			else if (player.GetSpellLine("ML"+(level-1)+" Sojourner") != null)
				line = "ML"+level+" Sojourner";
			else if (player.GetSpellLine("ML"+(level-1)+" Spymaster") != null)
				line = "ML"+level+" Spymaster";
			else if (player.GetSpellLine("ML"+(level-1)+" Stormlord") != null)
				line = "ML"+level+" Stormlord";
			else if (player.GetSpellLine("ML"+(level-1)+" Warlord") != null)
				line = "ML"+level+" Warlord";
			
			if(line == "")
				return;
			
			if(player.Inventory.RemoveItem(item))
			{
				player.MLLevel = level;
				player.MLGranted = false;
				player.MLExperience = 0;
				player.AddSpellLine(SkillBase.GetSpellLine(line));
				player.Out.SendMessage("You have gained "+line+".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				player.UpdateSpellLineLevels(true);
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();
				player.UpdatePlayerStatus();
				player.SaveIntoDatabase();
			}
			else
			{
				player.Out.SendMessage("I can't take your item !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			
		}
		
		private static void StartPlayerMLLevel(GamePlayer player, eMLLine mlLine, string mlSpell)
		{
			player.MLLine = (byte)mlLine;
			player.MLLevel = 1; 
			player.MLGranted = false; 
			player.MLExperience = 0;
			player.AddSpellLine(SkillBase.GetSpellLine(mlSpell));
			player.Out.SendMessage("You have gained "+mlSpell+".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			player.UpdateSpellLineLevels(true);
			player.Out.SendUpdatePlayerSkills();
			player.Out.SendUpdatePlayer();
			player.UpdatePlayerStatus();
		}
		
		protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			GiveItem(null, player, itemTemplate);
		}

		protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			InventoryItem item = GameInventoryItem.Create<ItemTemplate>(itemTemplate);
			if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
			{
				if (source == null)
				{
					player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				player.CreateItemOnTheGround(item);
				player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
			}
		}
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			string msg = "I can grant you Master Level Credits for Atlantean Glass, hand them back to me and you'll be rewarded with Master Levels Abilities !";
			if (player.Level < 40)
				msg += "\n\nPlease come back when you are over level 40.";


			player.Out.SendMessage(msg, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

			return true;
		}


		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str)) return false;
			if (!(source is GamePlayer)) return false;
			GamePlayer player = (GamePlayer)source;
			TurnTo(player.X, player.Y);

			if (player.Level < 40)
			{
				player.Out.SendMessage("Please come back when you are over level 40.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}

			switch (str)
			{

				case "Banelord"://11,19,31,33,43,45,58,59,
					if ((player.CharacterClass.ID == 11 ||
					     player.CharacterClass.ID == 19 || player.CharacterClass.ID == 31 ||
					     player.CharacterClass.ID == 33 || player.CharacterClass.ID == 43 ||
					     player.CharacterClass.ID == 45 || player.CharacterClass.ID == 58 ||
					     player.CharacterClass.ID == 59))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Banelord, "ML1 Banelord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Banelords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Battlemaster"://2,1,3,9,10,11,19,21,22,23,25,31,32,43,44,45,46,50,56,
					if ((player.CharacterClass.ID == 2 ||
					     player.CharacterClass.ID == 1 ||
					     player.CharacterClass.ID == 3 ||
					     player.CharacterClass.ID == 9 ||
					     player.CharacterClass.ID == 10 ||
					     player.CharacterClass.ID == 11 ||
					     player.CharacterClass.ID == 19 ||
					     player.CharacterClass.ID == 21 ||
					     player.CharacterClass.ID == 22 ||
					     player.CharacterClass.ID == 23 ||
					     player.CharacterClass.ID == 25 ||
					     player.CharacterClass.ID == 31 ||
					     player.CharacterClass.ID == 32 ||
					     player.CharacterClass.ID == 43 ||
					     player.CharacterClass.ID == 44 ||
					     player.CharacterClass.ID == 45 ||
					     player.CharacterClass.ID == 46 ||
					     player.CharacterClass.ID == 50 ||
					     player.CharacterClass.ID == 56 ||
					     player.CharacterClass.ID == 60 ||
					     player.CharacterClass.ID == 61 ||
					     player.CharacterClass.ID == 62))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.BattleMaster, "ML1 Battlemaster");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Battlemasters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Convoker"://5,7,8,12,13,27,28,29,30,39,40,41,47,55,59
					if ((player.CharacterClass.ID == 5 ||
					     player.CharacterClass.ID == 7 ||
					     player.CharacterClass.ID == 8 ||
					     player.CharacterClass.ID == 12 ||
					     player.CharacterClass.ID == 13 ||
					     player.CharacterClass.ID == 27 ||
					     player.CharacterClass.ID == 28 ||
					     player.CharacterClass.ID == 29 ||
					     player.CharacterClass.ID == 30 ||
					     player.CharacterClass.ID == 39 ||
					     player.CharacterClass.ID == 40 ||
					     player.CharacterClass.ID == 41 ||
					     player.CharacterClass.ID == 47 ||
					     player.CharacterClass.ID == 55 ||
					     player.CharacterClass.ID == 59))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Convoker, "ML1 Convoker");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Convokers, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Perfecter"://6,10,26,28,33,46,47,48,
					if ((player.CharacterClass.ID == 6 ||
					     player.CharacterClass.ID == 10 || player.CharacterClass.ID == 26 ||
					     player.CharacterClass.ID == 28 || player.CharacterClass.ID == 33 ||
					     player.CharacterClass.ID == 46 || player.CharacterClass.ID == 47 ||
					     player.CharacterClass.ID == 48))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Perfecter, "ML1 Perfecter");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Perfecters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Sojourner"://3,4,24,25,26,48,50,
					if ((player.CharacterClass.ID == 3 ||
					     player.CharacterClass.ID == 4 || player.CharacterClass.ID == 24 ||
					     player.CharacterClass.ID == 25 || player.CharacterClass.ID == 26 ||
					     player.CharacterClass.ID == 48 || player.CharacterClass.ID == 50))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Sojourner, "ML1 Sojourner");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Sojourners, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Spymaster"://9,23,49,
					if ((player.CharacterClass.ID == 9 ||
					     player.CharacterClass.ID == 23 || player.CharacterClass.ID == 49))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Spymaster, "ML1 Spymaster");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Spymasters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Stormlord"://5,7,8,12,13,21,27,29,30,34,39,40,41,42,49,55,56,
					if ((player.CharacterClass.ID == 5 ||
					     player.CharacterClass.ID == 7 ||
					     player.CharacterClass.ID == 8 ||
					     player.CharacterClass.ID == 12 ||
					     player.CharacterClass.ID == 13 ||
					     player.CharacterClass.ID == 21 ||
					     player.CharacterClass.ID == 27 ||
					     player.CharacterClass.ID == 29 ||
					     player.CharacterClass.ID == 30 ||
					     player.CharacterClass.ID == 34 ||
					     player.CharacterClass.ID == 39 ||
					     player.CharacterClass.ID == 40 ||
					     player.CharacterClass.ID == 41 ||
					     player.CharacterClass.ID == 42 ||
					     player.CharacterClass.ID == 49 ||
					     player.CharacterClass.ID == 55 ||
					     player.CharacterClass.ID == 56))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Stormlord, "ML1 Stormlord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Stormlords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Warlord"://2,1,4,6,22,24,32,34,42,44,58,
					if ((player.CharacterClass.ID == 2 ||
					     player.CharacterClass.ID == 1 ||
					     player.CharacterClass.ID == 4 ||
					     player.CharacterClass.ID == 6 ||
					     player.CharacterClass.ID == 22 ||
					     player.CharacterClass.ID == 24 ||
					     player.CharacterClass.ID == 32 ||
					     player.CharacterClass.ID == 34 ||
					     player.CharacterClass.ID == 42 ||
					     player.CharacterClass.ID == 44 ||
					     player.CharacterClass.ID == 58 ||
					     player.CharacterClass.ID == 60 ||
					     player.CharacterClass.ID == 61 ||
					     player.CharacterClass.ID == 62))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Warlord, "ML1 Warlord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Warlords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
					default: break;
			}

			return true;
		}

		public void SendReply(GamePlayer target, string msg)
		{
			target.Client.Out.SendMessage(
				msg,
				eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}
	}

		public class MLBountyNPC : GameBountyMerchant
	{
		private static ItemTemplate ml1token = null;
		private static ItemTemplate ml2token = null;
		private static ItemTemplate ml3token = null;
		private static ItemTemplate ml4token = null;
		private static ItemTemplate ml5token = null;
		private static ItemTemplate ml6token = null;
		private static ItemTemplate ml7token = null;
		private static ItemTemplate ml8token = null;
		private static ItemTemplate ml9token = null;
		private static ItemTemplate ml10token = null;

		string mlchoicea = "";
		string mlchoiceb = "";
		
		private const string ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM = "ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM";
		

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MLBountyNPC()
			: base()
		{
			Flags |= GameNPC.eFlags.PEACE;
			Level = 50;

			if(ml1token == null || ml2token == null || ml3token == null || ml4token == null || ml5token == null || ml6token == null || ml7token == null || ml8token == null || ml9token == null || ml10token == null)
			{
				ml1token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml1token");
	
				ml2token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml2token");
	
				ml3token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml3token");
	
				ml4token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml4token");
	
				ml5token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml5token");
	
				ml6token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml6token");
	
				ml7token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml7token");
	
				ml8token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml8token");
	
				ml9token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml9token");
	
				ml10token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml10token");
			}
			
			this.TradeItems = new MerchantTradeItems("");
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml1token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml2token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml3token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml4token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml5token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml6token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml7token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml8token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml9token);
			this.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, ml10token);
		}
		
		#region Add To World
		
		public override bool AddToWorld()
		{
			base.AddToWorld();
			return true;
		}
		
		#endregion Add To World
		
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer player = source as GamePlayer;
			
			if (!IsWithinRadius(source, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			
			if(player == null || item == null)
				return false;
			
			switch(item.Id_nb)
			{
				case "ml1token" :
					{
						switch(player.CharacterClass.ID)
						{
							case 1 : //Paladin
							case 2 : //Armsman
							case 32 : //Savage
							case 22 : //Warrior
							case 44 : //Hero
							case 60 :
							case 61 :
							case 62 ://Mauler
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 25 : //Hunter
							case 50 : //Ranger
							case 3 : //Scout
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Sojourner]";
								}
								break;
								
							case 24 : //Skald
							case 4 : //Minstrel
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Sojourner]";
								}
								break;
								
							case 5 : //Theurgist
							case 7 : //Wizard
							case 8 : //Sorcerer
							case 12 : //Necromancer
							case 27 : //Spiritmaster
							case 29 : //Runemaster
							case 41 : //Enchanter
							case 40 : //Eldritch
							case 39 : //Bainshee
							case 55 : //Animist
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Stormlord]";
								}
								break;
								
							case 6 : //Cleric
								{
									mlchoicea = "[Warlord]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 23 : //Shadowblade
							case 49 : //Nightshade
							case 9 : //Infiltrator
								{
									mlchoicea = "[Spymaster]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 46 : //Warden
							case 10 : //Frair
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 19 : //Reaver
							case 11 : //Mercenary
							case 31 : //Berserker
							case 43 : //Blademaster
							case 45 : //Champion
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Banelord]";
								}
								break;
								
							case 13 : //Cabalist
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Battlemaster]";
								}
								break;
								
							case 56 : //Valewalker
							case 21 : //Thane
								{
									mlchoicea = "[Battlemaster]";
									mlchoiceb = "[Stormlord]";
								}
								break;
								
							case 48 : //Bard
							case 26 : //Healer
								{
									mlchoicea = "[Sojourner]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 47 : //Druid
							case 28 : //Shaman
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 59 : //Warlock
							case 30 : //Bonedancer
								{
									mlchoicea = "[Convoker]";
									mlchoiceb = "[Banelord]";
								}
								break;
								
							case 33 : //Heretic
								{
									mlchoicea = "[Banelord]";
									mlchoiceb = "[Perfecter]";
								}
								break;
								
							case 42 : //Mentalist
							case 34 : //Valkyrie
								{
									mlchoicea = "[Stormlord]";
									mlchoiceb = "[Warlord]";
								}
								break;
								
							case 58 : //Vampiir
								{
									mlchoicea = "[Banelord]";
									mlchoiceb = "[Warlord]";
								}
								break;
						}
						
						if (player.GetSpellLine("ML1 Banelord") == null &&
						    player.GetSpellLine("ML1 Battlemaster") == null &&
						    player.GetSpellLine("ML1 Convoker") == null &&
						    player.GetSpellLine("ML1 Perfecter") == null &&
						    player.GetSpellLine("ML1 Sojourner") == null &&
						    player.GetSpellLine("ML1 Stormlord") == null &&
						    player.GetSpellLine("ML1 Spymaster") == null &&
						    player.GetSpellLine("ML1 Warlord") == null)
						{
							player.Out.SendMessage("I can grant you access to " + mlchoicea + " or " + mlchoiceb, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							player.TempProperties.setProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, item);
							return false;
						}
						else
						{
							player.Out.SendMessage("You already have an ML path.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
					}
					
				case "ml2token":
					{
						if(player.MLLevel == 1 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 2, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml3token":
					{
						if(player.MLLevel == 2 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 3, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml4token":
					{
						if(player.MLLevel == 3 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 4, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml5token":
					{
						if(player.MLLevel == 4 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 5, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml6token":
					{
						if(player.MLLevel == 5 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 6, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml7token":
					{
						if(player.MLLevel == 6 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 7, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml8token":
					{
						if(player.MLLevel == 7 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 8, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml9token":
					{
						if(player.MLLevel == 8 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 9, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
				case "ml10token":
					{
						if(player.MLLevel == 9 || player.Client.Account.PrivLevel > 1)
						{
							RaisePlayerMLLevel(player, 10, item);
							return base.ReceiveItem(player, item);
						}
						else
						{
							player.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
						
					}
					
				default:
					player.Out.SendMessage("I don't want this item !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return false;
			}

		}

		private static void RaisePlayerMLLevel(GamePlayer player, int level, InventoryItem item)
		{
			if(player == null || level < 2 || level > 10)
				return;
			
			string line = "";
			if (player.GetSpellLine("ML"+(level-1)+" Banelord") != null)
				line = "ML"+level+" Banelord";
			else if (player.GetSpellLine("ML"+(level-1)+" Battlemaster") != null)
				line = "ML"+level+" Battlemaster";
			else if (player.GetSpellLine("ML"+(level-1)+" Convoker") != null)
				line = "ML"+level+" Convoker";
			else if (player.GetSpellLine("ML"+(level-1)+" Perfecter") != null)
				line = "ML"+level+" Perfecter";
			else if (player.GetSpellLine("ML"+(level-1)+" Sojourner") != null)
				line = "ML"+level+" Sojourner";
			else if (player.GetSpellLine("ML"+(level-1)+" Spymaster") != null)
				line = "ML"+level+" Spymaster";
			else if (player.GetSpellLine("ML"+(level-1)+" Stormlord") != null)
				line = "ML"+level+" Stormlord";
			else if (player.GetSpellLine("ML"+(level-1)+" Warlord") != null)
				line = "ML"+level+" Warlord";
			
			if(line == "")
				return;
			
			if(player.Inventory.RemoveItem(item))
			{
				player.MLLevel = level;
				player.MLGranted = false;
				player.MLExperience = 0;
				player.AddSpellLine(SkillBase.GetSpellLine(line));
				player.Out.SendMessage("You have gained "+line+".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				player.UpdateSpellLineLevels(true);
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();
				player.UpdatePlayerStatus();
				player.SaveIntoDatabase();
			}
			else
			{
				player.Out.SendMessage("I can't take your item !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			
		}
		
		private static void StartPlayerMLLevel(GamePlayer player, eMLLine mlLine, string mlSpell)
		{
			player.MLLine = (byte)mlLine;
			player.MLLevel = 1; 
			player.MLGranted = false; 
			player.MLExperience = 0;
			player.AddSpellLine(SkillBase.GetSpellLine(mlSpell));
			player.Out.SendMessage("You have gained "+mlSpell+".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			player.UpdateSpellLineLevels(true);
			player.Out.SendUpdatePlayerSkills();
			player.Out.SendUpdatePlayer();
			player.UpdatePlayerStatus();
		}
		
		protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			GiveItem(null, player, itemTemplate);
		}

		protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			InventoryItem item = GameInventoryItem.Create<ItemTemplate>(itemTemplate);
			if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
			{
				if (source == null)
				{
					player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				player.CreateItemOnTheGround(item);
				player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
			}
		}
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			string msg = "I can grant you Master Level Credits for Atlantean Glass, hand them back to me and you'll be rewarded with Master Levels Abilities !";
			
			if (player.Level < 40)
				msg += "\n\nPlease come back when you are over level 40.";


			player.Out.SendMessage(msg, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

			return true;
		}


		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str)) return false;
			if (!(source is GamePlayer)) return false;
			GamePlayer player = (GamePlayer)source;
			TurnTo(player.X, player.Y);

			if (player.Level < 40)
			{
				player.Out.SendMessage("Please come back when you are over level 40.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}

			switch (str)
			{

				case "Banelord"://11,19,31,33,43,45,58,59,
					if ((player.CharacterClass.ID == 11 ||
					     player.CharacterClass.ID == 19 || player.CharacterClass.ID == 31 ||
					     player.CharacterClass.ID == 33 || player.CharacterClass.ID == 43 ||
					     player.CharacterClass.ID == 45 || player.CharacterClass.ID == 58 ||
					     player.CharacterClass.ID == 59))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Banelord, "ML1 Banelord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Banelords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Battlemaster"://2,1,3,9,10,11,19,21,22,23,25,31,32,43,44,45,46,50,56,
					if ((player.CharacterClass.ID == 2 ||
					     player.CharacterClass.ID == 1 ||
					     player.CharacterClass.ID == 3 ||
					     player.CharacterClass.ID == 9 ||
					     player.CharacterClass.ID == 10 ||
					     player.CharacterClass.ID == 11 ||
					     player.CharacterClass.ID == 19 ||
					     player.CharacterClass.ID == 21 ||
					     player.CharacterClass.ID == 22 ||
					     player.CharacterClass.ID == 23 ||
					     player.CharacterClass.ID == 25 ||
					     player.CharacterClass.ID == 31 ||
					     player.CharacterClass.ID == 32 ||
					     player.CharacterClass.ID == 43 ||
					     player.CharacterClass.ID == 44 ||
					     player.CharacterClass.ID == 45 ||
					     player.CharacterClass.ID == 46 ||
					     player.CharacterClass.ID == 50 ||
					     player.CharacterClass.ID == 56 ||
					     player.CharacterClass.ID == 60 ||
					     player.CharacterClass.ID == 61 ||
					     player.CharacterClass.ID == 62))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.BattleMaster, "ML1 Battlemaster");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Battlemasters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Convoker"://5,7,8,12,13,27,28,29,30,39,40,41,47,55,59
					if ((player.CharacterClass.ID == 5 ||
					     player.CharacterClass.ID == 7 ||
					     player.CharacterClass.ID == 8 ||
					     player.CharacterClass.ID == 12 ||
					     player.CharacterClass.ID == 13 ||
					     player.CharacterClass.ID == 27 ||
					     player.CharacterClass.ID == 28 ||
					     player.CharacterClass.ID == 29 ||
					     player.CharacterClass.ID == 30 ||
					     player.CharacterClass.ID == 39 ||
					     player.CharacterClass.ID == 40 ||
					     player.CharacterClass.ID == 41 ||
					     player.CharacterClass.ID == 47 ||
					     player.CharacterClass.ID == 55 ||
					     player.CharacterClass.ID == 59))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Convoker, "ML1 Convoker");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Convokers, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Perfecter"://6,10,26,28,33,46,47,48,
					if ((player.CharacterClass.ID == 6 ||
					     player.CharacterClass.ID == 10 || player.CharacterClass.ID == 26 ||
					     player.CharacterClass.ID == 28 || player.CharacterClass.ID == 33 ||
					     player.CharacterClass.ID == 46 || player.CharacterClass.ID == 47 ||
					     player.CharacterClass.ID == 48))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Perfecter, "ML1 Perfecter");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Perfecters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Sojourner"://3,4,24,25,26,48,50,
					if ((player.CharacterClass.ID == 3 ||
					     player.CharacterClass.ID == 4 || player.CharacterClass.ID == 24 ||
					     player.CharacterClass.ID == 25 || player.CharacterClass.ID == 26 ||
					     player.CharacterClass.ID == 48 || player.CharacterClass.ID == 50))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Sojourner, "ML1 Sojourner");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Sojourners, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Spymaster"://9,23,49,
					if ((player.CharacterClass.ID == 9 ||
					     player.CharacterClass.ID == 23 || player.CharacterClass.ID == 49))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Spymaster, "ML1 Spymaster");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Spymasters, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Stormlord"://5,7,8,12,13,21,27,29,30,34,39,40,41,42,49,55,56,
					if ((player.CharacterClass.ID == 5 ||
					     player.CharacterClass.ID == 7 ||
					     player.CharacterClass.ID == 8 ||
					     player.CharacterClass.ID == 12 ||
					     player.CharacterClass.ID == 13 ||
					     player.CharacterClass.ID == 21 ||
					     player.CharacterClass.ID == 27 ||
					     player.CharacterClass.ID == 29 ||
					     player.CharacterClass.ID == 30 ||
					     player.CharacterClass.ID == 34 ||
					     player.CharacterClass.ID == 39 ||
					     player.CharacterClass.ID == 40 ||
					     player.CharacterClass.ID == 41 ||
					     player.CharacterClass.ID == 42 ||
					     player.CharacterClass.ID == 49 ||
					     player.CharacterClass.ID == 55 ||
					     player.CharacterClass.ID == 56))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Stormlord, "ML1 Stormlord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Stormlords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
				case "Warlord"://2,1,4,6,22,24,32,34,42,44,58,
					if ((player.CharacterClass.ID == 2 ||
					     player.CharacterClass.ID == 1 ||
					     player.CharacterClass.ID == 4 ||
					     player.CharacterClass.ID == 6 ||
					     player.CharacterClass.ID == 22 ||
					     player.CharacterClass.ID == 24 ||
					     player.CharacterClass.ID == 32 ||
					     player.CharacterClass.ID == 34 ||
					     player.CharacterClass.ID == 42 ||
					     player.CharacterClass.ID == 44 ||
					     player.CharacterClass.ID == 58 ||
					     player.CharacterClass.ID == 60 ||
					     player.CharacterClass.ID == 61 ||
					     player.CharacterClass.ID == 62))
					{
						InventoryItem item = player.TempProperties.getProperty<InventoryItem>(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM, null);
						
						if(item != null && player.Inventory.RemoveItem(item))
						{
							player.TempProperties.removeProperty(ML_ATLANTEAN_GLASS_NPC_HANDED_ITEM);
							StartPlayerMLLevel(player, eMLLine.Warlord, "ML1 Warlord");
						}
						else
						{
							player.Out.SendMessage("Hand me the ML1 Token first.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
					}
					else
					{
						player.Out.SendMessage("Your can not learn the way of Warlords, please choose another Ability.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					break;
					default: break;
			}

			return true;
		}

		public void SendReply(GamePlayer target, string msg)
		{
			target.Client.Out.SendMessage(
				msg,
				eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}
	}
}