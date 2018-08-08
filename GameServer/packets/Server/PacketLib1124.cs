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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(1124, GameClient.eClientVersion.Version1124)]
	public class PacketLib1124 : PacketLib1123
	{
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
		/// Constructs a new PacketLib for Client Version 1.124
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1124(GameClient client)
			: base(client)
		{
		}

        /// <summary>
		/// This is used to build a server side "Position Object"
		/// Usually Position Packet Should only be relayed
		/// The only purpose of this method is refreshing postion when there is Lag        
		/// </summary>		
		public override void SendPlayerForgedPosition(GamePlayer player)
        {
            using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.PlayerPosition)))
            {                
                int heading = 4096 + player.Heading;
                pak.WriteFloatLowEndian(player.X);
                pak.WriteFloatLowEndian(player.Y);
                pak.WriteFloatLowEndian(player.Z);
                pak.WriteFloatLowEndian(player.CurrentSpeed);
                pak.WriteInt(0); // needs to be Zaxis speed
                pak.WriteShort((ushort)player.Client.SessionID);
                pak.WriteShort(player.CurrentZone.ZoneSkinID);
                // Write Speed
                if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
                {
                    player.Heading = player.Steed.Heading;
                    pak.WriteShort(0x1800);
                }
                else
                {
                    short rSpeed = player.CurrentSpeed;

                    if (player.IsIncapacitated)
                    {
                        rSpeed = 0;
                    }

                    ushort content;

                    if (rSpeed < 0)
                    {
                        content = (ushort)((Math.Abs(rSpeed) > 511 ? 511 : Math.Abs(rSpeed)) + 0x200);
                    }
                    else
                    {
                        content = (ushort)(rSpeed > 511 ? 511 : rSpeed);
                    }

                    if (!player.IsAlive)
                    {
                        content += 5 << 10;
                    }
                    else
                    {
                        ushort state = 0;

                        if (player.IsSwimming)
                        {
                            state = 1;
                        }
                        if (player.IsClimbing)
                        {
                            state = 7;
                        }
                        if (player.IsSitting)
                        {
                            state = 4;
                        }
                        content += (ushort)(state << 10);
                    }

                    content += (ushort)(player.IsStrafing ? 1 << 13 : 0 << 13);

                    pak.WriteShort(content);
                }
                
                pak.WriteByte(0);
                pak.WriteByte(0);
                pak.WriteShort((ushort)heading);
                // Write Flags
                byte flagcontent = 0;

                if (player.IsDiving)
                {
                    flagcontent += 0x04;
                }

                if (player.IsWireframe)
                {
                    flagcontent += 0x01;
                }

                if (player.IsStealthed)
                {
                    flagcontent += 0x02;
                }

                if (player.IsTorchLighted)
                {
                    flagcontent += 0x80;
                }

                pak.WriteByte(flagcontent);
                pak.WriteByte((byte)(player.RPFlag ? 1 : 0));
                pak.WriteByte(0);
                pak.WriteByte(player.HealthPercent);
                pak.WriteByte(player.ManaPercent);
                pak.WriteByte(player.EndurancePercent);
                SendUDP(pak);
            }

            // Update Cache
            m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(player.CurrentRegionID, (ushort)player.ObjectID)] = GameTimer.GetTickCount();
        }

        public override void SendPlayerPositionAndObjectID()
        {
            if (m_gameClient.Player == null)
            {
                return;
            }

            using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PositionAndObjectID)))
            {
                pak.WriteFloatLowEndian(m_gameClient.Player.X);
                pak.WriteFloatLowEndian(m_gameClient.Player.Y);
                pak.WriteFloatLowEndian(m_gameClient.Player.Z);
                pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
                pak.WriteShort(m_gameClient.Player.Heading);

                int flags = 0;
                Zone zone = m_gameClient.Player.CurrentZone;
                if (zone == null)
                {
                    return;
                }
                if (m_gameClient.Player.CurrentZone.IsDivingEnabled)
                {
                    flags = 0x80 | (m_gameClient.Player.IsUnderwater ? 0x01 : 0x00);
                }
                if (zone.IsDungeon)
                {
                    pak.WriteShort((ushort)(zone.XOffset / 0x2000));
                    pak.WriteShort((ushort)(zone.YOffset / 0x2000));
                }
                else
                {
                    pak.WriteShort(0);
                    pak.WriteShort(0);
                }
                //Dinberg - Changing to allow instances...
                pak.WriteShort(m_gameClient.Player.CurrentRegion.Skin);
                pak.WriteByte((byte)(flags));
                if (m_gameClient.Player.CurrentRegion.IsHousing)
                {
                    pak.WriteByte((byte)GameServer.Instance.Configuration.ServerType); // guess?
                    pak.WritePascalString(GameServer.Instance.Configuration.ServerName);
                }
                else pak.WriteByte(0);
                pak.WriteByte(0); // the rest is unknown for now, but seems inconsequential if sent as 0. Needs more research
                pak.WriteByte(0); // flag?
                pak.WriteByte(0); // flag? these seemingly randomly have a value, most common is last 2 bytes are 34 08 . Cannot replicate it 100%
                pak.WriteByte(0); // flag?
                SendTCP(pak);
            }
        }
        
        public override void SendPlayerCreate(GamePlayer playerToCreate)
        {
            if (playerToCreate == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("SendPlayerCreate: playerToCreate == null");
                }
                return;
            }

            if (m_gameClient.Player == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("SendPlayerCreate: m_gameClient.Player == null");
                }
                return;
            }

            Region playerRegion = playerToCreate.CurrentRegion;

            if (playerRegion == null)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn("SendPlayerCreate: playerRegion == null");
                }
                return;
            }

            Zone playerZone = playerToCreate.CurrentZone;

            if (playerZone == null)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn("SendPlayerCreate: playerZone == null");
                }
                return;
            }

            if (playerToCreate.IsVisibleTo(m_gameClient.Player) == false)
            {
                return;
            }

            using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerCreate172)))
            {
                pak.WriteFloatLowEndian(playerToCreate.X);
                pak.WriteFloatLowEndian(playerToCreate.Y);
                pak.WriteFloatLowEndian(playerToCreate.Z);
                pak.WriteShort((ushort)playerToCreate.Client.SessionID);
                pak.WriteShort((ushort)playerToCreate.ObjectID);
                pak.WriteShort(playerToCreate.Heading);
                pak.WriteShort(playerToCreate.Model);
                pak.WriteByte(playerToCreate.GetDisplayLevel(m_gameClient.Player));

                int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
                if (playerToCreate.IsAlive == false)
                {
                    flags |= 0x01;
                }
                if (playerToCreate.IsUnderwater)
                {
                    flags |= 0x02; //swimming
                }
                if (playerToCreate.IsStealthed)
                {
                    flags |= 0x10;
                }
                if (playerToCreate.IsWireframe)
                {
                    flags |= 0x20;
                }
                if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Vampiir)
                {
                    flags |= 0x40; //Vamp fly
                }

                pak.WriteByte((byte)flags);

                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeSize)); //1-4 = Eye Size / 5-8 = Nose Size
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.LipSize)); //1-4 = Ear size / 5-8 = Kin size
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.MoodType)); //1-4 = Ear size / 5-8 = Kin size
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeColor)); //1-4 = Skin Color / 5-8 = Eye Color                
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairColor)); //Hair: 1-4 = Color / 5-8 = unknown
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.FaceType)); //1-4 = Unknown / 5-8 = Face type
                pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairStyle)); //1-4 = Unknown / 5-8 = Hair Style
                               
                pak.WriteByte(0x00); // new in 1.74
                pak.WriteByte(0x00); //unknown
                pak.WriteByte(0x00); //unknown
                pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
                pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
                pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
                //RR 12 / 13
                pak.WritePascalString(GameServer.ServerRules.GetPlayerPrefixName(m_gameClient.Player, playerToCreate));
                pak.WritePascalString(GameServer.ServerRules.GetPlayerTitle(m_gameClient.Player, playerToCreate)); // new in 1.74, NewTitle
                if (playerToCreate.IsOnHorse)
                {
                    pak.WriteByte(playerToCreate.ActiveHorse.ID);

                    if (playerToCreate.ActiveHorse.BardingColor == 0 && playerToCreate.ActiveHorse.Barding != 0 && playerToCreate.Guild != null)
                    {
                        int newGuildBitMask = (playerToCreate.Guild.Emblem & 0x010000) >> 9;
                        pak.WriteByte((byte)(playerToCreate.ActiveHorse.Barding | newGuildBitMask));
                        pak.WriteShortLowEndian((ushort)playerToCreate.Guild.Emblem);
                    }
                    else
                    {
                        pak.WriteByte(playerToCreate.ActiveHorse.Barding);
                        pak.WriteShort(playerToCreate.ActiveHorse.BardingColor);
                    }

                    pak.WriteByte(playerToCreate.ActiveHorse.Saddle);
                    pak.WriteByte(playerToCreate.ActiveHorse.SaddleColor);
                }
                else
                {
                    pak.WriteByte(0); // trailing zero
                }

                SendTCP(pak);
            }

            // Update Cache
            m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(playerToCreate.CurrentRegionID, (ushort)playerToCreate.ObjectID)] = GameTimer.GetTickCount();

            SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server

            if (playerToCreate.GuildBanner != null)
            {
                SendRvRGuildBanner(playerToCreate, true);
            }
        }        
    }
}
