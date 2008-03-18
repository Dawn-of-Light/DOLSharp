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
*Source         : http://camelot.allakhazam.com
*Date           : 21 November 2004
*Quest Name     : The Desire of a God (Level 50)
*Quest Classes  : Healer, Shaman (Seer)
*Quest Version  : v1.2
*
*Changes:
*   The epic armour should now have the correct durability and condition
*   The armour will now be correctly rewarded with all peices
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*   Add bonuses to epic items
*ToDo:
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
	public class Seer_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Desire of a God";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Inaksha = null; // Start NPC
		private static GameNPC Loken = null; // Mob to kill
		private static GameNPC Miri = null; // Trainer for reward

		private static ItemTemplate ball_of_flame = null; //ball of flame
		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate HealerEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate HealerEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate HealerEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate HealerEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate HealerEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate HealerEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate ShamanEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ShamanEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ShamanEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ShamanEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ShamanEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ShamanEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Seer_50() : base()
		{
		}

		public Seer_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Seer_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Seer_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Inaksha", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Inaksha , creating it ...");
				Inaksha = new GameNPC();
				Inaksha.Model = 193;
				Inaksha.Name = "Inaksha";
				Inaksha.GuildName = "";
				Inaksha.Realm = eRealm.Midgard;
				Inaksha.CurrentRegionID = 100;
				Inaksha.Size = 50;
				Inaksha.Level = 41;
				Inaksha.X = 805929;
				Inaksha.Y = 702449;
				Inaksha.Z = 4960;
				Inaksha.Heading = 2116;
				Inaksha.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Inaksha.SaveIntoDatabase();
				}

			}
			else
				Inaksha = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Loken", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Loken , creating it ...");
				Loken = new GameNPC();
				Loken.Model = 212;
				Loken.Name = "Loken";
				Loken.GuildName = "";
				Loken.Realm = eRealm.None;
				Loken.CurrentRegionID = 100;
				Loken.Size = 50;
				Loken.Level = 65;
				Loken.X = 636784;
				Loken.Y = 762433;
				Loken.Z = 4596;
				Loken.Heading = 3777;
				Loken.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Loken.SaveIntoDatabase();
				}

			}
			else
				Loken = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Miri", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Miri , creating it ...");
				Miri = new GameNPC();
				Miri.Model = 220;
				Miri.Name = "Miri";
				Miri.GuildName = "";
				Miri.Realm = eRealm.Midgard;
				Miri.CurrentRegionID = 101;
				Miri.Size = 50;
				Miri.Level = 43;
				Miri.X = 30641;
				Miri.Y = 32093;
				Miri.Z = 8305;
				Miri.Heading = 3037;
				Miri.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Miri.SaveIntoDatabase();
				}

			}
			else
				Miri = npcs[0];
			// end npc

			#endregion

			#region defineItems
			ball_of_flame = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ball_of_flame");
			sealed_pouch = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sealed_pouch");
			HealerEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicBoots");
			HealerEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicHelm");
			HealerEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicGloves");
			HealerEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicVest");
			HealerEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicLegs");
			HealerEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HealerEpicArms");
			ShamanEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicBoots");
			ShamanEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicHelm");
			ShamanEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicGloves");
			ShamanEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicVest");
			ShamanEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicLegs");
			ShamanEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShamanEpicArms");
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.AddHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we bring to Inaksha the possibility to give this quest to players */
			Inaksha.AddQuestToGive(typeof (Seer_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Inaksha == null || Miri == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.RemoveHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we remove to Inaksha the possibility to give this quest to players */
			Inaksha.RemoveQuestToGive(typeof (Seer_50));
		}

		protected static void TalkToInaksha(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Inaksha.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Inaksha.SayTo(player, "Midgard needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Inaksha, QuestMgr.GetIDForQuestType(typeof(Seer_50)), "Will you help Inaksha [Seer Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "dead":
							if (quest.Step == 3)
							{
								Inaksha.SayTo(player, "Take this sealed pouch to Miri in Jordheim for your reward!");
								GiveItem(Inaksha, player, sealed_pouch);
								quest.Step = 4;
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}

			}

		}

		protected static void TalkToMiri(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Miri.SayTo(player, "Check your journal for instructions!");
				}
				else
				{
					Miri.SayTo(player, "I need your help to seek out loken in raumarik Loc 47k, 25k, 4k, and kill him ");
				}
			}

		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Shaman &&
				player.CharacterClass.ID != (byte) eCharacterClass.Healer)
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
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Seer_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Inaksha.GiveQuest(typeof (Seer_50), player, 1))
					return;

				player.Out.SendMessage("Good now go kill him!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Desire of a God (Level 50 Seer Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Loken in Raumarik Loc 47k, 25k kill him!";
					case 2:
						return "[Step #2] Return to Inaksha and give her the Ball of Flame!";
					case 3:
						return "[Step #3] Talk with Inaksha about Loken’s demise!";
					case 4:
						return "[Step #4] Go to Miri in Jordheim and give her the Sealed Pouch for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Seer_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Loken.Name)
				{
					m_questPlayer.Out.SendMessage("You get a ball of flame", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, ball_of_flame);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Inaksha.Name && gArgs.Item.TemplateID == ball_of_flame.TemplateID)
				{
					RemoveItem(Inaksha, player, ball_of_flame);
					Inaksha.SayTo(player, "So it seems Logan's [dead]");
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Miri.Name && gArgs.Item.TemplateID == sealed_pouch.TemplateID)
				{
					RemoveItem(Miri, player, sealed_pouch);
					Miri.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, ball_of_flame, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Shaman)
			{
				GiveItem(m_questPlayer, ShamanEpicArms);
				GiveItem(m_questPlayer, ShamanEpicBoots);
				GiveItem(m_questPlayer, ShamanEpicGloves);
				GiveItem(m_questPlayer, ShamanEpicHelm);
				GiveItem(m_questPlayer, ShamanEpicLegs);
				GiveItem(m_questPlayer, ShamanEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Healer)
			{
				GiveItem(m_questPlayer, HealerEpicArms);
				GiveItem(m_questPlayer, HealerEpicBoots);
				GiveItem(m_questPlayer, HealerEpicGloves);
				GiveItem(m_questPlayer, HealerEpicHelm);
				GiveItem(m_questPlayer, HealerEpicLegs);
				GiveItem(m_questPlayer, HealerEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Inaksha
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Inaksha 
        *#28 give her the ball of flame
        *#29 talk with Inaksha about Loken’s demise
        *#30 go to Miri in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Valhalla Touched Boots 
            *Valhalla Touched Coif
            *Valhalla Touched Gloves
            *Valhalla Touched Hauberk
            *Valhalla Touched Legs
            *Valhalla Touched Sleeves
            *Subterranean Boots
            *Subterranean Coif
            *Subterranean Gloves
            *Subterranean Hauberk
            *Subterranean Legs
            *Subterranean Sleeves
        */

		#endregion
	}
}
