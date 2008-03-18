/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Theurgist, Armsman, Scout, and Friar (Defenders of Albion)
*Quest Version  : v1
*
*ToDo:
*   Add Bonuses to Epic Items
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	public class Defenders_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Feast of the Decadent";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Lidmann = null; // Start NPC
		private static GameNPC Uragaig = null; // Mob to kill

		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate ScoutEpicBoots = null; //Brigandine of Vigilant Defense  Boots 
		private static ItemTemplate ScoutEpicHelm = null; //Brigandine of Vigilant Defense  Coif 
		private static ItemTemplate ScoutEpicGloves = null; //Brigandine of Vigilant Defense  Gloves 
		private static ItemTemplate ScoutEpicVest = null; //Brigandine of Vigilant Defense  Hauberk 
		private static ItemTemplate ScoutEpicLegs = null; //Brigandine of Vigilant Defense  Legs 
		private static ItemTemplate ScoutEpicArms = null; //Brigandine of Vigilant Defense  Sleeves 
		private static ItemTemplate ArmsmanEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ArmsmanEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ArmsmanEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ArmsmanEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ArmsmanEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ArmsmanEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate TheurgistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate TheurgistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate TheurgistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate TheurgistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate TheurgistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate TheurgistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate FriarEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate FriarEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate FriarEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate FriarEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate FriarEpicLegs = null; //Subterranean Legs
		private static ItemTemplate FriarEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate MaulerEpicBoots = null;
		private static ItemTemplate MaulerEpicHelm = null;
		private static ItemTemplate MaulerEpicGloves = null;
		private static ItemTemplate MaulerEpicVest = null;
		private static ItemTemplate MaulerEpicLegs = null;
		private static ItemTemplate MaulerEpicArms = null;

		// Constructors
		public Defenders_50() : base()
		{
		}

		public Defenders_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Defenders_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Defenders_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{

				Lidmann = new GameNPC();
				Lidmann.Model = 64;
				Lidmann.Name = "Lidmann Halsey";

				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Lidmann.Name + ", creating it ...");

				Lidmann.GuildName = "";
				Lidmann.Realm = eRealm.Albion;
				Lidmann.CurrentRegionID = 1;
				Lidmann.Size = 50;
				Lidmann.Level = 50;
				Lidmann.X = 466464;
				Lidmann.Y = 634554;
				Lidmann.Z = 1954;
				Lidmann.Heading = 1809;
				Lidmann.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lidmann.SaveIntoDatabase();
				}

			}
			else
				Lidmann = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailleach Uragaig", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Uragaig , creating it ...");
				Uragaig = new GameNPC();
				Uragaig.Model = 349;
				Uragaig.Name = "Cailleach Uragaig";
				Uragaig.GuildName = "";
				Uragaig.Realm = eRealm.None;
				Uragaig.CurrentRegionID = 1;
				Uragaig.Size = 55;
				Uragaig.Level = 70;
				Uragaig.X = 316218;
				Uragaig.Y = 664484;
				Uragaig.Z = 2736;
				Uragaig.Heading = 3072;
				Uragaig.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Uragaig.SaveIntoDatabase();
				}

			}
			else
				Uragaig = npcs[0];
			// end npc

			#endregion

			#region defineItems
			sealed_pouch = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "sealed_pouch");
			
			ScoutEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicBoots");
			ScoutEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicHelm");
			ScoutEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicGloves");
			ScoutEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicVest");
			ScoutEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicLegs");
			ScoutEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ScoutEpicArms");
			
			ArmsmanEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicBoots");
			ArmsmanEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicHelm");
			ArmsmanEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicGloves");
			ArmsmanEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicVest");
			ArmsmanEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicLegs");
			ArmsmanEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ArmsmanEpicArms");
			
			FriarEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicBoots");
			FriarEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicHelm");
			FriarEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicGloves");
			FriarEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicVest");
			FriarEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicLegs");
			FriarEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "FriarEpicArms");
			
			TheurgistEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicBoots");
			TheurgistEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicHelm");
			TheurgistEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicGloves");
			TheurgistEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicVest");
			TheurgistEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicLegs");
			TheurgistEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TheurgistEpicArms");
			
			MaulerEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicBoots");
			MaulerEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicHelm");
			MaulerEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicGloves");
			MaulerEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicVest");
			MaulerEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicLegs");
			MaulerEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicArms");
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			Lidmann.AddQuestToGive(typeof(Defenders_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			//if not loaded, don't worry
			if (Lidmann == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.RemoveHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			Lidmann.RemoveQuestToGive(typeof (Defenders_50));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lidmann.CanGiveQuest(typeof (Defenders_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Defenders_50 quest = player.IsDoingQuest(typeof (Defenders_50)) as Defenders_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Lidmann.SayTo(player, "Check your Journal for instructions!");
					return;
				}
				else
				{
					// Check if player is qualifed for quest                
					Lidmann.SayTo(player, "Albion needs your [services]");
					return;
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Lidmann, QuestMgr.GetIDForQuestType(typeof(Defenders_50)), "Will you help Lidmann [Defenders of Albion Level 50 Epic]?");
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

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Defenders_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Armsman &&
				player.CharacterClass.ID != (byte) eCharacterClass.Scout &&
				player.CharacterClass.ID != (byte) eCharacterClass.Theurgist &&
				player.CharacterClass.ID != (byte) eCharacterClass.Friar &&
				player.CharacterClass.ID != (byte) eCharacterClass.Mauler_Alb)
				return false;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;

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
			Defenders_50 quest = player.IsDoingQuest(typeof (Defenders_50)) as Defenders_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Defenders_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Lidmann.CanGiveQuest(typeof (Defenders_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Defenders_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Lidmann.GiveQuest(typeof (Defenders_50), player, 1))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Defenders of Albion Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailleach Uragaig in Lyonesse Loc 29k,33k kill her!";
					case 2:
						return "[Step #2] Give the sealed pouch to Lidmann Halse.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Defenders_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				{
					if (gArgs.Target.Name == Uragaig.Name)
					{
						m_questPlayer.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						GiveItem(m_questPlayer, sealed_pouch);
						Step = 2;
						return;
					}
				}
			}

            if (Step == 2 && e == GamePlayerEvent.GiveItem)
            {
                // Graveen: if not existing maulerepic in DB
                // player is not allowed to finish this quest until we fix this problem
                if (MaulerEpicArms == null || MaulerEpicBoots == null || MaulerEpicGloves == null ||
                    MaulerEpicHelm == null || MaulerEpicLegs == null || MaulerEpicVest == null)
                {
                    Lidmann.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
                    return;
                }

                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Lidmann.Name && gArgs.Item.TemplateID == sealed_pouch.TemplateID)
				{
					RemoveItem(Lidmann, player, sealed_pouch);
					Lidmann.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Armsman)
			{
				GiveItem(m_questPlayer, ArmsmanEpicBoots);
				GiveItem(m_questPlayer, ArmsmanEpicArms);
				GiveItem(m_questPlayer, ArmsmanEpicGloves);
				GiveItem(m_questPlayer, ArmsmanEpicHelm);
				GiveItem(m_questPlayer, ArmsmanEpicLegs);
				GiveItem(m_questPlayer, ArmsmanEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Scout)
			{
				GiveItem(m_questPlayer, ScoutEpicArms);
				GiveItem(m_questPlayer, ScoutEpicBoots);
				GiveItem(m_questPlayer, ScoutEpicGloves);
				GiveItem(m_questPlayer, ScoutEpicHelm);
				GiveItem(m_questPlayer, ScoutEpicLegs);
				GiveItem(m_questPlayer, ScoutEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Theurgist)
			{
				GiveItem(m_questPlayer, TheurgistEpicArms);
				GiveItem(m_questPlayer, TheurgistEpicBoots);
				GiveItem(m_questPlayer, TheurgistEpicGloves);
				GiveItem(m_questPlayer, TheurgistEpicHelm);
				GiveItem(m_questPlayer, TheurgistEpicLegs);
				GiveItem(m_questPlayer, TheurgistEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Friar)
			{
				GiveItem(m_questPlayer, FriarEpicArms);
				GiveItem(m_questPlayer, FriarEpicBoots);
				GiveItem(m_questPlayer, FriarEpicGloves);
				GiveItem(m_questPlayer, FriarEpicHelm);
				GiveItem(m_questPlayer, FriarEpicLegs);
				GiveItem(m_questPlayer, FriarEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mauler_Alb)
			{
				GiveItem(m_questPlayer, MaulerEpicArms);
				GiveItem(m_questPlayer, MaulerEpicBoots);
				GiveItem(m_questPlayer, MaulerEpicGloves);
				GiveItem(m_questPlayer, MaulerEpicHelm);
				GiveItem(m_questPlayer, MaulerEpicLegs);
				GiveItem(m_questPlayer, MaulerEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Lidmann
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Lidmann 
        *#28 give her the ball of flame
        *#29 talk with Lidmann about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Brigandine of Vigilant Defense  Boots 
            *Brigandine of Vigilant Defense  Coif
            *Brigandine of Vigilant Defense  Gloves
            *Brigandine of Vigilant Defense  Hauberk
            *Brigandine of Vigilant Defense  Legs
            *Brigandine of Vigilant Defense  Sleeves
            *Shadow Shrouded Boots
            *Shadow Shrouded Coif
            *Shadow Shrouded Gloves
            *Shadow Shrouded Hauberk
            *Shadow Shrouded Legs
            *Shadow Shrouded Sleeves
        */

		#endregion
	}
}
