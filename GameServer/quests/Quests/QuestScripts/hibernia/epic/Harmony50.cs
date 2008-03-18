/*
*Author         : Etaew - Fallen Realms
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BHorn%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Horn Twin (level 50)
*Quest Classes  : Mentalist, Druid, Blademaster, Nighthsade(Path of Essence)
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
	public class Harmony_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Horn Twin";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Revelin = null; // Start NPC
		//private static GameNPC Lauralaye = null; //Reward NPC
		private static GameNPC Cailean = null; // Mob to kill

		private static ItemTemplate Horn = null; //ball of flame        
		private static ItemTemplate BlademasterEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate BlademasterEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate BlademasterEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate BlademasterEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate BlademasterEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate BlademasterEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate DruidEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate DruidEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate DruidEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate DruidEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate DruidEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate DruidEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate MentalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate MentalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate MentalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate MentalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate MentalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate MentalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate AnimistEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate AnimistEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate AnimistEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate AnimistEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate AnimistEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate AnimistEpicArms = null; //Subterranean Sleeves 
		private static ItemTemplate ValewalkerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ValewalkerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ValewalkerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ValewalkerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ValewalkerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ValewalkerEpicArms = null; //Subterranean Sleeves  
		private static ItemTemplate VampiirEpicBoots = null;
		private static ItemTemplate VampiirEpicHelm = null;
		private static ItemTemplate VampiirEpicGloves = null;
		private static ItemTemplate VampiirEpicVest = null;
		private static ItemTemplate VampiirEpicLegs = null;
		private static ItemTemplate VampiirEpicArms = null;
		private static ItemTemplate BainsheeEpicBoots = null;
		private static ItemTemplate BainsheeEpicHelm = null;
		private static ItemTemplate BainsheeEpicGloves = null;
		private static ItemTemplate BainsheeEpicVest = null;
		private static ItemTemplate BainsheeEpicLegs = null;
		private static ItemTemplate BainsheeEpicArms = null;

		// Constructors
		public Harmony_50()
			: base()
		{
		}

		public Harmony_50(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		public Harmony_50(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public Harmony_50(GamePlayer questingPlayer, DBQuest dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Revelin", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Revelin , creating it ...");
				Revelin = new GameNPC();
				Revelin.Model = 361;
				Revelin.Name = "Revelin";
				Revelin.GuildName = "";
				Revelin.Realm = eRealm.Hibernia;
				Revelin.CurrentRegionID = 200;
				Revelin.Size = 42;
				Revelin.Level = 20;
				Revelin.X = 344387;
				Revelin.Y = 706197;
				Revelin.Z = 6351;
				Revelin.Heading = 2127;
				Revelin.Flags ^= (uint)GameNPC.eFlags.PEACE;
				Revelin.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Revelin.SaveIntoDatabase();
				}

			}
			else
				Revelin = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailean", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Cailean , creating it ...");
				Cailean = new GameNPC();
				Cailean.Model = 98;
				Cailean.Name = "Cailean";
				Cailean.GuildName = "";
				Cailean.Realm = eRealm.None;
				Cailean.CurrentRegionID = 200;
				Cailean.Size = 60;
				Cailean.Level = 65;
				Cailean.X = 479042;
				Cailean.Y = 508134;
				Cailean.Z = 4569;
				Cailean.Heading = 3319;
				Cailean.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Cailean.SaveIntoDatabase();
				}

			}
			else
				Cailean = npcs[0];
			// end npc

			#endregion

			#region Item Declarations
			Horn = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "Horn");
			DruidEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicBoots");
			DruidEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicHelm");
			DruidEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicGloves");
			DruidEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicVest");
			DruidEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicLegs");
			DruidEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "DruidEpicArms");
			BlademasterEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicBoots");
			BlademasterEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicHelm");
			BlademasterEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicGloves");
			BlademasterEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicVest");
			BlademasterEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicLegs");
			BlademasterEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BlademasterEpicArms");
			AnimistEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicBoots");
			AnimistEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicHelm");
			AnimistEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicGloves");
			AnimistEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicVest");
			AnimistEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicLegs");
			AnimistEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "AnimistEpicArms");
			MentalistEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicBoots");
			MentalistEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicHelm");
			MentalistEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicGloves");
			MentalistEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicVest");
			MentalistEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicLegs");
			MentalistEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MentalistEpicArms");
			ValewalkerEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicBoots");
			ValewalkerEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicHelm");
			ValewalkerEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicGloves");
			ValewalkerEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicVest");
			ValewalkerEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicLegs");
			ValewalkerEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValewalkerEpicArms");
			#region Vampiir
			VampiirEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicBoots");
			VampiirEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicHelm");
			VampiirEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicGloves");
			VampiirEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicVest");
			VampiirEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicLegs");
			VampiirEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "VampiirEpicArms");
			#endregion
			#region Bainshee
			BainsheeEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicBoots");
			BainsheeEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicHelm");
			BainsheeEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicGloves");
			BainsheeEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicVest");
			BainsheeEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicLegs");
			BainsheeEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BainsheeEpicArms");
			#endregion

			//Blademaster Epic Sleeves End
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.AddHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));

			/* Now we bring to Revelin the possibility to give this quest to players */
			Revelin.AddQuestToGive(typeof(Harmony_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Revelin == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.RemoveHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));

			/* Now we remove to Revelin the possibility to give this quest to players */
			Revelin.RemoveQuestToGive(typeof(Harmony_50));
		}

		protected static void TalkToRevelin(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Revelin.CanGiveQuest(typeof(Harmony_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Revelin.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Revelin.SayTo(player, "Hibernia needs your [services]");
				}
			}
			// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Revelin, QuestMgr.GetIDForQuestType(typeof(Harmony_50)), "Will you help Revelin [Path of Harmony Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Blademaster &&
				player.CharacterClass.ID != (byte)eCharacterClass.Druid &&
				player.CharacterClass.ID != (byte)eCharacterClass.Valewalker &&
				player.CharacterClass.ID != (byte)eCharacterClass.Animist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mentalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Vampiir &&
				player.CharacterClass.ID != (byte)eCharacterClass.Bainshee)
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
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Harmony_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (Revelin.CanGiveQuest(typeof(Harmony_50), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Revelin.GiveQuest(typeof(Harmony_50), player, 1))
					return;
				player.Out.SendMessage("Kill Cailean in Cursed Forest loc 28k 24k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Horn Twin (Level 50 Path of Harmony Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailean in Cursed Forest Loc 28k,24k kill him!";
					case 2:
						return "[Step #2] Return to Revelin and give the Horn!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Harmony_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == Cailean.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Horn from Cailean", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, Horn);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Revelin.Name && gArgs.Item.TemplateID == Horn.TemplateID)
				{
					RemoveItem(Revelin, player, Horn);
					Revelin.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Horn, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
			{
				case eCharacterClass.Blademaster:
					{
						GiveItem(m_questPlayer, BlademasterEpicArms);
						GiveItem(m_questPlayer, BlademasterEpicBoots);
						GiveItem(m_questPlayer, BlademasterEpicGloves);
						GiveItem(m_questPlayer, BlademasterEpicHelm);
						GiveItem(m_questPlayer, BlademasterEpicLegs);
						GiveItem(m_questPlayer, BlademasterEpicVest);
						break;
					}
				case eCharacterClass.Animist:
					{
						GiveItem(m_questPlayer, AnimistEpicArms);
						GiveItem(m_questPlayer, AnimistEpicBoots);
						GiveItem(m_questPlayer, AnimistEpicGloves);
						GiveItem(m_questPlayer, AnimistEpicHelm);
						GiveItem(m_questPlayer, AnimistEpicLegs);
						GiveItem(m_questPlayer, AnimistEpicVest);
						break;
					}
				case eCharacterClass.Mentalist:
					{
						GiveItem(m_questPlayer, MentalistEpicArms);
						GiveItem(m_questPlayer, MentalistEpicBoots);
						GiveItem(m_questPlayer, MentalistEpicGloves);
						GiveItem(m_questPlayer, MentalistEpicHelm);
						GiveItem(m_questPlayer, MentalistEpicLegs);
						GiveItem(m_questPlayer, MentalistEpicVest);
						break;
					}
				case eCharacterClass.Druid:
					{
						GiveItem(m_questPlayer, DruidEpicArms);
						GiveItem(m_questPlayer, DruidEpicBoots);
						GiveItem(m_questPlayer, DruidEpicGloves);
						GiveItem(m_questPlayer, DruidEpicHelm);
						GiveItem(m_questPlayer, DruidEpicLegs);
						GiveItem(m_questPlayer, DruidEpicVest);
						break;
					}
				case eCharacterClass.Valewalker:
					{
						GiveItem(m_questPlayer, ValewalkerEpicArms);
						GiveItem(m_questPlayer, ValewalkerEpicBoots);
						GiveItem(m_questPlayer, ValewalkerEpicGloves);
						GiveItem(m_questPlayer, ValewalkerEpicHelm);
						GiveItem(m_questPlayer, ValewalkerEpicLegs);
						GiveItem(m_questPlayer, ValewalkerEpicVest);
						break;
					}
				case eCharacterClass.Vampiir:
					{
						GiveItem(m_questPlayer, VampiirEpicArms);
						GiveItem(m_questPlayer, VampiirEpicBoots);
						GiveItem(m_questPlayer, VampiirEpicGloves);
						GiveItem(m_questPlayer, VampiirEpicHelm);
						GiveItem(m_questPlayer, VampiirEpicLegs);
						GiveItem(m_questPlayer, VampiirEpicVest);
						break;
					}
				case eCharacterClass.Bainshee:
					{
						GiveItem(m_questPlayer, BainsheeEpicArms);
						GiveItem(m_questPlayer, BainsheeEpicBoots);
						GiveItem(m_questPlayer, BainsheeEpicGloves);
						GiveItem(m_questPlayer, BainsheeEpicHelm);
						GiveItem(m_questPlayer, BainsheeEpicLegs);
						GiveItem(m_questPlayer, BainsheeEpicVest);
						break;
					}
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Revelin
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Revelin 
        *#28 give her the ball of flame
        *#29 talk with Revelin about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Sidhe Scale Boots 
            *Sidhe Scale Coif
            *Sidhe Scale Gloves
            *Sidhe Scale Hauberk
            *Sidhe Scale Legs
            *Sidhe Scale Sleeves
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
