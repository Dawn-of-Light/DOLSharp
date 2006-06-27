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
using DOL.GS.Collections;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.GS.Utils;
using DOL.AI;
using NHibernate.Expression;
using log4net;
using Hashtable=System.Collections.Hashtable;

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

		#region Declaration

		#region Persistant data

		/// <summary>
		/// The Region ID eg. 11
		/// </summary>
		protected int m_id;

		/// <summary>
		/// The Region Description eg. "Camelot Hills"
		/// </summary>
		protected string m_description;

		/// <summary>
		/// The region expansion
		/// </summary>
		protected byte m_expansion;

		/// <summary>
		/// Is housing enabled in region
		/// </summary>
		protected bool m_isHousingEnabled;

		/// <summary>
		/// Is diving anabled in region
		/// </summary>
		protected bool m_isDivingEnabled;

		/// <summary>
		/// Is a dungeon
		/// </summary>
		protected bool m_isDungeon;

		/// <summary>
		/// Is a instance
		/// </summary>
		protected bool m_isInstance;

		/// <summary>
		/// Holds all the Zones inside this Region
		/// </summary>
		protected Iesi.Collections.ISet m_zones;

		/// <summary>
		/// The unique Region id eg. 21
		/// </summary>
		public int RegionID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The Region Description eg. Cursed Forest
		/// </summary>
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		/// <summary>
		/// Gets or Sets the region expansion
		/// </summary>
		public byte Expansion
		{
			get { return m_expansion; }
			set { m_expansion = value; }
		}

		/// <summary>
		/// Gets or Sets housing flag for region
		/// </summary>
		public bool IsHousingEnabled
		{
			get { return m_isHousingEnabled; }
			set { m_isHousingEnabled = value; }
		}

		/// <summary>
		/// Gets or Sets diving flag for region
		/// </summary>
		public bool IsDivingEnabled
		{
			get { return m_isDivingEnabled; }
			set { m_isDivingEnabled = value; }
		}

		/// <summary>
		/// Gets or Sets if this region is a dungeon
		/// </summary>
		public bool IsDungeon
		{
			get { return m_isDungeon; }
			set { m_isDungeon = value; }
		}

		/// <summary>
		/// Gets or Sets if this region is a instance
		/// </summary>
		public bool IsInstance
		{
			get { return m_isInstance; }
			set { m_isInstance = value; }
		}

		/// <summary>
		/// Get or set the set of all zones within this Region
		/// </summary>
		public Iesi.Collections.ISet Zones 
		{
			get 
			{ 
				if(m_zones==null) m_zones = new Iesi.Collections.HybridSet();
				return m_zones; 
			}
			set { m_zones = value; }
		}

		#endregion

		#region Runtime data

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
		private GameObject[] m_objects = new GameObject[0];

		/// <summary>
		/// Object to lock when changing objects in the array
		/// </summary>
		public readonly object ObjectsSyncLock = new object();

		/// <summary>
		/// This array holds a bitarray
		/// Its used to know which slots in region object array are free and what allocated
		/// This is used to accelerate inserts a lot
		/// </summary>
		protected uint[] m_objectsAllocatedSlots = new uint[0];

		/// <summary>
		/// This holds the index of a possible next object slot
		/// but needs further checks (basically its lastaddedobjectIndex+1)
		/// </summary>
		protected int m_nextObjectSlot = 0;

		/// <summary>
		/// This holds a counter with the absolute count of all objects that are actually in this region
		/// </summary>
		private int m_objectsInRegion = 0;

		/// <summary>
		/// Contains the counter of players in the region
		/// </summary>
		protected int m_playersInRegion = 0;

		/// <summary>
		/// last relocation time
		/// </summary>
		private long m_lastRelocation = 0;

		/// <summary>
		/// The region time manager
		/// </summary>
		protected GameTimer.TimeManager m_timeManager;

		/// <summary>
		/// Returns the object array of this region
		/// </summary>
		public GameObject[] Objects
		{
			get { return m_objects; }
		}

		/// <summary>
		/// Gets the counter of players in the region
		/// </summary>
		public int NumPlayers
		{
			get { return m_playersInRegion; }
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
			set { m_timeManager = value; }
		}

		/// <summary>
		/// Gets the curret region time in milliseconds
		/// </summary>
		public long Time
		{
			get { return m_timeManager.CurrentTime; }
		}

		#endregion

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
		public void PreAllocateRegionSpace(int count) {
			if (count > MAXOBJECTS)
				count = MAXOBJECTS;
			lock (ObjectsSyncLock) {
				if (m_objects.Length > count) return;
				GameObject[] newObj = new GameObject[count];
				Array.Copy(m_objects, newObj, m_objects.Length);
				uint[] slotarray = new uint[count/32+1];
				Array.Copy(m_objectsAllocatedSlots, slotarray, m_objectsAllocatedSlots.Length);
				m_objectsAllocatedSlots = slotarray;
				m_objects = newObj;
			}
		}

		/// <summary>
		/// Loads the region from database
		/// </summary>
		/// <param name="mobCount">The count of loaded mobs</param>
		/// <param name="merchantCount">The count of loaded merchants</param>
		/// <param name="itemCount">The count of loaded items</param>
		public void LoadFromDatabase(ref long mobCount, ref long merchantCount, ref long itemCount)
		{
            SpawnGenerators.SpawnGeneratorMgr.Init(this);

			Assembly gasm = Assembly.GetAssembly(typeof (GameServer));
			IList NPCObjs = GameServer.Database.SelectObjects(typeof (DBNPC), Expression.Eq("Region", RegionID));
			IList staticObjs = GameServer.Database.SelectObjects(typeof (WorldObject), Expression.Eq("Region", RegionID));
            int count = 0;// NPCObjs.Count + staticObjs.Count;
			if (count > 0) PreAllocateRegionSpace(count+100);
			int myItemCount = staticObjs.Count;
			int myNPCCount = 0;
			int myMerchantCount = 0;
            if (NPCObjs.Count > 0)
			{
                foreach (DBNPC npc in NPCObjs)
				{
					GameNPC myMob = null;
                    INpcTemplate template = NpcTemplateMgr.GetTemplate(npc.TemplateID);
                    if (template == null)
                    {
                        if (log.IsWarnEnabled)
                            log.Warn("can not find the template with id :"+ npc.TemplateID);
                        continue;
                    }
					if (npc.ClassType != null && npc.ClassType.Length > 0)
					{
						try
						{
							myMob = (GameNPC) ScriptMgr.GetInstance(npc.ClassType, template );
						}
						catch
						{
						}
						if (myMob == null)
						{
                               myMob = new GameMob(template);
						}
					}
					else
					{
						myMob = new GameMob(template);
					}
                    myNPCCount++;

                    myMob.Name = npc.Name;
                    if (npc.BrainClass != null & npc.BrainClass != "") //overide template brain
                    {
                        ABrain mybrain = ScriptMgr.GetInstance(npc.BrainClass) as ABrain;
                        if (npc.BrainParams != null & npc.BrainParams != "")
                            mybrain.ParseParam(npc.BrainParams);
                        myMob.SetOwnBrain(mybrain);
                    }
                    myMob.Name = npc.Name;
                    myMob.ParseParam(npc.NPCTypeParameters);
                    myMob.Position = new Point(npc.X, npc.Y, npc.Z);
                    myMob.Region = this;
                    myMob.Heading = npc.Heading;
					myMob.AddToWorld();

						//						if (!myMob.AddToWorld()) // seems like some people store inactive NPCs in db and active them later
						//							log.ErrorFormat("Failed to add the mob to the world: {0}", myMob.ToString());
				}
			}


			if (staticObjs.Count > 0)
			{
				foreach (WorldObject item in staticObjs)
				{
					GameStaticItem myItem = null;

					if (item.ClassType != null && item.ClassType.Length > 0)
					{
						try
						{
							myItem = (GameStaticItem) gasm.CreateInstance(item.ClassType, false);
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
									myItem = (GameStaticItem) asm.CreateInstance(item.ClassType, false);
								}
								catch
								{
								}
								if (myItem != null)
									break;
							}
							if (myItem == null)
								myItem = new GameStaticItem(item);
						}
					}
					else
					{
						myItem = new GameStaticItem(item);
					}

					if (myItem != null)
					{
						myItem.CopyFrom(item);
						myItem.AddToWorld();
//						if (!myItem.AddToWorld())
//							log.ErrorFormat("Failed to add the item to the world: {0}", myItem.ToString());
					}
				}
			}

            if (myNPCCount + myItemCount + myMerchantCount > 0)
			{
				if (log.IsInfoEnabled)
                    log.Info(String.Format("Region: {0} loaded {1} npcs, {2} merchants, {3} items, from DB ({4})", Description, myNPCCount, myMerchantCount, myItemCount, TimeManager.Name));
				//WorldMgr.GCAction();
				log.Debug("Used Memory: "+GC.GetTotalMemory(false)/1024 + "KB");
				Thread.Sleep(0);
			}
            mobCount += myNPCCount;
			merchantCount += myMerchantCount;
			itemCount += myItemCount;
		}

		/// <summary>
		/// Adds an object to the region and assigns the object an id
		/// </summary>
		/// <param name="obj">A GameObject to be added to the region</param>
		/// <returns>success</returns>
		internal bool AddObject(GameObject obj)
		{
			//Thread.Sleep(10000);
			Zone zone = GetZone(obj.Position);
			if (zone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Zone not found for Object: " + obj.Name + "(ID=" + obj.InternalID + ")");
			}

			//Assign a new id
			lock (ObjectsSyncLock)
			{
				if (obj.ObjectID != -1) {
					if (obj.ObjectID < m_objects.Length && obj == m_objects[obj.ObjectID-1]) {
						log.WarnFormat("Object is already in the region ({0})", obj.ToString());
						return false;
					}
					log.Warn(obj.Name+" should be added to "+Description+" but had already an OID("+obj.ObjectID+") => not added\n"+Environment.StackTrace);
					return false;
				} 

				GameObject[] objectsRef = m_objects;

				//*** optimized object management for memory saving primary but keeping it very fast - Blue ***

				// find first free slot for the object
				int objID = m_nextObjectSlot;
				if (objID >= m_objects.Length || m_objects[objID] != null) {

					// we are at array end, are there any holes left?
					if (m_objects.Length > m_objectsInRegion) {
						// yes there are some places left in current object array, try to find them
						// by using the bit array (can check 32 slots at once!)
						
						int i = m_objects.Length / 32;
						// INVARIANT: i * 32 is always lower or equal to m_objects.Length (integer division property)
						if (i * 32 == m_objects.Length) {
							i -= 1;
						}

						bool found = false;
						objID = -1;

						while (!found && (i >= 0)) {
							if (m_objectsAllocatedSlots[i] != 0xffffffff) {
								// we found a free slot
								// => search for exact place

								int currentIndex = i * 32;
								int upperBound = (i + 1) * 32;
								while (!found && (currentIndex < m_objects.Length) && (currentIndex < upperBound)) {
									if (m_objects[currentIndex] == null) {
										found = true;
										objID = currentIndex;
									}

									currentIndex++;
								}

								// INVARIANT: at this point, found must be true (otherwise the underlying data structure is corrupt)
							}

							i--;
						}
					} else { // our array is full, we must resize now to fit new objects

						if (objectsRef.Length == 0) {

							// there is no array yet, so set it to a minimum at least
							objectsRef = new GameObject[MINIMUMSIZE];
							Array.Copy(m_objects, objectsRef, m_objects.Length);
							objID = 0;

						} else if (objectsRef.Length >= MAXOBJECTS) {

							// no available slot
							if (log.IsErrorEnabled) 
								log.Error("Can't add new object - region '" + Description + "' is full. (object: " + obj.ToString() + ")");
							return false;

						} 
						else 
						{

							// we need to add a certain amount to grow
							int size = (int)(m_objects.Length*1.20);
							if (size < m_objects.Length+256)
								size = m_objects.Length+256;
							if (size > MAXOBJECTS)
								size = MAXOBJECTS;
							objectsRef = new GameObject[size]; // grow the array by 20%, at least 256
							Array.Copy(m_objects, objectsRef, m_objects.Length);
							objID = m_objects.Length; // new object adds right behind the last object in old array

						}
						// resize the bitarray as well
						int diff = objectsRef.Length/32 - m_objectsAllocatedSlots.Length;
						if (diff >= 0) {
							uint[] newBitArray	= new uint[Math.Max(m_objectsAllocatedSlots.Length+diff+50, 100)];	// add at least 100 integers, makes it resize less often, serves 3200 new objects, only 400 bytes
							Array.Copy(m_objectsAllocatedSlots, newBitArray, m_objectsAllocatedSlots.Length);
							m_objectsAllocatedSlots = newBitArray;
						}
					}
				}

				if (objID < 0) {
					log.Warn("There was an unexpected problem while adding "+obj.Name+" to "+Description);
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
					m_objectsAllocatedSlots[objID/32] |= (uint)1 << (objID%32);
					Thread.MemoryBarrier();
					m_objects = objectsRef;

					if (obj is GamePlayer)
						++m_playersInRegion;
					
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
				if (index < 0) {
					return;
				}

				if (obj is GamePlayer)
					--m_playersInRegion;
				

				if (log.IsDebugEnabled)
					log.Debug("RemoveObject: OID" + obj.ObjectID + " " + obj.Name + "(R"+obj.RegionId+") from " + Description);

				GameObject inPlace = m_objects[obj.ObjectID - 1];
				if (inPlace == null) {
					log.Error("RemoveObject conflict! OID"+obj.ObjectID+" "+obj.Name+"("+obj.RegionId+") but there was no object at that slot");
					log.Error(new StackTrace().ToString());
					return;
				}
				if (obj != inPlace) {
					log.Error("RemoveObject conflict! OID"+obj.ObjectID+" "+obj.Name+"("+obj.RegionId+") but there was another object already "+inPlace.Name+" region:"+inPlace.RegionId+" state:"+inPlace.ObjectState);
					log.Error(new StackTrace().ToString());
					return;
				}

				if (m_objects[index] != obj) {
					log.Error("Object OID is already used by another object! (used by:" + m_objects[index].ToString() + ")");
				} else {
					m_objects[index] = null;
					m_objectsAllocatedSlots[index/32] &= ~(uint) (1 << (index%32));
				}
				obj.ObjectID = -1; // invalidate object id
				m_objectsInRegion--;
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
		/// Returns the zone that contains the specified point.
		/// </summary>
		/// <param name="pos">global position for the zone you're retrieving</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public Zone GetZone(Point pos)
		{
			foreach(Zone currentZone in m_zones)
			{
				if (currentZone.XOffset <= pos.X && currentZone.YOffset <= pos.Y && (currentZone.XOffset + currentZone.Width) > pos.X && (currentZone.YOffset + currentZone.Height) > pos.Y)
					return currentZone;
			}
			return null;
		}

		/// <summary>
		/// Returns the zone in this region with the id.
		/// </summary>
		/// <param name="zoneID">the id to search for</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public Zone GetZone(ushort zoneID)
		{
			foreach(Zone currentZone in m_zones)
			{
				if (currentZone.ZoneID == zoneID)
					return currentZone;
			}
			return null;
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

		/// <summary>
		/// Gets objects in a radius around a point
		/// </summary>
		/// <param name="type">OBJECT_TYPE (0=item, 1=npc, 2=player)</param>
		/// <param name="pos">origin</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		private IEnumerable GetInRadius(Zone.eGameObjectType type, Point pos, ushort radius, bool withDistance) 
		{
			DynamicList result = new DynamicList();
			foreach(Zone currentZone in m_zones)
			{
				if (CheckShortestDistance(currentZone, pos.X, pos.Y, radius) && currentZone.TotalNumberOfObjects > 0) 
				{
					result = currentZone.GetObjectsInRadius(type, pos, radius, result);
				}
			}

			//Return required enumerator
			IEnumerable tmp = null;
			if (withDistance) 
			{
				switch (type) 
				{
					case Zone.eGameObjectType.ITEM:
						tmp = new ItemDistanceEnumerator(pos, result);
						break;
					case Zone.eGameObjectType.NPC:
						tmp = new NPCDistanceEnumerator(pos, result);
						break;
					case Zone.eGameObjectType.PLAYER:
						tmp = new PlayerDistanceEnumerator(pos, result);
						break;
					default:
						tmp = new EmptyEnumerator();
						break;
				}
			}
			else 
			{
				tmp = new ObjectEnumerator(result);
			}
			return tmp;
		}


		/// <summary>
		/// get the shortest distance from a point to a zone
		/// </summary>
		/// <param name="zone">The zone to check</param>
		/// <param name="x">X value of the point</param>
		/// <param name="y">Y value of the point</param>
		/// <param name="radius">The radius to compare the distance with</param>
		/// <returns>True if the distance is shorter false either</returns>
		private static bool CheckShortestDistance(Zone zone, int x, int y, uint radius)
		{
			//  coordinates of zone borders
			int xLeft = zone.XOffset;
			int xRight = zone.XOffset + zone.Width;
			int yTop = zone.YOffset;
			int yBottom = zone.YOffset + zone.Height;
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
					distance = (long) xdiff*xdiff;
				}
			}
			else
			{
				if ((x >= xLeft) && (x <= xRight))
				{
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long) ydiff*ydiff;
				}
				else
				{
					int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long) xdiff*xdiff + (long) ydiff*ydiff;
				}
			}

			return (distance <= (radius*radius));
		}

		/// <summary>
		/// Gets Items in a radius around a spot
		/// </summary>
		/// <param name="pos">origin</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetItemsInRadius(Point pos, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.ITEM, pos, radius, withDistance);
		}

		/// <summary>
		/// Gets NPCs in a radius around a spot
		/// </summary>
		/// <param name="pos">origin</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetNPCsInRadius(Point pos, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.NPC, pos, radius, withDistance);
		}

		/// <summary>
		/// Gets Players in a radius around a spot
		/// </summary>
		/// <param name="pos">origin</param>
		/// <param name="radius">radius around origin</param>
		/// <param name="withDistance">Get an ObjectDistance enumerator</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public IEnumerable GetPlayerInRadius(Point pos, ushort radius, bool withDistance)
		{
			return GetInRadius(Zone.eGameObjectType.PLAYER, pos, radius, withDistance);
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

			public ObjectEnumerator(DynamicList objectSet)
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
						if (found = ((obj != null && ((int) obj.ObjectState) == (int) GameObject.eObjectState.Active)))
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
			protected readonly Point m_position;

			public DistanceEnumerator(Point position, DynamicList elements) : base(elements)
			{
				m_position = position;
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class PlayerDistanceEnumerator : DistanceEnumerator
		{
			public PlayerDistanceEnumerator(Point position, DynamicList elements) : base(position, elements)
			{
			}

			public override object Current
			{
				get
				{
					GamePlayer obj = (GamePlayer) m_currentObj;
					return new PlayerDistEntry(obj, m_position.GetDistance(obj.Position));
				}
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class NPCDistanceEnumerator : DistanceEnumerator
		{
			public NPCDistanceEnumerator(Point position, DynamicList elements) : base(position, elements)
			{
			}

			public override object Current
			{
				get
				{
					GameNPC obj = (GameNPC) m_currentObj;
					return new NPCDistEntry(obj, m_position.GetDistance(obj.Position));
				}
			}
		}

		/// <summary>
		/// This enumerator returns the object and the distance towards the object
		/// </summary>
		public class ItemDistanceEnumerator : DistanceEnumerator
		{
			public ItemDistanceEnumerator(Point position, DynamicList elements) : base(position, elements)
			{
			}

			public override object Current
			{
				get
				{
					GameStaticItem obj = (GameStaticItem) m_currentObj;
					return new ItemDistEntry(obj, m_position.GetDistance(obj.Position));
				}
			}
		}

		#endregion

		#endregion

		#region Automatic relocation

		public void Relocate() 
		{
			lock (m_zones.SyncRoot) 
			{
				foreach(Zone currentZone in m_zones)
				{
					currentZone.Relocate(null);
				}
				m_lastRelocation = DateTime.Now.Ticks/(10*1000);
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

	#endregion
}