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
using System.Reflection;
using DOL.Database2;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	   "&faction",
	   ePrivLevel.GM,
	   "create a faction and assign friend and enemy faction",
	   "'/faction create <name> <base aggro level>' to create a faction",
	   "'/faction assign' to assign the faction to the target mob",
	   "'/faction addfriend <factionid>' to add faction friend to current faction",
	   "'/faction addenemy  <factionid>' to add enemy to the current faction",
	   "'/faction list' to have a list of all faction",
	   "'/faction select <factionid>' to select the faction with this id")]
	public class FactionCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string TEMP_FACTION_LAST = "TEMP_FACTION_LAST";

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
			Faction myfaction = (Faction)client.Player.TempProperties.getObjectProperty(TEMP_FACTION_LAST, null);
			switch (args[1])
			{
				case "create":
					{
						if (args.Length < 4)
						{
							DisplaySyntax(client);
							return;
						}
						string name = args[2];
						int baseAggro = 0;
						try
						{
							baseAggro = Convert.ToInt32(args[3]);
						}
						catch
						{
							client.Player.Out.SendMessage("The baseAggro must be a number.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						int max = 0;
						log.Info("count:" + FactionMgr.Factions.Count.ToString());
						if (FactionMgr.Factions.Count != 0)
						{
							log.Info("count >0");
							IEnumerator enumerator = FactionMgr.Factions.Keys.GetEnumerator();
							while (enumerator.MoveNext())
							{
								log.Info("max :" + max + " et current :" + (int)enumerator.Current);
								max = System.Math.Max(max, (int)enumerator.Current);
							}
						}
						log.Info("max :" + max);
						DBFaction dbfaction = new DBFaction();
						dbfaction.BaseAggroLevel = baseAggro;
						dbfaction.Name = name;
						dbfaction.ID = (max + 1);
						log.Info("add obj to db with id :" + dbfaction.ID);
						GameServer.Database.AddNewObject(dbfaction);
						log.Info("add obj to db");
						myfaction = new Faction();
						myfaction.LoadFromDatabase(dbfaction);
						FactionMgr.Factions.Add(dbfaction.ID, myfaction);
						client.Player.TempProperties.setProperty(TEMP_FACTION_LAST, myfaction);
						client.Player.Out.SendMessage("New faction created", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					} break;
				case "assign":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage("You must select a faction first.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						GameNPC npc = client.Player.TargetObject as GameNPC;
						if (npc == null)
						{
							client.Player.Out.SendMessage("You must select a mob first.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						npc.Faction = myfaction;
						client.Player.Out.SendMessage("The mob " + npc.Name + " has joined the faction of " + myfaction.Name + ".", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					} break;
				case "addfriend":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage("You must select a faction first.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage("The index must be a number.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage("This Faction is not loaded", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.ID;
						dblinkedfaction.LinkedFactionID = linkedfaction.ID;
						dblinkedfaction.IsFriend = true;
						GameServer.Database.AddNewObject(dblinkedfaction);
						myfaction.AddFriendFaction(linkedfaction);
					} break;
				case "addenemy":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage("You must select a faction first.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage("The index must be a number.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage("This Faction is not loaded", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.ID;
						dblinkedfaction.LinkedFactionID = linkedfaction.ID;
						dblinkedfaction.IsFriend = false;
						GameServer.Database.AddNewObject(dblinkedfaction);
						myfaction.AddEnemyFaction(linkedfaction);
					} break;
				case "list":
					{
						foreach (Faction faction in FactionMgr.Factions.Values)
							client.Player.Out.SendMessage("#" + faction.ID.ToString() + ": " + faction.Name + " (" + faction.BaseAggroLevel + ")", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
						return;
					}
				case "select":
					{
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage("The index must be a number.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction tempfaction = FactionMgr.GetFactionByID(id);
						if (tempfaction == null)
						{
							client.Player.Out.SendMessage("This Faction is not loaded", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						client.Player.TempProperties.setProperty(TEMP_FACTION_LAST, tempfaction);
					} break;
				default:
					{
						DisplaySyntax(client);
						return;
					}
			}
		}
	}
}