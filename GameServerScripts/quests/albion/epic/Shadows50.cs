/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Cabalist, Reaver, Mercenary, Necromancer and Infiltrator (Guild of Shadows)
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
	public class Shadows_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Shadows_50); }
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
			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Reaver &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mercenary &&
				player.CharacterClass.ID != (byte)eCharacterClass.Cabalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Necromancer &&
				player.CharacterClass.ID != (byte)eCharacterClass.Infiltrator &&
				player.CharacterClass.ID != (byte)eCharacterClass.Heretic)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Shadows_50), ExtendsType = typeof(AbstractQuest))]
	public class Shadows_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Feast of the Decadent";

		private static GameNPC Lidmann = null; // Start NPC
		private static GameMob Uragaig = null; // Mob to kill

		private static GenericItemTemplate sealed_pouch = null; //sealed pouch

		private static FeetArmorTemplate MercenaryEpicBoots = null; // of the Shadowy Embers  Boots 
		private static HeadArmorTemplate MercenaryEpicHelm = null; // of the Shadowy Embers  Coif 
		private static HandsArmorTemplate MercenaryEpicGloves = null; // of the Shadowy Embers  Gloves 
		private static TorsoArmorTemplate MercenaryEpicVest = null; // of the Shadowy Embers  Hauberk 
		private static LegsArmorTemplate MercenaryEpicLegs = null; // of the Shadowy Embers  Legs 
		private static ArmsArmorTemplate MercenaryEpicArms = null; // of the Shadowy Embers  Sleeves 

		private static FeetArmorTemplate ReaverEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate ReaverEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate ReaverEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate ReaverEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate ReaverEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate ReaverEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate CabalistEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate CabalistEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate CabalistEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate CabalistEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate CabalistEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate CabalistEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate InfiltratorEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate InfiltratorEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate InfiltratorEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate InfiltratorEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate InfiltratorEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate InfiltratorEpicArms = null; //Subterranean Sleeves		

		private static FeetArmorTemplate NecromancerEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate NecromancerEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate NecromancerEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate NecromancerEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate NecromancerEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate NecromancerEpicArms = null; //Subterranean Sleeves

		private static FeetArmorTemplate HereticEpicBoots = null;
		private static HeadArmorTemplate HereticEpicHelm = null; 
		private static HandsArmorTemplate HereticEpicGloves = null;
		private static TorsoArmorTemplate HereticEpicVest = null;
		private static LegsArmorTemplate HereticEpicLegs = null;
		private static ArmsArmorTemplate HereticEpicArms = null;

		private static GameNPC[] MercenaryTrainers = null;
		private static GameNPC[] ReaverTrainers = null;
		private static GameNPC[] CabalistTrainers = null;
		private static GameNPC[] InfiltratorTrainers = null;
		private static GameNPC[] NecromancerTrainers = null;
		private static GameNPC[] HereticTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lidmann Halsey, creating it ...");
				Lidmann = new GameMob();
				Lidmann.Model = 64;
				Lidmann.Name = "Lidmann Halsey";
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

			#region Item Declarations

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
			#region Mercenary
			MercenaryEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "MercenaryEpicBoots");
			if (MercenaryEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicBoots";
				i.Name = "Boots of the Shadowy Embers";
				i.Level = 50;
				i.Model = 722;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 9));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			// of the Shadowy Embers  Coif
			MercenaryEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "MercenaryEpicHelm");
			if (MercenaryEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicHelm";
				i.Name = "Coif of the Shadowy Embers";
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
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicHelm = (HeadArmorTemplate)i;

			}
			//end item
			// of the Shadowy Embers  Gloves
			MercenaryEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "MercenaryEpicGloves");
			if (MercenaryEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicGloves";
				i.Name = "Gauntlets of the Shadowy Embers";
				i.Level = 50;
				i.Model = 721;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicGloves = (HandsArmorTemplate)i;

			}
			// of the Shadowy Embers  Hauberk
			MercenaryEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "MercenaryEpicVest");
			if (MercenaryEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicVest";
				i.Name = "Haurberk of the Shadowy Embers";
				i.Level = 50;
				i.Model = 718;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicVest = (TorsoArmorTemplate)i;

			}
			// of the Shadowy Embers  Legs
			MercenaryEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "MercenaryEpicLegs");
			if (MercenaryEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicLegs";
				i.Name = "Chausses of the Shadowy Embers";
				i.Level = 50;
				i.Model = 719;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicLegs = (LegsArmorTemplate)i;

			}
			// of the Shadowy Embers  Sleeves
			MercenaryEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "MercenaryEpicArms");
			if (MercenaryEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "MercenaryEpicArms";
				i.Name = "Sleeves of the Shadowy Embers";
				i.Level = 50;
				i.Model = 720;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Mercenary);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicArms = (ArmsArmorTemplate)i;

			}
			#endregion
			#region Reaver
			//Reaver Epic Sleeves End
			ReaverEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ReaverEpicBoots");
			if (ReaverEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ReaverEpicBoots";
				i.Name = "Boots of Murky Secrets";
				i.Level = 50;
				i.Model = 1270;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 9));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			//of Murky Secrets Coif
			ReaverEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ReaverEpicHelm");
			if (ReaverEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ReaverEpicHelm";
				i.Name = "Coif of Murky Secrets";
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
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Flexible_Weapon, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicHelm = (HeadArmorTemplate)i;

			}
			//end item
			//of Murky Secrets Gloves
			ReaverEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ReaverEpicGloves");
			if (ReaverEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ReaverEpicGloves";
				i.Name = "Gauntlets of Murky Secrets";
				i.Level = 50;
				i.Model = 1271;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicGloves = (HandsArmorTemplate)i;

			}
			//of Murky Secrets Hauberk
			ReaverEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ReaverEpicVest");
			if (ReaverEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ReaverEpicVest";
				i.Name = "Hauberk of Murky Secrets";
				i.Level = 50;
				i.Model = 1267;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicVest = (TorsoArmorTemplate)i;

			}
			//of Murky Secrets Legs
			ReaverEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ReaverEpicLegs");
			if (ReaverEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ReaverEpicLegs";
				i.Name = "Chausses of Murky Secrets";
				i.Level = 50;
				i.Model = 1268;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicLegs = (LegsArmorTemplate)i;

			}
			//of Murky Secrets Sleeves
			ReaverEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ReaverEpicArms");
			if (ReaverEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ReaverEpicArms";
				i.Name = "Sleeves of Murky Secrets";
				i.Level = 50;
				i.Model = 1269;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Reaver);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Slashing, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Infiltrator
			InfiltratorEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "InfiltratorEpicBoots");
			if (InfiltratorEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicBoots";
				i.Name = "Shadow-Woven Boots";
				i.Level = 50;
				i.Model = 796;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				InfiltratorEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			//Shadow-Woven Coif
			InfiltratorEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "InfiltratorEpicHelm");
			if (InfiltratorEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicHelm";
				i.Name = "Shadow-Woven Coif";
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
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				InfiltratorEpicHelm = (HeadArmorTemplate)i;

			}
			//end item
			//Shadow-Woven Gloves
			InfiltratorEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "InfiltratorEpicGloves");
			if (InfiltratorEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicGloves";
				i.Name = "Shadow-Woven Gloves";
				i.Level = 50;
				i.Model = 795;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Envenom, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Critical_Strike, 3));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				InfiltratorEpicGloves = (HandsArmorTemplate)i;

			}
			//Shadow-Woven Hauberk
			InfiltratorEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "InfiltratorEpicVest");
			if (InfiltratorEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicVest";
				i.Name = "Shadow-Woven Jerkin";
				i.Level = 50;
				i.Model = 792;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 36));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				InfiltratorEpicVest = (TorsoArmorTemplate)i;

			}
			//Shadow-Woven Legs
			InfiltratorEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "InfiltratorEpicLegs");
			if (InfiltratorEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicLegs";
				i.Name = "Shadow-Woven Leggings";
				i.Level = 50;
				i.Model = 793;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				InfiltratorEpicLegs = (LegsArmorTemplate)i;

			}
			//Shadow-Woven Sleeves
			InfiltratorEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "InfiltratorEpicArms");
			if (InfiltratorEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "InfiltratorEpicArms";
				i.Name = "Shadow-Woven Sleeves";
				i.Level = 50;
				i.Model = 794;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Infiltrator);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 41));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				InfiltratorEpicArms = (ArmsArmorTemplate)i;

			}
			#endregion
			#region Cabalist
			CabalistEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "CabalistEpicBoots");
			if (CabalistEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "CabalistEpicBoots";
				i.Name = "Warm Boots of the Construct";
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
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Matter, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicBoots = (FeetArmorTemplate)i;
			}
			//end item
			//Warm of the Construct Coif
			CabalistEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "CabalistEpicHelm");
			if (CabalistEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "CabalistEpicHelm";
				i.Name = "Warm Coif of the Construct";
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
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				CabalistEpicHelm = (HeadArmorTemplate)i;

			}
			//end item
			//Warm of the Construct Gloves
			CabalistEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "CabalistEpicGloves");
			if (CabalistEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "CabalistEpicGloves";
				i.Name = "Warm Gloves of the Construct";
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
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicGloves = (HandsArmorTemplate)i;
			}

			//Warm of the Construct Hauberk
			CabalistEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "CabalistEpicVest");
			if (CabalistEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "CabalistEpicVest";
				i.Name = "Warm Robe of the Construct";
				i.Level = 50;
				i.Model = 682;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicVest = (TorsoArmorTemplate)i;

			}
			//Warm of the Construct Legs
			CabalistEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "CabalistEpicLegs");
			if (CabalistEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "CabalistEpicLegs";
				i.Name = "Warm Leggings of the Construct";
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
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Spirit, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicLegs = (LegsArmorTemplate)i;

			}
			//Warm of the Construct Sleeves
			CabalistEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "CabalistEpicArms");
			if (CabalistEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "CabalistEpicArms";
				i.Name = "Warm Sleeves of the Construct";
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
				i.AllowedClass.Add(eCharacterClass.Cabalist);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Body, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				CabalistEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Necromancer
			NecromancerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "NecromancerEpicBoots");
			if (NecromancerEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicBoots";
				i.Name = "Boots of Forbidden Rites";
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
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Pain_working, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				NecromancerEpicBoots = (FeetArmorTemplate)i;
			}

			//of Forbidden Rites Coif
			NecromancerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "NecromancerEpicHelm");
			if (NecromancerEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicHelm";
				i.Name = "Cap of Forbidden Rites";
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
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicHelm = (HeadArmorTemplate)i;
			}

			//of Forbidden Rites Gloves
			NecromancerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "NecromancerEpicGloves");
			if (NecromancerEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicGloves";
				i.Name = "Gloves of Forbidden Rites";
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
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				NecromancerEpicGloves = (HandsArmorTemplate)i;
			}

			//of Forbidden Rites Hauberk
			NecromancerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "NecromancerEpicVest");
			if (NecromancerEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicVest";
				i.Name = "Robe of Forbidden Rites";
				i.Level = 50;
				i.Model = 1266;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				NecromancerEpicVest = (TorsoArmorTemplate)i;
			}

			//of Forbidden Rites Legs
			NecromancerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "NecromancerEpicLegs");
			if (NecromancerEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicLegs";
				i.Name = "Leggings of Forbidden Rites";
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
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Death_Servant, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicLegs = (LegsArmorTemplate)i;
			}

			//of Forbidden Rites Sleeves
			NecromancerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "NecromancerEpicArms");
			if (NecromancerEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "NecromancerEpicArms";
				i.Name = "Sleeves of Forbidden Rites";
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
				i.AllowedClass.Add(eCharacterClass.Necromancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_DeathSight, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Heretic
			HereticEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "HereticEpicBoots");
			if (HereticEpicBoots == null)
			{
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "HereticEpicBoots";
				i.Name = "Boots of the Zealous Renegade";
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
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				Strength: 16 pts
				Constitution: 18 pts
				Slash Resist: 8%
				Heat Resist: 8%
				*/

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				HereticEpicBoots = (FeetArmorTemplate)i;
			}

			//of Forbidden Rites Coif
			HereticEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "HereticEpicHelm");
			if (HereticEpicHelm == null)
			{
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "HereticEpicHelm";
				i.Name = "Cap of the Zealous Renegade";
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
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				 *   Thrust Resist: 6%
					Piety: 15 pts
					Hits: 48 pts
					Cold Resist: 4%
				 */
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HereticEpicHelm = (HeadArmorTemplate)i;
			}

			//of the Zealous Renegade Gloves
			HereticEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "HereticEpicGloves");
			if (HereticEpicGloves == null)
			{
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "HereticEpicGloves";
				i.Name = "Gloves of the Zealous Renegade";
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
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				 *   Strength: 9 pts
				Cold Resist: 8%
				Power: 14 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				HereticEpicGloves = (HandsArmorTemplate)i;
			}

			//of the Zealous Renegade Hauberk
			HereticEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "HereticEpicVest");
			if (HereticEpicVest == null)
			{
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "HereticEpicVest";
				i.Name = "Robe of the Zealous Renegade";
				i.Level = 50;
				i.Model = 2921;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				 *   Crush: 4 pts
					Constitution: 16 pts
					Dexterity: 15 pts
					Cold Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Crushing, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				HereticEpicVest = (TorsoArmorTemplate)i;
			}

			//of the Zealous Renegade Legs
			HereticEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "HereticEpicLegs");
			if (HereticEpicLegs == null)
			{
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "HereticEpicLegs";
				i.Name = "Leggings of the Zealous Renegade";
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
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				 *   Constitution: 15 pts
					Strength: 19 pts
					Crush Resist: 8%
					Matter Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HereticEpicLegs = (LegsArmorTemplate)i;
			}

			//of the Zealous Renegade Sleeves
			HereticEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "HereticEpicArms");
			if (HereticEpicArms == null)
			{
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "HereticEpicArms";
				i.Name = "Sleeves of the Zealous Renegade";
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
				i.AllowedClass.Add(eCharacterClass.Heretic);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				/*
				 *   Piety: 16 pts
					Thrust Resist: 8%
					Body Resist: 8%
					Flexible: 6 pts
				 */
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Flexible_Weapon, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HereticEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			MercenaryTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.MercenaryTrainer), eRealm.Albion);
			ReaverTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ReaverTrainer), eRealm.Albion);
			CabalistTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.CabalistTrainer), eRealm.Albion);
			InfiltratorTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.InfiltratorTrainer), eRealm.Albion);
			NecromancerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.NecromancerTrainer), eRealm.Albion);
			HereticTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.HereticTrainer), eRealm.Albion);

			foreach (GameNPC npc in MercenaryTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ReaverTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in CabalistTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in InfiltratorTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in NecromancerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in HereticTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Uragaig, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Lidmann the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Lidmann, typeof(Shadows_50Descriptor));

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

			foreach (GameNPC npc in MercenaryTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ReaverTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in CabalistTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in InfiltratorTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in NecromancerTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in HereticTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Lidmann the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Lidmann, typeof(Shadows_50Descriptor));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Shadows_50), player, Lidmann) <= 0)
				return;

			//We also check if the player is already doing the quest
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

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
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Shadows_50), player, Lidmann) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(Shadows_50), player, Lidmann))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Guild of Shadows Epic)"; }
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
						return "[Step #2] Return to Lidmann Halsey for your reward!";
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
			Shadows_50Descriptor a = new Shadows_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Shadows_50)) != null) return;

			npc.SayTo(player, "Lidmann Halsey has an important task for you, please seek him out at the tower guarding Adribards Retreat");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Shadows_50 quest = (Shadows_50)player.IsDoingQuest(typeof(Shadows_50));
				if (quest == null) continue;
				player.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(sealed_pouch, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Shadows_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Uragaig.Name)
				{
					m_questPlayer.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(sealed_pouch, Name));
					Step = 2;
					return;
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
				}
		*/
		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Reaver)
			{
				GiveItemToPlayer(ReaverEpicArms.CreateInstance());
				GiveItemToPlayer(ReaverEpicBoots.CreateInstance());
				GiveItemToPlayer(ReaverEpicGloves.CreateInstance());
				GiveItemToPlayer(ReaverEpicHelm.CreateInstance());
				GiveItemToPlayer(ReaverEpicLegs.CreateInstance());
				GiveItemToPlayer(ReaverEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mercenary)
			{
				GiveItemToPlayer(MercenaryEpicArms.CreateInstance());
				GiveItemToPlayer(MercenaryEpicBoots.CreateInstance());
				GiveItemToPlayer(MercenaryEpicGloves.CreateInstance());
				GiveItemToPlayer(MercenaryEpicHelm.CreateInstance());
				GiveItemToPlayer(MercenaryEpicLegs.CreateInstance());
				GiveItemToPlayer(MercenaryEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Cabalist)
			{
				GiveItemToPlayer(CabalistEpicArms.CreateInstance());
				GiveItemToPlayer(CabalistEpicBoots.CreateInstance());
				GiveItemToPlayer(CabalistEpicGloves.CreateInstance());
				GiveItemToPlayer(CabalistEpicHelm.CreateInstance());
				GiveItemToPlayer(CabalistEpicLegs.CreateInstance());
				GiveItemToPlayer(CabalistEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Infiltrator)
			{
				GiveItemToPlayer(InfiltratorEpicArms.CreateInstance());
				GiveItemToPlayer(InfiltratorEpicBoots.CreateInstance());
				GiveItemToPlayer(InfiltratorEpicGloves.CreateInstance());
				GiveItemToPlayer(InfiltratorEpicHelm.CreateInstance());
				GiveItemToPlayer(InfiltratorEpicLegs.CreateInstance());
				GiveItemToPlayer(InfiltratorEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Necromancer)
			{
				GiveItemToPlayer(NecromancerEpicArms.CreateInstance());
				GiveItemToPlayer(NecromancerEpicBoots.CreateInstance());
				GiveItemToPlayer(NecromancerEpicGloves.CreateInstance());
				GiveItemToPlayer(NecromancerEpicHelm.CreateInstance());
				GiveItemToPlayer(NecromancerEpicLegs.CreateInstance());
				GiveItemToPlayer(NecromancerEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClassID == (byte)eCharacterClass.Heretic)
			{
				GiveItemToPlayer(HereticEpicArms.CreateInstance());
				GiveItemToPlayer(HereticEpicBoots.CreateInstance());
				GiveItemToPlayer(HereticEpicGloves.CreateInstance());
				GiveItemToPlayer(HereticEpicHelm.CreateInstance());
				GiveItemToPlayer(HereticEpicLegs.CreateInstance());
				GiveItemToPlayer(HereticEpicVest.CreateInstance());
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
            * of the Shadowy Embers  Boots 
            * of the Shadowy Embers  Coif
            * of the Shadowy Embers  Gloves
            * of the Shadowy Embers  Hauberk
            * of the Shadowy Embers  Legs
            * of the Shadowy Embers  Sleeves
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
