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
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GameKeepTower is the tower in New frontiere link to keep
	/// </summary>
	public class GameKeepTower : AbstractGameKeep
	{
		public GameKeepTower()
		{
		}
        public override int KeepID
        {
            get
            {
                return Keep.KeepID;
            }
        }

	    public int TowerID
	    {
	        get { return (int)(KeepID/256); }
	    }
        public int LinkedKeepID
        {
            get { return KeepID - ((KeepID>>8) <<8); }
        }
	    /// <summary>
		/// table of claim bounty point take from guild each cycle
		/// </summary>
		public new static readonly int[] ClaimBountyPointCost =
		{
			5,
			5,
			5,
			5,
			10,
			20,
			30,
			40,
			50,
			100,
		};
		private GameKeep m_keep;

	    public override int CalculateTimeToUpgrade()
		{
			return 12*60*1000;
   		}
		public override bool CheckForClaim(GamePlayer player)
		{
			if (player.PlayerGroup == null)
			{
				player.Out.SendMessage("You must be a group of "+PlayerGroup.MAX_GROUP_SIZE/2+" members.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			if(player.PlayerGroup.PlayerCount < PlayerGroup.MAX_GROUP_SIZE/2)
			{
				player.Out.SendMessage("You must be a full group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			return base.CheckForClaim(player);
		}
		public override int CalculRP()
		{
            return 100 * DifficultyLevel();
		}

		public GameKeep Keep
		{
			set { m_keep=value; }
			get { return m_keep; }
		}
	}
}
