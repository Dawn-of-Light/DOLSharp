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
*Quest Name     : An End to the Daggers (level 50)
*Quest Classes  : Warrior, Berserker, Thane, Skald, Savage (Viking)
*Quest Version  : v1
*
*Done:
*Bonuses to epic items
*
*ToDo:   
*   Find Helm ModelID for epics..
*   checks for all other epics done
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Viking_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "An End to the Daggers";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Lynnleigh = null; // Start NPC
		private static GameNPC Ydenia = null; // Mob to kill
		private static GameNPC Elizabeth = null; // reward NPC

		private static ItemTemplate tome_enchantments = null;
		private static ItemTemplate sealed_pouch = null;
		private static ItemTemplate WarriorEpicBoots = null;
		private static ItemTemplate WarriorEpicHelm = null;
		private static ItemTemplate WarriorEpicGloves = null;
		private static ItemTemplate WarriorEpicLegs = null;
		private static ItemTemplate WarriorEpicArms = null;
		private static ItemTemplate WarriorEpicVest = null;
		private static ItemTemplate BerserkerEpicBoots = null;
		private static ItemTemplate BerserkerEpicHelm = null;
		private static ItemTemplate BerserkerEpicGloves = null;
		private static ItemTemplate BerserkerEpicLegs = null;
		private static ItemTemplate BerserkerEpicArms = null;
		private static ItemTemplate BerserkerEpicVest = null;
		private static ItemTemplate ThaneEpicBoots = null;
		private static ItemTemplate ThaneEpicHelm = null;
		private static ItemTemplate ThaneEpicGloves = null;
		private static ItemTemplate ThaneEpicLegs = null;
		private static ItemTemplate ThaneEpicArms = null;
		private static ItemTemplate ThaneEpicVest = null;
		private static ItemTemplate SkaldEpicBoots = null;
		private static ItemTemplate SkaldEpicHelm = null;
		private static ItemTemplate SkaldEpicGloves = null;
		private static ItemTemplate SkaldEpicVest = null;
		private static ItemTemplate SkaldEpicLegs = null;
		private static ItemTemplate SkaldEpicArms = null;
		private static ItemTemplate SavageEpicBoots = null;
		private static ItemTemplate SavageEpicHelm = null;
		private static ItemTemplate SavageEpicGloves = null;
		private static ItemTemplate SavageEpicVest = null;
		private static ItemTemplate SavageEpicLegs = null;
		private static ItemTemplate SavageEpicArms = null;
		private static ItemTemplate ValkyrieEpicBoots = null;
		private static ItemTemplate ValkyrieEpicHelm = null;
		private static ItemTemplate ValkyrieEpicGloves = null;
		private static ItemTemplate ValkyrieEpicVest = null;
		private static ItemTemplate ValkyrieEpicLegs = null;
		private static ItemTemplate ValkyrieEpicArms = null;
		private static ItemTemplate MaulerEpicBoots = null;
		private static ItemTemplate MaulerEpicHelm = null;
		private static ItemTemplate MaulerEpicGloves = null;
		private static ItemTemplate MaulerEpicVest = null;
		private static ItemTemplate MaulerEpicLegs = null;
		private static ItemTemplate MaulerEpicArms = null;

		// Constructors
		public Viking_50() : base()
		{
		}

		public Viking_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Viking_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Viking_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lynnleigh", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lynnleigh , creating it ...");
				Lynnleigh = new GameNPC();
				Lynnleigh.Model = 217;
				Lynnleigh.Name = "Lynnleigh";
				Lynnleigh.GuildName = "";
				Lynnleigh.Realm = eRealm.Midgard;
				Lynnleigh.CurrentRegionID = 100;
				Lynnleigh.Size = 51;
				Lynnleigh.Level = 50;
				Lynnleigh.X = 760085;
				Lynnleigh.Y = 758453;
				Lynnleigh.Z = 4736;
				Lynnleigh.Heading = 2197;
				Lynnleigh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lynnleigh.SaveIntoDatabase();
				}
			}
			else
				Lynnleigh = npcs[0];
			// end npc
			npcs = WorldMgr.GetNPCsByName("Elizabeth", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Elizabeth , creating it ...");
				Elizabeth = new GameNPC();
				Elizabeth.Model = 217;
				Elizabeth.Name = "Elizabeth";
				Elizabeth.GuildName = "Enchanter";
				Elizabeth.Realm = eRealm.Midgard;
				Elizabeth.CurrentRegionID = 100;
				Elizabeth.Size = 51;
				Elizabeth.Level = 41;
				Elizabeth.X = 802849;
				Elizabeth.Y = 727081;
				Elizabeth.Z = 4681;
				Elizabeth.Heading = 2480;
				Elizabeth.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Elizabeth.SaveIntoDatabase();
				}

			}
			else
				Elizabeth = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Ydenia of Seithkona", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ydenia , creating it ...");
				Ydenia = new GameNPC();
				Ydenia.Model = 217;
				Ydenia.Name = "Ydenia of Seithkona";
				Ydenia.GuildName = "";
				Ydenia.Realm = eRealm.None;
				Ydenia.CurrentRegionID = 100;
				Ydenia.Size = 100;
				Ydenia.Level = 65;
				Ydenia.X = 637680;
				Ydenia.Y = 767189;
				Ydenia.Z = 4480;
				Ydenia.Heading = 2156;
				Ydenia.Flags ^= (uint)GameNPC.eFlags.TRANSPARENT;
				Ydenia.MaxSpeedBase = 200;
				Ydenia.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ydenia.SaveIntoDatabase();
				}

			}
			else
				Ydenia = npcs[0];
			// end npc

			#endregion

			#region defineItems
			tome_enchantments = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "tome_enchantments");
			sealed_pouch = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sealed_pouch");
			WarriorEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicBoots");
			WarriorEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicHelm");
			WarriorEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicGloves");
			WarriorEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicVest");
			WarriorEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicLegs");
			WarriorEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicArms");
			BerserkerEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicBoots");
			BerserkerEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicHelm");
			BerserkerEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicGloves");
			BerserkerEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicVest");
			BerserkerEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicLegs");
			BerserkerEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicArms");
			ThaneEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicBoots");
			ThaneEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicHelm");
			ThaneEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicGloves");
			ThaneEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicVest");
			ThaneEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicLegs");
			ThaneEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicArms");
			SkaldEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicBoots");
			SkaldEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicHelm");
			SkaldEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicGloves");
			SkaldEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicVest");
			SkaldEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicLegs");
			SkaldEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicArms");
			SavageEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicBoots");
			SavageEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicHelm");
			SavageEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicGloves");
			SavageEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicVest");
			SavageEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicLegs");
			SavageEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicArms");
			#region Valkyrie
			ValkyrieEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicBoots");
			ValkyrieEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicHelm");
			ValkyrieEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicGloves");
			ValkyrieEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicVest");
			ValkyrieEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicLegs");
			ValkyrieEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ValkyrieEpicArms");
			#endregion
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

			GameEventMgr.AddHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.AddHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));

			/* Now we bring to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.AddQuestToGive(typeof (Viking_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Lynnleigh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.RemoveHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));
		
			/* Now we remove to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.RemoveQuestToGive(typeof (Viking_50));
		}

		protected static void TalkToLynnleigh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Lynnleigh.SayTo(player, "Check your Journal for information about what to do!");
				}
				else
				{
					Lynnleigh.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Ydenia to dispose of him. He also has a note here about how strong Ydenia really was. That [worries me].");
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
							Lynnleigh.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Ydenia] and his minions.");
							break;
						case "face Ydenia":
							player.Out.SendQuestSubscribeCommand(Lynnleigh, QuestMgr.GetIDForQuestType(typeof(Viking_50)), "Will you face Ydenia [Viking Level 50 Epic]?");
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

		protected static void TalkToElizabeth(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Lynnleigh.CanGiveQuest(typeof(Viking_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof(Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 4:
							Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
							break;
					}
				}
			}
			// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 4)
							{
								if (MaulerEpicArms == null || MaulerEpicBoots == null || MaulerEpicGloves == null ||
									MaulerEpicHelm == null || MaulerEpicLegs == null || MaulerEpicVest == null)
								{
									Elizabeth.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
									return;
								}
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Warrior &&
				player.CharacterClass.ID != (byte) eCharacterClass.Berserker &&
				player.CharacterClass.ID != (byte) eCharacterClass.Thane &&
				player.CharacterClass.ID != (byte) eCharacterClass.Skald &&
				player.CharacterClass.ID != (byte) eCharacterClass.Savage &&
				player.CharacterClass.ID != (byte) eCharacterClass.Valkyrie &&
				player.CharacterClass.ID != (byte) eCharacterClass.Mauler_Mid)
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
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Viking_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Lynnleigh.GiveQuest(typeof (Viking_50), player, 1))
					return;

				Lynnleigh.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Ydenia is strong. He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, I would highley recommed taking some friends with you to face Ydenia. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. According to the map you can find Ydenia in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Ydenia and his followers. Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "An End to the Daggers (level 50 Viking epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Ydenia in Raumarik Loc 48k, 30k kill her!";
					case 2:
						return "[Step #2] Return to Lynnleigh and give her tome of Enchantments!";
					case 3:
						return "[Step #3] Take the Sealed Pouch to Elizabeth in Mularn";
					case 4:
						return "[Step #4] Tell Elizabeth you can 'take them' for your rewards!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Viking_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == Ydenia.Name)
				{
					Step = 2;
					GiveItem(m_questPlayer, tome_enchantments);
					m_questPlayer.Out.SendMessage("Ydenia drops the Tome of Enchantments and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Lynnleigh.Name && gArgs.Item.TemplateID == tome_enchantments.TemplateID)
				{
					RemoveItem(Lynnleigh, player, tome_enchantments);
					Lynnleigh.SayTo(player, "Take this sealed pouch to Elizabeth in Mularn for your reward!");
					GiveItem(Lynnleigh, player, sealed_pouch);
					Step = 3;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Elizabeth.Name && gArgs.Item.TemplateID == sealed_pouch.TemplateID)
				{
					RemoveItem(Elizabeth, player, sealed_pouch);
					Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 4;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, tome_enchantments, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
			{
				case eCharacterClass.Warrior:
					{
						GiveItem(m_questPlayer, WarriorEpicArms);
						GiveItem(m_questPlayer, WarriorEpicBoots);
						GiveItem(m_questPlayer, WarriorEpicGloves);
						GiveItem(m_questPlayer, WarriorEpicHelm);
						GiveItem(m_questPlayer, WarriorEpicLegs);
						GiveItem(m_questPlayer, WarriorEpicVest);
						break;
					}
				case eCharacterClass.Berserker:
					{
						GiveItem(m_questPlayer, BerserkerEpicArms);
						GiveItem(m_questPlayer, BerserkerEpicBoots);
						GiveItem(m_questPlayer, BerserkerEpicGloves);
						GiveItem(m_questPlayer, BerserkerEpicHelm);
						GiveItem(m_questPlayer, BerserkerEpicLegs);
						GiveItem(m_questPlayer, BerserkerEpicVest);
						break;
					}
				case eCharacterClass.Thane:
					{
						GiveItem(m_questPlayer, ThaneEpicArms);
						GiveItem(m_questPlayer, ThaneEpicBoots);
						GiveItem(m_questPlayer, ThaneEpicGloves);
						GiveItem(m_questPlayer, ThaneEpicHelm);
						GiveItem(m_questPlayer, ThaneEpicLegs);
						GiveItem(m_questPlayer, ThaneEpicVest);
						break;
					}
				case eCharacterClass.Skald:
					{
						GiveItem(m_questPlayer, SkaldEpicArms);
						GiveItem(m_questPlayer, SkaldEpicBoots);
						GiveItem(m_questPlayer, SkaldEpicGloves);
						GiveItem(m_questPlayer, SkaldEpicHelm);
						GiveItem(m_questPlayer, SkaldEpicLegs);
						GiveItem(m_questPlayer, SkaldEpicVest);
						break;
					}
				case eCharacterClass.Savage:
					{
						GiveItem(m_questPlayer, SavageEpicArms);
						GiveItem(m_questPlayer, SavageEpicBoots);
						GiveItem(m_questPlayer, SavageEpicGloves);
						GiveItem(m_questPlayer, SavageEpicHelm);
						GiveItem(m_questPlayer, SavageEpicLegs);
						GiveItem(m_questPlayer, SavageEpicVest);
						break;
					}
				case eCharacterClass.Valkyrie:
					{
						GiveItem(m_questPlayer, ValkyrieEpicArms);
						GiveItem(m_questPlayer, ValkyrieEpicBoots);
						GiveItem(m_questPlayer, ValkyrieEpicGloves);
						GiveItem(m_questPlayer, ValkyrieEpicHelm);
						GiveItem(m_questPlayer, ValkyrieEpicLegs);
						GiveItem(m_questPlayer, ValkyrieEpicVest);
						break;
					}
				case eCharacterClass.Mauler_Mid:
					{
						GiveItem(m_questPlayer, MaulerEpicArms);
						GiveItem(m_questPlayer, MaulerEpicBoots);
						GiveItem(m_questPlayer, MaulerEpicGloves);
						GiveItem(m_questPlayer, MaulerEpicHelm);
						GiveItem(m_questPlayer, MaulerEpicLegs);
						GiveItem(m_questPlayer, MaulerEpicVest);
						break;
					}
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}
	}
}
