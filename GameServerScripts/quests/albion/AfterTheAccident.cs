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
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	public class AftertheAccident : RewardQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "After the Accident";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;

		private static GameNPC SirPrescott = null;
		private short PunySkeletonsKilled = 0;

		private static ItemTemplate RecruitsNecklaceofMight = null;
		private static ItemTemplate RecruitsNecklaceofInsight = null;
		private static ItemTemplate RecruitsNecklaceofFaith = null;
		private static ItemTemplate PunySkeletonSkull = null;

		public AftertheAccident()
			: base()
		{
			Init();
		}

		public AftertheAccident(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
			Init();
			SetCustomProperty("a_kills", "0");
		}

		public AftertheAccident(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
			SetCustomProperty("a_kills", "0");
		}

		public AftertheAccident(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
			String kills = GetCustomProperty("a_kills");
			if (kills != null && kills.Length > 0)
				PunySkeletonsKilled = Int16.Parse(GetCustomProperty("a_kills"));
		}

		private void Init()
		{
			//ItemTemplate bracerOfShavedBone = new ItemTemplate();
			//bracerOfShavedBone = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate),
			//        "Bracer_of_Shaved_Bone");
			#region defineItems

			// Recruit's Necklace of Might
			RecruitsNecklaceofMight = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_might");
			if (RecruitsNecklaceofMight == null)
			{
				RecruitsNecklaceofMight = new ItemTemplate();
				RecruitsNecklaceofMight.Name = "Recruit's Necklace of Might";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsNecklaceofMight.Name + ", creating it ...");

				RecruitsNecklaceofMight.Level = 3;
				RecruitsNecklaceofMight.Weight = 3;
				RecruitsNecklaceofMight.Model = 101;

				RecruitsNecklaceofMight.Object_Type = (int)eObjectType.Magical;
				RecruitsNecklaceofMight.Item_Type = (int)eEquipmentItems.NECK;
				RecruitsNecklaceofMight.Id_nb = "recruits_necklace_of_might";
				RecruitsNecklaceofMight.Gold = 0;
				RecruitsNecklaceofMight.Silver = 2;
				RecruitsNecklaceofMight.Copper = 40;
				RecruitsNecklaceofMight.IsPickable = false;
				RecruitsNecklaceofMight.IsDropable = true; // can be sold to merchand

				RecruitsNecklaceofMight.Bonus1 = 4;
				RecruitsNecklaceofMight.Bonus1Type = (int)eProperty.Strength;
				RecruitsNecklaceofMight.Bonus2 = 4;
				RecruitsNecklaceofMight.Bonus2Type = (int)eProperty.Quickness;
				RecruitsNecklaceofMight.Bonus3 = 1;
				RecruitsNecklaceofMight.Bonus3Type = (int)eProperty.Resist_Body;
				RecruitsNecklaceofMight.Quality = 100;
				RecruitsNecklaceofMight.Condition = 50000;
				RecruitsNecklaceofMight.MaxCondition = 50000;
				RecruitsNecklaceofMight.Durability = 50000;
				RecruitsNecklaceofMight.MaxDurability = 50000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(RecruitsNecklaceofMight);
			}
			//Recruit's Necklace of Insight
			RecruitsNecklaceofInsight = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_insight");
			if (RecruitsNecklaceofInsight == null)
			{
				RecruitsNecklaceofInsight = new ItemTemplate();
				RecruitsNecklaceofInsight.Name = "Recruit's Necklace of Insight";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsNecklaceofInsight.Name + ", creating it ...");

				RecruitsNecklaceofInsight.Level = 3;
				RecruitsNecklaceofInsight.Weight = 3;
				RecruitsNecklaceofInsight.Model = 101;

				RecruitsNecklaceofInsight.Object_Type = (int)eObjectType.Magical;
				RecruitsNecklaceofInsight.Item_Type = (int)eEquipmentItems.NECK;
				RecruitsNecklaceofInsight.Id_nb = "recruits_necklace_of_insight";
				RecruitsNecklaceofInsight.Gold = 0;
				RecruitsNecklaceofInsight.Silver = 2;
				RecruitsNecklaceofInsight.Copper = 40;
				RecruitsNecklaceofInsight.IsPickable = false;
				RecruitsNecklaceofInsight.IsDropable = true; // can be sold to merchand

				RecruitsNecklaceofInsight.Bonus1 = 4;
				RecruitsNecklaceofInsight.Bonus1Type = (int)eProperty.Acuity;
				RecruitsNecklaceofInsight.Bonus2 = 4;
				RecruitsNecklaceofInsight.Bonus2Type = (int)eProperty.Dexterity;
				RecruitsNecklaceofInsight.Bonus3 = 3;
				RecruitsNecklaceofInsight.Bonus3Type = (int)eProperty.MaxHealth;
				RecruitsNecklaceofInsight.Quality = 100;
				RecruitsNecklaceofInsight.Condition = 50000;
				RecruitsNecklaceofInsight.MaxCondition = 50000;
				RecruitsNecklaceofInsight.Durability = 50000;
				RecruitsNecklaceofInsight.MaxDurability = 50000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(RecruitsNecklaceofInsight);
			}
			//Recruit's Necklace of Faith
			RecruitsNecklaceofFaith = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_faith");
			if (RecruitsNecklaceofFaith == null)
			{
				RecruitsNecklaceofFaith = new ItemTemplate();
				RecruitsNecklaceofFaith.Name = "Recruit's Necklace of Faith";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsNecklaceofFaith.Name + ", creating it ...");

				RecruitsNecklaceofFaith.Level = 3;
				RecruitsNecklaceofFaith.Weight = 3;
				RecruitsNecklaceofFaith.Model = 101;

				RecruitsNecklaceofFaith.Object_Type = (int)eObjectType.Magical;
				RecruitsNecklaceofFaith.Item_Type = (int)eEquipmentItems.NECK;
				RecruitsNecklaceofFaith.Id_nb = "recruits_necklace_of_faith";
				RecruitsNecklaceofFaith.Gold = 0;
				RecruitsNecklaceofFaith.Silver = 2;
				RecruitsNecklaceofFaith.Copper = 40;
				RecruitsNecklaceofFaith.IsPickable = false;
				RecruitsNecklaceofFaith.IsDropable = true; // can be sold to merchand

				RecruitsNecklaceofFaith.Bonus1 = 4;
				RecruitsNecklaceofFaith.Bonus1Type = (int)eProperty.Acuity;
				RecruitsNecklaceofFaith.Bonus2 = 1;
				RecruitsNecklaceofFaith.Bonus2Type = (int)eProperty.Resist_Body;
				RecruitsNecklaceofFaith.Bonus3 = 1;
				RecruitsNecklaceofFaith.Bonus3Type = (int)eProperty.AllMagicSkills;
				RecruitsNecklaceofFaith.Quality = 100;
				RecruitsNecklaceofFaith.Condition = 50000;
				RecruitsNecklaceofFaith.MaxCondition = 50000;
				RecruitsNecklaceofFaith.Durability = 50000;
				RecruitsNecklaceofFaith.MaxDurability = 50000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(RecruitsNecklaceofFaith);
			}
			//The puny skeleton skulls.
			PunySkeletonSkull = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "puny_skeleton_skull");
			if (PunySkeletonSkull == null)
			{
				PunySkeletonSkull = new ItemTemplate();
				PunySkeletonSkull.Name = "Puny Skeleton Skull";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + PunySkeletonSkull.Name + ", creating it ...");

				PunySkeletonSkull.Level = 0;
				PunySkeletonSkull.Weight = 0;
				PunySkeletonSkull.Model = 540;

				PunySkeletonSkull.Object_Type = (int)eObjectType.GenericItem;
				PunySkeletonSkull.Id_nb = "puny_skeleton_skull";
				PunySkeletonSkull.Gold = 0;
				PunySkeletonSkull.Silver = 0;
				PunySkeletonSkull.Copper = 0;
				PunySkeletonSkull.IsPickable = false;
				PunySkeletonSkull.IsDropable = false; // can't be sold to merchand

				PunySkeletonSkull.Quality = 100;
				PunySkeletonSkull.Condition = 100;
				PunySkeletonSkull.MaxCondition = 100;
				PunySkeletonSkull.Durability = 100;
				PunySkeletonSkull.MaxDurability = 100;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(PunySkeletonSkull);
			}
			#endregion

			QuestGiver = SirPrescott;
			Rewards.Experience = 22;
			Rewards.MoneyPercent = 10;
			Rewards.AddOptionalItem(RecruitsNecklaceofMight);
			Rewards.AddOptionalItem(RecruitsNecklaceofInsight);
			Rewards.AddOptionalItem(RecruitsNecklaceofFaith);
			Rewards.ChoiceOf = 1;
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");


			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Prescott", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				SirPrescott = new GameNPC();
				SirPrescott.Model = 28;
				SirPrescott.Name = "Sir Prescott";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + SirPrescott.Name + ", creating him ...");
				//SirPrescott.GuildName = "Part of " + questTitle + " Quest";
				SirPrescott.Realm = (byte)eRealm.Albion;
				SirPrescott.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
				SirPrescott.Inventory = template.CloseTemplate();
				SirPrescott.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				SirPrescott.Size = 50;
				SirPrescott.Level = 50;
				SirPrescott.X = 559862;
				SirPrescott.Y = 513092;
				SirPrescott.Z = 2408;
				SirPrescott.Heading = 2480;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					SirPrescott.SaveIntoDatabase();

				SirPrescott.AddToWorld();
			}
			else
				SirPrescott = npcs[0];

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(SirPrescott, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirPrescott));
			GameEventMgr.AddHandler(SirPrescott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirPrescott));

			SirPrescott.AddQuestToGive(typeof(AftertheAccident));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (SirPrescott == null)
				return;

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(SirPrescott, GameObjectEvent.Interact, new DOLEventHandler(TalkToSirPrescott));
			GameEventMgr.RemoveHandler(SirPrescott, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirPrescott));

			SirPrescott.RemoveQuestToGive(typeof(AftertheAccident));
		}

		protected static void TalkToSirPrescott(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies      
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (SirPrescott.CanGiveQuest(typeof(AftertheAccident), player) <= 0)
				return;


			AftertheAccident quest = player.IsDoingQuest(typeof(AftertheAccident)) as AftertheAccident;
			SirPrescott.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new AftertheAccident();
					quest.QuestGiver = SirPrescott;
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.PunySkeletonsKilled == 2)
					{
						quest.ChooseRewards(player);
					}
				}
			}
		}

		/// <summary>
		/// Callback for player accept/decline action.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AftertheAccident)))
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
			if (player.IsDoingQuest(typeof(AftertheAccident)) != null)
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
			AftertheAccident quest = player.IsDoingQuest(typeof(AftertheAccident)) as AftertheAccident;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, now go out there and finish your work!");
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
			// We recheck the qualification, because we don't talk to players
			// who are not doing the quest.

			if (SirPrescott.CanGiveQuest(typeof(AftertheAccident), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(AftertheAccident)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!SirPrescott.GiveQuest(typeof(AftertheAccident), player, 1))
					return;
			}
		}

		/// <summary>
		/// The quest title.
		/// </summary>
		public override string Name
		{
			get { return questTitle; }
		}

		/// <summary>
		/// The text for individual quest steps as shown in the journal.
		/// </summary>

		public override string Description
		{
			get
			{
				switch (Step)
				{

					case 1:
						return Goal;
					default:
						return "No Queststep Description available.";
				}
			}
		}

		/// <summary>
		/// The fully-fledged story to the quest.
		/// </summary>
		public override string Story
		{
			get
			{
				String desc = "During my patrols of Cotswold, I've seen many things that gravely concern me, "
					+ "my friend. The dead, are restless, and walk aboveground when they should be resting peacefully below. "
					+ "While this isn't quite a new phenomenon, there are more and more sightings of skeletons, zombies, and the like since that accursed accident."
					+ "\nYou've likely heard by now about the tear in the fabric of reality that happened accidentally when the finger-wagglers at the Academy moved the portal leading to the Shrouded Isles from Avalon Marsh to here in Cotswold. They opened a door into hell, and I don't think anyone knows just yet what that means. They think they capped it, but I'm not so sure."
					+ "\nWhile I can't be certain about what foul magic brings these 'things' forth from their final resting places, I do know these abominations can't be allowed to roam the countryside. I'm authorized to pay a bounty to anyone who puts them down and returns to me with proof of the deed.";

				return desc;
			}
		}

		/// <summary>
		/// A summary of the quest's story.
		/// </summary>
		public override string Summary
		{
			get
			{
				return "Kill two puny skeletons. Return to Sir Prescott with two Puny Skeleton Skulls as proof that you completed this task.";
			}
		}

		/// <summary>
		/// The goal of the quest, keeps track of kill counts etc.
		/// </summary>
		public override string Goal
		{
			get
			{
				return String.Format("Quest Goal : Puny skeleton skulls ({0}/2).", PunySkeletonsKilled);
				//return String.Format("Kill two puny skeletons. Return to Sir Prescott with two Puny Skeleton Skulls as proof that you completed this task.\nQuest Goal : Puny skeleton skulls ({0}/2).", PunySkeletonsKilled);
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				String text = String.Format("You have done well.  Here is your reward, and my thanks for serving Camelot!",
					QuestPlayer.Name);
				return text;
			}
		}

		/// <summary>
		/// The level of the quest as it shows in the journal.
		/// </summary>
		public override int Level
		{
			get
			{
				return 1;
			}
		}

		/// <summary>
		/// Handles quest events.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			GamePlayer player = sender as GamePlayer;

			if (player == null)
				return;
			if (player.IsDoingQuest(typeof(AftertheAccident)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name.IndexOf("puny skeleton") >= 0)
				{
					if (PunySkeletonsKilled == 0)
					{
						PunySkeletonsKilled++;
						SetCustomProperty("a_kills", "1");
						SaveIntoDatabase();
						GoalAdvance(player);
						GiveItem(QuestPlayer, PunySkeletonSkull);
						return;
					}
					else if (PunySkeletonsKilled == 1)
					{
						PunySkeletonsKilled++;
						SetCustomProperty("a_kills", "2");
						SaveIntoDatabase();
						GoalAdvance(player);
						GiveItem(QuestPlayer, PunySkeletonSkull);
						return;
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

		}

		public override void FinishQuest()
		{
			base.FinishQuest();
			RemoveItem(QuestPlayer, PunySkeletonSkull);
			RemoveItem(QuestPlayer, PunySkeletonSkull);
		}
	}
}