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
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY object in the game world needs!
	/// </summary>
	public class GameGravestone : GameObject
	{
		/// <summary>
		/// how much xp are stored in this gravestone
		/// </summary>
		private long m_xpValue;

		/// <summary>
		/// get / set the xpvalue of this gravestone
		/// </summary>
		public long XPValue
		{
			get { return m_xpValue; }
			set { m_xpValue = value; }
		}

		/// <summary>
		/// store the db id of the linked player
		/// </summary>
		private int m_playerPersistantID;

		/// <summary>
		/// rget / set the db id of the linked player
		/// </summary>
		public int PlayerPersistantID
		{
			get { return m_playerPersistantID; }
			set { m_playerPersistantID = value; }
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendItemCreate(this);
		}

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendRemoveObject(this, eRemoveType.Disappear);
		}

		/// <summary>
		/// override the AddToWorld funtion to store the grave stone to the gravestone manadger manadger
		/// </summary>
		public override bool AddToWorld()
		{
			if(!base.AddToWorld()) return false;
			
			GravestoneMgr.AddGravestone(this);
			return true;
		}

		/// <summary>
		/// override the RemoveFromWorld funtion to remove the grave stone from the manadger
		/// </summary>
		public override bool RemoveFromWorld()
		{
			if(!base.RemoveFromWorld()) return false;
			
			GravestoneMgr.RemoveGravestone(this);
			return true;
		}
	}
}
