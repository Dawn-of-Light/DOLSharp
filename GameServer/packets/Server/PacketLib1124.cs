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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Reflection;

using DOL.Database;
using DOL.Language;
using DOL.AI.Brain;
using DOL.GS.Behaviour;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;

using log4net;
using DOL.GS.Geometry;

namespace DOL.GS.PacketHandler
{
	[PacketLib(1124, GameClient.eClientVersion.Version1124)]
	public class PacketLib1124 : PacketLib1123
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const ushort MAX_STORY_LENGTH = 1000;   // Via trial and error, 1.108 client.

		public PacketLib1124(GameClient client) : base(client)
		{
			icons = 1;
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
				pak.WriteByte(0);//model // patch 0072
				pak.WriteByte(0);//unk

				SendTCP(pak);
			}
		}

		public override void SendLivingEquipmentUpdate(GameLiving living)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.EquipmentUpdate)))
			{

				ICollection<InventoryItem> items = null;
				if (living.Inventory != null)
					items = living.Inventory.VisibleItems;

				pak.WriteShort((ushort)living.ObjectID);
				pak.WriteByte((byte)living.VisibleActiveWeaponSlots);
				pak.WriteByte((byte)living.CurrentSpeed); // new in 189b+, speed
				pak.WriteByte((byte)((living.IsCloakInvisible ? 0x01 : 0x00) | (living.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility
				pak.WriteByte((byte)((living.IsCloakHoodUp ? 0x01 : 0x00) | (int)living.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver

				if (items != null)
				{
					pak.WriteByte((byte)items.Count);
					foreach (InventoryItem item in items)
					{
						ushort model = (ushort)(item.Model & 0x1FFF);
						int slot = item.SlotPosition;
						//model = GetModifiedModel(model);
						int texture = item.Emblem != 0 ? item.Emblem : item.Color;
						if (item.SlotPosition == Slot.LEFTHAND || item.SlotPosition == Slot.CLOAK) // for test only cloack and shield
							slot = slot | ((texture & 0x010000) >> 9); // slot & 0x80 if new emblem
						pak.WriteByte((byte)slot);
						if ((texture & ~0xFF) != 0)
							model |= 0x8000;
						else if ((texture & 0xFF) != 0)
							model |= 0x4000;
						if (item.Effect != 0)
							model |= 0x2000;

						pak.WriteShort(model);

						if (item.SlotPosition > Slot.RANGED || item.SlotPosition < Slot.RIGHTHAND)
							pak.WriteByte((byte)item.Extension);

						if ((texture & ~0xFF) != 0)
							pak.WriteShort((ushort)texture);
						else if ((texture & 0xFF) != 0)
							pak.WriteByte((byte)texture);
						if (item.Effect != 0)
							pak.WriteShort((ushort)item.Effect); // effect changed to short
					}
				}
				else
				{
					pak.WriteByte(0x00);
				}
				SendTCP(pak);
			}
		}

		public override void SendLoginGranted(byte color)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginGranted)))
			{
				pak.WritePascalString(m_gameClient.Account.Name);
				pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
				pak.WriteByte(0x0C); //Server ID
				pak.WriteByte(color);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public override void SendNPCCreate(GameNPC npc)
		{

			if (m_gameClient.Player == null || npc.IsVisibleTo(m_gameClient.Player) == false)
				return;

			//Added by Suncheck - Mines are not shown to enemy players
			if (npc is GameMine)
			{
				if (GameServer.ServerRules.IsAllowedToAttack((npc as GameMine).Owner, m_gameClient.Player, true))
				{
					return;
				}
			}

			if (npc is GameMovingObject)
			{
				SendMovingObjectCreate(npc as GameMovingObject);
				return;
			}

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.NPCCreate)))
			{
				int speed = 0;
				ushort speedZ = 0;
				if (npc == null)
					return;
				if (!npc.IsAtTargetLocation)
				{
					speed = npc.CurrentSpeed;
					speedZ = (ushort)npc.ZSpeedFactor;
				}
				pak.WriteShort((ushort)npc.ObjectID);
				pak.WriteShort((ushort)(speed));
				pak.WriteShort(npc.Orientation.InHeading);
				pak.WriteShort((ushort)npc.Position.Z);
				pak.WriteInt((uint)npc.Position.X);
				pak.WriteInt((uint)npc.Position.Y);
				pak.WriteShort(speedZ);
				pak.WriteShort(npc.Model);
				pak.WriteByte(npc.Size);
				byte level = npc.GetDisplayLevel(m_gameClient.Player);
				if (npc.IsStatue)
				{
					level |= 0x80;
				}
				pak.WriteByte(level);

				byte flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);
				if ((npc.Flags & GameNPC.eFlags.GHOST) != 0) flags |= 0x01;
				if (npc.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
				if (npc.IsPeaceful) flags |= 0x10;
				if (npc.IsFlying) flags |= 0x20;
				if (npc.IsTorchLit) flags |= 0x04;

				pak.WriteByte(flags);
				pak.WriteByte(0x20); //TODO this is the default maxstick distance

				string add = "";
				byte flags2 = 0x00;
				IControlledBrain brain = npc.Brain as IControlledBrain;

				if (brain != null)
				{
					flags2 |= 0x80; // have Owner
				}

				if (npc.IsCannotTarget)
					if (m_gameClient.Account.PrivLevel > 1) add += "-DOR"; // indicates DOR flag for GMs
					else flags2 |= 0x01;
				if (npc.IsDontShowName)
					if (m_gameClient.Account.PrivLevel > 1) add += "-NON"; // indicates NON flag for GMs
					else flags2 |= 0x02;

				if (npc.IsStealthed)
					flags2 |= 0x04;

				eQuestIndicator questIndicator = npc.GetQuestIndicator(m_gameClient.Player);

				if (questIndicator == eQuestIndicator.Available)
					flags2 |= 0x08;//hex 8 - quest available
				if (questIndicator == eQuestIndicator.Finish)
					flags2 |= 0x10;//hex 16 - quest finish
								   //flags2 |= 0x20;//hex 32 - water mob?
								   //flags2 |= 0x40;//hex 64 - unknown
								   //flags2 |= 0x80;//hex 128 - has owner


				pak.WriteByte(flags2); // flags 2

				byte flags3 = 0x00;
				if (questIndicator == eQuestIndicator.Lesson)
					flags3 |= 0x01;
				if (questIndicator == eQuestIndicator.Lore)
					flags3 |= 0x02;
				if (questIndicator == eQuestIndicator.Pending) // new? patch 0031
					flags3 |= 0x20;
				pak.WriteByte(flags3); // new in 1.71 (region instance ID from StoC_0x20) OR flags 3?
				pak.WriteShort(0x00); // new in 1.71 unknown

				string name = npc.Name;
				string guildName = npc.GuildName;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, npc);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;

					if (!Util.IsEmpty(((DBLanguageNPC)translation).GuildName))
						guildName = ((DBLanguageNPC)translation).GuildName;
				}

				if (name.Length + add.Length + 2 > 47) // clients crash with too long names
					name = name.Substring(0, 47 - add.Length - 2);
				if (add.Length > 0)
					name = string.Format("[{0}]{1}", name, add);

				pak.WritePascalString(name);

				if (guildName.Length > 47)
					pak.WritePascalString(guildName.Substring(0, 47));
				else pak.WritePascalString(guildName);

				pak.WriteByte(0x00);
				SendTCP(pak);
			}
			/* removed, hack fix for client spamming requests for npcupdates/ creates
            if (_gameClient.Player.Client.Version < _gameClient.eClientVersion.Version1124) 
            {   // Update Cache
                _gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(npc.CurrentRegionID, (ushort)npc.ObjectID)] = 0;
            }*/
		}
		
		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			if (playerToCreate == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: playerToCreate == null");
				return;
			}

			if (m_gameClient.Player == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: _gameClient.Player == null");
				return;
			}

			Region playerRegion = playerToCreate.CurrentRegion;
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

			if (playerToCreate.IsVisibleTo(m_gameClient.Player) == false)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerCreate172)))
			{
				pak.WriteFloatLowEndian(playerToCreate.Position.X);
				pak.WriteFloatLowEndian(playerToCreate.Position.Y);
				pak.WriteFloatLowEndian(playerToCreate.Position.Z);
				pak.WriteShort((ushort)playerToCreate.Client.SessionID);
				pak.WriteShort((ushort)playerToCreate.ObjectID);
				pak.WriteShort(playerToCreate.Orientation.InHeading);
				pak.WriteShort(playerToCreate.Model);
				pak.WriteByte(playerToCreate.GetDisplayLevel(m_gameClient.Player));

				int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
				if (playerToCreate.IsAlive == false) flags |= 0x01;
				if (playerToCreate.IsUnderwater) flags |= 0x02; //swimming
				if (playerToCreate.IsStealthed) flags |= 0x10;
				if (playerToCreate.IsWireframe) flags |= 0x20;
				if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Vampiir) flags |= 0x40; //Vamp fly
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

			SendWarlockChamberEffect(playerToCreate);
		}

		public override void SendPlayerForgedPosition(GamePlayer player)
		{
			// doesn't work in 1.124+
			return;
		}

		public override void SendPlayerPositionAndObjectID()
		{
			if (m_gameClient.Player == null) return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PositionAndObjectID)))
			{
				pak.WriteFloatLowEndian(m_gameClient.Player.Position.X);
				pak.WriteFloatLowEndian(m_gameClient.Player.Position.Y);
				pak.WriteFloatLowEndian(m_gameClient.Player.Position.Z);
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
				pak.WriteShort(m_gameClient.Player.Orientation.InHeading);

				int flags = 0;
				Zone zone = m_gameClient.Player.CurrentZone;
				if (zone == null) return;

				if (m_gameClient.Player.CurrentZone.IsDivingEnabled)
					flags = 0x80 | (m_gameClient.Player.IsUnderwater ? 0x01 : 0x00);

				if (zone.IsDungeon)
				{
					pak.WriteShort((ushort)(zone.Offset.X / 0x2000));
					pak.WriteShort((ushort)(zone.Offset.Y / 0x2000));
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
					pak.WritePascalString(GameServer.Instance.Configuration.ServerName); //server name
				}
				else pak.WriteByte(0);
				pak.WriteByte(0); // rest is unknown for now
				pak.WriteByte(0); // flag?
				pak.WriteByte(0); // flag? these seemingly randomly have a value, most common is last 2 bytes are 34 08 
				pak.WriteByte(0); // flag?
				SendTCP(pak);
			}
		}

        public override void SendQuestListUpdate()
        {
            if (m_gameClient == null || m_gameClient.Player == null)
                return;

            SendTaskInfo();

            int questIndex = 1;
            lock (m_gameClient.Player.QuestList)
            {
                foreach (AbstractQuest quest in m_gameClient.Player.QuestList)
                {
                    SendQuestPacket((quest.Step == 0 || quest == null) ? null : quest, questIndex++);
                }
            }
        }

        protected override void SendQuestPacket(AbstractQuest q, int index)
		{
			if (q == null)
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					SendTCP(pak);
					return;
				}
			}
			else if (q is DQRewardQ)
            {
                DQRewardQ quest = q as DQRewardQ;
                using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
                {
                    pak.WriteByte((byte)index);
                    pak.WriteByte((byte)quest.Name.Length);
                    pak.WriteShort(0x00); // unknown
                    pak.WriteByte((byte)quest.Goals.Count);
                    pak.WriteByte((byte)quest.Level);
                    pak.WriteStringBytes(quest.Name);
                    pak.WritePascalString(quest.Description);
                    int goalindex = 0;
                    foreach (DQRQuestGoal goal in quest.Goals)
                    {
                        goalindex++;
                        String goalDesc = String.Format("{0}\r", goal.Description);
                        pak.WriteShortLowEndian((ushort)goalDesc.Length);
                        pak.WriteStringBytes(goalDesc);
                        // TODO commented out until i find out what these are used for
                        //pak.WriteShortLowEndian((ushort)goal.ZoneID2);
                        //pak.WriteShortLowEndian((ushort)goal.XOffset2);
                        //pak.WriteShortLowEndian((ushort)goal.YOffset2);
                        pak.Fill(0, 6);
                        pak.WriteShortLowEndian(0x00);  // unknown
                        pak.WriteShortLowEndian((ushort)goal.Type);
                        pak.WriteShortLowEndian(0x00);  // unknown
                        pak.WriteShortLowEndian((ushort)goal.ZoneID1);
                        pak.WriteShortLowEndian((ushort)goal.XOffset1);
                        pak.WriteShortLowEndian((ushort)goal.YOffset1);
                        pak.WriteByte((byte)((goal.IsAchieved) ? 0x01 : 0x00));
                        if (goal.QuestItem == null)
                        {
                            pak.WriteByte(0x00);
                        }
                        else
                        {
                            pak.WriteByte((byte)goalindex);
                            WriteTemplateData(pak, goal.QuestItem, 1);
                        }
                    }
                    SendTCP(pak);
                    return;
                }
            }
			else if (q is RewardQuest)
			{
				RewardQuest quest = q as RewardQuest;
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);
					pak.WriteByte((byte)quest.Name.Length);
					pak.WriteShort(0x00); // unknown
					pak.WriteByte((byte)quest.Goals.Count);
					pak.WriteByte((byte)quest.Level);
					pak.WriteStringBytes(quest.Name);
					pak.WritePascalString(quest.Description);
					int goalindex = 0;
					foreach (RewardQuest.QuestGoal goal in quest.Goals)
					{
						goalindex++;
						String goalDesc = String.Format("{0}\r", goal.Description);
						pak.WriteShortLowEndian((ushort)goalDesc.Length);
						pak.WriteStringBytes(goalDesc);
						pak.WriteShortLowEndian((ushort)goal.ZoneID2);
						pak.WriteShortLowEndian((ushort)goal.XOffset2);
						pak.WriteShortLowEndian((ushort)goal.YOffset2);
						pak.WriteShortLowEndian(0x00);  // unknown
						pak.WriteShortLowEndian((ushort)goal.Type);
						pak.WriteShortLowEndian(0x00);  // unknown
						pak.WriteShortLowEndian((ushort)goal.ZoneID1);
						pak.WriteShortLowEndian((ushort)goal.XOffset1);
						pak.WriteShortLowEndian((ushort)goal.YOffset1);
						pak.WriteByte((byte)((goal.IsAchieved) ? 0x01 : 0x00));
						if (goal.QuestItem == null)
						{
							pak.WriteByte(0x00);
						}
						else
						{
							pak.WriteByte((byte)goalindex);
							WriteTemplateData(pak, goal.QuestItem, 1);
						}
					}
					SendTCP(pak);
					return;
				}
			}
			else
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);

					string name = string.Format("{0} (Level {1})", q.Name, q.Level);
					string desc = string.Format("[Step #{0}]: {1}", q.Step, q.Description);
					if (name.Length > byte.MaxValue)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn(q.GetType().ToString() + ": name is too long for 1.68+ clients (" + name.Length + ") '" + name + "'");
						}
						name = name.Substring(0, byte.MaxValue);
					}
					if (desc.Length > byte.MaxValue)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn(q.GetType().ToString() + ": description is too long for 1.68+ clients (" + desc.Length + ") '" + desc + "'");
						}
						desc = desc.Substring(0, byte.MaxValue);
					}
					pak.WriteByte((byte)name.Length);
					pak.WriteShortLowEndian((ushort)desc.Length);
					pak.WriteByte(0); // Quest Zone ID ?
					pak.WriteByte(0);
					pak.WriteStringBytes(name); //Write Quest Name without trailing 0
					pak.WriteStringBytes(desc); //Write Quest Description without trailing 0                   

					SendTCP(pak);
				}
			}
		}

        public override void SendRegions(ushort regionId)
        {
            if (m_gameClient.Player != null)
            {
                if (!m_gameClient.Socket.Connected)
                    return;
                Region region = WorldMgr.GetRegion(regionId);
                if (region == null)
                    return;
                using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ClientRegion)))
                {
                    //				pak.WriteByte((byte)((region.Expansion + 1) << 4)); // Must be expansion
                    pak.WriteByte(0); // but this packet sended when client in old region. but this field must show expanstion for jump destanation region
                                      //Dinberg - trying to get instances to work.
                    pak.WriteByte((byte)region.Skin); // This was pak.WriteByte((byte)region.ID);
                    pak.Fill(0, 20);
                    pak.FillString(region.ServerPort.ToString(), 5);
                    pak.FillString(region.ServerPort.ToString(), 5);
                    string ip = region.ServerIP;
                    if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.") || ip.StartsWith("192.168."))
                        ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
                    pak.FillString(ip, 20);
                    SendTCP(pak);
                }
            }
        }

        public override void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon)
		{
			if (siegeWeapon == null)
				return;
			byte[] siegeID = new byte[siegeWeapon.ObjectID]; // test
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponAnimation)))
			{
				pak.WriteInt((uint)siegeWeapon.ObjectID);
                var aimCoordinate = siegeWeapon.AimCoordinate;
                pak.WriteInt((uint)aimCoordinate.X);
                pak.WriteInt((uint)aimCoordinate.Y);
                pak.WriteInt((uint)aimCoordinate.Z);
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.TimeUntilElapsed));
				pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
				switch ((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction)
				{
					case 0x01: //aiming
						{
							pak.WriteByte(siegeID[1]); // lowest value of siegeweapon.ObjectID
							pak.WriteShort((ushort)(siegeWeapon.TargetObject == null ? 0x0000 : siegeWeapon.TargetObject.ObjectID));
							break;
						}
					case 0x02: //arming
						{
							pak.WriteByte(0x5F);
							pak.WriteShort(0xD000);
							break;
						}
					case 0x03: // loading
						{
							pak.Fill(0, 3);
							break;
						}
				}
				//pak.WriteShort(0x5FD0);
				//pak.WriteByte(0x00);
				SendTCP(pak);

			}
		}

		public override void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon, int time)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponInterface)))
			{
				ushort flag = (ushort)((siegeWeapon.EnableToMove ? 1 : 0) | siegeWeapon.AmmoType << 8);
				pak.WriteShort(flag); //byte Ammo,  byte SiegeMoving(1/0)
				pak.WriteByte(0);
				pak.WriteByte(0); // Close interface(1/0)
				pak.WriteByte((byte)(time));//time x 100 eg 50 = 5000ms
				pak.WriteByte((byte)siegeWeapon.Ammo.Count); // external ammo count
				pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
				pak.WriteByte((byte)siegeWeapon.AmmoSlot);
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort(0); // SiegeHelperTimer ?
				pak.WriteShort(0); // SiegeTimer ?
				pak.WriteShort((ushort)siegeWeapon.ObjectID);

				string name = siegeWeapon.Name;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, siegeWeapon);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;
				}

				//pak.WritePascalString(name + " (" + siegeWeapon.CurrentState.ToString() + ")");
				foreach (InventoryItem item in siegeWeapon.Ammo)
				{
					if (item == null)
					{
						pak.Fill(0x00, 24);
						continue;
					}
					pak.WriteByte((byte)siegeWeapon.Ammo.IndexOf(item));
					pak.WriteShort(0); // unique objectID , can probably be 0
					pak.WriteByte((byte)item.Level);
					pak.WriteByte(0); // value1
					pak.WriteByte(0); //value2
					pak.WriteByte(0); // unknown
					pak.WriteByte((byte)item.Object_Type);
					pak.WriteByte(1); // unknown
					pak.WriteByte(0);//
					pak.WriteByte((byte)item.Count);
					//pak.WriteByte((byte)(item.Hand * 64));
					//pak.WriteByte((byte)((item.Type_Damage * 64) + item.Object_Type));
					//pak.WriteShort((ushort)item.Weight);
					pak.WriteByte(item.ConditionPercent); // % of con
					pak.WriteByte(item.DurabilityPercent); // % of dur
					pak.WriteByte((byte)item.Quality); // % of qua
					pak.WriteByte((byte)item.Bonus); // % bonus
					pak.WriteByte((byte)item.BonusLevel); // guessing
					pak.WriteShort((ushort)item.Model);
					pak.WriteByte((byte)item.Extension);
					pak.WriteShort(0); // unknown
					pak.WriteByte(4); // unknown flags?
					pak.WriteShort(0); // unknown
					if (item.Count > 1)
						pak.WritePascalString(item.Count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
				pak.WritePascalString(name + " (" + siegeWeapon.CurrentState.ToString() + ")");
				SendTCP(pak);
			}
		}

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
				pak.WriteByte((byte)m_gameClient.MinorRev[0]);

				// Build number
				pak.WriteByte(m_gameClient.MajorBuild); // last seen : 0x44 0x05
				pak.WriteByte(m_gameClient.MinorBuild);
				SendTCP(pak);
			}
		}

		protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GameLiving living)
		{
			pak.WriteByte((byte)(living.GroupIndex + 1)); // From 1 to 8
			if (living.CurrentRegion != m_gameClient.Player.CurrentRegion)
			{
				pak.WriteByte(0x00); // health
				pak.WriteByte(0x00); // mana
				pak.WriteByte(0x00); // endu
				pak.WriteByte(0x20); // player state (0x20 = another region)
				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | living.GroupIndex));
					pak.WriteByte(0);
				}
				return;
			}
			var player = living as GamePlayer;

			pak.WriteByte(player?.HealthPercentGroupWindow ?? living.HealthPercent);
			pak.WriteByte(living.ManaPercent);
			pak.WriteByte(living.EndurancePercent); // new in 1.69

			byte playerStatus = 0;
			if (!living.IsAlive)
				playerStatus |= 0x01;
			if (living.IsMezzed)
				playerStatus |= 0x02;
			if (living.IsDiseased)
				playerStatus |= 0x04;
			if (SpellHelper.FindEffectOnTarget(living, "DamageOverTime") != null)
				playerStatus |= 0x08;
			if (player?.Client?.ClientState == GameClient.eClientState.Linkdead)
				playerStatus |= 0x10;
			if (living.DebuffCategory[(int)eProperty.SpellRange] != 0 || living.DebuffCategory[(int)eProperty.ArcheryRange] != 0)
				playerStatus |= 0x40;
			pak.WriteByte(playerStatus);
			// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
			// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region, 0x40 - NS

			if (updateIcons)
			{
				pak.WriteByte((byte)(0x80 | living.GroupIndex));
				lock (living.EffectList)
				{
					byte i = 0;
					foreach (IGameEffect effect in living.EffectList)
						if (effect is GameSpellEffect)
							i++;
					pak.WriteByte(i);
					foreach (IGameEffect effect in living.EffectList)
						if (effect is GameSpellEffect)
						{
							pak.WriteByte(0);
							pak.WriteShort(effect.Icon);
						}
				}
			}
			WriteGroupMemberMapUpdate(pak, living);
		}

		protected override void WriteGroupMemberMapUpdate(GSTCPPacketOut pak, GameLiving living)
		{
			if (living.CurrentSpeed != 0)
			{
				Zone zone = living.CurrentZone;
                var zoneCoordinate = living.Coordinate - zone.Offset;
				if (zone == null)
					return;
				pak.WriteByte((byte)(0x40 | living.GroupIndex));
				//Dinberg - ZoneSkinID for group members aswell.
				pak.WriteShort(zone.ZoneSkinID);
				pak.WriteShort((ushort)(zoneCoordinate.X));
				pak.WriteShort((ushort)(zoneCoordinate.Y));
			}
		}

		protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 24); // +1 item.Effect changed to short
				return;
			}
			pak.WriteShort((ushort)0); // item uniqueID
			pak.WriteByte((byte)item.Level);

			int value1; // some object types use this field to display count
			int value2; // some object types use this field to display count
			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
					value1 = item.Count & 0xFF;
					value2 = (item.Count >> 8) & 0xFF;
					break;
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
					value1 = item.Count;
					value2 = item.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = item.DPS_AF;
					value2 = item.Count;
					break;
				case (int)eObjectType.Instrument:
					value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF);
					value2 = 0;
					break; // unused
				case (int)eObjectType.Shield:
					value1 = item.Type_Damage;
					value2 = item.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.HouseWallObject:
				case (int)eObjectType.HouseFloorObject:
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = item.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = item.DPS_AF;
					value2 = item.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (item.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(item.DPS_AF));
			else
				pak.WriteByte((byte)(item.Hand << 6));

			pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
			pak.WriteByte(0x00); //unk 1.112
			pak.WriteShort((ushort)item.Weight);
			pak.WriteByte(item.ConditionPercent); // % of con
			pak.WriteByte(item.DurabilityPercent); // % of dur
			pak.WriteByte((byte)item.Quality); // % of qua
			pak.WriteByte((byte)item.Bonus); // % bonus
			pak.WriteByte((byte)item.BonusLevel); // 1.109
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Extension);
			int flag = 0;
			int emblem = item.Emblem;
			int color = item.Color;
			if (emblem != 0)
			{
				pak.WriteShort((ushort)emblem);
				flag |= (emblem & 0x010000) >> 16; // = 1 for newGuildEmblem
			}
			else
			{
				pak.WriteShort((ushort)color);
			}
			//flag |= 0x01; // newGuildEmblem
			flag |= 0x02; // enable salvage button
			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_gameClient.Player.CraftingPrimarySkill);
			if (skill != null && skill is AdvancedCraftingSkill/* && ((AdvancedCraftingSkill)skill).IsAllowedToCombine(_gameClient.Player, item)*/)
				flag |= 0x04; // enable craft button
			ushort icon1 = 0;
			ushort icon2 = 0;
			string spell_name1 = "";
			string spell_name2 = "";
			if (item.Object_Type != (int)eObjectType.AlchemyTincture)
			{
				if (item.SpellID > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								flag |= 0x08;
								icon1 = spl.Icon;
								spell_name1 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
				if (item.SpellID1 > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID1)
							{
								flag |= 0x10;
								icon2 = spl.Icon;
								spell_name2 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
			}
			pak.WriteByte((byte)flag);
			if ((flag & 0x08) == 0x08)
			{
				pak.WriteShort((ushort)icon1);
				pak.WritePascalString(spell_name1);
			}
			if ((flag & 0x10) == 0x10)
			{
				pak.WriteShort((ushort)icon2);
				pak.WritePascalString(spell_name2);
			}
			pak.WriteShort((ushort)item.Effect); // item effect changed to short
			string name = item.Name;
			if (item.Count > 1)
				name = item.Count + " " + name;
			if (item.SellPrice > 0)
			{
				if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetString(item.SellPrice) + "]";
			}
			if (name == null) name = "";
			if (name.Length > 55)
				name = name.Substring(0, 55);
			pak.WritePascalString(name);
		}

		/// <summary>
		/// patch 0020
		/// </summary>       
		protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item, int questID)
		{
			if (item == null)
			{
				pak.Fill(0x00, 24); //item.Effect changed to short 1.119
				return;
			}

			pak.WriteShort((ushort)questID); // need to send an objectID for reward quest delve to work 1.115+
			pak.WriteByte((byte)item.Level);

			int value1; // some object types use this field to display count
			int value2; // some object types use this field to display count
			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
					value1 = item.Count & 0xFF;
					value2 = (item.Count >> 8) & 0xFF;
					break;
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
					value1 = item.Count;
					value2 = item.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = item.DPS_AF;
					value2 = item.Count;
					break;
				case (int)eObjectType.Instrument:
					value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF);
					value2 = 0;
					break; // unused
				case (int)eObjectType.Shield:
					value1 = item.Type_Damage;
					value2 = item.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.HouseWallObject:
				case (int)eObjectType.HouseFloorObject:
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = item.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = item.DPS_AF;
					value2 = item.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (item.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(item.DPS_AF));
			else
				pak.WriteByte((byte)(item.Hand << 6));

			pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
			pak.WriteByte(0x00); //unk 1.112
			pak.WriteShort((ushort)item.Weight);
			pak.WriteByte(item.ConditionPercent); // % of con
			pak.WriteByte(item.DurabilityPercent); // % of dur
			pak.WriteByte((byte)item.Quality); // % of qua
			pak.WriteByte((byte)item.Bonus); // % bonus
			pak.WriteByte((byte)item.BonusLevel); // 1.109
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Extension);
			int flag = 0;
			int emblem = item.Emblem;
			int color = item.Color;
			if (emblem != 0)
			{
				pak.WriteShort((ushort)emblem);
				flag |= (emblem & 0x010000) >> 16; // = 1 for newGuildEmblem
			}
			else
			{
				pak.WriteShort((ushort)color);
			}
			//flag |= 0x01; // newGuildEmblem
			flag |= 0x02; // enable salvage button

			// Enable craft button if the item can be modified and the player has alchemy or spellcrafting
			eCraftingSkill skill = CraftingMgr.GetCraftingSkill(item);
			switch (skill)
			{
				case eCraftingSkill.ArmorCrafting:
				case eCraftingSkill.Fletching:
				case eCraftingSkill.Tailoring:
				case eCraftingSkill.WeaponCrafting:
					if (m_gameClient.Player.CraftingSkills.ContainsKey(eCraftingSkill.Alchemy)
						|| m_gameClient.Player.CraftingSkills.ContainsKey(eCraftingSkill.SpellCrafting))
						flag |= 0x04; // enable craft button
					break;

				default:
					break;
			}

			ushort icon1 = 0;
			ushort icon2 = 0;
			string spell_name1 = "";
			string spell_name2 = "";
			if (item.Object_Type != (int)eObjectType.AlchemyTincture)
			{
				if (item.SpellID > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								flag |= 0x08;
								icon1 = spl.Icon;
								spell_name1 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
				if (item.SpellID1 > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID1)
							{
								flag |= 0x10;
								icon2 = spl.Icon;
								spell_name2 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
			}
			pak.WriteByte((byte)flag);
			if ((flag & 0x08) == 0x08)
			{
				pak.WriteShort((ushort)icon1);
				pak.WritePascalString(spell_name1);
			}
			if ((flag & 0x10) == 0x10)
			{
				pak.WriteShort((ushort)icon2);
				pak.WritePascalString(spell_name2);
			}
			pak.WriteShort((ushort)item.Effect); // changed to short 1.119
			string name = item.Name;
			if (item.Count > 1)
				name = item.Count + " " + name;
			if (item.SellPrice > 0)
			{
				if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetString(item.SellPrice) + "]";
			}
			if (name == null) name = "";
			if (name.Length > 55)
				name = name.Substring(0, 55);
			pak.WritePascalString(name);
		}

		protected override void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			if (template == null)
			{
				pak.Fill(0x00, 24); // 1.109 +1 byte
				return;
			}
			pak.WriteShort(0); // objectID
			pak.WriteByte((byte)template.Level);

			int value1;
			int value2;

			switch (template.Object_Type)
			{
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
				case (int)eObjectType.GenericItem:
					value1 = count; // Count
					value2 = template.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = template.DPS_AF;
					value2 = count; // Count
					break;
				case (int)eObjectType.Instrument:
					value1 = (template.DPS_AF == 2 ? 0 : template.DPS_AF);
					value2 = 0;
					break;
				case (int)eObjectType.Shield:
					value1 = template.Type_Damage;
					value2 = template.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = template.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = template.DPS_AF;
					value2 = template.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (template.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(template.DPS_AF));
			else
				pak.WriteByte((byte)(template.Hand << 6));
			pak.WriteByte((byte)((template.Type_Damage > 3
				? 0
				: template.Type_Damage << 6) | template.Object_Type));
			pak.Fill(0x00, 1); // 1.109, +1 byte, no clue what this is  - Tolakram
			pak.WriteShort((ushort)template.Weight);
			pak.WriteByte(template.BaseConditionPercent);
			pak.WriteByte(template.BaseDurabilityPercent);
			pak.WriteByte((byte)template.Quality);
			pak.WriteByte((byte)template.Bonus);
			pak.WriteByte((byte)template.BonusLevel); // 1.109
			pak.WriteShort((ushort)template.Model);
			pak.WriteByte((byte)template.Extension);
			if (template.Emblem != 0)
				pak.WriteShort((ushort)template.Emblem);
			else
				pak.WriteShort((ushort)template.Color);
			pak.WriteByte((byte)template.Flags);
			pak.WriteShort((ushort)template.Effect);
			if (count > 1)
				pak.WritePascalString(String.Format("{0} {1}", count, template.Name));
			else
				pak.WritePascalString(template.Name);
		}
	}
}
