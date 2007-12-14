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
* Description: The "Cemmeths Orders" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class CemmethsOrders : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Cemmeth's Orders";
        protected const int minimumLevel = 4;
        protected const int maximumLevel = 7;

        private static GameNPC CemmethBudgwold = null;
        private QuestGoal SkeletonsKilled;
        private QuestGoal DecayingGhoulsKilled;

        private static ItemTemplate RecruitsQuiltedVest = null;
        private static ItemTemplate RecruitsLeatherJerkin = null;
        private static ItemTemplate RecruitsStuddedVest = null;

        public CemmethsOrders()
            : base()
        {
            Init();
        }

        public CemmethsOrders(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public CemmethsOrders(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public CemmethsOrders(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check
            RecruitsQuiltedVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_quilted_vest");
            if (RecruitsQuiltedVest == null)
            {
                RecruitsQuiltedVest = new ItemTemplate();
                RecruitsQuiltedVest.Name = "Recruit's Quilted Vest";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsQuiltedVest.Name + ", creating it ...");

                RecruitsQuiltedVest.Level = 5;
                RecruitsQuiltedVest.Weight = 20;
                RecruitsQuiltedVest.Model = 139;
                RecruitsQuiltedVest.Color = 28;

                RecruitsQuiltedVest.Object_Type = (int)eObjectType.Cloth;
                RecruitsQuiltedVest.Item_Type = (int)eEquipmentItems.TORSO;
                RecruitsQuiltedVest.Id_nb = "k109_recruits_quilted_vest";
                RecruitsQuiltedVest.Gold = 0;
                RecruitsQuiltedVest.Silver = 0;
                RecruitsQuiltedVest.Copper = 40;
                RecruitsQuiltedVest.IsPickable = true;
                RecruitsQuiltedVest.IsDropable = true; // can't be sold to merchand

                RecruitsQuiltedVest.DPS_AF = 6;
                RecruitsQuiltedVest.SPD_ABS = 0;
                RecruitsQuiltedVest.Bonus1 = 3;
                RecruitsQuiltedVest.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsQuiltedVest.Bonus2 = 4;
                RecruitsQuiltedVest.Bonus2Type = (int)eProperty.Acuity;
                RecruitsQuiltedVest.Bonus3 = 1;
                RecruitsQuiltedVest.Bonus3Type = (int)eProperty.Resist_Spirit;
                RecruitsQuiltedVest.Bonus4 = 1;
                RecruitsQuiltedVest.Bonus4Type = (int)eProperty.Resist_Matter;

                RecruitsQuiltedVest.Quality = 100;
                RecruitsQuiltedVest.Condition = 50000;
                RecruitsQuiltedVest.MaxCondition = 50000;
                RecruitsQuiltedVest.Durability = 50000;
                RecruitsQuiltedVest.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsQuiltedVest);
            }
            RecruitsLeatherJerkin = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_leather_jerkin");
            if (RecruitsLeatherJerkin == null)
            {
                RecruitsLeatherJerkin = new ItemTemplate();
                RecruitsLeatherJerkin.Name = "Recruit's Leather Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsLeatherJerkin.Name + ", creating it ...");

                RecruitsLeatherJerkin.Level = 5;
                RecruitsLeatherJerkin.Weight = 40;
                RecruitsLeatherJerkin.Model = 31;
                RecruitsLeatherJerkin.Color = 11;

                RecruitsLeatherJerkin.Object_Type = (int)eObjectType.Leather;
                RecruitsLeatherJerkin.Item_Type = (int)eEquipmentItems.TORSO;
                RecruitsLeatherJerkin.Id_nb = "k109_recruits_leather_jerkin";
                RecruitsLeatherJerkin.Gold = 0;
                RecruitsLeatherJerkin.Silver = 0;
                RecruitsLeatherJerkin.Copper = 40;
                RecruitsLeatherJerkin.IsPickable = true;
                RecruitsLeatherJerkin.IsDropable = true; // can't be sold to merchand

                RecruitsLeatherJerkin.DPS_AF = 12;
                RecruitsLeatherJerkin.SPD_ABS = 10;
                RecruitsLeatherJerkin.Bonus1 = 4;
                RecruitsLeatherJerkin.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsLeatherJerkin.Bonus2 = 3;
                RecruitsLeatherJerkin.Bonus2Type = (int)eProperty.Strength;
                RecruitsLeatherJerkin.Bonus3 = 1;
                RecruitsLeatherJerkin.Bonus3Type = (int)eProperty.Constitution;
                RecruitsLeatherJerkin.Bonus4 = 1;
                RecruitsLeatherJerkin.Bonus4Type = (int)eProperty.Resist_Spirit;

                RecruitsLeatherJerkin.Quality = 100;
                RecruitsLeatherJerkin.Condition = 50000;
                RecruitsLeatherJerkin.MaxCondition = 50000;
                RecruitsLeatherJerkin.Durability = 50000;
                RecruitsLeatherJerkin.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsLeatherJerkin);
            }
            RecruitsStuddedVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_studded_vest");
            if (RecruitsStuddedVest == null)
            {
                RecruitsStuddedVest = new ItemTemplate();
                RecruitsStuddedVest.Name = "Recruit's Studded Vest";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsStuddedVest.Name + ", creating it ...");

                RecruitsStuddedVest.Level = 5;
                RecruitsStuddedVest.Weight = 60;
                RecruitsStuddedVest.Model = 81;
                RecruitsStuddedVest.Color = 11;

                RecruitsStuddedVest.Object_Type = (int)eObjectType.Studded;
                RecruitsStuddedVest.Item_Type = (int)eEquipmentItems.TORSO;
                RecruitsStuddedVest.Id_nb = "k109_recruits_studded_vest";
                RecruitsStuddedVest.Gold = 0;
                RecruitsStuddedVest.Silver = 0;
                RecruitsStuddedVest.Copper = 40;
                RecruitsStuddedVest.IsPickable = true;
                RecruitsStuddedVest.IsDropable = true; // can't be sold to merchand

                RecruitsStuddedVest.DPS_AF = 12;
                RecruitsStuddedVest.SPD_ABS = 19;
                RecruitsStuddedVest.Bonus1 = 1;
                RecruitsStuddedVest.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsStuddedVest.Bonus2 = 4;
                RecruitsStuddedVest.Bonus2Type = (int)eProperty.Strength;
                RecruitsStuddedVest.Bonus3 = 3;
                RecruitsStuddedVest.Bonus3Type = (int)eProperty.Constitution;
                RecruitsStuddedVest.Bonus4 = 1;
                RecruitsStuddedVest.Bonus4Type = (int)eProperty.Resist_Spirit;

                RecruitsStuddedVest.Quality = 100;
                RecruitsStuddedVest.Condition = 50000;
                RecruitsStuddedVest.MaxCondition = 50000;
                RecruitsStuddedVest.Durability = 50000;
                RecruitsStuddedVest.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsStuddedVest);
            }
            #endregion

            //Skeleton Skull
            ItemTemplate skeletonskull = new ItemTemplate();
            skeletonskull.Weight = 0;
            skeletonskull.Condition = 50000;
            skeletonskull.MaxCondition = 50000;
            skeletonskull.Model = 540;
            skeletonskull.Extension = 1;
            skeletonskull.Name = "Skeleton Skull";

            ItemTemplate zombieskin = new ItemTemplate();
            zombieskin.Weight = 0;
            zombieskin.Condition = 50000;
            zombieskin.MaxCondition = 50000;
            zombieskin.Model = 540;
            zombieskin.Extension = 1;
            zombieskin.Name = "Decayed Zombie Skin";

            QuestGiver = CemmethBudgwold;
            Rewards.Experience = 625;
            Rewards.MoneyPercent = 100;
            Rewards.AddOptionalItem(RecruitsQuiltedVest);
            Rewards.AddOptionalItem(RecruitsLeatherJerkin);
            Rewards.AddOptionalItem(RecruitsStuddedVest);
            Rewards.ChoiceOf = 1;

            SkeletonsKilled = AddGoal("Kill 1 Skeleton", QuestGoal.GoalType.KillTask, 1, skeletonskull);
            DecayingGhoulsKilled = AddGoal("Kill 1 Decayed Zombie", QuestGoal.GoalType.KillTask, 1, zombieskin);

        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Cemmeth Budgwold", eRealm.Albion);

            if (npcs.Length == 0)
            {
                CemmethBudgwold = new GameNPC();
                CemmethBudgwold.Model = 28;
                CemmethBudgwold.Name = "Cemmeth Budgwold";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + CemmethBudgwold.Name + ", creating him ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //Cemmeth.GuildName = "Part of " + questTitle + " Quest";
                CemmethBudgwold.Realm = (byte)eRealm.Albion;
                CemmethBudgwold.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);    //Slot 22
                template.AddNPCEquipment(eInventorySlot.HeadArmor, 93);     //Slot 21
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);     //Slot 23
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);    //Slot 25
                template.AddNPCEquipment(eInventorySlot.Cloak, 91);         //Slot 26
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);     //Slot 27
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);     //Slot 28
                template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 68); //Slot 12
                CemmethBudgwold.Inventory = template.CloseTemplate();
                CemmethBudgwold.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                CemmethBudgwold.Size = 50;
                CemmethBudgwold.Level = 38;
                CemmethBudgwold.X = 560528;
                CemmethBudgwold.Y = 513140;
                CemmethBudgwold.Z = 2394;
                CemmethBudgwold.Heading = 2275;

                if (SAVE_INTO_DATABASE)
                    CemmethBudgwold.SaveIntoDatabase();

                CemmethBudgwold.AddToWorld();
            }
            else
                CemmethBudgwold = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(CemmethBudgwold, GameLivingEvent.Interact, new DOLEventHandler(TalkToCemmethBudgwold));
            GameEventMgr.AddHandler(CemmethBudgwold, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCemmethBudgwold));

            CemmethBudgwold.AddQuestToGive(typeof(CemmethsOrders));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (CemmethBudgwold == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(CemmethBudgwold, GameObjectEvent.Interact, new DOLEventHandler(TalkToCemmethBudgwold));
            GameEventMgr.RemoveHandler(CemmethBudgwold, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCemmethBudgwold));

            CemmethBudgwold.RemoveQuestToGive(typeof(CemmethsOrders));
        }

        protected static void TalkToCemmethBudgwold(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (CemmethBudgwold.CanGiveQuest(typeof(CemmethsOrders), player) <= 0)
                return;


            CemmethsOrders quest = player.IsDoingQuest(typeof(CemmethsOrders)) as CemmethsOrders;
            CemmethBudgwold.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new CemmethsOrders();
                    quest.QuestGiver = CemmethBudgwold;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.SkeletonsKilled.IsAchieved && quest.DecayingGhoulsKilled.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(CemmethsOrders)))
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
            if (player.IsDoingQuest(typeof(CemmethsOrders)) != null)
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
            CemmethsOrders quest = player.IsDoingQuest(typeof(CemmethsOrders)) as CemmethsOrders;

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

            if (CemmethBudgwold.CanGiveQuest(typeof(CemmethsOrders), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(CemmethsOrders)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!CemmethBudgwold.GiveQuest(typeof(CemmethsOrders), player, 1))
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
                String desc = "Brother Lawrence recently called a meeting with the local militia leaders to discuss his concerns about the rise in the number of undead in our region,"
                    + "and how it may relate to the recent incident leading to the forming of the entrances into Demons' Breach. I must say, his concerns trouble me."
                    + "I don't pretend to know much about the how or why of it, but I do know that if these creatures pose a threat to our citizens, they must be put down.\n\n"
                    + "According to our reports, skeletons and decayed zombies are apparently two of the more dangerous things to worry about near Prydwen Ossuary."
                    + "Their numbers and aggressive posture seem to be increasing. I fear that if they go unchallenged, we may find ourselves in quite a predicament."
                    + "We mustn't have the undead legions gallivanting about Camelot Hills, after all!\n\n"
                    + "What say you?  Will you help to make our countryside safer by sending those vermin back to their resting places?"
                    + "Bring me proof of your deed should you embark upon this task.\n\n";
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
                return "Travel to Prydwen Ossuary. There are a variety of undead in the surrounding area. Kill a skeleton for a skull and a decayed zombie for some flesh. Return to Cemmeth Budgwold when you're done.";
            }
        }

        /// <summary>
        /// The goal of the quest, keeps track of kill counts etc.
        /// </summary>
        //public override string Goal
        //{
        //    get
        //    {

        //        //return String.Format("Kill 1 skeleton. ({0}/1)", SkeletonsKilled);
        //        return String.Format("Kill 1 decayed zombie. ({0}/1)", DecayingGhoulsKilled);
        //    }
        //}

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("");
                text += "Well done, " + QuestPlayer.CharacterClass.Name + ", well done!  We've certainly got our work cut out for us, what with all the unliving vermin stomping about.  You've set a fine example for the other would-be heroes and adventurers, my friend.  Please accept this small token of appreciation.";
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
            if (player.IsDoingQuest(typeof(CemmethsOrders)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("skeleton") >= 0)
                {
                    if (!SkeletonsKilled.IsAchieved)
                    {
                        SkeletonsKilled.Advance();
                        return;
                    }
                }
                if (gArgs.Target.Name.IndexOf("decayed zombie") >= 0)
                {
                    if (!DecayingGhoulsKilled.IsAchieved)
                    {
                        DecayingGhoulsKilled.Advance();
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
