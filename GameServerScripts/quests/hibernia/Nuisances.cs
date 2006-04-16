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
using DOL.GS.Database;
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

namespace DOL.GS.Quests.Hibernia
{

	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class NuisancesHibDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(NuisancesHib); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
				public override int MinLevel
				{
					get { return 2; }
				}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 2; }
		}

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			if (!BaseAddirQuest.CheckPartAccessible(player, typeof(NuisancesHib)))
				return false;
			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(NuisancesHib), ExtendsType = typeof(AbstractQuest))] 
	public class NuisancesHib : BaseAddirQuest
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

		private static GameNPC addrir = null;
		private GameNPC sluagh = null;

		private static GameLocation sluaghLocation = new GameLocation("sluagh Location", 200, 200, 27416, 4129, 5221, 310);
		private static Circle sluaghArea = null;

		private static GenericItemTemplate emptyMagicBox = null;
		private static GenericItemTemplate fullMagicBox = null;
		private static SwordTemplate recruitsShortSword = null;
		private static StaffTemplate recruitsStaff = null;

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
			emptyMagicBox = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "empty_wodden_magic_box");
			if (emptyMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Empty Wodden Magic Box, creating it ...");
				emptyMagicBox = new GenericItemTemplate();
				emptyMagicBox.Name = "Empty Wodden Magic Box";

				emptyMagicBox.Weight = 5;
				emptyMagicBox.Model = 602;

				emptyMagicBox.ItemTemplateID = "empty_wodden_magic_box";

				emptyMagicBox.IsDropable = false;
				emptyMagicBox.IsSaleable = false;
				emptyMagicBox.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(emptyMagicBox);
			}

			// item db check
			fullMagicBox = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "full_wodden_magic_box");
			if (fullMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Full Wodden Magic Box, creating it ...");
				fullMagicBox = new GenericItemTemplate();
				fullMagicBox.Name = "Full Wodden Magic Box";

				fullMagicBox.Weight = 3;
				fullMagicBox.Model = 602;

				fullMagicBox.ItemTemplateID = "full_wodden_magic_box";

				fullMagicBox.IsDropable = false;
				fullMagicBox.IsSaleable = false;
				fullMagicBox.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fullMagicBox);
			}

			// item db check
			recruitsShortSword = (SwordTemplate)GameServer.Database.FindObjectByKey(typeof(SwordTemplate), "recruits_short_sword_hib");
			if (recruitsShortSword == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Short Sword (Hib), creating it ...");
				recruitsShortSword = new SwordTemplate();
				recruitsShortSword.Name = "Recruit's Short Sword (Hib)";
				recruitsShortSword.Level = 4;

				recruitsShortSword.Weight = 18;
				recruitsShortSword.Model = 3; // studded Boots

				recruitsShortSword.DamagePerSecond = 24;
				recruitsShortSword.Speed = 3000;

				recruitsShortSword.ItemTemplateID = "recruits_short_sword_hib";
				recruitsShortSword.Value = 200;
				recruitsShortSword.Color = 46; // green metal

				recruitsShortSword.IsDropable = true;
				recruitsShortSword.IsSaleable = true;
				recruitsShortSword.IsTradable = true;

				recruitsShortSword.Bonus = 1; // default bonus

				recruitsShortSword.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 3));
				recruitsShortSword.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 1));
				recruitsShortSword.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsShortSword);
			}

			// item db check
			recruitsStaff = (StaffTemplate)GameServer.Database.FindObjectByKey(typeof(StaffTemplate), "recruits_staff");
			if (recruitsStaff == null)
			{
				recruitsStaff = new StaffTemplate();
				recruitsStaff.Name = "Recruit's Staff";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsStaff.Name + ", creating it ...");
				recruitsStaff.Level = 4;

				recruitsStaff.Weight = 45;
				recruitsStaff.Model = 442;

				recruitsStaff.DamagePerSecond = 24;
				recruitsStaff.Speed = 4500;

				recruitsStaff.ItemTemplateID = "recruits_staff";
				recruitsStaff.Value = 200;

				recruitsStaff.IsDropable = true;
				recruitsStaff.IsSaleable = true;
				recruitsStaff.IsTradable = true;
				recruitsStaff.Color = 45; // blue metal

				recruitsStaff.Bonus = 1; // default bonus

				recruitsStaff.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 3));
				recruitsStaff.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsStaff);
			}

			#endregion


			sluaghArea = new Circle();
			sluaghArea.Description = "Sluagh contamined Area";
			sluaghArea.X = sluaghLocation.Position.X;
			sluaghArea.Y = sluaghLocation.Position.Y;
			sluaghArea.Radius = 1500;
			sluaghArea.RegionID = sluaghLocation.Region.RegionID;
			AreaMgr.RegisterArea(sluaghArea);

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			//We want to be notified whenever a player enters the world
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.AddHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterSluaghArea));

			/* Now we bring to addrir the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(addrir, typeof(NuisancesHibDescriptor));

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

			AreaMgr.UnregisterArea(sluaghArea);

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterSluaghArea));

			/* Now we remove to addrir the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(addrir, typeof(NuisancesHibDescriptor));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			NuisancesHib quest = player.IsDoingQuest(typeof (NuisancesHib)) as NuisancesHib;
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
			sluagh = new GameMob();
			sluagh.Model = 603;
			sluagh.Name = "Sluagh Footsoldier";
			sluagh.GuildName = "Part of " + questTitle + " Quest";
			sluagh.Realm = (byte) eRealm.None;
			sluagh.RegionId = 200;
			sluagh.Size = 50;
			sluagh.Level = 4;
			Point pos = sluaghLocation.Position;
			pos.X += Util.Random(-150, 150);
			pos.Y += Util.Random(-150, 150);
			sluagh.Position = pos;
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

			NuisancesHib quest = (NuisancesHib) player.IsDoingQuest(typeof (NuisancesHib));
			if (quest == null)
				return;

			if (quest.Step == 1 && quest.sluagh != null)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Name == emptyMagicBox.Name)
				{
					if (player.Position.CheckSquareDistance(quest.sluagh.Position, 500*500))
					{
						foreach (GamePlayer visPlayer in quest.sluagh.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.sluagh, 1, 20);
						}

						SendSystemMessage(player, "You catch the sluagh footsoldier in your magical wodden box!");
						new RegionTimer(player, new RegionTimerCallback(quest.DeleteSluagh), 2000);
						player.Inventory.RemoveItem(item);
						player.ReceiveItem(player, fullMagicBox.CreateInstance());

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
			if (aargs.Area != sluaghArea) return;
			GamePlayer player = aargs.GameObject as GamePlayer;
			NuisancesHib quest = player.IsDoingQuest(typeof (NuisancesHib)) as NuisancesHib;

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

			NuisancesHib quest = player.IsDoingQuest(typeof (NuisancesHib)) as NuisancesHib;
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

			if (QuestMgr.CanGiveQuest(typeof(NuisancesHib), player, addrir) <= 0)
				return;

			//We also check if the player is already doing the quest
			NuisancesHib quest = player.IsDoingQuest(typeof (NuisancesHib)) as NuisancesHib;

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
							player.Out.SendCustomDialog("Will you help out Mag Mell and discover who or what is making this noise?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if (QuestMgr.CanGiveQuest(typeof(NuisancesHib), player, addrir) <= 0)
				return;

			NuisancesHib quest = player.IsDoingQuest(typeof (NuisancesHib)) as NuisancesHib;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(NuisancesHib), player, addrir))
					return;

				addrir.SayTo(player, "Excellent recruit! Now here, take this magical box. I have a feeling there is something out there, and I would like for you to catch it, as proof to Fagan that something needs to be done. When you find the spot that is the noisiest, USE the box and capture whatever it is. Good luck Lirone. Return to me as quickly as you can.");
				// give necklace        
				player.ReceiveItem(addrir, emptyMagicBox.CreateInstance());

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
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (NuisancesHib)) == null)
				return;

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Name == fullMagicBox.Name)
				{
					RemoveItemFromPlayer(addrir, fullMagicBox);

					addrir.TurnTo(m_questPlayer);
					addrir.SayTo(m_questPlayer, "Ah, it is quite heavy, let me take a peek.");
					SendEmoteMessage(m_questPlayer, "Addrir takes the box from you and carefully opens the lid. When he sees what is inside, he closes the lid quickly.");
					addrir.Emote(eEmote.Yes);
					Step = 3;
					return;
				}
			}

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsShortSword.CreateInstance() as EquipableItem))
				GiveItemToPlayer(addrir, recruitsShortSword.CreateInstance());
			else
				GiveItemToPlayer(addrir, recruitsStaff.CreateInstance());

			m_questPlayer.GainExperience(100, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
