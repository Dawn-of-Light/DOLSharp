/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Theurgist, Armsman, Scout, and Friar (Defenders of Albion)
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

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class Defenders_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Defenders_50); }
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
			if (player.IsDoingQuest(typeof(Defenders_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Armsman &&
				player.CharacterClass.ID != (byte)eCharacterClass.Scout &&
				player.CharacterClass.ID != (byte)eCharacterClass.Theurgist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Friar)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Defenders_50), ExtendsType = typeof(AbstractQuest))]
	public class Defenders_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Feast of the Decadent";

		private static GameNPC Lidmann = null; // Start NPC
		private static GameMob Uragaig = null; // Mob to kill

		private static GenericItemTemplate sealed_pouch = null; //sealed pouch

		private static FeetArmorTemplate ScoutEpicBoots = null; //Brigandine of Vigilant Defense  Boots 
		private static HeadArmorTemplate ScoutEpicHelm = null; //Brigandine of Vigilant Defense  Coif 
		private static HandsArmorTemplate ScoutEpicGloves = null; //Brigandine of Vigilant Defense  Gloves 
		private static TorsoArmorTemplate ScoutEpicVest = null; //Brigandine of Vigilant Defense  Hauberk 
		private static LegsArmorTemplate ScoutEpicLegs = null; //Brigandine of Vigilant Defense  Legs 
		private static ArmsArmorTemplate ScoutEpicArms = null; //Brigandine of Vigilant Defense  Sleeves 

		private static FeetArmorTemplate ArmsmanEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate ArmsmanEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate ArmsmanEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate ArmsmanEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate ArmsmanEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate ArmsmanEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate TheurgistEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate TheurgistEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate TheurgistEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate TheurgistEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate TheurgistEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate TheurgistEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate FriarEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate FriarEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate FriarEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate FriarEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate FriarEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate FriarEpicArms = null; //Subterranean Sleeves 

		private static GameNPC[] ScoutTrainers = null;
		private static GameNPC[] ArmsmanTrainers = null;
		private static GameNPC[] TheurgistTrainers = null;
		private static GameNPC[] FriarTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{

				Lidmann = new GameMob();
				Lidmann.Model = 64;
				Lidmann.Name = "Lidmann Halsey";

				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Lidmann.Name + ", creating it ...");

				Lidmann.GuildName = "";
				Lidmann.Realm = (byte)eRealm.Albion;
				Lidmann.RegionId = 1;
				Lidmann.Size = 50;
				Lidmann.Level = 50;
				Lidmann.Position = new Point(466464, 634554, 1954);
				Lidmann.Heading = 1809;
				Lidmann.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lidmann.SaveIntoDatabase();
				}

			}
			else
				Lidmann = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailleach Uragaig", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Uragaig , creating it ...");
				Uragaig = new GameMob();
				Uragaig.Model = 349;
				Uragaig.Name = "Cailleach Uragaig";
				Uragaig.GuildName = "";
				Uragaig.Realm = (byte)eRealm.None;
				Uragaig.RegionId = 1;
				Uragaig.Size = 55;
				Uragaig.Level = 70;
				Uragaig.Position = new Point(316218, 664484, 2736);
				Uragaig.Heading = 3072;
				Uragaig.RespawnInterval = 5 * 60 * 1000;
				Uragaig.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Uragaig.SaveIntoDatabase();
				}

			}
			else
				Uragaig = npcs[0] as GameMob;
			// end npc

			#endregion

			#region defineItems

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
			#region Scout
			ArmorTemplate i = null;
			ScoutEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ScoutEpicBoots");
			if (ScoutEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ScoutEpicBoots";
				i.Name = "Brigandine Boots of Vigilant Defense";
				i.Level = 50;
				i.Model = 731;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +10, Dex +18, Qui +15, Spirit +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ScoutEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			//Brigandine of Vigilant Defense  Coif
			ScoutEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ScoutEpicHelm");
			if (ScoutEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ScoutEpicHelm";
				i.Name = "Brigandine Coif of Vigilant Defense";
				i.Level = 50;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Dex +12, Qui +22, Crush +8%, Heat +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ScoutEpicHelm = (HeadArmorTemplate)i;
			}

			//Brigandine of Vigilant Defense  Gloves
			ScoutEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ScoutEpicGloves");
			if (ScoutEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ScoutEpicGloves";
				i.Name = "Brigandine Gloves of Vigilant Defense";
				i.Level = 50;
				i.Model = 732;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Dex +21, Longbow +5, Body +8%, Slash +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Long_bows, 5));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ScoutEpicGloves = (HandsArmorTemplate)i;
			}

			//Brigandine of Vigilant Defense  Hauberk
			ScoutEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ScoutEpicVest");
			if (ScoutEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ScoutEpicVest";
				i.Name = "Brigandine Jerkin of Vigilant Defense";
				i.Level = 50;
				i.Model = 728;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Str +18, HP +45, Spirit +4%, Thrust +4%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 45));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ScoutEpicVest = (TorsoArmorTemplate)i;
			}

			//Brigandine of Vigilant Defense  Legs
			ScoutEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ScoutEpicLegs");
			if (ScoutEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ScoutEpicLegs";
				i.Name = "Brigandine Legs of Vigilant Defense";
				i.Level = 50;
				i.Model = 729;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +22, Dex +15, Qui +7, Spirit +6%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ScoutEpicLegs = (LegsArmorTemplate)i;
			}

			//Brigandine of Vigilant Defense  Sleeves
			ScoutEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ScoutEpicArms");
			if (ScoutEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ScoutEpicArms";
				i.Name = "Brigandine Sleeves of Vigilant Defense";
				i.Level = 50;
				i.Model = 730;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Scout);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +22, Str +18, Energy +8%, Slash +4%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ScoutEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Armsman

			//Armsman Epic Boots Start
			ArmsmanEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ArmsmanEpicBoots");
			if (ArmsmanEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicBoots";
				i.Name = "Sabaton of the Stalwart Arm";
				i.Level = 50;
				i.Model = 692;
				i.IsDropable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Str +15, Qui +15, Spirit +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ArmsmanEpicBoots = (FeetArmorTemplate)i;
			}

			//of the Stalwart Arm Coif
			ArmsmanEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ArmsmanEpicHelm");
			if (ArmsmanEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicHelm";
				i.Name = "Coif of the Stalwart Arm";
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
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +19, Qui +18, Body +6%, Crush +6%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ArmsmanEpicHelm = (HeadArmorTemplate)i;
			}

			//of the Stalwart Arm Gloves
			ArmsmanEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ArmsmanEpicGloves");
			if (ArmsmanEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicGloves";
				i.Name = "Gloves of the Stalwart Arm";
				i.Level = 50;
				i.Model = 691;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Str +22, Dex +15, Cold +6%, Slash +6
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ArmsmanEpicGloves = (HandsArmorTemplate)i;
			}

			//of the Stalwart Arm Hauberk
			ArmsmanEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ArmsmanEpicVest");
			if (ArmsmanEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicVest";
				i.Name = "Jerkin of the Stalwart Arm";
				i.Level = 50;
				i.Model = 688;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: HP +45, Str +18, Energy +4%, Slash +4%
				// there is an additional bonus here I couldn't figure out how to add
				// 3 charges of 75 point shield ???
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 45));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ArmsmanEpicVest = (TorsoArmorTemplate)i;
			}

			//of the Stalwart Arm Legs
			ArmsmanEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ArmsmanEpicLegs");
			if (ArmsmanEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicLegs";
				i.Name = "Legs of the Stalwart Arm";
				i.Level = 50;
				i.Model = 689;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +24, Str +10, Matter +8%, Crush +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ArmsmanEpicLegs = (LegsArmorTemplate)i;
			}

			//of the Stalwart Arm Sleeves
			ArmsmanEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ArmsmanEpicArms");
			if (ArmsmanEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ArmsmanEpicArms";
				i.Name = "Sleeves of the Stalwart Arm";
				i.Level = 50;
				i.Model = 690;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.VeryHigh;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Armsman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +19, Dex +18, Heat +6%, Thrust +6%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ArmsmanEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Friar
			FriarEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "FriarEpicBoots");
			if (FriarEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "FriarEpicBoots";
				i.Name = "Prayer-bound Boots";
				i.Level = 50;
				i.Model = 40;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Qui +18, Dex +15, Spirit +10%, Con +12
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			//Prayer-bound Coif
			FriarEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "FriarEpicHelm");
			if (FriarEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "FriarEpicHelm";
				i.Name = "Prayer-bound Coif";
				i.Level = 50;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Dex +15, Pie +12, Con +10, Enchantment +4
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Enhancement, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicHelm = (HeadArmorTemplate)i;

			}
			//end item
			//Prayer-bound Gloves
			FriarEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "FriarEpicGloves");
			if (FriarEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "FriarEpicGloves";
				i.Name = "Prayer-bound Gloves";
				i.Level = 50;
				i.Model = 39;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Pie +15, Rejuvination +4, Qui +15, Crush +6%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Rejuvenation, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicGloves = (HandsArmorTemplate)i;
			}

			//Prayer-bound Hauberk
			FriarEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "FriarEpicVest");
			if (FriarEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "FriarEpicVest";
				i.Name = "Prayer-bound Jerkin";
				i.Level = 50;
				i.Model = 797;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: HP +33, Pwr +10, Spirit 4%, Crush 6%
				// Charged (3 Max) Self-Only Shield -- 75 AF, Duration 10 mins (no clue how to add this)
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicVest = (TorsoArmorTemplate)i;
			}

			//Prayer-bound Legs
			FriarEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "FriarEpicLegs");
			if (FriarEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "FriarEpicLegs";
				i.Name = "Prayer-bound Legs";
				i.Level = 50;
				i.Model = 37;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +22, Str +15, Heat +6%, Slash +6%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicLegs = (LegsArmorTemplate)i;
			}

			//Prayer-bound Sleeves
			FriarEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "FriarEpicArms");
			if (FriarEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "FriarEpicArms";
				i.Name = "Prayer-bound Sleeves";
				i.Level = 50;
				i.Model = 38;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Friar);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Pie +18, Dex +16, Cold +8%, Thrust +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				FriarEpicArms = (ArmsArmorTemplate)i;

			}
			#endregion
			#region Theurgist
			TheurgistEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "TheurgistEpicBoots");
			if (TheurgistEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicBoots";
				i.Name = "Boots of Shielding Power";
				i.Level = 50;
				i.Model = 143;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Dex +16, Cold +6%, Body +8%, Energy +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				TheurgistEpicBoots = (FeetArmorTemplate)i;
			}

			//of Shielding Power Coif
			TheurgistEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "TheurgistEpicHelm");
			if (TheurgistEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicHelm";
				i.Name = "Coif of Shielding Power";
				i.Level = 50;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Int +21, Dex +13, Spirit +8%, Crush +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				TheurgistEpicHelm = (HeadArmorTemplate)i;
			}

			//of Shielding Power Gloves
			TheurgistEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "TheurgistEpicGloves");
			if (TheurgistEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicGloves";
				i.Name = "Gloves of Shielding Power";
				i.Level = 50;
				i.Model = 142;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Dex +16, Int +18, Heat +8%, Matter +8%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				TheurgistEpicGloves = (HandsArmorTemplate)i;
			}

			//of Shielding Power Hauberk
			TheurgistEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "TheurgistEpicVest");
			if (TheurgistEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicVest";
				i.Name = "Jerkin of Shielding Power";
				i.Level = 50;
				i.Model = 733;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: HP +24, Power +14, Cold +4%,
				//triggered effect: Shield (3 charges max) duration 10 mins  (no clue how to implement)
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				TheurgistEpicVest = (TorsoArmorTemplate)i;
			}

			//of Shielding Power Legs
			TheurgistEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "TheurgistEpicLegs");
			if (TheurgistEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicLegs";
				i.Name = "Legs of Shielding Power";
				i.Level = 50;
				i.Model = 140;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Con +19, Wind +4, Energy +10%, Cold +10%
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Wind, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				TheurgistEpicLegs = (LegsArmorTemplate)i;

			}
			//of Shielding Power Sleeves
			TheurgistEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "TheurgistEpicArms");
			if (TheurgistEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "TheurgistEpicArms";
				i.Name = "Sleeves of Shielding Power";
				i.Level = 50;
				i.Model = 141;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Theurgist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				//bonuses: Int +18, Earth +4, Dex +16
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Earth, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);

				TheurgistEpicArms = (ArmsArmorTemplate)i;

			}
			#endregion

			#endregion

			ScoutTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ScoutTrainer), eRealm.Albion);
			ArmsmanTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ArmsmanTrainer), eRealm.Albion);
			TheurgistTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.TheurgistTrainer), eRealm.Albion);
			FriarTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.FriarTrainer), eRealm.Albion);

			foreach (GameNPC npc in ScoutTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ArmsmanTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in TheurgistTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in FriarTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Uragaig, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Lidmann, typeof(Defenders_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Lidmann == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.RemoveHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.RemoveHandler(Uragaig, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			foreach (GameNPC npc in ScoutTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ArmsmanTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in TheurgistTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in FriarTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Lidmann, typeof(Defenders_50Descriptor));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(BuildingABetterBow), player, Lidmann) <= 0)
				return;

			//We also check if the player is already doing the quest
			Defenders_50 quest = player.IsDoingQuest(typeof(Defenders_50)) as Defenders_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Lidmann.SayTo(player, "Check your Journal for instructions!");
					return;
				}
				else
				{
					// Check if player is qualifed for quest                
					Lidmann.SayTo(player, "Albion needs your [services]");
					return;
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
							player.Out.SendCustomDialog("Will you help Lidmann [Defenders of Albion Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
				}*/

			}

		}

		/* This is our callback hook that will be called when the player clicks
				 * on any button in the quest offer dialog. We check if he accepts or
				 * declines here...
				 */
		/*
		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Defenders_50 quest = player.IsDoingQuest(typeof (Defenders_50)) as Defenders_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Defenders_50), player, Lidmann) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Defenders_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(Defenders_50), player, Lidmann))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Defenders of Albion Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailleach Uragaig in Lyonesse Loc 29k,33k kill her!";
					case 2:
						return "[Step #2] Return to Lidmann Halsey and hand him the Sealed Pouch for your reward!";
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
			Defenders_50Descriptor a = new Defenders_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Defenders_50)) != null) return;

			npc.SayTo(player, "Lidmann Halsey has an important task for you, please seek him out at the tower guarding Adribards Retreat");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Defenders_50 quest = (Defenders_50)player.IsDoingQuest(typeof(Defenders_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(sealed_pouch, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Defenders_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				{
					if (gArgs.Target.Name == Uragaig.Name)
					{
						m_questPlayer.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						GiveItemToPlayer(CreateQuestItem(sealed_pouch, Name));
						Step = 2;
						return;
					}
				}
			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Lidmann.Name && gArgs.Item.Name == sealed_pouch.Name)
				{
					RemoveItemFromPlayer(Lidmann, sealed_pouch);
					Lidmann.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}
		/*
		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
		}*/

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Armsman)
			{
				GiveItemToPlayer(ArmsmanEpicBoots.CreateInstance());
				GiveItemToPlayer(ArmsmanEpicArms.CreateInstance());
				GiveItemToPlayer(ArmsmanEpicGloves.CreateInstance());
				GiveItemToPlayer(ArmsmanEpicHelm.CreateInstance());
				GiveItemToPlayer(ArmsmanEpicLegs.CreateInstance());
				GiveItemToPlayer(ArmsmanEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Scout)
			{
				GiveItemToPlayer(ScoutEpicArms.CreateInstance());
				GiveItemToPlayer(ScoutEpicBoots.CreateInstance());
				GiveItemToPlayer(ScoutEpicGloves.CreateInstance());
				GiveItemToPlayer(ScoutEpicHelm.CreateInstance());
				GiveItemToPlayer(ScoutEpicLegs.CreateInstance());
				GiveItemToPlayer(ScoutEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Theurgist)
			{
				GiveItemToPlayer(TheurgistEpicArms.CreateInstance());
				GiveItemToPlayer(TheurgistEpicBoots.CreateInstance());
				GiveItemToPlayer(TheurgistEpicGloves.CreateInstance());
				GiveItemToPlayer(TheurgistEpicHelm.CreateInstance());
				GiveItemToPlayer(TheurgistEpicLegs.CreateInstance());
				GiveItemToPlayer(TheurgistEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Friar)
			{
				GiveItemToPlayer(FriarEpicArms.CreateInstance());
				GiveItemToPlayer(FriarEpicBoots.CreateInstance());
				GiveItemToPlayer(FriarEpicGloves.CreateInstance());
				GiveItemToPlayer(FriarEpicHelm.CreateInstance());
				GiveItemToPlayer(FriarEpicLegs.CreateInstance());
				GiveItemToPlayer(FriarEpicVest.CreateInstance());
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Lidmann
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Lidmann 
        *#28 give her the ball of flame
        *#29 talk with Lidmann about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Brigandine of Vigilant Defense  Boots 
            *Brigandine of Vigilant Defense  Coif
            *Brigandine of Vigilant Defense  Gloves
            *Brigandine of Vigilant Defense  Hauberk
            *Brigandine of Vigilant Defense  Legs
            *Brigandine of Vigilant Defense  Sleeves
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
