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
*Date           : 22 November 2004
*Quest Name     : Unnatural Powers (level 50)
*Quest Classes  : Eldritch, Hero, Ranger, and Warden (Path of Focus)
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

namespace DOL.GS.Quests.Hibernia
{
	public class Focus_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Unnatural Powers";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Ainrebh = null; // Start NPC
		private static GameNPC GreenMaw = null; // Mob to kill

		private static ItemTemplate GreenMaw_key = null; //ball of flame
		private static ItemTemplate RangerEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate RangerEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate RangerEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate RangerEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate RangerEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate RangerEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate HeroEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate HeroEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate HeroEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate HeroEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate HeroEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate HeroEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EldritchEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EldritchEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EldritchEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EldritchEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EldritchEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EldritchEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate WardenEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate WardenEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate WardenEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate WardenEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate WardenEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate WardenEpicArms = null; //Subterranean Sleeves 
		private static ItemTemplate MaulerEpicBoots = null;
		private static ItemTemplate MaulerEpicHelm = null;
		private static ItemTemplate MaulerEpicGloves = null;
		private static ItemTemplate MaulerEpicVest = null;
		private static ItemTemplate MaulerEpicLegs = null;
		private static ItemTemplate MaulerEpicArms = null;

		// Constructors
		public Focus_50()
			: base()
		{
		}

		public Focus_50(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		public Focus_50(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public Focus_50(GamePlayer questingPlayer, DBQuest dbQuest)
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

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Ainrebh", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ainrebh , creating it ...");
				Ainrebh = new GameNPC();
				Ainrebh.Model = 384;
				Ainrebh.Name = "Ainrebh";
				Ainrebh.GuildName = "Enchanter";
				Ainrebh.Realm = eRealm.Hibernia;
				Ainrebh.CurrentRegionID = 200;
				Ainrebh.Size = 48;
				Ainrebh.Level = 40;
				Ainrebh.X = 421281;
				Ainrebh.Y = 516273;
				Ainrebh.Z = 1877;
				Ainrebh.Heading = 3254;
				Ainrebh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ainrebh.SaveIntoDatabase();
				}

			}
			else
				Ainrebh = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Green Maw", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find GreenMaw , creating it ...");
				GreenMaw = new GameNPC();
				GreenMaw.Model = 146;
				GreenMaw.Name = "Green Maw";
				GreenMaw.GuildName = "";
				GreenMaw.Realm = eRealm.None;
				GreenMaw.CurrentRegionID = 200;
				GreenMaw.Size = 50;
				GreenMaw.Level = 65;
				GreenMaw.X = 488306;
				GreenMaw.Y = 521440;
				GreenMaw.Z = 6328;
				GreenMaw.Heading = 1162;
				GreenMaw.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					GreenMaw.SaveIntoDatabase();
				}

			}
			else
				GreenMaw = npcs[0];
			// end npc

			#endregion

			#region Item Declarations
			GreenMaw_key = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "GreenMaw_key");
			RangerEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicBoots");
			RangerEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicHelm");
			RangerEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicGloves");
			RangerEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicVest");
			RangerEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicLegs");
			RangerEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RangerEpicArms");
			HeroEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicBoots");
			HeroEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicHelm");
			HeroEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicGloves");
			HeroEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicVest");
			HeroEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicLegs");
			HeroEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "HeroEpicArms");
			WardenEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicBoots");
			WardenEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicHelm");
			WardenEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicGloves");
			WardenEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicVest");
			WardenEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicLegs");
			WardenEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WardenEpicArms");
			EldritchEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicBoots");
			EldritchEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicHelm");
			EldritchEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicGloves");
			EldritchEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicVest");
			EldritchEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicLegs");
			EldritchEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "EldritchEpicArms");
			#region Mauler
			MaulerEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicBoots");
			MaulerEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicHelm");
			MaulerEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicGloves");
			MaulerEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicVest");
			MaulerEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicLegs");
			MaulerEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NewMaulerEpicArms");
			#endregion
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.AddHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));

			/* Now we bring to Ainrebh the possibility to give this quest to players */
			Ainrebh.AddQuestToGive(typeof(Focus_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Ainrebh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.RemoveHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));

			/* Now we remove to Ainrebh the possibility to give this quest to players */
			Ainrebh.RemoveQuestToGive(typeof(Focus_50));
		}

		protected static void TalkToAinrebh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Ainrebh.CanGiveQuest(typeof(Focus_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Focus_50 quest = player.IsDoingQuest(typeof(Focus_50)) as Focus_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Ainrebh.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Ainrebh.SayTo(player, "Hibernia needs your [services]");
				}

			}
			// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Ainrebh, QuestMgr.GetIDForQuestType(typeof(Focus_50)), "Will you help Ainrebh [Path of Focus Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof(Focus_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Hero &&
				player.CharacterClass.ID != (byte)eCharacterClass.Ranger &&
				player.CharacterClass.ID != (byte)eCharacterClass.Warden &&
				player.CharacterClass.ID != (byte)eCharacterClass.Eldritch &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mauler_Hib)
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
			Focus_50 quest = player.IsDoingQuest(typeof(Focus_50)) as Focus_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Focus_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (Ainrebh.CanGiveQuest(typeof(Focus_50), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Focus_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Ainrebh.GiveQuest(typeof(Focus_50), player, 1))
					return;
				player.Out.SendMessage("Kill Green Maw in Cursed Forest loc 37k, 38k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Unnatural Powers (Level 50 Path of Focus Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out GreenMaw in Cursed Forest Loc 37k,38k kill it!";
					case 2:
						return "[Step #2] Return to Ainrebh and give her Green Maw's Key!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Focus_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == GreenMaw.Name)
				{
					m_questPlayer.Out.SendMessage("You collect Green Maw's Key", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, GreenMaw_key);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				// Graveen: if not existing maulerepic in DB
				// player is not allowed to finish this quest until we fix this problem
				if (MaulerEpicArms == null || MaulerEpicBoots == null || MaulerEpicGloves == null ||
					MaulerEpicHelm == null || MaulerEpicLegs == null || MaulerEpicVest == null)
				{
					Ainrebh.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
					return;
				}

				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Ainrebh.Name && gArgs.Item.TemplateID == GreenMaw_key.TemplateID)
				{
					RemoveItem(Ainrebh, player, GreenMaw_key);
					Ainrebh.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, GreenMaw_key, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hero)
			{
				GiveItem(m_questPlayer, HeroEpicArms);
				GiveItem(m_questPlayer, HeroEpicBoots);
				GiveItem(m_questPlayer, HeroEpicGloves);
				GiveItem(m_questPlayer, HeroEpicHelm);
				GiveItem(m_questPlayer, HeroEpicLegs);
				GiveItem(m_questPlayer, HeroEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Ranger)
			{
				GiveItem(m_questPlayer, RangerEpicArms);
				GiveItem(m_questPlayer, RangerEpicBoots);
				GiveItem(m_questPlayer, RangerEpicGloves);
				GiveItem(m_questPlayer, RangerEpicHelm);
				GiveItem(m_questPlayer, RangerEpicLegs);
				GiveItem(m_questPlayer, RangerEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Eldritch)
			{
				GiveItem(m_questPlayer, EldritchEpicArms);
				GiveItem(m_questPlayer, EldritchEpicBoots);
				GiveItem(m_questPlayer, EldritchEpicGloves);
				GiveItem(m_questPlayer, EldritchEpicHelm);
				GiveItem(m_questPlayer, EldritchEpicLegs);
				GiveItem(m_questPlayer, EldritchEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warden)
			{
				GiveItem(m_questPlayer, WardenEpicArms);
				GiveItem(m_questPlayer, WardenEpicBoots);
				GiveItem(m_questPlayer, WardenEpicGloves);
				GiveItem(m_questPlayer, WardenEpicHelm);
				GiveItem(m_questPlayer, WardenEpicLegs);
				GiveItem(m_questPlayer, WardenEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mauler_Hib)
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
        *#25 talk to Ainrebh
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Ainrebh 
        *#28 give her the ball of flame
        *#29 talk with Ainrebh about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Mist Shrouded Boots 
            *Mist Shrouded Coif
            *Mist Shrouded Gloves
            *Mist Shrouded Hauberk
            *Mist Shrouded Legs
            *Mist Shrouded Sleeves
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
