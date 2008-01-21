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
 * 1) Travel to loc=10725,27708 Camelot Hills (Cotswold Village) to speak with Andrew Wyatt
 * 2) Go to loc=29164,22747 City of Camelot and give the bundle of bear skins to Geor Nadren. 
 * 3) Go to loc=28607,22593 City of Camelot and ask some threads to Ver Nuren (/whisper thread) (you gain some xp and money).
 * 4) Can back to Geor Nadren and give it a thread (you gain some xp and money).
 * 5) Can back to Andrew Wyatt at Camelot Hills (Cotswold Village) (/whisper finished) to have your reward.
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
 * GS Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class AndrewsSkins : BaseQuest
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
		protected const string questTitle = "Andrew's Skins";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 5;

		private static GameNPC andrewWyatt = null;
		private static GameNPC georNadren = null;
		private static GameNPC verNuren = null;

		private static ItemTemplate bundleOfBearSkins = null;
		private static ItemTemplate spoolOfLeatherworkingThread = null;

		private static ItemTemplate chokerOfTheBear = null;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public AndrewsSkins() : base()
		{
		}

		public AndrewsSkins(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public AndrewsSkins(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public AndrewsSkins(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Andrew Wyatt", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob GS, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				andrewWyatt = new GameNPC();
				andrewWyatt.Model = 80;
				andrewWyatt.Name = "Andrew Wyatt";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + andrewWyatt.Name + ", creating him ...");
				andrewWyatt.GuildName = "Part of " + questTitle + " Quest";
				andrewWyatt.Realm = eRealm.Albion;
				andrewWyatt.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 80);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 54);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 51);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 52);
				andrewWyatt.Inventory = template.CloseTemplate();
				andrewWyatt.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				andrewWyatt.Size = 48;
				andrewWyatt.Level = 30;
				andrewWyatt.X = 559590;
				andrewWyatt.Y = 511039;
				andrewWyatt.Z = 2488;
				andrewWyatt.Heading = 1524;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					andrewWyatt.SaveIntoDatabase();

				andrewWyatt.AddToWorld();
			}
			else
				andrewWyatt = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Geor Nadren", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Geor Nadren, creating him ...");
				georNadren = new GameNPC();
				georNadren.Model = 9;
				georNadren.Name = "Geor Nadren";
				georNadren.GuildName = "Part of " + questTitle + " Quest";
				georNadren.Realm = eRealm.Albion;
				georNadren.CurrentRegionID = 10;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
				georNadren.Inventory = template.CloseTemplate();
				georNadren.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				georNadren.Size = 51;
				georNadren.Level = 8;
				georNadren.X = 37355;
				georNadren.Y = 30943;
				georNadren.Z = 8002;
				georNadren.Heading = 3231;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					georNadren.SaveIntoDatabase();

				georNadren.AddToWorld();
			}
			else
				georNadren = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Ver Nuren", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ver Nuren, creating him ...");
				verNuren = new GameNPC();
				verNuren.Model = 9;
				verNuren.Name = "Ver Nuren";
				verNuren.GuildName = "Part of " + questTitle + " Quest";
				verNuren.Realm = eRealm.Albion;
				verNuren.CurrentRegionID = 10;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
				verNuren.Inventory = template.CloseTemplate();
				verNuren.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				verNuren.Size = 51;
				verNuren.Level = 8;
				verNuren.X = 36799;
				verNuren.Y = 30786;
				verNuren.Z = 8010;
				verNuren.Heading = 625;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					verNuren.SaveIntoDatabase();

				verNuren.AddToWorld();
			}
			else
				verNuren = npcs[0];

			#endregion
			
			#region defineItems

			// item db check
			bundleOfBearSkins = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "bundle_of_bear_skins");
			if (bundleOfBearSkins == null)
			{
				bundleOfBearSkins = new ItemTemplate();
				bundleOfBearSkins.Name = "Bundle of Bear Skins";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+bundleOfBearSkins.Name+", creating it ...");
				bundleOfBearSkins.Level = 0;
				bundleOfBearSkins.Weight = 0;
				bundleOfBearSkins.Model = 100;

				bundleOfBearSkins.Object_Type = (int) eObjectType.GenericItem;
				bundleOfBearSkins.Id_nb = "bundle_of_bear_skins";
				bundleOfBearSkins.Gold = 0;
				bundleOfBearSkins.Silver = 0;
				bundleOfBearSkins.Copper = 0;
				bundleOfBearSkins.IsPickable = false;
				bundleOfBearSkins.IsDropable = false;
				
				bundleOfBearSkins.Quality = 100;
				bundleOfBearSkins.Condition = 1000;
				bundleOfBearSkins.MaxCondition = 1000;
				bundleOfBearSkins.Durability = 1000;
				bundleOfBearSkins.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bundleOfBearSkins);
			}

			// item db check
			spoolOfLeatherworkingThread = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "spool_of_leatherworking_thread");
			if (spoolOfLeatherworkingThread == null)
			{
				spoolOfLeatherworkingThread = new ItemTemplate();
				spoolOfLeatherworkingThread.Name = "Spool of Leatherworking Thread";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+spoolOfLeatherworkingThread.Name+", creating it ...");
				spoolOfLeatherworkingThread.Level = 0;
				spoolOfLeatherworkingThread.Weight = 0;
				spoolOfLeatherworkingThread.Model = 537;

				spoolOfLeatherworkingThread.Object_Type = (int) eObjectType.GenericItem;
				spoolOfLeatherworkingThread.Id_nb = "spool_of_leatherworking_thread";
				spoolOfLeatherworkingThread.Gold = 0;
				spoolOfLeatherworkingThread.Silver = 0;
				spoolOfLeatherworkingThread.Copper = 0;
				spoolOfLeatherworkingThread.IsPickable = false;
				spoolOfLeatherworkingThread.IsDropable = false;

				spoolOfLeatherworkingThread.Quality = 100;
				spoolOfLeatherworkingThread.Condition = 1000;
				spoolOfLeatherworkingThread.MaxCondition = 1000;
				spoolOfLeatherworkingThread.Durability = 1000;
				spoolOfLeatherworkingThread.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(spoolOfLeatherworkingThread);
			}

			// item db check
			chokerOfTheBear = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "choker_of_the_bear");
			if (chokerOfTheBear == null)
			{
				chokerOfTheBear = new ItemTemplate();
				chokerOfTheBear.Name = "Choker of the Bear";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+chokerOfTheBear.Name+", creating it ...");
				
				chokerOfTheBear.Level = 5;
				chokerOfTheBear.Weight = 6;
				chokerOfTheBear.Model = 101;

				chokerOfTheBear.Object_Type = (int) eObjectType.Magical;
				chokerOfTheBear.Item_Type = (int) eEquipmentItems.NECK;
				chokerOfTheBear.Id_nb = "choker_of_the_bear";

				chokerOfTheBear.Gold = 0;
				chokerOfTheBear.Silver = 0;
				chokerOfTheBear.Copper = 30;
				chokerOfTheBear.IsPickable = true;
				chokerOfTheBear.IsDropable = true;

				chokerOfTheBear.Bonus = 1;
				chokerOfTheBear.Bonus1Type = (int)eProperty.Strength;
				chokerOfTheBear.Bonus1 = 4;
				chokerOfTheBear.Bonus2Type = (int)eProperty.Resist_Thrust;
				chokerOfTheBear.Bonus2 = 1;

				chokerOfTheBear.Quality = 100;
				chokerOfTheBear.Condition = 1000;
				chokerOfTheBear.MaxCondition = 1000;
				chokerOfTheBear.Durability = 1000;
				chokerOfTheBear.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(chokerOfTheBear);
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
			
			GameEventMgr.AddHandler(andrewWyatt, GameLivingEvent.Interact, new DOLEventHandler(TalkToAndrewWyatt));
			GameEventMgr.AddHandler(andrewWyatt, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAndrewWyatt));

			GameEventMgr.AddHandler(georNadren, GameObjectEvent.Interact, new DOLEventHandler(TalkToGeorNadren));
			GameEventMgr.AddHandler(georNadren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGeorNadren));

			GameEventMgr.AddHandler(verNuren, GameObjectEvent.Interact, new DOLEventHandler(TalkToVerNuren));
			GameEventMgr.AddHandler(verNuren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVerNuren));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			andrewWyatt.AddQuestToGive(typeof (AndrewsSkins));

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
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			/* If Andrew Wyatt has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (andrewWyatt == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(andrewWyatt, GameLivingEvent.Interact, new DOLEventHandler(TalkToAndrewWyatt));
			GameEventMgr.RemoveHandler(andrewWyatt, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAndrewWyatt));

			GameEventMgr.RemoveHandler(georNadren, GameObjectEvent.Interact, new DOLEventHandler(TalkToGeorNadren));
			GameEventMgr.RemoveHandler(georNadren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGeorNadren));
			
			GameEventMgr.RemoveHandler(verNuren, GameObjectEvent.Interact, new DOLEventHandler(TalkToVerNuren));
			GameEventMgr.RemoveHandler(verNuren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVerNuren));
			
			/* Now we remove to Ydenia the possibility to give this quest to players */
			andrewWyatt.RemoveQuestToGive(typeof (AndrewsSkins));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToAndrewWyatt(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(andrewWyatt.CanGiveQuest(typeof (AndrewsSkins), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			AndrewsSkins quest = player.IsDoingQuest(typeof (AndrewsSkins)) as AndrewsSkins;

			andrewWyatt.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					andrewWyatt.SayTo(player, "Greetings friend.  I am Andrew Wyatt, local hunter in these parts.  You must be a fresh, young adventurer, aren't you?  Well then, I might have an [errand] for you to run.");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						andrewWyatt.SayTo(player, "Ah, back so soon friend?  Well then, I take it you [finished] my errand?");
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
						case "errand":
							andrewWyatt.SayTo(player, "Aye friend.  I hunt all manner of creatures, but my specialty is bears.  I hunt every type of bear out there.  I use every part of them too.  I need to run my next shipment of skins to Geor Nadren in [Camelot].");
							break;
						case "Camelot":
							andrewWyatt.SayTo(player, "I'll tell you what.  If you take my skins to him, I'll set you up with a little reward for your troubles.  I'm enjoying my time here in the tavern, and I'd like to stay a little longer.  What do you say?  Are you [up] for it or not?");
							break;
					
							//If the player offered his help, we send the quest dialog now!
						case "up":
							player.Out.SendQuestSubscribeCommand(andrewWyatt, QuestMgr.GetIDForQuestType(typeof(AndrewsSkins)), "Will you deliver these skins to \nGeor Nadren in Camelot City?\n[Level " + player.Level + "]");
							break;
					}
				}
				else
				{	
					switch (wArgs.Text)
					{
						case "finished":
							if(quest.Step == 4)
							{
								andrewWyatt.SayTo(player, "I knew I could count on you.  All right then, as promised, a small reward for your troubles.  Use it well, and good luck "+player.Name+".  Perhaps I'll have some other work for you in the future.");
								quest.FinishQuest();
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AndrewsSkins)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToGeorNadren(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			AndrewsSkins quest = player.IsDoingQuest(typeof (AndrewsSkins)) as AndrewsSkins;

			georNadren.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch(quest.Step)
					{
						case 1:
							georNadren.SayTo(player, "Hello there friend.  How may I be of service today?");
							break;

						case 3:
							georNadren.SayTo(player, "Welcome back friend.  Did you get the thread I need?");
							break;
					}
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "errand":
							if(quest.Step == 2)
							{
								georNadren.SayTo(player, "I need a few spools of thread in order to continue making the armor I sell.  Ver Nuren's wife makes the thread I use.  Go ask him if he has any for me.  Come back to me when you have some.");
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToVerNuren(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			AndrewsSkins quest = player.IsDoingQuest(typeof (AndrewsSkins)) as AndrewsSkins;

			verNuren.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if(quest.Step < 3)
					{
						//Player is doing the quest...
						verNuren.SayTo(player, "Greetings to you traveler.  Is there something I can help you with today?");
						return;
					}
				}
			}
				// The player whispered to NPC (clicked on the text inside the [] or with /whisper)
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "thread":
							if(quest.Step == 2)
							{
								verNuren.SayTo(player, "Oh yes.  I know what you're talking about.  Geor told me the other day that he was out of the thread he needs to make his armor.  No worries.  I have some right here.  Now please, take this straight to Geor.");

								GiveItem(verNuren, player, spoolOfLeatherworkingThread);

								player.GainExperience(40, true);
								player.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You are awarded 3 silver and some copper!");

								quest.Step = 3;
							}
							break;
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
			if (player.IsDoingQuest(typeof (AndrewsSkins)) != null)
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
			AndrewsSkins quest = player.IsDoingQuest(typeof (AndrewsSkins)) as AndrewsSkins;

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
			if(andrewWyatt.CanGiveQuest(typeof (AndrewsSkins), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (AndrewsSkins)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!andrewWyatt.GiveQuest(typeof (AndrewsSkins), player, 1))
					return;

				// give letter                
				GiveItem(andrewWyatt, player, bundleOfBearSkins);

				SendReply(player, "Great, I knew you'd help me out.  All right friend, here are the skins.  Take them to Geor Nadren inside Camelot.  He'll be happy to see you.");
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
						return "[Step #1] Take the skins that Andrew gave you to Geor Nadren in Camelot City.";
					case 2:
						return "[Step #2] Ver Nuren in Camelot City has some of the thread that Geor needs to make his armor.  Find him and ask if he has any [thread].";
					case 3:
						return "[Step #3] Take the spools of thread back to Geor in Camelot City.";
					case 4:
						return "[Step #4] Return to Andrew Wyatt in Cotswold Village.  Tell him you have [finished] his errand.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (AndrewsSkins)) == null)
				return;


			if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 1)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == georNadren.Name && gArgs.Item.Id_nb == bundleOfBearSkins.Id_nb)
					{
						RemoveItem(georNadren, m_questPlayer, bundleOfBearSkins);

						georNadren.TurnTo(m_questPlayer);
						georNadren.SayTo(m_questPlayer, "Ah!  These must be the skins I've been waiting for from Andrew.  My, my, these are very high quality.  He's a very skilled hunter indeed, with a fine eye for good pelts.  Excellent!  I have but one more [errand] I need for you to run for me.");
						
						Step = 2;
						return;
					}
				}
				else if(Step == 3)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == georNadren.Name && gArgs.Item.Id_nb == spoolOfLeatherworkingThread.Id_nb)
					{
						RemoveItem(georNadren, m_questPlayer, spoolOfLeatherworkingThread);

						georNadren.TurnTo(m_questPlayer);
						georNadren.SayTo(m_questPlayer, "Excellent!  Why, there is enough here to make several suits of armor.  Thank you friend!  Now, I think you need to return to Andrew in Cotswold and let him know I received the skins.  Thank you again, and good journeys to you "+m_questPlayer.Name+".");
						
						m_questPlayer.GainExperience(40, true);
						m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You are awarded 2 silver and some copper!");

						Step = 4;
						return;
					}
				}


			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, bundleOfBearSkins, false);
			RemoveItem(m_questPlayer, spoolOfLeatherworkingThread, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			GiveItem(andrewWyatt, m_questPlayer, chokerOfTheBear);

			m_questPlayer.GainExperience(80, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You are awarded 4 silver and some copper!");
		}
	}
}
