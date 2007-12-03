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
	public class NoHopeForTheHopeful : RewardQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "No Hope For The Hopeful";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;

		private static GameNPC sirDorian = null;
		private short banditHopefulsKilled = 0;

		public NoHopeForTheHopeful()
			: base()
		{
			Init();
		}

		public NoHopeForTheHopeful(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
			Init();
			SetCustomProperty("kills", "0");
		}

		public NoHopeForTheHopeful(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
			SetCustomProperty("kills", "0");
		}

		public NoHopeForTheHopeful(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
			String kills = GetCustomProperty("kills");
			if (kills != null && kills.Length > 0)
				banditHopefulsKilled = Int16.Parse(GetCustomProperty("kills"));
		}

		private void Init()
		{
			ItemTemplate intelligentBracer = CreateBracer();
			intelligentBracer.Id_nb = "recruits_intelligent_bracer";
			intelligentBracer.Name = "Recruit's Intelligent Bracer";
			intelligentBracer.Bonus1 = 4;
			intelligentBracer.Bonus1Type = (int)eProperty.Acuity;
			intelligentBracer.Bonus2 = 3;
			intelligentBracer.Bonus2Type = (int)eProperty.Constitution;
			intelligentBracer.Bonus3 = 2;
			intelligentBracer.Bonus3Type = (int)eProperty.Resist_Slash;

			ItemTemplate mightyBracer = CreateBracer();
			mightyBracer.Id_nb = "recruits_mighty_bracer";
			mightyBracer.Name = "Recruit's Mighty Bracer";
			mightyBracer.Bonus1 = 4;
			mightyBracer.Bonus1Type = (int)eProperty.Strength;
			mightyBracer.Bonus2 = 4;
			mightyBracer.Bonus2Type = (int)eProperty.Constitution;
			mightyBracer.Bonus3 = 2;
			mightyBracer.Bonus3Type = (int)eProperty.Resist_Slash;

			ItemTemplate slyBracer = CreateBracer();
			slyBracer.Id_nb = "recruits_sly_bracer";
			slyBracer.Name = "Recruit's Sly Bracer";
			slyBracer.Bonus1 = 4;
			slyBracer.Bonus1Type = (int)eProperty.Dexterity;
			slyBracer.Bonus2 = 4;
			slyBracer.Bonus2Type = (int)eProperty.Quickness;
			slyBracer.Bonus3 = 2;
			slyBracer.Bonus3Type = (int)eProperty.Resist_Slash;

			ItemTemplate piousBracer = CreateBracer();
			piousBracer.Id_nb = "recruits_pious_bracer";
			piousBracer.Name = "Recruit's Pious Bracer";
			piousBracer.Bonus1 = 4;
			piousBracer.Bonus1Type = (int)eProperty.Acuity;
			piousBracer.Bonus2 = 3;
			piousBracer.Bonus2Type = (int)eProperty.Dexterity;
			piousBracer.Bonus3 = 2;
			piousBracer.Bonus3Type = (int)eProperty.Resist_Slash;

			Level = 1;
			QuestGiver = sirDorian;
			Rewards.Experience = 22;
			Rewards.MoneyPercent = 20;
			Rewards.AddOptionalItem(intelligentBracer);
			Rewards.AddOptionalItem(mightyBracer);
			Rewards.AddOptionalItem(slyBracer);
			Rewards.AddOptionalItem(piousBracer);
			Rewards.ChoiceOf = 1;
		}

		/// <summary>
		/// Create a raw bracer.
		/// </summary>
		/// <returns></returns>
		private ItemTemplate CreateBracer()
		{
			ItemTemplate template = new ItemTemplate();
			template.Level = 7;
			template.Durability = 50000;
			template.MaxDurability = 50000;
			template.Condition = 50000;
			template.MaxCondition = 50000;
			template.Quality = 100;
			template.Object_Type = 41;
			template.Item_Type = 33;
			template.Weight = 10;
			template.Model = 598;
			template.Bonus = 5;
			template.IsPickable = true;
			template.IsDropable = true;
			template.CanDropAsLoot = false;
			template.IsTradable = true;
			template.Platinum = 0;
			template.Gold = 0;
			template.Silver = 0;
			template.Copper = 22;
			template.MaxCount = 1;
			template.PackSize = 1;
			template.Realm = 1;
			return template;
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Dorian", eRealm.Albion);
			sirDorian = npcs[0];

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(sirDorian, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirDorian));
			GameEventMgr.AddHandler(sirDorian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirDorian));
			
			sirDorian.AddQuestToGive(typeof(NoHopeForTheHopeful));

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

			GameEventMgr.RemoveHandler(sirDorian, GameObjectEvent.Interact, new DOLEventHandler(TalkToSirDorian));
			GameEventMgr.RemoveHandler(sirDorian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirDorian));

			sirDorian.RemoveQuestToGive(typeof(NoHopeForTheHopeful));
		}

		protected static void TalkToSirDorian(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (sirDorian.CanGiveQuest(typeof(NoHopeForTheHopeful), player) <= 0)
				return;

			
			NoHopeForTheHopeful quest = player.IsDoingQuest(typeof(NoHopeForTheHopeful)) as NoHopeForTheHopeful;
			sirDorian.TurnTo(player);
			
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new NoHopeForTheHopeful();
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.banditHopefulsKilled == 2)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(NoHopeForTheHopeful)))
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
			if (player.IsDoingQuest(typeof(NoHopeForTheHopeful)) != null)
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
			NoHopeForTheHopeful quest = player.IsDoingQuest(typeof(NoHopeForTheHopeful)) as NoHopeForTheHopeful;

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

			if (sirDorian.CanGiveQuest(typeof(NoHopeForTheHopeful), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(NoHopeForTheHopeful)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!sirDorian.GiveQuest(typeof(NoHopeForTheHopeful), player, 1))
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
				String desc = "So you're one of the new arrivals, eh? Welcome to our village! "
					+ "Its always a pleasure to meet newcomers.\n\nYou may've heard a bit of a "
					+ "rumor about a lack of local guards here. Unfortunately, it's true, "
					+ "I'm afraid. King Constantine has spread out our defenses quite thin, leaving "
					+ "folks around these parts pretty nervous about their safety. Not that he can be "
					+ "blamed, we just have too many foes to worry about in these trying times.\n\n"
					+ "If you're truly willing to help like you say, then I could use your assistance. "
					+ "One never-ending problem we endure is the trouble stirred up by local brigands "
					+ "that seem to cover the countryside like a plague. The more we arrest, the "
					+ "more we find days later. They're like insects... where you see one, there are "
					+ "hundreds more you don't see.\n\nWould you be willing to help us out? Our people "
					+ "should feel free to go about their days without the constant worry of being set "
					+ "upon by thugs. Kill two bandit hopefuls and bring me two Bandit Cloaks as proof "
					+ "of this deed.";

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
				return "Find and defeat two bandit hopefuls, then return to Sir Dorian for your reward.";
			}
		}

		/// <summary>
		/// The goal of the quest, keeps track of kill counts etc.
		/// </summary>
		public override string Goal
		{
			get
			{
				return String.Format("Quest Goal : Defeat two bandit hopefuls ({0}/2)", banditHopefulsKilled);
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get 
			{
				String text = String.Format("Excellent, {0}!  You've done very well. ", 
					QuestPlayer.Name);
				text += "Perhaps word will spread among the bandits that the citizens of Cotswold will no ";
				text += "longer tolerate their fear-mongering.";
				return text;
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
			if (player.IsDoingQuest(typeof(NoHopeForTheHopeful)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name.IndexOf("bandit hopeful") >= 0)
				{
					if (banditHopefulsKilled == 0)
					{
						banditHopefulsKilled++;
						SetCustomProperty("kills", "1");
						SaveIntoDatabase();
						GoalAdvance(player);
						return;
					}
					else if (banditHopefulsKilled == 1)
					{
						banditHopefulsKilled++;
						SetCustomProperty("kills", "2");
						SaveIntoDatabase();
						GoalAdvance(player);
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
