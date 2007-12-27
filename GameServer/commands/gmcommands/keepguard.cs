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

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Movement;

namespace DOL.GS.Commands
{
	/// <summary>
	/// Various keep guard commands
	/// </summary>
	[CmdAttribute(
		"&keepguard", //command to handle
		ePrivLevel.GM, //minimum privelege level
		"Various keep guard commands!", //command description
		"'/keepguard create <lord/fighter/archer/healer/stealther/caster> (optional for archer and caster)static'",
		/*
		"'/keepguard fastcreate <type>' to create a guard for the keep with base template",
		"'/keepguard fastcreate ' to show all template available in fast create",
		"'/keepguard create ' to create a guard for the closed keep ",
		"'/keepguard create lord' to create the lord for the closed keep ",
		"'/keepguard keep <keepID>' to assign guard to keep",
		"'/keepguard keep ' to assign guard to the nearest keep",
		"'/keepguard equipment <equipmentid>' to put equipment on guard",
		"'/keepguard level <level>' to change base level of guard",
		"'/keepguard save' to save guard into DB",
		 */
		"Use '/mob' command if you want to change other param of guard")] //usage
	public class KeepGuardCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// The command handler itself
		/// </summary>
		/// <param name="client">The client using the command</param>
		/// <param name="args">The command arguments</param>
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1])
			{
				case "create":
					{
						GameKeepGuard guard = null;
						switch (args[2])
						{
							case "lord":
								{
									guard = new GuardLord();
									break;
								}
							case "fighter":
								{
									guard = new GuardFighter();
									break;
								}
							case "archer":
								{
									if (args.Length > 3)
										guard = new GuardStaticArcher();
									else guard = new GuardArcher();
									break;
								}
							case "healer":
								{
									guard = new GuardHealer();
									break;
								}
							case "stealther":
								{
									guard = new GuardStealther();
									break;
								}
							case "caster":
								{
									if (args.Length > 3)
										guard = new GuardStaticCaster();
									else guard = new GuardCaster();
									break;
								}
							case "hastener":
								{
									guard = new FrontierHastener();
									break;
								}
							case "mission":
								{
									guard = new MissionMaster();
									break;
								}
							case "patrol":
								{
									if (client.Player.TargetObject is GameKeepComponent == false)
									{
										DisplayMessage(client, "You need to target a keep component to create a patrol!", new object[] { });
										return;
									}
									GameKeepComponent c = client.Player.TargetObject as GameKeepComponent;;
									Patrol p = new Patrol(c);
									p.SpawnPosition = PositionMgr.CreatePatrolPosition(p.PatrolID, c, client.Player);
									p.PatrolID = p.SpawnPosition.TemplateID;
									p.InitialiseGuards();
									return;
								}
						}
						if (guard == null)
						{
							DisplaySyntax(client);
							return;
						}

						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component != null)
						{
							DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), component.Height, client.Player, Guid.NewGuid().ToString(), component);
							PositionMgr.AddPosition(pos);
							PositionMgr.FillPositions();
						}
						else
						{
							guard.CurrentRegion = client.Player.CurrentRegion;
							guard.X = client.Player.X;
							guard.Y = client.Player.Y;
							guard.Z = client.Player.Z;
							guard.Heading = client.Player.Heading;
							guard.Realm = (byte)guard.CurrentZone.GetRealm();
							guard.SaveIntoDatabase();
							foreach (AbstractArea area in guard.CurrentAreas)
							{
								if (area is KeepArea)
								{
									AbstractGameKeep keep = (area as KeepArea).Keep;
									guard.Component = new GameKeepComponent();
									guard.Component.Keep = keep;
									guard.Component.Keep.Guards.Add(guard.InternalID, guard);
									break;
								}
							}
							TemplateMgr.RefreshTemplate(guard);
							guard.AddToWorld();
						}

						DisplayMessage(client, "Guard added!", new object[] { });
						break;
					}
				case "addposition":
					{
						if (client.Player.TargetObject is GameKeepGuard == false)
						{
							DisplayMessage(client, "Target a guard first!", new object[] { });
							return;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return;
						}
						byte height = byte.Parse(args[2]);
						height = KeepMgr.GetHeightFromLevel(height);
						if (height > 3)
						{
							DisplayMessage(client, "Keep levels range from 0 to 10", new object[] { });
							return;
						}

						GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
						if (PositionMgr.GetPosition(guard) != null)
						{
							DisplayMessage(client, "You already have a position assigned for height " + height + ", remove first!", new object[] { });
							return;
						}

						DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), height, client.Player, guard.TemplateID, guard.Component);

						PositionMgr.AddPosition(pos);
						PositionMgr.FillPositions();

						DisplayMessage(client, "Guard position added");
						break;
					}
				case "removeposition":
					{
						if (client.Player.TargetObject is GameKeepGuard == false)
						{
							DisplayMessage(client, "Target a Guard first");
							return;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return;
						}

						GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;

						DBKeepPosition pos = guard.Position;

						PositionMgr.RemovePosition(pos);

						PositionMgr.FillPositions();

						DisplayMessage(client, "Guard position removed", new object[] { });
						break;
					}
				case "path":
					{
						switch (args[2].ToLower())
						{
							case "create":
								{
									//Remove old temp objects
									RemoveAllTempPathObjects(client);

									PathPoint startpoint = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, 100000, ePathType.Once);
									client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, startpoint);
									client.Player.TempProperties.setProperty(TEMP_PATH_LAST, startpoint);
									client.Player.Out.SendMessage("Path creation started! You can add new pathpoints via /keepguard path add now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									CreateTempPathObject(client, startpoint, "TMP PP 1");
									break;
								}
							case "add":
								{
									PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
									if (path == null)
									{
										DisplayMessage(client, "No path created yet! Use /keepguard path create first!");
										return;
									}

									int speedlimit = 1000;
									if (args.Length > 3)
									{
										try
										{
											speedlimit = int.Parse(args[3]);
										}
										catch
										{
											DisplayMessage(client, "No valid speedlimit '{0}'!", args[2]);
											return;
										}
									}
									PathPoint newpp = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, speedlimit, path.Type);
									path.Next = newpp;
									newpp.Prev = path;
									client.Player.TempProperties.setProperty(TEMP_PATH_LAST, newpp);

									int len = 0;
									while (path.Prev != null)
									{
										len++;
										path = path.Prev;
									}
									len += 2;
									CreateTempPathObject(client, newpp, "TMP PP " + len);
									DisplayMessage(client, "Pathpoint added. Current pathlength = {0}", len);
									break;
								}
							case "save":
								{
									PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
									if (args.Length < 3)
									{
										DisplayMessage(client, "Usage: /keepguard path save");
										return;
									}
									if (path == null)
									{
										DisplayMessage(client, "No path created yet! Use /keepguard path create first!");
										return;
									}
									GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
									if (guard == null || guard.PatrolGroup == null)
									{
										DisplayMessage(client, "Target a patrol guard first");
										return;
									}

									path.Type = ePathType.Loop;
									PositionMgr.SavePatrolPath(guard.TemplateID, path, guard.Component);
									DisplayMessage(client, "Path saved");
									RemoveAllTempPathObjects(client);
									break;
								}
						}
						break;
					}
			}
		}

		protected string TEMP_PATH_FIRST = "TEMP_PATH_FIRST";
		protected string TEMP_PATH_LAST = "TEMP_PATH_LAST";
		protected string TEMP_PATH_OBJS = "TEMP_PATH_OBJS";

		private void CreateTempPathObject(GameClient client, PathPoint pp, string name)
		{
			//Create a new object
			GameStaticItem obj = new GameStaticItem();
			//Fill the object variables
			obj.X = pp.X;
			obj.Y = pp.Y;
			obj.Z = pp.Z;
			obj.CurrentRegion = client.Player.CurrentRegion;
			obj.Heading = client.Player.Heading;
			obj.Name = name;
			obj.Model = 488;
			obj.Emblem = 0;
			obj.AddToWorld();

			ArrayList objs = (ArrayList)client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			if (objs == null)
				objs = new ArrayList();
			objs.Add(obj);
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, objs);
		}

		private void RemoveAllTempPathObjects(GameClient client)
		{
			ArrayList objs = (ArrayList)client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;

			foreach (GameStaticItem obj in objs)
				obj.Delete();
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, null);
		}
	}
}