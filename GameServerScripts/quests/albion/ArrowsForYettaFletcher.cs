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
 * 1) Travel to loc=12554,27008 Camelot Hills (Cotswold Village) to speak with Yetta Fletcher
 * 2) Go to loc=20736,47872 Camelot Hills and kill skeletons until have two well-preserved bones
 * 2) Came back to Cotswold Village and give your two bones to Elvar Ironhand to have your reward
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

	public class ArrowsForYettaFletcher : BaseQuest
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
		protected const string questTitle = "Arrows for Yetta Fletcher";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 7;

		private static GameNPC yettaFletcher = null;

		private static ItemTemplate bundleOfDecayedZombieLegs = null;
		
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public ArrowsForYettaFletcher() : base()
		{
		}

		public ArrowsForYettaFletcher(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public ArrowsForYettaFletcher(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public ArrowsForYettaFletcher(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Yetta Fletcher", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				yettaFletcher = new GameMob();
				yettaFletcher.Model = 82;
				yettaFletcher.Name = "Yetta Fletcher";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + yettaFletcher.Name + ", creating him ...");
				yettaFletcher.GuildName = "Part of " + questTitle + " Quest";
				yettaFletcher.Realm = (byte) eRealm.Albion;
				yettaFletcher.CurrentRegionID = 1;

				yettaFletcher.Size = 53;
				yettaFletcher.Level = 17;
				yettaFletcher.X = 560072;
				yettaFletcher.Y = 510125;
				yettaFletcher.Z = 2473;
				yettaFletcher.Heading = 1956;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					yettaFletcher.SaveIntoDatabase();

				yettaFletcher.AddToWorld();
			}
			else
				yettaFletcher = npcs[0];

			#endregion

			#region defineItems

			// item db check
			bundleOfDecayedZombieLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "bundle_of_decayed_zombie_legs");
			if (bundleOfDecayedZombieLegs == null)
			{
				bundleOfDecayedZombieLegs = new ItemTemplate();
				bundleOfDecayedZombieLegs.Name = "Bundle of Decayed Zombie Legs";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+bundleOfDecayedZombieLegs.Name+", creating it ...");
				
				bundleOfDecayedZombieLegs.Level = 0;
				bundleOfDecayedZombieLegs.Weight = 3;
				bundleOfDecayedZombieLegs.Model = 497;
				
				bundleOfDecayedZombieLegs.Object_Type = (int) eObjectType.GenericItem;
				bundleOfDecayedZombieLegs.Id_nb = "bundle_of_decayed_zombie_legs";
				bundleOfDecayedZombieLegs.Gold = 0;
				bundleOfDecayedZombieLegs.Silver = 0;
				bundleOfDecayedZombieLegs.Copper = 0;
				bundleOfDecayedZombieLegs.IsPickable = false;
				bundleOfDecayedZombieLegs.IsDropable = false;
				
				bundleOfDecayedZombieLegs.Quality = 100;
				bundleOfDecayedZombieLegs.MaxQuality = 100;
				bundleOfDecayedZombieLegs.Condition = 1000;
				bundleOfDecayedZombieLegs.MaxCondition = 1000;
				bundleOfDecayedZombieLegs.Durability = 1000;
				bundleOfDecayedZombieLegs.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bundleOfDecayedZombieLegs);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(yettaFletcher, GameLivingEvent.Interact, new DOLEventHandler(TalkToYettaFletcher));
			GameEventMgr.AddHandler(yettaFletcher, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYettaFletcher));

			/* Now we bring to Yetta Fletcher the possibility to give this quest to players */
			yettaFletcher.AddQuestToGive(typeof (ArrowsForYettaFletcher));

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
			/* If Yetta Fletcher has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (yettaFletcher == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(yettaFletcher, GameLivingEvent.Interact, new DOLEventHandler(TalkToYettaFletcher));
			GameEventMgr.RemoveHandler(yettaFletcher, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYettaFletcher));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
			yettaFletcher.RemoveQuestToGive(typeof (ArrowsForYettaFletcher));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToYettaFletcher(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(yettaFletcher.CanGiveQuest(typeof (ArrowsForYettaFletcher), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ArrowsForYettaFletcher quest = player.IsDoingQuest(typeof (ArrowsForYettaFletcher)) as ArrowsForYettaFletcher;

			yettaFletcher.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					yettaFletcher.SayTo(player, "Greetings to you, young "+player.CharacterClass.Name+". I was wondering if I might have a few minutes of your time. I have [a matter] I need some help with. Unfortunately I have been unable to find anyone to help me so far.");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						yettaFletcher.SayTo(player, "Welcome back, young "+player.Name+". Please give me one bundle of decaying zombie legs.");
						quest.Step = 5;
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
						case "a matter":
							yettaFletcher.SayTo(player, "Until a few years ago, I was a scout for the Defenders of Albion. I spent my time out in the Frontiers, looking for invaders from Midgard and Hibernia. Because of my skills, I was chosen to lead a group of Defenders in an attack on Dun nGed Watchtower. While the attack was a success and we took the tower, I [barely survived].");
							break;

						case "barely survived":
							yettaFletcher.SayTo(player, "A stray arrow from a Hibernian ranger just missed my heart. I was brought back to Castle Sauvage where I was healed enough to travel here to Cotswold Village. Once I recovered, I asked to leave the Defenders. My superiors agreed to let me go, but asked if I would continue to help them in [any way] I could.");
							break;

						case "any way":
							yettaFletcher.SayTo(player, "They were in need of good arrows for their scouts, since the scouts on in the Frontiers rarely have time to make their own arrows. I agreed and the Defenders set up a nice little shop for me here. Between them and the residents of Cotswold, I barely have time to keep up with their demands. That’s where [you can] help me.");
							break;

						case "you can":
							yettaFletcher.SayTo(player, "I am running low on supplies for making my special arrows for the Defenders. They are slightly different than the ones I sell, so I can’t just get the supplies anywhere. If you have some time, I would be willing [to pay you] to retrieve some supplies for me. Are you interested?");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "to pay you":
							player.Out.SendCustomDialog("Will you help Yetta Fletcher \nobtain the supplies she needs? \n[Levels 4-7]", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "shafts":
							if(quest.Step == 1)
							{
								yettaFletcher.SayTo(player, "I have tried many different things for the shafts in my attempt to make special arrows for the Defenders. Surprisingly, the leg bones of decayed zombies seem to make lightweight but strong shafts for arrows. If you can bring me two bundles of [decayed zombie] legs, you will be helping me out quite a bit.");
							}
							break;

						case "decayed zombie":
							if(quest.Step == 1)
							{
								yettaFletcher.SayTo(player, "To find the decaying zombies, leave this building and head south to the river. Follow the bank of the river south, taking care to avoid the river sprites. There is a graveyard along the river, south of here. You’ll find the decayed zombies there as well as on the hill northeast of the graveyard. Return to me when you have two bundles of decayed zombie legs, please.");
								quest.Step = 2;
							}
							break;	

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
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
			if (player.IsDoingQuest(typeof (ArrowsForYettaFletcher)) != null)
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
			ArrowsForYettaFletcher quest = player.IsDoingQuest(typeof (ArrowsForYettaFletcher)) as ArrowsForYettaFletcher;

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
			if(yettaFletcher.CanGiveQuest(typeof (ArrowsForYettaFletcher), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (ArrowsForYettaFletcher)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!yettaFletcher.GiveQuest(typeof (ArrowsForYettaFletcher), player, 1))
					return;

				SendReply(player, "Wonderful! I am so pleased that you will help me. I shall make sure that the Defenders know that you are a loyal subject of Albion. Now, let’s not waste any time. I have many arrow heads and feathers for the shaft, but I am low on the [shafts] themselves.");
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
						return "[Step #1] Speak with Yetta Fletcher to learn what you must obtain for her.";
					case 2:
						return "[Step #2] Find the decayed zombies and obtain two bundles of legs from them. From Yetta Fletcher, head south along the river's edge to the graveyard. They can also be found on the hill NE of the graveyward. Watch out for river sprites!";
					case 3:
						return "[Step #3] Get a second bundle of decayed zombie legs from the decayed zombies in the graveyard and on the hill NE of the graveyard. The graveyard is along the river, south of Yetta Fletcher."; 
					case 4:
						return "[Step #4] Return to Yetta Fletcher in Cotswold Village. From the graveyard, head north until you reach Yetta's shop.";
					case 5:
						return "[Step #5] Give Yetta Fletcher a Bundle of Decayed Zombie Legs.";
					case 6:
						return "[Step #6] Give Yetta Fletcher the second Bundle of Decayed Zombie Legs.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (ArrowsForYettaFletcher)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "decayed zombie")
					{
						if (Util.Chance(50))
						{
							GiveItem(gArgs.Target, player, bundleOfDecayedZombieLegs);
							Step = 3;
						}
						return;
					}
				}
				else if(Step == 3)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "decayed zombie")
					{
						if (Util.Chance(50))
						{
							GiveItem(gArgs.Target, player, bundleOfDecayedZombieLegs);
							Step = 4;
						}
						return;
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == yettaFletcher.Name && gArgs.Item.Id_nb == bundleOfDecayedZombieLegs.Id_nb)
				{
					if(Step == 5)
					{
						RemoveItem(yettaFletcher, m_questPlayer, bundleOfDecayedZombieLegs);

						yettaFletcher.TurnTo(m_questPlayer);
						yettaFletcher.SayTo(m_questPlayer, "Thank you, "+m_questPlayer.Name+". Now, please hand me the other bundle of decaying zombie legs.");
						Step = 6;
					}
					else if(Step == 6)
					{
						RemoveItem(yettaFletcher, m_questPlayer, bundleOfDecayedZombieLegs);

						yettaFletcher.TurnTo(m_questPlayer);
						yettaFletcher.SayTo(m_questPlayer, "Wonderful! I shall begin right away on making the new arrows for the Defenders of Albion. Please take these coins with my thanks as well as the thanks of the Defenders!");
						
						FinishQuest();
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, bundleOfDecayedZombieLegs, false);
			RemoveItem(m_questPlayer, bundleOfDecayedZombieLegs, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			
			m_questPlayer.GainExperience((long)(m_questPlayer.ExperienceForNextLevel / 15), 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), "You are awarded 67 copper!");
		}
	}
}