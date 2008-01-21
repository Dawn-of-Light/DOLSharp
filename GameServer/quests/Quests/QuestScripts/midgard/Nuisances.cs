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
 * Directory: /scripts/quests/midgard/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=41211,50221 Vale of Mularn to speak with Dalikor 
 * 2) Go to loc=44585,56194 Vale of Mularn and /use the Magical Wooden Box on the Fallen Askefruer when they appear. 
 * 3) Take the Full Magical Wooden Box back to Dalikor for your reward.
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
 * GS Code
 */

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class Nuisances : BaseDalikorQuest
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
		protected const string questTitle = "Nuisances (Mid)";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC dalikor = null;
		private GameNPC askefruer = null;

		private static GameLocation askefruerLocation = new GameLocation("Fallen Askefruer", 100, 100, 44585, 56194, 4780, 294);
		private static IArea askefruerArea = null;

		private static ItemTemplate emptyMagicBox = null;
		private static ItemTemplate fullMagicBox = null;
		private static ItemTemplate recruitsShortSword = null;
		private static ItemTemplate recruitsStaff = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public Nuisances() : base()
		{
		}

		public Nuisances(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Nuisances(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Nuisances(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
				* the world who comes from the Albion realm. If we find a the players,
				* this means we don't have to create a new one.
				* 
				* NOTE: You can do anything you want in this method, you don't have
				* to search for NPC's ... you could create a custom item, place it
				* on the ground and if a player picks it up, he will get the quest!
				* Just examples, do anything you like and feel comfortable with :)
				*/

			#region defineNPCs

			dalikor = GetDalikor();

			#endregion

			#region defineItems 

			// item db check
			emptyMagicBox = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "empty_wodden_magic_box");
			if (emptyMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Empty Wodden Magic Box, creating it ...");
				emptyMagicBox = new ItemTemplate();
				emptyMagicBox.Name = "Empty Wodden Magic Box";

				emptyMagicBox.Weight = 5;
				emptyMagicBox.Model = 602;

				emptyMagicBox.Object_Type = (int) eObjectType.GenericItem;
				emptyMagicBox.Id_nb = "empty_wodden_magic_box";

				emptyMagicBox.IsPickable = true;
				emptyMagicBox.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(emptyMagicBox);
			}

			// item db check
			fullMagicBox = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "full_wodden_magic_box");
			if (fullMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Full Wodden Magic Box, creating it ...");
				fullMagicBox = new ItemTemplate();
				fullMagicBox.Name = "Full Wodden Magic Box";

				fullMagicBox.Weight = 3;
				fullMagicBox.Model = 602;

				fullMagicBox.Object_Type = (int) eObjectType.GenericItem;

				fullMagicBox.Id_nb = "full_wodden_magic_box";
				fullMagicBox.IsPickable = true;
				fullMagicBox.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fullMagicBox);
			}

			// item db check
			recruitsShortSword = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "recruits_short_sword_mid");
			if (recruitsShortSword == null)
			{
				recruitsShortSword = new ItemTemplate();
				recruitsShortSword.Name = "Recruit's Short Sword (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsShortSword.Name + ", creating it ...");
				recruitsShortSword.Level = 4;

				recruitsShortSword.Weight = 18;
				recruitsShortSword.Model = 3; // studded Boots

				recruitsShortSword.DPS_AF = 23; // Armour
				recruitsShortSword.SPD_ABS = 30; // Absorption

				recruitsShortSword.Type_Damage = (int) eDamageType.Slash;
				recruitsShortSword.Object_Type = (int) eObjectType.Sword;
				recruitsShortSword.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsShortSword.Id_nb = "recruits_short_sword_mid";
				recruitsShortSword.Gold = 0;
				recruitsShortSword.Silver = 2;
				recruitsShortSword.Copper = 0;
				recruitsShortSword.IsPickable = true;
				recruitsShortSword.IsDropable = true;
				recruitsShortSword.Color = 61;

				recruitsShortSword.Bonus = 1; // default bonus

				recruitsShortSword.Bonus1 = 3;
				recruitsShortSword.Bonus1Type = (int) eStat.STR;

				recruitsShortSword.Bonus2 = 1;
				recruitsShortSword.Bonus2Type = (int) eResist.Body;

				recruitsShortSword.Quality = 100;
				recruitsShortSword.Condition = 1000;
				recruitsShortSword.MaxCondition = 1000;
				recruitsShortSword.Durability = 1000;
				recruitsShortSword.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsShortSword);
			}

			// item db check
			recruitsStaff = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "recruits_staff");
			if (recruitsStaff == null)
			{
				recruitsStaff = new ItemTemplate();
				recruitsStaff.Name = "Recruit's Staff";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsStaff.Name + ", creating it ...");
				recruitsStaff.Level = 4;

				recruitsStaff.Weight = 45;
				recruitsStaff.Model = 442;

				recruitsStaff.DPS_AF = 24;
				recruitsStaff.SPD_ABS = 45;

				recruitsStaff.Type_Damage = (int) eDamageType.Slash;
				recruitsStaff.Object_Type = (int) eObjectType.Staff;
				recruitsStaff.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsStaff.Id_nb = "recruits_staff";
				recruitsStaff.Gold = 0;
				recruitsStaff.Silver = 2;
				recruitsStaff.Copper = 0;
				recruitsStaff.IsPickable = true;
				recruitsStaff.IsDropable = true;
				recruitsStaff.Color = 61;

				recruitsStaff.Bonus = 1; // default bonus

				recruitsStaff.Bonus1 = 3;
				recruitsStaff.Bonus1Type = (int) eStat.INT;

				recruitsStaff.Bonus2 = 1;
				recruitsStaff.Bonus2Type = (int) eResist.Crush;

				recruitsStaff.Quality = 100;
				recruitsStaff.Condition = 1000;
				recruitsStaff.MaxCondition = 1000;
				recruitsStaff.Durability = 1000;
				recruitsStaff.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsStaff);
			}

			#endregion

			askefruerArea = WorldMgr.GetRegion(askefruerLocation.RegionID).AddArea(new Area.Circle("Askefruer contamined Area", askefruerLocation.X, askefruerLocation.Y, 0, 1500));
			askefruerArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterAskefruerArea));

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			//We want to be notified whenever a player enters the world
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (Nuisances));

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
			if (dalikor == null)
				return;

			askefruerArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterAskefruerArea));
			WorldMgr.GetRegion(askefruerLocation.RegionID).RemoveArea(askefruerArea);

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));
		
			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (Nuisances));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				if (quest.askefruer != null && quest.askefruer.ObjectState == GameObject.eObjectState.Active)
				{
					quest.askefruer.Delete();
				}
			}
		}

		protected virtual void CreateAskefruer()
		{
			askefruer = new GameNPC();
			askefruer.Model = 678;
			askefruer.Name = "Fallen Askefruer";
			askefruer.GuildName = "Part of " + questTitle + " Quest";
			askefruer.Realm = eRealm.None;
			askefruer.CurrentRegionID = askefruerLocation.RegionID;
			askefruer.Size = 50;
			askefruer.Level = 4;
			askefruer.X = askefruerLocation.X + Util.Random(-150, 150);
			askefruer.Y = askefruerLocation.Y + Util.Random(-150, 150);
			askefruer.Z = askefruerLocation.Z;
			askefruer.Heading = askefruerLocation.Heading;

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 20;
			brain.AggroRange = 200;
			askefruer.SetOwnBrain(brain);

			askefruer.AddToWorld();
		}

		protected virtual int DeleteAskefruer(RegionTimer callingTimer)
		{
			askefruer.Delete();
			askefruer = null;
			return 0;
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...            

			Nuisances quest = (Nuisances) player.IsDoingQuest(typeof (Nuisances));
			if (quest == null)
				return;

			if (quest.Step == 1 && quest.askefruer != null)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == emptyMagicBox.Id_nb)
				{
					if (WorldMgr.GetDistance(player, quest.askefruer) < 500)
					{
						foreach (GamePlayer visPlayer in quest.askefruer.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.askefruer, 1, 20);
						}

						SendSystemMessage(player, "You catch " + quest.askefruer.GetName(0, false) + " in your magical wodden box!");
						new RegionTimer(player, new RegionTimerCallback(quest.DeleteAskefruer), 2000);

						ReplaceItem(player, emptyMagicBox, fullMagicBox);

						quest.Step = 2;
					}
					else
					{
						SendSystemMessage(player, "There is nothing within the reach of the magic box that can be cought.");
					}
				}
			}
		}

		protected static void PlayerEnterAskefruerArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			if (quest != null && quest.askefruer == null && quest.Step == 1)
			{
				// player near grove
				SendSystemMessage(player, "It's Fallen Askefruer! Quickly now, /use your box to capture the Askefruer! To USE an item, right click on the item and type /use.");
				quest.CreateAskefruer();

				foreach (GamePlayer visPlayer in quest.askefruer.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(quest.askefruer, 1, 20);
				}
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToDalikor(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Nuisances), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			dalikor.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Recruit Eeinken. I'm afraid we have a [serious problem] on our hands.");
					return;
				}
				else
				{
					if (quest.Step == 2)
					{
						dalikor.SayTo(player, "Welcome back recruit. Did you find out what was making all that racket?");
					}
					else if (quest.Step == 3)
					{
						dalikor.SayTo(player, "Hrm...Fallen Askefruer. This is what has been causing us our problems? Interesting. I want to thank you recruit for your hard work in helping us solve this problem. A [reward] is in store for you I think.");
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
						case "serious problem":
							dalikor.SayTo(player, "There has been this noise that has been keeping the residents of Mularn up at night. I haven't been able to locate the source of the noise, neither have any of the guards. I was hoping you could try to [find] the noise.");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "find":
							player.Out.SendQuestSubscribeCommand(dalikor, QuestMgr.GetIDForQuestType(typeof(Nuisances)), "Will you help out Mularn and discover who or what is making this noise?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "reward":
							dalikor.SayTo(player, "Yes, I think this will do quite nicely. Here you are Eeinken. Use it well, and I'm sure it will last you your first few seasons anyhow. Be sure to come and speak with me when you are ready for more adventure.");
							if (quest.Step == 3)
							{
								quest.FinishQuest();
								dalikor.SayTo(player, "Don't go far, I have need of your services again Eeinken.");
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Nuisances)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Nuisances)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (!CheckPartAccessible(player, typeof (Nuisances)))
				return false;

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
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(dalikor.CanGiveQuest(typeof (Nuisances), player)  <= 0)
				return;

			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (Nuisances), player, 1))
					return;

				dalikor.SayTo(player, "Excellent, recruit! I believe the noise is coming from the south-southeast of this tower, near the base of the hills. You'll find that there are a lot of huldu near that area, but it is not them. Take this box. When you have discovered what or who is making this noise, USE the box to capture them, then bring it back to me. Be safe Eeinken.");
				// give necklace                
				GiveItem(dalikor, player, emptyMagicBox);

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
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
						return "[Step #1] Find the area where the sound is the loudest. USE the box and see if you can capture anything. Dalikor believes the area is to the south-southeast, near the base of the hills.";
					case 2:
						return "[Step #2] Take the Full Magical Wooden Box back to Dalikor at the guard tower near Mularn. Be sure to hand him the Full Magical Wooden Box.";
					case 3:
						return "[Step #3] Wait for Dalikor to reward you. If he stops speaking with you, simply ask him if there is a [reward] for your efforts.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Nuisances)) == null)
				return;

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == fullMagicBox.Id_nb)
				{
					RemoveItem(dalikor, m_questPlayer, fullMagicBox);

					dalikor.TurnTo(m_questPlayer);
					dalikor.SayTo(m_questPlayer, "Hm...It's quite heavy. Let me take a peek inside.");
					SendEmoteMessage(m_questPlayer, "Dalikor opens the top of the wooden box carefully. Once he spies the creatures inside, he closes the lid quickly.");

					m_questPlayer.Out.SendEmoteAnimation(dalikor, eEmote.Yes);
					Step = 3;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, emptyMagicBox, false);
			RemoveItem(m_questPlayer, fullMagicBox, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsShortSword))
				GiveItem(dalikor, m_questPlayer, recruitsShortSword);
			else
				GiveItem(dalikor, m_questPlayer, recruitsStaff);

			m_questPlayer.GainExperience(100, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
