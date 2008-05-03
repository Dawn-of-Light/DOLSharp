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
/*
 * Author:		Doulbousiouf
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=15075,25906 Camelot Hills to speak with Farmer Asma and take is plan
 * 2) Go to the first vacant field at loc=19590,20668 Camelot Hills (you can /use the plan for more infos).
 * 3) Go to the second vacant field at loc=25093,25428 Camelot Hills (you can /use the plan for more infos).
 * 4) Go to the third vacant field at loc=28083,29814 Camelot Hills (you can /use the plan for more infos).
 * 3) Come back to Farmer Asma to have have your reward. 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class GreenerPastures : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		 * You can declare static variables here, which will be available in 
		 * ALL instance of your quest and should be initialized ONLY ONCE inside
		 * the OnScriptLoaded method.
		 * 
		 * Or declare nonstatic variables here which can be unique for each Player
		 * and change through the quest journey...
		 * 
		 */
		protected const string questTitle = "Greener Pastures";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 5;

		private static GameNPC farmerAsma = null;

		private static ItemTemplate farmerAsmasMap = null;

		private static GameLocation firstField = new GameLocation("First Field", 1, 568278, 504052, 2168);
		private static GameLocation secondField = new GameLocation("Second Field", 1, 573718, 509044, 2192);
		private static GameLocation thirdField = new GameLocation("Third Field", 1, 577336, 513324, 2169);

		private static IArea firstFieldArea = null;
		private static IArea secondFieldArea = null;
		private static IArea thirdFieldArea = null;
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public GreenerPastures() : base()
		{
		}

		public GreenerPastures(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public GreenerPastures(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public GreenerPastures(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		/* The following method is called automatically when this quest class
		 * is loaded. You might notice that this method is the same as in standard
		 * game events. And yes, quests basically are game events for single players
		 * 
		 * To make this method automatically load, we have to declare it static
		 * and give it the [ScriptLoadedEvent] attribute. 
		 *
		 * Inside this method we initialize the quest. This is neccessary if we 
		 * want to set the quest hooks to the NPCs.
		 * 
		 * If you want, you can however add a quest to the player from ANY place
		 * inside your code, from events, from custom items, from anywhere you
		 * want. 
		 */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");
			/* First thing we do in here is to search for the NPCs inside
			* the world who comes from the certain Realm. If we find a the players,
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/
			
			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Farmer Asma", eRealm.Albion);
			if (npcs.Length == 0)
			{
				farmerAsma = new GameNPC();
				farmerAsma.Model = 82;
				farmerAsma.Name = "Farmer Asma";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + farmerAsma.Name + ", creating him ...");
				farmerAsma.GuildName = "Part of " + questTitle + " Quest";
				farmerAsma.Realm = eRealm.Albion;
				farmerAsma.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 31);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 32);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 33);
				farmerAsma.Inventory = template.CloseTemplate();
				farmerAsma.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				farmerAsma.Size = 50;
				farmerAsma.Level = 35;
				farmerAsma.X = 563939;
				farmerAsma.Y = 509234;
				farmerAsma.Z = 2744 ;
				farmerAsma.Heading = 21;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					farmerAsma.SaveIntoDatabase();

				farmerAsma.AddToWorld();
			}
			else
				farmerAsma = npcs[0];

			#endregion
			
			#region defineItems

			// item db check
			farmerAsmasMap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "farmer_asma_map");
			if (farmerAsmasMap == null)
			{
				farmerAsmasMap = new ItemTemplate();
				farmerAsmasMap.Name = "Farmer Asma's Map";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+farmerAsmasMap.Name+", creating it ...");
				farmerAsmasMap.Level = 0;
				farmerAsmasMap.Weight = 1;
				farmerAsmasMap.Model = 499;

				farmerAsmasMap.Object_Type = (int) eObjectType.GenericItem;
				farmerAsmasMap.Id_nb = "farmer_asma_map";
				farmerAsmasMap.Gold = 0;
				farmerAsmasMap.Silver = 0;
				farmerAsmasMap.Copper = 0;
				farmerAsmasMap.IsPickable = false;
				farmerAsmasMap.IsDropable = false;
				
				farmerAsmasMap.Quality = 100;
				farmerAsmasMap.Condition = 1000;
				farmerAsmasMap.MaxCondition = 1000;
				farmerAsmasMap.Durability = 1000;
				farmerAsmasMap.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(farmerAsmasMap);
			}

			#endregion


			firstFieldArea = WorldMgr.GetRegion(firstField.RegionID).AddArea(new Area.Circle("First Vacant Field", firstField.X, firstField.Y, 0, 1450));
			firstFieldArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterFirstFieldArea));

			secondFieldArea = WorldMgr.GetRegion(secondField.RegionID).AddArea(new Area.Circle("Second Vacant Field", secondField.X, secondField.Y, 0, 1100));
			secondFieldArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterSecondFieldArea));

			thirdFieldArea = WorldMgr.GetRegion(thirdField.RegionID).AddArea(new Area.Circle("Third Vacant Field", thirdField.X, thirdField.Y, 0, 1100));
			thirdFieldArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterThirdFieldArea));
			
			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));
			
			GameEventMgr.AddHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.AddHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			farmerAsma.AddQuestToGive(typeof (GreenerPastures));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}
		
		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			/* If Farmer Asma has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (farmerAsma == null)
				return;

			firstFieldArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterFirstFieldArea));
			WorldMgr.GetRegion(firstField.RegionID).RemoveArea(firstFieldArea);

			secondFieldArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterSecondFieldArea));
			WorldMgr.GetRegion(secondField.RegionID).RemoveArea(secondFieldArea);

			thirdFieldArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterThirdFieldArea));
			WorldMgr.GetRegion(thirdField.RegionID).RemoveArea(thirdFieldArea);

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.RemoveHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));
			
			/* Now we remove to Ydenia the possibility to give this quest to players */
			farmerAsma.RemoveQuestToGive(typeof (GreenerPastures));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToFarmerAsma(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(farmerAsma.CanGiveQuest(typeof (GreenerPastures), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;

			farmerAsma.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					farmerAsma.SayTo(player, "Good night. I wish I had time to talk, "+player.CharacterClass.Name+", but I'm in the process of trying to find a new field to lease. I'd like to return to my life as a farmer. It's not that Cotswold isn't a nice village, but I feel more at home in the [field].");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						farmerAsma.SayTo(player, "Ah, you've returned. I hope you were able to find the fields without too much difficulty. I'm still learning my way around the area.  Which field would you recommend renting?");
						SendMessage(player, "[I'd recommend the first field.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						SendMessage(player, "[The second field is best.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						SendMessage(player, "[You should rent the third one.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "field":
							farmerAsma.SayTo(player, "Ah, yes, Camelot Hills, where the wind comes sweepin' down the plain, and the wavin' barley can sure smell sweet when the wind comes right behind the rain. I have a lead on some fields that are up for lease, but I don't have time to [check them out].");
							break;
						case "check them out":
							farmerAsma.SayTo(player, "Would you be willing to take a look at these fields for me and let me know if you think they are worth leasing?");
							//If the player offered his help, we send the quest dialog now!
							player.Out.SendQuestSubscribeCommand(farmerAsma, QuestMgr.GetIDForQuestType(typeof(GreenerPastures)), "Will you help Farmer Asma \nsearch for new farmland?\n[Level 2-5]");
							break;
					}
				}
				else
				{	
					switch (wArgs.Text)
					{
						case "them":
							if(quest.Step < 4)
							{
								farmerAsma.SayTo(player, "When you're done taking a look at the fields, please return to me and let me know what you saw.");
							}
							break;

						case "I'd recommend the first field.":
						case "The second field is best.":
						case "You should rent the third one.":
							if(quest.Step == 4)
							{
								farmerAsma.SayTo(player, "Excellent. I'll speak to the owner tomorrow. May I have the map back?");
							}
							break;

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(GreenerPastures)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			GreenerPastures quest = (GreenerPastures) player.IsDoingQuest(typeof (GreenerPastures));
			if (quest == null)
				return;
		
			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Id_nb == farmerAsmasMap.Id_nb)
			{
				switch (quest.Step)
				{
					case 1:
						SendMessage(player, "To find the first vacant field, travel a short distance north from the Shrouded Isles portal.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;

					case 2:
						SendMessage(player, "Farmer Asma's map shows that the second field located across the road to the east southeast of the first vacant field.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;

					case 3:
						SendMessage(player, "You open Farmer Asma's Map and discover that the last field is a short trip to the southeast from the second field.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;
				}
			}
		}

		protected static void PlayerEnterFirstFieldArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;

			if (quest != null && quest.Step == 1)
			{
				player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the first field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
				quest.Step = 2;
			}
		}

		protected static void PlayerEnterSecondFieldArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;

			if (quest != null && quest.Step == 2)
			{
				player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the next field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
				quest.Step = 3;
			}
		}

		protected static void PlayerEnterThirdFieldArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;

			if (quest != null && quest.Step == 3)
			{
				player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the last field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
				quest.Step = 4;
			}
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (GreenerPastures)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		
		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			GreenerPastures quest = player.IsDoingQuest(typeof (GreenerPastures)) as GreenerPastures;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, no go out there and finish your work!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(farmerAsma.CanGiveQuest(typeof (GreenerPastures), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (GreenerPastures)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!farmerAsma.GiveQuest(typeof (GreenerPastures), player, 1))
					return;

				// give map                
				GiveItem(farmerAsma, player, farmerAsmasMap);

				SendReply(player, "Thank you for agreeing to help.  A man in Cotswold was kind enough to draw a map of some vacant fields in the area. I'll give you the map so that you can travel to the fields and take a look at [them].");

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		/* Now we set the quest name.
		 * If we don't override the base method, then the quest
		 * will have the name "UNDEFINED QUEST NAME" and we don't
		 * want that, do we? ;-)
		 */

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		 * If we don't override the base method, then the quest
		 * description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		 * and this isn't something nice either ;-)
		 */

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Find the first field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 2:
						return "[Step #2] Now that you've inspected the first field, find the second field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 3:
						return "[Step #3] You've completed your inspections of the first two fields. Now, find the third field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 4:
						return "[Step #4] You've taken a look at the three fields Farmer Asma asked you to inspect. Return to her in the camp near the Shrouded Isles portal, and tell her your findings.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (GreenerPastures)) == null)
				return;


			if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == farmerAsma.Name && gArgs.Item.Id_nb == farmerAsmasMap.Id_nb)
					{
						RemoveItem(farmerAsma, m_questPlayer, farmerAsmasMap);

						farmerAsma.TurnTo(m_questPlayer);
						farmerAsma.SayTo(m_questPlayer, "Thank you for you help. I can only offer you a small bit of coin as a reward for your assistance, but I am grateful for your advice.");
						
						FinishQuest();
						return;
					}
				}
			}
		}


		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, farmerAsmasMap, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			m_questPlayer.GainExperience(m_questPlayer.Level * 10, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 35 + m_questPlayer.Level), "You are awarded "+(35+m_questPlayer.Level)+" copper!");
		
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}
	}
}
