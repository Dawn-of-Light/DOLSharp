/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Cabalist, Reaver, Mercenary, Necromancer and Infiltrator (Guild of Shadows)
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

namespace DOL.GS.Quests.Albion
{
	public class Shadows_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Feast of the Decadent";
		protected const int minimumLevel = 50;
		protected const int maximumLevel = 50;

		private static GameNPC Lidmann = null; // Start NPC
		private static GameNPC Uragaig = null; // Mob to kill

		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate MercenaryEpicBoots = null; // of the Shadowy Embers  Boots 
		private static ItemTemplate MercenaryEpicHelm = null; // of the Shadowy Embers  Coif 
		private static ItemTemplate MercenaryEpicGloves = null; // of the Shadowy Embers  Gloves 
		private static ItemTemplate MercenaryEpicVest = null; // of the Shadowy Embers  Hauberk 
		private static ItemTemplate MercenaryEpicLegs = null; // of the Shadowy Embers  Legs 
		private static ItemTemplate MercenaryEpicArms = null; // of the Shadowy Embers  Sleeves 
		private static ItemTemplate ReaverEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ReaverEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ReaverEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ReaverEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ReaverEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ReaverEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate CabalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate CabalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate CabalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate CabalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate CabalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate CabalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate InfiltratorEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate InfiltratorEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate InfiltratorEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate InfiltratorEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate InfiltratorEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate InfiltratorEpicArms = null; //Subterranean Sleeves		
		private static ItemTemplate NecromancerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NecromancerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NecromancerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NecromancerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NecromancerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NecromancerEpicArms = null; //Subterranean Sleeves

		// Constructors
		public Shadows_50()
			: base()
		{
		}
		public Shadows_50(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		public Shadows_50(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public Shadows_50(GamePlayer questingPlayer, DBQuest dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lidmann Halsey, creating it ...");
				Lidmann = new GameMob();
				Lidmann.Model = 64;
				Lidmann.Name = "Lidmann Halsey";
				Lidmann.GuildName = "";
				Lidmann.Realm = (byte)eRealm.Albion;
				Lidmann.CurrentRegionID = 1;
				Lidmann.Size = 50;
				Lidmann.Level = 50;
				Lidmann.X = 466464;
				Lidmann.Y = 634554;
				Lidmann.Z = 1954;
				Lidmann.Heading = 1809;
				Lidmann.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lidmann.SaveIntoDatabase();
				}

			}
			else
				Lidmann = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailleach Uragaig", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Uragaig , creating it ...");
				Uragaig = new GameMob();
				Uragaig.Model = 349;
				Uragaig.Name = "Cailleach Uragaig";
				Uragaig.GuildName = "";
				Uragaig.Realm = (byte)eRealm.None;
				Uragaig.CurrentRegionID = 1;
				Uragaig.Size = 55;
				Uragaig.Level = 70;
				Uragaig.X = 316218;
				Uragaig.Y = 664484;
				Uragaig.Z = 2736;
				Uragaig.Heading = 3072;
				Uragaig.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Uragaig.SaveIntoDatabase();
				}

			}
			else
				Uragaig = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			sealed_pouch = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "sealed_pouch");
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
				sealed_pouch.MaxQuality = 100;
				sealed_pouch.Weight = 12;
				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddNewObject(sealed_pouch);
				}

			}
			// end item
			ItemTemplate i = null;
			MercenaryEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicBoots");
			if (MercenaryEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicBoots";
				i.Name = "Boots of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 722;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 16;
				i.Bonus2Type = (int)eStat.QUI;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 9;
				i.Bonus4Type = (int)eStat.STR;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicBoots = i;

			}
			//end item
			// of the Shadowy Embers  Coif
			MercenaryEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicHelm");
			if (MercenaryEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicHelm";
				i.Name = "Coif of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 16;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 18;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Body;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicHelm = i;

			}
			//end item
			// of the Shadowy Embers  Gloves
			MercenaryEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicGloves");
			if (MercenaryEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicGloves";
				i.Name = "Gauntlets of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 721;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 19;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.CON;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Crush;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicGloves = i;

			}
			// of the Shadowy Embers  Hauberk
			MercenaryEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicVest");
			if (MercenaryEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicVest";
				i.Name = "Haurberk of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 718;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 48;
				i.Bonus2Type = (int)eProperty.MaxHealth;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicVest = i;

			}
			// of the Shadowy Embers  Legs
			MercenaryEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicLegs");
			if (MercenaryEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicLegs";
				i.Name = "Chausses of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 719;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
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
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Slash;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicLegs = i;

			}
			// of the Shadowy Embers  Sleeves
			MercenaryEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "MercenaryEpicArms");
			if (MercenaryEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "MercenaryEpicArms";
				i.Name = "Sleeves of the Shadowy Embers";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 720;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 15;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 16;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 12;
				i.Bonus4Type = (int)eStat.QUI;
				{
					GameServer.Database.AddNewObject(i);
				}

				MercenaryEpicArms = i;

			}
			//Reaver Epic Sleeves End
			ReaverEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicBoots");
			if (ReaverEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicBoots";
				i.Name = "Boots of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 1270;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 14;
				i.Bonus1Type = (int)eProperty.MaxMana;

				i.Bonus2 = 9;
				i.Bonus2Type = (int)eStat.STR;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				//                    i.Bonus4 = 10;
				//                    i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicBoots = i;

			}
			//end item
			//of Murky Secrets Coif
			ReaverEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicHelm");
			if (ReaverEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicHelm";
				i.Name = "Coif of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 16;
				i.Bonus1Type = (int)eStat.PIE;

				i.Bonus2 = 6;
				i.Bonus2Type = (int)eProperty.Skill_Flexible_Weapon;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Body;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicHelm = i;

			}
			//end item
			//of Murky Secrets Gloves
			ReaverEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicGloves");
			if (ReaverEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicGloves";
				i.Name = "Gauntlets of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 1271;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 19;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.CON;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Matter;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Crush;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicGloves = i;

			}
			//of Murky Secrets Hauberk
			ReaverEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicVest");
			if (ReaverEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicVest";
				i.Name = "Hauberk of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 1267;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 48;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 15;
				i.Bonus2Type = (int)eStat.PIE;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 6;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicVest = i;

			}
			//of Murky Secrets Legs
			ReaverEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicLegs");
			if (ReaverEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicLegs";
				i.Name = "Chausses of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 1268;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
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
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Slash;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicLegs = i;

			}
			//of Murky Secrets Sleeves
			ReaverEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ReaverEpicArms");
			if (ReaverEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "ReaverEpicArms";
				i.Name = "Sleeves of Murky Secrets";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 1269;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 100;
				i.SPD_ABS = 27;
				i.Object_Type = 35;
				i.Quality = 100;
				i.MaxQuality = 100;
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

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 4;
				i.Bonus4Type = (int)eProperty.Skill_Slashing;
				{
					GameServer.Database.AddNewObject(i);
				}

				ReaverEpicArms = i;

			}
			InfiltratorEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicBoots");
			if (InfiltratorEpicBoots == null)
			{
				InfiltratorEpicBoots = new ItemTemplate();
				InfiltratorEpicBoots.Id_nb = "InfiltratorEpicBoots";
				InfiltratorEpicBoots.Name = "Shadow-Woven Boots";
				InfiltratorEpicBoots.Level = 50;
				InfiltratorEpicBoots.Item_Type = 23;
				InfiltratorEpicBoots.Model = 796;
				InfiltratorEpicBoots.IsDropable = true;
				InfiltratorEpicBoots.IsPickable = true;
				InfiltratorEpicBoots.DPS_AF = 100;
				InfiltratorEpicBoots.SPD_ABS = 10;
				InfiltratorEpicBoots.Object_Type = 33;
				InfiltratorEpicBoots.Quality = 100;
				InfiltratorEpicBoots.MaxQuality = 100;
				InfiltratorEpicBoots.Weight = 22;
				InfiltratorEpicBoots.Bonus = 35;
				InfiltratorEpicBoots.MaxCondition = 50000;
				InfiltratorEpicBoots.MaxDurability = 50000;
				InfiltratorEpicBoots.Condition = 50000;
				InfiltratorEpicBoots.Durability = 50000;

				InfiltratorEpicBoots.Bonus1 = 13;
				InfiltratorEpicBoots.Bonus1Type = (int)eStat.QUI;

				InfiltratorEpicBoots.Bonus2 = 13;
				InfiltratorEpicBoots.Bonus2Type = (int)eStat.DEX;

				InfiltratorEpicBoots.Bonus3 = 8;
				InfiltratorEpicBoots.Bonus3Type = (int)eResist.Cold;

				InfiltratorEpicBoots.Bonus4 = 13;
				InfiltratorEpicBoots.Bonus4Type = (int)eStat.CON;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicBoots);
				}

			}
			//end item
			//Shadow-Woven Coif
			InfiltratorEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicHelm");
			if (InfiltratorEpicHelm == null)
			{
				InfiltratorEpicHelm = new ItemTemplate();
				InfiltratorEpicHelm.Id_nb = "InfiltratorEpicHelm";
				InfiltratorEpicHelm.Name = "Shadow-Woven Coif";
				InfiltratorEpicHelm.Level = 50;
				InfiltratorEpicHelm.Item_Type = 21;
				InfiltratorEpicHelm.Model = 1290; //NEED TO WORK ON..
				InfiltratorEpicHelm.IsDropable = true;
				InfiltratorEpicHelm.IsPickable = true;
				InfiltratorEpicHelm.DPS_AF = 100;
				InfiltratorEpicHelm.SPD_ABS = 10;
				InfiltratorEpicHelm.Object_Type = 33;
				InfiltratorEpicHelm.Quality = 100;
				InfiltratorEpicHelm.MaxQuality = 100;
				InfiltratorEpicHelm.Weight = 22;
				InfiltratorEpicHelm.Bonus = 35;
				InfiltratorEpicHelm.MaxCondition = 50000;
				InfiltratorEpicHelm.MaxDurability = 50000;
				InfiltratorEpicHelm.Condition = 50000;
				InfiltratorEpicHelm.Durability = 50000;

				InfiltratorEpicHelm.Bonus1 = 13;
				InfiltratorEpicHelm.Bonus1Type = (int)eStat.DEX;

				InfiltratorEpicHelm.Bonus2 = 13;
				InfiltratorEpicHelm.Bonus2Type = (int)eStat.QUI;

				InfiltratorEpicHelm.Bonus3 = 8;
				InfiltratorEpicHelm.Bonus3Type = (int)eResist.Spirit;

				InfiltratorEpicHelm.Bonus4 = 13;
				InfiltratorEpicHelm.Bonus4Type = (int)eStat.STR;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicHelm);
				}

			}
			//end item
			//Shadow-Woven Gloves
			InfiltratorEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicGloves");
			if (InfiltratorEpicGloves == null)
			{
				InfiltratorEpicGloves = new ItemTemplate();
				InfiltratorEpicGloves.Id_nb = "InfiltratorEpicGloves";
				InfiltratorEpicGloves.Name = "Shadow-Woven Gloves";
				InfiltratorEpicGloves.Level = 50;
				InfiltratorEpicGloves.Item_Type = 22;
				InfiltratorEpicGloves.Model = 795;
				InfiltratorEpicGloves.IsDropable = true;
				InfiltratorEpicGloves.IsPickable = true;
				InfiltratorEpicGloves.DPS_AF = 100;
				InfiltratorEpicGloves.SPD_ABS = 10;
				InfiltratorEpicGloves.Object_Type = 33;
				InfiltratorEpicGloves.Quality = 100;
				InfiltratorEpicGloves.MaxQuality = 100;
				InfiltratorEpicGloves.Weight = 22;
				InfiltratorEpicGloves.Bonus = 35;
				InfiltratorEpicGloves.MaxCondition = 50000;
				InfiltratorEpicGloves.MaxDurability = 50000;
				InfiltratorEpicGloves.Condition = 50000;
				InfiltratorEpicGloves.Durability = 50000;


				InfiltratorEpicGloves.Bonus1 = 18;
				InfiltratorEpicGloves.Bonus1Type = (int)eStat.STR;

				InfiltratorEpicGloves.Bonus2 = 21;
				InfiltratorEpicGloves.Bonus2Type = (int)eProperty.MaxHealth;

				InfiltratorEpicGloves.Bonus3 = 3;
				InfiltratorEpicGloves.Bonus3Type = (int)eProperty.Skill_Envenom;

				InfiltratorEpicGloves.Bonus4 = 3;
				InfiltratorEpicGloves.Bonus4Type = (int)eProperty.Skill_Critical_Strike;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicGloves);
				}

			}
			//Shadow-Woven Hauberk
			InfiltratorEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicVest");
			if (InfiltratorEpicVest == null)
			{
				InfiltratorEpicVest = new ItemTemplate();
				InfiltratorEpicVest.Id_nb = "InfiltratorEpicVest";
				InfiltratorEpicVest.Name = "Shadow-Woven Jerkin";
				InfiltratorEpicVest.Level = 50;
				InfiltratorEpicVest.Item_Type = 25;
				InfiltratorEpicVest.Model = 792;
				InfiltratorEpicVest.IsDropable = true;
				InfiltratorEpicVest.IsPickable = true;
				InfiltratorEpicVest.DPS_AF = 100;
				InfiltratorEpicVest.SPD_ABS = 10;
				InfiltratorEpicVest.Object_Type = 33;
				InfiltratorEpicVest.Quality = 100;
				InfiltratorEpicVest.MaxQuality = 100;
				InfiltratorEpicVest.Weight = 22;
				InfiltratorEpicVest.Bonus = 35;
				InfiltratorEpicVest.MaxCondition = 50000;
				InfiltratorEpicVest.MaxDurability = 50000;
				InfiltratorEpicVest.Condition = 50000;
				InfiltratorEpicVest.Durability = 50000;

				InfiltratorEpicVest.Bonus1 = 36;
				InfiltratorEpicVest.Bonus1Type = (int)eProperty.MaxHealth;

				InfiltratorEpicVest.Bonus2 = 16;
				InfiltratorEpicVest.Bonus2Type = (int)eStat.DEX;

				InfiltratorEpicVest.Bonus3 = 8;
				InfiltratorEpicVest.Bonus3Type = (int)eResist.Cold;

				InfiltratorEpicVest.Bonus4 = 8;
				InfiltratorEpicVest.Bonus4Type = (int)eResist.Body;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicVest);
				}

			}
			//Shadow-Woven Legs
			InfiltratorEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicLegs");
			if (InfiltratorEpicLegs == null)
			{
				InfiltratorEpicLegs = new ItemTemplate();
				InfiltratorEpicLegs.Id_nb = "InfiltratorEpicLegs";
				InfiltratorEpicLegs.Name = "Shadow-Woven Leggings";
				InfiltratorEpicLegs.Level = 50;
				InfiltratorEpicLegs.Item_Type = 27;
				InfiltratorEpicLegs.Model = 793;
				InfiltratorEpicLegs.IsDropable = true;
				InfiltratorEpicLegs.IsPickable = true;
				InfiltratorEpicLegs.DPS_AF = 100;
				InfiltratorEpicLegs.SPD_ABS = 10;
				InfiltratorEpicLegs.Object_Type = 33;
				InfiltratorEpicLegs.Quality = 100;
				InfiltratorEpicLegs.MaxQuality = 100;
				InfiltratorEpicLegs.Weight = 22;
				InfiltratorEpicLegs.Bonus = 35;
				InfiltratorEpicLegs.MaxCondition = 50000;
				InfiltratorEpicLegs.MaxDurability = 50000;
				InfiltratorEpicLegs.Condition = 50000;
				InfiltratorEpicLegs.Durability = 50000;

				InfiltratorEpicLegs.Bonus1 = 21;
				InfiltratorEpicLegs.Bonus1Type = (int)eStat.CON;

				InfiltratorEpicLegs.Bonus2 = 16;
				InfiltratorEpicLegs.Bonus2Type = (int)eStat.QUI;

				InfiltratorEpicLegs.Bonus3 = 6;
				InfiltratorEpicLegs.Bonus3Type = (int)eResist.Heat;

				InfiltratorEpicLegs.Bonus4 = 6;
				InfiltratorEpicLegs.Bonus4Type = (int)eResist.Crush;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicLegs);
				}

			}
			//Shadow-Woven Sleeves
			InfiltratorEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "InfiltratorEpicArms");
			if (InfiltratorEpicArms == null)
			{
				InfiltratorEpicArms = new ItemTemplate();
				InfiltratorEpicArms.Id_nb = "InfiltratorEpicArms";
				InfiltratorEpicArms.Name = "Shadow-Woven Sleeves";
				InfiltratorEpicArms.Level = 50;
				InfiltratorEpicArms.Item_Type = 28;
				InfiltratorEpicArms.Model = 794;
				InfiltratorEpicArms.IsDropable = true;
				InfiltratorEpicArms.IsPickable = true;
				InfiltratorEpicArms.DPS_AF = 100;
				InfiltratorEpicArms.SPD_ABS = 10;
				InfiltratorEpicArms.Object_Type = 33;
				InfiltratorEpicArms.Quality = 100;
				InfiltratorEpicArms.MaxQuality = 100;
				InfiltratorEpicArms.Weight = 22;
				InfiltratorEpicArms.Bonus = 35;
				InfiltratorEpicArms.MaxCondition = 50000;
				InfiltratorEpicArms.MaxDurability = 50000;
				InfiltratorEpicArms.Condition = 50000;
				InfiltratorEpicArms.Durability = 50000;

				InfiltratorEpicArms.Bonus1 = 21;
				InfiltratorEpicArms.Bonus1Type = (int)eStat.DEX;

				InfiltratorEpicArms.Bonus2 = 18;
				InfiltratorEpicArms.Bonus2Type = (int)eStat.STR;

				InfiltratorEpicArms.Bonus3 = 6;
				InfiltratorEpicArms.Bonus3Type = (int)eResist.Matter;

				InfiltratorEpicArms.Bonus4 = 4;
				InfiltratorEpicArms.Bonus4Type = (int)eResist.Slash;
				{
					GameServer.Database.AddNewObject(InfiltratorEpicArms);
				}

			}
			CabalistEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicBoots");
			if (CabalistEpicBoots == null)
			{
				CabalistEpicBoots = new ItemTemplate();
				CabalistEpicBoots.Id_nb = "CabalistEpicBoots";
				CabalistEpicBoots.Name = "Warm Boots of the Construct";
				CabalistEpicBoots.Level = 50;
				CabalistEpicBoots.Item_Type = 23;
				CabalistEpicBoots.Model = 143;
				CabalistEpicBoots.IsDropable = true;
				CabalistEpicBoots.IsPickable = true;
				CabalistEpicBoots.DPS_AF = 50;
				CabalistEpicBoots.SPD_ABS = 0;
				CabalistEpicBoots.Object_Type = 32;
				CabalistEpicBoots.Quality = 100;
				CabalistEpicBoots.MaxQuality = 100;
				CabalistEpicBoots.Weight = 22;
				CabalistEpicBoots.Bonus = 35;
				CabalistEpicBoots.MaxCondition = 50000;
				CabalistEpicBoots.MaxDurability = 50000;
				CabalistEpicBoots.Condition = 50000;
				CabalistEpicBoots.Durability = 50000;

				CabalistEpicBoots.Bonus1 = 22;
				CabalistEpicBoots.Bonus1Type = (int)eStat.DEX;

				CabalistEpicBoots.Bonus2 = 3;
				CabalistEpicBoots.Bonus2Type = (int)eProperty.Skill_Matter;

				CabalistEpicBoots.Bonus3 = 8;
				CabalistEpicBoots.Bonus3Type = (int)eResist.Slash;

				CabalistEpicBoots.Bonus4 = 8;
				CabalistEpicBoots.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(CabalistEpicBoots);
				}

			}
			//end item
			//Warm of the Construct Coif
			CabalistEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicHelm");
			if (CabalistEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "CabalistEpicHelm";
				i.Name = "Warm Coif of the Construct";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 21;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.DEX;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddNewObject(i);
				}
				CabalistEpicHelm = i;

			}
			//end item
			//Warm of the Construct Gloves
			CabalistEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicGloves");
			if (CabalistEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "CabalistEpicGloves";
				i.Name = "Warm Gloves of the Construct";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 142;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.DEX;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eStat.INT;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eProperty.MaxMana;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicGloves = i;

			}
			//Warm of the Construct Hauberk
			CabalistEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicVest");
			if (CabalistEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "CabalistEpicVest";
				i.Name = "Warm Robe of the Construct";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 682;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;

				i.Bonus1 = 24;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 14;
				i.Bonus2Type = (int)eProperty.MaxMana;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eResist.Crush;

				//                    i.Bonus4 = 10;
				//                    i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicVest = i;

			}
			//Warm of the Construct Legs
			CabalistEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicLegs");
			if (CabalistEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "CabalistEpicLegs";
				i.Name = "Warm Leggings of the Construct";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 140;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 22;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Spirit;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddNewObject(i);
				}

				CabalistEpicLegs = i;

			}
			//Warm of the Construct Sleeves
			CabalistEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "CabalistEpicArms");
			if (CabalistEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "CabalistEpicArms";
				i.Name = "Warm Sleeves of the Construct";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 141;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Body;

				i.Bonus3 = 16;
				i.Bonus3Type = (int)eStat.DEX;

				//                    i.Bonus4 = 10;
				//                    i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}
				CabalistEpicArms = i;

			}


			NecromancerEpicBoots = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicBoots");
			if (NecromancerEpicBoots == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicBoots";
				i.Name = "Boots of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 23;
				i.Model = 143;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 22;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Pain_working;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Slash;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Thrust;
				{
					GameServer.Database.AddNewObject(i);
				}

				NecromancerEpicBoots = i;
			}
			//end item
			//of Forbidden Rites Coif
			NecromancerEpicHelm = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicHelm");
			if (NecromancerEpicHelm == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicHelm";
				i.Name = "Cap of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 21;
				i.Model = 1290; //NEED TO WORK ON..
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 21;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 13;
				i.Bonus2Type = (int)eStat.QUI;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Heat;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicHelm = i;

			}
			//end item
			//of Forbidden Rites Gloves
			NecromancerEpicGloves = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicGloves");
			if (NecromancerEpicGloves == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicGloves";
				i.Name = "Gloves of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 22;
				i.Model = 142;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 10;
				i.Bonus1Type = (int)eStat.STR;

				i.Bonus2 = 10;
				i.Bonus2Type = (int)eStat.INT;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eProperty.MaxMana;

				i.Bonus4 = 10;
				i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicGloves = i;

			}
			//of Forbidden Rites Hauberk
			NecromancerEpicVest = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicVest");
			if (NecromancerEpicVest == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicVest";
				i.Name = "Robe of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 25;
				i.Model = 1266;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 24;
				i.Bonus1Type = (int)eProperty.MaxHealth;

				i.Bonus2 = 14;
				i.Bonus2Type = (int)eProperty.MaxMana;

				i.Bonus3 = 4;
				i.Bonus3Type = (int)eResist.Crush;

				//                    i.Bonus4 = 10;
				//                    i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}

				NecromancerEpicVest = i;

			}
			//of Forbidden Rites Legs
			NecromancerEpicLegs = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicLegs");
			if (NecromancerEpicLegs == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicLegs";
				i.Name = "Leggings of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 27;
				i.Model = 140;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 22;
				i.Bonus1Type = (int)eStat.CON;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_Death_Servant;

				i.Bonus3 = 8;
				i.Bonus3Type = (int)eResist.Cold;

				i.Bonus4 = 8;
				i.Bonus4Type = (int)eResist.Matter;
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicLegs = i;

			}
			//of Forbidden Rites Sleeves
			NecromancerEpicArms = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "NecromancerEpicArms");
			if (NecromancerEpicArms == null)
			{
				i = new ItemTemplate();
				i.Id_nb = "NecromancerEpicArms";
				i.Name = "Sleeves of Forbidden Rites";
				i.Level = 50;
				i.Item_Type = 28;
				i.Model = 141;
				i.IsDropable = true;
				i.IsPickable = true;
				i.DPS_AF = 50;
				i.SPD_ABS = 0;
				i.Object_Type = 32;
				i.Quality = 100;
				i.MaxQuality = 100;
				i.Weight = 22;
				i.Bonus = 35;
				i.MaxCondition = 50000;
				i.MaxDurability = 50000;
				i.Condition = 50000;
				i.Durability = 50000;


				i.Bonus1 = 18;
				i.Bonus1Type = (int)eStat.INT;

				i.Bonus2 = 4;
				i.Bonus2Type = (int)eProperty.Skill_DeathSight;

				i.Bonus3 = 16;
				i.Bonus3Type = (int)eStat.DEX;

				//                    i.Bonus4 = 10;
				//                    i.Bonus4Type = (int)eResist.Energy;
				{
					GameServer.Database.AddNewObject(i);
				}
				NecromancerEpicArms = i;
				//Item Descriptions End
			}

			//Reaver Epic Sleeves End
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we bring to Lidmann the possibility to give this quest to players */
			Lidmann.AddQuestToGive(typeof(Shadows_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			//if not loaded, don't worry
			if (Lidmann == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.RemoveHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we remove to Lidmann the possibility to give this quest to players */
			Lidmann.RemoveQuestToGive(typeof(Shadows_50));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Lidmann.CanGiveQuest(typeof(Shadows_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Lidmann.SayTo(player, "Check your Journal for instructions!");
					return;
				}
				else
				{
					// Check if player is qualifed for quest                
					Lidmann.SayTo(player, "Albion needs your [services]");
					return;
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
							player.Out.SendCustomDialog("Will you help Lidmann [Defenders of Albion Level 50 Epic]?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Reaver &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mercenary &&
				player.CharacterClass.ID != (byte)eCharacterClass.Cabalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Necromancer &&
				player.CharacterClass.ID != (byte)eCharacterClass.Infiltrator)
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
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

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
			if (Lidmann.CanGiveQuest(typeof(Shadows_50), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Lidmann.GiveQuest(typeof(Shadows_50), player, 1))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Guild of Shadows Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailleach Uragaig in Lyonesse Loc 29k,33k kill her!";
					case 2:
						return "[Step #2] Return to Lidmann Halsey for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Shadows_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				if (gArgs.Target.Name == Uragaig.Name)
				{
					m_questPlayer.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(player, sealed_pouch);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Lidmann.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					RemoveItem(Lidmann, player, sealed_pouch);
					Lidmann.SayTo(player, "You have earned this Epic Armour!");
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

			if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Reaver)
			{
				GiveItem(m_questPlayer, ReaverEpicArms);
				GiveItem(m_questPlayer, ReaverEpicBoots);
				GiveItem(m_questPlayer, ReaverEpicGloves);
				GiveItem(m_questPlayer, ReaverEpicHelm);
				GiveItem(m_questPlayer, ReaverEpicLegs);
				GiveItem(m_questPlayer, ReaverEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mercenary)
			{
				GiveItem(m_questPlayer, MercenaryEpicArms);
				GiveItem(m_questPlayer, MercenaryEpicBoots);
				GiveItem(m_questPlayer, MercenaryEpicGloves);
				GiveItem(m_questPlayer, MercenaryEpicHelm);
				GiveItem(m_questPlayer, MercenaryEpicLegs);
				GiveItem(m_questPlayer, MercenaryEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Cabalist)
			{
				GiveItem(m_questPlayer, CabalistEpicArms);
				GiveItem(m_questPlayer, CabalistEpicBoots);
				GiveItem(m_questPlayer, CabalistEpicGloves);
				GiveItem(m_questPlayer, CabalistEpicHelm);
				GiveItem(m_questPlayer, CabalistEpicLegs);
				GiveItem(m_questPlayer, CabalistEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Infiltrator)
			{
				GiveItem(m_questPlayer, InfiltratorEpicArms);
				GiveItem(m_questPlayer, InfiltratorEpicBoots);
				GiveItem(m_questPlayer, InfiltratorEpicGloves);
				GiveItem(m_questPlayer, InfiltratorEpicHelm);
				GiveItem(m_questPlayer, InfiltratorEpicLegs);
				GiveItem(m_questPlayer, InfiltratorEpicVest);
			}
			else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Necromancer)
			{
				GiveItem(m_questPlayer, NecromancerEpicArms);
				GiveItem(m_questPlayer, NecromancerEpicBoots);
				GiveItem(m_questPlayer, NecromancerEpicGloves);
				GiveItem(m_questPlayer, NecromancerEpicHelm);
				GiveItem(m_questPlayer, NecromancerEpicLegs);
				GiveItem(m_questPlayer, NecromancerEpicVest);
			}

			m_questPlayer.GainExperience(1937768448, 0, 0, true);
			//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Lidmann
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Lidmann 
        *#28 give her the ball of flame
        *#29 talk with Lidmann about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            * of the Shadowy Embers  Boots 
            * of the Shadowy Embers  Coif
            * of the Shadowy Embers  Gloves
            * of the Shadowy Embers  Hauberk
            * of the Shadowy Embers  Legs
            * of the Shadowy Embers  Sleeves
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
