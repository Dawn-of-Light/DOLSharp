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
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BMoonstone%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Moonstone Twin (level 50)
*Quest Classes  : Enchanter, Bard, Champion, Nighthsade(Path of Essence)
*Quest Version  : v1
*
*Done: 
*
*Bonuses to epic items 
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

namespace DOL.GS.Quests.Hibernia
{
	public class Essence_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Moonstone Twin";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Brigit = null; // Start NPC        
		private static GameNPC Caithor = null; // Mob to kill

		private static ItemTemplate Moonstone = null; //ball of flame

		private static ItemTemplate ChampionEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate ChampionEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate ChampionEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate ChampionEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate ChampionEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate ChampionEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate BardEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate BardEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate BardEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate BardEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate BardEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate BardEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EnchanterEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EnchanterEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EnchanterEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EnchanterEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EnchanterEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EnchanterEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate NightshadeEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NightshadeEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NightshadeEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NightshadeEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NightshadeEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NightshadeEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Essence_50() : base()
		{
		}

		public Essence_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Essence_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Essence_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Brigit", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Brigit , creating it ...");
				Brigit = new GameNPC();
				Brigit.Model = 384;
				Brigit.Name = "Brigit";
				Brigit.GuildName = "";
				Brigit.Realm = eRealm.Hibernia;
				Brigit.CurrentRegionID = 201;
				Brigit.Size = 51;
				Brigit.Level = 50;
				Brigit.X = 33131;
				Brigit.Y = 32922;
				Brigit.Z = 8008;
				Brigit.Heading = 3254;
				Brigit.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Brigit.SaveIntoDatabase();
				}

			}
			else
				Brigit = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Caithor", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Caithor , creating it ...");
				Caithor = new GameNPC();
				Caithor.Model = 339;
				Caithor.Name = "Caithor";
				Caithor.GuildName = "";
				Caithor.Realm = eRealm.None;
				Caithor.CurrentRegionID = 200;
				Caithor.Size = 60;
				Caithor.Level = 65;
				Caithor.X = 470547;
				Caithor.Y = 531497;
				Caithor.Z = 4984;
				Caithor.Heading = 3319;
				Caithor.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Caithor.SaveIntoDatabase();
				}

			}
			else
				Caithor = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			Moonstone = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "Moonstone");	
	
			BardEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicBoots");
			BardEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicHelm");
			BardEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicGloves");
			BardEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicVest");
			BardEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicLegs");
			BardEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicArms");
			
			ChampionEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicBoots");
			ChampionEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicHelm");
			ChampionEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicGloves");
			ChampionEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicVest");
			ChampionEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicLegs");
			ChampionEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicArms");
			
			NightshadeEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicBoots");
			NightshadeEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicHelm");
			NightshadeEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicGloves");
			NightshadeEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicVest");
			NightshadeEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicLegs");
			NightshadeEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicArms");
			
			EnchanterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicBoots");
			EnchanterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicHelm");
			EnchanterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicGloves");
			EnchanterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicVest");
			EnchanterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicLegs");
			EnchanterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicArms");
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.AddHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we bring to Brigit the possibility to give this quest to players */
			Brigit.AddQuestToGive(typeof (Essence_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Brigit == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.RemoveHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we remove to Brigit the possibility to give this quest to players */
			Brigit.RemoveQuestToGive(typeof (Essence_50));
		}

		protected static void TalkToBrigit(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Brigit.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Brigit.SayTo(player, "Hibernia needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Brigit, QuestMgr.GetIDForQuestType(typeof(Essence_50)), "Will you help Brigit [Path of Essence Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Champion &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bard &&
				player.CharacterClass.ID != (byte) eCharacterClass.Nightshade &&
				player.CharacterClass.ID != (byte) eCharacterClass.Enchanter)
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
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Essence_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Brigit.GiveQuest(typeof (Essence_50), player, 1))
					return;
				player.Out.SendMessage("Kill Caithor in Cursed Forest loc 28k 48k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Moonstone Twin (Level 50 Path of Essence Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Caithor in Cursed Forest Loc 20k,48k kill him!";
					case 2:
						return "[Step #2] Return to Brigit and give the Moonstone!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Essence_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Caithor.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Moonstone from Caithor", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, Moonstone);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Brigit.Name && gArgs.Item.TemplateID == Moonstone.TemplateID)
				{
					RemoveItem(Brigit, player, Moonstone);
					Brigit.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Moonstone, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Champion)
			{
				GiveItem(m_questPlayer, ChampionEpicArms);
				GiveItem(m_questPlayer, ChampionEpicBoots);
				GiveItem(m_questPlayer, ChampionEpicGloves);
				GiveItem(m_questPlayer, ChampionEpicHelm);
				GiveItem(m_questPlayer, ChampionEpicLegs);
				GiveItem(m_questPlayer, ChampionEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Bard)
			{
				GiveItem(m_questPlayer, BardEpicArms);
				GiveItem(m_questPlayer, BardEpicBoots);
				GiveItem(m_questPlayer, BardEpicGloves);
				GiveItem(m_questPlayer, BardEpicHelm);
				GiveItem(m_questPlayer, BardEpicLegs);
				GiveItem(m_questPlayer, BardEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Enchanter)
			{
				GiveItem(m_questPlayer, EnchanterEpicArms);
				GiveItem(m_questPlayer, EnchanterEpicBoots);
				GiveItem(m_questPlayer, EnchanterEpicGloves);
				GiveItem(m_questPlayer, EnchanterEpicHelm);
				GiveItem(m_questPlayer, EnchanterEpicLegs);
				GiveItem(m_questPlayer, EnchanterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Nightshade)
			{
				GiveItem(m_questPlayer, NightshadeEpicArms);
				GiveItem(m_questPlayer, NightshadeEpicBoots);
				GiveItem(m_questPlayer, NightshadeEpicGloves);
				GiveItem(m_questPlayer, NightshadeEpicHelm);
				GiveItem(m_questPlayer, NightshadeEpicLegs);
				GiveItem(m_questPlayer, NightshadeEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Brigit
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Brigit 
        *#28 give her the ball of flame
        *#29 talk with Brigit about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Moonsung Boots 
            *Moonsung Coif
            *Moonsung Gloves
            *Moonsung Hauberk
            *Moonsung Legs
            *Moonsung Sleeves
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
