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
 * 2) Go to loc= 45394,39768 Vale of Mularn and speak with MasterBriedi. 
 * 3) Kill Queen Vuuna, center of the Askefruer village, loc=47071,38934 Vale of Mularn. 
 * 4) Speak with MasterBried. 
 * 5) Go back to Dalikor and hand him Queen Vuuna's Head for your rewards.
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

namespace DOL.GS.Quests.Midgard
{

	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class CulminationMidDescriptor : AbstractQuestDescriptor
	{
		public static Type descriptorType = typeof(CulminationMidDescriptor);

		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return CulminationMid.questType; }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 5; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 5; }
		}

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(CulminationMid.questType) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (!BaseDalikorQuest.CheckPartAccessible(player, CulminationMid.questType))
				return false;

			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(CulminationMid), ExtendsType = typeof(AbstractQuest))] 
	public class CulminationMid : BaseDalikorQuest
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

		protected const string questTitle = "Culmination (Mid)";

		public static Type questType = typeof(CulminationMid);

		private static GameNPC dalikor = null;

		private GameNPC briediClone = null;

		private GameNPC[] recruits = new GameNPC[4];

		private static GameMob queenVuuna = null;
		private static GameMob[] askefruerSorceress = new GameMob[4];

		private static GenericItemTemplate queenVuunaHead = null;

		private bool queenVuunaAttackStarted = false;

		private static HandsArmorTemplate recruitsGauntlets = null;
		private static HandsArmorTemplate recruitsGloves = null;
		private static JewelTemplate recruitsJewel = null;
		private static JewelTemplate recruitsJewelCloth = null;

		private static BracerTemplate recruitsBracer = null;


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

			dalikor = GetDalikor();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Queen Vuuna", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Queen Vuuna, creating ...");
				queenVuuna = new GameMob();

				Zone z = WorldMgr.GetRegion(100).GetZone(100);
				queenVuuna.Name = "Queen Vuuna";
				queenVuuna.Position = z.ToRegionPosition(new Point(47071, 38934, 4747));
				queenVuuna.Heading = 50;
				queenVuuna.Model = 678;
				queenVuuna.GuildName = "Part of " + questTitle + " Quest";
				queenVuuna.Realm = (byte) eRealm.None;
				queenVuuna.RegionId = 100;
				queenVuuna.Size = 49;
				queenVuuna.Level = 5;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 30;
				brain.AggroRange = 600;
				queenVuuna.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					queenVuuna.SaveIntoDatabase();

				queenVuuna.AddToWorld();
			}
			else
			{
				queenVuuna = (GameMob) npcs[0];
			}

			int counter = 0;
			foreach (GameNPC npc in queenVuuna.GetNPCsInRadius(500))
			{
				if (npc.Name == "askefruer sorceress")
				{
					askefruerSorceress[counter] = (GameMob) npc;
					counter++;
				}
				if (counter == askefruerSorceress.Length)
					break;
			}

			for (int i = 0; i < askefruerSorceress.Length; i++)
			{
				if (askefruerSorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find askefruer sorceress, creating ...");
					askefruerSorceress[i] = new GameMob();
					askefruerSorceress[i].Model = 678; // //819;
					askefruerSorceress[i].Name = "askefruer sorceress";
					askefruerSorceress[i].GuildName = "Part of " + questTitle + " Quest";
					askefruerSorceress[i].Realm = (byte) eRealm.None;
					askefruerSorceress[i].RegionId = 100;
					askefruerSorceress[i].Size = 35;
					askefruerSorceress[i].Level = 5;
					Point pos = queenVuuna.Position;
					pos.X += Util.Random(30, 150);
					pos.Y += Util.Random(30, 150);
					askefruerSorceress[i].Position = pos;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 30;
					brain.AggroRange = 600;
					askefruerSorceress[i].SetOwnBrain(brain);

					askefruerSorceress[i].Heading = 93;

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						askefruerSorceress[i].SaveIntoDatabase();
					askefruerSorceress[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems

			queenVuunaHead = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "queen_vuuna_head");
			if (queenVuunaHead == null)
			{
				queenVuunaHead = new GenericItemTemplate();
				queenVuunaHead.Name = "Queen Vuuna's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + queenVuunaHead.Name + " , creating it ...");

				queenVuunaHead.Weight = 15;
				queenVuunaHead.Model = 503;

				queenVuunaHead.ItemTemplateID = "queen_vuuna_head";

				queenVuunaHead.IsDropable = false;
				queenVuunaHead.IsSaleable = false;
				queenVuunaHead.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(queenVuunaHead);
			}

			// item db check
			recruitsGauntlets = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "recruits_studded_gauntlets_mid");
			if (recruitsGauntlets == null)
			{
				recruitsGauntlets = new HandsArmorTemplate();
				recruitsGauntlets.Name = "Recruit's Studded Gauntles (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGauntlets.Name + ", creating it ...");
				recruitsGauntlets.Level = 8;

				recruitsGauntlets.Weight = 24;
				recruitsGauntlets.Model = 80;

				recruitsGauntlets.ArmorFactor = 14;
				recruitsGauntlets.ArmorLevel = eArmorLevel.Medium;
				recruitsGauntlets.ItemTemplateID = "recruits_studded_gauntlets_mid";
				recruitsGauntlets.Value = 900;

				recruitsGauntlets.IsDropable = true;
				recruitsGauntlets.IsSaleable = true;
				recruitsGauntlets.IsTradable = true;
				recruitsGauntlets.Color = 36; // blue leather

				recruitsGauntlets.Bonus = 5; // default bonus

				recruitsGauntlets.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
				recruitsGauntlets.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGauntlets);
			}

			// item db check
			recruitsGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "recruits_quilted_gloves");
			if (recruitsGloves == null)
			{
				recruitsGloves = new HandsArmorTemplate();
				recruitsGloves.Name = "Recruit's Quilted Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGloves.Name + ", creating it ...");
				recruitsGloves.Level = 8;

				recruitsGloves.Weight = 8;
				recruitsGloves.Model = 154;

				recruitsGloves.ArmorFactor = 7;
				recruitsGloves.ArmorLevel = eArmorLevel.VeryLow;
				recruitsGloves.ItemTemplateID = "recruits_quilted_gloves";
				recruitsGloves.Value = 900;

				recruitsGloves.IsDropable = true;
				recruitsGloves.IsSaleable = true;
				recruitsGloves.IsTradable = true;
				recruitsGloves.Color = 36; // red cloth

				recruitsGloves.Bonus = 5; // default bonus

				recruitsGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 4));
				recruitsGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGloves);
			}

			recruitsJewel = (JewelTemplate)GameServer.Database.FindObjectByKey(typeof(JewelTemplate), "recruits_tarnished_bauble");
			if (recruitsJewel == null)
			{
				recruitsJewel = new JewelTemplate();
				recruitsJewel.Name = "Recruit's Tarnished Bauble";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewel.Name + ", creating it ...");
				recruitsJewel.Level = 7;

				recruitsJewel.Weight = 2;
				recruitsJewel.Model = 110;
				recruitsJewel.ItemTemplateID = "recruits_tarnished_bauble";
				recruitsJewel.Value = 900;

				recruitsJewel.IsDropable = true;
				recruitsJewel.IsSaleable = true;
				recruitsJewel.IsTradable = true;

				recruitsJewel.Bonus = 5; // default bonus

				recruitsJewel.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 4));
				recruitsJewel.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewel);
			}

			recruitsJewelCloth = (JewelTemplate)GameServer.Database.FindObjectByKey(typeof(JewelTemplate), "recruits_cloudy_jewel_cloth");
			if (recruitsJewelCloth == null)
			{
				recruitsJewelCloth = new JewelTemplate();
				recruitsJewelCloth.Name = "Recruit's Cloudy Jewel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewelCloth.Name + ", creating it ...");
				recruitsJewelCloth.Level = 7;

				recruitsJewelCloth.Weight = 2;
				recruitsJewelCloth.Model = 110;
				recruitsJewelCloth.ItemTemplateID = "recruits_cloudy_jewel_cloth";
				recruitsJewelCloth.Value = 900;

				recruitsJewelCloth.IsDropable = true;
				recruitsJewelCloth.IsSaleable = true;
				recruitsJewelCloth.IsTradable = true;

				recruitsJewelCloth.Bonus = 5; // default bonus

				recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 4));
				recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));
				recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewelCloth);
			}

			recruitsBracer = (BracerTemplate) GameServer.Database.FindObjectByKey(typeof (BracerTemplate), "recruits_golden_bracer");
			if (recruitsBracer == null)
			{
				recruitsBracer = new BracerTemplate();
				recruitsBracer.Name = "Recruit's Golden Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 7;

				recruitsBracer.Weight = 2;
				recruitsBracer.Model = 121;
				recruitsBracer.ItemTemplateID = "recruits_golden_bracer";
				recruitsBracer.Value = 900;

				recruitsBracer.IsDropable = true;
				recruitsBracer.IsSaleable = true;
				recruitsBracer.IsTradable = true;

				recruitsBracer.Bonus = 5; // default bonus

				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));
				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBracer);
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

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(queenVuuna, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenVuuna));

			/* Now we bring to Dalikor the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(dalikor, typeof(CulminationMidDescriptor));

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

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.RemoveHandler(queenVuuna, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenVuuna));

			/* Now we remove to dalikor the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(dalikor, typeof(CulminationMidDescriptor));
		}

		protected static void CheckNearQueenVuuna(DOLEvent e, object sender, EventArgs args)
		{
			GameMob queenTatiana = (GameMob) sender;

			// if princess is dead no ned to checks ...
			if (!queenTatiana.Alive || queenTatiana.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in queenTatiana.GetPlayersInRadius(1000))
			{
				CulminationMid quest = (CulminationMid) player.IsDoingQuest(typeof (CulminationMid));

				if (quest != null && !quest.queenVuunaAttackStarted && quest.Step == 2)
				{
					quest.queenVuunaAttackStarted = true;

					SendSystemMessage(player, "There they are. You take care of the queen I'll deal with the fairy sorcesses littleone.");
					IAggressiveBrain aggroBrain = queenTatiana.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 70);

					for (int i = 0; i < askefruerSorceress.Length; i++)
					{
						if (quest.recruits[i] != null)
						{
							aggroBrain = quest.recruits[i].Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(askefruerSorceress[i], 50);
						}

						aggroBrain = askefruerSorceress[i].Brain as IAggressiveBrain;
						if (aggroBrain != null)
							aggroBrain.AddToAggroList(quest.recruits[i], 50);
					}

					// if we find player doing quest stop looking for further ones ...
					break;
				}
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

			if (QuestMgr.CanGiveQuest(questType, player, dalikor) <= 0)
				return;

			//We also check if the player is already doing the quest
			CulminationMid quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;

			dalikor.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Hello again Eeinken. No doubt you have seen the Askefruer village northeast of here. They have been amassing as many Askfruer as they can, calling them from all over Midgard. We are dealing with a very large [problem].");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						dalikor.SayTo(player, "Alright my friend. Here is what you must do. You must travel northeast from this tower towards the road to Haggerfel. Follow the road a little ways. You will see the Askefruer village on your right. Master Briedi is waiting for you there. Good luck.");

					}
					else if (quest.Step == 4)
					{
						dalikor.SayTo(player, "Welcome back Eeinken. Where is Master Briedi? Don'T tell me he [returned to Gotar] again without visiting his old friend?");
					}
					else if (quest.Step == 5)
					{
						dalikor.SayTo(player, "I see. Excellent work recruit. Now, I have a [reward] for you.");
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
						case "problem":
							dalikor.SayTo(player, "I have already contacted Master Briedi and a few other former recruits to help with the situation. There are many Askefruer, but the task of slaying their [queen] has fallen to you.");
							break;
						case "queen":
							dalikor.SayTo(player, "This is a very important task I have set before you recruit Eeinken. Will you [take on] this burden and help Mularn?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "take on":
							player.Out.SendCustomDialog("Are you ready to take part in this monumental battle for the good of Mularn?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "returned to Gotar":
							dalikor.SayTo(player, "Ah, yes. I thought he would. He likes it there, for some reason. I'm hoping that this Askefruer threat is no more so I may go to visit with him. Tell me recruit, were you able to slay the queen?");
							break;

						case "reward":
							dalikor.SayTo(player, "The Council of Elders has authorized me to give you these two items as compensation for all the work you have done for us in this matter. I believe that now Eeinken, you have gone beyond my skills as a trainer. Always remember your training, it will come in more useful than you know. Now, return to your trainer, I'm sure they are anxious to assist you as your make your way in Midgard.");
							if (quest.Step == 5)
							{
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterBriedi(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(questType, player, dalikor) <= 0)
				return;

			//We also check if the player is already doing the quest
			CulminationMid quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;

			if (e == GameLivingEvent.Interact)
			{
				if (quest != null && quest.briediClone != null && quest.briediClone == sender)
				{
					quest.briediClone.TurnTo(player);

					if (quest.Step <= 1)
					{
						quest.briediClone.SayTo(player, "Ah, Eeinken, the one who always scares me. Yes, quite a pickle we're in this time, don't you agree? But don't worry, I have a [plan].");
					}
					else if (quest.Step == 2)
					{
						quest.briediClone.SayTo(player, "Go now and kill their queen, so that Mularn is at ease.");
						foreach (GameMob recruit in quest.recruits)
						{
							recruit.Follow(player, 50 + Util.Random(100), 4000);
						}
					}
					else if (quest.Step == 3)
					{
						quest.briediClone.SayTo(player, "Good fight Eeinken, good fight. Now that my work here is done, these fine recruits and I shall be heading back to Gotar where they can get some more training. Please tell Dalikor I returned to Gotar won't you? Be safe!");
						quest.ResetMasterBriedi();
						quest.Step = 4;
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
							{
								CulminationMid memberQuest = groupMember.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
								// we found another groupmember doing the same quest...
								if (memberQuest != null && memberQuest.Step == 3)
								{
									memberQuest.Step = 4;
								}
							}
						}
					}
					return;
				}
			} // The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.briediClone == sender)
				{
					quest.briediClone.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "plan":
							quest.briediClone.SayTo(player, "Remember last time, with Princess Aiyr? Surely you remember, you can't be that old, can you? These fresh recruits and I will stave off the army so you can concentrate on the Queen. [Got that]?");
							break;

						case "Got that":
							quest.briediClone.SayTo(player, "Good. Now, wait for her, she is tricky, but big, you should be able to spot her easily. Remember, let the recruits and I take care of the rest of them.");
							if (quest.Step == 1)
							{
								quest.briediClone.SayTo(player, "Attack!");
								quest.Step = 2;
								if (player.PlayerGroup != null)
								{
									foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
									{
										CulminationMid memberQuest = groupMember.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.Step == 1)
										{
											memberQuest.Step = 2;
										}
									}
								}
							}

							foreach (GameMob recruit in quest.recruits)
							{
								recruit.Follow(player, 50 + Util.Random(100), 4000);
							}
							break;
					}
				}
			}
		}

		protected virtual void ResetMasterBriedi()
		{
			if (briediClone != null && (briediClone.Alive || briediClone.ObjectState == GameObject.eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(briediClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimSpellSequence), 4000));

				m_animEmoteObjectQueue.Enqueue(briediClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimEmoteSequence), 5000));

				for (int i = 0; i < recruits.Length; i++)
				{
					if (recruits[i] != null)
					{
						m_animEmoteObjectQueue.Enqueue(recruits[i]);
						m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(recruits[i], new RegionTimerCallback(MakeAnimEmoteSequence), 4500));
					}
				}
			}

			if (briediClone != null)
			{
				RegionTimer castTimer = new RegionTimer(briediClone, new RegionTimerCallback(DeleteBriediClone), 6000);
			}
		}

		protected virtual int DeleteBriediClone(RegionTimer callingTimer)
		{
			if (briediClone != null)
			{
				briediClone.Delete();
				GameEventMgr.RemoveHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
				GameEventMgr.RemoveHandler(briediClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));
			}

			for (int i = 0; i < recruits.Length; i++)
			{
				if (recruits[i] != null)
					recruits[i].Delete();
			}

			return 0;
		}

		protected void CreateBriediClone()
		{
			GameNpcInventoryTemplate template;
			if (briediClone == null)
			{
				briediClone = new GameMob();
				briediClone.Model = 157;
				briediClone.Name = "Master Briedi";
				briediClone.GuildName = "Part of " + questTitle + " Quest";
				briediClone.Realm = (byte) eRealm.Midgard;
				briediClone.RegionId = 100;

				briediClone.Size = 50;
				briediClone.Level = 45;
				Zone z = WorldMgr.GetRegion(100).GetZone(100);
				briediClone.Position = z.ToRegionPosition(new Point(45394, 39768, 4709));
				briediClone.Heading = 107;

				template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
				briediClone.Inventory = template.CloseTemplate();
				briediClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				briediClone.SetOwnBrain(brain);

				briediClone.AddToWorld();

				GameEventMgr.AddHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
				GameEventMgr.AddHandler(briediClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));
			}
			else
			{
				Zone z = WorldMgr.GetRegion(100).GetZone(100);
				briediClone.MoveTo(100, z.ToRegionPosition(new Point(45394, 39768, 4709)), 107);
			}


			foreach (GamePlayer visPlayer in briediClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(briediClone, eEmote.Bind);
			}


			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i] = new GameMob();

				recruits[i].Name = "Recruit";

				recruits[i].GuildName = "Part of " + questTitle + " Quest";
				recruits[i].Realm = (byte) eRealm.Midgard;
				recruits[i].RegionId = briediClone.RegionId;

				recruits[i].Size = 50;
				recruits[i].Level = 6;
				Point pos = briediClone.Position;
				pos.X += Util.Random(-150, 150);
				pos.Y += Util.Random(-150, 150);
				recruits[i].Position = pos;
				recruits[i].Heading = 187;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				recruits[i].SetOwnBrain(brain);

			}

			recruits[0].Name = "Recruit Hietan";
			recruits[0].Model = 189;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 69);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
			recruits[0].Inventory = template.CloseTemplate();
			recruits[0].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

			recruits[1].Name = "Recruit Iduki";
			recruits[1].Model = 190;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 6);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 41);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 42);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 45);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 43);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 44);
			recruits[1].Inventory = template.CloseTemplate();
			recruits[1].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

			recruits[2].Name = "Recruit Odigi";
			recruits[2].Model = 774;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			recruits[2].Inventory = template.CloseTemplate();
			recruits[2].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

			recruits[3].Name = "Recruit Thulder";
			recruits[3].Model = 775;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 5);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			recruits[3].Inventory = template.CloseTemplate();
			recruits[3].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i].AddToWorld();
			}

		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			CulminationMid quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				quest.ResetMasterBriedi();
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			CulminationMid quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				if (quest.Step >= 1 && quest.Step <= 3)
				{
					quest.CreateBriediClone();
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
			if (QuestMgr.CanGiveQuest(questType, player, dalikor) <= 0)
				return;

			CulminationMid quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(CulminationMid), player, dalikor))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				dalikor.SayTo(player, "Alright my friend. Here is what you must do. You must travel northeast from this tower towards the road to Haggerfel. Follow the road a little ways. You will see the Askefruer village on your right. Master Briedi is waiting for you there. Good luck.");


				bool briediCloneCreated = false;
				if (player.PlayerGroup != null)
				{
					foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
					{
						CulminationMid memberQuest = groupMember.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
						// we found another groupmember doing the same quest...
						if (memberQuest != null && memberQuest.briediClone != null)
						{
							briediCloneCreated = true;
							break;
						}
					}
				}

				if (!briediCloneCreated)
				{
					quest = player.IsDoingQuest(typeof (CulminationMid)) as CulminationMid;
					if(quest != null) quest.CreateBriediClone();
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
						return "[Step #1] Make your way northeast from Dalikor and take the road towards Haggerfel. You will see Master Briedi and his troups off to the right of the road.";
					case 2:
						return "[Step #2] Find and kill Queen Vuuna.";
					case 3:
						return "[Step #3] Speak with Master Briedi when the fighting is over.";
					case 4:
						return "[Step #4] Return to Dalikor at the guard town near Mularn. Hand him the head of Queen Vuuna as evidence that the Askefuer threat has now been extinguished. Tell him Master Briedi has [returned to Gotar].";
					case 5:
						return "[Step #5] Wait for Dalikor to reward you. If he stops speaking with you, ask about a [reward] for your time and effort.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (CulminationMid)) == null)
				return;


			if (Step == 2 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == queenVuuna.Name)
				{
					SendSystemMessage("You slay the queen and take her head as proof.");
					GiveItemToPlayer(gArgs.Target, queenVuunaHead.CreateInstance());
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Name == queenVuunaHead.Name)
				{
					dalikor.SayTo(player, "I see. Excellent work recruit. Now, I have a [reward] for you.");
					RemoveItemFromPlayer(dalikor, queenVuunaHead);
					Step = 5;
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			// make sure to clean up, should be needed , but just to make certain
			ResetMasterBriedi();
			//Give reward to player here ...              
			m_questPlayer.GainExperience(1012, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 9, Util.Random(50)), "You recieve {0} as a reward.");
			if (m_questPlayer.HasAbilityToUseItem(recruitsGauntlets.CreateInstance() as EquipableItem))
			{
				GiveItemToPlayer(dalikor, recruitsGauntlets.CreateInstance());
				GiveItemToPlayer(dalikor, recruitsJewel.CreateInstance());
			}
			else
			{
				GiveItemToPlayer(dalikor, recruitsGloves.CreateInstance());
				GiveItemToPlayer(dalikor, recruitsJewelCloth.CreateInstance());
			}
			GiveItemToPlayer(dalikor, recruitsBracer.CreateInstance());

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
