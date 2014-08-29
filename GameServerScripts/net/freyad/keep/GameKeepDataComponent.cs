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
using System.Collections.Generic;

using DOL.GS;
using DOL.GS.Keeps;

namespace net.freyad.keep
{
	/// <summary>
	/// GameKeepDataComponent is the default component used by KeepData Manager
	/// </summary>
	public class GameKeepDataComponent : GameLiving, IGameKeepComponent
	{
		private IGameKeep m_keep;
		
		/// <summary>
		/// Keep Owning this Component
		/// </summary>
		public IGameKeep Keep {
			get { return m_keep; }
			set { m_keep = value; }
		}
		
		private int m_iD;
		
		/// <summary>
		/// Component Index.
		/// </summary>
		public int ID {
			get { return m_iD; }
			set { m_iD = value; UpdateKeepComponent(); }
		}

		private int m_skin;
		
		/// <summary>
		/// Component Skin ID.
		/// </summary>
		public int Skin {
			get { return m_skin; }
			set { m_skin = value; UpdateKeepComponent(); }
		}

		private int m_componentX;
		
		/// <summary>
		/// Component X
		/// </summary>
		public int ComponentX {
			get { return m_componentX; }
			set { m_componentX = value; UpdateKeepComponent(); }
		}
		
		private int m_componentY;
		
		/// <summary>
		/// Component Y
		/// </summary>
		public int ComponentY {
			get { return m_componentY; }
			set { m_componentY = value; UpdateKeepComponent(); }
		}

		private int m_componentHeading;
		
		/// <summary>
		/// Component Heading
		/// </summary>
		public int ComponentHeading {
			get { return m_componentHeading; }
			set { m_componentHeading = value; UpdateKeepComponent(); }
		}
		
		private int m_height;
		
		/// <summary>
		/// Component Height
		/// </summary>
		public int Height {
			get { return m_height; }
			set { m_height = value; UpdateKeepComponent(); }
		}
		
		/// <summary>
		/// Component Status (Raized, Broken, Healthy ?)
		/// </summary>
		public byte Status {
			get
			{
				// Tower
				if (Keep.SentKeepComponents.Count == 1)
				{
					// Raized, Broken or Healthy
					return (byte)(IsRaized ? 0x02 : (HealthPercent < 35 ? 0x01 : 0x00));
				}
				
				// Keep Healthy or Broken
				return (byte)(IsAlive ? 0x00 : 0x01);
			}
		}
		
		/// <summary>
		/// Component Raized (only for Towers)
		/// </summary>
		public bool IsRaized {
			get 
			{
				if (Keep.SentKeepComponents.Count == 1)
					return !IsAlive;
				
				return false;
			}
		}
		
		private bool m_climbing;
		
		/// <summary>
		/// Component Climbing enable.
		/// </summary>
		public bool Climbing {
			get { return m_climbing; }
			set { m_climbing = value; }
		}
		
		/// <summary>
		/// Component Hookpoints Dictionary
		/// </summary>
		public IDictionary<int, DOL.GS.Keeps.GameKeepHookPoint> HookPoints
		{
			get { return new Dictionary<int, DOL.GS.Keeps.GameKeepHookPoint>(); }
		}
		
		/// <summary>
		/// Component computed Angle from Heanding.
		/// </summary>
		public double Angle
		{
			get { return Keep.Heading * ((Math.PI * 2) / 360); }
		}
		
		/// <summary>
		/// Component computed X real poisition
		/// </summary>
		public override int X
		{
			get
			{
				return (int)(Keep.X + ((sbyte)ComponentX * 148 * Math.Cos(Angle) + (sbyte)ComponentY * 148 * Math.Sin(Angle)));
			}
		}
		
		/// <summary>
		/// Component computed Y real position
		/// </summary>
		public override int Y
		{
			get
			{
				return (int)(Keep.Y - ((sbyte)ComponentY * 148 * Math.Cos(Angle) - (sbyte)ComponentX * 148 * Math.Sin(Angle)));
			}
		}

		/// <summary>
		/// Component computed real Heading.
		/// </summary>
		public override ushort Heading
		{
			get
			{
				//need check to be sure for heading
				double tmpAngle = ComponentHeading * 90 + Keep.Heading;
				if (tmpAngle > 360)
					tmpAngle -= 360;
				return (ushort)(tmpAngle / 0.08789);
			}
		}
		
		/// <summary>
		/// Component real Z (based on Keep Z)
		/// </summary>
		public override int Z
		{
			get { return Keep.Z; }
		}
		
		/// <summary>
		/// Changing Health should trigger Updates.
		/// </summary>
		public override int Health
		{
			set { base.Health = value; UpdateKeepComponent(); }
		}
		
		/// <summary>
		/// Do not start the standard Living Health Regeneration.
		/// </summary>
		public override void StartHealthRegeneration()
		{
		}
		
		/// <summary>
		/// Do not start the standard Living Endurance Regeneration.
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
		}
		
		/// <summary>
		/// Do not start the standard Living Power Regeneration.
		/// </summary>
		public override void StartPowerRegeneration()
		{
		}
		
		/// <summary>
		/// Called when component need Updates...
		/// </summary>
		public void UpdateKeepComponent()
		{
			foreach (GameClient cli in WorldMgr.GetClientsOfRegion(CurrentRegionID))
			{
				GameClient client = cli;
				if (client.Player != null && client.Player.ObjectState == eObjectState.Active)
					client.Player.Out.SendKeepComponentUpdate(Keep, false);
			}
			
			// Update this Living status.
			BroadcastUpdate();
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="component"></param>
		public GameKeepDataComponent(KeepDataComponent component, IGameKeepData keep)
			:base()
		{
			m_keep = keep;
			m_iD = component.Index;
			m_skin = component.Skin;
			m_componentX = component.X;
			m_componentY = component.Y;
			m_componentHeading = component.Heading;
						
			X = X;
			Y = Y;
			Z = Z;
			
			Heading = Heading;
			
			m_health = 0;
			
			CurrentRegion = keep.CurrentRegion;

		}
	}
}
