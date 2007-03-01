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
 * 1) Travel to loc=10813,27525 Camelot Hills (Cotswold Village in the tavern) to speak with Godeleva Dowden
 * 2) Take his bucket and /use it in the river to scoop up some river water
 * 2) Take the full bucket back to Godeleva to have your reward
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
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

	public class GodelevasNeed : BaseQuest
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
		protected const string questTitle = "Godeleva's Need";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 6;

		private static GameNPC godelevaDowden = null;

		private static ItemTemplate woodenBucket= null;
		private static ItemTemplate fullWoodenBucket= null;
		
		private static ItemTemplate reedBracer = null;

		private static GameLocation cotswoldVillageBridge = new GameLocation("Bridge Location", 1, 557671, 512396, 1876);
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public GodelevasNeed() : base()
		{
		}

		public GodelevasNeed(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public GodelevasNeed(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public GodelevasNeed(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Godeleva Dowden", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				godelevaDowden = new GameNPC();
				godelevaDowden.Model = 7;
				godelevaDowden.Name = "Godeleva Dowden";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + godelevaDowden.Name + ", creating him ...");
				godelevaDowden.GuildName = "Part of " + questTitle + " Quest";
				godelevaDowden.Realm = (byte) eRealm.Albion;
				godelevaDowden.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 138);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 134);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 135);
				godelevaDowden.Inventory = template.CloseTemplate();
				godelevaDowden.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				godelevaDowden.Size = 48;
				godelevaDowden.Level = 40;
				godelevaDowden.X = 559528;
				godelevaDowden.Y = 510953;
				godelevaDowden.Z = 2488;
				godelevaDowden.Heading = 1217;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					godelevaDowden.SaveIntoDatabase();

				godelevaDowden.AddToWorld();
			}
			else
				godelevaDowden = npcs[0];

			#endregion

			#region defineItems

			// item db check
			woodenBucket = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "wooden_bucket");
			if (woodenBucket == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wooden Bucket, creating it ...");
				woodenBucket = new ItemTemplate();
				woodenBucket.Name = "Wooden Bucket";
				woodenBucket.Level = 1;
				woodenBucket.Weight = 10;
				woodenBucket.Model = 1610;

				woodenBucket.Object_Type = (int) eObjectType.GenericItem;
				woodenBucket.Id_nb = "wooden_bucket";
				woodenBucket.Gold = 0;
				woodenBucket.Silver = 0;
				woodenBucket.Copper = 0;
				woodenBucket.IsPickable = false;
				woodenBucket.IsDropable = false;
				
				woodenBucket.Quality = 100;
				woodenBucket.Condition = 1000;
				woodenBucket.MaxCondition = 1000;
				woodenBucket.Durability = 1000;
				woodenBucket.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(woodenBucket);
			}

			// item db check
			fullWoodenBucket = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "full_wooden_bucket");
			if (fullWoodenBucket == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Full Wooden Bucket, creating it ...");
				fullWoodenBucket = new ItemTemplate();
				fullWoodenBucket.Name = "Full Wooden Bucket";
				fullWoodenBucket.Level = 1;
				fullWoodenBucket.Weight = 250;
				fullWoodenBucket.Model = 1610;

				fullWoodenBucket.Object_Type = (int) eObjectType.GenericItem;
				fullWoodenBucket.Id_nb = "full_wooden_bucket";
				fullWoodenBucket.Gold = 0;
				fullWoodenBucket.Silver = 0;
				fullWoodenBucket.Copper = 0;
				fullWoodenBucket.IsPickable = false;
				fullWoodenBucket.IsDropable = false;
				
				fullWoodenBucket.Quality = 100;
				fullWoodenBucket.Condition = 1000;
				fullWoodenBucket.MaxCondition = 1000;
				fullWoodenBucket.Durability = 1000;
				fullWoodenBucket.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fullWoodenBucket);
			}

			// item db check
			reedBracer = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "reed_bracer");
			if (reedBracer == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Reed Bracer of Health creating it ...");
				reedBracer = new ItemTemplate();
				reedBracer.Name = "Reed Bracer";
				reedBracer.Level = 3;
				reedBracer.Weight = 1;
				reedBracer.Model = 598;

				reedBracer.Object_Type = (int) eObjectType.Magical;
				reedBracer.Item_Type = (int) eEquipmentItems.L_BRACER;
				reedBracer.Id_nb = "reed_bracer";

				reedBracer.Gold = 0;
				reedBracer.Silver = 0;
				reedBracer.Copper = 30;
				reedBracer.IsPickable = true;
				reedBracer.IsDropable = true;

				reedBracer.Bonus = 1;
				reedBracer.Bonus1Type = (int)eProperty.MaxHealth;
				reedBracer.Bonus1 = 8;
				reedBracer.Bonus2Type = (int)eProperty.Resist_Cold;
				reedBracer.Bonus2 = 1;

				reedBracer.Quality = 100;
				reedBracer.Condition = 1000;
				reedBracer.MaxCondition = 1000;
				reedBracer.Durability = 1000;
				reedBracer.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(reedBracer);
			}

			#endregion


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
			
			GameEventMgr.AddHandler(godelevaDowden, GameLivingEvent.Interact, new DOLEventHandler(TalkToGodelevaDowden));
			GameEventMgr.AddHandler(godelevaDowden, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGodelevaDowden));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			godelevaDowden.AddQuestToGive(typeof (GodelevasNeed));

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
			/* If sirQuait has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (godelevaDowden == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(godelevaDowden, GameLivingEvent.Interact, new DOLEventHandler(TalkToGodelevaDowden));
			GameEventMgr.RemoveHandler(godelevaDowden, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGodelevaDowden));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			godelevaDowden.RemoveQuestToGive(typeof (GodelevasNeed));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GodelevasNeed quest = player.IsDoingQuest(typeof (GodelevasNeed)) as GodelevasNeed;
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

			GodelevasNeed quest = player.IsDoingQuest(typeof (GodelevasNeed)) as GodelevasNeed;
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

		protected static void TalkToGodelevaDowden(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(godelevaDowden.CanGiveQuest(typeof (GodelevasNeed), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			GodelevasNeed quest = player.IsDoingQuest(typeof (GodelevasNeed)) as GodelevasNeed;

			godelevaDowden.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					godelevaDowden.SayTo(player, "Hello there friend.  I can't say I've seen you around here.  Of course, I'm usually so busy, I don't get a chance to chat with the locals like I used to.  Say, you're not [looking] for something to do, are you?");
					return;
				}
				else
				{
					if (quest.Step == 2)
					{
						godelevaDowden.SayTo(player, "Do you have the full bucket for me, "+player.Name+"?");
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
						case "looking":
							godelevaDowden.SayTo(player, "Oh yes!  I have met someone who is very interesting.  I think about him all the time when we are apart, which is often.  His name is Elvar Tambor, and he is a weapons merchant in Prydwen Keep.  We [met] about six months ago.");
							break;
						case "met":
							godelevaDowden.SayTo(player, "Great, because I have something that I need done.  I need to get some fresh water to clean my floors with, but I simply don't have time to run to the water to get some.  Will you [fetch] some for me?");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "fetch":
							player.Out.SendQuestSubscribeCommand(godelevaDowden, QuestMgr.GetIDForQuestType(typeof(GodelevasNeed)), "Will you take the bucket\ndown to the river and\nretrieve some water for\nGodeleva?\n[Level " + player.Level + "]");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(GodelevasNeed)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			GodelevasNeed quest = (GodelevasNeed) player.IsDoingQuest(typeof (GodelevasNeed));
			if (quest == null)
				return;

			if (quest.Step == 1)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == woodenBucket.Id_nb)
				{
					if (WorldMgr.CheckDistance(player, cotswoldVillageBridge, 500))
					{
						SendSystemMessage(player, "You use the wooden bucket and scoop up some fresh river water.");

						ReplaceItem(player, woodenBucket, fullWoodenBucket);
						quest.Step = 2;
					}
					else
					{
						SendSystemMessage(player, "You can't scoop up some river water here.");
					}
				}
			}
		}


		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (GodelevasNeed)) != null)
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
			GodelevasNeed quest = player.IsDoingQuest(typeof (GodelevasNeed)) as GodelevasNeed;

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
			if(godelevaDowden.CanGiveQuest(typeof (GodelevasNeed), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (GodelevasNeed)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!godelevaDowden.GiveQuest(typeof (GodelevasNeed), player, 1))
					return;

				// give letter                
				GiveItem(godelevaDowden, player, woodenBucket);

				SendReply(player, "Great!  Thanks a lot.  Here, take this bucket down the river's edge and use it to get me some water.  Bring the full bucket back to me.  Hurry now!  I need to get things cleaned up!");

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
						return "[Step #1] Take the bucket to the river and USE it to get some water.  The default key for USING items is E or F4.";
					case 2:
						return "[Step #2] Take the full bucket back to Godeleva in the tavern in Cotswold.";
				}
				return base.Description;
			}
		}


		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (GodelevasNeed)) == null)
				return;

			if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 2)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == godelevaDowden.Name && gArgs.Item.Id_nb == fullWoodenBucket.Id_nb)
					{
						RemoveItem(godelevaDowden, m_questPlayer, fullWoodenBucket);

						godelevaDowden.TurnTo(m_questPlayer);
						godelevaDowden.SayTo(m_questPlayer, "Ah, great!  This is a good and full bucket!  Thank you friend!  Here is a little something for you.  A traveler gave it to me, but it's not much use to me now.  It will serve you better.  Now, I'm off to clean.  Be safe friend!");
						
						FinishQuest();
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, woodenBucket, false);
			RemoveItem(m_questPlayer, fullWoodenBucket, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			GiveItem(godelevaDowden, m_questPlayer, reedBracer);

			m_questPlayer.GainExperience(40 + (m_questPlayer.Level - 1) * 4, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 7, Util.Random(50)), "You are awarded 7 silver and some copper!");
		
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}
	}
}
