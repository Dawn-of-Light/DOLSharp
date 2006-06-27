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
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// AbstractArea extend this if you wish to implement a new custom area.
	/// For examples see Area.Cricle, Area.Square
	/// </summary>
	public abstract class AbstractArea : IArea
	{		
		#region Declaration
		
		/// <summary>
		/// The ID of the Area eg. 15
		/// </summary>
		protected int m_id;		

		/// <summary>
		/// The description of the Area eg. "Camelot Hills"
		/// </summary>
		protected string m_description;

		/// <summary>
		/// The area sound to play on enter/leave events
		/// </summary>
		protected byte m_sound;	

		/// <summary>
		/// /broadcast enables in the area ?
		/// </summary>
		protected bool m_isBroadcastEnabled;
	
		/// <summary>
		/// the region id of the area
		/// </summary>
		protected int m_regionID;
	
		/// <summary>
		/// Returns the ID of this Area
		/// </summary>
		public int AreaID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Return the description of this Area
		/// </summary>
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		/// <summary>
		/// Gets or sets the area sound
		/// </summary>
		public byte Sound
		{
			get { return m_sound; }
			set { m_sound = value; }
		}

		/// <summary>
		/// Gets or sets if /broadcast is enable in the area
		/// </summary>
		public bool IsBroadcastEnabled
		{
			get { return m_isBroadcastEnabled; }
			set { m_isBroadcastEnabled = value; }
		}

		/// <summary>
		/// Gets or sets the regionID of the area
		/// </summary>
		public int RegionID
		{
			get { return m_regionID; }
			set { m_regionID = value; }
		}

		#endregion

		#region Function

		/// <summary>
		/// the list of all players inside this area (used for /broadcast)
		/// </summary>
		protected IList m_playersInArea = new ArrayList(1);

		/// <summary>
		/// Broadcast a message to all players inside the area
		/// </summary>
		public virtual void SendMessage(String message, eChatType type, eChatLoc loc)
		{
			lock(m_playersInArea.SyncRoot)
			{
				foreach(GamePlayer player in m_playersInArea)
				{
					player.Out.SendMessage(message, type, loc);
				}
			}
		}

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
		public abstract bool IsContaining(Point spot);

		/// <summary>
		/// Check if two areas are equals
		/// </summary>
		/// <param name="area">the area to compare with</param>
		/// <returns>true if equals</returns>
		public virtual bool IsEqual(AbstractArea area)
		{
			if(area == null)
				return false;

			if(area.RegionID != m_regionID)
				return false;

			return true;
		}

		/// <summary>
		/// Called whenever a player leaves the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerLeave(GamePlayer player)
		{
			m_playersInArea.Remove(player);
			if (Description!=null && Description!="")
				player.Out.SendMessage("(Region) You have left "+Description+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);

			player.Notify(AreaEvent.PlayerLeave,this,new AreaEventArgs(this,player));
		}

		/// <summary>
		/// Called whenever a player enters the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerEnter(GamePlayer player)
		{
			m_playersInArea.Add(player);
			if (Description!=null && Description!="")
			{
				player.Out.SendMessage("(Region) You have entered "+Description+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(Description, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
			}
			if (Sound != 0)
			{
				player.Out.SendRegionEnterSound(Sound);
			}
			player.Notify(AreaEvent.PlayerEnter, this, new AreaEventArgs(this, player));
		}
		#endregion
	}
}
