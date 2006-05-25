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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
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
	[Subclass(NameType = typeof(HuntForArachneida), ExtendsType = typeof(AbstractQuest))]
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

		private static GameMob kealan = null;

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

			kealan = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Kealan") as GameMob;
			if (kealan == null)
			{
				kealan = new GameMob();
				kealan.Model = 281;
				kealan.Name = "Kealan";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + kealan.Name + ", creating him ...");
				kealan.GuildName = "Part of " + questTitle + " Quest";
				kealan.Realm = (byte) eRealm.Albion;
				kealan.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(58, 40, 0));
				kealan.Inventory = template;
				kealan.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				kealan.Size = 48;
				kealan.Level = 32;
				kealan.Position = new Point(493414, 593089, 1797);
				kealan.Heading = 830;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = kealan;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				kealan.OwnBrain = newBrain;

				if(!kealan.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(kealan);
			}

			arachneida = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, "Arachneida") as GameMob;
			if (arachneida == null)
			{
				arachneida = new GameMob();
				arachneida.Model = 72;
				arachneida.Name = "Arachneida";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + arachneida.Name + ", creating him ...");
				arachneida.GuildName = "Part of " + questTitle + " Quest";
				arachneida.Realm = (byte) eRealm.None;
				arachneida.Region = WorldMgr.GetRegion(1);

				arachneida.Size = 90;
				arachneida.Level = 12;
				arachneida.Position = new Point(534851, 609656, 2456);
				arachneida.Heading = 2080;

				arachneida.RespawnInterval = -1; // auto respawn

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = arachneida;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 1000;
				arachneida.OwnBrain = newBrain;

				if(!arachneida.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(arachneida);
			}

			#endregion

			#region defineItems

			spiderSilkenRobe = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Spider Silken Robe")) as TorsoArmorTemplate;
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

			ringedSpiderChitinTunic = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Ringed Spider Chitin Tunic")) as TorsoArmorTemplate;
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

			studdedSpiderEyeVest = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Studded Spider Eye Vest")) as TorsoArmorTemplate;
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

			spiderEmblazonedTunic = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Spider Emblazoned Tunic")) as TorsoArmorTemplate;
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

			embossedSpiderTunic = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Embossed Spider Tunic")) as TorsoArmorTemplate;
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

			bloatedFang = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Bloated spider fang")) as GenericItemTemplate;
			if (bloatedFang == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find  Bloated spider fang, creating it ...");
				bloatedFang = new GenericItemTemplate();
				bloatedFang.Weight = 10;
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

			spiderChitin = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Arachneida's Chitin")) as GenericItemTemplate;
			if (spiderChitin == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Arachneida's Chitin, creating it ...");
				spiderChitin = new GenericItemTemplate();
				spiderChitin.Weight = 25;
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

			GameEventMgr.AddHandler(kealan, GameObjectEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.AddHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

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

			GameEventMgr.RemoveHandler(kealan, GameObjectEvent.Interact, new DOLEventHandler(TalkToKealan));
			GameEventMgr.RemoveHandler(kealan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToKealan));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

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
							QuestMgr.ProposeQuestToPlayer(typeof(HuntForArachneida), "Do you want to hunt Arachneida?", player, kealan);
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

				GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot) as GenericItem;
				if (item != null && item.QuestName == quest.Name && item.Name == bloatedFang.Name)
				{
					if (player.Position.CheckSquareDistance(arachneida.Position, 500*500) && !arachneida.Alive)
					{
						SendSystemMessage(player, "You use the bloated spider fang to retrieve arachneida's chitin!");
						GiveItemToPlayer(CreateQuestItem(spiderChitin, quest), player);
						quest.ChangeQuestStep(4);
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(HuntForArachneida)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(HuntForArachneida), player, gArgs.Source as GameNPC))
					{
						kealan.SayTo(player, "Good! I know we ca'count on ye. As a proof bring me Arachneida's chitin. But make sure to fetch a [bloated spider fang] first, or you won't be able to retrieve the chitin, it's rather hard to break.!");

						GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{

					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
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
						GiveItemToPlayer(CreateQuestItem(bloatedFang));
						ChangeQuestStep(2);
					}
					return;
				}
			}
			if (Step == 2 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target == arachneida)
				{
					SendSystemMessage("You strike down Arachneida, now use the fang to retrieve her chitin!");
					ChangeQuestStep(3);
					return;
				}
			}
			else if (Step >= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target == kealan && gArgs.Item.QuestName == Name && (gArgs.Item.Name == bloatedFang.Name || gArgs.Item.Name == spiderChitin.Name))
				{
					kealan.TurnTo(m_questPlayer);
					if (Step == 4)
					{
						RemoveItemFromPlayer(kealan, gArgs.Item);
						kealan.SayTo(player, "Very well now hand me over the rest and you will revieve your reward...");
						ChangeQuestStep(5);
					}
					else if (Step == 5)
					{
						RemoveItemFromPlayer(kealan, gArgs.Item);
						kealan.SayTo(m_questPlayer, "Great, the bloated spider fang and Arachneida's chitin. You did your job well! Now here, take this as a token of my gratitude.");
						FinishQuest();
					}
					return;
				}
			}
		}

		public override void FinishQuest()
		{
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
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
