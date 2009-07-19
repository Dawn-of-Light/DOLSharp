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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a region in DAOC. A region is everything where you
	/// need a loadingscreen to go there. Eg. whole Albion is one Region, Midgard and
	/// Hibernia are just one region too. Darkness Falls is a region. Each dungeon, city
	/// is a region ... you get the clue. Each Region can hold an arbitary number of
	/// Zones! Camelot Hills is one Zone, Tir na Nog is one Zone (and one Region)...
	/// </summary>
	public class Region
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Region Variables
		/// <summary>
		/// IMPORTANT: This variable defines the maximum number of objects
		/// that can exist in a realm! Setting it lower will quicken up certain
		/// loops but you can not add more than this number of objects to the realm
		/// at any given time then! Setting it higher will slow down certain
		/// loops but allows for more objects
		/// </summary>
		public static readonly int MAXOBJECTS = 30000;

		/// <summary>
		/// This is the minimumsize for object array that is allocated when
		/// the first object is added to the region must be dividable by 32 (optimization)
		/// </summary>
		public static readonly int MINIMUMSIZE = 256;


		/// <summary>
		/// This holds all objects inside this region. Their index = their id!
		/// </summary>
		private GameObject[] m_objects;

		/// <summary>
		/// Object to lock when changing objects in the array
		/// </summary>
		public readonly object ObjectsSyncLock = new object();

		/// <summary>
		/// This holds a counter with the absolute count of all objects that are actually in this region
		/// </summary>
		private int m_objectsInRegion;

		/// <summary>
		/// This array holds a bitarray
		/// Its used to know which slots in region object array are free and what allocated
		/// This is used to accelerate inserts a lot
		/// </summary>
		protected uint[] m_objectsAllocatedSlots;

		/// <summary>
		/// This holds the index of a possible next object slot
		/// but needs further checks (basically its lastaddedobjectIndex+1)
		/// </summary>
		protected int m_nextObjectSlot;

		/// <summary>
		/// This holds the gravestones in this region for fast access
		/// Player unique id(string) -> GameGraveStone
		/// </summary>
		protected readonly Hashtable m_graveStones;

		/// <summary>
		/// Holds all the Zones inside this Region
		/// </summary>
		protected readonly ArrayList m_Zones;

		/// <summary>
		/// Holds all the Areas inside this Region
		///
		/// Areas can be registed to a reagion via AddArea
		/// and events will be thrown if players/npcs/objects enter leave area
		/// </summary>
		protected readonly ArrayList m_Areas;

		public ArrayList Areas
		{
			get { return m_Areas; }
		}

		/// <summary>
		/// Cache for zone area mapping to quickly access all areas within a certain zone
		/// </summary>
		protected readonly ushort[][] m_ZoneAreas;

		/// <summary>
		/// /// Cache for number of items in m_ZoneAreas array.
		/// </summary>
		protected readonly ushort[] m_ZoneAreasCount;

		/// <summary>
		/// How often shall we remove unused objects
		/// </summary>
		protected static readonly int CLEANUPTIMER = 60000;

		/// <summary>
		/// Contains the # of players in the region
		/// </summary>
		protected int m_numPlrs = 0;

		/// <summary>
		/// last relocation time
		/// </summary>
		private long m_lastRelocation = 0;

		/// <summary>
		/// The region time manager
		/// </summary>
		protected readonly GameTimer.TimeManager m_timeManager;

		#endregion

		#region Constructor

		private RegionData m_regionData;
		public RegionData RegionData
		{
			get { return m_regionData; }
		}

		/// <summary>
		/// Constructs a new empty Region
		/// </summary>
		/// <param name="time">The time manager for this region</param>
		/// <param name="data">The region data</param>
		public Region(GameTimer.TimeManager time, RegionData data)
		{
			m_regionData = data;
			m_objects = new GameObject[0];
			m_objectsInRegion = 0;
			m_nextObjectSlot = 0;
			m_objectsAllocatedSlots = new uint[0];

			m_graveStones = new Hashtable();

			m_Zones = new ArrayList(1);
			m_ZoneAreas = new ushort[64][];
			m_ZoneAreasCount = new ushort[64];
			for (int i = 0; i < 64; i++)
			{
				m_ZoneAreas[i] = new ushort[AbstractArea.MAX_AREAS_PER_ZONE];
			}

			m_Areas = new ArrayList(1);

			m_timeManager = time;

			string[] list = null; 

			if (ServerProperties.Properties.DEBUG_LOAD_REGIONS != string.Empty)
				list = ServerProperties.Properties.DEBUG_LOAD_REGIONS.Split(';');

			if (list != null && list.Length > 0)
			{
				m_loadObjects = false;

				foreach (string region in list)
				{
					if (region.ToString() == ID.ToString())
					{
						m_loadObjects = true;
						break;
					}
				}
			}

			list = ServerProperties.Properties.DISABLED_REGIONS.Split(';');
			foreach (string region in list)
			{
				if (region.ToString() == ID.ToString())
				{
					m_isDisabled = true;
					break;
				}
			}

			list = ServerProperties.Properties.DISABLED_EXPANSIONS.Split(';');
			foreach (string expansion in list)
			{
				if (expansion.ToString() == m_regionData.Expansion.ToString())
				{
					m_isDisabled = true;
					break;
				}
			}
		}

		#endregion

		#region Properties

		public bool IsRvR
		{
			get
			{
				switch (m_regionData.Id)
				{
					case 163://new frontiers
					case 165://cathal valley
					case 233://Sumoner hall
					case 234://1to4BG
					case 235://5to9BG
					case 236://10to14BG
					case 237://15to19BG
					case 238://20to24BG
					case 239://25to29BG
					case 240://30to34BG
					case 241://35to39BG
					case 242://40to44BG and Test BG
					case 244://Frontiers RvR dungeon
					case 249://Darkness Falls - RvR dungeon
						return true;
					default:
						return false;
				}
			} 
		}

		/// <summary>
		/// Is the Region instanced
		/// </summary>
		public virtual bool IsInstance
		{
			get
			{
                //Dinberg: Region will always return false now, because only class type 'Instance' will return true.
				return false;
			}
		}

		/// <summary>
		/// Gets whether this region is a dungeon or not
		/// </summary>
		public bool IsDungeon
		{
			get
			{
				const int dungeonOffset = 8192;
				const int zoneCount = 1;

				if (Zones.Count != zoneCount)
					return false; //Dungeons only have 1 zone!

				Zone zone = (Zone)Zones[0];

				if (zone.XOffset == dungeonOffset && zone.YOffset == dungeonOffset)
					return true; //Only dungeons got this offset

				return false;
			}
		}

		/// <summary>
		/// Gets the # of players in the region
		/// </summary>
		public int NumPlayers
		{
			get { return m_numPlrs; }
		}

		/// <summary>
		/// The Region Name eg. Region000
		/// </summary>
		public virtual string Name
		{
			get { return m_regionData.Name; }
		}
        //Dinberg: Changed this to virtual, so that Instances can take a unique Name, for things like quest instances.

		/// <summary>
		/// The Region Description eg. Cursed Forest
		/// </summary>
		public virtual string Description
		{
			get { return m_regionData.Description; }
		}
        //Dinberg: Virtual, so that we can change this if need be, for quests eg 'Hermit Dinbargs Cave'
        //or for the hell of it, eg Jordheim (Instance).

		/// <summary>
		/// The ID of the Region eg. 21
		/// </summary>
		public virtual ushort ID
		{
			get { return m_regionData.Id; }
		}
        //Dinberg: Changed this to virtual, so that Instances can take a unique ID.

		/// <summary>
		/// The Region Server IP ... for future use
		/// </summary>
		public string ServerIP
		{
			get { return m_regionData.Ip; }
		}

		/// <summary>
		/// The Region Server Port ... for future use
		/// </summary>
		public ushort ServerPort
		{
			get { return m_regionData.Port; }
		}

		/// <summary>
		/// An ArrayList of all Zones within this Region
		/// </summary>
		public ArrayList Zones
		{
			get { return m_Zones; }
		}

		/// <summary>
		/// Returns the object array of this region
		/// </summary>
		public GameObject[] Objects
		{
			get { return m_objects; }
		}

		/// <summary>
		/// Gets or Sets the region expansion (we use client expansion + 1)
		/// </summary>
		public int Expansion
		{
			get { return m_regionData.Expansion + 1; }
		}

		/// <summary>
		/// Gets or Sets the water level in this region
		/// </summary>
		public int WaterLevel
		{
			get { return m_regionData.WaterLevel; }
		}

		/// <summary>
		/// Gets or Sets diving flag for region
		/// </summary>
		public bool DivingEnabled
		{
			get { return m_regionData.DivingEnabled; }
		}

		/// <summary>
		/// Gets or Sets housing flag for region
		/// </summary>
		public bool HousingEnabled
		{
			get { return m_regionData.HousingEnabled; }
		}

		/// <summary>
		/// Gets last relocation time
		/// </summary>
		public long LastRelocation
		{
			get { return m_lastRelocation; }
		}

		/// <summary>
		/// Gets the region time manager
		/// </summary>
		public GameTimer.TimeManager TimeManager
		{
			get { return m_timeManager; }
		}

		/// <summary>
		/// Gets the current region time in milliseconds
		/// </summary>
		public long Time
		{
			get { return m_timeManager.CurrentTime; }
		}

		private bool m_isDisabled = false;
		/// <summary>
		/// Is this region disabled
		/// </summary>
		public bool IsDisabled
		{
			get { return m_isDisabled; }
		}

		private bool m_loadObjects = true;
		/// <summary>
		/// Will this region load objects
		/// </summary>
		public bool LoadObjects
		{
			get { return m_loadObjects; }
		}

		//Dinberg: Added this for instances.
        /// <summary>
        /// Added to allow instances; the 'appearance' of the region, the map the GameClient uses.
        /// </summary>
        public virtual ushort Skin
        { get { return ID; } }

		#endregion

		#region Methods

		/// <summary>
		/// Starts the RegionMgr
		/// </summary>
		public void StartRegionMgr()
		{
			m_timeManager.Start();
			this.Notify(RegionEvent.RegionStart, this);
		}

		/// <summary>
		/// Stops the RegionMgr
		/// </summary>
		public void StopRegionMgr()
		{
			m_timeManager.Stop();
			this.Notify(RegionEvent.RegionStop, this);
		}

		/// <summary>
		/// Reallocates objects array with given size
		/// </summary>
		/// <param name="count">The size of new objects array, limited by MAXOBJECTS</param>
		public void PreAllocateRegionSpace(int count)
		{
			if (count > MAXOBJECTS)
				count = MAXOBJECTS;
			lock (ObjectsSyncLock)
			{
				if (m_objects.Length > count) return;
				GameObject[] newObj = new GameObject[count];
				Array.Copy(m_objects, newObj, m_objects.Length);
				if (count / 32 + 1 > m_objectsAllocatedSlots.Length)
				{
					uint[] slotarray = new uint[count / 32 + 1];
					Array.Copy(m_objectsAllocatedSlots, slotarray, m_objectsAllocatedSlots.Length);
					m_objectsAllocatedSlots = slotarray;
				}
				m_objects = newObj;
			}
		}

		/// <summary>
        /// Loads the region from database
		/// </summary>
		/// <param name="mobObjs"></param>
		/// <param name="mobCount"></param>
		/// <param name="merchantCount"></param>
		/// <param name="itemCount"></param>
		/// <param name="bindCount"></param>
		public void LoadFromDatabase(Mob[] mobObjs, ref long mobCount, ref long merchantCount, ref long itemCount, ref long bindCount)
		{
			if (!LoadObjects)
				return;

			Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
			WorldObject[] staticObjs = (WorldObject[])GameServer.Database.SelectObjects(typeof(WorldObject), "Region = " + ID);
			BindPoint[] bindPoints = (BindPoint[])GameServer.Database.SelectObjects(typeof(BindPoint), "Region = " + ID);
			int count = mobObjs.Length + staticObjs.Length;
			if (count > 0) PreAllocateRegionSpace(count + 100);
			int myItemCount = staticObjs.Length;
			int myMobCount = 0;
			int myMerchantCount = 0;
			int myBindCount = bindPoints.Length;
			string allErrors = string.Empty;

			if (mobObjs.Length > 0)
			{
				foreach (Mob mob in mobObjs)
				{
					GameNPC myMob = null;
					string error = string.Empty;

					if (mob.Guild.Length > 0 && mob.Realm >= 0 && mob.Realm <= (int)eRealm._Last)
					{
						Type type = ScriptMgr.FindNPCGuildScriptClass(mob.Guild, (eRealm)mob.Realm);
						if (type != null)
						{
							try
							{
								Type[] constructorParams;
								if (mob.NPCTemplateID != -1)
								{
									constructorParams = new Type[] { typeof(INpcTemplate) };
									ConstructorInfo handlerConstructor = typeof(GameNPC).GetConstructor(constructorParams);
									INpcTemplate template = NpcTemplateMgr.GetTemplate(mob.NPCTemplateID);
									myMob = (GameNPC)handlerConstructor.Invoke(new object[] { template });
								}
								else
								{
									myMob = (GameNPC)type.Assembly.CreateInstance(type.FullName);
								}
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.Error("LoadFromDatabase", e);
							}
						}
					}


					if (myMob == null)
					{
						string classtype = ServerProperties.Properties.GAMENPC_DEFAULT_CLASSTYPE;

						if (mob.ClassType != null && mob.ClassType.Length > 0 && mob.ClassType != Mob.DEFAULT_NPC_CLASSTYPE)
						{
							classtype = mob.ClassType;
						}

						try
						{
							myMob = (GameNPC)gasm.CreateInstance(classtype, false);
						}
						catch
						{
							error = classtype;
						}

						if (myMob == null)
						{
							foreach (Assembly asm in ScriptMgr.Scripts)
							{
								try
								{
									myMob = (GameNPC)asm.CreateInstance(classtype, false);
									error = string.Empty;
								}
								catch
								{
									error = classtype;
								}

								if (myMob != null)
									break;
							}

							if (myMob == null)
							{
								myMob = new GameNPC();
								error = classtype;
							}
						}
					}

					if (!allErrors.Contains(error))
						allErrors += " " + error + ",";

					if (myMob != null)
					{
						try
						{
							myMob.LoadFromDatabase(mob);

							if (myMob is GameMerchant)
							{
								myMerchantCount++;
							}
							else
							{
								myMobCount++;
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("Failed: " + myMob.GetType().FullName + ":LoadFromDatabase(" + mob.GetType().FullName + ");", e);
							throw e;
						}

						myMob.AddToWorld();
					}
				}
			}

			if (staticObjs.Length > 0)
			{
				foreach (WorldObject item in staticObjs)
				{
					GameStaticItem myItem = null;

					if (item.ClassType != null && item.ClassType.Length > 0)
					{
						try
						{
							myItem = (GameStaticItem)gasm.CreateInstance(item.ClassType, false);
						}
						catch
						{
						}
						if (myItem == null)
						{
							foreach (Assembly asm in ScriptMgr.Scripts)
							{
								try
								{
									myItem = (GameStaticItem)asm.CreateInstance(item.ClassType, false);
								}
								catch
								{
								}
								if (myItem != null)
									break;
							}
							if (myItem == null)
								myItem = new GameStaticItem();
						}
					}
					else
					{
						myItem = new GameStaticItem();
					}

					if (myItem != null)
					{
						myItem.LoadFromDatabase(item);
						myItem.AddToWorld();
						//						if (!myItem.AddToWorld())
						//							log.ErrorFormat("Failed to add the item to the world: {0}", myItem.ToString());
					}
				}
			}

			foreach (BindPoint point in bindPoints)
			{
				AddArea(new Area.BindArea("bind point", point));
			}

			if (myMobCount + myItemCount + myMerchantCount + myBindCount > 0)
			{
				if (log.IsInfoEnabled)
					log.Info(String.Format("Region: {0} ({1}) loaded {2} mobs, {3} merchants, {4} items {5} bindpoints, from DB ({6})", Description, ID, myMobCount, myMerchantCount, myItemCount, myBindCount, TimeManager.Name));

				log.Debug("Used Memory: " + GC.GetTotalMemory(false) / 1024 + "KB");

				if (allErrors != string.Empty)
					log.Error("Error loading the following NPC ClassType(s), GameNPC used instead:" + allErrors.TrimEnd(','));

				Thread.Sleep(0);  // give up remaining thread time to other resources
			}
			mobCount += myMobCount;
			merchantCount += myMerchantCount;
			itemCount += myItemCount;
			bindCount += myBindCount;
		}

		/// <summary>
		/// Adds an object to the region and assigns the object an id
		/// </summary>
		/// <param name="obj">A GameObject to be added to the region</param>
		/// <returns>success</returns>
		internal bool AddObject(GameObject obj)
		{
			//Thread.Sleep(10000);
			Zone zone = GetZone(obj.X, obj.Y);
			if (zone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Zone not found for Object: " + obj.Name + "(ID=" + obj.InternalID + ")");
			}

			//Assign a new id
			lock (ObjectsSyncLock)
			{
				if (obj.ObjectID != -1)
				{
					if (obj.ObjectID < m_objects.Length && obj == m_objects[obj.ObjectID - 1])
					{
						log.WarnFormat("Object is already in the region ({0})", obj.ToString());
						return false;
					}
					log.Warn(obj.Name + " should be added to " + Description + " but had already an OID(" + obj.ObjectID + ") => not added\n" + Environment.StackTrace);
					return false;
				}

				GameObject[] objectsRef = m_objects;

				//*** optimized object management for memory saving primary but keeping it very fast - Blue ***

				// find first free slot for the object
				int objID = m_nextObjectSlot;
				if (objID >= m_objects.Length || m_objects[objID] != null)
				{

					// we are at array end, are there any holes left?
					if (m_objects.Length > m_objectsInRegion)
					{
						// yes there are some places left in current object array, try to find them
						// by using the bit array (can check 32 slots at once!)

						int i = m_objects.Length / 32;
						// INVARIANT: i * 32 is always lower or equal to m_objects.Length (integer division property)
						if (i * 32 == m_objects.Length)
						{
							i -= 1;
						}

						bool found = false;
						objID = -1;

						while (!found && (i >= 0))
						{
							if (m_objectsAllocatedSlots[i] != 0xffffffff)
							{
								// we found a free slot
								// => search for exact place

								int currentIndex = i * 32;
								int upperBound = (i + 1) * 32;
								while (!found && (currentIndex < m_objects.Length) && (currentIndex < upperBound))
								{
									if (m_objects[currentIndex] == null)
									{
										found = true;
										objID = currentIndex;
									}

									currentIndex++;
								}

								// INVARIANT: at this point, found must be true (otherwise the underlying data structure is corrupt)
							}

							i--;
						}
					}
					else
					{ // our array is full, we must resize now to fit new objects

						if (objectsRef.Length == 0)
						{

							// there is no array yet, so set it to a minimum at least
							objectsRef = new GameObject[MINIMUMSIZE];
							Array.Copy(m_objects, objectsRef, m_objects.Length);
							objID = 0;

						}
						else if (objectsRef.Length >= MAXOBJECTS)
						{

							// no available slot
							if (log.IsErrorEnabled)
								log.Error("Can't add new object - region '" + Description + "' is full. (object: " + obj.ToString() + ")");
							return false;

						}
						else
						{

							// we need to add a certain amount to grow
							int size = (int)(m_objects.Length * 1.20);
							if (size < m_objects.Length + 256)
								size = m_objects.Length + 256;
							if (size > MAXOBJECTS)
								size = MAXOBJECTS;
							objectsRef = new GameObject[size]; // grow the array by 20%, at least 256
							Array.Copy(m_objects, objectsRef, m_objects.Length);
							objID = m_objects.Length; // new object adds right behind the last object in old array

						}
						// resize the bitarray as well
						int diff = objectsRef.Length / 32 - m_objectsAllocatedSlots.Length;
						if (diff >= 0)
						{
							uint[] newBitArray = new uint[Math.Max(m_objectsAllocatedSlots.Length + diff + 50, 100)];	// add at least 100 integers, makes it resize less often, serves 3200 new objects, only 400 bytes
							Array.Copy(m_objectsAllocatedSlots, newBitArray, m_objectsAllocatedSlots.Length);
							m_objectsAllocatedSlots = newBitArray;
						}
					}
				}

				if (objID < 0)
				{
					log.Warn("There was an unexpected problem while adding " + obj.Name + " to " + Description);
					return false;
				}

				// if we found a slot add the object
				GameObject oidObj = objectsRef[objID];
				if (oidObj == null)
				{
					objectsRef[objID] = obj;
					m_nextObjectSlot = objID + 1;
					m_objectsInRegion++;
					obj.ObjectID = objID + 1;
					m_objectsAllocatedSlots[objID / 32] |= (uint)1 << (objID % 32);
					Thread.MemoryBarrier();
					m_objects = objectsRef;

					if (obj is GamePlayer)
					{
						++m_numPlrs;
					}
					else
					{
						if (obj is GameGravestone)
						{
							lock (m_graveStones.SyncRoot)
							{
								m_graveStones[obj.InternalID] = obj;
							}
						}
					}

					return true;
				}
				else
				{
					// no available slot
					if (log.IsErrorEnabled)
						log.Error("Can't add new object - region '" + Description + "' (object: " + obj.ToString() + "); OID is used by " + oidObj.ToString());
					return false;
				}
			}
		}

		/// <summary>
		/// Removes the object with the specified ID from the region
		/// </summary>
		/// <param name="obj">A GameObject to be removed from the region</param>
		internal void RemoveObject(GameObject obj)
		{
			lock (ObjectsSyncLock)
			{
				int index = obj.ObjectID - 1;
				if (index < 0)
				{
					return;
				}

				if (obj is GamePlayer)
				{
					--m_numPlrs;
				}
				else
				{
					if (obj is GameGravestone)
					{
						lock (m_graveStones.SyncRoot)
						{
							m_graveStones.Remove(obj.InternalID);
						}
					}
				}


				if (log.IsDebugEnabled)
					log.Debug("RemoveObject: OID" + obj.ObjectID + " " + obj.Name + "(R" + obj.CurrentRegionID + ") from " + Description);

				GameObject inPlace = m_objects[obj.ObjectID - 1];
				if (inPlace == null)
				{
					log.Error("RemoveObject conflict! OID" + obj.ObjectID + " " + obj.Name + "(" + obj.CurrentRegionID + ") but there was no object at that slot");
					log.Error(new StackTrace().ToString());
					return;
				}
				if (obj != inPlace)
				{
					log.Error("RemoveObject conflict! OID" + obj.ObjectID + " " + obj.Name + "(" + obj.CurrentRegionID + ") but there was another object already " + inPlace.Name + " region:" + inPlace.CurrentRegionID + " state:" + inPlace.ObjectState);
					log.Error(new StackTrace().ToString());
					return;
				}

				if (m_objects[index] != obj)
				{
					log.Error("Object OID is already used by another object! (used by:" + m_objects[index].ToString() + ")");
				}
				else
				{
					m_objects[index] = null;
					m_nextObjectSlot = index;
					m_objectsAllocatedSlots[index / 32] &= ~(uint)(1 << (index % 32));
				}
				obj.ObjectID = -1; // invalidate object id
				m_objectsInRegion--;
			}
		}

		/// <summary>
		/// Searches for players gravestone in this region
		/// </summary>
		/// <param name="player"></param>
		/// <returns>the found gravestone or null</returns>
		public GameGravestone FindGraveStone(GamePlayer player)
		{
			lock (m_graveStones.SyncRoot)
			{
				return (GameGravestone)m_graveStones[player.InternalID];
			}
		}

		/// <summary>
		/// Gets the object with the specified ID
		/// </summary>
		/// <param name="id">The ID of the object to get</param>
		/// <returns>The object with the specified ID, null if it didn't exist</returns>
		public GameObject GetObject(ushort id)
		{
			if (id <= 0 || id > m_objects.Length)
				return null;
			return m_objects[id - 1];
		}

		/// <summary>
		/// Returns the zone that contains the specified x and y values
		/// </summary>
		/// <param name="x">X value for the zone you're retrieving</param>
		/// <param name="y">Y value for the zone you're retrieving</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public Zone GetZone(int x, int y)
		{
			int varX = x;
			int varY = y;
			for (int i = 0; i < m_Zones.Count; i++)
			{
				Zone zone = (Zone)m_Zones[i];
				if (zone.XOffset <= varX && zone.YOffset <= varY && (zone.XOffset + zone.Width) > varX && (zone.YOffset + zone.Height) > varY)
					return zone;
			}
			return null;
		}

		/// <summary>
		/// Gets the X offset for the specified zone
		/// </summary>
		/// <param name="x">X value for the zone's offset you're retrieving</param>
		/// <param name="y">Y value for the zone's offset you're retrieving</param>
		/// <returns>The X offset of the zone you specified or 0 if it couldn't be found</returns>
		public int GetXOffInZone(int x, int y)
		{
			Zone z = GetZone(x, y);
			if (z == null)
				return 0;
			return x - z.XOffset;
		}

		/// <summary>
		/// Gets the Y offset for the specified zone
		/// </summary>
		/// <param name="x">X value for the zone's offset you're retrieving</param>
		/// <param name="y">Y value for the zone's offset you're retrieving</param>
		/// <returns>The Y offset of the zone you specified or 0 if it couldn't be found</returns>
		public int GetYOffInZone(int x, int y)
		{
			Zone z = GetZone(x, y);
			if (z == null)
				return 0;
			return y - z.YOffset;
		}

		/// <summary>
		/// Check if this region is a capital city
		/// </summary>
		/// <returns>True, if region is a capital city, else false</returns>
		public bool IsCapitalCity
		{
			get
			{
				switch (this.ID)
				{
					case 10: return true; // Camelot City
					case 101: return true; // Jordheim
					case 201: return true; // Tir na Nog
					default: return false;
				}
			}
		}
		
		/// <summary>
		/// Check if this region is a housing zone
		/// </summary>
		/// <returns>True, if region is a housing zone, else false</returns>
		public bool IsHousing
		{
			get
			{
				switch (this.ID)
				{
					case 2: return true; 	// Housing alb
					case 102: return true; 	// Housing mid
					case 202: return true; 	// Housing hib
					default: return false;
				}
			}
		}
		#endregion

		#region Area

		/// <summary>
		/// Adds an area to the region and updates area-zone cache
		/// </summary>
		/// <param name="area"></param>
		/// <returns></returns>
		public IArea AddArea(IArea area)
		{
			lock (m_Areas.SyncRoot)
			{
				m_Areas.Add(area);
				area.ID = (ushort)(m_Areas.Count - 1);

				for (int i = 0; i < Zones.Count; i++)
				{
					Zone zone = (Zone)Zones[i];
					if (!area.IsIntersectingZone(zone))
						continue;

					m_ZoneAreas[i][m_ZoneAreasCount[i]++] = area.ID;
				}
				return area;
			}
		}

		/// <summary>
		/// Removes an are from the list of areas and updates area-zone chache
		/// </summary>
		/// <param name="area"></param>
		public void RemoveArea(IArea area)
		{
			lock (m_Areas.SyncRoot)
			{
				m_Areas.Remove(area);

				for (int i = 0; i < Zones.Count; i++)
				{
					for (int j = 0; j < m_ZoneAreasCount[i]; j++)
					{
						if (m_ZoneAreas[i][j] == area.ID)
						{
							// skip rest of m_ZoneAreas array one to the left.
							for (int k = j; k < m_ZoneAreasCount[i] - 1; k++)
							{
								m_ZoneAreas[i][k] = m_ZoneAreas[i][k + 1];
							}
							m_ZoneAreasCount[i]--;

							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the areas for given location,
		/// less performant than getAreasOfZone so use other on if possible
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public IList GetAreasOfSpot(IPoint3D point)
		{
			Zone zone = GetZone(point.X, point.Y);
			return GetAreasOfZone(zone, point);
		}

		/// <summary>
		/// Gets the areas for a certain spot,
		/// less performant than getAreasOfZone so use other on if possible
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public IList GetAreasOfSpot(int x, int y, int z)
		{
			Zone zone = GetZone(x, y);
			Point3D p = new Point3D(x, y, z);
			return GetAreasOfZone(zone, p);
		}

		public IList GetAreasOfZone(Zone zone, IPoint3D p)
		{
			return GetAreasOfZone(zone, p, true);
		}

		/// <summary>
        /// Gets the areas for a certain spot
		/// </summary>
		/// <param name="zone"></param>
		/// <param name="p"></param>
		/// <param name="checkZ"></param>
		/// <returns></returns>
		public IList GetAreasOfZone(Zone zone, IPoint3D p, bool checkZ)
		{
			lock (m_Areas.SyncRoot)
			{
				int zoneIndex = Zones.IndexOf(zone);
				IList areas = new ArrayList();

				if (zoneIndex >= 0)
				{
					try
					{
						for (int i = 0; i < m_ZoneAreasCount[zoneIndex]; i++)
						{
							IArea area = (IArea)m_Areas[m_ZoneAreas[zoneIndex][i]];
							if (area.IsContaining(p, checkZ))
								areas.Add(area);
						}
					}
					catch (Exception e)
					{
						log.Error("GetArea exception.Area count " + m_ZoneAreasCount[zoneIndex], e);
					}
				}
				return areas;
			}
		}

		public IList GetAreasOfZone(Zone zone, int x, int y, int z)
		{
			lock (m_Areas.SyncRoot)
			{
				int zoneIndex = Zones.IndexOf(zone);
				IList areas = new ArrayList();

				if (zoneIndex >= 0)
				{
					try
					{
						for (int i = 0; i < m_ZoneAreasCount[zoneIndex]; i++)
						{
							IArea area = (IArea)m_Areas[m_ZoneAreas[zoneIndex][i]];
							if (area.IsContaining(x, y, z))
								areas.Add(area);
						}
					}
					catch (Exception e)
					{
						log.Error("GetArea exception.Area count " + m_ZoneAreasCount[zoneIndex], e);
					}
				}
				return areas;
			}
		}

		#endregion

		#region Notify

		public virtual void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.Notify(e, sender, args);
		}

		public virtual void Notify(DOLEvent e, object sender)
		{
			Notify(e, sender, null);
		}

		public virtual void Notify(DOLEvent e)
		{
			Notify(e, null, null);
		}

		public virtual void Notify(DOLEvent e, EventArgs args)
		{
			Notify(e, null, args);
		}

		#endregion

		#region Object in Radius (Added by Konik & WitchKing)

		#region New Get in radius

		/*protected void ObjectPositionChangedHandler (DOLEvent e, object sender,EventArgs arg)
		{
			ObjectUpdatePositionEventArg oup = (ObjectUpdatePositionEventArg)arg;
			GameLiving glv = oup.GameLiving;
			m_RegionMgr.AddMovingObject(glv);
		}*/


		/// <summary>
		/// Gets objects in a radius around a point
		/// </summary>
		/// <param name="type">OBJECT_TYPE (0=item, 1=npc, 2=player)</param>
		/// <param name="x">origin X</param>
		/// <param name="y">origin Y</param>
		/// <param name="z">origin Z</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		private IEnumerable GetInRadius(Zone.eGameObjectType type, int x, int y, int z, ushort radius, bool withDistance)
		{
			// check if we are around borders of a zone
			Zone startingZone = GetZone(x, y);

			if (startingZone != null)
			{
				ArrayList res = startingZone.GetObjectsInRadius(type, x, y, z, radius, new ArrayList());

				uint sqRadius = (uint)radius * radius;

				// optimization (according to profiler)
				int sz = m_Zones.Count;

				Zone currentZone = null;
				for (int i = 0; i < sz; ++i)
				{
					currentZone = (Zone)m_Zones[i];
					if ((currentZone != startingZone)
						&& (currentZone.TotalNumberOfObjects > 0)
						&& CheckShortestDistance(currentZone, x, y, sqRadius))
					{
						res = currentZone.GetObjectsInRadius(type, x, y, z, radius, res);
					}
				}
				//Return required enumerator
				IEnumerable tmp = null;
				if (withDistance)
				{
					switch (type)
					{
						case Zone.eGameObjectType.ITEM:
							tmp = new ItemDistanceEnumerator(x, y, z, res);
							break;
						case Zone.eGameObjectType.NPC:
							tmp = new NPCDistanceEnumerator(x, y, z, res);
							break;
						case Zone.eGameObjectType.PLAYER:
							tmp = new PlayerDistanceEnumerator(x, y, z, res);
							break;
						case Zone.eGameObjectType.DOOR:
							tmp = new DoorDistanceEnumerator(x, y, z, res);
							break;
						default:
							tmp = new EmptyEnumerator();
							break;
					}
				}
				else
				{
					tmp = new ObjectEnumerator(res);
				}
				return tmp;
			}
			else
			{
				if (log.IsDebugEnabled)
				{
					log.Error("GetInRadius starting zone is null for (" + type + ", " + x + ", " + y + ", " + z + ", " + radius + ") in Region ID=" + ID);
					log.Error(Environment.StackTrace);
				}
				return new EmptyEnumerator();
			}
		}


		/// <summary>
		/// get the shortest distance from a point to a zone
		/// </summary>
		/// <param name="zone">The zone to check</param>
		/// <param name="x">X value of the point</param>
		/// <param name="y">Y value of the point</param>
		/// <param name="squareRadius">The square radius to compare the distance with</param>
		/// <returns>True if the distance is shorter false either</returns>
		private static bool CheckShortestDistance(Zone zone, int x, int y, uint squareRadius)
		{
			//  coordinates of zone borders
			int xLeft = zone.XOffset;
			int xRight = zone.XOffset + zone.Width;
			int yTop = zone.YOffset;
			int yBottom = zone.YOffset + zone.Height;
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
		/// Gets Items in a radius around a spot
		/// </summary>
		/// <param name="x">origin X</param>
		/// <param name="y">origin Y</param>
		/// <param name="z">origin Z</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetItemsInRadius(int x, int y, int z, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.ITEM, x, y, z, radius, withDistance);
		}

		/// <summary>
		/// Gets NPCs in a radius around a spot
		/// </summary>
		/// <param name="x">origin X</param>
		/// <param name="y">origin Y</param>
		/// <param name="z">origin Z</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetNPCsInRadius(int x, int y, int z, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.NPC, x, y, z, radius, withDistance);
		}

		/// <summary>
		/// Gets Players in a radius around a spot
		/// </summary>
		/// <param name="x">origin X</param>
		/// <param name="y">origin Y</param>
		/// <param name="z">origin Z</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetPlayersInRadius(int x, int y, int z, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.PLAYER, x, y, z, radius, withDistance);
		}

		/// <summary>
		/// Gets Doors in a radius around a spot
		/// </summary>
		/// <param name="x">origin X</param>
		/// <param name="y">origin Y</param>
		/// <param name="z">origin Z</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetDoorsInRadius(int x, int y, int z, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.DOOR, x, y, z, radius, withDistance);
		}

		#endregion

		#region Enumerators

		#region EmptyEnumerator

		/// <summary>
		/// An empty enumerator returned when no objects are found
		/// close to a certain range
		/// </summary>
		public class EmptyEnumerator : IEnumerator, IEnumerable
		{
			/// <summary>
			/// Implementation of the IEnumerable interface
			/// </summary>
			/// <returns>An Enumeration Interface of this class</returns>
			public IEnumerator GetEnumerator()
			{
				return this;
			}

			/// <summary>
			/// Implementation of the IEnumerator interface
			/// </summary>
			/// <returns>Always false to prevent Current</returns>
			public bool MoveNext()
			{
				return false;
			}

			/// <summary>
			/// Implementation of the IEnumerator interface,
			/// always returns null because it shouldn't be
			/// called at all.
			/// </summary>
			public object Current
			{
				get { return null; }
			}

			/// <summary>
			/// Implementation of the IEnumerator interface
			/// </summary>
			public void Reset()
			{
			}
		}

		#endregion

		#region ObjectEnumerator

		/// <summary>
		/// An enumerator over GameObjects. Used to enumerate over
		/// certain objects and do some testing before returning an
		/// object.
		/// </summary>
		public class ObjectEnumerator : IEnumerator, IEnumerable
		{
			/// <summary>
			/// Counter to the current object
			/// </summary>
			protected int m_current = -1;

			protected GameObject[] elements = null;
			//protected ArrayList elements = null;

			protected object m_currentObj = null;

			protected int m_count;

			public IEnumerator GetEnumerator()
			{
				return this;
			}

			public ObjectEnumerator(ArrayList objectSet)
			{
				//objectSet.DumpInfo();
				elements = new GameObject[objectSet.Count];
				objectSet.CopyTo(elements);
				m_count = elements.Length;
			}


			/// <summary>
			/// Get the next GameObjcte from the zone subset created in constructor
			/// and by restrictuing according distance
			/// </summary>
			/// <returns>The Next GameObject of this Enumerator</returns>
			public virtual bool MoveNext()
			{
				/*********NEW GET IN RADIUS SYSTEM ADDED BY KONIK**********/
				m_currentObj = null;
				bool found = false;
				do
				{
					m_current++;
					// break if no more object
					if (m_current < m_count)
					{
						// get the object
						//GameObject obj = (GameObject) elements[m_current];
						GameObject obj = elements[m_current];
						if (found = ((obj != null && ((int)obj.ObjectState) == (int)GameObject.eObjectState.Active)))
						{
							m_currentObj = obj;
						}
					}
				} while (m_current < m_count && !found);
				return found;
			}

			/// <summary>
			/// Returns the current Object in the Enumerator
			/// </summary>
			public virtual object Current
			{
				get { return m_currentObj; }
			}

			/// <summary>
			/// Resets the Enumerator
			/// </summary>
			public void Reset()
			{
				m_currentObj = null;
				m_current = -1;
			}
		}

		#endregion

		#region XXXDistanceEnumerator

		public abstract class DistanceEnumerator : ObjectEnumerator
		{
			protected int m_X;
			protected int m_Y;
			protected int m_Z;

			public DistanceEnumerator(int x, int y, int z, ArrayList elements)
				: base(elements)
			{
				m_X = x;
				m_Y = y;
				m_Z = z;
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class PlayerDistanceEnumerator : DistanceEnumerator
		{
			public PlayerDistanceEnumerator(int x, int y, int z, ArrayList elements)
				: base(x, y, z, elements)
			{
			}

			public override object Current
			{
				get
				{
					GamePlayer obj = (GamePlayer)m_currentObj;
                    return new PlayerDistEntry( obj, obj.GetDistanceTo( new Point3D( m_X, m_Y, m_Z ) ) );
				}
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class NPCDistanceEnumerator : DistanceEnumerator
		{
			public NPCDistanceEnumerator(int x, int y, int z, ArrayList elements)
				: base(x, y, z, elements)
			{
			}

			public override object Current
			{
				get
				{
					GameNPC obj = (GameNPC)m_currentObj;
                    return new NPCDistEntry( obj, obj.GetDistanceTo( new Point3D( m_X, m_Y, m_Z ) ) );
				}
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class ItemDistanceEnumerator : DistanceEnumerator
		{
			public ItemDistanceEnumerator(int x, int y, int z, ArrayList elements)
				: base(x, y, z, elements)
			{
			}

			public override object Current
			{
				get
				{
					GameStaticItem obj = (GameStaticItem)m_currentObj;
                    return new ItemDistEntry( obj, obj.GetDistanceTo( new Point3D( m_X, m_Y, m_Z ) ) );
				}
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class DoorDistanceEnumerator : DistanceEnumerator
		{
			public DoorDistanceEnumerator(int x, int y, int z, ArrayList elements)
				: base(x, y, z, elements)
			{
			}

			public override object Current
			{
				get
				{
					IDoor obj = (IDoor)m_currentObj;
                    return new DoorDistEntry( obj, obj.GetDistance( new Point3D( m_X, m_Y, m_Z ) ) );
				}
			}
		}

		#endregion

		#endregion

		#region Automatic relocation

		public void Relocate()
		{
			lock (m_Zones.SyncRoot)
			{
				for (int i = 0; i < m_Zones.Count; i++)
				{
					((Zone)m_Zones[i]).Relocate(null);
				}
				m_lastRelocation = DateTime.Now.Ticks / (10 * 1000);
			}
		}

		#endregion

		#endregion
	}

	#region Helpers classes

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class PlayerDistEntry
	{
		public PlayerDistEntry(GamePlayer o, int distance)
		{
			Player = o;
			Distance = distance;
		}

		public GamePlayer Player;
		public int Distance;
	}

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class NPCDistEntry
	{
		public NPCDistEntry(GameNPC o, int distance)
		{
			NPC = o;
			Distance = distance;
		}

		public GameNPC NPC;
		public int Distance;
	}

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class ItemDistEntry
	{
		public ItemDistEntry(GameStaticItem o, int distance)
		{
			Item = o;
			Distance = distance;
		}

		public GameStaticItem Item;
		public int Distance;
	}

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class DoorDistEntry
	{
		public DoorDistEntry(IDoor d, int distance)
		{
			Door = d;
			Distance = distance;
		}

		public IDoor Door;
		public int Distance;
	}

	#endregion
}
