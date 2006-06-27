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
using DOL.GS.Database;
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
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class HuntForArachneidaDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(HuntForArachneida); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 14; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(HuntForArachneida), ExtendsType = typeof(AbstractQuest))]
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

		private static GameNPC kealan = null;

		private static GameMob arachneida = null;

		private static TorsoArmorTemplate spiderSilkenRobe = null;
		private static TorsoArmorTemplate ringedSpiderChitinTunic = null;
		private static TorsoArmorTemplate studdedSpiderEyeVest = null;
		private static TorsoArmorTemplate spiderEmblazonedTunic = null;
		private static TorsoArmorTemplate embossedSpiderTunic = null;

		private static GenericItemTemplate bloatedFang = null;
		private static GenericItemTemplate spiderChitin = null;

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
				kealan = new GameMob();
				kealan.Model = 281;
				kealan.Name = "Kealan";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + kealan.Name + ", creating him ...");

				kealan.GuildName = "Part of " + questTitle + " Quest";
				kealan.Realm = (byte) eRealm.Albion;
				kealan.RegionId = 1;
				kealan.Size = 48;
				kealan.Level = 32;
				kealan.Position = new Point(493414, 593089, 1797);
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
				arachneida = new GameMob();
				arachneida.Model = 72;
				arachneida.Name = "Arachneida";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + arachneida.Name + ", creating her ...");

				arachneida.GuildName = "Part of " + questTitle + " Quest";
				arachneida.Realm = (byte) eRealm.None;
				arachneida.RegionId = 1;
				arachneida.Size = 90;
				arachneida.Level = 12;
				arachneida.Position = new Point(534851, 609656, 2456);
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
				arachneida = (GameMob) npcs[0];

			#endregion

			#region defineItems

			spiderSilkenRobe = (TorsoArmorTemplate) GameServer.Database.FindObjectByKey(typeof (TorsoArmorTemplate), "spider_silken_robe");
			if (spiderSilkenRobe == null)
			{
				spiderSilkenRobe = new TorsoArmorTemplate();
				spiderSilkenRobe.Name = "Spider Silken Robe";

				if (log.IsWarnEnabled)
					log.Warn("Could not find " + spiderSilkenRobe.Name + ", creating it ...");
				spiderSilkenRobe.Level = 20;
				spiderSilkenRobe.Weight = 5;
				spiderSilkenRobe.Model = 58;

				spiderSilkenRobe.Bonus = 5;

				spiderSilkenRobe.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));
				spiderSilkenRobe.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 3));
				spiderSilkenRobe.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 6));

				spiderSilkenRobe.ArmorFactor = 18;
				spiderSilkenRobe.ArmorLevel = eArmorLevel.VeryLow;

				spiderSilkenRobe.ItemTemplateID = "spider_silken_robe";
				spiderSilkenRobe.Value = Money.GetMoney(0, 0, 0, 8, 3);

				spiderSilkenRobe.IsDropable = true;
				spiderSilkenRobe.IsSaleable = true;
				spiderSilkenRobe.IsTradable = true;

				//You don't have to store the created wolfPeltCloak in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(spiderSilkenRobe);
			}

			ringedSpiderChitinTunic = (TorsoArmorTemplate) GameServer.Database.FindObjectByKey(typeof (TorsoArmorTemplate), "ringed_spider_chitin_tunic");
			if (ringedSpiderChitinTunic == null)
			{
				ringedSpiderChitinTunic = new TorsoArmorTemplate();
				ringedSpiderChitinTunic.Name = "Ringed Spider Chitin Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + ringedSpiderChitinTunic.Name + ", creating it ...");

				ringedSpiderChitinTunic.Level = 17;
				ringedSpiderChitinTunic.Weight = 80;
				ringedSpiderChitinTunic.Model = 41;

				ringedSpiderChitinTunic.Bonus = 5;

				ringedSpiderChitinTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
				ringedSpiderChitinTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 3));
				ringedSpiderChitinTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 9));

				ringedSpiderChitinTunic.ArmorFactor = 28;
				ringedSpiderChitinTunic.ArmorLevel = eArmorLevel.High;

				ringedSpiderChitinTunic.ItemTemplateID = "ringed_spider_chitin_tunic";
				ringedSpiderChitinTunic.Value = Money.GetMoney(0, 0, 0, 9, 3);

				ringedSpiderChitinTunic.IsDropable = true;
				ringedSpiderChitinTunic.IsSaleable = true;
				ringedSpiderChitinTunic.IsTradable = true;

				ringedSpiderChitinTunic.Color = 45;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(ringedSpiderChitinTunic);
			}

			studdedSpiderEyeVest = (TorsoArmorTemplate) GameServer.Database.FindObjectByKey(typeof (TorsoArmorTemplate), "studded_spider_eye_vest");
			if (studdedSpiderEyeVest == null)
			{
				studdedSpiderEyeVest = new TorsoArmorTemplate();
				studdedSpiderEyeVest.Name = "Studded Spider Eye Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + studdedSpiderEyeVest.Name + ", creating it ...");

				studdedSpiderEyeVest.Level = 17;
				studdedSpiderEyeVest.Weight = 60;
				studdedSpiderEyeVest.Model = 51;

				studdedSpiderEyeVest.Bonus = 5;

				studdedSpiderEyeVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 3));
				studdedSpiderEyeVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 2));
				studdedSpiderEyeVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 9));

				studdedSpiderEyeVest.ArmorFactor = 28;
				studdedSpiderEyeVest.ArmorLevel = eArmorLevel.Medium;

				studdedSpiderEyeVest.ItemTemplateID = "studded_spider_eye_vest";

				studdedSpiderEyeVest.Value = Money.GetMoney(0, 0, 0, 9, 3);

				studdedSpiderEyeVest.IsDropable = true;
				studdedSpiderEyeVest.IsSaleable = true;
				studdedSpiderEyeVest.IsTradable = true;

				studdedSpiderEyeVest.Color = 45;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(studdedSpiderEyeVest);
			}

			spiderEmblazonedTunic = (TorsoArmorTemplate) GameServer.Database.FindObjectByKey(typeof (TorsoArmorTemplate), "spider_emblazoned_tunic");
			if (spiderEmblazonedTunic == null)
			{
				spiderEmblazonedTunic = new TorsoArmorTemplate();
				spiderEmblazonedTunic.Name = "Spider Emblazoned Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + spiderEmblazonedTunic.Name + ", creating it ...");
				spiderEmblazonedTunic.Level = 17;
				spiderEmblazonedTunic.Weight = 40;
				spiderEmblazonedTunic.Model = 31;

				spiderEmblazonedTunic.Bonus = 5;

				spiderEmblazonedTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 2));
				spiderEmblazonedTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 3));
				spiderEmblazonedTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 9));

				spiderEmblazonedTunic.ArmorFactor = 28;
				spiderEmblazonedTunic.ArmorLevel = eArmorLevel.Low;

				spiderEmblazonedTunic.ItemTemplateID = "spider_emblazoned_tunic";
				spiderEmblazonedTunic.Value = Money.GetMoney(0, 0, 0, 9, 3);

				spiderEmblazonedTunic.IsDropable = true;
				spiderEmblazonedTunic.IsSaleable = true;
				spiderEmblazonedTunic.IsTradable = true;

				spiderEmblazonedTunic.Color = 45;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(spiderEmblazonedTunic);
			}

			embossedSpiderTunic = (TorsoArmorTemplate) GameServer.Database.FindObjectByKey(typeof (TorsoArmorTemplate), "embossed_spider_tunic");
			if (embossedSpiderTunic == null)
			{
				embossedSpiderTunic = new TorsoArmorTemplate();
				embossedSpiderTunic.Name = "Embossed Spider Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + embossedSpiderTunic.Name + ", creating it ...");

				embossedSpiderTunic.Level = 17;
				embossedSpiderTunic.Weight = 40;
				embossedSpiderTunic.Model = 31;

				embossedSpiderTunic.Bonus = 5;

				embossedSpiderTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 2));
				embossedSpiderTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 2));
				embossedSpiderTunic.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));

				embossedSpiderTunic.ArmorFactor = 28;
				embossedSpiderTunic.ArmorLevel = eArmorLevel.Low;

				embossedSpiderTunic.ItemTemplateID = "embossed_spider_tunic";

				embossedSpiderTunic.Value = Money.GetMoney(0, 0, 0, 9, 3);

				embossedSpiderTunic.IsDropable = true;
				embossedSpiderTunic.IsSaleable = true;
				embossedSpiderTunic.IsTradable = true;

				embossedSpiderTunic.Color = 45;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(embossedSpiderTunic);
			}

			bloatedFang = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "bloated_spider_fang");
			if (bloatedFang == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find  Bloated spider fang, creating it ...");
				bloatedFang = new GenericItemTemplate();
				bloatedFang.Weight = 10;
				bloatedFang.ItemTemplateID = "bloated_spider_fang";
				bloatedFang.Name = "Bloated spider fang";
				bloatedFang.Model = 106;
				bloatedFang.IsDropable = false;
				bloatedFang.IsSaleable = false;
				bloatedFang.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bloatedFang);
			}

			spiderChitin = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "arachneida_spider_chitin");
			if (spiderChitin == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Arachneida's Chitin, creating it ...");
				spiderChitin = new GenericItemTemplate();
				spiderChitin.Weight = 25;
				spiderChitin.ItemTemplateID = "arachneida_spider_chitin";
				spiderChitin.Name = "Arachneida's Chitin";
				spiderChitin.Model = 108;
				spiderChitin.IsDropable = false;
				spiderChitin.IsSaleable = false;
				spiderChitin.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(spiderChitin);
			}

			#endregion

			/* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(kealan, GameLivingEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.AddHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(kealan, typeof(HuntForArachneidaDescriptor));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(kealan, GameLivingEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.RemoveHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			/* Now we remove to kealan the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(kealan, typeof(HuntForArachneidaDescriptor));
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

			if (QuestMgr.CanGiveQuest(typeof(HuntForArachneida), player, kealan) <= 0)
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
					if (player.Inventory.GetFirstItemByName(bloatedFang.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
						kealan.SayTo(player, "Good, you managed to retrieve a bloated spider fang, but did you also slay Arachneida?");
					else if (player.Inventory.GetFirstItemByName(spiderChitin.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
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
							player.Out.SendCustomDialog("Do you want to hunt Arachneida?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
					}
				}

			}
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

				GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Name == bloatedFang.Name)
				{
					if (player.Position.CheckSquareDistance(arachneida.Position, 500*500) && !arachneida.Alive)
					{
						SendSystemMessage(player, "You use the bloated spider fang to retrieve arachneida's chitin!");
						player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, spiderChitin.CreateInstance());
						quest.Step = 4;
					}
				}
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
			if (QuestMgr.CanGiveQuest(typeof(HuntForArachneida), player, kealan) <= 0)
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
				if (!QuestMgr.GiveQuestToPlayer(typeof(HuntForArachneida), player, kealan))
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
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
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
						GiveItemToPlayer(gArgs.Target, bloatedFang.CreateInstance());
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
				if (gArgs.Target.Name == kealan.Name && (gArgs.Item.Name == bloatedFang.Name || gArgs.Item.Name == spiderChitin.Name))
				{
					kealan.TurnTo(m_questPlayer);
					if (Step == 4)
					{
						kealan.SayTo(player, "Very well now hand me over the rest and you will revieve your reward...");
						RemoveItemFromPlayer(kealan, gArgs.Item);
						Step = 5;
					}
					else if (Step == 5)
					{
						RemoveItemFromPlayer(kealan, gArgs.Item);
						FinishQuest();
					}
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			kealan.SayTo(m_questPlayer, "Great, the bloated spider fang and Arachneida's chitin. You did your job well! Now here, take this as a token of my gratitude.");
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			m_questPlayer.GainExperience(40050, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 22, Util.Random(50)), "You recieve {0} for your service.");

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Wizard ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Sorcerer ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cabalist ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Theurgist)
			{
				GiveItemToPlayer(kealan, spiderSilkenRobe.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Paladin ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Armsman ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Reaver ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Mercenary)
			{
				GiveItemToPlayer(kealan, ringedSpiderChitinTunic.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cleric ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Scout ||
				m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Minstrel)
			{
				GiveItemToPlayer(kealan, studdedSpiderEyeVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Friar)
			{
				GiveItemToPlayer(kealan, spiderEmblazonedTunic.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Infiltrator)
			{
				GiveItemToPlayer(kealan, embossedSpiderTunic.CreateInstance());
			}

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
