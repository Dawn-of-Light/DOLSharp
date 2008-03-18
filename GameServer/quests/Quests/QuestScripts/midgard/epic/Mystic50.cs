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
*               : http://daoc.warcry.com/quests/display_spoilquest.php?id=741
*Date           : 22 November 2004
*Quest Name     : Saving the Clan (Level 50)
*Quest Classes  : Runemaster, Bonedancer, Spiritmaster (Mystic)
*Quest Version  : v1.2
*
*Changes:
*   Fixed the texts to be like live.
*   The epic armor should now be described correctly
*   The epic armor should now fit into the correct slots
*   The epic armor should now have the correct durability and condition
*   The armour will now be correctly rewarded with all pieces
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*
*ToDo:
*   Find Helm ModelID for epics..
*   checks for all other epics done, once they are implemented
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Mystic_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Saving the Clan";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Danica = null; // Start NPC
		private static GameNPC Kelic = null; // Mob to kill

		private static ItemTemplate kelics_totem = null;
		private static ItemTemplate SpiritmasterEpicBoots = null;
		private static ItemTemplate SpiritmasterEpicHelm = null;
		private static ItemTemplate SpiritmasterEpicGloves = null;
		private static ItemTemplate SpiritmasterEpicLegs = null;
		private static ItemTemplate SpiritmasterEpicArms = null;
		private static ItemTemplate SpiritmasterEpicVest = null;
		private static ItemTemplate RunemasterEpicBoots = null;
		private static ItemTemplate RunemasterEpicHelm = null;
		private static ItemTemplate RunemasterEpicGloves = null;
		private static ItemTemplate RunemasterEpicLegs = null;
		private static ItemTemplate RunemasterEpicArms = null;
		private static ItemTemplate RunemasterEpicVest = null;
		private static ItemTemplate BonedancerEpicBoots = null;
		private static ItemTemplate BonedancerEpicHelm = null;
		private static ItemTemplate BonedancerEpicGloves = null;
		private static ItemTemplate BonedancerEpicLegs = null;
		private static ItemTemplate BonedancerEpicArms = null;
		private static ItemTemplate BonedancerEpicVest = null;
		private static ItemTemplate WarlockEpicBoots = null;
		private static ItemTemplate WarlockEpicHelm = null;
		private static ItemTemplate WarlockEpicGloves = null;
		private static ItemTemplate WarlockEpicLegs = null;
		private static ItemTemplate WarlockEpicArms = null;
		private static ItemTemplate WarlockEpicVest = null;

		// Constructors
		public Mystic_50() : base()
		{
		}

		public Mystic_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Danica", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Danica , creating it ...");
				Danica = new GameNPC();
				Danica.Model = 227;
				Danica.Name = "Danica";
				Danica.GuildName = "";
				Danica.Realm = eRealm.Midgard;
				Danica.CurrentRegionID = 100;
				Danica.Size = 51;
				Danica.Level = 50;
				Danica.X = 804440;
				Danica.Y = 722267;
				Danica.Z = 4719;
				Danica.Heading = 2116;
				Danica.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Danica.SaveIntoDatabase();
				}
			}
			else
				Danica = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Kelic", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic , creating it ...");
				Kelic = new GameNPC();
				Kelic.Model = 26;
				Kelic.Name = "Kelic";
				Kelic.GuildName = "";
				Kelic.Realm = eRealm.None;
				Kelic.CurrentRegionID = 100;
				Kelic.Size = 100;
				Kelic.Level = 65;
				Kelic.X = 621577;
				Kelic.Y = 745848;
				Kelic.Z = 4593;
				Kelic.Heading = 3538;
				Kelic.Flags ^= (uint)GameNPC.eFlags.TRANSPARENT;
				Kelic.MaxSpeedBase = 200;
				Kelic.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Kelic.SaveIntoDatabase();
				}
			}
			else
				Kelic = npcs[0];
			// end npc

			#endregion

			#region defineItems
			kelics_totem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "kelics_totem");
			SpiritmasterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicBoots");
			SpiritmasterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicHelm");
			SpiritmasterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicGloves");
			SpiritmasterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicVest");
			SpiritmasterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicLegs");
			SpiritmasterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicArms");
			RunemasterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicBoots");
			RunemasterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicHelm");
			RunemasterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicGloves");
			RunemasterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicVest");
			RunemasterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicLegs");
			RunemasterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicArms");
			BonedancerEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicBoots");
			BonedancerEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicHelm");
			BonedancerEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicGloves");
			BonedancerEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicVest");
			BonedancerEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicLegs");
			BonedancerEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicArms");
			#region Warlock
			WarlockEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicBoots");
			WarlockEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicHelm");
			WarlockEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicGloves");
			WarlockEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicVest");
			WarlockEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicLegs");
			WarlockEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "WarlockEpicArms");
			#endregion
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.AddHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));

			/* Now we bring to Danica the possibility to give this quest to players */
			Danica.AddQuestToGive(typeof (Mystic_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Danica == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.RemoveHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));

			/* Now we remove to Danica the possibility to give this quest to players */
			Danica.RemoveQuestToGive(typeof (Mystic_50));
		}

		protected static void TalkToDanica(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 1:
							Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
								"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
								"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
								"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
								"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
								"Return to me when you have the totem. May all the gods be with you.");
							break;
						case 2:
							Danica.SayTo(player, "It is good to see you were strong enough to survive Kelic. I can sense you have the controlling totem on you. Give me Kelic's totem now! Hurry!");
							quest.Step = 3;
							break;
						case 3:
							Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
							break;
					}
				}
				else
				{
					Danica.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Kelic to dispose of him. He also has a note here about how strong Kelic really was. That [worries me].");
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "worries me":
							Danica.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Kelic] and his minions.");
							break;
						case "face Kelic":
							player.Out.SendQuestSubscribeCommand(Danica, QuestMgr.GetIDForQuestType(typeof(Mystic_50)), "Will you face Kelic [Mystic Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 3)
								quest.FinishQuest();
							break;

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
			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Spiritmaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Runemaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bonedancer &&
				player.CharacterClass.ID != (byte) eCharacterClass.Warlock)
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
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Mystic_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Danica.GiveQuest(typeof (Mystic_50), player, 1))
					return;

				Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
					"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
					"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
					"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
					"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
					"Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Saving the Clan (Level 50 Mystic Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. Cross the river and head west. Follow the snowline until you reach a group of trees.";
					case 2:
						return "[Step #2] Return to Danica and give her the totem!";
					case 3:
						return "[Step #3] Tell Danica you can 'take them' for your rewards!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Mystic_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Kelic.Name)
				{
					Step = 2;
					GiveItem(m_questPlayer, kelics_totem);
					m_questPlayer.Out.SendMessage("Kelic drops his Totem and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Danica.Name && gArgs.Item.TemplateID == kelics_totem.TemplateID)
				{
					RemoveItem(Danica, player, kelics_totem);
					Danica.SayTo(player, "Ah, I can see how he wore the curse around the totem. I can now break the curse that is destroying the clan!");
					Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 3;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, kelics_totem, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
			{
				case eCharacterClass.Spiritmaster:
					{
						GiveItem(m_questPlayer, SpiritmasterEpicArms);
						GiveItem(m_questPlayer, SpiritmasterEpicBoots);
						GiveItem(m_questPlayer, SpiritmasterEpicGloves);
						GiveItem(m_questPlayer, SpiritmasterEpicHelm);
						GiveItem(m_questPlayer, SpiritmasterEpicLegs);
						GiveItem(m_questPlayer, SpiritmasterEpicVest);
						break;
					}
				case eCharacterClass.Runemaster:
					{
						GiveItem(m_questPlayer, RunemasterEpicArms);
						GiveItem(m_questPlayer, RunemasterEpicBoots);
						GiveItem(m_questPlayer, RunemasterEpicGloves);
						GiveItem(m_questPlayer, RunemasterEpicHelm);
						GiveItem(m_questPlayer, RunemasterEpicLegs);
						GiveItem(m_questPlayer, RunemasterEpicVest);
						break;
					}
				case eCharacterClass.Bonedancer:
					{
						GiveItem(m_questPlayer, BonedancerEpicArms);
						GiveItem(m_questPlayer, BonedancerEpicBoots);
						GiveItem(m_questPlayer, BonedancerEpicGloves);
						GiveItem(m_questPlayer, BonedancerEpicHelm);
						GiveItem(m_questPlayer, BonedancerEpicLegs);
						GiveItem(m_questPlayer, BonedancerEpicVest);
						break;
					}
				case eCharacterClass.Warlock:
					{
						GiveItem(m_questPlayer, WarlockEpicArms);
						GiveItem(m_questPlayer, WarlockEpicBoots);
						GiveItem(m_questPlayer, WarlockEpicGloves);
						GiveItem(m_questPlayer, WarlockEpicHelm);
						GiveItem(m_questPlayer, WarlockEpicLegs);
						GiveItem(m_questPlayer, WarlockEpicVest);
						break;
					}
			}
			Danica.SayTo(m_questPlayer, "May it serve you well, knowing that you have helped preserve the history of Midgard!");

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
Spirit Touched Boots 
Spirit Touched Cap 
Spirit Touched Gloves 
Spirit Touched Pants 
Spirit Touched Sleeves 
Spirit Touched Vest 
Raven-Rune Boots 
Raven-Rune Cap 
Raven-Rune Gloves 
Raven-Rune Pants 
Raven-Rune Sleeves 
Raven-Rune Vest 
Raven-boned Boots 
Raven-Boned Cap 
Raven-boned Gloves 
Raven-Boned Pants 
Raven-Boned Sleeves 
Bone-rune Vest 
        */

		#endregion
	}
}
