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
 * Directory: /scripts/quests/examples/
 * 
 * Description:
 * This script demonstrates how quests can be given to a player without
 * the use of a npc. Such script can be easily used to make custom events ...
 *
 * The quest dialog will automaticaly appear to all players at the start3
 * of their 3eme level. The quest dialog will only be show only to Fighter
 *
 * During this quest, the quest-description will be updated and
 * the step increased. More documentation inside the quest itself! 
 */

using System;
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using NHibernate.Mapping.Attributes;
using log4net;

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
	public class SimpleGlobalQuestDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(SimpleGlobalQuest); }
		}

		/* This value is used to retrieves how many times the 
		 * player can do the quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MaxQuestCount
		{
			get { return 1; }
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

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			if(base.CheckQuestQualification(player) == false)
				return false;

			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

			if (player.CharacterClass.Name != "Fighter")
				return false;

			return true;
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType=typeof(SimpleGlobalQuest), ExtendsType=typeof(AbstractQuest))] 
	public class SimpleGlobalQuest : BaseQuest
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
		private static GameNPC evilSpider = null;


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

			#region Define NPC and ItemTemplate
			
			GameNPC[] npcs = WorldMgr.GetNPCsByName("The evil spider", eRealm.None);

			/* If the npcs array length is 0 then no Sir Quait exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"SimpleGlobalQuest\" Quest could not find The evil spider, creating him ...");
				evilSpider = new GameMob();
				evilSpider.Model = 72;
				evilSpider.Name = "The evil spider";
				evilSpider.GuildName = "Part of DOL Quest Example";
				evilSpider.Realm = (byte) eRealm.None;
				evilSpider.RegionId = 1;
				evilSpider.Size = 90;
				evilSpider.Level = 3;
				evilSpider.Position = new Point(534000, 478955, 0);
				evilSpider.Heading = 1500;

				/* You don't have to store the created mob in the db if you don't want,
				 * it will be recreated each time it is not found, just give the value
				 * you want to SAVE_INTO_DATABASE in the file BaseQuest.cs
				 */
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(evilSpider);

				evilSpider.AddToWorld();
			}
			else
				evilSpider = npcs[0];
			#endregion

			/* Actually, we want to know when a player level up so we hook the
			 * GamePlayerEvent.LevelUp event and set the callback method to
			 * the "PlayerLevelUp" method. This means, the "PlayerLevelUp" method
			 * is called whenever a player level up in the game.
			*/
			GameEventMgr.AddHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(PlayerLevelUp));

			/* Now we add some hooks to trigger the quest dialog reponse.
			 */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogReponse));


			/* Now we register the linked quest descriptor */
			QuestMgr.AddQuestDescriptor(typeof (SimpleGlobalQuestDescriptor));

			if (log.IsInfoEnabled)
				log.Info("SimpleGlobalQuest initialized");
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
			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(PlayerLevelUp));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogReponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogReponse));

			/* Now we unregister the linked quest descriptor */
			QuestMgr.RemoveQuestDescriptor(typeof (SimpleGlobalQuestDescriptor));
		}


		/* This is the method we declared as callback for the hooks we set in
		 * the ScriptLoaded method. It will be called whenever a player levl up.
		 */
		protected static void PlayerLevelUp(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			//Now we check if the player is qualified for the quest
			//and only let the player do the quest MaxQuestCount times
			if( QuestMgr.CanGiveQuest(typeof (SimpleGlobalQuest), player)  <= 0)
				return;
		
			QuestMgr.ProposeQuestToPlayer(typeof (SimpleGlobalQuest), "Do you want to do a very simple quest?", player);			
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogReponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if(gArgs != null && gArgs.QuestDescriptor.LinkedQuestType.Equals(typeof (SimpleGlobalQuest)))
			{
				GamePlayer player = gArgs.Player;
				if(player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if(QuestMgr.GiveQuestToPlayer(typeof (SimpleGlobalQuest), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Well, it is very simple. Just kill the evil spider and you will have a reward.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
				}
				else if(e == GamePlayerEvent.DeclineQuest)
				{

					player.Out.SendMessage("It is like you want but you are loosing a lot of money ...", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
			}
		}


		/* Now we need to set the quest name.
		 * We must override the base method to do it.
		 */
		public override string Name
		{
			get { return "Simple global quest"; }
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
						return "[Step #1] Kill the evil spider!";
                    default:
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

			if (player == null || player.IsDoingQuest(typeof (SimpleGlobalQuest)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == evilSpider.Name)
				{
					player.Out.SendMessage("You defeated the evil spider!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			m_questPlayer.AddMoney(500, "Your gain {0} to kill the spider!");
			base.FinishQuest(); // This function must be called at the end
		}

	}
}
