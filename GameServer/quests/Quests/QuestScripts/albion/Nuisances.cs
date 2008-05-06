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
 *  1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 *  2) Go to loc=12336,22623 Camelot Hills and /use the Magical Wooden Box on the Ire Fairies when they appear. 
 *  3) Take the Full Magical Wooden Box back to Master Frederick for your reward.
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

	public class Nuisances : BaseFrederickQuest
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
		protected const string questTitle = "Nuisances";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC masterFrederick = null;

		protected GameNPC ireFairy = null;

		private static GameLocation fairyLocation = new GameLocation("Ire Fairy", 1, 561200, 505951, 2405);

		private static IArea fairyArea = null;

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

			#endregion

			#region defineItems

			// item db check
			emptyMagicBox = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "empty_wodden_magic_box");
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
			fullMagicBox = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "full_wodden_magic_box");
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
			recruitsShortSword = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "recruits_short_sword");
			if (recruitsShortSword == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Short Sword, creating it ...");
				recruitsShortSword = new ItemTemplate();
				recruitsShortSword.Name = "Recruit's Short Sword";
				recruitsShortSword.Level = 4;

				recruitsShortSword.Weight = 18;
				recruitsShortSword.Model = 3; // studded Boots

				recruitsShortSword.DPS_AF = 23; // Armour
				recruitsShortSword.SPD_ABS = 30; // Absorption

				recruitsShortSword.Type_Damage = (int) eDamageType.Slash;
				recruitsShortSword.Object_Type = (int) eObjectType.SlashingWeapon;
				recruitsShortSword.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsShortSword.Id_nb = "recruits_short_sword";
				recruitsShortSword.Gold = 0;
				recruitsShortSword.Silver = 2;
				recruitsShortSword.Copper = 0;
				recruitsShortSword.IsPickable = true;
				recruitsShortSword.IsDropable = true;
				recruitsShortSword.Color = 45; // blue metal

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
			recruitsStaff = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "recruits_staff");
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
				recruitsStaff.Color = 45; // blue metal

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

			fairyArea = WorldMgr.GetRegion(fairyLocation.RegionID).AddArea(new Area.Circle("Fairy contamined Area", fairyLocation.X, fairyLocation.Y, 0, 1500));
			fairyArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterFairyArea));

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			//We want to be notified whenever a player enters the world
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (Nuisances));

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

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			fairyArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterFairyArea));
			WorldMgr.GetRegion(fairyLocation.RegionID).RemoveArea(fairyArea);

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (Nuisances));
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

				if (quest.ireFairy != null && quest.ireFairy.ObjectState == GameObject.eObjectState.Active)
				{
					quest.ireFairy.Delete();
				}

			}
		}

		protected virtual void CreateFairy()
		{
			ireFairy = new GameNPC();
			ireFairy.Model = 603;
			ireFairy.Name = "Ire Fairy";
			ireFairy.GuildName = "Part of " + questTitle + " Quest";
			ireFairy.Realm = eRealm.None;
			ireFairy.CurrentRegionID = 1;
			ireFairy.Size = 50;
			ireFairy.Level = 4;
			ireFairy.X = GameLocation.ConvertLocalXToGlobalX(12336, 0) + Util.Random(-150, 150);
			ireFairy.Y = GameLocation.ConvertLocalYToGlobalY(22623, 0) + Util.Random(-150, 150);
			ireFairy.Z = 2405;
			ireFairy.Heading = 226;

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 20;
			brain.AggroRange = 200;
			ireFairy.SetOwnBrain(brain);

			ireFairy.AddToWorld();
		}

		protected virtual int DeleteFairy(RegionTimer callingTimer)
		{
			ireFairy.Delete();
			ireFairy = null;
			return 0;
		}

		protected static void PlayerEnterFairyArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			if (quest != null && quest.ireFairy == null && quest.Step == 1)
			{
				// player near grove            
				SendSystemMessage(player, "Ire Fairies! Quick! USE your Magical Wooden Box to capture the fairies! To USE an item, right click on the item and type /use.");
				quest.CreateFairy();

				foreach (GamePlayer visPlayer in quest.ireFairy.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(quest.ireFairy, 1, 20);
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			Nuisances quest = (Nuisances) player.IsDoingQuest(typeof (Nuisances));
			if (quest == null)
				return;

			if (quest.Step == 1 && quest.ireFairy != null)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == emptyMagicBox.Id_nb)
				{
					if (WorldMgr.GetDistance(player, quest.ireFairy) < 500)
					{
						foreach (GamePlayer visPlayer in quest.ireFairy.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.ireFairy, 1, 20);
						}

						SendSystemMessage(player, "You catch ire fairy in your magical wodden box!");
						new RegionTimer(player, new RegionTimerCallback(quest.DeleteFairy), 2000);

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

		protected static void TalkToMasterFrederick(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Nuisances), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "My young recruit, I fear we have a growing problem on our hands. For the past several nights, citizens in Cotswold have been complaining of a constant ringing noise. It has started to keep them up at [night].");
					return;
				}
				else
				{
					if (quest.Step == 2)
					{
						masterFrederick.SayTo(player, "Vinde, you've returned, and none the worse for wear. Tell me, what did you find?");
					}
					else if (quest.Step == 3)
					{
						masterFrederick.SayTo(player, "Ire fairies! They're the worst! Well, now we know who has been causing these problems. Vinde, you've done good work here today. It is time for a reward for your [efforts].");

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
						case "night":
							masterFrederick.SayTo(player, "It has even begun to affect the wildlife in this area. The guards can not commit any troops to finding out the cause of this ringing sound, so the responsibility falls to you Vinde. Will you help [Cotswold]?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "Cotswold":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(Nuisances)), "Will you help out Cotswold and discover who or what is making this noise?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "efforts":
							masterFrederick.SayTo(player, "A Fighter is nothing unless he has a good weapon by his or her side. For you, a new sword is in order. Use it well Vinde. For now, I must think about what to do with these Ire Fairies, and figure out why they are here.");
							if (quest.Step == 3)
							{
								quest.FinishQuest();
								masterFrederick.SayTo(player, "Don't go far, I have need of your services again Vinde.");
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
			if(masterFrederick.CanGiveQuest(typeof (Nuisances), player)  <= 0)
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
				if (!masterFrederick.GiveQuest(typeof (Nuisances), player, 1))
					return;

				masterFrederick.SayTo(player, "This magical box will help you capture whatever is making that noise. The reports indicate that the noise is the loudest to the west-northwest, near the banks of the river. Find the source of the noise Vinde. Take this box with you. Some of the other trainers seem to think it is magical in nature. I'm not so sure. USE the box in the area that is the loudest, or where you encounter trouble. See if you can capture anything.");
				// give necklace                
				GiveItem(masterFrederick, player, emptyMagicBox);

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
						return "[Step #1] Find the area where the sound is the loudest. USE the box and see if you can capture anything. Master Frederick believes the area is to the west-northwest, in some trees near the river.";
					case 2:
						return "[Step #2] Take the Full Magical Wooden Box back to Master Frederick in Cotswold. Be sure to hand him the Full Magical Wooden Box.";
					case 3:
						return "[Step #3] Wait for Master Frederick to reward you. If he stops speaking with you at any time, ask if there is something he can give you for your [efforts].";
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
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == fullMagicBox.Id_nb)
				{
					RemoveItem(masterFrederick, m_questPlayer, fullMagicBox);

					masterFrederick.TurnTo(m_questPlayer);
					masterFrederick.SayTo(m_questPlayer, "Ah, it is quite heavy, let me take a peek.");
					SendEmoteMessage(m_questPlayer, "Master Frederick opens the box carefully. When he sees the contents, he quickly closes it and turns his attention back to you.");

					m_questPlayer.Out.SendEmoteAnimation(masterFrederick, eEmote.Yes);
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
				GiveItem(masterFrederick, m_questPlayer, recruitsShortSword);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsStaff);

			m_questPlayer.GainExperience(100, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
