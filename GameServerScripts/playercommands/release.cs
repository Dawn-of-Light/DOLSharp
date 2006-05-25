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

using DOL.Database;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&release",
		(uint) ePrivLevel.Player,
		"When you are dead you can '/release'. This will bring you back to your bindpoint!",
		"/release")]
	public class ReleaseCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length > 1)
			{
				if ((GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP
					|| GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
					&& args[1].ToLower() == "city")
				{
					client.Player.Release(GamePlayer.eReleaseType.City, false);
					return 1;
				}
			}
			client.Player.Release(GamePlayer.eReleaseType.Normal, false);
			return 1;
		}
	}
}