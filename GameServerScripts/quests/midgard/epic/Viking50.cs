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
*Quest Name     : An End to the Daggers (level 50)
*Quest Classes  : Warrior, Berserker, Thane, Skald, Savage (Viking)
*Quest Version  : v1
*
*Done:
*Bonuses to epic items
*
*ToDo:   
*   Find Helm ModelID for epics..
*   checks for all other epics done
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Viking_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "An End to the Daggers";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Lynnleigh = null; // Start NPC
		private static GameNPC Ydenia = null; // Mob to kill
		private static GameNPC Elizabeth = null; // reward NPC

		private static ItemTemplate tome_enchantments = null;
		private static ItemTemplate sealed_pouch = null;
		private static ItemTemplate WarriorEpicBoots = null;
		private static ItemTemplate WarriorEpicHelm = null;
		private static ItemTemplate WarriorEpicGloves = null;
		private static ItemTemplate WarriorEpicLegs = null;
		private static ItemTemplate WarriorEpicArms = null;
		private static ItemTemplate WarriorEpicVest = null;
		private static ItemTemplate BerserkerEpicBoots = null;
		private static ItemTemplate BerserkerEpicHelm = null;
		private static ItemTemplate BerserkerEpicGloves = null;
		private static ItemTemplate BerserkerEpicLegs = null;
		private static ItemTemplate BerserkerEpicArms = null;
		private static ItemTemplate BerserkerEpicVest = null;
		private static ItemTemplate ThaneEpicBoots = null;
		private static ItemTemplate ThaneEpicHelm = null;
		private static ItemTemplate ThaneEpicGloves = null;
		private static ItemTemplate ThaneEpicLegs = null;
		private static ItemTemplate ThaneEpicArms = null;
		private static ItemTemplate ThaneEpicVest = null;
		private static ItemTemplate SkaldEpicBoots = null;
		private static ItemTemplate SkaldEpicHelm = null;
		private static ItemTemplate SkaldEpicGloves = null;
		private static ItemTemplate SkaldEpicVest = null;
		private static ItemTemplate SkaldEpicLegs = null;
		private static ItemTemplate SkaldEpicArms = null;
		private static ItemTemplate SavageEpicBoots = null;
		private static ItemTemplate SavageEpicHelm = null;
		private static ItemTemplate SavageEpicGloves = null;
		private static ItemTemplate SavageEpicVest = null;
		private static ItemTemplate SavageEpicLegs = null;
		private static ItemTemplate SavageEpicArms = null;

		// Constructors
		public Viking_50() : base()
		{
		}

		public Viking_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Viking_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Viking_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lynnleigh", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lynnleigh , creating it ...");
				Lynnleigh = new GameNPC();
				Lynnleigh.Model = 217;
				Lynnleigh.Name = "Lynnleigh";
				Lynnleigh.GuildName = "";
				Lynnleigh.Realm = (byte) eRealm.Midgard;
				Lynnleigh.CurrentRegionID = 100;
				Lynnleigh.Size = 51;
				Lynnleigh.Level = 50;
				Lynnleigh.X = 760085;
				Lynnleigh.Y = 758453;
				Lynnleigh.Z = 4736;
				Lynnleigh.Heading = 2197;
				Lynnleigh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lynnleigh.SaveIntoDatabase();
				}
			}
			else
				Lynnleigh = npcs[0];
			// end npc
			npcs = WorldMgr.GetNPCsByName("Elizabeth", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Elizabeth , creating it ...");
				Elizabeth = new GameNPC();
				Elizabeth.Model = 217;
				Elizabeth.Name = "Elizabeth";
				Elizabeth.GuildName = "Enchanter";
				Elizabeth.Realm = (byte) eRealm.Midgard;
				Elizabeth.CurrentRegionID = 100;
				Elizabeth.Size = 51;
				Elizabeth.Level = 41;
				Elizabeth.X = 802849;
				Elizabeth.Y = 727081;
				Elizabeth.Z = 4681;
				Elizabeth.Heading = 2480;
				Elizabeth.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Elizabeth.SaveIntoDatabase();
				}

			}
			else
				Elizabeth = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Ydenia of Seithkona", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ydenia , creating it ...");
				Ydenia = new GameNPC();
				Ydenia.Model = 217;
				Ydenia.Name = "Ydenia of Seithkona";
				Ydenia.GuildName = "";
				Ydenia.Realm = (byte) eRealm.None;
				Ydenia.CurrentRegionID = 100;
				Ydenia.Size = 100;
				Ydenia.Level = 65;
				Ydenia.X = 637680;
				Ydenia.Y = 767189;
				Ydenia.Z = 4480;
				Ydenia.Heading = 2156;
				Ydenia.Flags ^= (uint)GameNPC.eFlags.TRANSPARENT;
				Ydenia.MaxSpeedBase = 200;
				Ydenia.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ydenia.SaveIntoDatabase();
				}

			}
			else
				Ydenia = npcs[0];
			// end npc

			#endregion

			#region defineItems

			tome_enchantments = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "tome_enchantments");
			if (tome_enchantments == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Tome of Enchantments , creating it ...");
				tome_enchantments = new ItemTemplate();
				tome_enchantments.Id_nb = "tome_enchantments";
				tome_enchantments.Name = "Tome of Enchantments";
				tome_enchantments.Level = 8;
				tome_enchantments.Item_Type = 0;
				tome_enchantments.Model = 500;
				tome_enchantments.IsDropable = false;
				tome_enchantments.IsPickable = false;
				tome_enchantments.DPS_AF = 0;
				tome_enchantments.SPD_ABS = 0;
				tome_enchantments.Object_Type = 0;
				tome_enchantments.Hand = 0;
				tome_enchantments.Type_Damage = 0;
				tome_enchantments.Quality = 100;
				tome_enchantments.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(tome_enchantments);
				}

			}

			sealed_pouch = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sealed_pouch");
			if (sealed_pouch == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sealed Pouch , creating it ...");
				sealed_pouch = new ItemTemplate();
				sealed_pouch.Id_nb = "sealed_pouch";
				sealed_pouch.Name = "Sealed Pouch";
				sealed_pouch.Level = 8;
				sealed_pouch.Item_Type = 29;
				sealed_pouch.Model = 488;
				sealed_pouch.IsDropable = false;
				sealed_pouch.IsPickable = false;
				sealed_pouch.DPS_AF = 0;
				sealed_pouch.SPD_ABS = 0;
				sealed_pouch.Object_Type = 41;
				sealed_pouch.Hand = 0;
				sealed_pouch.Type_Damage = 0;
				sealed_pouch.Quality = 100;
				sealed_pouch.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(sealed_pouch);
				}
			}

			WarriorEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicBoots");
			if (WarriorEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Boots , creating it ...");
				WarriorEpicBoots = new ItemTemplate();
				WarriorEpicBoots.Id_nb = "WarriorEpicBoots";
				WarriorEpicBoots.Name = "Tyr's Might Boots";
				WarriorEpicBoots.Level = 50;
				WarriorEpicBoots.Item_Type = 23;
				WarriorEpicBoots.Model = 780;
				WarriorEpicBoots.IsDropable = true;
				WarriorEpicBoots.IsPickable = true;
				WarriorEpicBoots.DPS_AF = 100;
				WarriorEpicBoots.SPD_ABS = 27;
				WarriorEpicBoots.Object_Type = 35;
				WarriorEpicBoots.Quality = 100;
				WarriorEpicBoots.Weight = 22;
				WarriorEpicBoots.Bonus = 35;
				WarriorEpicBoots.MaxCondition = 50000;
				WarriorEpicBoots.MaxDurability = 50000;
				WarriorEpicBoots.Durability = 50000;
				WarriorEpicBoots.Condition = 50000;

				WarriorEpicBoots.Bonus1 = 16;
				WarriorEpicBoots.Bonus1Type = (int) eStat.CON;

				WarriorEpicBoots.Bonus2 = 15;
				WarriorEpicBoots.Bonus2Type = (int) eStat.QUI;

				WarriorEpicBoots.Bonus3 = 10;
				WarriorEpicBoots.Bonus3Type = (int) eResist.Heat;

				WarriorEpicBoots.Bonus4 = 10;
				WarriorEpicBoots.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicBoots);
				}

			}
//end item
			WarriorEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicHelm");
			if (WarriorEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Helm , creating it ...");
				WarriorEpicHelm = new ItemTemplate();
				WarriorEpicHelm.Id_nb = "WarriorEpicHelm";
				WarriorEpicHelm.Name = "Tyr's Might Coif";
				WarriorEpicHelm.Level = 50;
				WarriorEpicHelm.Item_Type = 21;
				WarriorEpicHelm.Model = 832; //NEED TO WORK ON..
				WarriorEpicHelm.IsDropable = true;
				WarriorEpicHelm.IsPickable = true;
				WarriorEpicHelm.DPS_AF = 100;
				WarriorEpicHelm.SPD_ABS = 27;
				WarriorEpicHelm.Object_Type = 35;
				WarriorEpicHelm.Quality = 100;
				WarriorEpicHelm.Weight = 22;
				WarriorEpicHelm.Bonus = 35;
				WarriorEpicHelm.MaxCondition = 50000;
				WarriorEpicHelm.MaxDurability = 50000;
				WarriorEpicHelm.Condition = 50000;
				WarriorEpicHelm.Durability = 50000;

				WarriorEpicHelm.Bonus1 = 12;
				WarriorEpicHelm.Bonus1Type = (int) eStat.STR;

				WarriorEpicHelm.Bonus2 = 12;
				WarriorEpicHelm.Bonus2Type = (int) eStat.CON;

				WarriorEpicHelm.Bonus3 = 12;
				WarriorEpicHelm.Bonus3Type = (int) eStat.DEX;

				WarriorEpicHelm.Bonus4 = 11;
				WarriorEpicHelm.Bonus4Type = (int) eResist.Crush;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicHelm);
				}

			}
//end item
			WarriorEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicGloves");
			if (WarriorEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Gloves , creating it ...");
				WarriorEpicGloves = new ItemTemplate();
				WarriorEpicGloves.Id_nb = "WarriorEpicGloves";
				WarriorEpicGloves.Name = "Tyr's Might Gloves";
				WarriorEpicGloves.Level = 50;
				WarriorEpicGloves.Item_Type = 22;
				WarriorEpicGloves.Model = 779;
				WarriorEpicGloves.IsDropable = true;
				WarriorEpicGloves.IsPickable = true;
				WarriorEpicGloves.DPS_AF = 100;
				WarriorEpicGloves.SPD_ABS = 27;
				WarriorEpicGloves.Object_Type = 35;
				WarriorEpicGloves.Quality = 100;
				WarriorEpicGloves.Weight = 22;
				WarriorEpicGloves.Bonus = 35;
				WarriorEpicGloves.MaxCondition = 50000;
				WarriorEpicGloves.MaxDurability = 50000;
				WarriorEpicGloves.Condition = 50000;
				WarriorEpicGloves.Durability = 50000;

				WarriorEpicGloves.Bonus1 = 3;
				WarriorEpicGloves.Bonus1Type = (int) eProperty.Skill_Shields;

				WarriorEpicGloves.Bonus2 = 3;
				WarriorEpicGloves.Bonus2Type = (int) eProperty.Skill_Parry;

				WarriorEpicGloves.Bonus3 = 15;
				WarriorEpicGloves.Bonus3Type = (int) eStat.STR;

				WarriorEpicGloves.Bonus4 = 13;
				WarriorEpicGloves.Bonus4Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicGloves);
				}

			}

			WarriorEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicVest");
			if (WarriorEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Vest , creating it ...");
				WarriorEpicVest = new ItemTemplate();
				WarriorEpicVest.Id_nb = "WarriorEpicVest";
				WarriorEpicVest.Name = "Tyr's Might Hauberk";
				WarriorEpicVest.Level = 50;
				WarriorEpicVest.Item_Type = 25;
				WarriorEpicVest.Model = 776;
				WarriorEpicVest.IsDropable = true;
				WarriorEpicVest.IsPickable = true;
				WarriorEpicVest.DPS_AF = 100;
				WarriorEpicVest.SPD_ABS = 27;
				WarriorEpicVest.Object_Type = 35;
				WarriorEpicVest.Quality = 100;
				WarriorEpicVest.Weight = 22;
				WarriorEpicVest.Bonus = 35;
				WarriorEpicVest.MaxCondition = 50000;
				WarriorEpicVest.MaxDurability = 50000;
				WarriorEpicVest.Condition = 50000;
				WarriorEpicVest.Durability = 50000;

				WarriorEpicVest.Bonus1 = 13;
				WarriorEpicVest.Bonus1Type = (int) eStat.STR;

				WarriorEpicVest.Bonus2 = 13;
				WarriorEpicVest.Bonus2Type = (int) eStat.DEX;

				WarriorEpicVest.Bonus3 = 6;
				WarriorEpicVest.Bonus3Type = (int) eResist.Matter;

				WarriorEpicVest.Bonus4 = 30;
				WarriorEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicVest);
				}

			}

			WarriorEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicLegs");
			if (WarriorEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Legs , creating it ...");
				WarriorEpicLegs = new ItemTemplate();
				WarriorEpicLegs.Id_nb = "WarriorEpicLegs";
				WarriorEpicLegs.Name = "Tyr's Might Legs";
				WarriorEpicLegs.Level = 50;
				WarriorEpicLegs.Item_Type = 27;
				WarriorEpicLegs.Model = 777;
				WarriorEpicLegs.IsDropable = true;
				WarriorEpicLegs.IsPickable = true;
				WarriorEpicLegs.DPS_AF = 100;
				WarriorEpicLegs.SPD_ABS = 27;
				WarriorEpicLegs.Object_Type = 35;
				WarriorEpicLegs.Quality = 100;
				WarriorEpicLegs.Weight = 22;
				WarriorEpicLegs.Bonus = 35;
				WarriorEpicLegs.MaxCondition = 50000;
				WarriorEpicLegs.MaxDurability = 50000;
				WarriorEpicLegs.Condition = 50000;
				WarriorEpicLegs.Durability = 50000;

				WarriorEpicLegs.Bonus1 = 22;
				WarriorEpicLegs.Bonus1Type = (int) eStat.CON;

				WarriorEpicLegs.Bonus2 = 15;
				WarriorEpicLegs.Bonus2Type = (int) eStat.STR;

				WarriorEpicLegs.Bonus3 = 8;
				WarriorEpicLegs.Bonus3Type = (int) eResist.Cold;

				WarriorEpicLegs.Bonus4 = 8;
				WarriorEpicLegs.Bonus4Type = (int) eResist.Body;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicLegs);
				}

			}

			WarriorEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "WarriorEpicArms");
			if (WarriorEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Arms , creating it ...");
				WarriorEpicArms = new ItemTemplate();
				WarriorEpicArms.Id_nb = "WarriorEpicArms";
				WarriorEpicArms.Name = "Tyr's Might Sleeves";
				WarriorEpicArms.Level = 50;
				WarriorEpicArms.Item_Type = 28;
				WarriorEpicArms.Model = 778;
				WarriorEpicArms.IsDropable = true;
				WarriorEpicArms.IsPickable = true;
				WarriorEpicArms.DPS_AF = 100;
				WarriorEpicArms.SPD_ABS = 27;
				WarriorEpicArms.Object_Type = 35;
				WarriorEpicArms.Quality = 100;
				WarriorEpicArms.Weight = 22;
				WarriorEpicArms.Bonus = 35;
				WarriorEpicArms.MaxCondition = 50000;
				WarriorEpicArms.MaxDurability = 50000;
				WarriorEpicArms.Condition = 50000;
				WarriorEpicArms.Durability = 50000;

				WarriorEpicArms.Bonus1 = 22;
				WarriorEpicArms.Bonus1Type = (int) eStat.DEX;

				WarriorEpicArms.Bonus2 = 15;
				WarriorEpicArms.Bonus2Type = (int) eStat.QUI;

				WarriorEpicArms.Bonus3 = 8;
				WarriorEpicArms.Bonus3Type = (int) eResist.Crush;

				WarriorEpicArms.Bonus4 = 8;
				WarriorEpicArms.Bonus4Type = (int) eResist.Slash;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(WarriorEpicArms);
				}

			}
			BerserkerEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicBoots");
			if (BerserkerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Boots , creating it ...");
				BerserkerEpicBoots = new ItemTemplate();
				BerserkerEpicBoots.Id_nb = "BerserkerEpicBoots";
				BerserkerEpicBoots.Name = "Courage Bound Boots";
				BerserkerEpicBoots.Level = 50;
				BerserkerEpicBoots.Item_Type = 23;
				BerserkerEpicBoots.Model = 755;
				BerserkerEpicBoots.IsDropable = true;
				BerserkerEpicBoots.IsPickable = true;
				BerserkerEpicBoots.DPS_AF = 100;
				BerserkerEpicBoots.SPD_ABS = 19;
				BerserkerEpicBoots.Object_Type = 34;
				BerserkerEpicBoots.Quality = 100;
				BerserkerEpicBoots.Weight = 22;
				BerserkerEpicBoots.Bonus = 35;
				BerserkerEpicBoots.MaxCondition = 50000;
				BerserkerEpicBoots.MaxDurability = 50000;
				BerserkerEpicBoots.Condition = 50000;
				BerserkerEpicBoots.Durability = 50000;

				BerserkerEpicBoots.Bonus1 = 19;
				BerserkerEpicBoots.Bonus1Type = (int) eStat.DEX;

				BerserkerEpicBoots.Bonus2 = 15;
				BerserkerEpicBoots.Bonus2Type = (int) eStat.QUI;

				BerserkerEpicBoots.Bonus3 = 8;
				BerserkerEpicBoots.Bonus3Type = (int) eResist.Spirit;

				BerserkerEpicBoots.Bonus4 = 8;
				BerserkerEpicBoots.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicBoots);
				}

			}
//end item
			BerserkerEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicHelm");
			if (BerserkerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Helm , creating it ...");
				BerserkerEpicHelm = new ItemTemplate();
				BerserkerEpicHelm.Id_nb = "BerserkerEpicHelm";
				BerserkerEpicHelm.Name = "Courage Bound Helm";
				BerserkerEpicHelm.Level = 50;
				BerserkerEpicHelm.Item_Type = 21;
				BerserkerEpicHelm.Model = 829; //NEED TO WORK ON..
				BerserkerEpicHelm.IsDropable = true;
				BerserkerEpicHelm.IsPickable = true;
				BerserkerEpicHelm.DPS_AF = 100;
				BerserkerEpicHelm.SPD_ABS = 19;
				BerserkerEpicHelm.Object_Type = 34;
				BerserkerEpicHelm.Quality = 100;
				BerserkerEpicHelm.Weight = 22;
				BerserkerEpicHelm.Bonus = 35;
				BerserkerEpicHelm.MaxCondition = 50000;
				BerserkerEpicHelm.MaxDurability = 50000;
				BerserkerEpicHelm.Condition = 50000;
				BerserkerEpicHelm.Durability = 50000;

				BerserkerEpicHelm.Bonus1 = 10;
				BerserkerEpicHelm.Bonus1Type = (int) eStat.STR;

				BerserkerEpicHelm.Bonus2 = 15;
				BerserkerEpicHelm.Bonus2Type = (int) eStat.CON;

				BerserkerEpicHelm.Bonus3 = 10;
				BerserkerEpicHelm.Bonus3Type = (int) eStat.DEX;

				BerserkerEpicHelm.Bonus4 = 10;
				BerserkerEpicHelm.Bonus4Type = (int) eStat.QUI;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicHelm);
				}
			}
//end item
			BerserkerEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicGloves");
			if (BerserkerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Gloves , creating it ...");
				BerserkerEpicGloves = new ItemTemplate();
				BerserkerEpicGloves.Id_nb = "BerserkerEpicGloves";
				BerserkerEpicGloves.Name = "Courage Bound Gloves";
				BerserkerEpicGloves.Level = 50;
				BerserkerEpicGloves.Item_Type = 22;
				BerserkerEpicGloves.Model = 754;
				BerserkerEpicGloves.IsDropable = true;
				BerserkerEpicGloves.IsPickable = true;
				BerserkerEpicGloves.DPS_AF = 100;
				BerserkerEpicGloves.SPD_ABS = 19;
				BerserkerEpicGloves.Object_Type = 34;
				BerserkerEpicGloves.Quality = 100;
				BerserkerEpicGloves.Weight = 22;
				BerserkerEpicGloves.Bonus = 35;
				BerserkerEpicGloves.MaxCondition = 50000;
				BerserkerEpicGloves.MaxDurability = 50000;
				BerserkerEpicGloves.Condition = 50000;
				BerserkerEpicGloves.Durability = 50000;

				BerserkerEpicGloves.Bonus1 = 3;
				BerserkerEpicGloves.Bonus1Type = (int) eProperty.Skill_Left_Axe;

				BerserkerEpicGloves.Bonus2 = 3;
				BerserkerEpicGloves.Bonus2Type = (int) eProperty.Skill_Parry;

				BerserkerEpicGloves.Bonus3 = 12;
				BerserkerEpicGloves.Bonus3Type = (int) eStat.STR;

				BerserkerEpicGloves.Bonus4 = 33;
				BerserkerEpicGloves.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicGloves);
				}
			}

			BerserkerEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicVest");
			if (BerserkerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Vest , creating it ...");
				BerserkerEpicVest = new ItemTemplate();
				BerserkerEpicVest.Id_nb = "BerserkerEpicVest";
				BerserkerEpicVest.Name = "Courage Bound Jerkin";
				BerserkerEpicVest.Level = 50;
				BerserkerEpicVest.Item_Type = 25;
				BerserkerEpicVest.Model = 751;
				BerserkerEpicVest.IsDropable = true;
				BerserkerEpicVest.IsPickable = true;
				BerserkerEpicVest.DPS_AF = 100;
				BerserkerEpicVest.SPD_ABS = 19;
				BerserkerEpicVest.Object_Type = 34;
				BerserkerEpicVest.Quality = 100;
				BerserkerEpicVest.Weight = 22;
				BerserkerEpicVest.Bonus = 35;
				BerserkerEpicVest.MaxCondition = 50000;
				BerserkerEpicVest.MaxDurability = 50000;
				BerserkerEpicVest.Condition = 50000;
				BerserkerEpicVest.Durability = 50000;

				BerserkerEpicVest.Bonus1 = 13;
				BerserkerEpicVest.Bonus1Type = (int) eStat.STR;

				BerserkerEpicVest.Bonus2 = 13;
				BerserkerEpicVest.Bonus2Type = (int) eStat.DEX;

				BerserkerEpicVest.Bonus3 = 6;
				BerserkerEpicVest.Bonus3Type = (int) eResist.Body;

				BerserkerEpicVest.Bonus4 = 30;
				BerserkerEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicVest);
				}
			}

			BerserkerEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicLegs");
			if (BerserkerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Legs , creating it ...");
				BerserkerEpicLegs = new ItemTemplate();
				BerserkerEpicLegs.Id_nb = "BerserkerEpicLegs";
				BerserkerEpicLegs.Name = "Courage Bound Leggings";
				BerserkerEpicLegs.Level = 50;
				BerserkerEpicLegs.Item_Type = 27;
				BerserkerEpicLegs.Model = 752;
				BerserkerEpicLegs.IsDropable = true;
				BerserkerEpicLegs.IsPickable = true;
				BerserkerEpicLegs.DPS_AF = 100;
				BerserkerEpicLegs.SPD_ABS = 19;
				BerserkerEpicLegs.Object_Type = 34;
				BerserkerEpicLegs.Quality = 100;
				BerserkerEpicLegs.Weight = 22;
				BerserkerEpicLegs.Bonus = 35;
				BerserkerEpicLegs.MaxCondition = 50000;
				BerserkerEpicLegs.MaxDurability = 50000;
				BerserkerEpicLegs.Condition = 50000;
				BerserkerEpicLegs.Durability = 50000;

				BerserkerEpicLegs.Bonus1 = 15;
				BerserkerEpicLegs.Bonus1Type = (int) eStat.STR;

				BerserkerEpicLegs.Bonus2 = 15;
				BerserkerEpicLegs.Bonus2Type = (int) eStat.CON;

				BerserkerEpicLegs.Bonus3 = 7;
				BerserkerEpicLegs.Bonus3Type = (int) eStat.DEX;

				BerserkerEpicLegs.Bonus4 = 12;
				BerserkerEpicLegs.Bonus4Type = (int) eResist.Slash;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicLegs);
				}
			}

			BerserkerEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BerserkerEpicArms");
			if (BerserkerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Arms , creating it ...");
				BerserkerEpicArms = new ItemTemplate();
				BerserkerEpicArms.Id_nb = "BerserkerEpicArms";
				BerserkerEpicArms.Name = "Courage Bound Sleeves";
				BerserkerEpicArms.Level = 50;
				BerserkerEpicArms.Item_Type = 28;
				BerserkerEpicArms.Model = 753;
				BerserkerEpicArms.IsDropable = true;
				BerserkerEpicArms.IsPickable = true;
				BerserkerEpicArms.DPS_AF = 100;
				BerserkerEpicArms.SPD_ABS = 19;
				BerserkerEpicArms.Object_Type = 34;
				BerserkerEpicArms.Quality = 100;
				BerserkerEpicArms.Weight = 22;
				BerserkerEpicArms.Bonus = 35;
				BerserkerEpicArms.MaxCondition = 50000;
				BerserkerEpicArms.MaxDurability = 50000;
				BerserkerEpicArms.Condition = 50000;
				BerserkerEpicArms.Durability = 50000;

				BerserkerEpicArms.Bonus1 = 19;
				BerserkerEpicArms.Bonus1Type = (int) eStat.STR;

				BerserkerEpicArms.Bonus2 = 15;
				BerserkerEpicArms.Bonus2Type = (int) eStat.CON;

				BerserkerEpicArms.Bonus3 = 8;
				BerserkerEpicArms.Bonus3Type = (int) eResist.Thrust;

				BerserkerEpicArms.Bonus4 = 8;
				BerserkerEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BerserkerEpicArms);
				}

			}
			ThaneEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicBoots");
			if (ThaneEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Boots , creating it ...");
				ThaneEpicBoots = new ItemTemplate();
				ThaneEpicBoots.Id_nb = "ThaneEpicBoots";
				ThaneEpicBoots.Name = "Storm Touched Boots";
				ThaneEpicBoots.Level = 50;
				ThaneEpicBoots.Item_Type = 23;
				ThaneEpicBoots.Model = 791;
				ThaneEpicBoots.IsDropable = true;
				ThaneEpicBoots.IsPickable = true;
				ThaneEpicBoots.DPS_AF = 100;
				ThaneEpicBoots.SPD_ABS = 27;
				ThaneEpicBoots.Object_Type = 35;
				ThaneEpicBoots.Quality = 100;
				ThaneEpicBoots.Weight = 22;
				ThaneEpicBoots.Bonus = 35;
				ThaneEpicBoots.MaxCondition = 50000;
				ThaneEpicBoots.MaxDurability = 50000;
				ThaneEpicBoots.Condition = 50000;
				ThaneEpicBoots.Durability = 50000;

				ThaneEpicBoots.Bonus1 = 13;
				ThaneEpicBoots.Bonus1Type = (int) eStat.CON;

				ThaneEpicBoots.Bonus2 = 13;
				ThaneEpicBoots.Bonus2Type = (int) eStat.DEX;

				ThaneEpicBoots.Bonus3 = 13;
				ThaneEpicBoots.Bonus3Type = (int) eStat.QUI;

				ThaneEpicBoots.Bonus4 = 8;
				ThaneEpicBoots.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicBoots);
				}

			}
//end item
			ThaneEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicHelm");
			if (ThaneEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Helm , creating it ...");
				ThaneEpicHelm = new ItemTemplate();
				ThaneEpicHelm.Id_nb = "ThaneEpicHelm";
				ThaneEpicHelm.Name = "Storm Touched Coif";
				ThaneEpicHelm.Level = 50;
				ThaneEpicHelm.Item_Type = 21;
				ThaneEpicHelm.Model = 834;
				ThaneEpicHelm.IsDropable = true;
				ThaneEpicHelm.IsPickable = true;
				ThaneEpicHelm.DPS_AF = 100;
				ThaneEpicHelm.SPD_ABS = 27;
				ThaneEpicHelm.Object_Type = 35;
				ThaneEpicHelm.Quality = 100;
				ThaneEpicHelm.Weight = 22;
				ThaneEpicHelm.Bonus = 35;
				ThaneEpicHelm.MaxCondition = 50000;
				ThaneEpicHelm.MaxDurability = 50000;
				ThaneEpicHelm.Condition = 50000;
				ThaneEpicHelm.Durability = 50000;

				ThaneEpicHelm.Bonus1 = 4;
				ThaneEpicHelm.Bonus1Type = (int) eProperty.Skill_Stormcalling;

				ThaneEpicHelm.Bonus2 = 18;
				ThaneEpicHelm.Bonus2Type = (int) eStat.CON;

				ThaneEpicHelm.Bonus3 = 4;
				ThaneEpicHelm.Bonus3Type = (int) eResist.Spirit;

				ThaneEpicHelm.Bonus4 = 6;
				ThaneEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicHelm);
				}

			}
//end item
			ThaneEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicGloves");
			if (ThaneEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Gloves , creating it ...");
				ThaneEpicGloves = new ItemTemplate();
				ThaneEpicGloves.Id_nb = "ThaneEpicGloves";
				ThaneEpicGloves.Name = "Storm Touched Gloves";
				ThaneEpicGloves.Level = 50;
				ThaneEpicGloves.Item_Type = 22;
				ThaneEpicGloves.Model = 790;
				ThaneEpicGloves.IsDropable = true;
				ThaneEpicGloves.IsPickable = true;
				ThaneEpicGloves.DPS_AF = 100;
				ThaneEpicGloves.SPD_ABS = 27;
				ThaneEpicGloves.Object_Type = 35;
				ThaneEpicGloves.Quality = 100;
				ThaneEpicGloves.Weight = 22;
				ThaneEpicGloves.Bonus = 35;
				ThaneEpicGloves.MaxCondition = 50000;
				ThaneEpicGloves.MaxDurability = 50000;
				ThaneEpicGloves.Condition = 50000;
				ThaneEpicGloves.Durability = 50000;

				ThaneEpicGloves.Bonus1 = 3;
				ThaneEpicGloves.Bonus1Type = (int) eProperty.Skill_Sword;

				ThaneEpicGloves.Bonus2 = 3;
				ThaneEpicGloves.Bonus2Type = (int) eProperty.Skill_Hammer;

				ThaneEpicGloves.Bonus3 = 3;
				ThaneEpicGloves.Bonus3Type = (int) eProperty.Skill_Axe;

				ThaneEpicGloves.Bonus4 = 19;
				ThaneEpicGloves.Bonus4Type = (int) eStat.STR;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicGloves);
				}

			}

			ThaneEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicVest");
			if (ThaneEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Vest , creating it ...");
				ThaneEpicVest = new ItemTemplate();
				ThaneEpicVest.Id_nb = "ThaneEpicVest";
				ThaneEpicVest.Name = "Storm Touched Hauberk";
				ThaneEpicVest.Level = 50;
				ThaneEpicVest.Item_Type = 25;
				ThaneEpicVest.Model = 787;
				ThaneEpicVest.IsDropable = true;
				ThaneEpicVest.IsPickable = true;
				ThaneEpicVest.DPS_AF = 100;
				ThaneEpicVest.SPD_ABS = 27;
				ThaneEpicVest.Object_Type = 35;
				ThaneEpicVest.Quality = 100;
				ThaneEpicVest.Weight = 22;
				ThaneEpicVest.Bonus = 35;
				ThaneEpicVest.MaxCondition = 50000;
				ThaneEpicVest.MaxDurability = 50000;
				ThaneEpicVest.Condition = 50000;
				ThaneEpicVest.Durability = 50000;

				ThaneEpicVest.Bonus1 = 13;
				ThaneEpicVest.Bonus1Type = (int) eStat.STR;

				ThaneEpicVest.Bonus2 = 13;
				ThaneEpicVest.Bonus2Type = (int) eStat.CON;

				ThaneEpicVest.Bonus3 = 6;
				ThaneEpicVest.Bonus3Type = (int) eResist.Slash;

				ThaneEpicVest.Bonus4 = 30;
				ThaneEpicVest.Bonus4Type = (int) eProperty.MaxHealth;


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicVest);
				}
			}

			ThaneEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicLegs");
			if (ThaneEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Legs , creating it ...");
				ThaneEpicLegs = new ItemTemplate();
				ThaneEpicLegs.Id_nb = "ThaneEpicLegs";
				ThaneEpicLegs.Name = "Storm Touched Legs";
				ThaneEpicLegs.Level = 50;
				ThaneEpicLegs.Item_Type = 27;
				ThaneEpicLegs.Model = 788;
				ThaneEpicLegs.IsDropable = true;
				ThaneEpicLegs.IsPickable = true;
				ThaneEpicLegs.DPS_AF = 100;
				ThaneEpicLegs.SPD_ABS = 27;
				ThaneEpicLegs.Object_Type = 35;
				ThaneEpicLegs.Quality = 100;
				ThaneEpicLegs.Weight = 22;
				ThaneEpicLegs.Bonus = 35;
				ThaneEpicLegs.MaxCondition = 50000;
				ThaneEpicLegs.MaxDurability = 50000;
				ThaneEpicLegs.Condition = 50000;
				ThaneEpicLegs.Durability = 50000;

				ThaneEpicLegs.Bonus1 = 19;
				ThaneEpicLegs.Bonus1Type = (int) eStat.CON;

				ThaneEpicLegs.Bonus2 = 15;
				ThaneEpicLegs.Bonus2Type = (int) eStat.PIE;

				ThaneEpicLegs.Bonus3 = 8;
				ThaneEpicLegs.Bonus3Type = (int) eResist.Crush;

				ThaneEpicLegs.Bonus4 = 8;
				ThaneEpicLegs.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicLegs);
				}
			}

			ThaneEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ThaneEpicArms");
			if (ThaneEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Arms , creating it ...");
				ThaneEpicArms = new ItemTemplate();
				ThaneEpicArms.Id_nb = "ThaneEpicArms";
				ThaneEpicArms.Name = "Storm Touched Sleeves";
				ThaneEpicArms.Level = 50;
				ThaneEpicArms.Item_Type = 28;
				ThaneEpicArms.Model = 789;
				ThaneEpicArms.IsDropable = true;
				ThaneEpicArms.IsPickable = true;
				ThaneEpicArms.DPS_AF = 100;
				ThaneEpicArms.SPD_ABS = 27;
				ThaneEpicArms.Object_Type = 35;
				ThaneEpicArms.Quality = 100;
				ThaneEpicArms.Weight = 22;
				ThaneEpicArms.Bonus = 35;
				ThaneEpicArms.MaxCondition = 50000;
				ThaneEpicArms.MaxDurability = 50000;
				ThaneEpicArms.Condition = 50000;
				ThaneEpicArms.Durability = 50000;

				ThaneEpicArms.Bonus1 = 18;
				ThaneEpicArms.Bonus1Type = (int) eStat.STR;

				ThaneEpicArms.Bonus2 = 16;
				ThaneEpicArms.Bonus2Type = (int) eStat.QUI;

				ThaneEpicArms.Bonus3 = 8;
				ThaneEpicArms.Bonus3Type = (int) eResist.Thrust;

				ThaneEpicArms.Bonus4 = 8;
				ThaneEpicArms.Bonus4Type = (int) eResist.Body;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ThaneEpicArms);
				}
			}
			//Valhalla Touched Boots
			SkaldEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicBoots");
			if (SkaldEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Boots , creating it ...");
				SkaldEpicBoots = new ItemTemplate();
				SkaldEpicBoots.Id_nb = "SkaldEpicBoots";
				SkaldEpicBoots.Name = "Battlesung Boots";
				SkaldEpicBoots.Level = 50;
				SkaldEpicBoots.Item_Type = 23;
				SkaldEpicBoots.Model = 775;
				SkaldEpicBoots.IsDropable = true;
				SkaldEpicBoots.IsPickable = true;
				SkaldEpicBoots.DPS_AF = 100;
				SkaldEpicBoots.SPD_ABS = 27;
				SkaldEpicBoots.Object_Type = 35;
				SkaldEpicBoots.Quality = 100;
				SkaldEpicBoots.Weight = 22;
				SkaldEpicBoots.Bonus = 35;
				SkaldEpicBoots.MaxCondition = 50000;
				SkaldEpicBoots.MaxDurability = 50000;
				SkaldEpicBoots.Condition = 50000;
				SkaldEpicBoots.Durability = 50000;

				SkaldEpicBoots.Bonus1 = 13;
				SkaldEpicBoots.Bonus1Type = (int) eStat.CON;

				SkaldEpicBoots.Bonus2 = 13;
				SkaldEpicBoots.Bonus2Type = (int) eStat.QUI;

				SkaldEpicBoots.Bonus3 = 10;
				SkaldEpicBoots.Bonus3Type = (int) eResist.Cold;

				SkaldEpicBoots.Bonus4 = 24;
				SkaldEpicBoots.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicBoots);
				}
			}
//end item
			//Valhalla Touched Coif 
			SkaldEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicHelm");
			if (SkaldEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Helm , creating it ...");
				SkaldEpicHelm = new ItemTemplate();
				SkaldEpicHelm.Id_nb = "SkaldEpicHelm";
				SkaldEpicHelm.Name = "Battlesung Coif";
				SkaldEpicHelm.Level = 50;
				SkaldEpicHelm.Item_Type = 21;
				SkaldEpicHelm.Model = 832; //NEED TO WORK ON..
				SkaldEpicHelm.IsDropable = true;
				SkaldEpicHelm.IsPickable = true;
				SkaldEpicHelm.DPS_AF = 100;
				SkaldEpicHelm.SPD_ABS = 27;
				SkaldEpicHelm.Object_Type = 35;
				SkaldEpicHelm.Quality = 100;
				SkaldEpicHelm.Weight = 22;
				SkaldEpicHelm.Bonus = 35;
				SkaldEpicHelm.MaxCondition = 50000;
				SkaldEpicHelm.MaxDurability = 50000;
				SkaldEpicHelm.Condition = 50000;
				SkaldEpicHelm.Durability = 50000;

				SkaldEpicHelm.Bonus1 = 5;
				SkaldEpicHelm.Bonus1Type = (int) eProperty.Skill_Battlesongs;

				SkaldEpicHelm.Bonus2 = 15;
				SkaldEpicHelm.Bonus2Type = (int) eStat.CHR;

				SkaldEpicHelm.Bonus3 = 33;
				SkaldEpicHelm.Bonus3Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicHelm);
				}
			}
//end item
			//Valhalla Touched Gloves 
			SkaldEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicGloves");
			if (SkaldEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Gloves , creating it ...");
				SkaldEpicGloves = new ItemTemplate();
				SkaldEpicGloves.Id_nb = "SkaldEpicGloves";
				SkaldEpicGloves.Name = "Battlesung Gloves";
				SkaldEpicGloves.Level = 50;
				SkaldEpicGloves.Item_Type = 22;
				SkaldEpicGloves.Model = 774;
				SkaldEpicGloves.IsDropable = true;
				SkaldEpicGloves.IsPickable = true;
				SkaldEpicGloves.DPS_AF = 100;
				SkaldEpicGloves.SPD_ABS = 27;
				SkaldEpicGloves.Object_Type = 35;
				SkaldEpicGloves.Quality = 100;
				SkaldEpicGloves.Weight = 22;
				SkaldEpicGloves.Bonus = 35;
				SkaldEpicGloves.MaxCondition = 50000;
				SkaldEpicGloves.MaxDurability = 50000;
				SkaldEpicGloves.Condition = 50000;
				SkaldEpicGloves.Durability = 50000;

				SkaldEpicGloves.Bonus1 = 18;
				SkaldEpicGloves.Bonus1Type = (int) eStat.STR;

				SkaldEpicGloves.Bonus2 = 15;
				SkaldEpicGloves.Bonus2Type = (int) eStat.DEX;

				SkaldEpicGloves.Bonus3 = 8;
				SkaldEpicGloves.Bonus3Type = (int) eResist.Body;

				SkaldEpicGloves.Bonus4 = 10;
				SkaldEpicGloves.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicGloves);
				}

			}
			//Valhalla Touched Hauberk 
			SkaldEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicVest");
			if (SkaldEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Vest , creating it ...");
				SkaldEpicVest = new ItemTemplate();
				SkaldEpicVest.Id_nb = "SkaldEpicVest";
				SkaldEpicVest.Name = "Battlesung Hauberk";
				SkaldEpicVest.Level = 50;
				SkaldEpicVest.Item_Type = 25;
				SkaldEpicVest.Model = 771;
				SkaldEpicVest.IsDropable = true;
				SkaldEpicVest.IsPickable = true;
				SkaldEpicVest.DPS_AF = 100;
				SkaldEpicVest.SPD_ABS = 27;
				SkaldEpicVest.Object_Type = 35;
				SkaldEpicVest.Quality = 100;
				SkaldEpicVest.Weight = 22;
				SkaldEpicVest.Bonus = 35;
				SkaldEpicVest.MaxCondition = 50000;
				SkaldEpicVest.MaxDurability = 50000;
				SkaldEpicVest.Condition = 50000;
				SkaldEpicVest.Durability = 50000;

				SkaldEpicVest.Bonus1 = 13;
				SkaldEpicVest.Bonus1Type = (int) eStat.STR;

				SkaldEpicVest.Bonus2 = 13;
				SkaldEpicVest.Bonus2Type = (int) eStat.CON;

				SkaldEpicVest.Bonus3 = 13;
				SkaldEpicVest.Bonus3Type = (int) eStat.CHR;

				SkaldEpicVest.Bonus4 = 8;
				SkaldEpicVest.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicVest);
				}
			}
			//Valhalla Touched Legs 
			SkaldEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicLegs");
			if (SkaldEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Legs , creating it ...");
				SkaldEpicLegs = new ItemTemplate();
				SkaldEpicLegs.Id_nb = "SkaldEpicLegs";
				SkaldEpicLegs.Name = "Battlesung Legs";
				SkaldEpicLegs.Level = 50;
				SkaldEpicLegs.Item_Type = 27;
				SkaldEpicLegs.Model = 772;
				SkaldEpicLegs.IsDropable = true;
				SkaldEpicLegs.IsPickable = true;
				SkaldEpicLegs.DPS_AF = 100;
				SkaldEpicLegs.SPD_ABS = 27;
				SkaldEpicLegs.Object_Type = 35;
				SkaldEpicLegs.Quality = 100;
				SkaldEpicLegs.Weight = 22;
				SkaldEpicLegs.Bonus = 35;
				SkaldEpicLegs.MaxCondition = 50000;
				SkaldEpicLegs.MaxDurability = 50000;
				SkaldEpicLegs.Condition = 50000;
				SkaldEpicLegs.Durability = 50000;

				SkaldEpicLegs.Bonus1 = 13;
				SkaldEpicLegs.Bonus1Type = (int) eStat.STR;

				SkaldEpicLegs.Bonus2 = 13;
				SkaldEpicLegs.Bonus2Type = (int) eStat.CON;

				SkaldEpicLegs.Bonus3 = 8;
				SkaldEpicLegs.Bonus3Type = (int) eResist.Spirit;

				SkaldEpicLegs.Bonus4 = 27;
				SkaldEpicLegs.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicLegs);
				}
			}
			//Valhalla Touched Sleeves 
			SkaldEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SkaldEpicArms");
			if (SkaldEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skald Epic Arms , creating it ...");
				SkaldEpicArms = new ItemTemplate();
				SkaldEpicArms.Id_nb = "SkaldEpicArms";
				SkaldEpicArms.Name = "Battlesung Sleeves";
				SkaldEpicArms.Level = 50;
				SkaldEpicArms.Item_Type = 28;
				SkaldEpicArms.Model = 773;
				SkaldEpicArms.IsDropable = true;
				SkaldEpicArms.IsPickable = true;
				SkaldEpicArms.DPS_AF = 100;
				SkaldEpicArms.SPD_ABS = 27;
				SkaldEpicArms.Object_Type = 35;
				SkaldEpicArms.Quality = 100;
				SkaldEpicArms.Weight = 22;
				SkaldEpicArms.Bonus = 35;
				SkaldEpicArms.MaxCondition = 50000;
				SkaldEpicArms.MaxDurability = 50000;
				SkaldEpicArms.Condition = 50000;
				SkaldEpicArms.Durability = 50000;

				SkaldEpicArms.Bonus1 = 16;
				SkaldEpicArms.Bonus1Type = (int) eStat.STR;

				SkaldEpicArms.Bonus2 = 15;
				SkaldEpicArms.Bonus2Type = (int) eStat.CON;

				SkaldEpicArms.Bonus3 = 10;
				SkaldEpicArms.Bonus3Type = (int) eResist.Thrust;

				SkaldEpicArms.Bonus4 = 10;
				SkaldEpicArms.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SkaldEpicArms);
				}
			}
			//Subterranean Boots 
			SavageEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicBoots");
			if (SavageEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Boots , creating it ...");
				SavageEpicBoots = new ItemTemplate();
				SavageEpicBoots.Id_nb = "SavageEpicBoots";
				SavageEpicBoots.Name = "Kelgor's Battle Boots";
				SavageEpicBoots.Level = 50;
				SavageEpicBoots.Item_Type = 23;
				SavageEpicBoots.Model = 1196;
				SavageEpicBoots.IsDropable = true;
				SavageEpicBoots.IsPickable = true;
				SavageEpicBoots.DPS_AF = 100;
				SavageEpicBoots.SPD_ABS = 27;
				SavageEpicBoots.Object_Type = 34;
				SavageEpicBoots.Quality = 100;
				SavageEpicBoots.Weight = 22;
				SavageEpicBoots.Bonus = 35;
				SavageEpicBoots.MaxCondition = 50000;
				SavageEpicBoots.MaxDurability = 50000;
				SavageEpicBoots.Condition = 50000;
				SavageEpicBoots.Durability = 50000;

				SavageEpicBoots.Bonus1 = 15;
				SavageEpicBoots.Bonus1Type = (int) eStat.CON;

				SavageEpicBoots.Bonus2 = 19;
				SavageEpicBoots.Bonus2Type = (int) eStat.DEX;

				SavageEpicBoots.Bonus3 = 8;
				SavageEpicBoots.Bonus3Type = (int) eResist.Matter;

				SavageEpicBoots.Bonus4 = 8;
				SavageEpicBoots.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicBoots);
				}
			}
			//Subterranean Coif 
			SavageEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicHelm");
			if (SavageEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Helm , creating it ...");
				SavageEpicHelm = new ItemTemplate();
				SavageEpicHelm.Id_nb = "SavageEpicHelm";
				SavageEpicHelm.Name = "Kelgor's Battle Helm";
				SavageEpicHelm.Level = 50;
				SavageEpicHelm.Item_Type = 21;
				SavageEpicHelm.Model = 824; //NEED TO WORK ON..
				SavageEpicHelm.IsDropable = true;
				SavageEpicHelm.IsPickable = true;
				SavageEpicHelm.DPS_AF = 100;
				SavageEpicHelm.SPD_ABS = 19;
				SavageEpicHelm.Object_Type = 34;
				SavageEpicHelm.Quality = 100;
				SavageEpicHelm.Weight = 22;
				SavageEpicHelm.Bonus = 35;
				SavageEpicHelm.MaxCondition = 50000;
				SavageEpicHelm.MaxDurability = 50000;
				SavageEpicHelm.Condition = 50000;
				SavageEpicHelm.Durability = 50000;

				SavageEpicHelm.Bonus1 = 15;
				SavageEpicHelm.Bonus1Type = (int) eStat.STR;

				SavageEpicHelm.Bonus2 = 10;
				SavageEpicHelm.Bonus2Type = (int) eStat.CON;

				SavageEpicHelm.Bonus3 = 10;
				SavageEpicHelm.Bonus3Type = (int) eStat.DEX;

				SavageEpicHelm.Bonus4 = 10;
				SavageEpicHelm.Bonus4Type = (int) eStat.QUI;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicHelm);
				}
			}
			//Subterranean Gloves 
			SavageEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicGloves");
			if (SavageEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Gloves , creating it ...");
				SavageEpicGloves = new ItemTemplate();
				SavageEpicGloves.Id_nb = "SavageEpicGloves";
				SavageEpicGloves.Name = "Kelgor's Battle Gauntlets";
				SavageEpicGloves.Level = 50;
				SavageEpicGloves.Item_Type = 22;
				SavageEpicGloves.Model = 1195;
				SavageEpicGloves.IsDropable = true;
				SavageEpicGloves.IsPickable = true;
				SavageEpicGloves.DPS_AF = 100;
				SavageEpicGloves.SPD_ABS = 19;
				SavageEpicGloves.Object_Type = 34;
				SavageEpicGloves.Quality = 100;
				SavageEpicGloves.Weight = 22;
				SavageEpicGloves.Bonus = 35;
				SavageEpicGloves.MaxCondition = 50000;
				SavageEpicGloves.MaxDurability = 50000;
				SavageEpicGloves.Condition = 50000;
				SavageEpicGloves.Durability = 50000;

				SavageEpicGloves.Bonus1 = 3;
				SavageEpicGloves.Bonus1Type = (int) eProperty.Skill_Parry;

				SavageEpicGloves.Bonus2 = 12;
				SavageEpicGloves.Bonus2Type = (int) eStat.DEX;

				SavageEpicGloves.Bonus3 = 33;
				SavageEpicGloves.Bonus3Type = (int) eProperty.MaxHealth;

				SavageEpicGloves.Bonus4 = 3;
				SavageEpicGloves.Bonus4Type = (int) eProperty.Skill_HandToHand;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicGloves);
				}
			}
			//Subterranean Hauberk 
			SavageEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicVest");
			if (SavageEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Vest , creating it ...");
				SavageEpicVest = new ItemTemplate();
				SavageEpicVest.Id_nb = "SavageEpicVest";
				SavageEpicVest.Name = "Kelgor's Battle Vest";
				SavageEpicVest.Level = 50;
				SavageEpicVest.Item_Type = 25;
				SavageEpicVest.Model = 1192;
				SavageEpicVest.IsDropable = true;
				SavageEpicVest.IsPickable = true;
				SavageEpicVest.DPS_AF = 100;
				SavageEpicVest.SPD_ABS = 19;
				SavageEpicVest.Object_Type = 34;
				SavageEpicVest.Quality = 100;
				SavageEpicVest.Weight = 22;
				SavageEpicVest.Bonus = 35;
				SavageEpicVest.MaxCondition = 50000;
				SavageEpicVest.MaxDurability = 50000;
				SavageEpicVest.Condition = 50000;
				SavageEpicVest.Durability = 50000;

				SavageEpicVest.Bonus1 = 13;
				SavageEpicVest.Bonus1Type = (int) eStat.STR;

				SavageEpicVest.Bonus2 = 13;
				SavageEpicVest.Bonus2Type = (int) eStat.QUI;

				SavageEpicVest.Bonus3 = 6;
				SavageEpicVest.Bonus3Type = (int) eResist.Slash;

				SavageEpicVest.Bonus4 = 30;
				SavageEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicVest);
				}
			}
			//Subterranean Legs 
			SavageEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicLegs");
			if (SavageEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Legs , creating it ...");
				SavageEpicLegs = new ItemTemplate();
				SavageEpicLegs.Id_nb = "SavageEpicLegs";
				SavageEpicLegs.Name = "Kelgor's Battle Leggings";
				SavageEpicLegs.Level = 50;
				SavageEpicLegs.Item_Type = 27;
				SavageEpicLegs.Model = 1193;
				SavageEpicLegs.IsDropable = true;
				SavageEpicLegs.IsPickable = true;
				SavageEpicLegs.DPS_AF = 100;
				SavageEpicLegs.SPD_ABS = 19;
				SavageEpicLegs.Object_Type = 34;
				SavageEpicLegs.Quality = 100;
				SavageEpicLegs.Weight = 22;
				SavageEpicLegs.Bonus = 35;
				SavageEpicLegs.MaxCondition = 50000;
				SavageEpicLegs.MaxDurability = 50000;
				SavageEpicLegs.Condition = 50000;
				SavageEpicLegs.Durability = 50000;

				SavageEpicLegs.Bonus1 = 12;
				SavageEpicLegs.Bonus1Type = (int) eResist.Heat;

				SavageEpicLegs.Bonus2 = 7;
				SavageEpicLegs.Bonus2Type = (int) eStat.CON;

				SavageEpicLegs.Bonus3 = 15;
				SavageEpicLegs.Bonus3Type = (int) eStat.DEX;

				SavageEpicLegs.Bonus4 = 15;
				SavageEpicLegs.Bonus4Type = (int) eStat.QUI;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicLegs);
				}
			}
			//Subterranean Sleeves 
			SavageEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SavageEpicArms");
			if (SavageEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Arms , creating it ...");
				SavageEpicArms = new ItemTemplate();
				SavageEpicArms.Id_nb = "SavageEpicArms";
				SavageEpicArms.Name = "Kelgor's Battle Sleeves";
				SavageEpicArms.Level = 50;
				SavageEpicArms.Item_Type = 28;
				SavageEpicArms.Model = 1194;
				SavageEpicArms.IsDropable = true;
				SavageEpicArms.IsPickable = true;
				SavageEpicArms.DPS_AF = 100;
				SavageEpicArms.SPD_ABS = 19;
				SavageEpicArms.Object_Type = 34;
				SavageEpicArms.Quality = 100;
				SavageEpicArms.Weight = 22;
				SavageEpicArms.Bonus = 35;
				SavageEpicArms.MaxCondition = 50000;
				SavageEpicArms.MaxDurability = 50000;
				SavageEpicArms.Condition = 50000;
				SavageEpicArms.Durability = 50000;

				SavageEpicArms.Bonus1 = 19;
				SavageEpicArms.Bonus1Type = (int) eStat.STR;

				SavageEpicArms.Bonus2 = 15;
				SavageEpicArms.Bonus2Type = (int) eStat.QUI;

				SavageEpicArms.Bonus3 = 8;
				SavageEpicArms.Bonus3 = (int) eResist.Cold;

				SavageEpicArms.Bonus4 = 8;
				SavageEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SavageEpicArms);
				}

			}
//Savage Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.AddHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));

			/* Now we bring to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.AddQuestToGive(typeof (Viking_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Lynnleigh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.RemoveHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));
		
			/* Now we remove to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.RemoveQuestToGive(typeof (Viking_50));
		}

		protected static void TalkToLynnleigh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Lynnleigh.SayTo(player, "Check your Journal for information about what to do!");
				}
				else
				{
					Lynnleigh.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Ydenia to dispose of him. He also has a note here about how strong Ydenia really was. That [worries me].");
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "worries me":
							Lynnleigh.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Ydenia] and his minions.");
							break;
						case "face Ydenia":
							player.Out.SendQuestSubscribeCommand(Lynnleigh, QuestMgr.GetIDForQuestType(typeof(Viking_50)), "Will you face Ydenia [Viking Level 50 Epic]?");
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

		protected static void TalkToElizabeth(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 4:
							Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
							break;
					}
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 4)
								quest.FinishQuest();
							break;
					}
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Warrior &&
				player.CharacterClass.ID != (byte) eCharacterClass.Berserker &&
				player.CharacterClass.ID != (byte) eCharacterClass.Thane &&
				player.CharacterClass.ID != (byte) eCharacterClass.Skald &&
				player.CharacterClass.ID != (byte) eCharacterClass.Savage)
				return false;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

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

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Viking_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Lynnleigh.GiveQuest(typeof (Viking_50), player, 1))
					return;

				Lynnleigh.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Ydenia is strong. He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, I would highley recommed taking some friends with you to face Ydenia. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. According to the map you can find Ydenia in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Ydenia and his followers. Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "An End to the Daggers (level 50 Viking epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Ydenia in Raumarik Loc 48k, 30k kill her!";
					case 2:
						return "[Step #2] Return to Lynnleigh and give her tome of Enchantments!";
					case 3:
						return "[Step #3] Take the Sealed Pouch to Elizabeth in Mularn";
					case 4:
						return "[Step #4] Tell Elizabeth you can 'take them' for your rewards!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Viking_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == Ydenia.Name)
				{
					Step = 2;
					GiveItem(m_questPlayer, tome_enchantments);
					m_questPlayer.Out.SendMessage("Ydenia drops the Tome of Enchantments and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Lynnleigh.Name && gArgs.Item.Id_nb == tome_enchantments.Id_nb)
				{
					RemoveItem(Lynnleigh, player, tome_enchantments);
					Lynnleigh.SayTo(player, "Take this sealed pouch to Elizabeth in Mularn for your reward!");
					GiveItem(Lynnleigh, player, sealed_pouch);
					Step = 3;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Elizabeth.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					RemoveItem(Elizabeth, player, sealed_pouch);
					Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 4;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, tome_enchantments, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Warrior)
			{
				GiveItem(m_questPlayer, WarriorEpicArms);
				GiveItem(m_questPlayer, WarriorEpicBoots);
				GiveItem(m_questPlayer, WarriorEpicGloves);
				GiveItem(m_questPlayer, WarriorEpicHelm);
				GiveItem(m_questPlayer, WarriorEpicLegs);
				GiveItem(m_questPlayer, WarriorEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Berserker)
			{
				GiveItem(m_questPlayer, BerserkerEpicArms);
				GiveItem(m_questPlayer, BerserkerEpicBoots);
				GiveItem(m_questPlayer, BerserkerEpicGloves);
				GiveItem(m_questPlayer, BerserkerEpicHelm);
				GiveItem(m_questPlayer, BerserkerEpicLegs);
				GiveItem(m_questPlayer, BerserkerEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Thane)
			{
				GiveItem(m_questPlayer, ThaneEpicArms);
				GiveItem(m_questPlayer, ThaneEpicBoots);
				GiveItem(m_questPlayer, ThaneEpicGloves);
				GiveItem(m_questPlayer, ThaneEpicHelm);
				GiveItem(m_questPlayer, ThaneEpicLegs);
				GiveItem(m_questPlayer, ThaneEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Skald)
			{
				GiveItem(m_questPlayer, SkaldEpicArms);
				GiveItem(m_questPlayer, SkaldEpicBoots);
				GiveItem(m_questPlayer, SkaldEpicGloves);
				GiveItem(m_questPlayer, SkaldEpicHelm);
				GiveItem(m_questPlayer, SkaldEpicLegs);
				GiveItem(m_questPlayer, SkaldEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Savage)
			{
				GiveItem(m_questPlayer, SavageEpicArms);
				GiveItem(m_questPlayer, SavageEpicBoots);
				GiveItem(m_questPlayer, SavageEpicGloves);
				GiveItem(m_questPlayer, SavageEpicHelm);
				GiveItem(m_questPlayer, SavageEpicLegs);
				GiveItem(m_questPlayer, SavageEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}
	}
}
