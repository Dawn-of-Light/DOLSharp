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
 * Description: The "Recruiting Nothing But Trouble" quest, mimics live US servers.
 */
using System;
using DOL.Database;
using DOL.Events;
using DOL.Language;

namespace DOL.GS.Quests.Albion
{
	public class RecruitingNothingButTrouble : RewardQuest
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.QuestTitle");

		protected const int minimumLevel = 2;
		protected const int maximumLevel = 5;

		private static GameNPC Rheda = null;
		private QuestGoal BanditRecruitsKilled;

		private static ItemTemplate RecruitsShortSword = null;
		private static ItemTemplate RecruitsDirk = null;
		private static ItemTemplate RecruitsMace = null;
		private static ItemTemplate RecruitsStaff = null;

		public RecruitingNothingButTrouble()
			: base()
		{
			Init();
		}

		public RecruitingNothingButTrouble(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public RecruitingNothingButTrouble(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public RecruitingNothingButTrouble(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			#region defineItems

			// item db check
			RecruitsShortSword = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_short_sword");
			if (RecruitsShortSword == null)
			{
				RecruitsShortSword = new ItemTemplate();
				RecruitsShortSword.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text1");
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsShortSword.Name + ", creating it ...");

				RecruitsShortSword.Level = 5;
				RecruitsShortSword.Weight = 18;
				RecruitsShortSword.Model = 3;
				RecruitsShortSword.Hand = 2;

				RecruitsShortSword.Object_Type = (int)eObjectType.SlashingWeapon;
				RecruitsShortSword.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
				RecruitsShortSword.Id_nb = "k109_recruits_short_sword";
				RecruitsShortSword.Price = 0;
				RecruitsShortSword.IsPickable = true;
				RecruitsShortSword.IsDropable = false;// can't be sold to merchand

				RecruitsShortSword.DPS_AF = 24;
				RecruitsShortSword.SPD_ABS = 30;
				RecruitsShortSword.Bonus = 5;
				RecruitsShortSword.Bonus1 = 3;
				RecruitsShortSword.Bonus1Type = (int)eProperty.Strength;
				RecruitsShortSword.Bonus2 = 1;
				RecruitsShortSword.Bonus2Type = (int)eProperty.AllMeleeWeaponSkills;
				RecruitsShortSword.Bonus3 = 1;
				RecruitsShortSword.Bonus3Type = (int)eProperty.Resist_Crush;
				RecruitsShortSword.Bonus4 = 1;
				RecruitsShortSword.Bonus4Type = (int)eProperty.Resist_Thrust;
				RecruitsShortSword.Bonus5 = 1;
				RecruitsShortSword.Bonus5Type = (int)eProperty.Quickness;

				RecruitsShortSword.Quality = 100;
				RecruitsShortSword.Condition = 50000;
				RecruitsShortSword.MaxCondition = 50000;
				RecruitsShortSword.Durability = 50000;
				RecruitsShortSword.MaxDurability = 50000;
				RecruitsShortSword.Type_Damage = 2;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(RecruitsShortSword);
			}
			RecruitsDirk = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_dirk");
			if (RecruitsDirk == null)
			{
				RecruitsDirk = new ItemTemplate();
				RecruitsDirk.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text2");
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsDirk.Name + ", creating it ...");

				RecruitsDirk.Level = 5;
				RecruitsDirk.Weight = 8;
				RecruitsDirk.Model = 21;
				RecruitsDirk.Hand = 2;

				RecruitsDirk.Object_Type = (int)eObjectType.ThrustWeapon;
				RecruitsDirk.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
				RecruitsDirk.Id_nb = "k109_recruits_dirk";
				RecruitsDirk.Price = 0;
				RecruitsDirk.IsPickable = true;
				RecruitsDirk.IsDropable = false;// can't be sold to merchand

				RecruitsDirk.DPS_AF = 24;
				RecruitsDirk.SPD_ABS = 26;
				RecruitsDirk.Bonus = 5;
				RecruitsDirk.Bonus1 = 3;
				RecruitsDirk.Bonus1Type = (int)eProperty.Dexterity;
				RecruitsDirk.Bonus2 = 1;
				RecruitsDirk.Bonus2Type = (int)eProperty.AllMeleeWeaponSkills;
				RecruitsDirk.Bonus3 = 1;
				RecruitsDirk.Bonus3Type = (int)eProperty.Resist_Crush;
				RecruitsDirk.Bonus4 = 1;
				RecruitsDirk.Bonus4Type = (int)eProperty.Resist_Thrust;
				RecruitsDirk.Bonus5 = 1;
				RecruitsDirk.Bonus5Type = (int)eProperty.Quickness;

				RecruitsDirk.Quality = 100;
				RecruitsDirk.Condition = 50000;
				RecruitsDirk.MaxCondition = 50000;
				RecruitsDirk.Durability = 50000;
				RecruitsDirk.MaxDurability = 50000;
				RecruitsDirk.Type_Damage = 3;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(RecruitsDirk);
			}
			RecruitsMace = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_mace");
			if (RecruitsMace == null)
			{
				RecruitsMace = new ItemTemplate();
				RecruitsMace.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text3");
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsMace.Name + ", creating it ...");

				RecruitsMace.Level = 5;
				RecruitsMace.Weight = 24;
				RecruitsMace.Model = 13;
				RecruitsMace.Hand = 2;

				RecruitsMace.Object_Type = (int)eObjectType.CrushingWeapon;
				RecruitsMace.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
				RecruitsMace.Id_nb = "k109_recruits_mace";
				RecruitsMace.Price = 0;
				RecruitsMace.IsPickable = true;
				RecruitsMace.IsDropable = false;// can't be sold to merchand

				RecruitsMace.DPS_AF = 24;
				RecruitsMace.SPD_ABS = 30;
				RecruitsMace.Bonus = 5;
				RecruitsMace.Bonus1 = 3;
				RecruitsMace.Bonus1Type = (int)eProperty.Acuity;
				RecruitsMace.Bonus2 = 1;
				RecruitsMace.Bonus2Type = (int)eProperty.AllMagicSkills;
				RecruitsMace.Bonus3 = 1;
				RecruitsMace.Bonus3Type = (int)eProperty.Resist_Crush;
				RecruitsMace.Bonus4 = 1;
				RecruitsMace.Bonus4Type = (int)eProperty.Resist_Thrust;
				RecruitsMace.Bonus5 = 1;
				RecruitsMace.Bonus5Type = (int)eProperty.Dexterity;

				RecruitsMace.Quality = 100;
				RecruitsMace.Condition = 50000;
				RecruitsMace.MaxCondition = 50000;
				RecruitsMace.Durability = 50000;
				RecruitsMace.MaxDurability = 50000;
				RecruitsMace.Type_Damage = 1;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(RecruitsMace);
			}
			RecruitsStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_staff");
			if (RecruitsStaff == null)
			{
				RecruitsStaff = new ItemTemplate();
				RecruitsStaff.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text4");
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + RecruitsStaff.Name + ", creating it ...");

				RecruitsStaff.Level = 5;
				RecruitsStaff.Weight = 24;
				RecruitsStaff.Model = 19;
				RecruitsStaff.Hand = 1;

				RecruitsStaff.Object_Type = (int)eObjectType.Staff;
				RecruitsStaff.Item_Type = (int)eEquipmentItems.TWO_HANDED;
				RecruitsStaff.Id_nb = "k109_recruits_staff";
				RecruitsStaff.Price = 0;
				RecruitsStaff.IsPickable = true;
				RecruitsStaff.IsDropable = false;// can't be sold to merchand

				RecruitsStaff.DPS_AF = 24;
				RecruitsStaff.SPD_ABS = 37;
				RecruitsStaff.Bonus = 5;
				RecruitsStaff.Bonus1 = 3;
				RecruitsStaff.Bonus1Type = (int)eProperty.Acuity;
				RecruitsStaff.Bonus2 = 1;
				RecruitsStaff.Bonus2Type = (int)eProperty.Constitution;
				RecruitsStaff.Bonus3 = 1;
				RecruitsStaff.Bonus3Type = (int)eProperty.Resist_Crush;
				RecruitsStaff.Bonus4 = 1;
				RecruitsStaff.Bonus4Type = (int)eProperty.Resist_Thrust;
				RecruitsStaff.Bonus5 = 1;
				RecruitsStaff.Bonus5Type = (int)eProperty.Dexterity;
				RecruitsStaff.Bonus6 = 10;
				RecruitsStaff.Bonus6Type = (int)eProperty.AllFocusLevels;  //Focus Spell Line?

				RecruitsStaff.Quality = 100;
				RecruitsStaff.Condition = 50000;
				RecruitsStaff.MaxCondition = 50000;
				RecruitsStaff.Durability = 50000;
				RecruitsStaff.MaxDurability = 50000;
				RecruitsStaff.Type_Damage = 1;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(RecruitsStaff);
			}
			#endregion

			ItemTemplate rhedasword = new ItemTemplate();
			rhedasword.Weight = 0;
			rhedasword.Condition = 50000;
			rhedasword.MaxCondition = 50000;
			rhedasword.Model = 3;
			rhedasword.Extension = 1;
			rhedasword.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text5");

			Level = 2;
			QuestGiver = Rheda;
			Rewards.Experience = 90;
			Rewards.MoneyPercent = 100;
			Rewards.AddOptionalItem(RecruitsShortSword);
			Rewards.AddOptionalItem(RecruitsDirk);
			Rewards.AddOptionalItem(RecruitsMace);
			Rewards.AddOptionalItem(RecruitsStaff);
			Rewards.ChoiceOf = 1;

			BanditRecruitsKilled = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.Init.Text6"), QuestGoal.GoalType.KillTask, 1, rhedasword);
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");


			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetObjectsByName<GameNPC>("Rheda", eRealm.Albion);

			if (npcs.Length == 0)
			{
				Rheda = new GameNPC();
				Rheda.Model = 6;
				Rheda.Name = "Rheda";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Rheda.Name + ", creating him ...");
				//k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
				//Rheda.GuildName = "Part of " + questTitle + " Quest";
				Rheda.Realm = eRealm.Albion;
				Rheda.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 80);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 54);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 51);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 52);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 53);
				Rheda.Inventory = template.CloseTemplate();
				Rheda.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				Rheda.Size = 52;
				Rheda.Level = 25;
				Rheda.X = 559712;
				Rheda.Y = 513513;
				Rheda.Z = 2428;
				Rheda.Heading = 3822;

				if (SAVE_INTO_DATABASE)
					Rheda.SaveIntoDatabase();

				Rheda.AddToWorld();
			}
			else
				Rheda = npcs[0];

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Rheda, GameLivingEvent.Interact, new DOLEventHandler(TalkToRheda));
			GameEventMgr.AddHandler(Rheda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRheda));

			Rheda.AddQuestToGive(typeof(RecruitingNothingButTrouble));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (Rheda == null)
				return;

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Rheda, GameObjectEvent.Interact, new DOLEventHandler(TalkToRheda));
			GameEventMgr.RemoveHandler(Rheda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRheda));

			Rheda.RemoveQuestToGive(typeof(RecruitingNothingButTrouble));
		}

		protected static void TalkToRheda(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Rheda.CanGiveQuest(typeof(RecruitingNothingButTrouble), player) <= 0)
				return;


			RecruitingNothingButTrouble quest = player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) as RecruitingNothingButTrouble;
			Rheda.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new RecruitingNothingButTrouble();
					quest.QuestGiver = Rheda;
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.BanditRecruitsKilled.IsAchieved)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(RecruitingNothingButTrouble)))
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
			if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			RecruitingNothingButTrouble quest = player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) as RecruitingNothingButTrouble;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.CheckPlayerAbortQuest.Text1"));
			}
			else
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.CheckPlayerAbortQuest.Text2", questTitle));
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

			if (Rheda.CanGiveQuest(typeof(RecruitingNothingButTrouble), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.
				if (!Rheda.GiveQuest(typeof(RecruitingNothingButTrouble), player, 1))
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
				String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.Story");
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
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.Summary");
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.Conclusion.Text1"));
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
			if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name.IndexOf(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.RecruitingNothingButTrouble.Notify.Text1")) >= 0)
				{
					if (!BanditRecruitsKilled.IsAchieved)
					{
						BanditRecruitsKilled.Advance();
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
