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
*Date           : 22 November 2004
*Quest Name     : Symbol of the Broken (level 50)
*Quest Classes  : Sorcerer, Minstrel, Wizard(Academy)
*Quest Version  : v1
*
*ToDo:
*   Add correct Text - Added custom text as workaround
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class Academy_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Academy_50); }
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
			if (player.IsDoingQuest(typeof(Academy_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Minstrel &&
				player.CharacterClass.ID != (byte)eCharacterClass.Wizard &&
				player.CharacterClass.ID != (byte)eCharacterClass.Sorcerer)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Academy_50), ExtendsType = typeof(AbstractQuest))] 
	public class Academy_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Symbol of the Broken";

		private static GameNPC Ferowl = null; // Start NPC
		private static GameNPC Morgana = null; // Mob
		private static GameMob Bechard = null; // Mob to kill
		private static GameMob Silcharde = null; // Mob to kill

		private static Circle morganaArea = null;

		private static GenericItemTemplate sealed_pouch = null; //sealed pouch

		private static FeetArmorTemplate WizardEpicBoots = null; //Bernor's Numinous Boots 
		private static HeadArmorTemplate WizardEpicHelm = null; //Bernor's Numinous Coif 
		private static HandsArmorTemplate WizardEpicGloves = null; //Bernor's Numinous Gloves 
		private static TorsoArmorTemplate WizardEpicVest = null; //Bernor's Numinous Hauberk 
		private static LegsArmorTemplate WizardEpicLegs = null; //Bernor's Numinous Legs 
		private static ArmsArmorTemplate WizardEpicArms = null; //Bernor's Numinous Sleeves 

		private static FeetArmorTemplate MinstrelEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate MinstrelEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate MinstrelEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate MinstrelEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate MinstrelEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate MinstrelEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate SorcererEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate SorcererEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate SorcererEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate SorcererEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate SorcererEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate SorcererEpicArms = null; //Valhalla Touched Sleeves

		private static GameNPC[] WizardTrainers = null;
		private static GameNPC[] MinstrelTrainers = null;
		private static GameNPC[] SorcererTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Ferowl", eRealm.Albion);
			if (npcs.Length == 0)
			{
				Ferowl = new GameMob();
				Ferowl.Model = 61;
				Ferowl.Name = "Master Ferowl";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Ferowl.Name + " , creating it ...");
				Ferowl.GuildName = "";
				Ferowl.Realm = (byte) eRealm.Albion;
				Ferowl.RegionId = 1;
				Ferowl.Size = 51;
				Ferowl.Level = 40;
				Ferowl.Position = new Point(559716, 510733, 2720);
				Ferowl.Heading = 703;
				Ferowl.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Ferowl.SaveIntoDatabase();
			}
			else
				Ferowl = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Morgana", eRealm.None);
			if (npcs.Length == 0)
			{
				Morgana = new GameMob();
				Morgana.Model = 283;
				Morgana.Name = "Morgana";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Morgana.Name + " , creating it ...");
				Morgana.GuildName = "";
				Morgana.Realm = (byte) eRealm.None;
				Morgana.RegionId = 1;
				Morgana.Size = 51;
				Morgana.Level = 90;
				Morgana.Position = new Point(306056, 670106, 3095);
				Morgana.Heading = 3261;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				Morgana.SetOwnBrain(brain);

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 98, 43, 0);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 133, 61, 0);
				Morgana.Inventory = template.CloseTemplate();

//				Morgana.AddNPCEquipment((byte) eVisibleItems.TORSO, 98, 43, 0, 0);
//				Morgana.AddNPCEquipment((byte) eVisibleItems.BOOT, 133, 61, 0, 0);

				//Morgana.AddToWorld(); will be added later during quest

				if (SAVE_INTO_DATABASE)
					Morgana.SaveIntoDatabase();
			}
			else
				Morgana = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Bechard", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bechard , creating it ...");
				Bechard = new GameMob();
				Bechard.Model = 606;
				Bechard.Name = "Bechard";
				Bechard.GuildName = "";
				Bechard.Realm = (byte) eRealm.None;
				Bechard.RegionId = 1;
				Bechard.Size = 50;
				Bechard.Level = 63;
				Bechard.Position = new Point(306025, 670473, 2863);
				Bechard.Heading = 3754;
				Bechard.RespawnInterval = 5 * 60 * 1000;
				Bechard.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Bechard.SaveIntoDatabase();
			}
			else
				Bechard = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Silcharde", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Silcharde , creating it ...");
				Silcharde = new GameMob();
				Silcharde.Model = 606;
				Silcharde.Name = "Silcharde";
				Silcharde.GuildName = "";
				Silcharde.Realm = (byte) eRealm.None;
				Silcharde.RegionId = 1;
				Silcharde.Size = 50;
				Silcharde.Level = 63;
				Silcharde.Position = new Point(306252, 670274, 2857);
				Silcharde.Heading = 3299;
				Silcharde.RespawnInterval = 5 * 60 * 1000;
				Silcharde.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Silcharde.SaveIntoDatabase();

			}
			else
				Silcharde = npcs[0] as GameMob;
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
					GameServer.Database.AddNewObject(sealed_pouch);
			}
			ArmorTemplate i = null;
			#region Wizard
			WizardEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "WizardEpicBoots");
			if (WizardEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizards Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "WizardEpicBoots";
				i.Name = "Bernor's Numinous Boots";
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
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Cold, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 22));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicBoots = (FeetArmorTemplate)i;
			}

			//Bernor's Numinous Coif 
			WizardEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "WizardEpicHelm");
			if (WizardEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizards Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "WizardEpicHelm";
				i.Name = "Bernor's Numinous Cap";
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
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicHelm = (HeadArmorTemplate)i;
			}

			//Bernor's Numinous Gloves 
			WizardEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "WizardEpicGloves");
			if (WizardEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizards Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "WizardEpicGloves";
				i.Name = "Bernor's Numinous Gloves ";
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
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicGloves = (HandsArmorTemplate)i;
			}

			//Bernor's Numinous Hauberk 
			WizardEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "WizardEpicVest");
			if (WizardEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizards Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "WizardEpicVest";
				i.Name = "Bernor's Numinous Robes";
				i.Level = 50;
				i.Model = 798;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicVest = (TorsoArmorTemplate)i;

			}
			//Bernor's Numinous Legs 
			WizardEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "WizardEpicLegs");
			if (WizardEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizards Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "WizardEpicLegs";
				i.Name = "Bernor's Numinous Pants";
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
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Fire, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicLegs = (LegsArmorTemplate)i;
			}

			//Bernor's Numinous Sleeves 
			WizardEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "WizardEpicArms");
			if (WizardEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wizard Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "WizardEpicArms";
				i.Name = "Bernor's Numinous Sleeves";
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
				i.AllowedClass.Add(eCharacterClass.Wizard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Albion;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Earth, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 16));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(i);
				WizardEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Minstrel
			MinstrelEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "MinstrelEpicBoots");
			if (MinstrelEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrels Epic Boots , creating it ...");
				MinstrelEpicBoots = new FeetArmorTemplate();
				MinstrelEpicBoots.ItemTemplateID = "MinstrelEpicBoots";
				MinstrelEpicBoots.Name = "Boots of Coruscating Harmony";
				MinstrelEpicBoots.Level = 50;
				MinstrelEpicBoots.Model = 727;
				MinstrelEpicBoots.IsDropable = true;
				MinstrelEpicBoots.IsSaleable = false;
				MinstrelEpicBoots.IsTradable = true;
				MinstrelEpicBoots.ArmorFactor = 100;
				MinstrelEpicBoots.ArmorLevel = eArmorLevel.High;
				MinstrelEpicBoots.Quality = 100;
				MinstrelEpicBoots.Weight = 22;
				MinstrelEpicBoots.Bonus = 35;
				MinstrelEpicBoots.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicBoots.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicBoots.Realm = eRealm.Albion;

				MinstrelEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 7));
				MinstrelEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 27));
				MinstrelEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				MinstrelEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicBoots);
			}

			//of Coruscating Harmony  Coif 
			MinstrelEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "MinstrelEpicHelm");
			if (MinstrelEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrels Epic Helm , creating it ...");
				MinstrelEpicHelm = new HeadArmorTemplate();
				MinstrelEpicHelm.ItemTemplateID = "MinstrelEpicHelm";
				MinstrelEpicHelm.Name = "Coif of Coruscating Harmony";
				MinstrelEpicHelm.Level = 50;
				MinstrelEpicHelm.Model = 1290; //NEED TO WORK ON..
				MinstrelEpicHelm.IsDropable = true;
				MinstrelEpicHelm.IsSaleable = false;
				MinstrelEpicHelm.IsTradable = true;
				MinstrelEpicHelm.ArmorFactor = 100;
				MinstrelEpicHelm.ArmorLevel = eArmorLevel.High;
				MinstrelEpicHelm.Quality = 100;
				MinstrelEpicHelm.Weight = 22;
				MinstrelEpicHelm.Bonus = 35;
				MinstrelEpicHelm.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicHelm.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicHelm.Realm = eRealm.Albion;

				MinstrelEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				MinstrelEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 18));
				MinstrelEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));
				MinstrelEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicHelm);
			}

			//of Coruscating Harmony  Gloves 
			MinstrelEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "MinstrelEpicGloves");
			if (MinstrelEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrels Epic Gloves , creating it ...");
				MinstrelEpicGloves = new HandsArmorTemplate();
				MinstrelEpicGloves.ItemTemplateID = "MinstrelEpicGloves";
				MinstrelEpicGloves.Name = "Gauntlets of Coruscating Harmony";
				MinstrelEpicGloves.Level = 50;
				MinstrelEpicGloves.Model = 726;
				MinstrelEpicGloves.IsDropable = true;
				MinstrelEpicGloves.IsSaleable = false;
				MinstrelEpicGloves.IsTradable = true;
				MinstrelEpicGloves.ArmorFactor = 100;
				MinstrelEpicGloves.ArmorLevel = eArmorLevel.High;
				MinstrelEpicGloves.Quality = 100;
				MinstrelEpicGloves.Weight = 22;
				MinstrelEpicGloves.Bonus = 35;
				MinstrelEpicGloves.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicGloves.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicGloves.Realm = eRealm.Albion;

				MinstrelEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				MinstrelEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				MinstrelEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				MinstrelEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicGloves);
			}

			//of Coruscating Harmony  Hauberk 
			MinstrelEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "MinstrelEpicVest");
			if (MinstrelEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrels Epic Vest , creating it ...");
				MinstrelEpicVest = new TorsoArmorTemplate();
				MinstrelEpicVest.ItemTemplateID = "MinstrelEpicVest";
				MinstrelEpicVest.Name = "Habergeon of Coruscating Harmony";
				MinstrelEpicVest.Level = 50;
				MinstrelEpicVest.Model = 723;
				MinstrelEpicVest.IsDropable = true;
				MinstrelEpicVest.IsSaleable = false;
				MinstrelEpicVest.IsTradable = true;
				MinstrelEpicVest.ArmorFactor = 100;
				MinstrelEpicVest.ArmorLevel = eArmorLevel.High;
				MinstrelEpicVest.Quality = 100;
				MinstrelEpicVest.Weight = 22;
				MinstrelEpicVest.Bonus = 35;
				MinstrelEpicVest.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicVest.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicVest.Realm = eRealm.Albion;

				MinstrelEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));
				MinstrelEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 8));
				MinstrelEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));
				MinstrelEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 6));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicVest);
			}

			//of Coruscating Harmony  Legs 
			MinstrelEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "MinstrelEpicLegs");
			if (MinstrelEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrels Epic Legs , creating it ...");
				MinstrelEpicLegs = new LegsArmorTemplate();
				MinstrelEpicLegs.ItemTemplateID = "MinstrelEpicLegs";
				MinstrelEpicLegs.Name = "Chaussess of Coruscating Harmony";
				MinstrelEpicLegs.Level = 50;
				MinstrelEpicLegs.Model = 724;
				MinstrelEpicLegs.IsDropable = true;
				MinstrelEpicLegs.IsSaleable = false;
				MinstrelEpicLegs.IsTradable = true;
				MinstrelEpicLegs.ArmorFactor = 100;
				MinstrelEpicLegs.ArmorLevel = eArmorLevel.High;
				MinstrelEpicLegs.Quality = 100;
				MinstrelEpicLegs.Weight = 22;
				MinstrelEpicLegs.Bonus = 35;
				MinstrelEpicLegs.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicLegs.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicLegs.Realm = eRealm.Albion;

				MinstrelEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				MinstrelEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				MinstrelEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				MinstrelEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicLegs);
			}

			//of Coruscating Harmony  Sleeves 
			MinstrelEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "MinstrelEpicArms");
			if (MinstrelEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Minstrel Epic Arms , creating it ...");
				MinstrelEpicArms = new ArmsArmorTemplate();
				MinstrelEpicArms.ItemTemplateID = "MinstrelEpicArms";
				MinstrelEpicArms.Name = "Sleeves of Coruscating Harmony";
				MinstrelEpicArms.Level = 50;
				MinstrelEpicArms.Model = 725;
				MinstrelEpicArms.IsDropable = true;
				MinstrelEpicArms.IsSaleable = false;
				MinstrelEpicArms.IsTradable = true;
				MinstrelEpicArms.ArmorFactor = 100;
				MinstrelEpicArms.ArmorLevel = eArmorLevel.High;
				MinstrelEpicArms.Quality = 100;
				MinstrelEpicArms.Weight = 22;
				MinstrelEpicArms.Bonus = 35;
				MinstrelEpicArms.AllowedClass.Add(eCharacterClass.Minstrel);
				MinstrelEpicArms.MaterialLevel = eMaterialLevel.Arcanium;
				MinstrelEpicArms.Realm = eRealm.Albion;

				MinstrelEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 16));
				MinstrelEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 21));
				MinstrelEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 8));
				MinstrelEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(MinstrelEpicArms);
			}
			#endregion
			#region Sorcerer
			SorcererEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "SorcererEpicBoots");
			if (SorcererEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Boots , creating it ...");
				SorcererEpicBoots = new FeetArmorTemplate();
				SorcererEpicBoots.ItemTemplateID = "SorcererEpicBoots";
				SorcererEpicBoots.Name = "Boots of Mental Acuity";
				SorcererEpicBoots.Level = 50;
				SorcererEpicBoots.Model = 143;
				SorcererEpicBoots.IsDropable = true;
				SorcererEpicBoots.IsSaleable = false;
				SorcererEpicBoots.IsTradable = false;
				SorcererEpicBoots.ArmorFactor = 50;
				SorcererEpicBoots.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicBoots.Quality = 100;
				SorcererEpicBoots.Weight = 22;
				SorcererEpicBoots.Bonus = 35;
				SorcererEpicBoots.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicBoots.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicBoots.Realm = eRealm.Albion;

				SorcererEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Matter, 4));
				SorcererEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 22));
				SorcererEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				SorcererEpicBoots.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicBoots);
			}

			//of Mental Acuity Coif 
			SorcererEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "SorcererEpicHelm");
			if (SorcererEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Helm , creating it ...");
				SorcererEpicHelm = new HeadArmorTemplate();
				SorcererEpicHelm.ItemTemplateID = "SorcererEpicHelm";
				SorcererEpicHelm.Name = "Cap of Mental Acuity";
				SorcererEpicHelm.Level = 50;
				SorcererEpicHelm.Model = 1290; //NEED TO WORK ON..
				SorcererEpicHelm.IsDropable = true;
				SorcererEpicHelm.IsSaleable = false;
				SorcererEpicHelm.IsTradable = true;
				SorcererEpicHelm.ArmorFactor = 50;
				SorcererEpicHelm.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicHelm.Quality = 100;
				SorcererEpicHelm.Weight = 22;
				SorcererEpicHelm.Bonus = 35;
				SorcererEpicHelm.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicHelm.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicHelm.Realm = eRealm.Albion;

				SorcererEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				SorcererEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 21));
				SorcererEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 8));
				SorcererEpicHelm.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicHelm);
			}
//end item
			//of Mental Acuity Gloves 
			SorcererEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "SorcererEpicGloves");
			if (SorcererEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Gloves , creating it ...");
				SorcererEpicGloves = new HandsArmorTemplate();
				SorcererEpicGloves.ItemTemplateID = "SorcererEpicGloves";
				SorcererEpicGloves.Name = "Gloves of Mental Acuity";
				SorcererEpicGloves.Level = 50;
				SorcererEpicGloves.Model = 142;
				SorcererEpicGloves.IsDropable = true;
				SorcererEpicGloves.IsSaleable = false;
				SorcererEpicGloves.IsTradable = true;
				SorcererEpicGloves.ArmorFactor = 50;
				SorcererEpicGloves.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicGloves.Quality = 100;
				SorcererEpicGloves.Weight = 22;
				SorcererEpicGloves.Bonus = 35;
				SorcererEpicGloves.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicGloves.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicGloves.Realm = eRealm.Albion;

				SorcererEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				SorcererEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));
				SorcererEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				SorcererEpicGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicGloves);
			}

			//of Mental Acuity Hauberk 
			SorcererEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "SorcererEpicVest");
			if (SorcererEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Vest , creating it ...");
				SorcererEpicVest = new TorsoArmorTemplate();
				SorcererEpicVest.ItemTemplateID = "SorcererEpicVest";
				SorcererEpicVest.Name = "Vest of Mental Acuity";
				SorcererEpicVest.Level = 50;
				SorcererEpicVest.Model = 804;
				SorcererEpicVest.IsDropable = true;
				SorcererEpicVest.IsSaleable = false;
				SorcererEpicVest.IsTradable = true;
				SorcererEpicVest.ArmorFactor = 50;
				SorcererEpicVest.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicVest.Quality = 100;
				SorcererEpicVest.Weight = 22;
				SorcererEpicVest.Bonus = 35;
				SorcererEpicVest.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicVest.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicVest.Realm = eRealm.Albion;

				SorcererEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 4));
				SorcererEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 14));
				SorcererEpicVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicVest);
			}

			//of Mental Acuity Legs 
			SorcererEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "SorcererEpicLegs");
			if (SorcererEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Legs , creating it ...");
				SorcererEpicLegs = new LegsArmorTemplate();
				SorcererEpicLegs.ItemTemplateID = "SorcererEpicLegs";
				SorcererEpicLegs.Name = "Pants of Mental Acuity";
				SorcererEpicLegs.Level = 50;
				SorcererEpicLegs.Model = 140;
				SorcererEpicLegs.IsDropable = true;
				SorcererEpicLegs.IsSaleable = false;
				SorcererEpicLegs.IsTradable = true;
				SorcererEpicLegs.ArmorFactor = 50;
				SorcererEpicLegs.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicLegs.Quality = 100;
				SorcererEpicLegs.Weight = 22;
				SorcererEpicLegs.Bonus = 35;
				SorcererEpicLegs.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicLegs.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicLegs.Realm = eRealm.Albion;

				SorcererEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mind, 4));
				SorcererEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				SorcererEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 8));
				SorcererEpicLegs.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 8));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicLegs);
			}

			//of Mental Acuity Sleeves 
			SorcererEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "SorcererEpicArms");
			if (SorcererEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sorcerer Epic Arms , creating it ...");
				SorcererEpicArms = new ArmsArmorTemplate();
				SorcererEpicArms.ItemTemplateID = "SorcererEpicArms";
				SorcererEpicArms.Name = "Sleeves of Mental Acuity";
				SorcererEpicArms.Level = 50;
				SorcererEpicArms.Model = 141;
				SorcererEpicArms.IsDropable = true;
				SorcererEpicArms.IsSaleable = false;
				SorcererEpicArms.IsTradable = true;
				SorcererEpicArms.ArmorFactor = 50;
				SorcererEpicArms.ArmorLevel = eArmorLevel.VeryLow;
				SorcererEpicArms.Quality = 100;
				SorcererEpicArms.Weight = 22;
				SorcererEpicArms.Bonus = 35;
				SorcererEpicArms.AllowedClass.Add(eCharacterClass.Sorcerer);
				SorcererEpicArms.MaterialLevel = eMaterialLevel.Arcanium;
				SorcererEpicArms.Realm = eRealm.Albion;

				SorcererEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Body, 4));
				SorcererEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				SorcererEpicArms.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(SorcererEpicArms);
			}
			#endregion
			#endregion
			 
			WizardTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.WizardTrainer), eRealm.Albion);
			MinstrelTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.MinstrelTrainer), eRealm.Albion);
			SorcererTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.SorcererTrainer), eRealm.Albion);

			foreach (GameNPC npc in WizardTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in MinstrelTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SorcererTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			morganaArea = new Circle();
			morganaArea.X = Morgana.Position.X;
			morganaArea.Y = Morgana.Position.Y;
			morganaArea.Radius = 1000;
			AreaMgr.RegisterArea(morganaArea);

			GameEventMgr.AddHandler(Ferowl, GameObjectEvent.Interact, new DOLEventHandler(TalkToFerowl));
			GameEventMgr.AddHandler(Ferowl, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFerowl));
			GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterMorganaArea));
			GameEventMgr.AddHandler(Bechard, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));
			GameEventMgr.AddHandler(Silcharde, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Ferowl the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Ferowl, typeof(Academy_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Ferowl == null)
				return;

			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterMorganaArea));
			AreaMgr.UnregisterArea(morganaArea);
			// remove handlers
			GameEventMgr.RemoveHandler(Ferowl, GameObjectEvent.Interact, new DOLEventHandler(TalkToFerowl));
			GameEventMgr.RemoveHandler(Ferowl, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFerowl));
			GameEventMgr.RemoveHandler(Bechard, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));
			GameEventMgr.RemoveHandler(Silcharde, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			foreach (GameNPC npc in WizardTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in MinstrelTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in SorcererTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Ferowl the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Ferowl, typeof(Academy_50Descriptor));
		}

		protected static void PlayerEnterMorganaArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			if (aargs.Area != morganaArea) return;
			GamePlayer player = aargs.GameObject as GamePlayer;
			Academy_50 quest = player.IsDoingQuest(typeof (Academy_50)) as Academy_50;

			if (quest != null && Morgana.ObjectState != GameObject.eObjectState.Active)
			{
				// player near grove
				SendSystemMessage(player, "As you approach the fallen tower you see Morgana standing on top of the tower.");
				quest.CreateMorgana();

				if (player.PlayerGroup != null)
					Morgana.Yell("Ha, is this all the forces of Albion have to offer? I expected a whole army leaded by my brother Arthur, but what do they send a little group of adventurers lead by a poor " + player.CharacterClass.Name + "?");
				else
					Morgana.Yell("Ha, is this all the forces of Albion have to offer? I expected a whole army leaded by my brother Arthur, but what do they send a poor " + player.CharacterClass.Name + "?");

				foreach (GamePlayer visPlayer in Morgana.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(Morgana, 1, 20);
				}
			}
		}

		protected virtual void CreateMorgana()
		{
			if (Morgana == null)
			{
				Morgana = new GameMob();
				Morgana.Model = 283;
				Morgana.Name = "Morgana";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Morgana.Name + " , creating it ...");
				Morgana.GuildName = "";
				Morgana.Realm = (byte) eRealm.None;
				Morgana.RegionId = 1;
				Morgana.Size = 51;
				Morgana.Level = 90;
				Morgana.Position = new Point(306056, 670106, 3095);
				Morgana.Heading = 3261;

				
				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				Morgana.SetOwnBrain(brain);

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 98, 43, 0);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 133, 61, 0);
				Morgana.Inventory = template.CloseTemplate();
			}

			Morgana.AddToWorld();
		}

		protected virtual void DeleteMorgana()
		{
			if (Morgana != null)
				Morgana.Delete();
		}

		protected static void TalkToFerowl(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Academy_50), player, Ferowl) <= 0)
				return;

			//We also check if the player is already doing the quest
			Academy_50 quest = player.IsDoingQuest(typeof (Academy_50)) as Academy_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Ferowl.SayTo(player, "Where you able to [fulfill] your given task? Albions fate lies in you hands. ");
				}
				else
				{
					Ferowl.SayTo(player, "Ah good to see you, there are rumors about your tasks all over Albion, yet we are in need of your [services] once again!");
				}
				return;
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendCustomDialog("Will you help Ferowl [Academy Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "Morgana":
							Ferowl.SayTo(player, "You must have heard about her, she's the evil sister of King Arthur, tried to take his throne. She must be [stopped]!");
							break;
						case "stopped":
							Ferowl.SayTo(player, "Once Morgana has summoned her army everything is lost. So hurry and stop her unholy rituals. With the help of two mighty demons Silcharde and Bechard she can summon as many minions as she wants. Killing one of them should be enough to stop here [ritual].");
							break;
						case "ritual":
							Ferowl.SayTo(player, "Morgana is probably performing her rital at the fallen tower in Lyonesse. To get there follow the Telamon road past the majority of the Danaoian Farmers, until you see the [fallen tower].");
							break;
						case "fallen tower":
							Ferowl.SayTo(player, "Be wise and don't take any unneccessary risks by going directly on Morgana , you might be a strong " + player.CharacterClass.Name + ", but you are no match for Morgana herself. Kill her demons and return to me, we will then try to take care of the rest, once her time has come.");
							break;

							// once the deomns are dead:
						case "fulfill":
							Ferowl.SayTo(player, "Did you find anything near the fallen tower? If yes give it to me, we could need any hints we can get on our crusade against Morgana.");
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
			Academy_50 quest = player.IsDoingQuest(typeof (Academy_50)) as Academy_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Academy_50), player, Ferowl) <= 0)
				return;

			if (player.IsDoingQuest(typeof (Academy_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(Academy_50), player))
					return;

				Ferowl.SayTo(player, "I have heard rumors about the witch [Morgana] trying to summon an army of demons to crush the mighty city of Camelot!");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Symbol of the Broken (Level 50 Academy Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Bechard or Silcharde at the fallen tower in Lyonesse and kill one!";
					case 2:
						return "[Step #2] Return to Master Ferowl and hand him the Sealed Pouch for your reward!";
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
			Academy_50Descriptor a = new Academy_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Academy_50)) != null) return;

			npc.SayTo(player, "Master Ferowl has an important task for you, please seek him out in Cotswold");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Academy_50 quest = (Academy_50)player.IsDoingQuest(typeof(Academy_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				Morgana.Yell("You may have stopped me here, but I'll come back! Albion will be mine!");
				quest.DeleteMorgana();

				player.Out.SendMessage("Take the pouch to " + Ferowl.GetName(0, true), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(sealed_pouch, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Academy_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				{
					if (gArgs.Target.Name == Bechard.Name || gArgs.Target.Name == Silcharde.Name)
					{
						Morgana.Yell("You may have stopped me here, but I'll come back! Albion will be mine!");
						DeleteMorgana();

						m_questPlayer.Out.SendMessage("Take the pouch to " + Ferowl.GetName(0, true), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						GiveItemToPlayer(CreateQuestItem(sealed_pouch, Name));
						Step = 2;
						return;
					}
				}
			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Ferowl.Name && gArgs.Item.Name == sealed_pouch.Name)
				{
					RemoveItemFromPlayer(Ferowl, sealed_pouch);
					Ferowl.SayTo(player, "You have earned this Epic Armour, wear it with honor!");
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

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Minstrel)
			{
				GiveItemToPlayer(MinstrelEpicBoots.CreateInstance());
				GiveItemToPlayer(MinstrelEpicHelm.CreateInstance());
				GiveItemToPlayer(MinstrelEpicGloves.CreateInstance());
				GiveItemToPlayer(MinstrelEpicArms.CreateInstance());
				GiveItemToPlayer(MinstrelEpicVest.CreateInstance());
				GiveItemToPlayer(MinstrelEpicLegs.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Wizard)
			{
				GiveItemToPlayer(WizardEpicBoots.CreateInstance());
				GiveItemToPlayer(WizardEpicHelm.CreateInstance());
				GiveItemToPlayer(WizardEpicGloves.CreateInstance());
				GiveItemToPlayer(WizardEpicVest.CreateInstance());
				GiveItemToPlayer(WizardEpicArms.CreateInstance());
				GiveItemToPlayer(WizardEpicLegs.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Sorcerer)
			{
				GiveItemToPlayer(SorcererEpicBoots.CreateInstance());
				GiveItemToPlayer(SorcererEpicHelm.CreateInstance());
				GiveItemToPlayer(SorcererEpicGloves.CreateInstance());
				GiveItemToPlayer(SorcererEpicVest.CreateInstance());
				GiveItemToPlayer(SorcererEpicArms.CreateInstance());
				GiveItemToPlayer(SorcererEpicLegs.CreateInstance());
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        * Return to Esmond in Cornwall Station once you reach level 50. If you had given him the dagger at the end of the level 48 epic, he will ask you if you want it back. If he asks, accept the knife back and continue. If not, make sure you have the ritual dagger with you. 
		* Go to Lyonesse and find the tower, which is located at 20k, 39k. You can simply follow the Telamon road past the majority of the Danaoian Farmers, until you see a fallen tower with two large named demons (purple to 50) and Morgana sitting on top of the tower. 
		* To defeat them is quite easy and can take as little as 6 people. As long as you have at least one tank, a healer, and someone who can root or mez, you should be ok. 
		* Do not attack Morgana. She will not do anything during this attack. Have someone root or mez one of the named demons, while the tank(s) hold aggro on the second one. When the aggroed one is defeated, a large group of tiny demons will appear and fly around the tower (they were all green to a 50). Take care of the previously rooted/mezed demon and another group of tiny demons will appear. Morgana will spout off something that can be heard across the zone, then leave. Kill all the tiny demons that remain. 
		* Once all the aggro has been cleared, stand next to the tower. There will be a message that says, "You sense the tower is clear of necromantic ties!" about 5 or so times. Your dagger should dissapear from your inventory, followed by a message that says, "A sense of calm settles about you!" When you recieve that message, your journal will update and tell you go to meet Master Ferowl again. 
		* Master Ferowl congratulates you on a job well done and asks you to go meet your trainer in Camelot for your reward. Also, Ferowl gives you 1,937,768,448 experience for some reason. 
		* Your trainer in Camelot should give you your epic armor, with another congratulations. 
		* The description of this quest was done by a Wizard. Other Academy classes might be slightly different. Also, this quest takes into consideration that you gave the knife to Esmond at the end of the 48 epic quest, which may or may not be a big deal.
        */

		#endregion
	}
}
