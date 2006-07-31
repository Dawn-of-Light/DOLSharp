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
using DOL.Database;
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
			get { return m_keep; }
			set { m_keep = value; }
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
			get { return m_id; }
			set { m_id = value; }
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
			get { return m_height; }
			set { m_height = value; }
		}

		/// <summary>
		/// skin of keep component (wall, tower, ...)
		/// </summary>
		private int m_skin;
		public int Skin
		{
			get { return m_skin; }
			set { m_skin = value; }
		}

		public bool Climbing
		{
			get
			{
				if (m_skin == (int)eComponentSkin.Wall)
					return true;
				return false;
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
			get { return m_componentx; }
			set { m_componentx = value; }
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
			get { return m_componenty; }
			set { m_componenty = value; }
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
			get { return m_componentHeading; }
			set { m_componentHeading = value; }
		}

		/// <summary>
		/// Level of component
		/// </summary>
		public override byte Level
		{
			get
			{
				return (byte)(40 + Keep.Level);
			}
		}
		private Hashtable m_hookPoints;
		private byte m_oldHealthPercent;
		private bool m_rized;

		public Hashtable HookPoints
		{
			get { return m_hookPoints; }
			set { m_hookPoints = value; }
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
			GameEventMgr.AddHandler(this, GameObjectEvent.TakeDamage, new DOLEventHandler(SendComponentUpdate));
		}

		/// <summary>
		/// load component from db object
		/// </summary>
		public void LoadFromDatabase(DBKeepComponent component, AbstractGameKeep keep)
		{
			Region myregion = WorldMgr.GetRegion((ushort)keep.Region);
			if (myregion == null)
				return;
			this.Keep = keep;
			//this.DBKeepComponent = component;
			base.LoadFromDatabase(component);
			//this x and y is for get object in radius
			double angle = (keep.Heading * 0.017453292519943295769236907684886); // angle*2pi/360;
			X = (int)(keep.X + ((sbyte)component.X * 148 * Math.Cos(angle) + (sbyte)component.Y * 148 * Math.Sin(angle)));
			Y = (int)(keep.Y - ((sbyte)component.Y * 148 * Math.Cos(angle) - (sbyte)component.X * 148 * Math.Sin(angle)));
			this.Z = keep.Z;
			// and this one for packet sent
			this.ComponentX = component.X;
			this.ComponentY = component.Y;
			this.ComponentHeading = (ushort)component.Heading;
			//need check to be sure for heading
			angle = (component.Heading * 90 + keep.Heading);
			if (angle > 360) angle -= 360;
			this.Heading = (ushort)(angle / 0.08789);
			this.Name = keep.Name;
			this.Model = INVISIBLE_MODEL;
			this.Skin = component.Skin;
			this.Level = (byte)keep.Level;
			this.Health = MaxHealth;
			//			this.Health = component.Health;
			this.m_oldHealthPercent = this.HealthPercent;
			this.CurrentRegion = myregion;
			this.Height = component.Height;
			this.ID = component.ID;
			this.SaveInDB = false;
			this.Rized = false;
			this.AddToWorld();
		}

		/// <summary>
		/// save component in DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBKeepComponent obj = null;
			bool New = false;
			if (InternalID != null)
				obj = (DBKeepComponent)GameServer.Database.FindObjectByKey(typeof(DBKeepComponent), InternalID);
			if (obj == null)
			{
				obj = new DBKeepComponent();
				New = true;
			}
			obj.KeepID = Keep.KeepID;
			obj.Heading = ComponentHeading;
			obj.Health = Health;
			obj.X = this.ComponentX;
			obj.Y = this.ComponentY;
			obj.ID = this.ID;
			obj.Height = this.Height;
			obj.Skin = this.Skin;

			if (New)
			{
				GameServer.Database.AddNewObject(obj);
				InternalID = obj.ObjectId;
			}
			else
			{
				GameServer.Database.SaveObject(obj);
			}
			base.SaveIntoDatabase();
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

			foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
				player.Out.SendKeepComponentDetailUpdate(this);
		}

		public override void Die(GameObject killer)
		{
			base.Die(killer);
			foreach (GameClient cln in WorldMgr.GetClientsOfRegion(CurrentRegion.ID))
				cln.Out.SendKeepComponentDetailUpdate(this);
		}

		public override void Delete()
		{
			StopHealthRegeneration();
			base.Delete();
			GameEventMgr.RemoveHandler(this, GameObjectEvent.TakeDamage, new DOLEventHandler(SendComponentUpdate));
			DBKeepComponent obj = null;
			if (this.InternalID != null)
				obj = (DBKeepComponent)GameServer.Database.FindObjectByKey(typeof(DBKeepComponent), this.InternalID);
			if (obj != null)
				GameServer.Database.DeleteObject(obj);
			//todo find a packet to remove the keep
		}

		/// <summary>
		/// IComparable.CompareTo implementation.
		/// </summary>
		public int CompareTo(object obj)
		{
			if (obj is GameKeepComponent)
				return (this.ID - ((GameKeepComponent)obj).ID);
			else
				return 0;
		}

		public byte Status
		{
			get
			{
				if (this.Keep is GameKeepTower)
					if (this.HealthPercent < 25) return 0x01;//broken
				if (this.Keep is GameKeep)
					if (!Alive) return 0x01;//broken

				return 0x00;

			}
		}

		public void Update()
		{
			if (this.Keep.Level > 7)
				this.Height = 3;
			else if (this.Keep.Level > 4)
				this.Height = 2;
			else if (this.Keep.Level > 1)
				this.Height = 1;
			else
				this.Height = 0;

			this.Health = this.MaxHealth;
		}

		public bool Rized
		{
			get { return m_rized; }
			set { m_rized = value; }
		}

		public void Repair(int amount)
		{
			byte oldStatus = Status;
			Health += amount;
			m_oldHealthPercent = HealthPercent;
			if (oldStatus != Status)
				foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
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
