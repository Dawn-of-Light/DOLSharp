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
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 7 December 2004
*Quest Name     : Passage to Eternity (level 50)
*Quest Classes  : Paladin, Cleric, (Church)
*Quest Version  : v1
*
*ToDo:
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the quest requirement
 * class linked with the new Quest. To do this, we derive 
 * from the abstract class AbstractQuestDescriptor
 */
	public class Church_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Church_50); }
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
			if (player.IsDoingQuest(typeof(Church_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Cleric &&
				player.CharacterClass.ID != (byte)eCharacterClass.Paladin)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Church_50), ExtendsType = typeof(AbstractQuest))] 
	public class Church_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Passage to Eternity";

		private static GameNPC Roben = null; // Start NPC
		private static GameMob Blythe = null; // Mob to kill

		private static GenericItemTemplate statue_of_arawn = null; //sealed pouch

		private static FeetArmorTemplate ClericEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate ClericEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate ClericEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate ClericEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate ClericEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate ClericEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate PaladinEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate PaladinEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate PaladinEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate PaladinEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate PaladinEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate PaladinEpicArms = null; //Valhalla Touched Sleeves

		private static GameNPC[] ClericTrainers = null;
		private static GameNPC[] PaladinTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Roben Fraomar", eRealm.Albion);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Roben , creating it ...");
				Roben = new GameMob();
				Roben.Model = 36;
				Roben.Name = "Roben Fraomar";
				Roben.GuildName = "";
				Roben.Realm = (byte) eRealm.Albion;
				Roben.RegionId = 1;
				Roben.Size = 52;
				Roben.Level = 50;
				Roben.Position = new Point(408557, 651675, 5200);
				Roben.Heading = 3049;
				Roben.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Roben.SaveIntoDatabase();
			}
			else
				Roben = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Sister Blythe", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Blythe , creating it ...");
				Blythe = new GameMob();
				Blythe.Model = 67;
				Blythe.Name = "Sister Blythe";
				Blythe.GuildName = "";
				Blythe.Realm = (byte) eRealm.None;
				Blythe.RegionId = 1;
				Blythe.Size = 50;
				Blythe.Level = 69;
				Blythe.Position = new Point(322231, 671546, 2762);
				Blythe.Heading = 1683;
				Blythe.RespawnInterval = 5 * 60 * 1000;
				Blythe.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Blythe.SaveIntoDatabase();
			}
			else
				Blythe = npcs[0] as GameMob;
			// end npc

			#endregion

			#region defineItems

			statue_of_arawn = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "statue_of_arawn");
			if (statue_of_arawn == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Statue of Arawn, creating it ...");
				statue_of_arawn = new GenericItemTemplate();
				statue_of_arawn.ItemTemplateID = "statue_of_arawn";
				statue_of_arawn.Name = "Statue of Arawn";
				statue_of_arawn.Level = 8;
				statue_of_arawn.Model = 593;
				statue_of_arawn.IsDropable = false;
				statue_of_arawn.IsSaleable = false;
				statue_of_arawn.IsTradable = false;
				statue_of_arawn.Weight = 12;

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(statue_of_arawn);
			}

			ArmorTemplate i = null;
			#region Cleric
			ClericEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ClericEpicBoots");
			if (ClericEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ClericEpicBoots";
				i.Name = "Boots of Defiant Soul";
				i.Level = 50;
				i.Model = 717;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ClericEpicBoots = (FeetArmorTemplate)i;
			}

			//of the Defiant Soul  Coif 
			ClericEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ClericEpicHelm");
			if (ClericEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ClericEpicHelm";
				i.Name = "Coif of Defiant Soul";
				i.Level = 50;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Enhancement, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicHelm = (HeadArmorTemplate)i;
			}

			//of the Defiant Soul  Gloves 
			ClericEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ClericEpicGloves");
			if (ClericEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ClericEpicGloves";
				i.Name = "Gauntlets of Defiant Soul";
				i.Level = 50;
				i.Model = 716;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Smiting, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicGloves = (HandsArmorTemplate)i;
			}

			//of the Defiant Soul  Hauberk 
			ClericEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ClericEpicVest");
			if (ClericEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ClericEpicVest";
				i.Name = "Habergeon of Defiant Soul";
				i.Level = 50;
				i.Model = 713;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ClericEpicVest = (TorsoArmorTemplate)i;
			}

			//of the Defiant Soul  Legs 
			ClericEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ClericEpicLegs");
			if (ClericEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ClericEpicLegs";
				i.Name = "Chaussess of Defiant Soul";
				i.Level = 50;
				i.Model = 714;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Rejuvenation, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicLegs = (LegsArmorTemplate)i;
			}

			//of the Defiant Soul  Sleeves 
			ClericEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ClericEpicArms");
			if (ClericEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Cleric Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ClericEpicArms";
				i.Name = "Sleeves of Defiant Soul";
				i.Level = 50;
				i.Model = 715;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cleric);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Paladin
			PaladinEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "PaladinEpicBoots");
			if (PaladinEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "PaladinEpicBoots";
				i.Name = "Sabaton of the Iron Will";
				i.Level = 50;
				i.Model = 697;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicBoots = (FeetArmorTemplate)i;
			}

			//of the Iron Will Coif 
			PaladinEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "PaladinEpicHelm");
			if (PaladinEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "PaladinEpicHelm";
				i.Name = "Hounskull of the Iron Will";
				i.Level = 50;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicHelm = (HeadArmorTemplate)i;
			}

			//of the Iron Will Gloves 
			PaladinEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "PaladinEpicGloves");
			if (PaladinEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "PaladinEpicGloves";
				i.Name = "Gauntlets of the Iron Will";
				i.Level = 50;
				i.Model = 696;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicGloves = (HandsArmorTemplate)i;
			}

			//of the Iron Will Hauberk 
			PaladinEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "PaladinEpicVest");
			if (PaladinEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "PaladinEpicVest";
				i.Name = "Curiass of the Iron Will";
				i.Level = 50;
				i.Model = 693;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicVest = (TorsoArmorTemplate)i;
			}

			//of the Iron Will Legs 
			PaladinEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "PaladinEpicLegs");
			if (PaladinEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "PaladinEpicLegs";
				i.Name = "Greaves of the Iron Will";
				i.Level = 50;
				i.Model = 694;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicLegs = (LegsArmorTemplate)i;
			}

			//of the Iron Will Sleeves 
			PaladinEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "PaladinEpicArms");
			if (PaladinEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "PaladinEpicArms";
				i.Name = "Spaulders of the Iron Will";
				i.Level = 50;
				i.Model = 695;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Paladin);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			ClericTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ClericTrainer), eRealm.Albion);
			PaladinTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.PaladinTrainer), eRealm.Albion);

			foreach (GameNPC npc in ClericTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in PaladinTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Roben, GameObjectEvent.Interact, new DOLEventHandler(TalkToRoben));
			GameEventMgr.AddHandler(Roben, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRoben));
			GameEventMgr.AddHandler(Blythe, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Roben the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Roben, typeof(Church_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");

		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Roben == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Roben, GameObjectEvent.Interact, new DOLEventHandler(TalkToRoben));
			GameEventMgr.RemoveHandler(Roben, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRoben));
			GameEventMgr.RemoveHandler(Blythe, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we remove to Roben the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Roben, typeof(Church_50Descriptor));
		}

		protected static void TalkToRoben(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Church_50), player, Roben) <= 0)
				return;

			//We also check if the player is already doing the quest
			Church_50 quest = player.IsDoingQuest(typeof (Church_50)) as Church_50;

			Roben.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest == null)
				{
					Roben.SayTo(player, "It appears that those present when the glyph was made whole received a [vision].");
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							Roben.SayTo(player, "You must not let this occur " + player.GetName(0, false) + "! I am familar with [Lyonesse]. I suggest that you gather a strong group of adventurers in order to succeed in this endeavor!");
							break;
						case 2:
							Roben.SayTo(player, "Where you able to defeat the cult of the dark lord Arawn?");
							break;
					}
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
						case "vision":
							Roben.SayTo(player, "They speak of a broken cathedral located within the borders of Lyonesse. The glyph was able to show the new [occupants] of this cathedral.");
							break;
						case "occupants":
							Roben.SayTo(player, "Occupants that worship not the church of Albion, but the dark lord Arawn! Magess Axton requests that you gather a group and destroy the leader of these dark disciples. She believes these worshippers of Arawan strive to [break the light of camelot] and establish their own religion within our realm.");
							break;
						case "break the light of camelot":
							player.Out.SendCustomDialog("Will you help Roben [Church Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "Lyonesse":
							Roben.SayTo(player, "The cathedral that Axton speaks of lies deep at the heart of that land, behind the Pikeman, across from the Trees. Its remaining walls can be seen at great distances during the day so you should not miss it. I would travel with thee, but my services are required elswhere. Fare thee well " + player.CharacterClass.Name + ".");
							break;
							/*
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;*/
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		/*
		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Church_50 quest = player.IsDoingQuest(typeof (Church_50)) as Church_50;

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
		}*/

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (QuestMgr.CanGiveQuest(typeof(Church_50), player, Roben) <= 0)
				return;

			if (player.IsDoingQuest(typeof (Church_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(Church_50), player, Roben))
					return;

				Roben.SayTo(player, "You must not let this occur " + player.GetName(0, false) + "! I am familar with [Lyonesse]. I suggest that you gather a strong group of adventurers in order to succeed in this endeavor!");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Passage to Eternity (Level 50 Church Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Gather a strong group of adventures and travel to the ancient temple of Arwan. This temple can be found within Lyonesse, surrounded by the dark one's priests. Only by slaying their leader can this evil be stopped!";
					case 2:
						return "[Step #2] Return the statue of Arawn to Roben Fraomar for your reward!";
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
			Church_50Descriptor a = new Church_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Church_50)) != null) return;

			npc.SayTo(player, "Roben Fromar has an important task for you, please seek her out in Cornwall Station");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Church_50 quest = (Church_50)player.IsDoingQuest(typeof(Church_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("As you search the dead body of sister Blythe, you find a sacred " + statue_of_arawn.Name + ", bring it to " + Roben.Name + " has proof of your success.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(statue_of_arawn, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Church_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Blythe.Name)
				{
					m_questPlayer.Out.SendMessage("As you search the dead body of sister Blythe, you find a sacred " + statue_of_arawn.Name + ", bring it to " + Roben.Name + " has proof of your success.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(statue_of_arawn, Name));
					Step = 2;
					return;
				}
			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Roben.Name && gArgs.Item.Name == statue_of_arawn.Name)
				{
					RemoveItemFromPlayer(Roben, statue_of_arawn);
					Roben.SayTo(player, "You have earned this Epic Armour, wear it with honour!");

					FinishQuest();
					return;
				}
			}
		}

		/*
		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(statue_of_arawn);
		}
		 */

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cleric)
			{
				GiveItemToPlayer(ClericEpicBoots.CreateInstance());
				GiveItemToPlayer(ClericEpicArms.CreateInstance());
				GiveItemToPlayer(ClericEpicGloves.CreateInstance());
				GiveItemToPlayer(ClericEpicHelm.CreateInstance());
				GiveItemToPlayer(ClericEpicVest.CreateInstance());
				GiveItemToPlayer(ClericEpicLegs.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Paladin)
			{
				GiveItemToPlayer(PaladinEpicBoots.CreateInstance());
				GiveItemToPlayer(PaladinEpicArms.CreateInstance());
				GiveItemToPlayer(PaladinEpicGloves.CreateInstance());
				GiveItemToPlayer(PaladinEpicHelm.CreateInstance());
				GiveItemToPlayer(PaladinEpicVest.CreateInstance());
				GiveItemToPlayer(PaladinEpicLegs.CreateInstance());
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Roben
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Roben 
        *#28 give her the ball of flame
        *#29 talk with Roben about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Bernor's Numinous Boots 
            *Bernor's Numinous Coif
            *Bernor's Numinous Gloves
            *Bernor's Numinous Hauberk
            *Bernor's Numinous Legs
            *Bernor's Numinous Sleeves
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
