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

            // item db check
            RecruitsQuiltedBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_quilted_boots");
            if (RecruitsQuiltedBoots == null)
            {
                RecruitsQuiltedBoots = new ItemTemplate();
                RecruitsQuiltedBoots.Name = "Recruit's Quilted Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsQuiltedBoots.Name + ", creating it ...");

                RecruitsQuiltedBoots.Level = 5;
                RecruitsQuiltedBoots.Weight = 8;
                RecruitsQuiltedBoots.Model = 143;
                RecruitsQuiltedBoots.Color = 28;

                RecruitsQuiltedBoots.Object_Type = (int)eObjectType.Cloth;
                RecruitsQuiltedBoots.Item_Type = (int)eEquipmentItems.FEET;
                RecruitsQuiltedBoots.Id_nb = "k109_recruits_quilted_boots";
                RecruitsQuiltedBoots.Gold = 0;
                RecruitsQuiltedBoots.Silver = 0;
                RecruitsQuiltedBoots.Copper = 40;
                RecruitsQuiltedBoots.IsPickable = true;
                RecruitsQuiltedBoots.IsDropable = true; // can't be sold to merchand

                RecruitsQuiltedBoots.DPS_AF = 6;
                RecruitsQuiltedBoots.SPD_ABS = 0;
                RecruitsQuiltedBoots.Bonus1 = 3;
                RecruitsQuiltedBoots.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsQuiltedBoots.Bonus2 = 3;
                RecruitsQuiltedBoots.Bonus2Type = (int)eProperty.Acuity;
                RecruitsQuiltedBoots.Bonus3 = 1;
                RecruitsQuiltedBoots.Bonus3Type = (int)eProperty.Constitution;
                RecruitsQuiltedBoots.Bonus4 = 1;
                RecruitsQuiltedBoots.Bonus4Type = (int)eProperty.Resist_Thrust;
                RecruitsQuiltedBoots.Bonus5 = 1;
                RecruitsQuiltedBoots.Bonus5Type = (int)eProperty.AllMagicSkills;

                RecruitsQuiltedBoots.Quality = 100;
                RecruitsQuiltedBoots.Condition = 50000;
                RecruitsQuiltedBoots.MaxCondition = 50000;
                RecruitsQuiltedBoots.Durability = 50000;
                RecruitsQuiltedBoots.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsQuiltedBoots);
            }
            RecruitsLeatherBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_leather_boots");
            if (RecruitsLeatherBoots == null)
            {
                RecruitsLeatherBoots = new ItemTemplate();
                RecruitsLeatherBoots.Name = "Recruit's Leather Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsLeatherBoots.Name + ", creating it ...");

                RecruitsLeatherBoots.Level = 5;
                RecruitsLeatherBoots.Weight = 16;
                RecruitsLeatherBoots.Model = 133;
                RecruitsLeatherBoots.Color = 11;

                RecruitsLeatherBoots.Object_Type = (int)eObjectType.Leather;
                RecruitsLeatherBoots.Item_Type = (int)eEquipmentItems.FEET;
                RecruitsLeatherBoots.Id_nb = "k109_recruits_leather_boots";
                RecruitsLeatherBoots.Gold = 0;
                RecruitsLeatherBoots.Silver = 0;
                RecruitsLeatherBoots.Copper = 40;
                RecruitsLeatherBoots.IsPickable = true;
                RecruitsLeatherBoots.IsDropable = true; // can't be sold to merchand

                RecruitsLeatherBoots.DPS_AF = 12;
                RecruitsLeatherBoots.SPD_ABS = 10;
                RecruitsLeatherBoots.Bonus1 = 3;
                RecruitsLeatherBoots.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsLeatherBoots.Bonus2 = 3;
                RecruitsLeatherBoots.Bonus2Type = (int)eProperty.Strength;
                RecruitsLeatherBoots.Bonus3 = 1;
                RecruitsLeatherBoots.Bonus3Type = (int)eProperty.AllMagicSkills;
                RecruitsLeatherBoots.Bonus4 = 1;
                RecruitsLeatherBoots.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                RecruitsLeatherBoots.Bonus5 = 1;
                RecruitsLeatherBoots.Bonus5Type = (int)eProperty.Quickness;
                RecruitsLeatherBoots.Bonus6 = 1;
                RecruitsLeatherBoots.Bonus6Type = (int)eProperty.Resist_Thrust;

                RecruitsLeatherBoots.Quality = 100;
                RecruitsLeatherBoots.Condition = 50000;
                RecruitsLeatherBoots.MaxCondition = 50000;
                RecruitsLeatherBoots.Durability = 50000;
                RecruitsLeatherBoots.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsLeatherBoots);
            }
            RecruitsStuddedBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_studded_boots");
            if (RecruitsStuddedBoots == null)
            {
                RecruitsStuddedBoots = new ItemTemplate();
                RecruitsStuddedBoots.Name = "Recruit's Studded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsStuddedBoots.Name + ", creating it ...");

                RecruitsStuddedBoots.Level = 5;
                RecruitsStuddedBoots.Weight = 24;
                RecruitsStuddedBoots.Model = 84;
                RecruitsStuddedBoots.Color = 11;

                RecruitsStuddedBoots.Object_Type = (int)eObjectType.Studded;
                RecruitsStuddedBoots.Item_Type = (int)eEquipmentItems.FEET;
                RecruitsStuddedBoots.Id_nb = "k109_recruits_studded_boots";
                RecruitsStuddedBoots.Gold = 0;
                RecruitsStuddedBoots.Silver = 0;
                RecruitsStuddedBoots.Copper = 40;
                RecruitsStuddedBoots.IsPickable = true;
                RecruitsStuddedBoots.IsDropable = true; // can't be sold to merchand

                RecruitsStuddedBoots.DPS_AF = 12;
                RecruitsStuddedBoots.SPD_ABS = 19;
                RecruitsStuddedBoots.Bonus1 = 3;
                RecruitsStuddedBoots.Bonus1Type = (int)eProperty.Dexterity;
                RecruitsStuddedBoots.Bonus2 = 3;
                RecruitsStuddedBoots.Bonus2Type = (int)eProperty.Strength;
                RecruitsStuddedBoots.Bonus3 = 1;
                RecruitsStuddedBoots.Bonus3Type = (int)eProperty.AllMagicSkills;
                RecruitsStuddedBoots.Bonus4 = 1;
                RecruitsStuddedBoots.Bonus4Type = (int)eProperty.AllMeleeWeaponSkills;
                RecruitsStuddedBoots.Bonus5 = 1;
                RecruitsStuddedBoots.Bonus5Type = (int)eProperty.Quickness;
                RecruitsStuddedBoots.Bonus6 = 1;
                RecruitsStuddedBoots.Bonus6Type = (int)eProperty.Resist_Thrust;

                RecruitsStuddedBoots.Quality = 100;
                RecruitsStuddedBoots.Condition = 50000;
                RecruitsStuddedBoots.MaxCondition = 50000;
                RecruitsStuddedBoots.Durability = 50000;
                RecruitsStuddedBoots.MaxDurability = 50000;


                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsStuddedBoots);
            }
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
                LadyGrynoch.Name = "LadyGrynoch";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + LadyGrynoch.Name + ", creating her ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //LadyGrynoch.GuildName = "Part of " + questTitle + " Quest";
                LadyGrynoch.Realm = (byte)eRealm.Albion;
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
