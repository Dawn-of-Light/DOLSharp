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
		private const ushort SUBZONE_NBR = (SUBZONE_NBR_ON_ZONE_SIDE * SUBZONE_NBR_ON_ZONE_SIDE);

		private const ushort SUBZONE_SIZE = (65536 / SUBZONE_NBR_ON_ZONE_SIDE);

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
		/// already initialized?
		/// </summary>
		private bool m_initialized = false;

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
			ZoneRegion = region;
			ID = id;
			Description = desc;
			XOffset = xoff;
			YOffset = yoff;
			Width = width;
			Height = height;
			ZoneSkinID = zoneskinID;
			Waterlevel = waterlevel;
			IsDivingEnabled = isDivingEnabled;
			IsLava = islava;

			BonusExperience = xpBonus;
			BonusRealmpoints = rpBonus;
			BonusBountypoints = bpBonus;
			BonusCoin = coinBonus;

			// initialise subzone objects and counters
			m_subZoneElements = new SubNodeElement[SUBZONE_NBR][];
			m_initialized = false;
            Realm = (eRealm)realm;

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
			ZoneRegion = null;
			Events.GameEventMgr.RemoveAllHandlersForObject(this);
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
        public virtual LanguageDataObject.eTranslationIdentifier TranslationIdentifier => LanguageDataObject.eTranslationIdentifier.eZone;

	    public string TranslationId
        {
            get { return ID.ToString(); }
            set { }
        }

        public eRealm Realm { get; }

	    public bool IsDungeon
		{
			get
			{
				switch (ZoneRegion.ID)
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
		public Region ZoneRegion { get; set; }

	    /// <summary>
		/// Returns the ID of this zone
		/// </summary>
		public ushort ID { get; }

	    //Dinberg: added for instances.
		/// <summary>
		/// The ID we send to the client, for client-side positioning of gameobjects and npcs.
		/// </summary>
		public ushort ZoneSkinID { get; }

	    /// <summary>
		/// Return the description of this zone
		/// </summary>
		public string Description { get; set; }

	    /// <summary>
		/// Returns the XOffset of this Zone
		/// </summary>
		public int XOffset { get; }

	    /// <summary>
		/// Returns the YOffset of this Zone
		/// </summary>
		public int YOffset { get; }

	    /// <summary>
		/// Returns the Width of this Zone
		/// </summary>
		public int Width { get; }

	    /// <summary>
		/// Returns the Height of this Zone
		/// </summary>
		public int Height { get; }

	    public int Waterlevel { get; set; }

	    public bool IsDivingEnabled { get; set; }

	    /// <summary>
		/// Is water in this zone lava?
		/// </summary>
		public virtual bool IsLava { get; set; }

	    /// <summary>
		/// Returns the total number of objects held in the zone
		/// </summary>
		public int TotalNumberOfObjects => m_objectCount;

	    #endregion

		#region New subzone Management function

		private short GetSubZoneOffset(int lineSubZoneIndex, int columnSubZoneIndex)
		{
			return (short)(columnSubZoneIndex + (lineSubZoneIndex << SUBZONE_ARRAY_Y_SHIFT));
		}


		/// <summary>
		/// Returns the SubZone index using a position in the zone
		/// </summary>
		/// <param name="p_X">X position</param>
		/// <param name="p_Y">Y position</param>
		/// <returns>The SubZoneIndex</returns>
		private short GetSubZoneIndex(int p_X, int p_Y)
		{
			int xDiff = p_X - XOffset;
			int yDiff = p_Y - YOffset;

			if ((xDiff < 0) || (xDiff > 65535) || (yDiff < 0) || (yDiff > 65535))
			{
				// the object is out of the zone
				return -1;
			}

			// the object is in the zone
			//DOLConsole.WriteWarning("GetSubZoneIndex : " + SUBZONE_NBR_ON_ZONE_SIDE + ", " + SUBZONE_NBR + ", " + SUBZONE_SHIFT + ", " + SUBZONE_ARRAY_Y_SHIFT);

			xDiff >>= SUBZONE_SHIFT;
			yDiff >>= SUBZONE_SHIFT;

			return GetSubZoneOffset(yDiff, xDiff);
		}


		/// <summary>
		/// Get the index of the subzone from the GameObject position
		/// </summary>
		/// <param name="p_Obj">The GameObject</param>
		/// <returns>The index of the subzone</returns>
		private short GetSubZoneIndex(GameObject p_Obj)
		{
			return GetSubZoneIndex(p_Obj.X, p_Obj.Y);
		}


		/// <summary>
		/// Handle a GameObject entering a zone
		/// </summary>
		/// <param name="p_Obj">The GameObject object</param>
		public void ObjectEnterZone(GameObject p_Obj)
		{

			if (!m_initialized) InitializeZone();
			int subZoneIndex = GetSubZoneIndex(p_Obj);
			if ((subZoneIndex >= 0) && (subZoneIndex < SUBZONE_NBR))
			{
				SubNodeElement element = new SubNodeElement();
				element.data = p_Obj;

				int type = -1;

				//Only GamePlayer, GameNPC and GameStaticItem classes
				//are handled.
				if (p_Obj is GamePlayer)
				{
				    type = (int)eGameObjectType.PLAYER;
				}
				else if (p_Obj is GameNPC)
				{
				    type = (int)eGameObjectType.NPC;
				}
				else if (p_Obj is GameStaticItem)
				{
				    type = (int)eGameObjectType.ITEM;
				}
				else if (p_Obj is IDoor)
				{
				    type = (int)eGameObjectType.DOOR;
				}

				if (type == -1)
				{
				    return;
				}

				if (log.IsDebugEnabled)
				{
					log.Debug($"Object {p_Obj.ObjectID} ({((eGameObjectType) type)}) entering subzone {subZoneIndex} in zone {ID} in region {ZoneRegion.ID}");
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
			int subZoneIndex = GetSubZoneIndex(element.data);

			if (log.IsDebugEnabled)
			{
				log.Debug($"Object {element.data.ObjectID}({objectType}) entering (inner) subzone {subZoneIndex} in zone {ID} in region {ZoneRegion.ID}");
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
		/// <param name="type">the type of objects to look for</param>
		/// <param name="x">the x coordinate of the observation position</param>
		/// <param name="y">the y coordinate of the observation position</param>
		/// <param name="z">the z coordinate of the observation position</param>
		/// <param name="radius">the radius to check against</param>
		/// <param name="partialList">an initial (eventually empty but initialized, i.e. never null !!) list of objects</param>
		/// <returns>partialList augmented with the new objects verigying both type and radius in the current Zone</returns>
		internal ArrayList GetObjectsInRadius(eGameObjectType type, int x, int y, int z, ushort radius, ArrayList partialList, bool ignoreZ)
		{
		    if (!m_initialized)
		    {
		        InitializeZone();
		    }

			// initialise parameters
			uint sqRadius = radius * (uint)radius;
			int referenceSubzoneIndex = GetSubZoneIndex(x, y);
			int typeIndex = (int)type;

			int xInZone = x - XOffset; // x in zone coordinates
			int yInZone = y - YOffset; // y in zone coordinates

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

            Collections.Hashtable inZoneElements = new Collections.Hashtable();
            Collections.Hashtable outOfZoneElements = new Collections.Hashtable();

			for (int currentLine = minLine; currentLine <= maxLine; ++currentLine)
			{
				int currentSubZoneIndex;
				SubNodeElement startElement;

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
								UnsafeAddToListWithDistanceCheck(startElement, x, y, z, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements, ignoreZ);
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
										UnsafeAddToListWithDistanceCheck(startElement, x, y, z, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements, ignoreZ);
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
					log.Debug($"Zone{ID} {inZoneElements.Count} objects moved inside zone");
				}
			}

			if (outOfZoneElements.Count > 0)
			{
				PlaceElementsInOtherZones(outOfZoneElements);

				if (log.IsDebugEnabled)
				{
					log.Debug($"Zone{ID} {outOfZoneElements.Count} objects moved outside zone");
				}
			}


			return partialList;
		}


		private void UnsafeAddToListWithoutDistanceCheck(SubNodeElement startElement, int typeIndex, int subZoneIndex, ArrayList partialList, Collections.Hashtable inZoneElements, Collections.Hashtable outOfZoneElements)
		{
			SubNodeElement currentElement = startElement.next;
			SubNodeElement elementToRemove;
			GameObject currentObject;

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
						log.Debug($"Zone{ID}: {(currentObject != null ? $"object {currentObject.ObjectID}" : "ghost object")} removed for subzone");
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
			int x,
			int y,
			int z,
			uint sqRadius,
			int typeIndex,
			int subZoneIndex,
			ArrayList partialList,
            Collections.Hashtable inZoneElements,
            Collections.Hashtable outOfZoneElements,
			bool ignoreZ)
		{

			// => check all distances for all objects in the subzone

			SubNodeElement currentElement = startElement.next;
			SubNodeElement elementToRemove;
			GameObject currentObject;

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
						log.Debug($"Zone{ID}: {(currentObject != null ? $"object {currentObject.ObjectID}" : "ghost object")} removed for subzone");
					}
				}
				else
				{
					if (CheckSquareDistance(x, y, z, currentObject.X, currentObject.Y, currentObject.Z, sqRadius, ignoreZ) && !partialList.Contains(currentObject))
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
				SubNodeElement startElement;
				SubNodeElement currentElement;
				SubNodeElement elementToRemove;

                Collections.Hashtable outOfZoneElements = new Collections.Hashtable();
                Collections.Hashtable inZoneElements = new Collections.Hashtable();

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
												log.Debug($"Zone{ID} object {elementToRemove.data.ObjectID} removed for subzone");
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
						log.Debug($"Zone{ID} {inZoneElements.Count} objects moved inside zone");
					}
				}

				if (outOfZoneElements.Count > 0)
				{
					PlaceElementsInOtherZones(outOfZoneElements);

					if (log.IsDebugEnabled)
					{
						log.Debug($"Zone{ID} {outOfZoneElements.Count} objects moved outside zone");
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


		private bool ShouldElementMove(SubNodeElement currentElement, int typeIndex, int subZoneIndex, Collections.Hashtable inZoneElements, Collections.Hashtable outOfZoneElements)
		{

			if (!m_initialized) InitializeZone();
			bool removeElement = true;
			GameObject currentObject = currentElement.data;

			if (currentObject != null &&
			    (int)currentObject.ObjectState == (int)GameObject.eObjectState.Active &&
                currentObject.CurrentRegion == ZoneRegion)
			{
				// the current object exists, is Active and still in the Region where this Zone is located

				int currentElementSubzoneIndex = GetSubZoneIndex(currentObject.X, currentObject.Y);

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


		private void PlaceElementsInZone(Collections.Hashtable elements)
		{
            Collections.DictionaryEntry currentEntry;
			ArrayList currentList;
			SubNodeElement currentStartElement;
			SubNodeElement currentElement;

			IEnumerator entryEnumerator = elements.GetEntryEnumerator();
			while (entryEnumerator.MoveNext())
			{
				currentEntry = (Collections.DictionaryEntry)entryEnumerator.Current;
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


		private void PlaceElementsInOtherZones(Collections.Hashtable elements)
		{
            Collections.DictionaryEntry currentEntry;
			int currentType;
			ArrayList currentList;
			Zone currentZone;
			SubNodeElement currentElement;

			IEnumerator entryEnumerator = elements.GetEntryEnumerator();
			while (entryEnumerator.MoveNext())
			{
				currentEntry = (Collections.DictionaryEntry)entryEnumerator.Current;
				currentType = (int)currentEntry.key;
				currentList = (ArrayList)currentEntry.value;

				for (int i = 0; i < currentList.Count; i++)
				{
					currentElement = (SubNodeElement)currentList[i];
					currentZone = ZoneRegion.GetZone(currentElement.data.X, currentElement.data.Y);

				    currentZone?.ObjectEnterZone((eGameObjectType)currentType, currentElement);
				}
			}
		}

		#endregion

		/// <summary>
		/// Checks that the square distance between two arbitary points in space is lower or equal to the given square distance
		/// </summary>
		/// <param name="x1">X of Point1</param>
		/// <param name="y1">Y of Point1</param>
		/// <param name="z1">Z of Point1</param>
		/// <param name="x2">X of Point2</param>
		/// <param name="y2">Y of Point2</param>
		/// <param name="z2">Z of Point2</param>
		/// <param name="sqDistance">the square distance to check for</param>
		/// <returns>The distance</returns>
		public static bool CheckSquareDistance(int x1, int y1, int z1, int x2, int y2, int z2, uint sqDistance, bool ignoreZ)
		{
			int xDiff = x1 - x2;
			long dist = (long)xDiff * xDiff;

			if (dist > sqDistance)
			{
				return false;
			}

			int yDiff = y1 - y2;
			dist += (long)yDiff * yDiff;

			if (dist > sqDistance)
			{
				return false;
			}

			if (ignoreZ == false)
			{
				int zDiff = z1 - z2;
				dist += (long)zDiff * zDiff;
			}

			return dist <= sqDistance;
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
			long distance;

			if (y >= yTop && y <= yBottom)
			{
				int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
				distance = (long)xdiff * xdiff;
			}
			else
			{
				if (x >= xLeft && x <= xRight)
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

			return distance <= squareRadius;
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
			long distance;

			int xdiff = Math.Max(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
			int ydiff = Math.Max(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
			distance = (long)xdiff * xdiff + (long)ydiff * ydiff;

			return distance <= squareRadius;
		}

		#endregion

		#region Area functions

		/// <summary>
		/// Convinientmethod for Region.GetAreasOfZone(),
		/// since zone.Region.getAreasOfZone(zone,x,y,z) is a bit confusing ...
		/// </summary>
		/// <param name="spot"></param>
		/// <returns></returns>
		public IList<IArea> GetAreasOfSpot(IPoint3D spot)
		{
			return GetAreasOfSpot(spot, true);
		}

		public IList<IArea> GetAreasOfSpot(int x, int y, int z)
		{
			return ZoneRegion.GetAreasOfZone(this, x, y, z);
		}

		public IList<IArea> GetAreasOfSpot(IPoint3D spot, bool checkZ)
		{
			return ZoneRegion.GetAreasOfZone(this, spot, checkZ);
		}

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
			List<GameNPC> npcs = GetNPCsOfZone(new[] { realm }, 0, 0, compareLevel, conLevel, true);
			GameNPC randomNPC = npcs.Count == 0 ? null : npcs[0];
			return randomNPC;
		}

		/// <summary>
		/// Get a random NPC belonging to a realm
		/// </summary>
		/// <param name="realm">The realm the NPC belong to</param>
		/// <returns>a npc</returns>
		public GameNPC GetRandomNPC(eRealm realm)
		{
			return GetRandomNPC(new[] { realm }, -1, -1);
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
			return GetRandomNPC(new[] { realm }, minLevel, maxLevel);
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
			GameNPC randomNPC = npcs.Count == 0 ? null : npcs[0];
			return randomNPC;
		}

		/// <summary>
		/// Gets all NPC's in zone
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public List<GameNPC> GetNPCsOfZone(eRealm realm)
		{
			return GetNPCsOfZone(new[] { realm }, 0, 0, 0, 0, false);
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
				GameNPC currentNPC;
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
										{
										    addToList = (int)GameObject.GetConLevel(compareLevel, currentNPC.Level) == conLevel;
										}
										else
										{
											if (minLevel > 0 && currentNPC.Level < minLevel)
											{
											    addToList = false;
											}

											if (maxLevel > 0 && currentNPC.Level > maxLevel)
											{
											    addToList = false;
											}
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
					{
					    break;
					}

				} while (currentSubZoneIndex != startSubZoneIndex);
			}
			catch (Exception ex)
			{
				log.Error($"GetNPCsOfZone: Caught Exception for zone {Description}.", ex);
			}

			return list;
		}

		#endregion

		#region Zone Bonuses
		/// <summary>
		/// Bonus XP Gained (%)
		/// </summary>
		public int BonusExperience { get; set; } = 0;

	    /// <summary>
		/// Bonus RP Gained (%)
		/// </summary>
		public int BonusRealmpoints { get; set; } = 0;

	    /// <summary>
		/// Bonus BP Gained (%)
		/// </summary>
		public int BonusBountypoints { get; set; } = 0;

	    /// <summary>
		/// Bonus Money Gained (%)
		/// </summary>
		public int BonusCoin { get; set; } = 0;

	    #endregion
	}
}
