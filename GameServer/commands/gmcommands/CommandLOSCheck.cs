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

using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	/// <summary>
	/// CommandLOSCheck is used to test LoS check requests.
	/// </summary>
	[CmdAttribute(
		"&loscheck",
		ePrivLevel.GM,
		"Used to Test Line of Sight Check Requests",
		"/loscheck")]
	public class CommandLOSCheck : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Handle Command, No args handling.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public void OnCommand(GameClient client, string[] args)
		{
			client.Out.SendCheckLOS(client.Player, client.Player.TargetObject, new CheckLOSResponse(LOSCheckResponse));
		}
		
		/// <summary>
		/// Handle LoS Check Response
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		/// <param name="targetOID"></param>
		protected void LOSCheckResponse(GamePlayer player, ushort response, ushort targetOID)
		{
			ChatUtil.SendDebugMessage(player, string.Format("Los Check Response From {0}, resp : {1}, OID : {2}", player.Name, response, targetOID));
		}
	}
}
