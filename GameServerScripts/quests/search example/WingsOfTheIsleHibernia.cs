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
 * Author:		Tolakram (taken from code originally written by Doulbousiouf R.I.P.)
 * Date:		27 March 2010
 *
 * Description (from Allakhazam):

	http://camelot.allakhazam.com/quests.html?realm=&cquest=3739

	Levels 3 - 10

	Speak with Baeth, the stable master, in Fintain, loc=29006, 27389 Lamfhota's Sound.

	Story:
	Excuse me (Your Name), can I ask a favor of you? My daughter Jessica has been having a terrible time ever since the fighting broke out. She 
	used to love taking a boat over to the island to play, but now a days all she does is sulk in the woods north of town. Perhaps you could 
	find the time to speak with her, she won't talk with her father anymore but perhaps you can find out what's been bugging her?

	Summary:
	Visit with Jessica north of town to find out what is bothering her, if you can try and help her for Baeth.

	Quest Goal: Find Jessica north of town and speak with her.
	Quest Goal: Search the light house on the island for the flute.
	Quest Goal: Return the flute to Jessica.

	Rewards:
	2 silver 48 copper
	Reed Flute

	lvl 5 received 772 experience.

	Reward:
	  Reed Flute * 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class WingsOfTheIsleHibernia : RewardQuest
	{
		/* Declare the variables we need inside our quest.
		 * You can declare static variables here, which will be available in 
		 * ALL instance of your quest and should be initialized ONLY ONCE inside
		 * the OnScriptLoaded method.
		 * 
		 * Or declare nonstatic variables here which can be unique for each Player
		 * and change through the quest journey...
		 * 
		 */

		protected const string questTitle = "Wings of the Isle";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 10;

		private static GameNPC npcBaeth = null;
		private static GameNPC npcJessica = null;

		private static ItemTemplate reedFlute = null;

		private const string STEP1_GOAL_ID = "TalkToJessica";
		private const string STEP2_GOAL_ID = "FindFlute";
		private const string STEP3_GOAL_ID = "ReturnFlute";

		// This defines the location for a /search plus the step that it's valid for.  This needs to be static, we only want to add one area for the quest.
		// This also needs to be added to the searchLocation list for this quest in each constructor.
		// In addition override QuestCommandCompleted to do anything required on a successful search.
		private static SearchLocation searchLocation = new SearchLocation(typeof(WingsOfTheIsleHibernia), 2, "Use /search to look for Jessica's flute.", 27, 356182, 382308, 5237);

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public WingsOfTheIsleHibernia() : base()
		{
			InitializeQuest(null);
		}

		public WingsOfTheIsleHibernia(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
			InitializeQuest(questingPlayer);
		}

		public WingsOfTheIsleHibernia(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
			InitializeQuest(questingPlayer);
		}

		public WingsOfTheIsleHibernia(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			InitializeQuest(questingPlayer);
		}

		/// <summary>
		/// Perform any initialization actions needed when quest is created
		/// </summary>
		protected void InitializeQuest(GamePlayer player)
		{
			AddSearchLocation(searchLocation);

			Rewards.Experience = 772;
			Rewards.MoneyPercent = 10;
			Rewards.AddBasicItem(reedFlute);
			Rewards.ChoiceOf = 1;

			if (player != null)
			{
				// Since this quest progresses through the goals we need to add the active goals

				AddGoal(STEP1_GOAL_ID, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Goal1", QuestPlayer.Name), QuestGoal.GoalType.ScoutMission, 1, null);

				if (Step > 1)
				{
					AddGoal(STEP2_GOAL_ID, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Goal2", QuestPlayer.Name), QuestGoal.GoalType.ScoutMission, 1, null);
				}

				if (Step > 2)
				{
					AddGoal(STEP3_GOAL_ID, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Goal3", QuestPlayer.Name), QuestGoal.GoalType.ScoutMission, 1, reedFlute);
				}
			}
		}

		/// <summary>
		/// This is the critical method to override when using the /search command
		/// For this quest will will give the player the item and advance the quest step.
		/// </summary>
		/// <param name="command"></param>
		protected override void QuestCommandCompleted(AbstractQuest.eQuestCommand command)
		{
			if (command == eQuestCommand.Search)
			{
				foreach (QuestGoal goal in Goals)
				{
					if (goal.Id == STEP2_GOAL_ID && goal.IsAchieved == false)
					{
						// Here we try to place the item in the players backpack.  If their inventory is full it fails and quest does not advance.
						if (TryGiveItem(QuestPlayer, reedFlute))
						{
							// If we can give the flute to the player then we advance to the last step
							goal.Advance();
							AddGoal(STEP3_GOAL_ID, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Goal3", QuestPlayer.Name), QuestGoal.GoalType.ScoutMission, 1, reedFlute);
							Step = 3;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// The name of this quest
		/// </summary>
		public override string Name
		{
			get { return questTitle; }
		}

		/// <summary>
		/// The fully-fledged story to the quest.
		/// </summary>
		public override string Story
		{
			get
			{
				if (QuestPlayer != null)
				{
					return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Story", QuestPlayer.Name);
				}
				else if (OfferPlayer != null)
				{
					return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Story", OfferPlayer.Name);
				}

				return "undefined";
			}
		}

		/// <summary>
		/// Summary displayed when being offered the quest
		/// </summary>
		public override string Summary
		{
			get
			{
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Summary");
			}
		}

		/// <summary>
		/// Goal descriptions used when doing the quest
		/// </summary>
		public override string Description
		{
			get
			{
				return Summary;
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Conclusion");
			}
		}

		/// <summary>
		/// The level of the quest as it shows in the journal.
		/// </summary>
		public override int Level
		{
			get
			{
				return 3;
			}
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
			* the world who comes from the certain Realm. If we find the npc
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Baeth", eRealm.Hibernia);

			/*  If the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				npcBaeth = new GameNPC();
				npcBaeth.Model = 361;
				npcBaeth.Name = "Baeth";
				if (log.IsWarnEnabled)
				{
					log.Warn("Could not find " + npcBaeth.Name + ", creating him ...");
				}
				npcBaeth.GuildName = "Part of " + questTitle + " Quest";
				npcBaeth.Realm = eRealm.Hibernia;
				npcBaeth.CurrentRegionID = 27;

				npcBaeth.Size = 52;
				npcBaeth.Level = 30;
				npcBaeth.X = 356650;
				npcBaeth.Y = 355078;
				npcBaeth.Z = 5015;
				npcBaeth.Heading = 2959;

				if (SAVE_INTO_DATABASE)
				{
					npcBaeth.SaveIntoDatabase();
				}

				npcBaeth.AddToWorld();
			}
			else
			{
				npcBaeth = npcs[0];
			}

			npcs = WorldMgr.GetNPCsByName("Jessica", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				npcJessica = new GameNPC();
				npcJessica.Model = 366;
				npcJessica.Name = "Jessica";
				if (log.IsWarnEnabled)
				{
					log.Warn("Could not find " + npcJessica.Name + ", creating him ...");
				}
				npcJessica.GuildName = "Part of " + questTitle + " Quest";
				npcJessica.Realm = eRealm.Hibernia;
				npcJessica.CurrentRegionID = 27;

				npcJessica.Size = 38;
				npcJessica.Level = 1;
				npcJessica.X = 358068;
				npcJessica.Y = 347553;
				npcJessica.Z = 5491;
				npcJessica.Heading = 49;

				if (SAVE_INTO_DATABASE)
				{
					npcJessica.SaveIntoDatabase();
				}

				npcJessica.AddToWorld();
			}
			else
			{
				npcJessica = npcs[0];
			}


			#endregion

			#region defineItems

			// item db check
			reedFlute = GameServer.Database.FindObjectByKey<ItemTemplate>("quest_reed_flute");
			if (reedFlute == null)
			{
				reedFlute = new ItemTemplate();
				reedFlute.Name = "Reed Flute";
				if (log.IsWarnEnabled)
				{
					log.Warn("Could not find " + reedFlute.Name + ", creating it ...");
				}
				
				reedFlute.Level = 1;
				reedFlute.Weight = 1;
				reedFlute.Model = 325;
				
				reedFlute.Object_Type = (int)eObjectType.Magical;
				reedFlute.Item_Type = (int)eInventorySlot.FirstBackpack;
				reedFlute.Id_nb = "quest_reed_flute";
				reedFlute.Gold = 0;
				reedFlute.Silver = 0;
				reedFlute.Copper = 0;
				reedFlute.IsPickable = false;
				reedFlute.IsDropable = false;
				reedFlute.IsTradable = false;
				
				reedFlute.Quality = 100;
				reedFlute.Condition = 5000;
				reedFlute.MaxCondition = 5000;
				reedFlute.Durability = 5000;
				reedFlute.MaxDurability = 5000;

				reedFlute.SpellID = 65001;
				reedFlute.CanUseEvery = 300;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(reedFlute);
				}
			}

			// Add spell and npctemplate to the DB
			/*

			insert into spell (`Spell_ID`, `SpellID`, `ClientEffect`, `Icon`, `Name`, `Description`, `Target`, `Range`, `Power`, `CastTime`,
			`Damage`, `DamageType`, `Type`, `Duration`, `Frequency`, `Pulse`, `PulsePower`, `Radius`, `Value`, `LifeDrainReturn`, `Message1`, `PackageID`)
			values ('summon_dragonfly', 65001, 0, 0, 'Call Dragonfly', 'Summons a dragonfly to travel at your side, but cannot be used in the battlegrounds or the frontier regions.',
			'Self', 0, 0, 0, 1, 0, 'SummonNoveltyPet', 65535, 0, 0, 0, 0, 1, 65001, 'A small dragonfly appears as the sound of the whistle fades.', 'NoveltyPets');

			insert into npctemplate (`NpcTemplate_ID`, `TemplateId`, `Name`, `GuildName`, `Model`, `Size`, `MaxSpeed`, `Flags`, `Level`, `ClassType`, `PackageID`)
			values ('quest_pet_dragonfly', 65001, 'Dragonfly', '', 819, 12, 250, 48, 1, '', 'NoveltyPets');

			*/


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

			GameEventMgr.AddHandler(npcBaeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToBaeth));
			GameEventMgr.AddHandler(npcBaeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBaeth));
			GameEventMgr.AddHandler(npcJessica, GameLivingEvent.Interact, new DOLEventHandler(TalkToJessica));
			GameEventMgr.AddHandler(npcJessica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJessica));

			npcBaeth.AddQuestToGive(typeof(WingsOfTheIsleHibernia));

			if (log.IsInfoEnabled)
			{
				log.Info("Quest \"" + questTitle + "\" initialized");
			}
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
			if (npcBaeth == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(npcBaeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToBaeth));
			GameEventMgr.RemoveHandler(npcBaeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBaeth));
			GameEventMgr.RemoveHandler(npcJessica, GameLivingEvent.Interact, new DOLEventHandler(TalkToJessica));
			GameEventMgr.RemoveHandler(npcJessica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJessica));

			npcBaeth.RemoveQuestToGive(typeof(WingsOfTheIsleHibernia));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToBaeth(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(npcBaeth.CanGiveQuest(typeof(WingsOfTheIsleHibernia), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			WingsOfTheIsleHibernia quest = player.IsDoingQuest(typeof(WingsOfTheIsleHibernia)) as WingsOfTheIsleHibernia;

			if (quest != null)
				return;

			npcBaeth.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new WingsOfTheIsleHibernia();
					quest.QuestGiver = npcBaeth;
					quest.OfferQuest(player);
				}
			}
		}


		protected static void TalkToJessica(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			WingsOfTheIsleHibernia quest = player.IsDoingQuest(typeof(WingsOfTheIsleHibernia)) as WingsOfTheIsleHibernia;

			if (quest == null)
				return;

			if (e == GameObjectEvent.Interact)
			{
				if (quest.Step == 1)
				{
					npcJessica.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Jessica1"));
				}
				else if (quest.Step == 3)  // step 2 is the /search and is advanced when item is found
				{
					foreach (QuestGoal goal in quest.Goals)
					{
						if (goal.Id == STEP3_GOAL_ID && goal.IsAchieved == false)
						{
							RemoveItem(player, reedFlute, false);

							// Jessica will display the conclusion text in the quest dialog
							quest.QuestGiver = npcJessica;
							quest.ChooseRewards(player);
						}
					}

				}
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

				if (wArgs.Text == "island")
				{
					npcJessica.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Jessica2"));
				}
				else if (wArgs.Text == "lost")
				{
					foreach (QuestGoal goal in quest.Goals)
					{
						if (goal.Id == STEP1_GOAL_ID && goal.IsAchieved == false)
						{
							goal.Advance();
							quest.AddGoal(STEP2_GOAL_ID, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Goal2", player.Name), QuestGoal.GoalType.ScoutMission, 1, null);
							npcJessica.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.WingsOfTheIsle.Jessica3"));
							quest.Step = 2;
							break;
						}
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
			if (player.IsDoingQuest(typeof(WingsOfTheIsleHibernia)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(WingsOfTheIsleHibernia)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
			{
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			}
			else if (e == GamePlayerEvent.DeclineQuest)
			{
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
			}
		}

		
		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			WingsOfTheIsleHibernia quest = player.IsDoingQuest(typeof(WingsOfTheIsleHibernia)) as WingsOfTheIsleHibernia;

			if (quest == null)
				return;

			if (response != 0x00)
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
			if (npcBaeth.CanGiveQuest(typeof(WingsOfTheIsleHibernia), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(WingsOfTheIsleHibernia)) != null)
				return;

			if (response != 0x00)
			{
				npcBaeth.GiveQuest(typeof(WingsOfTheIsleHibernia), player, 1);
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest();
			RemoveItem(m_questPlayer, reedFlute, false);
		}
	}
}
