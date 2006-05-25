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
 * 1) Travel to loc=11305,30802 Camelot Hills (Cotswold Village) to speak with Eowyln Astos
 * 2) Go to loc=12320,43841 Camelot Hills and kill some river sprite until Sephucoth pop
 * 2) Kill Sephucoth to have his heart and comme back to Eowyln Astos to give it
 * 3) Go to loc=25268,36339 and kill some large skeleton to drop a polished bone
 * 4) Come back to Camelot Hills and give the bone to Eowyln Astos to have your reward
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
	public class HeartOfSephucothDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(HeartOfSephucoth); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 7; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 14; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(HeartOfSephucoth), ExtendsType = typeof(AbstractQuest))]
	public class HeartOfSephucoth : BaseQuest
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
		protected const string questTitle = "Heart of Sephucoth";

		private static GameMob eowylnAstos = null;
		private static GameMob sephucoth = null;
		
		private static GenericItemTemplate sephucothsHeart = null;
		private static GenericItemTemplate polishedBone = null;

		private static NecklaceTemplate fieryCrystalPendant = null;

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

			eowylnAstos = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Eowyln Astos") as GameMob;
			if (eowylnAstos == null)
			{
				eowylnAstos = new GameMob();
				eowylnAstos.Model = 35;
				eowylnAstos.Name = "Eowyln Astos";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + eowylnAstos.Name + ", creating him ...");
				eowylnAstos.GuildName = "Part of " + questTitle + " Quest";
				eowylnAstos.Realm = (byte) eRealm.Albion;
				eowylnAstos.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(58, 40, 0));
				eowylnAstos.Inventory = template;
				eowylnAstos.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				eowylnAstos.Size = 54;
				eowylnAstos.Level = 17;
				eowylnAstos.Position = new Point(559680, 513793, 2619);
				eowylnAstos.Heading = 3185;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = eowylnAstos;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				eowylnAstos.OwnBrain = newBrain;

				if(!eowylnAstos.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(eowylnAstos);
			}

			#endregion

			#region defineItems

			// item db check
			sephucothsHeart = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Sephucoth's Heart")) as GenericItemTemplate;
			if (sephucothsHeart == null)
			{
				sephucothsHeart = new GenericItemTemplate();
				sephucothsHeart.Name = "Sephucoth's Heart";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+sephucothsHeart.Name+", creating it ...");
				sephucothsHeart.Level = 0;
				sephucothsHeart.Weight = 0;
				sephucothsHeart.Model = 595;

				sephucothsHeart.IsDropable = false;
				sephucothsHeart.IsSaleable = false;
				sephucothsHeart.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sephucothsHeart);
			}

			// item db check
			polishedBone = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Polished Bone")) as GenericItemTemplate;
			if (polishedBone == null)
			{
				polishedBone = new GenericItemTemplate();
				polishedBone.Name = "Polished Bone";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+polishedBone.Name+", creating it ...");
				polishedBone.Level = 0;
				polishedBone.Weight = 15;
				polishedBone.Model = 497;

				polishedBone.IsDropable = false;
				polishedBone.IsSaleable = false;
				polishedBone.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(polishedBone);
			}

			// item db check
			fieryCrystalPendant = GameServer.Database.SelectObject(typeof (NecklaceTemplate), Expression.Eq("Name", "Fiery Crystal Pendant")) as NecklaceTemplate;
			if (fieryCrystalPendant == null)
			{
				fieryCrystalPendant = new NecklaceTemplate();
				fieryCrystalPendant.Name = "Fiery Crystal Pendant";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+fieryCrystalPendant.Name+", creating it ...");
			
				fieryCrystalPendant.Level = 8;
				fieryCrystalPendant.Weight = 8;
				fieryCrystalPendant.Model = 101;
			
				fieryCrystalPendant.Value = 30;

				fieryCrystalPendant.IsDropable = true;
				fieryCrystalPendant.IsSaleable = true;
				fieryCrystalPendant.IsTradable = true;

				fieryCrystalPendant.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Fire, 1));
				fieryCrystalPendant.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fieryCrystalPendant);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(eowylnAstos, GameObjectEvent.Interact, new DOLEventHandler(TalkToEowylnAstos));
			GameEventMgr.AddHandler(eowylnAstos, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEowylnAstos));
			
			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(eowylnAstos, typeof(HeartOfSephucothDescriptor));

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
			if (eowylnAstos == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(eowylnAstos, GameObjectEvent.Interact, new DOLEventHandler(TalkToEowylnAstos));
			GameEventMgr.RemoveHandler(eowylnAstos, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEowylnAstos));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(eowylnAstos, typeof(HeartOfSephucothDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToEowylnAstos(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(HeartOfSephucothDescriptor), player, eowylnAstos) <= 0)
				return;

			//We also check if the player is already doing the quest
			HeartOfSephucoth quest = player.IsDoingQuest(typeof (HeartOfSephucoth)) as HeartOfSephucoth;

			eowylnAstos.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					eowylnAstos.SayTo(player, "Hail traveler! I may have a bit of [profitable information] for you!");
					return;
				}
				else
				{
					switch(quest.Step)
					{
						case 1:	
							eowylnAstos.SayTo(player, "You must seek out the monster Sephucoth! Slay it and bring me its heart!");
							break;

						case 2:
							eowylnAstos.SayTo(player, "Hand to me the heart needed for this construct.");
							break;

						case 4:
							eowylnAstos.SayTo(player, "Hand to me the polished bone needed for this construct.");
							break;
					}
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
						case "profitable information":
							eowylnAstos.SayTo(player, "I have learned how to [fashion a pendant] of immense value.");
							break;

						case "fashion a pendant":
							eowylnAstos.SayTo(player, "I can do so, but I would require the heart from a [terrible beast].");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "terrible beast":
							QuestMgr.ProposeQuestToPlayer(typeof(HeartOfSephucoth), "Do you accept the \nHeart of Sephucoth quest? \n[Levels 7-10]", player, eowylnAstos);
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(HeartOfSephucoth)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(HeartOfSephucoth), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Aye, the locals have given it the name Sephucoth. Return its heart to me and I will instruct you further.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
						return "[Step #1] Slay the river sprite Sephucoth and return to Eowyln Astos with his heart.";
					case 2:
						return "[Step #2] Give Sephucoth's heart to Eowyln.";
					case 3:
						return "[Step #3] Retrieve a piece of polished bone from a large skeleton.";
					case 4:
						return "[Step #4] Return the polished bone to Eowyln.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (HeartOfSephucoth)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if(Step == 1)
				{
					if (gArgs.Target.Name == "river sprite")
					{
						if (Util.Chance(25))
						{
							if(sephucoth == null)
							{
								sephucoth = new GameMob();
								sephucoth.Model = 136;
								sephucoth.Name = "Sephucoth";
								sephucoth.Realm = (byte) eRealm.None;
								sephucoth.Region = WorldMgr.GetRegion(1);

								sephucoth.Size = 55;
								sephucoth.Level = 7;
								sephucoth.Position = new Point(560836, 527260, 2082);
								sephucoth.Heading = 1480;

								StandardMobBrain brain = new StandardMobBrain();  // set a brain witch find a lot mob friend to attack the player
								brain.Body = sephucoth;
								brain.AggroLevel = 100;
								brain.AggroRange = 500;
								sephucoth.OwnBrain = brain;					  // so this mob must be abble to cast a poison

								sephucoth.AddToWorld();
							}
						}
					}
					else if (sephucoth != null && gArgs.Target == sephucoth)
					{
						GiveItemToPlayer(CreateQuestItem(sephucothsHeart));
						if(sephucoth != null) { sephucoth = null; }

						ChangeQuestStep(2);
					}
				}
				else if(Step == 3)
				{
					if (gArgs.Target.Name == "large skeleton")
					{
						if (Util.Chance(50))
						{
							GiveItemToPlayer(CreateQuestItem(polishedBone));
							ChangeQuestStep(4);
						}
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target == eowylnAstos && gArgs.Item.QuestName == Name)
				{
					if(Step == 2 && gArgs.Item.Name == sephucothsHeart.Name)
					{
						RemoveItemFromPlayer(eowylnAstos, gArgs.Item);

						eowylnAstos.TurnTo(m_questPlayer);
						eowylnAstos.SayTo(m_questPlayer, "You have done well traveler! I will still require one final object to complete the pendant. Seek out a large skeleton and bring from it a piece of polished bone! Return this to me and I shall finish your pendant.");
						ChangeQuestStep(3);
					}
					else if(Step == 4 && gArgs.Item.Name == polishedBone.Name)
					{
						RemoveItemFromPlayer(eowylnAstos, gArgs.Item);

						eowylnAstos.TurnTo(m_questPlayer);
						eowylnAstos.SayTo(m_questPlayer, "Eowyln draws two items before her. Gathering her strength, she shouts.");
						
						new RegionTimer(eowylnAstos, new RegionTimerCallback(BuildNecklace), 5000);
					}
				}
			}
		}

		protected virtual int BuildNecklace(RegionTimer callingTimer)
		{
			eowylnAstos.Emote(eEmote.Yes);
			SendMessage(m_questPlayer, "Eowyln carefully fashions a delicate necklace about the crystal and smiles.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
											
			FinishQuest();
			return 0;
		}

		public override void FinishQuest()
		{
			//Give reward to player here ...
			eowylnAstos.SayTo(m_questPlayer, "Here is your pendant. I thank you for allowing me the opportunity to create such a great item!");
			GiveItemToPlayer(eowylnAstos, fieryCrystalPendant.CreateInstance());
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
	
