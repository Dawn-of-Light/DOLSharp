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
using System.Reflection;

using DOL.GS;
using DOL.Database;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description of AdventureWingInstance. Based on Catacombs Instances Implementation
	/// Used to create personal or group dungeon instance so player can play in there without being annoyed
	/// will try to maintain owner ship of the instance so player or group can get back to it
	/// The Instance should destroy 5 min after nobody is inside or when all mobs are down
	/// Other manager are needed to prevent too much instance spawning (using JumpPoint for example)
	/// </summary>
	public class AdventureWingInstance : RegionInstance
	{
		/// <summary>
		/// Console Logger
		/// </summary>
		private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Group Owner
		/// </summary>
		private Group m_group;
		
		/// <summary>
		/// Group Owner
		/// </summary>
		public Group Group
        {
            get { return m_group; }
            set { m_group = value; }
        }

		/// <summary>
		/// Player Owner or Group Leader
		/// </summary>
		private GamePlayer m_player;
		
		/// <summary>
		/// Player Owner or Group Leader
		/// </summary>
		public GamePlayer Player
        {
            get { return m_player; }
            set { m_player = value; }
        }

				
		/// <summary>
		/// AdventureWingInstance Constructor
		/// </summary>
		public AdventureWingInstance(ushort ID, GameTimer.GameScheduler time, RegionData dat)
			: base(ID, time, dat)
		{
		}
				
		/// <summary>
		/// Update Instance Owner based on population still inside.
		/// </summary>
		public void UpdateInstanceOwner()
		{
			if(this.NumPlayers < 1) 
			{
				return;	
			}
			
			//there is still player inside search for owner or any change.
        	
        	//search vars
        	GamePlayer arbitraryplayer = null;
            Group arbitrarygroup = null;
            bool stillOwner = false;
            
            // group instance
            if(m_group != null) 
            {
            	// check if group still inside
				foreach(GamePlayer ininstance in this.PlayersInside) 
				{
					if(ininstance.Group != null && m_group == ininstance.Group) 
					{
						m_player = ininstance.Group.Leader;
						stillOwner = true;
						break;
					}
					else if(ininstance.Group != null) 
					{
						arbitrarygroup = ininstance.Group;
						arbitraryplayer = ininstance.Group.Leader;
					}
				}
            }
            
            // check if player owner is still inside
            if(!stillOwner && m_player != null)
            {
            	foreach(GamePlayer ininstance in this.PlayersInside) 
				{
					if(ininstance == m_player)
					{
						if(m_player.Group != null)
						{
							m_group = m_player.Group;
							m_player = m_player.Group.Leader;
						}
						stillOwner = true;
						break;
					}
            		else
            		{
            			arbitraryplayer = ininstance;
            		}
				}
            }
            
            // if no owner found
            if(!stillOwner) 
            {
            	//give ownership arbitrarly
            	if(arbitrarygroup != null)
            	{
            		m_group = arbitrarygroup;
            		m_player = arbitrarygroup.Leader;
            	}
            	else if(arbitraryplayer != null)
            	{
            		m_group = null;
					m_player = arbitraryplayer;
            	}
            	
            }
		}
		
		/// <summary>
		/// Override because mobs shouldn't respawn in Adventure wings.
		/// </summary>
		public override void LoadFromDatabase(Mob[] mobObjs, ref long mobCount, ref long merchantCount, ref long itemCount, ref long bindCount)
		{
			base.LoadFromDatabase(mobObjs, ref mobCount, ref merchantCount, ref itemCount, ref bindCount);
			
			// Set respawn to false
			foreach(GameNPC mob in GetMobsInsideInstance(true))
			{
				mob.RespawnInterval = -1;
			}
		}
		
		/// <summary>
		/// Player Leaving instance, start destroy timer if needed
		/// </summary>
		/// <param name="player">The Player exiting</param>
		public override void OnPlayerLeaveInstance(GamePlayer player)
        {
			// last player going out
			if(this.NumPlayers == 1) 
			{
				if(player.Group != null) 
				{
					this.m_group = player.Group;
					this.m_player = player.Group.Leader;
				}
				else {
					this.m_group = null;
					this.m_player = player;
				}
			}
			
            //Decrease the amount of players
            base.OnPlayerLeaveInstance(player);
            	
            if(this.NumPlayers > 0) 
            {
            	UpdateInstanceOwner();
            }
            else
            {
            	// check if there is still alive mobs
            	foreach (GameNPC mob in GetMobsInsideInstance(true))
            	{
            		// there is still something => standard autoclosure + break;
	            	log.Warn("Instance now empty, will destroy instance " + Description + ", ID: " + ID + ", type=" + GetType().ToString() + ". In " + ServerProperties.Properties.ADVENTUREWING_TIME_TO_DESTROY + " min.");
	            	this.BeginAutoClosureCountdown(ServerProperties.Properties.ADVENTUREWING_TIME_TO_DESTROY);
                	
                	return;
            	}
            	
        	    //destroy
        		log.Warn("Instance now empty, will destroy instance " + Description + ", ID: " + ID + ", type=" + GetType().ToString() + ". Now !");
            	WorldMgr.RemoveInstance(this);
            }
        }
	}
}
