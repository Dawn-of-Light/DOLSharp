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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Seer_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Desire of a God";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Inaksha = null; // Start NPC
		private static GameNPC Loken = null; // Mob to kill
		private static GameNPC Miri = null; // Trainer for reward

		private static ItemTemplate ball_of_flame = null; //ball of flame
		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate HealerEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate HealerEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate HealerEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate HealerEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate HealerEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate HealerEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate ShamanEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ShamanEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ShamanEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ShamanEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ShamanEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ShamanEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Seer_50() : base()
		{
		}

		public Seer_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Seer_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Seer_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Inaksha", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Inaksha , creating it ...");
				Inaksha = new GameNPC();
				Inaksha.Model = 193;
				Inaksha.Name = "Inaksha";
				Inaksha.GuildName = "";
				Inaksha.Realm = eRealm.Midgard;
				Inaksha.CurrentRegionID = 100;
				Inaksha.Size = 50;
				Inaksha.Level = 41;
				Inaksha.X = 805929;
				Inaksha.Y = 702449;
				Inaksha.Z = 4960;
				Inaksha.Heading = 2116;
				Inaksha.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Inaksha.SaveIntoDatabase();
				}

			}
			else
				Inaksha = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Loken", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Loken , creating it ...");
				Loken = new GameNPC();
				Loken.Model = 212;
				Loken.Name = "Loken";
				Loken.GuildName = "";
				Loken.Realm = eRealm.None;
				Loken.CurrentRegionID = 100;
				Loken.Size = 50;
				Loken.Level = 65;
				Loken.X = 636784;
				Loken.Y = 762433;
				Loken.Z = 4596;
				Loken.Heading = 3777;
				Loken.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Loken.SaveIntoDatabase();
				}

			}
			else
				Loken = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Miri", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Miri , creating it ...");
				Miri = new GameNPC();
				Miri.Model = 220;
				Miri.Name = "Miri";
				Miri.GuildName = "";
				Miri.Realm = eRealm.Midgard;
				Miri.CurrentRegionID = 101;
				Miri.Size = 50;
				Miri.Level = 43;
				Miri.X = 30641;
				Miri.Y = 32093;
				Miri.Z = 8305;
				Miri.Heading = 3037;
				Miri.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Miri.SaveIntoDatabase();
				}

			}
			else
				Miri = npcs[0];
			// end npc

			#endregion

			#region defineItems

			ball_of_flame = GameServer.Database.FindObjectByKey<ItemTemplate>("ball_of_flame");
			if (ball_of_flame == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find ball_of_flame , creating it ...");
				ball_of_flame = new ItemTemplate();
				ball_of_flame.Id_nb = "ball_of_flame";
				ball_of_flame.Name = "Ball of Flame";
				ball_of_flame.Level = 8;
				ball_of_flame.Item_Type = 29;
				ball_of_flame.Model = 601;
				ball_of_flame.IsDropable = false;
				ball_of_flame.IsPickable = false;
				ball_of_flame.DPS_AF = 0;
				ball_of_flame.SPD_ABS = 0;
				ball_of_flame.Object_Type = 41;
				ball_of_flame.Hand = 0;
				ball_of_flame.Type_Damage = 0;
				ball_of_flame.Quality = 100;
				ball_of_flame.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ball_of_flame);
				}
			}

// end item
			sealed_pouch = GameServer.Database.FindObjectByKey<ItemTemplate>("sealed_pouch");
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
					GameServer.Database.AddObject(sealed_pouch);
				}
			}
// end item

			//Valhalla Touched Boots
			HealerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicBoots");
			if (HealerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Boots , creating it ...");
				HealerEpicBoots = new ItemTemplate();
				HealerEpicBoots.Id_nb = "HealerEpicBoots";
				HealerEpicBoots.Name = "Valhalla Touched Boots";
				HealerEpicBoots.Level = 50;
				HealerEpicBoots.Item_Type = 23;
				HealerEpicBoots.Model = 702;
				HealerEpicBoots.IsDropable = true;
				HealerEpicBoots.IsPickable = true;
				HealerEpicBoots.DPS_AF = 100;
				HealerEpicBoots.SPD_ABS = 27;
				HealerEpicBoots.Object_Type = 35;
				HealerEpicBoots.Quality = 100;
				HealerEpicBoots.Weight = 22;
				HealerEpicBoots.Bonus = 35;
				HealerEpicBoots.MaxCondition = 50000;
				HealerEpicBoots.MaxDurability = 50000;
				HealerEpicBoots.Condition = 50000;
				HealerEpicBoots.Durability = 50000;

				HealerEpicBoots.Bonus1 = 12;
				HealerEpicBoots.Bonus1Type = (int) eStat.CON;

				HealerEpicBoots.Bonus2 = 12;
				HealerEpicBoots.Bonus2Type = (int) eStat.DEX;

				HealerEpicBoots.Bonus3 = 12;
				HealerEpicBoots.Bonus3Type = (int) eStat.QUI;

				HealerEpicBoots.Bonus4 = 21;
				HealerEpicBoots.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicBoots);
				}
			}
//end item
			//Valhalla Touched Coif 
			HealerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicHelm");
			if (HealerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Helm , creating it ...");
				HealerEpicHelm = new ItemTemplate();
				HealerEpicHelm.Id_nb = "HealerEpicHelm";
				HealerEpicHelm.Name = "Valhalla Touched Coif";
				HealerEpicHelm.Level = 50;
				HealerEpicHelm.Item_Type = 21;
				HealerEpicHelm.Model = 1291; //NEED TO WORK ON..
				HealerEpicHelm.IsDropable = true;
				HealerEpicHelm.IsPickable = true;
				HealerEpicHelm.DPS_AF = 100;
				HealerEpicHelm.SPD_ABS = 27;
				HealerEpicHelm.Object_Type = 35;
				HealerEpicHelm.Quality = 100;
				HealerEpicHelm.Weight = 22;
				HealerEpicHelm.Bonus = 35;
				HealerEpicHelm.MaxCondition = 50000;
				HealerEpicHelm.MaxDurability = 50000;
				HealerEpicHelm.Condition = 50000;
				HealerEpicHelm.Durability = 50000;

				HealerEpicHelm.Bonus1 = 4;
				HealerEpicHelm.Bonus1Type = (int) eProperty.Skill_Augmentation;

				HealerEpicHelm.Bonus2 = 18;
				HealerEpicHelm.Bonus2Type = (int) eStat.PIE;

				HealerEpicHelm.Bonus3 = 4;
				HealerEpicHelm.Bonus3Type = (int) eResist.Slash;

				HealerEpicHelm.Bonus4 = 6;
				HealerEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicHelm);
				}

			}
//end item
			//Valhalla Touched Gloves 
			HealerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicGloves");
			if (HealerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Gloves , creating it ...");
				HealerEpicGloves = new ItemTemplate();
				HealerEpicGloves.Id_nb = "HealerEpicGloves";
				HealerEpicGloves.Name = "Valhalla Touched Gloves ";
				HealerEpicGloves.Level = 50;
				HealerEpicGloves.Item_Type = 22;
				HealerEpicGloves.Model = 701;
				HealerEpicGloves.IsDropable = true;
				HealerEpicGloves.IsPickable = true;
				HealerEpicGloves.DPS_AF = 100;
				HealerEpicGloves.SPD_ABS = 27;
				HealerEpicGloves.Object_Type = 35;
				HealerEpicGloves.Quality = 100;
				HealerEpicGloves.Weight = 22;
				HealerEpicGloves.Bonus = 35;
				HealerEpicGloves.MaxCondition = 50000;
				HealerEpicGloves.MaxDurability = 50000;
				HealerEpicGloves.Condition = 50000;
				HealerEpicGloves.Durability = 50000;

				HealerEpicGloves.Bonus1 = 4;
				HealerEpicGloves.Bonus1Type = (int) eProperty.Skill_Mending;

				HealerEpicGloves.Bonus2 = 16;
				HealerEpicGloves.Bonus2Type = (int) eStat.PIE;

				HealerEpicGloves.Bonus3 = 4;
				HealerEpicGloves.Bonus3Type = (int) eResist.Crush;

				HealerEpicGloves.Bonus4 = 6;
				HealerEpicGloves.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicGloves);
				}

			}
			//Valhalla Touched Hauberk 
			HealerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicVest");
			if (HealerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Vest , creating it ...");
				HealerEpicVest = new ItemTemplate();
				HealerEpicVest.Id_nb = "HealerEpicVest";
				HealerEpicVest.Name = "Valhalla Touched Haukberk";
				HealerEpicVest.Level = 50;
				HealerEpicVest.Item_Type = 25;
				HealerEpicVest.Model = 698;
				HealerEpicVest.IsDropable = true;
				HealerEpicVest.IsPickable = true;
				HealerEpicVest.DPS_AF = 100;
				HealerEpicVest.SPD_ABS = 27;
				HealerEpicVest.Object_Type = 35;
				HealerEpicVest.Quality = 100;
				HealerEpicVest.Weight = 22;
				HealerEpicVest.Bonus = 35;
				HealerEpicVest.MaxCondition = 50000;
				HealerEpicVest.MaxDurability = 50000;
				HealerEpicVest.Condition = 50000;
				HealerEpicVest.Durability = 50000;

				HealerEpicVest.Bonus1 = 16;
				HealerEpicVest.Bonus1Type = (int) eStat.CON;

				HealerEpicVest.Bonus2 = 16;
				HealerEpicVest.Bonus2Type = (int) eStat.PIE;

				HealerEpicVest.Bonus3 = 8;
				HealerEpicVest.Bonus3Type = (int) eResist.Cold;

				HealerEpicVest.Bonus4 = 10;
				HealerEpicVest.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicVest);
				}

			}
			//Valhalla Touched Legs 
			HealerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicLegs");
			if (HealerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Legs , creating it ...");
				HealerEpicLegs = new ItemTemplate();
				HealerEpicLegs.Id_nb = "HealerEpicLegs";
				HealerEpicLegs.Name = "Valhalla Touched Legs";
				HealerEpicLegs.Level = 50;
				HealerEpicLegs.Item_Type = 27;
				HealerEpicLegs.Model = 699;
				HealerEpicLegs.IsDropable = true;
				HealerEpicLegs.IsPickable = true;
				HealerEpicLegs.DPS_AF = 100;
				HealerEpicLegs.SPD_ABS = 27;
				HealerEpicLegs.Object_Type = 35;
				HealerEpicLegs.Quality = 100;
				HealerEpicLegs.Weight = 22;
				HealerEpicLegs.Bonus = 35;
				HealerEpicLegs.MaxCondition = 50000;
				HealerEpicLegs.MaxDurability = 50000;
				HealerEpicLegs.Condition = 50000;
				HealerEpicLegs.Durability = 50000;

				HealerEpicLegs.Bonus1 = 15;
				HealerEpicLegs.Bonus1Type = (int) eStat.STR;

				HealerEpicLegs.Bonus2 = 16;
				HealerEpicLegs.Bonus2Type = (int) eStat.CON;

				HealerEpicLegs.Bonus3 = 10;
				HealerEpicLegs.Bonus3Type = (int) eResist.Spirit;

				HealerEpicLegs.Bonus4 = 10;
				HealerEpicLegs.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicLegs);
				}

			}
			//Valhalla Touched Sleeves 
			HealerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicArms");
			if (HealerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healer Epic Arms , creating it ...");
				HealerEpicArms = new ItemTemplate();
				HealerEpicArms.Id_nb = "HealerEpicArms";
				HealerEpicArms.Name = "Valhalla Touched Sleeves";
				HealerEpicArms.Level = 50;
				HealerEpicArms.Item_Type = 28;
				HealerEpicArms.Model = 700;
				HealerEpicArms.IsDropable = true;
				HealerEpicArms.IsPickable = true;
				HealerEpicArms.DPS_AF = 100;
				HealerEpicArms.SPD_ABS = 27;
				HealerEpicArms.Object_Type = 35;
				HealerEpicArms.Quality = 100;
				HealerEpicArms.Weight = 22;
				HealerEpicArms.Bonus = 35;
				HealerEpicArms.MaxCondition = 50000;
				HealerEpicArms.MaxDurability = 50000;
				HealerEpicArms.Condition = 50000;
				HealerEpicArms.Durability = 50000;

				HealerEpicArms.Bonus1 = 4;
				HealerEpicArms.Bonus1Type = (int) eProperty.Skill_Mending;

				HealerEpicArms.Bonus2 = 13;
				HealerEpicArms.Bonus2Type = (int) eStat.STR;

				HealerEpicArms.Bonus3 = 15;
				HealerEpicArms.Bonus3Type = (int) eStat.PIE;

				HealerEpicArms.Bonus4 = 6;
				HealerEpicArms.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicArms);
				}

			}
			//Subterranean Boots 
			ShamanEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicBoots");
			if (ShamanEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Boots , creating it ...");
				ShamanEpicBoots = new ItemTemplate();
				ShamanEpicBoots.Id_nb = "ShamanEpicBoots";
				ShamanEpicBoots.Name = "Subterranean Boots";
				ShamanEpicBoots.Level = 50;
				ShamanEpicBoots.Item_Type = 23;
				ShamanEpicBoots.Model = 770;
				ShamanEpicBoots.IsDropable = true;
				ShamanEpicBoots.IsPickable = true;
				ShamanEpicBoots.DPS_AF = 100;
				ShamanEpicBoots.SPD_ABS = 27;
				ShamanEpicBoots.Object_Type = 35;
				ShamanEpicBoots.Quality = 100;
				ShamanEpicBoots.Weight = 22;
				ShamanEpicBoots.Bonus = 35;
				ShamanEpicBoots.MaxCondition = 50000;
				ShamanEpicBoots.MaxDurability = 50000;
				ShamanEpicBoots.Condition = 50000;
				ShamanEpicBoots.Durability = 50000;

				ShamanEpicBoots.Bonus1 = 13;
				ShamanEpicBoots.Bonus1Type = (int) eStat.DEX;

				ShamanEpicBoots.Bonus2 = 13;
				ShamanEpicBoots.Bonus2Type = (int) eStat.QUI;

				ShamanEpicBoots.Bonus3 = 39;
				ShamanEpicBoots.Bonus3Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicBoots);
				}

			}
			//Subterranean Coif 
			ShamanEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicHelm");
			if (ShamanEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Helm , creating it ...");
				ShamanEpicHelm = new ItemTemplate();
				ShamanEpicHelm.Id_nb = "ShamanEpicHelm";
				ShamanEpicHelm.Name = "Subterranean Coif";
				ShamanEpicHelm.Level = 50;
				ShamanEpicHelm.Item_Type = 21;
				ShamanEpicHelm.Model = 63; //NEED TO WORK ON..
				ShamanEpicHelm.IsDropable = true;
				ShamanEpicHelm.IsPickable = true;
				ShamanEpicHelm.DPS_AF = 100;
				ShamanEpicHelm.SPD_ABS = 27;
				ShamanEpicHelm.Object_Type = 35;
				ShamanEpicHelm.Quality = 100;
				ShamanEpicHelm.Weight = 22;
				ShamanEpicHelm.Bonus = 35;
				ShamanEpicHelm.MaxCondition = 50000;
				ShamanEpicHelm.MaxDurability = 50000;
				ShamanEpicHelm.Condition = 50000;
				ShamanEpicHelm.Durability = 50000;

				ShamanEpicHelm.Bonus1 = 4;
				ShamanEpicHelm.Bonus1Type = (int) eProperty.Skill_Mending;

				ShamanEpicHelm.Bonus2 = 18;
				ShamanEpicHelm.Bonus2Type = (int) eStat.PIE;

				ShamanEpicHelm.Bonus3 = 4;
				ShamanEpicHelm.Bonus3Type = (int) eResist.Thrust;

				ShamanEpicHelm.Bonus4 = 6;
				ShamanEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicHelm);
				}

			}
			//Subterranean Gloves 
			ShamanEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicGloves");
			if (ShamanEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Gloves , creating it ...");
				ShamanEpicGloves = new ItemTemplate();
				ShamanEpicGloves.Id_nb = "ShamanEpicGloves";
				ShamanEpicGloves.Name = "Subterranean Gloves";
				ShamanEpicGloves.Level = 50;
				ShamanEpicGloves.Item_Type = 22;
				ShamanEpicGloves.Model = 769;
				ShamanEpicGloves.IsDropable = true;
				ShamanEpicGloves.IsPickable = true;
				ShamanEpicGloves.DPS_AF = 100;
				ShamanEpicGloves.SPD_ABS = 27;
				ShamanEpicGloves.Object_Type = 35;
				ShamanEpicGloves.Quality = 100;
				ShamanEpicGloves.Weight = 22;
				ShamanEpicGloves.Bonus = 35;
				ShamanEpicGloves.MaxCondition = 50000;
				ShamanEpicGloves.MaxDurability = 50000;
				ShamanEpicGloves.Condition = 50000;
				ShamanEpicGloves.Durability = 50000;

				ShamanEpicGloves.Bonus1 = 4;
				ShamanEpicGloves.Bonus1Type = (int) eProperty.Skill_Subterranean;

				ShamanEpicGloves.Bonus2 = 18;
				ShamanEpicGloves.Bonus2Type = (int) eStat.PIE;

				ShamanEpicGloves.Bonus3 = 4;
				ShamanEpicGloves.Bonus3Type = (int) eResist.Crush;

				ShamanEpicGloves.Bonus4 = 6;
				ShamanEpicGloves.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicGloves);
				}

			}
			//Subterranean Hauberk 
			ShamanEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicVest");
			if (ShamanEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Vest , creating it ...");
				ShamanEpicVest = new ItemTemplate();
				ShamanEpicVest.Id_nb = "ShamanEpicVest";
				ShamanEpicVest.Name = "Subterranean Hauberk";
				ShamanEpicVest.Level = 50;
				ShamanEpicVest.Item_Type = 25;
				ShamanEpicVest.Model = 766;
				ShamanEpicVest.IsDropable = true;
				ShamanEpicVest.IsPickable = true;
				ShamanEpicVest.DPS_AF = 100;
				ShamanEpicVest.SPD_ABS = 27;
				ShamanEpicVest.Object_Type = 35;
				ShamanEpicVest.Quality = 100;
				ShamanEpicVest.Weight = 22;
				ShamanEpicVest.Bonus = 35;
				ShamanEpicVest.MaxCondition = 50000;
				ShamanEpicVest.MaxDurability = 50000;
				ShamanEpicVest.Condition = 50000;
				ShamanEpicVest.Durability = 50000;

				ShamanEpicVest.Bonus1 = 16;
				ShamanEpicVest.Bonus1Type = (int) eStat.CON;

				ShamanEpicVest.Bonus2 = 16;
				ShamanEpicVest.Bonus2Type = (int) eStat.PIE;

				ShamanEpicVest.Bonus3 = 10;
				ShamanEpicVest.Bonus3Type = (int) eResist.Matter;

				ShamanEpicVest.Bonus4 = 8;
				ShamanEpicVest.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicVest);
				}

			}
			//Subterranean Legs 
			ShamanEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicLegs");
			if (ShamanEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Legs , creating it ...");
				ShamanEpicLegs = new ItemTemplate();
				ShamanEpicLegs.Id_nb = "ShamanEpicLegs";
				ShamanEpicLegs.Name = "Subterranean Legs";
				ShamanEpicLegs.Level = 50;
				ShamanEpicLegs.Item_Type = 27;
				ShamanEpicLegs.Model = 767;
				ShamanEpicLegs.IsDropable = true;
				ShamanEpicLegs.IsPickable = true;
				ShamanEpicLegs.DPS_AF = 100;
				ShamanEpicLegs.SPD_ABS = 27;
				ShamanEpicLegs.Object_Type = 35;
				ShamanEpicLegs.Quality = 100;
				ShamanEpicLegs.Weight = 22;
				ShamanEpicLegs.Bonus = 35;
				ShamanEpicLegs.MaxCondition = 50000;
				ShamanEpicLegs.MaxDurability = 50000;
				ShamanEpicLegs.Condition = 50000;
				ShamanEpicLegs.Durability = 50000;

				ShamanEpicLegs.Bonus1 = 16;
				ShamanEpicLegs.Bonus1Type = (int) eStat.CON;

				ShamanEpicLegs.Bonus2 = 16;
				ShamanEpicLegs.Bonus2Type = (int) eStat.DEX;

				ShamanEpicLegs.Bonus3 = 8;
				ShamanEpicLegs.Bonus3Type = (int) eResist.Cold;

				ShamanEpicLegs.Bonus4 = 10;
				ShamanEpicLegs.Bonus4Type = (int) eResist.Spirit;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicLegs);
				}

			}
			//Subterranean Sleeves 
			ShamanEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicArms");
			if (ShamanEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Arms , creating it ...");
				ShamanEpicArms = new ItemTemplate();
				ShamanEpicArms.Id_nb = "ShamanEpicArms";
				ShamanEpicArms.Name = "Subterranean Sleeves";
				ShamanEpicArms.Level = 50;
				ShamanEpicArms.Item_Type = 28;
				ShamanEpicArms.Model = 768;
				ShamanEpicArms.IsDropable = true;
				ShamanEpicArms.IsPickable = true;
				ShamanEpicArms.DPS_AF = 100;
				ShamanEpicArms.SPD_ABS = 27;
				ShamanEpicArms.Object_Type = 35;
				ShamanEpicArms.Quality = 100;
				ShamanEpicArms.Weight = 22;
				ShamanEpicArms.Bonus = 35;
				ShamanEpicArms.MaxCondition = 50000;
				ShamanEpicArms.MaxDurability = 50000;
				ShamanEpicArms.Condition = 50000;
				ShamanEpicArms.Durability = 50000;

				ShamanEpicArms.Bonus1 = 4;
				ShamanEpicArms.Bonus1Type = (int) eProperty.Skill_Augmentation;

				ShamanEpicArms.Bonus2 = 12;
				ShamanEpicArms.Bonus2Type = (int) eStat.STR;

				ShamanEpicArms.Bonus3 = 18;
				ShamanEpicArms.Bonus3Type = (int) eStat.PIE;

				ShamanEpicArms.Bonus4 = 6;
				ShamanEpicArms.Bonus4Type = (int) eResist.Energy;


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicArms);
				}

			}
//Shaman Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.AddHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we bring to Inaksha the possibility to give this quest to players */
			Inaksha.AddQuestToGive(typeof (Seer_50));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.RemoveHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we remove to Inaksha the possibility to give this quest to players */
			Inaksha.RemoveQuestToGive(typeof (Seer_50));
		}

		protected static void TalkToInaksha(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Inaksha, QuestMgr.GetIDForQuestType(typeof(Seer_50)), "Will you help Inaksha [Seer Level 50 Epic]?");
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
								GiveItem(Inaksha, player, sealed_pouch);
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
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

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

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Shaman &&
				player.CharacterClass.ID != (byte) eCharacterClass.Healer)
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
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Seer_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Inaksha.GiveQuest(typeof (Seer_50), player, 1))
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
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Seer_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Loken.Name)
				{
					m_questPlayer.Out.SendMessage("You get a ball of flame", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, ball_of_flame);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Inaksha.Name && gArgs.Item.Id_nb == ball_of_flame.Id_nb)
				{
					RemoveItem(Inaksha, player, ball_of_flame);
					Inaksha.SayTo(player, "So it seems Logan's [dead]");
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Miri.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					Miri.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, ball_of_flame, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Miri, m_questPlayer, sealed_pouch);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shaman)
				{
					GiveItem(m_questPlayer, ShamanEpicArms);
					GiveItem(m_questPlayer, ShamanEpicBoots);
					GiveItem(m_questPlayer, ShamanEpicGloves);
					GiveItem(m_questPlayer, ShamanEpicHelm);
					GiveItem(m_questPlayer, ShamanEpicLegs);
					GiveItem(m_questPlayer, ShamanEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Healer)
				{
					GiveItem(m_questPlayer, HealerEpicArms);
					GiveItem(m_questPlayer, HealerEpicBoots);
					GiveItem(m_questPlayer, HealerEpicGloves);
					GiveItem(m_questPlayer, HealerEpicHelm);
					GiveItem(m_questPlayer, HealerEpicLegs);
					GiveItem(m_questPlayer, HealerEpicVest);
				}

				m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 1937768448, true);
				//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
			}
			else
			{
				m_questPlayer.Out.SendMessage("You do not have enough free space in your inventory!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
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
