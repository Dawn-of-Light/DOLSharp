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
*Author         : Etaew - Fallen Realms
*Editor			: Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 22 November 2004
*Quest Name     : War Concluded (Level 50)
*Quest Classes  : Hunter, Shadowblade (Rogue)
*Quest Version  : v1
*
*Changes:
*add bonuses to epic items
*
*ToDo:
*
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Rogue_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "War Concluded";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Masrim = null; // Start NPC
		private static GameNPC Oona = null; // Mob to kill
		private static GameNPC MorlinCaan = null; // Trainer for reward

		private static ItemTemplate oona_head = null; //ball of flame
		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate HunterEpicBoots = null; //Call of the Hunt Boots 
		private static ItemTemplate HunterEpicHelm = null; //Call of the Hunt Coif 
		private static ItemTemplate HunterEpicGloves = null; //Call of the Hunt Gloves 
		private static ItemTemplate HunterEpicVest = null; //Call of the Hunt Hauberk 
		private static ItemTemplate HunterEpicLegs = null; //Call of the Hunt Legs 
		private static ItemTemplate HunterEpicArms = null; //Call of the Hunt Sleeves 
		private static ItemTemplate ShadowbladeEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ShadowbladeEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ShadowbladeEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ShadowbladeEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ShadowbladeEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ShadowbladeEpicArms = null; //Shadow Shrouded Sleeves         

		// Constructors
		public Rogue_50() : base()
		{
		}

		public Rogue_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Rogue_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Rogue_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Masrim", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Masrim , creating it ...");
				Masrim = new GameNPC();
				Masrim.Model = 177;
				Masrim.Name = "Masrim";
				Masrim.GuildName = "";
				Masrim.Realm = eRealm.Midgard;
				Masrim.CurrentRegionID = 100;
				Masrim.Size = 52;
				Masrim.Level = 40;
				Masrim.X = 749099;
				Masrim.Y = 813104;
				Masrim.Z = 4437;
				Masrim.Heading = 2605;
				Masrim.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Masrim.SaveIntoDatabase();
				}

			}
			else
				Masrim = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Oona", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Oona , creating it ...");
				Oona = new GameNPC();
				Oona.Model = 356;
				Oona.Name = "Oona";
				Oona.GuildName = "";
				Oona.Realm = eRealm.None;
				Oona.CurrentRegionID = 100;
				Oona.Size = 50;
				Oona.Level = 65;
				Oona.X = 607233;
				Oona.Y = 786850;
				Oona.Z = 4384;
				Oona.Heading = 3891;
				Oona.Flags ^= (uint) GameNPC.eFlags.TRANSPARENT;
				Oona.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Oona.SaveIntoDatabase();
				}

			}
			else
				Oona = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Morlin Caan", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Morlin Caan , creating it ...");
				MorlinCaan = new GameNPC();
				MorlinCaan.Model = 235;
				MorlinCaan.Name = "Morlin Caan";
				MorlinCaan.GuildName = "Smith";
				MorlinCaan.Realm = eRealm.Midgard;
				MorlinCaan.CurrentRegionID = 101;
				MorlinCaan.Size = 50;
				MorlinCaan.Level = 54;
				MorlinCaan.X = 33400;
				MorlinCaan.Y = 33620;
				MorlinCaan.Z = 8023;
				MorlinCaan.Heading = 523;
				MorlinCaan.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					MorlinCaan.SaveIntoDatabase();
				}

			}
			else
				MorlinCaan = npcs[0];
			// end npc

			#endregion

			#region defineItems
			oona_head = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "oona_head");
			sealed_pouch = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sealed_pouch");
			HunterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicBoots");
			HunterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicHelm");
			HunterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicGloves");
			HunterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicVest");
			HunterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicLegs");
			HunterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicArms");
			ShadowbladeEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicBoots");
			ShadowbladeEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicHelm");
			ShadowbladeEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicGloves");
			ShadowbladeEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicVest");
			ShadowbladeEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicLegs");
			ShadowbladeEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicArms");
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Masrim, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasrim));
			GameEventMgr.AddHandler(Masrim, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasrim));

			GameEventMgr.AddHandler(MorlinCaan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMorlinCaan));
			GameEventMgr.AddHandler(MorlinCaan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMorlinCaan));

			/* Now we bring to Masrim the possibility to give this quest to players */
			Masrim.AddQuestToGive(typeof (Rogue_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Masrim == null || MorlinCaan == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Masrim, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasrim));
			GameEventMgr.RemoveHandler(Masrim, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasrim));

			GameEventMgr.RemoveHandler(MorlinCaan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMorlinCaan));
			GameEventMgr.RemoveHandler(MorlinCaan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMorlinCaan));

			/* Now we remove to Masrim the possibility to give this quest to players */
			Masrim.RemoveQuestToGive(typeof (Rogue_50));
		}

		protected static void TalkToMasrim(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Masrim.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Masrim.SayTo(player, "Midgard needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Masrim, QuestMgr.GetIDForQuestType(typeof(Rogue_50)), "Will you help Masrim [Rogue Level 50 Epic]?");
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

		protected static void TalkToMorlinCaan(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					MorlinCaan.SayTo(player, "Check your journal for instructions!");
				}
				return;
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Rogue_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Shadowblade &&
				player.CharacterClass.ID != (byte) eCharacterClass.Hunter)
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
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Rogue_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Rogue_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Masrim.GiveQuest(typeof (Rogue_50), player, 1))
					return;

				player.Out.SendMessage("Kill Oona in Raumarik loc 20k,51k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "War Concluded (Level 50 Rogue Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Oona in Raumarik Loc 20k,51k kill it!";
					case 2:
						return "[Step #2] Return to Masrim and give her Oona's Head!";
					case 3:
						return "[Step #3] Go to Morlin Caan in Jordheim and give him the Sealed Pouch for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Rogue_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Oona.Name)
				{
					m_questPlayer.Out.SendMessage("You collect Oona's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, oona_head);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Masrim.Name && gArgs.Item.TemplateID == oona_head.TemplateID)
				{
					RemoveItem(Masrim, player, oona_head);
					Masrim.SayTo(player, "Take this sealed pouch to Morlin Caan in Jordheim for your reward!");
					GiveItem(player, sealed_pouch);
					Step = 3;
					return;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == MorlinCaan.Name && gArgs.Item.TemplateID == sealed_pouch.TemplateID)
				{
					RemoveItem(MorlinCaan, player, sealed_pouch);
					MorlinCaan.SayTo(player, "You have earned this Epic Armour!");
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

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Shadowblade)
			{
				GiveItem(m_questPlayer, ShadowbladeEpicArms);
				GiveItem(m_questPlayer, ShadowbladeEpicBoots);
				GiveItem(m_questPlayer, ShadowbladeEpicGloves);
				GiveItem(m_questPlayer, ShadowbladeEpicHelm);
				GiveItem(m_questPlayer, ShadowbladeEpicLegs);
				GiveItem(m_questPlayer, ShadowbladeEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Hunter)
			{
				GiveItem(m_questPlayer, HunterEpicArms);
				GiveItem(m_questPlayer, HunterEpicBoots);
				GiveItem(m_questPlayer, HunterEpicGloves);
				GiveItem(m_questPlayer, HunterEpicHelm);
				GiveItem(m_questPlayer, HunterEpicLegs);
				GiveItem(m_questPlayer, HunterEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Masrim
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Masrim 
        *#28 give her the ball of flame
        *#29 talk with Masrim about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Call of the Hunt Boots 
            *Call of the Hunt Coif
            *Call of the Hunt Gloves
            *Call of the Hunt Hauberk
            *Call of the Hunt Legs
            *Call of the Hunt Sleeves
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
