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
 * Author:		SmallHorse & Duff
 * Date:			12th September 2004
 * Directory: /scripts/quests/examples/
 * 
 * Description:
 * This script demonstrates how quests can be added to DOL
 * In this case, it searches for two specific mobs on startup
 * and if it can not find them, it creates them. Once the mobs
 * are found/created, some hooks are added.
 * 
 * Once the player talks to "Sir Quait", his qualification for
 * the quest will be checked (Fighter & Level > 2) and if he
 * matches the qualification, he will be offered the quest.
 * 
 * If he accepts the quest, it will be added to his quest list
 * and he is sent on the journey to find the "Evil Thief", kill
 * him and retrieve the sword of "Sir Quait" and bring it back
 * to him.
 * 
 * During this quest, the quest-description will be updated and
 * the step increased. More documentation inside the quest itself! 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Mapping.Attributes;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Examples
{
	
	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class HelpSirQuaitDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(HelpSirQuait); }
		}

		/* This value is used to retrieves how many times the 
		 * player can do the quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MaxQuestCount
		{
			get { return 3; }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
//		public override int MinLevel
//		{
//			get { return 1; }
//		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 10; }
		}

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the linked quest his class is no longer of relevance
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

			if (player.CharacterClass.Name != "Fighter")
				return false;

			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType=typeof(HelpSirQuait), ExtendsType=typeof(AbstractQuest))] 
	public class HelpSirQuait : BaseQuest
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
		 * We store our two mobs as static variables, since we need them
		 */
		private static GameMob sirQuait = null;
		private static GameMob evilThief = null;
		private static SlashingWeaponTemplate sirQuaitsSwordTemplate = null;


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
		 * want. We will do it the standard way here ... and make Sir Quait wail
		 * a bit about the loss of his sword! 
		 */
		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
		   /* First thing we do in here is to search for a Sir Quait NPC inside
			* the world who comes from the Albion realm. If we find a Sir Quait,
			* this means we don't have to create a new one. 
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region Define NPC
			
				/* If sirQuait == null then no Sir Quait exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one.
				*/
			sirQuait = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Sir Quait") as GameMob;
			if (sirQuait == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Sir Quait, creating him ...");
				sirQuait = new GameMob();
				sirQuait.Position = new Point(531971, 478955, 0);
				sirQuait.Heading = 3570;
				sirQuait.Region = WorldMgr.GetRegion(1);
				sirQuait.Name = "Sir Quait";
				sirQuait.Model = 40;
				sirQuait.Realm = (byte) eRealm.Albion;
				sirQuait.Level = 10;
				sirQuait.GuildName = "Part of DOL Quest Example";
				sirQuait.Size = 50;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = sirQuait;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				sirQuait.OwnBrain = newBrain;

				if(!sirQuait.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest HelpSirQuait quest abort because a needed region is not in use in this server!");
					return;
				}

				/* You don't have to store the created mob in the db if you don't want,
				 * it will be recreated each time it is not found, just give the value
				 * you want to SAVE_INTO_DATABASE in the file BaseQuest.cs
				 */
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sirQuait);
			}
			

			/* Now we do the same for the evil thief who stole the sword from
				* Sir Quait. Same procedure as with Sir Quait above
				*/
			evilThief = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, "Evil Thief of the Shadowclan") as GameMob;
			if (evilThief == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Evil Thief of the Shadowclan, creating default");
				evilThief = new GameMob();
				evilThief.Model = 55;
				evilThief.Name = "Evil Thief of the Shadowclan";
				evilThief.GuildName = "Part of DOL Quest Example";
				evilThief.Realm = (byte) eRealm.None; //Needs to be none, else we can't kill him ;-)
				evilThief.Region = WorldMgr.GetRegion(1);
				evilThief.Size = 50;
				evilThief.Level = 1;
				evilThief.Position = new Point(532571, 479055, 0);
				evilThief.Heading = 3570;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = evilThief;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				evilThief.OwnBrain = newBrain;

				if(!evilThief.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest HelpSirQuait quest abort because a needed region is not in use in this server!");
					return;
				}

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(evilThief);
			}

			#endregion

			#region Define ItemTemplate

			sirQuaitsSwordTemplate = (SlashingWeaponTemplate) GameServer.Database.FindObjectByKey(typeof (SlashingWeaponTemplate), "SirQuaitsSword");
			if (sirQuaitsSwordTemplate == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Sir Quait's Sword, creating it ...");

				sirQuaitsSwordTemplate = new SlashingWeaponTemplate();
				sirQuaitsSwordTemplate.ItemTemplateID = "SirQuaitsSword";
				sirQuaitsSwordTemplate.Name = "Sir Quaits Sword";
				sirQuaitsSwordTemplate.Level = 1;
				sirQuaitsSwordTemplate.Durability = 100;
				sirQuaitsSwordTemplate.Condition = 100;
				sirQuaitsSwordTemplate.Quality = 90;
				sirQuaitsSwordTemplate.Bonus = 0;
				sirQuaitsSwordTemplate.DamagePerSecond = 12;
				sirQuaitsSwordTemplate.Speed = 2500;
				sirQuaitsSwordTemplate.Weight = 10;
				sirQuaitsSwordTemplate.Model = 3;
				sirQuaitsSwordTemplate.Realm = eRealm.Albion;
				sirQuaitsSwordTemplate.IsDropable = true; 
				sirQuaitsSwordTemplate.IsTradable = true; 
				sirQuaitsSwordTemplate.IsSaleable = true;
				sirQuaitsSwordTemplate.MaterialLevel = eMaterialLevel.Bronze;

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sirQuaitsSwordTemplate);
			}
			#endregion

			/* Now we add some hooks to the Sir Quait we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of Sir Quait and set the callback method to the "TalkToSirQuait"
			* method. This means, the "TalkToSirQuait" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			GameEventMgr.AddHandler(sirQuait, GameObjectEvent.Interact, new DOLEventHandler(TalkToSirQuait));
			GameEventMgr.AddHandler(sirQuait, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirQuait));

			/* Now we add some hooks to trigger the quest dialog reponse.
			 */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.AddHandler(GamePlayerEvent.AbortQuest, new DOLEventHandler(QuestCancelDialogResponse));

			/* Now we bring to SirQuait the possibility to give this quest to players,
			 */
			QuestMgr.AddQuestDescriptor(sirQuait, typeof (HelpSirQuaitDescriptor));

			if (log.IsInfoEnabled)
				log.Info("HelpSirQuait Quest initialized");
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
			if (sirQuait == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(sirQuait, GameObjectEvent.Interact, new DOLEventHandler(TalkToSirQuait));
			GameEventMgr.RemoveHandler(sirQuait, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirQuait));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.AbortQuest, new DOLEventHandler(QuestCancelDialogResponse));
			
			/* Now we remove to SirQuait the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(sirQuait, typeof (HelpSirQuaitDescriptor));
		}


		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */
		protected static void TalkToSirQuait(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//Now we check if the player is qualified for the quest
			//and only let the player do the quest 3 times
			if (QuestMgr.CanGiveQuest(typeof(HelpSirQuait), player, sirQuait) <= 0)
				return;

			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (player.IsDoingQuest(typeof (HelpSirQuait)) != null)
				{
					//If the player is already doing the quest, we ask if he found the sword!
					player.Out.SendMessage("Did you [find] my [sword]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					return;
				}

				//If the player qualifies, we begin talking...
				player.Out.SendMessage("Hello adventurer, an [evil thief] has stolen my [sword], can [you help me] get it back?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return;
			}
				// The player whispered to Sir Quait (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				//We also check if the player is already doing the quest
				if (player.IsDoingQuest(typeof (HelpSirQuait)) == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "sword":
							player.Out.SendMessage("I really need it and if [you help me], I will give you a little reward!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;
						case "evil thief":
							player.Out.SendMessage("This evil thief of the shadowclan bastard, he stole it! Kill him and get my [sword] back to me!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "you help me":
							QuestMgr.ProposeQuestToPlayer(typeof (HelpSirQuait), "Do you want to help Sir Quait?", player, sirQuait);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "sword":
							player.Out.SendMessage("I really need it I am so glad you are helping me!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;
						case "find":
							player.Out.SendMessage("Really!? Please give it to me!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogReponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if(gArgs != null && gArgs.QuestType.Equals(typeof (HelpSirQuait)))
			{
				GamePlayer player = gArgs.Player;
				if(player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if(QuestMgr.GiveQuestToPlayer(typeof (HelpSirQuait), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Thank you! Please bring the [sword] back to me!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					}
				}
				else if(e == GamePlayerEvent.DeclineQuest)
				{

					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest cancel dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestCancelDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestCancelEventArgs gArgs = args as QuestCancelEventArgs;

			if (gArgs != null && gArgs.Quest.GetType().Equals(typeof(HelpSirQuait)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AbortQuest)
				{
					RemoveItemFromPlayer(player.Inventory.GetFirstItemByName(sirQuaitsSwordTemplate.Name, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv), player);

					gArgs.Quest.AbortQuest();
				}		
			}
		}


		/* Now we need to set the quest name.
		 * We must override the base method to do it.
		 */
		public override string Name
		{
			get { return "Help Sir Quait to find his sword"; }
		}

		/* Now we set the quest step descriptions.
		 * You must do it by overriding the base
		 * method.
		 */
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Find the evil thief and get the magic sword from him!";
					case 2:
						return "[Step #2] Bring back Sir Quait's magic sword to receive your reward!";
					default :
                        return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		/* This method needs to be implemented in each quest.
		 * It is the core of the quest. The global event hook of the GamePlayer.
		 * This method will be called whenever a GamePlayer with this quest
		 * fires ANY event!
		 */
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (HelpSirQuait)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == evilThief.Name)
				{
					player.Out.SendMessage("You defeated the evil thief and quickly pick up Sir Quait's sword!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(sirQuaitsSwordTemplate));
					ChangeQuestStep(2);
					return;
				}
			}
			else if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == sirQuait.Name && gArgs.Item.Name == sirQuaitsSwordTemplate.Name)
				{
					RemoveItemFromPlayer(sirQuait, gArgs.Item);
					m_questPlayer.Out.SendMessage("Sir Quait thanks you for bringing back his holy sword!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					FinishQuest();
					return;
				}
			}
		}

		/* This method is the end of the quest
		 * Use it to give thanks and reward to the player.
		 * You must call the base function at the end 
		 * of it to save the finished quest in the db.
		 */
		public override void FinishQuest()
		{
			base.FinishQuest(); // This function must be called at the end
		}
	}
}
