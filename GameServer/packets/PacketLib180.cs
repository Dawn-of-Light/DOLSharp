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
#define NOENCRYPTION
using System;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(180, GameClient.eClientVersion.Version180)]
	public class PacketLib180 : PacketLib179
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.80 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib180(GameClient client):base(client)
		{
		}

		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerCreate172));
			Region playerRegion = playerToCreate.Region;
			if (playerRegion == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerRegion == null");
				return;
			}
			Zone playerZone = playerToCreate.CurrentZone;
			if (playerZone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerZone == null");
				return;
			}
			Point zonePos = playerZone.ToLocalPosition(playerToCreate.Position);
			pak.WriteShort((ushort)playerToCreate.Client.SessionID);
			pak.WriteShort((ushort)playerToCreate.ObjectID);
			pak.WriteShort((ushort)playerToCreate.Model);
			pak.WriteShort((ushort)zonePos.Z);
			pak.WriteShort((ushort)playerZone.ZoneID);
			pak.WriteShort((ushort)zonePos.X);
			pak.WriteShort((ushort)zonePos.Y);
			pak.WriteShort((ushort) playerToCreate.Heading);

			pak.WriteByte(playerToCreate.EyeSize); //1-4 = Eye Size / 5-8 = Nose Size
			pak.WriteByte(playerToCreate.LipSize); //1-4 = Ear size / 5-8 = Kin size
			pak.WriteByte(playerToCreate.MoodType); //1-4 = Ear size / 5-8 = Kin size
			pak.WriteByte(playerToCreate.EyeColor); //1-4 = Skin Color / 5-8 = Eye Color
			pak.WriteByte(playerToCreate.Level);
			pak.WriteByte(playerToCreate.HairColor); //Hair: 1-4 = Color / 5-8 = unknown
			pak.WriteByte(playerToCreate.FaceType); //1-4 = Unknown / 5-8 = Face type
			pak.WriteByte(playerToCreate.HairStyle); //1-4 = Unknown / 5-8 = Hair Style

			int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
			if (playerToCreate.Alive == false) flags |= 0x01;
			if (playerToCreate.IsUnderwater) flags |= 0x02; //swimming
			if (playerToCreate.IsStealthed)  flags |= 0x10;
			// 0x20 = wireframe
			pak.WriteByte((byte)flags);
			pak.WriteByte(0x00); // new in 1.74

			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			pak.WriteByte(0x00);
			pak.WritePascalString(playerToCreate.CurrentTitle.GetValue(playerToCreate)); // new in 1.74, NewTitle
//			bool ShowAllOnHorseWithBanners = (m_gameClient.Player.TempProperties.getObjectProperty(GamePlayer.DEBUG_MODE_PROPERTY, null) != null);
//			if(ShowAllOnHorseWithBanners)
//			{
//				pak.WriteByte((byte)playerToCreate.Realm); // horse id (from horsemap.csv);
//				pak.WriteShort(0); // horse boot ?
//				pak.WriteShort(0); // horse saddle ?
//			}
			pak.WriteByte(0); // trailing zero
			SendTCP(pak);

			if(GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
				SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server

//			if(ShowAllOnHorseWithBanners && playerToCreate.Guild != null)
//			{
//				pak = new GSTCPPacketOut(GetPacketCode(ePackets.VisualEffect));
//				pak.WriteShort((ushort)playerToCreate.ObjectID);
//				pak.WriteByte(0xC); // show Banner
//				pak.WriteByte((byte)0); // 0-enable, 1-disable
//				pak.WriteInt(playerToCreate.Guild.theGuildDB.Emblem);
//				SendTCP(pak);
//			}
		}
	}
}
