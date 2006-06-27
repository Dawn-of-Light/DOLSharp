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

namespace DOL.GS.Quests.Examples
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 
	 * This quest for example will be stored in the database with
	 * the name: DOL.GS.Quests.Examples.SirQuaitHelp
	 */

	public class HelpSirQuait : AbstractQuest
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
		 * We store our two mobs as static variables, since we need them
		 */
		private static GameNPC sirQuait = null;
		private static GameNPC evilThief = null;
		private static ItemTemplate sirQuaitsSword = null;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public HelpSirQuait() : base()
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
			GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Quait", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no Sir Quait exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Sir Quait, creating him ...");
				sirQuait = new GameMob();
				sirQuait.Model = 40;
				sirQuait.Name = "Sir Quait";
				sirQuait.GuildName = "Part of DOL Quest Example";
				sirQuait.Realm = (byte) eRealm.Albion;
				sirQuait.CurrentRegionID = 1;
				sirQuait.Size = 50;
				sirQuait.Level = 10;
				sirQuait.X = 531971;
				sirQuait.Y = 478955;
				sirQuait.Z = 0;
				sirQuait.Heading = 3570;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				sirQuait.SaveIntoDatabase();

				sirQuait.AddToWorld();
			}
			else
				sirQuait = npcs[0];

			/* Now we do the same for the evil thief who stole the sword from
				* Sir Quait. Same procedure as with Sir Quait above
				*/
			npcs = WorldMgr.GetNPCsByName("Evil Thief of the Shadowclan", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Evil Thief of the Shadowclan, creating default");
				evilThief = new GameMob();
				evilThief.Model = 55;
				evilThief.Name = "Evil Thief of the Shadowclan";
				evilThief.GuildName = "Part of DOL Quest Example";
				evilThief.Realm = (byte) eRealm.None; //Needs to be none, else we can't kill him ;-)
				evilThief.CurrentRegionID = 1;
				evilThief.Size = 50;
				evilThief.Level = 1;
				evilThief.X = 532571;
				evilThief.Y = 479055;
				evilThief.Z = 0;
				evilThief.Heading = 3570;

				//You don't have to store the creted mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				evilThief.SaveIntoDatabase();
				evilThief.AddToWorld();
			}
			else
				evilThief = npcs[0];

			sirQuaitsSword = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SirQuaitsSword");
			if (sirQuaitsSword == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("\"Help Sir Quait\" Quest could not find Sir Quait's Sword, creating it ...");
				sirQuaitsSword = new ItemTemplate();
				sirQuaitsSword.Id_nb = "SirQuaitsSword";
				sirQuaitsSword.Name = "Sir Quait's Sword";
				sirQuaitsSword.Level = 1;
				sirQuaitsSword.Model = 847;
				sirQuaitsSword.IsDropable = false;
				sirQuaitsSword.IsPickable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				GameServer.Database.AddNewObject(sirQuaitsSword);
			}

			/* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToSirQuait"
				* method. This means, the "TalkToSirQuait" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/
			GameEventMgr.AddHandler(sirQuait, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirQuait));
			GameEventMgr.AddHandler(sirQuait, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirQuait));

			/* Now we bring to SirQuait the possibility to give this quest to players */
			sirQuait.AddQuestToGive(typeof (HelpSirQuait));

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

			/* Now we remove to SirQuait the possibility to give this quest to players */
			sirQuait.RemoveQuestToGive(typeof (HelpSirQuait));
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
			if( sirQuait.CanGiveQuest(typeof (HelpSirQuait), player)  <= 0)
				return;

			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				//We get the player from the event arguments and check if he qualifies
				//for the quest!
				InteractEventArgs iargs = args as InteractEventArgs;

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
							player.Out.SendCustomDialog("Do you want to help Sir Quait?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if( sirQuait.CanGiveQuest(typeof (HelpSirQuait), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (HelpSirQuait)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!sirQuait.GiveQuest(typeof (HelpSirQuait), player, 1))
					return;

				player.Out.SendMessage("Thank you! Please bring the [sword] back to me!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
		}

		/// <summary>
		/// This method checks if a player is qualified for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (HelpSirQuait)) != null)
				return true;

			if (player.Level > 2 || player.CharacterClass.Name != "Fighter")
				return false;
			return true;
		}

		/// <summary>
		/// Retrieves how much time player can do the quest (default 1 time)
		/// </summary>
		public override int MaxQuestCount
		{
			get { return 3; }
		}

		/* Now we set the quest name.
		 * If we don't override the base method, then the quest
		 * will have the name "UNDEFINED QUEST NAME" and we don't
		 * want that, do we? ;-)
		 */
		public override string Name
		{
			get { return "Help Sir Quait to find his sword"; }
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
						return "[Step #1] Find the evil thief and get the magic sword from him!";
					case 2:
						return "[Step #2] Bring back Sir Quait's magic sword to receive your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (HelpSirQuait)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == "Evil Thief of the Shadowclan")
				{
					player.Out.SendMessage("You defeated the evil thief and quickly pick up Sir Quait's sword!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					InventoryItem item = new InventoryItem();
					item.CopyFrom(sirQuaitsSword);
					player.Inventory.AddItem(eInventorySlot.FirstBackpack, item);
					Step = 2;
					return;
				}
			}
			else if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == "Sir Quait" && gArgs.Item.Id_nb == "SirQuaitsSword")
				{
//					player.Inventory.FindItemByID("SirQuaitsSword"); // huh?
					if (!player.Inventory.RemoveItem(gArgs.Item))
						return;
					FinishQuest();
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			m_questPlayer.Out.SendMessage("Sir Quait thanks you for bringing back his holy sword!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
			//Give reward to player here ...
		}

	}
}
