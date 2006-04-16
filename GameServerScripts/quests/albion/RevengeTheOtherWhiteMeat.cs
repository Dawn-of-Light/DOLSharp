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
 * 1) Travel to loc=15075,25906 Camelot Hills to speak with Farmer Asma
 * 2) Go to loc=17318,57079 Black Mountain South and kill the pig Wilbur
 * 3) Come back to Farmer Asma and you will have your reward. 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using NHibernate.Expression;
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
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class RevengeTheOtherWhiteMeatDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(RevengeTheOtherWhiteMeat); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 5; }
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(RevengeTheOtherWhiteMeat), ExtendsType = typeof(AbstractQuest))]
	public class RevengeTheOtherWhiteMeat : BaseQuest
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
		protected const string questTitle = "Revenge, the Other White Meat";

		private static GameMob farmerAsma = null;
		
		private static GameLocation wilburSpawnLocation = new GameLocation("Wilbur Location", 1, 500646, 491255, 2298);
		
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

			farmerAsma = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Farmer Asma") as GameMob;
			if (farmerAsma == null)
			{
				farmerAsma = new GameMob();
				farmerAsma.Model = 82;
				farmerAsma.Name = "Farmer Asma";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + farmerAsma.Name + ", creating him ...");
				farmerAsma.GuildName = "Part of " + questTitle + " Quest";
				farmerAsma.Realm = (byte) eRealm.Albion;
				farmerAsma.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(31));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(32));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(33));
				farmerAsma.Inventory = template;
				farmerAsma.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				farmerAsma.Size = 50;
				farmerAsma.Level = 35;
				farmerAsma.Position = new Point(563939, 509234, 2744);
				farmerAsma.Heading = 21;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = farmerAsma;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				farmerAsma.OwnBrain = newBrain;

				if(!farmerAsma.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(farmerAsma);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.AddHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(farmerAsma, typeof(RevengeTheOtherWhiteMeatDescriptor));

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
			if (farmerAsma == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			
			GameEventMgr.RemoveHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.RemoveHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));
			
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(farmerAsma, typeof(RevengeTheOtherWhiteMeatDescriptor));
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

			if (QuestMgr.CanGiveQuest(typeof(RevengeTheOtherWhiteMeat), player, farmerAsma) <= 0)
				return;

			//We also check if the player is already doing the quest
			RevengeTheOtherWhiteMeat quest = player.IsDoingQuest(typeof (RevengeTheOtherWhiteMeat)) as RevengeTheOtherWhiteMeat;

			farmerAsma.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					farmerAsma.SayTo(player, "Greetings, "+player.CharacterClass.Name+".  You wouldn't believe how expensive it is to lease land here in the Camelot Hills area. Just the other day, I went to check out some fields, and the asking price is just too high.  Things were better in the Black [Mountains].");
					return;
				}
				else
				{
					if (quest.Step == 3)
					{
						farmerAsma.SayTo(player, "Did you really kill [Wilbur]?");
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
						case "Mountains":
							farmerAsma.SayTo(player, "It seemed like an ideal place for a farm.  It's shame those pig herders had to select it as the site for their new 'sport.' I still get angry just thinking about it. I mean, who do they think they are, tossing me out [like that]?.");
							break;
						case "like that":
							farmerAsma.SayTo(player, "I wish I could give them a taste of their own medicine. If someone took something important away from them... Hey, that gives me an idea! Are you willing to help me get a little revenge on those pig herders?");
							QuestMgr.ProposeQuestToPlayer(typeof(RevengeTheOtherWhiteMeat), "Will you help Farmer Asma get \nrevenge on the pig herders?\n[Level 5-8]", player, farmerAsma);
							break;
					}
				}
				else
				{	
					switch (wArgs.Text)
					{
						case "mind":
							if(quest.Step == 1)
							{
								farmerAsma.SayTo(player, "Just as a farmer's land is the source of her livelihood, the pig herders' prize pig must be the center of their game. If a little accident should befall their precious pig, they will know exactly how I felt when my farm was [taken away].");
							}
							break;

						case "taken away":
							if(quest.Step == 1)
							{
								farmerAsma.SayTo(player, "When the game isn't in session, they keep the pig in an area adjacent to the field.  To get there, travel through the city of Camelot and exit through the North Gate, near the main Church building. After exiting the city, you'll need to travel [west].");
							}
							break;

						case "west":
							if(quest.Step == 1)
							{
								farmerAsma.SayTo(player, "You'll see Vetusta Abbey in the distance. Keep running toward it, and you'll see a field and some stables. The pigs should be in that area. The pig you're looking for is one they've nicknamed Wilbur.");
								quest.ChangeQuestStep(2);
							}
							break;

						case "Wilbur":
							if(quest.Step == 3)
							{
								SendMessage(player, "You tell Farmer Asma that you succeeded in killing Wilbur, but that one of the pig herders discovered you and chased you away.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								farmerAsma.SayTo(player, "They saw you? Oh, no...what have I done? I feel terrible, Gwonn.  I shouldn't have asked you to kill Wilbur, but we can't undo what's been done.  If the guards come looking for you, I'll tell them that I'm the one who did [the deed].");
							}
							break;

						case "the deed":
							if(quest.Step == 3)
							{
								farmerAsma.SayTo(player, "You don't think they'll come after me, do you? I mean, pigs die all the time. We make food out of them, and they turn into pork and bacon and all kinds of good [things]...");
							}
							break;

						case "things":
							if(quest.Step == 3)
							{
								SendMessage(player, "Farmer Asma begins to ramble and soon becomes incoherent. When she realizes what's happened, she takes a deep breath and tries to compose herself.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								farmerAsma.SayTo(player, "Oh, my. I'm so sorry, I just...well, I don't know what to do. I need a break from farming and pigs, and rural life. Maybe I'll take up something completely different...like brewing. Yes, brewing! Please take these coins and never speak of this again.");
								quest.FinishQuest();
							}
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(RevengeTheOtherWhiteMeat)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(RevengeTheOtherWhiteMeat), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Farmer Asma rubs her hands together in anticipation.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						SendReply(player, "Excellent. Thank you for agreeing to help. Here's what I have in [mind].");
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
						return "[Step #1] Continue speaking to Farmer Asma about the plan she has in [mind] to get revenge on the pig herders.";
					case 2:
						return "[Step #2] Farmer Asma wants you to kill the pig herders' favorite puck, 'Wilbur.' To find Wilbur, enter Camelot city and exit through the North Gate. Travel west until you see Vetusta Abbey. Wilbur will be in the fields in that area.";
					case 3:
						return "[Step #3] Now that you've killed Wilbur and made your escape, return to Farmer Asma in the camp near the Shrouded Isles portal in Cotswold, and give her the news.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (RevengeTheOtherWhiteMeat)) == null)
				return;


			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "Wilbur")
					{
						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've succeeded in killing Wilbur. In the \ndistance you hear the angry voice of \na pig herder. Make your escape!");
						ChangeQuestStep(3);

						player.GainExperience(player.ExperienceForNextLevel / 25, 0, 0, true);
						
						GameNPC pigHerderWyatt = new GameMob();
						pigHerderWyatt.Model = 39;
						pigHerderWyatt.Name = "Pig Herder Wyatt";
						pigHerderWyatt.Realm = (byte) eRealm.Albion;
						pigHerderWyatt.Region = WorldMgr.GetRegion(1);

						GameNpcInventory template = new GameNpcInventory();
						template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(143));
						template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(1005));
						pigHerderWyatt.Inventory = template;
						pigHerderWyatt.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

						pigHerderWyatt.Size = 54;
						pigHerderWyatt.Level = 33;
						Point pos = wilburSpawnLocation.Position;
						pos.X -= 1000;
						pos.Y += 1500;
						pigHerderWyatt.Position = pos;
						pigHerderWyatt.Heading = 2548;
						pigHerderWyatt.AddToWorld();

						GameEventMgr.AddHandler(pigHerderWyatt, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnCloseToDeadWilbur));
						
						pos = gArgs.Target.Position;
						pos.X -= 90;
						pos.Y += 90;
						pigHerderWyatt.WalkTo(pos, 200);
						
						return;
					}
				}
			}
		}

		protected void OnCloseToDeadWilbur(DOLEvent e, object n, EventArgs args) 
		{			
			GameNPC pigHerderWyatt = n as GameNPC;
			if(pigHerderWyatt != null)
			{
				GameEventMgr.RemoveHandler(pigHerderWyatt, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnCloseToDeadWilbur));
				pigHerderWyatt.Emote(eEmote.Beg);
				pigHerderWyatt.Yell("Wilbur? What have you done to Wilbur, you scoundrel!");
				
				RegionTimer waitTimer = new RegionTimer(pigHerderWyatt);
				waitTimer.Callback = new RegionTimerCallback(OnCloseToDeadWilburCallBack);
				waitTimer.Properties.setProperty(questTitle, pigHerderWyatt);
				waitTimer.Start(4000);
			}
		}

		public int OnCloseToDeadWilburCallBack(RegionTimer timer)
		{
			GameNPC pigHerderWyatt = (GameNPC)timer.Properties.getObjectProperty(questTitle, null);
			if (pigHerderWyatt != null)
			{
				pigHerderWyatt.Yell("The King's men will hear about this!!! Oh, Wilbur...");
				pigHerderWyatt.Emote(eEmote.Cry);
				GameEventMgr.AddHandler(pigHerderWyatt, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnRemovePigHerder));
				Point pos = wilburSpawnLocation.Position;
				pos.X -= 1000;
				pos.Y += 1500;
				pigHerderWyatt.WalkTo(pos, 200);
			}
			return 0;
		}

		protected void OnRemovePigHerder(DOLEvent e, object n, EventArgs args) 
		{			
			GameNPC pigHerderWyatt = n as GameNPC;
			if(pigHerderWyatt != null)
			{
				GameEventMgr.RemoveHandler(pigHerderWyatt, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnRemovePigHerder));
				pigHerderWyatt.RemoveFromWorld();
			}
		}


		public override void FinishQuest()
		{
			//Give reward to player here ...
			m_questPlayer.GainExperience(m_questPlayer.ExperienceForNextLevel / 25, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, m_questPlayer.Level * 10 + 30), "You are awarded "+m_questPlayer.Level * 10 + 30+" copper!");
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}

