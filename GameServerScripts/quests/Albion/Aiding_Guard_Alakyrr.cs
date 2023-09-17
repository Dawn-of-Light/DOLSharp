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
 * Author:		Doulbousiouf
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=10813,27525 Camelot Hills (Cotswold Village in the tavern) to speak with Godeleva Dowden
 * 2) Take his bucket and /use it in the river to scoop up some river water
 * 2) Take the full bucket back to Godeleva to have your reward
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Finance;
using DOL.GS.Geometry;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{

	public class AidingGuardAlakyrr : BaseQuest
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Aiding Guard Alakyrr";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;

		private static GameNPC GuardAlakyrr = null;

        private static ItemTemplate enchantedtenebrousflask = null;

        private static ItemTemplate quarterfulltenebrousflask = null;

        private static ItemTemplate halffulltenebrousflask = null;

        private static ItemTemplate threequarterfulltenebrousflask = null;

        private static ItemTemplate fullflaskoftenebrousessence = null;

		public AidingGuardAlakyrr()
			: base()
		{
		}

		public AidingGuardAlakyrr(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public AidingGuardAlakyrr(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public AidingGuardAlakyrr(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Guard Alakyrr", eRealm.None);

			if (npcs.Length == 0)
			{
                GuardAlakyrr = new DOL.GS.GameNPC();
                GuardAlakyrr.Model = 748;
                GuardAlakyrr.Name = "Guard Alakyrr";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + GuardAlakyrr.Name + ", creating ...");
                GuardAlakyrr.GuildName = "Part of " + questTitle + " Quest";
                GuardAlakyrr.Realm = eRealm.Albion;
                GuardAlakyrr.Size = 50;
                GuardAlakyrr.Level = 30;
                GuardAlakyrr.MaxSpeedBase = 191;
                GuardAlakyrr.Faction = FactionMgr.GetFactionByID(0);
                GuardAlakyrr.Position = Position.Create(regionID: 63, x: 28707, y: 20147, z: 16760, heading: 4016);
                GuardAlakyrr.RespawnInterval = -1;
                GuardAlakyrr.BodyType = 0;


                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 500;
                GuardAlakyrr.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					GuardAlakyrr.SaveIntoDatabase();

				GuardAlakyrr.AddToWorld();
			}
			else
				GuardAlakyrr = npcs[0];

			#endregion

			#region defineItems
            
            enchantedtenebrousflask = GameServer.Database.FindObjectByKey<ItemTemplate>("enchantedtenebrousflask");
			if (enchantedtenebrousflask == null)
			{
				if (log.IsWarnEnabled)
                    log.Warn("Could not find Enchanted Tenebrous Flask, creating it ...");
				enchantedtenebrousflask = new ItemTemplate();
                enchantedtenebrousflask.Name = "enchanted tenebrous flask";
				enchantedtenebrousflask.Level = 1;
				enchantedtenebrousflask.Weight = 10;
				enchantedtenebrousflask.Model = 1610;
				enchantedtenebrousflask.Object_Type = (int)eObjectType.GenericItem;
                enchantedtenebrousflask.Id_nb = "enchantedtenebrousflask";
				enchantedtenebrousflask.Price = 0;
				enchantedtenebrousflask.IsPickable = false;
				enchantedtenebrousflask.IsDropable = false;
				enchantedtenebrousflask.Quality = 100;
				enchantedtenebrousflask.Condition = 1000;
				enchantedtenebrousflask.MaxCondition = 1000;
				enchantedtenebrousflask.Durability = 1000;
				enchantedtenebrousflask.MaxDurability = 1000;
				
					GameServer.Database.AddObject(enchantedtenebrousflask);
			}
            quarterfulltenebrousflask = GameServer.Database.FindObjectByKey<ItemTemplate>("quarterfulltenebrousflask");
			if (quarterfulltenebrousflask == null)
			{
				if (log.IsWarnEnabled)
                    log.Warn("Could not find Quarter Full Tenebrous Flask, creating it ...");
				quarterfulltenebrousflask = new ItemTemplate();
                quarterfulltenebrousflask.Name = "quarter full tenebrous flask";
				quarterfulltenebrousflask.Level = 1;
				quarterfulltenebrousflask.Weight = 250;
				quarterfulltenebrousflask.Model = 1610;
				quarterfulltenebrousflask.Object_Type = (int)eObjectType.GenericItem;
                quarterfulltenebrousflask.Id_nb = "quarterfulltenebrousflask";
				quarterfulltenebrousflask.Price = 0;
				quarterfulltenebrousflask.IsPickable = false;
				quarterfulltenebrousflask.IsDropable = false;
				quarterfulltenebrousflask.Quality = 100;
				quarterfulltenebrousflask.Condition = 1000;
				quarterfulltenebrousflask.MaxCondition = 1000;
				quarterfulltenebrousflask.Durability = 1000;
				quarterfulltenebrousflask.MaxDurability = 1000;

					GameServer.Database.AddObject(quarterfulltenebrousflask);
			}
            halffulltenebrousflask = GameServer.Database.FindObjectByKey<ItemTemplate>("halffulltenebrousflask");
			if (halffulltenebrousflask == null)
			{
				if (log.IsWarnEnabled)
                    log.Warn("Could not find Half Full Tenebrous Flask, creating it ...");
				halffulltenebrousflask = new ItemTemplate();
                halffulltenebrousflask.Name = "half full tenebrous flask";
				halffulltenebrousflask.Level = 1;
				halffulltenebrousflask.Weight = 250;
				halffulltenebrousflask.Model = 1610;
				halffulltenebrousflask.Object_Type = (int)eObjectType.GenericItem;
                halffulltenebrousflask.Id_nb = "halffulltenebrousflask";
				halffulltenebrousflask.Price = 0;
				halffulltenebrousflask.IsPickable = false;
				halffulltenebrousflask.IsDropable = false;
				halffulltenebrousflask.Quality = 100;
				halffulltenebrousflask.Condition = 1000;
				halffulltenebrousflask.MaxCondition = 1000;
				halffulltenebrousflask.Durability = 1000;
				halffulltenebrousflask.MaxDurability = 1000;
				
					GameServer.Database.AddObject(halffulltenebrousflask);
            }
            threequarterfulltenebrousflask = GameServer.Database.FindObjectByKey<ItemTemplate>("threequarterfulltenebrousflask");
            if (threequarterfulltenebrousflask == null)
			{
				if (log.IsWarnEnabled)
                    log.Warn("Could not find Three Quarter Full Tenebrous Flask, creating it ...");
                threequarterfulltenebrousflask = new ItemTemplate();
                threequarterfulltenebrousflask.Name = "three quarter full tenebrous flask";
                threequarterfulltenebrousflask.Level = 1;
                threequarterfulltenebrousflask.Weight = 250;
                threequarterfulltenebrousflask.Model = 1610;
                threequarterfulltenebrousflask.Object_Type = (int)eObjectType.GenericItem;
                threequarterfulltenebrousflask.Id_nb = "threequarterfulltenebrousflask";
                threequarterfulltenebrousflask.Price = 0;
                threequarterfulltenebrousflask.IsPickable = false;
                threequarterfulltenebrousflask.IsDropable = false;
                threequarterfulltenebrousflask.Quality = 100;
                threequarterfulltenebrousflask.Condition = 1000;
                threequarterfulltenebrousflask.MaxCondition = 1000;
                threequarterfulltenebrousflask.Durability = 1000;
                threequarterfulltenebrousflask.MaxDurability = 1000;

                GameServer.Database.AddObject(threequarterfulltenebrousflask);
            }
             fullflaskoftenebrousessence = GameServer.Database.FindObjectByKey<ItemTemplate>("fullflaskoftenebrousessence");
             if (fullflaskoftenebrousessence == null)
             {
                 if (log.IsWarnEnabled)
                     log.Warn("Could not find Full Flask Of Tenebrous Essence, creating it ...");
                 fullflaskoftenebrousessence = new ItemTemplate();
                 fullflaskoftenebrousessence.Name = "full flask of tenebrous essence";
                 fullflaskoftenebrousessence.Level = 1;
                 fullflaskoftenebrousessence.Weight = 250;
                 fullflaskoftenebrousessence.Model = 1610;
                 fullflaskoftenebrousessence.Object_Type = (int)eObjectType.GenericItem;
                 fullflaskoftenebrousessence.Id_nb = "fullflaskoftenebrousessence";
                 fullflaskoftenebrousessence.Price = 0;
                 fullflaskoftenebrousessence.IsPickable = false;
                 fullflaskoftenebrousessence.IsDropable = false;
                 fullflaskoftenebrousessence.Quality = 100;
                 fullflaskoftenebrousessence.Condition = 1000;
                 fullflaskoftenebrousessence.MaxCondition = 1000;
                 fullflaskoftenebrousessence.Durability = 1000;
                 fullflaskoftenebrousessence.MaxDurability = 1000;

                 GameServer.Database.AddObject(fullflaskoftenebrousessence);
             }
			
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(GuardAlakyrr, GameLivingEvent.Interact, new DOLEventHandler(TalkToGuardAlakyrr));
			GameEventMgr.AddHandler(GuardAlakyrr, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardAlakyrr));

			GuardAlakyrr.AddQuestToGive(typeof(AidingGuardAlakyrr));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (GuardAlakyrr == null)
				return;


			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(GuardAlakyrr, GameLivingEvent.Interact, new DOLEventHandler(TalkToGuardAlakyrr));
			GameEventMgr.RemoveHandler(GuardAlakyrr, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardAlakyrr));

			GuardAlakyrr.RemoveQuestToGive(typeof(AidingGuardAlakyrr));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			AidingGuardAlakyrr quest = player.IsDoingQuest(typeof(AidingGuardAlakyrr)) as AidingGuardAlakyrr;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			AidingGuardAlakyrr quest = player.IsDoingQuest(typeof(AidingGuardAlakyrr)) as AidingGuardAlakyrr;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}


		protected static void TalkToGuardAlakyrr(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (GuardAlakyrr.CanGiveQuest(typeof(AidingGuardAlakyrr), player) <= 0)
				return;

			AidingGuardAlakyrr quest = player.IsDoingQuest(typeof(AidingGuardAlakyrr)) as AidingGuardAlakyrr;

			GuardAlakyrr.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
                    GuardAlakyrr.SayTo(player, "You are new to this area aren't you? Yes, you look to be one of those from upper Albion. You will find that this lower realm is not as [familiar] as upper Albion and your Camelot.");
					return;
				}
				else
				{
                    if (quest.Step == 1)
                    {
                        if (e == GameLivingEvent.WhisperReceive)
                        {
                            WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                            switch (wArgs.Text)
                            {
                                case "study":

                                    GuardAlakyrr.SayTo(player, "You will need to slay tenebrae and then use an enchanted Tenebrous Flask, while inside the Tenebrous Quarter, to capture their essence. I need you to obtain a full flask of Tenebrous Essence. Return to me once you complete this duty and I will reward you for your efforts.");
                                    quest.Step = 2;
                                    break;
                            }
                        }
                    }
                    if(quest.Step == 10)
					{
                        GuardAlakyrr.SayTo(player, "It was Arawn's will to allow your return. I am grateful that you made it back. Please give me the Full Flask of Tenebrous Essence.");
                        quest.Step = 11;
                    }                    
					return;
				}
                
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
				if (quest == null)
				{
					switch (wArgs.Text)
					{
                        case "familiar":
                            GuardAlakyrr.SayTo(player, "Everything here is quite unlike upper Albion, from the surroundings to the creatures. We, the chosen of Arawn, are not even familiar with some of the [creatures] which lurk in the Aqueducts.");
							break;
                        case "creatures":
                            GuardAlakyrr.SayTo(player, "We have been trying to study some of these creatures so that we can learn more of their origins to better defend ourselves. Our forces are spread thin with the war against the evil army being [waged] below.");
							break;
                        case "waged":
                            GuardAlakyrr.SayTo(player, "We have not had enough troops to spare to help us combat some of these hostile beings. The [tenebrae] are some of these creatures.");
                            break;
                        case "tenebrae":
                            GuardAlakyrr.SayTo(player, "They seem to hate all that is living, and it is an intense hate, indeed. Their origins are shrouded in mystery, but we do know that they were created out of [darkness].");
                            break;
                        case "darkness":
                            GuardAlakyrr.SayTo(player, "The attacks by the tenebrae have been numerous, and we need to cease some of these attacks. I am authorized to provide money to any who can slay these tenebrae and bring me proof of their deed. Will you [aid] us in this time of need?");
                            break;
                        case "aid":
                            player.Out.SendQuestSubscribeCommand(GuardAlakyrr, QuestMgr.GetIDForQuestType(typeof(AidingGuardAlakyrr)), "Will you slay the tenebrae for Guard Alakyrr? [Levels 1-4]");
							break;                        
					}
				}
				else
				{
                    switch (wArgs.Text)
                    {

                        case "study":
                            if (quest.Step == 1)
                            {
                                GuardAlakyrr.SayTo(player, "You will need to slay tenebrae and then use an enchanted Tenebrous Flask, while inside the Tenebrous Quarter, to capture their essence. I need you to obtain a full flask of Tenebrous Essence. Return to me once you complete this duty and I will reward you for your efforts.");
                                quest.Step = 2;
                            }
                            break;
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                    }
                }                
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AidingGuardAlakyrr)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}


        public override bool CheckQuestQualification(GamePlayer player)
		{
			if (player.IsDoingQuest(typeof(AidingGuardAlakyrr)) != null)
				return true;
            
			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			AidingGuardAlakyrr quest = player.IsDoingQuest(typeof(AidingGuardAlakyrr)) as AidingGuardAlakyrr;

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
            AidingGuardAlakyrr quest = player.IsDoingQuest(typeof(AidingGuardAlakyrr)) as AidingGuardAlakyrr;
			if (GuardAlakyrr.CanGiveQuest(typeof(AidingGuardAlakyrr), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(AidingGuardAlakyrr)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
            {
                if (!GuardAlakyrr.GiveQuest(typeof(AidingGuardAlakyrr), player, 1))
                    return;
                SendReply(player, "Very well. Please travel south down this tunnel to what we guards call the Tenebrous Quarter and slay tenebrae. I will need you to collect some of their essence so that we can send it to our scholars to [study].");                
                GiveItem(GuardAlakyrr, player, enchantedtenebrousflask);    
    
                GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
            }
		}
        
		public override string Name
		{
			get { return questTitle; }
		}

        #region Steps

        public override string Description
		{
			get
			{
				switch (Step)
				{
                    case 1:
                        return "Speak with Guard Alakyrr about the tenebrae.";

                    case 2:
                        return "Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";

                    case 3:
                        return "/Use the Enchanted Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";

                    case 4:
                        return "Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";

                    case 5:
                        return "/Use the Quarter Full Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";

                    case 6:
                        return "Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";

                    case 7:
                        return "/Use the Half Full Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";

                    case 8:
                        return "Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";

                    case 9:
                        return "/Use the Three Quarter Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";

                    case 10:
                        return "Return to Guard Alakyrr with the Full Flask of Tenebrous Essence.";

                    case 11:
                        return "Give Guard Alakyrr the Full Flask of Tenebrous Essence to receive your reward.";
                                            
                }				
				return base.Description;
			}
		}
        #endregion

        public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
            #region mobkilled

			GamePlayer player = sender as GamePlayer;
           
            if (player == null || player.IsDoingQuest(typeof(AidingGuardAlakyrr)) == null)
                return;

            if (e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (Step == 2)
                {
                    if (gArgs.Target.Name == "tenebrous fighter")
                    {
                        SendReply(player,"A tenebrous shadow is released. Use the Enchanted Tenebrous Flask to capture the Tenebrous Essence.");
                        Step = 3;
                    }
                }
                if (Step == 4)
                {
                    if (gArgs.Target.Name == "tenebrous fighter")
                    {
                        SendReply(player, "A tenebrous shadow is released. Use the Quarter Full Tenebrous Flask to capture the Tenebrous Essence.");
                        Step = 5;
                    }
                }
                if (Step == 6)
                {
                    if (gArgs.Target.Name == "tenebrous fighter")
                    {
                        SendReply(player, "A tenebrous shadow is released. Use the Half Full Tenebrous Flask to capture the Tenebrous Essence.");
                        Step = 7;
                    }
                }
                if (Step == 8)
                {
                    if (gArgs.Target.Name == "tenebrous fighter")
                    {
                        SendReply(player, "A tenebrous shadow is released. Use the 3 Quarters Full Tenebrous Flask to capture the Tenebrous Essence.");
                        Step = 9;
                    }
                }
            }
        #endregion

            if (player == null || player.IsDoingQuest(typeof(AidingGuardAlakyrr)) == null)
				return;

			if (e == GamePlayerEvent.GiveItem)
			{
				if (Step == 11)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
					if (gArgs.Target.Name == GuardAlakyrr.Name && gArgs.Item.Id_nb == fullflaskoftenebrousessence.Id_nb)
					{
						RemoveItem(GuardAlakyrr, m_questPlayer, fullflaskoftenebrousessence);

						GuardAlakyrr.TurnTo(m_questPlayer);
                        GuardAlakyrr.SayTo(m_questPlayer, "Thank you. This is a promising specimen for study. Please take this coin as a show of our appreciation. Blessings of Arawn be upon you.");

						FinishQuest();
					}
				}
			}
		}

        #region useslot
        protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            AidingGuardAlakyrr quest = (AidingGuardAlakyrr)player.IsDoingQuest(typeof(AidingGuardAlakyrr));
            if (quest == null)
                return;

            UseSlotEventArgs uArgs = (UseSlotEventArgs)args;

            InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
            if (item != null && item.Id_nb == enchantedtenebrousflask.Id_nb)
            {
                if (quest.Step == 3)
                {
                    SendSystemMessage(player, "You use the Enchanted Tenebrous Flask.");

                    ReplaceItem(player, enchantedtenebrousflask, quarterfulltenebrousflask);
                    SendSystemMessage(player, "The flask is one quarter full.");
                    quest.Step = 4;
                }
            }
            if (item != null && item.Id_nb == quarterfulltenebrousflask.Id_nb)
            {
                if (quest.Step == 5)
                {
                    SendSystemMessage(player, "You use the Quarter Full Tenebrous Flask.");

                    ReplaceItem(player, quarterfulltenebrousflask, halffulltenebrousflask);
                    SendSystemMessage(player, "The flask is half full.");
                    quest.Step = 6;
                }
            }
            if (item != null && item.Id_nb == halffulltenebrousflask.Id_nb)
            {
                if (quest.Step == 7)
                {
                    SendSystemMessage(player, "You use the Half Full Tenebrous Flask.");

                    ReplaceItem(player, halffulltenebrousflask, threequarterfulltenebrousflask);
                    SendSystemMessage(player, "The flask is three quarters full.");
                    quest.Step = 8;
                }
            }
            if (item != null && item.Id_nb == threequarterfulltenebrousflask.Id_nb)
            {
                if (quest.Step == 9)
                {
                    SendSystemMessage(player, "You use the 3 Quarters Full Tenebrous Flask.");

                    ReplaceItem(player, threequarterfulltenebrousflask, fullflaskoftenebrousessence);
                    SendSystemMessage(player, "You fill the flask with Tenebrous Essence.");
                    quest.Step = 10;
                }
            }
        }
        #endregion

		public override void AbortQuest()
		{
			base.AbortQuest();
			RemoveItem(m_questPlayer, enchantedtenebrousflask, false);
			RemoveItem(m_questPlayer, quarterfulltenebrousflask, false);
            RemoveItem(m_questPlayer, halffulltenebrousflask, false);
            RemoveItem(m_questPlayer, fullflaskoftenebrousessence, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest();

			m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 40 + (m_questPlayer.Level - 1) * 4, true);
            long money = Money.GetMoney(0, 0, 0, 7, Util.Random(50));
			m_questPlayer.AddMoney(Currency.Copper.Mint(money));
			m_questPlayer.SendSystemMessage("You are awarded 7 silver and some copper!");
            InventoryLogging.LogInventoryAction("(QUEST;" + Name + ")", m_questPlayer, eInventoryActionType.Quest, money);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}
	}
}
