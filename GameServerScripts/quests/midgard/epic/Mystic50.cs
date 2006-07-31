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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Mystic_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Saving the Clan";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Danica = null; // Start NPC
		private static GameNPC Kelic = null; // Mob to kill

		private static ItemTemplate kelics_totem = null;
		private static ItemTemplate SpiritmasterEpicBoots = null;
		private static ItemTemplate SpiritmasterEpicHelm = null;
		private static ItemTemplate SpiritmasterEpicGloves = null;
		private static ItemTemplate SpiritmasterEpicLegs = null;
		private static ItemTemplate SpiritmasterEpicArms = null;
		private static ItemTemplate SpiritmasterEpicVest = null;
		private static ItemTemplate RunemasterEpicBoots = null;
		private static ItemTemplate RunemasterEpicHelm = null;
		private static ItemTemplate RunemasterEpicGloves = null;
		private static ItemTemplate RunemasterEpicLegs = null;
		private static ItemTemplate RunemasterEpicArms = null;
		private static ItemTemplate RunemasterEpicVest = null;
		private static ItemTemplate BonedancerEpicBoots = null;
		private static ItemTemplate BonedancerEpicHelm = null;
		private static ItemTemplate BonedancerEpicGloves = null;
		private static ItemTemplate BonedancerEpicLegs = null;
		private static ItemTemplate BonedancerEpicArms = null;
		private static ItemTemplate BonedancerEpicVest = null;

		// Constructors
		public Mystic_50() : base()
		{
		}

		public Mystic_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}


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
				Danica.Realm = (byte) eRealm.Midgard;
				Danica.CurrentRegionID = 100;
				Danica.Size = 51;
				Danica.Level = 50;
				Danica.X = 804440;
				Danica.Y = 722267;
				Danica.Z = 4719;
				Danica.Heading = 2116;
				Danica.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Danica.SaveIntoDatabase();
				}
			}
			else
				Danica = npcs[0];
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
				Kelic.Realm = (byte) eRealm.None;
				Kelic.CurrentRegionID = 100;
				Kelic.Size = 100;
				Kelic.Level = 65;
				Kelic.X = 621577;
				Kelic.Y = 745848;
				Kelic.Z = 4593;
				Kelic.Heading = 3538;
				Kelic.Flags ^= (uint)GameNPC.eFlags.TRANSPARENT;
				Kelic.MaxSpeedBase = 200;
				Kelic.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Kelic.SaveIntoDatabase();
				}
			}
			else
				Kelic = npcs[0];
			// end npc

			#endregion

			#region defineItems

			kelics_totem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "kelics_totem");
			if (kelics_totem == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic's Totem , creating it ...");
				kelics_totem = new ItemTemplate();
				kelics_totem.Id_nb = "kelics_totem";
				kelics_totem.Name = "Kelic's Totem";
				kelics_totem.Level = 8;
				kelics_totem.Item_Type = 0;
				kelics_totem.Model = 488;
				kelics_totem.IsDropable = false;
				kelics_totem.IsPickable = false;
				kelics_totem.DPS_AF = 0;
				kelics_totem.SPD_ABS = 0;
				kelics_totem.Object_Type = 0;
				kelics_totem.Hand = 0;
				kelics_totem.Type_Damage = 0;
				kelics_totem.Quality = 100;
				kelics_totem.MaxQuality = 100;
				kelics_totem.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(kelics_totem);
				}

			}

			SpiritmasterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicBoots");
			if (SpiritmasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Boots , creating it ...");
				SpiritmasterEpicBoots = new ItemTemplate();
				SpiritmasterEpicBoots.Id_nb = "SpiritmasterEpicBoots";
				SpiritmasterEpicBoots.Name = "Spirit Touched Boots";
				SpiritmasterEpicBoots.Level = 50;
				SpiritmasterEpicBoots.Item_Type = 23;
				SpiritmasterEpicBoots.Model = 803;
				SpiritmasterEpicBoots.IsDropable = true;
				SpiritmasterEpicBoots.IsPickable = true;
				SpiritmasterEpicBoots.DPS_AF = 50;
				SpiritmasterEpicBoots.SPD_ABS = 0;
				SpiritmasterEpicBoots.Object_Type = 32;
				SpiritmasterEpicBoots.Quality = 100;
				SpiritmasterEpicBoots.MaxQuality = 100;
				SpiritmasterEpicBoots.Weight = 22;
				SpiritmasterEpicBoots.Bonus = 35;
				SpiritmasterEpicBoots.MaxCondition = 50000;
				SpiritmasterEpicBoots.MaxDurability = 50000;
				SpiritmasterEpicBoots.Condition = 50000;
				SpiritmasterEpicBoots.Durability = 50000;

				SpiritmasterEpicBoots.Bonus1 = 16;
				SpiritmasterEpicBoots.Bonus1Type = (int) eStat.CON;

				SpiritmasterEpicBoots.Bonus2 = 16;
				SpiritmasterEpicBoots.Bonus2Type = (int) eStat.DEX;

				SpiritmasterEpicBoots.Bonus3 = 8;
				SpiritmasterEpicBoots.Bonus3Type = (int) eResist.Matter;

				SpiritmasterEpicBoots.Bonus4 = 10;
				SpiritmasterEpicBoots.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicBoots);
				}

			}
//end item
			SpiritmasterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicHelm");
			if (SpiritmasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Helm , creating it ...");
				SpiritmasterEpicHelm = new ItemTemplate();
				SpiritmasterEpicHelm.Id_nb = "SpiritmasterEpicHelm";
				SpiritmasterEpicHelm.Name = "Spirit Touched Cap";
				SpiritmasterEpicHelm.Level = 50;
				SpiritmasterEpicHelm.Item_Type = 21;
				SpiritmasterEpicHelm.Model = 825; //NEED TO WORK ON..
				SpiritmasterEpicHelm.IsDropable = true;
				SpiritmasterEpicHelm.IsPickable = true;
				SpiritmasterEpicHelm.DPS_AF = 50;
				SpiritmasterEpicHelm.SPD_ABS = 0;
				SpiritmasterEpicHelm.Object_Type = 32;
				SpiritmasterEpicHelm.Quality = 100;
				SpiritmasterEpicHelm.MaxQuality = 100;
				SpiritmasterEpicHelm.Weight = 22;
				SpiritmasterEpicHelm.Bonus = 35;
				SpiritmasterEpicHelm.MaxCondition = 50000;
				SpiritmasterEpicHelm.MaxDurability = 50000;
				SpiritmasterEpicHelm.Condition = 50000;
				SpiritmasterEpicHelm.Durability = 50000;

				SpiritmasterEpicHelm.Bonus1 = 4;
				SpiritmasterEpicHelm.Bonus1Type = (int) eProperty.Focus_Darkness;

				SpiritmasterEpicHelm.Bonus2 = 4;
				SpiritmasterEpicHelm.Bonus2Type = (int) eProperty.Focus_Suppression;

				SpiritmasterEpicHelm.Bonus3 = 13;
				SpiritmasterEpicHelm.Bonus3Type = (int) eStat.PIE;

				SpiritmasterEpicHelm.Bonus4 = 4;
				SpiritmasterEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicHelm);
				}

			}
//end item
			SpiritmasterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicGloves");
			if (SpiritmasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Gloves , creating it ...");
				SpiritmasterEpicGloves = new ItemTemplate();
				SpiritmasterEpicGloves.Id_nb = "SpiritmasterEpicGloves";
				SpiritmasterEpicGloves.Name = "Spirit Touched Gloves ";
				SpiritmasterEpicGloves.Level = 50;
				SpiritmasterEpicGloves.Item_Type = 22;
				SpiritmasterEpicGloves.Model = 802;
				SpiritmasterEpicGloves.IsDropable = true;
				SpiritmasterEpicGloves.IsPickable = true;
				SpiritmasterEpicGloves.DPS_AF = 50;
				SpiritmasterEpicGloves.SPD_ABS = 0;
				SpiritmasterEpicGloves.Object_Type = 32;
				SpiritmasterEpicGloves.Quality = 100;
				SpiritmasterEpicGloves.MaxQuality = 100;
				SpiritmasterEpicGloves.Weight = 22;
				SpiritmasterEpicGloves.Bonus = 35;
				SpiritmasterEpicGloves.MaxCondition = 50000;
				SpiritmasterEpicGloves.MaxDurability = 50000;
				SpiritmasterEpicGloves.Condition = 50000;
				SpiritmasterEpicGloves.Durability = 50000;

				SpiritmasterEpicGloves.Bonus1 = 4;
				SpiritmasterEpicGloves.Bonus1Type = (int) eProperty.Focus_Summoning;

				SpiritmasterEpicGloves.Bonus2 = 13;
				SpiritmasterEpicGloves.Bonus2Type = (int) eStat.DEX;

				SpiritmasterEpicGloves.Bonus3 = 12;
				SpiritmasterEpicGloves.Bonus3Type = (int) eStat.PIE;

				SpiritmasterEpicGloves.Bonus4 = 4;
				SpiritmasterEpicGloves.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicGloves);
				}

			}

			SpiritmasterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicVest");
			if (SpiritmasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Vest , creating it ...");
				SpiritmasterEpicVest = new ItemTemplate();
				SpiritmasterEpicVest.Id_nb = "SpiritmasterEpicVest";
				SpiritmasterEpicVest.Name = "Spirit Touched Vest";
				SpiritmasterEpicVest.Level = 50;
				SpiritmasterEpicVest.Item_Type = 25;
				SpiritmasterEpicVest.Model = 799;
				SpiritmasterEpicVest.IsDropable = true;
				SpiritmasterEpicVest.IsPickable = true;
				SpiritmasterEpicVest.DPS_AF = 50;
				SpiritmasterEpicVest.SPD_ABS = 0;
				SpiritmasterEpicVest.Object_Type = 32;
				SpiritmasterEpicVest.Quality = 100;
				SpiritmasterEpicVest.MaxQuality = 100;
				SpiritmasterEpicVest.Weight = 22;
				SpiritmasterEpicVest.Bonus = 35;
				SpiritmasterEpicVest.MaxCondition = 50000;
				SpiritmasterEpicVest.MaxDurability = 50000;
				SpiritmasterEpicVest.Condition = 50000;
				SpiritmasterEpicVest.Durability = 50000;

				SpiritmasterEpicVest.Bonus1 = 12;
				SpiritmasterEpicVest.Bonus1Type = (int) eStat.DEX;

				SpiritmasterEpicVest.Bonus2 = 13;
				SpiritmasterEpicVest.Bonus2Type = (int) eStat.PIE;

				SpiritmasterEpicVest.Bonus3 = 12;
				SpiritmasterEpicVest.Bonus3Type = (int) eResist.Slash;

				SpiritmasterEpicVest.Bonus4 = 24;
				SpiritmasterEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicVest);
				}

			}

			SpiritmasterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicLegs");
			if (SpiritmasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Legs , creating it ...");
				SpiritmasterEpicLegs = new ItemTemplate();
				SpiritmasterEpicLegs.Id_nb = "SpiritmasterEpicLegs";
				SpiritmasterEpicLegs.Name = "Spirit Touched Pants";
				SpiritmasterEpicLegs.Level = 50;
				SpiritmasterEpicLegs.Item_Type = 27;
				SpiritmasterEpicLegs.Model = 800;
				SpiritmasterEpicLegs.IsDropable = true;
				SpiritmasterEpicLegs.IsPickable = true;
				SpiritmasterEpicLegs.DPS_AF = 50;
				SpiritmasterEpicLegs.SPD_ABS = 0;
				SpiritmasterEpicLegs.Object_Type = 32;
				SpiritmasterEpicLegs.Quality = 100;
				SpiritmasterEpicLegs.MaxQuality = 100;
				SpiritmasterEpicLegs.Weight = 22;
				SpiritmasterEpicLegs.Bonus = 35;
				SpiritmasterEpicLegs.MaxCondition = 50000;
				SpiritmasterEpicLegs.MaxDurability = 50000;
				SpiritmasterEpicLegs.Condition = 50000;
				SpiritmasterEpicLegs.Durability = 50000;

				SpiritmasterEpicLegs.Bonus1 = 13;
				SpiritmasterEpicLegs.Bonus1Type = (int) eStat.CON;

				SpiritmasterEpicLegs.Bonus2 = 13;
				SpiritmasterEpicLegs.Bonus2Type = (int) eStat.DEX;

				SpiritmasterEpicLegs.Bonus3 = 12;
				SpiritmasterEpicLegs.Bonus3Type = (int) eResist.Crush;

				SpiritmasterEpicLegs.Bonus4 = 24;
				SpiritmasterEpicLegs.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicLegs);
				}

			}

			SpiritmasterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SpiritmasterEpicArms");
			if (SpiritmasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Arms , creating it ...");
				SpiritmasterEpicArms = new ItemTemplate();
				SpiritmasterEpicArms.Id_nb = "SpiritmasterEpicArms";
				SpiritmasterEpicArms.Name = "Spirit Touched Sleeves";
				SpiritmasterEpicArms.Level = 50;
				SpiritmasterEpicArms.Item_Type = 28;
				SpiritmasterEpicArms.Model = 801;
				SpiritmasterEpicArms.IsDropable = true;
				SpiritmasterEpicArms.IsPickable = true;
				SpiritmasterEpicArms.DPS_AF = 50;
				SpiritmasterEpicArms.SPD_ABS = 0;
				SpiritmasterEpicArms.Object_Type = 32;
				SpiritmasterEpicArms.Quality = 100;
				SpiritmasterEpicArms.MaxQuality = 100;
				SpiritmasterEpicArms.Weight = 22;
				SpiritmasterEpicArms.Bonus = 35;
				SpiritmasterEpicArms.MaxCondition = 50000;
				SpiritmasterEpicArms.MaxDurability = 50000;
				SpiritmasterEpicArms.Condition = 50000;
				SpiritmasterEpicArms.Durability = 50000;

				SpiritmasterEpicArms.Bonus1 = 9;
				SpiritmasterEpicArms.Bonus1Type = (int) eStat.PIE;

				SpiritmasterEpicArms.Bonus2 = 6;
				SpiritmasterEpicArms.Bonus2Type = (int) eResist.Thrust;

				SpiritmasterEpicArms.Bonus3 = 12;
				SpiritmasterEpicArms.Bonus3Type = (int) eProperty.MaxHealth;

				SpiritmasterEpicArms.Bonus4 = 8;
				SpiritmasterEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(SpiritmasterEpicArms);
				}
			}

			RunemasterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicBoots");
			if (RunemasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Boots , creating it ...");
				RunemasterEpicBoots = new ItemTemplate();
				RunemasterEpicBoots.Id_nb = "RunemasterEpicBoots";
				RunemasterEpicBoots.Name = "Raven-Rune Boots";
				RunemasterEpicBoots.Level = 50;
				RunemasterEpicBoots.Item_Type = 23;
				RunemasterEpicBoots.Model = 707;
				RunemasterEpicBoots.IsDropable = true;
				RunemasterEpicBoots.IsPickable = true;
				RunemasterEpicBoots.DPS_AF = 50;
				RunemasterEpicBoots.SPD_ABS = 0;
				RunemasterEpicBoots.Object_Type = 32;
				RunemasterEpicBoots.Quality = 100;
				RunemasterEpicBoots.MaxQuality = 100;
				RunemasterEpicBoots.Weight = 22;
				RunemasterEpicBoots.Bonus = 35;
				RunemasterEpicBoots.MaxCondition = 50000;
				RunemasterEpicBoots.MaxDurability = 50000;
				RunemasterEpicBoots.Condition = 50000;
				RunemasterEpicBoots.Durability = 50000;

				RunemasterEpicBoots.Bonus1 = 16;
				RunemasterEpicBoots.Bonus1Type = (int) eStat.CON;

				RunemasterEpicBoots.Bonus2 = 16;
				RunemasterEpicBoots.Bonus2Type = (int) eStat.DEX;

				RunemasterEpicBoots.Bonus3 = 8;
				RunemasterEpicBoots.Bonus3Type = (int) eResist.Matter;

				RunemasterEpicBoots.Bonus4 = 10;
				RunemasterEpicBoots.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicBoots);
				}
			}
//end item
			RunemasterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicHelm");
			if (RunemasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Helm , creating it ...");
				RunemasterEpicHelm = new ItemTemplate();
				RunemasterEpicHelm.Id_nb = "RunemasterEpicHelm";
				RunemasterEpicHelm.Name = "Raven-Rune Cap";
				RunemasterEpicHelm.Level = 50;
				RunemasterEpicHelm.Item_Type = 21;
				RunemasterEpicHelm.Model = 825; //NEED TO WORK ON..
				RunemasterEpicHelm.IsDropable = true;
				RunemasterEpicHelm.IsPickable = true;
				RunemasterEpicHelm.DPS_AF = 50;
				RunemasterEpicHelm.SPD_ABS = 0;
				RunemasterEpicHelm.Object_Type = 32;
				RunemasterEpicHelm.Quality = 100;
				RunemasterEpicHelm.MaxQuality = 100;
				RunemasterEpicHelm.Weight = 22;
				RunemasterEpicHelm.Bonus = 35;
				RunemasterEpicHelm.MaxCondition = 50000;
				RunemasterEpicHelm.MaxDurability = 50000;
				RunemasterEpicHelm.Condition = 50000;
				RunemasterEpicHelm.Durability = 50000;

				RunemasterEpicHelm.Bonus1 = 4;
				RunemasterEpicHelm.Bonus1Type = (int) eProperty.Focus_Darkness;

				RunemasterEpicHelm.Bonus2 = 4;
				RunemasterEpicHelm.Bonus2Type = (int) eProperty.Focus_Suppression;

				RunemasterEpicHelm.Bonus3 = 13;
				RunemasterEpicHelm.Bonus3Type = (int) eStat.PIE;

				RunemasterEpicHelm.Bonus4 = 4;
				RunemasterEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicHelm);
				}
			}
//end item
			RunemasterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicGloves");
			if (RunemasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Gloves , creating it ...");
				RunemasterEpicGloves = new ItemTemplate();
				RunemasterEpicGloves.Id_nb = "RunemasterEpicGloves";
				RunemasterEpicGloves.Name = "Raven-Rune Gloves ";
				RunemasterEpicGloves.Level = 50;
				RunemasterEpicGloves.Item_Type = 22;
				RunemasterEpicGloves.Model = 706;
				RunemasterEpicGloves.IsDropable = true;
				RunemasterEpicGloves.IsPickable = true;
				RunemasterEpicGloves.DPS_AF = 50;
				RunemasterEpicGloves.SPD_ABS = 0;
				RunemasterEpicGloves.Object_Type = 32;
				RunemasterEpicGloves.Quality = 100;
				RunemasterEpicGloves.MaxQuality = 100;
				RunemasterEpicGloves.Weight = 22;
				RunemasterEpicGloves.Bonus = 35;
				RunemasterEpicGloves.MaxCondition = 50000;
				RunemasterEpicGloves.MaxDurability = 50000;
				RunemasterEpicGloves.Condition = 50000;
				RunemasterEpicGloves.Durability = 50000;

				RunemasterEpicGloves.Bonus1 = 4;
				RunemasterEpicGloves.Bonus1Type = (int) eProperty.Focus_Summoning;

				RunemasterEpicGloves.Bonus2 = 13;
				RunemasterEpicGloves.Bonus2Type = (int) eStat.DEX;

				RunemasterEpicGloves.Bonus3 = 12;
				RunemasterEpicGloves.Bonus3Type = (int) eStat.PIE;

				RunemasterEpicGloves.Bonus4 = 6;
				RunemasterEpicGloves.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicGloves);
				}
			}

			RunemasterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicVest");
			if (RunemasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Vest , creating it ...");
				RunemasterEpicVest = new ItemTemplate();
				RunemasterEpicVest.Id_nb = "RunemasterEpicVest";
				RunemasterEpicVest.Name = "Raven-Rune Vest";
				RunemasterEpicVest.Level = 50;
				RunemasterEpicVest.Item_Type = 25;
				RunemasterEpicVest.Model = 703;
				RunemasterEpicVest.IsDropable = true;
				RunemasterEpicVest.IsPickable = true;
				RunemasterEpicVest.DPS_AF = 50;
				RunemasterEpicVest.SPD_ABS = 0;
				RunemasterEpicVest.Object_Type = 32;
				RunemasterEpicVest.Quality = 100;
				RunemasterEpicVest.MaxQuality = 100;
				RunemasterEpicVest.Weight = 22;
				RunemasterEpicVest.Bonus = 35;
				RunemasterEpicVest.MaxCondition = 50000;
				RunemasterEpicVest.MaxDurability = 50000;
				RunemasterEpicVest.Condition = 50000;
				RunemasterEpicVest.Durability = 50000;

				RunemasterEpicVest.Bonus1 = 12;
				RunemasterEpicVest.Bonus1Type = (int) eStat.DEX;

				RunemasterEpicVest.Bonus2 = 13;
				RunemasterEpicVest.Bonus2Type = (int) eStat.PIE;

				RunemasterEpicVest.Bonus3 = 12;
				RunemasterEpicVest.Bonus3Type = (int) eResist.Slash;

				RunemasterEpicVest.Bonus4 = 24;
				RunemasterEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicVest);
				}
			}

			RunemasterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicLegs");
			if (RunemasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Legs , creating it ...");
				RunemasterEpicLegs = new ItemTemplate();
				RunemasterEpicLegs.Id_nb = "RunemasterEpicLegs";
				RunemasterEpicLegs.Name = "Raven-Rune Pants";
				RunemasterEpicLegs.Level = 50;
				RunemasterEpicLegs.Item_Type = 27;
				RunemasterEpicLegs.Model = 704;
				RunemasterEpicLegs.IsDropable = true;
				RunemasterEpicLegs.IsPickable = true;
				RunemasterEpicLegs.DPS_AF = 50;
				RunemasterEpicLegs.SPD_ABS = 0;
				RunemasterEpicLegs.Object_Type = 32;
				RunemasterEpicLegs.Quality = 100;
				RunemasterEpicLegs.MaxQuality = 100;
				RunemasterEpicLegs.Weight = 22;
				RunemasterEpicLegs.Bonus = 35;
				RunemasterEpicLegs.MaxCondition = 50000;
				RunemasterEpicLegs.MaxDurability = 50000;
				RunemasterEpicLegs.Condition = 50000;
				RunemasterEpicLegs.Durability = 50000;

				RunemasterEpicLegs.Bonus1 = 13;
				RunemasterEpicLegs.Bonus1Type = (int) eStat.CON;

				RunemasterEpicLegs.Bonus2 = 13;
				RunemasterEpicLegs.Bonus2Type = (int) eStat.DEX;

				RunemasterEpicLegs.Bonus3 = 12;
				RunemasterEpicLegs.Bonus3Type = (int) eResist.Crush;

				RunemasterEpicLegs.Bonus4 = 24;
				RunemasterEpicLegs.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicLegs);
				}
			}

			RunemasterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "RunemasterEpicArms");
			if (RunemasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Arms , creating it ...");
				RunemasterEpicArms = new ItemTemplate();
				RunemasterEpicArms.Id_nb = "RunemasterEpicArms";
				RunemasterEpicArms.Name = "Raven-Rune Sleeves";
				RunemasterEpicArms.Level = 50;
				RunemasterEpicArms.Item_Type = 28;
				RunemasterEpicArms.Model = 705;
				RunemasterEpicArms.IsDropable = true;
				RunemasterEpicArms.IsPickable = true;
				RunemasterEpicArms.DPS_AF = 50;
				RunemasterEpicArms.SPD_ABS = 0;
				RunemasterEpicArms.Object_Type = 32;
				RunemasterEpicArms.Quality = 100;
				RunemasterEpicArms.MaxQuality = 100;
				RunemasterEpicArms.Weight = 22;
				RunemasterEpicArms.Bonus = 35;
				RunemasterEpicArms.MaxCondition = 50000;
				RunemasterEpicArms.MaxDurability = 50000;
				RunemasterEpicArms.Condition = 50000;
				RunemasterEpicArms.Durability = 50000;

				RunemasterEpicArms.Bonus1 = 9;
				RunemasterEpicArms.Bonus1Type = (int) eStat.PIE;

				RunemasterEpicArms.Bonus2 = 6;
				RunemasterEpicArms.Bonus2Type = (int) eResist.Thrust;

				RunemasterEpicArms.Bonus3 = 12;
				RunemasterEpicArms.Bonus3Type = (int) eProperty.MaxHealth;

				RunemasterEpicArms.Bonus4 = 8;
				RunemasterEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(RunemasterEpicArms);
				}
			}

			BonedancerEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicBoots");
			if (BonedancerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Boots , creating it ...");
				BonedancerEpicBoots = new ItemTemplate();
				BonedancerEpicBoots.Id_nb = "BonedancerEpicBoots";
				BonedancerEpicBoots.Name = "Raven-Boned Boots";
				BonedancerEpicBoots.Level = 50;
				BonedancerEpicBoots.Item_Type = 23;
				BonedancerEpicBoots.Model = 1190;
				BonedancerEpicBoots.IsDropable = true;
				BonedancerEpicBoots.IsPickable = true;
				BonedancerEpicBoots.DPS_AF = 50;
				BonedancerEpicBoots.SPD_ABS = 0;
				BonedancerEpicBoots.Object_Type = 32;
				BonedancerEpicBoots.Quality = 100;
				BonedancerEpicBoots.MaxQuality = 100;
				BonedancerEpicBoots.Weight = 22;
				BonedancerEpicBoots.Bonus = 35;
				BonedancerEpicBoots.MaxCondition = 50000;
				BonedancerEpicBoots.MaxDurability = 50000;
				BonedancerEpicBoots.Condition = 50000;
				BonedancerEpicBoots.Durability = 50000;

				BonedancerEpicBoots.Bonus1 = 16;
				BonedancerEpicBoots.Bonus1Type = (int) eStat.CON;

				BonedancerEpicBoots.Bonus2 = 16;
				BonedancerEpicBoots.Bonus2Type = (int) eStat.DEX;

				BonedancerEpicBoots.Bonus3 = 8;
				BonedancerEpicBoots.Bonus3Type = (int) eResist.Matter;

				BonedancerEpicBoots.Bonus4 = 10;
				BonedancerEpicBoots.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicBoots);
				}

			}
//end item
			BonedancerEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicHelm");
			if (BonedancerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Helm , creating it ...");
				BonedancerEpicHelm = new ItemTemplate();
				BonedancerEpicHelm.Id_nb = "BonedancerEpicHelm";
				BonedancerEpicHelm.Name = "Raven-Boned Cap";
				BonedancerEpicHelm.Level = 50;
				BonedancerEpicHelm.Item_Type = 21;
				BonedancerEpicHelm.Model = 825; //NEED TO WORK ON..
				BonedancerEpicHelm.IsDropable = true;
				BonedancerEpicHelm.IsPickable = true;
				BonedancerEpicHelm.DPS_AF = 50;
				BonedancerEpicHelm.SPD_ABS = 0;
				BonedancerEpicHelm.Object_Type = 32;
				BonedancerEpicHelm.Quality = 100;
				BonedancerEpicHelm.MaxQuality = 100;
				BonedancerEpicHelm.Weight = 22;
				BonedancerEpicHelm.Bonus = 35;
				BonedancerEpicHelm.MaxCondition = 50000;
				BonedancerEpicHelm.MaxDurability = 50000;
				BonedancerEpicHelm.Condition = 50000;
				BonedancerEpicHelm.Durability = 50000;

				BonedancerEpicHelm.Bonus1 = 4;
				BonedancerEpicHelm.Bonus1Type = (int) eProperty.Focus_Suppression;

				BonedancerEpicHelm.Bonus2 = 13;
				BonedancerEpicHelm.Bonus2Type = (int) eStat.PIE;

				BonedancerEpicHelm.Bonus3 = 4;
				BonedancerEpicHelm.Bonus3Type = (int) eProperty.PowerRegenerationRate;

				BonedancerEpicHelm.Bonus4 = 4;
				BonedancerEpicHelm.Bonus4Type = (int) eProperty.Focus_BoneArmy;


				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicHelm);
				}

			}
//end item
			BonedancerEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicGloves");
			if (BonedancerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Gloves , creating it ...");
				BonedancerEpicGloves = new ItemTemplate();
				BonedancerEpicGloves.Id_nb = "BonedancerEpicGloves";
				BonedancerEpicGloves.Name = "Raven-Boned Gloves ";
				BonedancerEpicGloves.Level = 50;
				BonedancerEpicGloves.Item_Type = 22;
				BonedancerEpicGloves.Model = 1191;
				BonedancerEpicGloves.IsDropable = true;
				BonedancerEpicGloves.IsPickable = true;
				BonedancerEpicGloves.DPS_AF = 50;
				BonedancerEpicGloves.SPD_ABS = 0;
				BonedancerEpicGloves.Object_Type = 32;
				BonedancerEpicGloves.Quality = 100;
				BonedancerEpicGloves.MaxQuality = 100;
				BonedancerEpicGloves.Weight = 22;
				BonedancerEpicGloves.Bonus = 35;
				BonedancerEpicGloves.MaxCondition = 50000;
				BonedancerEpicGloves.MaxDurability = 50000;
				BonedancerEpicGloves.Condition = 50000;
				BonedancerEpicGloves.Durability = 50000;

				BonedancerEpicGloves.Bonus1 = 4;
				BonedancerEpicGloves.Bonus1Type = (int) eProperty.Focus_Darkness;

				BonedancerEpicGloves.Bonus2 = 13;
				BonedancerEpicGloves.Bonus2Type = (int) eStat.DEX;

				BonedancerEpicGloves.Bonus3 = 12;
				BonedancerEpicGloves.Bonus3Type = (int) eStat.PIE;

				BonedancerEpicGloves.Bonus4 = 6;
				BonedancerEpicGloves.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicGloves);
				}
			}

			BonedancerEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicVest");
			if (BonedancerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Vest , creating it ...");
				BonedancerEpicVest = new ItemTemplate();
				BonedancerEpicVest.Id_nb = "BonedancerEpicVest";
				BonedancerEpicVest.Name = "Raven-Boned Vest";
				BonedancerEpicVest.Level = 50;
				BonedancerEpicVest.Item_Type = 25;
				BonedancerEpicVest.Model = 1187;
				BonedancerEpicVest.IsDropable = true;
				BonedancerEpicVest.IsPickable = true;
				BonedancerEpicVest.DPS_AF = 50;
				BonedancerEpicVest.SPD_ABS = 0;
				BonedancerEpicVest.Object_Type = 32;
				BonedancerEpicVest.Quality = 100;
				BonedancerEpicVest.MaxQuality = 100;
				BonedancerEpicVest.Weight = 22;
				BonedancerEpicVest.Bonus = 35;
				BonedancerEpicVest.MaxCondition = 50000;
				BonedancerEpicVest.MaxDurability = 50000;
				BonedancerEpicVest.Condition = 50000;
				BonedancerEpicVest.Durability = 50000;

				BonedancerEpicVest.Bonus1 = 12;
				BonedancerEpicVest.Bonus1Type = (int) eStat.DEX;

				BonedancerEpicVest.Bonus2 = 13;
				BonedancerEpicVest.Bonus2Type = (int) eStat.PIE;

				BonedancerEpicVest.Bonus3 = 12;
				BonedancerEpicVest.Bonus3Type = (int) eResist.Slash;

				BonedancerEpicVest.Bonus4 = 24;
				BonedancerEpicVest.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicVest);
				}
			}

			BonedancerEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicLegs");
			if (BonedancerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Legs , creating it ...");
				BonedancerEpicLegs = new ItemTemplate();
				BonedancerEpicLegs.Id_nb = "BonedancerEpicLegs";
				BonedancerEpicLegs.Name = "Raven-Boned Pants";
				BonedancerEpicLegs.Level = 50;
				BonedancerEpicLegs.Item_Type = 27;
				BonedancerEpicLegs.Model = 1188;
				BonedancerEpicLegs.IsDropable = true;
				BonedancerEpicLegs.IsPickable = true;
				BonedancerEpicLegs.DPS_AF = 50;
				BonedancerEpicLegs.SPD_ABS = 0;
				BonedancerEpicLegs.Object_Type = 32;
				BonedancerEpicLegs.Quality = 100;
				BonedancerEpicLegs.MaxQuality = 100;
				BonedancerEpicLegs.Weight = 22;
				BonedancerEpicLegs.Bonus = 35;
				BonedancerEpicLegs.MaxCondition = 50000;
				BonedancerEpicLegs.MaxDurability = 50000;
				BonedancerEpicLegs.Condition = 50000;
				BonedancerEpicLegs.Durability = 50000;

				BonedancerEpicLegs.Bonus1 = 13;
				BonedancerEpicLegs.Bonus1Type = (int) eStat.CON;

				BonedancerEpicLegs.Bonus2 = 13;
				BonedancerEpicLegs.Bonus2Type = (int) eStat.DEX;

				BonedancerEpicLegs.Bonus3 = 12;
				BonedancerEpicLegs.Bonus3Type = (int) eResist.Crush;

				BonedancerEpicLegs.Bonus4 = 24;
				BonedancerEpicLegs.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicLegs);
				}

			}

			BonedancerEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BonedancerEpicArms");
			if (BonedancerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Arms , creating it ...");
				BonedancerEpicArms = new ItemTemplate();
				BonedancerEpicArms.Id_nb = "BonedancerEpicArms";
				BonedancerEpicArms.Name = "Raven-Boned Sleeves";
				BonedancerEpicArms.Level = 50;
				BonedancerEpicArms.Item_Type = 28;
				BonedancerEpicArms.Model = 1189;
				BonedancerEpicArms.IsDropable = true;
				BonedancerEpicArms.IsPickable = true;
				BonedancerEpicArms.DPS_AF = 50;
				BonedancerEpicArms.SPD_ABS = 0;
				BonedancerEpicArms.Object_Type = 32;
				BonedancerEpicArms.Quality = 100;
				BonedancerEpicArms.MaxQuality = 100;
				BonedancerEpicArms.Weight = 22;
				BonedancerEpicArms.Bonus = 35;
				BonedancerEpicArms.MaxCondition = 50000;
				BonedancerEpicArms.MaxDurability = 50000;
				BonedancerEpicArms.Condition = 50000;
				BonedancerEpicArms.Durability = 50000;

				BonedancerEpicArms.Bonus1 = 9;
				BonedancerEpicArms.Bonus1Type = (int) eStat.PIE;

				BonedancerEpicArms.Bonus2 = 6;
				BonedancerEpicArms.Bonus2Type = (int) eResist.Thrust;

				BonedancerEpicArms.Bonus3 = 12;
				BonedancerEpicArms.Bonus3Type = (int) eProperty.MaxHealth;

				BonedancerEpicArms.Bonus4 = 8;
				BonedancerEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BonedancerEpicArms);
				}

			}
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.AddHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));

			/* Now we bring to Danica the possibility to give this quest to players */
			Danica.AddQuestToGive(typeof (Mystic_50));

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

			/* Now we remove to Danica the possibility to give this quest to players */
			Danica.RemoveQuestToGive(typeof (Mystic_50));
		}

		protected static void TalkToDanica(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
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

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Spiritmaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Runemaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bonedancer)
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
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

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
			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Danica.GiveQuest(typeof (Mystic_50), player, 1))
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
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Mystic_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Kelic.Name)
				{
					Step = 2;
					GiveItem(player, kelics_totem);
					m_questPlayer.Out.SendMessage("Kelic drops his Totem and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Danica.Name && gArgs.Item.Id_nb == kelics_totem.Id_nb)
				{
					RemoveItem(Danica, player, kelics_totem);
					Danica.SayTo(player, "Ah, I can see how he wore the curse around the totem. I can now break the curse that is destroying the clan!");
					Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 3;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, kelics_totem, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Spiritmaster)
			{
				GiveItem(m_questPlayer, SpiritmasterEpicArms);
				GiveItem(m_questPlayer, SpiritmasterEpicBoots);
				GiveItem(m_questPlayer, SpiritmasterEpicGloves);
				GiveItem(m_questPlayer, SpiritmasterEpicHelm);
				GiveItem(m_questPlayer, SpiritmasterEpicLegs);
				GiveItem(m_questPlayer, SpiritmasterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Runemaster)
			{
				GiveItem(m_questPlayer, RunemasterEpicArms);
				GiveItem(m_questPlayer, RunemasterEpicBoots);
				GiveItem(m_questPlayer, RunemasterEpicGloves);
				GiveItem(m_questPlayer, RunemasterEpicHelm);
				GiveItem(m_questPlayer, RunemasterEpicLegs);
				GiveItem(m_questPlayer, RunemasterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Bonedancer)
			{
				GiveItem(m_questPlayer, BonedancerEpicArms);
				GiveItem(m_questPlayer, BonedancerEpicBoots);
				GiveItem(m_questPlayer, BonedancerEpicGloves);
				GiveItem(m_questPlayer, BonedancerEpicHelm);
				GiveItem(m_questPlayer, BonedancerEpicLegs);
				GiveItem(m_questPlayer, BonedancerEpicVest);
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
