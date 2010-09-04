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
        private byte m_boatType = 0;
        protected DBBoat m_dbBoat;
        private string m_boatID;
        private string m_boatOwner;
        private string m_boatName;
        private ushort m_boatModel;
        private short m_boatMaxSpeedBase;

		private RegionTimer m_removeTimer = null;

        public GameBoat(byte type)
            : base()
        {
            m_boatType = type;
            base.OwnerID = BoatOwner;
        }

        public GameBoat()
			: base()
        {
            base.OwnerID = BoatOwner;
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
            get { return m_dbBoat; }
            set { m_dbBoat = value; }
        }

        public string BoatID
        {
            get
            {
                return m_boatID;
            }
            set
            {
                m_boatID = value;
            }
        }
        
        public override string Name
        {
            get
            {
                return m_boatName;
            }
            set
            {
                m_boatName = value;
            }
        }

        public override ushort Model
        {
            get
            {
                return m_boatModel;
            }
            set
            {
                m_boatModel = value;
            }
        }

        public override short MaxSpeedBase
        {
            get
            {
                return m_boatMaxSpeedBase;
            }
            set
            {
                m_boatMaxSpeedBase = value;
            }
        }

        public string BoatOwner
        {
            get
            {
                return m_boatOwner;
            }
            set
            {
                m_boatOwner = value;
            }
        }

        public override int MAX_PASSENGERS
        {
            get
            {
                switch (m_boatType)
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
				switch (m_boatType)
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
            if (this.OwnerID != "")            
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

            m_dbBoat = (DBBoat)obj;
            m_boatID = m_dbBoat.ObjectId;
            m_boatName = m_dbBoat.BoatName;
            m_boatMaxSpeedBase = m_dbBoat.BoatMaxSpeedBase;
            m_boatModel = m_dbBoat.BoatModel;
            m_boatOwner = m_dbBoat.BoatOwner;
            switch (m_boatModel)
            {
                case 1616: m_boatType = 0; break;
                case 2648: m_boatType = 1; break;
                case 2646: m_boatType = 2; break;
                case 2647: m_boatType = 3; break;
                case 1615: m_boatType = 4; break;
                case 1595: m_boatType = 5; break;
                case 1612: m_boatType = 6; break;
                case 1613: m_boatType = 7; break;
                case 1614: m_boatType = 8; break;
            }
            theBoatDB = m_dbBoat;
            base.LoadFromDatabase(obj);
        }

        public override void SaveIntoDatabase()
        {
            GameServer.Database.SaveObject(theBoatDB);
        }
    }
}