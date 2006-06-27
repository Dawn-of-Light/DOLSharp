/*
*Author         : Etaew - Fallen Realms
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BHorn%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Horn Twin (level 50)
*Quest Classes  : Mentalist, Druid, Blademaster, Nighthsade, Animist, Valewaker(Path of Essence)
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
	public class Harmony_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Harmony_50); }
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
			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Blademaster &&
				player.CharacterClass.ID != (byte)eCharacterClass.Druid &&
				player.CharacterClass.ID != (byte)eCharacterClass.Valewalker &&
				player.CharacterClass.ID != (byte)eCharacterClass.Animist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mentalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Vampiir &&
				player.CharacterClass.ID != (byte)eCharacterClass.Bainshee)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Harmony_50), ExtendsType = typeof(AbstractQuest))]
	public class Harmony_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Horn Twin";

		private static GameMob Revelin = null; // Start NPC
		//private static GameNPC Lauralaye = null; //Reward NPC
		private static GameMob Cailean = null; // Mob to kill

		private static GenericItemTemplate Horn = null; //ball of flame

		private static FeetArmorTemplate BlademasterEpicBoots = null; //Mist Shrouded Boots 
		private static HeadArmorTemplate BlademasterEpicHelm = null; //Mist Shrouded Coif 
		private static HandsArmorTemplate BlademasterEpicGloves = null; //Mist Shrouded Gloves 
		private static TorsoArmorTemplate BlademasterEpicVest = null; //Mist Shrouded Hauberk 
		private static LegsArmorTemplate BlademasterEpicLegs = null; //Mist Shrouded Legs 
		private static ArmsArmorTemplate BlademasterEpicArms = null; //Mist Shrouded Sleeves 

		private static FeetArmorTemplate DruidEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate DruidEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate DruidEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate DruidEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate DruidEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate DruidEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate MentalistEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate MentalistEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate MentalistEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate MentalistEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate MentalistEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate MentalistEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate AnimistEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate AnimistEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate AnimistEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate AnimistEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate AnimistEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate AnimistEpicArms = null; //Subterranean Sleeves 

		private static FeetArmorTemplate ValewalkerEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate ValewalkerEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate ValewalkerEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate ValewalkerEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate ValewalkerEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate ValewalkerEpicArms = null; //Subterranean Sleeves

		private static FeetArmorTemplate VampiirEpicBoots = null;
		private static HeadArmorTemplate VampiirEpicHelm = null;
		private static HandsArmorTemplate VampiirEpicGloves = null;
		private static TorsoArmorTemplate VampiirEpicVest = null;
		private static LegsArmorTemplate VampiirEpicLegs = null;
		private static ArmsArmorTemplate VampiirEpicArms = null;

		private static FeetArmorTemplate BainsheeEpicBoots = null;
		private static HeadArmorTemplate BainsheeEpicHelm = null;
		private static HandsArmorTemplate BainsheeEpicGloves = null;
		private static TorsoArmorTemplate BainsheeEpicVest = null;
		private static LegsArmorTemplate BainsheeEpicLegs = null;
		private static ArmsArmorTemplate BainsheeEpicArms = null;

		private static GameNPC[] BlademasterTrainers = null;
		private static GameNPC[] DruidTrainers = null;
		private static GameNPC[] MentalistTrainers = null;
		private static GameNPC[] AnimistTrainers = null;
		private static GameNPC[] ValewalkerTrainers = null;
		private static GameNPC[] VampiirTrainers = null;
		private static GameNPC[] BainsheeTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Revelin", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Revelin , creating it ...");
				Revelin = new GameMob();
				Revelin.Model = 361;
				Revelin.Name = "Revelin";
				Revelin.GuildName = "";
				Revelin.Realm = (byte)eRealm.Hibernia;
				Revelin.RegionId = 200;
				Revelin.Size = 42;
				Revelin.Level = 20;
				Revelin.Position = new Point(344387, 706197, 6351);
				Revelin.Heading = 2127;
				Revelin.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Revelin.SaveIntoDatabase();
				}

			}
			else
				Revelin = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailean", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Cailean , creating it ...");
				Cailean = new GameMob();
				Cailean.Model = 98;
				Cailean.Name = "Cailean";
				Cailean.GuildName = "";
				Cailean.Realm = (byte)eRealm.None;
				Cailean.RegionId = 200;
				Cailean.Size = 60;
				Cailean.Level = 65;
				Cailean.Position = new Point(479042, 508134, 4569);
				Cailean.Heading = 3319;
				Cailean.RespawnInterval = 5 * 60 * 1000;
				Cailean.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Cailean.SaveIntoDatabase();
				}

			}
			else
				Cailean = npcs[0] as GameMob;
			// end npc

			#endregion

			#region Item Declarations

			Horn = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "Horn");
			if (Horn == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Horn , creating it ...");
				Horn = new GenericItemTemplate();
				Horn.ItemTemplateID = "Horn";
				Horn.Name = "Horn";
				Horn.Level = 8;
				Horn.Model = 586;
				Horn.IsDropable = false;
				Horn.IsSaleable = false;
				Horn.IsTradable = false;
				Horn.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(Horn);
				}
			}

			ArmorTemplate i = null;
			#region Druid
			DruidEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "DruidEpicBoots");
			if (DruidEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "DruidEpicBoots";
				i.Name = "Sidhe Scale Boots";
				i.Level = 50;
				i.Model = 743;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 36));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				DruidEpicBoots = (FeetArmorTemplate)i;
			}

			//Sidhe Scale Coif
			DruidEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "DruidEpicHelm");
			if (DruidEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "DruidEpicHelm";
				i.Name = "Sidhe Scale Coif";
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
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nurture, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nature, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				DruidEpicHelm = (HeadArmorTemplate)i;
			}

			//Sidhe Scale Gloves
			DruidEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "DruidEpicGloves");
			if (DruidEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "DruidEpicGloves";
				i.Name = "Sidhe Scale Gloves ";
				i.Level = 50;
				i.Model = 742;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Regrowth, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				DruidEpicGloves = (HandsArmorTemplate)i;
			}

			//Sidhe Scale Hauberk
			DruidEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "DruidEpicVest");
			if (DruidEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "DruidEpicVest";
				i.Name = "Sidhe Scale Breastplate";
				i.Level = 50;
				i.Model = 739;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nature, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				DruidEpicVest = (TorsoArmorTemplate)i;
			}

			//Sidhe Scale Legs
			DruidEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "DruidEpicLegs");
			if (DruidEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "DruidEpicLegs";
				i.Name = "Sidhe Scale Leggings";
				i.Level = 50;
				i.Model = 740;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 57));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				DruidEpicLegs = (LegsArmorTemplate)i;
			}

			//Sidhe Scale Sleeves
			DruidEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "DruidEpicArms");
			if (DruidEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "DruidEpicArms";
				i.Name = "Sidhe Scale Sleeves";
				i.Level = 50;
				i.Model = 741;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Druid);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				DruidEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Blademaster
			//Blademaster Epic Sleeves End
			BlademasterEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "BlademasterEpicBoots");
			if (BlademasterEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicBoots";
				i.Name = "Sidhe Studded Boots";
				i.Level = 50;
				i.Model = 786;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicBoots = (FeetArmorTemplate)i;
			}

			//Sidhe Studded Coif
			BlademasterEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "BlademasterEpicHelm");
			if (BlademasterEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicHelm";
				i.Name = "Sidhe Studded Helm";
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
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicHelm = (HeadArmorTemplate)i;
			}

			//Sidhe Studded Gloves
			BlademasterEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "BlademasterEpicGloves");
			if (BlademasterEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicGloves";
				i.Name = "Sidhe Studded Gloves ";
				i.Level = 50;
				i.Model = 785;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Celtic_Dual, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicGloves = (HandsArmorTemplate)i;
			}

			//Sidhe Studded Hauberk
			BlademasterEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "BlademasterEpicVest");
			if (BlademasterEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicVest";
				i.Name = "Sidhe Studded Hauberk";
				i.Level = 50;
				i.Model = 782;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicVest = (TorsoArmorTemplate)i;
			}

			//Sidhe Studded Legs
			BlademasterEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "BlademasterEpicLegs");
			if (BlademasterEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicLegs";
				i.Name = "Sidhe Studded Leggings";
				i.Level = 50;
				i.Model = 783;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicLegs = (LegsArmorTemplate)i;
			}

			//Sidhe Studded Sleeves
			BlademasterEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "BlademasterEpicArms");
			if (BlademasterEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "BlademasterEpicArms";
				i.Name = "Sidhe Studded Sleeves";
				i.Level = 50;
				i.Model = 784;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Blademaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BlademasterEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Animist
			AnimistEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "AnimistEpicBoots");
			if (AnimistEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "AnimistEpicBoots";
				i.Name = "Brightly Woven Boots";
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
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicBoots = (FeetArmorTemplate)i;
			}

			//Brightly Woven Coif
			AnimistEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "AnimistEpicHelm");
			if (AnimistEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "AnimistEpicHelm";
				i.Name = "Brightly Woven Cap";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Arboreal, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicHelm = (HeadArmorTemplate)i;
			}

			//Brightly Woven Gloves
			AnimistEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "AnimistEpicGloves");
			if (AnimistEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "AnimistEpicGloves";
				i.Name = "Brightly Woven Gloves ";
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
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Creeping, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicGloves = (HandsArmorTemplate)i;
			}

			//Brightly Woven Hauberk
			AnimistEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "AnimistEpicVest");
			if (AnimistEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "AnimistEpicVest";
				i.Name = "Brightly Woven Robe";
				i.Level = 50;
				i.Model = 1186;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicVest = (TorsoArmorTemplate)i;
			}

			//Brightly Woven Legs
			AnimistEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "AnimistEpicLegs");
			if (AnimistEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "AnimistEpicLegs";
				i.Name = "Brightly Woven Pants";
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
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicLegs = (LegsArmorTemplate)i;
			}

			//Brightly Woven Sleeves
			AnimistEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "AnimistEpicArms");
			if (AnimistEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "AnimistEpicArms";
				i.Name = "Brightly Woven Sleeves";
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
				i.AllowedClass.Add(eCharacterClass.Animist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				AnimistEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Mentalist
			MentalistEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "MentalistEpicBoots");
			if (MentalistEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "MentalistEpicBoots";
				i.Name = "Sidhe Woven Boots";
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
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicBoots = (FeetArmorTemplate)i;
			}

			//Sidhe Woven Coif
			MentalistEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "MentalistEpicHelm");
			if (MentalistEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "MentalistEpicHelm";
				i.Name = "Sidhe Woven Cap";
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
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mentalism, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicHelm = (HeadArmorTemplate)i;
			}

			//Sidhe Woven Gloves
			MentalistEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "MentalistEpicGloves");
			if (MentalistEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "MentalistEpicGloves";
				i.Name = "Sidhe Woven Gloves ";
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
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Light, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicGloves = (HandsArmorTemplate)i;
			}

			//Sidhe Woven Hauberk
			MentalistEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "MentalistEpicVest");
			if (MentalistEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "MentalistEpicVest";
				i.Name = "Sidhe Woven Vest";
				i.Level = 50;
				i.Model = 745;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicVest = (TorsoArmorTemplate)i;
			}

			//Sidhe Woven Legs
			MentalistEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "MentalistEpicLegs");
			if (MentalistEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "MentalistEpicLegs";
				i.Name = "Sidhe Woven Pants";
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
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicLegs = (LegsArmorTemplate)i;
			}

			//Sidhe Woven Sleeves
			MentalistEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "MentalistEpicArms");
			if (MentalistEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "MentalistEpicArms";
				i.Name = "Sidhe Woven Sleeves";
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
				i.AllowedClass.Add(eCharacterClass.Mentalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mana, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				MentalistEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Valewalker
			ValewalkerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ValewalkerEpicBoots");
			if (ValewalkerEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicBoots";
				i.Name = "Boots of the Misty Glade";
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
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicBoots = (FeetArmorTemplate)i;
			}

			//Misty Glade Coif
			ValewalkerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ValewalkerEpicHelm");
			if (ValewalkerEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicHelm";
				i.Name = "Cap of the Misty Glade";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Arboreal, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicHelm = (HeadArmorTemplate)i;
			}

			//Misty Glade Gloves
			ValewalkerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ValewalkerEpicGloves");
			if (ValewalkerEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicGloves";
				i.Name = "Gloves of the Misty Glades";
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
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicGloves = (HandsArmorTemplate)i;
			}

			//Misty Glade Hauberk
			ValewalkerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ValewalkerEpicVest");
			if (ValewalkerEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicVest";
				i.Name = "Robe of the Misty Glade";
				i.Level = 50;
				i.Model = 1003;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Arboreal, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicVest = (TorsoArmorTemplate)i;
			}

			//Misty Glade Legs
			ValewalkerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ValewalkerEpicLegs");
			if (ValewalkerEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicLegs";
				i.Name = "Pants of the Misty Glade";
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
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 18));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicLegs = (LegsArmorTemplate)i;
			}

			//Misty Glade Sleeves
			ValewalkerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ValewalkerEpicArms");
			if (ValewalkerEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ValewalkerEpicArms";
				i.Name = "Sleeves of the Misty Glade";
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
				i.AllowedClass.Add(eCharacterClass.Valewalker);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Scythe, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ValewalkerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Vampiir
			VampiirEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "VampiirEpicBoots");
			if (VampiirEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "VampiirEpicBoots";
				i.Name = "Archfiend Etched Boots";
				i.Level = 50;
				i.Model = 2927;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Strength: 12 pts
				 *   Dexterity: 15 pts
				 *   Thrust Resist: 10%
				 *   Hits: 24 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicBoots = (FeetArmorTemplate)i;
			}

			//Misty Glade Coif
			VampiirEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "VampiirEpicHelm");
			if (VampiirEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "VampiirEpicHelm";
				i.Name = "Archfiend Etched Helm";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Strength: 6 pts
				 *   Constitution: 16 pts
				 *   Dexterity: 6 pts
				 *   Hits: 30 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicHelm = (HeadArmorTemplate)i;
			}

			//Misty Glade Gloves
			VampiirEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "VampiirEpicGloves");
			if (VampiirEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "VampiirEpicGloves";
				i.Name = "Archfiend Etched Gloves";
				i.Level = 50;
				i.Model = 2926;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Dexterity: 12 pts
				 *   Quickness: 13 pts
				 *   Dementia: +2 pts
				 *   Shadow Mastery: +5 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Dementia, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_ShadowMastery, 5));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicGloves = (HandsArmorTemplate)i;
			}

			//Misty Glade Hauberk
			VampiirEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "VampiirEpicVest");
			if (VampiirEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "VampiirEpicVest";
				i.Name = "Archfiend Etched Vest";
				i.Level = 50;
				i.Model = 2923;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Strength: 10 pts
				 *   Dexterity: 10 pts
				 *   Quickness: 10 pts
				 *   Hits: 30 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicVest = (TorsoArmorTemplate)i;
			}

			//Misty Glade Legs
			VampiirEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "VampiirEpicLegs");
			if (VampiirEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "VampiirEpicLegs";
				i.Name = "Archfiend Etched Leggings";
				i.Level = 50;
				i.Model = 2924;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Constitution: 16 pts
				 *   Dexterity: 15 pts
				 *   Crush Resist: 10%
				 *   Slash Resist: 10%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicLegs = (LegsArmorTemplate)i;
			}

			//Misty Glade Sleeves
			VampiirEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "VampiirEpicArms");
			if (VampiirEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "VampiirEpicArms";
				i.Name = "Archfiend Etched Sleeves";
				i.Level = 50;
				i.Model = 2925;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Vampiir);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				/*
				 *   Strength: 15 pts
				 *   Dexterity: 15 pts
				 *   Cold Resist: 6%
				 *   Vampiiric Embrace: +4 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_VampiiricEmbrace, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				VampiirEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Bainshee
			BainsheeEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "BainsheeEpicBoots");
			if (BainsheeEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicBoots";
				i.Name = "Boots of the Keening Spirit";
				i.Level = 50;
				i.Model = 2952;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicBoots = (FeetArmorTemplate)i;
			}

			//Keening Spirit Coif
			BainsheeEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "BainsheeEpicHelm");
			if (BainsheeEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicHelm";
				i.Name = "Cap of the Keening Spirit";
				i.Level = 50;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Arboreal, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicHelm = (HeadArmorTemplate)i;
			}

			//Keening Spirit Gloves
			BainsheeEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "BainsheeEpicGloves");
			if (BainsheeEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicGloves";
				i.Name = "Gloves of the Keening Spirits";
				i.Level = 50;
				i.Model = 2950;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicGloves = (HandsArmorTemplate)i;
			}

			//Keening Spirit Hauberk
			BainsheeEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "BainsheeEpicVest");
			if (BainsheeEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicVest";
				i.Name = "Robe of the Keening Spirit";
				i.Level = 50;
				i.Model = 2922;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Arboreal, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicVest = (TorsoArmorTemplate)i;
			}

			//Keening Spirit Legs
			BainsheeEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "BainsheeEpicLegs");
			if (BainsheeEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicLegs";
				i.Name = "Pants of the Keening Spirit";
				i.Level = 50;
				i.Model = 2949;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 18));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicLegs = (LegsArmorTemplate)i;
			}

			//Keening Spirit Sleeves
			BainsheeEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "BainsheeEpicArms");
			if (BainsheeEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "BainsheeEpicArms";
				i.Name = "Sleeves of the Keening Spirit";
				i.Level = 50;
				i.Model = 2948;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bainshee);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Scythe, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BainsheeEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			BlademasterTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.BlademasterTrainer), eRealm.Hibernia);
			DruidTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.DruidTrainer), eRealm.Hibernia);
			MentalistTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.MentalistTrainer), eRealm.Hibernia);
			AnimistTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.AnimistTrainer), eRealm.Hibernia);
			ValewalkerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ValewalkerTrainer), eRealm.Hibernia);
			VampiirTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.VampiirTrainer), eRealm.Hibernia);
			BainsheeTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.BainsheeTrainer), eRealm.Hibernia);

			foreach (GameNPC npc in BlademasterTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in DruidTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in MentalistTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in AnimistTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ValewalkerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in VampiirTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BainsheeTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.AddHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.AddHandler(Cailean, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Revelin the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Revelin, typeof(Harmony_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Revelin == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.RemoveHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));

			foreach (GameNPC npc in BlademasterTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in DruidTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in MentalistTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in AnimistTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ValewalkerTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in VampiirTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BainsheeTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Revelin the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Revelin, typeof(Harmony_50Descriptor));
		}

		protected static void TalkToRevelin(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Harmony_50), player, Revelin) <= 0)
				return;

			//We also check if the player is already doing the quest
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Revelin.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Revelin.SayTo(player, "Hibernia needs your [services]");
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
						case "services":
							player.Out.SendCustomDialog("Will you help Revelin [Path of Harmony Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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

		/* This is our callback hook that will be called when the player clicks
				 * on any button in the quest offer dialog. We check if he accepts or
				 * declines here...
				 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Harmony_50), player, Revelin) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(Harmony_50), player, Revelin))
					return;
				player.Out.SendMessage("Kill Cailean in Cursed Forest loc 28k 24k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Horn Twin (Level 50 Path of Harmony Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailean in Cursed Forest Loc 28k,24k kill him!";
					case 2:
						return "[Step #2] Return to Revelin and give the Horn!";
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
			Harmony_50Descriptor a = new Harmony_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Focus_50)) != null) return;

			npc.SayTo(player, "Revelin has an important task for you, please seek him out at the Parth Farm near Innis Carthaig.");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Harmony_50 quest = (Harmony_50)player.IsDoingQuest(typeof(Harmony_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("You collect the Horn from Cailean", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				quest.GiveItemToPlayer(CreateQuestItem(Horn, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Harmony_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == Cailean.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Horn from Cailean", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(Horn, Name));
					Step = 2;
					return;
				}

			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Revelin.Name && gArgs.Item.Name == Horn.Name)
				{
					RemoveItemFromPlayer(Revelin, Horn);
					Revelin.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
			RemoveItemFromPlayer(Horn);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Blademaster)
			{
				GiveItemToPlayer(BlademasterEpicArms);
				GiveItemToPlayer(BlademasterEpicBoots);
				GiveItemToPlayer(BlademasterEpicGloves);
				GiveItemToPlayer(BlademasterEpicHelm);
				GiveItemToPlayer(BlademasterEpicLegs);
				GiveItemToPlayer(BlademasterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Animist)
			{
				GiveItemToPlayer(AnimistEpicArms);
				GiveItemToPlayer(AnimistEpicBoots);
				GiveItemToPlayer(AnimistEpicGloves);
				GiveItemToPlayer(AnimistEpicHelm);
				GiveItemToPlayer(AnimistEpicLegs);
				GiveItemToPlayer(AnimistEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mentalist)
			{
				GiveItemToPlayer(MentalistEpicArms);
				GiveItemToPlayer(MentalistEpicBoots);
				GiveItemToPlayer(MentalistEpicGloves);
				GiveItemToPlayer(MentalistEpicHelm);
				GiveItemToPlayer(MentalistEpicLegs);
				GiveItemToPlayer(MentalistEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Druid)
			{
				GiveItemToPlayer(DruidEpicArms);
				GiveItemToPlayer(DruidEpicBoots);
				GiveItemToPlayer(DruidEpicGloves);
				GiveItemToPlayer(DruidEpicHelm);
				GiveItemToPlayer(DruidEpicLegs);
				GiveItemToPlayer(DruidEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Valewalker)
			{
				GiveItemToPlayer(ValewalkerEpicArms);
				GiveItemToPlayer(ValewalkerEpicBoots);
				GiveItemToPlayer(ValewalkerEpicGloves);
				GiveItemToPlayer(ValewalkerEpicHelm);
				GiveItemToPlayer(ValewalkerEpicLegs);
				GiveItemToPlayer(ValewalkerEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Vampiir)
			{
				GiveItemToPlayer(VampiirEpicArms);
				GiveItemToPlayer(VampiirEpicBoots);
				GiveItemToPlayer(VampiirEpicGloves);
				GiveItemToPlayer(VampiirEpicHelm);
				GiveItemToPlayer(VampiirEpicLegs);
				GiveItemToPlayer(VampiirEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bainshee)
			{
				GiveItemToPlayer(BainsheeEpicArms);
				GiveItemToPlayer(BainsheeEpicBoots);
				GiveItemToPlayer(BainsheeEpicGloves);
				GiveItemToPlayer(BainsheeEpicHelm);
				GiveItemToPlayer(BainsheeEpicLegs);
				GiveItemToPlayer(BainsheeEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Revelin
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Revelin 
        *#28 give her the ball of flame
        *#29 talk with Revelin about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Sidhe Scale Boots 
            *Sidhe Scale Coif
            *Sidhe Scale Gloves
            *Sidhe Scale Hauberk
            *Sidhe Scale Legs
            *Sidhe Scale Sleeves
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
