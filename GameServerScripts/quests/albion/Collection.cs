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
 * Author:		Gandulf Kohlweiss
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 * 2) /Use the Dusty Old Map and go to loc=19547,19079 Camelot Hills and kill Palearis for her wing. 
 * 3) /Use the Dusty Old Map and go to loc=23423,16337 Camelot Hills and kill Bohad for her wing. 
 * 4) /Use the Dusty Old Map and go to loc=23036,27231 Camelot Hills and kill Fluvale for her wing. 
 * 5) Return to Master Frederick and hand him the wings when asked for your reward.
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
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
    /* The first thing we do, is to declare the quest requirement
    * class linked with the new Quest. To do this, we derive 
    * from the abstract class AbstractQuestDescriptor
    */
    public class CollectionDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base methid like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(Collection); }
        }

        /* This value is used to retrieves the minimum level needed
         *  to be able to make this quest. Override it only if you need, 
         * the default value is 1
         */
        public override int MinLevel
        {
            get { return 3; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 3; }
        }

        public override bool CheckQuestQualification(GamePlayer player)
        {
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

            // This checks below are only performed is player isn't doing quest already
            if (player.HasFinishedQuest(typeof(Frontiers)) == 0)
                return false;

            if (!BaseFrederickQuest.CheckPartAccessible(player, typeof(Collection)))
                return false;

            return base.CheckQuestQualification(player);
        }
    }

    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(Collection), ExtendsType = typeof(AbstractQuest))]
	public class Collection : BaseFrederickQuest
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

		protected const string questTitle = "Collection";

		private static GameMob masterFrederick = null;

		private static GameMob[] general = new GameMob[3];
		private static String[] generalNames = {"Palearis", "Bohad", "Fluvale"};
		private static GameLocation[] generalLocations = new GameLocation[3];

		private static GenericItemTemplate fairyGeneralWings = null;
		private static GenericItemTemplate dustyOldMap = null;
		private static ArmsArmorTemplate recruitsArms = null;
		private static ArmsArmorTemplate recruitsSleeves = null;

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

			masterFrederick = GetMasterFrederick();
			if(masterFrederick == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			generalLocations[0] = new GameLocation(generalNames[0], 1, 568589, 501801, 2134, 23);
			generalLocations[1] = new GameLocation(generalNames[1], 1, 572320, 499246, 2472, 14);
			generalLocations[2] = new GameLocation(generalNames[2], 1, 571900, 510559, 2210, 170);

			for (int i = 0; i < general.Length; i++)
			{
				general[i] = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, generalNames[i]) as GameMob;
					
				if (general[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + generalNames[i] + ", creating her ...");
					general[i] = new GameMob();

					general[i].Model = 603;

					general[i].Name = generalNames[i];
					general[i].Position = generalLocations[i].Position;
					general[i].Heading = generalLocations[i].Heading;

					general[i].GuildName = "Part of " + questTitle + " Quest";
					general[i].Realm = (byte) eRealm.None;
					general[i].Region = generalLocations[i].Region;
					general[i].Size = 49;
					general[i].Level = 2;

					general[i].RespawnInterval = -1; // autorespawn

					StandardMobBrain brain = new StandardMobBrain();
					brain.Body = general[i];
					brain.AggroLevel = 80;
					brain.AggroRange = 1000;
					general[i].OwnBrain = brain;

					if(!general[i].AddToWorld())
					{
						if (log.IsWarnEnabled)
							log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
						return;
					}

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						GameServer.Database.AddNewObject(general[i]);
				}
			}

			#endregion

			#region defineItems

			fairyGeneralWings = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Wings of Fairy General")) as GenericItemTemplate;
			if (fairyGeneralWings == null)
			{
				fairyGeneralWings = new GenericItemTemplate();
				fairyGeneralWings.Name = "Wings of Fairy General";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + fairyGeneralWings.Name + " , creating it ...");

				fairyGeneralWings.Weight = 2;
				fairyGeneralWings.Model = 551;

				fairyGeneralWings.IsDropable = false;
                fairyGeneralWings.IsSaleable = false;
                fairyGeneralWings.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fairyGeneralWings);
			}

			dustyOldMap = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Dusty Old Map")) as GenericItemTemplate;
			if (dustyOldMap == null)
			{
				dustyOldMap = new GenericItemTemplate();
				dustyOldMap.Name = "Dusty Old Map";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dustyOldMap.Name + " , creating it ...");

				dustyOldMap.Weight = 10;
				dustyOldMap.Model = 498;

				dustyOldMap.IsDropable = false;
                dustyOldMap.IsSaleable = false;
                dustyOldMap.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(dustyOldMap);
			}


			// item db check
			recruitsArms = GameServer.Database.SelectObject(typeof (ArmsArmorTemplate), Expression.Eq("Name", "Recruit's Studded Arms")) as ArmsArmorTemplate;
			if (recruitsArms == null)
			{
				recruitsArms = new ArmsArmorTemplate();
				recruitsArms.Name = "Recruit's Studded Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsArms.Name + ", creating it ...");
				recruitsArms.Level = 7;

				recruitsArms.Weight = 36;
				recruitsArms.Model = 83; // studded Boots

                recruitsArms.ArmorFactor = 10;
                recruitsArms.ArmorLevel = eArmorLevel.Medium;

				recruitsArms.Value = 400;

				recruitsArms.IsDropable = true;
                recruitsArms.IsSaleable = true;
                recruitsArms.IsTradable = true;

				recruitsArms.Color = 9; // red leather

				recruitsArms.Bonus = 5; // default bonus

                recruitsArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 4));
                recruitsArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				recruitsArms.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsArms);
			}

			recruitsSleeves = GameServer.Database.SelectObject(typeof (ArmsArmorTemplate), Expression.Eq("Name", "Recruit's Quilted Sleeves")) as ArmsArmorTemplate;
			if (recruitsSleeves == null)
			{
				recruitsSleeves = new ArmsArmorTemplate();
				recruitsSleeves.Name = "Recruit's Quilted Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsSleeves.Name + ", creating it ...");
				recruitsSleeves.Level = 7;

				recruitsSleeves.Weight = 12;
				recruitsSleeves.Model = 153;

                recruitsSleeves.ArmorLevel = eArmorLevel.VeryLow;
                recruitsSleeves.ArmorFactor = 6;

				recruitsSleeves.Value = 400;

				recruitsSleeves.IsDropable = true;
                recruitsSleeves.IsSaleable = true;
                recruitsSleeves.IsTradable = true;

				recruitsSleeves.Color = 27; // red cloth

				recruitsSleeves.Bonus = 5; // default bonus

                recruitsSleeves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 4));
                recruitsSleeves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				recruitsSleeves.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsSleeves);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(masterFrederick, typeof(CollectionDescriptor));

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
			if (masterFrederick == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));
		
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(Collection));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterFrederick(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(Collection), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;

			masterFrederick.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "My young recruit. You are advancing very quickly, but now I have a somewhat [difficult assignment] I need for you to undertake.");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "Alright recruit, let's get started. You will need to find and defeat three key ire fairy generals, so to speak. According to my map, there is a [camp] to the north of this tower, to the northeast and to the south, near the stable across the road.");

					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Welcome back "+player.Name+". I take it you were successful in your mission. If so, hand me the first fairie wing.");
						quest.ChangeQuestStep(6);
					}
					else if (quest.Step == 9)
					{
						masterFrederick.SayTo(player, "For you, a pair of rugged sleeves, great for protecting your arms from the wildlife around here. Thank you again "+player.Name+". I promise my next task will be a little easier on the fighting. *hehe*");
						quest.FinishQuest();
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
						case "difficult assignment":
							masterFrederick.SayTo(player, "The plans I had translated by Scryer Alice revealed more of what the ire fairies plan on doing to and with Cotswold. I'm afraid we need to [teach them a lesson].");
							break;
						case "teach them a lesson":
							masterFrederick.SayTo(player, "We need to curb these issues before they get out of hand. Will you assist me with this [situation]?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "situation":
							QuestMgr.ProposeQuestToPlayer(typeof(Collection), "Will you find and slay these three ire fairies?", player, masterFrederick);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "camp":
							masterFrederick.SayTo(player, "You will need to use your compass in order to find your way to these locations. If your compass is not yet visible on your screen, use your shift + C keys to display it. Good luck.recruit. We're counting on you.");
							if (quest.Step == 1)
							{
                                GiveItemToPlayer(masterFrederick, CreateQuestItem(dustyOldMap, quest), player);
								quest.ChangeQuestStep(2);
							}
							break;

						case "reward":
							masterFrederick.SayTo(player, "For you, a pair of rugged sleeves, great for protecting your arms from the wildlife around here. Thank you again "+player.Name+". I promise my next task will be a little easier on the fighting. *hehe*");
							if (quest.Step == 9)
							{
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...            

			Collection quest = (Collection) player.IsDoingQuest(typeof (Collection));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot) as GenericItem;
			if (item != null && item.Name == dustyOldMap.Name)
			{
				if (quest.Step == 2)
				{
					SendReply(player, "Travel north from the guard tower and look for the campfire. Do not go too far past the field.");
				}
				else if (quest.Step == 3)
				{
					SendReply(player, " From the first location, travel east-northeast and search for the campfire. Look near the weeping willow trees. You have two more fairy generals to find.");
				}
				else if (quest.Step == 4)
				{
					SendReply(player, "Now make your way south from the second location to find the last general. She might be near the road, close to the field. Look around for a campfire. You have one more fairy general to find.");
				}
				else if (quest.Step == 5)
				{
					SendReply(player, "From the third location, travel west across the road to reach Master Frederick.");
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(Collection)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(Collection), player, gArgs.Source as GameNPC))
					{
						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
						GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

						masterFrederick.SayTo(player, "Alright recruit, let's get started. You will need to find and defeat three key ire fairy generals, so to speak. According to my map, there is a [camp] to the north of this tower, to the northeast and to the south, near the stable across the road.");
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{

					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
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
						return "[Step #1] Listen to Master Frederick. If you are stuck and he won't talk with you, ask him what [camp] he knows of for the Ire Fairies.";
					case 2:
						return "[Step #2] You must now USE the Dusty Old Map for directions to the first Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 3:
						return "[Step #3] You must now USE the Dusty Old Map for the directions to the second Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 4:
						return "[Step #4] You must now USE the Dusty Old Map for directions to the last Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 5:
						return "[Step #5] Return to Master Frederick in Cotswold. From the third location, travel west across the road.";
					case 6:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 7:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 8:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 9:
						return "[Step #7] Wait for Master Frederick to reward you. If he stops speaking with you, ask if you might have a [reward] for your troubles.";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Collection)) == null)
				return;

			if (Step >= 2 && Step <= 4 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target == general[Step - 2])
				{
					SendSystemMessage("You slay the creature and take it's wing.");
					GiveItemToPlayer(CreateQuestItem(fairyGeneralWings));
					SendSystemMessage(player, generalNames[Step - 2] + " yells at you, \"A curse on you land-bound monster! I curse you for killing me!\"");
					ChangeQuestStep((byte)(Step + 1));
					return;
				}
			}

			if (Step >= 6 && Step <= 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target == masterFrederick && gArgs.Item.Name == fairyGeneralWings.Name)
				{
					RemoveItemFromPlayer(masterFrederick, gArgs.Item);
					
					if (Step == 6)
					{
						masterFrederick.SayTo(player, "Excellent. Please, hand me the second one now.");
					}
					else if (Step == 7)
					{
						masterFrederick.SayTo(player, "Yes, now please, hand me the third one.");
					}
					else if (Step == 8)
					{
						masterFrederick.SayTo(player, "Ah, the third wing. Yes, excellent job "+player.Name+". Now, for your [reward]");
					}

					ChangeQuestStep((byte)(Step + 1));
					return;
				}
			}

		}

		public override void FinishQuest()
		{
			//Give reward to player here ...  
			RemoveItemFromPlayer(m_questPlayer.Inventory.GetFirstItemByName(dustyOldMap.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack));

			if (m_questPlayer.HasAbilityToUseItem(recruitsArms.CreateInstance() as ArmsArmor))
				GiveItemToPlayer(masterFrederick, recruitsArms.CreateInstance());
			else
				GiveItemToPlayer(masterFrederick, recruitsSleeves.CreateInstance());

			m_questPlayer.GainExperience(240, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 6, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
