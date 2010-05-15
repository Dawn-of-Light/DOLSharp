/*
*Author         : Etaew - Fallen Realms
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BHorn%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Horn Twin (level 50)
*Quest Classes  : Mentalist, Druid, Blademaster, Nighthsade(Path of Essence)
*Quest Version  : v1
*
*ToDo:
*   Add Bonuses to Epic Items
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
	public class Harmony_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Horn Twin";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Revelin = null; // Start NPC
		//private static GameNPC Lauralaye = null; //Reward NPC
		private static GameNPC Cailean = null; // Mob to kill

		private static ItemTemplate Horn = null; //ball of flame        
		private static ItemTemplate BlademasterEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate BlademasterEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate BlademasterEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate BlademasterEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate BlademasterEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate BlademasterEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate DruidEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate DruidEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate DruidEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate DruidEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate DruidEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate DruidEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate MentalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate MentalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate MentalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate MentalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate MentalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate MentalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate AnimistEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate AnimistEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate AnimistEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate AnimistEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate AnimistEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate AnimistEpicArms = null; //Subterranean Sleeves 
		private static ItemTemplate ValewalkerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ValewalkerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ValewalkerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ValewalkerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ValewalkerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ValewalkerEpicArms = null; //Subterranean Sleeves  
		private static ItemTemplate VampiirEpicBoots = null;
		private static ItemTemplate VampiirEpicHelm = null;
		private static ItemTemplate VampiirEpicGloves = null;
		private static ItemTemplate VampiirEpicVest = null;
		private static ItemTemplate VampiirEpicLegs = null;
		private static ItemTemplate VampiirEpicArms = null;
		private static ItemTemplate BainsheeEpicBoots = null;
		private static ItemTemplate BainsheeEpicHelm = null;
		private static ItemTemplate BainsheeEpicGloves = null;
		private static ItemTemplate BainsheeEpicVest = null;
		private static ItemTemplate BainsheeEpicLegs = null;
		private static ItemTemplate BainsheeEpicArms = null;

		// Constructors
		public Harmony_50()
			: base()
		{
		}

		public Harmony_50(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		public Harmony_50(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public Harmony_50(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Revelin", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Revelin , creating it ...");
				Revelin = new GameNPC();
				Revelin.Model = 361;
				Revelin.Name = "Revelin";
				Revelin.GuildName = "";
				Revelin.Realm = eRealm.Hibernia;
				Revelin.CurrentRegionID = 200;
				Revelin.Size = 42;
				Revelin.Level = 20;
				Revelin.X = 344387;
				Revelin.Y = 706197;
				Revelin.Z = 6351;
				Revelin.Heading = 2127;
				Revelin.Flags ^= (uint)GameNPC.eFlags.PEACE;
				Revelin.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Revelin.SaveIntoDatabase();
				}

			}
			else
				Revelin = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailean", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Cailean , creating it ...");
				Cailean = new GameNPC();
				Cailean.Model = 98;
				Cailean.Name = "Cailean";
				Cailean.GuildName = "";
				Cailean.Realm = eRealm.None;
				Cailean.CurrentRegionID = 200;
				Cailean.Size = 60;
				Cailean.Level = 65;
				Cailean.X = 479042;
				Cailean.Y = 508134;
				Cailean.Z = 4569;
				Cailean.Heading = 3319;
				Cailean.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Cailean.SaveIntoDatabase();
				}

			}
			else
				Cailean = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			Horn = GameServer.Database.FindObjectByKey<ItemTemplate>("Horn");
			if (Horn == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Horn , creating it ...");
				Horn = new ItemTemplate();
				Horn.Id_nb = "Horn";
				Horn.Name = "Horn";
				Horn.Level = 8;
				Horn.Item_Type = 29;
				Horn.Model = 586;
				Horn.IsDropable = false;
				Horn.IsPickable = false;
				Horn.DPS_AF = 0;
				Horn.SPD_ABS = 0;
				Horn.Object_Type = 41;
				Horn.Hand = 0;
				Horn.Type_Damage = 0;
				Horn.Quality = 100;
				Horn.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(Horn);
				}

			}
			// end item
			ItemTemplate i = null;

			DruidEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicBoots");
			if (DruidEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicBoots";
				i.Name = "Sidhe Scale Boots";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 743;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 9;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 9;
				i.Bonus2Type = (int)eStat.QUI;

				i.Bonus3 = 14;
				i.Bonus3Type = (int)eResist.Body;

				i.Bonus4 = 36;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}

				DruidEpicBoots = i;

			}
			//end item
			//Sidhe Scale Coif
			DruidEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicHelm");
			if (DruidEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicHelm";
				i.Name = "Sidhe Scale Coif";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.EMP;

				i.Bonus2 = 3;
				i.Bonus2Type = (int)eProperty.Skill_Nurture;

				i.Bonus3 = 3;
				i.Bonus3Type = (int)eProperty.Skill_Nature;

				i.Bonus4 = 27;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				DruidEpicHelm = i;

			}
			//end item
			//Sidhe Scale Gloves
			DruidEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicGloves");
			if (DruidEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicGloves";
				i.Name = "Sidhe Scale Gloves ";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 742;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 3;
				i.Bonus1Type = (int)eProperty.Skill_Regrowth;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eProperty.MaxMana;

				i.Bonus3 = 12;
				i.Bonus3Type = (int)eStat.DEX;

				i.Bonus4 = 12;
				i.Bonus4Type = (int)eStat.EMP;
				{
					GameServer.Database.AddObject(i);
				}
				DruidEpicGloves = i;

			}
			//Sidhe Scale Hauberk
			DruidEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicVest");
			if (DruidEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicVest";
				i.Name = "Sidhe Scale Breastplate";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 739;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.EMP;

				i.Bonus2 = 3;
				i.Bonus2Type = (int)eProperty.Skill_Nature;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Slash;

				i.Bonus4 = 30;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				DruidEpicVest = i;

			}
			//Sidhe Scale Legs
			DruidEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicLegs");
			if (DruidEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicLegs";
				i.Name = "Sidhe Scale Leggings";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 740;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 57;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 8;
				i.Bonus2Type = (int)eResist.Crush;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Spirit;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Cold;
				{
					GameServer.Database.AddObject(i);
				}

				DruidEpicLegs = i;

			}
			//Sidhe Scale Sleeves
			DruidEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("DruidEpicArms");
			if (DruidEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "DruidEpicArms";
				i.Name = "Sidhe Scale Sleeves";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 741;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 38;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 13;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 13;
				i.Bonus3Type = (int)eStat.EMP;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddObject(i);
				}

				DruidEpicArms = i;

			}
			//Blademaster Epic Sleeves End
			BlademasterEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicBoots");
			if (BlademasterEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicBoots";
				i.Name = "Sidhe Studded Boots";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 786;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.QUI;

				i.Bonus3 = 24;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Cold;
				{
					GameServer.Database.AddObject(i);
				}
				BlademasterEpicBoots = i;

			}
			//end item
			//Sidhe Studded Coif
			BlademasterEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicHelm");
			if (BlademasterEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicHelm";
				i.Name = "Sidhe Studded Helm";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 30;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eResist.Spirit;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 16;
				i.Bonus4Type = (int)eStat.QUI;
				{
					GameServer.Database.AddObject(i);
				}

				BlademasterEpicHelm = i;

			}
			//end item
			//Sidhe Studded Gloves
			BlademasterEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicGloves");
			if (BlademasterEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicGloves";
				i.Name = "Sidhe Studded Gloves ";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 785;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 3;
				i.Bonus3Type = (int)eProperty.Skill_Celtic_Dual;

				i.Bonus4 = 3;
				i.Bonus4Type = (int)eProperty.Skill_Parry;
				{
					GameServer.Database.AddObject(i);
				}

				BlademasterEpicGloves = i;

			}
			//Sidhe Studded Hauberk
			BlademasterEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicVest");
			if (BlademasterEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicVest";
				i.Name = "Sidhe Studded Hauberk";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 782;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 12;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 33;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Slash;
				{
					GameServer.Database.AddObject(i);
				}

				BlademasterEpicVest = i;

			}
			//Sidhe Studded Legs
			BlademasterEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicLegs");
			if (BlademasterEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicLegs";
				i.Name = "Sidhe Studded Leggings";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 783;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.QUI;

				i.Bonus2 = 12;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 27;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 12;
				i.Bonus4Type = (int)eResist.Cold;
				{
					GameServer.Database.AddObject(i);
				}

				BlademasterEpicLegs = i;

			}
			//Sidhe Studded Sleeves
			BlademasterEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterEpicArms");
			if (BlademasterEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BlademasterEpicArms";
				i.Name = "Sidhe Studded Sleeves";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 784;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 19;
				i.Object_Type = 37;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 16;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Heat;
				{
					GameServer.Database.AddObject(i);
				}

				BlademasterEpicArms = i;

			}
			AnimistEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicBoots");
			if (AnimistEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicBoots";
				i.Name = "Brightly Woven Boots";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 382;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 12;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 27;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 12;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddObject(i);
				}
				AnimistEpicBoots = i;

			}
			//end item
			//Brightly Woven Coif
			AnimistEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicHelm");
			if (AnimistEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicHelm";
				i.Name = "Brightly Woven Cap";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Arboreal;

				i.Bonus3 = 21;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddObject(i);
				}

				AnimistEpicHelm = i;

			}
			//end item
			//Brightly Woven Gloves
			AnimistEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicGloves");
			if (AnimistEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicGloves";
				i.Name = "Brightly Woven Gloves ";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 381;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 9;
				i.Bonus2Type = (int)eStat.INT;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eProperty.Skill_Creeping;

				i.Bonus4 = 30;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				AnimistEpicGloves = i;

			}
			//Brightly Woven Hauberk
			AnimistEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicVest");
			if (AnimistEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicVest";
				i.Name = "Brightly Woven Robe";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 1186;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 30;
				i.Bonus2Type = (int)eProperty.MaxHealth;

				i.Bonus3 = 6;
				i.Bonus3Type = (int)eProperty.MaxMana;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Body;
				{
					GameServer.Database.AddObject(i);
				}
				AnimistEpicVest = i;

			}
			//Brightly Woven Legs
			AnimistEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicLegs");
			if (AnimistEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicLegs";
				i.Name = "Brightly Woven Pants";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 379;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 16;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Body;
				{
					GameServer.Database.AddObject(i);
				}
				AnimistEpicLegs = i;

			}
			//Brightly Woven Sleeves
			AnimistEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("AnimistEpicArms");
			if (AnimistEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "AnimistEpicArms";
				i.Name = "Brightly Woven Sleeves";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 380;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 27;
				i.Bonus2Type = (int)eProperty.MaxHealth;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eStat.INT;

				i.Bonus4 = 4;
				i.Bonus4Type = (int)eProperty.Skill_Mana;
				{
					GameServer.Database.AddObject(i);
				}
				AnimistEpicArms = i;

			}
			MentalistEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicBoots");
			if (MentalistEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicBoots";
				i.Name = "Sidhe Woven Boots";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 382;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 12;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 12;
				i.Bonus3Type = (int)eResist.Matter;

				i.Bonus4 = 27;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicBoots = i;

			}
			//end item
			//Sidhe Woven Coif
			MentalistEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicHelm");
			if (MentalistEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicHelm";
				i.Name = "Sidhe Woven Cap";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1298; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Mentalism;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Thrust;

				i.Bonus4 = 21;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicHelm = i;

			}
			//end item
			//Sidhe Woven Gloves
			MentalistEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicGloves");
			if (MentalistEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicGloves";
				i.Name = "Sidhe Woven Gloves ";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 381;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 30;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Light;

				i.Bonus3 = 9;
				i.Bonus3Type = (int)eStat.INT;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eStat.DEX;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicGloves = i;

			}
			//Sidhe Woven Hauberk
			MentalistEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicVest");
			if (MentalistEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicVest";
				i.Name = "Sidhe Woven Vest";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 745;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 8;
				i.Bonus2Type = (int)eResist.Body;

				i.Bonus3 = 30;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eProperty.MaxMana;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicVest = i;

			}
			//Sidhe Woven Legs
			MentalistEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicLegs");
			if (MentalistEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicLegs";
				i.Name = "Sidhe Woven Pants";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 379;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 16;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Body;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicLegs = i;

			}
			//Sidhe Woven Sleeves
			MentalistEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("MentalistEpicArms");
			if (MentalistEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MentalistEpicArms";
				i.Name = "Sidhe Woven Sleeves";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 380;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 27;
				i.Bonus2Type = (int)eProperty.MaxHealth;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eStat.INT;

				i.Bonus4 = 4;
				i.Bonus4Type = (int)eProperty.Skill_Mana;
				{
					GameServer.Database.AddObject(i);
				}
				MentalistEpicArms = i;

			}
			ValewalkerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicBoots");
			if (ValewalkerEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicBoots";
				i.Name = "Boots of the Misty Glade";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 382;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eResist.Matter;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 33;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicBoots = i;

			}
			//end item
			//Misty Glade Coif
			ValewalkerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicHelm");
			if (ValewalkerEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicHelm";
				i.Name = "Cap of the Misty Glade";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 3;
				i.Bonus1Type = (int)eProperty.Skill_Arboreal;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eProperty.MaxMana;

				i.Bonus3 = 12;
				i.Bonus3Type = (int)eStat.CON;

				i.Bonus4 = 12;
				i.Bonus4Type = (int)eStat.INT;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicHelm = i;

			}
			//end item
			//Misty Glade Gloves
			ValewalkerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicGloves");
			if (ValewalkerEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicGloves";
				i.Name = "Gloves of the Misty Glades";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 381;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 3;
				i.Bonus1Type = (int)eProperty.Skill_Parry;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.CON;

				i.Bonus3 = 15;
				i.Bonus3Type = (int)eStat.DEX;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Crush;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicGloves = i;

			}
			//Misty Glade Hauberk
			ValewalkerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicVest");
			if (ValewalkerEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicVest";
				i.Name = "Robe of the Misty Glade";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 1003;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 13;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eProperty.Skill_Arboreal;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicVest = i;

			}
			//Misty Glade Legs
			ValewalkerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicLegs");
			if (ValewalkerEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicLegs";
				i.Name = "Pants of the Misty Glade";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 379;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.CON;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Crush;

				i.Bonus4 = 18;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicLegs = i;

			}
			//Misty Glade Sleeves
			ValewalkerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ValewalkerEpicArms");
			if (ValewalkerEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ValewalkerEpicArms";
				i.Name = "Sleeves of the Misty Glade";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 380;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 3;
				i.Bonus1Type = (int)eProperty.Skill_Scythe;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eStat.INT;

				i.Bonus4 = 33;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				ValewalkerEpicArms = i;

			}

			#region Vampiir
			VampiirEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicBoots");
			if (VampiirEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicBoots";
				i.Name = "Archfiend Etched Boots";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 2927;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Strength: 12 pts
				 *   Dexterity: 15 pts
				 *   Thrust Resist: 10%
				 *   Hits: 24 pts
				 */

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Thrust;

				i.Bonus4 = 24;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicBoots = i;

			}
			//end item
			//Misty Glade Coif
			VampiirEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicHelm");
			if (VampiirEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicHelm";
				i.Name = "Archfiend Etched Helm";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Strength: 6 pts
				 *   Constitution: 16 pts
				 *   Dexterity: 6 pts
				 *   Hits: 30 pts
				 */

				i.Bonus1 = 6;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 16;
				i.Bonus2Type = (int)eStat.CON;

				i.Bonus3 = 6;
				i.Bonus3Type = (int)eStat.DEX;

				i.Bonus4 = 30;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicHelm = i;

			}
			//end item
			//Misty Glade Gloves
			VampiirEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicGloves");
			if (VampiirEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicGloves";
				i.Name = "Archfiend Etched Gloves";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 2926;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Dexterity: 12 pts
				 *   Quickness: 13 pts
				 *   Dementia: +2 pts
				 *   Shadow Mastery: +5 pts
				 */

				i.Bonus1 = 12;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.QUI;

				i.Bonus3 = 2;
				i.Bonus3Type = (int)eProperty.Skill_Dementia;

				i.Bonus4 = 5;
				i.Bonus4Type = (int)eProperty.Skill_ShadowMastery;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicGloves = i;

			}
			//Misty Glade Hauberk
			VampiirEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicVest");
			if (VampiirEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicVest";
				i.Name = "Archfiend Etched Vest";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 2923;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Strength: 10 pts
				 *   Dexterity: 10 pts
				 *   Quickness: 10 pts
				 *   Hits: 30 pts
				 */

				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eStat.QUI;

				i.Bonus4 = 30;
				i.Bonus4Type = (int)eProperty.MaxHealth;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicVest = i;

			}
			//Misty Glade Legs
			VampiirEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicLegs");
			if (VampiirEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicLegs";
				i.Name = "Archfiend Etched Leggings";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 2924;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Constitution: 16 pts
				 *   Dexterity: 15 pts
				 *   Crush Resist: 10%
				 *   Slash Resist: 10%
				 */

				i.Bonus1 = 16;
				i.Bonus1Type = (int)eStat.CON ;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 10;
				i.Bonus3Type = (int)eResist.Crush;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Slash;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicLegs = i;

			}
			//Misty Glade Sleeves
			VampiirEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirEpicArms");
			if (VampiirEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "VampiirEpicArms";
				i.Name = "Archfiend Etched Sleeves";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 2925;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 10;
				i.Object_Type = (int)eObjectType.Leather;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Strength: 15 pts
				 *   Dexterity: 15 pts
				 *   Cold Resist: 6%
				 *   Vampiiric Embrace: +4 pts
				 */

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 6;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 4;
				i.Bonus4Type = (int)eProperty.Skill_VampiiricEmbrace;
				{
					GameServer.Database.AddObject(i);
				}
				VampiirEpicArms = i;

			}
			#endregion
			#region Bainshee
			BainsheeEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicBoots");
			if (BainsheeEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicBoots";
				i.Name = "Boots of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 2952;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Intelligence: 18 pts
				 *   Cold Resist: 6%
				 *   Hits: 40 pts
				 *   Heat Resist: 6%
				 *  int cap 5
				 * hit cap 40
				 */

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eResist.Cold;

				i.Bonus3 = 40;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eResist.Heat;

				i.Bonus5 = 5;
				i.Bonus5Type = (int)eProperty.IntCapBonus;

				i.Bonus6 = 40;
				i.Bonus6Type = (int)eProperty.MaxHealthCapBonus;

				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicBoots = i;

			}
			//end item
			//Misty Glade Coif
			BainsheeEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicHelm");
			if (BainsheeEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicHelm";
				i.Name = "Wreath of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1292; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Constitution: 18 pts
				 *   Body Resist: 6%
				 *   Hits: 40 pts
				 *   Energy Resist: 6%
				 *   Hit Points bonus cap: 40
				 *   Constitution attribute cap: 5
				 */

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eResist.Body;

				i.Bonus3 = 40;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eResist.Energy;

				i.Bonus5 = 40;
				i.Bonus4Type = (int)eProperty.MaxHealthCapBonus;

				i.Bonus6 = 5;
				i.Bonus4Type = (int)eProperty.ConCapBonus;
				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicHelm = i;

			}
			//end item
			//Misty Glade Gloves
			BainsheeEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicGloves");
			if (BainsheeEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicGloves";
				i.Name = "Gloves of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 2950;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Dexterity: 18 pts
				 *   Matter Resist: 6%
				 *   Spirit Resist: 6%
				 *   Power bonus cap: 6
				 *   Dexterity attribute cap: 5
				 *   Power Pool: 6%
				 */

				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eResist.Matter;

				i.Bonus3 = 6;
				i.Bonus3Type = (int)eResist.Spirit;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eProperty.MaxMana;

				i.Bonus5 = 5;
				i.Bonus5Type = (int)eProperty.DexCapBonus;

				i.Bonus6 = 6;
				i.Bonus6Type = (int)eProperty.PowerPool;

				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicGloves = i;

			}
			//Keening Spirit Hauberk
			BainsheeEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicVest");
			if (BainsheeEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicVest";
				i.Name = "Robe of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 2922;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Intelligence: 15 pts
				 *   Crush Resist: 7%
				 *   Hits: 40 pts
				 *   ALL Magic Skills: +3
				 *   Intelligence attribute cap: 5
				 *   Hit Points bonus cap: 40
				 */

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 7;
				i.Bonus2Type = (int)eResist.Crush;

				i.Bonus3 = 40;
				i.Bonus3Type = (int)eProperty.MaxHealth;

				i.Bonus4 = 3;
				i.Bonus4Type = (int)eProperty.AllMagicSkills;

				i.Bonus5 = 5;
				i.Bonus5Type = (int)eProperty.IntCapBonus;

				i.Bonus6 = 40;
				i.Bonus6Type = (int)eProperty.MaxHealthCapBonus;
				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicVest = i;

			}
			//Keening Spirit Legs
			BainsheeEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicLegs");
			if (BainsheeEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicLegs";
				i.Name = "Pants of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 2949;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Constitution: 15 pts
				 *   Thrust Resist: 7%
				 *   Power Pool: 6%
				 *   Intelligence attribute cap: 5
				 *   Constitution attribute cap: 5
				 *   Power bonus cap: 6
				 */

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 7;
				i.Bonus2Type = (int)eResist.Thrust;

				i.Bonus3 = 6;
				i.Bonus3Type = (int)eProperty.PowerPool;

				i.Bonus4 = 5;
				i.Bonus4Type = (int)eProperty.IntCapBonus;

				i.Bonus5 = 5;
				i.Bonus5Type = (int)eProperty.ConCapBonus;

				i.Bonus6 = 6;
				i.Bonus6Type = (int)eProperty.PowerPoolCapBonus;
				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicLegs = i;

			}
			//Keening Spirit Sleeves
			BainsheeEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("BainsheeEpicArms");
			if (BainsheeEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "BainsheeEpicArms";
				i.Name = "Sleeves of the Keening Spirit";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 2948;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				/*
				 *   Dexterity: 15 pts
				 *   Slash Resist: 7%
				 *   ALL Magic Skills: +3
				 *   Power Pool: 6%
				 *   Dexterity attribute cap: 5
				 *   Power bonus cap: 6
				 */

				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 7;
				i.Bonus2Type = (int)eResist.Slash;

				i.Bonus3 = 3;
				i.Bonus3Type = (int)eProperty.AllMagicSkills;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eProperty.PowerPool;

				i.Bonus5 = 7;
				i.Bonus5Type = (int)eProperty.DexCapBonus;

				i.Bonus6 = 6;
				i.Bonus6Type = (int)eProperty.PowerPoolCapBonus;
				{
					GameServer.Database.AddObject(i);
				}
				BainsheeEpicArms = i;

			}
			#endregion

			//Blademaster Epic Sleeves End
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.AddHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));

			/* Now we bring to Revelin the possibility to give this quest to players */
			Revelin.AddQuestToGive(typeof(Harmony_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Revelin == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Revelin, GameObjectEvent.Interact, new DOLEventHandler(TalkToRevelin));
			GameEventMgr.RemoveHandler(Revelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRevelin));

			/* Now we remove to Revelin the possibility to give this quest to players */
			Revelin.RemoveQuestToGive(typeof(Harmony_50));
		}

		protected static void TalkToRevelin(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Revelin.CanGiveQuest(typeof(Harmony_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Revelin.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Revelin.SayTo(player, "Hibernia needs your [services]");
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
						case "services":
							player.Out.SendQuestSubscribeCommand(Revelin, QuestMgr.GetIDForQuestType(typeof(Harmony_50)), "Will you help Revelin [Path of Harmony Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Blademaster &&
				player.CharacterClass.ID != (byte)eCharacterClass.Druid &&
				player.CharacterClass.ID != (byte)eCharacterClass.Valewalker &&
				player.CharacterClass.ID != (byte)eCharacterClass.Animist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mentalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Vampiir &&
				player.CharacterClass.ID != (byte)eCharacterClass.Bainshee)
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
			Harmony_50 quest = player.IsDoingQuest(typeof(Harmony_50)) as Harmony_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Harmony_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (Revelin.CanGiveQuest(typeof(Harmony_50), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Harmony_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Revelin.GiveQuest(typeof(Harmony_50), player, 1))
					return;
				player.Out.SendMessage("Kill Cailean in Cursed Forest loc 28k 24k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Horn Twin (Level 50 Path of Harmony Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailean in Cursed Forest Loc 28k,24k kill him!";
					case 2:
						return "[Step #2] Return to Revelin and give the Horn!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Harmony_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name == Cailean.Name)
				{
					m_questPlayer.Out.SendMessage("You collect the Horn from Cailean", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, Horn);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Revelin.Name && gArgs.Item.Id_nb == Horn.Id_nb)
				{
					Revelin.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Horn, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Revelin, m_questPlayer, Horn);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
				{
					case eCharacterClass.Blademaster:
						{
							GiveItem(m_questPlayer, BlademasterEpicArms);
							GiveItem(m_questPlayer, BlademasterEpicBoots);
							GiveItem(m_questPlayer, BlademasterEpicGloves);
							GiveItem(m_questPlayer, BlademasterEpicHelm);
							GiveItem(m_questPlayer, BlademasterEpicLegs);
							GiveItem(m_questPlayer, BlademasterEpicVest);
							break;
						}
					case eCharacterClass.Animist:
						{
							GiveItem(m_questPlayer, AnimistEpicArms);
							GiveItem(m_questPlayer, AnimistEpicBoots);
							GiveItem(m_questPlayer, AnimistEpicGloves);
							GiveItem(m_questPlayer, AnimistEpicHelm);
							GiveItem(m_questPlayer, AnimistEpicLegs);
							GiveItem(m_questPlayer, AnimistEpicVest);
							break;
						}
					case eCharacterClass.Mentalist:
						{
							GiveItem(m_questPlayer, MentalistEpicArms);
							GiveItem(m_questPlayer, MentalistEpicBoots);
							GiveItem(m_questPlayer, MentalistEpicGloves);
							GiveItem(m_questPlayer, MentalistEpicHelm);
							GiveItem(m_questPlayer, MentalistEpicLegs);
							GiveItem(m_questPlayer, MentalistEpicVest);
							break;
						}
					case eCharacterClass.Druid:
						{
							GiveItem(m_questPlayer, DruidEpicArms);
							GiveItem(m_questPlayer, DruidEpicBoots);
							GiveItem(m_questPlayer, DruidEpicGloves);
							GiveItem(m_questPlayer, DruidEpicHelm);
							GiveItem(m_questPlayer, DruidEpicLegs);
							GiveItem(m_questPlayer, DruidEpicVest);
							break;
						}
					case eCharacterClass.Valewalker:
						{
							GiveItem(m_questPlayer, ValewalkerEpicArms);
							GiveItem(m_questPlayer, ValewalkerEpicBoots);
							GiveItem(m_questPlayer, ValewalkerEpicGloves);
							GiveItem(m_questPlayer, ValewalkerEpicHelm);
							GiveItem(m_questPlayer, ValewalkerEpicLegs);
							GiveItem(m_questPlayer, ValewalkerEpicVest);
							break;
						}
					case eCharacterClass.Vampiir:
						{
							GiveItem(m_questPlayer, VampiirEpicArms);
							GiveItem(m_questPlayer, VampiirEpicBoots);
							GiveItem(m_questPlayer, VampiirEpicGloves);
							GiveItem(m_questPlayer, VampiirEpicHelm);
							GiveItem(m_questPlayer, VampiirEpicLegs);
							GiveItem(m_questPlayer, VampiirEpicVest);
							break;
						}
					case eCharacterClass.Bainshee:
						{
							GiveItem(m_questPlayer, BainsheeEpicArms);
							GiveItem(m_questPlayer, BainsheeEpicBoots);
							GiveItem(m_questPlayer, BainsheeEpicGloves);
							GiveItem(m_questPlayer, BainsheeEpicHelm);
							GiveItem(m_questPlayer, BainsheeEpicLegs);
							GiveItem(m_questPlayer, BainsheeEpicVest);
							break;
						}
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
        *#25 talk to Revelin
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Revelin 
        *#28 give her the ball of flame
        *#29 talk with Revelin about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Sidhe Scale Boots 
            *Sidhe Scale Coif
            *Sidhe Scale Gloves
            *Sidhe Scale Hauberk
            *Sidhe Scale Legs
            *Sidhe Scale Sleeves
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
