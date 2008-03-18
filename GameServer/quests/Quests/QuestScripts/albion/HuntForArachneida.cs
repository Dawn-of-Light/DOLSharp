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
 * Author:		Gandulf Kohlweiss
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *
 * Talk to Kealan, in Campacorentin Station, and he asks you to kill a Giant spider named Arachineida and you
 * have to kill a bloated spider first to get a Bloated spider fang and then you have to kill Arachneida who
 * spawns with the bloated spiders. You do not have to go back till you have both fang and Chitin.
 * The Bloated spiders and Arachneida can be found just East of Caer Ulywuch, right past the undead 
 * druids and ghostly knights. She spawns at 52k, 29k. This quest will only be given to you at 12 or 13+ and 
 * the bloated spiders are blue to 13th I think (they were green at 14th) and Archinae was blue to 14th.
 * The reward is pretty good as you get a different item for each type of class.
 */

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 
	 * This quest for example will be stored in the database with
	 * the name: DOL.GS.Quests.Albion.HuntForArachneida
	 */

	public class HuntForArachneida : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		 * You can declare static variables here, which will be available in 
		 * ALL instance of your quest and should be initialized ONLY ONCE inside
		 * the OnScriptLoaded method.
		 * 
		 * Or declare nonstatic variables here which can be unique for each Player
		 * and change through the quest journey...
		 * 
		 * We store our two mobs as static variables, since we need them
		 */

		protected const string questTitle = "The Hunt for Arachneida";
		protected const int minimumLevel = 14;
		protected const int maximumLevel = 50;

		private static GameNPC kealan = null;

		private static GameNPC arachneida = null;

		private static ItemTemplate spiderSilkenRobe = null;
		private static ItemTemplate ringedSpiderChitinTunic = null;
		private static ItemTemplate studdedSpiderEyeVest = null;
		private static ItemTemplate spiderEmblazonedTunic = null;
		private static ItemTemplate embossedSpiderTunic = null;

		private static ItemTemplate bloatedFang = null;
		private static ItemTemplate spiderChitin = null;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public HuntForArachneida() : base()
		{
		}

		public HuntForArachneida(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public HuntForArachneida(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public HuntForArachneida(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}


		/* The following method is called automatically when this quest class
		 * is loaded. You might notice that this method is the same as in standard
		 * game events. And yes, quests basically are game events for single players
		 * 
		 * To make this method automatically load, we have to declare it static
		 * and give it the [ScriptLoadedEvent] attribute. 
		 *
		 * Inside this method we initialize the quest. This is neccessary if we 
		 * want to set the quest hooks to the NPCs.
		 * 
		 * If you want, you can however add a quest to the player from ANY place
		 * inside your code, from events, from custom items, from anywhere you
		 * want. We will do it the standard way here ... and make Sir Quait wail
		 * a bit about the loss of his sword! 
		 */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");
			/* First thing we do in here is to search for the NPCs inside
			* the world who comes from the certain Realm. If we find a the players,
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Kealan", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no Sir Quait exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				kealan = new GameNPC();
				kealan.Model = 281;
				kealan.Name = "Kealan";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + kealan.Name + ", creating him ...");

				kealan.GuildName = "Part of " + questTitle + " Quest";
				kealan.Realm = eRealm.Albion;
				kealan.CurrentRegionID = 1;
				kealan.Size = 48;
				kealan.Level = 32;
				kealan.X = 493414;
				kealan.Y = 593089;
				kealan.Z = 1797;
				kealan.Heading = 830;

				kealan.EquipmentTemplateID = "11704675";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					kealan.SaveIntoDatabase();


				kealan.AddToWorld();
			}
			else
				kealan = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Arachneida", eRealm.None);
			if (npcs.Length == 0)
			{
				arachneida = new GameNPC();
				arachneida.Model = 72;
				arachneida.Name = "Arachneida";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + arachneida.Name + ", creating her ...");

				arachneida.GuildName = "Part of " + questTitle + " Quest";
				arachneida.Realm = eRealm.None;
				arachneida.CurrentRegionID = 1;
				arachneida.Size = 90;
				arachneida.Level = 12;
				arachneida.X = 534851;
				arachneida.Y = 609656;
				arachneida.Z = 2456;
				arachneida.Heading = 2080;

				arachneida.EquipmentTemplateID = "2";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					arachneida.SaveIntoDatabase();

				arachneida.AddToWorld();
			}
			else
				arachneida = npcs[0];

			#endregion

			#region defineItems
			spiderSilkenRobe = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "spider_silken_robe");
			ringedSpiderChitinTunic = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ringed_spider_chitin_tunic");
			studdedSpiderEyeVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "studded_spider_eye_vest");
			spiderEmblazonedTunic = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "spider_emblazoned_tunic");
			embossedSpiderTunic = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "embossed_spider_tunic");
			bloatedFang = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "bloated_spider_fang");
			spiderChitin = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "arachneida_spider_chitin");
			#endregion

			/* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(kealan, GameLivingEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.AddHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			/* Now we bring to kealan the possibility to give this quest to players */
			kealan.AddQuestToGive(typeof (HuntForArachneida));	

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			/* If sirQuait has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (kealan == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(kealan, GameLivingEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.RemoveHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			/* Now we remove to kealan the possibility to give this quest to players */
			kealan.RemoveQuestToGive(typeof (HuntForArachneida));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToKealan(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(kealan.CanGiveQuest(typeof (HuntForArachneida), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			HuntForArachneida quest = player.IsDoingQuest(typeof (HuntForArachneida)) as HuntForArachneida;

			kealan.TurnTo(player);
			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (quest == null)
				{
					//If the player qualifies, we begin talking...                    
					kealan.SayTo(player, "Aye, hello there! Have ye come to hunt the [giant spider] deep down in the [forrest]?");
					return;
				}
				else
				{
					//If the player is already doing the quest, we ask if he found the fur!
					if (player.Inventory.GetFirstItemByID(bloatedFang.TemplateID, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
						kealan.SayTo(player, "Good, you managed to retrieve a bloated spider fang, but did you also slay Arachneida?");
					else if (player.Inventory.GetFirstItemByID(spiderChitin.TemplateID, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
						kealan.SayTo(player, "Ah, I see you killed her, I knew you were other than the rest. Now hand me over the chitin and fang so that I can give you your reward.");
					else
						kealan.SayTo(player, "Go now, and bring back her chitin as prof of your success.");
					return;
				}
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//We also check if the player is already doing the quest
				if (quest == null)
				{
					//Do some small talk :)                    
					switch (wArgs.Text)
					{
						case "giant spider":
							kealan.SayTo(player, "Ay, she's that big she even got her own name, everybody calls her \"Arachneida\". But take care nobody who went out to [hunt] her ever returned.");
							break;
						case "forrest":
							kealan.SayTo(player, "She lives in the forrest east of here. It's a dark forrest full of bloated spiders so take care of yourself if you go there.");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "hunt":
							player.Out.SendQuestSubscribeCommand(kealan, QuestMgr.GetIDForQuestType(typeof(HuntForArachneida)), "Do you want to hunt Arachneida?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "bloated spider fang":
							kealan.SayTo(player, "There should be lots of them out there near Arachneida's lair.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(HuntForArachneida)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			HuntForArachneida quest = player.IsDoingQuest(typeof (HuntForArachneida)) as HuntForArachneida;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			HuntForArachneida quest = player.IsDoingQuest(typeof (HuntForArachneida)) as HuntForArachneida;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			HuntForArachneida quest = (HuntForArachneida) player.IsDoingQuest(typeof (HuntForArachneida));
			if (quest == null)
				return;

			if (quest.Step == 3)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.TemplateID == bloatedFang.TemplateID)
				{
					if (WorldMgr.GetDistance(player, arachneida) < 500 && !arachneida.IsAlive)
					{
						SendSystemMessage(player, "You use the bloated spider fang to retrieve arachneida's chitin!");
						GiveItem(player, spiderChitin);

						quest.Step = 4;
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
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (HuntForArachneida)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

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
			HuntForArachneida quest = player.IsDoingQuest(typeof (HuntForArachneida)) as HuntForArachneida;

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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(kealan.CanGiveQuest(typeof (HuntForArachneida), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (HuntForArachneida)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!kealan.GiveQuest(typeof (HuntForArachneida), player, 1))
					return;

				kealan.SayTo(player, "Good! I know we ca'count on ye. As a proof bring me Arachneida's chitin. But make sure to fetch a [bloated spider fang] first, or you won't be able to retrieve the chitin, it's rather hard to break.!");

				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		/* Now we set the quest name.
		 * If we don't override the base method, then the quest
		 * will have the name "UNDEFINED QUEST NAME" and we don't
		 * want that, do we? ;-)
		 */

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		 * If we don't override the base method, then the quest
		 * description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		 * and this isn't something nice either ;-)
		 */

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Find a bloated spider and take one of its fangs.";
					case 2:
						return "[Step #2] Find Arachneida, take her down in battle";
					case 3:
						return "[Step #3] Use the bloated spider fang to retrieve Arachnaidas chitin.";
					case 4:
						return "[Step #4] Bring both, bloated spider fang and Arachneida's chitin to Kealan.";
					case 5:
						return "[Step #4] Bring both, bloated spider fang and Arachneida's chitin to Kealan.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (HuntForArachneida)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == "bloated spider")
				{
					if (Util.Chance(50))
					{
						SendSystemMessage("You were able to slay a bloated spider, but her fang was broken during combat.!");
					}
					else
					{
						SendSystemMessage("You were able to slay a bloated spider, and take her fang!");
						GiveItem(gArgs.Target, player, bloatedFang);
						Step = 2;
					}
					return;
				}
			}
			if (Step == 2 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == arachneida.Name)
				{
					SendSystemMessage("You strike down Arachneida, now use the fang to retrieve her chitin!");
					Step = 3;
					return;
				}
			}
			else if (Step >= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == kealan.Name && (gArgs.Item.TemplateID == bloatedFang.TemplateID || gArgs.Item.TemplateID == spiderChitin.TemplateID))
				{
					kealan.TurnTo(m_questPlayer);
					if (Step == 4)
					{
						kealan.SayTo(player, "Very well now hand me over the rest and you will revieve your reward...");
						RemoveItem(kealan, player, gArgs.Item.Template);
						Step = 5;
					}
					else if (Step == 5)
					{
						RemoveItem(kealan, player, gArgs.Item.Template);
						FinishQuest();
					}
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, spiderChitin, false);
			RemoveItem(m_questPlayer, bloatedFang, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

		public override void FinishQuest()
		{
			kealan.SayTo(m_questPlayer, "Great, the bloated spider fang and Arachneida's chitin. You did your job well! Now here, take this as a token of my gratitude.");
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			m_questPlayer.GainExperience(40050, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 22, Util.Random(50)), "You recieve {0} for your service.");

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Wizard ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Sorcerer ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cabalist ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Theurgist)
			{
				GiveItem(kealan, m_questPlayer, spiderSilkenRobe);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Paladin ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Armsman ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Reaver ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Mercenary)
			{
				GiveItem(kealan, m_questPlayer, ringedSpiderChitinTunic);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cleric ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Scout ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Minstrel)
			{
				GiveItem(kealan, m_questPlayer, studdedSpiderEyeVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Friar)
			{
				GiveItem(kealan, m_questPlayer, spiderEmblazonedTunic);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Infiltrator)
			{
				GiveItem(kealan, m_questPlayer, embossedSpiderTunic);
			}

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
