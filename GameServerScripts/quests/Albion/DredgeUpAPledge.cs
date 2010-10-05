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
 * Author: k109
 * 
 * Date: 12/5/07
 * Directory: /scripts/quests/albion/
 * 
 * Compiled on SVN 905
 * 
 * Description: The "Dredge Up A Pledge" quest, mimics live US servers.
 */
using System;
using DOL.Database;
using DOL.Events;
using DOL.Language;

namespace DOL.GS.Quests.Albion
{
	public class DredgeUpAPledge : RewardQuest
	{
        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.QuestTitle");
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;

		private static GameNPC sirDorian = null;
		private QuestGoal banditPledgesKilled;

		public DredgeUpAPledge()
			: base()
		{
			Init();
		}

		public DredgeUpAPledge(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public DredgeUpAPledge(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public DredgeUpAPledge(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			ItemTemplate insigniarings = new ItemTemplate();
			insigniarings.Weight = 0;
			insigniarings.Condition = 50000;
			insigniarings.MaxCondition = 50000;
			insigniarings.Model = 103;
			insigniarings.Extension = 1;
			insigniarings.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.DredgeUpAPledge.Init.Text1");

			Level = 1;
			QuestGiver = sirDorian;
			Rewards.Experience = 30;
			Rewards.MoneyPercent = 100;
			banditPledgesKilled = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Init.Text2"), QuestGoal.GoalType.KillTask, 2, insigniarings);

		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");


			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Dorian", eRealm.Albion);

			if (npcs.Length == 0)
			{
				sirDorian = new GameNPC();
				sirDorian.Model = 28;
				sirDorian.Name = "Sir Dorian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sirDorian.Name + ", creating him ...");
				//k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
				//sirDorian.GuildName = "Part of " + questTitle + " Quest";
				sirDorian.Realm = eRealm.Albion;
				sirDorian.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
				sirDorian.Inventory = template.CloseTemplate();
				sirDorian.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				sirDorian.Size = 52;
				sirDorian.Level = 40;
				sirDorian.X = 560869;
				sirDorian.Y = 511737;
				sirDorian.Z = 2344;
				sirDorian.Heading = 2930;

				if (SAVE_INTO_DATABASE)
					sirDorian.SaveIntoDatabase();

				sirDorian.AddToWorld();
			}
			else
				sirDorian = npcs[0];

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(sirDorian, GameLivingEvent.Interact, new DOLEventHandler(TalkTosirDorian));
			GameEventMgr.AddHandler(sirDorian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkTosirDorian));

			sirDorian.AddQuestToGive(typeof(DredgeUpAPledge));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (sirDorian == null)
				return;

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(sirDorian, GameObjectEvent.Interact, new DOLEventHandler(TalkTosirDorian));
			GameEventMgr.RemoveHandler(sirDorian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkTosirDorian));

			sirDorian.RemoveQuestToGive(typeof(DredgeUpAPledge));
		}

		protected static void TalkTosirDorian(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			DredgeUpAPledge quest = player.IsDoingQuest(typeof(DredgeUpAPledge)) as DredgeUpAPledge;
			sirDorian.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new DredgeUpAPledge();
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.banditPledgesKilled.IsAchieved)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(DredgeUpAPledge)))
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
			// We're not going to offer this quest if the player is already on it...

			if (player.IsDoingQuest(this.GetType()) != null)
				return false;

			// ...nor will we let him do it again.

			if (player.HasFinishedQuest(this.GetType()) > 0)
				return false;

			// Also, he needs to finish another quest first.

			if (player.HasFinishedQuest(typeof(NoHopeForTheHopeful)) <= 0)
				return false;

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
			DredgeUpAPledge quest = player.IsDoingQuest(typeof(DredgeUpAPledge)) as DredgeUpAPledge;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.CheckPlayerAbortQuest.Text1"));
			}
			else
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.CheckPlayerAbortQuest.Text2", questTitle));
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

			if (sirDorian.CanGiveQuest(typeof(DredgeUpAPledge), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(DredgeUpAPledge)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!sirDorian.GiveQuest(typeof(DredgeUpAPledge), player, 1))
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
				String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Story");
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
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Summary");
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Conclusion.Text1", QuestPlayer.Name));
				text += LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Conclusion.Text2");
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
			if (player.IsDoingQuest(typeof(DredgeUpAPledge)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name.IndexOf(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.DredgeUpAPledge.Notify")) >= 0)
				{
					if (!banditPledgesKilled.IsAchieved)
					{
						banditPledgesKilled.Advance();
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
		}

	}
}
