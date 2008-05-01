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
 * 1) Travel to loc=25344,47360 Camelot Hills (Prydwen Keep) to speak with Sir Jerem
 * 2) Go to loc=25025,23755 Mithra's Tomb, kill the little spider and speak with Suire Galune
 * 2) Came back to Prydwen Keep and speak with Sir Jerem to have your reward
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

	public class ShakenSquire : BaseQuest
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
		protected const string questTitle = "Shaken Squire";
		protected const int minimumLevel = 6;
		protected const int maximumLevel = 9;

		private static GameNPC sirJerem = null;
		private static GameNPC squireGalune = null;

		private static GameNPC smallSpider = null;

		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public ShakenSquire() : base()
		{
		}

		public ShakenSquire(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public ShakenSquire(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public ShakenSquire(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Jerem", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				sirJerem = new GameNPC();
				sirJerem.Model = 254;
				sirJerem.Name = "Sir Jerem";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sirJerem.Name + ", creating him ...");
				sirJerem.GuildName = "Part of " + questTitle + " Quest";
				sirJerem.Realm = eRealm.Albion;
				sirJerem.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 68, 21);
				template.AddNPCEquipment(eInventorySlot.HeadArmor, 64);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 27);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 158);
				sirJerem.Inventory = template.CloseTemplate();
				sirJerem.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				sirJerem.Size = 51;
				sirJerem.Level = 38;
				sirJerem.X = 573815;
				sirJerem.Y = 530850;
				sirJerem.Z = 2933;
				sirJerem.Heading = 2685;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					sirJerem.SaveIntoDatabase();

				sirJerem.AddToWorld();
			}
			else
				sirJerem = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Squire Galune", eRealm.Albion);
			if (npcs.Length == 0)
			{
				squireGalune = new GameNPC();
				squireGalune.Model = 254;
				squireGalune.Name = "Squire Galune";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + squireGalune.Name + ", creating him ...");
				squireGalune.GuildName = "Part of " + questTitle + " Quest";
				squireGalune.Realm = eRealm.Albion;
				squireGalune.CurrentRegionID = 21;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 320);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 137);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 138);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 134);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 135);
				squireGalune.Inventory = template.CloseTemplate();
				squireGalune.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				squireGalune.Size = 45;
				squireGalune.Level = 8;
				squireGalune.X = 33219;
				squireGalune.Y = 31931;
				squireGalune.Z = 16240;
				squireGalune.Heading = 477;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					squireGalune.SaveIntoDatabase();

				squireGalune.AddToWorld();
			}
			else
				squireGalune = npcs[0];


			foreach (GameNPC npc in squireGalune.GetNPCsInRadius(400))
			{
				if (npc.Name == "small spider")
				{
					smallSpider = npc;
					break;
				}
			}

			if (smallSpider == null)
			{
				smallSpider = new GameNPC();
				smallSpider.Model = 72;
				smallSpider.Name = "small spider";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + smallSpider.Name + ", creating him ...");
				smallSpider.GuildName = "Part of " + questTitle + " Quest";
				smallSpider.Realm = eRealm.None;
				smallSpider.CurrentRegionID = 21;
				smallSpider.Size = 17;
				smallSpider.Level = 5;
				smallSpider.X = 33158;
				smallSpider.Y = 31973;
				smallSpider.Z = 16240;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				smallSpider.SetOwnBrain(brain);

				smallSpider.Heading = 2605;
				smallSpider.MaxSpeedBase = 0;
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					smallSpider.SaveIntoDatabase();

				smallSpider.AddToWorld();
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
			
			GameEventMgr.AddHandler(sirJerem, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirJerem));
			GameEventMgr.AddHandler(sirJerem, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirJerem));

			GameEventMgr.AddHandler(squireGalune, GameLivingEvent.Interact, new DOLEventHandler(TalkToSquireGalune));
			GameEventMgr.AddHandler(squireGalune, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSquireGalune));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			sirJerem.AddQuestToGive(typeof (ShakenSquire));

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
			if (sirJerem == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
			
			GameEventMgr.RemoveHandler(sirJerem, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirJerem));
			GameEventMgr.RemoveHandler(sirJerem, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirJerem));

			GameEventMgr.RemoveHandler(squireGalune, GameLivingEvent.Interact, new DOLEventHandler(TalkToSquireGalune));
			GameEventMgr.RemoveHandler(squireGalune, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSquireGalune));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			sirJerem.RemoveQuestToGive(typeof (ShakenSquire));
		}


		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToSirJerem(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(sirJerem.CanGiveQuest(typeof (ShakenSquire), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ShakenSquire quest = player.IsDoingQuest(typeof (ShakenSquire)) as ShakenSquire;

			sirJerem.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					sirJerem.SayTo(player, "Good day, "+player.CharacterClass.Name+".  I'm sorry that I can't spare much time for idle chatter, but I've got a number of things on my mind right now.  Keeping track of a keep full of squires certainly isn't easy [work].");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						sirJerem.SayTo(player, "Ah, you've returned. Were you able to track down Squire [Galune]?");
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
						case "work":
							sirJerem.SayTo(player, "They're good lads, mostly, but they tend to get a little overeager at times.  In fact, one of my youngest squires has gone missing. I suspect he heard rumors from the locals and went to [investigate] them.");
							break;

							//If the player offered his help, we send the quest dialog now!
						case "investigate":
							sirJerem.SayTo(player, "This happens every time we get a new squire. The merchants fill his head with nonsense about becoming a hero and then the boy goes off exploring and gets himself into trouble.  Will you help me locate my missing squire?");
							player.Out.SendQuestSubscribeCommand(sirJerem, QuestMgr.GetIDForQuestType(typeof(ShakenSquire)), "Will you help Sir Jerem find \nthe missing squire? \n[Levels 6-9]");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "once":
							if(quest.Step == 1)
							{
								sirJerem.SayTo(player, "I think Graent envies them, in a way. The squires are still young enough to have a bright-eyed enthusiasm for exploration and danger, and he feeds that with his stories.  I think the Tomb of Mithra is probably the best place to start your [search].");
							}
							break;

						case "search":
							if(quest.Step == 1)
							{
								sirJerem.SayTo(player, "The Tomb can be found to the east of here, past the bridge, and across the road.  If you need help finding it, don't forget that you can always consult your map. The name of the squire you're looking for is Galune. Good luck.");
								quest.Step = 2;
							}
							break;	

						case "Galune":
							if(quest.Step == 4)
							{
								SendMessage(player, "You tell Sir Jerem that you found Squire Galune in the Tomb, and that he is making his way back to the keep.  You decide not to mention the spider, but you get the sense that Sir Jerem will find out anyway.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								sirJerem.SayTo(player, "I'm glad to hear that you found the squire before he got himself into too much trouble. Most likely, he fainted at the sight of some rat or spider, and decided to wait for someone to [rescue him].");
							}
							break;

						case "rescue him":
							if(quest.Step ==4)
							{
								sirJerem.SayTo(player, "I won't give him too much trouble over it. I have a feeling that he's learned a good lesson from all of this and won't be nearly so adventurous in the future. Here's a bit of money for your efforts.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ShakenSquire)))
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

		protected static void TalkToSquireGalune(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			ShakenSquire quest = player.IsDoingQuest(typeof (ShakenSquire)) as ShakenSquire;

			squireGalune.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch(quest.Step)
					{
						case 2:
							squireGalune.SayTo(player, "Oh, no, it's a spider! Please, someone get this awful thing away from me!");
							squireGalune.Emote(eEmote.Beg);
							break;

						case 3:
							squireGalune.SayTo(player, "Sir Jerem finally sent someone? Nevermind, I don't care who sent you, I'm just relieved that horrible spider is dead! I thought for sure it was going to kill me. I'll never listen to [Master Graent's] stories again.");
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
						case "Master Graent's":
							if(quest.Step == 3)
							{
								squireGalune.SayTo(player, "He's full of stories about hidden treasures and deeds of valor, but he never mentioned spiders! He conveniently left that part out. When I get back to the castle, I'm going to give him a piece of my [mind].");
							}
							break;

						case "mind":
							if(quest.Step == 3)
							{
								squireGalune.SayTo(player, "I think I'll be able to make it back on my own now. Thank you so much for rescuing me from that spider.  Please let Sir Jerem know that I'm safe when you get back to the keep.");
								quest.Step = 4;
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
			if (player.IsDoingQuest(typeof (ShakenSquire)) != null)
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
			ShakenSquire quest = player.IsDoingQuest(typeof (ShakenSquire)) as ShakenSquire;

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
			if(sirJerem.CanGiveQuest(typeof (ShakenSquire), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (ShakenSquire)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!sirJerem.GiveQuest(typeof (ShakenSquire), player, 1))
					return;

				SendReply(player, "Thank you for agreeing to help. I'm fairly sure that Master Graent has been telling him all kinds of tales about the Tomb of Mithra to the east.  Come to think of it, I know I've seen the squire speaking with him more than [once].");
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
						return "[Step #1] Continue speaking with Sir Jerem about your [search] for the missing squire.";
					case 2:
						return "[Step #2] Travel east from Prydwen Keep to the Tomb of Mithra. If you need help locating the dungeon entrance, type /map to view a map of Camelot Hills. Enter the dungeon and look for Squire Galune.";
					case 3:
						return "[Step #3] Now that you've defeated the spider, speak to Squire Galune.";
					case 4:
						return "[Step #4] You've rescued Squire Galune from the Tomb of Mithra. Travel west from the dungeon to Prydwen Keep and let Sir Jerem know that you found his squire."; 
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (ShakenSquire)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "small spider" && gArgs.Target.CurrentRegionID == smallSpider.CurrentRegionID && gArgs.Target.X == smallSpider.X && gArgs.Target.Y == smallSpider.Y)
					{
						Step = 3;
						return;
					}
				}
			}
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			m_questPlayer.GainExperience((long)(m_questPlayer.ExperienceForNextLevel / 35), true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, Util.Random(100)), "You are awarded 1 silver and some copper!");
		}
	}
}