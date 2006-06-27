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
	public class GameGravestone : GameStaticItem
	{
		/// <summary>
		/// how much xp are stored in this gravestone
		/// </summary>
		private long m_xpValue;

		/// <summary>
		/// returns the xpvalue of this gravestone
		/// </summary>
		public long XPValue
		{
			get
			{
				return m_xpValue;
			}
			set
			{
				m_xpValue = value;
			}
		}

		/// <summary>
		/// Constructs a new empty Gravestone
		/// </summary>
		public GameGravestone(GamePlayer player, long xpValue):base()
		{
			//Objects should NOT be saved back to the DB
			//as standard! We want our mobs/items etc. at
			//the same startingspots when we restart!
			m_Name = player.Name+"'s Grave";
			m_Heading = (ushort)player.Heading;
			m_position = player.Position;
			Region = player.Region;
			m_Level = 0;

			if(player.Realm==(byte)eRealm.Albion)
				m_Model=145; //Albion Gravestone
			else if(player.Realm==(byte)eRealm.Midgard)
				m_Model=636; //Midgard Gravestone
			else if(player.Realm==(byte)eRealm.Hibernia)
				m_Model=637; //Hibernia Gravestone

			m_xpValue = xpValue;

			ObjectState = eObjectState.Inactive;
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
