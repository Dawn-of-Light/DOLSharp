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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&announce2",
		(uint) ePrivLevel.GM,
		"Broadcast something to all players in the game",
		"/broadcast <message>")]
	public class AnnounceCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			string message = string.Join(" ", args, 1, args.Length - 1);
			foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
			{
				IList textList = new ArrayList();
				textList.Add("HOST Broadcasts: ");
				textList.Add("");
				textList.Add(message);
				clientz.Player.Out.SendCustomTextWindow("Broadcast", textList);

			}
			return 1;
		}
	}
}