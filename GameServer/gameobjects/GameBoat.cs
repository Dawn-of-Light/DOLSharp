using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS;
using DOL.Database;
using DOL.Language;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using log4net;
using DOL.AI.Brain;

namespace DOL.GS
{
	public class GameBoat : GameMovingObject
	{
        private byte m_type = 0;
        protected DBBoat m_DBboat;
        private string boat_id;
        private string boat_owner;
        private string boat_name;
        private ushort boat_model;
        private short boat_maxspeedbase;

		private RegionTimer m_removeTimer = null;

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GameBoat(byte type)
            : base()
        {
            m_type = type;
            base.BoatOwnerID = OwnerID;
        }

        public GameBoat()
			: base()
        {
            base.BoatOwnerID = OwnerID;
        }

		public override bool AddToWorld()
		{
            if (!base.AddToWorld())
            {
                return false;
            }
            return true;            
        }

        /// <summary>
        /// Gets or sets the boats db
        /// </summary>
        public DBBoat theBoatDB
        {
            get { return m_DBboat; }
            set { m_DBboat = value; }
        }

        public string BoatID
        {
            get
            {
                return boat_id;
            }
            set
            {
                boat_id = value;
            }
        }
        
        public override string Name
        {
            get
            {
                return boat_name;
            }
            set
            {
                boat_name = value;
            }
        }

        public override ushort Model
        {
            get
            {
                return boat_model;
            }
            set
            {
                boat_model = value;
            }
        }

        public override short MaxSpeedBase
        {
            get
            {
                return boat_maxspeedbase;
            }
            set
            {
                boat_maxspeedbase = value;
            }
        }

        public string OwnerID
        {
            get
            {
                return boat_owner;
            }
            set
            {
                boat_owner = value;
            }
        }

        public override int MAX_PASSENGERS
        {
            get
            {
                switch (m_type)
                {
                    case 0: return 8;
                    case 1: return 8;
                    case 2: return 16;
                    case 3: return 32;
                    case 4: return 32;
                    case 5: return 31;
                    case 6: return 24;
                    case 7: return 64;
                    case 8: return 33;
                    default: return 2;
                }
            }
        }

		public override int REQUIRED_PASSENGERS
		{
			get
			{
				switch (m_type)
				{
                    case 0: return 1;
                    case 1: return 1;
                    case 2: return 1;
                    case 3: return 1;
                    case 4: return 1;
                    case 5: return 1;
                    case 6: return 1;
                    case 7: return 1;
                    case 8: return 1;
                    default: return 1;
				}
			}
		}

		public override int SLOT_OFFSET
		{
			get
			{
				return 1;
			}
		}

		public override bool RiderMount(GamePlayer rider, bool forced)
		{
			if (!base.RiderMount(rider, forced))
				return false;

			if (m_removeTimer != null && m_removeTimer.IsAlive)
				m_removeTimer.Stop();

			return true;
		}

		public override bool RiderDismount(bool forced, GamePlayer player)
		{
			if (!base.RiderDismount(forced, player))
				return false;

			if (CurrentRiders.Length == 0)
			{
				if (m_removeTimer == null)
					m_removeTimer = new RegionTimer(this, new RegionTimerCallback(RemoveCallback));
				else if (m_removeTimer.IsAlive)
					m_removeTimer.Stop();
				m_removeTimer.Start(15 * 60 * 1000);
			}
      
			return true;
		}

        protected int RemoveCallback(RegionTimer timer)
		{
			m_removeTimer.Stop();
			m_removeTimer = null;
			Delete();
			return 0;
        }

        /// <summary>
        /// Checks if a player can see the boat.
        /// </summary>
        public bool CanSeeBoat(GamePlayer player, GameBoat boat)
        {
            foreach (GamePlayer plr in boat.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player.Name == plr.Name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Must overide interact for Game Boat - Cannot allow boarders on player boat
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (this.BoatOwnerID != "")            
                return false;
              
 	        return base.Interact(player);
        }       
       
        /// <summary>
        /// Loads this boat from a boat table
        /// </summary>
        /// <param name="obj"></param>
        public override void LoadFromDatabase(DataObject obj)
        {
            if (!(obj is DBBoat))
                return;

            m_DBboat = (DBBoat)obj;
            boat_id = m_DBboat.ObjectId;
            boat_name = m_DBboat.BoatName;
            boat_maxspeedbase = m_DBboat.BoatMaxSpeedBase;
            boat_model = m_DBboat.BoatModel;
            boat_owner = m_DBboat.BoatOwner;
            switch (boat_model)
            {
                case 1616: m_type = 0; break;
                case 2648: m_type = 1; break;
                case 2646: m_type = 2; break;
                case 2647: m_type = 3; break;
                case 1615: m_type = 4; break;
                case 1595: m_type = 5; break;
                case 1612: m_type = 6; break;
                case 1613: m_type = 7; break;
                case 1614: m_type = 8; break;
            }
            theBoatDB = m_DBboat;
            base.LoadFromDatabase(obj);
        }

        public override void SaveIntoDatabase()
        {
            GameServer.Database.SaveObject(theBoatDB);
        }
    }
}