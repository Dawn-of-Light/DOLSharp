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

using System.Collections;
using System.Collections.Generic;

using DOL.Database;
using DOL.GS.Keeps;

using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1115, GameClient.eClientVersion.Version1115)]
    public class PacketLib1115 : PacketLib1114
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.115
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1115(GameClient client)
            : base(client)
        {

        }

        /// <summary>
        /// Reply on Server Opening to Client Encryption Request
        /// Actually forces Encryption Off to work with Portal.
        /// </summary>
        public override void SendVersionAndCryptKey()
		{
			//Construct the new packet
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CryptKey)))
			{
				pak.WriteByte((byte)m_gameClient.ClientType);
				
				//Disable encryption (1110+ always encrypt)
				pak.WriteByte(0x00);

				// Reply with current version
				pak.WriteString((((int)m_gameClient.Version) / 1000) + "." + (((int)m_gameClient.Version) - 1000), 5);
				
				// revision, last seen (c) 0x63
				pak.WriteByte(0x00);
				
				// Build number
				pak.WriteByte(0x00); // last seen : 0x44 0x05
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public override void SendLoginGranted(byte color)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginGranted)))
			{
				pak.WritePascalString(m_gameClient.Account.Name);
				pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
				pak.WriteByte(0x29); //Server ID
				pak.WriteByte(0x07); // test value...
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		
		/// <summary>
		/// New Item Packet Data in v1.115
		/// </summary>
		/// <param name="pak"></param>
		/// <param name="item"></param>
		protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 23); // added one short in front of item data, v1.115
				return;
			}

			// Unknown
			pak.WriteShort((ushort)0);
			base.WriteItemData(pak, item);
		}
		
		/// <summary>
		/// New Item Packet Template Data in v1.115
		/// </summary>
		/// <param name="pak"></param>
		/// <param name="template"></param>
		/// <param name="count"></param>
		protected override void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			if (template == null)
			{
				pak.Fill(0x00, 23); // added one short in front of item data, v1.115
				return;
			}
			
			// Unknown
			pak.WriteShort((ushort)0);
			base.WriteTemplateData(pak, template, count);
		}
		
		/// <summary>
		/// Default Keep Model changed for 1.1115
		/// </summary>
		/// <param name="keep"></param>
		public override void SendKeepInfo(IGameKeep keep)
		{
			if (m_gameClient.Player == null) 
				return;
			
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepInfo)))
			{
				pak.WriteShort(keep.KeepID);
				pak.WriteShort(0);
				pak.WriteInt((uint)keep.X);
				pak.WriteInt((uint)keep.Y);
				pak.WriteShort((ushort)keep.Orientation.InDegrees);
				pak.WriteByte((byte)keep.Realm);
				pak.WriteByte((byte)keep.Level);//level
				pak.WriteShort(0);//unk
				pak.WriteByte(0xF7);//model
				pak.WriteByte(0);//unk
	
				SendTCP(pak);
			}
		}
        
        public override void SendWarmapUpdate(ICollection<IGameKeep> list)
		{
			if (m_gameClient.Player == null) return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarMapClaimedKeeps)))
			{
				int KeepCount = 0;
				int TowerCount = 0;
				foreach (AbstractGameKeep keep in list)
				{
					// New Agramon tower are counted as keep
					if (keep is GameKeep || (keep.KeepID & 0xFF) > 150)
						KeepCount++;
					else
						TowerCount++;
				}
				pak.WriteShort(0x0F00);
				pak.WriteByte((byte)KeepCount);
				pak.WriteByte((byte)TowerCount);
				byte albStr=0;
				byte hibStr=0;
				byte midStr=0;
				byte albMagic=0;
				byte hibMagic=0;
				byte midMagic=0;
				foreach (GameRelic relic in RelicMgr.getNFRelics())
				{
					switch (relic.OriginalRealm)
	                {
	                    case eRealm.Albion:
							if(relic.RelicType==eRelicType.Strength)
							{
								albStr=(byte)relic.Realm;
							}
							if(relic.RelicType==eRelicType.Magic)
							{
								albMagic=(byte)relic.Realm;
							}
							break;
						case eRealm.Hibernia:
							if(relic.RelicType==eRelicType.Strength)
							{
								hibStr=(byte)relic.Realm;
							}
							if(relic.RelicType==eRelicType.Magic)
							{
								hibMagic=(byte)relic.Realm;
							}
							break;
						case eRealm.Midgard:
							if(relic.RelicType==eRelicType.Strength)
							{
								midStr=(byte)relic.Realm;
							}
							if(relic.RelicType==eRelicType.Magic)
							{
								midMagic=(byte)relic.Realm;
							}
							break;
					}
				}
				pak.WriteByte(albStr);
				pak.WriteByte(midStr);
				pak.WriteByte(hibStr);
				pak.WriteByte(albMagic);
				pak.WriteByte(midMagic);
				pak.WriteByte(hibMagic);
				foreach (AbstractGameKeep keep in list)
	            {
	                int keepId = keep.KeepID;
	
	                /*if (ServerProperties.Properties.USE_NEW_KEEPS == 1 || ServerProperties.Properties.USE_NEW_KEEPS == 2)
	                {
	                    keepId -= 12;
	                    if ((keep.KeepID > 74 && keep.KeepID < 114) || (keep.KeepID > 330 && keep.KeepID < 370) || (keep.KeepID > 586 && keep.KeepID < 626) 
	                        || (keep.KeepID > 842 && keep.KeepID < 882) || (keep.KeepID > 1098 && keep.KeepID < 1138)) 
	                        keepId += 5;
	                }*/
	
	                int id = keepId & 0xFF;
					int tower = keep.KeepID >> 8;
					int map = (id / 25) - 1;
					
					int index = id - (map * 25 + 25);
					
					// Special Agramon zone
					if ((keep.KeepID & 0xFF) > 150)
						index = keep.KeepID - 151;
					
					int flag = (byte)keep.Realm; // 3 bits
					Guild guild = keep.Guild;
					string name = "";
					// map is now 0 indexed
					pak.WriteByte((byte)(((map - 1) << 6) | (index << 3) | tower));
					if (guild != null)
					{
						flag |= (byte)eRealmWarmapKeepFlags.Claimed;
						name = guild.Name;
					}
	
					//Teleport
					if (m_gameClient.Account.PrivLevel > (int)ePrivLevel.Player)
					{
						flag |= (byte)eRealmWarmapKeepFlags.Teleportable;
					}
					else
					{
						if (GameServer.KeepManager.FrontierRegionsList.Contains(m_gameClient.Player.CurrentRegionID) && m_gameClient.Player.Realm == keep.Realm)
						{
							GameKeep theKeep = keep as GameKeep;
							if (theKeep != null)
							{
								if (theKeep.OwnsAllTowers && !theKeep.InCombat)
								{
									flag |= (byte)eRealmWarmapKeepFlags.Teleportable;
								}
							}
						}
					}
	
					if (keep.InCombat)
					{
						flag |= (byte)eRealmWarmapKeepFlags.UnderSiege;
					}
	
					pak.WriteByte((byte)flag);
					pak.WritePascalString(name);
				}
				SendTCP(pak);
			}
		}

    }
}
