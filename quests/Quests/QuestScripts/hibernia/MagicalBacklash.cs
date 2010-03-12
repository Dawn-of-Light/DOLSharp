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
 * Author:		k109
 * 
 * Date:		12/5/07	
 * Directory: /scripts/quests/hibernia/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Talk with Josson
 * 2) Slay a Orchard nipper and retrieve it's wing.  You will receive some xp, copper and the weapon of your choice.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
    public class MagicalBacklash : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Magical Backlash";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC Josson = null;
        private QuestGoal OrchardNipperKilled;

        private static ItemTemplate RecruitsShortSword = null;
        private static ItemTemplate RecruitsDirk = null;
        private static ItemTemplate RecruitsClub = null;
        private static ItemTemplate RecruitsStaff = null;

        public MagicalBacklash()
            : base()
        {
            Init();
        }

        public MagicalBacklash(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public MagicalBacklash(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public MagicalBacklash(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check
            RecruitsShortSword = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_short_sword_hib");
            if (RecruitsShortSword == null)
            {
                RecruitsShortSword = new ItemTemplate();
                RecruitsShortSword.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Hib.MagicalBacklash.Init.Text1");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsShortSword.Name + ", creating it ...");

                RecruitsShortSword.Level = 5;
                RecruitsShortSword.Weight = 18;
                RecruitsShortSword.Model = 3;
                RecruitsShortSword.Hand = 2;

                RecruitsShortSword.Object_Type = (int)eObjectType.Blades;
                RecruitsShortSword.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
                RecruitsShortSword.Id_nb = "recruits_short_sword_hib";
                RecruitsShortSword.Gold = 0;
                RecruitsShortSword.Silver = 0;
                RecruitsShortSword.Copper = 0;
                RecruitsShortSword.IsPickable = true;
                RecruitsShortSword.IsDropable = false; // can't be sold to merchand

                RecruitsShortSword.DPS_AF = 24;
                RecruitsShortSword.SPD_ABS = 30;
                RecruitsShortSword.Bonus = 1;
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
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsShortSword);
            }
            RecruitsDirk = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_dirk");
            if (RecruitsDirk == null)
            {
                RecruitsDirk = new ItemTemplate();
                RecruitsDirk.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Hib.MagicalBacklash.Init.Text2");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsDirk.Name + ", creating it ...");

                RecruitsDirk.Level = 5;
                RecruitsDirk.Weight = 8;
                RecruitsDirk.Model = 21;
                RecruitsDirk.Hand = 2;

                RecruitsDirk.Object_Type = (int)eObjectType.ThrustWeapon;
                RecruitsDirk.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
                RecruitsDirk.Id_nb = "recruits_dirk";
                RecruitsDirk.Gold = 0;
                RecruitsDirk.Silver = 0;
                RecruitsDirk.Copper = 0;
                RecruitsDirk.IsPickable = true;
                RecruitsDirk.IsDropable = false; // can't be sold to merchand

                RecruitsDirk.DPS_AF = 24;
                RecruitsDirk.SPD_ABS = 26;
                RecruitsDirk.Bonus = 1;
                RecruitsDirk.Bonus1 = 3;
                RecruitsDirk.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsDirk.Bonus2 = 1;
                RecruitsDirk.Bonus2Type = (int)eProperty.AllMeleeWeaponSkills;
                RecruitsDirk.Bonus3 = 1;
                RecruitsDirk.Bonus3Type = (int)eProperty.Resist_Crush;
                RecruitsDirk.Bonus4 = 1;
                RecruitsDirk.Bonus4Type = (int)eProperty.Resist_Thrust;
                RecruitsDirk.Bonus5 = 1;
                RecruitsDirk.Bonus5Type = (int)eProperty.Strength;

                RecruitsDirk.Quality = 100;
                RecruitsDirk.Condition = 50000;
                RecruitsDirk.MaxCondition = 50000;
                RecruitsDirk.Durability = 50000;
                RecruitsDirk.MaxDurability = 50000;
                RecruitsDirk.Type_Damage = 3;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsDirk);
            }
            RecruitsClub = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_club");
            if (RecruitsClub == null)
            {
                RecruitsClub = new ItemTemplate();
                RecruitsClub.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Hib.MagicalBacklash.Init.Text3");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsClub.Name + ", creating it ...");

                RecruitsClub.Level = 5;
                RecruitsClub.Weight = 24;
                RecruitsClub.Model = 14;//11
                RecruitsClub.Hand = 2;

                RecruitsClub.Object_Type = (int)eObjectType.Blunt;
                RecruitsClub.Item_Type = (int)eEquipmentItems.RIGHT_HAND;
                RecruitsClub.Id_nb = "recruits_club";
                RecruitsClub.Gold = 0;
                RecruitsClub.Silver = 0;
                RecruitsClub.Copper = 0;
                RecruitsClub.IsPickable = true;
                RecruitsClub.IsDropable = false; // can't be sold to merchand

                RecruitsClub.DPS_AF = 24;
                RecruitsClub.SPD_ABS = 30;
                RecruitsClub.Bonus = 1;
                RecruitsClub.Bonus1 = 3;
                RecruitsClub.Bonus1Type = (int)eProperty.Acuity;
                RecruitsClub.Bonus2 = 1;
                RecruitsClub.Bonus2Type = (int)eProperty.AllMagicSkills;
                RecruitsClub.Bonus3 = 1;
                RecruitsClub.Bonus3Type = (int)eProperty.Resist_Crush;
                RecruitsClub.Bonus4 = 1;
                RecruitsClub.Bonus4Type = (int)eProperty.Resist_Thrust;
                RecruitsClub.Bonus5 = 1;
                RecruitsClub.Bonus5Type = (int)eProperty.Constitution;

                RecruitsClub.Quality = 100;
                RecruitsClub.Condition = 50000;
                RecruitsClub.MaxCondition = 50000;
                RecruitsClub.Durability = 50000;
                RecruitsClub.MaxDurability = 50000;
                RecruitsClub.Type_Damage = 1;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsClub);
            }
            RecruitsStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_staff");
            if (RecruitsStaff == null)
            {
                RecruitsStaff = new ItemTemplate();
                RecruitsStaff.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Hib.MagicalBacklash.Init.Text4");
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsStaff.Name + ", creating it ...");

                RecruitsStaff.Level = 5;
                RecruitsStaff.Weight = 24;
                RecruitsStaff.Model = 19;
                RecruitsStaff.Hand = 1;

                RecruitsStaff.Object_Type = (int)eObjectType.Staff;
                RecruitsStaff.Item_Type = (int)eEquipmentItems.TWO_HANDED;
                RecruitsStaff.Id_nb = "recruits_staff";
                RecruitsStaff.Gold = 0;
                RecruitsStaff.Silver = 0;
                RecruitsStaff.Copper = 0;
                RecruitsStaff.IsPickable = true;
                RecruitsStaff.IsDropable = false; // can't be sold to merchand

                RecruitsStaff.DPS_AF = 24;
                RecruitsStaff.SPD_ABS = 37;
                RecruitsStaff.Bonus = 1;
                RecruitsStaff.Bonus1 = 3;
                RecruitsStaff.Bonus1Type = (int)eProperty.Acuity;
                RecruitsStaff.Bonus2 = 1;
                RecruitsStaff.Bonus2Type = (int)eProperty.Dexterity;
                RecruitsStaff.Bonus3 = 1;
                RecruitsStaff.Bonus3Type = (int)eProperty.Resist_Crush;
                RecruitsStaff.Bonus4 = 1;
                RecruitsStaff.Bonus4Type = (int)eProperty.Resist_Thrust;
                RecruitsStaff.Bonus5 = 1;
                RecruitsStaff.Bonus5Type = (int)eProperty.AllMagicSkills;

                RecruitsStaff.Quality = 100;
                RecruitsStaff.Condition = 50000;
                RecruitsStaff.MaxCondition = 50000;
                RecruitsStaff.Durability = 50000;
                RecruitsStaff.MaxDurability = 50000;
                RecruitsStaff.Type_Damage = 1;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(RecruitsStaff);
            }
            #endregion

            ItemTemplate nipperwing = new ItemTemplate();
            nipperwing.Weight = 0;
            nipperwing.Condition = 50000;
            nipperwing.MaxCondition = 50000;
            nipperwing.Model = 551;
            nipperwing.Extension = 1;
            nipperwing.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Hib.MagicalBacklash.Init.Text5");

            Level = 2;
            QuestGiver = Josson;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 20;
            Rewards.AddOptionalItem(RecruitsShortSword);
            Rewards.AddOptionalItem(RecruitsDirk);
            Rewards.AddOptionalItem(RecruitsClub);
            Rewards.AddOptionalItem(RecruitsStaff);
            Rewards.ChoiceOf = 1;

            OrchardNipperKilled = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.Init.Text6"), QuestGoal.GoalType.KillTask, 1, nipperwing);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Josson", eRealm.Hibernia);

            if (npcs.Length == 0)
            {
                Josson = new GameNPC();
                Josson.Model = 382;
                Josson.Name = "Josson";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Josson.Name + ", creating him ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //Josson.GuildName = "Part of " + questTitle + " Quest";
                Josson.Realm = eRealm.Hibernia;
                Josson.CurrentRegionID = 200;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 386);   //Slot 22
                template.AddNPCEquipment(eInventorySlot.HeadArmor, 835);    //Slot 21
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 387);     //Slot 23
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 383);    //Slot 25
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 384);    //Slot 27
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 385);    //Slot 28
                Josson.Inventory = template.CloseTemplate();
                Josson.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                Josson.Size = 48;
                Josson.Level = 50;
                Josson.X = 346627;
                Josson.Y = 491453;
                Josson.Z = 5247;
                Josson.Heading = 2946;

                if (SAVE_INTO_DATABASE)
                    Josson.SaveIntoDatabase();

                Josson.AddToWorld();
            }
            else
                Josson = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Josson, GameLivingEvent.Interact, new DOLEventHandler(TalkToJosson));
            GameEventMgr.AddHandler(Josson, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJosson));

            Josson.AddQuestToGive(typeof(MagicalBacklash));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (Josson == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Josson, GameObjectEvent.Interact, new DOLEventHandler(TalkToJosson));
            GameEventMgr.RemoveHandler(Josson, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJosson));

            Josson.RemoveQuestToGive(typeof(MagicalBacklash));
        }

        protected static void TalkToJosson(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Josson.CanGiveQuest(typeof(MagicalBacklash), player) <= 0)
                return;


            MagicalBacklash quest = player.IsDoingQuest(typeof(MagicalBacklash)) as MagicalBacklash;
            Josson.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new MagicalBacklash();
                    quest.QuestGiver = Josson;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.OrchardNipperKilled.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(MagicalBacklash)))
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
            if (player.IsDoingQuest(typeof(MagicalBacklash)) != null)
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
            MagicalBacklash quest = player.IsDoingQuest(typeof(MagicalBacklash)) as MagicalBacklash;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.CheckPlayerAbortQuest.Text1"));
            }
            else
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.CheckPlayerAbortQuest.Text2", questTitle));
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

            if (Josson.CanGiveQuest(typeof(MagicalBacklash), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(MagicalBacklash)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!Josson.GiveQuest(typeof(MagicalBacklash), player, 1))
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
                String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.Story");
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
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.Summary");
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.Conclusion.Text1"));
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
            if (player.IsDoingQuest(typeof(MagicalBacklash)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Hib.MagicalBacklash.Notify.Text1")) >= 0)
                    {
                    if (!OrchardNipperKilled.IsAchieved)
                    {
                        OrchardNipperKilled.Advance();
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
