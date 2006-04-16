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
using System.Threading;
using DOL.GS.Collections;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de Zone.
	/// </summary>
	public class Zone
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region constants data

		/// <summary>
		/// Define a subZone size
		/// </summary>
		public const int ZONE_SIZE = 65536;

		/// <summary>
		/// Define how many subZone are created in a zone
		/// </summary>
		public const ushort SUBZONE_NBR = (ushort)(SUBZONE_NBR_ON_ZONE_SIDE * SUBZONE_NBR_ON_ZONE_SIDE);

		/// <summary>
		/// Define how many subZone are in a zone side
		/// </summary>
		public const ushort SUBZONE_NBR_ON_ZONE_SIDE = 32; // MUST BE A POWER OF 2 (current implementation limit is 128 inclusive)

		/// <summary>
		/// Define a subZone length
		/// </summary>
		public const ushort SUBZONE_SIZE = ZONE_SIZE / SUBZONE_NBR_ON_ZONE_SIDE;

		#endregion

		#region Persistant data

		/// <summary>
		/// The ID of the Zone eg. 15
		/// </summary>
		private int m_id;

		/// <summary>
		/// The description of the Zone eg. "Camelot Hills"
		/// </summary>
		private string m_description;

		/// <summary>
		/// The XOffset of this Zone inside the region
		/// </summary>
		private int m_xOffset = 8192;

		/// <summary>
		/// The YOffset of this Zone inside the region
		/// </summary>
		private int m_yOffset = 8192;

		/// <summary>
		/// Holds a pointer to the region that is the parent of this zone
		/// </summary>
		private Region m_region;

		/// <summary>
		/// Returns the ID of this zone
		/// </summary>
		public int ZoneID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Return the description of this zone
		/// </summary>
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		/// <summary>
		/// Returns the XOffset of this Zone
		/// </summary>
		public int XOffset
		{
			get { return m_xOffset; }
			set { m_xOffset = value; }
		}

		/// <summary>
		/// Returns the YOffset of this Zone
		/// </summary>
		public int YOffset
		{
			get { return m_yOffset; }
			set { m_yOffset = value; }
		}

		/// <summary>
		/// Returns the parent region of this zone
		/// </summary>
		public Region Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		#endregion

		#region Runtime data
		/// <summary>
		/// Contains the list of objects per subzone
		/// </summary>
		private readonly SubZone[][] m_subZoneElements = new SubZone[SUBZONE_NBR_ON_ZONE_SIDE][];

		/// <summary>
		/// Stores the amount of calls to GetObjectsInRadius.
		/// </summary>
		private static long m_inRadiusCalls;

		/// <summary>
		/// Gets the amount of calls to GetObjectsInRadius.
		/// </summary>
		public static long InRadiusCalls
		{
			get { return m_inRadiusCalls; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Init all subZone with a empty subZone
		/// </summary>
		public Zone()
		{
			for (int i = 0 ; i < SUBZONE_NBR_ON_ZONE_SIDE ; i++)
			{
				m_subZoneElements[i] = new SubZone[SUBZONE_NBR_ON_ZONE_SIDE]; 

				for (int j = 0 ; j < SUBZONE_NBR_ON_ZONE_SIDE ; j++)
				{
					m_subZoneElements[i][j] = new SubZone();
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Handle a GameObject entering a zone
		/// </summary>
		/// <param name="obj">The GameObject object</param>
		public void ObjectEnterZone(GeometryEngineNode obj)
		{	
			int subZoneXIndex = (obj.Position.X - m_xOffset) / SUBZONE_SIZE;
			int subZoneYIndex = (obj.Position.Y - m_yOffset) / SUBZONE_SIZE;

			SubZone targetSubZone = m_subZoneElements[subZoneXIndex][subZoneYIndex];
			
			targetSubZone.LockObject.AcquireWriterLock(-1); // be sure only one thread write in the subZone at a time
			if(targetSubZone.ElementsList != null)
			{
				targetSubZone.ElementsList = targetSubZone.ElementsList.AddToElementsList(obj);
			}
			else
			{
				targetSubZone.ElementsList = obj;
			}
			targetSubZone.LockObject.ReleaseWriterLock(); // release the writing lock
		}

		/// <summary>
		/// Handle a GameObject exit a zone
		/// </summary>
		/// <param name="obj">The GameObject object</param>
		public void ObjectExitZone(GeometryEngineNode obj)
		{	
			int subZoneXIndex = (obj.Position.X - m_xOffset) / SUBZONE_SIZE;
			int subZoneYIndex = (obj.Position.Y - m_yOffset) / SUBZONE_SIZE;

			SubZone targetSubZone = m_subZoneElements[subZoneXIndex][subZoneYIndex];
			
			targetSubZone.LockObject.AcquireWriterLock(-1); // be sure only one thread write in the subZone at a time
			if(targetSubZone.ElementsList != null)
			{
				targetSubZone.ElementsList = targetSubZone.ElementsList.RemoveFromElementsList(obj);
			}
			targetSubZone.LockObject.ReleaseWriterLock(); // release the writing lock
		}

		/// <summary>
		/// Handle a GameObject exit a zone
		/// </summary>
		/// <param name="obj">The GameObject object</param>
		public void ObjectExitSubZone(GeometryEngineNode obj, SubZone targetSubZone)
		{	
			targetSubZone.LockObject.AcquireWriterLock(-1); // be sure only one thread write in the subZone at a time
			if(targetSubZone.ElementsList != null)
			{
				targetSubZone.ElementsList = targetSubZone.ElementsList.RemoveFromElementsList(obj);
			}
			targetSubZone.LockObject.ReleaseWriterLock(); // release the writing lock
		}

		/// <summary>
		/// Handle a GameObject exit a zone
		/// </summary>
		/// <param name="obj">The GameObject object</param>
		public void ObjectEnterSubZone(GeometryEngineNode obj, SubZone targetSubZone)
		{	
			targetSubZone.LockObject.AcquireWriterLock(-1); // be sure only one thread write in the subZone at a time
			if(targetSubZone.ElementsList != null)
			{
				targetSubZone.ElementsList = targetSubZone.ElementsList.AddToElementsList(obj);
			}
			else
			{
				targetSubZone.ElementsList = obj;
			}
			targetSubZone.LockObject.ReleaseWriterLock(); // release the writing lock
		}

		/// <summary>
		/// Returns the zone that contains the specified point.
		/// </summary>
		/// <param name="pos">global position for the zone you're retrieving</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public SubZone GetSubZone(Point pos)
		{
			int targetSubZoneXIndex = (pos.X - m_xOffset) / SUBZONE_SIZE;
			int targetSubZoneYIndex = (pos.Y - m_yOffset) / SUBZONE_SIZE;

			return m_subZoneElements[targetSubZoneXIndex][targetSubZoneYIndex];
		}

		/// <summary>
		/// Gets the lists of all objects of the given type located in the current Zone
		/// </summary>
		/// <param name="type">the type of objects to look for</param>
		/// <returns>partialList augmented with the new objects verigying both type and radius in the current Zone</returns>
		internal DynamicList GetAllObjects(Type type)
		{
			DynamicList result = new DynamicList();
			for (int currentLine = 0 ; currentLine <= SUBZONE_NBR_ON_ZONE_SIDE - 1 ; ++currentLine)
			{
				for (int currentColumn = 0 ; currentColumn <= SUBZONE_NBR_ON_ZONE_SIDE - 1 ; ++currentColumn)
				{
					SubZone currentSubZone = m_subZoneElements[currentColumn][currentLine];

					currentSubZone.LockObject.AcquireReaderLock(-1); // be sure no thread write in the subZone but stay the others thread read in the subZone

					GeometryEngineNode currentElement = currentSubZone.ElementsList;
					while (currentElement != null)
					{
						if (currentElement.ObjectState == eObjectState.Active && type.IsInstanceOfType(currentElement))
						{
							// the current object exists, is Active and of the given type
							result.Add(currentElement);
						}

						currentElement = currentElement.next;
					}
					
					currentSubZone.LockObject.ReleaseReaderLock(); // release the reading loc
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the lists of objects, located in the current Zone and of the given type, that are at most at a 'radius' distance from (x,y,z)
		/// The found objects are appended to the given 'partialList'.
		/// </summary>
		/// <param name="type">the type of objects to look for</param>
		/// <param name="pos">the observation position</param>
		/// <param name="radius">the radius to check against</param>
		/// <param name="partialList">an initial (eventually empty but initialized, i.e. never null !!) list of objects</param>
		/// <returns>partialList augmented with the new objects verigying both type and radius in the current Zone</returns>
		internal DynamicList GetObjectsInRadius(Type type, Point pos, ushort radius, DynamicList partialList)
		{
			m_inRadiusCalls ++;

			uint sqRadius = (uint)(radius * radius);
			
			int xInZone = pos.X - m_xOffset; // x in zone coordinates
			int yInZone = pos.Y - m_yOffset; // y in zone coordinates

			int cellNbr = (radius / SUBZONE_SIZE) + 1; // radius in terms of subzone number
			int xInCell = xInZone / SUBZONE_SIZE; // xInZone in terms of subzone coord
			int yInCell = yInZone / SUBZONE_SIZE; // yInZone in terms of subzone coord

			int minColumn = xInCell - cellNbr;
			if (minColumn < 0)
			{
				minColumn = 0;
			}

			int maxColumn = xInCell + cellNbr;
			if (maxColumn > (SUBZONE_NBR_ON_ZONE_SIDE - 1))
			{
				maxColumn = SUBZONE_NBR_ON_ZONE_SIDE - 1;
			}

			int minLine = yInCell - cellNbr;
			if (minLine < 0)
			{
				minLine = 0;
			}

			int maxLine = yInCell + cellNbr;
			if (maxLine > (SUBZONE_NBR_ON_ZONE_SIDE - 1))
			{
				maxLine = SUBZONE_NBR_ON_ZONE_SIDE - 1;
			}

			for (int currentLine = minLine; currentLine <= maxLine; ++currentLine)
			{
				for (int currentColumn = minColumn; currentColumn <= maxColumn; ++currentColumn)
				{
					int xLeft = currentColumn * SUBZONE_SIZE;
					int xRight = xLeft + SUBZONE_SIZE;
					int yTop = currentLine * SUBZONE_SIZE;
					int yBottom = yTop + SUBZONE_SIZE;

					if (CheckMinDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
					{
						// the minimum distance is smaller than radius
						if (CheckMaxDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
						{
							// the current subzone is fully enclosed within the radius
							// => add all the objects of the current subzone
							AddToListWithoutDistanceCheck(m_subZoneElements[currentColumn][currentLine], type, partialList);
						}
						else
						{
							// the current subzone is partially enclosed within the radius
							// => only add the objects within the right area
							AddToListWithDistanceCheck(m_subZoneElements[currentColumn][currentLine], pos, sqRadius, type, partialList);
						}
					}
				}
			}
			return partialList;
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Checks that the minimal distance between a point and a subzone (defined by the four position of the sides) is lower or equal
		/// to the distance (given as a square distance)
		/// </summary>
		/// <param name="x">X position of the point</param>
		/// <param name="y">Y position of the square</param>
		/// <param name="xLeft">X value of the left side of the square</param>
		/// <param name="xRight">X value of the right side of the square</param>
		/// <param name="yTop">Y value of the top side of the square</param>
		/// <param name="yBottom">Y value of the bottom side of the square</param>
		/// <param name="squareRadius">the square of the radius to check for</param>
		/// <returns>The distance</returns>
		private bool CheckMinDistance(int x, int y, int xLeft, int xRight, int yTop, int yBottom, uint squareRadius)
		{
			long distance = 0;

			if ((y >= yTop) && (y <= yBottom))
			{
				if ((x >= xLeft) && (x <= xRight))
				{
					return true;
				}
				else
				{ 
					int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
					distance = (long)xdiff * xdiff;
				}
			}
			else
			{
				if ((x >= xLeft) && (x <= xRight))
				{
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long)ydiff * ydiff;
				}
				else
				{
					int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long)xdiff * xdiff + (long)ydiff * ydiff;
				}
			}

			return (distance <= squareRadius);
		}


		/// <summary>
		/// Checks that the maximal distance between a point and a subzone (defined by the four position of the sides) is lower or equal
		/// to the distance (given as a square distance)
		/// </summary>
		/// <param name="x">X position of the point</param>
		/// <param name="y">Y position of the square</param>
		/// <param name="xLeft">X value of the left side of the square</param>
		/// <param name="xRight">X value of the right side of the square</param>
		/// <param name="yTop">Y value of the top side of the square</param>
		/// <param name="yBottom">Y value of the bottom side of the square</param>
		/// <param name="squareRadius">the square of the radius to check for</param>
		/// <returns>The distance</returns>
		private bool CheckMaxDistance(int x, int y, int xLeft, int xRight, int yTop, int yBottom, uint squareRadius)
		{
			long distance = 0;

			int xdiff = Math.Max(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
			int ydiff = Math.Max(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
			distance = (long)xdiff * xdiff + (long)ydiff * ydiff;

			return (distance <= squareRadius);
		}

		/// <summary>
		/// Add all object of the given type in the currentSubZone to the partial list
		/// </summary>
		private void AddToListWithoutDistanceCheck(SubZone currentSubZone, Type type, DynamicList partialList)
		{
			currentSubZone.LockObject.AcquireReaderLock(-1); // be sure no thread write in the subZone but stay the others thread read in the subZone
			
			GeometryEngineNode currentElement = currentSubZone.ElementsList;
			while (currentElement != null)
			{
				if (currentElement.ObjectState == eObjectState.Active && type.IsInstanceOfType(currentElement))
				{
					// the current object exists, is Active and good type
					if (!partialList.Contains(currentElement))
					{
						// and not already in the partialList
						// => add it
						partialList.Add(currentElement);
					}
				}

				currentElement = currentElement.next;
			}

			currentSubZone.LockObject.ReleaseReaderLock(); // release the reading loc
		}

		/// <summary>
		/// Add all object of the given type in the currentSubZone within the given radius to the partial list
		/// </summary>
		private void AddToListWithDistanceCheck(SubZone currentSubZone, Point pos, uint sqRadius, Type type, DynamicList partialList)
		{
			currentSubZone.LockObject.AcquireReaderLock(-1); // be sure no thread write in the subZone but stay the others thread read in the subZone
			
			GeometryEngineNode currentElement = currentSubZone.ElementsList;
			while (currentElement != null)
			{
				if(currentElement.ObjectState == eObjectState.Active && type.IsInstanceOfType(currentElement))
				{
					// the current object exists, is Active and good type
					if ( pos.CheckSquareDistance(currentElement.Position, sqRadius) && !partialList.Contains(currentElement))
					{
						// in the checked radius and not already in the partialList
						// => add it
						partialList.Add(currentElement);
					}
				}

				currentElement = currentElement.next;
			}

			currentSubZone.LockObject.ReleaseReaderLock(); // release the reading loc
		}
		#endregion

		#region Position conversion methods
		
		/// <summary>
		/// Converts global position to local zone position.
		/// </summary>
		/// <param name="regionPosition">Position in the region.</param>
		/// <returns>Position in the zone.</returns>
		public Point ToLocalPosition(Point regionPosition)
		{
			regionPosition.X -= XOffset;
			regionPosition.Y -= YOffset;
			return regionPosition;
		}
		
		/// <summary>
		/// Converts zone position to region position.
		/// </summary>
		/// <param name="localPos">Position in the zone.</param>
		/// <returns>Position in the region.</returns>
		public Point ToRegionPosition(Point localPos)
		{
			localPos.X += XOffset;
			localPos.Y += YOffset;
			return localPos;
		}
		
		#endregion
	}
}
