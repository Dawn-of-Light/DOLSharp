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
* Description: The "A New Heroes Welcome" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class ANewHeroesWelcome : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "A New Heroes Welcome";
        protected const int minimumLevel = 1;
        protected const int maximumLevel = 5;

        private static GameNPC MasterClaistan = null;
        private static GameNPC PompinTheCrier = null;

        private QuestGoal pompinsletter;

        private static ItemTemplate LetterToPompin = null;
        private static ItemTemplate RecruitsCloak = null;

        public ANewHeroesWelcome()
            : base()
        {
            Init();
        }

        public ANewHeroesWelcome(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public ANewHeroesWelcome(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public ANewHeroesWelcome(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check
            RecruitsCloak = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_cloak");
            if (RecruitsCloak == null)
            {
                RecruitsCloak = new ItemTemplate();
                RecruitsCloak.Name = "Recruit's Cloak";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsCloak.Name + ", creating it ...");

                RecruitsCloak.Level = 5;
                RecruitsCloak.Weight = 3;
                RecruitsCloak.Model = 443;
                RecruitsCloak.Color = 36;

                RecruitsCloak.Object_Type = (int)eObjectType.Magical;
                RecruitsCloak.Item_Type = (int)eEquipmentItems.CLOAK;
                RecruitsCloak.Id_nb = "k109_recruits_cloak";
                RecruitsCloak.Gold = 0;
                RecruitsCloak.Silver = 0;
                RecruitsCloak.Copper = 0;
                RecruitsCloak.IsPickable = true;
                RecruitsCloak.IsDropable = false; // can't be sold to merchand

                RecruitsCloak.Bonus1 = 6;
                RecruitsCloak.Bonus1Type = (int)eProperty.MaxHealth;

                RecruitsCloak.Quality = 100;
                RecruitsCloak.Condition = 50000;
                RecruitsCloak.MaxCondition = 50000;
                RecruitsCloak.Durability = 50000;
                RecruitsCloak.MaxDurability = 50000;

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsCloak);
            }
            LetterToPompin = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "letter_to_pompin");
            if (LetterToPompin == null)
            {
                LetterToPompin = new ItemTemplate();
                LetterToPompin.Weight = 0;
                LetterToPompin.Condition = 50000;
                LetterToPompin.MaxCondition = 50000;
                LetterToPompin.Model = 499;
                LetterToPompin.Extension = 1;
                LetterToPompin.Name = "Letter To Pompin";
				LetterToPompin.Id_nb = "letter_to_pompin";

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(LetterToPompin);
            }
            #endregion
            Level = 1;
            QuestGiver = MasterClaistan;
            Rewards.Experience = 22;
            Rewards.MoneyPercent = 100;
            Rewards.AddBasicItem(RecruitsCloak);
            Rewards.ChoiceOf = 1;

            pompinsletter = AddGoal("Take the message to Pompin the Crier", QuestGoal.GoalType.ScoutMission, 1, LetterToPompin);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Claistan", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no npc exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                MasterClaistan = new GameNPC();
                MasterClaistan.Model = 33;
                MasterClaistan.Name = "Master Claistan";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + MasterClaistan.Name + ", creating him ...");
                //MasterClaistan.GuildName = "Part of " + questTitle + " Quest";
                MasterClaistan.Realm = (byte)eRealm.Albion;
                MasterClaistan.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
                MasterClaistan.Inventory = template.CloseTemplate();
                MasterClaistan.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                MasterClaistan.Size = 52;
                MasterClaistan.Level = 51;
                MasterClaistan.X = 562190;
                MasterClaistan.Y = 512571;
                MasterClaistan.Z = 2500;
                MasterClaistan.Heading = 1592;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    MasterClaistan.SaveIntoDatabase();

                MasterClaistan.AddToWorld();
            }
            else
                MasterClaistan = npcs[0];

            //Pompin The Crier
            npcs = WorldMgr.GetNPCsByName("Pompin the Crier", eRealm.Albion);
            if (npcs.Length == 0)
            {
                PompinTheCrier = new GameNPC();
                PompinTheCrier.Model = 10;
                PompinTheCrier.Name = "Pompin the Crier";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + PompinTheCrier.Name + ", creating him ...");
                //MasterClaistan.GuildName = "Part of " + questTitle + " Quest";
                PompinTheCrier.Realm = (byte)eRealm.Albion;
                PompinTheCrier.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
                PompinTheCrier.Inventory = template.CloseTemplate();
                PompinTheCrier.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                PompinTheCrier.Size = 50;
                PompinTheCrier.Level = 5;
                PompinTheCrier.X = 560484;
                PompinTheCrier.Y = 511756;
                PompinTheCrier.Z = 2344;
                PompinTheCrier.Heading = 420;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    PompinTheCrier.SaveIntoDatabase();

                PompinTheCrier.AddToWorld();
            }
            else
                PompinTheCrier = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(MasterClaistan, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterClaistan));
            GameEventMgr.AddHandler(MasterClaistan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterClaistan));

            GameEventMgr.AddHandler(PompinTheCrier, GameLivingEvent.Interact, new DOLEventHandler(TalkToPompinTheCrier));
            GameEventMgr.AddHandler(PompinTheCrier, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPompinTheCrier));

            MasterClaistan.AddQuestToGive(typeof(ANewHeroesWelcome));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (MasterClaistan == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(MasterClaistan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterClaistan));
            GameEventMgr.RemoveHandler(MasterClaistan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterClaistan));

            GameEventMgr.RemoveHandler(PompinTheCrier, GameLivingEvent.Interact, new DOLEventHandler(TalkToPompinTheCrier));
            GameEventMgr.RemoveHandler(PompinTheCrier, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPompinTheCrier));

            MasterClaistan.RemoveQuestToGive(typeof(ANewHeroesWelcome));
        }

        protected static void TalkToMasterClaistan(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (MasterClaistan.CanGiveQuest(typeof(ANewHeroesWelcome), player) <= 0)
                return;


            ANewHeroesWelcome quest = player.IsDoingQuest(typeof(ANewHeroesWelcome)) as ANewHeroesWelcome;
            MasterClaistan.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new ANewHeroesWelcome();
                    quest.QuestGiver = MasterClaistan;
                    quest.OfferQuest(player);
                }

            }
        }

        protected static void TalkToPompinTheCrier(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            ANewHeroesWelcome quest = player.IsDoingQuest(typeof(ANewHeroesWelcome)) as ANewHeroesWelcome;
            PompinTheCrier.TurnTo(player);
            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    PompinTheCrier.SayTo(player, "Hello there, welcome to Cotswold.");
                }
                else
                {
                    if (quest.Step == 1)
                    {
                        quest.QuestGiver = PompinTheCrier;
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ANewHeroesWelcome)))
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
            if (player.IsDoingQuest(typeof(ANewHeroesWelcome)) != null)
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
            ANewHeroesWelcome quest = player.IsDoingQuest(typeof(ANewHeroesWelcome)) as ANewHeroesWelcome;

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

            if (MasterClaistan.CanGiveQuest(typeof(ANewHeroesWelcome), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(ANewHeroesWelcome)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.
                if (!MasterClaistan.GiveQuest(typeof(ANewHeroesWelcome), player, 1))
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
                String desc = "Welcome to Cotswold! I see you're one of the latest arrivals to the outskirts of Camelot. If you're here to help with realm defense, you're most welcome. I'm pleased to do my part in readying you for that experience.\n"
                    + "\nKing Constantine's hands are full dealing with the troubles that plague Albion. These are dark times, but we're a stalwart people and are fortunate to have a king that takes the well-being of his people seriously. Still, we're beset by many dangers. There's the threat of invasion by Midgard and Hibernia, precious relics to guard, lands to protect, and forces of evil that dwell below, awaiting the opportunity to strike.\n"
                    + "\nThe realm war is taking a toll on our women and men fighting in the frontlines. The Guild leaders informed us that they're recruiting as may Healers as possible for the Frontiers. I need you to deliver Pompin the Crier a message regarding this news. He'll see to it that this message makes it into the right hands. He may also have a task for you.";
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
                return "Deliver the message from Master Claistan to Pompin the Crier.  He's most knowledgable about current events.  He can fill you in on the latest local news and perhaps give you a task.";
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("Thank you, {0}.",QuestPlayer.CharacterClass.Name);
                text += "I'll send a herald to the healers in Black Mountains South.  This war is depleting our resources and we have much to do on all fronts.  I certainly have more than my share of work.  Let me know if you would like to help out.";
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
    }
}
