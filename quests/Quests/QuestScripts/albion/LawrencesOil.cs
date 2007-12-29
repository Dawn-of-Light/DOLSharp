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
 * 1) Travel to loc=10695,30110 Camelot Hills (Cotswold Village) to speak with Brother Lawrence
 * 2) Go to loc=12288,36480 Camelot Hills (near the housing zone) and kill river spriteling until your flask will be full
 * 2) Came back to Cotswold Village and give your filled flask to Brother Lawrence to have your reward
 */

using System;
using System.Reflection;
using DOL.GS.Scripts;
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

	public class LawrencesOil : BaseQuest
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
		protected const string questTitle = "Lawrence's Oil";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 7;

		private static GameNPC brotherLawrence = null;

		private static ItemTemplate lawrencesEmptyFlask = null;
		private static ItemTemplate lawrencesFilledFlask= null;
		
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public LawrencesOil() : base()
		{
		}

		public LawrencesOil(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public LawrencesOil(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public LawrencesOil(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Brother Lawrence", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				brotherLawrence = new GameHealer();
				brotherLawrence.Model = 32;
				brotherLawrence.Name = "Brother Lawrence";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + brotherLawrence.Name + ", creating him ...");
				brotherLawrence.GuildName = "Part of " + questTitle + " Quest";
				brotherLawrence.Realm = eRealm.Albion;
				brotherLawrence.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 14, 20);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 98, 44);
				brotherLawrence.Inventory = template.CloseTemplate();
				brotherLawrence.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				brotherLawrence.Size = 54;
				brotherLawrence.Level = 29;
				brotherLawrence.X = 560559;
				brotherLawrence.Y = 511892;
				brotherLawrence.Z = 2344;
				brotherLawrence.Heading = 662;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					brotherLawrence.SaveIntoDatabase();

				brotherLawrence.AddToWorld();
			}
			else
				brotherLawrence = npcs[0];

			#endregion

			#region defineItems

			// item db check
			lawrencesEmptyFlask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "lawrences_empty_flask");
			if (lawrencesEmptyFlask == null)
			{
				lawrencesEmptyFlask = new ItemTemplate();
				lawrencesEmptyFlask.Name = "Lawrence's Empty Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+lawrencesEmptyFlask.Name+", creating it ...");
				
				lawrencesEmptyFlask.Level = 0;
				lawrencesEmptyFlask.Weight = 1;
				lawrencesEmptyFlask.Model = 490;
				
				lawrencesEmptyFlask.Object_Type = (int) eObjectType.GenericItem;
				lawrencesEmptyFlask.Id_nb = "lawrences_empty_flask";
				lawrencesEmptyFlask.Gold = 0;
				lawrencesEmptyFlask.Silver = 0;
				lawrencesEmptyFlask.Copper = 0;
				lawrencesEmptyFlask.IsPickable = false;
				lawrencesEmptyFlask.IsDropable = false;
				
				lawrencesEmptyFlask.Quality = 100;
				lawrencesEmptyFlask.Condition = 1000;
				lawrencesEmptyFlask.MaxCondition = 1000;
				lawrencesEmptyFlask.Durability = 1000;
				lawrencesEmptyFlask.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(lawrencesEmptyFlask);
			}

			// item db check
			lawrencesFilledFlask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "lawrences_filled_flask");
			if (lawrencesFilledFlask == null)
			{
				lawrencesFilledFlask = new ItemTemplate();
				lawrencesFilledFlask.Name = "Lawrence's Filled Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+lawrencesFilledFlask.Name+", creating it ...");
				
				lawrencesFilledFlask.Level = 0;
				lawrencesFilledFlask.Weight = 1;
				lawrencesFilledFlask.Model = 490;
				
				lawrencesFilledFlask.Object_Type = (int) eObjectType.GenericItem;
				lawrencesFilledFlask.Id_nb = "lawrences_filled_flask";
				lawrencesFilledFlask.Gold = 0;
				lawrencesFilledFlask.Silver = 0;
				lawrencesFilledFlask.Copper = 0;
				lawrencesFilledFlask.IsPickable = false;
				lawrencesFilledFlask.IsDropable = false;
				
				lawrencesFilledFlask.Quality = 100;
				lawrencesFilledFlask.Condition = 1000;
				lawrencesFilledFlask.MaxCondition = 1000;
				lawrencesFilledFlask.Durability = 1000;
				lawrencesFilledFlask.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(lawrencesFilledFlask);
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
			
			GameEventMgr.AddHandler(brotherLawrence, GameLivingEvent.Interact, new DOLEventHandler(TalkToBrotherLawrence));
			GameEventMgr.AddHandler(brotherLawrence, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherLawrence));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			brotherLawrence.AddQuestToGive(typeof (LawrencesOil));

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
			/* If Elvar Ironhand has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (brotherLawrence == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
			
			GameEventMgr.RemoveHandler(brotherLawrence, GameLivingEvent.Interact, new DOLEventHandler(TalkToBrotherLawrence));
			GameEventMgr.RemoveHandler(brotherLawrence, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherLawrence));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			brotherLawrence.RemoveQuestToGive(typeof (LawrencesOil));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToBrotherLawrence(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(brotherLawrence.CanGiveQuest(typeof (LawrencesOil), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			LawrencesOil quest = player.IsDoingQuest(typeof (LawrencesOil)) as LawrencesOil;

			brotherLawrence.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					brotherLawrence.SayTo(player, "Greetings. My name is Brother Lawrence, and I'm the head healer here in Cotswold. It's my responsibility to keep the townspeople in good health and ward off the plague.  People come to me with everything from cuts and rashes to more [serious] ailments.");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						brotherLawrence.SayTo(player, "Welcome back, "+player.CharacterClass.Name+". I've almost finished making my preparations for the demonstration. May I have the flask of oil?");
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
						case "serious":
							brotherLawrence.SayTo(player, "Even in a small community like Cotswold, I'm almost always busy.  The Church teaches us to use prayers and magic to speed the natural healing process, but that requires a lot of energy. It's not uncommon for healers to be worn out at the end of the [day].");
							break;

							//If the player offered his help, we send the quest dialog now!
						case "day":
							brotherLawrence.SayTo(player, "It's exhausting work, but I believe I've found my calling.  I think I may have discovered a way to help deal with minor injuries and preserve the use of magic for more serious cases. Are you willing to help me prepare for a demonstration of my methods?");
							player.Out.SendQuestSubscribeCommand(brotherLawrence, QuestMgr.GetIDForQuestType(typeof(LawrencesOil)), "Will you help Brother Lawrence gather \nthe oil he needs for his demonstration? \n[Levels 4-7]");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "healing":
							if(quest.Step == 1)
							{
								brotherLawrence.SayTo(player, "So far, I've used it as the base for a variety of salves and other treatments. It's proven very valuable in healing minor injuries, rashes and infections.  If we work quickly, I can have all my supplies ready in time for the [demonstration].");
							}
							break;

						case "demonstration":
							if(quest.Step == 1)
							{
								brotherLawrence.SayTo(player, "Here's a flask to store the oil in. Killing two of the river spritelings should provide enough oil for the demonstration and the next week's use. To find the spritelings, cross the bridge toward Camelot, but turn south before you get to the gates. Continue following the west bank of the river to the south, and you should see the spritelings before you come to the entrance to the Housing areas.");
								
								GiveItem(brotherLawrence, player, lawrencesEmptyFlask);
								
								quest.Step = 2;
							}
							break;	

						case "methods":
							if(quest.Step == 5)
							{
								brotherLawrence.SayTo(player, "Here's a bit of copper for your trouble. I wish I could offer more, but times are tough right now. If you are ever in need of healing, please don't hesitate to visit me.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(LawrencesOil)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (LawrencesOil)) != null)
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
			LawrencesOil quest = player.IsDoingQuest(typeof (LawrencesOil)) as LawrencesOil;

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
			if(brotherLawrence.CanGiveQuest(typeof (LawrencesOil), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (LawrencesOil)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!brotherLawrence.GiveQuest(typeof (LawrencesOil), player, 1))
					return;

				SendReply(player, "Thank you for agreeing to help! This should make things go much more smoothly. I was concerned I might not have enough time to gather all the materials. I've found that the oil from river sprites and spritelings is a very versatile compound for [healing].");
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
						return "[Step #1] Continue speaking with Brother Lawrence about his use of spriteling oil in his [healing] duties.";
					case 2:
						return "[Step #2] Search for river spritelings by crossing the bridge from Cotswold toward Camelot and heading south before you get to the city gates. Kill two of the spritelings and gather their oil in the flask provided by Brother Lawrence.";
					case 3:
						return "[Step #3] Search for river spritelings by crossing the bridge from Cotswold toward Camelot and heading south before you get to the city gates. Kill two of the spritelings and gather their oil in the flask provided by Brother Lawrence.";
					case 4:
						return "[Step #4] Return to Brother Lawrence with the filled flask and hand it to him when he asks."; 
					case 5:
						return "[Step #5] Continue speaking with Brother Lawrence about the demonstration of his healing [methods].";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (LawrencesOil)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "river spriteling")
					{
						if (Util.Chance(50))
						{
							player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You gather oil from the spriteling in Brother \nLawrence's Flask. Your journal \nhas been updated.");
							InventoryItem item = player.Inventory.GetFirstItemByID(lawrencesEmptyFlask.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
							if (item != null)
							{
								item.Name = "Lawrence's Flask (Half Full)";
								player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
						
								Step = 3;
							}
						}
						return;
					}
				}
				else if(Step == 3)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "river spriteling")
					{
						if (Util.Chance(50))
						{
							player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You gather oil from the spriteling in Brother \nLawrence's Flask. Your journal \nhas been updated.");
							ReplaceItem(player, lawrencesEmptyFlask, lawrencesFilledFlask);
							Step = 4;
						}
						return;
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == brotherLawrence.Name && gArgs.Item.Id_nb == lawrencesFilledFlask.Id_nb)
					{
						RemoveItem(brotherLawrence, m_questPlayer, lawrencesFilledFlask);

						brotherLawrence.TurnTo(m_questPlayer);
						brotherLawrence.SayTo(m_questPlayer, "Thank you for retrieving this. You've been a tremendous help to me in preparing for this visit.  With a little luck, perhaps soon all the Church's healers will be using my [methods].");
						Step = 5;
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, lawrencesEmptyFlask, false);
			RemoveItem(m_questPlayer, lawrencesFilledFlask, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			m_questPlayer.GainExperience((long)(m_questPlayer.ExperienceForNextLevel/15.5), true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), "You are awarded 67 copper!");
		}
	}
}
