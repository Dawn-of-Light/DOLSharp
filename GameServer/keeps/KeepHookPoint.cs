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
using System.Linq;

using DOL.Database;
using DOL.Events;
using DOL.GS.Geometry;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// A keepComponent
	/// </summary>
	public class GameKeepHookPoint
	{
		public GameKeepHookPoint(int id, GameKeepComponent component)
		{
			m_index = id;
			m_component = component;
			m_hookpointTimer = new HookpointTimer(this, this.Component);
            Position = component.Position;
		}

		public GameKeepHookPoint(DBKeepHookPoint dbhookPoint, GameKeepComponent component)
		{
			var angle = component.Keep.Orientation + component.RelativeOrientationToKeep;
            var offset = Vector.Create(dbhookPoint.X, -dbhookPoint.Y, dbhookPoint.Z).RotatedClockwise(angle);
            Position = component.Position + offset;
			Position = Position.With(component.Orientation + Angle.Heading(dbhookPoint.Heading));
			m_index = dbhookPoint.HookPointID;
			Component = component;
			m_hookpointTimer = new HookpointTimer(this, this.Component);
		}

        public Position Position { get; private init; }

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
			var item = DOLDB<DBKeepHookPointItem>.SelectObject(DB.Column(nameof(DBKeepHookPointItem.KeepID)).IsEqualTo(Component.Keep.KeepID).And(DB.Column(nameof(DBKeepHookPointItem.ComponentID)).IsEqualTo(Component.ID)).And(DB.Column(nameof(DBKeepHookPointItem.HookPointID)).IsEqualTo(ID)));
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