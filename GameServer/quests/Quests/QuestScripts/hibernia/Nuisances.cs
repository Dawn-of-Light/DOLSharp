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
 * Directory: /scripts/quests/hibernia/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=26955,7789 Lough Derg to speak with Addrir. 
 * 2) Go to Go to loc=27416,4129 Lough Derg and /use the Magical Wooden Box on the Sluagh Footsoldiers when they appear. 
 * 3) Take the Full Magical Wooden Box back to Addrir for your reward.
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

namespace DOL.GS.Quests.Hibernia
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class Nuisances : BaseAddrirQuest
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
		protected const string questTitle = "Nuisances (Hib)";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC addrir = null;
		private GameNPC sluagh = null;

		private static GameLocation sluaghLocation = new GameLocation("sluagh Location", 200, 200, 27416, 4129, 5221, 310);
		private static IArea sluaghArea = null;

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

			addrir = GetAddrir();

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
			recruitsShortSword = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "recruits_short_sword_hib");
			if (recruitsShortSword == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Short Sword (Hib), creating it ...");
				recruitsShortSword = new ItemTemplate();
				recruitsShortSword.Name = "Recruit's Short Sword (Hib)";
				recruitsShortSword.Level = 4;

				recruitsShortSword.Weight = 18;
				recruitsShortSword.Model = 3; // studded Boots

				recruitsShortSword.DPS_AF = 24; // Armour
				recruitsShortSword.SPD_ABS = 30; // Absorption

				recruitsShortSword.Type_Damage = (int) eDamageType.Slash;
				recruitsShortSword.Object_Type = (int) eObjectType.Blades;
				recruitsShortSword.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsShortSword.Id_nb = "recruits_short_sword_hib";
				recruitsShortSword.Gold = 0;
				recruitsShortSword.Silver = 2;
				recruitsShortSword.Copper = 0;
				recruitsShortSword.IsPickable = true;
				recruitsShortSword.IsDropable = true;
				recruitsShortSword.Color = 46; // green metal

				recruitsShortSword.Bonus = 1; // default bonus

				recruitsShortSword.Bonus1 = 3;
				recruitsShortSword.Bonus1Type = (int) eStat.STR;

				recruitsShortSword.Bonus2 = 1;
				recruitsShortSword.Bonus2Type = (int) eStat.QUI;

				recruitsShortSword.Bonus3 = 1;
				recruitsShortSword.Bonus3Type = (int) eResist.Body;

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

			sluaghArea = WorldMgr.GetRegion(sluaghLocation.RegionID).AddArea(new Area.Circle("Sluagh contamined Area", sluaghLocation.X, sluaghLocation.Y, 0, 1500));
			sluaghArea.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterSluaghArea));

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

			GameEventMgr.AddHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.AddHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			/* Now we bring to addrir the possibility to give this quest to players */
			addrir.AddQuestToGive(typeof (Nuisances));

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
			if (addrir == null)
				return;

			sluaghArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterSluaghArea));
			WorldMgr.GetRegion(sluaghLocation.RegionID).RemoveArea(sluaghArea);

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			/* Now we remove to addrir the possibility to give this quest to players */
			addrir.RemoveQuestToGive(typeof (Nuisances));
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

				if (quest.sluagh != null && quest.sluagh.ObjectState == GameObject.eObjectState.Active)
				{
					quest.sluagh.Delete();
				}
			}
		}

		protected virtual void CreateSluagh()
		{
			sluagh = new GameNPC();
			sluagh.Model = 603;
			sluagh.Name = "Sluagh Footsoldier";
			sluagh.GuildName = "Part of " + questTitle + " Quest";
			sluagh.Realm = eRealm.None;
			sluagh.CurrentRegionID = 200;
			sluagh.Size = 50;
			sluagh.Level = 4;
			sluagh.X = sluaghLocation.X + Util.Random(-150, 150);
			sluagh.Y = sluaghLocation.Y + Util.Random(-150, 150);
			sluagh.Z = sluaghLocation.Z;
			sluagh.Heading = sluaghLocation.Heading;

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 20;
			brain.AggroRange = 200;
			sluagh.SetOwnBrain(brain);

			sluagh.AddToWorld();
		}

		protected virtual int DeleteSluagh(RegionTimer callingTimer)
		{
			sluagh.Delete();
			sluagh = null;
			return 0;
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			Nuisances quest = (Nuisances) player.IsDoingQuest(typeof (Nuisances));
			if (quest == null)
				return;

			if (quest.Step == 1 && quest.sluagh != null)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == emptyMagicBox.Id_nb)
				{
					if (WorldMgr.GetDistance(player, quest.sluagh) < 500)
					{
						foreach (GamePlayer visPlayer in quest.sluagh.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.sluagh, 1, 20);
						}

						SendSystemMessage(player, "You catch the sluagh footsoldier in your magical wodden box!");
						new RegionTimer(player, new RegionTimerCallback(quest.DeleteSluagh), 2000);

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

		protected static void PlayerEnterSluaghArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			if (quest != null && quest.sluagh == null && quest.Step == 1)
			{
				// player near grove            
				SendSystemMessage(player, "Sluaghs! Quick! USE your Magical Wooden Box to capture the sluagh footsoldier! To USE an item, right click on the item and type /use.");
				quest.CreateSluagh();

				foreach (GamePlayer visPlayer in quest.sluagh.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(quest.sluagh, 1, 20);
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

		protected static void TalkToAddrir(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(addrir.CanGiveQuest(typeof (Nuisances), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Nuisances quest = player.IsDoingQuest(typeof (Nuisances)) as Nuisances;

			addrir.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					addrir.SayTo(player, "Ah, my friend. I'm afraid there is a [new problem] that we must deal with.");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							addrir.SayTo(player, "Excellent recruit! Now here, take this magical box. I have a feeling there is something out there, and I would like for you to catch it, as proof to Fagan that something needs to be done. When you find the spot that is the noisiest, USE the box and capture whatever it is. Good luck Lirone. Return to me as quickly as you can.");
							break;
						case 3:
							addrir.SayTo(player, "Sluagh...These beasts are evil, evil creatures. I can't begin to imagine why they are here. I will have to bring this to Fagan's attention immediately. Thank you Lirone, for showing this to me. Please take this as a show of my appreciation for your bravery and dedication to not only Mag Mell, but to Hibernia. I shall go present this to Fagan straight away. Be safe until my return Lirone.");
							quest.FinishQuest();
							break;
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
						case "new problem":
							addrir.SayTo(player, "Aye Lirone. There is this hideously loud noise that is coming from somewhere to the North of Mag Mell. It is driving the citizens mad! Fagan has offered a reward to the first person who can [quell] the noise.");
							break;
						case "quell":
							addrir.SayTo(player, "Will you do this for us recruit? Will you find out the cause of this noise and [stop] it?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "stop":
							player.Out.SendQuestSubscribeCommand(addrir, QuestMgr.GetIDForQuestType(typeof(Nuisances)), "Will you help out Mag Mell and discover who or what is making this noise?");
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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(addrir.CanGiveQuest(typeof (Nuisances), player)  <= 0)
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
				if (!addrir.GiveQuest(typeof (Nuisances), player, 1))
					return;

				addrir.SayTo(player, "Excellent recruit! Now here, take this magical box. I have a feeling there is something out there, and I would like for you to catch it, as proof to Fagan that something needs to be done. When you find the spot that is the noisiest, USE the box and capture whatever it is. Good luck Lirone. Return to me as quickly as you can.");
				// give necklace                
				GiveItem(addrir, player, emptyMagicBox);

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
						return "[Step #1] Travel North from Addrir into the small wooded area near Mag Mell. Once there, USE your box to capture whatever is making the noise. To USE an item, right click on the item and type /use.";
					case 2:
						return "[Step #2] Take the Full Magical Wooden Box back to Addrir in Mag Mell.";
					case 3:
						return "[Step #3] Wait for Addrir to reward you. If he stops speaking with you at any time, ask if there is something he can give you for your [efforts].";
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
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Id_nb == fullMagicBox.Id_nb)
				{
					RemoveItem(addrir, m_questPlayer, fullMagicBox);

					addrir.TurnTo(m_questPlayer);
					addrir.SayTo(m_questPlayer, "Ah, it is quite heavy, let me take a peek.");
					SendEmoteMessage(m_questPlayer, "Addrir takes the box from you and carefully opens the lid. When he sees what is inside, he closes the lid quickly.");

					m_questPlayer.Out.SendEmoteAnimation(addrir, eEmote.Yes);
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
				GiveItem(addrir, m_questPlayer, recruitsShortSword);
			else
				GiveItem(addrir, m_questPlayer, recruitsStaff);

			m_questPlayer.GainExperience(100, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
