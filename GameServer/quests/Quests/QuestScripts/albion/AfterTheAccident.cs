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
	public class AfterTheAccident : RewardQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "After The Accident";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;

		private static GameNPC SirPrescott = null;
		private QuestGoal punySkeletonGoal;

		private static ItemTemplate RecruitsNecklaceofMight = null;
		private static ItemTemplate RecruitsNecklaceofInsight = null;
		private static ItemTemplate RecruitsNecklaceofFaith = null;

		public AfterTheAccident()
			: base()
		{
			Init();
		}

		public AfterTheAccident(GamePlayer questingPlayer)
			: this(questingPlayer, 1) { }

		public AfterTheAccident(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public AfterTheAccident(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			#region defineItems
			RecruitsNecklaceofMight = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_might");
			RecruitsNecklaceofInsight = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_insight");
			RecruitsNecklaceofFaith = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_necklace_of_faith");
			
			ItemTemplate punySkeletonSkull = new ItemTemplate();
			punySkeletonSkull.Weight = 1;
			punySkeletonSkull.Condition = 50000;
			punySkeletonSkull.MaxCondition = 50000;
			punySkeletonSkull.Model = 540;
			punySkeletonSkull.Extension = 1;
			punySkeletonSkull.Name = "Puny Skeleton Skull";
			
			#endregion

			QuestGiver = SirPrescott;
			Rewards.Experience = 22;
			Rewards.MoneyPercent = 10;
			Rewards.AddOptionalItem(RecruitsNecklaceofMight);
			Rewards.AddOptionalItem(RecruitsNecklaceofInsight);
			Rewards.AddOptionalItem(RecruitsNecklaceofFaith);
			Rewards.ChoiceOf = 1;

			punySkeletonGoal = AddGoal("Puny skeleton skulls", QuestGoal.GoalType.KillTask, 2, 
				punySkeletonSkull);
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
				SirPrescott.Realm = eRealm.Albion;
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

			SirPrescott.AddQuestToGive(typeof(AfterTheAccident));

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

			SirPrescott.RemoveQuestToGive(typeof(AfterTheAccident));
		}

		protected static void TalkToSirPrescott(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies      
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (SirPrescott.CanGiveQuest(typeof(AfterTheAccident), player) <= 0)
				return;


			AfterTheAccident quest = player.IsDoingQuest(typeof(AfterTheAccident)) as AfterTheAccident;
			SirPrescott.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new AfterTheAccident();
					quest.QuestGiver = SirPrescott;
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.punySkeletonGoal.IsAchieved)
						quest.ChooseRewards(player);
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AfterTheAccident)))
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
			if (player.IsDoingQuest(typeof(AfterTheAccident)) != null)
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
			AfterTheAccident quest = player.IsDoingQuest(typeof(AfterTheAccident)) as AfterTheAccident;

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

			if (SirPrescott.CanGiveQuest(typeof(AfterTheAccident), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(AfterTheAccident)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!SirPrescott.GiveQuest(typeof(AfterTheAccident), player, 1))
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
						return Summary;
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
			if (player.IsDoingQuest(typeof(AfterTheAccident)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name.IndexOf("puny skeleton") >= 0)
				{
					if (!punySkeletonGoal.IsAchieved)
					{
						punySkeletonGoal.Advance();
						return;
					}
				}
			}
		}
	}
}