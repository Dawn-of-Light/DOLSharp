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

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.CryptKeyRequest, "Handles crypt key requests", eClientStatus.None)]
    public class CryptKeyRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
                // we don't handle Encryption for 1.115c
                // the rc4 secret can't be unencrypted from RSA.

                // register client type
                byte clientType = (byte)packet.ReadByte();
                client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
                client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);

                // if the DataSize is above 7 then the RC4 key is bundled
                // this is stored in case we find a way to handle encryption someday !
                if (packet.DataSize > 7)
                {
                    packet.Skip(6);
                    ushort length = packet.ReadShortLowEndian();
                    packet.Read(client.PacketProcessor.Encoding.SBox, 0, length);

                    // ((PacketEncoding168)client.PacketProcessor.Encoding).EncryptionState=PacketEncoding168.eEncryptionState.PseudoRC4Encrypted;
                }

                // Send the crypt key to the client
                client.Out.SendVersionAndCryptKey();
            
        }
    }
}