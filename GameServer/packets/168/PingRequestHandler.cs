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
using System.Text;

namespace DOL.GS.PacketHandler.v168
{
	/// <summary>
	/// Handles the ping packet
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x0B^168,"Sends the ping reply")]
	public class PingRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Called when the packet has been received
		/// </summary>
		/// <param name="client">Client that sent the packet</param>
		/// <param name="packet">Packet data</param>
		/// <returns>Non zero if function was successfull</returns>
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			packet.Skip(4); //Skip the first 4 bytes
			long pingDiff = (DateTime.Now.Ticks - client.PingTime)/1000;
			client.PingTime = DateTime.Now.Ticks;
			ulong timestamp = packet.ReadInt();
			if (client.Player != null)
			{
				if (client.Player.ObjectState == GameObject.eObjectState.Active)
				{
					long clientLocalTicksCount = client.Player.TempProperties.getLongProperty("clientLocalTicksCount", 0L);
					if (clientLocalTicksCount > 0)
					{
						long clientTickDiff = (long)timestamp - clientLocalTicksCount;
						if (clientTickDiff - pingDiff > 10000)
						{
							string temp = string.Format("difstamp:{0} difPing:{1} diff:{2} SHLevel:{3}", clientTickDiff, pingDiff, clientTickDiff - pingDiff, (clientTickDiff - pingDiff)/16000);
							StringBuilder builder = new StringBuilder();
							builder.Append("PING_SH_DETECT(");
							builder.Append(temp);
							builder.Append("): CharName=");
							builder.Append(client.Player.Name);
							builder.Append(" Account=");
							builder.Append(client.Account.Name);
							builder.Append(" IP=");
							builder.Append(client.TcpEndpoint);
							GameServer.Instance.LogCheatAction(builder.ToString());
							if (client.Account.PrivLevel == 3)
							{
//								TimeSpan timeUp = new TimeSpan((long) timestamp * 1000);
//								client.Out.SendMessage(string.Format("clientMachineUptime:{0}", timeUp) , eChatType.CT_Important, eChatLoc.CL_SystemWindow);
								client.Out.SendMessage(temp, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							}
//							else
//								client.Out.SendMessage(string.Format("SpeedHack({0}) detected", (clientTickDiff - pingDiff)/16000), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						}
					}
				}
				client.Player.TempProperties.setProperty("clientLocalTicksCount", (long)timestamp);
			}
			client.Out.SendPingReply(timestamp,packet.Sequence);
			return 1;
		}
	}
}
