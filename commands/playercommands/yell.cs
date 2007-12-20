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

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&yell",
		new string[] {"&y"},
		ePrivLevel.Player,
		"Yell something to other players around you",
		"/yell <message>")]
	public class YellCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			const string YELL_TICK = "YELL_Tick";
			long YELLTick = client.Player.TempProperties.getLongProperty(YELL_TICK, 0);
			if (YELLTick > 0 && YELLTick - client.Player.CurrentRegion.Time <= 0)
			{
				client.Player.TempProperties.removeProperty(YELL_TICK);
			}

			long changeTime = client.Player.CurrentRegion.Time - YELLTick;
			if (changeTime < 500 && YELLTick > 0)
			{
				client.Player.Out.SendMessage("Slow down! Think before you say each word!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			if (args.Length < 2)
			{
				foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.YELL_DISTANCE))
				{
					if (player != client.Player)
					{
						ushort headingtemp = player.GetHeadingToTarget(client.Player);
						ushort headingtotarget = (ushort) (headingtemp - player.Heading);
						string direction = "";
						if (headingtotarget < 0)
							headingtotarget += 4096;
						if (headingtotarget >= 3840 || headingtotarget <= 256)
							direction = "South";
						else if (headingtotarget > 256 && headingtotarget < 768)
							direction = "South West";
						else if (headingtotarget >= 768 && headingtotarget <= 1280)
							direction = "West";
						else if (headingtotarget > 1280 && headingtotarget < 1792)
							direction = "North West";
						else if (headingtotarget >= 1792 && headingtotarget <= 2304)
							direction = "North";
						else if (headingtotarget > 2304 && headingtotarget < 2816)
							direction = "North East";
						else if (headingtotarget >= 2816 && headingtotarget <= 3328)
							direction = "East";
						else if (headingtotarget > 3328 && headingtotarget < 3840)
							direction = "South East";
						player.Out.SendMessage(client.Player.Name + " yells for help from the " + direction + "!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
					}
					else
						client.Out.SendMessage("You yell for help!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				}
				client.Player.TempProperties.setProperty(YELL_TICK, client.Player.CurrentRegion.Time);
				return 1;
			}

			string message = string.Join(" ", args, 1, args.Length - 1);
			client.Player.Yell(message);
			client.Player.TempProperties.setProperty(YELL_TICK, client.Player.CurrentRegion.Time);
			return 1;
		}
	}
}