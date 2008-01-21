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
using DOL.GS;
using DOL.Database2;
using DOL.Events;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// A keepComponent
	/// </summary>
	public class GameKeepHookPoint : DatabaseObject, IPoint3D
	{
		public GameKeepHookPoint(int id, GameKeepComponent component)
		{
			m_index = id;
			m_component = component;
			m_hookpointTimer = new HookpointTimer(this, this.Component);
			this.X = component.X;
			this.Y = component.Y;
			this.Z = component.Z;
			this.Heading = component.Heading;
		}



		#region properties

		// id <0x20=red,>0x20 - blue,>0x40 - green and yellow: 0x41(ballista),0x61(trebuchet),0x81(cauldron)
		private int m_index;
		public int ID
		{
			get { return m_index; }
			set { m_index = value; }
		}
        [NonSerialized]
		private HookpointTimer m_hookpointTimer = null;
        [NonSerialized]
		private GameKeepComponent m_component = null;
        [NonSerialized]
		public GameKeepComponent Component
		{
			get {
                if (m_component == null)
                {
                    if (!DatabaseLayer.Instance.DatabaseObjects.TryGetValue(m_componentID, m_component))
                    {
                        throw new Exception("Could not find KeepComponent " + m_componentID + " for Hook Point " + ID);
                    }
                }
                return m_component; }
			set { m_component = value;
            m_componentID = value.ID;
        }
		}

        private UInt64 m_componentID;

		public bool IsFree
		{
			get { return (m_object == null); }
		}

		private int m_z;
		public int Z
		{
			get { return m_z; }
            set { m_z = value; Dirty = true; }
		}

		private int m_x;
		public int X
		{
			get { return m_x; }
            set { m_x = value; Dirty = true; }
		}

		private int m_y;
		public int Y
		{
			get { return m_y; }
            set { m_y = value; Dirty = true; }
		}

		private ushort m_heading;
		public ushort Heading
		{
			get { return m_heading; }
            set { m_heading = value; Dirty = true; }
		}
        private UInt64 m_objectid;
        [NonSerialized]
		private GameLiving m_object;

		public GameLiving Object
		{
			get {
                if (m_object == null)
                {
                    if (DatabaseLayer.Instance.DatabaseObjects.TryGetValue(m_objectid, m_object))
                    {
                        throw new Exception("Could not retrieve Object" + m_objectid + " for KeepHookPoint " + ID);
                    }
                }
                return m_object; 
                }
			set
			{
				m_object = value;
                m_objectid = value.ID;
				if (value != null)
				{
					m_hookpointTimer.Start(1800000);//30*60*1000 = 30 min
					GameEventMgr.AddHandler(value, GameLivingEvent.Dying, new DOLEventHandler(ObjectDie));
				}
                Dirty = true;
			}
		}

		#endregion

		private void ObjectDie(DOLEvent e, object sender, EventArgs arguments)
		{
			m_hookpointTimer.Start(300000);//5*60*1000 = 5 min
			GameEventMgr.RemoveHandler(m_object, GameLivingEvent.Dying, new DOLEventHandler(ObjectDie));
            //TODO: Fix this query here ..., we should keep a cross reference
			DBKeepHookPointItem item = (DBKeepHookPointItem)GameServer.Database.SelectObject(typeof(DBKeepHookPointItem), "KeepID = '" + Component.Keep.KeepID + "' AND ComponentID = '" + Component.ID + "' AND HookPointID = '" + ID + "'");
			if (item != null)
				GameServer.Database.DeleteObject(item);
		}
	}

	public class HookpointTimer : RegionAction
	{
		private GameKeepHookPoint m_hookpoint;

		public HookpointTimer(GameKeepHookPoint hookpoint, GameKeepComponent component)
			: base(component)
		{
			m_hookpoint = hookpoint;
		}

		protected override void OnTick()
		{
			if (m_hookpoint.Object is GameSiegeWeapon)
				(m_hookpoint.Object as GameSiegeWeapon).ReleaseControl();
			if (m_hookpoint.Object.ObjectState != GameObject.eObjectState.Deleted)
			{
				m_hookpoint.Object.Delete();
				this.Start(300000);//5*60*1000 = 5 min
			}
			else
				m_hookpoint.Object = null;
		}
	}
}