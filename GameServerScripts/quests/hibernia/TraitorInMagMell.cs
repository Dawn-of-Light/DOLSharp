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
 * 2) Go to loc=12235,8713 Lough Derg. and /use the Sluagh Necklace from Addrir. 
 * 3) Speak with Lady Legada when she arrives to receive the Sluagh Plans. 
 * 4) Take the Sluagh Plans back to Addrir for your reward.
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
	public class TraitorInMagMellDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(TraitorInMagMell); }
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
			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof(NuisancesHib)) == 0)
				return false;

			if (!BaseAddirQuest.CheckPartAccessible(player, typeof(TraitorInMagMell)))
				return false;

			return base.CheckQuestQualification(player);
		}
	}

	/* The second thing we do, is to declare the class we create
	* as Quest. We must make it persistant using attributes, to
	* do this, we derive from the abstract class AbstractQuest
	*/
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(TraitorInMagMell), ExtendsType = typeof(AbstractQuest))] 
	public class TraitorInMagMell : BaseAddirQuest
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
		protected const string questTitle = "Traitor in Mag Mell";

		private static GameNPC addrir = null;
		private static GameNPC ladyLegada = null;

		private static GenericItemTemplate necklaceOfDoppelganger = null;
		private static GenericItemTemplate sluaghPlans = null;
		private static FeetArmorTemplate recruitsBoots = null;
		private static FeetArmorTemplate recruitsQuiltedBoots = null;

		private static GameLocation legadaEnd = new GameLocation("Lady Legada", 200, 200, 12235, 8713, 5304, 218);
		private static GameLocation legadaStart = new GameLocation("Lady Legada", 200, 330877, 492742, 5439, 2657);

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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Legada", eRealm.None);
			if (npcs.Length == 0)
			{
				//if (log.IsWarnEnabled) log.Warn("Could not find Lady Felin, creating her ...");
				ladyLegada = new GameMob();
				ladyLegada.Model = 679;
				ladyLegada.Name = "Lady Legada";
				ladyLegada.GuildName = "Part of " + questTitle + " Quest";
				ladyLegada.Realm = (byte) eRealm.None;
				ladyLegada.Region = legadaStart.Region;
				ladyLegada.Size = 50;
				ladyLegada.Level = 30;
				ladyLegada.Position = legadaStart.Position;
				ladyLegada.Heading = legadaStart.Heading;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				ladyLegada.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				//ladyFelin.SaveIntoDatabase();                
				//ladyFelin.AddToWorld();
			}
			else
				ladyLegada = npcs[0];

			#endregion

			#region defineItems

			// item db check
			necklaceOfDoppelganger = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "necklace_of_the_doppelganger");
			if (necklaceOfDoppelganger == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Necklace of the Doppelganger, creating it ...");
				necklaceOfDoppelganger = new GenericItemTemplate();
				necklaceOfDoppelganger.Name = "Necklace of the Doppelganger";
				necklaceOfDoppelganger.Level = 2;
				necklaceOfDoppelganger.Weight = 2;
				necklaceOfDoppelganger.Model = 101;

				necklaceOfDoppelganger.ItemTemplateID = "necklace_of_the_doppelganger";

				necklaceOfDoppelganger.IsDropable = false;
				necklaceOfDoppelganger.IsSaleable = false;
				necklaceOfDoppelganger.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(necklaceOfDoppelganger);
			}

			// item db check
			sluaghPlans = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "sluagh_plans");
			if (sluaghPlans == null)
			{
				sluaghPlans = new GenericItemTemplate();
				sluaghPlans.Name = "Sluagh Plans";
				if (log.IsWarnEnabled)
					log.Warn("Could not find" + sluaghPlans.Name + " , creating it ...");

				sluaghPlans.Weight = 3;
				sluaghPlans.Model = 498;

				sluaghPlans.ItemTemplateID = "sluagh_plans";

				sluaghPlans.IsDropable = false;
				sluaghPlans.IsSaleable = false;
				sluaghPlans.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sluaghPlans);
			}

			// item db check
			recruitsBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "recruits_cailiocht_boots");
			if (recruitsBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Cailiocht Boots, creating it ...");
				recruitsBoots = new FeetArmorTemplate();
				recruitsBoots.Name = "Recruit's Cailiocht Boots";
				recruitsBoots.Level = 7;

				recruitsBoots.Weight = 24;
				recruitsBoots.Model = 84; // studded Boots

				recruitsBoots.ArmorFactor = 12;
				recruitsBoots.ArmorLevel = eArmorLevel.Medium;

				recruitsBoots.ItemTemplateID = "recruits_cailiocht_boots";
				recruitsBoots.Value = 1000;

				recruitsBoots.IsDropable = true;
				recruitsBoots.IsSaleable = true;
				recruitsBoots.IsTradable = true;

				recruitsBoots.Color = 13; // green leather

				recruitsBoots.Bonus = 1; // default bonus

				recruitsBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 3));
				recruitsBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBoots);
			}

			// item db check
			recruitsQuiltedBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "recruits_quilted_boots");
			if (recruitsQuiltedBoots == null)
			{
				recruitsQuiltedBoots = new FeetArmorTemplate();
				recruitsQuiltedBoots.Name = "Recruit's Quilted Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsQuiltedBoots.Name + ", creating it ...");
				recruitsQuiltedBoots.Level = 7;

				recruitsQuiltedBoots.Weight = 8;
				recruitsQuiltedBoots.Model = 155; // studded Boots

				recruitsQuiltedBoots.ArmorLevel = eArmorLevel.VeryLow;
				recruitsQuiltedBoots.ArmorFactor = 6;

				recruitsQuiltedBoots.ItemTemplateID = "recruits_quilted_boots";
				recruitsQuiltedBoots.Value = 1000;

				recruitsQuiltedBoots.IsDropable = true;
				recruitsQuiltedBoots.IsSaleable = true;
				recruitsQuiltedBoots.IsTradable = true;
				recruitsQuiltedBoots.Color = 32;

				recruitsQuiltedBoots.Bonus = 5; // default bonus

				recruitsQuiltedBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));
				recruitsQuiltedBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 1));
				recruitsQuiltedBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsQuiltedBoots);
			}

			#endregion

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

			GameEventMgr.AddHandler(ladyLegada, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyLegada));
			GameEventMgr.AddHandler(ladyLegada, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyLegada));

			/* Now we bring to addrir the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(addrir, typeof(TraitorInMagMellDescriptor));

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

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.RemoveHandler(ladyLegada, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyLegada));
			GameEventMgr.RemoveHandler(ladyLegada, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyLegada));

			/* Now we remove to addrir the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(addrir, typeof(TraitorInMagMellDescriptor));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (player.Model == ladyLegada.Model)
				{
					GameClient client = player.Client;
					player.Model = player.CreationModel;
					SendSystemMessage(player, "You change back to your normal form!");
				}

				if (quest.Step == 3)
				{
					if (ladyLegada != null)
						ladyLegada.Delete();
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...
			if (player.Model == ladyLegada.Model)
				return;

			TraitorInMagMell quest = (TraitorInMagMell) player.IsDoingQuest(typeof (TraitorInMagMell));
			if (quest == null)
				return;

			if (quest.Step == 2 || quest.Step == 3)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;
				GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Name == necklaceOfDoppelganger.Name)
				{
					if (player.Position.CheckSquareDistance(legadaEnd.Position, 2500*2500))
					{
						foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
						}

						player.Model = ladyLegada.Model; // morph to fairie
						SendSystemMessage(player, "You change into a new form!");
						RegionTimer resetTimer = new RegionTimer(player, new RegionTimerCallback(quest.ResetPlayerModel), 60000); // call after 10 minutes                    

						if (!ladyLegada.Alive || ladyLegada.ObjectState != GameObject.eObjectState.Active)
						{
							ladyLegada.Position = legadaStart.Position;
							ladyLegada.Heading = legadaStart.Heading;
							ladyLegada.AddToWorld();
							ladyLegada.WalkTo(legadaEnd.Position, ladyLegada.MaxSpeed);
						}
						quest.Step = 3;
					}
				}
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step == 3 && (!ladyLegada.Alive || ladyLegada.ObjectState != GameObject.eObjectState.Active))
				{
					ladyLegada.Position = legadaEnd.Position;
					ladyLegada.Heading = legadaEnd.Heading;
					ladyLegada.AddToWorld();
				}
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

			if (QuestMgr.CanGiveQuest(typeof(TraitorInMagMell), player, addrir) <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			addrir.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					addrir.SayTo(player, "Lirone, this Sluagh problem has its roots deeper into Mag Mell than I originally thought! We have apprehended a [traitor]!");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							addrir.SayTo(player, "Go west from Mag Mell towards Tir na Nog. You will come to the base of the foot hills. Head southwest-south along the base of the hills. The necklace will alert you when you are in the correct place. Hurry now. Time is wasting.");
							break;
						case 4:
							addrir.SayTo(player, ""+player.Name+", you've returned. I'm sure you were successful with your mission. Come come my friend. Hand over the information.");
							break;
						case 5:
							addrir.SayTo(player, "For you, a nice pair of boots to help keep your feet dry and clean. Thank you again for the help you've given me in this matter Lirone. I shall report this to Fagan at once.");
							if (quest.Step == 5)
							{
								quest.FinishQuest();
							}
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
							//case "ire fairies": SendReply(player,"I managed to have a little chat with some of the guardsmen, and it turns out there is a [traitor] here in Cotswold!"); break;
						case "traitor":
							addrir.SayTo(player, "Aye Lirone. I could scarcely believe my ears when I heard the news. A man by the name of Samyr has been in [league] with the Sluagh for a while now.");
							break;
						case "league":
							addrir.SayTo(player, "Samyr has given up some information, but not nearly as much as we hoped for. Fagan has asked me to recruit anyone I could to help deal with this problem. Are you up for the [challenge] Lirone?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "challenge":
							player.Out.SendCustomDialog("Will you help Mag Mell by taking on this vital mission?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "necklace":
							addrir.SayTo(player, "Aye. I am giving it to you to use for this meeting. You will not have to say anything, or so Samyr says. Just go to the [meeting location], use the necklace and wait for the Sluagh.");
							break;
						case "meeting location":
							addrir.SayTo(player, "Go west from Mag Mell towards Tir na Nog. You will come to the base of the foot hills. Head southwest-south along the base of the hills. Hurry now. Time is wasting.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
							}
							break;

							// step 5
						case "reward":
							addrir.SayTo(player, "For you, a nice pair of boots to help keep your feet dry and clean. Thank you again for the help you've given me in this matter Lirone. I shall report this to Fagan at once.");
							if (quest.Step == 5)
							{
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		protected static void TalkToLadyLegada(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(TraitorInMagMell), player, addrir) <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			ladyLegada.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step >= 2)
				{
					ladyLegada.SayTo(player, "Greetings Ally. It is good to see you here. My time is short, so I will make this [brief].");
					return;
				}

				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "brief":
							ladyLegada.SayTo(player, "Our Queen has laid out the plans for Mag Mell and is eager to have your assistance. Here are the plans, memorize your part. Do not fail us in this, lest dire circumstances befall you. Till we meet again.");
							if (quest.Step == 3)
							{
								player.ReceiveItem(ladyLegada, sluaghPlans.CreateInstance());

								new RegionTimer(ladyLegada, new RegionTimerCallback(quest.CastLadyLegada), 10000);
								new RegionTimer(ladyLegada, new RegionTimerCallback(quest.RemoveLadyLegada), 12000);

								quest.Step = 4;
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

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if (QuestMgr.CanGiveQuest(typeof(TraitorInMagMell), player, addrir) <= 0)
				return;

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(TraitorInMagMell), player, addrir))
					return;

				addrir.SayTo(player, "Excellent! Now listen, Samyr was to meet with the Sluagh today! He gave us a [necklace] that helped him transform into a Sluagh for a small amount of time.");
				// give necklace
				player.ReceiveItem(addrir, necklaceOfDoppelganger.CreateInstance());

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected virtual int CastLadyLegada(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in ladyLegada.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendSpellCastAnimation(ladyLegada, 1, 20);
			}
			return 0;

		}


		protected virtual int RemoveLadyLegada(RegionTimer callingTimer)
		{
			if (ladyLegada != null)
				ladyLegada.Delete();
			return 0;
		}


		protected virtual int ResetPlayerModel(RegionTimer callingTimer)
		{
			m_questPlayer.Model = m_questPlayer.CreationModel;
			SendSystemMessage("You change back to your normal form!");
			return 0;
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
						return "[Step #1] Listen as Addrir outlines the details of this important mission. If he stops speaking with you try interacting with him again.";
					case 2:
						return "[Step #2] Take the necklace Addrir has given you and look for the meeting location. Make your way to the Tir na Nog gates, they are to the west of Addrir. Then travel south-southeast along the base of the hills. Use the necklace near to the location.";
					case 3:
						return "[Step #3] Speak with Lady Legada once she arrives.";
					case 4:
						return "[Step #4] Take the Sluagh Plans you received from Lady Legada back to Addrir for further analysis.";
					case 5:
						return "[Step #5] Wait for Addrir to reward you. If he stops speaking with you, ask if he has a [reward] for your efforts.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (TraitorInMagMell)) == null)
				return;

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Name == sluaghPlans.Name)
				{
					RemoveItemFromPlayer(addrir, sluaghPlans.CreateInstance());

					addrir.TurnTo(m_questPlayer);
					addrir.SayTo(m_questPlayer, "Ah! Their plans, but alas, I can not read their language. Hrm...I shall have to think on this. I'm sure there is someone I can find to translate this for me. But never mind that right now. I have a [reward] for you for your hard work and bravery.");
					addrir.Emote(eEmote.Ponder);
					Step = 5;
					return;
				}
			}

		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(addrir, necklaceOfDoppelganger);
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsBoots.CreateInstance() as EquipableItem))
				GiveItemToPlayer(addrir, recruitsBoots.CreateInstance());
			else
				GiveItemToPlayer(addrir, recruitsQuiltedBoots.CreateInstance());

			m_questPlayer.GainExperience(40, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
