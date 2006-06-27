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
using System.Text;
using DOL.GS.Database;
using DOL.Events;

namespace DOL.GS
{
	//TODO : find all skin of keep door to load it from here
	/// <summary>
	/// A keepComponent
	/// </summary>
	public class GameKeepComponent : GameLiving, IComparable
	{
		private readonly ushort INVISIBLE_MODEL = 150;

		public enum eComponentSkin : byte
		{
			Gate = 0,
			WallInclined = 1,
			WallInclined2 = 2,
			WallAngle2 = 3,
			TowerAngle = 4,
			WallAngle = 5,
			WallAngleInternal = 6,
			TowerHalf = 7,
			WallHalfAngle = 8,
			Wall = 9,
			Keep = 10,
			Tower = 11,
			WallWithDoorLow = 12,
			WallWithDoorHigh = 13,
			BridgeHigh = 14,
			WallInclinedLow = 15,
			BridgeLow = 16,
			BridgeHightSolid = 17,
			BridgeHighWithHook = 18,
			GateFree = 19,
			BridgeHightWithHook2 = 20,
		}

		#region properties

		/// <summary>
		/// keep owner of component
		/// </summary>
		private AbstractGameKeep m_keep;
		/// <summary>
		/// keep owner of component
		/// </summary>
		public AbstractGameKeep Keep
		{
			get	{ return m_keep; }
			set	{ m_keep = value; }
		}

		/// <summary>
		/// id of keep component id keep
		/// </summary>
		private int m_id;
		/// <summary>
		/// id of keep component id keep
		/// </summary>
		public int ID
		{
			get	{ return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// height of keep grow with level
		/// </summary>
		private int m_height;
		/// <summary>
		/// height of keep grow with level
		/// </summary>
		public int Height
		{
			get	{ return m_height; }
			set	{ m_height = value; }
		}

		/// <summary>
		/// skin of keep component (wall, tower, ...)
		/// </summary>
		private int m_skin;
		public int Skin
		{
			get	{ return m_skin; }
			set	{ m_skin = value; }
		}

		public bool Climbing
		{
			get
			{
// TODO add climbable flag for component in database
//				if (m_skin == (int)eComponentSkin.Wall)
//					return true;
				return false;
			}
		}
        public override string Name
        {
            get
            {
                return Keep.Name;
            }
            set
            {
                Keep.Name = value;
            }
        }
        public override Region Region
        {
            get
            {
                return Keep.Region;
            }
            set
            {
                Keep.Region = value;
            }
        }
		/// <summary>
		/// relative X to keep
		/// </summary>
		private int m_componentx;
		/// <summary>
		/// relative X to keep
		/// </summary>
		public int ComponentX
		{
			get	{ return m_componentx; }
			set
			{
                double angle = (Keep.Heading * 0.017453292519943295769236907684886); // angle*2pi/360;
                m_componentx = value;
                m_position.m_x = (int)(Keep.Position.X + ((sbyte)m_componentx * 148 * Math.Cos(angle) + (sbyte)m_componenty * 148 * Math.Sin(angle)));
                m_position.m_y = (int)(Keep.Position.Y - ((sbyte)m_componenty * 148 * Math.Cos(angle) - (sbyte)m_componentx * 148 * Math.Sin(angle)));
			}
		}

		/// <summary>
		/// relative Y to keep
		/// </summary>
		private int m_componenty;
		/// <summary>
		/// relative Y to keep
		/// </summary>
		public int ComponentY
		{
			get	{ return m_componenty; }
			set
			{
                double angle = (Keep.Heading * 0.017453292519943295769236907684886); // angle*2pi/360;
			    m_componenty = value;
                m_position.m_x = (int)(Keep.Position.X + ((sbyte)m_componentx * 148 * Math.Cos(angle) + (sbyte)m_componenty * 148 * Math.Sin(angle)));
                m_position.m_y = (int)(Keep.Position.Y - ((sbyte)m_componenty * 148 * Math.Cos(angle) - (sbyte)m_componentx * 148 * Math.Sin(angle)));
			}
		}

		/// <summary>
		/// relative heading to keep ( 0, 1, 2, 3)
		/// </summary>
		private int m_componentHeading;
		/// <summary>
		/// relative heading to keep ( 0, 1, 2, 3)
		/// </summary>
		public int ComponentHeading
		{
			get	{ return m_componentHeading; }
			set
			{
			    m_componentHeading = value;
                //need check to be sure for heading
                double angle = (m_componentHeading * 90 + Keep.Heading);
                if (angle > 360) angle -= 360;
                this.Heading = (ushort)(angle / 0.08789);
			}
		}

		/// <summary>
		/// Level of component
		/// </summary>
		public override byte Level
		{
			get
			{
				return (byte)(40+ Keep.Level);
			}
		}
		private Hashtable m_hookPoints;
		private byte m_oldHealthPercent;
		private bool m_rized = false;

		public Hashtable HookPoints
		{
			get	{ return m_hookPoints; }
			set	{ m_hookPoints = value; }
		}
		#endregion

		/// <summary>
		/// do not regen
		/// </summary>
		public override void StartHealthRegeneration()
		{
		}

		/// <summary>
		/// do not gain xp
		/// </summary>
		public override void AddXPGainer(GameObject xpGainer, float damageAmount)
		{
		}

		/// <summary>
		/// constructor of component
		/// </summary>
		public GameKeepComponent()
		{
			m_hookPoints = new Hashtable(41);
			GameEventMgr.AddHandler(this,GameLivingEvent.TakeDamage, new DOLEventHandler(SendComponentUpdate));
            this.Model = INVISIBLE_MODEL;
		    
		}
        public GameKeepComponent(int componentID, int componentSkinID, int componentX, int componentY, int componentHead, int componentHeight, int componentHealth, int keepid) : this()
        { 
            
        }

		/// <summary>
		/// load component from db object
		/// </summary>
		public void Load()
		{
            m_position.Z = Keep.Position.Z;
			this.m_oldHealthPercent = this.HealthPercent;
			this.AddToWorld();
		}

		/// <summary>
		/// save component in DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
            GameServer.Database.SaveObject(this);
		}

		/// <summary>
		/// broadcast life of keep component
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void SendComponentUpdate(DOLEvent e, object sender, EventArgs args)
		{
			//only on hp change
			if (m_oldHealthPercent == this.HealthPercent) return;
			m_oldHealthPercent = this.HealthPercent;

			foreach(GameClient client in WorldMgr.GetClientsOfRegion((ushort)RegionId))
				client.Out.SendKeepComponentDetailUpdate(this);
		}

		public override void Delete()
		{
			StopHealthRegeneration();
			base.Delete();
			GameEventMgr.RemoveHandler(this,GameLivingEvent.TakeDamage, new DOLEventHandler(SendComponentUpdate));
			GameServer.Database.DeleteObject(this);
			//todo find a packet to remove the keep
		}

		/// <summary>
		/// IComparable.CompareTo implementation.
		/// </summary>
		public int CompareTo(object obj)
		{
			if(obj is GameKeepComponent)
				return (this.ID - ((GameKeepComponent)obj).ID);
			else
				return 0;
		}

		public byte Status
		{
			get
			{
				if(Keep is GameKeepTower)
					if (HealthPercent<25) return 0x01;//broken
				if(Keep is GameKeep)
					if (!Alive) return 0x01;//broken

				return 0x00;

			}
		}

		public void Update()
		{
			if (this.Keep.Level>7)
				this.Height = 3;
			else if (this.Keep.Level>4)
				this.Height = 2;
			else if (this.Keep.Level>1)
				this.Height = 1;
			else
				this.Height = 0;

			this.Health = this.MaxHealth;
		}

		public bool Rized
		{
			get{return m_rized;}
			set{m_rized=value;}
		}

		public void Repair(int amount)
		{
			byte oldStatus = Status;
			Health += amount;
			m_oldHealthPercent = HealthPercent;
			if (oldStatus != Status)
				foreach(GameClient client in WorldMgr.GetClientsOfRegion((ushort)RegionId))
					client.Out.SendKeepComponentDetailUpdate(this);
		}

		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" ComponentID=").Append(ID)
				.Append(" Skin=").Append(Skin)
				.Append(" Height=").Append(Height)
				.Append(" Heading=").Append(Heading)
				.Append(" nComponentX=").Append((sbyte)ComponentX)
				.Append(" ComponentY=").Append((sbyte)ComponentY)
				.Append(" ComponentHeading=").Append(ComponentHeading)
				.ToString();
		}
	}
}
