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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS.Utils;

namespace DOL.GS.Commands
{
	[Cmd("&mob", //command to handle
	     ePrivLevel.GM, //minimum privelege level
	     "Mob creation and modification commands.", //command description
	     // usage
	     "'/mob create [ClassName(DOL.GS.GameNPC)] [eRealm(0)]' to create a new mob.",
	     "'/mob fastcreate <ModelID> <level> [save(default = 0; use 1 to save)] <name>' to create mob with specified info.",
	     "'/mob nfastcreate <ModelID> <level> <number> [radius(10)] [name]' to create multiple mobs within radius.",
	     "'/mob nrandcreate <number> [radius(50)]' to create multiple random mobs within radius.",
	     "'/mob model <ModelID> [OID]' to set the mob's model, optionally using OID if mob isn't targeted.",
	     "'/mob modelinc <#> Increments the mob model by 1.",
	     "'/mob modeldec <#> Decrements the mob model by 1.",
	     "'/mob size <size>' to set the mob's size (1..255).",
	     "'/mob translationid <translation id>' to set the mob's translation id.",
	     "'/mob name <name>' to set the mob's name.",
	     "'/mob suffix <suffix>' to set the mob's suffix.",
	     "'/mob guild <guild name>' to set the mob's guild name (blank to remove).",
	     "'/mob examinearticle <examine article>' to set the mob's examine article.",
	     "'/mob messagearticle <message article>' to set the mob's message article.",
	     "'/mob peace' toggle whether the mob can be attacked.",
	     "'/mob aggro <level>' to set mob's aggro level (0..100)%.",
	     "'/mob range <distance>' to set mob's aggro range.",
	     "'/mob distance <maxdistance>' set mob max distance from its spawn (>0=real, 0=no check, <0=percent of aggro range).",
	     "'/mob roaming <distance>' set mob random range radius (0=noroaming, -1=standard, >0=individual).",
	     "'/mob damagetype <eDamageType>' set mob damage type.",
	     "'/mob movehere' move mob to player's location.",
	     "'/mob location' say location information in the chat window.",
	     "'/mob remove [true]' to remove this mob from the DB; specify true to also remove loot templates (if no other mobs of same name exist).",
	     "'/mob ghost' makes this mob ghost-like.",
	     "'/mob stealth' makes the mob stealthed (invisible).",
	     "'/mob torch' turns this mobs torch on and off.",
	     "'/mob statue' toggles statue effect.",
	     "'/mob fly [height]' makes this mob able to fly by changing the Z coordinate; moves mob up by height.",
	     "'/mob swimming' toggles mob's swimming flag (helpful for flying mobs).",
	     "'/mob noname' still possible to target this mob, but removes the name from above mob.",
	     "'/mob notarget' makes it impossible to target this mob and removes the name from above it.",
	     "'/mob kill' kills the mob without removing it from the DB.",
	     "'/mob heal' restores the mob's health to maximum.",
	     "'/mob attack <PlayerName>' command mob to attack a player.",
	     "'/mob state' show mob state (attackers, effects).",
	     "'/mob info' extended information about the mob.",
	     "'/mob realm <eRealm>' set the mob's realm.",
	     "'/mob speed <speed>' set the mob's max speed.",
	     "'/mob level <level>' set the mob's level.",
	     "'/mob levela <level>' set the mob's level and auto adjust stats.",
	     "'/mob brain <ClassName>' set the mob's brain.",
	     "'/mob respawn <duration>' set the mob's respawn time (in ms).",
	     "'/mob questinfo' show mob's quest info.",
	     "'/mob refreshquests' Update this mobs list of data quests.",
	     "'/mob equipinfo' show mob's inventory info.",
	     "'/mob equiptemplate load <EquipmentTemplateID>' to load the inventory template from the database, it is open for modification after.",
	     "'/mob equiptemplate create' to create an empty inventory template.",
	     "'/mob equiptemplate add <slot> <model> [color] [effect] [extension]' to add an item to the inventory template.",
	     "'/mob equiptemplate remove <slot>' to remove item from the specified slot in the inventory template.",
	     "'/mob equiptemplate clear' to remove the inventory template from mob.",
	     "'/mob equiptemplate save <EquipmentTemplateID> [replace]' to save the inventory template with a new name.",
	     "'/mob equiptemplate close' to finish the inventory template you are creating.",
	     "'/mob visibleslot <slot>' to set the visible weapon slot.  use slot names (left, right, two, distance).",
	     "'/mob dropcount [number]' to set the max number of drops for mob (omit number to view current value).",
	     "'/mob dropcount2 [number]' same as '/mob dropcount' but for the 'MobDrop' generator.",
	     "'/mob addloot <ItemTemplateID> <chance> [count]' to add loot to the mob's unique drop table.  Optionally specify count of how many to drop if chance = 100%.",
	     "'/mob addloot2 <ItemTemplateID> <chance> [count]' to add loot to the mob's drop table. Optionally specify count of how many item to drop if chance < 100%.",
	     "'/mob addotd <ItemTemplateID> <min level>' add a one time drop to this mob..",
	     "'/mob viewloot [random] [inv]' to view the selected mob's loot table.  Use random to simulate a kill drop, random inv to simulate and generate the loots.",
	     "'/mob removeloot <ItemTemplateID>' to remove loot from the mob's unique drop table.",
	     "'/mob removeloot2 <ItemTemplateID>' same as '/mob removeloot' but for the 'MobDrop' generator.",
	     "'/mob removeotd <ItemTemplateID>' to remove a one time drop from the mob's unique drop table.",
	     "'/mob refreshloot' to refresh all loot generators for this mob.",
	     "'/mob copy [name]' copies a mob exactly and places it at your location.",
	     "'/mob npctemplate <NPCTemplateID>' creates a mob with npc template, or modifies target.",
	     "'/mob npctemplate create <NPCTemplateID>' creates a new template from selected mob.",
	     "'/mob class <ClassName>' replaces mob with a clone of the specified class.",
	     "'/mob path <PathID>' associate the mob to the specified path.",
	     "'/mob house <HouseNumber>' set NPC's house number.",
	     "'/mob <stat> <amount>' Set the mob's stats (str, con, dex, qui, int, emp, pie, cha).",
	     "'/mob tether <tether range>' set mob tether range (>0: check, <=0: no check).",
	     "'/mob hood' toggle cloak hood visibility.",
	     "'/mob cloak' toggle cloak visibility.",
	     "'/mob race [ID]' view or set NPC's race (ID can be # or name).",
	     "'/mob race reload' reload race resists from the database.",
	     "'/mob bodytype <ID>' changing the mob's bodytype.",
	     "'/mob gender <0 = neutral | 1 = male | 2 = female>' set gender for this mob.",
	     "'/mob packageid <string>' set the package ID for this mob.",
	     "'/mob select' select the mob within 100 radius (used for selection of non-targettable GameNPC).",
	     "'/mob load <Mob_ID>' load the Mob_ID from the DB and update the Mob cache.",
	     "'/mob reload <name>' reload the targetted or named mob(s) from the database.",
	     "'/mob findname <name> <#>' search for a mob with a name like <name> with maximum <#> (def. 10) matches.",
	     "'/mob trigger <type> <chance> <emote> <text>' adds a trigger to targeted mob class.  Use '/mob trigger help' for more info.",
	     "'/mob trigger info' Give trigger informations.",
	     "'/mob trigger remove <id>' Remove a trigger.",
	     "'/mob ownerid <id>' Sets and saves the OwnerID for this mob."
	    )]
	public class MobCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const ushort AUTOSELECT_RADIUS = 100; // /mob select command

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			try
			{
				GameNPC targetMob = null;
				if (client.Player.TargetObject != null && client.Player.TargetObject is GameNPC)
					targetMob = (GameNPC)client.Player.TargetObject;

				if (args[1] != "create"
				    && args[1] != "fastcreate"
				    && args[1] != "nrandcreate"
				    && args[1] != "nfastcreate"
				    && args[1] != "npctemplate"
				    && args[1] != "model"
				    && args[1] != "modelinc"
				    && args[1] != "modeldec"
				    && args[1] != "copy"
				    && args[1] != "select"
				    && args[1] != "reload"
				    && args[1] != "findname"
				    && targetMob == null)
				{
					// it is not a mob
					if (client.Player.TargetObject != null)
					{
						client.Out.SendMessage("Cannot use " + client.Player.TargetObject + " for /mob command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					// nothing selected - trying to autoselect a mob within AUTOSELECT_RADIUS radius
					if (client.Player.TargetObject == null)
					{
						select(AUTOSELECT_RADIUS, client);
						return;
					}
				}

				switch (args[1])
				{
						case "create": create(client, args); break;
						case "fastcreate": fastcreate(client, args); break;
						case "nfastcreate": nfastcreate(client, args); break;
						case "nrandcreate": nrandcreate(client, args); break;
						case "model": model(client, targetMob, args); break;
						case "modelinc": modelinc(client, targetMob, args); break;
						case "modeldec": modeldec(client, targetMob, args); break;
						case "size": size(client, targetMob, args); break;
						//case "translationid": translationid(client, targetMob, args); break;
						case "name": name(client, targetMob, args); break;
						case "suffix": suffix(client, targetMob, args); break;
						case "guild": guild(client, targetMob, args); break;
						case "examinearticle": examinearticle(client, targetMob, args); break;
						case "messagearticle": messagearticle(client, targetMob, args); break;
						case "peace": peace(client, targetMob, args); break;
						case "aggro": aggro(client, targetMob, args); break;
						case "range": range(client, targetMob, args); break;
						case "distance": distance(client, targetMob, args); break;
						case "roaming": roaming(client, targetMob, args); break;
						case "damagetype": damagetype(client, targetMob, args); break;
						case "movehere": movehere(client, targetMob, args); break;
						case "location": location(client, targetMob, args); break;
						case "remove": remove(client, targetMob, args); break;
						case "ghost": ghost(client, targetMob, args); break;
						case "stealth": stealth(client, targetMob, args); break;
						case "torch": torch(client, targetMob, args); break;
						case "statue": statue(client, targetMob, args); break;
						case "fly": fly(client, targetMob, args); break;
						case "swimming": swimming(client, targetMob, args); break;
						case "noname": noname(client, targetMob, args); break;
						case "notarget": notarget(client, targetMob, args); break;
						case "kill": kill(client, targetMob, args); break;
						case "flags": flags(client, targetMob, args); break;
						case "heal": heal(client, targetMob, args); break;
						case "attack": attack(client, targetMob, args); break;
						case "info": info(client, targetMob, args); break;
						case "stats": stats(client, targetMob, args); break;
						case "state": state(client, targetMob); break;
						case "realm": realm(client, targetMob, args); break;
						case "speed": speed(client, targetMob, args); break;
						case "level": level(client, targetMob, args); break;
						case "levela": levela(client, targetMob, args); break;
						case "brain": brain(client, targetMob, args); break;
						case "respawn": respawn(client, targetMob, args); break;
						case "questinfo": questinfo(client, targetMob, args); break;
						case "refreshquests": refreshquests(client, targetMob, args); break;
						case "equipinfo": equipinfo(client, targetMob, args); break;
						case "equiptemplate": equiptemplate(client, targetMob, args); break;
						case "visibleslot": visibleslot(client, targetMob, args); break;
						case "dropcount": dropcount<MobXLootTemplate>(client, targetMob, args); break;
						case "dropcount2": dropcount<MobDropTemplate>(client, targetMob, args); break;
						case "addloot": addloot<MobXLootTemplate, LootTemplate>(client, targetMob, args); break;
						case "addloot2": addloot<MobDropTemplate, DropTemplateXItemTemplate>(client, targetMob, args); break;
						case "addotd": addotd(client, targetMob, args); break;
						case "viewloot": viewloot(client, targetMob, args); break;
						case "removeloot": removeloot<LootTemplate>(client, targetMob, args); break;
						case "removeloot2": removeloot<DropTemplateXItemTemplate>(client, targetMob, args); break;
						case "removeotd": removeotd(client, targetMob, args); break;
						case "refreshloot": refreshloot(client, targetMob, args); break;
						case "copy": copy(client, targetMob, args); break;
						case "npctemplate": npctemplate(client, targetMob, args); break;
						case "class": setClass(client, targetMob, args); break;
						case "path": path(client, targetMob, args); break;
						case "house": house(client, targetMob, args); break;
					case "str":
					case "con":
					case "dex":
					case "qui":
					case "int":
					case "emp":
					case "pie":
						case "cha": stat(client, targetMob, args); break;
						case "tether": tether(client, targetMob, args); break;
						case "hood": hood(client, targetMob, args); break;
						case "cloak": cloak(client, targetMob, args); break;
						case "bodytype": bodytype(client, targetMob, args); break;
						case "race": race(client, targetMob, args); break;
						case "gender": gender(client, targetMob, args); break;
						case "packageid": packageid(client, targetMob, args); break;
						case "ownerid": ownerid(client, targetMob, args); break;
						case "select": select(AUTOSELECT_RADIUS, client); break;
						case "load": load(client, args); break;
						case "reload": reload(client, targetMob, args); break;
						case "findname": findname(client, args); break;
						case "trigger": trigger(client, targetMob, args); break;
					default:
						DisplaySyntax(client);
						return;
				}
			}
			catch (Exception ex)
			{
				client.Out.SendMessage(ex.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				DisplaySyntax(client);
			}
		}


		private void create(GameClient client, string[] args)
		{
			string theType = "DOL.GS.GameNPC";
			byte realm = 0;

			if (args.Length > 2)
				theType = args[2];

			if (args.Length > 3)
			{
				if (!byte.TryParse(args[3], out realm))
				{
					DisplaySyntax(client, args[1]);
					return;
				}
			}

			//Create a new mob
			GameNPC mob = null;

			foreach (Assembly script in ScriptMgr.GameServerScripts)
			{
				try
				{
					client.Out.SendDebugMessage(script.FullName);
					mob = (GameNPC)script.CreateInstance(theType, false);

					if (mob != null)
						break;
				}
				catch (Exception e)
				{
					client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
			}

			if (mob == null)
			{
				client.Out.SendMessage("There was an error creating an instance of " + theType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			//Fill the object variables
			mob.X = client.Player.X;
			mob.Y = client.Player.Y;
			mob.Z = client.Player.Z;
			mob.CurrentRegion = client.Player.CurrentRegion;
			mob.Heading = client.Player.Heading;
			mob.Level = 1;
			mob.Realm = (eRealm)realm;
			mob.Name = "New Mob";
			mob.Model = 408;

			//Fill the living variables
			mob.CurrentSpeed = 0;
			mob.MaxSpeedBase = 200;
			mob.GuildName = "";
			mob.Size = 50;
			mob.Flags |= GameNPC.eFlags.PEACE;
			mob.AddToWorld();
			mob.LoadedFromScript = false; // allow saving
			mob.SaveIntoDatabase();
			client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("The mob has been created with the peace flag, so it can't be attacked, to remove type /mob peace", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
		}

		private void fastcreate(GameClient client, string[] args)
		{
			byte level = 1;
			ushort model = 408;
			string name = "New Mob";
			bool saveMob = false;

			if (args.Length < 5)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			try
			{
				if (args[4] == "0" || args[4] == "1")
				{
					if (args.Length < 6)
					{
						DisplaySyntax(client, args[1]);
						return;
					}

					saveMob = args[4].Equals("1");
					name = String.Join(" ", args, 5, args.Length - 5);
				}
				else
				{
					name = String.Join(" ", args, 4, args.Length - 4);
				}

				level = Convert.ToByte(args[3]);
				model = Convert.ToUInt16(args[2]);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			name = CheckName(name, client);
			GameNPC mob = new GameNPC();

			//Fill the object variables
			mob.X = client.Player.X;
			mob.Y = client.Player.Y;
			mob.Z = client.Player.Z;
			mob.CurrentRegion = client.Player.CurrentRegion;
			mob.Heading = client.Player.Heading;
			mob.Level = level;
			mob.Realm = 0;
			mob.Name = name;
			mob.Model = model;

			//Fill the living variables
			if (mob.Brain is IOldAggressiveBrain)
			{
				((IOldAggressiveBrain)mob.Brain).AggroLevel = 100;
				((IOldAggressiveBrain)mob.Brain).AggroRange = 500;
			}

			mob.CurrentSpeed = 0;
			mob.MaxSpeedBase = 200;
			mob.GuildName = "";
			mob.Size = 50;
			mob.AddToWorld();

			if (saveMob)
			{
				mob.LoadedFromScript = false;
				mob.SaveIntoDatabase();
			}

			client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void nfastcreate(GameClient client, string[] args)
		{
			byte level = 1;
			ushort model = 408;
			byte realm = 0;
			ushort number = 1;
			string name = "New Mob";
			ushort radius = 10;

			if (args.Length < 5)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			try
			{
				if (args.Length > 5)
				{
					if (ushort.TryParse(args[5], out radius))
					{
						if (args.Length > 6)
							name = String.Join(" ", args, 6, args.Length - 6);
					}
					else
					{
						radius = 10;
						name = String.Join(" ", args, 5, args.Length - 5);
					}
				}

				number = Convert.ToUInt16(args[4]);
				level = Convert.ToByte(args[3]);
				model = Convert.ToUInt16(args[2]);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			for (int i = 0; i < number; ++i)
			{
				GameNPC mob = new GameNPC();

				//Fill the object variables
				int x = client.Player.X + Util.Random(-radius, radius);
				int y = client.Player.Y + Util.Random(-radius, radius);
				mob.X = FastMath.Abs(x);
				mob.Y = FastMath.Abs(y);
				mob.Z = client.Player.Z;
				mob.CurrentRegion = client.Player.CurrentRegion;
				mob.Heading = client.Player.Heading;
				mob.Level = level;
				mob.Realm = (eRealm)realm;
				mob.Name = name;
				mob.Model = model;

				//Fill the living variables
				if (mob.Brain is IOldAggressiveBrain)
				{
					((IOldAggressiveBrain)mob.Brain).AggroLevel = 100;
					((IOldAggressiveBrain)mob.Brain).AggroRange = 500;
				}

				mob.CurrentSpeed = 0;
				mob.MaxSpeedBase = 200;
				mob.GuildName = "";
				mob.Size = 50;
				mob.AddToWorld();
				client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private void nrandcreate(GameClient client, string[] args)
		{
			ushort number = 1;
			ushort radius = 50;

			if (args.Length < 3)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			try
			{
				number = ushort.Parse(args[2]);

				if (args.Length > 3)
				{
					if (ushort.TryParse(args[3], out radius) == false)
						radius = 50;
				}
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[2]);
				return;
			}

			for (int i = 0; i < number; ++i)
			{
				GameNPC mob = new GameNPC();

				//Fill the object variables
				int x = client.Player.X + DOL.GS.Util.Random(-radius, radius);
				int y = client.Player.Y + DOL.GS.Util.Random(-radius, radius);
				mob.X = FastMath.Abs(x);
				mob.Y = FastMath.Abs(y);
				mob.Z = client.Player.Z;
				mob.CurrentRegion = client.Player.CurrentRegion;
				mob.Heading = client.Player.Heading;
				mob.Level = (byte)Util.Random(10, 50);
				mob.Realm = (eRealm)Util.Random(1, 3);
				mob.Name = "rand_" + i;
				mob.Model = (byte)Util.Random(568, 699);

				//Fill the living variables
				if (mob.Brain is IOldAggressiveBrain)
				{
					((IOldAggressiveBrain)mob.Brain).AggroLevel = 100;
					((IOldAggressiveBrain)mob.Brain).AggroRange = 500;
				}

				mob.CurrentSpeed = 0;
				mob.MaxSpeedBase = 200;
				mob.GuildName = "";
				mob.Size = 50;
				mob.AddToWorld();
			}

			client.Out.SendMessage("Created " + number + " mobs", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		
		#region auto increment/decrement model display
		// List of non-displayed models
		// used with modelinc and modeldec
		List<ushort> invmodel = new List<ushort>() { 3, 4, 604, 666, 1000, 1001, 1002, 1003, 1004, 1237, 1238, 1239, 1240, 1241, 1242, 1243, 1244, 1245, 1246,
			1247, 1248, 1249, 1250, 1251, 1252, 1253, 1254, 1275, 1391, 1392, 1393, 1441, 1442, 1443, 1444, 1445, 1446, 1447, 1448, 1449,
			1450, 1451, 1452, 1453, 1454, 1455, 1456, 1457, 1458, 1459, 1460, 1461, 1462, 1463, 1464, 1465, 1466, 1467, 1468, 1469, 1470,
			1471, 1472, 1473, 1474, 1475, 1476, 1477, 1478, 1479, 1480, 1481, 1482, 1483, 1484, 1485, 1486, 1487, 1488, 1489, 1490, 1491,
			1492, 1493, 1494, 1495, 1496, 1497, 1498, 1499, 1500, 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510, 1511, 1512,
			1513, 1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522, 1523, 1524, 1525, 1526, 1527, 1528, 1529, 1530, 1531, 1532, 1533,
			1534, 1535, 1536, 1537, 1538, 1539, 1540, 1541, 1542, 1543, 1544, 1545, 1546, 1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554,
			1555, 1556, 1557, 1558, 1559, 1560, 1561, 1562, 1567, 1568, 1569, 1570, 1571, 1572, 1573, 1587, 1588, 1589, 1590, 1591, 1592,
			1593, 1594, 1595, 1598, 1599, 1600, 1601, 1602, 1603, 1604, 1605, 1608, 1609, 1610, 1611, 1612, 1613, 1614, 1615, 1616, 1617,
			1618, 1619, 1620, 1621, 1622, 1624, 1626, 1627, 1628, 1629, 1630, 1631, 1632, 1633, 1634, 1635, 1636, 1637, 1638, 1639, 1640,
			1641, 1643, 1644, 1645, 1646, 1671, 1673, 1674, 1675, 1677, 1678, 1679, 1680, 1682, 1683, 1684, 1685, 1686, 1737, 1738, 1739,
			1761, 1762,	1763, 1764, 1765, 1766, 1767, 1768, 1769, 1822, 1871, 1872, 1873, 1874, 1875, 1876, 1877, 1880, 1881, 1882, 1886,
			1907, 1923,	1926, 1927, 1928, 1943, 2004, 2005, 2006, 2007, 2008, 2019, 2067, 2068, 2069, 2070, 2071, 2072, 2073, 2280, 2281,
			2282, 2283,	2284, 2285, 2286, 2287, 2288, 2289, 2290, 2291, 2292, 2293, 2294, 2299, 2300, 2302, 2305, 2319, 2333, 2334, 2335,
			2336, 2337,	2338, 2364 };
		
		private void modelinc(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob == null)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
			
			if (targetMob.Model >= 2363)
			{
				client.Out.SendMessage("Highest mob model reached!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			ushort model = targetMob.Model;
			model++;
			
			//Some models are not used, or cannot be targetted, i.e. ambient mobs and some invisible mobs, don't set mobs to these models
			while (invmodel.Contains(model))
				model++;
			
			try
			{
				targetMob.Model = model;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob model changed to: " + targetMob.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		private void modeldec(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob == null)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
			
			if (targetMob.Model == 1)
			{
				client.Out.SendMessage("Mob model cannot be 0!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			ushort model = targetMob.Model;
			model--;
			
			//Some models are not used, or cannot be targetted, i.e. ambient mobs and some invisible mobs, don't set mobs to these models
			while (invmodel.Contains(model))
				model--;
			
			try
			{
				targetMob.Model = model;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob model changed to: " + targetMob.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}
		#endregion
		
		private void model(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length == 4)
				targetMob = FindOID(client, targetMob, args);

			if (targetMob == null)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			ushort model;

			try
			{
				model = Convert.ToUInt16(args[2]);
				targetMob.Model = model;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob model changed to: " + targetMob.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		GameNPC FindOID(GameClient client, GameNPC targetMob, string[] args)
		{
			ushort mobOID;
			if (ushort.TryParse(args[3], out mobOID))
			{
				GameObject obj = client.Player.CurrentRegion.GetObject(mobOID);
				if (obj == null)
				{
					client.Out.SendMessage("No object with OID: " + args[1] + " in current Region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return null;
				}
				else
				{
					if (obj is GameNPC)
					{
						return (GameNPC)obj;
					}
					else
					{
						client.Out.SendMessage("Object " + mobOID + " is a " + obj.GetType().ToString() + ", not a GameNPC.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return null;
					}
				}
			}
			else
			{
				DisplaySyntax(client, args[1]);
				return null;
			}
		}

		private void size(GameClient client, GameNPC targetMob, string[] args)
		{
			ushort mobSize;

			try
			{
				mobSize = Convert.ToUInt16(args[2]);

				if (mobSize > 255)
				{
					mobSize = 255;
				}

				targetMob.Size = (byte)mobSize;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob size changed to: " + targetMob.Size, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		//private void translationid(GameClient client, GameNPC targetMob, string[] args)
		//{
		//    if (targetMob.GetType().IsSubclassOf(typeof(GameMovingObject)))
		//    {
		//        // Can be funny when we remove (e.g.) a ram or a boat (why are we dropped on the sea???) from the world. ;-)
		//        client.Out.SendMessage("The selected object is type of GameMovingObject and it's translation id cannot be changed " +
		//                               "via command. Please change the translation id in your code / database.",
		//                               eChatType.CT_System, eChatLoc.CL_SystemWindow);
		//        return;
		//    }

		//    string id = "";

		//    if (args.Length > 2)
		//        id = String.Join("", args, 2, args.Length - 2);

		//    if (id != "")
		//    {
		//        targetMob.TranslationId = id;
		//        targetMob.SaveIntoDatabase();

		//        targetMob.RemoveFromWorld();
		//        GameNPC.RefreshTranslation(null, id);
		//        targetMob.AddToWorld();

		//        client.Out.SendMessage("Mob translation id changed to: " + id, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		//    }
		//    else
		//        DisplaySyntax(client, args[1]);
		//}

		private void name(GameClient client, GameNPC targetMob, string[] args)
		{
			string mobName = "";

			if (args.Length > 2)
				mobName = String.Join(" ", args, 2, args.Length - 2);

			if (mobName != "")
			{
				targetMob.Name = CheckName(mobName, client);
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob name changed to: " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void suffix(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob.GetType().IsSubclassOf(typeof(GameMovingObject)))
			{
				client.Out.SendMessage("You cannot set a suffix for GameMovingObjects.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			string suf = "";

			if (args.Length > 2)
				suf = String.Join(" ", args, 2, args.Length - 2);

			if (suf != "")
			{
				targetMob.Suffix = suf;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob suffix changed to: " + suf, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
				DisplaySyntax(client, args[1]);
		}

		private void guild(GameClient client, GameNPC targetMob, string[] args)
		{
			string guildName = "";

			if (args.Length > 2)
				guildName = String.Join(" ", args, 2, args.Length - 2);

			if (guildName != "")
			{
				targetMob.GuildName = CheckGuildName(guildName, client);
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob guild changed to: " + targetMob.GuildName, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (targetMob.GuildName != "")
				{
					targetMob.GuildName = "";
					targetMob.SaveIntoDatabase();
					client.Out.SendMessage("Mob guild removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
					DisplaySyntax(client, args[1]);
			}
		}

		private void examinearticle(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob.GetType().IsSubclassOf(typeof(GameMovingObject)))
			{
				client.Out.SendMessage("Please change the default language examine article for GameMovingObjects in your code / database. If you want to set a " +
				                       "examine article for other languages, please use '/translate examinearticle <language> <examine article>'.",
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			string exa = "";

			if (args.Length > 2)
				exa = String.Join(" ", args, 2, args.Length - 2);

			if (exa != "")
			{
				targetMob.ExamineArticle = exa;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob examine article changed to: " + exa, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
				DisplaySyntax(client, args[1]);
		}

		private void messagearticle(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob.GetType().IsSubclassOf(typeof(GameMovingObject)))
			{
				client.Out.SendMessage("Please change the default language message article for GameMovingObjects in your code / database. If you want to set a " +
				                       "message article for other languages, please use '/translate messagearticle <language> <message article>'.",
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			string msg = "";

			if (args.Length > 2)
				msg = String.Join(" ", args, 2, args.Length - 2);

			if (msg != "")
			{
				targetMob.Suffix = msg;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob message article changed to: " + msg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
				DisplaySyntax(client, args[1]);
		}

		private void peace(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.PEACE;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob PEACE flag is set to " + ((targetMob.Flags & GameNPC.eFlags.PEACE) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void aggro(GameClient client, GameNPC targetMob, string[] args)
		{
			int aggroLevel;

			try
			{
				aggroLevel = int.Parse(args[2]);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			if (targetMob.Brain is IOldAggressiveBrain)
			{
				((IOldAggressiveBrain)targetMob.Brain).AggroLevel = aggroLevel;
				targetMob.SaveIntoDatabase();
				DisplayMessage(client, "Mob aggro changed to " + aggroLevel);
			}
			else
				DisplayMessage(client, "Selected mob does not have an aggressive brain.");
		}

		private void race(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length < 3)
			{
				DisplayMessage(client, targetMob.Name + "'s race is " + targetMob.Race + ".");
				return;
			}


			bool reloadResists = args[2].ToLower().Equals("reload");

			if (reloadResists)
			{
				SkillBase.InitializeRaceResists();
				DisplayMessage(client, "Race resists reloaded from database.");
				return;
			}


			short raceID = 0;

			if (!short.TryParse(args[2], out raceID))
			{
				string raceName = string.Join(" ", args, 2, args.Length - 2);

				Race npcRace = GameServer.Database.SelectObjects<Race>("`Name` = @Name", new QueryParameter("@Name", raceName)).FirstOrDefault();

				if (npcRace == null)
				{
					DisplayMessage(client, "No race found named:  " + raceName);
				}
				else
				{
					raceID = (short)npcRace.ID;
				}
			}

			if (raceID != 0)
			{
				targetMob.Race = raceID;
				targetMob.SaveIntoDatabase();
				DisplayMessage(client, targetMob.Name + "'s race set to " + raceID);
			}
		}

		private void range(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				IOldAggressiveBrain aggroBrain = targetMob.Brain as IOldAggressiveBrain;

				if (aggroBrain != null)
				{
					int range = int.Parse(args[2]);
					aggroBrain.AggroRange = range;
					targetMob.SaveIntoDatabase();
					DisplayMessage(client, "Mob aggro range changed to {0}", aggroBrain.AggroRange);
				}
				else
				{
					DisplayMessage(client, "Selected mob does not have an aggressive brain.");
				}
			}
			catch
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		private void distance(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				int distance = Convert.ToInt32(args[2]);
				targetMob.MaxDistance = distance;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob max distance changed to: " + targetMob.MaxDistance, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		private void roaming(GameClient client, GameNPC targetMob, string[] args)
		{
			int maxRoamingRange;

			try
			{
				maxRoamingRange = Convert.ToInt32(args[2]);
				targetMob.RoamingRange = maxRoamingRange;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob Roaming Range changed to: " + targetMob.RoamingRange, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		private void damagetype(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				eDamageType damage = (eDamageType)Enum.Parse(typeof(eDamageType), args[2], true);
				targetMob.MeleeDamageType = damage;
				targetMob.SaveIntoDatabase();
				DisplayMessage(client, "Mob damage type changed to: {0}", targetMob.MeleeDamageType);
			}
			catch
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void movehere(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Target Mob '" + targetMob.Name + "' moved to your location!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void location(GameClient client, GameNPC targetMob, string[] args)
		{
			client.Out.SendMessage("\"" + targetMob.Name + "\", " +
			                       targetMob.CurrentRegionID + ", " +
			                       targetMob.X + ", " +
			                       targetMob.Y + ", " +
			                       targetMob.Z + ", " +
			                       targetMob.Heading,
			                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void remove(GameClient client, GameNPC targetMob, string[] args)
		{
			string mobName = targetMob.Name;

			targetMob.StopAttack();
			targetMob.StopCurrentSpellcast();
			targetMob.DeleteFromDatabase();
			targetMob.Delete();

			if (args.Length > 2 && args[2] == "true")
			{
				var mobs = GameServer.Database.SelectObjects<Mob>("`Name` = @Name", new QueryParameter("@Name", mobName)).FirstOrDefault();

				if (mobs == null)
				{
					var deleteLoots = GameServer.Database.SelectObjects<MobXLootTemplate>("`MobName` = @MobName", new QueryParameter("@MobName", mobName));

					GameServer.Database.DeleteObject(deleteLoots);

					var deleteLootTempl = GameServer.Database.SelectObjects<LootTemplate>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", mobName));
					
					GameServer.Database.DeleteObject(deleteLootTempl);

					DisplayMessage(client, "Removed MobXLootTemplate and LootTemplate entries for " + mobName + " from DB.");
				}
			}

			client.Out.SendMessage("Target Mob removed from DB.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void flags(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length == 4)
				targetMob = FindOID(client, targetMob, args);

			if (targetMob == null)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			uint flag;
			uint.TryParse(args[2], out flag);

			targetMob.Flags = (GameNPC.eFlags)flag;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob flags are set to " + targetMob.Flags.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void ghost(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.GHOST;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob GHOST flag is set to " + ((targetMob.Flags & GameNPC.eFlags.GHOST) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void stealth(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.STEALTH;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob STEALTH flag is set to " + ((targetMob.Flags & GameNPC.eFlags.STEALTH) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void torch(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.TORCH;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob TORCH flag is set to " + ((targetMob.Flags & GameNPC.eFlags.TORCH) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void statue(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.STATUE;
			targetMob.SaveIntoDatabase();

			if ((targetMob.Flags & GameNPC.eFlags.STATUE) > 0)
				client.Out.SendMessage("You have set the STATUE flag - you will need to use \"/debug on\" to target this NPC.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

			client.Out.SendMessage(targetMob.Name + "'s STATUE flag is set to " + ((targetMob.Flags & GameNPC.eFlags.STATUE) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void fly(GameClient client, GameNPC targetMob, string[] args)
		{
			int height = 0;

			if (args.Length > 2)
			{

				if (!int.TryParse(args[2], out height))
				{
					DisplaySyntax(client, args[1]);
					return;
				}
			}

			targetMob.Flags ^= GameNPC.eFlags.FLYING;

			if ((targetMob.Flags & GameNPC.eFlags.FLYING) != 0)
				targetMob.MoveTo(targetMob.CurrentRegionID, targetMob.X, targetMob.Y, targetMob.Z + height, targetMob.Heading);

			targetMob.SaveIntoDatabase();

			client.Out.SendMessage(targetMob.Name + "'s FLYING flag is set to " + ((targetMob.Flags & GameNPC.eFlags.FLYING) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void swimming(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.SWIMMING;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob SWIMMING flag is set to " + ((targetMob.Flags & GameNPC.eFlags.SWIMMING) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void noname(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.DONTSHOWNAME;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob DONTSHOWNAME flag is set to " + ((targetMob.Flags & GameNPC.eFlags.DONTSHOWNAME) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void notarget(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.Flags ^= GameNPC.eFlags.CANTTARGET;
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob CANTTARGET flag is set to " + ((targetMob.Flags & GameNPC.eFlags.CANTTARGET) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void kill(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				targetMob.AddAttacker(client.Player);
				targetMob.AddXPGainer(client.Player, targetMob.Health);
				targetMob.Die(client.Player);
				targetMob.XPGainers.Clear();
				client.Out.SendMessage("Mob '" + targetMob.Name + "' killed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private void heal(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				targetMob.Health = targetMob.MaxHealth;
				targetMob.SaveIntoDatabase();
				client.Out.SendObjectUpdate(targetMob);
				client.Out.SendMessage("Mob '" + targetMob.Name + "' healed (" + targetMob.Health + "/" + targetMob.MaxHealth + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private void attack(GameClient client, GameNPC targetMob, string[] args)
		{

			foreach (GamePlayer player in targetMob.GetPlayersInRadius(3000))
			{
				if (player.Name == args[2])
				{
					targetMob.StartAttack(player);
					break;
				}
			}
		}

		private void info(GameClient client, GameNPC targetMob, string[] args)
		{
			var info = new List<string>();

			if (targetMob.LoadedFromScript)
				info.Add(" + Loaded: from Script");
			else
				info.Add(" + Loaded: from Database");

			info.Add(" + Class: " + targetMob.GetType().ToString());
			info.Add(" + Realm: " + GlobalConstants.RealmToName(targetMob.Realm));

			if (targetMob.Faction != null)
				info.Add($" + Faction: {targetMob.Faction.Name} [{targetMob.Faction.ID}]");

			info.Add(" + Level: " + targetMob.Level);
			info.Add(" + Brain: " + (targetMob.Brain == null ? "(null)" : targetMob.Brain.GetType().ToString()));

			if (targetMob.DamageRvRMemory > 0)
				info.Add("  - Damage RvR Memory: " + targetMob.DamageRvRMemory);

			if (targetMob.Brain != null && targetMob.Brain is IControlledBrain)
			{
				try
				{
					info.Add(" + -- Owner: " + (targetMob.Brain as IControlledBrain).GetPlayerOwner() != null ? (targetMob.Brain as IControlledBrain).GetPlayerOwner().Name : (targetMob.Brain as IControlledBrain).GetNPCOwner().Name);
				}
				catch
				{
					info.Add(" + -- Owner: Not found!");
				}
			}
			if (targetMob.NPCTemplate != null)
			{
				info.Add(" + NPCTemplate: " + "[" + targetMob.NPCTemplate.TemplateId + "] " + targetMob.NPCTemplate.Name);
			}

			if (targetMob is Keeps.IKeepItem)
			{
				info.Add(" ");
				Keeps.IKeepItem keepItem = targetMob as Keeps.IKeepItem;
				if (keepItem.Component != null && keepItem.Component.Keep != null)
				{
					info.Add(" + KeepItem: " + keepItem.Component.AbstractKeep.Name);
				}
				else
				{
					info.Add(" + KeepItem:  No Component or Keep found!");
				}
				info.Add(" ");
			}

			IOldAggressiveBrain aggroBrain = targetMob.Brain as IOldAggressiveBrain;

			if (aggroBrain != null)
			{
				info.Add(" + Aggro level: " + aggroBrain.AggroLevel);
				info.Add(" + Aggro range: " + aggroBrain.AggroRange);

				if (targetMob.MaxDistance < 0)
					info.Add(" + MaxDistance: " + -targetMob.MaxDistance * aggroBrain.AggroRange / 100);
				else
					info.Add(" + MaxDistance: " + targetMob.MaxDistance);

			}
			else
			{
				info.Add(" + Not aggressive brain");
			}

			info.Add(" + Roaming Range: " + targetMob.RoamingRange);
			//info.Add(" + Tether Range: " + targetMob.TetherRange);

			TimeSpan respawn = TimeSpan.FromMilliseconds(targetMob.RespawnInterval);

			if (targetMob.RespawnInterval <= 0)
				info.Add(" + Respawn: NPC will not respawn");
			else
			{
				string days = "";
				string hours = "";

				if (respawn.Days > 0)
					days = respawn.Days + " days ";

				if (respawn.Hours > 0)
					hours = respawn.Hours + " hours ";

				info.Add(" + Respawn: " + days + hours + respawn.Minutes + " minutes " + respawn.Seconds + " seconds");
				info.Add(" + SpawnPoint:  " + targetMob.SpawnPoint.X + ", " + targetMob.SpawnPoint.Y + ", " + targetMob.SpawnPoint.Z);
			}

			info.Add(" ");
			info.Add(" +     STR      /      CON      /      DEX      /      QUI");
			info.Add(" + " + targetMob.Strength + " (" + targetMob.GetModified(eProperty.Strength) + ")  /  " + targetMob.Constitution + " (" + targetMob.GetModified(eProperty.Constitution) + ")  /  " + targetMob.Dexterity + " (" + targetMob.GetModified(eProperty.Dexterity) + ")  /  " + targetMob.Quickness + " (" + targetMob.GetModified(eProperty.Quickness) + ")");
			info.Add(" +     INT      /     EMP     /     PIE     /     CHR");
			info.Add(" + " + targetMob.Intelligence + " (" + targetMob.GetModified(eProperty.Intelligence) + ")  /  " + targetMob.Empathy + " (" + targetMob.GetModified(eProperty.Empathy) + ")  /  " + targetMob.Piety + " (" + targetMob.GetModified(eProperty.Piety) + ")  /  " + targetMob.Charisma + " (" + targetMob.GetModified(eProperty.Charisma) + ")");
			info.Add(" + Block / Parry / Evade %:  " + targetMob.BlockChance + " / " + targetMob.ParryChance + " / " + targetMob.EvadeChance);
			info.Add(" + Attack Speed (Melee Speed Increase %):  " + targetMob.AttackSpeed(targetMob.AttackWeapon) + " (" + (100 - targetMob.GetModified(eProperty.MeleeSpeed)) + ")");

			if (targetMob.GetModified(eProperty.MeleeDamage) != 0)
				info.Add($" + Damage Bonus: {targetMob.GetModified(eProperty.MeleeDamage)}");

			info.Add(" + Casting Speed Increase %:  " + targetMob.GetModified(eProperty.CastingSpeed));

			if (targetMob.LeftHandSwingChance > 0)
				info.Add(" + Left Swing %: " + targetMob.LeftHandSwingChance);

			if (targetMob.Abilities != null && targetMob.Abilities.Count > 0)
				info.Add(" + Abilities: " + targetMob.Abilities.Count);

			if (targetMob.Spells != null && targetMob.Spells.Count > 0)
				info.Add(" + Spells: " + targetMob.Spells.Count);

			if (targetMob.Styles != null && targetMob.Styles.Count > 0)
				info.Add(" + Styles: " + targetMob.Styles.Count);

			info.Add(" ");

			info.Add(" + Model:  " + targetMob.Model + " sized to " + targetMob.Size);
			info.Add(" + Damage type: " + targetMob.MeleeDamageType);

			if (targetMob.Race > 0)
				info.Add(" + Race:  " + targetMob.Race);

			if (targetMob.BodyType > 0)
				info.Add(" + Body Type:  " + targetMob.BodyType);

			info.Add(" ");

			info.Add("Race Resists:");
			info.Add(" +  -- Crush/Slash/Thrust:  " + targetMob.GetDamageResist(eProperty.Resist_Crush)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Slash)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Thrust));
			info.Add(" +  -- Heat/Cold/Matter:  " + targetMob.GetDamageResist(eProperty.Resist_Heat)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Cold)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Matter));
			info.Add(" +  -- Body/Spirit/Energy:  " + targetMob.GetDamageResist(eProperty.Resist_Body)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Spirit)
			         + " / " + targetMob.GetDamageResist(eProperty.Resist_Energy));
			info.Add(" +  -- Natural:  " + targetMob.GetDamageResist(eProperty.Resist_Natural));

			info.Add(" ");

			info.Add("Current Resists:");
			info.Add(" +  -- Crush/Slash/Thrust:  " + targetMob.GetModified(eProperty.Resist_Crush)
			         + " / " + targetMob.GetModified(eProperty.Resist_Slash)
			         + " / " + targetMob.GetModified(eProperty.Resist_Thrust));
			info.Add(" +  -- Heat/Cold/Matter:  " + targetMob.GetModified(eProperty.Resist_Heat)
			         + " / " + targetMob.GetModified(eProperty.Resist_Cold)
			         + " / " + targetMob.GetModified(eProperty.Resist_Matter));
			info.Add(" +  -- Body/Spirit/Energy:  " + targetMob.GetModified(eProperty.Resist_Body)
			         + " / " + targetMob.GetModified(eProperty.Resist_Spirit)
			         + " / " + targetMob.GetModified(eProperty.Resist_Energy));
			info.Add(" +  -- Natural:  " + targetMob.GetModified(eProperty.Resist_Natural));

			info.Add(" ");

			info.Add(" + Position (X, Y, Z, H):  " + targetMob.X + ", " + targetMob.Y + ", " + targetMob.Z + ", " + targetMob.Heading);

			if (targetMob.GuildName != null && targetMob.GuildName.Length > 0)
				info.Add(" + Guild: " + targetMob.GuildName);

			if (targetMob.InHouse)
			{
				info.Add(" ");
				info.Add(" + In House # " + targetMob.CurrentHouse.HouseNumber);
				info.Add(" ");
			}

			info.Add(string.Format(" + Flags: {0} (0x{1})", ((GameNPC.eFlags)targetMob.Flags).ToString("G"), targetMob.Flags.ToString("X")));
			info.Add(" + OID: " + targetMob.ObjectID);
			info.Add(" + Active weapon slot: " + targetMob.ActiveWeaponSlot);
			info.Add(" + Visible weapon slot: " + targetMob.VisibleActiveWeaponSlots);
			info.Add(" + Speed(current/max): " + targetMob.CurrentSpeed + "/" + targetMob.MaxSpeedBase);
			info.Add(" + Health: " + targetMob.Health + "/" + targetMob.MaxHealth);

			if (targetMob.EquipmentTemplateID != null && targetMob.EquipmentTemplateID.Length > 0)
				info.Add(" + Equipment Template ID: " + targetMob.EquipmentTemplateID);

			if (targetMob.Inventory != null)
				info.Add(" + Inventory: " + targetMob.Inventory.AllItems.Count + " items");

			info.Add(" + Quests to give:  " + targetMob.QuestListToGive.Count);

			if (targetMob.PathID != null && targetMob.PathID.Length > 0)
				info.Add(" + Path: " + targetMob.PathID);

			if (targetMob.OwnerID != null && targetMob.OwnerID.Length > 0)
				info.Add(" + OwnerID: " + targetMob.OwnerID);

			info.Add(" + Package ID:  " + targetMob.PackageID);
			info.Add(" ");
			info.Add(" + Mob_ID:  " + targetMob.InternalID);

			if (targetMob.ambientTexts != null)
			{
				info.Add (" ");
				info.Add ("Ambient Texts:");
				foreach(var txt in targetMob.ambientTexts)
				{
					info.Add(" - (" + txt.Trigger + " " + txt.Chance + "% with emote #" + txt.Emote + ")");
					info.Add("    " + txt.Text);
				}
			}
			client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
		}
		
		private void stats(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob == null)
			{
				client.Out.SendMessage("No target selected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			var info = new List<string>();
			info.Add("Modified stats:");
			info.Add("");
			for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; ++property)
				info.Add(String.Format("{0}: {1}",
				                       GlobalConstants.PropertyToName(property),
				                       targetMob.GetModified(property)));
			info.Add("");
			info.Add("Modified resists:");
			info.Add("");
			for (eProperty property = eProperty.Resist_First + 1; property <= eProperty.Resist_Last; ++property)
				info.Add(String.Format("{0}: {1}",
				                       GlobalConstants.PropertyToName(property),
				                       targetMob.GetModified(property)));
			info.Add("");
			info.Add("Miscellaneous:");
			info.Add("");

			if (targetMob.GetModified(eProperty.MeleeDamage) != 0)
				info.Add($" + Damage Bonus: {targetMob.GetModified(eProperty.MeleeDamage)}");

			info.Add(" + Attack Speed (Melee Speed Increase %):  " + targetMob.AttackSpeed(targetMob.AttackWeapon) + " (" + (100 - targetMob.GetModified(eProperty.MeleeSpeed)) + ")");

			info.Add(String.Format("Maximum Health: {0}", targetMob.MaxHealth));
			info.Add(String.Format("Armor Factor (AF): {0}", targetMob.GetModified(eProperty.ArmorFactor)));
			info.Add(String.Format("Absorption (ABS): {0}", targetMob.GetModified(eProperty.ArmorAbsorption)));
			client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
			return;
		}

		private void realm(GameClient client, GameNPC targetMob, string[] args)
		{
			byte realm;

			try
			{
				realm = Convert.ToByte(args[2]);
				targetMob.Realm = (eRealm)realm;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob realm changed to: " + GlobalConstants.RealmToName(targetMob.Realm), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void speed(GameClient client, GameNPC targetMob, string[] args)
		{
			short maxSpeed;

			try
			{
				maxSpeed = Convert.ToInt16(args[2]);
				targetMob.MaxSpeedBase = maxSpeed;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob MaxSpeed changed to: " + targetMob.MaxSpeedBase, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void level(GameClient client, GameNPC targetMob, string[] args)
		{
			byte level;

			if (args.Length > 2 && byte.TryParse(args[2], out level))
			{
				targetMob.Level = level;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob level changed to: " + targetMob.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void levela(GameClient client, GameNPC targetMob, string[] args)
		{
			byte level;

			try
			{
				level = Convert.ToByte(args[2]);
				targetMob.Level = level; // Also calls AutoSetStats()
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob level changed to: " + targetMob.Level + " and stats adjusted", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void brain(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				ABrain brains = null;
				string brainType = args[2];

				try
				{
					client.Out.SendDebugMessage(Assembly.GetAssembly(typeof(GameServer)).FullName);
					brains = (ABrain)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(brainType, false);
				}
				catch (Exception e)
				{
					client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}

				if (brains == null)
				{
					try
					{
						client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
						brains = (ABrain)Assembly.GetExecutingAssembly().CreateInstance(brainType, false);
					}
					catch (Exception e)
					{
						client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
				}
				if (brains == null)
				{
					foreach (Assembly script in ScriptMgr.GameServerScripts)
					{
						try
						{							
							client.Out.SendDebugMessage(script.FullName);
							brains = (ABrain)script.CreateInstance(brainType, false);
							
							if (brains != null) break;
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
					}
				}
				if (brains == null)
				{
					client.Out.SendMessage("Could not find brain " + brainType + ". Check spelling and script namespace (case sensitive)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					targetMob.SetOwnBrain(brains);
					targetMob.SaveIntoDatabase();
					client.Out.SendMessage(targetMob.Name + "'s brain set to " + targetMob.Brain.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void respawn(GameClient client, GameNPC targetMob, string[] args)
		{
			int interval;

			try
			{
				interval = Convert.ToInt32(args[2]);
				targetMob.RespawnInterval = interval;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob respawn interval changed to: " + interval, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void questinfo(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob.QuestListToGive.Count == 0 && targetMob.DataQuestList.Count == 0)
			{
				client.Out.SendMessage("Mob does not have any quests.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Out.SendMessage("Scripted Quests: ------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);

				foreach (AbstractQuest quest in targetMob.QuestListToGive)
					client.Out.SendMessage("Quest Name: [" + quest.Name + "]", eChatType.CT_System, eChatLoc.CL_PopupWindow);

				client.Out.SendMessage("Data Quests: ----------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);

				foreach (DataQuest dq in targetMob.DataQuestList)
					client.Out.SendMessage("Quest Name: [" + dq.Name + "] : " + (DataQuest.eStartType)dq.DBDataQuest.StartType, eChatType.CT_System, eChatLoc.CL_PopupWindow);

			}
		}

		private void refreshquests(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				GameObject.FillDataQuestCache();
				targetMob.LoadDataQuests(client.Player);
				foreach (GamePlayer player in targetMob.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendNPCsQuestEffect(targetMob, targetMob.GetQuestIndicator(player));
				}
				client.Out.SendMessage(targetMob.DataQuestList.Count + " Data Quests loaded for this mob.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception ex)
			{
				Log.Error("Error refreshing quests.", ex);
				throw;
			}
		}

		private void equipinfo(GameClient client, GameNPC targetMob, string[] args)
		{
			if (targetMob.Inventory == null)
			{
				client.Out.SendMessage("Mob inventory not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			client.Out.SendMessage("-------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			string closed = "";

			if (targetMob.Inventory is GameNpcInventoryTemplate)
			{
				GameNpcInventoryTemplate t = (GameNpcInventoryTemplate)targetMob.Inventory;
				closed = t.IsClosed ? " (closed)" : " (open)";
			}

			string message = string.Format("			         Inventory: {0}{1}, equip template ID: {2}", targetMob.Inventory.GetType().ToString(), closed, targetMob.EquipmentTemplateID);
			client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("-------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);

			foreach (InventoryItem item in targetMob.Inventory.AllItems)
			{
				client.Out.SendMessage("Slot Description : [" + GlobalConstants.SlotToName(item.SlotPosition) + "]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("         Slot: " + GlobalConstants.SlotToName(item.Item_Type), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("        Model: " + item.Model, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("        Color: " + item.Color, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("       Effect: " + item.Effect, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		private void equiptemplate(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			if (args[2].ToLower() == "clear")
			{
				targetMob.Inventory = null;
				targetMob.EquipmentTemplateID = null;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob equipment cleared.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				targetMob.BroadcastLivingEquipmentUpdate();
				return;
			}
			else if (args[2].ToLower() == "create")
			{
				try
				{
					if (targetMob.Inventory != null)
					{
						client.Out.SendMessage("Target mob inventory is set to " + targetMob.Inventory.GetType() + ", remove it first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					targetMob.Inventory = new GameNpcInventoryTemplate();

					client.Out.SendMessage("Inventory template created.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				catch
				{
					DisplaySyntax(client, args[1], args[2]);
				}

				targetMob.BroadcastLivingEquipmentUpdate();
				return;
			}
			else if (args[2].ToLower() == "load")
			{
				if (args.Length > 3)
				{
					if (targetMob.Inventory != null && !(targetMob.Inventory is GameNpcInventoryTemplate))
					{
						client.Out.SendMessage("Target mob is not using GameNpcInventoryTemplate.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					try
					{
						GameNpcInventoryTemplate load = new GameNpcInventoryTemplate();

						if (!load.LoadFromDatabase(args[3]))
						{
							client.Out.SendMessage("Error loading equipment template \"" + args[3] + "\"", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						targetMob.EquipmentTemplateID = args[3];
						targetMob.Inventory = load;
						targetMob.SaveIntoDatabase();
						targetMob.BroadcastLivingEquipmentUpdate();
						client.Out.SendMessage("Mob equipment loaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					catch
					{
						DisplaySyntax(client, args[1], args[2]);
					}
				}
				else
				{
					DisplaySyntax(client, args[1], args[2]);
				}

				return;
			}

			GameNpcInventoryTemplate template = targetMob.Inventory as GameNpcInventoryTemplate;
			if (template == null)
			{
				client.Out.SendMessage("Target mob is not using GameNpcInventoryTemplate.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (args[2])
			{
				case "add":
					{
						if (args.Length >= 5)
						{
							try
							{
								int slot = GlobalConstants.NameToSlot(args[3]);

								if (slot == 0)
								{
									client.Out.SendMessage("No such slot available, remember to use slot name (distance, torso, cloak, etc.)!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}

								int model = Convert.ToInt32(args[4]);
								int color = 0;
								int effect = 0;
								int extension = 0;

								if (args.Length >= 6)
									color = Convert.ToInt32(args[5]);
								if (args.Length >= 7)
									effect = Convert.ToInt32(args[6]);
								if (args.Length >= 8)
									extension = Convert.ToInt32(args[7]);

								if (!template.AddNPCEquipment((eInventorySlot)slot, model, color, effect, extension))
								{
									client.Out.SendMessage("Couldn't add new item to slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}

								client.Out.SendMessage("Item added to the mob's inventory template.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							catch
							{
								DisplaySyntax(client, args[1], args[2]);
								return;
							}
						}
						else
						{
							DisplaySyntax(client, args[1], args[2]);
							return;
						}
					}
					break;

				case "remove":
					{
						if (args.Length > 3)
						{
							try
							{
								int slot = GlobalConstants.NameToSlot(args[3]);

								if (slot == 0)
								{
									client.Out.SendMessage("No such slot available!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}

								if (!template.RemoveNPCEquipment((eInventorySlot)slot))
								{
									client.Out.SendMessage("Couldn't remove item from slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								client.Out.SendMessage("Mob inventory template slot " + slot + " cleaned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							catch
							{
								DisplaySyntax(client, args[1], args[2]);
								return;
							}
						}
						else
						{
							DisplaySyntax(client, args[1], args[2]);
							return;
						}
					}
					break;

				case "close":
					{
						if (template.IsClosed)
						{
							client.Out.SendMessage("Template is already closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						targetMob.Inventory = template.CloseTemplate();
						client.Out.SendMessage("Inventory template closed succesfully.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				case "save":
					{
						if (args.Length > 3)
						{
							bool replace = (args.Length > 4 && args[4].ToLower() == "replace");

							var existingTemplates = GameServer.Database.SelectObjects<NPCEquipment>("`TemplateID` = @TemplateID", new QueryParameter("@TemplateID", args[3]));

							if (existingTemplates.Count > 0)
							{
								if (replace)
								{
									GameServer.Database.DeleteObject(existingTemplates);
								}
								else
								{
									client.Out.SendMessage("Template with name '" + args[3] + "' already exists. Use the 'replace' parameter if you want to overwrite it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
							}

							if (!targetMob.Inventory.SaveIntoDatabase(args[3]))
							{
								client.Out.SendMessage("Error saving template with name " + args[3], eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}

							targetMob.EquipmentTemplateID = args[3];
							targetMob.SaveIntoDatabase();
							GameNpcInventoryTemplate.Init();
							client.Out.SendMessage("Target mob equipment template is saved as '" + args[3] + "'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						else
						{
							DisplaySyntax(client, args[1], args[2]);
						}
					}
					break;

				default:
					DisplaySyntax(client, args[1], args[2]);
					break;
			}

			targetMob.BroadcastLivingEquipmentUpdate();
		}

		private void visibleslot(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				int slot = GlobalConstants.NameToSlot(args[2]);

				if (slot == 0)
				{
					client.Out.SendMessage("Bad slot.  Use names like right, left, two, distance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				string slotname = GlobalConstants.SlotToName(slot);

				switch (slotname)
				{
					case "righthand":
					case "lefthand":
						targetMob.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;
					case "twohanded":
						targetMob.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						break;
					case "distance":
						targetMob.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
						break;

					default:
						client.Out.SendMessage("Invalid slot, must be a weapon slot (right, left, two, distance)!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
				}

				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Visible weapon slot set to " + slotname, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch
			{
				DisplaySyntax(client, args[1]);
				return;
			}
		}

		private void dropcount<T>(GameClient client, GameNPC targetMob, string[] args) where T : MobXLootTemplate
		{
			T mxlt =
				GameServer.Database.SelectObjects<T>("`MobName` = @MobName AND `LootTemplateName` = @LootTemplateName", new [] { new QueryParameter("@MobName", targetMob.Name), new QueryParameter("@LootTemplateName", targetMob.Name) }).FirstOrDefault();

			if (args.Length < 3)
			{
				DisplayMessage(client, "Mob '" + targetMob.Name + "' drops " + mxlt.DropCount + " items.");
			}
			else
			{
				if (mxlt == null)
				{
					mxlt = Activator.CreateInstance<T>();
					mxlt.MobName = targetMob.Name;
					mxlt.LootTemplateName = targetMob.Name;
				}
				int dropCount;
				if (!int.TryParse(args[2], out dropCount))
				{
					DisplaySyntax(client, args[1]);
					return;
				}

				if (dropCount < 1)
					dropCount = 1;

				mxlt.DropCount = dropCount;
				if (!mxlt.IsPersisted)
					GameServer.Database.AddObject(mxlt);
				else
					GameServer.Database.SaveObject(mxlt);
				DisplayMessage(client, "Mob '" + targetMob.Name + "' will drop a maximum of " + mxlt.DropCount + " items!");
			}
		}

		private void addloot<MobXLootType, LootTemplateType>(GameClient client, GameNPC targetMob, string[] args)
			where MobXLootType : MobXLootTemplate
			where LootTemplateType : LootTemplate
		{
			try
			{
				string lootTemplateID = args[2];
				string name = targetMob.Name;
				int chance = Convert.ToInt16(args[3]);
				int numDrops = 0;

				if (chance == 100 && args.Length > 4)
				{
					numDrops = Convert.ToInt16(args[4]);
				}

				if (numDrops < 1)
					numDrops = 1;

				ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplateID);
				if (item == null)
				{
					DisplayMessage(client,
					               "You cannot add the " + lootTemplateID + " to the " + targetMob.Name + " because the item does not exist.");
					return;
				}

				var template =
					GameServer.Database.SelectObjects<LootTemplateType>("`TemplateName` = @TemplateName AND `ItemTemplateID` = @ItemTemplateID", new[] { new QueryParameter("@TemplateName", name), new QueryParameter("@ItemTemplateID", lootTemplateID) });
				if (template != null)
				{
					GameServer.Database.DeleteObject(template);

					LootTemplateType lt = Activator.CreateInstance<LootTemplateType>();
					lt.Chance = chance;
					lt.TemplateName = name;
					lt.ItemTemplateID = lootTemplateID;
					lt.Count = numDrops;
					GameServer.Database.AddObject(lt);
					refreshloot(client, targetMob, null);
				}
				else
				{
					ItemTemplate itemtemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplateID);
					if (itemtemplate == null)
					{
						DisplayMessage(client, "ItemTemplate " + lootTemplateID + " not found!");
						return;
					}

					LootTemplateType lt = Activator.CreateInstance<LootTemplateType>();
					lt.Chance = chance;
					lt.TemplateName = name;
					lt.ItemTemplateID = lootTemplateID;
					lt.Count = numDrops;
					GameServer.Database.AddObject(lt);
					refreshloot(client, targetMob, null);
				}

				if (chance < 100)
				{
					client.Out.SendMessage(
						item.Name + " was succesfully added to the loot list for  " + name + " with a " + chance + "% chance to drop!",
						eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Out.SendMessage(
						item.Name + " was succesfully added to the loot list for " + name + " with a drop count of " + numDrops + "!",
						eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				MobXLootType mxlt =
					GameServer.Database.SelectObjects<MobXLootType>("`MobName` = @MobName AND `LootTemplateName` = @LootTemplateName", new [] { new QueryParameter("@MobName", targetMob.Name), new QueryParameter("@LootTemplateName", name) }).FirstOrDefault();

				if (mxlt == null)
				{
					DisplayMessage(client,
					               "If you need to limit max number of drops per kill for this mob then use /mob addmobxlt <max num drops> to add a MobXLootTemplate entry.");
				}
				else
				{
					DisplayMessage(client,
					               "A MobXLootTemplate entry exists for this mob and limits the total drops per kill to " + mxlt.DropCount);
				}

			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void addotd(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				string mobName = targetMob.Name;
				string itemTemplateID = args[2];
				int minlevel = Convert.ToInt32(args[3]);

				ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplateID);
				if (item == null)
				{
					DisplayMessage(client, "You cannot add the " + itemTemplateID + " to the " + targetMob.Name + " because the item does not exist.");
					return;
				}

				LootOTD otd = GameServer.Database.SelectObjects<LootOTD>("`MobName` = @MobName AND `ItemTemplateID` = @ItemTemplateID", new [] { new QueryParameter("@MobName", mobName), new QueryParameter("@ItemTemplateID", itemTemplateID) }).FirstOrDefault();

				if (otd != null)
				{
					DisplayMessage(client, "ItemTemplate " + itemTemplateID + " is already in this in " + mobName + "'s OTD list!");
				}
				else
				{
					ItemTemplate itemtemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplateID);
					if (itemtemplate == null)
					{
						DisplayMessage(client, "ItemTemplate " + itemTemplateID + " not found!");
						return;
					}

					LootOTD loot = new LootOTD();
					loot.MobName = mobName;
					loot.ItemTemplateID = itemtemplate.Id_nb;
					loot.MinLevel = minlevel;
					GameServer.Database.AddObject(loot);
					refreshloot(client, targetMob, null);
					client.Out.SendMessage(itemTemplateID + " was succesfully added to " + mobName + "'s one time drop list.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void viewloot(GameClient client, GameNPC targetMob, string[] args)
		{
			refreshloot(client, targetMob, null);

			if (args.Length > 2 && args[2] == "random")
			{
				// generated loots go in inventory
				if (args.Length > 3 && args[3] == "inv")
				{
					targetMob.AddXPGainer(client.Player, 1);
					targetMob.DropLoot(client.Player);
					targetMob.RemoveAttacker(client.Player);
					return;
				}

				ItemTemplate[] templates = LootMgr.GetLoot(targetMob, client.Player);
				DisplayMessage(client, "[ " + targetMob.Name + "'s Loot Table ]\n\n");
				foreach (ItemTemplate temp in templates)
				{
					string message = string.Format("Name: {0}, Id_nb: {1}", temp.Name, temp.Id_nb);
					DisplayMessage(client, message);
				}
			}
			else
			{
				var text = new List<string>();
				text.Add("");

				IList<LootOTD> otds = GameServer.Database.SelectObjects<LootOTD>("`MobName` = @MobName", new QueryParameter("@MobName", targetMob.Name));

				if (otds != null && otds.Count > 0)
				{
					text.Add("One time drops:");
					text.Add("");

					foreach (LootOTD otd in otds)
					{
						ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(otd.ItemTemplateID);

						if (drop != null)
						{
							text.Add("- " + drop.Name + " (" + drop.Id_nb + "), Min Level = " + otd.MinLevel);
						}
						else
						{
							text.Add("Error, Item not found: " + otd.ItemTemplateID);
						}
					}

					client.Out.SendCustomTextWindow(targetMob.Name + "'s One Time Drops", text);
				}

				text.Add("");
				text.Add("LootGeneratorTemplate:");
				text.Add("");
				DisplayLoots<MobXLootTemplate, LootTemplate>(text, targetMob);

				text.Add("");
				text.Add("LootGeneratorMobTemplate:");
				text.Add("");
				DisplayLoots<MobDropTemplate, DropTemplateXItemTemplate>(text, targetMob);

				client.Out.SendCustomTextWindow(targetMob.Name + "'s Loot Table", text);
			}
		}

		private static void DisplayLoots<MobDropTemplateType, LootTemplateType>(List<string> text, GameNPC mob)
			where MobDropTemplateType : MobXLootTemplate
			where LootTemplateType : LootTemplate
		{
			bool didDefault = false;
			bool fromNPCT = false;
			
			// is a templated mob, so its drop list is
			IEnumerable<MobDropTemplateType> mobXloot = null;
			string mobName = mob.Name;
			if (mob.NPCTemplate != null)
			{
				fromNPCT = true;
				mobXloot = GameServer.Database.SelectObjects<MobDropTemplateType>("`MobName` = @MobName", new QueryParameter("@MobName", mob.NPCTemplate.TemplateId));
			}
			if (mobXloot==null || (mobXloot!=null && mobXloot.Count()==0)) mobXloot = GameServer.Database.SelectObjects<MobDropTemplateType>("`MobName` = @MobName", new QueryParameter("@MobName", mobName));
			
			foreach (var mobXtemplate in mobXloot)
			{
				didDefault = didDefault || mobXtemplate.LootTemplateName == mobName;
				var template = GameServer.Database.SelectObjects<LootTemplateType>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", mobXtemplate.LootTemplateName));
				if (template.Count > 0)
					text.Add("+ Mob's template [from " + (fromNPCT?mob.NPCTemplate.TemplateId.ToString():mobName) + "]: "+ mobXtemplate.LootTemplateName + " (DropCount: " + mobXtemplate.DropCount + ")");
				text.AddRange(
					from loot in template
					let drop = GameServer.Database.FindObjectByKey<ItemTemplate>(loot.ItemTemplateID)
					select "- " + (drop == null ? "(Template Not Found)" : drop.Name) +
					" (" + loot.ItemTemplateID + ") Count: " + loot.Count + " Chance: " + loot.Chance
				);
			}
			if (!didDefault)
			{
				var template = GameServer.Database.SelectObjects<LootTemplateType>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", mobName));
				if (template.Count > 0)
					text.Add("+ Default: ");
				text.AddRange(
					from loot in template
					let drop = GameServer.Database.FindObjectByKey<ItemTemplate>(loot.ItemTemplateID)
					select "- " + (drop == null ? "(Template Not Found)" : drop.Name) +
					" (" + loot.ItemTemplateID + ") Count: " + loot.Count + " Chance: " + loot.Chance
				);
			}
		}

		private void removeloot<LootTemplateType>(GameClient client, GameNPC targetMob, string[] args)
			where LootTemplateType : LootTemplate
		{
			string lootTemplateID = args[2];
			string name = targetMob.Name;

			if (lootTemplateID.ToLower() == "all items")
			{
				var template = GameServer.Database.SelectObjects<LootTemplateType>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", name));

				if (template != null && template.Count > 0)
				{
					Log.ErrorFormat("{0} removing all items from LootTemplate {1}!", client.Player.Name, name);

					foreach (LootTemplateType loot in template)
					{
						Log.ErrorFormat("{0}", loot.ItemTemplateID);
						GameServer.Database.DeleteObject(loot);
					}

					refreshloot(client, targetMob, null);
					client.Out.SendMessage("Removed all items from " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Out.SendMessage("No items found on " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				IList<LootTemplateType> template = GameServer.Database.SelectObjects<LootTemplateType>("`TemplateName` = @TemplateName AND `ItemTemplateID` = @ItemTemplateID", new [] { new QueryParameter("@TemplateName", name), new QueryParameter("@ItemTemplateID", lootTemplateID) });

				if (template != null && template.Count > 0)
				{

					GameServer.Database.DeleteObject(template);

					refreshloot(client, targetMob, null);
					client.Out.SendMessage(lootTemplateID + " removed from " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Out.SendMessage(lootTemplateID + " does not exist on " + name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		private void removeotd(GameClient client, GameNPC targetMob, string[] args)
		{
			string itemTemplateID = args[2];
			string name = targetMob.Name;

			IList<LootOTD> template = GameServer.Database.SelectObjects<LootOTD>("`MobName` = @MobName AND `ItemTemplateID` = @ItemTemplateID", new[] { new QueryParameter("@MobName", name), new QueryParameter("@ItemTemplateID", itemTemplateID) });

			if (template != null)
			{
				GameServer.Database.DeleteObject(template);

				refreshloot(client, targetMob, null);

				client.Out.SendMessage(itemTemplateID + " OTD removed from " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Out.SendMessage(itemTemplateID + " OTD does not exist on " + name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private void refreshloot(GameClient client, GameNPC targetMob, string[] args)
		{
			LootMgr.RefreshGenerators(targetMob);
		}

		void setClass(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			GameNPC mob = null;

			try
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					mob = assembly.CreateInstance(args[2], true) as GameNPC;

					if (mob != null)
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Error("/mob class - mob", ex);
			}

			if (mob == null)
			{
				try
				{
					foreach (Assembly assembly in ScriptMgr.Scripts)
					{
						mob = assembly.CreateInstance(args[2], true) as GameNPC;

						if (mob != null)
							break;
					}
				}
				catch (Exception ex)
				{
					Log.Error("/mob class - mob", ex);
				}
			}

			if (mob == null)
			{
				client.Out.SendMessage("There was an error creating an instance of " + args[2] + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			targetMob.StopAttack();
			targetMob.StopCurrentSpellcast();

			mob.X = targetMob.X;
			mob.Y = targetMob.Y;
			mob.Z = targetMob.Z;
			mob.CurrentRegion = targetMob.CurrentRegion;
			mob.Heading = targetMob.Heading;
			mob.Level = targetMob.Level;
			mob.Realm = targetMob.Realm;
			mob.Name = targetMob.Name;
			mob.GuildName = targetMob.GuildName;
			mob.Model = targetMob.Model;
			mob.Size = targetMob.Size;
			mob.Flags = targetMob.Flags;
			mob.MeleeDamageType = targetMob.MeleeDamageType;
			mob.RespawnInterval = targetMob.RespawnInterval;
			mob.RoamingRange = targetMob.RoamingRange;
			mob.Strength = targetMob.Strength;
			mob.Constitution = targetMob.Constitution;
			mob.Dexterity = targetMob.Dexterity;
			mob.Quickness = targetMob.Quickness;
			mob.Intelligence = targetMob.Intelligence;
			mob.Empathy = targetMob.Empathy;
			mob.Piety = targetMob.Piety;
			mob.Charisma = targetMob.Charisma;
			mob.CurrentSpeed = 0;
			mob.MaxSpeedBase = targetMob.MaxSpeedBase;
			mob.Inventory = targetMob.Inventory;
			mob.EquipmentTemplateID = targetMob.EquipmentTemplateID;

			if (mob.Inventory != null)
				mob.SwitchWeapon(targetMob.ActiveWeaponSlot);

			ABrain brain = null;
			try
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					brain = (ABrain)assembly.CreateInstance(targetMob.Brain.GetType().FullName, true);
					if (brain != null)
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Error("/mob class - brain", ex);
			}

			if (brain == null)
			{
				try
				{
					foreach (Assembly assembly in ScriptMgr.Scripts)
					{
						brain = assembly.CreateInstance(targetMob.Brain.GetType().FullName, true) as ABrain;

						if (brain != null)
							break;
					}
				}
				catch (Exception ex)
				{
					Log.Error("/mob class - brain", ex);
				}
			}

			if (brain == null)
			{
				client.Out.SendMessage("Cannot create brain, standard brain being applied", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				mob.SetOwnBrain(new StandardMobBrain());
			}
			else
			{
				StandardMobBrain sbrain = (StandardMobBrain)brain;
				StandardMobBrain tsbrain = (StandardMobBrain)targetMob.Brain;
				sbrain.AggroLevel = tsbrain.AggroLevel;
				sbrain.AggroRange = tsbrain.AggroRange;
				mob.SetOwnBrain(sbrain);
			}

			mob.AddToWorld();
			mob.LoadedFromScript = false;
			mob.SaveIntoDatabase();

			// delete old mob
			targetMob.DeleteFromDatabase();
			targetMob.Delete();

			client.Out.SendMessage("Mob class changed: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void copy(GameClient client, GameNPC targetMob, string[] args)
		{
			GameNPC mob = null;

			if (args.Length > 2)
			{
				string mobName = string.Join(" ", args, 2, args.Length - 2);

				Mob dbMob = GameServer.Database.SelectObjects<Mob>("`Name` = @Name", new QueryParameter("@Name", mobName)).FirstOrDefault();

				if (dbMob != null)
				{
					if (dbMob.ClassType == typeof(GameNPC).FullName)
					{
						mob = new GameNPC();
						targetMob = new GameNPC();
					}
					else
					{
						foreach (Assembly script in ScriptMgr.GameServerScripts)
						{
							try
							{
								mob = (GameNPC)script.CreateInstance(dbMob.ClassType, false);
								targetMob = (GameNPC)script.CreateInstance(dbMob.ClassType, false);

								if (mob != null)
									break;
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
					}

					if (targetMob != null)
					{
						targetMob.LoadFromDatabase(dbMob);
					}
				}

				if (mob == null || targetMob == null)
				{
					client.Out.SendMessage("Unable to find mob named:  " + mobName, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
			else
			{
				if (targetMob == null)
				{
					client.Out.SendMessage("You must have a mob targeted to copy.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					mob = (GameNPC)assembly.CreateInstance(targetMob.GetType().FullName, true);
					if (mob != null)
						break;
				}

				if (mob == null)
				{
					client.Out.SendMessage("There was an error creating an instance of " + targetMob.GetType().FullName + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			// Load NPCTemplate before overriding NPCTemplate's variables
			//Kataract : Added a control to avoid an ingame error while copying reloaded mobs
			if (targetMob.NPCTemplate != null && targetMob.NPCTemplate.TemplateId != 0)
			{
				mob.LoadTemplate(targetMob.NPCTemplate);
			}

			//Fill the object variables
			mob.X = client.Player.X;
			mob.Y = client.Player.Y;
			mob.Z = client.Player.Z;
			mob.CurrentRegion = client.Player.CurrentRegion;
			mob.Heading = client.Player.Heading;
			mob.Level = targetMob.Level;
			mob.Realm = targetMob.Realm;
			mob.Name = targetMob.Name;
			mob.Model = targetMob.Model;
			mob.Flags = targetMob.Flags;
			mob.MeleeDamageType = targetMob.MeleeDamageType;
			mob.RespawnInterval = targetMob.RespawnInterval;
			mob.RoamingRange = targetMob.RoamingRange;
			mob.MaxDistance = targetMob.MaxDistance;
			mob.BodyType = targetMob.BodyType;

			// also copies the stats

			mob.Strength = targetMob.Strength;
			mob.Constitution = targetMob.Constitution;
			mob.Dexterity = targetMob.Dexterity;
			mob.Quickness = targetMob.Quickness;
			mob.Intelligence = targetMob.Intelligence;
			mob.Empathy = targetMob.Empathy;
			mob.Piety = targetMob.Piety;
			mob.Charisma = targetMob.Charisma;

			//Fill the living variables
			mob.CurrentSpeed = 0;
			mob.MaxSpeedBase = targetMob.MaxSpeedBase;
			mob.GuildName = targetMob.GuildName;
			mob.Size = targetMob.Size;
			mob.Race = targetMob.Race;

			mob.Inventory = targetMob.Inventory;
			if (mob.Inventory != null)
				mob.SwitchWeapon(targetMob.ActiveWeaponSlot);

			mob.EquipmentTemplateID = targetMob.EquipmentTemplateID;

			if (mob is GameMerchant)
			{
				((GameMerchant)mob).TradeItems = ((GameMerchant)targetMob).TradeItems;
			}

			ABrain brain = null;
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				brain = (ABrain)assembly.CreateInstance(targetMob.Brain.GetType().FullName, true);
				if (brain != null)
					break;
			}

			if (brain == null)
			{
				client.Out.SendMessage("Cannot create brain, standard brain being applied", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				mob.SetOwnBrain(new StandardMobBrain());
			}
			else if (brain is StandardMobBrain)
			{
				StandardMobBrain sbrain = (StandardMobBrain)brain;
				StandardMobBrain tsbrain = (StandardMobBrain)targetMob.Brain;
				sbrain.AggroLevel = tsbrain.AggroLevel;
				sbrain.AggroRange = tsbrain.AggroRange;
				mob.SetOwnBrain(sbrain);
			}

			mob.PackageID = targetMob.PackageID;
			mob.OwnerID = targetMob.OwnerID;

			mob.AddToWorld();
			mob.LoadedFromScript = false;
			mob.SaveIntoDatabase();
			client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if ((mob.Flags & GameNPC.eFlags.PEACE) != 0)
			{
				// because copying 100 mobs with their peace flag set is not fun
				client.Out.SendMessage("This mobs PEACE flag is set!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		private void npctemplate(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			if (args[2].ToLower().Equals("create"))
			{
				npctCreate(client, targetMob, args);
				return;
			}

			if (args[2].ToLower().Equals("clear"))
			{
				if (targetMob != null)
				{
					targetMob.NPCTemplate = null;
					targetMob.BroadcastLivingEquipmentUpdate();
					targetMob.SaveIntoDatabase();
				}
			}

			int id = 0;

			try
			{
				id = Convert.ToInt32(args[2]);
			}
			catch
			{
				DisplaySyntax(client, args[1]);
				return;
			}

			NpcTemplateMgr.Reload();
			INpcTemplate template = NpcTemplateMgr.GetTemplate(id);

			if (template == null)
			{
				DisplayMessage(client, "No template found for " + id, new object[] { });
				return;
			}

			if (targetMob == null)
			{
				GameNPC mob = new GameNPC(template);
				mob.X = client.Player.X;
				mob.Y = client.Player.Y;
				mob.Z = client.Player.Z;
				mob.Heading = client.Player.Heading;
				mob.CurrentRegion = client.Player.CurrentRegion;
				mob.AddToWorld();
				DisplayMessage(client, "Created npc based on template " + id, new object[] { });
			}
			else
			{
				targetMob.LoadTemplate(template);
				targetMob.BroadcastLivingEquipmentUpdate();
				targetMob.NPCTemplate = template as NpcTemplate;
				targetMob.SaveIntoDatabase();
				DisplayMessage(client, "Updated npc based on template " + id, new object[] { });
			}
		}

		void npctCreate(GameClient client, GameNPC targetMob, string[] args)
		{
			if (args.Length < 4)
			{
				DisplaySyntax(client, args[1], args[2]);
				return;
			}

			if (targetMob == null)
			{
				DisplayMessage(client, "You must have a mob selected to create the template from.");
				return;
			}

			int id;
			if (Int32.TryParse(args[3], out id) == false)
			{
				DisplaySyntax(client, args[1], args[2]);
				return;
			}

			bool replace = args[args.Length - 1].Equals("replace");

			NpcTemplate template = NpcTemplateMgr.GetTemplate(id);

			if (template != null && replace == false)
			{
				DisplayMessage(client, "A template with the ID " + id + " already exists.");
				return;
			}

			template = new NpcTemplate(targetMob);
			template.TemplateId = id;
			template.SaveIntoDatabase();
			NpcTemplateMgr.AddTemplate(template);
			DisplayMessage(client, "NPCTemplate saved with ID = " + id + ".");
		}

		private void path(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				string pathname = String.Join(" ", args, 2, args.Length - 2);

				if (pathname != "" && MovementMgr.LoadPath(pathname) == null)
				{
					client.Out.SendMessage("The specified path does not exist", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					targetMob.PathID = pathname;
					targetMob.SaveIntoDatabase();

					if (targetMob.Brain.Stop())
						targetMob.Brain.Start();

					client.Out.SendMessage("The path has been assigned to this mob", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void house(GameClient client, GameNPC targetMob, string[] args)
		{
			ushort house;
			House H;

			try
			{
				house = Convert.ToUInt16(args[2]);
				H = HouseMgr.GetHouse(house);

				if (H != null)
				{
					targetMob.HouseNumber = house;
					targetMob.CurrentHouse = H;
					targetMob.SaveIntoDatabase();
					client.Out.SendMessage("Mob house changed to: " + house, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
					client.Out.SendMessage("House number " + house + " doesn't exist.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void stat(GameClient client, GameNPC targetMob, string[] args)
		{
			string statType = args[1].ToUpper();
			short statval;

			try
			{
				statval = Convert.ToInt16(args[2]);

				switch (statType)
				{
						case "STR": targetMob.Strength = statval; break;
					case "CON":
						targetMob.Constitution = statval;
						targetMob.Health = targetMob.MaxHealth;
						break;
						case "DEX": targetMob.Dexterity = statval; break;
						case "QUI": targetMob.Quickness = statval; break;
						case "INT": targetMob.Intelligence = statval; break;
						case "EMP": targetMob.Empathy = statval; break;
						case "PIE": targetMob.Piety = statval; break;
						case "CHA": targetMob.Charisma = statval; break;
				}

				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob " + statType + " changed to: " + statval, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				if (targetMob.LoadedFromScript == true)
				{
					// Maybe stat changes work on the current script-loaded mob, but are lost on repop or reboot?
					// Send user a warning message, but don't cancel the function altogether
					client.Out.SendMessage("This mob is loaded from a script - stat changes cannot be saved.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception)
			{
				DisplaySyntax(client, "<stat>");
				return;
			}
		}

		private void tether(GameClient client, GameNPC targetMob, string[] args)
		{
			try
			{
				int tether = Convert.ToInt32(args[2]);
				targetMob.TetherRange = tether;
				client.Out.SendMessage("Mob tether range changed to: " + tether, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Keep in mind that this setting is volatile, it needs to be set in this NPC's template to become permanent.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void hood(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.IsCloakHoodUp ^= true;
			targetMob.BroadcastLivingEquipmentUpdate();
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob IsCloakHoodUp flag is set to " + targetMob.IsCloakHoodUp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void cloak(GameClient client, GameNPC targetMob, string[] args)
		{
			targetMob.IsCloakInvisible ^= true;
			targetMob.BroadcastLivingEquipmentUpdate();
			targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob IsCloakInvisible flag is set to " + targetMob.IsCloakHoodUp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void bodytype(GameClient client, GameNPC targetMob, string[] args)
		{
			ushort type;

			if (args.Length > 2 && ushort.TryParse(args[2], out type))
			{
				targetMob.BodyType = type;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("Mob BodyType changed to " + targetMob.BodyType, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void gender(GameClient client, GameNPC targetMob, string[] args)
		{
			byte gender;
			try
			{
				gender = Convert.ToByte(args[2]);

				if (gender > 2)
				{
					DisplaySyntax(client, args[1]);
					return;
				}

				targetMob.Gender = (eGender)gender;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage(String.Format("Mob gender changed to {0}.",
				                                     targetMob.Gender.ToString().ToLower()),
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void packageid(GameClient client, GameNPC targetMob, string[] args)
		{
			string packageID;
			try
			{
				packageID = args[2];

				if (packageID == "")
				{
					DisplaySyntax(client, args[1]);
					return;
				}

				targetMob.PackageID = packageID;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("PackageID set to " + packageID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void ownerid(GameClient client, GameNPC targetMob, string[] args)
		{
			string ownerID;
			try
			{
				ownerID = args[2];

				if (ownerID == "")
				{
					DisplaySyntax(client, args[1]);
					return;
				}

				targetMob.OwnerID = ownerID;
				targetMob.SaveIntoDatabase();
				client.Out.SendMessage("OwnerID set to " + ownerID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private string CheckName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}

		private string CheckGuildName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: guild name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}

		private GameNPC select(ushort range, GameClient client)
		{
			// try to target another mob in radius AUTOSELECT_RADIUS units
			foreach (GameNPC wantedMob in client.Player.GetNPCsInRadius(range))
			{
				if (wantedMob == null)
				{
					client.Out.SendMessage("Unable to autoselect an NPC - nothing in range", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Out.SendMessage("You have autoselected the mob with OID " + wantedMob.ObjectID.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendChangeTarget((GameObject)wantedMob);
				}
				return wantedMob;
			}
			return null;
		}

		private void reload(GameClient client, GameNPC targetMob, string[] args)
		{
			ArrayList mobs = new ArrayList();
			// Find the mob(s) to reload.
			if (args.Length > 2)
			{
				// Reload the mob(s)
				for (eRealm i = eRealm._First; i <= eRealm._Last; i++)
				{
					mobs.Add(WorldMgr.GetNPCsByName(args[2], i));
				}
				foreach (GameNPC[] ma in mobs)
				{
					foreach (GameNPC n in ma)
					{
						if (n.LoadedFromScript == false)
						{
							n.RemoveFromWorld();
							n.LoadFromDatabase(GameServer.Database.FindObjectByKey<Mob>(n.InternalID));
							n.AddToWorld();
							client.Player.Out.SendMessage(n.Name + " reloaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
				}
			}
			else if (targetMob != null)
			{
				// Reload the target
				if (targetMob.LoadedFromScript == false)
				{
					targetMob.RemoveFromWorld();
					targetMob.LoadFromDatabase(GameServer.Database.FindObjectByKey<Mob>(targetMob.InternalID));
					targetMob.AddToWorld();
					client.Player.Out.SendMessage(targetMob.Name + " reloaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Player.Out.SendMessage(targetMob.Name + " is loaded from a script and can't be reloaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}


		/// <summary>
		/// Loads a mob from the database
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		private void load(GameClient client, string[] args)
		{
			if (args.Length > 2)
			{
				Mob mob = GameServer.Database.FindObjectByKey<Mob>(args[2]);
				if (mob != null)
				{
					Log.DebugFormat("Mob_ID {0} loaded from database.", args[2]);
					DisplayMessage(client, "Mob_ID {0} loaded from database.", args[2]);
					GameNPC npc = new GameNPC();
					npc.LoadFromDatabase(mob);
					npc.AddToWorld();
					DisplayMessage(client, "Mob {0} added to the world.  Use care if this is a duplicate, removing original will delete mob from the DB!", args[2]);
				}
				else
				{
					Log.DebugFormat("Mob_ID {0} not found.", args[2]);
					DisplayMessage(client, "Mob_ID {0} not found.", args[2]);
				}
			}
		}


		private void findname(GameClient client, string[] args)
		{
			if (args.Length < 3)
				return;

			int maxreturn;
			try
			{
				maxreturn = Convert.ToInt32(args[3]);
				maxreturn = Math.Min(60, maxreturn); //Too much would probably packet overflow anyway.
			}
			catch (Exception)
			{
				maxreturn = 10;
			}

			var mobs = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", string.Format("%{0}%", args[2]))).OrderByDescending(m => m.Level).Take(maxreturn).ToArray();
			if (mobs != null && mobs.Length > 0)
			{
				string mnames = "Found : \n";
				for (int i = 0; i < mobs.Length; i++)
				{
					mnames = mnames + mobs[i].Name + "\n";
				}
				client.Player.Out.SendMessage(mnames, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Player.Out.SendMessage("No matches found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private void state(GameClient client, GameNPC targetMob)
		{

			if (targetMob == null)
			{
				client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			var text = new List<string>();

			if (targetMob.Brain != null && targetMob.Brain.IsActive)
			{
				text.Add(targetMob.Brain.GetType().FullName);
				text.Add(targetMob.Brain.ToString());
				text.Add("");
			}

			if (targetMob.IsReturningHome || targetMob.IsReturningToSpawnPoint)
			{
				text.Add("IsReturningHome: " + targetMob.IsReturningHome);
				text.Add("IsReturningToSpawnPoint: " + targetMob.IsReturningToSpawnPoint);
				text.Add("");
			}

			text.Add("InCombat: " + targetMob.InCombat);
			text.Add("AttackState: " + targetMob.AttackState);
			text.Add("LastCombatPVE: " + targetMob.LastAttackedByEnemyTickPvE);
			text.Add("LastCombatPVP: " + targetMob.LastAttackedByEnemyTickPvP);

			if (targetMob.InCombat || targetMob.AttackState)
			{
				text.Add("RegionTick: " + targetMob.CurrentRegion.Time);
			}

			text.Add("");

			if (targetMob.TargetObject != null)
			{
				text.Add("TargetObject: " + targetMob.TargetObject.Name);
				text.Add("InView: " + targetMob.TargetInView);
			}

			if (targetMob.Brain != null && targetMob.Brain is StandardMobBrain)
			{
				Dictionary<GameLiving, long> aggroList = (targetMob.Brain as StandardMobBrain).AggroTable;

				if (aggroList.Count > 0)
				{
					text.Add("");
					text.Add("Aggro List:");

					foreach (GameLiving living in aggroList.Keys)
					{
						text.Add(living.Name + ": " + aggroList[living]);
					}
				}
			}

			if (targetMob.Attackers != null && targetMob.Attackers.Count > 0)
			{
				text.Add("");
				text.Add("Attacker List:");

				foreach (GameLiving attacker in targetMob.Attackers)
				{
					text.Add(attacker.Name);
				}
			}

			if (targetMob.EffectList.Count > 0)
			{
				text.Add("");
				text.Add("Effect List:");

				foreach (IGameEffect effect in targetMob.EffectList)
				{
					text.Add(effect.Name + " remaining " + effect.RemainingTime);
				}
			}

			client.Out.SendCustomTextWindow("Mob State", text);
		}

		private void trigger(GameClient client, GameNPC targetMob, string[] args)
		{
			string text = "";
			ushort emote = 0;
			ushort chance = 0;
			try
			{
				string type = args[2].ToLower();

				switch (type)
				{
					case "":
					case "help":
						trigger_help(client);
						return;

					case "info":
						trigger_info(client, targetMob);
						return;

					case "remove":
						trigger_remove(client, targetMob, args);
						return;
				}

				GameNPC.eAmbientTrigger trig;
				try
				{
					trig = (GameNPC.eAmbientTrigger)Enum.Parse(typeof(GameNPC.eAmbientTrigger), type, true);
				}
				catch
				{
					client.Out.SendMessage("You must specify a proper type for the trigger <spawning, aggroing, dieing, fighting, moving, roaming, seeing, interact>.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("example: dieing none This is what I will say when I die!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				try
				{
					emote = Convert.ToUInt16((args[4]));
					chance = Convert.ToUInt16((args[3]));
				}
				catch
				{
					client.Out.SendMessage("You must specify a valid chance percent/emote number", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				if (args.Length > 4)
				{
					text = String.Join(" ", args, 5, args.Length - 5);
				}

				if (text == "")
				{
					client.Out.SendMessage("You must specify some text for the trigger.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				string voice = string.Empty;
				if (text.Contains("{b}"))
					voice = "b";
				if (text.Contains("{y}"))
					voice = "y";
				text = text.Replace("{b}", string.Empty).Replace("{y}", string.Empty);
				GameServer.Database.AddObject(new MobXAmbientBehaviour(targetMob.Name, trig.ToString(), emote, text, chance, voice) {Dirty = true, AllowAdd = true});
				client.Out.SendMessage(" Trigger added to mobs with name " + targetMob.Name + " when they " + type + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void trigger_remove(GameClient client, GameNPC targetMob, string[] args)
		{
			int i;
			if (args.Length < 4 || !int.TryParse(args[3], out i))
			{
				DisplaySyntax(client);
				return;
			}
			var triggers = client.Player.TempProperties.getProperty<IList<MobXAmbientBehaviour>>("mob_triggers", null);
			if (triggers == null)
			{
				ChatUtil.SendSystemMessage(client, "You must use '/mob trigger info' before using this command !");
				return;
			}
			var trigger = triggers[i - 1];
			GameServer.Database.DeleteObject(trigger);
			ChatUtil.SendSystemMessage(client, "Trigger: \"" + trigger.Trigger + ", chance: " + trigger.Chance + ", voice: " + trigger.Voice + ", emote: " + trigger.Emote + ", text: " + trigger.Text + "\" has been removed.");
		}

		private void trigger_info(GameClient client, GameNPC targetMob)
		{
			var triggers = GameServer.Instance.NpcManager.AmbientBehaviour[targetMob.Name];
			client.Player.TempProperties.setProperty("mob_triggers", triggers);
			ChatUtil.SendSystemMessage(client, targetMob.Name + "'s triggers:");
			var i = 0;
			foreach (var trigger in triggers)
				ChatUtil.SendSystemMessage(client, ++i + ". " + trigger.Trigger + ", chance: " + trigger.Chance + ", voice: " + trigger.Voice + ", emote: " + trigger.Emote + ", text: " + trigger.Text);
		}

		private void trigger_help(GameClient client)
		{
			client.Out.SendMessage("The trigger command lets you add sentences for the mob to say when he spawns, aggros, fights or dies.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Triggers apply to all mobs with the same mob name.  Each mob name can have multiple triggers and trigger types.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("The aggro and die trigger types allow for keywords of {targetname} (gets replaced with the target name), {class} {race} (gets replaced with the players class/race) and {sourcename} gets replaced with the source name and {controller}, the pet's controller.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Ex: /mob trigger aggro 50 10 This is what I'll say 50% time when I aggro!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Ex: /mob trigger die 100 5 This is what I'll say when I die!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Ex: /mob trigger aggro 100 0 {y}I really hate {class}'s like you!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Ex: /mob trigger roam 5 12 Prepare to die {targetname}!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Ex: /mob trigger aggro 5 0 {b}I've been waiting for this moment ever since I was a young {sourcename}!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Usage: '/mob trigger <type(die,aggro,spawn,fight,kill,roam)> <chance in percent> <emote (0 for no emote)> <sentence(can include {targetname},{sourcename},{class},{race}, {controller})(can also be formatted with {y} for yelled and {b} for broadcasted sentence)>'", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Usage: '/mob trigger info' Give trigger informations", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			client.Out.SendMessage("Usage: '/mob trigger remove <id>' Remove a trigger", eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}
	}
}
