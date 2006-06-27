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
using System.Text;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY object in the game world needs!
	/// </summary>
	public abstract class GameObject : IWorldPosition
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region State/Random/Type

		/// <summary>
		/// Holds the current state of the object
		/// </summary>
		public enum eObjectState : byte
		{
			/// <summary>
			/// Active, visibly in world
			/// </summary>
			Active,
			/// <summary>
			/// Inactive, currently being moved or stuff
			/// </summary>
			Inactive,
			/// <summary>
			/// Deleted, waiting to be cleaned up
			/// </summary>
			Deleted
		}

		/// <summary>
		/// The Object's state! This is needed because
		/// when we remove an object it isn't instantly
		/// deleted but the state is merely set to "Deleted"
		/// This prevents the object from vanishing when
		/// there still might be enumerations running over it.
		/// A timer will collect the deleted objects and free
		/// them at certain intervals.
		/// </summary>
		protected volatile eObjectState m_ObjectState;

		/// <summary>
		/// Returns the current state of the object.
		/// Object's with state "Deleted" should not be used!
		/// </summary>
		public eObjectState ObjectState
		{
			get { return m_ObjectState; }
			set
			{
				if (log.IsDebugEnabled)
					log.Debug("ObjectState: OID" + ObjectID + " " + Name + " " + m_ObjectState + " => " + value);
				m_ObjectState = value;
			}
		}

		#endregion

		#region Position

		/// <summary>
		/// The object's position in the current region.
		/// </summary>
		protected Point m_position;
		
		/// <summary>
		/// The object's current region.
		/// </summary>
		protected Region m_region;

		/// <summary>
		/// The direction the Object is facing
		/// </summary>
		protected ushort m_Heading;

		/// <summary>
		/// Holds the realm of this object
		/// </summary>
		protected byte m_Realm;
		
		/// <summary>
		/// Gets or sets the object's position in the region.
		/// </summary>
		public virtual Point Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public virtual Region Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		/// <summary>
		/// Gets or sets the current Region by the ID
		/// </summary>
		public virtual int RegionId
		{
			get
			{
				return m_region == null ? (ushort)0 : m_region.RegionID;
			}
			set
			{
				Region = WorldMgr.GetRegion((ushort)value);
			}
		}

		/// <summary>
		/// Gets the current Zone of the Object
		/// </summary>
		public Zone CurrentZone
		{
			get 
			{
				Region region = m_region;
				if (region != null) 
				{
					return region.GetZone(Position);
				}
				return null;
			}
		}

		/// <summary>
		/// Gets or Sets the current Realm of the Object
		/// </summary>
		public virtual byte Realm
		{
			get { return m_Realm; }
			set { m_Realm = value; }
		}

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public virtual int Heading
		{
			get { return m_Heading; }
			set { m_Heading = (ushort)(value&0xFFF); }
		}

		/// <summary>
		/// Returns the angle towards a target spot in degrees, clockwise
		/// </summary>
		/// <param name="point">target point</param>
		/// <returns>the angle towards the spot</returns>
		public float GetAngleToSpot(Point point)
		{
			float headingDifference = (Position.GetHeadingTo(point) - Heading) & 0xFFF;
			return (headingDifference*360.0f/4096.0f);
		}

		/// <summary>
		/// This constant is used to calculate the heading quickly
		/// </summary>
		public const double HEADING_CONST = 651.89864690440329530934789477382;

		/// <summary>
		/// Calculates a spot into the heading direction
		/// </summary>
		/// <param name="distance">the distance to the spot</param>
		/// <returns>the result position</returns>
		public Point GetSpotFromHeading(int distance)
		{
			Point pos = Position;
			double angle = Heading/HEADING_CONST;
			double x = pos.X - Math.Sin(angle)*distance;
			double y = pos.Y + Math.Cos(angle)*distance;
			
			pos.X = (x > 0 ? (int) x : 0);
			pos.Y = (y > 0 ? (int) y : 0);
			pos.Z = 0;
			return pos;
		}

		/// <summary>
		/// determines wether a target object is front
		/// in front is defined as north +- viewangle/2
		/// </summary>
		/// <param name="target"></param>
		/// <param name="viewangle"></param>
		/// <returns></returns>
		public virtual bool IsObjectInFront(GameObject target, double viewangle)
		{
			if (target == null)
				return false;
			float angle = GetAngleToSpot(target.Position);
			viewangle *= 0.5;
			if (angle >= 360.0 - viewangle || angle < viewangle)
				return true;
			// if target is closer than 32 units it is considered always in view
			// tested and works this way for noraml evade, parry, block (in 1.69)
			return Position.CheckSquareDistance(target.Position, 32*32);
		}

		/// <summary>
		/// Checks if object is underwater
		/// </summary>
		public bool IsUnderwater
		{
			get
			{
				Region region = Region;
				if (region == null)
					return false;
				Zone zone = region.GetZone(Position);
				if(zone == null)
					return false;

				return Position.Z < zone.WaterLevel;
			}
		}

		#endregion

		#region Level/Name/Model/GetName/GetPronoun/GetExamineMessage

		/// <summary>
		/// The level of the Object
		/// </summary>
		protected byte m_Level;

		/// <summary>
		/// The name of the Object
		/// </summary>
		protected string m_Name;

		/// <summary>
		/// The model of the Object
		/// </summary>
		protected int m_Model;

		/// <summary>
		/// Gets or Sets the current level of the Object
		/// </summary>
		public virtual byte Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		/// <summary>
		/// Gets or Sets the effective level of the Object
		/// </summary>
		public virtual int EffectiveLevel
		{
			get { return Level; }
			set {}
		}

		/// <summary>
		/// Gets or Sets the current Name of the Object
		/// </summary>
		public virtual string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		/// <summary>
		/// Gets or Sets the current Model of the Object
		/// </summary>
		public virtual int Model
		{
			get { return m_Model; }
			set { m_Model = value; }
		}

		private const string m_vowels = "aeuio";

		/// <summary>
		/// Returns name with article for nouns
		/// </summary>
		/// <param name="article">0=definite, 1=indefinite</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>name of this object (includes article if needed)</returns>
		public virtual string GetName(int article, bool firstLetterUppercase)
		{
			/*
			 * http://www.camelotherald.com/more/888.shtml
			 * - All monsters names whose names begin with a vowel should now use the article 'an' instead of 'a'.
			 * 
			 * http://www.camelotherald.com/more/865.shtml
			 * - Instances where objects that began with a vowel but were prefixed by the article "a" (a orb of animation) have been corrected.
			 */


			// actually this should be only for Named mobs (like dragon, legion) but there is no way to find that out
			if (char.IsUpper(Name[0])) // proper noun
			{
				return Name;
			}
			else // common noun
				if (article == 0)
				{
					if (firstLetterUppercase)
						return "The " + Name;
					else
						return "the " + Name;
				}
				else
				{
					// if first letter is a vowel
					if (m_vowels.IndexOf(Name[0]) != -1)
					{
						if (firstLetterUppercase)
							return "An " + Name;
						else
							return "an " + Name;
					}
					else
					{
						if (firstLetterUppercase)
							return "A " + Name;
						else
							return "a " + Name;
					}
				}
		}

		/// <summary>
		/// Pronoun of this object in case you need to refer it in 3rd person
		/// http://webster.commnet.edu/grammar/cases.htm
		/// </summary>
		/// <param name="firstLetterUppercase"></param>
		/// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
		/// <returns>pronoun of this object</returns>
		public virtual string GetPronoun(int form, bool firstLetterUppercase)
		{
			switch (form)
			{
				default: // Subjective
					if (firstLetterUppercase)
						return "It";
					else
						return "it";
				case 1: // Possessive
					if (firstLetterUppercase)
						return "Its";
					else
						return "its";
				case 2: // Objective
					if (firstLetterUppercase)
						return "It";
					else
						return "it";
			}
		}

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public virtual IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(4);
			list.Add("You target [" + GetName(0, false) + "]");
			return list;
		}

		#endregion

		#region IDs/Database

		/// <summary>
		/// The objectID. This is -1 as long as the object is not added to a region!
		/// </summary>
		protected int m_ObjectID = -1;

		/// <summary>
		/// The internalID. This is the unique ID of the object in the DB!
		/// </summary>
		protected string m_InternalID;

		/// <summary>
		/// Gets or Sets the current ObjectID of the Object
		/// This is done automatically by the Region and should
		/// not be done manually!!!
		/// </summary>
		public int ObjectID
		{
			get { return m_ObjectID; }
			set
			{
				if (log.IsDebugEnabled)
					log.Debug("ObjectID: " + Name + " " + m_ObjectID + " => " + value);
				m_ObjectID = value;
			}
		}

		/// <summary>
		/// Gets or Sets the internal ID (DB ID) of the Object
		/// </summary>
		public string InternalID
		{
			get { return m_InternalID; }
			set { m_InternalID = value; }
		}

		/// <summary>
		/// Saves an object into the database
		/// </summary>
		public virtual void SaveIntoDatabase()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public virtual void LoadFromDatabase(object obj)
		{
		}
		/// <summary>
		/// Deletes a character from the DB
		/// </summary>
		public virtual void DeleteFromDatabase()
		{
		}

		#endregion

		#region Add-/Remove-/Create-/Move-

		/// <summary>
		/// Creates the item in the world
		/// </summary>
		/// <returns>true if object was created</returns>
		public virtual bool AddToWorld()
		{
			/****** MODIFIED BY KONIK & WITCHKING *******/
			Zone currentZone = CurrentZone;
			// CurrentZone checks for null Region.
			// Should it be the case, currentZone will be null as well.
			if (currentZone == null || m_ObjectState == eObjectState.Active)
				return false;

			if (!m_region.AddObject(this))
				return false;
			Notify(GameObjectEvent.AddToWorld, this);
			ObjectState = eObjectState.Active;

			CurrentZone.ObjectEnterZone(this);
			/*********** END OF MODIFICATION ***********/

			m_spawnTick = Region.Time;
			return true;
		}

		/// <summary>
		/// Removes the item from the world
		/// </summary>
		public virtual bool RemoveFromWorld()
		{
			if (m_region == null || ObjectState != eObjectState.Active)
				return false;
			Notify(GameObjectEvent.RemoveFromWorld, this);
			ObjectState = eObjectState.Inactive;
			m_region.RemoveObject(this);
			return true;
		}

		/// <summary>
		/// Moves the item from one spot to another spot, possible even
		/// over region boundaries.
		/// </summary>
		/// <param name="regionID">new regionid</param>
		/// <param name="newPosition">The new position.</param>
		/// <param name="heading">new heading</param>
		/// <returns>true if moved</returns>
		public virtual bool MoveTo(ushort regionID, Point newPosition, ushort heading)
		{
			if (m_ObjectState != eObjectState.Active)
				return false;

			Region rgn = WorldMgr.GetRegion(regionID);
			if (rgn == null)
				return false;
			if (rgn.GetZone(newPosition) == null)
				return false;

			Notify(GameObjectEvent.MoveTo, this, new MoveToEventArgs(regionID, newPosition, heading));

			if (!RemoveFromWorld())
				return false;
			m_position = newPosition;
			m_Heading = heading;
			RegionId = regionID;
			return AddToWorld();
		}

		/// <summary>
		/// Marks this object as deleted!
		/// </summary>
		public virtual void Delete()
		{
			Notify(GameObjectEvent.Delete, this);
			RemoveFromWorld();
			ObjectState = eObjectState.Deleted;
		}

		/// <summary>
		/// Holds the GameTick of when this object was added to the world
		/// </summary>
		protected long m_spawnTick = 0;

		/// <summary>
		/// Gets the GameTick of when this object was added to the world
		/// </summary>
		public long SpawnTick
		{
			get { return m_spawnTick; }
		}

		#endregion

		#region Interact

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public virtual bool Interact(GamePlayer player)
		{
			if (!Position.CheckSquareDistance(player.Position, (uint)(WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE)))
			{
				player.Out.SendMessage("You are too far away to interact with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			Notify(GameObjectEvent.Interact, this, new InteractEventArgs(player));
			player.Notify(GameObjectEvent.InteractWith, player, new InteractWithEventArgs(this));

			return true;
		}

		#endregion

		#region ConLevel/DurLevel

		/// <summary>
		/// Calculate con-level against other object
		/// &lt;=-3 = grey
		/// -2 = green
		/// -1 = blue
		/// 0 = yellow (same level)
		/// 1 = orange
		/// 2 = red
		/// &gt;=3 = violet
		/// </summary>
		/// <returns>conlevel</returns>
		public double GetConLevel(GameObject compare)
		{
			return GetConLevel(EffectiveLevel, compare.EffectiveLevel);
//			return (compare.Level - Level) / ((double)(Level / 10 + 1));
		}

		/// <summary>
		/// Calculate con-level against other compareLevel
		/// &lt;=-3 = grey
		/// -2 = green
		/// -1 = blue  (compareLevel is 1 con lower)
		/// 0 = yellow (same level)
		/// 1 = orange (compareLevel is 1 con higher)
		/// 2 = red
		/// &gt;=3 = violet
		/// </summary>
		/// <returns>conlevel</returns>
		public static double GetConLevel(int level, int compareLevel)
		{
			int constep = Math.Max(1, (level+9)/10);
			double stepping = 1.0 / constep;
			int leveldiff = level - compareLevel + 1;
			return 1.0 - leveldiff * stepping;
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

		#region ObjectsInRadius
		/********************ADDED BY KONIK & WITCHKING FOR NEW GETINRADIUS SYSTEM********************/
		#region OIRData
		public class OIRData
		{
			private class OIRElement
			{
				private IEnumerable[][] m_enumeratorLists = null;

				/// <summary>
				/// Constructor
				/// </summary>
				public OIRElement() 
				{
					m_enumeratorLists = new IEnumerable[3][];
					for (int i = 0; i < m_enumeratorLists.Length; i++) 
					{
						m_enumeratorLists[i] = new IEnumerable[2];
					}
				}

				/// <summary>
				/// This function will check if the data cache must be recomputed or if we can use the cache
				/// </summary>
				/// <param name="mustUpdate">force the update of the list</param>
				/// <param name="p_currentRegion">The current region</param>
				/// <param name="p_type">Type of object to retreive</param>
				/// <param name="p_pos">position in region</param>
				/// <param name="p_radius">Radius to check</param>
				/// <param name="p_withDistance">We want the distance</param>
				/// <returns></returns>
				public IEnumerable GetInRadius(bool mustUpdate,Region p_currentRegion,Zone.eGameObjectType p_type, Point p_pos, ushort p_radius,bool p_withDistance)
				{
					int type = (int) p_type;
					int distanceIndex = (p_withDistance) ? 0 : 1;
					IEnumerable candidateEnumerator = m_enumeratorLists[type][distanceIndex];

					// get the right list and update it if necessary
					if (mustUpdate || (candidateEnumerator == null)) 
					{
						switch (p_type) 
						{
							case Zone.eGameObjectType.ITEM: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetItemsInRadius(p_pos, p_radius, p_withDistance); break;
							case Zone.eGameObjectType.NPC: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetNPCsInRadius(p_pos, p_radius, p_withDistance); break;
							default: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetPlayerInRadius(p_pos, p_radius, p_withDistance); break;
						};

						candidateEnumerator = m_enumeratorLists[type][distanceIndex];
					}

					((IEnumerator) candidateEnumerator).Reset();
					return candidateEnumerator;
				}
			}


			// the region attached to this OIRData
			private Region m_currentRegion = null;

			// the OIRElements of this OIRData
			private readonly DOL.GS.Collections.Hashtable m_oirElements = new DOL.GS.Collections.Hashtable();

			// constants
			private const short UPDATE_RATE_ALIVE = 100;
			private const short UPDATE_RATE_DEAD = 500;
			private const short POS_OFFSET = 35;
			
			// timing information
			private readonly int[] m_lastUpdate = new int[3];
				
			// position information
			private readonly Point[] m_lastXYZ = new Point[3<<1]; // 3types and two fields for distance and without

			/// <summary>
			/// This function will check if the data cache must be recomputed or if we can use the cache
			/// </summary>
			/// <param name="p_currentRegion">The current region</param>
			/// <param name="p_type">Type of object to retreive</param>
			/// <param name="p_pos">position in region</param>
			/// <param name="p_radius">Radius to check</param>
			/// <param name="p_withDistance">We want the distance</param>
			/// <returns></returns>
			public IEnumerable GetInRadius(GameObject p_obj, Region p_currentRegion, Zone.eGameObjectType p_type, Point p_pos, ushort p_radius, bool p_withDistance)
			{
				bool mustUpdate = false; // use cache when we can
				
				// first check that we didn't changed region
				OIRElement curElem = null;
				if (p_currentRegion != m_currentRegion) 
				{
					// if we changed region empty hashtable
					m_oirElements.Clear();

					// set the new current region
					m_currentRegion = p_currentRegion;

					// force an update
					mustUpdate = true;
				}
				else 
				{
					// try to get the OIRElement for the current radius
					curElem = (OIRElement)m_oirElements[p_radius];
				}

				// if nothing found create and set the element for this radius
				if(curElem == null) 
				{
					curElem = new OIRElement();
					m_oirElements[p_radius]=curElem;
				}

				// set some variables
				int  type = (int) p_type;
				int  index = (p_withDistance) ? 0 : 1;
				int  currentTick = Environment.TickCount;
				bool isAlive = true;
				int  neededTick=0;

				// get the right time tracker
				neededTick = m_lastUpdate[type];

				// check if the gameobject is dead
				isAlive = p_obj is GameLiving && ((GameLiving)p_obj).Alive;

				int i = (type<<1)+index;
				// check that we must update data
				if	( mustUpdate ||
					( isAlive && //we are alive and
					(currentTick >= (neededTick+UPDATE_RATE_ALIVE)  ||  // its a long time we didn't update
					(FastMath.Abs(m_lastXYZ[i].X - p_pos.X) > POS_OFFSET) || // we have moved at least POS_DECAL in one direction
					(FastMath.Abs(m_lastXYZ[i].Y - p_pos.Y) > POS_OFFSET) ||
					(FastMath.Abs(m_lastXYZ[i].Z - p_pos.Z) > POS_OFFSET)
					)
					) || (!isAlive && // we are dead
					(currentTick >= (neededTick+UPDATE_RATE_DEAD)       // its a long time we didnt update
					)
					)
					)
				{
					mustUpdate = true;
					m_lastXYZ[i] = p_pos;

					m_lastUpdate[type] = currentTick;
				}

				// get the right object list
				return curElem.GetInRadius(mustUpdate,p_currentRegion,p_type, p_pos, p_radius,p_withDistance);
			}
		}

		protected readonly OIRData m_oirData = new OIRData();
		#endregion
		
		
		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(ushort radiusToCheck) {
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetPlayersInRadius(radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(ushort radiusToCheck, bool withDistance) {
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (Region != null)
			{
				return m_oirData.GetInRadius(this, Region, Zone.eGameObjectType.PLAYER, Position, radiusToCheck, withDistance);
			}
			return new Region.EmptyEnumerator();
			/***************************************************************/
		}

		/// <summary>
		/// Gets all npcs close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetNPCsInRadius(ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetNPCsInRadius(radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all npcs close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetNPCsInRadius(ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (Region != null) 
			{
				return m_oirData.GetInRadius(this,Region,Zone.eGameObjectType.NPC, Position, radiusToCheck, withDistance);
			}
			return new Region.EmptyEnumerator();
			/***************************************************************/
		}

		/// <summary>
		/// Gets all items close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetItemsInRadius(ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetItemsInRadius(radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all items close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetItemsInRadius(ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (Region != null)
			{
				return m_oirData.GetInRadius(this,Region,Zone.eGameObjectType.ITEM, Position, radiusToCheck, withDistance);
			}
			return new Region.EmptyEnumerator();
			/***************************************************************/
		}
		#endregion

		#region Item/Money

		/// <summary>
		/// Called when the object is about to get an item from someone
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public virtual bool ReceiveItem(GameLiving source, GenericItem item)
		{
			return false;
		}

		/// <summary>
		/// Called when the object is about to get an item from someone
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="templateID">templateID for item to add</param>
		/// <returns>true if the item was successfully received</returns>
		public virtual bool ReceiveItem(GameLiving source, string templateID)
		{
			GenericItemTemplate template = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), templateID);
			if (template == null)
			{
				if (log.IsErrorEnabled)
					log.Error("Item Creation: ItemTemplate not found ID=" + templateID);
				return false;
			}

			return ReceiveItem(source, template.CreateInstance());
		}

		/// <summary>
		/// Receive an item from a living
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns>true if player took the item</returns>
		public virtual bool ReceiveItem(GameLiving source, GameInventoryItem item)
		{
			return ReceiveItem(source, item.Item);
		}

		/// <summary>
		/// Called when the object is about to get money from someone
		/// </summary>
		/// <param name="source">Source from where to get the money</param>
		/// <param name="money">array of money to get</param>
		/// <returns>true if the money was successfully received</returns>
		public virtual bool ReceiveMoney(GameLiving source, long money)
		{
			return false;
		}

		#endregion

		/// <summary>
		/// Returns the string representation of the GameObject
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			Region reg = Region;
			return new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" name=").Append(Name)
				.Append(" DB_ID=").Append(InternalID)
				.Append(" oid=").Append(ObjectID.ToString())
				.Append(" state=").Append(ObjectState.ToString())
				.Append(" reg=").Append(reg == null ? "null" : reg.RegionID.ToString())
				.Append(" loc=").Append(Position.ToString())
				.ToString();
		}

		/// <summary>
		/// Constructs a new empty GameObject
		/// </summary>
		public GameObject()
		{
			//Objects should NOT be saved back to the DB
			//as standard! We want our mobs/items etc. at
			//the same startingspots when we restart!
			m_Name = "";
			m_ObjectState = eObjectState.Inactive;
		}
	}
}