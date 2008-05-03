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
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Talk with Lady Grynoch in Cotswold Village
 * 2) Find the Entrance to the Demon's Breach and then head back to her.  
 * You will receive some xp, copper and the armor of your choice.
 */
using System;
using System.Reflection;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class ToReachTheBreach : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "To Reach the Breach";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC LadyGrynoch = null;
        private QuestGoal FoundBreach;

        private static GameLocation Demons_Breach = new GameLocation("Demon's Breach", 1, 562731, 514531, 2751);

        private static IArea Demons_Breach_Area = null;

        private static ItemTemplate RecruitsIntelligentBelt;
        private static ItemTemplate RecruitsMightyBelt;
        private static ItemTemplate RecruitsPiousBelt;

        public ToReachTheBreach()
            : base()
        {
            Init();
        }

        public ToReachTheBreach(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public ToReachTheBreach(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public ToReachTheBreach(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
          #region defineItems
          RecruitsIntelligentBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate),"RecruitsIntelligentBelt");
          if(RecruitsIntelligentBelt == null)
          {
              RecruitsIntelligentBelt = new ItemTemplate();
              RecruitsIntelligentBelt.Name = "Recruit's Intelligent Belt";
              RecruitsIntelligentBelt.Id_nb = "RecruitsIntelligentBelt";
              RecruitsIntelligentBelt.Level = 4;
              RecruitsIntelligentBelt.Weight = 3;
              RecruitsIntelligentBelt.Model = 597;
              RecruitsIntelligentBelt.Color = 0;
              RecruitsIntelligentBelt.Object_Type = (int)eObjectType.GenericArmor;
              RecruitsIntelligentBelt.Item_Type = (int)eEquipmentItems.WAIST;
              RecruitsIntelligentBelt.Gold = 0;
              RecruitsIntelligentBelt.Silver = 0;
              RecruitsIntelligentBelt.Copper = 10;
              RecruitsIntelligentBelt.Quality = 100;
              RecruitsIntelligentBelt.Condition = 50000;
              RecruitsIntelligentBelt.MaxCondition = 50000;
              RecruitsIntelligentBelt.Durability = 50000;
              RecruitsIntelligentBelt.MaxDurability = 50000;
              RecruitsIntelligentBelt.Bonus1 = 3;
              RecruitsIntelligentBelt.Bonus1Type = (int)eProperty.Acuity ;
              RecruitsIntelligentBelt.Bonus2 = 3;
              RecruitsIntelligentBelt.Bonus2Type = (int)eProperty.Dexterity;
              RecruitsIntelligentBelt.Bonus3 = 1;
              RecruitsIntelligentBelt.Bonus3Type = (int)eProperty.Constitution;
              RecruitsIntelligentBelt.Bonus4 = 8;
              RecruitsIntelligentBelt.Bonus4Type = (int)eProperty.MaxHealth;
              RecruitsIntelligentBelt.IsDropable = false;
          }
          RecruitsMightyBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate),"RecruitsMightyBelt");
          if(RecruitsMightyBelt == null)
          {
              RecruitsMightyBelt = new ItemTemplate();
              RecruitsMightyBelt.Name = "Recruit's Mighty Belt";
              RecruitsMightyBelt.Id_nb = "RecruitsMightyBelt";
              RecruitsMightyBelt.Level = 4;
              RecruitsMightyBelt.Weight = 3;
              RecruitsMightyBelt.Model = 597;
              RecruitsMightyBelt.Color = 0;
              RecruitsMightyBelt.Object_Type = (int)eObjectType.GenericArmor;
              RecruitsMightyBelt.Item_Type = (int)eEquipmentItems.WAIST;
              RecruitsMightyBelt.Gold = 0;
              RecruitsMightyBelt.Silver = 0;
              RecruitsMightyBelt.Copper = 10;
              RecruitsMightyBelt.Quality = 100;
              RecruitsMightyBelt.Condition = 50000;
              RecruitsMightyBelt.MaxCondition = 50000;
              RecruitsMightyBelt.Durability = 50000;
              RecruitsMightyBelt.MaxDurability = 50000;
              RecruitsMightyBelt.Bonus1 = 3;
              RecruitsMightyBelt.Bonus1Type = (int)eProperty.Strength;
              RecruitsMightyBelt.Bonus2 = 3;
              RecruitsMightyBelt.Bonus2Type = (int)eProperty.Dexterity;
              RecruitsMightyBelt.Bonus3 = 1;
              RecruitsMightyBelt.Bonus3Type = (int)eProperty.Quickness;
              RecruitsMightyBelt.Bonus4 = 8;
              RecruitsMightyBelt.Bonus4Type = (int)eProperty.MaxHealth;
              RecruitsMightyBelt.IsDropable = false;
          }
          RecruitsPiousBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate),"RecruitsPiousBelt");
          if(RecruitsPiousBelt == null)
          {
              RecruitsPiousBelt = new ItemTemplate();
              RecruitsPiousBelt.Name = "Recruit's Pious Belt";
              RecruitsPiousBelt.Id_nb = "RecruitsPiousBelt";
              RecruitsPiousBelt.Level = 4;
              RecruitsPiousBelt.Weight = 3;
              RecruitsPiousBelt.Model = 597;
              RecruitsPiousBelt.Color = 0;
              RecruitsPiousBelt.Object_Type = (int)eObjectType.GenericArmor;
              RecruitsPiousBelt.Item_Type = (int)eEquipmentItems.WAIST;
              RecruitsPiousBelt.Gold = 0;
              RecruitsPiousBelt.Silver = 0;
              RecruitsPiousBelt.Copper = 10;
              RecruitsPiousBelt.Quality = 100;
              RecruitsPiousBelt.Condition = 50000;
              RecruitsPiousBelt.MaxCondition = 50000;
              RecruitsPiousBelt.Durability = 50000;
              RecruitsPiousBelt.MaxDurability = 50000;
              RecruitsPiousBelt.Bonus1 = 3;
              RecruitsPiousBelt.Bonus1Type = (int)eProperty.Acuity;
              RecruitsPiousBelt.Bonus2 = 1;
              RecruitsPiousBelt.Bonus2Type = (int)eProperty.AllMagicSkills;
              RecruitsPiousBelt.Bonus3 = 3;
              RecruitsPiousBelt.Bonus3Type = (int)eProperty.Dexterity;
              RecruitsPiousBelt.Bonus4 = 8;
              RecruitsPiousBelt.Bonus4Type = (int)eProperty.MaxHealth;
              RecruitsPiousBelt.IsDropable = false;
          }
            #endregion

            Level = 3;
            QuestGiver = LadyGrynoch;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 100;
            Rewards.AddOptionalItem(RecruitsIntelligentBelt);
            Rewards.AddOptionalItem(RecruitsMightyBelt);
            Rewards.AddOptionalItem(RecruitsPiousBelt);
            Rewards.ChoiceOf = 1;

            FoundBreach = AddGoal("Find the entrance to the Demon's Breach", QuestGoal.GoalType.ScoutMission, 1, null);

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
            Demons_Breach_Area = WorldMgr.GetRegion(Demons_Breach.RegionID).AddArea(new Area.Circle("", Demons_Breach.X, Demons_Breach.Y, Demons_Breach.Z, 200));
            Demons_Breach_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
            GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

            LadyGrynoch.AddQuestToGive(typeof(ToReachTheBreach));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (LadyGrynoch == null)
                return;

            Demons_Breach_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
            WorldMgr.GetRegion(Demons_Breach.RegionID).RemoveArea(Demons_Breach_Area);

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(LadyGrynoch, GameObjectEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
            GameEventMgr.RemoveHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

            LadyGrynoch.RemoveQuestToGive(typeof(ToReachTheBreach));
        }

        protected static void TalkToLadyGrynoch(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (LadyGrynoch.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
                return;


            ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;
            LadyGrynoch.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new ToReachTheBreach();
                    quest.QuestGiver = LadyGrynoch;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.FoundBreach.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ToReachTheBreach)))
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
            if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
                return true;

            // This checks below are only performed is player isn't doing quest already

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

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

            if (LadyGrynoch.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!LadyGrynoch.GiveQuest(typeof(ToReachTheBreach), player, 1))
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
                String desc = "Long have we known that demonic creatures lurk below.  Darkness Falls is but one example.  Recently, more evil managed to bubble to the surface.  When the Headmasters of the Academy moved the Shrouded Isles portal from it's former home to Cotswold, they inadvertently created a tear in the fabric of reality.  This tear allowed evil to claws it's way to the surface!\n\n"
                    + "Luckily, this was noticed immediately by a Sorceress involved in the project, and the hole was capped, disallowing entry by the demons into our world.  Until a permanent solution is found, there is worry that they will amass their numbers in an effort to break free.  Some of our townsfolk have taken it upon themselves to venture inside to reduce the number of demons.  Alarmingly, there are reports of encountering our enemies from Hibernia and Midgard within, as well.\n\n"
                    + "You must familiarize yourself with the entrance to Demon's Breach, so that you will know where to go to keep the forces of evil, Hibernia and Midgard at bay.\n\n";
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
                return "Find the entrance to the Demon's Breach. Return to Lady Grynoch once you've visited the Breach.";
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("You found your way to Demon's Breach, eh?  Mark it's location well, {0}",QuestPlayer.CharacterClass.Name);
                text += ", for your assistance may be needed there one day.  We can't afford to allow hordes of demons, Hibernians or Midgardians to break free into our lands.";
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
        protected static void PlayerEnterDemonBreachArea(DOLEvent e, object sender, EventArgs args)
        {
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(ToReachTheBreach)) == null)
                return;

            ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

            if (quest.Step == 1 && quest.FoundBreach.IsAchieved == false)
            {
                quest.FoundBreach.Advance();
                return;
            }
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

        }
    }
}
