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
using System.Reflection;
using DOL.Language;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;


namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&region",
		 new string[] { "&reg" },
		 ePrivLevel.Player,
		 "Broadcast something to other players in the same region",
		 "/region <message>")]
	public class RegionCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			const string BROAD_TICK = "Broad_Tick";

			if (args.Length < 2)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Broadcast.NoText"));
				return;
			}
			if (client.Player.IsMuted)
			{
				client.Player.Out.SendMessage("You have been muted. You cannot broadcast.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				return;
			}
			string message = string.Join(" ", args, 1, args.Length - 1);

			long BroadTick = client.Player.TempProperties.getProperty<long>(BROAD_TICK);
			if (BroadTick > 0 && BroadTick - client.Player.CurrentRegion.Time <= 0)
			{
				client.Player.TempProperties.removeProperty(BROAD_TICK);
			}
			long changeTime = client.Player.CurrentRegion.Time - BroadTick;
			if (changeTime < 800 && BroadTick > 0)
			{
				client.Player.Out.SendMessage("Slow down! Think before you say each word!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
				return;
			}
			Broadcast(client.Player, message);

			client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
		}

		private void Broadcast(GamePlayer player, string message)
		{
			foreach (GameClient c in WorldMgr.GetClientsOfRegion(player.CurrentRegionID))
			{
				if (GameServer.ServerRules.IsAllowedToUnderstand(c.Player, player))
				{
					c.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Region.Message", player.Name, message), eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
				}
			}

		}

	}
}


