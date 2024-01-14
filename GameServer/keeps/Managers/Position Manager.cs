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
using System.Linq;
using System.Collections;

using DOL.Database;
using DOL.GS;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using System.Collections.Generic;
using DOL.GS.Geometry;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class to manage the guards Positions
	/// </summary>
	public class PositionMgr
	{
		/// <summary>
		/// Gets the most usable position directly from the database
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition GetUsablePosition(GameKeepGuard guard)
		{
			var filterClassType = DB.Column(nameof(DBKeepPosition.ClassType)).IsNotEqualTo("DOL.GS.Keeps.Banner");
			var filterTemplateID = DB.Column(nameof(DBKeepPosition.TemplateID)).IsEqualTo(guard.TemplateID);
			var filterComponentSkin = DB.Column(nameof(DBKeepPosition.ComponentSkin)).IsEqualTo(guard.Component.Skin);
			var filterHeight = DB.Column(nameof(DBKeepPosition.Height)).IsLessOrEqualTo(guard.Component.Height);
			return DOLDB<DBKeepPosition>.SelectObjects(filterClassType.And(filterTemplateID).And(filterComponentSkin).And(filterHeight))
				.OrderByDescending(it => it.Height).FirstOrDefault();
		}

		/// <summary>
		/// Gets the most usuable position for a banner directly from the database
		/// </summary>
		/// <param name="b">The banner object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition GetUsablePosition(GameKeepBanner b)
		{
			var filterClassType = DB.Column(nameof( DBKeepPosition.ClassType ) ).IsNotEqualTo("DOL.GS.Keeps.Banner");
			var filterTemplateID = DB.Column(nameof(DBKeepPosition.TemplateID)).IsEqualTo(b.TemplateID);
			var filterComponentSkin = DB.Column(nameof(DBKeepPosition.ComponentSkin)).IsEqualTo(b.Component.Skin);
			var filterHeight = DB.Column(nameof(DBKeepPosition.Height)).IsLessOrEqualTo(b.Component.Height);
			return DOLDB<DBKeepPosition>.SelectObjects(filterClassType.And(filterTemplateID).And(filterComponentSkin).And(filterHeight))
				.OrderByDescending(it => it.Height).FirstOrDefault();
		}

		/// <summary>
		/// Gets the position at the exact entry from the database
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition GetPosition(GameKeepGuard guard)
		{
			var filterTemplateID = DB.Column(nameof(DBKeepPosition.TemplateID)).IsEqualTo(guard.TemplateID);
			var filterComponentSkin = DB.Column(nameof(DBKeepPosition.ComponentSkin)).IsEqualTo(guard.Component.Skin);
			var filterHeight = DB.Column(nameof(DBKeepPosition.Height)).IsLessOrEqualTo(guard.Component.Height);
			return DOLDB<DBKeepPosition>.SelectObject(filterTemplateID.And(filterComponentSkin).And(filterHeight));
		}


		public static void LoadGuardPosition(DBKeepPosition pos, GameKeepGuard guard)
		{
			LoadKeepItemPosition(pos, guard);

            guard.SpawnPosition = guard.Position;
		}

        public static void LoadKeepItemPosition(DBKeepPosition pos, IKeepItem item)
        {
            var keepPositionOffset = Vector.Create(pos.XOff, -pos.YOff, pos.ZOff).RotatedClockwise(item.Component.Orientation);
            var orientation = item.Component.Orientation + Angle.Heading(pos.HOff);
            item.Position = (item.Component.Position + keepPositionOffset).With(orientation);
            item.DbKeepPosition = pos;
        }

        public static Vector SaveXY(GameKeepComponent component, Coordinate keepPointCoordinate)
        {
            var angle = component.Keep.Orientation + component.RelativeOrientationToKeep;
            var vector = keepPointCoordinate - component.Coordinate;
            return vector.RotatedClockwise(angle - Angle.Degrees(90));
        }

		public static DBKeepPosition CreatePosition(Type type, int height, GamePlayer player, string guardID, GameKeepComponent component)
		{
			DBKeepPosition pos = CreatePosition(guardID, component, player);
			pos.Height = height;
			pos.ClassType = type.ToString();
			GameServer.Database.AddObject(pos);
			return pos;
		}

		/// <summary>
		/// Creates a guard patrol position
		/// </summary>
		/// <param name="guardID">The guard ID</param>
		/// <param name="component">The component object</param>
		/// <param name="player">The player object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition CreatePatrolPosition(string guardID, GameKeepComponent component, GamePlayer player, AbstractGameKeep.eKeepType keepType)
		{
			DBKeepPosition pos = CreatePosition(guardID, component, player);
			pos.Height = 0;
			pos.ClassType = "DOL.GS.Keeps.Patrol";
			pos.KeepType = (int)keepType;
			GameServer.Database.AddObject(pos);
			return pos;
		}

		/// <summary>
		/// Creates a position
		/// </summary>
		/// <param name="templateID">The template ID</param>
		/// <param name="component">The component object</param>
		/// <param name="player">The creating player object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition CreatePosition(string templateID, GameKeepComponent component, GamePlayer player)
		{
			DBKeepPosition pos = new DBKeepPosition();
			pos.ComponentSkin = component.Skin;
			pos.ComponentRotation = component.ComponentHeading;
			pos.TemplateID = templateID;

			var keepPositionOffset = SaveXY(component, player.Coordinate);
			pos.XOff = keepPositionOffset.X;
			pos.YOff = keepPositionOffset.Y;
			pos.ZOff = keepPositionOffset.Z;

			pos.HOff = (player.Orientation - component.Orientation).InHeading;
			return pos;
		}

		public static void AddPosition(DBKeepPosition position)
		{
			foreach (AbstractGameKeep keep in GameServer.KeepManager.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					DBKeepPosition[] list = component.Positions[position.TemplateID] as DBKeepPosition[];
					if (list == null)
					{
						list = new DBKeepPosition[4];
						component.Positions[position.TemplateID] = list;
					}
					//list.SetValue(position, position.Height);
					list[position.Height] = position;
				}
			}
		}

		public static void RemovePosition(DBKeepPosition position)
		{
			foreach (AbstractGameKeep keep in GameServer.KeepManager.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					DBKeepPosition[] list = component.Positions[position.TemplateID] as DBKeepPosition[];
					if (list == null)
					{
						list = new DBKeepPosition[4];
						component.Positions[position.TemplateID] = list;
					}
					//list.SetValue(position, position.Height);
					list[position.Height] = null;
				}
			}
			GameServer.Database.DeleteObject(position);
		}

		public static void FillPositions()
		{
			foreach (AbstractGameKeep keep in GameServer.KeepManager.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					component.LoadPositions();
					component.FillPositions();
				}
			}
		}

		/// <summary>
		/// Method to retrieve the Patrol Path from the Patrol ID and Component
		/// 
		/// We need this because we store this all using our offset system
		/// </summary>
		/// <param name="pathID">The path ID, which is the Patrol ID</param>
		/// <param name="component">The Component object</param>
		/// <returns>The Patrol path</returns>
		public static PathPoint LoadPatrolPath(string pathID, GameKeepComponent component)
		{
			SortedList sorted = new SortedList();
			pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
			var dbpath = DOLDB<DBPath>.SelectObject(DB.Column(nameof(DBPath.PathID)).IsEqualTo(pathID));
			IList<DBPathPoint> pathpoints = null;
			ePathType pathType = ePathType.Once;

			if (dbpath != null)
			{
				pathType = (ePathType)dbpath.PathType;
			}
			if (pathpoints == null)
			{
				pathpoints = DOLDB<DBPathPoint>.SelectObjects(DB.Column(nameof(DBPathPoint.PathID)).IsEqualTo(pathID));
			}

			foreach (DBPathPoint point in pathpoints)
			{
				sorted.Add(point.Step, point);
			}
			PathPoint prev = null;
			PathPoint first = null;
			for (int i = 0; i < sorted.Count; i++)
			{
				var dbPathPoint = (DBPathPoint)sorted.GetByIndex(i);
				var pathPoint = new PathPoint(dbPathPoint, pathType);
                var relativeOffset = Vector.Create(pathPoint.Coordinate.X, -pathPoint.Coordinate.Y, pathPoint.Coordinate.Z)
                    .RotatedClockwise(component.Orientation);
                pathPoint.Coordinate = component.Coordinate + relativeOffset;

				if (first == null)
				{
					first = pathPoint;
				}
				pathPoint.Prev = prev;
				if (prev != null)
				{
					prev.Next = pathPoint;
				}
				prev = pathPoint;
			}
			return first;
		}

		/// <summary>
        /// Method to save the Patrol Path using the Patrol ID and the Component
		/// </summary>
		/// <param name="pathID"></param>
		/// <param name="path"></param>
		/// <param name="component"></param>
		public static void SavePatrolPath(string pathID, PathPoint path, GameKeepComponent component)
		{
			if (path == null)
				return;

			pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
			GameServer.Database.DeleteObject(DOLDB<DBPath>.SelectObjects(DB.Column(nameof(DBPath.PathID)).IsEqualTo(pathID)));
			PathPoint root = MovementMgr.FindFirstPathPoint(path);

			//Set the current pathpoint to the rootpoint!
			path = root;
			DBPath dbp = new DBPath(pathID, ePathType.Loop);
			GameServer.Database.AddObject(dbp);

			int i = 1;
			do
			{
                var dbPathPoint = path.GenerateDbEntry();
                var offset = SaveXY(component, Coordinate.Create(dbPathPoint.X, dbPathPoint.Y));
                dbPathPoint.X = offset.X;
                dbPathPoint.Y = offset.Y;
                dbPathPoint.Z = offset.Z;
                dbPathPoint.Step = i++;
                dbPathPoint.PathID = pathID;

				GameServer.Database.AddObject(dbPathPoint);
				path = path.Next;
			} while (path != null && path != root);
		}

		public static void CreateDoor(int doorID, GamePlayer player)
		{
			int ownerKeepId = (doorID / 100000) % 1000;
			int towerNum = (doorID / 10000) % 10;
			int keepID = ownerKeepId + towerNum * 256;
			int componentID = (doorID / 100) % 100;
			int doorIndex = doorID % 10;

			AbstractGameKeep keep = GameServer.KeepManager.GetKeepByID(keepID);
			if (keep == null)
			{
				player.Out.SendMessage("Cannot create door as keep is null!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			GameKeepComponent component = null;
			foreach (GameKeepComponent c in keep.KeepComponents)
			{
				if (c.ID == componentID)
				{
					component = c;
					break;
				}
			}
			if (component == null)
			{
				player.Out.SendMessage("Cannot create door as component is null!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			DBKeepPosition pos = new DBKeepPosition();
			pos.ClassType = "DOL.GS.Keeps.GameKeepDoor";
			pos.TemplateType = doorIndex;
			pos.ComponentSkin = component.Skin;
			pos.ComponentRotation = component.ComponentHeading;
			pos.TemplateID = Guid.NewGuid().ToString();

			var keepPositionOffset = SaveXY(component, player.Coordinate);
			pos.XOff = keepPositionOffset.X;
			pos.YOff = keepPositionOffset.Y;
			pos.ZOff = keepPositionOffset.Z;

			pos.HOff = (player.Orientation - component.Orientation).InHeading;

			GameServer.Database.AddObject(pos);

			player.Out.SendMessage("Added door as a position to keep.  A server restart will be required to load this position.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}
