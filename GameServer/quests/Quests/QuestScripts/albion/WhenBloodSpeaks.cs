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
* Description: The "When Blood Speaks" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class WhenBloodSpeaks : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "When Blood Speaks";
        protected const int minimumLevel = 3;
        protected const int maximumLevel = 5;

        private static GameNPC MasterKless = null;
        private QuestGoal spriggarnsKilled;

        private static ItemTemplate RecruitsQuiltedPants = null;
        private static ItemTemplate RecruitsLeatherLeggings = null;
        private static ItemTemplate RecruitsStuddedLegs = null;

        public WhenBloodSpeaks()
            : base()
        {
            Init();
        }

        public WhenBloodSpeaks(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public WhenBloodSpeaks(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public WhenBloodSpeaks(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check - All my quests with Recruit newbie items make a seperate
            // item then the older quests.
            RecruitsQuiltedPants = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_quilted_pants");
            if (RecruitsQuiltedPants == null)
            {
                RecruitsQuiltedPants = new ItemTemplate();
                RecruitsQuiltedPants.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.WhenBloodSpeaks.Init.Text1");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsQuiltedPants.Name + ", creating it ...");

                RecruitsQuiltedPants.Level = 5;
                RecruitsQuiltedPants.Weight = 20;
                RecruitsQuiltedPants.Model = 140;
                RecruitsQuiltedPants.Color = 28;

                RecruitsQuiltedPants.Object_Type = (int)eObjectType.Cloth;
                RecruitsQuiltedPants.Item_Type = (int)eEquipmentItems.LEGS;
                RecruitsQuiltedPants.Id_nb = "k109_recruits_quilted_pants";
                RecruitsQuiltedPants.Gold = 0;
                RecruitsQuiltedPants.Silver = 0;
                RecruitsQuiltedPants.Copper = 40;
                RecruitsQuiltedPants.IsPickable = true;
                RecruitsQuiltedPants.IsDropable = false; // can't be sold to merchand

                RecruitsQuiltedPants.DPS_AF = 6;
                RecruitsQuiltedPants.SPD_ABS = 0;
                RecruitsQuiltedPants.Bonus1 = 20;
                RecruitsQuiltedPants.Bonus1Type = (int)eProperty.MaxHealth;
                RecruitsQuiltedPants.Bonus2 = 1;
                RecruitsQuiltedPants.Bonus2Type = (int)eProperty.Resist_Body;
                RecruitsQuiltedPants.Bonus3 = 1;
                RecruitsQuiltedPants.Bonus3Type = (int)eProperty.Resist_Cold;
                RecruitsQuiltedPants.Bonus4 = 1;
                RecruitsQuiltedPants.Bonus4Type = (int)eProperty.Resist_Energy;
                RecruitsQuiltedPants.Bonus5 = 1;
                RecruitsQuiltedPants.Bonus5Type = (int)eProperty.Resist_Heat;
                RecruitsQuiltedPants.Bonus6 = 1;
                RecruitsQuiltedPants.Bonus6Type = (int)eProperty.Resist_Matter;
                RecruitsQuiltedPants.Bonus7 = 1;
                RecruitsQuiltedPants.Bonus7Type = (int)eProperty.Resist_Spirit;
                RecruitsQuiltedPants.Quality = 100;
                RecruitsQuiltedPants.Condition = 50000;
                RecruitsQuiltedPants.MaxCondition = 50000;
                RecruitsQuiltedPants.Durability = 50000;
                RecruitsQuiltedPants.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsQuiltedPants);
            }
            RecruitsLeatherLeggings = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_leather_leggings");
            if (RecruitsLeatherLeggings == null)
            {
                RecruitsLeatherLeggings = new ItemTemplate();
                RecruitsLeatherLeggings.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.WhenBloodSpeaks.Init.Text2");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsLeatherLeggings.Name + ", creating it ...");

                RecruitsLeatherLeggings.Level = 5;
                RecruitsLeatherLeggings.Weight = 40;
                RecruitsLeatherLeggings.Model = 32;
                RecruitsLeatherLeggings.Color = 11;

                RecruitsLeatherLeggings.Object_Type = (int)eObjectType.Leather;
                RecruitsLeatherLeggings.Item_Type = (int)eEquipmentItems.LEGS;
                RecruitsLeatherLeggings.Id_nb = "k109_recruits_leather_leggings";
                RecruitsLeatherLeggings.Gold = 0;
                RecruitsLeatherLeggings.Silver = 0;
                RecruitsLeatherLeggings.Copper = 40;
                RecruitsLeatherLeggings.IsPickable = true;
                RecruitsLeatherLeggings.IsDropable = false; // can't be sold to merchand

                RecruitsLeatherLeggings.DPS_AF = 12;
                RecruitsLeatherLeggings.SPD_ABS = 10;
                RecruitsLeatherLeggings.Bonus1 = 20;
                RecruitsLeatherLeggings.Bonus1Type = (int)eProperty.MaxHealth;
                RecruitsLeatherLeggings.Bonus2 = 1;
                RecruitsLeatherLeggings.Bonus2Type = (int)eProperty.Resist_Body;
                RecruitsLeatherLeggings.Bonus3 = 1;
                RecruitsLeatherLeggings.Bonus3Type = (int)eProperty.Resist_Cold;
                RecruitsLeatherLeggings.Bonus4 = 1;
                RecruitsLeatherLeggings.Bonus4Type = (int)eProperty.Resist_Energy;
                RecruitsLeatherLeggings.Bonus5 = 1;
                RecruitsLeatherLeggings.Bonus5Type = (int)eProperty.Resist_Heat;
                RecruitsLeatherLeggings.Bonus6 = 1;
                RecruitsLeatherLeggings.Bonus6Type = (int)eProperty.Resist_Matter;
                RecruitsLeatherLeggings.Bonus7 = 1;
                RecruitsLeatherLeggings.Bonus7Type = (int)eProperty.Resist_Spirit;

                RecruitsLeatherLeggings.Quality = 100;
                RecruitsLeatherLeggings.Condition = 50000;
                RecruitsLeatherLeggings.MaxCondition = 50000;
                RecruitsLeatherLeggings.Durability = 50000;
                RecruitsLeatherLeggings.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsLeatherLeggings);
            }
            RecruitsStuddedLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("k109_recruits_studded_legs");
            if (RecruitsStuddedLegs == null)
            {
                RecruitsStuddedLegs = new ItemTemplate();
                RecruitsStuddedLegs.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.WhenBloodSpeaks.Init.Text3");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsStuddedLegs.Name + ", creating it ...");

                RecruitsStuddedLegs.Level = 5;
                RecruitsStuddedLegs.Weight = 60;
                RecruitsStuddedLegs.Model = 82;
                RecruitsStuddedLegs.Color = 11;

                RecruitsStuddedLegs.Object_Type = (int)eObjectType.Studded;
                RecruitsStuddedLegs.Item_Type = (int)eEquipmentItems.LEGS;
                RecruitsStuddedLegs.Id_nb = "k109_recruits_studded_legs";
                RecruitsStuddedLegs.Gold = 0;
                RecruitsStuddedLegs.Silver = 0;
                RecruitsStuddedLegs.Copper = 40;
                RecruitsStuddedLegs.IsPickable = true;
                RecruitsStuddedLegs.IsDropable = false; // can't be sold to merchand

                RecruitsStuddedLegs.DPS_AF = 12;
                RecruitsStuddedLegs.SPD_ABS = 19;
                RecruitsStuddedLegs.Bonus1 = 20;
                RecruitsStuddedLegs.Bonus1Type = (int)eProperty.MaxHealth;
                RecruitsStuddedLegs.Bonus2 = 1;
                RecruitsStuddedLegs.Bonus2Type = (int)eProperty.Resist_Body;
                RecruitsStuddedLegs.Bonus3 = 1;
                RecruitsStuddedLegs.Bonus3Type = (int)eProperty.Resist_Cold;
                RecruitsStuddedLegs.Bonus4 = 1;
                RecruitsStuddedLegs.Bonus4Type = (int)eProperty.Resist_Energy;
                RecruitsStuddedLegs.Bonus5 = 1;
                RecruitsStuddedLegs.Bonus5Type = (int)eProperty.Resist_Heat;
                RecruitsStuddedLegs.Bonus6 = 1;
                RecruitsStuddedLegs.Bonus6Type = (int)eProperty.Resist_Matter;
                RecruitsStuddedLegs.Bonus7 = 1;
                RecruitsStuddedLegs.Bonus7Type = (int)eProperty.Resist_Spirit;

                RecruitsStuddedLegs.Quality = 100;
                RecruitsStuddedLegs.Condition = 50000;
                RecruitsStuddedLegs.MaxCondition = 50000;
                RecruitsStuddedLegs.Durability = 50000;
                RecruitsStuddedLegs.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsStuddedLegs);
            }
            #endregion

            ItemTemplate spriggarnBlood = new ItemTemplate();
            spriggarnBlood.Weight = 0;
            spriggarnBlood.Condition = 50000;
            spriggarnBlood.MaxCondition = 50000;
            spriggarnBlood.Model = 99;
            spriggarnBlood.Extension = 1;
            spriggarnBlood.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.WhenBloodSpeaks.Init.Text4");

            Level = 1;
            QuestGiver = MasterKless;
            Rewards.Experience = 270;
            Rewards.MoneyPercent = 50;
            Rewards.AddOptionalItem(RecruitsQuiltedPants);
            Rewards.AddOptionalItem(RecruitsLeatherLeggings);
            Rewards.AddOptionalItem(RecruitsStuddedLegs);
            Rewards.ChoiceOf = 1;
            spriggarnsKilled = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.Init.Text5"), QuestGoal.GoalType.KillTask, 2, spriggarnBlood);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.ScriptLoaded.Text1"), eRealm.Albion);

            if (npcs.Length == 0)
            {
                MasterKless = new GameNPC();
                MasterKless.Model = 64;
                MasterKless.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.ScriptLoaded.Text1");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + MasterKless.Name + ", creating him ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //MasterKless.GuildName = "Part of " + questTitle + " Quest";
                MasterKless.Realm = eRealm.Albion;
                MasterKless.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1005);  //Slot 25
                template.AddNPCEquipment(eInventorySlot.Cloak, 96);         //Slot 26
                template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 1166);     //Slot 12
                MasterKless.Inventory = template.CloseTemplate();
                MasterKless.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                MasterKless.Size = 51;
                MasterKless.Level = 50;
                MasterKless.X = 559370;
                MasterKless.Y = 513587;
                MasterKless.Z = 2428;
                MasterKless.Heading = 2685;

                if (SAVE_INTO_DATABASE)
                    MasterKless.SaveIntoDatabase();

                MasterKless.AddToWorld();
            }
            else
                MasterKless = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(MasterKless, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterKless));
            GameEventMgr.AddHandler(MasterKless, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterKless));

            MasterKless.AddQuestToGive(typeof(WhenBloodSpeaks));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (MasterKless == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(MasterKless, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterKless));
            GameEventMgr.RemoveHandler(MasterKless, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterKless));

            MasterKless.RemoveQuestToGive(typeof(WhenBloodSpeaks));
        }

        protected static void TalkToMasterKless(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (MasterKless.CanGiveQuest(typeof(WhenBloodSpeaks), player) <= 0)
                return;


            WhenBloodSpeaks quest = player.IsDoingQuest(typeof(WhenBloodSpeaks)) as WhenBloodSpeaks;
            MasterKless.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new WhenBloodSpeaks();
                    quest.QuestGiver = MasterKless;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.spriggarnsKilled.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(WhenBloodSpeaks)))
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
            if (player.IsDoingQuest(typeof(WhenBloodSpeaks)) != null)
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
            WhenBloodSpeaks quest = player.IsDoingQuest(typeof(WhenBloodSpeaks)) as WhenBloodSpeaks;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.CheckPlayerAbortQuest.Text1"));
            }
            else
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.CheckPlayerAbortQuest.Text2", questTitle));
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

            if (MasterKless.CanGiveQuest(typeof(WhenBloodSpeaks), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(WhenBloodSpeaks)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!MasterKless.GiveQuest(typeof(WhenBloodSpeaks), player, 1))
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
                String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.Story");
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
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.Summary");
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.Conclusion.Text1", QuestPlayer.Name));
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
            if (player.IsDoingQuest(typeof(WhenBloodSpeaks)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.WhenBloodSpeaks.Notify.Text1")) >= 0)
                    {
                    if (!spriggarnsKilled.IsAchieved)
                    {
                        spriggarnsKilled.Advance();
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
