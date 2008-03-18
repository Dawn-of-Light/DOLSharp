
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

/*
* Author:	Lamyuras
* Edited by: k109
* Date:		11/26/07
*
* Notes: Changed this quest to work like live server.
* UPDATE:  You must edit the Database if you want this quest to work correctly.
* remove any reference in the DB to "Statue Demons Breach", there will be 200+ if using rev 818DB
* also run this script: update mob set aggrolevel = 0 where flags = 12 and region = 489;
* the ambient corpses are all agro, and will attack if you get to close.
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.AI.Brain;

namespace DOL.GS.Quests.Albion
{
    public class childsplay : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        ///
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Child's Play";

        protected const int minimumLevel = 1;
        protected const int maximumLevel = 4;

        private static GameNPC Charles = null;
        private static ItemTemplate daringpaddedboots_alb = null;
        private static ItemTemplate daringpaddedcap_alb = null;
        private static ItemTemplate daringpaddedgloves_alb = null;
        private static ItemTemplate daringpaddedpants_alb = null;
        private static ItemTemplate daringpaddedsleeves_alb = null;
        private static ItemTemplate daringpaddedvest_alb = null;
        private static ItemTemplate daringleatherboots_alb = null;
        private static ItemTemplate daringleathercap_alb = null;
        private static ItemTemplate daringleathergloves_alb = null;
        private static ItemTemplate daringleatherjerkin_alb = null;
        private static ItemTemplate daringleatherleggings_alb = null;
        private static ItemTemplate daringleathersleeves_alb = null;
        private static ItemTemplate daringstuddedboots_alb = null;
        private static ItemTemplate daringstuddedcap_alb = null;
        private static ItemTemplate daringstuddedgloves_alb = null;
        private static ItemTemplate daringstuddedjerkin_alb = null;
        private static ItemTemplate daringstuddedleggings_alb = null;
        private static ItemTemplate daringstuddedsleeves_alb = null;
        private static GameLocation Albion_Statue = new GameLocation("Childs Play (Alb)", 489, 27580, 40006, 14483);

        private static IArea Albion_Statue_Area = null;

        public childsplay()
            : base()
        {
        }

        public childsplay(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public childsplay(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public childsplay(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" (Alb) initializing ...");

            #region defineNPCs
            GameNPC[] npcs;

            npcs = WorldMgr.GetNPCsByName("Charles", (eRealm)1);
            if (npcs.Length == 0)
            {
                Charles = new DOL.GS.GameNPC();
                Charles.Model = 92;
                Charles.Name = "Charles";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Charles.Name + ", creating ...");
                Charles.Realm = eRealm.Albion;
                Charles.CurrentRegionID = 1;
                Charles.Size = 37;
                Charles.Level = 1;
                Charles.MaxSpeedBase = 191;
                Charles.Faction = FactionMgr.GetFactionByID(0);
                Charles.X = 559883;
                Charles.Y = 511489;
                Charles.Z = 2382;
                Charles.Heading = 3515;
                Charles.RespawnInterval = -1;
                Charles.BodyType = 0;


                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 500;
                Charles.SetOwnBrain(brain);

                if (SAVE_INTO_DATABASE)
                    Charles.SaveIntoDatabase();

                Charles.AddToWorld();

            }
            else
            {
                Charles = npcs[0];
            }


            #endregion

            #region defineItems
            daringpaddedboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedboots_alb");
            daringpaddedcap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedcap_alb");
            daringpaddedgloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedgloves_alb");
            daringpaddedpants_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedpants_alb");
            daringpaddedsleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedsleeves_alb");
            daringpaddedvest_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedvest_alb");
            daringleatherboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherboots_alb");
            daringleathercap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathercap_alb");
            daringleathergloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathergloves_alb");
            daringleatherjerkin_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherjerkin_alb");
            daringleatherleggings_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherleggings_alb");
            daringleathersleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathersleeves_alb");
            daringstuddedboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedboots_alb");
            daringstuddedcap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedcap_alb");
            daringstuddedgloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedgloves_alb");
            daringstuddedjerkin_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedjerkin_alb");
            daringstuddedleggings_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedleggings_alb");
            daringstuddedsleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedsleeves_alb");
            #endregion

            #region defineAreas
            Albion_Statue_Area = WorldMgr.GetRegion(Albion_Statue.RegionID).AddArea(new Area.Circle("", Albion_Statue.X, Albion_Statue.Y, Albion_Statue.Z, 500));
            Albion_Statue_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Charles, GameLivingEvent.Interact, new DOLEventHandler(TalkToCharles));
            GameEventMgr.AddHandler(Charles, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCharles));

            Charles.AddQuestToGive(typeof(childsplay));
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {

            // Custom Scriptunloaded Code Begin

            // Custom Scriptunloaded Code End

            Albion_Statue_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));
            WorldMgr.GetRegion(Albion_Statue.RegionID).RemoveArea(Albion_Statue_Area);
            if (Charles == null)
                return;
            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Charles, GameLivingEvent.Interact, new DOLEventHandler(TalkToCharles));
            GameEventMgr.RemoveHandler(Charles, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCharles));

            Charles.RemoveQuestToGive(typeof(childsplay));
        }

        protected static void TalkToCharles(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Charles.CanGiveQuest(typeof(childsplay), player) <= 0)
                return;

            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            Charles.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {

                    Charles.SayTo(player, "Hello there, "+ player.CharacterClass.BaseName +". Have you ever gotten tired of playing the same game over and over? That's what it's like here. All the adults say we can do is sit here and play 'Truth or Dare.' Sure, it was fun at first, but most of us have gotten bored and Raymond's too chicken to do any of the [dares] I think of for him.");
                    return;
                }
                if (quest.Step == 2)
                {
                    Charles.SayTo(player, "You made it back alive! I mean, uh, welcome back! How was the statue? Did you get scared? Does this mean you're going to stay and play the game with us again? If you are, you can have these. I don't need them anymore.");

                    //k109:  Until I can get the quest dialog from live, I reward based on class, feel free to edit.
                    player.Out.SendMessage(" You have completed the Childs Play quest.", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                    if (player.CharacterClass.BaseName == "Fighter")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedsleeves_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == "Mage")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == "Disciple")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == "Elementalist")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == "Rogue")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == "Acolyte")
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_alb);
                        quest.FinishQuest();
                    }
                }
            }
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                if (quest == null)
                {
                    //k109:  This is the "old" way of doing quests, by clicking on keywords, but have to use this until I can get the new quest dialog window.
                    switch (wArgs.Text)
                    {
                        case "dares":
                            Charles.SayTo(player, "It was a lot more fun when we could play 'Spin the Elixir' in the tavern basement with the girls, but Tiff's parents caught us there and we've been treated like babies ever since. I bet this game would be a whole lot better if you could [join] us. I'd even kick Raymond out and think up the bestest dare ever, just for you.");
                            break;

                        case "join":
                            Charles.SayTo(player, "Hmm. Give me a minute. I know just the thing. The guards keep talking about a big, scary statue in the demon dungeon beyond the limits of town. I dare you to go in and touch it, and come back out without getting eaten by a monster! What do you say?");
                            player.Out.SendQuestSubscribeCommand(Charles, QuestMgr.GetIDForQuestType(typeof(childsplay)), "Will you join Charles's game of 'Truth or Dare'? \n[Level 1]");
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// This method checks if a player qualifies for this quest
        /// </summary>
        /// <returns>true if qualified, false if not</returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (player.IsDoingQuest(typeof(childsplay)) != null)
                return true;

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, "Good, no go out there and finish your work!");
            }
            else
            {
                SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
                quest.AbortQuest();
            }
        }

        protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
        {
            QuestEventArgs qargs = args as QuestEventArgs;
            if (qargs == null)
                return;

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(childsplay)))
                return;

            if (e == GamePlayerEvent.AcceptQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x01);
            else if (e == GamePlayerEvent.DeclineQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x00);
        }
        protected static void PlayerEnterStatueArea(DOLEvent e, object sender, EventArgs args)
        {
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;
            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            if (quest != null && quest.Step == 1)
            {
                player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've reached the statue Charles told you about. Return to him and let him know you've completed his dare.");
                quest.Step = 2;
            }
        }
        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            if (Charles.CanGiveQuest(typeof(childsplay), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(childsplay)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                if (!Charles.GiveQuest(typeof(childsplay), player, 1))
                    return;

                SendReply(player, "Great, we'll, uh, wait right here while you go do it. See, Raymond, not everyone's a big chicken like you. Chicken!");
            }
        }

        public override string Name
        {
            get { return questTitle; }
        }

        public override string Description
        {
            get
            {
                switch (Step)
                {
                    //k109: Update each time a kill is made.
                    case 1:
                        return "Charles has dared you to touch the statue in the center of Nergal's section of the Demons' Breach dungeon. Travel a short distance southeast of Cotswold's bindstone to find the dungeon entrance.";
                    case 2:
                        return "Return to Charles in Cotswold and let him know you've completed his dare.";
                    case 3:
                        return "[Step #2] Return to Charles in Mularn and let him know you've completed his dare.";
                }
                return base.Description;
            }
        }
        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //k109: xp and money Rewards...
            m_questPlayer.GainExperience(2, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), "You are awarded {0} copper!");
        }
    }
}
