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
using System.Reflection;
using System.Threading;

using DOL.Database;
using DOL.Language;
using DOL.GS.Utils;
using log4net;
using DOL.GS.Geometry;

namespace DOL.GS
{
	/// <summary>
	/// This class represents one Zone in DAOC. It holds all relevant information
	/// that is needed to do different calculations.
	/// </summary>
	public class Zone : ITranslatableObject
	{
		/* 
        This file has been extensively modified for the new subzone management system
        So for old version please have a look in old release
		 */

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
			PLAYER = 2,
			DOOR = 3,
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

		#region variables

		/// <summary>
		/// Contains the list of objects per subzone
		/// </summary>
		private SubNodeElement[][] m_subZoneElements;

		/// <summary>
		/// Should be accessed as [(subzone/4)|objectType]
		/// </summary>
		private int[] m_subZoneTimestamps;

		private int m_objectCount;

		/// <summary>
		/// Holds a pointer to the region that is the parent of this zone
		/// </summary>
		private Region m_Region;

		/// <summary>
		/// The ID of the Zone eg. 15
		/// </summary>
		private readonly ushort m_ID;

		/// <summary>
		/// The id of the fake zone we send to the client.
		/// This is used for instances, which also need to create fake zones aswell as regions!
		/// </summary>
		private ushort m_zoneSkinID;

		/// <summary>
		/// The description of the Zone eg. "Camelot Hills"
		/// </summary>
		private string m_Description;

		/// <summary>
		/// The Width of the Zone in Coordinates
		/// </summary>
		private readonly int m_Width;

		/// <summary>
		/// The Height of the Zone in Coordinates
		/// </summary>
		private readonly int m_Height;

		/// <summary>
		/// The waterlevel of this zone
		/// </summary>
		private int m_waterlevel;

		/// <summary>
		/// Does this zone support diving?
		/// </summary>
		private bool m_isDivingEnabled;

		/// <summary>
		/// Does this zone contain Lava
		/// </summary>
		private bool m_isLava;

		/// <summary>
		/// already initialized?
		/// </summary>
		private bool m_initialized = false;


		private int m_bonusXP = 0;
		private int m_bonusRP = 0;
		private int m_bonusBP = 0;
		private int m_bonusCoin = 0;

        private eRealm m_realm;
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
		/// <param name="zoneskinID">For clientside positioning in instances: The 'fake' zoneid we send to clients.</param>
		public Zone(Region region, ushort id, string desc, int xoff, int yoff, int width, int height, ushort zoneskinID, bool isDivingEnabled, int waterlevel, bool islava, int xpBonus, int rpBonus, int bpBonus, int coinBonus, byte realm)
		{
			m_Region = region;
			m_ID = id;
			m_Description = desc;
            Offset = Vector.Create(x: xoff, y: yoff);
			m_Width = width;
			m_Height = height;
			m_zoneSkinID = zoneskinID;
			m_waterlevel = waterlevel;
			m_isDivingEnabled = isDivingEnabled;
			m_isLava = islava;

			m_bonusXP = xpBonus;
			m_bonusRP = rpBonus;
			m_bonusBP = bpBonus;
			m_bonusCoin = coinBonus;

			// initialise subzone objects and counters
			m_subZoneElements = new SubNodeElement[SUBZONE_NBR][];
			m_initialized = false;
            m_realm = (eRealm)realm;

		}

		public void Delete()
		{
			for (int i = 0; i < SUBZONE_NBR; i++)
			{
				if (m_subZoneElements[i] != null)
				{
					for (int k = 0; k < m_subZoneElements[i].Length; k++)
					{
						if (m_subZoneElements[i][k] != null)
						{
							m_subZoneElements[i][k].data = null;
							m_subZoneElements[i][k] = null;
						}
					}

					m_subZoneElements[i] = null;
				}
			}

			m_subZoneElements = null;
			m_subZoneTimestamps = null;
			m_Region = null;
			DOL.Events.GameEventMgr.RemoveAllHandlersForObject(this);
		}

		private void InitializeZone()
		{
			if (m_initialized) return;
			for (int i = 0; i < SUBZONE_NBR; i++)
			{
				m_subZoneElements[i] = new SubNodeElement[4];
				for (int k = 0; k < m_subZoneElements[i].Length; k++)
				{
					m_subZoneElements[i][k] = new SubNodeElement();
				}
			}
			m_subZoneTimestamps = new int[SUBZONE_NBR << 2];
			m_initialized = true;
		}

		#endregion

		#region properties
        public virtual LanguageDataObject.eTranslationIdentifier TranslationIdentifier
        {
            get { return LanguageDataObject.eTranslationIdentifier.eZone; }
        }

        public string TranslationId
        {
            get { return ID.ToString(); }
            set { }
        }

        public eRealm Realm
        {
            get
            {
                return m_realm;
            }
        }

		public bool IsDungeon
		{
			get
			{
				switch (m_Region.ID)
				{
					case 24:
					case 65:
					case 66:
					case 67:
					case 68:
					case 92:
					case 93:
					case 109:
					case 149:
					case 196:
					case 221:
					case 227:
					case 228:
					case 229:
					case 244:
					case 249:
					case 296:
					case 297:
					case 298:
					case 326:
					case 335:
					case 352:
					case 356:
					case 376:
					case 377:
					case 379:
					case 382:
					case 383:
					case 386:
					case 387:
					case 388:
					case 390:
					case 395:
					case 396:
					case 397:
					case 415:
					case 443:
					case 489://lvl5-9 Demons breach
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Returns the region of this zone
		/// </summary>
		public Region ZoneRegion
		{
			get { return m_Region; }
			set { m_Region = value; }
		}

		/// <summary>
		/// Returns the ID of this zone
		/// </summary>
		public ushort ID
		{
			get { return m_ID; }
		}

		//Dinberg: added for instances.
		/// <summary>
		/// The ID we send to the client, for client-side positioning of gameobjects and npcs.
		/// </summary>
		public ushort ZoneSkinID
		{ get { return m_zoneSkinID; } }

		/// <summary>
		/// Return the description of this zone
		/// </summary>
		public string Description
		{
			get { return m_Description; }
			set { m_Description = value; }
		}

        public Vector Offset { get; init; }

        [Obsolete("Use .Offset instead!")]
        public int XOffset => Offset.X;

        [Obsolete("Use .Offset instead!")]
        public int YOffset => Offset.Y;

		/// <summary>
		/// Returns the Width of this Zone
		/// </summary>
		public int Width
		{
			get { return m_Width; }
		}

		/// <summary>
		/// Returns the Height of this Zone
		/// </summary>
		public int Height
		{
			get { return m_Height; }
		}

		public int Waterlevel
		{
			get { return m_waterlevel; }
			set { m_waterlevel = value; }
		}

		public bool IsDivingEnabled
		{
			get { return m_isDivingEnabled; }
			set { m_isDivingEnabled = value; }
		}

		/// <summary>
		/// Is water in this zone lava?
		/// </summary>
		public virtual bool IsLava
		{
			get { return m_isLava; }
			set { m_isLava = value; }
		}

		/// <summary>
		/// Returns the total number of objects held in the zone
		/// </summary>
		public int TotalNumberOfObjects
		{
			get { return m_objectCount; }
		}

		public bool IsPathingEnabled { get; set; } = false;
		#endregion

		#region New subzone Management function

		private short GetSubZoneOffset(int lineSubZoneIndex, int columnSubZoneIndex)
		{
			return (short)(columnSubZoneIndex + (lineSubZoneIndex << SUBZONE_ARRAY_Y_SHIFT));
		}

        private short GetSubZoneIndex(Coordinate loc)
        {
            int xDiff = loc.X - Offset.X;
            int yDiff = loc.Y - Offset.Y;

            var isOutOfZone = (xDiff < 0) || (xDiff > 65535) || (yDiff < 0) || (yDiff > 65535);
            if (isOutOfZone) return -1;

            xDiff >>= SUBZONE_SHIFT;
            yDiff >>= SUBZONE_SHIFT;
            return GetSubZoneOffset(yDiff, xDiff);
        }


		/// <summary>
		/// Handle a GameObject entering a zone
		/// </summary>
		/// <param name="p_Obj">The GameObject object</param>
		public void ObjectEnterZone(GameObject p_Obj)
		{

			if (!m_initialized) InitializeZone();
			int subZoneIndex = GetSubZoneIndex(p_Obj.Coordinate);
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
				else if (p_Obj is IDoor)
					type = (int)eGameObjectType.DOOR;

				if (type == -1)
					return;

				if (log.IsDebugEnabled)
				{
					log.Debug("Object " + p_Obj.ObjectID + " (" + ((eGameObjectType)type) + ") entering subzone " + subZoneIndex + " in zone " + this.ID + " in region " + m_Region.ID);
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
			int subZoneIndex = GetSubZoneIndex(element.data.Coordinate);

			if (log.IsDebugEnabled)
			{
				log.Debug("Object " + element.data.ObjectID + "(" + objectType + ") entering (inner) subzone " + subZoneIndex + " in zone " + this.ID + " in region " + m_Region.ID);
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
        /// Gets the lists of objects, located in the current Zone and of the given type, that are at most at a 'radius' distance from (x,y,z)
        /// The found objects are appended to the given 'partialList'.
        /// </summary>
        internal ArrayList GetObjectsInRadius(eGameObjectType type, Coordinate coordinate, ushort radius, ArrayList partialList, bool ignoreZ)
		{
			if (!m_initialized) InitializeZone();
			// initialise parameters
			uint sqRadius = (uint)radius * (uint)radius;
			int referenceSubzoneIndex = GetSubZoneIndex(coordinate);
			int typeIndex = (int)type;

			int xInZone = coordinate.X - Offset.X; // x in zone coordinates
			int yInZone = coordinate.Y - Offset.Y; // y in zone coordinates

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
					{ // allow dirty read here to enable a more efficient and fine grained locking later...
						// the current subzone contains some objects

						if (currentSubZoneIndex == referenceSubzoneIndex)
						{
							lock (startElement)
							{
								// we are in the subzone of the observation point
								// => check all distances for all objects in the subzone
								UnsafeAddToListWithDistanceCheck(startElement, coordinate, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements, ignoreZ);
								UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
							}
						}
						else
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
										UnsafeAddToListWithoutDistanceCheck(startElement, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
										UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
									}
								}
								else
								{
									// the current subzone is partially enclosed within the radius
									// => only add the objects within the right area

									lock (startElement)
									{
										UnsafeAddToListWithDistanceCheck(startElement, coordinate, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements, ignoreZ);
										UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
									}
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
					log.Debug("Zone" + ID + " " + inZoneElements.Count + " objects moved inside zone");
				}
			}

			if (outOfZoneElements.Count > 0)
			{
				PlaceElementsInOtherZones(outOfZoneElements);

				if (log.IsDebugEnabled)
				{
					log.Debug("Zone" + ID + " " + outOfZoneElements.Count + " objects moved outside zone");
				}
			}


			return partialList;
		}


		private void UnsafeAddToListWithoutDistanceCheck(SubNodeElement startElement, int typeIndex, int subZoneIndex, ArrayList partialList, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
		{
			SubNodeElement currentElement = startElement.next;
			SubNodeElement elementToRemove = null;
			GameObject currentObject = null;

			do
			{
				currentObject = currentElement.data;
				if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
				{
					elementToRemove = currentElement;
					currentElement = currentElement.next;

					elementToRemove.Remove();

					if (log.IsDebugEnabled)
					{
						log.Debug("Zone" + ID + ": " + ((currentObject != null) ? "object " + currentObject.ObjectID : "ghost object") + " removed for subzone");
					}
				}
				else
				{
					// the current object exists, is Active and still in the current subzone
					// => add it
					if (!partialList.Contains(currentObject))
					{
						partialList.Add(currentObject);
					}

					currentElement = currentElement.next;
				}
			} while (currentElement != startElement);
		}


		private void UnsafeAddToListWithDistanceCheck(
			SubNodeElement startElement,
			Coordinate coordinate,
			uint sqRadius,
			int typeIndex,
			int subZoneIndex,
			ArrayList partialList,
			DOL.GS.Collections.Hashtable inZoneElements,
			DOL.GS.Collections.Hashtable outOfZoneElements,
			bool ignoreZ)
		{

			// => check all distances for all objects in the subzone

			SubNodeElement currentElement = startElement.next;
			SubNodeElement elementToRemove = null;
			GameObject currentObject = null;

			do
			{
				currentObject = currentElement.data;

				if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
				{
					elementToRemove = currentElement;
					currentElement = currentElement.next;

					elementToRemove.Remove();

					if (log.IsDebugEnabled)
					{
						log.Debug("Zone" + ID + ": " + ((currentObject != null) ? "object " + currentObject.ObjectID : "ghost object") + " removed for subzone");
					}
				}
				else
				{
					if (CheckSquareDistance(coordinate, currentObject.Coordinate, sqRadius, ignoreZ) && !partialList.Contains(currentObject))
					{
						// the current object exists, is Active and still in the current subzone
						// moreover it is in the right range and not yet in the result set
						// => add it
						partialList.Add(currentObject);
					}

					currentElement = currentElement.next;
				}
			} while (currentElement != startElement);
		}


		#region Relocation

		internal void Relocate(object state)
		{
			if (!m_initialized) return;
			if (m_objectCount > 0)
			{
				SubNodeElement startElement = null;
				SubNodeElement currentElement = null;

				SubNodeElement elementToRemove = null;

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
							startElement = m_subZoneElements[subZoneIndex][typeIndex];

							lock (startElement)
							{
								if (startElement != startElement.next)
								{
									// there are some elements in the list

									currentElement = startElement.next;

									do
									{
										if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
										{
											elementToRemove = currentElement;
											currentElement = currentElement.next;

											elementToRemove.Remove();

											if (log.IsDebugEnabled)
											{
												log.Debug("Zone" + ID + " object " + elementToRemove.data.ObjectID + " removed for subzone");
											}
										}
										else
										{
											currentElement = currentElement.next;
										}
									} while (currentElement != startElement);

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
						log.Debug("Zone" + ID + " " + inZoneElements.Count + " objects moved inside zone");
					}
				}

				if (outOfZoneElements.Count > 0)
				{
					PlaceElementsInOtherZones(outOfZoneElements);

					if (log.IsDebugEnabled)
					{
						log.Debug("Zone" + ID + " " + outOfZoneElements.Count + " objects moved outside zone");
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


		private bool ShouldElementMove(SubNodeElement currentElement, int typeIndex, int subZoneIndex, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
		{

			if (!m_initialized) InitializeZone();
			bool removeElement = true;
			GameObject currentObject = currentElement.data;

			if ((currentObject != null) &&
			    (((int)currentObject.ObjectState) == (int)GameObject.eObjectState.Active)
			    && (currentObject.CurrentRegion == ZoneRegion))
			{
				// the current object exists, is Active and still in the Region where this Zone is located

				int currentElementSubzoneIndex = GetSubZoneIndex(currentObject.Coordinate);

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

					Interlocked.Decrement(ref m_objectCount);
				}
				else
				{
					// the object is still inside this Zone

					if (removeElement = (currentElementSubzoneIndex != subZoneIndex))
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
					}
				}
			}
			else
			{
				// ghost object
				// => remove it

				Interlocked.Decrement(ref m_objectCount);
			}

			return removeElement;
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
					currentZone = ZoneRegion.GetZone(currentElement.data.Coordinate);

					if (currentZone != null)
					{
						currentZone.ObjectEnterZone((eGameObjectType)currentType, currentElement);
					}
				}
			}
		}

        #endregion

        public static bool CheckSquareDistance(Coordinate locA, Coordinate locB, uint squaredDistance, bool ignoreZ)
        {
            int xDiff = locA.X - locB.X;
            var dist = ((long)xDiff) * xDiff;

            if (dist > squaredDistance) return false;

            int yDiff = locA.Y - locB.Y;
            dist += ((long)yDiff) * yDiff;

            if (dist > squaredDistance) return false;

            if (ignoreZ == false)
            {
                int zDiff = locA.Z - locB.Z;
                dist += ((long)zDiff) * zDiff;
            }

            if (dist > squaredDistance) return false;

            return true;
        }


		/// <summary>
		/// Checks that the minimal distance between a point and a subzone (defined by the four position of the sides) is lower or equal
		/// to the distance (given as a square distance)
		/// PRECONDITION : the point is not in the tested subzone
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
				int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
				distance = (long)xdiff * xdiff;
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

		#region Area functions

		public IList<IArea> GetAreasOfSpot(Coordinate spot)
            => m_Region.GetAreasOfZone(this, spot, true);

		#endregion

		#region Get random NPC

		/// <summary>
		/// Get's a random NPC based on a con level
		/// </summary>
		/// <param name="realm"></param>
		/// <param name="compareLevel"></param>
		/// <param name="conLevel">-3 grey, -2 green, -1 blue, 0 yellow, 1 - orange, 2 red, 3 purple</param>
		/// <returns></returns>
		public GameNPC GetRandomNPCByCon(eRealm realm, int compareLevel, int conLevel)
		{
			List<GameNPC> npcs = GetNPCsOfZone(new eRealm[] { realm }, 0, 0, compareLevel, conLevel, true);
			GameNPC randomNPC = (npcs.Count == 0 ? null : npcs[0]);
			return randomNPC;
		}

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
			List<GameNPC> npcs = GetNPCsOfZone(realms, minLevel, maxLevel, 0, 0, true);
			GameNPC randomNPC = (npcs.Count == 0 ? null : npcs[0]);
			return randomNPC;
		}

		/// <summary>
		/// Gets all NPC's in zone
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public List<GameNPC> GetNPCsOfZone(eRealm realm)
		{
			return GetNPCsOfZone(new eRealm[] { realm }, 0, 0, 0, 0, false);
		}


		/// <summary>
		/// Get NPCs of a zone given various parameters
		/// </summary>
		/// <param name="realms"></param>
		/// <param name="minLevel"></param>
		/// <param name="maxLevel"></param>
		/// <param name="compareLevel"></param>
		/// <param name="conLevel"></param>
		/// <param name="firstOnly"></param>
		/// <returns></returns>
		public List<GameNPC> GetNPCsOfZone(eRealm[] realms, int minLevel, int maxLevel, int compareLevel, int conLevel, bool firstOnly)
		{
			List<GameNPC> list = new List<GameNPC>();

			try
			{
				if (!m_initialized) InitializeZone();
				// select random starting subzone and iterate over all objects in subzone than in all subzone...
				int currentSubZoneIndex = Util.Random(SUBZONE_NBR);
				int startSubZoneIndex = currentSubZoneIndex;
				GameNPC currentNPC = null;
				bool stopSearching = false;
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
								bool added = false;
								// Check for specified realms
								for (int i = 0; i < realms.Length; ++i)
								{
									eRealm realm = realms[i];
									if (currentNPC.Realm == realm)
									{
										// Check for min-max level, if any specified
										bool addToList = true;
										if (compareLevel > 0 && conLevel > 0)
											addToList = ((int)GameObject.GetConLevel(compareLevel, currentNPC.Level) == conLevel);
										else
										{
											if (minLevel > 0 && currentNPC.Level < minLevel)
												addToList = false;
											if (maxLevel > 0 && currentNPC.Level > maxLevel)
												addToList = false;
										}
										if (addToList)
										{
											list.Add(currentNPC);
											added = true;
											break;
										}
									}
								}
								// If we have added and must return one only result,
								// then mark for stop searching
								if (firstOnly && added)
								{
									stopSearching = true;
									break;
								}
								curElement = curElement.next;
							} while (curElement != startElement);
						}
					}
					if (++currentSubZoneIndex >= SUBZONE_NBR)
					{
						currentSubZoneIndex = 0;
					}
					// If stop searching forced, then exit
					if (stopSearching)
						break;
				} while (currentSubZoneIndex != startSubZoneIndex);
			}
			catch (Exception ex)
			{
				log.Error("GetNPCsOfZone: Caught Exception for zone " + Description + ".", ex);
			}

			return list;
		}

		#endregion

		#region Zone Bonuses
		/// <summary>
		/// Bonus XP Gained (%)
		/// </summary>
		public int BonusExperience
		{
			get { return m_bonusXP; }
			set { m_bonusXP = value; }
		}
		/// <summary>
		/// Bonus RP Gained (%)
		/// </summary>
		public int BonusRealmpoints
		{
			get { return m_bonusRP; }
			set { m_bonusRP = value; }
		}
		/// <summary>
		/// Bonus BP Gained (%)
		/// </summary>
		public int BonusBountypoints
		{
			get { return m_bonusBP; }
			set { m_bonusBP = value; }
		}
		/// <summary>
		/// Bonus Money Gained (%)
		/// </summary>
		public int BonusCoin
		{
			get { return m_bonusCoin; }
			set { m_bonusCoin = value; }
		}
		#endregion
	}
}
