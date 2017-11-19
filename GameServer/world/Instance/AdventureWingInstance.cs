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

using System.Linq;
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
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    /// <summary>
		/// Group Owner
		/// </summary>
		public Group Group { get; set; }

	    /// <summary>
		/// Player Owner or Group Leader
		/// </summary>
		public GamePlayer Player { get; set; }


	    /// <summary>
		/// AdventureWingInstance Constructor
		/// </summary>
		public AdventureWingInstance(ushort ID, GameTimer.TimeManager time, RegionData dat)
			: base(ID, time, dat)
		{
		}
				
		/// <summary>
		/// Update Instance Owner based on population still inside.
		/// </summary>
		public void UpdateInstanceOwner()
		{
			if(NumPlayers < 1) 
			{
				return;	
			}
			
			//there is still player inside search for owner or any change.
        	
        	//search vars
        	GamePlayer arbitraryplayer = null;
            Group arbitrarygroup = null;
            bool stillOwner = false;
            
            // group instance
            if(Group != null) 
            {
            	// check if group still inside
				foreach(GamePlayer ininstance in PlayersInside) 
				{
					if(ininstance.Group != null && Group == ininstance.Group) 
					{
						Player = ininstance.Group.Leader;
						stillOwner = true;
						break;
					}

                    if (ininstance.Group != null) 
					{
						arbitrarygroup = ininstance.Group;
						arbitraryplayer = ininstance.Group.Leader;
					}
				}
            }
            
            // check if player owner is still inside
            if(!stillOwner && Player != null)
            {
            	foreach(GamePlayer ininstance in PlayersInside) 
				{
					if(ininstance == Player)
					{
						if(Player.Group != null)
						{
							Group = Player.Group;
							Player = Player.Group.Leader;
						}
						stillOwner = true;
						break;
					}

            		arbitraryplayer = ininstance;
				}
            }
            
            // if no owner found
            if(!stillOwner) 
            {
            	//give ownership arbitrarly
            	if(arbitrarygroup != null)
            	{
            		Group = arbitrarygroup;
            		Player = arbitrarygroup.Leader;
            	}
            	else if(arbitraryplayer != null)
            	{
            		Group = null;
					Player = arbitraryplayer;
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
			if(NumPlayers == 1) 
			{
				if(player.Group != null) 
				{
                    Group = player.Group;
                    Player = player.Group.Leader;
				}
				else {
                    Group = null;
                    Player = player;
				}
			}
			
            //Decrease the amount of players
            base.OnPlayerLeaveInstance(player);
            	
            if(NumPlayers > 0) 
            {
            	UpdateInstanceOwner();
            }
            else
            {
            	// check if there is still alive mobs
            	if (GetMobsInsideInstance(true).Any())
            	{
            		// there is still something => standard autoclosure + break;
	            	log.Warn($"Instance now empty, will destroy instance {Description}, ID: {ID}, type={GetType()}. In {ServerProperties.Properties.ADVENTUREWING_TIME_TO_DESTROY} min.");
                    BeginAutoClosureCountdown(ServerProperties.Properties.ADVENTUREWING_TIME_TO_DESTROY);
                	
                	return;
            	}
            	
        	    //destroy
        		log.Warn($"Instance now empty, will destroy instance {Description}, ID: {ID}, type={GetType()}. Now !");
            	WorldMgr.RemoveInstance(this);
            }
        }
	}
}
