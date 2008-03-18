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
* Description: The "Tomb With A View" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class TombWithAView : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Tomb With A View";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC LadyGrynoch = null;
        private QuestGoal FoundTomb;

        private static GameLocation Burial_Tomb = new GameLocation("Burial Tomb", 1, 562679, 517225, 2935);

        private static IArea Burial_Tomb_Area = null;

        private static ItemTemplate RecruitsQuiltedBoots = null;
        private static ItemTemplate RecruitsLeatherBoots = null;
        private static ItemTemplate RecruitsStuddedBoots = null;

        public TombWithAView()
            : base()
        {
            Init();
        }

        public TombWithAView(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public TombWithAView(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public TombWithAView(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems
            RecruitsQuiltedBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_quilted_boots");
            RecruitsLeatherBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_leather_boots");
            RecruitsStuddedBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_studded_boots");
            #endregion

            Level = 2;
            QuestGiver = LadyGrynoch;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 50;
            Rewards.AddOptionalItem(RecruitsQuiltedBoots);
            Rewards.AddOptionalItem(RecruitsLeatherBoots);
            Rewards.AddOptionalItem(RecruitsStuddedBoots);
            Rewards.ChoiceOf = 1;

            FoundTomb = AddGoal("Find the entrance to the Burial Tomb", QuestGoal.GoalType.ScoutMission, 1,null);

        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Grynoch", eRealm.Albion);

            if (npcs.Length == 0)
            {
                LadyGrynoch = new GameNPC();
                LadyGrynoch.Model = 5;
                LadyGrynoch.Name = "Lady Grynoch";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + LadyGrynoch.Name + ", creating her ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //LadyGrynoch.GuildName = "Part of " + questTitle + " Quest";
                LadyGrynoch.Realm = eRealm.Albion;
                LadyGrynoch.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);    //Slot 25
                LadyGrynoch.Inventory = template.CloseTemplate();
                LadyGrynoch.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                LadyGrynoch.Size = 51;
                LadyGrynoch.Level = 38;
                LadyGrynoch.X = 559698;
                LadyGrynoch.Y = 513578;
                LadyGrynoch.Z = 2428;
                LadyGrynoch.Heading = 2742;

                if (SAVE_INTO_DATABASE)
                    LadyGrynoch.SaveIntoDatabase();

                LadyGrynoch.AddToWorld();
            }
            else
                LadyGrynoch = npcs[0];

            #endregion
            #region defineAreas
            Burial_Tomb_Area = WorldMgr.GetRegion(Burial_Tomb.RegionID).AddArea(new Area.Circle("", Burial_Tomb.X, Burial_Tomb.Y, Burial_Tomb.Z, 200));
            Burial_Tomb_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterBurialTombArea));
             #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
            GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

            LadyGrynoch.AddQuestToGive(typeof(TombWithAView));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (LadyGrynoch == null)
                return;

            Burial_Tomb_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterBurialTombArea));
            WorldMgr.GetRegion(Burial_Tomb.RegionID).RemoveArea(Burial_Tomb_Area);

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(LadyGrynoch, GameObjectEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
            GameEventMgr.RemoveHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

            LadyGrynoch.RemoveQuestToGive(typeof(TombWithAView));
        }

        protected static void TalkToLadyGrynoch(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (LadyGrynoch.CanGiveQuest(typeof(TombWithAView), player) <= 0)
                return;


            TombWithAView quest = player.IsDoingQuest(typeof(TombWithAView)) as TombWithAView;
            LadyGrynoch.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new TombWithAView();
                    quest.QuestGiver = LadyGrynoch;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.FoundTomb.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(TombWithAView)))
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
            if (player.IsDoingQuest(typeof(TombWithAView)) != null)
                return true;

            // This checks below are only performed is player isn't doing quest already

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            TombWithAView quest = player.IsDoingQuest(typeof(TombWithAView)) as TombWithAView;

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

            if (LadyGrynoch.CanGiveQuest(typeof(TombWithAView), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(TombWithAView)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!LadyGrynoch.GiveQuest(typeof(TombWithAView), player, 1))
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
                String desc = "What an exhausting day! The Academy sent me to Cotswold to be an advisor on matters of magic and such, and I'm happy to be of service, but there are days when my research takes a lot out of me. It seems like I've been a bit busier lately. Cotswold has become a hub of many goings-on lately, some of which are mundane and some are of a more diabolical nature.\n\n"
                    + "Take, for example, the nearby Burial Tomb, to our south-southeast. The Romans that occupied this region of Britain had a penchant for crypts, tombs, mausoleums and the like. Over the course of hundreds of years, the nearby tombs became home to hundreds of fearsome creatures, many of whom took up residence below, away from daylight and men above. Some of these previously normal creatures are twisted versions of themselves. By what foul sorcery, can only be guessed.\n\n"
                    + "Taskmaster Traint is seeking out volunteers to eradicate the creatures that moved into the tomb. You'll want to make yourself familiar with where to find it.\n\n";
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
                return "Find the entrance to the Burial Tomb. Return to Lady Grynoch once you've visited the Tomb.";
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
                text += "Ah, found your way to the Burial Tomb, did you?  'Tis a fearsome place, to be sure!  Now that you know where to find the tomb, you'll know where to go, if and when Taskmaster Traint tasks you with helping clear it out.";
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
        protected static void PlayerEnterBurialTombArea(DOLEvent e, object sender, EventArgs args)
        {
            
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(TombWithAView)) == null)
                return;

            TombWithAView quest = player.IsDoingQuest(typeof(TombWithAView)) as TombWithAView;
            
            if (quest.Step ==1 && !quest.FoundTomb.IsAchieved)
            {
                quest.FoundTomb.Advance();
                return;
            }
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

        }
    }
}
