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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Rogue_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "War Concluded";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Masrim = null; // Start NPC
		private static GameNPC Oona = null; // Mob to kill
		private static GameNPC MorlinCaan = null; // Trainer for reward

		private static ItemTemplate oona_head = null; //ball of flame
		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate HunterEpicBoots = null; //Call of the Hunt Boots 
		private static ItemTemplate HunterEpicHelm = null; //Call of the Hunt Coif 
		private static ItemTemplate HunterEpicGloves = null; //Call of the Hunt Gloves 
		private static ItemTemplate HunterEpicVest = null; //Call of the Hunt Hauberk 
		private static ItemTemplate HunterEpicLegs = null; //Call of the Hunt Legs 
		private static ItemTemplate HunterEpicArms = null; //Call of the Hunt Sleeves 
		private static ItemTemplate ShadowbladeEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ShadowbladeEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ShadowbladeEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ShadowbladeEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ShadowbladeEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ShadowbladeEpicArms = null; //Shadow Shrouded Sleeves         

		// Constructors
		public Rogue_50() : base()
		{
		}

		public Rogue_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Rogue_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Rogue_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

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
				Masrim.Realm = (byte) eRealm.Midgard;
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
				Oona.Realm = (byte) eRealm.None;
				Oona.RegionId = 100;
				Oona.Size = 50;
				Oona.Level = 65;
				Oona.Position = new Point(607233, 786850, 4384);
				Oona.Heading = 3891;
				Oona.Flags = 1;
				Oona.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Oona.SaveIntoDatabase();
				}

			}
			else
				Oona = npcs[0];
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
				MorlinCaan.Realm = (byte) eRealm.Midgard;
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

			oona_head = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "oona_head");
			if (oona_head == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Oona's Head , creating it ...");
				oona_head = new ItemTemplate();
				oona_head.ItemTemplateID = "oona_head";
				oona_head.Name = "Oona's Head";
				oona_head.Level = 8;
				oona_head.Item_Type = 29;
				oona_head.Model = 503;
				oona_head.IsDropable = false;
				oona_head.IsPickable = false;
				oona_head.DPS_AF = 0;
				oona_head.SPD_ABS = 0;
				oona_head.Object_Type = 41;
				oona_head.Hand = 0;
				oona_head.Type_Damage = 0;
				oona_head.Quality = 100;
				oona_head.MaxQuality = 100;
				oona_head.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(oona_head);
				}

			}

			sealed_pouch = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sealed_pouch");
			if (sealed_pouch == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sealed Pouch , creating it ...");
				sealed_pouch = new ItemTemplate();
				sealed_pouch.ItemTemplateID = "sealed_pouch";
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
				sealed_pouch.MaxQuality = 100;
				sealed_pouch.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(sealed_pouch);
				}

			}
// end item

			HunterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicBoots");
			if (HunterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Boots , creating it ...");
				HunterEpicBoots = new ItemTemplate();
				HunterEpicBoots.ItemTemplateID = "HunterEpicBoots";
				HunterEpicBoots.Name = "Call of the Hunt Boots";
				HunterEpicBoots.Level = 50;
				HunterEpicBoots.Item_Type = 23;
				HunterEpicBoots.Model = 760;
				HunterEpicBoots.IsDropable = true;
				HunterEpicBoots.IsPickable = true;
				HunterEpicBoots.DPS_AF = 100;
				HunterEpicBoots.SPD_ABS = 19;
				HunterEpicBoots.Object_Type = 34;
				HunterEpicBoots.Quality = 100;
				HunterEpicBoots.MaxQuality = 100;
				HunterEpicBoots.Weight = 22;
				HunterEpicBoots.Bonus = 35;
				HunterEpicBoots.MaxCondition = 50000;
				HunterEpicBoots.MaxDurability = 50000;
				HunterEpicBoots.Condition = 50000;
				HunterEpicBoots.Durability = 50000;

				HunterEpicBoots.Bonus1 = 19;
				HunterEpicBoots.Bonus1Type = (int) eStat.CON;

				HunterEpicBoots.Bonus2 = 19;
				HunterEpicBoots.Bonus2Type = (int) eStat.DEX;

				HunterEpicBoots.Bonus3 = 10;
				HunterEpicBoots.Bonus3Type = (int) eResist.Thrust;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicBoots);
				}

			}
//end item
			//Call of the Hunt Coif 
			HunterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicHelm");
			if (HunterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Helm , creating it ...");
				HunterEpicHelm = new ItemTemplate();
				HunterEpicHelm.ItemTemplateID = "HunterEpicHelm";
				HunterEpicHelm.Name = "Call of the Hunt Coif";
				HunterEpicHelm.Level = 50;
				HunterEpicHelm.Item_Type = 21;
				HunterEpicHelm.Model = 829; //NEED TO WORK ON..
				HunterEpicHelm.IsDropable = true;
				HunterEpicHelm.IsPickable = true;
				HunterEpicHelm.DPS_AF = 100;
				HunterEpicHelm.SPD_ABS = 19;
				HunterEpicHelm.Object_Type = 34;
				HunterEpicHelm.Quality = 100;
				HunterEpicHelm.MaxQuality = 100;
				HunterEpicHelm.Weight = 22;
				HunterEpicHelm.Bonus = 35;
				HunterEpicHelm.MaxCondition = 50000;
				HunterEpicHelm.MaxDurability = 50000;
				HunterEpicHelm.Condition = 50000;
				HunterEpicHelm.Durability = 50000;

				HunterEpicHelm.Bonus1 = 3;
				HunterEpicHelm.Bonus1Type = (int) eProperty.Skill_Spear;

				HunterEpicHelm.Bonus2 = 3;
				HunterEpicHelm.Bonus2Type = (int) eProperty.Skill_Stealth;

				HunterEpicHelm.Bonus3 = 3;
				HunterEpicHelm.Bonus3Type = (int) eProperty.Skill_Composite;

				HunterEpicHelm.Bonus4 = 19;
				HunterEpicHelm.Bonus4Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicHelm);
				}

			}
//end item
			//Call of the Hunt Gloves 
			HunterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicGloves");
			if (HunterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Gloves , creating it ...");
				HunterEpicGloves = new ItemTemplate();
				HunterEpicGloves.ItemTemplateID = "HunterEpicGloves";
				HunterEpicGloves.Name = "Call of the Hunt Gloves ";
				HunterEpicGloves.Level = 50;
				HunterEpicGloves.Item_Type = 22;
				HunterEpicGloves.Model = 759;
				HunterEpicGloves.IsDropable = true;
				HunterEpicGloves.IsPickable = true;
				HunterEpicGloves.DPS_AF = 100;
				HunterEpicGloves.SPD_ABS = 19;
				HunterEpicGloves.Object_Type = 34;
				HunterEpicGloves.Quality = 100;
				HunterEpicGloves.MaxQuality = 100;
				HunterEpicGloves.Weight = 22;
				HunterEpicGloves.Bonus = 35;
				HunterEpicGloves.MaxCondition = 50000;
				HunterEpicGloves.MaxDurability = 50000;
				HunterEpicGloves.Condition = 50000;
				HunterEpicGloves.Durability = 50000;

				HunterEpicGloves.Bonus1 = 5;
				HunterEpicGloves.Bonus1Type = (int) eProperty.Skill_Composite;

				HunterEpicGloves.Bonus2 = 15;
				HunterEpicGloves.Bonus2Type = (int) eStat.QUI;

				HunterEpicGloves.Bonus3 = 33;
				HunterEpicGloves.Bonus3Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicGloves);
				}

			}
			//Call of the Hunt Hauberk 
			HunterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicVest");
			if (HunterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Vest , creating it ...");
				HunterEpicVest = new ItemTemplate();
				HunterEpicVest.ItemTemplateID = "HunterEpicVest";
				HunterEpicVest.Name = "Call of the Hunt Jerkin";
				HunterEpicVest.Level = 50;
				HunterEpicVest.Item_Type = 25;
				HunterEpicVest.Model = 756;
				HunterEpicVest.IsDropable = true;
				HunterEpicVest.IsPickable = true;
				HunterEpicVest.DPS_AF = 100;
				HunterEpicVest.SPD_ABS = 19;
				HunterEpicVest.Object_Type = 34;
				HunterEpicVest.Quality = 100;
				HunterEpicVest.MaxQuality = 100;
				HunterEpicVest.Weight = 22;
				HunterEpicVest.Bonus = 35;
				HunterEpicVest.MaxCondition = 50000;
				HunterEpicVest.MaxDurability = 50000;
				HunterEpicVest.Condition = 50000;
				HunterEpicVest.Durability = 50000;

				HunterEpicVest.Bonus1 = 13;
				HunterEpicVest.Bonus1Type = (int) eStat.STR;

				HunterEpicVest.Bonus2 = 15;
				HunterEpicVest.Bonus2Type = (int) eStat.CON;

				HunterEpicVest.Bonus3 = 13;
				HunterEpicVest.Bonus3Type = (int) eStat.DEX;

				HunterEpicVest.Bonus4 = 6;
				HunterEpicVest.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicVest);
				}

			}
			//Call of the Hunt Legs 
			HunterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicLegs");
			if (HunterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunters Epic Legs , creating it ...");
				HunterEpicLegs = new ItemTemplate();
				HunterEpicLegs.ItemTemplateID = "HunterEpicLegs";
				HunterEpicLegs.Name = "Call of the Hunt Legs";
				HunterEpicLegs.Level = 50;
				HunterEpicLegs.Item_Type = 27;
				HunterEpicLegs.Model = 757;
				HunterEpicLegs.IsDropable = true;
				HunterEpicLegs.IsPickable = true;
				HunterEpicLegs.DPS_AF = 100;
				HunterEpicLegs.SPD_ABS = 19;
				HunterEpicLegs.Object_Type = 34;
				HunterEpicLegs.Quality = 100;
				HunterEpicLegs.MaxQuality = 100;
				HunterEpicLegs.Weight = 22;
				HunterEpicLegs.Bonus = 35;
				HunterEpicLegs.MaxCondition = 50000;
				HunterEpicLegs.MaxDurability = 50000;
				HunterEpicLegs.Condition = 50000;
				HunterEpicLegs.Durability = 50000;

				HunterEpicLegs.Bonus1 = 15;
				HunterEpicLegs.Bonus1Type = (int) eStat.CON;

				HunterEpicLegs.Bonus2 = 15;
				HunterEpicLegs.Bonus2Type = (int) eStat.DEX;

				HunterEpicLegs.Bonus3 = 7;
				HunterEpicLegs.Bonus3Type = (int) eStat.QUI;

				HunterEpicLegs.Bonus4 = 12;
				HunterEpicLegs.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicLegs);
				}

			}
			//Call of the Hunt Sleeves 
			HunterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "HunterEpicArms");
			if (HunterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hunter Epic Arms , creating it ...");
				HunterEpicArms = new ItemTemplate();
				HunterEpicArms.ItemTemplateID = "HunterEpicArms";
				HunterEpicArms.Name = "Call of the Hunt Sleeves";
				HunterEpicArms.Level = 50;
				HunterEpicArms.Item_Type = 28;
				HunterEpicArms.Model = 758;
				HunterEpicArms.IsDropable = true;
				HunterEpicArms.IsPickable = true;
				HunterEpicArms.DPS_AF = 100;
				HunterEpicArms.SPD_ABS = 19;
				HunterEpicArms.Object_Type = 34;
				HunterEpicArms.Quality = 100;
				HunterEpicArms.MaxQuality = 100;
				HunterEpicArms.Weight = 22;
				HunterEpicArms.Bonus = 35;
				HunterEpicArms.MaxCondition = 50000;
				HunterEpicArms.MaxDurability = 50000;
				HunterEpicArms.Condition = 50000;
				HunterEpicArms.Durability = 50000;

				HunterEpicArms.Bonus1 = 15;
				HunterEpicArms.Bonus1Type = (int) eStat.STR;

				HunterEpicArms.Bonus2 = 15;
				HunterEpicArms.Bonus2Type = (int) eStat.QUI;

				HunterEpicArms.Bonus3 = 10;
				HunterEpicArms.Bonus3Type = (int) eResist.Crush;

				HunterEpicArms.Bonus4 = 10;
				HunterEpicArms.Bonus4Type = (int) eResist.Slash;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(HunterEpicArms);
				}

			}
			//Shadow Shrouded Boots 
			ShadowbladeEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicBoots");
			if (ShadowbladeEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Boots , creating it ...");
				ShadowbladeEpicBoots = new ItemTemplate();
				ShadowbladeEpicBoots.ItemTemplateID = "ShadowbladeEpicBoots";
				ShadowbladeEpicBoots.Name = "Shadow Shrouded Boots";
				ShadowbladeEpicBoots.Level = 50;
				ShadowbladeEpicBoots.Item_Type = 23;
				ShadowbladeEpicBoots.Model = 765;
				ShadowbladeEpicBoots.IsDropable = true;
				ShadowbladeEpicBoots.IsPickable = true;
				ShadowbladeEpicBoots.DPS_AF = 100;
				ShadowbladeEpicBoots.SPD_ABS = 10;
				ShadowbladeEpicBoots.Object_Type = 33;
				ShadowbladeEpicBoots.Quality = 100;
				ShadowbladeEpicBoots.MaxQuality = 100;
				ShadowbladeEpicBoots.Weight = 22;
				ShadowbladeEpicBoots.Bonus = 35;
				ShadowbladeEpicBoots.MaxCondition = 50000;
				ShadowbladeEpicBoots.MaxDurability = 50000;
				ShadowbladeEpicBoots.Condition = 50000;
				ShadowbladeEpicBoots.Durability = 50000;

				ShadowbladeEpicBoots.Bonus1 = 5;
				ShadowbladeEpicBoots.Bonus1Type = (int) eProperty.Skill_Stealth;

				ShadowbladeEpicBoots.Bonus2 = 13;
				ShadowbladeEpicBoots.Bonus2Type = (int) eStat.DEX;

				ShadowbladeEpicBoots.Bonus3 = 13;
				ShadowbladeEpicBoots.Bonus3Type = (int) eStat.QUI;

				ShadowbladeEpicBoots.Bonus4 = 6;
				ShadowbladeEpicBoots.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicBoots);
				}

			}
			//Shadow Shrouded Coif 
			ShadowbladeEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicHelm");
			if (ShadowbladeEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Helm , creating it ...");
				ShadowbladeEpicHelm = new ItemTemplate();
				ShadowbladeEpicHelm.ItemTemplateID = "ShadowbladeEpicHelm";
				ShadowbladeEpicHelm.Name = "Shadow Shrouded Coif";
				ShadowbladeEpicHelm.Level = 50;
				ShadowbladeEpicHelm.Item_Type = 21;
				ShadowbladeEpicHelm.Model = 335; //NEED TO WORK ON..
				ShadowbladeEpicHelm.IsDropable = true;
				ShadowbladeEpicHelm.IsPickable = true;
				ShadowbladeEpicHelm.DPS_AF = 100;
				ShadowbladeEpicHelm.SPD_ABS = 10;
				ShadowbladeEpicHelm.Object_Type = 33;
				ShadowbladeEpicHelm.Quality = 100;
				ShadowbladeEpicHelm.MaxQuality = 100;
				ShadowbladeEpicHelm.Weight = 22;
				ShadowbladeEpicHelm.Bonus = 35;
				ShadowbladeEpicHelm.MaxCondition = 50000;
				ShadowbladeEpicHelm.MaxDurability = 50000;
				ShadowbladeEpicHelm.Condition = 50000;
				ShadowbladeEpicHelm.Durability = 50000;

				ShadowbladeEpicHelm.Bonus1 = 10;
				ShadowbladeEpicHelm.Bonus1Type = (int) eStat.STR;

				ShadowbladeEpicHelm.Bonus2 = 12;
				ShadowbladeEpicHelm.Bonus2Type = (int) eStat.CON;

				ShadowbladeEpicHelm.Bonus3 = 10;
				ShadowbladeEpicHelm.Bonus3Type = (int) eStat.DEX;

				ShadowbladeEpicHelm.Bonus4 = 10;
				ShadowbladeEpicHelm.Bonus4Type = (int) eStat.QUI;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicHelm);
				}

			}
			//Shadow Shrouded Gloves 
			ShadowbladeEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicGloves");
			if (ShadowbladeEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Gloves , creating it ...");
				ShadowbladeEpicGloves = new ItemTemplate();
				ShadowbladeEpicGloves.ItemTemplateID = "ShadowbladeEpicGloves";
				ShadowbladeEpicGloves.Name = "Shadow Shrouded Gloves";
				ShadowbladeEpicGloves.Level = 50;
				ShadowbladeEpicGloves.Item_Type = 22;
				ShadowbladeEpicGloves.Model = 764;
				ShadowbladeEpicGloves.IsDropable = true;
				ShadowbladeEpicGloves.IsPickable = true;
				ShadowbladeEpicGloves.DPS_AF = 100;
				ShadowbladeEpicGloves.SPD_ABS = 10;
				ShadowbladeEpicGloves.Object_Type = 33;
				ShadowbladeEpicGloves.Quality = 100;
				ShadowbladeEpicGloves.MaxQuality = 100;
				ShadowbladeEpicGloves.Weight = 22;
				ShadowbladeEpicGloves.Bonus = 35;
				ShadowbladeEpicGloves.MaxCondition = 50000;
				ShadowbladeEpicGloves.MaxDurability = 50000;
				ShadowbladeEpicGloves.Condition = 50000;
				ShadowbladeEpicGloves.Durability = 50000;

				ShadowbladeEpicGloves.Bonus1 = 2;
				ShadowbladeEpicGloves.Bonus1Type = (int) eProperty.Skill_Critical_Strike;

				ShadowbladeEpicGloves.Bonus2 = 12;
				ShadowbladeEpicGloves.Bonus2Type = (int) eStat.QUI;

				ShadowbladeEpicGloves.Bonus3 = 33;
				ShadowbladeEpicGloves.Bonus3Type = (int) eProperty.MaxHealth;

				ShadowbladeEpicGloves.Bonus4 = 4;
				ShadowbladeEpicGloves.Bonus4Type = (int) eProperty.Skill_Envenom;


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicGloves);
				}

			}
			//Shadow Shrouded Hauberk 
			ShadowbladeEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicVest");
			if (ShadowbladeEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Vest , creating it ...");
				ShadowbladeEpicVest = new ItemTemplate();
				ShadowbladeEpicVest.ItemTemplateID = "ShadowbladeEpicVest";
				ShadowbladeEpicVest.Name = "Shadow Shrouded Jerkin";
				ShadowbladeEpicVest.Level = 50;
				ShadowbladeEpicVest.Item_Type = 25;
				ShadowbladeEpicVest.Model = 761;
				ShadowbladeEpicVest.IsDropable = true;
				ShadowbladeEpicVest.IsPickable = true;
				ShadowbladeEpicVest.DPS_AF = 100;
				ShadowbladeEpicVest.SPD_ABS = 10;
				ShadowbladeEpicVest.Object_Type = 33;
				ShadowbladeEpicVest.Quality = 100;
				ShadowbladeEpicVest.MaxQuality = 100;
				ShadowbladeEpicVest.Weight = 22;
				ShadowbladeEpicVest.Bonus = 35;
				ShadowbladeEpicVest.MaxCondition = 50000;
				ShadowbladeEpicVest.MaxDurability = 50000;
				ShadowbladeEpicVest.Condition = 50000;
				ShadowbladeEpicVest.Durability = 50000;

				ShadowbladeEpicVest.Bonus1 = 13;
				ShadowbladeEpicVest.Bonus1Type = (int) eStat.STR;

				ShadowbladeEpicVest.Bonus2 = 13;
				ShadowbladeEpicVest.Bonus2Type = (int) eStat.DEX;

				ShadowbladeEpicVest.Bonus3 = 30;
				ShadowbladeEpicVest.Bonus3Type = (int) eProperty.MaxHealth;

				ShadowbladeEpicVest.Bonus4 = 6;
				ShadowbladeEpicVest.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicVest);
				}

			}
			//Shadow Shrouded Legs 
			ShadowbladeEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicLegs");
			if (ShadowbladeEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Legs , creating it ...");
				ShadowbladeEpicLegs = new ItemTemplate();
				ShadowbladeEpicLegs.ItemTemplateID = "ShadowbladeEpicLegs";
				ShadowbladeEpicLegs.Name = "Shadow Shrouded Legs";
				ShadowbladeEpicLegs.Level = 50;
				ShadowbladeEpicLegs.Item_Type = 27;
				ShadowbladeEpicLegs.Model = 762;
				ShadowbladeEpicLegs.IsDropable = true;
				ShadowbladeEpicLegs.IsPickable = true;
				ShadowbladeEpicLegs.DPS_AF = 100;
				ShadowbladeEpicLegs.SPD_ABS = 10;
				ShadowbladeEpicLegs.Object_Type = 33;
				ShadowbladeEpicLegs.Quality = 100;
				ShadowbladeEpicLegs.MaxQuality = 100;
				ShadowbladeEpicLegs.Weight = 22;
				ShadowbladeEpicLegs.Bonus = 35;
				ShadowbladeEpicLegs.MaxCondition = 50000;
				ShadowbladeEpicLegs.MaxDurability = 50000;
				ShadowbladeEpicLegs.Condition = 50000;
				ShadowbladeEpicLegs.Durability = 50000;

				ShadowbladeEpicLegs.Bonus1 = 12;
				ShadowbladeEpicLegs.Bonus1Type = (int) eStat.STR;

				ShadowbladeEpicLegs.Bonus2 = 15;
				ShadowbladeEpicLegs.Bonus2Type = (int) eStat.CON;

				ShadowbladeEpicLegs.Bonus3 = 12;
				ShadowbladeEpicLegs.Bonus3Type = (int) eStat.QUI;

				ShadowbladeEpicLegs.Bonus4 = 10;
				ShadowbladeEpicLegs.Bonus4Type = (int) eResist.Slash;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicLegs);
				}

			}
			//Shadow Shrouded Sleeves 
			ShadowbladeEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ShadowbladeEpicArms");
			if (ShadowbladeEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shadowblade Epic Arms , creating it ...");
				ShadowbladeEpicArms = new ItemTemplate();
				ShadowbladeEpicArms.ItemTemplateID = "ShadowbladeEpicArms";
				ShadowbladeEpicArms.Name = "Shadow Shrouded Sleeves";
				ShadowbladeEpicArms.Level = 50;
				ShadowbladeEpicArms.Item_Type = 28;
				ShadowbladeEpicArms.Model = 763;
				ShadowbladeEpicArms.IsDropable = true;
				ShadowbladeEpicArms.IsPickable = true;
				ShadowbladeEpicArms.DPS_AF = 100;
				ShadowbladeEpicArms.SPD_ABS = 10;
				ShadowbladeEpicArms.Object_Type = 33;
				ShadowbladeEpicArms.Quality = 100;
				ShadowbladeEpicArms.MaxQuality = 100;
				ShadowbladeEpicArms.Weight = 22;
				ShadowbladeEpicArms.Bonus = 35;
				ShadowbladeEpicArms.MaxCondition = 50000;
				ShadowbladeEpicArms.MaxDurability = 50000;
				ShadowbladeEpicArms.Condition = 50000;
				ShadowbladeEpicArms.Durability = 50000;

				ShadowbladeEpicArms.Bonus1 = 15;
				ShadowbladeEpicArms.Bonus1Type = (int) eStat.CON;

				ShadowbladeEpicArms.Bonus2 = 16;
				ShadowbladeEpicArms.Bonus2Type = (int) eStat.DEX;

				ShadowbladeEpicArms.Bonus3 = 10;
				ShadowbladeEpicArms.Bonus3Type = (int) eResist.Crush;

				ShadowbladeEpicArms.Bonus4 = 10;
				ShadowbladeEpicArms.Bonus4Type = (int) eResist.Thrust;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ShadowbladeEpicArms);
				}

			}
//Shadowblade Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(Masrim, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasrim));
			GameEventMgr.AddHandler(Masrim, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasrim));

			GameEventMgr.AddHandler(MorlinCaan, GameObjectEvent.Interact, new DOLEventHandler(TalkToMorlinCaan));
			GameEventMgr.AddHandler(MorlinCaan, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMorlinCaan));

			/* Now we bring to Masrim the possibility to give this quest to players */
			Masrim.AddQuestToGive(typeof (Rogue_50));

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

			/* Now we remove to Masrim the possibility to give this quest to players */
			Masrim.RemoveQuestToGive(typeof (Rogue_50));
		}

		protected static void TalkToMasrim(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
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
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					MorlinCaan.SayTo(player, "Check your journal for instructions!");
				}
				return;
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Rogue_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Shadowblade &&
				player.CharacterClass.ID != (byte) eCharacterClass.Hunter)
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
			Rogue_50 quest = player.IsDoingQuest(typeof (Rogue_50)) as Rogue_50;

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
			if(Masrim.CanGiveQuest(typeof (Rogue_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Rogue_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Masrim.GiveQuest(typeof (Rogue_50), player, 1))
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
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Rogue_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Oona.Name)
				{
					m_questPlayer.Out.SendMessage("You collect Oona's Head", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(player, oona_head);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Masrim.Name && gArgs.Item.ItemTemplateID == oona_head.ItemTemplateID)
				{
					RemoveItem(Masrim, player, oona_head);
					Masrim.SayTo(player, "Take this sealed pouch to Morlin Caan in Jordheim for your reward!");
					GiveItem(player, sealed_pouch);
					Step = 3;
					return;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == MorlinCaan.Name && gArgs.Item.ItemTemplateID == sealed_pouch.ItemTemplateID)
				{
					RemoveItem(MorlinCaan, player, sealed_pouch);
					MorlinCaan.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Shadowblade)
			{
				GiveItem(m_questPlayer, ShadowbladeEpicArms);
				GiveItem(m_questPlayer, ShadowbladeEpicBoots);
				GiveItem(m_questPlayer, ShadowbladeEpicGloves);
				GiveItem(m_questPlayer, ShadowbladeEpicHelm);
				GiveItem(m_questPlayer, ShadowbladeEpicLegs);
				GiveItem(m_questPlayer, ShadowbladeEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Hunter)
			{
				GiveItem(m_questPlayer, HunterEpicArms);
				GiveItem(m_questPlayer, HunterEpicBoots);
				GiveItem(m_questPlayer, HunterEpicGloves);
				GiveItem(m_questPlayer, HunterEpicHelm);
				GiveItem(m_questPlayer, HunterEpicLegs);
				GiveItem(m_questPlayer, HunterEpicVest);
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
