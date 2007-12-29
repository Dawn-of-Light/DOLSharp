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
*Date           : 7 December 2004
*Quest Name     : Passage to Eternity (level 50)
*Quest Classes  : Paladin, Cleric, (Church)
*Quest Version  : v1
*
*ToDo:
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	public class Church_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Passage to Eternity";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Roben = null; // Start NPC
		private static GameNPC Blythe = null; // Mob to kill

		private static ItemTemplate statue_of_arawn = null; //sealed pouch
		private static ItemTemplate ClericEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ClericEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ClericEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ClericEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ClericEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ClericEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate PaladinEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate PaladinEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate PaladinEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate PaladinEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate PaladinEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate PaladinEpicArms = null; //Valhalla Touched Sleeves

		// Constructors
		public Church_50() : base()
		{
		}

		public Church_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Church_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Church_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Roben Fraomar", eRealm.Albion);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Roben , creating it ...");
				Roben = new GameNPC();
				Roben.Model = 36;
				Roben.Name = "Roben Fraomar";
				Roben.GuildName = "";
				Roben.Realm = eRealm.Albion;
				Roben.CurrentRegionID = 1;
				Roben.Size = 52;
				Roben.Level = 50;
				Roben.X = 408557;
				Roben.Y = 651675;
				Roben.Z = 5200;
				Roben.Heading = 3049;
				Roben.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Roben.SaveIntoDatabase();
			}
			else
				Roben = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Sister Blythe", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Blythe , creating it ...");
				Blythe = new GameNPC();
				Blythe.Model = 67;
				Blythe.Name = "Sister Blythe";
				Blythe.GuildName = "";
				Blythe.Realm = eRealm.None;
				Blythe.CurrentRegionID = 1;
				Blythe.Size = 50;
				Blythe.Level = 69;
				Blythe.X = 322231;
				Blythe.Y = 671546;
				Blythe.Z = 2762;
				Blythe.Heading = 1683;
				Blythe.AddToWorld();

				if (SAVE_INTO_DATABASE)
					Blythe.SaveIntoDatabase();
			}
			else
				Blythe = npcs[0];
			// end npc

			#endregion

			#region defineItems

			statue_of_arawn = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "statue_of_arawn");
			if (statue_of_arawn == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Statue of Arawn, creating it ...");
				statue_of_arawn = new ItemTemplate();
				statue_of_arawn.Id_nb = "statue_of_arawn";
				statue_of_arawn.Name = "Statue of Arawn";
				statue_of_arawn.Level = 8;
				statue_of_arawn.Item_Type = 29;
				statue_of_arawn.Model = 593;
				statue_of_arawn.IsDropable = false;
				statue_of_arawn.IsPickable = false;
				statue_of_arawn.DPS_AF = 0;
				statue_of_arawn.SPD_ABS = 0;
				statue_of_arawn.Object_Type = 41;
				statue_of_arawn.Hand = 0;
				statue_of_arawn.Type_Damage = 0;
				statue_of_arawn.Quality = 100;
				statue_of_arawn.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(statue_of_arawn);
				}

			}
// end item
			ItemTemplate i = null;
			ClericEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicBoots");
			if (ClericEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Boots , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicBoots";
				i.Name = "Boots of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 717;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 13;
				i.Bonus1Type = (int) eStat.CON;

				i.Bonus2 = 13;
				i.Bonus2Type = (int) eStat.DEX;

				i.Bonus3 = 13;
				i.Bonus3Type = (int) eStat.QUI;

				i.Bonus4 = 8;
				i.Bonus4Type = (int) eResist.Spirit;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ClericEpicBoots = i;

			}
//end item
			//of the Defiant Soul  Coif 
			ClericEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicHelm");
			if (ClericEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Helm , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicHelm";
				i.Name = "Coif of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 4;
				i.Bonus1Type = (int) eProperty.Focus_Enchantments;

				i.Bonus2 = 12;
				i.Bonus2Type = (int) eStat.CON;

				i.Bonus3 = 19;
				i.Bonus3Type = (int) eStat.PIE;

				i.Bonus4 = 8;
				i.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicHelm = i;

			}
//end item
			//of the Defiant Soul  Gloves 
			ClericEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicGloves");
			if (ClericEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Gloves , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicGloves";
				i.Name = "Gauntlets of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 716;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 4;
				i.Bonus1Type = (int) eProperty.Skill_Smiting;

				i.Bonus2 = 22;
				i.Bonus2Type = (int) eStat.PIE;

				i.Bonus3 = 8;
				i.Bonus3Type = (int) eResist.Crush;

				i.Bonus4 = 8;
				i.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicGloves = i;

			}
			//of the Defiant Soul  Hauberk 
			ClericEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicVest");
			if (ClericEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Vest , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicVest";
				i.Name = "Habergeon of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 713;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 4;
				i.Bonus1Type = (int) eResist.Crush;

				i.Bonus2 = 4;
				i.Bonus2Type = (int) eResist.Spirit;

				i.Bonus3 = 12;
				i.Bonus3Type = (int) eProperty.PowerRegenerationRate;

				i.Bonus4 = 27;
				i.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}
				ClericEpicVest = i;

			}
			//of the Defiant Soul  Legs 
			ClericEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicLegs");
			if (ClericEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Clerics Epic Legs , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicLegs";
				i.Name = "Chaussess of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 714;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 4;
				i.Bonus1Type = (int) eProperty.Skill_Rejuvenation;

				i.Bonus2 = 22;
				i.Bonus2Type = (int) eStat.CON;

				i.Bonus3 = 8;
				i.Bonus3Type = (int) eResist.Slash;

				i.Bonus4 = 8;
				i.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicLegs = i;

			}
			//of the Defiant Soul  Sleeves 
			ClericEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ClericEpicArms");
			if (ClericEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Cleric Epic Arms , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "ClericEpicArms";
				i.Name = "Sleeves of Defiant Soul";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 715;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 16;
				i.Bonus1Type = (int) eStat.STR;

				i.Bonus2 = 18;
				i.Bonus2Type = (int) eStat.PIE;

				i.Bonus3 = 8;
				i.Bonus3Type = (int) eResist.Thrust;

				i.Bonus4 = 8;
				i.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				ClericEpicArms = i;
			}

			PaladinEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicBoots");
			if (PaladinEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Boots , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicBoots";
				i.Name = "Sabaton of the Iron Will";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 697;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 18;
				i.Bonus1Type = (int) eStat.STR;

				i.Bonus2 = 19;
				i.Bonus2Type = (int) eStat.QUI;

				i.Bonus3 = 6;
				i.Bonus3Type = (int) eResist.Slash;

				i.Bonus4 = 6;
				i.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicBoots = i;

			}
//end item
			//of the Iron Will Coif 
			PaladinEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicHelm");
			if (PaladinEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Helm , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicHelm";
				i.Name = "Hounskull of the Iron Will";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 18;
				i.Bonus1Type = (int) eStat.CON;

				i.Bonus2 = 19;
				i.Bonus2Type = (int) eStat.DEX;

				i.Bonus3 = 6;
				i.Bonus3Type = (int) eResist.Crush;

				i.Bonus4 = 6;
				i.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicHelm = i;

			}
//end item
			//of the Iron Will Gloves 
			PaladinEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicGloves");
			if (PaladinEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Gloves , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicGloves";
				i.Name = "Gauntlets of the Iron Will";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 696;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 19;
				i.Bonus1Type = (int) eStat.STR;

				i.Bonus2 = 18;
				i.Bonus2Type = (int) eStat.QUI;

				i.Bonus3 = 6;
				i.Bonus3Type = (int) eResist.Crush;

				i.Bonus4 = 6;
				i.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicGloves = i;

			}
			//of the Iron Will Hauberk 
			PaladinEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicVest");
			if (PaladinEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Vest , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicVest";
				i.Name = "Curiass of the Iron Will";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 693;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 15;
				i.Bonus1Type = (int) eStat.STR;

				i.Bonus2 = 6;
				i.Bonus2Type = (int) eResist.Body;

				i.Bonus3 = 6;
				i.Bonus3Type = (int) eResist.Spirit;

				i.Bonus4 = 24;
				i.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicVest = i;

			}
			//of the Iron Will Legs 
			PaladinEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicLegs");
			if (PaladinEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Legs , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicLegs";
				i.Name = "Greaves of the Iron Will";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 694;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 22;
				i.Bonus1Type = (int) eStat.CON;

				i.Bonus2 = 15;
				i.Bonus2Type = (int) eStat.DEX;

				i.Bonus3 = 6;
				i.Bonus3Type = (int) eResist.Crush;

				i.Bonus4 = 6;
				i.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicLegs = i;

			}
			//of the Iron Will Sleeves 
			PaladinEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "PaladinEpicArms");
			if (PaladinEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Paladin Epic Arms , creating it ...");
				i = new ItemTemplate();
				i.Id_nb = "PaladinEpicArms";
				i.Name = "Spaulders of the Iron Will";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 695;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 34;
				i.Object_Type = 36;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 19;
				i.Bonus1Type = (int) eStat.CON;

				i.Bonus2 = 15;
				i.Bonus2Type = (int) eStat.DEX;

				i.Bonus3 = 9;
				i.Bonus3Type = (int) eStat.QUI;

				i.Bonus4 = 6;
				i.Bonus4Type = (int) eResist.Spirit;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(i);
				}

				PaladinEpicArms = i;
			}

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Roben, GameObjectEvent.Interact, new DOLEventHandler(TalkToRoben));
			GameEventMgr.AddHandler(Roben, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRoben));

			/* Now we bring to Roben the possibility to give this quest to players */
			Roben.AddQuestToGive(typeof (Church_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");

		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			//if not loaded, don't worry
			if (Roben == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Roben, GameObjectEvent.Interact, new DOLEventHandler(TalkToRoben));
			GameEventMgr.RemoveHandler(Roben, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRoben));

			/* Now we remove to Roben the possibility to give this quest to players */
			Roben.RemoveQuestToGive(typeof (Church_50));
		}

		protected static void TalkToRoben(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Roben.CanGiveQuest(typeof (Church_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Church_50 quest = player.IsDoingQuest(typeof (Church_50)) as Church_50;

			Roben.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest == null)
				{
					Roben.SayTo(player, "It appears that those present when the glyph was made whole received a [vision].");
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							Roben.SayTo(player, "You must not let this occur " + player.GetName(0, false) + "! I am familar with [Lyonesse]. I suggest that you gather a strong group of adventurers in order to succeed in this endeavor!");
							break;
						case 2:
							Roben.SayTo(player, "Where you able to defeat the cult of the dark lord Arawn?");
							break;
					}
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "vision":
							Roben.SayTo(player, "They speak of a broken cathedral located within the borders of Lyonesse. The glyph was able to show the new [occupants] of this cathedral.");
							break;
						case "occupants":
							Roben.SayTo(player, "Occupants that worship not the church of Albion, but the dark lord Arawn! Magess Axton requests that you gather a group and destroy the leader of these dark disciples. She believes these worshippers of Arawan strive to [break the light of camelot] and establish their own religion within our realm.");
							break;
						case "break the light of camelot":
							player.Out.SendQuestSubscribeCommand(Roben, QuestMgr.GetIDForQuestType(typeof(Church_50)), "Will you help Roben [Church Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "Lyonesse":
							Roben.SayTo(player, "The cathedral that Axton speaks of lies deep at the heart of that land, behind the Pikeman, across from the Trees. Its remaining walls can be seen at great distances during the day so you should not miss it. I would travel with thee, but my services are required elswhere. Fare thee well " + player.CharacterClass.Name + ".");
							break;

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Church_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Cleric &&
				player.CharacterClass.ID != (byte) eCharacterClass.Paladin)
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
			Church_50 quest = player.IsDoingQuest(typeof (Church_50)) as Church_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Church_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Roben.CanGiveQuest(typeof (Church_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Church_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Roben.GiveQuest(typeof (Church_50), player, 1))
					return;;

				Roben.SayTo(player, "You must not let this occur " + player.GetName(0, false) + "! I am familar with [Lyonesse]. I suggest that you gather a strong group of adventurers in order to succeed in this endeavor!");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Passage to Eternity (Level 50 Church Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Gather a strong group of adventures and travel to the ancient temple of Arwan. This temple can be found within Lyonesse, surrounded by the dark one's priests. Only by slaying their leader can this evil be stopped!";
					case 2:
						return "[Step #2] Return the statue of Arawn to Roben Fraomar for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Church_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Blythe.Name)
				{
					m_questPlayer.Out.SendMessage("As you search the dead body of sister Blythe, you find a sacred " + statue_of_arawn.Name + ", bring it to " + Roben.Name + " has proof of your success.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, statue_of_arawn);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Roben.Name && gArgs.Item.Id_nb == statue_of_arawn.Id_nb)
				{
					RemoveItem(player, statue_of_arawn, true);
					Roben.SayTo(player, "You have earned this Epic Armour, wear it with honour!");

					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, statue_of_arawn, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Cleric)
			{
				GiveItem(m_questPlayer, ClericEpicBoots);
				GiveItem(m_questPlayer, ClericEpicArms);
				GiveItem(m_questPlayer, ClericEpicGloves);
				GiveItem(m_questPlayer, ClericEpicHelm);
				GiveItem(m_questPlayer, ClericEpicVest);
				GiveItem(m_questPlayer, ClericEpicLegs);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Paladin)
			{
				GiveItem(m_questPlayer, PaladinEpicBoots);
				GiveItem(m_questPlayer, PaladinEpicArms);
				GiveItem(m_questPlayer, PaladinEpicGloves);
				GiveItem(m_questPlayer, PaladinEpicHelm);
				GiveItem(m_questPlayer, PaladinEpicVest);
				GiveItem(m_questPlayer, PaladinEpicLegs);
			}

			m_questPlayer.GainExperience(1937768448, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Roben
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Roben 
        *#28 give her the ball of flame
        *#29 talk with Roben about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Bernor's Numinous Boots 
            *Bernor's Numinous Coif
            *Bernor's Numinous Gloves
            *Bernor's Numinous Hauberk
            *Bernor's Numinous Legs
            *Bernor's Numinous Sleeves
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
