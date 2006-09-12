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
using System.Reflection;
using System.Collections;
using System.Text;
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Keeps
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
			get { return KeepMgr.GetHeightFromLevel(this.Keep.Level); }
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

		public override byte Realm
		{
			get { return Keep.Realm; }
		}

		private Hashtable m_hookPoints;
		private byte m_oldHealthPercent;
		private bool m_isRaized;

		public Hashtable HookPoints
		{
			get { return m_hookPoints; }
			set { m_hookPoints = value; }
		}


		private Hashtable m_positions;
		public Hashtable Positions
		{
			get { return m_positions; }
		}
		#endregion

		public override int RealmPointsValue
		{
			get
			{
				foreach (GameKeepComponent component in this.Keep.KeepComponents)
				{
					if (component.IsAlive == false && component != this)
						return MaxHealth / 100;
				}
				return MaxHealth / 10;
			}
		}

		public override long ExperienceValue
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// do not regen
		/// </summary>
		public override void StartHealthRegeneration()
		{
		}

		/// <summary>
		/// constructor of component
		/// </summary>
		public GameKeepComponent()
		{
			m_hookPoints = new Hashtable(41);
			m_positions = new Hashtable();
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
			double angle = keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
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
			this.ID = component.ID;
			this.SaveInDB = false;
			this.IsRaized = false;
			LoadPositions();
			this.AddToWorld();
			FillPositions();
		}

		public void LoadPositions()
		{
			DBKeepPosition[] DBPositions = (DBKeepPosition[])GameServer.Database.SelectObjects(typeof(DBKeepPosition), "`ComponentSkin` = '" + this.Skin + "'");
			foreach (DBKeepPosition position in DBPositions)
			{
				DBKeepPosition[] list = this.Positions[position.TemplateID] as DBKeepPosition[];
				if (list == null)
				{
					list = new DBKeepPosition[4];
					this.Positions[position.TemplateID] = list;
				}
				//list.SetValue(position, position.Height);
				list[position.Height] = position;
			}
		}

		public void FillPositions()
		{
			foreach (DBKeepPosition[] positionGroup in this.Positions.Values)
			{
				for (int i = this.Height; i >= 0; i--)
				{
					DBKeepPosition position = positionGroup[i] as DBKeepPosition;
					if (position != null)
					{
						bool create = false;
						if (position.ClassType == "DOL.GS.Keeps.GameKeepBanner")
						{
							if (this.Keep.Banners[position.TemplateID] == null)
								create = true;
						}
						else if (position.ClassType == "DOL.GS.Keeps.GameKeepDoor")
						{
							if (this.Keep.Doors[position.TemplateID] == null)
								create = true;
						}
						else
						{
							if (this.Keep.Guards[position.TemplateID] == null)
								create = true;
						}
						if (create)
						{
							//create the object
							Assembly asm = Assembly.GetExecutingAssembly();
							IKeepItem obj = (IKeepItem)asm.CreateInstance(position.ClassType, true);
							obj.LoadFromPosition(position, this);
						}
						else
						{
							//move the object
							if (position.ClassType == "DOL.GS.Keeps.GameKeepBanner")
							{
								IKeepItem banner = this.Keep.Banners[position.TemplateID] as IKeepItem;
								if (banner.Position != position)
								{
									banner.MoveToPosition(position);
								}
							}
							else if (position.ClassType == "DOL.GS.Keeps.GameKeepDoor")
							{
								//doors dont move
							}
							else if (position.ClassType == "DOL.GS.Keeps.FrontierPortalStone")
							{ 
								//these dont move
							}
							else
							{
								IKeepItem guard = this.Keep.Guards[position.TemplateID] as IKeepItem;
								if (guard.Position != position)
								{
									guard.MoveToPosition(position);
								}
							}
						}
						break;
					}
				}
			}

			foreach (GameKeepGuard guard in this.Keep.Guards.Values)
			{
				if (guard.Position.Height > guard.Component.Height)
					guard.RemoveFromWorld();
				else
				{
					if (guard.Position.Height <= guard.Component.Height &&
						guard.ObjectState != GameObject.eObjectState.Active && !guard.IsRespawning)
						guard.AddToWorld();
				}
			}

			foreach (GameKeepBanner banner in this.Keep.Banners.Values)
			{
				if (banner.Position.Height > banner.Component.Height)
					banner.RemoveFromWorld();
				else
				{
					if (banner.Position.Height <= banner.Component.Height &&
						banner.ObjectState != GameObject.eObjectState.Active)
						banner.AddToWorld();
				}
			}
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
		public override void  TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			this.Keep.LastAttackedByEnemyTick = this.CurrentRegion.Time;
			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
			//only on hp change
			if (m_oldHealthPercent == this.HealthPercent) return;
			m_oldHealthPercent = this.HealthPercent;

			foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
				client.Out.SendKeepComponentDetailUpdate(this);
		}

		public override void Die(GameObject killer)
		{
			base.Die(killer);
			if (this.Keep is GameKeepTower && ServerProperties.Properties.CLIENT_VERSION_MIN >= (int)GameClient.eClientVersion.Version175)
			{
				if (IsRaized == false)
				{
					Notify(KeepEvent.TowerRaized, this.Keep, new KeepEventArgs(this.Keep, killer.Realm));
					PlayerMgr.BroadcastRaize(this.Keep, killer.Realm);
					IsRaized = true;

					foreach (GameKeepGuard guard in this.Keep.Guards.Values)
					{
						guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, this.Keep.Z, guard.Heading);
						guard.SpawnZ = this.Keep.Z;
					}
				}
			}

			foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
				client.Out.SendKeepComponentDetailUpdate(this);
		}

		public override void Delete()
		{
			StopHealthRegeneration();
			base.Delete();
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
				{
					if (this.m_isRaized)
					{
						if (this.HealthPercent >= 25)
						{
							IsRaized = false;
						}
						else return 0x02;
					}
					if (this.HealthPercent < 35) return 0x01;//broken
				}
				if (this.Keep is GameKeep)
					if (!IsAlive) return 0x01;//broken

				return 0x00;

			}
		}

		public void UpdateLevel()
		{
			if (IsRaized == false)
				this.Health = this.MaxHealth;
		}

		public bool IsRaized
		{
			get { return m_isRaized; }
			set
			{
				if (value == true)
				{
					StartRebuildTimer();
					Keep.ChangeLevel(1);
				}
				else
				{
					FillPositions();
				}
				m_isRaized = value;
			}
		}

		private RegionTimer m_rebuildTimer;
		private static int rebuildInterval = 30 * 60 * 1000;
		private void StartRebuildTimer()
		{
			m_rebuildTimer = new RegionTimer(CurrentRegion.TimeManager);
			m_rebuildTimer.Callback = new RegionTimerCallback(RebuildTimerCallback);
			m_rebuildTimer.Interval = rebuildInterval;
			m_rebuildTimer.Start(1);
		}

		public int RebuildTimerCallback(RegionTimer timer)
		{
			Repair((MaxHealth / 100) * 5);
			if (HealthPercent >= 25)
			{
				FillPositions();
				return 0;
			}
			return rebuildInterval;
		}

		public void Repair(int amount)
		{
			byte oldStatus = Status;
			Health += amount;
			m_oldHealthPercent = HealthPercent;
			if (oldStatus != Status)
			{
				foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
				{
					client.Out.SendKeepComponentDetailUpdate(this);
				}
			}
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
