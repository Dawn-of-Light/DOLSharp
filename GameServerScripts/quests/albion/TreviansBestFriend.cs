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
 * Trevian's best friend, Lilybet, has been abducted by bandits and he needs your help to get her back.
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
	public class TreviansBestFriendDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(TreviansBestFriend); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 15; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(TreviansBestFriend), ExtendsType = typeof(AbstractQuest))]
	public class TreviansBestFriend : BaseQuest
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
		protected const string questTitle = "Trevian's best Friend";

		private static GameMob trevian = null;
		private static GameMob puppy = null;

		private static GameMob guardBrydus = null;

		// lilybet is not static since it must be created and deleted for each quest to allow multi users to do quest at the same time...
		private GameMob lilybet = null;

		private const string banditAbductorLeaderName = "Bandit Abductor Leader";
		private static GameMob banditAbductorLeader = null;

		private const string banditAbductorName = "Bandit Abductor Henchman";
		private static GameMob banditAbductor1 = null;
		private static GameMob banditAbductor2 = null;


		private static CloakTemplate treviansHoodedCloak = null;

		private static FeetArmorTemplate bootsOfRescuer = null;
		private static FeetArmorTemplate bootsOfBaneful = null;
		private static FeetArmorTemplate bootsOfProtector = null;

		private static FeetArmorTemplate bootsOfErudition = null;
		private static FeetArmorTemplate bootsOfReverence = null;
		private static FeetArmorTemplate bootsOfShadow = null;
		private static FeetArmorTemplate bootsOfEvanescent = null;
		private static FeetArmorTemplate bootsOfInfluence = null;
		private static FeetArmorTemplate bootsOfTheDevoted = null;

		private static MagicalItemTemplate whistleReward = null; // anyone        

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

			#region defineNPCS

			trevian = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Trevian") as GameMob;
			if (trevian == null)
			{
				trevian = new GameMob();
				trevian.Model = 61;
				trevian.Name = "Trevian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + trevian.Name + ", creating him ...");
				trevian.GuildName = "Part of " + questTitle + " Quest";
				trevian.Realm = (byte) eRealm.Albion;
				trevian.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(798, 36, 0));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(326, 44));
				template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(19));
				trevian.Inventory = template;
				trevian.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				trevian.Size = 52;
				trevian.Level = 20;
				trevian.Position = new Point(456104, 633914, 1693);
				trevian.Heading = 289;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = trevian;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				trevian.OwnBrain = newBrain;

				if(!trevian.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(trevian);
			}

			puppy = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Trevian's Puppy") as GameMob;
			if (puppy == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Trevian's Puppy, creating him ...");
				puppy = new GameMob();
				puppy.Model = 459;
				puppy.Name = "Trevian's Puppy";
				puppy.GuildName = "Part of " + questTitle + " Quest";
				puppy.Realm = (byte) eRealm.Albion;
				puppy.Region = WorldMgr.GetRegion(1);
				puppy.Size = 22;
				puppy.Level = 5;
				puppy.Position = new Point(456051, 633858, 1728);
				puppy.Heading = 3781;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = puppy;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				puppy.OwnBrain = newBrain;

				if(!puppy.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(puppy);
			}

			guardBrydus = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Guard Brydus") as GameMob;
			if (guardBrydus == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Guard Brydus, creating him ...");
				guardBrydus = new GameMob();
				guardBrydus.Model = 27;
				guardBrydus.Name = "Guard Brydus";
				guardBrydus.GuildName = "Part of " + questTitle + " Quest";
				guardBrydus.Realm = (byte) eRealm.Albion;
				guardBrydus.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(6));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(91));
				guardBrydus.Inventory = template;
				guardBrydus.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				guardBrydus.Size = 52;
				guardBrydus.Level = 30;
				guardBrydus.Position = new Point(436698, 650425, 2448);
				guardBrydus.Heading = 184;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = guardBrydus;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				guardBrydus.OwnBrain = newBrain;

				if(!guardBrydus.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(guardBrydus);
			}


			// mob db check
			banditAbductorLeader = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, banditAbductorLeaderName) as GameMob;
			if (banditAbductorLeader == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Mob " + banditAbductorLeaderName + ", creating him ...");
				banditAbductorLeader = new GameMob();
				banditAbductorLeader.Model = 18;
				banditAbductorLeader.Name = banditAbductorLeaderName;
				banditAbductorLeader.GuildName = "Part of " + questTitle + " Quest";
				banditAbductorLeader.Realm = (byte) eRealm.None;
				banditAbductorLeader.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(4));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57));
				banditAbductorLeader.Inventory = template;
				banditAbductorLeader.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				banditAbductorLeader.Size = 52;
				banditAbductorLeader.Level = 10;
				banditAbductorLeader.Position = new Point(438629, 644884, 1904);
				banditAbductorLeader.Heading = 6;

				banditAbductorLeader.RespawnInterval = -1; //auto respawn

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = banditAbductorLeader;
				newBrain.AggroLevel = 0;
				newBrain.AggroRange = 0;
				banditAbductorLeader.OwnBrain = newBrain;

				if(!banditAbductorLeader.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(banditAbductorLeader);
			}

			Region reg = WorldMgr.GetRegion(1);
			if(reg == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			foreach(GameMob obj in reg.GetAllObjects(typeof(GameMob)))
			{
				if(obj.Realm == (byte)eRealm.None && obj.Name == banditAbductorName)
				{
					if (banditAbductor1 == null)
					{
						banditAbductor1 = obj;
						continue;
					}
					
					if(banditAbductor2 == null)
					{
						banditAbductor2 = obj;
						continue;
					}
					
					break;
				}
			}

			GameNpcInventory inventory = new GameNpcInventory();
			inventory.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(4));

			if (banditAbductor1 == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Mob " + banditAbductorName + ", creating him ...");
				banditAbductor1 = new GameMob();
				banditAbductor1.Model = 16;
				banditAbductor1.Name = banditAbductorName;
				banditAbductor1.GuildName = "Part of " + questTitle + " Quest";
				banditAbductor1.Realm = (byte) eRealm.None;
				banditAbductor1.Region = WorldMgr.GetRegion(1);

				banditAbductor1.Inventory = inventory;
				banditAbductor1.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				banditAbductor1.Size = 50;
				banditAbductor1.Level = 9;
				Point pos = banditAbductorLeader.Position;
				pos.X += 100;
				pos.Y -= 100;
				banditAbductor1.Position = pos;
				banditAbductor1.Heading = 50;

				banditAbductor1.RespawnInterval = -1; //auto respawn

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = banditAbductor1;
				newBrain.AggroLevel = 0;
				newBrain.AggroRange = 0;
				banditAbductor1.OwnBrain = newBrain;

				if(!banditAbductor1.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(banditAbductor1);
			}

			if (banditAbductor2 == null)
			{
				// We add two of them ...
				if (log.IsWarnEnabled)
					log.Warn("Could not find Mob " + banditAbductorName + ", creating him ...");
				banditAbductor2 = new GameMob();
				banditAbductor2.Model = 16;
				banditAbductor2.Name = banditAbductorName;
				banditAbductor2.GuildName = "Part of " + questTitle + " Quest";
				banditAbductor2.Realm = (byte) eRealm.None;
				banditAbductor2.Region = WorldMgr.GetRegion(1);

				banditAbductor1.Inventory = inventory;
				banditAbductor1.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				banditAbductor2.Size = 50;
				banditAbductor2.Level = 9;
				Point pos = banditAbductorLeader.Position;
				pos.X -= 150;
				pos.Y -= 150;
				banditAbductor2.Position = pos;
				banditAbductor2.Heading = 0;

				banditAbductor2.RespawnInterval = -1; //auto respawn

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = banditAbductor2;
				newBrain.AggroLevel = 0;
				newBrain.AggroRange = 0;
				banditAbductor2.OwnBrain = newBrain;

				if(!banditAbductor2.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(banditAbductor2);
			}

			#endregion

			#region defineItems

			// item db check
			treviansHoodedCloak = GameServer.Database.SelectObject(typeof (CloakTemplate), Expression.Eq("Name", "Trevians Hooded Cloak")) as CloakTemplate;
			if (treviansHoodedCloak == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Trevians Hodded Cloak, creating it ...");
				treviansHoodedCloak = new CloakTemplate();
				treviansHoodedCloak.Name = "Trevians Hooded Cloak";
				treviansHoodedCloak.Level = 16;
				treviansHoodedCloak.Weight = 2;
				treviansHoodedCloak.Model = 326;

				treviansHoodedCloak.Color = 44;

				treviansHoodedCloak.IsDropable = false;
				treviansHoodedCloak.IsSaleable = false;
				treviansHoodedCloak.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(treviansHoodedCloak);
			}

			// item db check
			bootsOfRescuer = GameServer.Database.SelectObject(typeof (FeetArmorTemplate),  Expression.Eq("Name", "Boots of Rescuer")) as FeetArmorTemplate;
			if (bootsOfRescuer == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Trevians Hodded Cloak, creating it ...");
				bootsOfRescuer = new FeetArmorTemplate();
				bootsOfRescuer.Name = "Boots of Rescuer";
				bootsOfRescuer.Level = 23;

				bootsOfRescuer.Weight = 32;
				bootsOfRescuer.Model = 215; // Special Plate Boots

				bootsOfRescuer.ArmorFactor = 36;
				bootsOfRescuer.ArmorLevel = eArmorLevel.VeryHigh;

				bootsOfRescuer.IsDropable = true;
				bootsOfRescuer.IsSaleable = false;
				bootsOfRescuer.IsTradable = true;
				bootsOfRescuer.Color = 49; // red metal

				bootsOfRescuer.Bonus = 10; // default bonus

				bootsOfRescuer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 7));
				bootsOfRescuer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 6));
				bootsOfRescuer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 6));
				bootsOfRescuer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfRescuer);
			}

			// item db check
			bootsOfBaneful = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Baneful")) as FeetArmorTemplate;
			if (bootsOfBaneful == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Baneful, creating it ...");
				bootsOfBaneful = new FeetArmorTemplate();
				bootsOfBaneful.Name = "Boots of Baneful";
				bootsOfBaneful.Level = 16;

				bootsOfBaneful.Weight = 32;
				bootsOfBaneful.Model = 200; // Special Chain Boots

				bootsOfBaneful.ArmorFactor = 36;
				bootsOfBaneful.ArmorLevel = eArmorLevel.VeryHigh;

				bootsOfBaneful.IsDropable = true;
				bootsOfBaneful.IsSaleable = false;
				bootsOfBaneful.IsTradable = false;
				bootsOfBaneful.Color = 49; // red metal

				bootsOfBaneful.Bonus = 10; // default bonus

				bootsOfBaneful.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 6));
				bootsOfBaneful.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 6));
				bootsOfBaneful.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 3));
				bootsOfBaneful.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 10));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfBaneful);
			}

			// item db check
			bootsOfProtector = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Protector")) as FeetArmorTemplate;
			if (bootsOfProtector == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Protector, creating it ...");
				bootsOfProtector = new FeetArmorTemplate();
				bootsOfProtector.Name = "Boots of Protector";
				bootsOfProtector.Level = 16;

				bootsOfProtector.Weight = 32;
				bootsOfProtector.Model = 200; // Special Chain Boots

				bootsOfProtector.ArmorFactor = 36;
				bootsOfProtector.ArmorLevel = eArmorLevel.VeryHigh;

				bootsOfProtector.IsDropable = true;
				bootsOfProtector.IsSaleable = false;
				bootsOfProtector.IsTradable = true;
				bootsOfProtector.Color = 49; // red metal

				bootsOfProtector.Bonus = 10; // default bonus

				bootsOfProtector.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 6));
				bootsOfProtector.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 2));
				bootsOfProtector.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 3));
				bootsOfProtector.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfProtector);
			}

			// item db check
			bootsOfErudition = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Erudition")) as FeetArmorTemplate;
			if (bootsOfErudition == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Erudition creating it ...");
				bootsOfErudition = new FeetArmorTemplate();
				bootsOfErudition.Name = "Boots of Erudition";
				bootsOfErudition.Level = 16;

				bootsOfErudition.Weight = 8;
				bootsOfErudition.Model = 175; // Special Chain Boots

				bootsOfErudition.ArmorFactor = 18;
				bootsOfErudition.ArmorLevel = eArmorLevel.VeryLow;

				bootsOfErudition.IsDropable = true;
				bootsOfErudition.IsSaleable = false;
				bootsOfErudition.IsTradable = true;
				bootsOfErudition.Color = 27; // red cloth

				bootsOfErudition.Bonus = 10; // default bonus

				bootsOfErudition.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				bootsOfErudition.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 9));
				bootsOfErudition.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfErudition);
			}

			bootsOfReverence = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Reverence")) as FeetArmorTemplate;
			if (bootsOfReverence == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Reverence, creating it ...");
				bootsOfReverence = new FeetArmorTemplate();
				bootsOfReverence.Name = "Boots of Reverence";
				bootsOfReverence.Level = 16;

				bootsOfReverence.Weight = 18;
				bootsOfReverence.Model = 225; // Special Studded Boots

				bootsOfReverence.ArmorFactor = 36;
				bootsOfReverence.ArmorLevel = eArmorLevel.Medium;

				bootsOfReverence.IsDropable = true;
				bootsOfReverence.IsSaleable = false;
				bootsOfReverence.IsTradable = true;
				bootsOfReverence.Color = 9; // red leather

				bootsOfReverence.Bonus = 10; // default bonus

				bootsOfReverence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 7));
				bootsOfReverence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 3));
				bootsOfReverence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold , 3));
				bootsOfReverence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfReverence);
			}

			bootsOfShadow = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Shadow")) as FeetArmorTemplate;
			if (bootsOfShadow == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Shadow, creating it ...");
				bootsOfShadow = new FeetArmorTemplate();
				bootsOfShadow.Name = "Boots of Shadow";
				bootsOfShadow.Level = 16;

				bootsOfShadow.Weight = 18;
				bootsOfShadow.Model = 180; // Special Leather Boots

				bootsOfShadow.ArmorFactor = 36;
				bootsOfShadow.ArmorLevel = eArmorLevel.Low;

				bootsOfShadow.IsDropable = true;
				bootsOfShadow.IsSaleable = false;
				bootsOfShadow.IsTradable = true;
				bootsOfShadow.Color = 9; // red leather

				bootsOfShadow.Bonus = 10; // default bonus

				bootsOfShadow.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 7));
				bootsOfShadow.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 6));
				bootsOfShadow.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 3));
				bootsOfShadow.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfShadow);
			}

			bootsOfEvanescent = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Evanescent")) as FeetArmorTemplate;
			if (bootsOfEvanescent == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Evanescent, creating it ...");
				bootsOfEvanescent = new FeetArmorTemplate();
				bootsOfEvanescent.Name = "Boots of Evanescent";
				bootsOfEvanescent.Level = 16;

				bootsOfEvanescent.Weight = 18;
				bootsOfEvanescent.Model = 180; // Special Studded Boots

				bootsOfEvanescent.ArmorFactor = 36;
				bootsOfEvanescent.ArmorLevel = eArmorLevel.Low;

				bootsOfEvanescent.IsDropable = true;
				bootsOfEvanescent.IsSaleable = false;
				bootsOfEvanescent.IsTradable = true;
				bootsOfEvanescent.Color = 9; // red leather

				bootsOfEvanescent.Bonus = 10; // default bonus

				bootsOfEvanescent.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 7));
				bootsOfEvanescent.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 6));
				bootsOfEvanescent.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 3));
				bootsOfEvanescent.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfEvanescent);
			}

			bootsOfInfluence = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of Influence")) as FeetArmorTemplate;
			if (bootsOfInfluence == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of Influence, creating it ...");
				bootsOfInfluence = new FeetArmorTemplate();
				bootsOfInfluence.Name = "Boots of Influence";
				bootsOfInfluence.Level = 16;

				bootsOfInfluence.Weight = 18;
				bootsOfInfluence.Model = 225; // Special Studded Boots

				bootsOfInfluence.ArmorFactor = 36;
				bootsOfInfluence.ArmorLevel = eArmorLevel.Medium;
				
				bootsOfInfluence.IsDropable = true;
				bootsOfInfluence.IsSaleable = false;
				bootsOfInfluence.IsTradable = true;
				bootsOfInfluence.Color = 9; // red leather

				bootsOfInfluence.Bonus = 10; // default bonus

				bootsOfInfluence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				bootsOfInfluence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 9));
				bootsOfInfluence.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfInfluence);
			}

			bootsOfTheDevoted = GameServer.Database.SelectObject(typeof (FeetArmorTemplate), Expression.Eq("Name", "Boots of the Devoted")) as FeetArmorTemplate;
			if (bootsOfTheDevoted == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Boots of the Devoted, creating it ...");
				bootsOfTheDevoted = new FeetArmorTemplate();
				bootsOfTheDevoted.Name = "Boots of the Devoted";
				bootsOfTheDevoted.Level = 16;

				bootsOfTheDevoted.Weight = 18;
				bootsOfTheDevoted.Model = 180; // Special Leather Boots

				bootsOfTheDevoted.ArmorFactor = 36;
				bootsOfTheDevoted.ArmorLevel = eArmorLevel.Low;
				
				bootsOfTheDevoted.IsDropable = false;
				bootsOfTheDevoted.IsSaleable = false;
				bootsOfTheDevoted.IsTradable = true;
				bootsOfTheDevoted.Color = 9; // red leather

				bootsOfTheDevoted.Bonus = 10; // default bonus

				bootsOfTheDevoted.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 7));
				bootsOfTheDevoted.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 9));
				bootsOfTheDevoted.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 3));
				bootsOfTheDevoted.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 2));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bootsOfTheDevoted);
			}

			whistleReward = GameServer.Database.SelectObject(typeof(MagicalItemTemplate), Expression.Eq("Name", "Trevian's Whistle")) as MagicalItemTemplate;
			if (whistleReward == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Trevian's Whistle, creating it ...");
				whistleReward = new MagicalItemTemplate();
				whistleReward.Name = "Trevian's Whistle";
				whistleReward.Level = 16;
				whistleReward.Weight = 5;
				whistleReward.Model = 1874;

				whistleReward.IsDropable = false;
				whistleReward.IsSaleable = false;
				whistleReward.IsTradable = false;

				//TODO add puppy charge

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(whistleReward);
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

			GameEventMgr.AddHandler(trevian, GameObjectEvent.Interact, new DOLEventHandler(TalkToTrevian));
			GameEventMgr.AddHandler(trevian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToTrevian));

			GameEventMgr.AddHandler(guardBrydus, GameObjectEvent.Interact, new DOLEventHandler(TalkToGuardBrydus));
			GameEventMgr.AddHandler(guardBrydus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardBrydus));

			GameEventMgr.AddHandler(banditAbductorLeader, GameObjectEvent.Interact, new DOLEventHandler(TalkToBanditAbductorLeader));
			GameEventMgr.AddHandler(banditAbductorLeader, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBanditAbductorLeader));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(trevian, typeof(TreviansBestFriendDescriptor));

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
			if (trevian == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(trevian, GameObjectEvent.Interact, new DOLEventHandler(TalkToTrevian));
			GameEventMgr.RemoveHandler(trevian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToTrevian));

			GameEventMgr.RemoveHandler(guardBrydus, GameObjectEvent.Interact, new DOLEventHandler(TalkToGuardBrydus));
			GameEventMgr.RemoveHandler(guardBrydus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardBrydus));

			GameEventMgr.RemoveHandler(banditAbductorLeader, GameObjectEvent.Interact, new DOLEventHandler(TalkToBanditAbductorLeader));
			GameEventMgr.RemoveHandler(banditAbductorLeader, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBanditAbductorLeader));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to trevian the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(trevian , typeof(TreviansBestFriendDescriptor));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				if (quest.lilybet != null)
				{
					quest.lilybet.RemoveFromWorld();
				}
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				if (quest.Step == 6)
				{
					quest.AddLilybet();
					Point pos = player.Position;
					pos.X += 100;
					quest.lilybet.MoveTo(player.Region, pos, (ushort)player.Heading);

					GenericItem cloak = player.Inventory.GetItem(eInventorySlot.Cloak) as GenericItem;
					if (cloak != null && cloak.QuestName == quest.Name && cloak.Name == treviansHoodedCloak.Name)
					{
						quest.lilybet.Follow(player, 100, 5000);
					}
				}
			}


		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToTrevian(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(TreviansBestFriend), player, trevian) <= 0)
				return;

			//We also check if the player is already doing the quest
			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;

			trevian.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					trevian.SayTo(player, "Hail and well met, traveler. You certainly look like an adventuring sort. May I hire you for a mission? Well, it is actually more of a rescue... I should really fill you in on my misfortune before I try to solicit your services, even though it [pains] me to do so.");
					return;
				}
				else
				{
					if (quest.Step >= 6 && quest.lilybet != null && quest.lilybet.Position.CheckSquareDistance(trevian.Position, 2000*2000))
					{
						trevian.SayTo(player, "Thank you so very much! I thought I would never see Lilybet again. She has been such a faithful companion to me considering I am an amateur Sorcerer; I could not bear the thought of [losing her]. Lilybet, go back to the Wharf for some much needed rest.");
						quest.ChangeQuestStep(7);
						quest.lilybet.StopFollow();
						quest.lilybet.WalkTo(new Point(455433, 633010, 1736), quest.lilybet.MaxSpeed);
					}
					else if (quest.Step == 1) // resume talk with trevian 
					{
						trevian.SayTo(player, "We were traveling near Caer Witrin when the dreadful [kidnapping] occurred.");
					}
					else if (quest.Step == 2) // resume talk with trevian 
					{
						trevian.SayTo(player, "You will need to be careful, as the bandit abductors are vicious; fortunately, there were only three present when they stole away with Lilybet. You should go alone when [hunting] for the camp.");
					}
					else
					{
						//If the player is already doing the quest, we ask if he found the fur!                    
						trevian.SayTo(player, "Go and find her, I hope the bandits did no harm to her.");
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
						case "pains":
							trevian.SayTo(player, "My name is Trevian. I do apologize for being so bold in asking for your assistance, but I am so very [distressed] I just don't know what to do.");
							break;
						case "distressed":
							trevian.SayTo(player, "My friend Lilybet and I were traveling near Caer Witrin when some horrid bandits ambushed us and kidnapped her. It was so awful! There was nothing I could do but stand there helpless and watch as they dragged her [away].");
							break;
						case "away":
							trevian.SayTo(player, "I dared not pursue them as I feared for my life, so I ran to fetch help. When I did finally make it back to town I was so distraught that none of the townsfolk could make any sense out of my sobs. They managed to calm me down after awhile, and we set out to search for Lilybet, but to no avail. We searched all night but failed to find the wicked bandits who abducted [Lilybet].");
							break;
						case "Lilybet":
							trevian.SayTo(player, "Three eves have passed since then, and I have searched relentlessly, but I grow weary. Every attempt in finding my dear friend has been in vain. I refuse to give up the search though as I know she is still alive, and I know that she needs my help. Would you please aid me in my search for Lilybet? Please, I [implore] you.");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "implore":
							QuestMgr.ProposeQuestToPlayer(typeof(TreviansBestFriend), "Will you help Trevian find Lilybet?", player, trevian);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "safely":
							trevian.SayTo(player, "We were traveling near Caer Witrin when the dreadful [kidnapping] occurred.");
							break;
						case "kidnapping":
							trevian.SayTo(player, "You should search the areas near Caer Witrin. I have heard rumors of some of those vile thieves setting up camp there. My heart tells me it is they who took Lilybet. Unfortunately, I am at a loss as to where this camp is, but I do know of someone that [might].");
							break;
						case "might":
							trevian.SayTo(player, "Guard Brydus is a guard at Caer Witrin, and he is in charge of some of the patrols in that area. He has eyes and ears in many places. Ask him about bandit abductors, as he may know where their camp is. If he tells you where it is, please travel to the camp immediately to try to [find] Lilybet.");
							if (quest.Step == 1)
							{
								quest.ChangeQuestStep(2);
							}
							break;
						case "find":
							trevian.SayTo(player, "You will need to be careful, as the bandit abductors are vicious; fortunately, there were only three present when they stole away with Lilybet. You should go alone when [hunting] for the camp.");
							break;
						case "hunting":
							trevian.SayTo(player, "Lilybet is mistrustful by nature, and I am sure that this incident has traumatized her. Take my [cloak].");
							break;
						case "cloak":
							// add trevians cloak to inventory
							if (quest.Step == 2)
							{
								GiveItemToPlayer(trevian, CreateQuestItem(treviansHoodedCloak, quest), player);
								trevian.SayTo(player, "Be sure to have this item on you when you find Lilybet after defeating the bandit abductors. Do not lose the cloak. She will know that I sent you by you having this cloak. You will need to bring her back home as soon as you find him as I am sure that she will be disoriented and weak. Please go now as time is running out. I wish you the best on this journey.");
								quest.ChangeQuestStep(3);
							}
							break;

							// Step 6
						case "losing her":
							trevian.SayTo(player, "I was worried about how I would reward you upon your return, but now I know just the perfect thing! I will give you a sturdy pair of boots, and one of the puppies from [Lilybet's litter].");
							break;

						case "Lilybet's litter":
							trevian.SayTo(player, "The puppies are out playing right now, but I will give you a whistle that you can use whenever you want your [puppy] to come to you.");
							break;
						case "puppy":
							trevian.SayTo(player, "Thank you again for your service. I wish you well on your journeys with your new little friend. ");
							if (quest.Step == 7)
							{
								quest.RemoveLylibet();

								RemoveItemFromPlayer(trevian, player.Inventory.GetFirstItemByName(treviansHoodedCloak.Name, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv), player);

								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		protected static void TalkToGuardBrydus(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(TreviansBestFriend), player, trevian) <= 0)
				return;

			//We also check if the player is already doing the quest
			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;

			guardBrydus.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					guardBrydus.SayTo(player, "Hail my friend, you here to help this poor fellow Trevian out. Looking for the [bandit abductor camp],are you?");
					return;
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "bandit abductor camp":
							guardBrydus.SayTo(player, "As a matter of fact, I did see some of those ne'er-do-well scoundrels come fairly close to the Caer just the other night. They ran away after our guards spotted them so we went in pursuit of them. They raced off to the north, northeast. It was too dark to keep up a good chase though, and since there were only three of them they did not pose any real threat to us, so we called off the [chase].");
							break;
						case "chase":
							guardBrydus.SayTo(player, "Aye, try searching the areas to the north northeast, but take caution if ye plan on exploring and gallivanting around. Dangerous creatures lurk about in our realm in these troubled times. Creatures formidable even to veterans of my stature.");
							if (quest.Step == 3)
							{
								quest.ChangeQuestStep(4);
							}
							break;
					}
				}
			}
		}

		protected static void TalkToBanditAbductorLeader(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(TreviansBestFriend), player, trevian) <= 0)
				return;

			//We also check if the player is already doing the quest
			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step >= 3)
				{
					banditAbductorLeader.TurnTo(player);
					banditAbductorLeader.SayTo(player, "Look what we have here, guys. Another little hero from the town trying to rescue our new guard dog. Your here to pick a [fight] with us, or do you want to make us a [offer] for the dog, scum?");
					return;
				}

				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.Step >= 3)
				{
					banditAbductorLeader.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "offer":
							banditAbductorLeader.SayTo(player, "Oh my what a coward. Not even man enough to [fight] for her freedom. Well, ... perhaps we could arrange another [deal]");
							break;
						case "deal":
							banditAbductorLeader.SayTo(player, "I tell you what. For a ransom of, well ..., lets say 50 Gold pieces we will let your little doggie here [free].");
							break;

						case "free":
							player.Out.SendCustomDialog("Do you pay the ransom for Lilybet?", new CustomDialogResponse(CheckPlayerAcceptDeal));
							break;

						case "fight":
							banditAbductorLeader.SayTo(player, "Haa, lets see if you are better than the others that came running to steal our little guarding dog. Get that bastard!");
							AttackPlayer(player);
							break;
					}
				}
			}
		}

		protected static void TalkToLilybet(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(TreviansBestFriend), player, trevian) <= 0)
				return;

			//We also check if the player is already doing the quest
			TreviansBestFriend quest = player.IsDoingQuest(typeof(TreviansBestFriend)) as TreviansBestFriend;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.lilybet == sender)
				{
					quest.lilybet.TurnTo(player);
					
					Cloak cloak = player.Inventory.GetItem(eInventorySlot.Cloak) as Cloak;
					if (cloak != null && cloak.QuestName == quest.Name && cloak.Name == treviansHoodedCloak.Name)
					{
						SendSystemMessage(player, "Mistrust turning to caution, she looks at you anxiously, waiting for you to take her home to Trevian.");
						quest.lilybet.Follow(player, 100, 5000);
						quest.ChangeQuestStep(6);
					}
					else
					{
						SendSystemMessage(player, "Lylibet look mistrusting at you.");
					}
					return;
				}
				return;
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(TreviansBestFriend)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(TreviansBestFriend), player, gArgs.Source as GameNPC))
					{
						SendReply(player, "Thank you so much! You will not be sorry, friend. You are providing a noble service to Lilybet and me as well. I have faith that you will find her and return her [safely].");

						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{
					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
			}
		}

		private static void CheckPlayerAcceptDeal(GamePlayer player, byte response)
		{
			TreviansBestFriend quest = player.IsDoingQuest(typeof (TreviansBestFriend)) as TreviansBestFriend;
			if (quest == null)
				return;

			if (response == 0x00)
			{
				banditAbductorLeader.SayTo(player, "Ohh, my trying to be brave. Show that scum what we do to people trying to be too smart.");
				AttackPlayer(player);
			}
			else
			{
				if (player.RemoveMoney(Money.GetMoney(0, 0, 50, 0, 0)))
				{
					banditAbductorLeader.SayTo(player, "Well lets call it a deal. Let her free, she wasn't any good anyway.");
					quest.AddLilybet();
				}
				else
				{
					banditAbductorLeader.SayTo(player, "You don't have enough money, so don't try to fool me.");
					AttackPlayer(player);
				}
			}
		}

		private static void AttackPlayer(GamePlayer player)
		{
			banditAbductorLeader.StartAttack(player);
			IAggressiveBrain aggroBrain = banditAbductorLeader.Brain as IAggressiveBrain;
			if (aggroBrain != null)
				aggroBrain.AddToAggroList(player, 10);

			banditAbductor1.StartAttack(player);
			aggroBrain = banditAbductor1.Brain as IAggressiveBrain;
			if (aggroBrain != null)
				aggroBrain.AddToAggroList(player, 10);

			banditAbductor2.StartAttack(player);
			aggroBrain = banditAbductor2.Brain as IAggressiveBrain;
			if (aggroBrain != null)
				aggroBrain.AddToAggroList(player, 10);
		}

		private void AddLilybet()
		{
			if (lilybet == null)
			{
				lilybet = new GameMob();
				lilybet.Model = 459;
				lilybet.Name = "Lilybet";
				lilybet.GuildName = "Part of " + questTitle + " Quest";
				lilybet.Realm = (byte) eRealm.Albion;
				lilybet.Region = WorldMgr.GetRegion(1);
				lilybet.Size = 43;
				lilybet.Level = 6;
				Point pos = banditAbductorLeader.Position;
				pos.X -= 300;
				pos.Y -= 500;
				lilybet.Position = pos;
				lilybet.Heading = 330;

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = lilybet;
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				lilybet.OwnBrain = brain;

				lilybet.TurnTo(m_questPlayer);
				lilybet.MaxSpeedBase = m_questPlayer.MaxSpeedBase; // make her as fast as player so that she can keep track with player during follow.

				lilybet.AddToWorld();
				
				GameEventMgr.AddHandler(lilybet, GameObjectEvent.Interact, new DOLEventHandler(TalkToLilybet));
			}
			else
			{
				// if lilybet is alive move here to origial position
				if (lilybet.Alive)
				{
					Point pos = banditAbductorLeader.Position;
					pos.X -= 300;
					pos.Y -= 500;
					lilybet.MoveTo(WorldMgr.GetRegion(1), pos, 330);
				}
				else
				{
					// if she died respawn here to oiginal position
					lilybet.Health = lilybet.MaxHealth;
					lilybet.Mana = lilybet.MaxMana;
					Point pos = banditAbductorLeader.Position;
					pos.X -= 300;
					pos.Y -= 500;
					lilybet.Position = pos;

					lilybet.AddToWorld();
				}
			}
		}

		private void CheckFreeLylibet()
		{
			if ((Step == 4 || Step == 5) && !banditAbductor1.Alive && !banditAbductor1.Alive && !banditAbductor2.Alive)
			{
				SendSystemMessage("As the last of her captors are defeated, Lilybet creeps from out of hiding. ");
				AddLilybet();

				ChangeQuestStep(5);
			}
		}

		private void RemoveLylibet()
		{
			if (lilybet != null)
			{
				GameEventMgr.RemoveHandler(lilybet, GameObjectEvent.Interact, new DOLEventHandler(TalkToLilybet));

				lilybet.RemoveFromWorld();
				lilybet = null;
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
						return "[Step #1] Speak with Trevian.";
					case 2:
						return "[Step #2] Talk to Trevian about trying to find his friend.";
					case 3:
						return "[Step #3] Seek out Guard Brydus at Caer Witrin. Ask him if he knows of a bandit abductor camp nearby.";
					case 4:
						return "[Step #4] Find the bandit camp and slay the bandit abductors. Search the area to the north, northeast of Caer Witrin.";
					case 5:
						return "[Step #5] Speak with Lilybet to get her to follow you back to Trevian. Remember the only way you can earn her trust is by having Trevian's Hooded Cloak. If she goes back to hiding kill the bandit abductor again.";
					case 6:
						return "[Step #6] Return to Trevian as quickly as you can with Lilybet. If she will not follow you it is because you don't have Trevian's Hooded Cloak and she doesn't trust you. Return to Trevian to let him know what happened.";
					case 7:
						return "[Step #7] Speak with Trevian to let him know that Lilybet is safe now.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (TreviansBestFriend)) == null)
				return;

			if (Step >= 3 && Step < 6 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target == banditAbductorLeader)
				{
					CheckFreeLylibet();
					return;
				}
				else if (gArgs.Target == banditAbductor1)
				{
					CheckFreeLylibet();
					return;
				}
				else if (gArgs.Target == banditAbductor2)
				{
					CheckFreeLylibet();
					return;
				}
			}

			if (e == GameLivingBaseEvent.AttackedByEnemy)
			{
				AttackedByEnemyEventArgs aArgs = (AttackedByEnemyEventArgs) args;

				if (aArgs.AttackData.Attacker == banditAbductorLeader || aArgs.AttackData.Attacker == banditAbductor1 || aArgs.AttackData.Attacker == banditAbductor2)
				{
					AttackPlayer(player);
					return;
				}
			}
		}


		public override void FinishQuest()
		{
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			//Give reward to player here ...            
			GiveItemToPlayer(trevian, whistleReward.CreateInstance());

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Armsman)
			{
				GiveItemToPlayer(trevian, bootsOfRescuer.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Reaver)
			{
				GiveItemToPlayer(trevian, bootsOfBaneful.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Paladin)
			{
				GiveItemToPlayer(trevian, bootsOfProtector.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Theurgist || m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Wizard || m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Sorcerer)
			{
				GiveItemToPlayer(trevian, bootsOfErudition.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cleric)
			{
				GiveItemToPlayer(trevian, bootsOfReverence.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Scout)
			{
				GiveItemToPlayer(trevian, bootsOfShadow.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Infiltrator)
			{
				GiveItemToPlayer(trevian, bootsOfEvanescent.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Minstrel)
			{
				GiveItemToPlayer(trevian, bootsOfInfluence.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Friar)
			{
				GiveItemToPlayer(trevian, bootsOfTheDevoted.CreateInstance());
			}

			m_questPlayer.GainExperience(500000, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, Util.Random(10) + 40, Util.Random(50)), "You recieve {0} as a reward for helping Trevian.");

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
