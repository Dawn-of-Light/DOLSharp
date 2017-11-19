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
using System.Collections.Generic;
using System.Reflection;
using DOL.Language;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.BonusesListRequest, "Handles player bonuses button clicks", eClientStatus.PlayerInGame)]
	public class PlayerBonusesListRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player == null)
				return;

			int code = packet.ReadByte();
			if (code != 0)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("bonuses button: code is other than zero ({0})", code);
			}

			new RegionTimerAction<GamePlayer>(client.Player,
			                                  p => p.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Bonuses")
			                                                                  , new List<string>(client.Player.GetBonuses()))).Start(1);
		}
	}
}
