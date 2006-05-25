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
using DOL.Database;
using DOL.Events;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&path",
		(uint) ePrivLevel.GM,
		"There are several path functions",
		"/path create <path type> - creates a temp path in ram",
		"/path load <pathID> - loads a path from db",
		"/path add [speedlimit] - adds a point at the end of the current path",
		"/path close - the next point will be the first (not avalable with TripPath)",
		"/path travel - makes a target mob travel the current path",
		"/path save - saves a path to db",
		"/path steed <model> <name> - set a model and name to the steed (only avalable with TripPath)",
		"/path hide - removes the temporary objects shown when a path is loaded or created")]
	public class PathCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		protected string TEMP_PATH = "TEMP_PATH";
		protected string TEMP_PATH_LAST = "TEMP_PATH_LAST";
		protected string TEMP_PATH_OBJS = "TEMP_PATH_OBJS";

		private void CreateTempPathObject(GameClient client, PathPoint pp, String name)
		{
			//Create a new object
			GameStaticItem pathObj = new GameStaticItem();
			pathObj.Position = pp.Position;
			pathObj.Region = client.Player.Region;
			pathObj.Heading = client.Player.Heading;
			pathObj.Name = name;
			pathObj.Model = 488;
			pathObj.AddToWorld();

			ArrayList objs = (ArrayList) client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			
			if (objs == null) objs = new ArrayList();
			objs.Add(pathObj);

			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, objs);
		}

		private void RemoveAllTempPathObjects(GameClient client)
		{
			ArrayList objs = (ArrayList) client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;

			foreach (GameStaticItem obj in objs) obj.RemoveFromWorld();
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, null);
		}

		private int PathCreate(GameClient client, string[] args)
		{
			//Remove old temp objects
			RemoveAllTempPathObjects(client);

			if (args.Length < 3)
			{
				DisplayError(client, "Usage: /path create <path type>");
				return 0;
			}

			Path newPath = Assembly.GetAssembly(typeof (GameServer)).CreateInstance(args[2], false) as Path;
			if(newPath == null)
			{
				DisplayError(client, "Type '{0}' is not a valid path type!", args[2]);
				return 0;
			}

			newPath.Region = client.Player.Region;
			newPath.StartingPoint = new PathPoint();
			newPath.StartingPoint.Position = client.Player.Position;
			newPath.StartingPoint.Speed = 0;

			client.Player.TempProperties.setProperty(TEMP_PATH, newPath);
			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, newPath.StartingPoint);
			
			client.Player.Out.SendMessage("Path creation started! You can add new pathpoints via /path add now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			CreateTempPathObject(client, newPath.StartingPoint, "TMP PP START");

			return 1;
		}

		private int PathAdd(GameClient client, string[] args)
		{
			PathPoint oldPathPoint = (PathPoint) client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (oldPathPoint == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			if(oldPathPoint.NextPoint != null)
			{
				DisplayError(client, "This path is already closed!");
				return 0;
			}

			int speedlimit = 1000;
			if (args.Length > 2)
			{
				try
				{
					speedlimit = int.Parse(args[2]);
				}
				catch
				{
					DisplayError(client, "No valid speedlimit '{0}'!", args[2]);
					return 0;
				}
			}

			PathPoint newPathPoint = new PathPoint();
			newPathPoint.Position = client.Player.Position;
			newPathPoint.Speed = speedlimit;
			
			oldPathPoint.NextPoint = newPathPoint;
			

			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, newPathPoint);

			CreateTempPathObject(client, newPathPoint, "TMP PP");
			DisplayMessage(client, "Pathpoint added.");
			return 1;
		}

		private int PathTravel(GameClient client)
		{
			Path path = (Path) client.Player.TempProperties.getObjectProperty(TEMP_PATH, null);
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			GameSteed steed = new GameSteed();
			steed.Region = path.Region;
			steed.Position = path.StartingPoint.Position;
			steed.Heading = steed.Position.GetHeadingTo(path.StartingPoint.NextPoint.Position);
			steed.Name = "Path Tester Mob";
			if(path is TripPath) steed.Model = ((TripPath)path).SteedModel;
			else steed.Model = 450;
			steed.Realm = client.Player.Realm;
			steed.Level = 1;
			steed.Size = 50;
			steed.AddToWorld();

			client.Player.MountSteed(steed);
						
			GameEventMgr.AddHandler(steed, GameSteedEvent.PathMoveEnds, new DOLEventHandler(OnSteedAtRouteEnd));					
						
			steed.MoveOnPath(path.StartingPoint.NextPoint);

			return 1;			
		}

		public void OnSteedAtRouteEnd(DOLEvent e, object o, EventArgs args)
		{
			GameSteed steed = o as GameSteed;
			if (steed == null) return;

			GameEventMgr.RemoveHandler(steed, GameSteedEvent.PathMoveEnds, new DOLEventHandler(OnSteedAtRouteEnd));
		
			if(steed.Rider != null)
			{
				steed.Rider.DismountSteed();
			}
			steed.RemoveFromWorld();
		}

		private int PathLoad(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplayError(client, "Usage: /path load <pathID>");
				return 0;
			}

			Path newPath = GameServer.Database.FindObjectByKey(typeof(Path), args[2]) as Path;
			if(newPath == null)
			{
				DisplayError(client, "Path with id '{0}' not found!", args[2]);
				return 0;
			}

			DisplayMessage(client, "Path '{0}' loaded. Type : '{1}'", args[2], newPath.GetType().FullName);
			
			RemoveAllTempPathObjects(client);
			
			client.Player.TempProperties.setProperty(TEMP_PATH, newPath);
			PathPoint startingPoint = newPath.StartingPoint;
			CreateTempPathObject(client, startingPoint, "TMP PP START");
			
			while (startingPoint.NextPoint != null)
			{
				CreateTempPathObject(client, startingPoint.NextPoint, "TMP PP");
				startingPoint = startingPoint.NextPoint;
			}

			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, startingPoint);
			return 1;
		}
		
		private int PathSave(GameClient client)
		{
			Path path = (Path) client.Player.TempProperties.getObjectProperty(TEMP_PATH, null);
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			TripPath trip = path as TripPath;
			if(trip != null && (trip.SteedModel == 0 || trip.SteedName == ""))
			{
				DisplayMessage(client, "You must define a steed model and a steed name before save this TripPath!", path.PathID);
				return 1;
			}

			GameServer.Database.AddNewObject(path);
			DisplayMessage(client, "Path saved as '{0}'", path.PathID);
			return 1;
		}

		private int PathClose(GameClient client)
		{
			Path path = (Path) client.Player.TempProperties.getObjectProperty(TEMP_PATH, null);
			PathPoint oldPathPoint = (PathPoint) client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			
			if (path == null || oldPathPoint == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			if(path is TripPath)
			{
				DisplayError(client, "You can't close a TripPath !");
				return 0;
			}

			if(oldPathPoint.NextPoint != null)
			{
				DisplayError(client, "This path is already closed!");
				return 0;
			}

			oldPathPoint.NextPoint = path.StartingPoint;
			
			DisplayMessage(client, "Pathpoint closed.");
			return 1;
		}

		private int PathSetSteed(GameClient client, string[] args)
		{
			TripPath path = client.Player.TempProperties.getObjectProperty(TEMP_PATH, null) as TripPath;
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			if (args.Length < 4)
			{
				DisplayError(client, "Usage: /path steed <model> <name>");
				return 0;
			}

			path.SteedModel = int.Parse(args[2]);
			path.SteedName = args[3];

			DisplayMessage(client, "Steed model set '{0}' and name set '{1}'", path.SteedModel, path.SteedName);
			return 1;
		}


		public int OnCommand(GameClient client, string[] args)
		{
			if(args.Length < 2)
			{
				DisplaySyntax(client);
				return 0;
			}
			switch (args[1])
			{
				case "create": return PathCreate(client, args);
				case "add": return PathAdd(client, args);
				case "close": return PathClose(client);
				case "travel": return PathTravel(client);
				case "save": return PathSave(client);
				case "load": return PathLoad(client, args);
				case "steed": return PathSetSteed(client, args);
				case "hide": RemoveAllTempPathObjects(client); return 1;
			}
			DisplaySyntax(client);
			return 0;
		}
	}
}