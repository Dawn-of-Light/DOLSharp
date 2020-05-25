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

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CryptKeyRequest, "Handles crypt key requests", eClientStatus.None)]
	public class CryptKeyRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			// for 1.115c+ The First client packet Changes.
			if (client.Version < GameClient.eClientVersion.Version1115)
			{
				int rc4 = packet.ReadByte();
				byte clientType = (byte)packet.ReadByte();
				client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
				client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);
				client.MajorBuild = (byte)packet.ReadByte();
				client.MinorBuild = (byte)packet.ReadByte();
				client.MinorRev = packet.ReadString(1);
				if (rc4 == 1)
				{
					//DOLConsole.Log("SBox=\n");
					//DOLConsole.LogDump(client.PacketProcessor.Encoding.SBox);
					packet.Read(((PacketEncoding168)client.PacketProcessor.Encoding).SBox,0,256);
					((PacketEncoding168)client.PacketProcessor.Encoding).EncryptionState = eEncryptionState.PseudoRC4Encrypted;
					//DOLConsole.WriteLine(client.Socket.RemoteEndPoint.ToString()+": SBox set!");
					//DOLConsole.Log("SBox=\n");
					//DOLConsole.LogDump(((PacketEncoding168)client.PacketProcessor.Encoding).SBox);
				}
				else
				{
				  //Send the crypt key to the client
					client.Out.SendVersionAndCryptKey();
				}
			}
			else
			{
				// if the DataSize is above 7 then the RC4 key is bundled
				if (packet.DataSize > 7)
				{
					var length = packet.ReadIntLowEndian();
					packet.Read(client.PacketProcessor.Encoding.SBox, 0, (int)length);
					// in 1.126 the second packet has random values for clientType, addons, etc
					return;
				}

				// register client type
				byte clientType = (byte)packet.ReadByte();
				client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
				client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);
				// the next 4 bytes are the game.dll version but not in string form
				// ie: 01 01 19 61 = 1.125a
				// this version is handled elsewhere before being sent here.
				packet.Skip(3); // skip the numbers in the version
				client.MinorRev = packet.ReadString(1); // get the minor revision letter // 1125d support
				packet.Skip(2); // build


				//Send the crypt key to the client
				client.Out.SendVersionAndCryptKey();
			}
		}
	}
}