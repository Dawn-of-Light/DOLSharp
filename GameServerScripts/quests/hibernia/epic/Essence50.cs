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
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BMoonstone%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Moonstone Twin (level 50)
*Quest Classes  : Enchanter, Bard, Champion, Nighthsade(Path of Essence)
*Quest Version  : v1
*
*Done: 
*
*Bonuses to epic items 
*
*ToDo:
*   
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
	public class Essence_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Essence_50); }
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
			if (player.IsDoingQuest(typeof(Essence_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Champion &&
				player.CharacterClass.ID != (byte)eCharacterClass.Bard &&
				player.CharacterClass.ID != (byte)eCharacterClass.Nightshade &&
				player.CharacterClass.ID != (byte)eCharacterClass.Enchanter)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Essence_50), ExtendsType = typeof(AbstractQuest))]
	public class Essence_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Moonstone Twin";

		private static GameMob Brigit = null; // Start NPC        
		private static GameMob Caithor = null; // Mob to kill

		private static GenericItemTemplate Moonstone = null; //ball of flame

		private static FeetArmorTemplate ChampionEpicBoots = null; //Mist Shrouded Boots 
		private static HeadArmorTemplate ChampionEpicHelm = null; //Mist Shrouded Coif 
		private static HandsArmorTemplate ChampionEpicGloves = null; //Mist Shrouded Gloves 
		private static TorsoArmorTemplate ChampionEpicVest = null; //Mist Shrouded Hauberk 
		private static LegsArmorTemplate ChampionEpicLegs = null; //Mist Shrouded Legs 
		private static ArmsArmorTemplate ChampionEpicArms = null; //Mist Shrouded Sleeves 

		private static FeetArmorTemplate BardEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate BardEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate BardEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate BardEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate BardEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate BardEpicArms = null; //Shadow Shrouded Sleeves 

		private static FeetArmorTemplate EnchanterEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate EnchanterEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate EnchanterEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate EnchanterEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate EnchanterEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate EnchanterEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate NightshadeEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate NightshadeEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate NightshadeEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate NightshadeEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate NightshadeEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate NightshadeEpicArms = null; //Subterranean Sleeves

		private static GameNPC[] ChampionTrainers = null;
		private static GameNPC[] BardTrainers = null;
		private static GameNPC[] EnchanterTrainers = null;
		private static GameNPC[] NightshadeTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Brigit", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Brigit , creating it ...");
				Brigit = new GameMob();
				Brigit.Model = 384;
				Brigit.Name = "Brigit";
				Brigit.GuildName = "";
				Brigit.Realm = (byte)eRealm.Hibernia;
				Brigit.RegionId = 201;
				Brigit.Size = 51;
				Brigit.Level = 50;
				Brigit.Position = new Point(33131, 32922, 8008);
				Brigit.Heading = 3254;
				Brigit.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Brigit.SaveIntoDatabase();
				}

			}
			else
				Brigit = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Caithor", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Caithor , creating it ...");
				Caithor = new GameMob();
				Caithor.Model = 339;
				Caithor.Name = "Caithor";
				Caithor.GuildName = "";
				Caithor.Realm = (byte)eRealm.None;
				Caithor.RegionId = 200;
				Caithor.Size = 60;
				Caithor.Level = 65;
				Caithor.Position = new Point(470547, 531497, 4984);
				Caithor.Heading = 3319;
				Caithor.RespawnInterval = 5 * 60 * 1000;
				Caithor.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Caithor.SaveIntoDatabase();
				}

			}
			else
				Caithor = npcs[0] as GameMob;
			// end npc

			#endregion

			#region Item Declarations

			Moonstone = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "Moonstone");
			if (Moonstone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Moonstone , creating it ...");
				Moonstone = new GenericItemTemplate();
				Moonstone.ItemTemplateID = "Moonstone";
				Moonstone.Name = "Moonstone";
				Moonstone.Level = 8;
				Moonstone.Model = 514;
				Moonstone.IsDropable = false;
				Moonstone.IsSaleable = false;
				Moonstone.IsTradable = false;
				Moonstone.Weight = 12;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(Moonstone);
				}
			}

			ArmorTemplate i = null;
			#region Bard	
			BardEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "BardEpicBoots");
			if (BardEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "BardEpicBoots";
				i.Name = "Moonsung Boots";
				i.Level = 50;
				i.Model = 738;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicBoots = (FeetArmorTemplate)i;
			}

			//Moonsung Coif 
			BardEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "BardEpicHelm");
			if (BardEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "BardEpicHelm";
				i.Name = "Moonsung Coif";
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
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Regrowth, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicHelm = (HeadArmorTemplate)i;
			}

			//Moonsung Gloves 
			BardEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "BardEpicGloves");
			if (BardEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "BardEpicGloves";
				i.Name = "Moonsung Gloves ";
				i.Level = 50;
				i.Model = 737;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nurture, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Music, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicGloves = (HandsArmorTemplate)i;
			}

			//Moonsung Hauberk 
			BardEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "BardEpicVest");
			if (BardEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "BardEpicVest";
				i.Name = "Moonsung Hauberk";
				i.Level = 50;
				i.Model = 734;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Regrowth, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Nurture, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, 15));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicVest = (TorsoArmorTemplate)i;
			}

			//Moonsung Legs 
			BardEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "BardEpicLegs");
			if (BardEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "BardEpicLegs";
				i.Name = "Moonsung Legs";
				i.Level = 50;
				i.Model = 735;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicLegs = (LegsArmorTemplate)i;
			}

			//Moonsung Sleeves 
			BardEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "BardEpicArms");
			if (BardEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bard Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "BardEpicArms";
				i.Name = "Moonsung Sleeves";
				i.Level = 50;
				i.Model = 736;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bard);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BardEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Champion
			//Champion Epic Sleeves End
			ChampionEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ChampionEpicBoots");
			if (ChampionEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ChampionEpicBoots";
				i.Name = "Moonglow Boots";
				i.Level = 50;
				i.Model = 814;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicBoots = (FeetArmorTemplate)i;
			}

			//Moonglow Coif 
			ChampionEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ChampionEpicHelm");
			if (ChampionEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ChampionEpicHelm";
				i.Name = "Moonglow Coif";
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
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Valor, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicHelm = (HeadArmorTemplate)i;
			}

			//Moonglow Gloves 
			ChampionEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ChampionEpicGloves");
			if (ChampionEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ChampionEpicGloves";
				i.Name = "Moonglow Gloves ";
				i.Level = 50;
				i.Model = 813;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Parry, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicGloves = (HandsArmorTemplate)i;
			}

			//Moonglow Hauberk 
			ChampionEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ChampionEpicVest");
			if (ChampionEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ChampionEpicVest";
				i.Name = "Moonglow Brestplate";
				i.Level = 50;
				i.Model = 810;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Valor, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicVest = (TorsoArmorTemplate)i;
			}

			//Moonglow Legs 
			ChampionEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ChampionEpicLegs");
			if (ChampionEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ChampionEpicLegs";
				i.Name = "Moonglow Legs";
				i.Level = 50;
				i.Model = 811;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 18));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicLegs = (LegsArmorTemplate)i;
			}

			//Moonglow Sleeves 
			ChampionEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ChampionEpicArms");
			if (ChampionEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champion Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ChampionEpicArms";
				i.Name = "Moonglow Sleeves";
				i.Level = 50;
				i.Model = 812;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Champion);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Large_Weapon, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ChampionEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Nightshade
			NightshadeEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "NightshadeEpicBoots");
			if (NightshadeEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "NightshadeEpicBoots";
				i.Name = "Moonlit Boots";
				i.Level = 50;
				i.Model = 750;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicBoots = (FeetArmorTemplate)i;
			}

			//Moonlit Coif 
			NightshadeEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "NightshadeEpicHelm");
			if (NightshadeEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "NightshadeEpicHelm";
				i.Name = "Moonlit Helm";
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
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicHelm = (HeadArmorTemplate)i;
			}

			//Moonlit Gloves 
			NightshadeEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "NightshadeEpicGloves");
			if (NightshadeEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "NightshadeEpicGloves";
				i.Name = "Moonlit Gloves ";
				i.Level = 50;
				i.Model = 749;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Critical_Strike, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Envenom, 5));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicGloves = (HandsArmorTemplate)i;
			}
			//Moonlit Hauberk 
			NightshadeEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "NightshadeEpicVest");
			if (NightshadeEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "NightshadeEpicVest";
				i.Name = "Moonlit Leather Jerking";
				i.Level = 50;
				i.Model = 746;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicVest = (TorsoArmorTemplate)i;
			}

			//Moonlit Legs 
			NightshadeEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "NightshadeEpicLegs");
			if (NightshadeEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "NightshadeEpicLegs";
				i.Name = "Moonlit Leggings";
				i.Level = 50;
				i.Model = 747;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicLegs = (LegsArmorTemplate)i;
			}

			//Moonlit Sleeves 
			NightshadeEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "NightshadeEpicArms");
			if (NightshadeEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Arms , creating it ...");
				NightshadeEpicArms = new ArmsArmorTemplate();
				NightshadeEpicArms.ItemTemplateID = "NightshadeEpicArms";
				NightshadeEpicArms.Name = "Moonlit Sleeves";
				NightshadeEpicArms.Level = 50;
				NightshadeEpicArms.Model = 748;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Nightshade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Celtic_Dual, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				NightshadeEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Enchanter
			EnchanterEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "EnchanterEpicBoots");
			if (EnchanterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "EnchanterEpicBoots";
				i.Name = "Moonspun Boots";
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
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EnchanterEpicBoots = (FeetArmorTemplate)i;

			}
			//end item
			//Moonspun Coif 
			EnchanterEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "EnchanterEpicHelm");
			if (EnchanterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "EnchanterEpicHelm";
				i.Name = "Moonspun Cap";
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
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Enchantments, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 18));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EnchanterEpicHelm = (HeadArmorTemplate)i;
			}

			//Moonspun Gloves 
			EnchanterEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "EnchanterEpicGloves");
			if (EnchanterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "EnchanterEpicGloves";
				i.Name = "Moonspun Gloves ";
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
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EnchanterEpicGloves = (HandsArmorTemplate)i;
			}

			//Moonspun Hauberk 
			EnchanterEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "EnchanterEpicVest");
			if (EnchanterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "EnchanterEpicVest";
				i.Name = "Moonspun Vest";
				i.Level = 50;
				i.Model = 781;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EnchanterEpicVest = (TorsoArmorTemplate)i;
			}

			//Moonspun Legs 
			EnchanterEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "EnchanterEpicLegs");
			if (EnchanterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "EnchanterEpicLegs";
				i.Name = "Moonspun Pants";
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
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				EnchanterEpicLegs = (LegsArmorTemplate)i;
			}

			//Moonspun Sleeves 
			EnchanterEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "EnchanterEpicArms");
			if (EnchanterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Arms , creating it ...");
				EnchanterEpicArms = new ArmsArmorTemplate();
				EnchanterEpicArms.ItemTemplateID = "EnchanterEpicArms";
				EnchanterEpicArms.Name = "Moonspun Sleeves";
				EnchanterEpicArms.Level = 50;
				EnchanterEpicArms.Model = 380;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Enchanter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Hibernia;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 27));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Light, 5));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicArms);
				}
				EnchanterEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			ChampionTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ChampionTrainer), eRealm.Hibernia);
			BardTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.BardTrainer), eRealm.Hibernia);
			EnchanterTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.EnchanterTrainer), eRealm.Hibernia);
			NightshadeTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.NightshadeTrainer), eRealm.Hibernia);

			foreach (GameNPC npc in ChampionTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BardTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in EnchanterTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in NightshadeTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.AddHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.AddHandler(Caithor, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Brigit the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Brigit, typeof(Essence_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Brigit == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.RemoveHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			foreach (GameNPC npc in ChampionTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BardTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in EnchanterTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in NightshadeTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Brigit the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Brigit, typeof(Essence_50Descriptor));
		}

		protected static void TalkToBrigit(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;
			if (QuestMgr.CanGiveQuest(typeof(Essence_50), player, Brigit) <= 0)
				return;

			//We also check if the player is already doing the quest
			Essence_50 quest = player.IsDoingQuest(typeof(Essence_50)) as Essence_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Brigit.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Brigit.SayTo(player, "Hibernia needs your [services]");
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
							player.Out.SendCustomDialog("Will you help Brigit [Path of Essence Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
			Essence_50 quest = player.IsDoingQuest(typeof(Essence_50)) as Essence_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Essence_50), player, Brigit) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Essence_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Essence_50), player, Brigit))
					return;
				player.Out.SendMessage("Kill Caithor in Cursed Forest loc 28k 48k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
						return "[Step #1] Seek out Caithor in Cursed Forest Loc 20k,48k kill him!";
					case 2:
						return "[Step #2] Return to Brigit and give the Moonstone!";
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
			Essence_50Descriptor a = new Essence_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Essence_50)) != null) return;

			npc.SayTo(player, "Brigit has an important task for you, please seek her out in Tir na Nog");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Essence_50 quest = (Essence_50)player.IsDoingQuest(typeof(Essence_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("You collect the Moonstone from Caithor", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.ReceiveItem(null, CreateQuestItem(Moonstone, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Essence_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Caithor.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Moonstone from Caithor", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(Moonstone, typeof(Essence_50)));
					Step = 2;
					return;
				}

			}
			*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Brigit.Name && gArgs.Item.Name == Moonstone.Name)
				{
					RemoveItemFromPlayer(Brigit, Moonstone);
					Brigit.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}
		/*
		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Moonstone, false);
		}
		*/
		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Champion)
			{
				GiveItemToPlayer(ChampionEpicArms.CreateInstance());
				GiveItemToPlayer(ChampionEpicBoots.CreateInstance());
				GiveItemToPlayer(ChampionEpicGloves.CreateInstance());
				GiveItemToPlayer(ChampionEpicHelm.CreateInstance());
				GiveItemToPlayer(ChampionEpicLegs.CreateInstance());
				GiveItemToPlayer(ChampionEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bard)
			{
				GiveItemToPlayer(BardEpicArms.CreateInstance());
				GiveItemToPlayer(BardEpicBoots.CreateInstance());
				GiveItemToPlayer(BardEpicGloves.CreateInstance());
				GiveItemToPlayer(BardEpicHelm.CreateInstance());
				GiveItemToPlayer(BardEpicLegs.CreateInstance());
				GiveItemToPlayer(BardEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Enchanter)
			{
				GiveItemToPlayer(EnchanterEpicArms.CreateInstance());
				GiveItemToPlayer(EnchanterEpicBoots.CreateInstance());
				GiveItemToPlayer(EnchanterEpicGloves.CreateInstance());
				GiveItemToPlayer(EnchanterEpicHelm.CreateInstance());
				GiveItemToPlayer(EnchanterEpicLegs.CreateInstance());
				GiveItemToPlayer(EnchanterEpicVest.CreateInstance());
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
			{
				GiveItemToPlayer(NightshadeEpicArms.CreateInstance());
				GiveItemToPlayer(NightshadeEpicBoots.CreateInstance());
				GiveItemToPlayer(NightshadeEpicGloves.CreateInstance());
				GiveItemToPlayer(NightshadeEpicHelm.CreateInstance());
				GiveItemToPlayer(NightshadeEpicLegs.CreateInstance());
				GiveItemToPlayer(NightshadeEpicVest.CreateInstance());
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Brigit
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Brigit 
        *#28 give her the ball of flame
        *#29 talk with Brigit about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Moonsung Boots 
            *Moonsung Coif
            *Moonsung Gloves
            *Moonsung Hauberk
            *Moonsung Legs
            *Moonsung Sleeves
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