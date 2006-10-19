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
 * 1) Travel to loc=10813,27525 Camelot Hills (Cotswold Village) to speak with Ydenia Philpott and get his letter
 * 2) Go to loc=25414,47426 Prydwen Keep and give the letter to Elvar Tambor 
 * 3) Take Elvar's letter back to Ydenia in Cotswold Village.  She is in the tavern there. 
 * 4) Deliver the letter to Ydenia and ask her for your reward (/whisper something).
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

	public class YdeniasCrush : BaseQuest
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
		protected const string questTitle = "Ydenia's Crush";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;

		private static GameNPC ydeniaPhilpott = null;
		private static GameNPC elvarTambor = null;

		private static ItemTemplate letterToElvar= null;
		private static ItemTemplate letterToYdenia = null;

		private static ItemTemplate silverRingOfHealth = null;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public YdeniasCrush() : base()
		{
		}

		public YdeniasCrush(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public YdeniasCrush(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public YdeniasCrush(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Ydenia Philpott", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				ydeniaPhilpott = new GameMob();
				ydeniaPhilpott.Model = 6;
				ydeniaPhilpott.Name = "Ydenia Philpott";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + ydeniaPhilpott.Name + ", creating him ...");
				ydeniaPhilpott.GuildName = "Part of " + questTitle + " Quest";
				ydeniaPhilpott.Realm = (byte) eRealm.Albion;
				ydeniaPhilpott.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 227);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 80);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 54);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 51);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 52);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 53);
				ydeniaPhilpott.Inventory = template.CloseTemplate();
				ydeniaPhilpott.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				ydeniaPhilpott.Size = 51;
				ydeniaPhilpott.Level = 40;
				ydeniaPhilpott.X = 559315;
				ydeniaPhilpott.Y = 510705;
				ydeniaPhilpott.Z = 2488;
				ydeniaPhilpott.Heading = 3993;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					ydeniaPhilpott.SaveIntoDatabase();

				ydeniaPhilpott.AddToWorld();
			}
			else
				ydeniaPhilpott = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Elvar Tambor", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Elvar Tambor, creating him ...");
				elvarTambor = new GameMob();
				elvarTambor.Model = 9;
				elvarTambor.Name = "Elvar Tambor";
				elvarTambor.GuildName = "Part of " + questTitle + " Quest";
				elvarTambor.Realm = (byte) eRealm.Albion;
				elvarTambor.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 159, 67);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 160, 63);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 156, 67);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 157, 63);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 158, 67);
				elvarTambor.Inventory = template.CloseTemplate();
				elvarTambor.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				elvarTambor.Size = 50;
				elvarTambor.Level = 15;
				elvarTambor.X = 574711;
				elvarTambor.Y = 529887;
				elvarTambor.Z = 2896;
				elvarTambor.Heading = 2366;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					elvarTambor.SaveIntoDatabase();

				elvarTambor.AddToWorld();
			}
			else
				elvarTambor = npcs[0];

			#endregion

			#region defineItems

			// item db check
			letterToElvar = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "letter_to_elvar_tambor");
			if (letterToElvar == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Letter to Healvar, creating it ...");
				letterToElvar = new ItemTemplate();
				letterToElvar.Name = "Letter to Elvar";
				letterToElvar.Level = 0;
				letterToElvar.Weight = 10;
				letterToElvar.Model = 499;

				letterToElvar.Object_Type = (int) eObjectType.GenericItem;
				letterToElvar.Id_nb = "letter_to_elvar_tambor";
				letterToElvar.Gold = 0;
				letterToElvar.Silver = 0;
				letterToElvar.Copper = 0;
				letterToElvar.IsPickable = false;
				letterToElvar.IsDropable = false;
				
				letterToElvar.Quality = 100;
				letterToElvar.MaxQuality = 100;
				letterToElvar.Condition = 1000;
				letterToElvar.MaxCondition = 1000;
				letterToElvar.Durability = 1000;
				letterToElvar.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterToElvar);
			}

			// item db check
			letterToYdenia = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "letter_to_yderia_philpott");
			if (letterToYdenia == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Letter to Yderia creating it ...");
				letterToYdenia = new ItemTemplate();
				letterToYdenia.Name = "Letter to Ydenia";
				letterToYdenia.Level = 0;
				letterToYdenia.Weight = 10;
				letterToYdenia.Model = 499;

				letterToYdenia.Object_Type = (int) eObjectType.GenericItem;
				letterToYdenia.Id_nb = "letter_to_yderia_philpott";
				letterToYdenia.Gold = 0;
				letterToYdenia.Silver = 0;
				letterToYdenia.Copper = 0;
				letterToYdenia.IsPickable = false;
				letterToYdenia.IsDropable = false;

				letterToYdenia.Quality = 100;
				letterToYdenia.MaxQuality = 100;
				letterToYdenia.Condition = 1000;
				letterToYdenia.MaxCondition = 1000;
				letterToYdenia.Durability = 1000;
				letterToYdenia.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterToYdenia);
			}

			// item db check
			silverRingOfHealth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "silver_ring_of_health");
			if (silverRingOfHealth == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Silver Ring of Health creating it ...");
				silverRingOfHealth = new ItemTemplate();
				silverRingOfHealth.Name = "Silver Ring of Health";
				silverRingOfHealth.Level = 3;
				silverRingOfHealth.Weight = 1;
				silverRingOfHealth.Model = 103;

				silverRingOfHealth.Object_Type = (int) eObjectType.Magical;
				silverRingOfHealth.Item_Type = (int) eEquipmentItems.L_RING;
				silverRingOfHealth.Id_nb = "silver_ring_of_health";

				silverRingOfHealth.Gold = 0;
				silverRingOfHealth.Silver = 0;
				silverRingOfHealth.Copper = 30;
				silverRingOfHealth.IsPickable = true;
				silverRingOfHealth.IsDropable = true;

				silverRingOfHealth.Bonus = 1;
				silverRingOfHealth.Bonus1Type = (int)eProperty.MaxHealth;
				silverRingOfHealth.Bonus1 = 8;
				silverRingOfHealth.Bonus2Type = (int)eProperty.Resist_Slash;
				silverRingOfHealth.Bonus2 = 1;

				silverRingOfHealth.Quality = 100;
				silverRingOfHealth.MaxQuality = 100;
				silverRingOfHealth.Condition = 1000;
				silverRingOfHealth.MaxCondition = 1000;
				silverRingOfHealth.Durability = 1000;
				silverRingOfHealth.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(silverRingOfHealth);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(ydeniaPhilpott, GameLivingEvent.Interact, new DOLEventHandler(TalkToYdeniaPhilpott));
			GameEventMgr.AddHandler(ydeniaPhilpott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYdeniaPhilpott));

			GameEventMgr.AddHandler(elvarTambor, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarTambor));
			GameEventMgr.AddHandler(elvarTambor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarTambor));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			ydeniaPhilpott.AddQuestToGive(typeof (YdeniasCrush));

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
			if (ydeniaPhilpott == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(ydeniaPhilpott, GameLivingEvent.Interact, new DOLEventHandler(TalkToYdeniaPhilpott));
			GameEventMgr.RemoveHandler(ydeniaPhilpott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYdeniaPhilpott));

			GameEventMgr.RemoveHandler(elvarTambor, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarTambor));
			GameEventMgr.RemoveHandler(elvarTambor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarTambor));
			
			/* Now we remove to Ydenia the possibility to give this quest to players */
			ydeniaPhilpott.RemoveQuestToGive(typeof (YdeniasCrush));
		}


		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToYdeniaPhilpott(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(ydeniaPhilpott.CanGiveQuest(typeof (YdeniasCrush), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			YdeniasCrush quest = player.IsDoingQuest(typeof (YdeniasCrush)) as YdeniasCrush;

			ydeniaPhilpott.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					ydeniaPhilpott.SayTo(player, "Hello there!  Are you new here?  My name is Ydenia.  I am a minstrel.  I lost my husband two years ago in a great battle in Pennine Mountains.  I was sad for a very long time, but now, I am [happy] again.");
					return;
				}
				else
				{
					if (quest.Step == 3)
					{
						ydeniaPhilpott.SayTo(player, "Oh!  Thank you for delivering my letter for me!  Did he have one in return?");
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
						case "happy":
							ydeniaPhilpott.SayTo(player, "Oh yes!  I have met someone who is very interesting.  I think about him all the time when we are apart, which is often.  His name is Elvar Tambor, and he is a weapons merchant in Prydwen Keep.  We [met] about six months ago.");
							break;
						case "met":
							ydeniaPhilpott.SayTo(player, "He came to Cotswold to do some business with the other merchants here and came into the tavern for a drink.  He listened to me play and then came over and talked with me when I was done.  He said he would like to [write] to me");
							break;
						case "write":
							ydeniaPhilpott.SayTo(player, "So we have been writing to each other ever since.  I rarely get the chance to go to Prydwen Keep, so it's just easier if I send someone over there to deliver the letters for me.  Say, how would you like to earn a [few silvers] and deliver this for me");
							break;
				
							//If the player offered his help, we send the quest dialog now!
						case "few silvers":
							player.Out.SendCustomDialog("Will you deliver the letter to \nElvar Tambor for Ydenia?\n[Level "+player.Level+"]", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{	
					switch (wArgs.Text)
					{
						case "something":
							if(quest.Step == 4)
							{
								ydeniaPhilpott.SayTo(player, "I found this in my belongings.  I don't use it anymore, so I thought you could use it.  Thank you again Gwonn.  You have made both of us very happy today.");
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


		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToElvarTambor(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			YdeniasCrush quest = player.IsDoingQuest(typeof (YdeniasCrush)) as YdeniasCrush;

			elvarTambor.TurnTo(player);
			
			// The player whispered to NPC (clicked on the text inside the [])
			if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "Cotswold":
							elvarTambor.SayTo(player, "If you are traveling back that way, would you mind delivering this letter to Ydenia for me?  I would be ever so appreciative.");
							if (quest.Step == 2)
							{
								player.GainExperience(10, 0, 0, true);
								player.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You are awarded 2 silver and some copper!");

								// give letter                
								GiveItem(elvarTambor, player, letterToYdenia);

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
			if (player.IsDoingQuest(typeof (YdeniasCrush)) != null)
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
			YdeniasCrush quest = player.IsDoingQuest(typeof (YdeniasCrush)) as YdeniasCrush;

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
			if(ydeniaPhilpott.CanGiveQuest(typeof (YdeniasCrush), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (YdeniasCrush)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!ydeniaPhilpott.GiveQuest(typeof (YdeniasCrush), player, 1))
					return;

				// give letter                
				GiveItem(ydeniaPhilpott, player, letterToElvar);

				SendReply(player, "Great!  Here you are!  Just take this letter to Elvar Tambor at Prydwen Keep.  I know he will be happy.");
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
						return "[Step #1] Deliver the letter Ydenia just handed to you to Elvar Tambor in Prydwen Keep.";
					case 2:
						return "[Step #2] Listen to Elvar.";
					case 3:
						return "[Step #3] Take Elvar's letter back to Ydenia in Cotswold Village.  She is in the tavern there.";
					case 4:
						return "[Step #4] Listen to Ydenia.  If she stops talking to you and becomes engrossed in her letter, ask her if she has [something] for you for your hard work.";
				}
				return base.Description;
			}
		}


		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (YdeniasCrush)) == null)
				return;


			if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 1)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == elvarTambor.Name && gArgs.Item.Id_nb == letterToElvar.Id_nb)
					{
						RemoveItem(elvarTambor, m_questPlayer, letterToElvar);

						elvarTambor.TurnTo(m_questPlayer);
						elvarTambor.SayTo(m_questPlayer, "Ah!  A letter from my Ydenia.  Thank you for delivering it to me.  I can't wait to see what she has to say.  I was just sitting here wondering if there was going to be someone who was traveling back to [Cotswold].");
						
						Step = 2;
						return;
					}
				}

				if(Step == 3)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == ydeniaPhilpott.Name && gArgs.Item.Id_nb == letterToYdenia.Id_nb)
					{
						RemoveItem(ydeniaPhilpott, m_questPlayer, letterToYdenia);

						ydeniaPhilpott.TurnTo(m_questPlayer);
						ydeniaPhilpott.SayTo(m_questPlayer, "Thank you friend!  Here, I have [something] for you.");
						
						Step = 4;
						return;
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, letterToElvar, false);
			RemoveItem(m_questPlayer, letterToYdenia, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			GiveItem(ydeniaPhilpott, m_questPlayer, silverRingOfHealth);

			m_questPlayer.GainExperience(20 + (m_questPlayer.Level - 1) * 5, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 5, Util.Random(50)), "You are awarded 5 silver and some copper!");
		}
	}
}
