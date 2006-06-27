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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the quest requirement
* class linked with the new Quest. To do this, we derive 
* from the abstract class AbstractQuestDescriptor
*/
	public class Viking_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Viking_50); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 50; }
		}

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof(Viking_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Warrior &&
				player.CharacterClass.ID != (byte)eCharacterClass.Berserker &&
				player.CharacterClass.ID != (byte)eCharacterClass.Thane &&
				player.CharacterClass.ID != (byte)eCharacterClass.Skald &&
				player.CharacterClass.ID != (byte)eCharacterClass.Savage &&
				player.CharacterClassID != (byte)eCharacterClass.Valkyrie)
				return false;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;
			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Viking_50), ExtendsType = typeof(AbstractQuest))]
	public class Viking_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "An End to the Daggers";

		private static GameNPC Lynnleigh = null; // Start NPC
		private static GameMob Ydenia = null; // Mob to kill
		private static GameNPC Elizabeth = null; // reward NPC

		private static GenericItemTemplate tome_enchantments = null;
		private static GenericItemTemplate sealed_pouch = null;

		private static FeetArmorTemplate WarriorEpicBoots = null;
		private static HeadArmorTemplate WarriorEpicHelm = null;
		private static HandsArmorTemplate WarriorEpicGloves = null;
		private static LegsArmorTemplate WarriorEpicLegs = null;
		private static ArmsArmorTemplate WarriorEpicArms = null;
		private static TorsoArmorTemplate WarriorEpicVest = null;

		private static FeetArmorTemplate BerserkerEpicBoots = null;
		private static HeadArmorTemplate BerserkerEpicHelm = null;
		private static HandsArmorTemplate BerserkerEpicGloves = null;
		private static LegsArmorTemplate BerserkerEpicLegs = null;
		private static ArmsArmorTemplate BerserkerEpicArms = null;
		private static TorsoArmorTemplate BerserkerEpicVest = null;

		private static FeetArmorTemplate ThaneEpicBoots = null;
		private static HeadArmorTemplate ThaneEpicHelm = null;
		private static HandsArmorTemplate ThaneEpicGloves = null;
		private static LegsArmorTemplate ThaneEpicLegs = null;
		private static ArmsArmorTemplate ThaneEpicArms = null;
		private static TorsoArmorTemplate ThaneEpicVest = null;

		private static FeetArmorTemplate SkaldEpicBoots = null;
		private static HeadArmorTemplate SkaldEpicHelm = null;
		private static HandsArmorTemplate SkaldEpicGloves = null;
		private static LegsArmorTemplate SkaldEpicLegs = null;
		private static ArmsArmorTemplate SkaldEpicArms = null;
		private static TorsoArmorTemplate SkaldEpicVest = null;

		private static FeetArmorTemplate SavageEpicBoots = null;
		private static HeadArmorTemplate SavageEpicHelm = null;
		private static HandsArmorTemplate SavageEpicGloves = null;
		private static LegsArmorTemplate SavageEpicLegs = null;
		private static ArmsArmorTemplate SavageEpicArms = null;
		private static TorsoArmorTemplate SavageEpicVest = null;

		private static FeetArmorTemplate ValkyrieEpicBoots = null;
		private static HeadArmorTemplate ValkyrieEpicHelm = null;
		private static HandsArmorTemplate ValkyrieEpicGloves = null;
		private static LegsArmorTemplate ValkyrieEpicLegs = null;
		private static ArmsArmorTemplate ValkyrieEpicArms = null;
		private static TorsoArmorTemplate ValkyrieEpicVest = null;

		private static GameNPC[] WarriorTrainers = null;
		private static GameNPC[] BerserkerTrainers = null;
		private static GameNPC[] ThaneTrainers = null;
		private static GameNPC[] SkaldTrainers = null;
		private static GameNPC[] SavageTrainers = null;
		private static GameNPC[] ValkyrieTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lynnleigh", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lynnleigh , creating it ...");
				Lynnleigh = new GameMob();
				Lynnleigh.Model = 217;
				Lynnleigh.Name = "Lynnleigh";
				Lynnleigh.GuildName = "";
				Lynnleigh.Realm = (byte)eRealm.Midgard;
				Lynnleigh.RegionId = 100;
				Lynnleigh.Size = 51;
				Lynnleigh.Level = 50;
				Lynnleigh.Position = new Point(760085, 758453, 4736);
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
				Elizabeth = new GameMob();
				Elizabeth.Model = 217;
				Elizabeth.Name = "Elizabeth";
				Elizabeth.GuildName = "Enchanter";
				Elizabeth.Realm = (byte)eRealm.Midgard;
				Elizabeth.RegionId = 100;
				Elizabeth.Size = 51;
				Elizabeth.Level = 41;
				Elizabeth.Position = new Point(802849, 727081, 4681);
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
				Ydenia = new GameMob();
				Ydenia.Model = 217;
				Ydenia.Name = "Ydenia of Seithkona";
				Ydenia.GuildName = "";
				Ydenia.Realm = (byte)eRealm.None;
				Ydenia.RegionId = 100;
				Ydenia.Size = 100;
				Ydenia.Level = 65;
				Ydenia.Position = new Point(637680, 767189, 4480);
				Ydenia.Heading = 2156;
				Ydenia.Flags = 1;
				Ydenia.MaxSpeedBase = 200;
				Ydenia.RespawnInterval = 5 * 60 * 1000;
				Ydenia.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ydenia.SaveIntoDatabase();
				}

			}
			else
				Ydenia = npcs[0] as GameMob;
			// end npc

			#endregion

			#region defineItems

			tome_enchantments = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "tome_enchantments");
			if (tome_enchantments == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Tome of Enchantments , creating it ...");
				tome_enchantments = new GenericItemTemplate();
				tome_enchantments.ItemTemplateID = "tome_enchantments";
				tome_enchantments.Name = "Tome of Enchantments";
				tome_enchantments.Level = 8;
				tome_enchantments.Model = 500;
				tome_enchantments.IsDropable = false;
				tome_enchantments.IsSaleable = false;
				tome_enchantments.IsTradable = false;
				tome_enchantments.Weight = 12;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(tome_enchantments);
				}
			}

			sealed_pouch = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "sealed_pouch");
			if (sealed_pouch == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sealed Pouch , creating it ...");
				sealed_pouch = new GenericItemTemplate();
				sealed_pouch.ItemTemplateID = "sealed_pouch";
				sealed_pouch.Name = "Sealed Pouch";
				sealed_pouch.Level = 8;
				sealed_pouch.Model = 488;
				sealed_pouch.IsDropable = false;
				sealed_pouch.IsSaleable = false;
				sealed_pouch.IsTradable = false;
				sealed_pouch.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(sealed_pouch);
				}
			}
			ArmorTemplate i = null;
			#region Warrior
			WarriorEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "WarriorEpicBoots");
			if (WarriorEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "WarriorEpicBoots";
				i.Name = "Tyr's Might Boots";
				i.Level = 50;
				i.Model = 780;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicBoots = (FeetArmorTemplate)i;
			}

			WarriorEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "WarriorEpicHelm");
			if (WarriorEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "WarriorEpicHelm";
				i.Name = "Tyr's Might Coif";
				i.Level = 50;
				i.Model = 832; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 11));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicHelm = (HeadArmorTemplate)i;
			}

			WarriorEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "WarriorEpicGloves");
			if (WarriorEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "WarriorEpicGloves";
				i.Name = "Tyr's Might Gloves";
				i.Level = 50;
				i.Model = 779;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Shields, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicGloves = (HandsArmorTemplate)i;
			}

			WarriorEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "WarriorEpicVest");
			if (WarriorEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "WarriorEpicVest";
				i.Name = "Tyr's Might Hauberk";
				i.Level = 50;
				i.Model = 776;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicVest = (TorsoArmorTemplate)i;
			}

			WarriorEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "WarriorEpicLegs");
			if (WarriorEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "WarriorEpicLegs";
				i.Name = "Tyr's Might Legs";
				i.Level = 50;
				i.Model = 777;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicLegs = (LegsArmorTemplate)i;
			}

			WarriorEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "WarriorEpicArms");
			if (WarriorEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "WarriorEpicArms";
				i.Name = "Tyr's Might Sleeves";
				i.Level = 50;
				i.Model = 778;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warrior);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 9));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarriorEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Berserker
			BerserkerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "BerserkerEpicBoots");
			if (BerserkerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicBoots";
				i.Name = "Courage Bound Boots";
				i.Level = 50;
				i.Model = 755;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicBoots);
				}
				BerserkerEpicBoots = (FeetArmorTemplate)i;
			}

			BerserkerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "BerserkerEpicHelm");
			if (BerserkerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicHelm";
				i.Name = "Courage Bound Helm";
				i.Level = 50;
				i.Model = 829; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BerserkerEpicHelm = (HeadArmorTemplate)i;
			}

			BerserkerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "BerserkerEpicGloves");
			if (BerserkerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicGloves";
				i.Name = "Courage Bound Gloves";
				i.Level = 50;
				i.Model = 754;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Left_Axe, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BerserkerEpicGloves = (HandsArmorTemplate)i;
			}

			BerserkerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "BerserkerEpicVest");
			if (BerserkerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicVest";
				i.Name = "Courage Bound Jerkin";
				i.Level = 50;
				i.Model = 751;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BerserkerEpicVest = (TorsoArmorTemplate)i;
			}

			BerserkerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "BerserkerEpicLegs");
			if (BerserkerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicLegs";
				i.Name = "Courage Bound Leggings";
				i.Level = 50;
				i.Model = 752;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BerserkerEpicLegs = (LegsArmorTemplate)i;
			}

			BerserkerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "BerserkerEpicArms");
			if (BerserkerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "BerserkerEpicArms";
				i.Name = "Courage Bound Sleeves";
				i.Level = 50;
				i.Model = 753;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Berserker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BerserkerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Thane
			ThaneEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ThaneEpicBoots");
			if (ThaneEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ThaneEpicBoots";
				i.Name = "Storm Touched Boots";
				i.Level = 50;
				i.Model = 791;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicBoots = (FeetArmorTemplate)i;
			}

			ThaneEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ThaneEpicHelm");
			if (ThaneEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ThaneEpicHelm";
				i.Name = "Storm Touched Coif";
				i.Level = 50;
				i.Model = 832; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Stormcalling, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicHelm = (HeadArmorTemplate)i;
			}

			ThaneEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ThaneEpicGloves");
			if (ThaneEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ThaneEpicGloves";
				i.Name = "Storm Touched Gloves";
				i.Level = 50;
				i.Model = 790;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Sword, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Hammer, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Axe, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicGloves = (HandsArmorTemplate)i;
			}

			ThaneEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ThaneEpicVest");
			if (ThaneEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ThaneEpicVest";
				i.Name = "Storm Touched Hauberk";
				i.Level = 50;
				i.Model = 787;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicLegs = (LegsArmorTemplate)i;
			}

			ThaneEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ThaneEpicLegs");
			if (ThaneEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ThaneEpicLegs";
				i.Name = "Storm Touched Legs";
				i.Level = 50;
				i.Model = 788;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicLegs = (LegsArmorTemplate)i;
			}

			ThaneEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ThaneEpicArms");
			if (ThaneEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ThaneEpicArms";
				i.Name = "Storm Touched Sleeves";
				i.Level = 50;
				i.Model = 789;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Thane);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ThaneEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Skald
			//Valhalla Touched Boots
			SkaldEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "SkaldEpicBoots");
			if (SkaldEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "SkaldEpicBoots";
				i.Name = "Battlesung Boots";
				i.Level = 50;
				i.Model = 775;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicBoots = (FeetArmorTemplate)i;
			}

			//Valhalla Touched Coif 
			SkaldEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "SkaldEpicHelm");
			if (SkaldEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "SkaldEpicHelm";
				i.Name = "Battlesung Coif";
				i.Level = 50;
				i.Model = 832; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Battlesongs, 5));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicHelm = (HeadArmorTemplate)i;
			}

			//Valhalla Touched Gloves 
			SkaldEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "SkaldEpicGloves");
			if (SkaldEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "SkaldEpicGloves";
				i.Name = "Battlesung Gloves";
				i.Level = 50;
				i.Model = 774;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicGloves = (HandsArmorTemplate)i;
			}

			//Valhalla Touched Hauberk 
			SkaldEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "SkaldEpicVest");
			if (SkaldEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "SkaldEpicVest";
				i.Name = "Battlesung Hauberk";
				i.Level = 50;
				i.Model = 771;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicVest = (TorsoArmorTemplate)i;
			}

			//Valhalla Touched Legs 
			SkaldEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "SkaldEpicLegs");
			if (SkaldEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "SkaldEpicLegs";
				i.Name = "Battlesung Legs";
				i.Level = 50;
				i.Model = 772;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicLegs = (LegsArmorTemplate)i;
			}

			//Valhalla Touched Sleeves 
			SkaldEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "SkaldEpicArms");
			if (SkaldEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skald Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "SkaldEpicArms";
				i.Name = "Battlesung Sleeves";
				i.Level = 50;
				i.Model = 773;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Skald);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SkaldEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Savage
			//Subterranean Boots 
			SavageEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "SavageEpicBoots");
			if (SavageEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "SavageEpicBoots";
				i.Name = "Kelgor's Battle Boots";
				i.Level = 50;
				i.Model = 1196;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicBoots = (FeetArmorTemplate)i;
			}

			//Subterranean Coif 
			SavageEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "SavageEpicHelm");
			if (SavageEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "SavageEpicHelm";
				i.Name = "Kelgor's Battle Helm";
				i.Level = 50;
				i.Model = 824; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicHelm = (HeadArmorTemplate)i;
			}

			//Subterranean Gloves 
			SavageEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "SavageEpicGloves");
			if (SavageEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "SavageEpicGloves";
				i.Name = "Kelgor's Battle Gauntlets";
				i.Level = 50;
				i.Model = 1195;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_HandToHand, 3));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicGloves = (HandsArmorTemplate)i;
			}

			//Subterranean Hauberk 
			SavageEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "SavageEpicVest");
			if (SavageEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "SavageEpicVest";
				i.Name = "Kelgor's Battle Vest";
				i.Level = 50;
				i.Model = 1192;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicVest = (TorsoArmorTemplate)i;
			}

			//Subterranean Legs 
			SavageEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "SavageEpicLegs");
			if (SavageEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "SavageEpicLegs";
				i.Name = "Kelgor's Battle Leggings";
				i.Level = 50;
				i.Model = 1193;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicLegs = (LegsArmorTemplate)i;
			}

			//Subterranean Sleeves 
			SavageEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "SavageEpicArms");
			if (SavageEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "SavageEpicArms";
				i.Name = "Kelgor's Battle Sleeves";
				i.Level = 50;
				i.Model = 1194;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Savage);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SavageEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Valkyrie
			//Valhalla Touched Boots
			ValkyrieEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ValkyrieEpicBoots");
			if (ValkyrieEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyries Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicBoots";
				i.Name = "Battle Maiden's Boots";
				i.Level = 50;
				i.Model = 2932;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 7 pts
				 *   Dexterity: 13 pts
				 *   Quickness: 13 pts
				 *   Body Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicBoots = (FeetArmorTemplate)i;
			}

			//Valhalla Touched Coif 
			ValkyrieEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ValkyrieEpicHelm");
			if (ValkyrieEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyries Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicHelm";
				i.Name = "Battle Maiden's Coif";
				i.Level = 50;
				i.Model = 832; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Sword: 4 pts
				 *   Constitution: 18 pts
				 *   Cold Resist: 4%
				 *   Energy Resist: 6%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Sword, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicHelm = (HeadArmorTemplate)i;
			}

			//Valhalla Touched Gloves 
			ValkyrieEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ValkyrieEpicGloves");
			if (ValkyrieEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyries Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicGloves";
				i.Name = "Battle Maiden's Gloves";
				i.Level = 50;
				i.Model = 2931;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Spear: 3 pts
				 *   Parry: +3 pts
				 *   Strength: 19 pts
				 *   Odin's Will: +3 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Spear, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_OdinsWill, 3));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicGloves = (HandsArmorTemplate)i;
			}

			//Valhalla Touched Hauberk 
			ValkyrieEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ValkyrieEpicVest");
			if (ValkyrieEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyries Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicVest";
				i.Name = "Battle Maiden's Hauberk";
				i.Level = 50;
				i.Model = 2928;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Strength: 13 pts
				 *   Constitution: 13 pts
				 *   Slash Resist: 6%
				 *   Hits: 30 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicVest = (TorsoArmorTemplate)i;
			}

			//Valhalla Touched Legs 
			ValkyrieEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ValkyrieEpicLegs");
			if (ValkyrieEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyries Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicLegs";
				i.Name = "Battle Maiden's Legs";
				i.Level = 50;
				i.Model = 2929;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 19 pts
				 *	 Piety: 15 pts
				 *   Crush Resist: 8%
				 *   Heat Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicLegs = (LegsArmorTemplate)i;
			}

			//Valhalla Touched Sleeves 
			ValkyrieEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ValkyrieEpicArms");
			if (ValkyrieEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ValkyrieEpicArms";
				i.Name = "Battle Maiden's Sleeves";
				i.Level = 50;
				i.Model = 2930;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valkyrie);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Strength: 18 pts
				 *   Quickness: 16 pts
				 *   Thrust Resist: 8%
				 *   Spirit Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValkyrieEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			WarriorTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.WarriorTrainer), eRealm.Midgard);
			BerserkerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.BerserkerTrainer), eRealm.Midgard);
			ThaneTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ThaneTrainer), eRealm.Midgard);
			SkaldTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.SkaldTrainer), eRealm.Midgard);
			SavageTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.SavageTrainer), eRealm.Midgard);
			ValkyrieTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ValkyrieTrainer), eRealm.Midgard);


			foreach (GameNPC npc in WarriorTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BerserkerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ThaneTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SkaldTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SavageTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ValkyrieTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.AddHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));

			GameEventMgr.AddHandler(Ydenia, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Lynnleigh the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Lynnleigh, typeof(Viking_50Descriptor));

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
			GameEventMgr.RemoveHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.RemoveHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));

			GameEventMgr.RemoveHandler(Ydenia, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));
			foreach (GameNPC npc in WarriorTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BerserkerTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ThaneTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SkaldTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SavageTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ValkyrieTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Lynnleigh the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Lynnleigh, typeof(Viking_50Descriptor));
		}

		protected static void TalkToLynnleigh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Viking_50), player, Lynnleigh) <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof(Viking_50)) as Viking_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "worries me":
							Lynnleigh.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Ydenia] and his minions.");
							break;
						case "face Ydenia":
							player.Out.SendCustomDialog("Will you face Ydenia [Viking Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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

			if (QuestMgr.CanGiveQuest(typeof(Viking_50), player, Lynnleigh) <= 0)
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
								quest.FinishQuest();
							break;
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Viking_50 quest = player.IsDoingQuest(typeof(Viking_50)) as Viking_50;

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

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (QuestMgr.CanGiveQuest(typeof(Viking_50), player, Lynnleigh) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Viking_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Viking_50), player, Lynnleigh))
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
						return "[Step #3] Take the sealed pouch to Elizabeth in Mularn for your reward, and say you ready to 'take them'!";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		protected static void TalkToTrainer(DOLEvent e, object sender, EventArgs args)
		{
			InteractEventArgs iargs = args as InteractEventArgs;

			GamePlayer player = iargs.Source as GamePlayer;
			GameNPC npc = sender as GameNPC;
			Viking_50Descriptor a = new Viking_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Viking_50)) == null) return;

			npc.SayTo(player, "Lynnleigh has an important task for you, please seek her out on the island South West of Vasudheim");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Viking_50 quest = (Viking_50)player.IsDoingQuest(typeof(Viking_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				quest.Step = 2;
				quest.GiveItemToPlayer(CreateQuestItem(tome_enchantments, quest.Name));
				player.Out.SendMessage("Ydenia drops the Tome of Enchantments and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Viking_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == Ydenia.Name)
				{
					Step = 2;
					GiveItemToPlayer(CreateQuestItem(tome_enchantments, Name));
					m_questPlayer.Out.SendMessage("Ydenia drops the Tome of Enchantments and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
			 */

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Lynnleigh.Name && gArgs.Item.Name == tome_enchantments.Name)
				{
					RemoveItemFromPlayer(Lynnleigh, tome_enchantments);
					Lynnleigh.SayTo(player, "Take this sealed pouch to Elizabeth in Mularn for your reward!");
					GiveItemToPlayer(Lynnleigh, CreateQuestItem(sealed_pouch, Name));
					Step = 3;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Elizabeth.Name && gArgs.Item.Name == sealed_pouch.Name)
				{
					RemoveItemFromPlayer(Elizabeth, sealed_pouch);
					Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 4;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(sealed_pouch);
			RemoveItemFromPlayer(tome_enchantments);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warrior)
			{
				GiveItemToPlayer(WarriorEpicArms);
				GiveItemToPlayer(WarriorEpicBoots);
				GiveItemToPlayer(WarriorEpicGloves);
				GiveItemToPlayer(WarriorEpicHelm);
				GiveItemToPlayer(WarriorEpicLegs);
				GiveItemToPlayer(WarriorEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Berserker)
			{
				GiveItemToPlayer(BerserkerEpicArms);
				GiveItemToPlayer(BerserkerEpicBoots);
				GiveItemToPlayer(BerserkerEpicGloves);
				GiveItemToPlayer(BerserkerEpicHelm);
				GiveItemToPlayer(BerserkerEpicLegs);
				GiveItemToPlayer(BerserkerEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Thane)
			{
				GiveItemToPlayer(ThaneEpicArms);
				GiveItemToPlayer(ThaneEpicBoots);
				GiveItemToPlayer(ThaneEpicGloves);
				GiveItemToPlayer(ThaneEpicHelm);
				GiveItemToPlayer(ThaneEpicLegs);
				GiveItemToPlayer(ThaneEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Skald)
			{
				GiveItemToPlayer(SkaldEpicArms);
				GiveItemToPlayer(SkaldEpicBoots);
				GiveItemToPlayer(SkaldEpicGloves);
				GiveItemToPlayer(SkaldEpicHelm);
				GiveItemToPlayer(SkaldEpicLegs);
				GiveItemToPlayer(SkaldEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Savage)
			{
				GiveItemToPlayer(SavageEpicArms);
				GiveItemToPlayer(SavageEpicBoots);
				GiveItemToPlayer(SavageEpicGloves);
				GiveItemToPlayer(SavageEpicHelm);
				GiveItemToPlayer(SavageEpicLegs);
				GiveItemToPlayer(SavageEpicVest);
			}
			else if (m_questPlayer.CharacterClassID == (byte)eCharacterClass.Valkyrie)
			{
				GiveItemToPlayer(ValkyrieEpicArms);
				GiveItemToPlayer(ValkyrieEpicBoots);
				GiveItemToPlayer(ValkyrieEpicGloves);
				GiveItemToPlayer(ValkyrieEpicHelm);
				GiveItemToPlayer(ValkyrieEpicLegs);
				GiveItemToPlayer(ValkyrieEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}
	}
}
