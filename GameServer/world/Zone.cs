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
 * Modified by Konik & WitchKing 15.03.2005
 * Modified the way that the GetXXXInRadius works.
 * A lot faster and more accurate now...
 */

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using DOL.GS.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;

using log4net;
using Hashtable=System.Collections.Hashtable;

namespace DOL.GS
{
	/// <summary>
	/// This class represents one Zone in DAOC. It holds all relevant information
	/// that is needed to do different calculations. 
	/// </summary>
	public class Zone
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region constants data

		private const ushort SUBZONE_NBR_ON_ZONE_SIDE = 32; // MUST BE A POWER OF 2 (current implementation limit is 128 inclusive)

		/// <summary>
		/// Number of SubZone in a Zone
		/// </summary>
		private const ushort SUBZONE_NBR = (ushort)(SUBZONE_NBR_ON_ZONE_SIDE * SUBZONE_NBR_ON_ZONE_SIDE);

		private const ushort SUBZONE_SIZE = (ushort)(65536 / SUBZONE_NBR_ON_ZONE_SIDE);

		private static readonly ushort SUBZONE_SHIFT = (ushort)Math.Round(Math.Log(SUBZONE_SIZE) / Math.Log(2)); // to get log in base 2

		private static readonly ushort SUBZONE_ARRAY_Y_SHIFT = (ushort)Math.Round(Math.Log(SUBZONE_NBR_ON_ZONE_SIDE) / Math.Log(2));

		public const ushort MAX_REFRESH_INTERVAL = 2000; // in milliseconds

		#endregion

		#region Structures Definition

		/// <summary>
		/// Object Type is Item
		/// </summary>
		public enum eGameObjectType : byte
		{
			ITEM = 0,
			NPC = 1,
			PLAYER = 2
		}

		/// <summary>
		/// This class represent a node in a doubly linked list
		/// </summary>
		private class SubNodeElement
		{
			public GameObject data = null;
			public SubNodeElement next = null;
			public SubNodeElement previous = null;


			public SubNodeElement()
			{
				next = this;
				previous = this;
			}


			/// <summary>
			/// Insert a node before this one
			/// </summary>
			/// <param name="p_elem">The node to insert</param>
			public void PushBack(SubNodeElement p_elem)
			{
				p_elem.previous = this;
				p_elem.next = next;
				next.previous = p_elem;
				next = p_elem;
			}


			/// <summary>
			/// Remove this node from the list
			/// </summary>
			public void Remove()
			{
				if (previous != this)
				{
					previous.next = next;
					next.previous = previous;
				}

				previous = this;
				next = this;
			}
		}

		#endregion

		#region Declaration

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
		private int m_xOffset;

		/// <summary>
		/// The YOffset of this Zone inside the region
		/// </summary>
		private int m_yOffset;

		/// <summary>
		/// The Width of the Zone in Coordinates
		/// </summary>
		private int m_width;

		/// <summary>
		/// The Height of the Zone in Coordinates
		/// </summary>
		private int m_height;

		/// <summary>
		/// The Z value of the water in this zone
		/// </summary>
		private int m_waterLevel;

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
		/// Returns the Width of this Zone
		/// </summary>
		public int Width
		{
			get { return m_width; }
			set { m_width = value; }
		}

		/// <summary>
		/// Returns the Height of this Zone
		/// </summary>
		public int Height
		{
			get { return m_height; }
			set { m_height = value; }
		}

		/// <summary>
		/// Returns the water level of this Zone (Z coohor)
		/// </summary>
		public int WaterLevel
		{
			get { return m_waterLevel; }
			set { m_waterLevel = value; }
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
		private readonly SubNodeElement[][] m_subZoneElements = new SubNodeElement[SUBZONE_NBR][];

		/// <summary>
		/// Should be accessed as [(subzone/4)|objectType]
		/// </summary>
		private int[] m_subZoneTimestamps;

		private int m_objectCount;

		/// <summary>
		/// already initialized?
		/// </summary>
		private bool m_initialized = false;

		/// <summary>
		/// Returns the total number of objects held in the zone
		/// </summary>
		public int TotalNumberOfObjects
		{
			get { return m_objectCount; }
		}

		#endregion

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a new Zone object
		/// </summary>
		/// <param name="region">the parent region</param>
		/// <param name="id">the zone id (eg. 15)</param>
		/// <param name="desc">the zone description (eg. "Camelot Hills")</param>
		/// <param name="xoff">the X offset of this zone inside the region</param>
		/// <param name="yoff">the Y offset of this zone inside the region</param>
		/// <param name="width">the Width of this zone</param>
		/// <param name="height">the Height of this zone</param>
	/*	public Zone(Region region, ushort id, string desc, int xoff, int yoff, int width, int height)
		{
			m_Region = region;
			m_ID = id;
			m_Description = desc;
			m_XOffset = xoff;
			m_YOffset = yoff;
			m_Width = width;
			m_Height = height;

			// initialise subzone objects and counters

			m_subZoneElements = new SubNodeElement[SUBZONE_NBR][];
			m_initialized = false;
		}*/

		private void InitializeZone()
		{
			if (m_initialized) return;
			for (int i = 0; i < SUBZONE_NBR; i++)
			{
				m_subZoneElements[i] = new SubNodeElement[3];
				for (int k = 0; k < m_subZoneElements[i].Length; k++)
				{
					m_subZoneElements[i][k] = new SubNodeElement();
				}
			}
			m_subZoneTimestamps = new int[SUBZONE_NBR << 2];
			m_initialized = true;
		}

		#endregion

		#region New subzone Management function

		private short GetSubZoneOffset(int lineSubZoneIndex, int columnSubZoneIndex)
		{
			return (short)(columnSubZoneIndex + (lineSubZoneIndex << SUBZONE_ARRAY_Y_SHIFT));
		}


		/// <summary>
		/// Returns the SubZone index using a position in the zone
		/// </summary>
		/// <param name="p_pos">position</param>
		/// <returns>The SubZoneIndex</returns>
		private short GetSubZoneIndex(Point p_pos)
		{
			int xDiff = p_pos.X - m_xOffset;
			int yDiff = p_pos.Y - m_yOffset;

			if ((xDiff < 0) || (xDiff > 65535) || (yDiff < 0) || (yDiff > 65535))
			{
				// the object is out of the zone
				return -1;
			}
			else
			{
				// the object is in the zone
				//DOLConsole.WriteWarning("GetSubZoneIndex : " + SUBZONE_NBR_ON_ZONE_SIDE + ", " + SUBZONE_NBR + ", " + SUBZONE_SHIFT + ", " + SUBZONE_ARRAY_Y_SHIFT);

				xDiff >>= SUBZONE_SHIFT;
				yDiff >>= SUBZONE_SHIFT;

				return GetSubZoneOffset(yDiff, xDiff);
			}
		}


		/// <summary>
		/// Handle a GameObject entering a zone
		/// </summary>
		/// <param name="p_Obj">The GameObject object</param>
		public void ObjectEnterZone(GameObject p_Obj)
		{

			if (!m_initialized) InitializeZone();
			int subZoneIndex = GetSubZoneIndex(p_Obj.Position);
			if ((subZoneIndex >= 0) && (subZoneIndex < SUBZONE_NBR))
			{
				SubNodeElement element = new SubNodeElement();
				element.data = p_Obj;

				int type = -1;

				//Only GamePlayer, GameNPC and GameStaticItem classes
				//are handled.
				if (p_Obj is GamePlayer)
					type = (int)eGameObjectType.PLAYER;
				else if (p_Obj is GameNPC)
					type = (int)eGameObjectType.NPC;
				else if (p_Obj is GameStaticItem)
					type = (int)eGameObjectType.ITEM;

				if (type == -1)
					return;

				if (log.IsDebugEnabled)
				{
					log.Debug("Object " + p_Obj.ObjectID + " (" + ((eGameObjectType)type) + ") entering subzone " + subZoneIndex + " in zone " + ZoneID + " in region " + Region.RegionID);
				}

				lock (m_subZoneElements[subZoneIndex][type])
				{
					// add to subzone list
					m_subZoneElements[subZoneIndex][type].PushBack(element);
				}

				Interlocked.Increment(ref m_objectCount);
			}
		}


		/// <summary>
		/// Handles movement of objects from zone to zone
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="element"></param>
		private void ObjectEnterZone(eGameObjectType objectType, SubNodeElement element)
		{

			if (!m_initialized) InitializeZone();
			int subZoneIndex = GetSubZoneIndex(element.data.Position);

			if (log.IsDebugEnabled)
			{
				log.Debug("Object " + element.data.ObjectID + "(" + objectType + ") entering (inner) subzone " + subZoneIndex + " in zone " + ZoneID + " in region " + Region.RegionID);
			}

			if ((subZoneIndex >= 0) && (subZoneIndex < SUBZONE_NBR))
			{
				int type = (int)objectType;

				lock (m_subZoneElements[subZoneIndex][type])
				{
					// add to subzone list
					m_subZoneElements[subZoneIndex][type].PushBack(element);
				}

				Interlocked.Increment(ref m_objectCount);
			}
		}

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

		/// <summary>
		/// Gets the lists of objects, located in the current Zone and of the given type, that are at most at a 'radius' distance from (x,y,z)
		/// The found objects are appended to the given 'partialList'.
		/// </summary>
		/// <param name="type">the type of objects to look for</param>
		/// <param name="pos">the observation position</param>
		/// <param name="radius">the radius to check against</param>
		/// <param name="partialList">an initial (eventually empty but initialized, i.e. never null !!) list of objects</param>
		/// <returns>partialList augmented with the new objects verigying both type and radius in the current Zone</returns>
		//internal GameObjectSet GetObjectsInRadius(eGameObjectType type, int x, int y, int z, ushort radius, GameObjectSet partialList) {
		internal DynamicList GetObjectsInRadius(eGameObjectType type, Point pos, ushort radius, DynamicList partialList)
		{
			m_inRadiusCalls++;
			if (!m_initialized) InitializeZone();
			// initialise parameters
			uint sqRadius = (uint)radius * (uint)radius;
			int typeIndex = (int)type;

			int xInZone = pos.X - m_xOffset; // x in zone coordinates
			int yInZone = pos.Y - m_yOffset; // y in zone coordinates

			int cellNbr = (radius >> SUBZONE_SHIFT) + 1; // radius in terms of subzone number
			int xInCell = xInZone >> SUBZONE_SHIFT; // xInZone in terms of subzone coord
			int yInCell = yInZone >> SUBZONE_SHIFT; // yInZone in terms of subzone coord

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

			DOL.GS.Collections.Hashtable inZoneElements = new DOL.GS.Collections.Hashtable();
			DOL.GS.Collections.Hashtable outOfZoneElements = new DOL.GS.Collections.Hashtable();
			
			for (int currentLine = minLine; currentLine <= maxLine; ++currentLine)
			{
				int currentSubZoneIndex = 0;
				SubNodeElement startElement = null;

				for (int currentColumn = minColumn; currentColumn <= maxColumn; ++currentColumn)
				{
					currentSubZoneIndex = GetSubZoneOffset(currentLine, currentColumn);

					// get the right list of objects
					startElement = m_subZoneElements[currentSubZoneIndex][typeIndex];

					if (startElement != startElement.next)
					{
						int xLeft = currentColumn << SUBZONE_SHIFT;
						int xRight = xLeft + SUBZONE_SIZE;
						int yTop = currentLine << SUBZONE_SHIFT;
						int yBottom = yTop + SUBZONE_SIZE;

						if (CheckMinDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
						{
							// the minimum distance is smaller than radius

							if (CheckMaxDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
							{
								// the current subzone is fully enclosed within the radius
								// => add all the objects of the current subzone

								lock (startElement)
								{
									UnsafeAddToListWithoutDistanceCheck(startElement, pos, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
									UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
								}
							}
							else
							{
								// the current subzone is partially enclosed within the radius
								// => only add the objects within the right area 

								lock (startElement)
								{
									UnsafeAddToListWithDistanceCheck(startElement, pos, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
									UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
								}
							}
						}
					}
				}
			}

			//
			// perform needed relocations
			//

			if (inZoneElements.Count > 0)
			{
				PlaceElementsInZone(inZoneElements);

				if (log.IsDebugEnabled)
				{
					log.Debug("Zone" + ZoneID + " " + inZoneElements.Count + " objects moved inside zone");
				}
			}

			if (outOfZoneElements.Count > 0)
			{
				PlaceElementsInOtherZones(outOfZoneElements);

				if (log.IsDebugEnabled)
				{
					log.Debug("Zone" + ZoneID + " " + outOfZoneElements.Count + " objects moved outside zone");
				}
			}


			return partialList;
		}


		private void UnsafeAddToListWithoutDistanceCheck(SubNodeElement startElement, Point pos, uint sqRadius, int typeIndex, int subZoneIndex, DynamicList partialList, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
		{
			SubNodeElement currentElement = startElement.next;
			SubNodeElement nextElement = currentElement.next; // always store the next element before class ShouldElementMove because the current can be changed
			GameObject currentObject = null;

			do
			{
				currentObject = currentElement.data;

				byte result = ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements);
				if (result == 1 && pos.CheckSquareDistance(currentObject.Position, sqRadius))
				{
					if (!partialList.Contains(currentObject))
					{
						partialList.Add(currentObject);
					}
				}
				else if(result == 2)
				{
					// the current object exists, is Active and still in the current subzone
					// => add it
					if (!partialList.Contains(currentObject))
					{
						partialList.Add(currentObject);
					}
				}

				currentElement = nextElement;
				nextElement = currentElement.next;
			}
			while (currentElement != startElement);
		}


		private void UnsafeAddToListWithDistanceCheck(SubNodeElement startElement, Point pos, uint sqRadius, int typeIndex, int subZoneIndex, DynamicList partialList, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
		{
			SubNodeElement currentElement = startElement.next;
			SubNodeElement nextElement = currentElement.next; // always store the next element before class ShouldElementMove because the current can be changed
			GameObject currentObject = null;

			do
			{
				currentObject = currentElement.data;

				if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements) > 0)
				{
					if (pos.CheckSquareDistance(currentObject.Position, sqRadius) && !partialList.Contains(currentObject))
					{
						partialList.Add(currentObject);
					}
				}

				currentElement = nextElement;
				nextElement = currentElement.next;
			}
			while (currentElement != startElement);
		}


		#region Relocation

		internal void Relocate(object state)
		{
			if (!m_initialized) return;
			if (m_objectCount > 0)
			{
				DOL.GS.Collections.Hashtable outOfZoneElements = new DOL.GS.Collections.Hashtable();
				DOL.GS.Collections.Hashtable inZoneElements = new DOL.GS.Collections.Hashtable();

				for (int subZoneIndex = 0; subZoneIndex < m_subZoneElements.Length; subZoneIndex++)
				{
					for (int typeIndex = 0; typeIndex < m_subZoneElements[subZoneIndex].Length; typeIndex++)
					{
						if (Environment.TickCount > m_subZoneTimestamps[(subZoneIndex << 2) | typeIndex])
						{
							// it is time to relocate some elements in this subzone
							// => perform needed relocations of elements
							SubNodeElement startElement = m_subZoneElements[subZoneIndex][typeIndex];
							SubNodeElement currentElement = startElement.next;
							SubNodeElement nextElement = currentElement.next; // always store the next element before class ShouldElementMove because the current can be changed

							lock (startElement)
							{
								if (startElement != startElement.next)
								{
									do
									{
										ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements);
										
										currentElement = nextElement;
										nextElement = currentElement.next;
									}
									while (currentElement != startElement);

									UnsafeUpdateSubZoneTimestamp(subZoneIndex, typeIndex);
								}
							}
						}
					}
				}


				if (inZoneElements.Count > 0)
				{
					PlaceElementsInZone(inZoneElements);

					if (log.IsDebugEnabled)
					{
						log.Debug("Zone" + ZoneID + " " + inZoneElements.Count + " objects moved inside zone");
					}
				}

				if (outOfZoneElements.Count > 0)
				{
					PlaceElementsInOtherZones(outOfZoneElements);

					if (log.IsDebugEnabled)
					{
						log.Debug("Zone" + ZoneID + " " + outOfZoneElements.Count + " objects moved outside zone");
					}
				}
			}
		}


		private void UnsafeUpdateSubZoneTimestamp(int subZoneIndex, int typeIndex)
		{
			int nextUpdateTimestamp = Environment.TickCount + Zone.MAX_REFRESH_INTERVAL;

			if (nextUpdateTimestamp < 0)
			{
				// an overflow occured
				nextUpdateTimestamp += int.MaxValue; // as TickCount wraps around 0
			}

			m_subZoneTimestamps[(subZoneIndex << 2) | typeIndex] = nextUpdateTimestamp;
		}


		private byte ShouldElementMove(SubNodeElement currentElement, int typeIndex, int subZoneIndex, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
		{

			if (!m_initialized) InitializeZone();
			GameObject currentObject = currentElement.data;

			if ((currentObject != null) &&
				(((int)currentObject.ObjectState) == (int)GameObject.eObjectState.Active)
				&& (currentObject.Region == Region))
			{
				// the current object exists, is Active and still in the Region where this Zone is located

				int currentElementSubzoneIndex = GetSubZoneIndex(currentObject.Position);

				if (currentElementSubzoneIndex == -1)
				{
					// the object has moved in another Zone in the same Region

					ArrayList movedElements = (ArrayList)outOfZoneElements[typeIndex];

					if (movedElements == null)
					{
						movedElements = new ArrayList();
						outOfZoneElements[typeIndex] = movedElements;
					}

					movedElements.Add(currentElement);

					currentElement.Remove();

					Interlocked.Decrement(ref m_objectCount);

					return 1;
				}
				else if (currentElementSubzoneIndex != subZoneIndex)
				{
					// it has changed of subzone
					SubNodeElement newSubZoneStartElement = m_subZoneElements[currentElementSubzoneIndex][typeIndex];
					ArrayList movedElements = (ArrayList)inZoneElements[newSubZoneStartElement];

					if (movedElements == null)
					{
						movedElements = new ArrayList();
						inZoneElements[newSubZoneStartElement] = movedElements;
					}

					// make it available for relocation
					movedElements.Add(currentElement);

					currentElement.Remove();

					return 1;
				}

				return 2;
			}
			else
			{
				// ghost object
				// => remove it

				currentElement.Remove();

				Interlocked.Decrement(ref m_objectCount);
				return 0;
			}
		}


		private void PlaceElementsInZone(DOL.GS.Collections.Hashtable elements)
		{
			DOL.GS.Collections.DictionaryEntry currentEntry = null;
			ArrayList currentList = null;
			SubNodeElement currentStartElement = null;
			SubNodeElement currentElement = null;

			IEnumerator entryEnumerator = elements.GetEntryEnumerator();

			while (entryEnumerator.MoveNext())
			{
				currentEntry = (DOL.GS.Collections.DictionaryEntry)entryEnumerator.Current;
				currentStartElement = (SubNodeElement)currentEntry.key;

				currentList = (ArrayList)currentEntry.value;

				lock (currentStartElement)
				{
					for (int i = 0; i < currentList.Count; i++)
					{
						currentElement = (SubNodeElement)currentList[i];
						currentStartElement.PushBack(currentElement);
					}
				}
			}
		}


		private void PlaceElementsInOtherZones(DOL.GS.Collections.Hashtable elements)
		{
			DOL.GS.Collections.DictionaryEntry currentEntry = null;

			int currentType = 0;
			ArrayList currentList = null;

			Zone currentZone = null;
			SubNodeElement currentElement = null;

			IEnumerator entryEnumerator = elements.GetEntryEnumerator();

			while (entryEnumerator.MoveNext())
			{
				currentEntry = (DOL.GS.Collections.DictionaryEntry)entryEnumerator.Current;
				currentType = (int)currentEntry.key;

				currentList = (ArrayList)currentEntry.value;

				for (int i = 0; i < currentList.Count; i++)
				{
					currentElement = (SubNodeElement)currentList[i];
					currentZone = Region.GetZone(currentElement.data.Position);

					if (currentZone != null)
					{
						currentZone.ObjectEnterZone((eGameObjectType)currentType, currentElement);
					}
				}
			}
		}

		#endregion

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

		#endregion

		#region Get random NPC

		/// <summary>
		/// Get a random NPC belonging to a realm
		/// </summary>
		/// <param name="realm">The realm the NPC belong to</param>
		/// <returns>a npc</returns>
		public GameNPC GetRandomNPC(eRealm realm)
		{
			return GetRandomNPC(new eRealm[] { realm }, -1, -1);
		}


		/// <summary>
		/// Get a random NPC belonging to a realm between levels minlevel and maxlevel
		/// </summary>
		/// <param name="realm">The realm the NPC belong to</param>
		/// <param name="minLevel">The minimal level of the NPC</param>
		/// <param name="maxLevel">The maximal level NPC</param>
		/// <returns>A npc</returns>
		public GameNPC GetRandomNPC(eRealm realm, int minLevel, int maxLevel)
		{
			return GetRandomNPC(new eRealm[] { realm }, minLevel, maxLevel);
		}


		/// <summary>
		/// Get a random npc from zone with given realms
		/// </summary>
		/// <param name="realms">The realms to get the NPC from</param>
		/// <returns>The NPC</returns>
		public GameNPC GetRandomNPC(eRealm[] realms)
		{
			return GetRandomNPC(realms, -1, -1);
		}


		/// <summary>
		/// Get a random npc from zone with given realms
		/// </summary>
		/// <param name="realms">The realms to get the NPC from</param>
		/// <param name="maxLevel">The minimal level of the NPC</param>
		/// <param name="minLevel">The maximum level of the NPC</param>
		/// <returns>The NPC</returns>
		public GameNPC GetRandomNPC(eRealm[] realms, int minLevel, int maxLevel)
		{

			if (!m_initialized) InitializeZone();
			// select random starting subzone and iterate over all objects in subzone than in all subzone...
			int currentSubZoneIndex = Util.Random(SUBZONE_NBR);
			int startSubZoneIndex = currentSubZoneIndex;
			GameNPC randomNPC = null;
			GameNPC currentNPC = null;
			do
			{
				SubNodeElement startElement = m_subZoneElements[currentSubZoneIndex][(int)eGameObjectType.NPC];
				lock (startElement)
				{
					// if list is not empty
					if (startElement != startElement.next)
					{
						SubNodeElement curElement = startElement.next;
						do
						{
							currentNPC = (GameNPC)curElement.data;
							if (currentNPC != null && currentNPC.ObjectState == GameObject.eObjectState.Active)
							{
								if (minLevel >= 0 && currentNPC.Level < minLevel &&
									maxLevel >= 0 && currentNPC.Level > maxLevel)
								{
									for (int i = 0; i < realms.Length; ++i)
									{
										byte realm = (byte)realms[i];
										if (currentNPC.Realm == realm)
										{
											randomNPC = currentNPC;
											break;
										}
									}
								}
							}
							curElement = curElement.next;
						} while ((randomNPC != null) && (curElement != startElement));
					}
				}

				if (randomNPC == null)
				{
					if (++currentSubZoneIndex >= SUBZONE_NBR)
					{
						currentSubZoneIndex = 0;
					}
				}
			} while ((randomNPC != null) && (currentSubZoneIndex != startSubZoneIndex));

			return randomNPC;
		}

		#endregion

		#region position conversion methods
		
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

		/// <summary>
		/// Gets the Zones Realm by passing the ZoneID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/*public eRealm GetRealmByZoneID(ushort id)
		{
			//nice website here http://www.teatromusica.it/valmerwolf/varie/mappe/numzones.htm
			//wish there was a better way to do this

			//classic
			if (id >= 0 && id <= 26) return eRealm.Albion;
			else if (id >= 100 && id <= 129) return eRealm.Midgard;
			else if (id >= 200 && id <= 224) return eRealm.Hibernia;
			//SI
			else if (id >= 51 && id <= 62) return eRealm.Albion;
			else if (id >= 151 && id <= 161) return eRealm.Midgard;
			else if (id >= 181 && id <= 191) return eRealm.Hibernia;
			//foundations
			else if (id >= 13 && id <= 20) return eRealm.Albion;
			else if (id == 64) return eRealm.Albion;
			else if (id >= 260 && id <= 261) return eRealm.Albion;
			else if (id >= 114 && id <= 122) return eRealm.Midgard;
			else if (id >= 266 && id <= 267) return eRealm.Midgard;
			else if (id >= 213 && id <= 219) return eRealm.Hibernia;
			else if (id >= 272 && id <= 273) return eRealm.Hibernia;
			//old frontiers
			else if (id >= 11 && id <= 15) return eRealm.Albion;
			else if (id >= 111 & id <= 115) return eRealm.Midgard;
			else if (id >= 210 && id <= 214) return eRealm.Hibernia;
			//others
			else if (id == 27) return eRealm.Albion;
			else if (id == 28 || id == 157) return eRealm.Midgard;
			else if (id == 29) return eRealm.Hibernia;
			//TOA
			else if (id == 70) return eRealm.Albion;
			else if (id >= 30 && id <= 47) return eRealm.Albion;
			else if (id == 71) return eRealm.Midgard;
			else if (id >= 73 && id <= 90) return eRealm.Midgard;
			else if (id == 72) return eRealm.Hibernia;
			else if (id >= 130 && id <= 147) return eRealm.Hibernia;
			//catacombs
			else if (id == 59) return eRealm.Albion;
			else if (id == 61) return eRealm.Hibernia;
			else if (id >= 63 && id <= 69) return eRealm.Albion;
			else if (id == 109 || id == 196 || id == 227) return eRealm.Albion;
			else if (id == 58) return eRealm.Midgard;
			else if (id == 148 || id == 149 || id == 162 || id == 188 || id == 189 ||
				id == 195 || id == 226 || id == 229 || id == 243) return eRealm.Midgard;
			else if (id >= 92 && id <= 99) return eRealm.Hibernia;
			else if (id == 197 || id == 228) return eRealm.Hibernia;
			//instanced dungeons
			else if (id == 343 || id == 498) return eRealm.Albion;
			else if (id >= 376 && id <= 436) return eRealm.Albion;
			else if (id >= 300 && id <= 375) return eRealm.Midgard;
			else if (id == 439 || id == 440 || id == 497) return eRealm.Midgard;
			else if (id == 49 || id == 344) return eRealm.Hibernia;
			else if (id >= 437 && id <= 499) return eRealm.Hibernia;
			//new frontier & common
			else if (id >= 167 && id <= 170) return eRealm.Midgard;
			else if (id >= 171 && id <= 174) return eRealm.Hibernia;
			else if (id >= 175 && id <= 178) return eRealm.Albion;
			else if (id == 234) return eRealm.Albion;
			else if (id == 235) return eRealm.Midgard;
			else if (id == 236) return eRealm.Hibernia;
			else if (id >= 167 && id <= 170) return eRealm.Midgard; 
			else if (id >= 171 && id <= 174) return eRealm.Hibernia; 
			else if (id >= 175 && id <= 178) return eRealm.Albion; 
			//new frontier and common
			//bg1 Fort Brolorn 
			else if (id == 234) return eRealm.Midgard; 
			//bg5 Leonis Keep 
			else if (id == 235) return eRealm.Hibernia; 
			//bg10 Caer Claret 
			else if (id == 236) return eRealm.Albion; 
			//bg15 Dun Killaloe 
			else if (id == 247) return eRealm.Hibernia; 
			//bg20 Thidranki Faste 
			else if (id == 248) return eRealm.Midgard; 
			//bg25 Dun Braemer 
			else if (id == 249) return eRealm.Hibernia; 
			//bg30 Caer Wilton 
			else if (id == 250) return eRealm.Albion; 
			//bg35 Molvik Faste 
			else if (id == 241) return eRealm.Midgard; 
			//bg40 Leirvik Castle 
			else if (id == 242) return eRealm.Hibernia;

			//todo get the base realm for the other bgs not just the first 3
			return eRealm.None;
		}*/
	}
}