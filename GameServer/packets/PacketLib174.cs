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
using DOL.GS.Effects;
using System.Collections;
using DOL.GS.Database;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.PlayerTitles;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(174, GameClient.eClientVersion.Version174)]
	public class PacketLib174 : PacketLib173
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.74 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib174(GameClient client):base(client)
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
			Zone playerZone = playerRegion.GetZone(playerToCreate.Position);
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
			if (playerToCreate.IsSwimming) flags |= 0x02; //swimming
			if (playerToCreate.IsStealthed)  flags |= 0x10;
			// 0x20 = wireframe
			pak.WriteByte((byte)flags);
			pak.WriteByte(0x00); // new in 1.74

			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			pak.WriteByte(0x00);
			pak.WritePascalString(playerToCreate.CurrentTitle.GetValue(playerToCreate)); // new in 1.74, NewTitle
			SendTCP(pak);

			if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
				SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
		}

		public override void SendPlayerPositionAndObjectID()
		{
			if (m_gameClient.Player==null) return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PositionAndObjectID));
			pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
			Point pos = m_gameClient.Player.Position;
			pak.WriteShort((ushort)pos.Z);
			pak.WriteInt((uint)pos.X);
			pak.WriteInt((uint)pos.Y);
			pak.WriteShort((ushort) m_gameClient.Player.Heading);

			int flags = 0;
			if (m_gameClient.Player.Region.IsDivingEnabled)
				flags = 0x80 | (m_gameClient.Player.IsDiving ? 0x01:0x00);
			pak.WriteByte((byte)(flags));

			pak.WriteByte(0x00);	//TODO Unknown
			Region reg = m_gameClient.Player.Region;
			if(reg == null) return;

			if(reg.IsDungeon)
			{
				Zone zone = reg.GetZone(pos);
				if (zone == null) return;
				pak.WriteShort((ushort)(zone.XOffset/0x2000));
				pak.WriteShort((ushort)(zone.YOffset/0x2000));
			}
			else
			{
				pak.WriteShort(0);
				pak.WriteShort(0);
			}
			pak.WriteShort((ushort) reg.RegionID);
			pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); // new in 1.74, same as in SendLoginGranted
			pak.WriteByte(0x00); //TODO: unknown, new in 1.74
			SendTCP(pak);
		}

		protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GamePlayer player)
		{
			base.WriteGroupMemberUpdate(pak, updateIcons, player);

			Region playerRegion = player.Region;
			if (playerRegion == m_gameClient.Player.Region && player.CurrentSpeed != 0)//todo : find a better way to detect when player change coord
			{
				Zone zone = playerRegion.GetZone(player.Position);
				if (zone == null)
					return;
				Point zonePos = zone.ToLocalPosition(player.Position);
				pak.WriteByte((byte)(0x40 | player.PlayerGroupIndex));
				pak.WriteShort((ushort)zone.ZoneID);
				pak.WriteShort((ushort)zonePos.X);
				pak.WriteShort((ushort)zonePos.Y);
			}
		}

		public override void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			if(pak.Length > 1000)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - first));
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)first);
				SendTCP(pak);
				pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
				pak.WriteByte(0x01); //subcode
				pak.WriteByte((byte)maxSkills); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)first);
				first = maxSkills;
			}
			maxSkills++;
		}

		public override void SendRegionChanged()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionChanged));
			pak.WriteShort((ushort) m_gameClient.Player.Region.RegionID);
			pak.WriteShort(0x00); // Zone ID?
			pak.WriteShort(0x00); // ?
			pak.WriteShort(0x01); // cause region change ?
			pak.WriteByte(0x0C); //Server ID
			pak.WriteByte(0); // ?
			pak.WriteShort(0xFFBF); // ?
			SendTCP(pak);
		}

		public override void SendSpellEffectAnimation(GameLiving spellCaster, GameLiving spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SpellEffectAnimation));
			pak.WriteShort((ushort) spellCaster.ObjectID);
			pak.WriteShort(spellid);
			pak.WriteShort((ushort) (spellTarget == null ? 0 : spellTarget.ObjectID));
			pak.WriteShort(boltTime);
			pak.WriteByte((byte) (noSound ? 1 : 0));
			pak.WriteByte(success);
			SendTCP(pak);
		}

		/*public override void SendWarmapBonuses()
		{
			if (m_gameClient.Player==null) return;
			int AlbTowers = 0;
			int MidTowers = 0;
			int HibTowers = 0;
			int AlbKeeps = 0;
			int MidKeeps = 0;
			int HibKeeps = 0;
			int OwnerDFTowers = 0;
			eRealm OwnerDF = eRealm.None;
			foreach(AbstractGameKeep keep in KeepMgr.getNFKeeps())
			{

				switch ((eRealm)keep.Realm)
				{
					case eRealm.Albion:
						if (keep is GameKeep)
							AlbKeeps++;
						else
							AlbTowers++;
						break;
					case eRealm.Midgard:
						if (keep is GameKeep)
							MidKeeps++;
						else
							MidTowers++;
						break;
					case eRealm.Hibernia:
						if (keep is GameKeep)
							HibKeeps++;
						else
							HibTowers++;
						break;
					default:
						break;
				}
			}
			if (AlbTowers > MidTowers && AlbTowers > HibTowers)
			{
				OwnerDF = eRealm.Albion;
				OwnerDFTowers = AlbTowers;
			}
			else if (MidTowers > AlbTowers && MidTowers > HibTowers)
			{
				OwnerDF = eRealm.Midgard;
				OwnerDFTowers = MidTowers;
			}
			else if (HibTowers > AlbTowers && HibTowers > MidTowers)
			{
				OwnerDF = eRealm.Hibernia;
				OwnerDFTowers = HibTowers;
			}
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.WarmapBonuses));
			int RealmKeeps = 0;
			int RealmTowers = 0;
			switch ((eRealm)m_gameClient.Player.Realm)
			{
				case eRealm.Albion:
					RealmKeeps = AlbKeeps;
					RealmTowers = AlbTowers;
					break;
				case eRealm.Midgard:
					RealmKeeps = MidKeeps;
					RealmTowers = MidTowers;
					break;
				case eRealm.Hibernia:
					RealmKeeps = HibKeeps;
					RealmTowers = HibTowers;
					break;
				default:
					break;
			}
			pak.WriteByte((byte)RealmKeeps);
			pak.WriteByte((byte)0); // Relics = CountPowerRelics << 4 | CountStrenghtRelics;
			pak.WriteByte((byte)OwnerDF); // Relics = CountPowerRelics << 4 | CountStrenghtRelics;
			pak.WriteByte((byte)RealmTowers);
			pak.WriteByte((byte)OwnerDFTowers);
			SendTCP(pak);
		}*/
	}
}
