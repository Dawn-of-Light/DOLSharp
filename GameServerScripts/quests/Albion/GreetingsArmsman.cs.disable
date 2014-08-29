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
using DOL.Database;
using DOL.Events;
using DOL.Language;
using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	public class GreetingsArmsman : RewardQuest
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.QuestTitle");
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 10;

		private static GameNPC 	captainKinzee = null;
		private QuestGoal greetingsArmsmanGoal;

		public GreetingsArmsman()
			: base()
		{
			Init();
		}

		public GreetingsArmsman(GamePlayer questingPlayer)
			: this(questingPlayer, 1) { }

		public GreetingsArmsman(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public GreetingsArmsman(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			#region defineItems


            ItemTemplate standardDagger = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_dagger_alb");
			if (standardDagger == null)
			{
				standardDagger = CreateOneHand();
				standardDagger.Id_nb = "standard_dagger_alb";
                standardDagger.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text1");
				standardDagger.Bonus1 = 3;
				standardDagger.Bonus1Type = (int)eProperty.Strength;
				standardDagger.Bonus2 = 1;
				standardDagger.Bonus2Type = (int)eProperty.Dexterity;
				standardDagger.Bonus3 = 1;
				standardDagger.Bonus3Type = (int)eProperty.Quickness;
                standardDagger.Bonus4 = 1;
                standardDagger.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardDagger.SPD_ABS = 23;
                standardDagger.Type_Damage = 3;
                standardDagger.Model = 25;
                standardDagger.Object_Type = 4;
                standardDagger.Item_Type = 11;
                GameServer.Database.AddObject(standardDagger);
			}

            ItemTemplate standardSword = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_sword_alb");
            if (standardSword == null)
            {
                standardSword = CreateOneHand();
                standardSword.Id_nb = "standard_sword_alb";
                standardSword.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text2");
                standardSword.Bonus1 = 3;
                standardSword.Bonus1Type = (int)eProperty.Strength;
                standardSword.Bonus2 = 1;
                standardSword.Bonus2Type = (int)eProperty.Dexterity;
                standardSword.Bonus3 = 1;
                standardSword.Bonus3Type = (int)eProperty.Quickness;
                standardSword.Bonus4 = 1;
                standardSword.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardSword.SPD_ABS = 25;
                standardSword.Object_Type = 3;
                standardSword.Type_Damage = 2;
                standardSword.Item_Type = 10;
                standardSword.Model = 651;
                GameServer.Database.AddObject(standardSword);
            }

            ItemTemplate standardHammer = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_hammer_alb_str");
            if (standardHammer == null)
            {
                standardHammer = CreateOneHand();
                standardHammer.Id_nb = "standard_hammer_alb_str";
                standardHammer.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text3");
                standardHammer.Bonus1 = 3;
                standardHammer.Bonus1Type = (int)eProperty.Strength;
                standardHammer.Bonus2 = 1;
                standardHammer.Bonus2Type = (int)eProperty.Dexterity;
                standardHammer.Bonus3 = 1;
                standardHammer.Bonus3Type = (int)eProperty.Quickness;
                standardHammer.Bonus4 = 1;
                standardHammer.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardHammer.SPD_ABS = 26;
                standardHammer.Object_Type = 2;
                standardHammer.Type_Damage = 1;
                standardHammer.Item_Type = 10;
                standardHammer.Model = 12;
                GameServer.Database.AddObject(standardHammer);
            }

            ItemTemplate standardWarPike = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_war_pike");
            if (standardWarPike == null)
            {
                standardWarPike = CreateTwoHanded();
                standardWarPike.Id_nb = "standard_war_pike";
                standardWarPike.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text4");
                standardWarPike.Bonus1 = 3;
                standardWarPike.Bonus1Type = (int)eProperty.Strength;
                standardWarPike.Bonus2 = 1;
                standardWarPike.Bonus2Type = (int)eProperty.Dexterity;
                standardWarPike.Bonus3 = 1;
                standardWarPike.Bonus3Type = (int)eProperty.Quickness;
                standardWarPike.Bonus4 = 1;
                standardWarPike.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardWarPike.SPD_ABS = 28;
                standardWarPike.Type_Damage = 3;
                standardWarPike.Model = 26;
                GameServer.Database.AddObject(standardWarPike);
            }

            ItemTemplate standardHalberd = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_halberd");
            if (standardHalberd == null)
            {
                standardHalberd = CreateTwoHanded();
                standardHalberd.Id_nb = "standard_halberd";
                standardHalberd.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text5");
                standardHalberd.Bonus1 = 3;
                standardHalberd.Bonus1Type = (int)eProperty.Strength;
                standardHalberd.Bonus2 = 1;
                standardHalberd.Bonus2Type = (int)eProperty.Dexterity;
                standardHalberd.Bonus3 = 1;
                standardHalberd.Bonus3Type = (int)eProperty.Quickness;
                standardHalberd.Bonus4 = 1;
                standardHalberd.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardHalberd.SPD_ABS = 28;
                standardHalberd.Type_Damage = 2;
                standardHalberd.Model = 67;
                GameServer.Database.AddObject(standardHalberd);
            }

            ItemTemplate standardLucerneHammer = GameServer.Database.FindObjectByKey<ItemTemplate>("standard_lucerne_hammer");
            if (standardLucerneHammer == null)
            {
                standardLucerneHammer = CreateTwoHanded();
                standardLucerneHammer.Id_nb = "standard_lucerne_hammer";
                standardLucerneHammer.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Init.Text6");
                standardLucerneHammer.Bonus1 = 3;
                standardLucerneHammer.Bonus1Type = (int)eProperty.Strength;
                standardLucerneHammer.Bonus2 = 1;
                standardLucerneHammer.Bonus2Type = (int)eProperty.Dexterity;
                standardLucerneHammer.Bonus3 = 1;
                standardLucerneHammer.Bonus3Type = (int)eProperty.Quickness;
                standardLucerneHammer.Bonus4 = 1;
                standardLucerneHammer.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                standardLucerneHammer.SPD_ABS = 28;
                standardLucerneHammer.Type_Damage = 1;
                standardLucerneHammer.Model = 70;
                GameServer.Database.AddObject(standardLucerneHammer);
            }
			#endregion

			Level = 1;
            QuestGiver = captainKinzee;
			Rewards.Experience = 22;
			Rewards.MoneyPercent = 18;
			Rewards.AddOptionalItem(standardDagger);
			Rewards.AddOptionalItem(standardSword);
			Rewards.AddOptionalItem(standardHammer);
            Rewards.AddOptionalItem(standardWarPike);
            Rewards.AddOptionalItem(standardHalberd);
            Rewards.AddOptionalItem(standardLucerneHammer);
			Rewards.ChoiceOf = 1;

            greetingsArmsmanGoal = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description"), QuestGoal.GoalType.KillTask, 0, null);

		}

		/// <summary>
		/// Create a one hand weapon.
		/// </summary>
		/// <returns></returns>
		
        private ItemTemplate CreateOneHand()
        {
            ItemTemplate template = new ItemTemplate();
            template.Level = 8;
            template.Durability = 50000;
            template.MaxDurability = 50000;
            template.Condition = 50000;
            template.MaxCondition = 50000;
            template.Quality = 100;
            template.DPS_AF = 28;
            template.Weight = 8;
            template.Bonus = 5;
            template.IsPickable = true;
            template.IsDropable = true;
            template.CanDropAsLoot = false;
            template.IsTradable = true;
            template.Price = Money.GetMoney(0, 0, 0, 0, 22);
            template.MaxCount = 1;
            template.PackSize = 1;
            template.Realm = 1;
            template.AllowedClasses = "2;9;11;4;1;19;3";
            return template;
        }

        private ItemTemplate CreateTwoHanded()
        {
            ItemTemplate template = new ItemTemplate();
            template.Level = 10;
            template.Durability = 50000;
            template.MaxDurability = 50000;
            template.Condition = 50000;
            template.MaxCondition = 50000;
            template.Quality = 100;
            template.DPS_AF = 28;
            template.Object_Type = 6;
            template.Item_Type = 12;
            template.Weight = 12;
            template.Bonus = 5;
            template.IsPickable = true;
            template.IsDropable = true;
            template.CanDropAsLoot = false;
            template.IsTradable = true;
            template.Price = Money.GetMoney(0, 0, 0, 0, 22);
            template.MaxCount = 1;
            template.PackSize = 1;
            template.Realm = 1;
            template.AllowedClasses = "1";
            return template;
        }

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Captain Kinzee", eRealm.Albion);

            if (npcs == null || npcs.Length == 0)
            {
                captainKinzee = new GameNPC();
                captainKinzee.Model = 39;
                captainKinzee.Name = "Captain Kinzee";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + captainKinzee.Name + ", creating him ...");
                captainKinzee.GuildName = "Part of " + questTitle + " Quest";
                captainKinzee.Realm = eRealm.Albion;
                captainKinzee.CurrentRegionID = 27;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 691);
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 692);
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 688);
                template.AddNPCEquipment(eInventorySlot.Cloak, 676);
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 689);
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 690);
                template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3294); 
                captainKinzee.Inventory = template.CloseTemplate();
                captainKinzee.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                captainKinzee.Size = 50;
                captainKinzee.Level = 50;
                captainKinzee.X = 98343;
                captainKinzee.Y = 90564;
                captainKinzee.Z = 5716;
                captainKinzee.Heading = 3698;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    captainKinzee.SaveIntoDatabase();

                captainKinzee.AddToWorld();
            }
            else
                captainKinzee = npcs[0];
                
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(captainKinzee, GameLivingEvent.Interact, new DOLEventHandler(TalkToCaptainKinzee));
            GameEventMgr.AddHandler(captainKinzee, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCaptainKinzee));

            captainKinzee.AddQuestToGive(typeof(GreetingsArmsman));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
            if (captainKinzee == null)
				return;

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(captainKinzee, GameObjectEvent.Interact, new DOLEventHandler(TalkToCaptainKinzee));
            GameEventMgr.RemoveHandler(captainKinzee, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCaptainKinzee));

            captainKinzee.RemoveQuestToGive(typeof(GreetingsArmsman));
		}

		protected static void TalkToCaptainKinzee(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

            if (player.CharacterClass.ID != (byte)eCharacterClass.Armsman)
                return;
			
			GreetingsArmsman quest = player.IsDoingQuest(typeof(GreetingsArmsman)) as GreetingsArmsman;
            captainKinzee.TurnTo(player);
			
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new GreetingsArmsman();
					quest.OfferQuest(player);
				}
				else
				{
                    if (quest.Step == 1)
                    {
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description1"));
                    }
                    else if (quest.Step == 2)
                    {
                        quest.ChooseRewards(player);
                    }
				}
			}
            else if (e == GameLivingEvent.WhisperReceive)
            {
                if (quest.Step == 2)
                    return;
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                
                
                switch (wArgs.Text)
                {
                    case "Crush":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Crush"));
                        break;
                    case "Slash":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Slash"));
                        break;                    
                    case "Thrust":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Thrust"));
                        break;
                    case "Shield":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Shield"));
                        break;
                    case "Polearm":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Polearm"));
                        break;                    
                    case "done":
                        captainKinzee.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Description.Done"));
                        quest.Step = 2;
                        break;
                    default:break;
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(GreetingsArmsman)))
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

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

            if (player.CharacterClass.ID != (byte)eCharacterClass.Armsman)
                return false;

			return true;
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			GreetingsArmsman quest = player.IsDoingQuest(typeof(GreetingsArmsman)) as GreetingsArmsman;

			if (quest == null)
				return;

			if (response == 0x00)
			{
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.CheckPlayerAbortQuest.Text1"));
			}
			else
			{
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.CheckPlayerAbortQuest.Text2", questTitle));
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

            if (captainKinzee.CanGiveQuest(typeof(GreetingsArmsman), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(GreetingsArmsman)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

                if (!captainKinzee.GiveQuest(typeof(GreetingsArmsman), player, 1))
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
                    default: return "No Queststep Description available.";
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
                String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Story");
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
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Summary");
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
                String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Conclusion.Text1", QuestPlayer.Name));
                text += LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.GreetingsArmsman.Conclusion.Text2");
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
			if (player.IsDoingQuest(typeof(GreetingsArmsman)) == null)
				return;


			/*if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.GreetingsPaladin.Notify")) >= 0)
				{
                    if (!greetingsPaladinGoal.IsAchieved)
					{
                        greetingsPaladinGoal.Advance();
						return;
					}
				}
			}*/
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
