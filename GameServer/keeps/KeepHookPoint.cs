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
using DOL.GS.Database;
using DOL.Events;
namespace DOL.GS
{
	/// <summary>
	/// A keepComponent
	/// </summary>
	public class GameKeepHookPoint : IWorldPosition
	{
		public GameKeepHookPoint(int id,GameKeepComponent component)
		{
			m_index = id;
			m_component = component;
			m_hookpointTimer = new HookpointTimer(this,this.Component);
			m_position = component.Position;
			Heading = (ushort)component.Heading;
		}

		public GameKeepHookPoint(DBKeepHookPoint dbhookPoint,GameKeepComponent component)
		{
			double angle = (component.Keep.Heading * 0.017453292519943295769236907684886); // angle*2pi/360;
			int x = -1;
			int y = -1;
			Point pos = component.Position;
			switch(component.ComponentHeading)
            {
				case 0:
					x = (int)(pos.X + Math.Cos(angle) * dbhookPoint.X + Math.Sin(angle) * dbhookPoint.Y);
					y = (int)(pos.Y - Math.Cos(angle) * dbhookPoint.Y + Math.Sin(angle) * dbhookPoint.X);
					break;
				case 1:
					x = (int)(pos.X + Math.Cos(angle) * dbhookPoint.Y - Math.Sin(angle) * dbhookPoint.X);
					y = (int)(pos.Y + Math.Cos(angle) * dbhookPoint.X + Math.Sin(angle) * dbhookPoint.Y);
					break;
				case 2:
					x = (int)(pos.X - Math.Cos(angle) * dbhookPoint.X - Math.Sin(angle) * dbhookPoint.Y);
					y = (int)(pos.Y + Math.Cos(angle) * dbhookPoint.Y - Math.Sin(angle) * dbhookPoint.X);
					break;
				case 3:
					x = (int)(pos.X - Math.Cos(angle) * dbhookPoint.Y + Math.Sin(angle) * dbhookPoint.X);
					y = (int)(pos.Y - Math.Cos(angle) * dbhookPoint.X - Math.Sin(angle) * dbhookPoint.Y);
					break;
            }
			int z = pos.Z + dbhookPoint.Z;
			m_position = new Point(x, y, z);
//			this.Heading = (ushort) dbhookPoint.Heading;
			Heading = (ushort)component.Heading;
			this.m_index = dbhookPoint.HookPointID;
			this.Component = component;
			m_hookpointTimer = new HookpointTimer(this, this.Component);
		}
		#region properties

		/// <summary>
		/// id < 0x20 = red , > 0x20 - blue, >0x40 - green and yellow: 0x41(ballista),0x61(trebuchet),0x81(cauldron)
		/// </summary>
		private int m_index;
		public int ID
		{
			get	{ return m_index;}
			set	{ m_index = value;}
		}
		private HookpointTimer m_hookpointTimer;
		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get	{ return m_component;}
			set	{ m_component = value;}
		}
		
		public bool IsFree
		{
			get { return (m_object == null); }
		}

		/// <summary>
		/// Gets the object's region.
		/// </summary>
		public Region Region
		{
			get { return m_component.Region; }
			set {  }
		}

		/// <summary>
		/// The hookpoint's position.
		/// </summary>
		private Point m_position;

		/// <summary>
		/// Gets or sets the object's position in the region.
		/// </summary>
		public Point Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		private ushort m_heading;
		public ushort Heading
		{
			get{return m_heading;}
			set{m_heading = value;}
		}
		private GameLiving m_object;
		
		public GameLiving Object
		{
			get{ return m_object; }
			set
			{
				m_object = value;
				if (value != null)
				{
					m_hookpointTimer.Start(1800000);//30*60*1000 = 30 min
					GameEventMgr.AddHandler(value,GameLivingEvent.Dying,new DOLEventHandler(ObjectDie));
				}
			}
		}

		#endregion

        private void ObjectDie(DOLEvent e, object sender, EventArgs arguments)
		{
			m_hookpointTimer.Start(300000);//5*60*1000 = 5 min
			GameEventMgr.RemoveHandler(m_object,GameLivingEvent.Dying,new DOLEventHandler(ObjectDie));
		}
	}
	public class HookpointTimer : RegionAction
	{
		private GameKeepHookPoint m_hookpoint;
		public HookpointTimer(GameKeepHookPoint hookpoint,GameKeepComponent component) : base(component)
		{
			m_hookpoint = hookpoint;
		}
		protected override void OnTick()
		{
			if(m_hookpoint.Object is GameSiegeWeapon)
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