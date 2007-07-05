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
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// A keepComponent
	/// </summary>
	public class GameKeepHookPoint : IPoint3D
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

		public GameKeepHookPoint(DBKeepHookPoint dbhookPoint, GameKeepComponent component)
		{
			double angle = component.Keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
			switch (component.ComponentHeading)
			{
				case 0:
					X = (int)(component.X + Math.Cos(angle) * dbhookPoint.X + Math.Sin(angle) * dbhookPoint.Y);
					Y = (int)(component.Y - Math.Cos(angle) * dbhookPoint.Y + Math.Sin(angle) * dbhookPoint.X);
					break;
				case 1:
					X = (int)(component.X + Math.Cos(angle) * dbhookPoint.Y - Math.Sin(angle) * dbhookPoint.X);
					Y = (int)(component.Y + Math.Cos(angle) * dbhookPoint.X + Math.Sin(angle) * dbhookPoint.Y);
					break;
				case 2:
					X = (int)(component.X - Math.Cos(angle) * dbhookPoint.X - Math.Sin(angle) * dbhookPoint.Y);
					Y = (int)(component.Y + Math.Cos(angle) * dbhookPoint.Y - Math.Sin(angle) * dbhookPoint.X);
					break;
				case 3:
					X = (int)(component.X - Math.Cos(angle) * dbhookPoint.Y + Math.Sin(angle) * dbhookPoint.X);
					Y = (int)(component.Y - Math.Cos(angle) * dbhookPoint.X - Math.Sin(angle) * dbhookPoint.Y);
					break;
			}
			this.Z = component.Z + dbhookPoint.Z;
			this.Heading = (ushort)(component.Heading + dbhookPoint.Heading);
			this.m_index = dbhookPoint.HookPointID;
			this.Component = component;
			m_hookpointTimer = new HookpointTimer(this, this.Component);
		}

		#region properties

		// id <0x20=red,>0x20 - blue,>0x40 - green and yellow: 0x41(ballista),0x61(trebuchet),0x81(cauldron)
		private int m_index;
		public int ID
		{
			get { return m_index; }
			set { m_index = value; }
		}
		private HookpointTimer m_hookpointTimer;
		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
			set { m_component = value; }
		}

		public bool IsFree
		{
			get { return (m_object == null); }
		}

		private int m_z;
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

		private int m_x;
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		private int m_y;
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		private ushort m_heading;
		public ushort Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
		private GameLiving m_object;

		public GameLiving Object
		{
			get { return m_object; }
			set
			{
				m_object = value;
				if (value != null)
				{
					m_hookpointTimer.Start(1800000);//30*60*1000 = 30 min
					GameEventMgr.AddHandler(value, GameLivingEvent.Dying, new DOLEventHandler(ObjectDie));
				}
			}
		}

		#endregion

		private void ObjectDie(DOLEvent e, object sender, EventArgs arguments)
		{
			m_hookpointTimer.Start(300000);//5*60*1000 = 5 min
			GameEventMgr.RemoveHandler(m_object, GameLivingEvent.Dying, new DOLEventHandler(ObjectDie));
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