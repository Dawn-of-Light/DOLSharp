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
 *  Brief Walkthrough: 
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 * 2) Go to Dragonfly Handler Colm, at loc=14040,29170 Camelot Hills (Cotswold), and purchase a dragonfly ticket to Avalon Marsh. 
 * 3) Once in Avalon Marsh look for Master Dunwyn, he tends to roam a bit from the guard tower and the Shrouded Isles portal. , I found him at loc=47591,20373,1840 dir=187 Avalon Marsh. Hand him the Scroll for Master Dunwyn from Master Frederick. 
 * 4) /use the List for Master Dunwyn from Master Dunwyn 
 * 5) Kill a swamp slime. 
 * 6) /use the List for Master Dunwyn from Master Dunwyn 
 * 7) Kill a river spriteling. 
 * 8) /use the List for Master Dunwyn from Master Dunwyn 
 * 9) Kill a swamp rat. 
 * 10) Go to Master Dunwyn at the Shrouded Isles portal and hand him the items he asks for that you have collected. 
 * 11) Master Dunwyn will teleport you back to Master Frederick. Talk with Master Frederick. 
 * 12) Right click on Master Dunwyn and the click on Master Frederick for instructions. 
 * 13) Go to loc=30425,24872 Camelot Hills with Master Dunwyn in tow. When you get there you will see a swarm of Ire Fairy Sorceress's there with Princess Obera. Focus on killing Princess Obera, blue con to a lvl 4, Master Dunwyn will deal with the Ire Fairy Sorceress's. Once Princess Obera is dead you can help Master Dunwyn finish killing the other Ire Fairies. 
 * 14) Talk with Master Dunwy after the fight. 
 * 15) Go back to Master Frederick and type /whisper returned to the Marsh. 
 * 16) Hand Master Frederick Princess Obera's Head to collect your reward.
 */

using System;
using System.Collections;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Database;
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
    public class BeginningOfWarDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base method like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(BeginningOfWar); }
        }

        /* This value is used to retrieves the minimum level needed
         *  to be able to make this quest. Override it only if you need, 
         * the default value is 1
         */
        public override int MinLevel
        {
            get { return 4; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 4; }
        }

        public override bool CheckQuestQualification(GamePlayer player)
        {
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

            // This checks below are only performed is player isn't doing quest already
            if (player.HasFinishedQuest(typeof(IreFairyIre)) == 0)
                return false;

            if (!BaseFrederickQuest.CheckPartAccessible(player, LinkedQuestType))
                return false;

            return base.CheckQuestQualification(player);
        }
    }
    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(BeginningOfWar), ExtendsType = typeof(AbstractQuest))] 
	public class BeginningOfWar : BaseFrederickQuest
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
		 */

		protected static GameLocation locationFrederick = new GameLocation("Master Frederick", 1, 0, 19105 + 50, 26552 + 40, 2861, 0);

		protected const string questTitle = "Beginning of War";

		private static GameMob masterFrederick = null;

		private static GameLocation locationDunwynClone = new GameLocation(null, 1, 567604, 509619, 2813, 3292);

		private static GameMob dunwyn = null;
		private GameMob dunwynClone = null;

		private static GameMob princessObera = null;
		private static GameMob[] fairySorceress = new GameMob[4];

		private static GenericItemTemplate swampSlimeItem = null;
		private static GenericItemTemplate swampRatTail = null;
		private static GenericItemTemplate princessOberasHead = null;
		private static GenericItemTemplate riverSpritlingClaw = null;

		private static GenericItemTemplate scrollDunwyn = null;
		private static GenericItemTemplate listDunwyn = null;
		private static HeadArmorTemplate recruitsHelm = null;
		private static HeadArmorTemplate recruitsCap = null;
		private static RingTemplate recruitsRing = null;

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
		 * want. 
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

			#region defineNPCs

			masterFrederick = GetMasterFrederick();
			if(masterFrederick == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			dunwyn = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Master Dunwyn") as GameMob;
			if (dunwyn == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Master Dunwyn, creating ...");
				dunwyn = new GameMob();
				dunwyn.Model = 9;
				dunwyn.Name = "Master Dunwyn";
				dunwyn.GuildName = "Part of " + questTitle + " Quest";
				dunwyn.Realm = (byte) eRealm.Albion;
				dunwyn.Region = WorldMgr.GetRegion(1);

				dunwyn.Size = 50;
				dunwyn.Level = 20;
				dunwyn.Position = new Point(465383, 634773, 1840);
				dunwyn.Heading = 187;

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(798));
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(19));
				dunwyn.Inventory = template;
				dunwyn.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = dunwyn;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				dunwyn.OwnBrain = newBrain;

				if(!dunwyn.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(dunwyn);
			}


			princessObera = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Princess Obera") as GameMob;
			if (princessObera == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Princess Obera, creating ...");
				princessObera = new GameMob();

				princessObera.Name = "Princess Obera";
				princessObera.Position = new Point(579289, 508200, 2779);
				princessObera.Heading = 347;
				princessObera.Model = 603;
				princessObera.GuildName = "Part of " + questTitle + " Quest";
				princessObera.Realm = (byte) eRealm.None;
				princessObera.Region = WorldMgr.GetRegion(1);
				princessObera.Size = 49;
				princessObera.Level = 3;

				princessObera.RespawnInterval = -1; // auto respawn

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = princessObera;
				newBrain.AggroLevel = 80;
				newBrain.AggroRange = 1000;
				princessObera.OwnBrain = newBrain;

				if(!princessObera.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(princessObera);
			}

			int counter = 0;
			foreach (GameMob mob in princessObera.GetInRadius(typeof(GameMob), 500))
			{
				if (mob.Name == "ire fairy sorceress")
				{
					fairySorceress[counter] = mob;
					counter++;
				}
				if (counter == fairySorceress.Length) break;
			}

			for (int i = 0; i < fairySorceress.Length; i++)
			{
				if (fairySorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find ire fairy sorceress, creating ...");
					fairySorceress[i] = new GameMob();
					fairySorceress[i].Model = 603; // //819;
					fairySorceress[i].Name = "ire fairy sorceress";
					fairySorceress[i].GuildName = "Part of " + questTitle + " Quest";
					fairySorceress[i].Realm = (byte) eRealm.None;
					fairySorceress[i].Region = WorldMgr.GetRegion(1);
					fairySorceress[i].Size = 35;
					fairySorceress[i].Level = 3;
					Point pos = princessObera.Position;
					pos.X += Util.Random(-150, 150);
					pos.Y += Util.Random(-150, 150);
					fairySorceress[i].Position = pos;
					fairySorceress[i].Heading = 93;

					fairySorceress[i].RespawnInterval = -1; //autorespawn

					StandardMobBrain brain = new StandardMobBrain();
					brain.Body = fairySorceress[i];
					brain.AggroLevel = 50;
					brain.AggroRange = 300;
					fairySorceress[i].OwnBrain = brain;

					if(!fairySorceress[i].AddToWorld())
					{
						if (log.IsWarnEnabled)
							log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
						return;
					}

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						GameServer.Database.AddNewObject(fairySorceress[i]);
				}
			}

			#endregion

			#region defineItems

			swampRatTail = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Swamp Rat Tail")) as GenericItemTemplate;
			if (swampRatTail == null)
			{
				swampRatTail = new GenericItemTemplate();
				swampRatTail.Name = "Swamp Rat Tail";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + swampRatTail.Name + " , creating it ...");

				swampRatTail.Weight = 110;
				swampRatTail.Model = 515;

				swampRatTail.IsDropable = false;
                swampRatTail.IsSaleable = false;
                swampRatTail.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(swampRatTail);
			}

			swampSlimeItem = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Swamp Slime")) as GenericItemTemplate;
			if (swampSlimeItem == null)
			{
				swampSlimeItem = new GenericItemTemplate();
				swampSlimeItem.Name = "Swamp Slime";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + swampSlimeItem.Name + " , creating it ...");

				swampSlimeItem.Weight = 10;
				swampSlimeItem.Model = 553;

				swampSlimeItem.IsDropable = false;
                swampSlimeItem.IsSaleable = false;
                swampSlimeItem.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(swampSlimeItem);
			}

			scrollDunwyn = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Scroll for Master Dunwyn")) as GenericItemTemplate;
			if (scrollDunwyn == null)
			{
				scrollDunwyn = new GenericItemTemplate();
				scrollDunwyn.Name = "Scroll for Master Dunwyn";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollDunwyn.Name + " , creating it ...");

				scrollDunwyn.Weight = 10;
				scrollDunwyn.Model = 498;

				scrollDunwyn.IsDropable = false;
                scrollDunwyn.IsSaleable = false;
                scrollDunwyn.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollDunwyn);
			}

			listDunwyn = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "List for Master Dunwyn")) as GenericItemTemplate;
			if (listDunwyn == null)
			{
				listDunwyn = new GenericItemTemplate();
				listDunwyn.Name = "List for Master Dunwyn";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + listDunwyn.Name + " , creating it ...");

				listDunwyn.Weight = 10;
				listDunwyn.Model = 498;

				listDunwyn.IsDropable = false;
                listDunwyn.IsSaleable = false;
                listDunwyn.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(listDunwyn);
			}

			riverSpritlingClaw = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "River Spriteling Claw")) as GenericItemTemplate;
			if (riverSpritlingClaw == null)
			{
				riverSpritlingClaw = new GenericItemTemplate();
				riverSpritlingClaw.Name = "River Spriteling Claw";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + riverSpritlingClaw.Name + " , creating it ...");

				riverSpritlingClaw.Weight = 2;
				riverSpritlingClaw.Model = 106;

				riverSpritlingClaw.IsDropable = false;
                riverSpritlingClaw.IsSaleable = false;
                riverSpritlingClaw.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(riverSpritlingClaw);
			}

			princessOberasHead = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Princess Obera's Head")) as GenericItemTemplate;
			if (princessOberasHead == null)
			{
				princessOberasHead = new GenericItemTemplate();
				princessOberasHead.Name = "Princess Obera's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + princessOberasHead.Name + " , creating it ...");

				princessOberasHead.Weight = 15;
				princessOberasHead.Model = 503;

				princessOberasHead.IsDropable = false;
                princessOberasHead.IsSaleable = false;
                princessOberasHead.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(princessOberasHead);
			}

			// item db check
			recruitsHelm = GameServer.Database.SelectObject(typeof (HeadArmorTemplate), Expression.Eq("Name", "Recruit's Studded Helm")) as HeadArmorTemplate;
			if (recruitsHelm == null)
			{
				recruitsHelm = new HeadArmorTemplate();
				recruitsHelm.Name = "Recruit's Studded Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsHelm.Name + ", creating it ...");
				recruitsHelm.Level = 8;

				recruitsHelm.Weight = 24;
				recruitsHelm.Model = 824;

                recruitsHelm.ArmorFactor = 12;
                recruitsHelm.ArmorLevel = eArmorLevel.Medium;

				recruitsHelm.Value = 900;
                recruitsHelm.IsDropable = true;
                recruitsHelm.IsSaleable = true;
                recruitsHelm.IsTradable = true;

				recruitsHelm.Color = 9; // red leather
                recruitsHelm.Quality = 100;

				recruitsHelm.Bonus = 5; // default bonus

                recruitsHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 4));
                recruitsHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 1));
                recruitsHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsHelm);
			}

			// item db check
			recruitsCap = GameServer.Database.SelectObject(typeof (HeadArmorTemplate), Expression.Eq("Name", "Recruit's Quilted Cap")) as HeadArmorTemplate;
			if (recruitsCap == null)
			{
				recruitsCap = new HeadArmorTemplate();
				recruitsCap.Name = "Recruit's Quilted Cap";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsCap.Name + ", creating it ...");
				recruitsCap.Level = 8;

				recruitsCap.Weight = 8;
				recruitsCap.Model = 822;

                recruitsCap.ArmorLevel = eArmorLevel.VeryLow;
                recruitsCap.ArmorFactor = 6;

                recruitsCap.Value = 900;
                recruitsCap.IsDropable = true;
                recruitsCap.IsSaleable = true;
                recruitsCap.IsTradable = true;
				recruitsCap.Color = 27; // red cloth
                recruitsCap.Quality = 100;

				recruitsCap.Bonus = 5; // default bonus

                recruitsCap.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 4));
                recruitsCap.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 1));
                recruitsCap.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 20));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsCap);
			}

			recruitsRing = GameServer.Database.SelectObject(typeof (RingTemplate), Expression.Eq("Name", "Recruit's Silver Ring")) as RingTemplate;
			if (recruitsRing == null)
			{
				recruitsRing = new RingTemplate();
				recruitsRing.Name = "Recruit's Silver Ring";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsRing.Name + ", creating it ...");
				recruitsRing.Level = 6;

				recruitsRing.Weight = 2;
				recruitsRing.Model = 103;                

				recruitsRing.Value = 900;

				recruitsRing.IsDropable = true;
                recruitsRing.IsSaleable = true;
                recruitsRing.IsTradable = true;

				recruitsRing.Bonus = 5; // default bonus

                recruitsRing.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 20));

				recruitsRing.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRing);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(dunwyn, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
			GameEventMgr.AddHandler(dunwyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));

			GameEventMgr.AddHandler(princessObera, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessObera));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(masterFrederick, typeof(BeginningOfWarDescriptor));

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
			if (masterFrederick == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(dunwyn, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
			GameEventMgr.RemoveHandler(dunwyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));

			GameEventMgr.RemoveHandler(princessObera, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessObera));
	
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(BeginningOfWarDescriptor));
		}

		protected static void CheckNearPrincessObera(DOLEvent e, object sender, EventArgs args)
		{
			GameMob princessObera = sender as GameMob;

			// if princess is dead no ned to checks ...
			if (princessObera == null || princessObera.ObjectState != eObjectState.Active || !princessObera.Alive)
				return;

			foreach (GamePlayer player in princessObera.GetInRadius(typeof(GamePlayer), 1000))
			{
				BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));

				if (quest != null && !princessObera.AttackState && quest.Step == 16)
				{
					if (quest.dunwynClone != null)
					{
						foreach (GamePlayer visPlayer in quest.dunwynClone.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.dunwynClone, 1, 20);
						}

						new RegionTimer(quest.dunwynClone, new RegionTimerCallback(quest.CastDunwynClone), 2000);
						
						SendSystemMessage(player, "There they are. You take care of the princess I'll deal with the fairy sorcesses littleone.");
						
						foreach (GameMob fairy in fairySorceress)
						{
							IAggressiveBrain aggroBrain = quest.dunwynClone.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(fairy, Util.Random(50));
							aggroBrain = fairy.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(quest.dunwynClone, Util.Random(50));
						}
					}
					
					IAggressiveBrain brain = princessObera.Brain as IAggressiveBrain;
					if (brain != null)
						brain.AddToAggroList(player, 50 + Util.Random(20));
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterFrederick(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(BeginningOfWar), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			masterFrederick.TurnTo(player.Position);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Recruit, we need to take care of these fairies. I have heard from passers-by that they are trying to amass an army to defeat Cotswold! Can you [believe] that?");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "There is an old wizard I know by the name of Dunwyn. Well, Master Dunwyn, to show respect. He patrols a small section of road in the Avalon Marsh. Take this note to Master Dunwyn explaining our situation. I'm sure he would be interested in one more fight for the good of Albion.");
                        GiveItemToPlayer(masterFrederick, CreateQuestItem(scrollDunwyn, quest), player);
						quest.ChangeQuestStep(2);
					}
					else if (quest.Step == 13)
					{
						masterFrederick.SayTo(player, "Welcome back recruit. I take it you found Master Dunwyn? What did he say? Will he be [assisting] us in our fairy problem?");
					}
					else if (quest.Step == 15)
					{
						masterFrederick.SayTo(player, "Good job recruit. Now listen. Since Master Dunwyn has graciously agreed to help us with this fairy problem, let's [detail] what we're going to do.");
					}
					else if (quest.Step == 18)
					{
						masterFrederick.SayTo(player, "Ah, my favorite recruit, back from battle. Where is Master Dunwyn? Has he already [returned to the Marsh]?");

					}
					else if (quest.Step == 19)
					{
						masterFrederick.SayTo(player, "Yes, for you my young recruit, please take this helm and this ring. I know these magical items will help you as you make your way through this life. Be safe now "+player.Name+". We will speak again very soon.");
						quest.FinishQuest();
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "believe":
							masterFrederick.SayTo(player, "I know, it's hard to take in. But listen "+player.Name+", we must strike hard at the fairies. We must destroy this supposed army before they can come and wreck Cotswold. What do you say recruit? Are you [in] or not?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "in":
							QuestMgr.ProposeQuestToPlayer(typeof(BeginningOfWar), "Will you take the letter to Master Dunwyn in Avalon Marsh for help with the fairy problem?", player, masterFrederick);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "solution":
							masterFrederick.SayTo(player, "There is an old wizard I know by the name of Dunwyn. Well, Master Dunwyn, to show respect. He patrols a small section of road in the Avalon Marsh at the guard tower near the big portal. Take this note to Master Dunwyn explaining our situation. I'm sure he would be interested in one more fight for the good of Albion.");
							if (quest.Step == 1)
							{
								GiveItemToPlayer(masterFrederick, CreateQuestItem(scrollDunwyn, quest), player);
								quest.ChangeQuestStep(2);
							}
							break;
						case "assisting":
							masterFrederick.SayTo(player, "Excellent! I knew he would. Now listen, there isn't much time. We have to start [formulating] our plan of attack.");
							if (quest.Step == 13)
							{
								quest.ChangeQuestStep(14);
							}
							break;
						case "formulating":
							masterFrederick.SayTo(player, "Now, I think we should...What the...?");
							if (quest.Step >= 14 && quest.Step <= 17)
							{
								SendSystemMessage(player, "Master Frederick looks around as a gate starts to swirl open.");

								bool dunwynCloneCreated = false;
								if (player.PlayerGroup != null)
								{
									foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
									{
										BeginningOfWar memberQuest = groupMember.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.dunwynClone != null)
										{
											dunwynCloneCreated = true;
											break;
										}
									}
								}

								if (!dunwynCloneCreated)
								{
									quest.CreateDunwynClone();
									masterFrederick.SayTo(player, "Ah! Master Dunwyn! So good to have you here! Recruit, please help Master Dunwyn over here to me. To help him, right click on him.");
								}
							}
							break;

						case "detail":
							masterFrederick.SayTo(player, "What we've heard from travelers is that the fairies are amassing a small army to invade Cotswold. And by small, I don't mean their stature. In fact, it may be a large army, but that's where [Master Dunwyn] comes in.");
							break;

						case "Master Dunwyn":
							masterFrederick.SayTo(player, "He should be able to take out the majority of the fairies, but you need to make sure you find and kill their leader. She should be obvious. I don't know who she is, but I [trust] your instincts.");
							break;
						case "trust":
							masterFrederick.SayTo(player, "Travel due east from this tower until you reach the sunken statue. That is where I am told they are massing. Good luck to the both of you.");
							if (quest.Step == 15)
							{
								quest.ChangeQuestStep(16);
							}
							break;
						case "returned to the Marsh":
							masterFrederick.SayTo(player, "Ah well, it was expected. He prefers it there, for some odd reason. Though, it was nice to see him again. Perhaps when this fairy business is finally at a close, I'll go visit him. But first, what proof do you have the leader of this army is dead?");
							break;
						case "reward":
							masterFrederick.SayTo(player, "Yes, for you my young recruit, please take this helm and this ring. I know these magical items will help you as you make your way through this life. Be safe now "+player.Name+". We will speak again very soon.");
							if (quest.Step == 19)
							{
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterDunwyn(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(BeginningOfWar), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			dunwyn.TurnTo(player.Position);
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step <= 2)
					{
						dunwyn.SayTo(player, "What? Who are you? What do you want? Can't you leave an old man in peace?");
					}
					else if (quest.Step == 3)
					{
						dunwyn.SayTo(player, "So...Frederick wants me to help fend off this fairy thingamabobber whatsamacallit. Well, fine! I see I didn't raise that boy right. Now lookee here, youngster. I have to make some [plans] before traipsing off to Camelot.");
					}
					else if (quest.Step == 10)
					{
						dunwyn.SayTo(player, "You're back faster than I thought. Alright then, showoff, give me that swamp slime.");
					}
					return;
				}
			} // The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "plans":
							dunwyn.SayTo(player, "Yes, plans! Have you ever heard of them? I swear, young people these days don't have a lick of common sense. I can't just run off to Camelot without making the proper preparations, now can I? Here, give me a [moment], will you?");
							SendSystemMessage(player, "Master Dunwyn takes the note and scribbles a few things on the back of it and hands it back to you.");
							break;

						case "moment":
							dunwyn.SayTo(player, "Alright young'un. You get me those supplies there on that list, and I'll be sure to go help you defeat that thingiemabobber whateveritscalled over there near Camelot. Now go on. Get!");
							if (quest.Step == 3)
							{
                                GiveItemToPlayer(dunwyn, CreateQuestItem(listDunwyn, quest), player);
								quest.ChangeQuestStep(4);
							}
							break;

						case "teleport":
							dunwyn.SayTo(player, "Hehe! Here you go!");
							if (quest.Step == 13)
							{
								quest.TeleportTo(player, dunwyn, locationFrederick, 10);
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterDunwynClone(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(BeginningOfWar), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (quest != null && quest.dunwynClone != null && quest.dunwynClone == sender)
				{
					quest.dunwynClone.TurnTo(player);

					if (quest.Step == 14)
					{
						quest.dunwynClone.SayTo(player, "Wha...? Who? Oh, it's you again. Yes yes. Come to help the feeble old man at your trainer's request. Confounded. Fine then. I'll follow you. Just walk slowly.");
						quest.dunwynClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.dunwynClone.Follow(player, 100, 3000);
						quest.ChangeQuestStep(15);
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
							{
								BeginningOfWar memberQuest = groupMember.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
								// we found another groupmember doing the same quest...
								if (memberQuest != null && memberQuest.Step == 14)
								{
									memberQuest.ChangeQuestStep(15);
								}
							}
						}
					}
					else if (quest.Step >= 15 && quest.Step < 17)
					{
						quest.dunwynClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.dunwynClone.Follow(player, 100, 3000);
					}
					return;
				}
			}
		}

		protected virtual void ResetMasterDunwyn()
		{
			if (dunwynClone != null && (dunwynClone.Alive || dunwynClone.ObjectState == eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(dunwynClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimEmoteSequence), 500));

				m_animEmoteObjectQueue.Enqueue(dunwynClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimEmoteSequence), 2500));
			}

			if (dunwynClone != null)
			{
				new RegionTimer(dunwynClone, new RegionTimerCallback(DeleteDunwynClone), 3500);
			}
		}

		protected virtual int CastDunwynClone(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in fairySorceress[0].GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				foreach (GameMob fairy in fairySorceress)
				{
					visPlayer.Out.SendSpellEffectAnimation(dunwynClone, fairy, 61, 10, false, 0x01);
				}
			}
			return 0;
		}

		protected virtual int DeleteDunwynClone(RegionTimer callingTimer)
		{
			if (dunwynClone != null)
			{
				GameEventMgr.RemoveHandler(dunwynClone, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwynClone));

				dunwynClone.RemoveFromWorld();
			}
			return 0;
		}

		protected virtual int TalkDunwynClone(RegionTimer callingTimer)
		{
			if (Step == 17)
			{
				dunwynClone.SayTo(m_questPlayer, "Well youngun, I have to get back to my patrolling. Good job with that princess whoever. Now, be a sport and let Frederick know that I went back to the Marsh. Darn son of mine.");
				dunwynClone.SayTo(m_questPlayer, "Tally ho!");

				ResetMasterDunwyn();
				ChangeQuestStep(18);
			}
			return 0;
		}


		protected void CreateDunwynClone()
		{
			if (dunwynClone == null)
			{
				dunwynClone = new GameMob();
				dunwynClone.Name = dunwyn.Name;
				dunwynClone.Model = dunwyn.Model;
				dunwynClone.GuildName = dunwyn.GuildName;
				dunwynClone.Realm = dunwyn.Realm;
				dunwynClone.Region = WorldMgr.GetRegion(1);
				dunwynClone.Size = dunwyn.Size;
				dunwynClone.Level = 15; // to make the figthing against fairy sorceress a bit more dramatic :)

				dunwynClone.Position = new Point(
					567604 + Util.Random(-150, 150),
					509619 + Util.Random(-150, 150),
					2813);
				dunwynClone.Heading = 3292;

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(798));
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(19));
				dunwynClone.Inventory = template;
				dunwynClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = dunwynClone;
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dunwynClone.OwnBrain = brain;

				dunwynClone.AddToWorld();

				GameEventMgr.AddHandler(dunwynClone, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwynClone));

				foreach (GamePlayer visPlayer in dunwynClone.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendEmoteAnimation(dunwynClone, eEmote.Bind);
				}
			}
			else
			{
				TeleportTo(dunwynClone, dunwynClone, locationDunwynClone);
			}

		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				quest.ResetMasterDunwyn();
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			if (player == null)
				return;

			// assistant works only in camelot...            
			BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot) as GenericItem;
			if (item != null && item.QuestName == quest.Name && item.Name == listDunwyn.Name)
			{
				if (quest.Step >= 4 && quest.Step <= 8)
				{
					IList textList = new ArrayList();
					textList.Add("swamp slime");
					textList.Add("river spriteling claw");
					textList.Add("swamp rat tail");
					player.Out.SendCustomTextWindow("List of Master Dunwyn", textList);
				}

				if (quest.Step == 4)
				{
					quest.ChangeQuestStep(5);
				}
				else if (quest.Step == 6)
				{
					quest.ChangeQuestStep(7);
				}
				else if (quest.Step == 8)
				{
					quest.ChangeQuestStep(9);
					SendSystemMessage(player, "The List disappears in a puff of blue smoke.");
                    RemoveItemFromPlayer(item, player);
				}
			}

		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step >= 15 && quest.Step <= 17)
				{
					quest.CreateDunwynClone();
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(BeginningOfWar)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(BeginningOfWar), player, gArgs.Source as GameNPC))
					{
						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
						GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

						masterFrederick.SayTo(player, "Excellent recruit, simply excellent! Now listen, I know you won't be able to defeat this fairie army on your own. So I have a [solution] to this problem.");
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
						return "[Step #1] Liste to Master Frederick. If you go link dead or log out, ask him the Solution] to the problem might be.";
					case 2:
						return "[Step #2] Deliver the letter to Master Dunwyn in Avalon Marsh.";
					case 3:
						return "[Step #3] Wait for Master Dunwyn to read the letter. If he stops reading the letter, ask him if he has any [plans] for helping.";
					case 4:
						return "[Step #4] You must now READ Master Dunwyn's list. In order to do that, right click on the list in your inventory and type /use. Get the things on his list for him.";
					case 5:
						return "[Step #5] You must now find a swamp slime. Slay the creature and retieve some of its slime for Master Dunwyn.";
					case 6:
						return "[Step #6] Read the list to find out what the second item is.";
					case 7:
						return "[Step #7] Find a river spriteling. You can find them near small bodies of water in the Avalon Marsh. Slay one and get its claw.";
					case 8:
						return "[Step #8] Read the list for the last time to find out what the final ingredient is.";
					case 9:
						return "[Step #9] Look around for a swamp rat. There are many of them in Avalon Marsh. Slay the creature for its tail.";
					case 10:
						return "[Step #10] Return to Master Dunwyn near the Shrouded Isles portal. Be sure to give him the swamp slime first.";
					case 11:
						return "[Step #11] Now give Master Dunwyn the river spriteling claw.";
					case 12:
						return "[Step #12] Now give Master Dunwyn the swamp rat tail.";
					case 13:
						return "[Step #13] Return to Master Frederick and let him know you've contacted Master Dunwyn and that he will be [assisting] with the fairy problem.";
					case 14:
						return "[Step #14] Listen to Master Frederick give you the details behind his plan.";
					case 15:
						return "[Step #15] Accompany Master Dunwyn to Master Frederick.";
					case 16:
						return "[Step #16] Travel due East from the guard tower in Cotswold. You will see a sunken statue in the ground. That is where the fairies are amassing. Be sure Master Dunwyn follows you.";
					case 17:
						return "[Step #17] When the fighting is done, speak with Master Dunwyn. If you went linkdead, logged out or the like and Master Dunwyn is not there, return to Master Frederick in Cotswold.";
					case 18:
						return "[Step #18] Return to Master Frederick. Be sure to tell him that Master Dunwyn has [returned to the Marsh].";
					case 19:
						return "[Step #19] Wait for Master Frederick to reward you for your efforts in defeating the fairy army. If he forgets, ask him about the [reward].";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";

				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (BeginningOfWar)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == "swamp slime" && (Step == 5 || Step == 4))
				{
					SendSystemMessage("You slay the swamp slime and retrieve some of it's slime for Master Dunwyn.");
                    GiveItemToPlayer(CreateQuestItem(swampSlimeItem));
					ChangeQuestStep(6);
					return;
				}

				if (gArgs.Target.Name == "river spriteling" && (Step == 7 || Step == 6))
				{
					SendSystemMessage("You slay the spriteling and get it's claw for Master Dunwyn.");
                    GiveItemToPlayer(CreateQuestItem(riverSpritlingClaw));
					ChangeQuestStep(8);
					return;
				}

				if (gArgs.Target.Name == "swamp rat" && (Step == 9 || Step == 8))
				{
					foreach(GenericItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						if(item.QuestName == Name && item.Name == listDunwyn.Name)
						{
							SendSystemMessage(player, "The List disappears in a puff of blue smoke.");
							RemoveItemFromPlayer(item);
							break;
						}
					}

					SendSystemMessage("You slay the rat and retrieve it's tail for Master Dunwyn.");
                    GiveItemToPlayer(CreateQuestItem(swampRatTail));
					ChangeQuestStep(10);
					return;
				}
			}

			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;

				if (gArgs.Target == dunwyn && gArgs.Item.QuestName == Name)
				{
					if(Step == 2 && gArgs.Item.Name == scrollDunwyn.Name)
					{
						dunwyn.SayTo(player, "What? What's this? A letter, for me? Hrm...Let me see what it says. Yes...one moment.");
						RemoveItemFromPlayer(dunwyn, gArgs.Item);
						ChangeQuestStep(3);
						return;
					}
					else if (Step == 10 && gArgs.Item.Name == swampSlimeItem.Name)
					{
						dunwyn.SayTo(player, "Well, least you managed to get a good amount of it. Ok, now give me the spriteling claw.");
                        RemoveItemFromPlayer(dunwyn, gArgs.Item);
						ChangeQuestStep(11);
						return;
					}
					else if (Step == 11 && gArgs.Item.Name == riverSpritlingClaw.Name)
					{
						dunwyn.SayTo(player, "Good, good. A nice sharp one. I like it. Now, give me the swamp rat tail. This had better be a good, thick one!");
                        RemoveItemFromPlayer(dunwyn, gArgs.Item);
						ChangeQuestStep(12);
						return;
					}
					else if (Step == 12 && gArgs.Item.Name == swampRatTail.Name)
					{
						dunwyn.SayTo(player, "Yes yes, this will have to do. Now listen here whipersnapper. I have a few things to do yet, so why don't you get yourself back to your trainer and I'll be along promptly. In fact, let me [teleport] you home. *hehe*");
                        RemoveItemFromPlayer(dunwyn, gArgs.Item);
						ChangeQuestStep(13);
						return;
					}
				}
			}

			if (Step == 16 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target == princessObera)
				{
					SendSystemMessage("You slay the princess and take her head as proof.");
                    GiveItemToPlayer(CreateQuestItem(princessOberasHead));
					ChangeQuestStep(17);

					new RegionTimer(gArgs.Target, new RegionTimerCallback(TalkDunwynClone), 7000);
					return;
				}
			}

			if (Step == 18 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Name == princessOberasHead.Name)
				{
					RemoveItemFromPlayer(masterFrederick, gArgs.Item);
					masterFrederick.SayTo(player, "Excellent work recruit! My, this is a rather large fairy head, don't you think? Well, nevermind that. I'm sure we won't hear from those fairies ever again. And, as per our usual agreement, I have a [reward] for you.");
                    ChangeQuestStep(19);
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			// make sure to clean up, should be needed , but just to make certain
			ResetMasterDunwyn();

			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 8, Util.Random(50)), "You recieve {0} as a reward.");

			if (m_questPlayer.HasAbilityToUseItem(recruitsHelm.CreateInstance() as HeadArmor))
				GiveItemToPlayer(masterFrederick, recruitsHelm.CreateInstance());
			else
				GiveItemToPlayer(masterFrederick, recruitsCap.CreateInstance());

			GiveItemToPlayer(masterFrederick, recruitsRing.CreateInstance());

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
