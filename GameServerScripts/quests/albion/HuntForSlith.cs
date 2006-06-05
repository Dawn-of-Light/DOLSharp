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
 * 1) Travel to loc=33902,61495 Black Mountain South to speak with Commander Burcrif
 * 2) Go to loc=41272,55711 Black Mountain South and kill some slith broodling until Slith pop
 * 2) Kill Slith to have the Slith's tail as reward
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
	public class HuntForSlithDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(HuntForSlith); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 4; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 8; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(HuntForSlith), ExtendsType = typeof(AbstractQuest))]
	public class HuntForSlith : BaseQuest
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
		protected const string questTitle = "Hunt for Slith";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 8;

		private static GameNPC commanderBurcrif = null;
		private static GameNPC slith = null;
		
		private static RingTemplate slithsTail = null;

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

			commanderBurcrif = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Commander Burcrif") as GameMob;
			if (commanderBurcrif == null)
			{
				commanderBurcrif = new GameMob();
				commanderBurcrif.Model = 28;
				commanderBurcrif.Name = "Commander Burcrif";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + commanderBurcrif.Name + ", creating him ...");
				commanderBurcrif.GuildName = "Part of " + questTitle + " Quest";
				commanderBurcrif.Realm = (byte) eRealm.Albion;
				commanderBurcrif.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(26));
				template.AddItem(eInventorySlot.HeadArmor, new NPCArmor(93));
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(49));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(50));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(662));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(91));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(47));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(48));
				commanderBurcrif.Inventory = template;
				commanderBurcrif.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				commanderBurcrif.Size = 53;
				commanderBurcrif.Level = 45;
				commanderBurcrif.Position = new Point(517270, 495711, 3352);
				commanderBurcrif.Heading = 2093;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = commanderBurcrif;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				commanderBurcrif.OwnBrain = newBrain;

				if(!commanderBurcrif.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(commanderBurcrif);
			}


			#endregion

			#region defineItems

			// item db check
			slithsTail = GameServer.Database.SelectObject(typeof (RingTemplate), Expression.Eq("Name", "Slith's Tail")) as RingTemplate;
			if (slithsTail == null)
			{
				slithsTail = new RingTemplate();
				slithsTail.Name = "Slith's Tail";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+slithsTail.Name+", creating it ...");
				
				slithsTail.Level = 7;
				slithsTail.Weight = 10;
				slithsTail.Model = 515;
				
				slithsTail.Value = 30;

				slithsTail.IsDropable = true;
				slithsTail.IsSaleable = true;
				slithsTail.IsTradable = true;

				slithsTail.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(slithsTail);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(commanderBurcrif, GameObjectEvent.Interact, new DOLEventHandler(TalkToCommanderBurcrif));
			GameEventMgr.AddHandler(commanderBurcrif, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCommanderBurcrif));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(commanderBurcrif, typeof(HuntForSlithDescriptor));

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
			if (commanderBurcrif == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(commanderBurcrif, GameObjectEvent.Interact, new DOLEventHandler(TalkToCommanderBurcrif));
			GameEventMgr.RemoveHandler(commanderBurcrif, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCommanderBurcrif));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(commanderBurcrif, typeof(HuntForSlithDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToCommanderBurcrif(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(HuntForSlith), player, commanderBurcrif) <= 0)
				return;

			//We also check if the player is already doing the quest
			HuntForSlith quest = player.IsDoingQuest(typeof (HuntForSlith)) as HuntForSlith;

			commanderBurcrif.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					commanderBurcrif.SayTo(player, "Hail! I hear tell of a rumor. Would you perhaps like to [hear it]?");
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
						case "hear it":
							commanderBurcrif.SayTo(player, "Some talk of a red snake that will only come should he become [displeased].");
							break;

						case "displeased":
							commanderBurcrif.SayTo(player, "I know not what might truly displease a snake, but I do know where he may be found. Are you [interested]?");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "interested":
							QuestMgr.ProposeQuestToPlayer(typeof(HuntForSlith), "Do you accept the Hunt for Slith quest? \n[Levels 4-8]", player, commanderBurcrif);
							break;
					}
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(HuntForSlith)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(HuntForSlith), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Good! This rare snake was last seen to the east. Not far from the walls of Camelot. Best of luck in your endeavor!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
						return "[Step #1] Locate Slith by angering him. Then slay the beast!";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}


		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (HuntForSlith)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if(Step == 1)
				{
					if (gArgs.Target.Name == "slith broodling")
					{
						if (Util.Chance(25))
						{
							if(slith == null)
							{
								slith = new GameMob();
								slith.Model = 31;
								slith.Name = "Slith";
								slith.Realm = (byte) eRealm.None;
								slith.Region = WorldMgr.GetRegion(1);

								slith.Size = 50;
								slith.Level = 7;
								slith.Position = new Point(524840, 490529, 2545);
								slith.Heading = 2082;

								StandardMobBrain brain = new StandardMobBrain();  // set a brain witch find a lot mob friend to attack the player
								brain.Body = slith;
								brain.AggroLevel = 100;
								brain.AggroRange = 1000;
								slith.SetOwnBrain(brain);

								slith.AddToWorld();
							}
						}
					}
					else if (gArgs.Target.Name == "Slith")
					{
						if(slith != null) { slith = null; }
						FinishQuest();
					}
				}
			}
		}

		public override void FinishQuest()
		{
			GiveItemToPlayer(CreateQuestItem(slithsTail));		
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}