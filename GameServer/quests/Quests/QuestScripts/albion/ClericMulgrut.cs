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
 * 1) Travel to loc=22723,48005 Camelot Hills (Prydwen Keep) to speak with Hugh Gallen
 * 2) Go to loc=17500,45153 Camelot Hills and kill some punny skeleton until Mulgrut pop
 * 2) Kill Mulgrut to have your reward
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database2;
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

	public class ClericMulgrut : BaseQuest
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
		protected const string questTitle = "Cleric Mulgrut";
		protected const int minimumLevel = 5;
		protected const int maximumLevel = 10;

		private static GameNPC hughGallen = null;
		private static GameNPC mulgrutMaggot = null;
		
		private static ItemTemplate beltOfAnimation = null;
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public ClericMulgrut() : base()
		{
		}

		public ClericMulgrut(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public ClericMulgrut(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public ClericMulgrut(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Hugh Gallen", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				hughGallen = new GameNPC();
				hughGallen.Model = 40;
				hughGallen.Name = "Hugh Gallen";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hughGallen.Name + ", creating him ...");
				hughGallen.GuildName = "Part of " + questTitle + " Quest";
				hughGallen.Realm = eRealm.Albion;
				hughGallen.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
				hughGallen.Inventory = template.CloseTemplate();
				hughGallen.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				hughGallen.Size = 49;
				hughGallen.Level = 38;
				hughGallen.X = 574640;
				hughGallen.Y = 531109;
				hughGallen.Z = 2896;
				hughGallen.Heading = 2275;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					hughGallen.SaveIntoDatabase();

				hughGallen.AddToWorld();
			}
			else
				hughGallen = npcs[0];

			#endregion

			#region defineItems

			// item db check
			beltOfAnimation = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "belt_of_animation");
			if (beltOfAnimation == null)
			{
				beltOfAnimation = new ItemTemplate();
				beltOfAnimation.Name = "Belt of Animation";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+beltOfAnimation.Name+", creating it ...");
				
				beltOfAnimation.Level = 5;
				beltOfAnimation.Weight = 3;
				beltOfAnimation.Model = 597;
				
				beltOfAnimation.Object_Type = (int) eObjectType.Magical;
				beltOfAnimation.Item_Type = (int) eEquipmentItems.WAIST;
				beltOfAnimation.Id_nb = "belt_of_animation";
				beltOfAnimation.Gold = 0;
				beltOfAnimation.Silver = 0;
				beltOfAnimation.Copper = 0;
				beltOfAnimation.IsPickable = true;
				beltOfAnimation.IsDropable = false; // can't be sold to merchand

				beltOfAnimation.Bonus1 = 6;
				beltOfAnimation.Bonus1Type = (int)eProperty.MaxHealth;
				
				beltOfAnimation.Quality = 100;
				beltOfAnimation.Condition = 1000;
				beltOfAnimation.MaxCondition = 1000;
				beltOfAnimation.Durability = 1000;
				beltOfAnimation.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(beltOfAnimation);
			}

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
			
			GameEventMgr.AddHandler(hughGallen, GameLivingEvent.Interact, new DOLEventHandler(TalkToHughGallen));
			GameEventMgr.AddHandler(hughGallen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHughGallen));

			/* Now we bring to Yetta Fletcher the possibility to give this quest to players */
			hughGallen.AddQuestToGive(typeof (ClericMulgrut));

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
			if (hughGallen == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(hughGallen, GameLivingEvent.Interact, new DOLEventHandler(TalkToHughGallen));
			GameEventMgr.RemoveHandler(hughGallen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHughGallen));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
			hughGallen.RemoveQuestToGive(typeof (ClericMulgrut));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToHughGallen(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(hughGallen.CanGiveQuest(typeof (ClericMulgrut), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ClericMulgrut quest = player.IsDoingQuest(typeof (ClericMulgrut)) as ClericMulgrut;

			hughGallen.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					hughGallen.SayTo(player, "I have a [bit of information] you might be interested in should you wish to hear it.");
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
						case "bit of information":
							hughGallen.SayTo(player, "Listen close! Lord Prydwen once had a faithful cleric known as Mulgrut. His service to our realm was unparalleled, yet, during Arthur's siege on Lancelot, Mulgrut's son Durren [was slain].");
							break;

						case "was slain":
							hughGallen.SayTo(player, "Yes! Mulgrut [never recovered] from that. He turned his eyes from God and never looked back.");
							break;

						case "never recovered":
							hughGallen.SayTo(player, "Aye! Even in death his soul never rests. If you are interested, I can tell you how to [make a profit] from this!");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "make a profit":
							player.Out.SendQuestSubscribeCommand(hughGallen, QuestMgr.GetIDForQuestType(typeof(ClericMulgrut)), "Do you accept the Cleric Mulgrut quest? \n[Levels 5-10]");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
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
			if (player.IsDoingQuest(typeof (ClericMulgrut)) != null)
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
			ClericMulgrut quest = player.IsDoingQuest(typeof (ClericMulgrut)) as ClericMulgrut;

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

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ClericMulgrut)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(hughGallen.CanGiveQuest(typeof (ClericMulgrut), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (ClericMulgrut)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!hughGallen.GiveQuest(typeof (ClericMulgrut), player, 1))
					return;

				SendReply(player, "It is said that upon his death, he carried into the after life an item of great worth. Some say he still walks the cemetery not far from here!");
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
						return "[Step #1] Locate and slay Mulgrut for his magical item.  He can usually be found either at the graveyard near Prydwen Keep or wandering about Camelot Hills.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (ClericMulgrut)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if(Step == 1)
				{
					if (gArgs.Target.Name == "puny skeleton")
					{
						if (Util.Chance(25))
						{
							if(mulgrutMaggot == null)
							{
								mulgrutMaggot = new GameNPC();
								mulgrutMaggot.Model = 467;
								mulgrutMaggot.Name = "Mulgrut Maggot";
								mulgrutMaggot.Realm = eRealm.None;
								mulgrutMaggot.CurrentRegionID = 1;

								mulgrutMaggot.Size = 60;
								mulgrutMaggot.Level = 5;
								mulgrutMaggot.X = 565941;
								mulgrutMaggot.Y = 528121;
								mulgrutMaggot.Z = 2152;
								mulgrutMaggot.Heading = 2278;

								StandardMobBrain brain = new StandardMobBrain();  // set a brain witch find a lot mob friend to attack the player
								mulgrutMaggot.SetOwnBrain(brain);

								mulgrutMaggot.RespawnInterval = 0; // don't respawn when killed

								mulgrutMaggot.AddToWorld();
							}
						}
					}
					else if (gArgs.Target.Name == "Mulgrut Maggot")
					{
						GiveItem(gArgs.Target, player, beltOfAnimation);
						if(mulgrutMaggot != null) { mulgrutMaggot = null; }
						FinishQuest();
					}
				}
			}
		}
	}
}