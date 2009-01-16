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

using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY object in the game world needs!
	/// </summary>
	public abstract class GameObject : Point3D
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

        /*
		/// <summary>
		/// The Object's X inside the current Region
		/// </summary>
		protected int m_X;

		/// <summary>
		/// The Object's Y inside the current Region
		/// </summary>
		protected int m_Y;

		/// <summary>
		/// The Object's Z inside the current Region
		/// </summary>
		protected int m_Z;*/

		/// <summary>
		/// The Object's current Region
		/// </summary>
		protected Region m_CurrentRegion;

		/// <summary>
		/// The direction the Object is facing
		/// </summary>
		protected ushort m_Heading;

		/// <summary>
		/// Holds the realm of this object
		/// </summary>
		protected eRealm m_Realm;

		/// <summary>
		/// Gets or Sets the current Realm of the Object
		/// </summary>
		public virtual eRealm Realm
		{
			get { return m_Realm; }
			set { m_Realm = value; }
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public virtual Region CurrentRegion
		{
			get { return m_CurrentRegion; }
			set { m_CurrentRegion = value; }
		}

		/// <summary>
		/// Get's or sets the current Region by the ID
		/// </summary>
		public virtual ushort CurrentRegionID
		{
			get { return m_CurrentRegion == null ? (ushort)0 : m_CurrentRegion.ID; }
			set
			{
				CurrentRegion = WorldMgr.GetRegion(value);
			}
		}

		/// <summary>
		/// Gets the current Zone of the Object
		/// </summary>
		public Zone CurrentZone
		{
			get
			{
				if (m_CurrentRegion != null)
				{
					return m_CurrentRegion.GetZone(X, Y);
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public virtual ushort Heading
		{
			get { return m_Heading; }
			set { m_Heading = (ushort)(value & 0xFFF); }
		}

        /// <summary>
        /// Calculates the heading this object needs to have to face the target spot
        /// </summary>
        /// <param name="tx">target x</param>
        /// <param name="ty">target y</param>
        /// <returns>the heading towards the target spot</returns>
        [Obsolete( "Use GetHeading" )]
        public ushort GetHeadingToSpot( int tx, int ty )
        {
            return GetHeading( new Point2D( tx, ty ) );
        }

        /// <summary>
        /// Calculates the heading this object needs to have, to face the target
        /// </summary>
        /// <param name="target">IPoint3D target</param>
        /// <returns>the heading towards the target</returns>
        [Obsolete( "Use GetHeading" )]
        public ushort GetHeadingToTarget( IPoint3D target )
        {
            return GetHeading( target );
        }

        /// <summary>
        /// Returns the angle towards a target, clockwise
        /// </summary>
        /// <param name="target">the target</param>
        /// <returns>the angle towards the target</returns>
        [Obsolete( "Use GetAngleToPoint" )]
        public float GetAngleToTarget( GameObject target )
        {
            return GetAngleToPoint( target );
        }

        [Obsolete( "Use GetAngleToPoint" )]
        public float GetAngleToSpot( int x, int y )
        {
            return GetAngleToPoint( new Point2D( x, y ) );
        }

        [Obsolete( "Use GetPointFromHeading" )]
        public void GetSpotFromHeading( int distance, out int tx, out int ty )
        {
            Point2D point = GetPointFromHeading( this.Heading, distance );
            tx = point.X;
            ty = point.Y;
        }

		/// <summary>
		/// Returns the angle towards a target spot in degrees, clockwise
		/// </summary>
		/// <param name="tx">target x</param>
		/// <param name="ty">target y</param>
		/// <returns>the angle towards the spot</returns>
		public float GetAngleToPoint( IPoint2D point )
		{
			float headingDifference = ( GetHeading( point ) & 0xFFF ) - ( this.Heading & 0xFFF );

			if (headingDifference < 0)
				headingDifference += 4096.0f;

			return (headingDifference * 360.0f / 4096.0f);
		}

        /// <summary>
        /// Get distance to a GameObject
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <returns>The distance, or -1 if the target object is null or in a different region</returns>
        /*public int GetDistance( GameObject obj )
        {
            if ( obj == null || this.CurrentRegionID != obj.CurrentRegionID )
                return -1;
            else
                return GetDistance( (IPoint3D)obj );
        }*/

        /// <summary>
        /// Get distance to a GameObject (with z-axis adjustment)
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="zfactor">Z-axis factor - use values between 0 and 1 to decrease influence of Z-axis</param>
        /// <returns>Adjusted distance, or -1 if the target object is null or in a different region</returns>
        /*public int GetDistance( GameObject obj, double zfactor )
        {
            if ( obj == null || this.CurrentRegionID != obj.CurrentRegionID )
                return -1;
            else
                return base.GetDistance( (IPoint3D)obj, zfactor );
        }*/

        /// <summary>
        /// Get distance to a point
        /// </summary>
        /// <remarks>
        /// If either Z-value is zero, the z-axis is ignored
        /// </remarks>
        /// <param name="point">Target point</param>
        /// <returns>Distance</returns>
        public override int GetDistance( IPoint3D point )
        {
			GameObject obj = point as GameObject;

			if ( obj == null || this.CurrentRegionID == obj.CurrentRegionID )
			{
				return base.GetDistance( point );
			}
			else
			{
				return -1;
			}
        }

        /// <summary>
        /// Get distance to a point (with z-axis adjustment)
        /// </summary>
        /// <remarks>
        /// If either Z-value is zero, the z-axis is ignored
        /// </remarks>
        /// <param name="point">Target point</param>
        /// <param name="zfactor">Z-axis factor - use values between 0 and 1 to decrease the influence of Z-axis</param>
        /// <returns>Adjusted distance</returns>
        public override int GetDistance( IPoint3D point, double zfactor )
        {
			GameObject obj = point as GameObject;

			if ( obj == null || this.CurrentRegionID == obj.CurrentRegionID )
			{
				return base.GetDistance( point, zfactor );
			}
			else
			{
				return -1;
			}
        }

        /// <summary>
        /// Checks if an object is within a given radius
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="radius">Radius</param>
        /// <returns>False if the object is null, in a different region, or outside the radius; otherwise true</returns>
        public bool IsWithinRadius( GameObject obj, int radius )
        {
            if ( obj == null )
                return false;

            if ( this.CurrentRegionID != obj.CurrentRegionID )
                return false;

			return base.IsWithinRadius( obj, radius );
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
			return IsObjectInFront(target, viewangle, true);
		}

		/// <summary>
		/// determines wether a target object is front
		/// in front is defined as north +- viewangle/2
		/// </summary>
		/// <param name="target"></param>
		/// <param name="viewangle"></param>
		/// <param name="rangeCheck"></param>
		/// <returns></returns>
		public virtual bool IsObjectInFront(GameObject target, double viewangle, bool rangeCheck)
		{
			if (target == null)
				return false;
			float angle = this.GetAngleToPoint(target);
			if (angle >= 360 - viewangle / 2 || angle < viewangle / 2)
				return true;
			// if target is closer than 32 units it is considered always in view
			// tested and works this way for normal evade, parry, block (in 1.69)
			if (rangeCheck)
                return this.IsWithinRadius( target, 32 );
			else
                return false;
		}

		/// <summary>
		/// Checks if object is underwater
		/// </summary>
		public bool IsUnderwater
		{
			get
			{
				if (CurrentRegion == null)
					return false;
				// Special land areas below the waterlevel in NF
				if (CurrentRegion.ID == 163)
				{
					// Mount Collory
					if ((Y > 664000) && (Y < 670000) && (X > 479000) && (X < 488000)) return false;
					if ((Y > 656000) && (Y < 664000) && (X > 472000) && (X < 488000)) return false;
					if ((Y > 624000) && (Y < 654000) && (X > 468500) && (X < 488000)) return false;
					if ((Y > 659000) && (Y < 683000) && (X > 431000) && (X < 466000)) return false;
					if ((Y > 646000) && (Y < 659001) && (X > 431000) && (X < 460000)) return false;
					if ((Y > 624000) && (Y < 646001) && (X > 431000) && (X < 455000)) return false;
					if ((Y > 671000) && (Y < 683000) && (X > 431000) && (X < 471000)) return false;
					// Breifine
					if ((Y > 558000) && (Y < 618000) && (X > 456000) && (X < 479000)) return false;
					// Cruachan Gorge
					if ((Y > 586000) && (Y < 618000) && (X > 360000) && (X < 424000)) return false;
					if ((Y > 563000) && (Y < 578000) && (X > 360000) && (X < 424000)) return false;
					// Emain Macha
					if ((Y > 505000) && (Y < 555000) && (X > 428000) && (X < 444000)) return false;
					// Hadrian's Wall
					if ((Y > 500000) && (Y < 553000) && (X > 603000) && (X < 620000)) return false;
					// Snowdonia
					if ((Y > 633000) && (Y < 678000) && (X > 592000) && (X < 617000)) return false;
					if ((Y > 662000) && (Y < 678000) && (X > 581000) && (X < 617000)) return false;
					// Sauvage Forrest
					if ((Y > 584000) && (Y < 615000) && (X > 626000) && (X < 681000)) return false;
					// Uppland
					if ((Y > 297000) && (Y < 353000) && (X > 610000) && (X < 652000)) return false;
					// Yggdra
					if ((Y > 408000) && (Y < 421000) && (X > 671000) && (X < 693000)) return false;
					if ((Y > 364000) && (Y < 394000) && (X > 674000) && (X < 716000)) return false;
				}
				return Z < CurrentRegion.WaterLevel;
			}
		}

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		public virtual IList CurrentAreas
		{
			get
			{
				if (CurrentZone != null)
					return CurrentZone.GetAreasOfSpot(this);
				return new ArrayList();
			}
			set { }
		}

		private House m_currentHouse;
		public House CurrentHouse
		{
			get { return m_currentHouse; }
			set { m_currentHouse = value; }
		}
		private bool m_inHouse;
		public bool InHouse
		{
			get { return m_inHouse; }
			set { m_inHouse = value; }
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
		protected ushort m_Model;

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
			set { }
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
		public virtual ushort Model
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
			if (char.IsUpper(Name[0]) && this is GameLiving) // proper noun
			{
				return Name;
			}
			else // common noun
				if (article == 0)
				{
					if (firstLetterUppercase)
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article1", Name);
					else
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article2", Name);
				}
				else
				{
					// if first letter is a vowel
					if (m_vowels.IndexOf(Name[0]) != -1)
					{
						if (firstLetterUppercase)
							return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article3", Name);
						else
							return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article4", Name);
					}
					else
					{
						if (firstLetterUppercase)
							return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article5", Name);
						else
							return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetName.Article6", Name);
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
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun1");
					else
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun2");
				case 1: // Possessive
					if (firstLetterUppercase)
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun3");
					else
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun4");
				case 2: // Objective
					if (firstLetterUppercase)
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun5");
					else
						return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameObject.GetPronoun.Pronoun6");
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
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameObject.GetExamineMessages.YouTarget", GetName(0, false)));
			return list;
		}

		#endregion

		#region IDs/Database

		/// <summary>
		/// True if this object is saved in the DB
		/// </summary>
		protected bool m_saveInDB;

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
		/// Sets the state for this object on whether or not it is saved in the database
		/// </summary>
		public bool SaveInDB
		{
			get { return m_saveInDB; }
			set { m_saveInDB = value; }
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
		public virtual void LoadFromDatabase(DataObject obj)
		{
			InternalID = obj.ObjectId;
		}

		/// <summary>
		/// Deletes a character from the DB
		/// </summary>
		public virtual void DeleteFromDatabase()
		{
			GameBoat boat = BoatMgr.GetBoatByOwner(InternalID);
			if (boat != null)
				boat.DeleteFromDatabase();
		}

		#endregion

		#region Add-/Remove-/Create-/Move-

		/// <summary>
		/// Creates this object in the gameworld
		/// </summary>
		/// <param name="regionID">region target</param>
		/// <param name="x">x target</param>
		/// <param name="y">y target</param>
		/// <param name="z">z target</param>
		/// <param name="heading">heading</param>
		/// <returns>true if created successfully</returns>
		public virtual bool Create(ushort regionID, int x, int y, int z, ushort heading)
		{
			if (m_ObjectState == eObjectState.Active)
				return false;
			CurrentRegionID = regionID;
			m_x = x;
			m_y = y;
			m_z = z;
			m_Heading = heading;
			return AddToWorld();
		}

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

			if (!m_CurrentRegion.AddObject(this))
				return false;
			Notify(GameObjectEvent.AddToWorld, this);
			ObjectState = eObjectState.Active;

			CurrentZone.ObjectEnterZone(this);
			/*********** END OF MODIFICATION ***********/

			m_spawnTick = CurrentRegion.Time;
			return true;
		}

		/// <summary>
		/// Removes the item from the world
		/// </summary>
		public virtual bool RemoveFromWorld()
		{
			if (m_CurrentRegion == null || ObjectState != eObjectState.Active)
				return false;
			Notify(GameObjectEvent.RemoveFromWorld, this);
			ObjectState = eObjectState.Inactive;
			m_CurrentRegion.RemoveObject(this);
			return true;
		}

		/// <summary>
		/// Moves the item from one spot to another spot, possible even
		/// over region boundaries
		/// </summary>
		/// <param name="regionID">new regionid</param>
		/// <param name="x">new x</param>
		/// <param name="y">new y</param>
		/// <param name="z">new z</param>
		/// <param name="heading">new heading</param>
		/// <returns>true if moved</returns>
		public virtual bool MoveTo(ushort regionID, int x, int y, int z, ushort heading)
		{
			if (m_ObjectState != eObjectState.Active)
				return false;

			Region rgn = WorldMgr.GetRegion(regionID);
			if (rgn == null)
				return false;
			if (rgn.GetZone(x, y) == null)
				return false;

			Notify(GameObjectEvent.MoveTo, this, new MoveToEventArgs(regionID, x, y, z, heading));

			if (!RemoveFromWorld())
				return false;
			m_x = x;
			m_y = y;
			m_z = z;
			m_Heading = heading;
			CurrentRegionID = regionID;
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
			if (player.Client.Account.PrivLevel == 1 && !this.IsWithinRadius(player, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation("GameObject.Interact.TooFarAway", GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Notify(GameObjectEvent.InteractFailed, this, new InteractEventArgs(player));
				return false;
			}
			Notify(GameObjectEvent.Interact, this, new InteractEventArgs(player));
			player.Notify(GameObjectEvent.InteractWith, player, new InteractWithEventArgs(this));

			return true;
		}

		#endregion

		#region Combat

		/// <summary>
		/// This method is called whenever this living 
		/// should take damage from some source
		/// </summary>
		/// <param name="source">the damage source</param>
		/// <param name="damageType">the damage type</param>
		/// <param name="damageAmount">the amount of damage</param>
		/// <param name="criticalAmount">the amount of critical damage</param>
		public virtual void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			Notify(GameObjectEvent.TakeDamage, this, new TakeDamageEventArgs(source, damageType, damageAmount, criticalAmount));
		}

		#endregion

		#region Immunity

		private bool m_immuneToMagic = false;

		/// <summary>
		/// Whether or not this object is immune to magic.
		/// </summary>
		public bool IsImmuneToMagic
		{
			get { return m_immuneToMagic; }
			set { m_immuneToMagic = value; }
		}

		private bool m_immuneToMelee = false;

		/// <summary>
		/// Whether or not this object is immune to melee.
		/// </summary>
		public bool IsImmuneToMelee
		{
			get { return m_immuneToMelee; }
			set { m_immuneToMelee = value; }
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
			int constep = Math.Max(1, (level + 9) / 10);
			double stepping = 1.0 / constep;
			int leveldiff = level - compareLevel + 1;
			return 1.0 - leveldiff * stepping;
		}

		/// <summary>
		/// Calculate a level based on source level and a con level
		/// </summary>
		/// <param name="level"></param>
		/// <param name="con"></param>
		/// <returns></returns>
		public static int GetLevelFromCon(int level, double con)
		{
			int constep = Math.Max(1, (level + 10) / 10);
			return Math.Max((int)0, (int)(level + constep * con));
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
		/*#region OIRData
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
				/// <param name="p_X">X position in region</param>
				/// <param name="p_Y">Y position in region</param>
				/// <param name="p_Z">Z position in region</param>
				/// <param name="p_radius">Radius to check</param>
				/// <param name="p_withDistance">We want the distance</param>
				/// <returns></returns>
				public IEnumerable GetInRadius(bool mustUpdate, Region p_currentRegion, Zone.eGameObjectType p_type, int p_X, int p_Y, int p_Z, ushort p_radius, bool p_withDistance)
				{
					int type = (int)p_type;
					int distanceIndex = (p_withDistance) ? 0 : 1;
					IEnumerable candidateEnumerator = m_enumeratorLists[type][distanceIndex];

					// get the right list and update it if necessary
					if (mustUpdate || (candidateEnumerator == null))
					{
						switch (p_type)
						{
							case Zone.eGameObjectType.ITEM: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetItemsInRadius(p_X, p_Y, p_Z, p_radius, p_withDistance); break;
							case Zone.eGameObjectType.NPC: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetNPCsInRadius(p_X, p_Y, p_Z, p_radius, p_withDistance); break;
							default: m_enumeratorLists[type][distanceIndex] = p_currentRegion.GetPlayerInRadius(p_X, p_Y, p_Z, p_radius, p_withDistance); break;
						};

						candidateEnumerator = m_enumeratorLists[type][distanceIndex];
					}

					((IEnumerator)candidateEnumerator).Reset();
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
			private readonly XYZStruct[] m_lastXYZ = new XYZStruct[3 << 1]; // 3types and two fields for distance and without

			public struct XYZStruct
			{
				public int X;
				public int Y;
				public int Z;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			public OIRData()
			{
				//				for (int i = 0; i < m_lastX.Length; i++) 
				//				{
				//					m_lastX[i] = new int[2];
				//					m_lastY[i] = new int[2];
				//					m_lastZ[i] = new int[2];
				//				}
			}
			/// <summary>
			/// This function will check if the data cache must be recomputed or if we can use the cache
			/// </summary>
			/// <param name="p_obj">The GameObject object instance</param>
			/// <param name="p_useCache">use the cache</param>
			/// <param name="p_currentRegion">The current region</param>
			/// <param name="p_type">Type of object to retreive</param>
			/// <param name="p_X">X position in region</param>
			/// <param name="p_Y">Y position in region</param>
			/// <param name="p_Z">Z position in region</param>
			/// <param name="p_radius">Radius to check</param>
			/// <param name="p_withDistance">We want the distance</param>
			/// <returns></returns>
			public IEnumerable GetInRadius(GameObject p_obj, Region p_currentRegion, Zone.eGameObjectType p_type, int p_X, int p_Y, int p_Z, ushort p_radius, bool p_withDistance, bool p_useCache)
			{
				// first check that we didn't changed region
				OIRElement curElem = null;
				if (p_currentRegion != m_currentRegion)
				{
					// if we changed region empty hashtable
					m_oirElements.Clear();

					// set the new current region
					m_currentRegion = p_currentRegion;

					// force an update
					p_useCache = false;
				}
				else
				{
					// try to get the OIRElement for the current radius
					curElem = (OIRElement)m_oirElements[p_radius];
				}

				// if nothing found create and set the element for this radius
				if (curElem == null)
				{
					curElem = new OIRElement();
					m_oirElements[p_radius] = curElem;
				}

				// set some variables
				int type = (int)p_type;
				int index = (p_withDistance) ? 0 : 1;
				int currentTick = Environment.TickCount;
				bool mustUpdate = false;
				bool isAlive = true;
				int neededTick = 0;

				// get the right time tracker
				neededTick = m_lastUpdate[type];

				// check if the gameobject is dead
				isAlive = p_obj is GameLiving && ((GameLiving)p_obj).IsAlive;

				int i = (type << 1) + index;
				// check that we must update data
				if (!p_useCache ||
					(isAlive && //we are alive and
					(currentTick >= (neededTick + UPDATE_RATE_ALIVE) ||  // its a long time we didn't update
					(FastMath.Abs(m_lastXYZ[i].X - p_X) > POS_OFFSET) || // we have moved at least POS_DECAL in one direction
					(FastMath.Abs(m_lastXYZ[i].Y - p_Y) > POS_OFFSET) ||
					(FastMath.Abs(m_lastXYZ[i].Z - p_Z) > POS_OFFSET)
					)
					) || (!isAlive && // we are dead
					(currentTick >= (neededTick + UPDATE_RATE_DEAD)       // its a long time we didnt update
					)
					)
					)
				{
					mustUpdate = true;
					m_lastXYZ[i].X = p_X;
					m_lastXYZ[i].Y = p_Y;
					m_lastXYZ[i].Z = p_Z;

					m_lastUpdate[type] = currentTick;
				}

				// get the right object list
				return curElem.GetInRadius(mustUpdate, p_currentRegion, p_type, p_X, p_Y, p_Z, p_radius, p_withDistance);
			}
		}

		protected readonly OIRData m_oirData = new OIRData();
		#endregion*/


		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetPlayersInRadius(false, radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="useCache">true may return a cached result, false not.</param>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(bool useCache, ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetPlayersInRadius(useCache, radiusToCheck, false);
			/***************************************************************/
		}



		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetPlayersInRadius(true, radiusToCheck, withDistance);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all players close to this object inside a certain radius
		/// </summary>
		/// <param name="useCache">true may return a cached result, false not.</param>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetPlayersInRadius(bool useCache, ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentRegion.GetZone(X, Y) == null)
				{
					if (this is GamePlayer && (this as GamePlayer).Client.Account.PrivLevel < 3 && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					return CurrentRegion.GetPlayersInRadius(X, Y, Z, radiusToCheck, withDistance);
				}
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
			/*
			if (CurrentRegion != null)
				return m_oirData.GetInRadius(this,CurrentRegion,Zone.eGameObjectType.NPC, X, Y, Z, radiusToCheck, false);
			return new Region.EmptyEnumerator();
			*/
			return GetNPCsInRadius(true, radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all npcs close to this object inside a certain radius
		/// </summary>
		/// <param name="useCache">use the cache</param>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetNPCsInRadius(bool useCache, ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			/*
			if (CurrentRegion != null)
				return m_oirData.GetInRadius(this,CurrentRegion,Zone.eGameObjectType.NPC, X, Y, Z, radiusToCheck, false);
			return new Region.EmptyEnumerator();
			*/
			return GetNPCsInRadius(useCache, radiusToCheck, false);
			/***************************************************************/
		}

		/// <summary>
		/// Gets all npcs close to this object inside a certain radius
		/// </summary>
		/// <param name="useCache">use the cache</param>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetNPCsInRadius(bool useCache, ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentRegion.GetZone(X, Y) == null)
				{
					if (this is GamePlayer && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					IEnumerable result = CurrentRegion.GetNPCsInRadius(X, Y, Z, radiusToCheck, withDistance);
					return result;
				}
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
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentRegion.GetZone(X, Y) == null)
				{
					if (this is GamePlayer && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					return CurrentRegion.GetItemsInRadius(X, Y, Z, radiusToCheck, withDistance);
				}
			}
			return new Region.EmptyEnumerator();
			/***************************************************************/
		}

		/// <summary>
		/// Gets all doors close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetDoorsInRadius(ushort radiusToCheck)
		{
			return GetDoorsInRadius(radiusToCheck, false);
		}

		/// <summary>
		/// Gets all doors close to this object inside a certain radius
		/// </summary>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <param name="withDistance">if the objects are to be returned with distance</param>
		/// <returns>An enumerator</returns>
		public IEnumerable GetDoorsInRadius(ushort radiusToCheck, bool withDistance)
		{
			if (CurrentRegion != null)
			{
				//Eden : avoid server freeze
				if (CurrentRegion.GetZone(X, Y) == null)
				{
					if (this is GamePlayer && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					return CurrentRegion.GetDoorsInRadius(X, Y, Z, radiusToCheck, withDistance);
				}
			}
			return new Region.EmptyEnumerator();
		}
		#endregion

		#region Item/Money

		/// <summary>
		/// Called when the object is about to get an item from someone
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public virtual bool ReceiveItem(GameLiving source, InventoryItem item)
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
			ItemTemplate template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), templateID);
			if (template == null)
			{
				if (log.IsErrorEnabled)
					log.Error("Item Creation: ItemTemplate not found ID=" + templateID);
				return false;
			}

			return ReceiveItem(source, new InventoryItem(template));
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
			Region reg = CurrentRegion;
			return new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" name=").Append(Name)
				.Append(" DB_ID=").Append(InternalID)
				.Append(" oid=").Append(ObjectID.ToString())
				.Append(" state=").Append(ObjectState.ToString())
				.Append(" reg=").Append(reg == null ? "null" : reg.ID.ToString())
				.Append(" loc=").Append(X.ToString()).Append(',').Append(Y.ToString()).Append(',').Append(Z.ToString())
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
			m_saveInDB = false;
			m_Name = "";
			m_ObjectState = eObjectState.Inactive;
			m_boat_ownerid = "";
		}
		public static bool PlayerHasItem(GamePlayer player, string str)
		{
			InventoryItem item = player.Inventory.GetFirstItemByID(str, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
			if (item != null)
				return true;
			return false;
		}
		private static string m_boat_ownerid;
		public static string ObjectHasOwner()
		{
			if (m_boat_ownerid == "")
				return "";
			else
				return m_boat_ownerid;
		}
	}
}
