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
*Source         : http://camelot.allakhazam.com
*Date           : 22 November 2004
*Quest Name     : Unnatural Powers (level 50)
*Quest Classes  : Eldritch, Hero, Ranger, and Warden (Path of Focus)
*Quest Version  : v1
*
*ToDo:
*   Add Bonuses to Epic Items
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	/* The first thing we do, is to declare the quest requirement
 * class linked with the new Quest. To do this, we derive 
 * from the abstract class AbstractQuestDescriptor
 */
	public class Focus_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Focus_50); }
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
			if (player.IsDoingQuest(typeof(Focus_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Hero &&
				player.CharacterClass.ID != (byte)eCharacterClass.Ranger &&
				player.CharacterClass.ID != (byte)eCharacterClass.Warden &&
				player.CharacterClass.ID != (byte)eCharacterClass.Eldritch)
				return false;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;
			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Focus_50), ExtendsType = typeof(AbstractQuest))]
	public class Focus_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Unnatural Powers";

		private static GameNPC Ainrebh = null; // Start NPC
		private static GameMob GreenMaw = null; // Mob to kill

		private static GenericItemTemplate GreenMaw_key = null; //ball of flame

		private static FeetArmorTemplate RangerEpicBoots = null; //Mist Shrouded Boots 
		private static HeadArmorTemplate RangerEpicHelm = null; //Mist Shrouded Coif 
		private static HandsArmorTemplate RangerEpicGloves = null; //Mist Shrouded Gloves 
		private static TorsoArmorTemplate RangerEpicVest = null; //Mist Shrouded Hauberk 
		private static LegsArmorTemplate RangerEpicLegs = null; //Mist Shrouded Legs 
		private static ArmsArmorTemplate RangerEpicArms = null; //Mist Shrouded Sleeves 

		private static FeetArmorTemplate HeroEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate HeroEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate HeroEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate HeroEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate HeroEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate HeroEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate EldritchEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate EldritchEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate EldritchEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate EldritchEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate EldritchEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate EldritchEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate WardenEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate WardenEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate WardenEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate WardenEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate WardenEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate WardenEpicArms = null; //Subterranean Sleeves    

		private static GameNPC[] RangerTrainers = null;
		private static GameNPC[] HeroTrainers = null;
		private static GameNPC[] EldritchTrainers = null;
		private static GameNPC[] WardenTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Ainrebh", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ainrebh , creating it ...");
				Ainrebh = new GameMob();
				Ainrebh.Model = 384;
				Ainrebh.Name = "Ainrebh";
				Ainrebh.GuildName = "Enchanter";
				Ainrebh.Realm = (byte)eRealm.Hibernia;
				Ainrebh.RegionId = 200;
				Ainrebh.Size = 48;
				Ainrebh.Level = 40;
				Ainrebh.Position = new Point(421281, 516273, 1877);
				Ainrebh.Heading = 3254;
				Ainrebh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ainrebh.SaveIntoDatabase();
				}

			}
			else
				Ainrebh = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Green Maw", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find GreenMaw , creating it ...");
				GreenMaw = new GameMob();
				GreenMaw.Model = 146;
				GreenMaw.Name = "Green Maw";
				GreenMaw.GuildName = "";
				GreenMaw.Realm = (byte)eRealm.None;
				GreenMaw.RegionId = 200;
				GreenMaw.Size = 50;
				GreenMaw.Level = 65;
				GreenMaw.Position = new Point(488306, 521440, 6328);
				GreenMaw.Heading = 1162;
				GreenMaw.RespawnInterval = 5 * 60 * 1000;
				GreenMaw.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					GreenMaw.SaveIntoDatabase();
				}

			}
			else
				GreenMaw = npcs[0] as GameMob;
			// end npc

			#endregion

			#region Item Declarations

			GreenMaw_key = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "GreenMaw_key");
			if (GreenMaw_key == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find GreenMaw's Key , creating it ...");
				GreenMaw_key = new GenericItemTemplate();
				GreenMaw_key.ItemTemplateID = "GreenMaw_key";
				GreenMaw_key.Name = "GreenMaw's Key";
				GreenMaw_key.Level = 8;
				GreenMaw_key.Model = 583;
				GreenMaw_key.IsDropable = false;
				GreenMaw_key.IsSaleable = false;
				GreenMaw_key.IsTradable = false;
				GreenMaw_key.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(GreenMaw_key);
				}
			}
			ArmorTemplate i = null;

			#region Ranger
			RangerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "RangerEpicBoots");
			if (RangerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "RangerEpicBoots";
				i.Name = "Mist Shrouded Boots";
				i.Level = 50;
				i.Model = 819;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicBoots = (FeetArmorTemplate)i;
			}

			//Mist Shrouded Coif 
			RangerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "RangerEpicHelm");
			if (RangerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "RangerEpicHelm";
				i.Name = "Mist Shrouded Helm";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicHelm = (HeadArmorTemplate)i;
			}

			//Mist Shrouded Gloves 
			RangerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "RangerEpicGloves");
			if (RangerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "RangerEpicGloves";
				i.Name = "Mist Shrouded Gloves ";
				i.Level = 50;
				i.Model = 818;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_RecurvedBow, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicGloves = (HandsArmorTemplate)i;
			}
			//Mist Shrouded Hauberk 
			RangerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "RangerEpicVest");
			if (RangerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "RangerEpicVest";
				i.Name = "Mist Shrouded Hauberk";
				i.Level = 50;
				i.Model = 815;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicVest = (TorsoArmorTemplate)i;
			}

			//Mist Shrouded Legs 
			RangerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "RangerEpicLegs");
			if (RangerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "RangerEpicLegs";
				i.Name = "Mist Shrouded Leggings";
				i.Level = 50;
				i.Model = 816;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicLegs = (LegsArmorTemplate)i;
			}

			//Mist Shrouded Sleeves 
			RangerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "RangerEpicArms");
			if (RangerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ranger Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "RangerEpicArms";
				i.Name = "Mist Shrouded Sleeves";
				i.Level = 50;
				i.Model = 817;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Ranger);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RangerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Hero
			HeroEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "HeroEpicBoots");
			if (HeroEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "HeroEpicBoots";
				i.Name = "Misted Boots";
				i.Level = 50;
				i.Model = 712;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicBoots = (FeetArmorTemplate)i;
			}

			//Misted Coif 
			HeroEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "HeroEpicHelm");
			if (HeroEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "HeroEpicHelm";
				i.Name = "Misted Coif";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicHelm = (HeadArmorTemplate)i;
			}

			//Misted Gloves 
			HeroEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "HeroEpicGloves");
			if (HeroEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "HeroEpicGloves";
				i.Name = "Misted Gloves ";
				i.Level = 50;
				i.Model = 711;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Shields, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 18));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicGloves = (HandsArmorTemplate)i;
			}

			//Misted Hauberk 
			HeroEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "HeroEpicVest");
			if (HeroEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "HeroEpicVest";
				i.Name = "Misted Hauberk";
				i.Level = 50;
				i.Model = 708;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicVest = (TorsoArmorTemplate)i;
			}

			//Misted Legs 
			HeroEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "HeroEpicLegs");
			if (HeroEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "HeroEpicLegs";
				i.Name = "Misted Leggings";
				i.Level = 50;
				i.Model = 709;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicLegs = (LegsArmorTemplate)i;
			}

			//Misted Sleeves 
			HeroEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "HeroEpicArms");
			if (HeroEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hero Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "HeroEpicArms";
				i.Name = "Misted Sleeves";
				i.Level = 50;
				i.Model = 710;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MagicalBonus.Add(eCharacterClass.Hero);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HeroEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Warden
			WardenEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "WardenEpicBoots");
			if (WardenEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "WardenEpicBoots";
				i.Name = "Mystical Boots";
				i.Level = 50;
				i.Model = 809;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WardenEpicBoots = (FeetArmorTemplate)i;
			}

			//Mystical Coif 
			WardenEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "WardenEpicHelm");
			if (WardenEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "WardenEpicHelm";
				i.Name = "Mystical Coif";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Regrowth, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WardenEpicHelm = (HeadArmorTemplate)i;
			}

			//Mystical Gloves 
			WardenEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "WardenEpicGloves");
			if (WardenEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "WardenEpicGloves";
				i.Name = "Mystical Gloves ";
				i.Level = 50;
				i.Model = 808;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nurture, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WardenEpicGloves = (HandsArmorTemplate)i;
			}

			//Mystical Hauberk 
			WardenEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "WardenEpicVest");
			if (WardenEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "WardenEpicVest";
				i.Name = "Mystical Hauberk";
				i.Level = 50;
				i.Model = 805;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WardenEpicVest = (TorsoArmorTemplate)i;
			}

			//Mystical Legs 
			WardenEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "WardenEpicLegs");
			if (WardenEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "WardenEpicLegs";
				i.Name = "Mystical Legs";
				i.Level = 50;
				i.Model = 806;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WardenEpicLegs);
				}
				WardenEpicLegs = (LegsArmorTemplate)i;
			}

			//Mystical Sleeves 
			WardenEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "WardenEpicArms");
			if (WardenEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "WardenEpicArms";
				i.Name = "Mystical Sleeves";
				i.Level = 50;
				i.Model = 807;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warden);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 45));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WardenEpicArms);
				}
				WardenEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Eldritch
			EldritchEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "EldritchEpicBoots");
			if (EldritchEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "EldritchEpicBoots";
				i.Name = "Mistwoven Boots";
				i.Level = 50;
				i.Model = 382;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicBoots = (FeetArmorTemplate)i;
			}

			//Mist Woven Coif 
			EldritchEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "EldritchEpicHelm");
			if (EldritchEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "EldritchEpicHelm";
				i.Name = "Mistwoven Cap";
				i.Level = 50;
				i.Model = 1298; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Void, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 19));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicHelm = (HeadArmorTemplate)i;
			}

			//Mist Woven Gloves 
			EldritchEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "EldritchEpicGloves");
			if (EldritchEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "EldritchEpicGloves";
				i.Name = "Mistwoven Gloves ";
				i.Level = 50;
				i.Model = 381;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Light, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicGloves = (HandsArmorTemplate)i;
			}

			//Mist Woven Hauberk 
			EldritchEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "EldritchEpicVest");
			if (EldritchEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "EldritchEpicVest";
				i.Name = "Mistwoven Vest";
				i.Level = 50;
				i.Model = 378;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicVest = (TorsoArmorTemplate)i;
			}

			//Mist Woven Legs 
			EldritchEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "EldritchEpicLegs");
			if (EldritchEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "EldritchEpicLegs";
				i.Name = "Mistwoven Pants";
				i.Level = 50;
				i.Model = 379;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicLegs = (LegsArmorTemplate)i;
			}

			//Mist Woven Sleeves 
			EldritchEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "EldritchEpicArms");
			if (EldritchEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "EldritchEpicArms";
				i.Name = "Mistwoven Sleeves";
				i.Level = 50;
				i.Model = 380;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Eldritch);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EldritchEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			RangerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.RangerTrainer), eRealm.Hibernia);
			HeroTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.HeroTrainer), eRealm.Hibernia);
			EldritchTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.EldritchTrainer), eRealm.Hibernia);
			WardenTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.WardenTrainer), eRealm.Hibernia);

			foreach (GameNPC npc in RangerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in HeroTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in EldritchTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in WardenTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.AddHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.AddHandler(GreenMaw, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Ainrebh the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Ainrebh, typeof(Focus_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Ainrebh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.RemoveHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));

			foreach (GameNPC npc in RangerTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in HeroTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in EldritchTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in WardenTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Ainrebh the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Ainrebh, typeof(Focus_50Descriptor));
		}

		protected static void TalkToAinrebh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Focus_50), player, Ainrebh) <= 0)
				return;

			//We also check if the player is already doing the quest
			Focus_50 quest = player.IsDoingQuest(typeof(Focus_50)) as Focus_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Ainrebh.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Ainrebh.SayTo(player, "Hibernia needs your [services]");
				}

			}
			// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendCustomDialog("Will you help Ainrebh [Path of Focus Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}/*
				else
				{
					switch (wArgs.Text)
					{
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
				  */
			}

		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		/*
		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Focus_50 quest = player.IsDoingQuest(typeof(Focus_50)) as Focus_50;

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
		*/
		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (QuestMgr.CanGiveQuest(typeof(Focus_50), player, Ainrebh) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Focus_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Focus_50), player, Ainrebh))
					return;
				player.Out.SendMessage("Kill Green Maw in Cursed Forest loc 42k, 35k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Unnatural Powers (Level 50 Path of Focus Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out GreenMaw in Cursed Forest loc 42k, 35k and kill it!";
					case 2:
						return "[Step #2] Return to Ainrebh and give her Green Maw's Head!";
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
			Focus_50Descriptor a = new Focus_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Focus_50)) != null) return;

			npc.SayTo(player, "Ainrebh has an important task for you, please seek him out in Caille");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Focus_50 quest = (Focus_50)player.IsDoingQuest(typeof(Focus_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("You collect Green Maw's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(GreenMaw_key, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Focus_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == GreenMaw.Name)
				{
					m_questPlayer.Out.SendMessage("You collect Green Maw's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(GreenMaw_key, Name));
					Step = 2;
					return;
				}

			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Ainrebh.Name && gArgs.Item.Name == GreenMaw_key.Name)
				{
					RemoveItemFromPlayer(Ainrebh, GreenMaw_key);
					Ainrebh.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}
		/*
		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, GreenMaw_key, false);
		}
		*/
		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hero)
			{
				GiveItemToPlayer(HeroEpicArms.CreateInstance());
				GiveItemToPlayer(HeroEpicBoots.CreateInstance());
				GiveItemToPlayer(HeroEpicGloves.CreateInstance());
				GiveItemToPlayer(HeroEpicHelm.CreateInstance());
				GiveItemToPlayer(HeroEpicLegs.CreateInstance());
				GiveItemToPlayer(HeroEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Ranger)
			{
				GiveItemToPlayer(RangerEpicArms.CreateInstance());
				GiveItemToPlayer(RangerEpicBoots.CreateInstance());
				GiveItemToPlayer(RangerEpicGloves.CreateInstance());
				GiveItemToPlayer(RangerEpicHelm.CreateInstance());
				GiveItemToPlayer(RangerEpicLegs.CreateInstance());
				GiveItemToPlayer(RangerEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Eldritch)
			{
				GiveItemToPlayer(EldritchEpicArms.CreateInstance());
				GiveItemToPlayer(EldritchEpicBoots.CreateInstance());
				GiveItemToPlayer(EldritchEpicGloves.CreateInstance());
				GiveItemToPlayer(EldritchEpicHelm.CreateInstance());
				GiveItemToPlayer(EldritchEpicLegs.CreateInstance());
				GiveItemToPlayer(EldritchEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warden)
			{
				GiveItemToPlayer(WardenEpicArms.CreateInstance());
				GiveItemToPlayer(WardenEpicBoots.CreateInstance());
				GiveItemToPlayer(WardenEpicGloves.CreateInstance());
				GiveItemToPlayer(WardenEpicHelm.CreateInstance());
				GiveItemToPlayer(WardenEpicLegs.CreateInstance());
				GiveItemToPlayer(WardenEpicVest.CreateInstance());
			}


			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Ainrebh
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Ainrebh 
        *#28 give her the ball of flame
        *#29 talk with Ainrebh about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Mist Shrouded Boots 
            *Mist Shrouded Coif
            *Mist Shrouded Gloves
            *Mist Shrouded Hauberk
            *Mist Shrouded Legs
            *Mist Shrouded Sleeves
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
