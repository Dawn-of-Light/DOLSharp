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
* Description: The "When Good Brownies Go Bad" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class WhenGoodBrowniesGoBad : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "When Good Brownies Go Bad";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC MasterKless = null;
        private QuestGoal browniesKilled;

        private static ItemTemplate lightredclothdye = null;
        private static ItemTemplate lightredleatherdye = null;
        private static ItemTemplate lightredenamel = null;
        public WhenGoodBrowniesGoBad()
            : base()
        {
            Init();
        }

        public WhenGoodBrowniesGoBad(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public WhenGoodBrowniesGoBad(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public WhenGoodBrowniesGoBad(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check
            lightredclothdye = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "light_red_cloth_dye");
            if (lightredclothdye == null)
            {
                lightredclothdye = new ItemTemplate();
                lightredclothdye.Name = "Light Red Cloth Dye";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + lightredclothdye.Name + ", creating it ...");

                lightredclothdye.Level = 1;
                lightredclothdye.Weight = 0;
                lightredclothdye.Model = 229;
                lightredclothdye.Color = 0;

                lightredclothdye.Object_Type = (int)eObjectType.GenericItem;
                lightredclothdye.TemplateID = "light_red_cloth_dye";
                lightredclothdye.Gold = 0;
                lightredclothdye.Silver = 0;
                lightredclothdye.Copper = 40;
                lightredclothdye.IsPickable = true;
                lightredclothdye.IsDropable = true; // can't be sold to merchand
                lightredclothdye.Quality = 100;
                lightredclothdye.Condition = 50000;
                lightredclothdye.MaxCondition = 50000;
                lightredclothdye.Durability = 50000;
                lightredclothdye.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(lightredclothdye);
            }
            lightredleatherdye = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "light_red_leather_dye");
            if (lightredleatherdye == null)
            {
                lightredleatherdye = new ItemTemplate();
                lightredleatherdye.Name = "Light Red Leather Dye";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + lightredleatherdye.Name + ", creating it ...");

                lightredleatherdye.Level = 1;
                lightredleatherdye.Weight = 0;
                lightredleatherdye.Model = 229;
                lightredleatherdye.Color = 0;

                lightredleatherdye.Object_Type = (int)eObjectType.GenericItem;
                lightredleatherdye.TemplateID = "light_red_leather_dye";
                lightredleatherdye.Gold = 0;
                lightredleatherdye.Silver = 0;
                lightredleatherdye.Copper = 40;
                lightredleatherdye.IsPickable = true;
                lightredleatherdye.IsDropable = true; // can't be sold to merchand
                lightredleatherdye.Quality = 100;
                lightredleatherdye.Condition = 50000;
                lightredleatherdye.MaxCondition = 50000;
                lightredleatherdye.Durability = 50000;
                lightredleatherdye.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(lightredleatherdye);
            }
            lightredenamel = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "light_red_enamel");
            if (lightredenamel == null)
            {
                lightredenamel = new ItemTemplate();
                lightredenamel.Name = "Light Red Enamel";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + lightredenamel.Name + ", creating it ...");

                lightredenamel.Level = 1;
                lightredenamel.Weight = 0;
                lightredenamel.Model = 229;
                lightredenamel.Color = 0;

                lightredenamel.Object_Type = (int)eObjectType.GenericItem;
                lightredenamel.TemplateID = "light_red_enamel";
                lightredenamel.Gold = 0;
                lightredenamel.Silver = 0;
                lightredenamel.Copper = 40;
                lightredenamel.IsPickable = true;
                lightredenamel.IsDropable = true; // can't be sold to merchand
                lightredenamel.Quality = 100;
                lightredenamel.Condition = 50000;
                lightredenamel.MaxCondition = 50000;
                lightredenamel.Durability = 50000;
                lightredenamel.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(lightredenamel);
            }
            #endregion

            ItemTemplate brownieBlood = new ItemTemplate();
            brownieBlood.Weight = 0;
            brownieBlood.Condition = 50000;
            brownieBlood.MaxCondition = 50000;
            brownieBlood.Model = 99;
            brownieBlood.Extension = 1;
            brownieBlood.Name = "Brownie Blood";

            Level = 1;
            QuestGiver = MasterKless;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 80;
            Rewards.AddOptionalItem(lightredclothdye);
            Rewards.AddOptionalItem(lightredleatherdye);
            Rewards.AddOptionalItem(lightredenamel);
            Rewards.ChoiceOf = 1;
            browniesKilled = AddGoal("Kill 3 brownies", QuestGoal.GoalType.KillTask, 3, brownieBlood);

        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Kless", eRealm.Albion);

            if (npcs.Length == 0)
            {
                MasterKless = new GameNPC();
                MasterKless.Model = 64;
                MasterKless.Name = "Master Kless";
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

            MasterKless.AddQuestToGive(typeof(WhenGoodBrowniesGoBad));

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

            MasterKless.RemoveQuestToGive(typeof(WhenGoodBrowniesGoBad));
        }

        protected static void TalkToMasterKless(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (MasterKless.CanGiveQuest(typeof(WhenGoodBrowniesGoBad), player) <= 0)
                return;


            WhenGoodBrowniesGoBad quest = player.IsDoingQuest(typeof(WhenGoodBrowniesGoBad)) as WhenGoodBrowniesGoBad;
            MasterKless.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new WhenGoodBrowniesGoBad();
                    quest.QuestGiver = MasterKless;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.browniesKilled.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(WhenGoodBrowniesGoBad)))
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
            if (player.IsDoingQuest(typeof(WhenGoodBrowniesGoBad)) != null)
                return true;

            // This checks below are only performed is player isn't doing quest already

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            WhenGoodBrowniesGoBad quest = player.IsDoingQuest(typeof(WhenGoodBrowniesGoBad)) as WhenGoodBrowniesGoBad;

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

        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            // We recheck the qualification, because we don't talk to players
            // who are not doing the quest.

            if (MasterKless.CanGiveQuest(typeof(WhenGoodBrowniesGoBad), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(WhenGoodBrowniesGoBad)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!MasterKless.GiveQuest(typeof(WhenGoodBrowniesGoBad), player, 1))
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
                String desc = "I'm troubled by recent changes in the few fae creatures that are left in this densely-settled region."
                    + "While I'm sure they've no love for our kind, after being forced to go into hiding - or else run the risk of"
                    + "attracting the attention of the Church - they've seldom been a menace.  Meddlesome, yes, but seldom have they been malevolent.\n\n"
                    + "Recently, however, some villagers have spoken of having encounters with members of the wee folk that seem out of the ordinary;"
                    + "almost sinister, even. Children being abducted, farmers being attacked without provocation, foodstuffs being poisoned,"
                    + "livestock being slaughtered in their pens at night; it's almost as if they are under some kind of enchantment, or something."
                    + "This is not like them. I might not be a follower of the old ways, but I'm no religious zealot, either. If King Arthur found"
                    + "it in his heart to respect the old ways, then so shall I.\n\n  I must study this further, and I could use your help, if you'd be kind enough to assist me.";
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
                return "Master Kless needs brownie blood to use in a spell that will help determine if they are bespelled.  Kill three brownies, and return to him with their blood.";
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("");
                text += "Ah, you've returned with the blood! While I hate to see harm done to any creature not inherently evil, it's becoming necessary to take drastic measures to get to the root of this problem. Hopefully, I shall. Please accept this reward for your aid.";
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
            if (player.IsDoingQuest(typeof(WhenGoodBrowniesGoBad)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("brownie") >= 0)
                {
                    if (!browniesKilled.IsAchieved)
                    {
                        browniesKilled.Advance();
                        return;
                    }
                }
            }
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

        }
    }
}
