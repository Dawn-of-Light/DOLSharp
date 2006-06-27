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
*               : http://daoc.warcry.com/quests/display_spoilquest.php?id=741
*Date           : 22 November 2004
*Quest Name     : Saving the Clan (Level 50)
*Quest Classes  : Runemaster, Bonedancer, Spiritmaster (Mystic)
*Quest Version  : v1.2
*
*Changes:
*   Fixed the texts to be like live.
*   The epic armor should now be described correctly
*   The epic armor should now fit into the correct slots
*   The epic armor should now have the correct durability and condition
*   The armour will now be correctly rewarded with all pieces
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*
*ToDo:
*   Find Helm ModelID for epics..
*   checks for all other epics done, once they are implemented
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
	public class Mystic_50Descriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Mystic_50); }
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
			if (player.IsDoingQuest(typeof(Mystic_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Spiritmaster &&
				player.CharacterClass.ID != (byte)eCharacterClass.Runemaster &&
				player.CharacterClass.ID != (byte)eCharacterClass.Bonedancer &&
				player.CharacterClass.ID != (byte)eCharacterClass.Warlock)
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
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(Mystic_50), ExtendsType = typeof(AbstractQuest))]
	public class Mystic_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Saving the Clan";

		private static GameMob Danica = null; // Start NPC
		private static GameMob Kelic = null; // Mob to kill

		private static GenericItemTemplate kelics_totem = null;

		private static FeetArmorTemplate SpiritmasterEpicBoots = null;
		private static HeadArmorTemplate SpiritmasterEpicHelm = null;
		private static HandsArmorTemplate SpiritmasterEpicGloves = null;
		private static LegsArmorTemplate SpiritmasterEpicLegs = null;
		private static ArmsArmorTemplate SpiritmasterEpicArms = null;
		private static TorsoArmorTemplate SpiritmasterEpicVest = null;

		private static FeetArmorTemplate RunemasterEpicBoots = null;
		private static HeadArmorTemplate RunemasterEpicHelm = null;
		private static HandsArmorTemplate RunemasterEpicGloves = null;
		private static LegsArmorTemplate RunemasterEpicLegs = null;
		private static ArmsArmorTemplate RunemasterEpicArms = null;
		private static TorsoArmorTemplate RunemasterEpicVest = null;

		private static FeetArmorTemplate BonedancerEpicBoots = null;
		private static HeadArmorTemplate BonedancerEpicHelm = null;
		private static HandsArmorTemplate BonedancerEpicGloves = null;
		private static LegsArmorTemplate BonedancerEpicLegs = null;
		private static ArmsArmorTemplate BonedancerEpicArms = null;
		private static TorsoArmorTemplate BonedancerEpicVest = null;

		private static FeetArmorTemplate WarlockEpicBoots = null;
		private static HeadArmorTemplate WarlockEpicHelm = null;
		private static HandsArmorTemplate WarlockEpicGloves = null;
		private static LegsArmorTemplate WarlockEpicLegs = null;
		private static ArmsArmorTemplate WarlockEpicArms = null;
		private static TorsoArmorTemplate WarlockEpicVest = null;

		private static GameNPC[] SpiritmasterTrainers = null;
		private static GameNPC[] RunemasterTrainers = null;
		private static GameNPC[] BonedancerTrainers = null;
		private static GameNPC[] WarlockTrainers = null;


		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Danica", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Danica , creating it ...");
				Danica = new GameMob();
				Danica.Model = 227;
				Danica.Name = "Danica";
				Danica.GuildName = "";
				Danica.Realm = (byte)eRealm.Midgard;
				Danica.RegionId = 100;
				Danica.Size = 51;
				Danica.Level = 50;
				Danica.Position = new Point(804440, 722267, 4719);
				Danica.Heading = 2116;
				Danica.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Danica.SaveIntoDatabase();
				}
			}
			else
				Danica = npcs[0] as GameMob;
			// end npc

			npcs = WorldMgr.GetNPCsByName("Kelic", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic , creating it ...");
				Kelic = new GameMob();
				Kelic.Model = 26;
				Kelic.Name = "Kelic";
				Kelic.GuildName = "";
				Kelic.Realm = (byte)eRealm.None;
				Kelic.RegionId = 100;
				Kelic.Size = 100;
				Kelic.Level = 65;
				Kelic.Position = new Point(621577, 745848, 4593);
				Kelic.Heading = 3538;
				Kelic.Flags = 1;
				Kelic.MaxSpeedBase = 200;
				Kelic.RespawnInterval = 5 * 60 * 1000;
				Kelic.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Kelic.SaveIntoDatabase();
				}
			}
			else
				Kelic = npcs[0] as GameMob;
			// end npc

			#endregion

			#region defineItems

			kelics_totem = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "kelics_totem");
			if (kelics_totem == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic's Totem , creating it ...");
				kelics_totem = new GenericItemTemplate();
				kelics_totem.ItemTemplateID = "kelics_totem";
				kelics_totem.Name = "Kelic's Totem";
				kelics_totem.Level = 8;
				kelics_totem.Model = 488;
				kelics_totem.IsDropable = false;
				kelics_totem.IsSaleable = false;
				kelics_totem.IsTradable = false;
				kelics_totem.Weight = 12;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(kelics_totem);
				}
			}
			ArmorTemplate i = null;
			#region Spiritmaster
			SpiritmasterEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "SpiritmasterEpicBoots");
			if (SpiritmasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicBoots";
				i.Name = "Spirit Touched Boots";
				i.Level = 50;
				i.Model = 803;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat , 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicBoots = (FeetArmorTemplate)i;
			}

			SpiritmasterEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "SpiritmasterEpicHelm");
			if (SpiritmasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicHelm";
				i.Name = "Spirit Touched Cap";
				i.Level = 50;
				i.Model = 825; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Darkness, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Suppression, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicHelm = (HeadArmorTemplate)i;
			}

			SpiritmasterEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "SpiritmasterEpicGloves");
			if (SpiritmasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicGloves";
				i.Name = "Spirit Touched Gloves ";
				i.Level = 50;
				i.Model = 802;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Summoning, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicGloves = (HandsArmorTemplate)i;
			}

			SpiritmasterEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "SpiritmasterEpicVest");
			if (SpiritmasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicVest";
				i.Name = "Spirit Touched Vest";
				i.Level = 50;
				i.Model = 799;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicVest = (TorsoArmorTemplate)i;
			}

			SpiritmasterEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "SpiritmasterEpicLegs");
			if (SpiritmasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicLegs";
				i.Name = "Spirit Touched Pants";
				i.Level = 50;
				i.Model = 800;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicLegs = (LegsArmorTemplate)i;
			}

			SpiritmasterEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "SpiritmasterEpicArms");
			if (SpiritmasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "SpiritmasterEpicArms";
				i.Name = "Spirit Touched Sleeves";
				i.Level = 50;
				i.Model = 801;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Spiritmaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				SpiritmasterEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Runemaster
			RunemasterEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "RunemasterEpicBoots");
			if (RunemasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicBoots";
				i.Name = "Raven-Rune Boots";
				i.Level = 50;
				i.Model = 707;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicBoots = (FeetArmorTemplate)i;
			}

			RunemasterEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "RunemasterEpicHelm");
			if (RunemasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicHelm";
				i.Name = "Raven-Rune Cap";
				i.Level = 50;
				i.Model = 825; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Darkness, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Suppression, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicHelm = (HeadArmorTemplate)i;
			}

			RunemasterEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "RunemasterEpicGloves");
			if (RunemasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicGloves";
				i.Name = "Raven-Rune Gloves ";
				i.Level = 50;
				i.Model = 706;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Summoning, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicGloves = (HandsArmorTemplate)i;
			}

			RunemasterEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "RunemasterEpicVest");
			if (RunemasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicVest";
				i.Name = "Raven-Rune Vest";
				i.Level = 50;
				i.Model = 703;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicVest = (TorsoArmorTemplate)i;
			}

			RunemasterEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "RunemasterEpicLegs");
			if (RunemasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicLegs";
				i.Name = "Raven-Rune Pants";
				i.Level = 50;
				i.Model = 704;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicLegs = (LegsArmorTemplate)i;
			}

			RunemasterEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "RunemasterEpicArms");
			if (RunemasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "RunemasterEpicArms";
				i.Name = "Raven-Rune Sleeves";
				i.Level = 50;
				i.Model = 705;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Runemaster);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				RunemasterEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Bonedancer
			BonedancerEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "BonedancerEpicBoots");
			if (BonedancerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "BonedancerEpicBoots";
				i.Name = "Raven-Boned Boots";
				i.Level = 50;
				i.Model = 1190;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicBoots = (FeetArmorTemplate)i;
			}

			BonedancerEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "BonedancerEpicHelm");
			if (BonedancerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "BonedancerEpicHelm";
				i.Name = "Raven-Boned Cap";
				i.Level = 50;
				i.Model = 825; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Suppression, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_BoneArmy, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicHelm = (HeadArmorTemplate)i;
			}

			BonedancerEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "BonedancerEpicGloves");
			if (BonedancerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "BonedancerEpicGloves";
				i.Name = "Raven-Boned Gloves ";
				i.Level = 50;
				i.Model = 1191;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Darkness, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 6));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicGloves = (HandsArmorTemplate)i;
			}

			BonedancerEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "BonedancerEpicVest");
			if (BonedancerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "BonedancerEpicVest";
				i.Name = "Raven-Boned Vest";
				i.Level = 50;
				i.Model = 1187;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicVest = (TorsoArmorTemplate)i;
			}

			BonedancerEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "BonedancerEpicLegs");
			if (BonedancerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Legs , creating it ...");
				BonedancerEpicLegs = new LegsArmorTemplate();
				BonedancerEpicLegs.ItemTemplateID = "BonedancerEpicLegs";
				BonedancerEpicLegs.Name = "Raven-Boned Pants";
				BonedancerEpicLegs.Level = 50;
				BonedancerEpicLegs.Model = 1188;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicLegs = (LegsArmorTemplate)i;
			}

			BonedancerEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "BonedancerEpicArms");
			if (BonedancerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "BonedancerEpicArms";
				i.Name = "Raven-Boned Sleeves";
				i.Level = 50;
				i.Model = 1189;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Bonedancer);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				BonedancerEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#region Warlock
			WarlockEpicBoots = (FeetArmorTemplate)GameServer.Database.FindObjectByKey(typeof(FeetArmorTemplate), "WarlockEpicBoots");
			if (WarlockEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Boots , creating it ...");
				i = new FeetArmorTemplate();
				i.ItemTemplateID = "WarlockEpicBoots";
				i.Name = "Bewitched Soothsayer Boots";
				i.Level = 50;
				i.Model = 2937;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 16 pts
				 *   Matter Resist: 8%
				 *   Hits: 48 pts
				 *   Heat Resist: 10%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 16));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Matter, 8));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 48));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 10));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicBoots = (FeetArmorTemplate)i;
			}

			WarlockEpicHelm = (HeadArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HeadArmorTemplate), "WarlockEpicHelm");
			if (WarlockEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Helm , creating it ...");
				i = new HeadArmorTemplate();
				i.ItemTemplateID = "WarlockEpicHelm";
				i.Name = "Bewitched Soothsayer Cap";
				i.Level = 50;
				i.Model = 825; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Piety: 13 pts
				 *   Power: 4 pts
				 *   Cursing: +4 pts
				 *   Hexing: +4 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Cursing, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Hexing, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicHelm = (HeadArmorTemplate)i;
			}

			WarlockEpicGloves = (HandsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(HandsArmorTemplate), "WarlockEpicGloves");
			if (WarlockEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Gloves , creating it ...");
				i = new HandsArmorTemplate();
				i.ItemTemplateID = "WarlockEpicGloves";
				i.Name = "Bewitched Soothsayer Gloves ";
				i.Level = 50;
				i.Model = 2936;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 13 pts
				 *   Piety: 12 pts
				 *   Power: 4 pts
				 *   Hexing: +4 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 4));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Hexing, 4));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicGloves = (HandsArmorTemplate)i;
			}

			WarlockEpicVest = (TorsoArmorTemplate)GameServer.Database.FindObjectByKey(typeof(TorsoArmorTemplate), "WarlockEpicVest");
			if (WarlockEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Vest , creating it ...");
				i = new TorsoArmorTemplate();
				i.ItemTemplateID = "WarlockEpicVest";
				i.Name = "Bewitched Soothsayer Vest";
				i.Level = 50;
				i.Model = 2933;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 12 pts
				 *   Piety: 13 pts
				 *   Slash Resist: 12%
				 *   Hits: 24 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicVest = (TorsoArmorTemplate)i;
			}

			WarlockEpicLegs = (LegsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(LegsArmorTemplate), "WarlockEpicLegs");
			if (WarlockEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Legs , creating it ...");
				i = new LegsArmorTemplate();
				i.ItemTemplateID = "WarlockEpicLegs";
				i.Name = "Bewitched Soothsayer Pants";
				i.Level = 50;
				i.Model = 2934;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Constitution: 13 pts
				 *   Piety: 13 pts
				 *   Crush Resist: 12%
				 *   Hits: 24 pts
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 13));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 24));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicLegs = (LegsArmorTemplate)i;
			}

			WarlockEpicArms = (ArmsArmorTemplate)GameServer.Database.FindObjectByKey(typeof(ArmsArmorTemplate), "WarlockEpicArms");
			if (WarlockEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Arms , creating it ...");
				i = new ArmsArmorTemplate();
				i.ItemTemplateID = "WarlockEpicArms";
				i.Name = "Bewitched Soothsayer Sleeves";
				i.Level = 50;
				i.Model = 1189;
				i.IsDropable = true;
				i.IsSaleable = false;
				i.IsTradable = true;
				i.ArmorFactor = 50;
				i.ArmorLevel = eArmorLevel.VeryLow;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.AllowedClass.Add(eCharacterClass.Warlock);
				i.MaterialLevel = eMaterialLevel.Arcanium;
				i.Realm = eRealm.Midgard;

				/*
				 *   Piety: 9 pts
				 *   Thrust Resist: 6%
				 *   Power: 12 pts
				 *   Heat Resist: 8%
				 */

				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Piety, 9));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 6));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxMana, 12));
				i.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Heat, 8));

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				WarlockEpicArms = (ArmsArmorTemplate)i;
			}
			#endregion
			#endregion

			SpiritmasterTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.SpiritmasterTrainer), eRealm.Midgard);
			RunemasterTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.RunemasterTrainer), eRealm.Midgard);
			BonedancerTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.BonedancerTrainer), eRealm.Midgard);
			WarlockTrainers = WorldMgr.GetNPCsByType(typeof(DOL.GS.Trainer.WarlockTrainer), eRealm.Midgard);

			foreach (GameNPC npc in SpiritmasterTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in RunemasterTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BonedancerTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in WarlockTrainers)
				GameEventMgr.AddHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			GameEventMgr.AddHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.AddHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));
			GameEventMgr.AddHandler(Kelic, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			/* Now we bring to Danica the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(Danica, typeof(Mystic_50Descriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Danica == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.RemoveHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));
			GameEventMgr.RemoveHandler(Kelic, GameNPCEvent.Dying, new DOLEventHandler(TargetDying));

			foreach (GameNPC npc in SpiritmasterTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in RunemasterTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in BonedancerTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));
			foreach (GameNPC npc in WarlockTrainers)
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.Interact, new DOLEventHandler(TalkToTrainer));

			/* Now we remove to Danica the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(Danica, typeof(Mystic_50Descriptor));
		}

		protected static void TalkToDanica(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(Mystic_50), player, Danica) <= 0)
				return;

			//We also check if the player is already doing the quest
			Mystic_50 quest = player.IsDoingQuest(typeof(Mystic_50)) as Mystic_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 1:
							Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
								"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
								"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
								"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
								"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
								"Return to me when you have the totem. May all the gods be with you.");
							break;
						case 2:
							Danica.SayTo(player, "It is good to see you were strong enough to survive Kelic. I can sense you have the controlling totem on you. Give me Kelic's totem now! Hurry!");
							quest.Step = 3;
							break;
						case 3:
							Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
							break;
					}
				}
				else
				{
					Danica.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Kelic to dispose of him. He also has a note here about how strong Kelic really was. That [worries me].");
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
							Danica.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Kelic] and his minions.");
							break;
						case "face Kelic":
							player.Out.SendCustomDialog("Will you face Kelic [Mystic Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 3)
								quest.FinishQuest();
							break;

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
			Mystic_50 quest = player.IsDoingQuest(typeof(Mystic_50)) as Mystic_50;

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
			if (QuestMgr.CanGiveQuest(typeof(Mystic_50), player, Danica) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Mystic_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(Mystic_50), player, Danica))
					return;

				Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
					"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
					"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
					"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
					"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
					"Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Saving the Clan (Level 50 Mystic Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. Cross the river and head west. Follow the snowline until you reach a group of trees.";
					case 2:
						return "[Step #2] Return to Danica and give her the totem!";
					case 3:
						return "[Step #3] Tell Danica you can 'take them' for your rewards!";
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
			Mystic_50Descriptor a = new Mystic_50Descriptor();

			if (!a.CheckQuestQualification(player)) return;
			if (player.IsDoingQuest(typeof(Mystic_50)) == null) return;

			npc.SayTo(player, "Danica has an important task for you, please seek her out in Mularn");
		}

		protected static void TargetDying(DOLEvent e, object sender, EventArgs args)
		{
			GameMob mob = sender as GameMob;
			foreach (GamePlayer player in mob.XPGainers)
			{
				Mystic_50 quest = (Mystic_50)player.IsDoingQuest(typeof(Mystic_50));
				if (quest == null) continue;
				if (quest.Step != 1) continue;
				quest.Step = 2;
				quest.GiveItemToPlayer(CreateQuestItem(kelics_totem, quest.Name));
				player.Out.SendMessage("Kelic drops his Totem and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Mystic_50)) == null)
				return;
			/*
			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Kelic.Name)
				{
					Step = 2;
					GiveItemToPlayer(CreateQuestItem(kelics_totem, Name));
					m_questPlayer.Out.SendMessage("Kelic drops his Totem and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
*/
			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Danica.Name && gArgs.Item.Name == kelics_totem.Name)
				{
					RemoveItemFromPlayer(Danica, kelics_totem);
					Danica.SayTo(player, "Ah, I can see how he wore the curse around the totem. I can now break the curse that is destroying the clan!");
					Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 3;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItemFromPlayer(kelics_totem);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Spiritmaster)
			{
				GiveItemToPlayer(SpiritmasterEpicArms);
				GiveItemToPlayer(SpiritmasterEpicBoots);
				GiveItemToPlayer(SpiritmasterEpicGloves);
				GiveItemToPlayer(SpiritmasterEpicHelm);
				GiveItemToPlayer(SpiritmasterEpicLegs);
				GiveItemToPlayer(SpiritmasterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Runemaster)
			{
				GiveItemToPlayer(RunemasterEpicArms);
				GiveItemToPlayer(RunemasterEpicBoots);
				GiveItemToPlayer(RunemasterEpicGloves);
				GiveItemToPlayer(RunemasterEpicHelm);
				GiveItemToPlayer(RunemasterEpicLegs);
				GiveItemToPlayer(RunemasterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bonedancer)
			{
				GiveItemToPlayer(BonedancerEpicArms);
				GiveItemToPlayer(BonedancerEpicBoots);
				GiveItemToPlayer(BonedancerEpicGloves);
				GiveItemToPlayer(BonedancerEpicHelm);
				GiveItemToPlayer(BonedancerEpicLegs);
				GiveItemToPlayer(BonedancerEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warlock)
			{
				GiveItemToPlayer(WarlockEpicArms);
				GiveItemToPlayer(WarlockEpicBoots);
				GiveItemToPlayer(WarlockEpicGloves);
				GiveItemToPlayer(WarlockEpicHelm);
				GiveItemToPlayer(WarlockEpicLegs);
				GiveItemToPlayer(WarlockEpicVest);
			}

			Danica.SayTo(m_questPlayer, "May it serve you well, knowing that you have helped preserve the history of Midgard!");

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
Spirit Touched Boots 
Spirit Touched Cap 
Spirit Touched Gloves 
Spirit Touched Pants 
Spirit Touched Sleeves 
Spirit Touched Vest 
Raven-Rune Boots 
Raven-Rune Cap 
Raven-Rune Gloves 
Raven-Rune Pants 
Raven-Rune Sleeves 
Raven-Rune Vest 
Raven-boned Boots 
Raven-Boned Cap 
Raven-boned Gloves 
Raven-Boned Pants 
Raven-Boned Sleeves 
Bone-rune Vest 
        */

		#endregion
	}
}
