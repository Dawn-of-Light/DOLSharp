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
	public class YdeniasCrushDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(YdeniasCrush); }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 5; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(YdeniasCrush), ExtendsType = typeof(AbstractQuest))]
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

		private static GameMob ydeniaPhilpott = null;
		private static GameMob elvarTambor = null;

		private static GenericItemTemplate letterToElvar= null;
		private static GenericItemTemplate letterToYdenia = null;

		private static RingTemplate silverRingOfHealth = null;

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

			ydeniaPhilpott = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Ydenia Philpott") as GameMob;
			if (ydeniaPhilpott == null)
			{
				ydeniaPhilpott = new GameMob();
				ydeniaPhilpott.Model = 6;
				ydeniaPhilpott.Name = "Ydenia Philpott";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + ydeniaPhilpott.Name + ", creating him ...");
				ydeniaPhilpott.GuildName = "Part of " + questTitle + " Quest";
				ydeniaPhilpott.Realm = (byte) eRealm.Albion;
				ydeniaPhilpott.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(227));
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(80));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(54));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(51));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(52));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(53));
				ydeniaPhilpott.Inventory = template;
				ydeniaPhilpott.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				ydeniaPhilpott.Size = 51;
				ydeniaPhilpott.Level = 40;
				ydeniaPhilpott.Position = new Point(559315, 510705, 2488);
				ydeniaPhilpott.Heading = 3993;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = ydeniaPhilpott;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				ydeniaPhilpott.OwnBrain = newBrain;

				if(!ydeniaPhilpott.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(ydeniaPhilpott);
			}

			elvarTambor = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Elvar Tambor") as GameMob;
			if (elvarTambor == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Elvar Tambor, creating him ...");
				elvarTambor = new GameMob();
				elvarTambor.Model = 9;
				elvarTambor.Name = "Elvar Tambor";
				elvarTambor.GuildName = "Part of " + questTitle + " Quest";
				elvarTambor.Realm = (byte) eRealm.Albion;
				elvarTambor.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(3));
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(159, 67, 0));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(160, 63, 0));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(156, 67, 0));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(157, 63, 0));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(158, 67, 0));
				elvarTambor.Inventory = template;
				elvarTambor.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				elvarTambor.Size = 50;
				elvarTambor.Level = 15;
				elvarTambor.Position = new Point(574711, 529887, 2896);
				elvarTambor.Heading = 2366;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = elvarTambor;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				elvarTambor.OwnBrain = newBrain;

				if(!elvarTambor.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(elvarTambor);
			}

			#endregion

			#region defineItems

			// item db check
			letterToElvar = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Letter to Elvar")) as GenericItemTemplate;
			if (letterToElvar == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Letter to Healvar, creating it ...");
				letterToElvar = new GenericItemTemplate();
				letterToElvar.Name = "Letter to Elvar";
				letterToElvar.Level = 0;
				letterToElvar.Weight = 10;
				letterToElvar.Model = 499;

				letterToElvar.IsDropable = false;
				letterToElvar.IsSaleable = false;
				letterToElvar.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterToElvar);
			}

			// item db check
			letterToYdenia = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Letter to Ydenia")) as GenericItemTemplate;
			if (letterToYdenia == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Letter to Yderia creating it ...");
				letterToYdenia = new GenericItemTemplate();
				letterToYdenia.Name = "Letter to Ydenia";
				letterToYdenia.Level = 0;
				letterToYdenia.Weight = 10;
				letterToYdenia.Model = 499;

				letterToYdenia.IsDropable = false;
				letterToYdenia.IsSaleable = false;
				letterToYdenia.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterToYdenia);
			}

			// item db check
			silverRingOfHealth = GameServer.Database.SelectObject(typeof(RingTemplate), Expression.Eq("Name", "Silver Ring of Health")) as RingTemplate;
			if (silverRingOfHealth == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Silver Ring of Health creating it ...");
				silverRingOfHealth = new RingTemplate();
				silverRingOfHealth.Name = "Silver Ring of Health";
				silverRingOfHealth.Level = 3;
				silverRingOfHealth.Weight = 1;
				silverRingOfHealth.Model = 103;

				silverRingOfHealth.Value = 30;

				silverRingOfHealth.IsDropable = true;
				silverRingOfHealth.IsSaleable = true;
				silverRingOfHealth.IsTradable = true;

				silverRingOfHealth.Bonus = 1;

				silverRingOfHealth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 8));
				silverRingOfHealth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 1));

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
			
			GameEventMgr.AddHandler(ydeniaPhilpott, GameObjectEvent.Interact, new DOLEventHandler(TalkToYdeniaPhilpott));
			GameEventMgr.AddHandler(ydeniaPhilpott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYdeniaPhilpott));

			GameEventMgr.AddHandler(elvarTambor, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarTambor));
			GameEventMgr.AddHandler(elvarTambor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarTambor));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(ydeniaPhilpott, typeof(YdeniasCrushDescriptor));

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
			GameEventMgr.RemoveHandler(ydeniaPhilpott, GameObjectEvent.Interact, new DOLEventHandler(TalkToYdeniaPhilpott));
			GameEventMgr.RemoveHandler(ydeniaPhilpott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYdeniaPhilpott));

			GameEventMgr.RemoveHandler(elvarTambor, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarTambor));
			GameEventMgr.RemoveHandler(elvarTambor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarTambor));
			
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(ydeniaPhilpott, typeof(AgainstTheGrainDescriptor));
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

			if (QuestMgr.CanGiveQuest(typeof(YdeniasCrush), player, ydeniaPhilpott) <= 0)
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
							QuestMgr.ProposeQuestToPlayer(typeof(YdeniasCrush), "Will you deliver the letter to \nElvar Tambor for Ydenia?", player, ydeniaPhilpott);
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
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			YdeniasCrush quest = player.IsDoingQuest(typeof(YdeniasCrush)) as YdeniasCrush;

			elvarTambor.TurnTo(player);

			// The player whispered to NPC (clicked on the text inside the [])
			if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
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
								GiveItemToPlayer(elvarTambor, CreateQuestItem(letterToYdenia, quest), player);
								
								quest.ChangeQuestStep(3);
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(YdeniasCrush)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(YdeniasCrush), player, gArgs.Source as GameNPC))
					{
						GiveItemToPlayer(ydeniaPhilpott, CreateQuestItem(letterToElvar, player.IsDoingQuest(typeof (YdeniasCrush))), player);

						SendReply(player, "Great!  Here you are!  Just take this letter to Elvar Tambor at Prydwen Keep.  I know he will be happy.");
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
						return "[Step #1] Deliver the letter Ydenia just handed to you to Elvar Tambor in Prydwen Keep.";
					case 2:
						return "[Step #2] Listen to Elvar.";
					case 3:
						return "[Step #3] Take Elvar's letter back to Ydenia in Cotswold Village.  She is in the tavern there.";
					case 4:
						return "[Step #4] Listen to Ydenia.  If she stops talking to you and becomes engrossed in her letter, ask her if she has [something] for you for your hard work.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
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
					if (gArgs.Target == elvarTambor && gArgs.Item.QuestName == Name && gArgs.Item.Name == letterToElvar.Name)
					{
						RemoveItemFromPlayer(elvarTambor, gArgs.Item);

						elvarTambor.TurnTo(m_questPlayer);
						elvarTambor.SayTo(m_questPlayer, "Ah!  A letter from my Ydenia.  Thank you for delivering it to me.  I can't wait to see what she has to say.  I was just sitting here wondering if there was going to be someone who was traveling back to [Cotswold].");
						
						ChangeQuestStep(2);
						return;
					}
				}

				if(Step == 3)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == ydeniaPhilpott && gArgs.Item.QuestName == Name && gArgs.Item.Name == letterToYdenia.Name)
					{
						RemoveItemFromPlayer(ydeniaPhilpott, gArgs.Item);

						ydeniaPhilpott.TurnTo(m_questPlayer);
						ydeniaPhilpott.SayTo(m_questPlayer, "Thank you friend!  Here, I have [something] for you.");
						
						ChangeQuestStep(4);
						return;
					}
				}
			}
		}

		public override void FinishQuest()
		{
			//Give reward to player here ...
			GiveItemToPlayer(ydeniaPhilpott, silverRingOfHealth.CreateInstance());

			m_questPlayer.GainExperience(20 + (m_questPlayer.Level - 1) * 5, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 5, Util.Random(50)), "You are awarded 5 silver and some copper!");
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
