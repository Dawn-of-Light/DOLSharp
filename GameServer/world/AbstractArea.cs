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
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// AbstractArea extend this if you wish to implement e new custom area.
	/// For examples see Area.Cricle, Area.Square
	/// </summary>
	public abstract class AbstractArea : IArea
	{
		/// <summary>
		/// Variable holding whether or not players can broadcast in this area
		/// </summary>
		public bool CanBroadcast = false;

		protected bool m_checkLOS = false;
		/// <summary>
		/// Variable holding whether or not to check for LOS for spells in this area
		/// </summary>
		public bool CheckLOS
		{
			get { return m_checkLOS; }
			set { m_checkLOS = value; }
		}

		protected bool m_displayMessage = true;
		/// <summary>
		/// Display entered message
		/// </summary>
		public bool DisplayMessage
		{
			get { return m_displayMessage; }
			set { m_displayMessage = value; }
		}
		/// <summary>
		/// Constant holding max number of areas per zone, increase if more ares are needed,
		/// this will slightly increase memory usage on server
		/// </summary>		
		public const ushort MAX_AREAS_PER_ZONE = 50;

		/// <summary>
		/// The ID of the Area eg. 15 ( == index in Region.m_areas array)
		/// </summary>
		protected ushort m_ID;

		/// <summary>
		/// The description of the Area eg. "Camelot Hills"
		/// </summary>
		protected string m_Description;

		/// <summary>
		/// The area sound to play on enter/leave events
		/// </summary>
		protected byte m_sound;

		/// <summary>
		/// Constructs a new AbstractArea
		/// </summary>
		/// <param name="desc"></param>
		public AbstractArea(string desc)
		{
			m_Description = desc;
		}

		/// <summary>
		/// Returns the ID of this Area
		/// </summary>
		public ushort ID
		{
			get { return m_ID; }
			set { m_ID = value; }
		}

		/// <summary>
		/// Return the description of this Area
		/// </summary>
		public string Description
		{
			get { return m_Description; }
		}

		/// <summary>
		/// Gets or sets the area sound
		/// </summary>
		public byte Sound
		{
			get { return m_sound; }
			set { m_sound = value; }
		}

		#region Event handling

		public void UnRegisterPlayerEnter(DOLEventHandler callback)
		{
			GameEventMgr.RemoveHandler(this, AreaEvent.PlayerEnter, callback);
		}

		public void UnRegisterPlayerLeave(DOLEventHandler callback)
		{
			GameEventMgr.RemoveHandler(this, AreaEvent.PlayerLeave, callback);
		}

		public void RegisterPlayerEnter(DOLEventHandler callback)
		{
			GameEventMgr.AddHandler(this, AreaEvent.PlayerEnter, callback);
		}

		public void RegisterPlayerLeave(DOLEventHandler callback)
		{
			GameEventMgr.AddHandler(this, AreaEvent.PlayerLeave, callback);
		}
		#endregion

		/// <summary>
		/// Checks wether area intersects with given zone
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public abstract bool IsIntersectingZone(Zone zone);

		/// <summary>
		/// Checks wether given spot is within areas boundaries or not
		/// </summary>
		/// <param name="spot"></param>
		/// <returns></returns>
		public abstract bool IsContaining(IPoint3D spot);

		public abstract bool IsContaining(IPoint3D spot, bool checkZ);

		public abstract bool IsContaining(int x, int y, int z);

		public abstract bool IsContaining(int x, int y, int z, bool checkZ);

		/// <summary>
		/// Called whenever a player leaves the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerLeave(GamePlayer player)
		{
			if (m_displayMessage && Description != null && Description != "")
				player.Out.SendMessage("(Region) You have left " + Description + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			player.Notify(AreaEvent.PlayerLeave, this, new AreaEventArgs(this, player));
		}

		/// <summary>
		/// Called whenever a player enters the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerEnter(GamePlayer player)
		{
			if (m_displayMessage && Description != null && Description != "")
			{
				player.Out.SendMessage("(Region) You have entered " + Description + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(Description, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
			}
			if (Sound != 0)
			{
				player.Out.SendRegionEnterSound(Sound);
			}
			player.Notify(AreaEvent.PlayerEnter, this, new AreaEventArgs(this, player));
		}
	}
}
