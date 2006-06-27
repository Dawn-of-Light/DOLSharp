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
*Date           : 21 November 2004
*Quest Name     : The Desire of a God (Level 50)
*Quest Classes  : Healer, Shaman (Seer)
*Quest Version  : v1.2
*
*Changes:
*   The epic armour should now have the correct durability and condition
*   The armour will now be correctly rewarded with all peices
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*   Add bonuses to epic items
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

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the quest requirement
 * class linked with the new Quest. To do this, we derive 
 * from the abstract class AbstractQuestDescriptor
 */
	public class Seer_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Seer_50); }
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
			if (player.IsDoingQuest(typeof(Seer_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Shaman &&
				player.CharacterClass.ID != (byte)eCharacterClass.Healer)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Seer_50), ExtendsType = typeof(AbstractQuest))]
	public class Seer_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Desire of a God";

		private static GameMob Inaksha = null; // Start NPC
		private static GameMob Loken = null; // Mob to kill
		private static GameMob Miri = null; // Trainer for reward

		private static GenericItemTemplate ball_of_flame = null; //ball of flame
		private static GenericItemTemplate sealed_pouch = null; //sealed pouch

		private static FeetArmorTemplate HealerEpicBoots = null; //Valhalla Touched Boots 
		private static HeadArmorTemplate HealerEpicHelm = null; //Valhalla Touched Coif 
		private static HandsArmorTemplate HealerEpicGloves = null; //Valhalla Touched Gloves 
		private static TorsoArmorTemplate HealerEpicVest = null; //Valhalla Touched Hauberk 
		private static LegsArmorTemplate HealerEpicLegs = null; //Valhalla Touched Legs 
		private static ArmsArmorTemplate HealerEpicArms = null; //Valhalla Touched Sleeves 

		private static FeetArmorTemplate ShamanEpicBoots = null; //Subterranean Boots 
		private static HeadArmorTemplate ShamanEpicHelm = null; //Subterranean Coif 
		private static HandsArmorTemplate ShamanEpicGloves = null; //Subterranean Gloves 
		private static TorsoArmorTemplate ShamanEpicVest = null; //Subterranean Hauberk 
		private static LegsArmorTemplate ShamanEpicLegs = null; //Subterranean Legs 
		private static ArmsArmorTemplate ShamanEpicArms = null; //Subterranean Sleeves

		private static GameNPC[] HealerTrainers = null;
		private static GameNPC[] ShamanTrainers = null;

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Inaksha", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Inaksha , creating it ...");
				Inaksha = new GameMob();
				Inaksha.Model = 193;
				Inaksha.Name = "Inaksha";
				Inaksha.GuildName = "";
				Inaksha.Realm = (byte)eRealm.Midgard;
				Inaksha.RegionId = 100;
				Inaksha.Size = 50;
				Inaksha.Level = 41;
				Inaksha.Position = new Point(805929, 702449, 4960);
				Inaksha.Heading = 2116;
				Inaksha.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Inaksha.SaveIntoDatabase();
				}

			}
			else
				Inaksha = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Loken", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Loken , creating it ...");
				Loken = new GameMob();
				Loken.Model = 212;
				Loken.Name = "Loken";
				Loken.GuildName = "";
				Loken.Realm = (byte)eRealm.None;
				Loken.RegionId = 100;
				Loken.Size = 50;
				Loken.Level = 65;
				Loken.Position = new Point(636784, 762433, 4596);
				Loken.Heading = 3777;
				Loken.RespawnInterval = 5 * 60 * 1000;
				Loken.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Loken.SaveIntoDatabase();
				}

			}
			else
				Loken = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Miri", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Miri , creating it ...");
				Miri = new GameMob();
				Miri.Model = 220;
				Miri.Name = "Miri";
				Miri.GuildName = "";
				Miri.Realm = (byte)eRealm.Midgard;
				Miri.RegionId = 101;
				Miri.Size = 50;
				Miri.Level = 43;
				Miri.Position = new Point(30641, 32093, 8305);
				Miri.Heading = 3037;
				Miri.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Miri.SaveIntoDatabase();
				}

			}
			else
				Miri = npcs[0] as GameMob;
			// end npc

			#endregion

			#region defineItems

			ball_of_flame = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "ball_of_flame");
			if (ball_of_flame == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find ball_of_flame , creating it ...");
				ball_of_flame = new GenericItemTemplate();
				ball_of_flame.ItemTemplateID = "ball_of_flame";
				ball_of_flame.Name = "Ball of Flame";
				ball_of_flame.Level = 8;
				ball_of_flame.Model = 601;
				ball_of_flame.IsDropable = false;
				ball_of_flame.IsSaleable = false;
				ball_of_flame.IsTradable = false;
				ball_of_flame.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ball_of_flame);
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
			#region Healer
			//Valhalla Touched Boots
			HealerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "HealerEpicBoots");
			if (HealerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "HealerEpicBoots";
				i.Name = "Valhalla Touched Boots";
				i.Level = 50;
				i.Model = 702;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 21));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicBoots = (FeetArmorTemplate)i;
			}

			//Valhalla Touched Coif 
			HealerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "HealerEpicHelm");
			if (HealerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "HealerEpicHelm";
				i.Name = "Valhalla Touched Coif";
				i.Level = 50;
				i.Model = 63; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Augmentation, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicHelm = (HeadArmorTemplate)i;
			}

			//Valhalla Touched Gloves 
			HealerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "HealerEpicGloves");
			if (HealerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "HealerEpicGloves";
				i.Name = "Valhalla Touched Gloves ";
				i.Level = 50;
				i.Model = 701;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mending, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicGloves = (HandsArmorTemplate)i;
			}

			//Valhalla Touched Hauberk 
			HealerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "HealerEpicVest");
			if (HealerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "HealerEpicVest";
				i.Name = "Valhalla Touched Haukberk";
				i.Level = 50;
				i.Model = 698;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicVest = (TorsoArmorTemplate)i;
			}

			//Valhalla Touched Legs 
			HealerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "HealerEpicLegs");
			if (HealerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "HealerEpicLegs";
				i.Name = "Valhalla Touched Legs";
				i.Level = 50;
				i.Model = 699;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicLegs = (LegsArmorTemplate)i;
			}

			//Valhalla Touched Sleeves 
			HealerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "HealerEpicArms");
			if (HealerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healer Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "HealerEpicArms";
				i.Name = "Valhalla Touched Sleeves";
				i.Level = 50;
				i.Model = 700;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Healer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mending, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 15));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				HealerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Shaman
			//Subterranean Boots 
			ShamanEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "ShamanEpicBoots");
			if (ShamanEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "ShamanEpicBoots";
				i.Name = "Subterranean Boots";
				i.Level = 50;
				i.Model = 770;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 39));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicBoots = (FeetArmorTemplate)i;
			}

			//Subterranean Coif 
			ShamanEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "ShamanEpicHelm");
			if (ShamanEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "ShamanEpicHelm";
				i.Name = "Subterranean Coif";
				i.Level = 50;
				i.Model = 63; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Mending, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicHelm = (HeadArmorTemplate)i;
			}

			//Subterranean Gloves 
			ShamanEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "ShamanEpicGloves");
			if (ShamanEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "ShamanEpicGloves";
				i.Name = "Subterranean Gloves";
				i.Level = 50;
				i.Model = 769;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Subterranean, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicGloves = (HandsArmorTemplate)i;
			}

			//Subterranean Hauberk 
			ShamanEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "ShamanEpicVest");
			if (ShamanEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "ShamanEpicVest";
				i.Name = "Subterranean Hauberk";
				i.Level = 50;
				i.Model = 766;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 10));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicVest = (TorsoArmorTemplate)i;
			}

			//Subterranean Legs 
			ShamanEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "ShamanEpicLegs");
			if (ShamanEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "ShamanEpicLegs";
				i.Name = "Subterranean Legs";
				i.Level = 50;
				i.Model = 767;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Cold, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Spirit, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicLegs = (LegsArmorTemplate)i;
			}

			//Subterranean Sleeves 
			ShamanEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "ShamanEpicArms");
			if (ShamanEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "ShamanEpicArms";
				i.Name = "Subterranean Sleeves";
				i.Level = 50;
				i.Model = 768;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 100;
				i.ArmorLevel = eArmorLevel.High;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Shaman);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Augmentation, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 18));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Energy, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ShamanEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			HealerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.HealerTrainer), eRealm.Midgard);
			ShamanTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.ShamanTrainer), eRealm.Midgard);

			foreach (GameNPC npc in HealerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in ShamanTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.AddHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));
			GameEventMgr.AddHandler(Loken, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Inaksha the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Inaksha, typeof(Seer_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Inaksha == null || Miri == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.RemoveHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));
			GameEventMgr.RemoveHandler(Loken, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we remove to Inaksha the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Inaksha, typeof(Seer_50Descriptor));
		}

		protected static void TalkToInaksha(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Seer_50), player, Inaksha) <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof(Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Inaksha.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Inaksha.SayTo(player, "Midgard needs your [services]");
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
							player.Out.SendCustomDialog("Will you help Inaksha [Seer Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "dead":
							if (quest.Step == 3)
							{
								Inaksha.SayTo(player, "Take this sealed pouch to Miri in Jordheim for your reward!");
								quest.GiveItemToPlayer(Inaksha, sealed_pouch);
								quest.Step = 4;
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}

			}

		}

		protected static void TalkToMiri(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Seer_50), player, Inaksha) <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof(Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Miri.SayTo(player, "Check your journal for instructions!");
				}
				else
				{
					Miri.SayTo(player, "I need your help to seek out loken in raumarik Loc 47k, 25k, 4k, and kill him ");
				}
			}

		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Seer_50 quest = player.IsDoingQuest(typeof(Seer_50)) as Seer_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Seer_50), player, Inaksha) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Seer_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Seer_50), player, Inaksha))
					return;

				player.Out.SendMessage("Good now go kill him!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Desire of a God (Level 50 Seer Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Loken in Raumarik Loc 47k, 25k kill him!";
					case 2:
						return "[Step #2] Return to Inaksha and give her the Ball of Flame!";
					case 3:
						return "[Step #3] Talk with Inaksha about Loken’s demise!";
					case 4:
						return "[Step #4] Go to Miri in Jordheim and give her the Sealed Pouch for your reward!";
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
			Seer_50Descriptor a = new Seer_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Seer_50)) == null) return;

			npc.SayTo(player, "Inaksha has an important task for you, please seek her out in Haggerfel");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Seer_50 quest = (Seer_50)player.IsDoingQuest(typeof(Seer_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				player.Out.SendMessage("You get a ball of flame", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				quest.GiveItemToPlayer(CreateQuestItem(ball_of_flame, quest.Name));
				quest.Step = 2;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Seer_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Loken.Name)
				{
					m_questPlayer.Out.SendMessage("You get a ball of flame", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItemToPlayer(CreateQuestItem(ball_of_flame, Name));
					Step = 2;
					return;
				}
			}
			 */

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Inaksha.Name && gArgs.Item.Name == ball_of_flame.Name)
				{
					RemoveItemFromPlayer(Inaksha, ball_of_flame);
					Inaksha.SayTo(player, "So it seems Logan's [dead]");
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Miri.Name && gArgs.Item.Name == sealed_pouch.Name)
				{
					RemoveItemFromPlayer(Miri, sealed_pouch);
					Miri.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(sealed_pouch);
			RemoveItemFromPlayer(ball_of_flame);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shaman)
			{
				GiveItemToPlayer(ShamanEpicArms);
				GiveItemToPlayer(ShamanEpicBoots);
				GiveItemToPlayer(ShamanEpicGloves);
				GiveItemToPlayer(ShamanEpicHelm);
				GiveItemToPlayer(ShamanEpicLegs);
				GiveItemToPlayer(ShamanEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Healer)
			{
				GiveItemToPlayer(HealerEpicArms);
				GiveItemToPlayer(HealerEpicBoots);
				GiveItemToPlayer(HealerEpicGloves);
				GiveItemToPlayer(HealerEpicHelm);
				GiveItemToPlayer(HealerEpicLegs);
				GiveItemToPlayer(HealerEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Inaksha
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Inaksha 
        *#28 give her the ball of flame
        *#29 talk with Inaksha about Loken’s demise
        *#30 go to Miri in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Valhalla Touched Boots 
            *Valhalla Touched Coif
            *Valhalla Touched Gloves
            *Valhalla Touched Hauberk
            *Valhalla Touched Legs
            *Valhalla Touched Sleeves
            *Subterranean Boots
            *Subterranean Coif
            *Subterranean Gloves
            *Subterranean Hauberk
            *Subterranean Legs
            *Subterranean Sleeves
        */

		#endregion
	}
}
