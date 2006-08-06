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

using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GameKeep is the keep in New Frontiere
	/// </summary>
	public class GameKeep : AbstractGameKeep
	{
		public GameKeep()
		{
		}
		/// <summary>
		/// time to upgrade from one level to another
		/// </summary>
		public static int[] UpgradeTime =
		{
			12*60*1000, // 1 12min
			12*60*1000, // 2 12min
			12*60*1000, // 3 12min
			12*60*1000, // 4 12min
			24*60*1000, // 5 24min
			60*60*1000, // 6 60min 1h
			120*60*1000, // 7 120min 2h
			240*60*1000, // 8 240min 4h
			480*60*1000, // 9 480min 8h
			960*60*1000, // 10 960min 16h
		};
		private ArrayList m_towers = new ArrayList(4);

		public override int CalculateTimeToUpgrade()
		{
			return UpgradeTime[this.Level - 1];
		}
		public override bool CheckForClaim(GamePlayer player)
		{
			if (player.PlayerGroup == null)
			{
				player.Out.SendMessage("You must be in a group to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.PlayerGroup.PlayerCount < ServerProperties.ClaimNumServerProperty.Value)
			{
				player.Out.SendMessage("You need " + ServerProperties.ClaimNumServerProperty.Value + " players to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return base.CheckForClaim(player);
		}
		public override int CalculRP()
		{
			return 1000 * DifficultyLevel;
		}

		public void AddTower(GameKeepTower tower)
		{
			if (!m_towers.Contains(tower))
				m_towers.Add(tower);
		}
		public ArrayList Towers
		{
			get { return m_towers; }
			set { m_towers = value; }
		}
	}
}
