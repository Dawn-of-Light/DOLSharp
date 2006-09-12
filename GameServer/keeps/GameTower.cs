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

using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// GameKeepTower is the tower in New frontiere link to keep
	/// </summary>
	public class GameKeepTower : AbstractGameKeep
	{
		public GameKeepTower()
		{
		}

		/// <summary>
		/// table of claim bounty point take from guild each cycle
		/// </summary>
		public new static readonly int[] ClaimBountyPointCost =
		{
			0,
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
		/// <summary>
		/// The towers keep
		/// </summary>
		public GameKeep Keep
		{
			set { m_keep = value; }
			get { return m_keep; }
		}

		/// <summary>
		/// The time for a tower to upgrade
		/// </summary>
		/// <returns></returns>
		public override int CalculateTimeToUpgrade()
		{
			return 12 * 60 * 1000;
		}

		/// <summary>
		/// The checks we need to run before we allow a player to claim
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckForClaim(GamePlayer player)
		{
			//let gms do everything
			if (player.Client.Account.PrivLevel > 1)
				return true;

			if (player.PlayerGroup == null)
			{
				player.Out.SendMessage("You must be in a group to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.PlayerGroup.PlayerCount < ServerProperties.Properties.CLAIM_NUM / 2)
			{
				player.Out.SendMessage("You need " + ServerProperties.Properties.CLAIM_NUM / 2 + " players to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Guild.BountyPoints < 50)
			{
				player.Out.SendMessage("Your guild must have at least 50 guild bounty points to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return base.CheckForClaim(player);
		}

		/// <summary>
		/// The RP reward for claiming based on difficulty level
		/// </summary>
		/// <returns></returns>
		public override int CalculRP()
		{
			return 100 * DifficultyLevel;
		}
	}
}
