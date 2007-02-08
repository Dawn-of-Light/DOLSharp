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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	public class Essence_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Moonstone Twin";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Brigit = null; // Start NPC        
		private static GameNPC Caithor = null; // Mob to kill

		private static ItemTemplate Moonstone = null; //ball of flame

		private static ItemTemplate ChampionEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate ChampionEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate ChampionEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate ChampionEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate ChampionEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate ChampionEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate BardEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate BardEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate BardEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate BardEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate BardEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate BardEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EnchanterEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EnchanterEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EnchanterEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EnchanterEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EnchanterEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EnchanterEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate NightshadeEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NightshadeEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NightshadeEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NightshadeEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NightshadeEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NightshadeEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Essence_50() : base()
		{
		}

		public Essence_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Essence_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Essence_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Brigit", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Brigit , creating it ...");
				Brigit = new GameNPC();
				Brigit.Model = 384;
				Brigit.Name = "Brigit";
				Brigit.GuildName = "";
				Brigit.Realm = (byte) eRealm.Hibernia;
				Brigit.CurrentRegionID = 201;
				Brigit.Size = 51;
				Brigit.Level = 50;
				Brigit.X = 33131;
				Brigit.Y = 32922;
				Brigit.Z = 8008;
				Brigit.Heading = 3254;
				Brigit.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Brigit.SaveIntoDatabase();
				}

			}
			else
				Brigit = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Caithor", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Caithor , creating it ...");
				Caithor = new GameNPC();
				Caithor.Model = 339;
				Caithor.Name = "Caithor";
				Caithor.GuildName = "";
				Caithor.Realm = (byte) eRealm.None;
				Caithor.CurrentRegionID = 200;
				Caithor.Size = 60;
				Caithor.Level = 65;
				Caithor.X = 470547;
				Caithor.Y = 531497;
				Caithor.Z = 4984;
				Caithor.Heading = 3319;
				Caithor.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Caithor.SaveIntoDatabase();
				}

			}
			else
				Caithor = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			Moonstone = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "Moonstone");
			if (Moonstone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Moonstone , creating it ...");
				Moonstone = new ItemTemplate();
				Moonstone.Id_nb = "Moonstone";
				Moonstone.Name = "Moonstone";
				Moonstone.Level = 8;
				Moonstone.Item_Type = 29;
				Moonstone.Model = 514;
				Moonstone.IsDropable = false;
				Moonstone.IsPickable = false;
				Moonstone.DPS_AF = 0;
				Moonstone.SPD_ABS = 0;
				Moonstone.Object_Type = 41;
				Moonstone.Hand = 0;
				Moonstone.Type_Damage = 0;
				Moonstone.Quality = 100;
				Moonstone.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(Moonstone);
				}

			}
// end item			
			BardEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicBoots");
			if (BardEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Boots , creating it ...");
				BardEpicBoots = new ItemTemplate();
				BardEpicBoots.Id_nb = "BardEpicBoots";
				BardEpicBoots.Name = "Moonsung Boots";
				BardEpicBoots.Level = 50;
				BardEpicBoots.Item_Type = 23;
				BardEpicBoots.Model = 738;
				BardEpicBoots.IsDropable = true;
				BardEpicBoots.IsPickable = true;
				BardEpicBoots.DPS_AF = 100;
				BardEpicBoots.SPD_ABS = 19;
				BardEpicBoots.Object_Type = 37;
				BardEpicBoots.Quality = 100;
				BardEpicBoots.Weight = 22;
				BardEpicBoots.Bonus = 35;
				BardEpicBoots.MaxCondition = 50000;
				BardEpicBoots.MaxDurability = 50000;
				BardEpicBoots.Condition = 50000;
				BardEpicBoots.Durability = 50000;

				BardEpicBoots.Bonus1 = 15;
				BardEpicBoots.Bonus1Type = (int) eStat.QUI;

				BardEpicBoots.Bonus2 = 10;
				BardEpicBoots.Bonus2Type = (int) eResist.Matter;

				BardEpicBoots.Bonus3 = 4;
				BardEpicBoots.Bonus3Type = (int) eProperty.PowerRegenerationRate;

				BardEpicBoots.Bonus4 = 33;
				BardEpicBoots.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicBoots);
				}

			}
//end item
			//Moonsung Coif 
			BardEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicHelm");
			if (BardEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Helm , creating it ...");
				BardEpicHelm = new ItemTemplate();
				BardEpicHelm.Id_nb = "BardEpicHelm";
				BardEpicHelm.Name = "Moonsung Coif";
				BardEpicHelm.Level = 50;
				BardEpicHelm.Item_Type = 21;
				BardEpicHelm.Model = 1292; //NEED TO WORK ON..
				BardEpicHelm.IsDropable = true;
				BardEpicHelm.IsPickable = true;
				BardEpicHelm.DPS_AF = 100;
				BardEpicHelm.SPD_ABS = 19;
				BardEpicHelm.Object_Type = 37;
				BardEpicHelm.Quality = 100;
				BardEpicHelm.Weight = 22;
				BardEpicHelm.Bonus = 35;
				BardEpicHelm.MaxCondition = 50000;
				BardEpicHelm.MaxDurability = 50000;
				BardEpicHelm.Condition = 50000;
				BardEpicHelm.Durability = 50000;

				BardEpicHelm.Bonus1 = 18;
				BardEpicHelm.Bonus1Type = (int) eStat.CHR;

				BardEpicHelm.Bonus2 = 4;
				BardEpicHelm.Bonus2Type = (int) eProperty.PowerRegenerationRate;

				BardEpicHelm.Bonus3 = 3;
				BardEpicHelm.Bonus3Type = (int) eProperty.Skill_Regrowth;

				BardEpicHelm.Bonus4 = 21;
				BardEpicHelm.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicHelm);
				}


			}
//end item
			//Moonsung Gloves 
			BardEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicGloves");
			if (BardEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Gloves , creating it ...");
				BardEpicGloves = new ItemTemplate();
				BardEpicGloves.Id_nb = "BardEpicGloves";
				BardEpicGloves.Name = "Moonsung Gloves ";
				BardEpicGloves.Level = 50;
				BardEpicGloves.Item_Type = 22;
				BardEpicGloves.Model = 737;
				BardEpicGloves.IsDropable = true;
				BardEpicGloves.IsPickable = true;
				BardEpicGloves.DPS_AF = 100;
				BardEpicGloves.SPD_ABS = 19;
				BardEpicGloves.Object_Type = 37;
				BardEpicGloves.Quality = 100;
				BardEpicGloves.Weight = 22;
				BardEpicGloves.Bonus = 35;
				BardEpicGloves.MaxCondition = 50000;
				BardEpicGloves.MaxDurability = 50000;
				BardEpicGloves.Condition = 50000;
				BardEpicGloves.Durability = 50000;

				BardEpicGloves.Bonus1 = 3;
				BardEpicGloves.Bonus1Type = (int) eProperty.Skill_Nurture;

				BardEpicGloves.Bonus2 = 3;
				BardEpicGloves.Bonus2Type = (int) eProperty.Skill_Music;

				BardEpicGloves.Bonus3 = 12;
				BardEpicGloves.Bonus3Type = (int) eStat.DEX;

				BardEpicGloves.Bonus4 = 33;
				BardEpicGloves.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicGloves);
				}

			}
			//Moonsung Hauberk 
			BardEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicVest");
			if (BardEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Vest , creating it ...");
				BardEpicVest = new ItemTemplate();
				BardEpicVest.Id_nb = "BardEpicVest";
				BardEpicVest.Name = "Moonsung Hauberk";
				BardEpicVest.Level = 50;
				BardEpicVest.Item_Type = 25;
				BardEpicVest.Model = 734;
				BardEpicVest.IsDropable = true;
				BardEpicVest.IsPickable = true;
				BardEpicVest.DPS_AF = 100;
				BardEpicVest.SPD_ABS = 19;
				BardEpicVest.Object_Type = 37;
				BardEpicVest.Quality = 100;
				BardEpicVest.Weight = 22;
				BardEpicVest.Bonus = 35;
				BardEpicVest.MaxCondition = 50000;
				BardEpicVest.MaxDurability = 50000;
				BardEpicVest.Condition = 50000;
				BardEpicVest.Durability = 50000;

				BardEpicVest.Bonus1 = 3;
				BardEpicVest.Bonus1Type = (int) eProperty.Skill_Regrowth;

				BardEpicVest.Bonus2 = 3;
				BardEpicVest.Bonus2Type = (int) eProperty.Skill_Nurture;

				BardEpicVest.Bonus3 = 13;
				BardEpicVest.Bonus3Type = (int) eStat.CON;

				BardEpicVest.Bonus4 = 15;
				BardEpicVest.Bonus4Type = (int) eStat.CHR;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicVest);
				}

			}
			//Moonsung Legs 
			BardEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicLegs");
			if (BardEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Legs , creating it ...");
				BardEpicLegs = new ItemTemplate();
				BardEpicLegs.Id_nb = "BardEpicLegs";
				BardEpicLegs.Name = "Moonsung Legs";
				BardEpicLegs.Level = 50;
				BardEpicLegs.Item_Type = 27;
				BardEpicLegs.Model = 735;
				BardEpicLegs.IsDropable = true;
				BardEpicLegs.IsPickable = true;
				BardEpicLegs.DPS_AF = 100;
				BardEpicLegs.SPD_ABS = 19;
				BardEpicLegs.Object_Type = 37;
				BardEpicLegs.Quality = 100;
				BardEpicLegs.Weight = 22;
				BardEpicLegs.Bonus = 35;
				BardEpicLegs.MaxCondition = 50000;
				BardEpicLegs.MaxDurability = 50000;
				BardEpicLegs.Condition = 50000;
				BardEpicLegs.Durability = 50000;

				BardEpicLegs.Bonus1 = 16;
				BardEpicLegs.Bonus1Type = (int) eStat.CON;

				BardEpicLegs.Bonus2 = 15;
				BardEpicLegs.Bonus2Type = (int) eStat.DEX;

				BardEpicLegs.Bonus3 = 10;
				BardEpicLegs.Bonus3Type = (int) eResist.Body;

				BardEpicLegs.Bonus4 = 10;
				BardEpicLegs.Bonus4Type = (int) eResist.Matter;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicLegs);
				}

			}
			//Moonsung Sleeves 
			BardEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "BardEpicArms");
			if (BardEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bard Epic Arms , creating it ...");
				BardEpicArms = new ItemTemplate();
				BardEpicArms.Id_nb = "BardEpicArms";
				BardEpicArms.Name = "Moonsung Sleeves";
				BardEpicArms.Level = 50;
				BardEpicArms.Item_Type = 28;
				BardEpicArms.Model = 736;
				BardEpicArms.IsDropable = true;
				BardEpicArms.IsPickable = true;
				BardEpicArms.DPS_AF = 100;
				BardEpicArms.SPD_ABS = 19;
				BardEpicArms.Object_Type = 37;
				BardEpicArms.Quality = 100;
				BardEpicArms.Weight = 22;
				BardEpicArms.Bonus = 35;
				BardEpicArms.MaxCondition = 50000;
				BardEpicArms.MaxDurability = 50000;
				BardEpicArms.Condition = 50000;
				BardEpicArms.Durability = 50000;

				BardEpicArms.Bonus1 = 15;
				BardEpicArms.Bonus1Type = (int) eStat.STR;

				BardEpicArms.Bonus2 = 12;
				BardEpicArms.Bonus2Type = (int) eStat.CHR;

				BardEpicArms.Bonus3 = 10;
				BardEpicArms.Bonus3Type = (int) eStat.CON;

				BardEpicArms.Bonus4 = 12;
				BardEpicArms.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(BardEpicArms);
				}

			}
//Champion Epic Sleeves End
			ChampionEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicBoots");
			if (ChampionEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Boots , creating it ...");
				ChampionEpicBoots = new ItemTemplate();
				ChampionEpicBoots.Id_nb = "ChampionEpicBoots";
				ChampionEpicBoots.Name = "Moonglow Boots";
				ChampionEpicBoots.Level = 50;
				ChampionEpicBoots.Item_Type = 23;
				ChampionEpicBoots.Model = 814;
				ChampionEpicBoots.IsDropable = true;
				ChampionEpicBoots.IsPickable = true;
				ChampionEpicBoots.DPS_AF = 100;
				ChampionEpicBoots.SPD_ABS = 27;
				ChampionEpicBoots.Object_Type = 38;
				ChampionEpicBoots.Quality = 100;
				ChampionEpicBoots.Weight = 22;
				ChampionEpicBoots.Bonus = 35;
				ChampionEpicBoots.MaxCondition = 50000;
				ChampionEpicBoots.MaxDurability = 50000;
				ChampionEpicBoots.Condition = 50000;
				ChampionEpicBoots.Durability = 50000;

				ChampionEpicBoots.Bonus1 = 33;
				ChampionEpicBoots.Bonus1Type = (int) eProperty.MaxHealth;

				ChampionEpicBoots.Bonus2 = 10;
				ChampionEpicBoots.Bonus2Type = (int) eResist.Heat;

				ChampionEpicBoots.Bonus3 = 10;
				ChampionEpicBoots.Bonus3Type = (int) eResist.Matter;

				ChampionEpicBoots.Bonus4 = 15;
				ChampionEpicBoots.Bonus4Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicBoots);
				}

			}
//end item
			//Moonglow Coif 
			ChampionEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicHelm");
			if (ChampionEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Helm , creating it ...");
				ChampionEpicHelm = new ItemTemplate();
				ChampionEpicHelm.Id_nb = "ChampionEpicHelm";
				ChampionEpicHelm.Name = "Moonglow Coif";
				ChampionEpicHelm.Level = 50;
				ChampionEpicHelm.Item_Type = 21;
				ChampionEpicHelm.Model = 1292; //NEED TO WORK ON..
				ChampionEpicHelm.IsDropable = true;
				ChampionEpicHelm.IsPickable = true;
				ChampionEpicHelm.DPS_AF = 100;
				ChampionEpicHelm.SPD_ABS = 27;
				ChampionEpicHelm.Object_Type = 38;
				ChampionEpicHelm.Quality = 100;
				ChampionEpicHelm.Weight = 22;
				ChampionEpicHelm.Bonus = 35;
				ChampionEpicHelm.MaxCondition = 50000;
				ChampionEpicHelm.MaxDurability = 50000;
				ChampionEpicHelm.Condition = 50000;
				ChampionEpicHelm.Durability = 50000;

				ChampionEpicHelm.Bonus1 = 3;
				ChampionEpicHelm.Bonus1Type = (int) eProperty.Skill_Valor;

				ChampionEpicHelm.Bonus2 = 12;
				ChampionEpicHelm.Bonus2Type = (int) eStat.CON;

				ChampionEpicHelm.Bonus3 = 12;
				ChampionEpicHelm.Bonus3Type = (int) eStat.QUI;

				ChampionEpicHelm.Bonus4 = 6;
				ChampionEpicHelm.Bonus4Type = (int) eProperty.PowerRegenerationRate;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicHelm);
				}

			}
//end item
			//Moonglow Gloves 
			ChampionEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicGloves");
			if (ChampionEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Gloves , creating it ...");
				ChampionEpicGloves = new ItemTemplate();
				ChampionEpicGloves.Id_nb = "ChampionEpicGloves";
				ChampionEpicGloves.Name = "Moonglow Gloves ";
				ChampionEpicGloves.Level = 50;
				ChampionEpicGloves.Item_Type = 22;
				ChampionEpicGloves.Model = 813;
				ChampionEpicGloves.IsDropable = true;
				ChampionEpicGloves.IsPickable = true;
				ChampionEpicGloves.DPS_AF = 100;
				ChampionEpicGloves.SPD_ABS = 27;
				ChampionEpicGloves.Object_Type = 38;
				ChampionEpicGloves.Quality = 100;
				ChampionEpicGloves.Weight = 22;
				ChampionEpicGloves.Bonus = 35;
				ChampionEpicGloves.MaxCondition = 50000;
				ChampionEpicGloves.MaxDurability = 50000;
				ChampionEpicGloves.Condition = 50000;
				ChampionEpicGloves.Durability = 50000;

				ChampionEpicGloves.Bonus1 = 3;
				ChampionEpicGloves.Bonus1Type = (int) eProperty.Skill_Parry;

				ChampionEpicGloves.Bonus2 = 15;
				ChampionEpicGloves.Bonus2Type = (int) eStat.STR;

				ChampionEpicGloves.Bonus3 = 15;
				ChampionEpicGloves.Bonus3Type = (int) eStat.QUI;

				ChampionEpicGloves.Bonus4 = 10;
				ChampionEpicGloves.Bonus4Type = (int) eResist.Crush;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicGloves);
				}

			}
			//Moonglow Hauberk 
			ChampionEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicVest");
			if (ChampionEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Vest , creating it ...");
				ChampionEpicVest = new ItemTemplate();
				ChampionEpicVest.Id_nb = "ChampionEpicVest";
				ChampionEpicVest.Name = "Moonglow Brestplate";
				ChampionEpicVest.Level = 50;
				ChampionEpicVest.Item_Type = 25;
				ChampionEpicVest.Model = 810;
				ChampionEpicVest.IsDropable = true;
				ChampionEpicVest.IsPickable = true;
				ChampionEpicVest.DPS_AF = 100;
				ChampionEpicVest.SPD_ABS = 27;
				ChampionEpicVest.Object_Type = 38;
				ChampionEpicVest.Quality = 100;
				ChampionEpicVest.Weight = 22;
				ChampionEpicVest.Bonus = 35;
				ChampionEpicVest.MaxCondition = 50000;
				ChampionEpicVest.MaxDurability = 50000;
				ChampionEpicVest.Condition = 50000;
				ChampionEpicVest.Durability = 50000;

				ChampionEpicVest.Bonus1 = 4;
				ChampionEpicVest.Bonus1Type = (int) eProperty.Skill_Valor;

				ChampionEpicVest.Bonus2 = 13;
				ChampionEpicVest.Bonus2Type = (int) eStat.STR;

				ChampionEpicVest.Bonus3 = 13;
				ChampionEpicVest.Bonus3Type = (int) eStat.QUI;

				ChampionEpicVest.Bonus4 = 10;
				ChampionEpicVest.Bonus4Type = (int) eResist.Energy;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicVest);
				}

			}
			//Moonglow Legs 
			ChampionEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicLegs");
			if (ChampionEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Legs , creating it ...");
				ChampionEpicLegs = new ItemTemplate();
				ChampionEpicLegs.Id_nb = "ChampionEpicLegs";
				ChampionEpicLegs.Name = "Moonglow Legs";
				ChampionEpicLegs.Level = 50;
				ChampionEpicLegs.Item_Type = 27;
				ChampionEpicLegs.Model = 811;
				ChampionEpicLegs.IsDropable = true;
				ChampionEpicLegs.IsPickable = true;
				ChampionEpicLegs.DPS_AF = 100;
				ChampionEpicLegs.SPD_ABS = 27;
				ChampionEpicLegs.Object_Type = 38;
				ChampionEpicLegs.Quality = 100;
				ChampionEpicLegs.Weight = 22;
				ChampionEpicLegs.Bonus = 35;
				ChampionEpicLegs.MaxCondition = 50000;
				ChampionEpicLegs.MaxDurability = 50000;
				ChampionEpicLegs.Condition = 50000;
				ChampionEpicLegs.Durability = 50000;

				ChampionEpicLegs.Bonus1 = 15;
				ChampionEpicLegs.Bonus1Type = (int) eStat.CON;

				ChampionEpicLegs.Bonus2 = 15;
				ChampionEpicLegs.Bonus2Type = (int) eStat.DEX;

				ChampionEpicLegs.Bonus3 = 10;
				ChampionEpicLegs.Bonus3Type = (int) eResist.Crush;

				ChampionEpicLegs.Bonus4 = 18;
				ChampionEpicLegs.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicLegs);
				}

			}
			//Moonglow Sleeves 
			ChampionEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ChampionEpicArms");
			if (ChampionEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champion Epic Arms , creating it ...");
				ChampionEpicArms = new ItemTemplate();
				ChampionEpicArms.Id_nb = "ChampionEpicArms";
				ChampionEpicArms.Name = "Moonglow Sleeves";
				ChampionEpicArms.Level = 50;
				ChampionEpicArms.Item_Type = 28;
				ChampionEpicArms.Model = 812;
				ChampionEpicArms.IsDropable = true;
				ChampionEpicArms.IsPickable = true;
				ChampionEpicArms.DPS_AF = 100;
				ChampionEpicArms.SPD_ABS = 27;
				ChampionEpicArms.Object_Type = 38;
				ChampionEpicArms.Quality = 100;
				ChampionEpicArms.Weight = 22;
				ChampionEpicArms.Bonus = 35;
				ChampionEpicArms.MaxCondition = 50000;
				ChampionEpicArms.MaxDurability = 50000;
				ChampionEpicArms.Condition = 50000;
				ChampionEpicArms.Durability = 50000;

				ChampionEpicArms.Bonus1 = 3;
				ChampionEpicArms.Bonus1Type = (int) eProperty.Skill_Large_Weapon;

				ChampionEpicArms.Bonus2 = 10;
				ChampionEpicArms.Bonus2Type = (int) eStat.STR;

				ChampionEpicArms.Bonus3 = 10;
				ChampionEpicArms.Bonus3Type = (int) eStat.QUI;

				ChampionEpicArms.Bonus4 = 33;
				ChampionEpicArms.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(ChampionEpicArms);
				}

			}
			NightshadeEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicBoots");
			if (NightshadeEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Boots , creating it ...");
				NightshadeEpicBoots = new ItemTemplate();
				NightshadeEpicBoots.Id_nb = "NightshadeEpicBoots";
				NightshadeEpicBoots.Name = "Moonlit Boots";
				NightshadeEpicBoots.Level = 50;
				NightshadeEpicBoots.Item_Type = 23;
				NightshadeEpicBoots.Model = 750;
				NightshadeEpicBoots.IsDropable = true;
				NightshadeEpicBoots.IsPickable = true;
				NightshadeEpicBoots.DPS_AF = 100;
				NightshadeEpicBoots.SPD_ABS = 10;
				NightshadeEpicBoots.Object_Type = 33;
				NightshadeEpicBoots.Quality = 100;
				NightshadeEpicBoots.Weight = 22;
				NightshadeEpicBoots.Bonus = 35;
				NightshadeEpicBoots.MaxCondition = 50000;
				NightshadeEpicBoots.MaxDurability = 50000;
				NightshadeEpicBoots.Condition = 50000;
				NightshadeEpicBoots.Durability = 50000;

				NightshadeEpicBoots.Bonus1 = 12;
				NightshadeEpicBoots.Bonus1Type = (int) eStat.STR;

				NightshadeEpicBoots.Bonus2 = 15;
				NightshadeEpicBoots.Bonus2Type = (int) eStat.DEX;

				NightshadeEpicBoots.Bonus3 = 10;
				NightshadeEpicBoots.Bonus3Type = (int) eResist.Thrust;

				NightshadeEpicBoots.Bonus4 = 24;
				NightshadeEpicBoots.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(NightshadeEpicBoots);
				}

			}
//end item
			//Moonlit Coif 
			NightshadeEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicHelm");
			if (NightshadeEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Helm , creating it ...");
				NightshadeEpicHelm = new ItemTemplate();
				NightshadeEpicHelm.Id_nb = "NightshadeEpicHelm";
				NightshadeEpicHelm.Name = "Moonlit Helm";
				NightshadeEpicHelm.Level = 50;
				NightshadeEpicHelm.Item_Type = 21;
				NightshadeEpicHelm.Model = 1292; //NEED TO WORK ON..
				NightshadeEpicHelm.IsDropable = true;
				NightshadeEpicHelm.IsPickable = true;
				NightshadeEpicHelm.DPS_AF = 100;
				NightshadeEpicHelm.SPD_ABS = 10;
				NightshadeEpicHelm.Object_Type = 33;
				NightshadeEpicHelm.Quality = 100;
				NightshadeEpicHelm.Weight = 22;
				NightshadeEpicHelm.Bonus = 35;
				NightshadeEpicHelm.MaxCondition = 50000;
				NightshadeEpicHelm.MaxDurability = 50000;
				NightshadeEpicHelm.Condition = 50000;
				NightshadeEpicHelm.Durability = 50000;

				NightshadeEpicHelm.Bonus1 = 9;
				NightshadeEpicHelm.Bonus1Type = (int) eStat.STR;

				NightshadeEpicHelm.Bonus2 = 9;
				NightshadeEpicHelm.Bonus2Type = (int) eStat.DEX;

				NightshadeEpicHelm.Bonus3 = 9;
				NightshadeEpicHelm.Bonus3Type = (int) eStat.QUI;

				NightshadeEpicHelm.Bonus4 = 39;
				NightshadeEpicHelm.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(NightshadeEpicHelm);
				}

			}
//end item
			//Moonlit Gloves 
			NightshadeEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicGloves");
			if (NightshadeEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Gloves , creating it ...");
				NightshadeEpicGloves = new ItemTemplate();
				NightshadeEpicGloves.Id_nb = "NightshadeEpicGloves";
				NightshadeEpicGloves.Name = "Moonlit Gloves ";
				NightshadeEpicGloves.Level = 50;
				NightshadeEpicGloves.Item_Type = 22;
				NightshadeEpicGloves.Model = 749;
				NightshadeEpicGloves.IsDropable = true;
				NightshadeEpicGloves.IsPickable = true;
				NightshadeEpicGloves.DPS_AF = 100;
				NightshadeEpicGloves.SPD_ABS = 10;
				NightshadeEpicGloves.Object_Type = 33;
				NightshadeEpicGloves.Quality = 100;
				NightshadeEpicGloves.Weight = 22;
				NightshadeEpicGloves.Bonus = 35;
				NightshadeEpicGloves.MaxCondition = 50000;
				NightshadeEpicGloves.MaxDurability = 50000;
				NightshadeEpicGloves.Condition = 50000;
				NightshadeEpicGloves.Durability = 50000;

				NightshadeEpicGloves.Bonus1 = 2;
				NightshadeEpicGloves.Bonus1Type = (int) eProperty.Skill_Critical_Strike;

				NightshadeEpicGloves.Bonus2 = 12;
				NightshadeEpicGloves.Bonus2Type = (int) eStat.DEX;

				NightshadeEpicGloves.Bonus3 = 13;
				NightshadeEpicGloves.Bonus3Type = (int) eStat.QUI;

				NightshadeEpicGloves.Bonus4 = 5;
				NightshadeEpicGloves.Bonus4Type = (int) eProperty.Skill_Envenom;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(NightshadeEpicGloves);
				}

			}
			//Moonlit Hauberk 
			NightshadeEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicVest");
			if (NightshadeEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Vest , creating it ...");
				NightshadeEpicVest = new ItemTemplate();
				NightshadeEpicVest.Id_nb = "NightshadeEpicVest";
				NightshadeEpicVest.Name = "Moonlit Leather Jerking";
				NightshadeEpicVest.Level = 50;
				NightshadeEpicVest.Item_Type = 25;
				NightshadeEpicVest.Model = 746;
				NightshadeEpicVest.IsDropable = true;
				NightshadeEpicVest.IsPickable = true;
				NightshadeEpicVest.DPS_AF = 100;
				NightshadeEpicVest.SPD_ABS = 10;
				NightshadeEpicVest.Object_Type = 33;
				NightshadeEpicVest.Quality = 100;
				NightshadeEpicVest.Weight = 22;
				NightshadeEpicVest.Bonus = 35;
				NightshadeEpicVest.MaxCondition = 50000;
				NightshadeEpicVest.MaxDurability = 50000;
				NightshadeEpicVest.Condition = 50000;
				NightshadeEpicVest.Durability = 50000;

				NightshadeEpicVest.Bonus1 = 10;
				NightshadeEpicVest.Bonus1Type = (int) eStat.STR;

				NightshadeEpicVest.Bonus2 = 10;
				NightshadeEpicVest.Bonus2Type = (int) eStat.DEX;

				NightshadeEpicVest.Bonus3 = 30;
				NightshadeEpicVest.Bonus3Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(NightshadeEpicVest);
				}

			}
			//Moonlit Legs 
			NightshadeEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicLegs");
			if (NightshadeEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Legs , creating it ...");
				NightshadeEpicLegs = new ItemTemplate();
				NightshadeEpicLegs.Id_nb = "NightshadeEpicLegs";
				NightshadeEpicLegs.Name = "Moonlit Leggings";
				NightshadeEpicLegs.Level = 50;
				NightshadeEpicLegs.Item_Type = 27;
				NightshadeEpicLegs.Model = 747;
				NightshadeEpicLegs.IsDropable = true;
				NightshadeEpicLegs.IsPickable = true;
				NightshadeEpicLegs.DPS_AF = 100;
				NightshadeEpicLegs.SPD_ABS = 10;
				NightshadeEpicLegs.Object_Type = 33;
				NightshadeEpicLegs.Quality = 100;
				NightshadeEpicLegs.Weight = 22;
				NightshadeEpicLegs.Bonus = 35;
				NightshadeEpicLegs.MaxCondition = 50000;
				NightshadeEpicLegs.MaxDurability = 50000;
				NightshadeEpicLegs.Condition = 50000;
				NightshadeEpicLegs.Durability = 50000;

				NightshadeEpicLegs.Bonus1 = 16;
				NightshadeEpicLegs.Bonus1Type = (int) eStat.CON;

				NightshadeEpicLegs.Bonus2 = 15;
				NightshadeEpicLegs.Bonus2Type = (int) eStat.DEX;

				NightshadeEpicLegs.Bonus3 = 10;
				NightshadeEpicLegs.Bonus3Type = (int) eResist.Crush;

				NightshadeEpicLegs.Bonus4 = 10;
				NightshadeEpicLegs.Bonus4Type = (int) eResist.Slash;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(NightshadeEpicLegs);
				}

			}
			//Moonlit Sleeves 
			NightshadeEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "NightshadeEpicArms");
			if (NightshadeEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Arms , creating it ...");
				NightshadeEpicArms = new ItemTemplate();
				NightshadeEpicArms.Id_nb = "NightshadeEpicArms";
				NightshadeEpicArms.Name = "Moonlit Sleeves";
				NightshadeEpicArms.Level = 50;
				NightshadeEpicArms.Item_Type = 28;
				NightshadeEpicArms.Model = 748;
				NightshadeEpicArms.IsDropable = true;
				NightshadeEpicArms.IsPickable = true;
				NightshadeEpicArms.DPS_AF = 100;
				NightshadeEpicArms.SPD_ABS = 10;
				NightshadeEpicArms.Object_Type = 33;
				NightshadeEpicArms.Quality = 100;
				NightshadeEpicArms.Weight = 22;
				NightshadeEpicArms.Bonus = 35;
				NightshadeEpicArms.MaxCondition = 50000;
				NightshadeEpicArms.MaxDurability = 50000;
				NightshadeEpicArms.Condition = 50000;
				NightshadeEpicArms.Durability = 50000;

				NightshadeEpicArms.Bonus1 = 4;
				NightshadeEpicArms.Bonus1Type = (int) eProperty.Skill_Celtic_Dual;

				NightshadeEpicArms.Bonus2 = 16;
				NightshadeEpicArms.Bonus2Type = (int) eStat.CON;

				NightshadeEpicArms.Bonus3 = 15;
				NightshadeEpicArms.Bonus3Type = (int) eStat.DEX;

				NightshadeEpicArms.Bonus4 = 6;
				NightshadeEpicArms.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
                    GameServer.Database.AddNewObject(NightshadeEpicArms);
				}

			}
			EnchanterEpicBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicBoots");
			if (EnchanterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Boots , creating it ...");
				EnchanterEpicBoots = new ItemTemplate();
				EnchanterEpicBoots.Id_nb = "EnchanterEpicBoots";
				EnchanterEpicBoots.Name = "Moonspun Boots";
				EnchanterEpicBoots.Level = 50;
				EnchanterEpicBoots.Item_Type = 23;
				EnchanterEpicBoots.Model = 382;
				EnchanterEpicBoots.IsDropable = true;
				EnchanterEpicBoots.IsPickable = true;
				EnchanterEpicBoots.DPS_AF = 50;
				EnchanterEpicBoots.SPD_ABS = 0;
				EnchanterEpicBoots.Object_Type = 32;
				EnchanterEpicBoots.Quality = 100;
				EnchanterEpicBoots.Weight = 22;
				EnchanterEpicBoots.Bonus = 35;
				EnchanterEpicBoots.MaxCondition = 50000;
				EnchanterEpicBoots.MaxDurability = 50000;
				EnchanterEpicBoots.Condition = 50000;
				EnchanterEpicBoots.Durability = 50000;

				EnchanterEpicBoots.Bonus1 = 12;
				EnchanterEpicBoots.Bonus1Type = (int) eStat.CON;

				EnchanterEpicBoots.Bonus2 = 12;
				EnchanterEpicBoots.Bonus2Type = (int) eStat.DEX;

				EnchanterEpicBoots.Bonus3 = 12;
				EnchanterEpicBoots.Bonus3Type = (int) eResist.Body;

				EnchanterEpicBoots.Bonus4 = 39;
				EnchanterEpicBoots.Bonus4Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicBoots);
				}

			}
//end item
			//Moonspun Coif 
			EnchanterEpicHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicHelm");
			if (EnchanterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Helm , creating it ...");
				EnchanterEpicHelm = new ItemTemplate();
				EnchanterEpicHelm.Id_nb = "EnchanterEpicHelm";
				EnchanterEpicHelm.Name = "Moonspun Cap";
				EnchanterEpicHelm.Level = 50;
				EnchanterEpicHelm.Item_Type = 21;
				EnchanterEpicHelm.Model = 1298; //NEED TO WORK ON..
				EnchanterEpicHelm.IsDropable = true;
				EnchanterEpicHelm.IsPickable = true;
				EnchanterEpicHelm.DPS_AF = 50;
				EnchanterEpicHelm.SPD_ABS = 0;
				EnchanterEpicHelm.Object_Type = 32;
				EnchanterEpicHelm.Quality = 100;
				EnchanterEpicHelm.Weight = 22;
				EnchanterEpicHelm.Bonus = 35;
				EnchanterEpicHelm.MaxCondition = 50000;
				EnchanterEpicHelm.MaxDurability = 50000;
				EnchanterEpicHelm.Condition = 50000;
				EnchanterEpicHelm.Durability = 50000;

				EnchanterEpicHelm.Bonus1 = 21;
				EnchanterEpicHelm.Bonus1Type = (int) eProperty.MaxHealth;

				EnchanterEpicHelm.Bonus2 = 8;
				EnchanterEpicHelm.Bonus2Type = (int) eResist.Energy;

				EnchanterEpicHelm.Bonus3 = 4;
				EnchanterEpicHelm.Bonus3Type = (int) eProperty.Skill_Enchantments;

				EnchanterEpicHelm.Bonus4 = 18;
				EnchanterEpicHelm.Bonus4Type = (int) eStat.INT;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicHelm);
				}

			}
//end item
			//Moonspun Gloves 
			EnchanterEpicGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicGloves");
			if (EnchanterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Gloves , creating it ...");
				EnchanterEpicGloves = new ItemTemplate();
				EnchanterEpicGloves.Id_nb = "EnchanterEpicGloves";
				EnchanterEpicGloves.Name = "Moonspun Gloves ";
				EnchanterEpicGloves.Level = 50;
				EnchanterEpicGloves.Item_Type = 22;
				EnchanterEpicGloves.Model = 381;
				EnchanterEpicGloves.IsDropable = true;
				EnchanterEpicGloves.IsPickable = true;
				EnchanterEpicGloves.DPS_AF = 50;
				EnchanterEpicGloves.SPD_ABS = 0;
				EnchanterEpicGloves.Object_Type = 32;
				EnchanterEpicGloves.Quality = 100;
				EnchanterEpicGloves.Weight = 22;
				EnchanterEpicGloves.Bonus = 35;
				EnchanterEpicGloves.MaxCondition = 50000;
				EnchanterEpicGloves.MaxDurability = 50000;
				EnchanterEpicGloves.Condition = 50000;
				EnchanterEpicGloves.Durability = 50000;

				EnchanterEpicGloves.Bonus1 = 30;
				EnchanterEpicGloves.Bonus1Type = (int) eProperty.MaxHealth;

				EnchanterEpicGloves.Bonus2 = 4;
				EnchanterEpicGloves.Bonus2Type = (int) eProperty.Skill_Mana;

				EnchanterEpicGloves.Bonus3 = 6;
				EnchanterEpicGloves.Bonus3Type = (int) eStat.INT;

				EnchanterEpicGloves.Bonus4 = 13;
				EnchanterEpicGloves.Bonus4Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicGloves);
				}

			}
			//Moonspun Hauberk 
			EnchanterEpicVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicVest");
			if (EnchanterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Vest , creating it ...");
				EnchanterEpicVest = new ItemTemplate();
				EnchanterEpicVest.Id_nb = "EnchanterEpicVest";
				EnchanterEpicVest.Name = "Moonspun Vest";
				EnchanterEpicVest.Level = 50;
				EnchanterEpicVest.Item_Type = 25;
				EnchanterEpicVest.Model = 781;
				EnchanterEpicVest.IsDropable = true;
				EnchanterEpicVest.IsPickable = true;
				EnchanterEpicVest.DPS_AF = 50;
				EnchanterEpicVest.SPD_ABS = 0;
				EnchanterEpicVest.Object_Type = 32;
				EnchanterEpicVest.Quality = 100;
				EnchanterEpicVest.Weight = 22;
				EnchanterEpicVest.Bonus = 35;
				EnchanterEpicVest.MaxCondition = 50000;
				EnchanterEpicVest.MaxDurability = 50000;
				EnchanterEpicVest.Condition = 50000;
				EnchanterEpicVest.Durability = 50000;

				EnchanterEpicVest.Bonus1 = 30;
				EnchanterEpicVest.Bonus1Type = (int) eProperty.MaxHealth;

				EnchanterEpicVest.Bonus2 = 15;
				EnchanterEpicVest.Bonus2Type = (int) eStat.INT;

				EnchanterEpicVest.Bonus3 = 15;
				EnchanterEpicVest.Bonus3Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicVest);
				}

			}
			//Moonspun Legs 
			EnchanterEpicLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicLegs");
			if (EnchanterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Legs , creating it ...");
				EnchanterEpicLegs = new ItemTemplate();
				EnchanterEpicLegs.Id_nb = "EnchanterEpicLegs";
				EnchanterEpicLegs.Name = "Moonspun Pants";
				EnchanterEpicLegs.Level = 50;
				EnchanterEpicLegs.Item_Type = 27;
				EnchanterEpicLegs.Model = 379;
				EnchanterEpicLegs.IsDropable = true;
				EnchanterEpicLegs.IsPickable = true;
				EnchanterEpicLegs.DPS_AF = 50;
				EnchanterEpicLegs.SPD_ABS = 0;
				EnchanterEpicLegs.Object_Type = 32;
				EnchanterEpicLegs.Quality = 100;
				EnchanterEpicLegs.Weight = 22;
				EnchanterEpicLegs.Bonus = 35;
				EnchanterEpicLegs.MaxCondition = 50000;
				EnchanterEpicLegs.MaxDurability = 50000;
				EnchanterEpicLegs.Condition = 50000;
				EnchanterEpicLegs.Durability = 50000;

				EnchanterEpicLegs.Bonus1 = 16;
				EnchanterEpicLegs.Bonus1Type = (int) eStat.CON;

				EnchanterEpicLegs.Bonus2 = 15;
				EnchanterEpicLegs.Bonus2Type = (int) eStat.DEX;

				EnchanterEpicLegs.Bonus3 = 10;
				EnchanterEpicLegs.Bonus3Type = (int) eResist.Heat;

				EnchanterEpicLegs.Bonus4 = 10;
				EnchanterEpicLegs.Bonus4Type = (int) eResist.Cold;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicLegs);
				}

			}
			//Moonspun Sleeves 
			EnchanterEpicArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "EnchanterEpicArms");
			if (EnchanterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Arms , creating it ...");
				EnchanterEpicArms = new ItemTemplate();
				EnchanterEpicArms.Id_nb = "EnchanterEpicArms";
				EnchanterEpicArms.Name = "Moonspun Sleeves";
				EnchanterEpicArms.Level = 50;
				EnchanterEpicArms.Item_Type = 28;
				EnchanterEpicArms.Model = 380;
				EnchanterEpicArms.IsDropable = true;
				EnchanterEpicArms.IsPickable = true;
				EnchanterEpicArms.DPS_AF = 50;
				EnchanterEpicArms.SPD_ABS = 0;
				EnchanterEpicArms.Object_Type = 32;
				EnchanterEpicArms.Quality = 100;
				EnchanterEpicArms.Weight = 22;
				EnchanterEpicArms.Bonus = 35;
				EnchanterEpicArms.MaxCondition = 50000;
				EnchanterEpicArms.MaxDurability = 50000;
				EnchanterEpicArms.Condition = 50000;
				EnchanterEpicArms.Durability = 50000;

				EnchanterEpicArms.Bonus1 = 27;
				EnchanterEpicArms.Bonus1Type = (int) eProperty.MaxHealth;

				EnchanterEpicArms.Bonus2 = 10;
				EnchanterEpicArms.Bonus2Type = (int) eStat.INT;

				EnchanterEpicArms.Bonus3 = 5;
				EnchanterEpicArms.Bonus3Type = (int) eProperty.Skill_Light;

				EnchanterEpicArms.Bonus4 = 10;
				EnchanterEpicArms.Bonus4Type = (int) eStat.DEX;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(EnchanterEpicArms);
				}

			}

//Champion Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.AddHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we bring to Brigit the possibility to give this quest to players */
			Brigit.AddQuestToGive(typeof (Essence_50));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.RemoveHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we remove to Brigit the possibility to give this quest to players */
			Brigit.RemoveQuestToGive(typeof (Essence_50));
		}

		protected static void TalkToBrigit(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Brigit, QuestMgr.GetIDForQuestType(typeof(Essence_50)), "Will you help Brigit [Path of Essence Level 50 Epic]?");
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

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Champion &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bard &&
				player.CharacterClass.ID != (byte) eCharacterClass.Nightshade &&
				player.CharacterClass.ID != (byte) eCharacterClass.Enchanter)
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
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Essence_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Brigit.GiveQuest(typeof (Essence_50), player, 1))
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
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Essence_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Caithor.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Moonstone from Caithor", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, Moonstone);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Brigit.Name && gArgs.Item.Id_nb == Moonstone.Id_nb)
				{
					RemoveItem(Brigit, player, Moonstone);
					Brigit.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Moonstone, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Champion)
			{
				GiveItem(m_questPlayer, ChampionEpicArms);
				GiveItem(m_questPlayer, ChampionEpicBoots);
				GiveItem(m_questPlayer, ChampionEpicGloves);
				GiveItem(m_questPlayer, ChampionEpicHelm);
				GiveItem(m_questPlayer, ChampionEpicLegs);
				GiveItem(m_questPlayer, ChampionEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Bard)
			{
				GiveItem(m_questPlayer, BardEpicArms);
				GiveItem(m_questPlayer, BardEpicBoots);
				GiveItem(m_questPlayer, BardEpicGloves);
				GiveItem(m_questPlayer, BardEpicHelm);
				GiveItem(m_questPlayer, BardEpicLegs);
				GiveItem(m_questPlayer, BardEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Enchanter)
			{
				GiveItem(m_questPlayer, EnchanterEpicArms);
				GiveItem(m_questPlayer, EnchanterEpicBoots);
				GiveItem(m_questPlayer, EnchanterEpicGloves);
				GiveItem(m_questPlayer, EnchanterEpicHelm);
				GiveItem(m_questPlayer, EnchanterEpicLegs);
				GiveItem(m_questPlayer, EnchanterEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte) eCharacterClass.Nightshade)
			{
				GiveItem(m_questPlayer, NightshadeEpicArms);
				GiveItem(m_questPlayer, NightshadeEpicBoots);
				GiveItem(m_questPlayer, NightshadeEpicGloves);
				GiveItem(m_questPlayer, NightshadeEpicHelm);
				GiveItem(m_questPlayer, NightshadeEpicLegs);
				GiveItem(m_questPlayer, NightshadeEpicVest);
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
