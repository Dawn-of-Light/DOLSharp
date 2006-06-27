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
*Quest Name     : War Concluded (Level 50)
*Quest Classes  : Hunter, Shadowblade (Rogue)
*Quest Version  : v1
*
*Changes:
*add bonuses to epic items
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

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the quest requirement
* class linked with the new Quest. To do this, we derive 
* from the abstract class AbstractQuestDescriptor
*/
	public class Rogue_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Rogue_50); }
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
			if (player.IsDoingQuest(typeof(Rogue_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Shadowblade &&
				player.CharacterClass.ID != (byte)eCharacterClass.Hunter)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Rogue_50), ExtendsType = typeof(AbstractQuest))]
	public class Rogue_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "War Concluded";

		private static GameNPC Masrim = null; // Start NPC
		private static GameMob Oona = null; // Mob to kill
		private static GameNPC MorlinCaan = null; // Trainer for reward

		private static GenericItemTemplate oona_head = null; //ball of flame
		private static GenericItemTemplate sealed_pouch = null; //sealed pouch

		private static FeetArmorTemplate HunterEpicBoots = null; //Call of the Hunt Boots 
		private static HeadArmorTemplate HunterEpicHelm = null; //Call of the Hunt Coif 
		private static HandsArmorTemplate HunterEpicGloves = null; //Call of the Hunt Gloves 
		private static TorsoArmorTemplate HunterEpicVest = null; //Call of the Hunt Hauberk 
		private static LegsArmorTemplate HunterEpicLegs = null; //Call of the Hunt Legs 
		private static ArmsArmorTemplate HunterEpicArms = null; //Call of the Hunt Sleeves 

		private static FeetArmorTemplate ShadowbladeEpicBoots = null; //Shadow Shrouded Boots 
		private static HeadArmorTemplate ShadowbladeEpicHelm = null; //Shadow Shrouded Coif 
		private static HandsArmorTemplate ShadowbladeEpicGloves = null; //Shadow Shrouded Gloves 
		private static TorsoArmorTemplate ShadowbladeEpicVest = null; //Shadow Shrouded Hauberk 
		private static LegsArmorTemplate ShadowbladeEpicLegs = null; //Shadow Shrouded Legs 
		private static ArmsArmorTemplate ShadowbladeEpicArms = null; //Shadow Shrouded Sleeves

		private static GameNPC[] HunterTrainers = null;
		private static GameNPC[] ShadowbladeTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Masrim", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Masrim , creating it ...");
				Masrim = new GameMob();
				Masrim.Model = 177;
				Masrim.Name = "Masrim";
				Masrim.GuildName = "";
				Masrim.Realm = (byte)eRealm.Midgard;
				Masrim.RegionId = 100;
				Masrim.Size = 52;
				Masrim.Level = 40;
				Masrim.Position = new Point(749099, 813104, 4437);
				Masrim.Heading = 2605;
				Masrim.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Masrim.SaveIntoDatabase();
				}

			}
			else
				Masrim = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Oona", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Oona , creating it ...");
				Oona = new GameMob();
				Oona.Model = 356;
				Oona.Name = "Oona";
				Oona.GuildName = "";
				Oona.Realm = (byte)eRealm.None;
				Oona.RegionId = 100;
				Oona.Size = 50;
				Oona.Level = 65;
				Oona.Position = new Point(607233, 786850, 4384);
				Oona.Heading = 3891;
				Oona.Flags = 1;
				Oona.RespawnInterval = 5 * 60 * 1000;
				Oona.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Oona.SaveIntoDatabase();
				}

			}
			else
				Oona = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Morlin Caan", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Morlin Caan , creating it ...");
				MorlinCaan = new GameMob();
				MorlinCaan.Model = 235;
				MorlinCaan.Name = "Morlin Caan";
				MorlinCaan.GuildName = "Smith";
				MorlinCaan.Realm = (byte)eRealm.Midgard;
				MorlinCaan.RegionId = 101;
				MorlinCaan.Size = 50;
				MorlinCaan.Level = 54;
				MorlinCaan.Position = new Point(33400, 33620, 8023);
				MorlinCaan.Heading = 523;
				MorlinCaan.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					MorlinCaan.SaveIntoDatabase();
				}

			}
			else
				MorlinCaan = npcs[0];
			// end npc

			#endregion

			#region defineItems

			oona_head = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "oona_head");
			if (oona_head == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Oona's Head , creating it ...");
				oona_head = new GenericItemTemplate();
				oona_head.ItemTemplateID = "oona_head";
				oona_head.Name = "Oona's Head";
				oona_head.Level = 8;
				oona_head.Model = 503;
				oona_head.IsDropable = false;
				oona_head.IsSaleable = false;
				oona_head.IsTradable = false;
				oona_head.Weight = 12;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(oona_head);
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
			#region Hunter
			HunterEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "HunterEpicBoots");
			if (HunterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "HunterEpicBoots";
				i.Name = "Call of the Hunt Boots";
				i.Level = 50;
				i.Model = 760;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicBoots = (FeetArmorTemplate)i;
			}

			//Call of the Hunt Coif 
			HunterEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "HunterEpicHelm");
			if (HunterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "HunterEpicHelm";
				i.Name = "Call of the Hunt Coif";
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
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Spear, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Stealth, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Composite, 3));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 19));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicHelm = (HeadArmorTemplate)i;
			}

			//Call of the Hunt Gloves 
			HunterEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "HunterEpicGloves");
			if (HunterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "HunterEpicGloves";
				i.Name = "Call of the Hunt Gloves ";
				i.Level = 50;
				i.Model = 759;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Composite, 5));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicGloves = (HandsArmorTemplate)i;
			}

			//Call of the Hunt Hauberk 
			HunterEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "HunterEpicVest");
			if (HunterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "HunterEpicVest";
				i.Name = "Call of the Hunt Jerkin";
				i.Level = 50;
				i.Model = 756;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicVest = (TorsoArmorTemplate)i;
			}

			//Call of the Hunt Legs 
			HunterEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "HunterEpicLegs");
			if (HunterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "HunterEpicLegs";
				i.Name = "Call of the Hunt Legs";
				i.Level = 50;
				i.Model = 757;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 7));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 12));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicLegs = (LegsArmorTemplate)i;
			}

			//Call of the Hunt Sleeves 
			HunterEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "HunterEpicArms");
			if (HunterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunter Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "HunterEpicArms";
				i.Name = "Call of the Hunt Sleeves";
				i.Level = 50;
				i.Model = 758;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Medium;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Hunter);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HunterEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Shadowblade
			//Shadow Shrouded Boots 
			ShadowbladeEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ShadowbladeEpicBoots");
			if (ShadowbladeEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicBoots";
				i.Name = "Shadow Shrouded Boots";
				i.Level = 50;
				i.Model = 765;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Stealth, 5));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicBoots = (FeetArmorTemplate)i;
			}

			//Shadow Shrouded Coif 
			ShadowbladeEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ShadowbladeEpicHelm");
			if (ShadowbladeEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicHelm";
				i.Name = "Shadow Shrouded Coif";
				i.Level = 50;
				i.Model = 335; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicHelm = (HeadArmorTemplate)i;
			}

			//Shadow Shrouded Gloves 
			ShadowbladeEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ShadowbladeEpicGloves");
			if (ShadowbladeEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicGloves";
				i.Name = "Shadow Shrouded Gloves";
				i.Level = 50;
				i.Model = 764;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Critical_Strike, 2));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 33));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Envenom, 4));


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicGloves = (HandsArmorTemplate)i;
			}

			//Shadow Shrouded Hauberk 
			ShadowbladeEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ShadowbladeEpicVest");
			if (ShadowbladeEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicVest";
				i.Name = "Shadow Shrouded Jerkin";
				i.Level = 50;
				i.Model = 761;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 30));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicLegs = (LegsArmorTemplate)i;
			}

			//Shadow Shrouded Legs 
			ShadowbladeEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ShadowbladeEpicLegs");
			if (ShadowbladeEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicLegs";
				i.Name = "Shadow Shrouded Legs";
				i.Level = 50;
				i.Model = 762;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicLegs = (LegsArmorTemplate)i;
			}

			//Shadow Shrouded Sleeves 
			ShadowbladeEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ShadowbladeEpicArms");
			if (ShadowbladeEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ShadowbladeEpicArms";
				i.Name = "Shadow Shrouded Sleeves";
				i.Level = 50;
				i.Model = 763;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.Low;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shadowblade);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShadowbladeEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			HunterTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.HunterTrainer), eRealm.Midgard);
			ShadowbladeTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ShadowbladeTrainer), eRealm.Midgard);

			foreach (GameNPC npc in HunterTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ShadowbladeTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Masrim, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasrim));
			GameEventMgr.AddHandler(Masrim, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasrim));

			GameEventMgr.AddHandler(MorlinCaan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMorlinCaan));
			GameEventMgr.AddHandler(MorlinCaan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMorlinCaan));

			GameEventMgr.AddHandler(Oona, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Masrim the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Masrim, typeof(Rogue_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Masrim == null || MorlinCaan == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Masrim, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasrim));
			GameEventMgr.RemoveHandler(Masrim, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasrim));

			GameEventMgr.RemoveHandler(MorlinCaan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMorlinCaan));
			GameEventMgr.RemoveHandler(MorlinCaan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMorlinCaan));

			GameEventMgr.RemoveHandler(Oona, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			foreach (GameNPC npc in HunterTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ShadowbladeTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Masrim the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Masrim, typeof(Rogue_50Descriptor));
		}

		protected static void TalkToMasrim(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Rogue_50), player, Masrim) <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof(Rogue_50)) as Rogue_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Masrim.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Masrim.SayTo(player, "Midgard needs your [services]");
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
							player.Out.SendCustomDialog("Will you help Masrim [Rogue Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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

		protected static void TalkToMorlinCaan(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Rogue_50), player, Masrim) <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof(Rogue_50)) as Rogue_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					MorlinCaan.SayTo(player, "Check your journal for instructions!");
				}
				return;
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Rogue_50 quest = player.IsDoingQuest(typeof(Rogue_50)) as Rogue_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Rogue_50), player, Masrim) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Rogue_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Rogue_50), player, Masrim))
					return;

				player.Out.SendMessage("Kill Oona in Raumarik loc 20k,51k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "War Concluded (Level 50 Rogue Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Oona in Raumarik Loc 20k,51k kill it!";
					case 2:
						return "[Step #2] Return to Masrim and give her Oona's Head!";
					case 3:
						return "[Step #3] Go to Morlin Caan in Jordheim and give him the Sealed Pouch for your reward!";
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
			Rogue_50Descriptor a = new Rogue_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Rogue_50)) == null) return;

			npc.SayTo(player, "Masrim has an important task for you, please seek her out in Fort Atla");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Rogue_50 quest = (Rogue_50)player.IsDoingQuest(typeof(Rogue_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("You collect Oona's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				quest.GiveItemToPlayer(CreateQuestItem(oona_head, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Rogue_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Oona.Name)
				{
					m_questPlayer.Out.SendMessage("You collect Oona's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(oona_head, Name));
					Step = 2;
					return;
				}
			}*/

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Masrim.Name && gArgs.Item.Name == oona_head.Name)
				{
					RemoveItemFromPlayer(Masrim, oona_head);
					Masrim.SayTo(player, "Take this sealed pouch to Morlin Caan in Jordheim for your reward!");
					GiveItemToPlayer(CreateQuestItem(sealed_pouch, Name));
					Step = 3;
					return;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == MorlinCaan.Name && gArgs.Item.Name == sealed_pouch.Name)
				{
					RemoveItemFromPlayer(MorlinCaan, sealed_pouch);
					MorlinCaan.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(sealed_pouch);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shadowblade)
			{
				GiveItemToPlayer(ShadowbladeEpicArms);
				GiveItemToPlayer(ShadowbladeEpicBoots);
				GiveItemToPlayer(ShadowbladeEpicGloves);
				GiveItemToPlayer(ShadowbladeEpicHelm);
				GiveItemToPlayer(ShadowbladeEpicLegs);
				GiveItemToPlayer(ShadowbladeEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hunter)
			{
				GiveItemToPlayer(HunterEpicArms);
				GiveItemToPlayer(HunterEpicBoots);
				GiveItemToPlayer(HunterEpicGloves);
				GiveItemToPlayer(HunterEpicHelm);
				GiveItemToPlayer(HunterEpicLegs);
				GiveItemToPlayer(HunterEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Masrim
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Masrim 
        *#28 give her the ball of flame
        *#29 talk with Masrim about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Call of the Hunt Boots 
            *Call of the Hunt Coif
            *Call of the Hunt Gloves
            *Call of the Hunt Hauberk
            *Call of the Hunt Legs
            *Call of the Hunt Sleeves
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
