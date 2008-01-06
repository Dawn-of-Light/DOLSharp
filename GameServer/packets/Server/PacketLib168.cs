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
#define  NOENCRYPTION

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(168, GameClient.eClientVersion.Version168)]
	public class PacketLib168 : AbstractPacketLib, IPacketLib
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.68 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib168(GameClient client)
			: base(client)
		{
		}

		//Packets
		public virtual void SendVersionAndCryptKey()
		{
			//Construct the new packet
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CryptKey));
			//Enable encryption
#if ! NOENCRYPTION
			pak.WriteByte(0x01);
#else
			pak.WriteByte(0x00);
#endif

			//if(is_si)
			pak.WriteByte(0x32);
			//else
			//	pak.WriteByte(0x31);
			int version = (int)m_gameClient.Version;
			pak.WriteByte((byte)(version / 100));
			pak.WriteByte((byte)((version % 100) / 10));
			//pak.WriteByte(build);
			pak.WriteByte(0x00);
#if ! NOENCRYPTION
			byte[] publicKey = new byte[500];
			UInt32 keyLen = CryptLib168.ExportRSAKey(publicKey, (UInt32) 500, false);
			pak.WriteShort((ushort) keyLen);
			pak.Write(publicKey, 0, (int) keyLen);
			//From now on we expect RSA!
			((PacketEncoding168) m_gameClient.PacketProcessor.Encoding).EncryptionState = PacketEncoding168.eEncryptionState.RSAEncrypted;
#endif
			SendTCP(pak);
		}

		public virtual void SendWarlockChamberEffect(GamePlayer player)
		{ }

		public virtual void SendLoginDenied(eLoginError et)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.LoginDenied));
			pak.WriteByte((byte)et); // Error Code
			/*
			if(is_si)
				pak.WriteByte(0x32);
			else
				pak.WriteByte(0x31);
			*/
			pak.WriteByte(0x01);
			int version = (int)m_gameClient.Version;
			pak.WriteByte((byte)(version / 100));
			pak.WriteByte((byte)((version % 100) / 10));
			//pak.WriteByte(build);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendLoginGranted()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.LoginGranted));
			/*
				if(is_si)
					pak.WriteByte(0x32);
				else
					pak.WriteByte(0x31);
				*/
			pak.WriteByte(0x01);
			int version = (int)m_gameClient.Version;
			pak.WriteByte((byte)(version / 100));
			pak.WriteByte((byte)((version % 100) / 10));
			//pak.WriteByte(build);
			pak.WriteByte(0x00);
			pak.WritePascalString(m_gameClient.Account.Name);
			pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
			pak.WriteByte(0x0C); //Server ID
			pak.WriteByte(GameServer.ServerRules.GetColorHandling(m_gameClient));
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendSessionID()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SessionID));
			pak.WriteShortLowEndian((ushort)m_gameClient.SessionID);
			SendTCP(pak);
		}

		public virtual void SendPingReply(ulong timestamp, ushort sequence)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PingReply));
			pak.WriteInt((uint)timestamp);
			pak.Fill(0x00, 4);
			pak.WriteShort((ushort)(sequence + 1));
			pak.Fill(0x00, 6);
			SendTCP(pak);
		}

		public virtual void SendRealm(eRealm realm)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Realm));

			pak.WriteByte((byte)realm);
			SendTCP(pak);
		}

		public virtual void SendCharacterOverview(eRealm realm)
		{
			int firstAccountSlot;
			switch (realm)
			{
				case eRealm.Albion:
					firstAccountSlot = 100;
					break;
				case eRealm.Midgard:
					firstAccountSlot = 200;
					break;
				case eRealm.Hibernia:
					firstAccountSlot = 300;
					break;
				default:
					throw new Exception("CharacterOverview requested for unknown realm " + realm);
			}

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterOverview));
			pak.FillString(m_gameClient.Account.Name, 24);
			InventoryItem[] items;
			Character[] characters = m_gameClient.Account.Characters;
			if (characters == null)
			{
				pak.Fill(0x0, 1848);
			}
			else
			{
				for (int i = firstAccountSlot; i < firstAccountSlot + 8; i++)
				{
					bool written = false;
					for (int j = 0; j < characters.Length && written == false; j++)
						if (characters[j].AccountSlot == i)
						{
							pak.FillString(characters[j].Name, 24);
							pak.Fill(0x0, 24); //0 String


							Region reg = WorldMgr.GetRegion((ushort)characters[j].Region);
							Zone zon = null;
							if (reg != null)
								zon = reg.GetZone(characters[j].Xpos, characters[j].Ypos);
							if (zon != null)
							{
								IList areas = zon.GetAreasOfSpot(characters[j].Xpos, characters[j].Ypos, characters[j].Zpos);
								string description = "";

								foreach (AbstractArea area in areas)
								{
									if (!area.DisplayMessage) continue;
									description = area.Description;
									break;
								}

								if (description == "")
									description = zon.Description;
								pak.FillString(description, 24);
							}
							else
								pak.Fill(0x0, 24); //No known location

							pak.FillString("", 24); //Class name

							pak.FillString(GamePlayer.RACENAMES[characters[j].Race], 24);
							pak.WriteByte((byte)characters[j].Level);
							pak.WriteByte((byte)characters[j].Class);
							pak.WriteByte((byte)characters[j].Realm);
							pak.WriteByte((byte)((((characters[j].Race & 0xF0) << 2) + (characters[j].Race & 0x0F)) | (characters[j].Gender << 4)));
							pak.WriteShortLowEndian((ushort)characters[j].CurrentModel);
							pak.WriteByte((byte)characters[j].Region);
							if (reg == null || (int)m_gameClient.ClientType > reg.Expansion)
								pak.WriteByte(0x00);
							else
								pak.WriteByte((byte)(reg.Expansion + 1)); //0x04-Cata zone, 0x05 - DR zone
							pak.WriteInt(0x0); // Internal database ID
							pak.WriteByte((byte)characters[j].Strength);
							pak.WriteByte((byte)characters[j].Dexterity);
							pak.WriteByte((byte)characters[j].Constitution);
							pak.WriteByte((byte)characters[j].Quickness);
							pak.WriteByte((byte)characters[j].Intelligence);
							pak.WriteByte((byte)characters[j].Piety);
							pak.WriteByte((byte)characters[j].Empathy);
							pak.WriteByte((byte)characters[j].Charisma);

							items = (InventoryItem[])GameServer.Database.SelectObjects(typeof(InventoryItem), "OwnerID = '" + GameServer.Database.Escape(characters[j].ObjectId) + "' AND SlotPosition >='10' AND SlotPosition <= '29'");
							int found = 0;
							//16 bytes: armor model
							for (int k = 0x15; k < 0x1D; k++)
							{
								found = 0;
								foreach (InventoryItem item in items)
								{
									if (item.SlotPosition == k && found == 0)
									{
										pak.WriteShortLowEndian((ushort)item.Model);
										found = 1;
									}
								}
								if (found == 0)
									pak.WriteShort(0x00);
							}
							//16 bytes: armor color
							for (int k = 0x15; k < 0x1D; k++)
							{
								int l;
								if (k == 0x15 + 3)
									//shield emblem
									l = (int)eInventorySlot.LeftHandWeapon;
								else
									l = k;

								found = 0;
								foreach (InventoryItem item in items)
								{
									if (item.SlotPosition == l && found == 0)
									{
										if (item.Emblem != 0)
											pak.WriteShortLowEndian((ushort)item.Emblem);
										else
											pak.WriteShortLowEndian((ushort)item.Color);
										found = 1;
									}
								}
								if (found == 0)
									pak.WriteShort(0x00);
							}
							//8 bytes: weapon model
							for (int k = 0x0A; k < 0x0E; k++)
							{
								found = 0;
								foreach (InventoryItem item in items)
								{
									if (item.SlotPosition == k && found == 0)
									{
										pak.WriteShortLowEndian((ushort)item.Model);
										found = 1;
									}
								}
								if (found == 0)
									pak.WriteShort(0x00);
							}
							if (characters[j].ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.TwoHanded)
							{
								pak.WriteByte(0x02);
								pak.WriteByte(0x02);
							}
							else if (characters[j].ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.Distance)
							{
								pak.WriteByte(0x03);
								pak.WriteByte(0x03);
							}
							else
							{
								byte righthand = 0xFF;
								byte lefthand = 0xFF;
								foreach (InventoryItem item in items)
								{
									if (item.SlotPosition == (int)eInventorySlot.RightHandWeapon)
										righthand = 0x00;
									if (item.SlotPosition == (int)eInventorySlot.LeftHandWeapon)
										lefthand = 0x01;
								}
								if (righthand == lefthand)
								{
									if (characters[j].ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.TwoHanded)
										righthand = lefthand = 0x02;
									else if (characters[j].ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.Distance)
										righthand = lefthand = 0x03;
								}
								pak.WriteByte(righthand);
								pak.WriteByte(lefthand);
							}
							if (reg == null || reg.Expansion != 1)
								pak.WriteByte(0x00);
							else
								pak.WriteByte(0x01); //0x01=char in SI zone, classic client can't "play"
							pak.WriteByte(0x00);
							//pak.Fill(0x00,2);
							written = true;
						}
					if (written == false)
						pak.Fill(0x0, 184);
				}
			}
			pak.Fill(0x0, 0x68); //Don't know why so many trailing 0's | Corillian: Cuz they're stupid like that ;)

			SendTCP(pak);
		}

		public virtual void SendDupNameCheckReply(string name, bool nameExists)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DupNameCheckReply));
			pak.FillString(name, 30);
			pak.FillString(m_gameClient.Account.Name, 20);
			pak.WriteByte((byte)(nameExists ? 0x1 : 0x0));
			pak.Fill(0x0, 3);
			SendTCP(pak);
		}

		public virtual void SendBadNameCheckReply(string name, bool bad)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.BadNameCheckReply));
			pak.FillString(name, 30);
			pak.FillString(m_gameClient.Account.Name, 20);
			pak.WriteByte((byte)(bad ? 0x0 : 0x1));
			pak.Fill(0x0, 3);
			SendTCP(pak);
		}

		public virtual void SendAttackMode(bool attackState)
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.AttackMode));
			pak.WriteByte((byte)(attackState ? 0x01 : 0x00));
			pak.Fill(0x00, 3);
			SendTCP(pak);
		}

		public virtual void SendCharCreateReply(string name)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterCreateReply));
			pak.FillString(name, 24);
			SendTCP(pak);
		}

		public virtual void SendCharStatsUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.StatsUpdate));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.STR));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.DEX));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.CON));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.QUI));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.INT));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.PIE));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.EMP));
			pak.WriteShort((ushort)m_gameClient.Player.GetBaseStat(eStat.CHR));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Strength) - m_gameClient.Player.GetBaseStat(eStat.STR)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Dexterity) - m_gameClient.Player.GetBaseStat(eStat.DEX)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Constitution) - m_gameClient.Player.GetBaseStat(eStat.CON)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Quickness) - m_gameClient.Player.GetBaseStat(eStat.QUI)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Intelligence) - m_gameClient.Player.GetBaseStat(eStat.INT)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Piety) - m_gameClient.Player.GetBaseStat(eStat.PIE)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Empathy) - m_gameClient.Player.GetBaseStat(eStat.EMP)));
			pak.WriteShort((ushort)(m_gameClient.Player.GetModified(eProperty.Charisma) - m_gameClient.Player.GetBaseStat(eStat.CHR)));
			pak.WriteShort((ushort)m_gameClient.Player.MaxHealth);
			pak.WriteByte(0x24); //TODO Unknown
			pak.WriteByte(0x25); //TODO Unknown
			SendTCP(pak);
		}

		public virtual void SendCharResistsUpdate()
		{
			return;
		}

		public virtual void SendRegions()
		{
			RegionEntry[] entries = WorldMgr.GetRegionList();

			if (entries == null)
				return;
			int index = 0;
			int num = 0;
			int count = entries.Length;
			while (entries != null && count > index)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ClientRegions));
				for (int i = 0; i < 4; i++)
				{
					while (index < count && (entries[index].id > byte.MaxValue || (int)m_gameClient.ClientType <= entries[index].expansion))
					{ //skip high ID regions added with catacombs
						index++;
					}

					if (index >= count)
					{ //If we have no more entries
						pak.Fill(0x0, 52);
					}
					else
					{
						pak.WriteByte((byte)(++num));
						pak.WriteByte((byte)entries[index].id);
						pak.FillString(entries[index].name, 20);
						pak.FillString(entries[index].fromPort, 5);
						pak.FillString(entries[index].toPort, 5);

						//Try to fix the region ip so UDP is enabled!
						string ip = entries[index].ip;
						if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
							ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
						pak.FillString(ip, 20);
						//						DOLConsole.WriteLine(string.Format(" ip={3}; fromPort={1}; toPort={2}; num={4}; id={0}; region name={5}", (byte)entries[index].id, entries[index].fromPort, entries[index].toPort, entries[index].ip, num, entries[index].name));
						index++;
					}
				}
				SendTCP(pak);
			}
		}

		public virtual void SendGameOpenReply()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.GameOpenReply));
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendPlayerPositionAndObjectID()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PositionAndObjectID));
			pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
			pak.WriteShort((ushort)m_gameClient.Player.Z);
			pak.WriteInt((uint)m_gameClient.Player.X);
			pak.WriteInt((uint)m_gameClient.Player.Y);
			pak.WriteShort(m_gameClient.Player.Heading);

			int flags = 0;
			if (m_gameClient.Player.CurrentRegion.DivingEnabled)
				flags = 0x80 | (m_gameClient.Player.IsUnderwater ? 0x01 : 0x00);
			pak.WriteByte((byte)(flags));

			pak.WriteByte(0x00); //TODO Unknown
			SendTCP(pak);
		}

		public virtual void SendPlayerJump(bool headingOnly)
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterJump));
			pak.WriteInt((uint)(headingOnly ? 0 : m_gameClient.Player.X));
			pak.WriteInt((uint)(headingOnly ? 0 : m_gameClient.Player.Y));
			pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
			pak.WriteShort((ushort)(headingOnly ? 0 : m_gameClient.Player.Z));
			pak.WriteShort(m_gameClient.Player.Heading);
			if (m_gameClient.Player.CurrentHouse == null)
			{
				pak.WriteShort(0);
			}
			else
			{
				pak.WriteShort((ushort)m_gameClient.Player.CurrentHouse.HouseNumber);
			}
			SendTCP(pak);
		}

		public virtual void SendPlayerInitFinished(byte mobs)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterInitFinished));
			pak.WriteByte(mobs);
			SendTCP(pak);
		}

		public virtual void SendUDPInitReply()
		{
			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.UDPInitReply));
			Region playerRegion = null;
			if (!m_gameClient.Socket.Connected)
				return;
			if (m_gameClient.Player != null && m_gameClient.Player.CurrentRegion != null)
				playerRegion = m_gameClient.Player.CurrentRegion;
			if (playerRegion == null)
				pak.Fill(0x0, 0x18);
			else
			{
				//Try to fix the region ip so UDP is enabled!
				string ip = playerRegion.ServerIP;
				if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
					ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
				pak.FillString(ip, 22);
				pak.WriteShort(playerRegion.ServerPort);
			}
			SendUDP(pak, true);
		}

		public virtual void SendTime()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Time));
			pak.WriteInt(WorldMgr.GetCurrentDayTime());
			pak.WriteInt(WorldMgr.GetDayIncrement());
			SendTCP(pak);
		}

		public virtual void SendMessage(string msg, eChatType type, eChatLoc loc)
		{
			if (m_gameClient.ClientState == GameClient.eClientState.CharScreen)
				return;

			// types not supported by 1.68+ clients
			switch (type)
			{
				case eChatType.CT_ScreenCenterSmaller:
				case eChatType.CT_ScreenCenter:
					return;
			}

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Message));
			pak.WriteShort((ushort)m_gameClient.SessionID);
			pak.WriteShort(0x00);
			pak.WriteByte((byte)type);
			pak.Fill(0x0, 3);

			String str;
			if (loc == eChatLoc.CL_ChatWindow)
				str = "@@";
			else if (loc == eChatLoc.CL_PopupWindow)
				str = "##";
			else
				str = "";

			str = String.Concat(str, msg);
			pak.WriteString(str);
			SendTCP(pak);
		}

		public virtual void SendPlayerCreate(GamePlayer playerToCreate)
		{
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

			if (playerToCreate.CurrentHouse != m_gameClient.Player.CurrentHouse)
				return;

			if (playerToCreate.CurrentRegion != m_gameClient.Player.CurrentRegion)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerCreate));
			pak.WriteShort((ushort)playerToCreate.Client.SessionID);
			pak.WriteShort((ushort)playerToCreate.ObjectID);
			//pak.WriteInt(playerToCreate.X);
			//pak.WriteInt(playerToCreate.Y);
			pak.WriteShort((ushort)playerRegion.GetXOffInZone(playerToCreate.X, playerToCreate.Y));
			pak.WriteShort((ushort)playerRegion.GetYOffInZone(playerToCreate.X, playerToCreate.Y));
			pak.WriteByte((byte)playerZone.ID);
			pak.WriteByte(0);
			pak.WriteShort((ushort)playerToCreate.Z);
			pak.WriteShort(playerToCreate.Heading);
			pak.WriteShort(playerToCreate.Model);
			//DOLConsole.WriteLine("send created player "+target.Player.Name+" to "+client.Player.Name+" alive="+target.Player.Alive);
			pak.WriteByte((byte)(playerToCreate.IsAlive ? 0x1 : 0x0));
			pak.WriteByte(0x00);
			pak.WriteByte(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate));
			pak.WriteByte(playerToCreate.Level);
			pak.WriteByte((byte)(playerToCreate.IsStealthed ? 0x01 : 0x00));
			pak.WriteByte(0x00); //Unused (??)
			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			pak.WriteByte(0x00); //Trialing 0 ... needed!
			SendTCP(pak);
			if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Warlock)
			{
				/*
				ChamberEffect ce = (ChamberEffect)playerToCreate.EffectList.GetOfType(typeof(ChamberEffect));
				if (ce != null)
				{
					ce.SendChamber(m_gameClient.Player);
				}
				 */
			}
			//if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
				SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
		}

		public virtual void SendObjectGuildID(GameObject obj, Guild guild)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectGuildID));
			pak.WriteShort((ushort)obj.ObjectID);
			if (guild == null)
				pak.WriteInt(0x00);
			else
			{
				pak.WriteShort(guild.ID);
				pak.WriteShort(guild.ID);
			}
			pak.WriteShort(0x00); //seems random, not used by the client
			SendTCP(pak);
		}

		public virtual void SendObjectUpdate(GameObject obj)
		{
			Zone z = obj.CurrentZone;
			if (z == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendObjectUpdate: obj zone == null:" + obj.InternalID);
				return;
			}

			if (m_gameClient.Player == null || obj.CurrentHouse != m_gameClient.Player.CurrentHouse || obj.CurrentRegion != m_gameClient.Player.CurrentRegion)
				return;
			ushort XOffsetInZone = (ushort)(obj.X - z.XOffset);
			ushort YOffsetInZone = (ushort)(obj.Y - z.YOffset);
			ushort XOffsetInTargetZone = 0;
			ushort YOffsetInTargetZone = 0;
			ushort ZOffsetInTargetZone = 0;

			int speed = 0;
			ushort targetZone = 0;
			byte flags = 0;
			int targetOID = 0;
			if (obj is GameNPC)
			{
				GameNPC npc = obj as GameNPC;
				flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);

				if (m_gameClient.Account.PrivLevel < 2)
				{
					// no name only if normal player
					if ((npc.Flags & (uint)GameNPC.eFlags.CANTTARGET) != 0)
						flags |= 0x01;
					if ((npc.Flags & (uint)GameNPC.eFlags.DONTSHOWNAME) != 0)
						flags |= 0x02;
				}
				if (npc.IsUnderwater)
					flags |= 0x10;
				if ((npc.Flags & (uint)GameNPC.eFlags.FLYING) != 0)
					flags |= 0x20;

				if (npc.IsMoving && !npc.IsOnTarget())
				{
					speed = npc.CurrentSpeed;
					if (npc.TargetX != 0 || npc.TargetY != 0 || npc.TargetZ != 0)
					{
						Zone tz = npc.CurrentRegion.GetZone(npc.TargetX, npc.TargetY);
						if (tz != null)
						{
							XOffsetInTargetZone = (ushort)(npc.TargetX - tz.XOffset);
							YOffsetInTargetZone = (ushort)(npc.TargetY - tz.YOffset);
							ZOffsetInTargetZone = (ushort)(npc.TargetZ);
							targetZone = tz.ID;
						}
					}

					if (speed > 0x07FF)
					{
						if (log.IsErrorEnabled)
							log.Error("Too high NPC speed. (" + speed + ")" + npc.Name);
						speed = 0x07FF;
					}
					else if (speed < 0)
					{
						if (log.IsErrorEnabled)
							log.Error("NPC speed can't be negative. (" + speed + ")" + npc.Name);
						speed = 0;
					}

					//flags|=0x10;
				}

				GameObject target = npc.TargetObject;
				if (npc.AttackState && target != null && target.ObjectState == GameObject.eObjectState.Active && !npc.IsTurningDisabled)
					targetOID = (ushort)target.ObjectID;
			}

			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.ObjectUpdate));
			pak.WriteShort((ushort)speed);
			if (obj is GameNPC)
				pak.WriteShort((ushort)(obj.Heading & 0xFFF));
			else pak.WriteShort(obj.Heading);
			pak.WriteShort(XOffsetInZone);
			pak.WriteShort(XOffsetInTargetZone);
			pak.WriteShort(YOffsetInZone);
			pak.WriteShort(YOffsetInTargetZone);
			pak.WriteShort((ushort)obj.Z);
			pak.WriteShort(ZOffsetInTargetZone);
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort((ushort)targetOID);
			//health
			if (obj is GameLiving)
				pak.WriteByte((obj as GameLiving).HealthPercent);
			else pak.WriteByte(0);
			flags |= (byte)(((z.ID & 0x100) >> 6) | ((targetZone & 0x100) >> 5));
			pak.WriteByte(flags);
			pak.WriteByte((byte)z.ID);
			pak.WriteByte((byte)targetZone);
			SendUDP(pak);

			if (obj is GameNPC)
			{
				(obj as GameNPC).NPCUpdatedCallback();
			}
		}

		public virtual void SendPlayerQuit(bool totalOut)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Quit));
			pak.WriteByte((byte)(totalOut ? 0x01 : 0x00));
			if (m_gameClient.Player == null)
				pak.WriteByte(0);
			else
				pak.WriteByte((byte)m_gameClient.Player.Level);
			SendTCP(pak);
		}

		public virtual void SendObjectRemove(GameObject obj)
		{
			int oType = 0;
			if (obj is GamePlayer)
				oType = 2;
			else if (obj is GameNPC)
				oType = (((GameLiving)obj).IsAlive ? 1 : 0);

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RemoveObject));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort((ushort)oType);
			SendTCP(pak);
		}

		public virtual void SendObjectCreate(GameObject obj)
		{
			if (obj.CurrentHouse != m_gameClient.Player.CurrentHouse)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			if (obj is GameStaticItem)
				pak.WriteShort((ushort)(obj as GameStaticItem).Emblem);
			else pak.WriteShort(0);
			pak.WriteShort(obj.Heading);
			pak.WriteShort((ushort)obj.Z);
			pak.WriteInt((uint)obj.X);
			pak.WriteInt((uint)obj.Y);
			if (obj is GameNPC && obj.IsUnderwater)
				pak.WriteShort((ushort)(obj.Model | 0x8000));
			else pak.WriteShort(obj.Model);
			int flag = ((byte)obj.Realm & 3) << 4;
			if (obj is GameKeepBanner)
				flag |= 0x08;
			if (obj is GameStaticItemTimed && m_gameClient.Player != null && (obj as GameStaticItemTimed).IsOwner(m_gameClient.Player))
				flag |= 0x04;
			pak.WriteShort((ushort)flag);
			pak.WritePascalString(obj.Name);
			if (obj is IDoor)
			{
				pak.WriteByte((byte)(obj as IDoor).Flag);
				pak.WriteInt((uint)(obj as IDoor).DoorID);
			}
			else pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendDebugMode(bool on)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DebugMode));
			if (m_gameClient.Account.PrivLevel == 1)
			{
				pak.WriteByte((byte)(0x00));
			}
			else
			{
				pak.WriteByte((byte)(on ? 0x01 : 0x00));
			}
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendModelChange(GameObject obj, ushort newModel)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ModelChange));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort(newModel);
			pak.WriteInt(0x00);
			SendTCP(pak);
		}

		public virtual void SendEmoteAnimation(GameObject obj, eEmote emote)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EmoteAnimation));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteByte((byte)emote);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendNPCCreate(GameNPC npc)
		{
			if (m_gameClient.Player == null || npc.CurrentHouse != m_gameClient.Player.CurrentHouse || npc.CurrentRegion != m_gameClient.Player.CurrentRegion)
				return;

			if (npc is GameMovingObject)
			{
				SendMovingObjectCreate(npc as GameMovingObject); 
				return;
			}

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.NPCCreate));
			int speed = 0;
			ushort speedZ = 0;
			if (npc.IsMoving && !npc.IsOnTarget())
			{
				speed = npc.CurrentSpeed;
				speedZ = (ushort)npc.ZAddition;
			}
			pak.WriteShort((ushort)npc.ObjectID);
			pak.WriteShort((ushort)speed);
			pak.WriteShort(npc.Heading);
			pak.WriteShort((ushort)npc.Z);
			pak.WriteInt((uint)npc.X);
			pak.WriteInt((uint)npc.Y);
			pak.WriteShort(speedZ);
			pak.WriteShort(npc.Model);
			pak.WriteByte(npc.Size);
			pak.WriteByte(npc.Level);

			byte flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);
			if ((npc.Flags & (uint)GameNPC.eFlags.TRANSPARENT) != 0) flags |= 0x01;
			if (npc.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
			if ((npc.Flags & (uint)GameNPC.eFlags.PEACE) != 0) flags |= 0x10;
			if ((npc.Flags & (uint)GameNPC.eFlags.FLYING) != 0) flags |= 0x20;

			pak.WriteByte(flags);
			pak.WriteByte(0x20); //TODO this is the default maxstick distance

			string add = "";
			if (m_gameClient.Account.PrivLevel > 1)
			{
				if ((npc.Flags & (uint)GameNPC.eFlags.CANTTARGET) != 0)
					add += "-DOR"; // indicates DOR flag for GMs
				if ((npc.Flags & (uint)GameNPC.eFlags.DONTSHOWNAME) != 0)
					add += "-NON"; // indicates NON flag for GMs
			}
			string name = npc.Name;
			if (name.Length + add.Length + 2 > 47) // clients crash with too long names
				name = name.Substring(0, 47 - add.Length - 2);
			if (add.Length > 0)
				name = string.Format("[{0}]{1}", name, add);

			pak.WritePascalString(name);

			if (npc.GuildName.Length > 47)
				pak.WritePascalString(npc.GuildName.Substring(0, 47));
			else pak.WritePascalString(npc.GuildName);

			pak.WriteByte(0x00);
			SendTCP(pak);

			//if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
			{
				IControlledBrain brain = npc.Brain as IControlledBrain;
				if (brain != null)
				{
					GamePlayer playerowner = brain.GetPlayerOwner();
					if (playerowner != null)
						SendObjectGuildID(npc, playerowner.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
				}
			}
		}

		public virtual void SendLivingEquipmentUpdate(GameLiving living)
		{
			if (m_gameClient.Player == null || living.CurrentHouse != m_gameClient.Player.CurrentHouse || living.CurrentRegion != m_gameClient.Player.CurrentRegion)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EquipmentUpdate));

			pak.WriteShort((ushort)living.ObjectID);
			pak.WriteByte((byte)((living.IsCloakHoodUp ? 0x01 : 0x00) | (int)living.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte(living.VisibleActiveWeaponSlots);

			if (living.Inventory != null)
			{
				ICollection items = living.Inventory.VisibleItems;
				pak.WriteByte((byte)items.Count);
				foreach (InventoryItem item in items)
				{
					pak.WriteByte((byte)item.SlotPosition);
					ushort model = (ushort)(item.Model & 0x1FFF);
					int texture = (item.Emblem != 0) ? item.Emblem : item.Color;

					if ((texture & ~0xFF) != 0)
						model |= 0x8000;
					else if ((texture & 0xFF) != 0)
						model |= 0x4000;
					if (item.Effect != 0)
						model |= 0x2000;

					pak.WriteShort(model);

					if ((texture & ~0xFF) != 0)
						pak.WriteShort((ushort)texture);
					else if ((texture & 0xFF) != 0)
						pak.WriteByte((byte)texture);
					if (item.Effect != 0)
						pak.WriteShort((ushort)item.Effect);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
		}

		public virtual void SendRegionChanged()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionChanged));
			pak.WriteShort(m_gameClient.Player.CurrentRegionID);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendUpdatePoints()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterPointsUpdate));
			pak.WriteInt((uint)m_gameClient.Player.RealmPoints);
			pak.WriteShort(m_gameClient.Player.LevelPermill);
			pak.WriteShort((ushort)m_gameClient.Player.SkillSpecialtyPoints);
			pak.WriteInt((uint)m_gameClient.Player.BountyPoints);
			pak.WriteShort((ushort)m_gameClient.Player.RealmSpecialtyPoints);
			pak.WriteShort(0); // unknown
			SendTCP(pak);
		}

		public virtual void SendUpdateMoney()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MoneyUpdate));
			pak.WriteByte((byte)m_gameClient.Player.PlayerCharacter.Copper);
			pak.WriteByte((byte)m_gameClient.Player.PlayerCharacter.Silver);
			pak.WriteShort((ushort)m_gameClient.Player.PlayerCharacter.Gold);
			pak.WriteShort((ushort)m_gameClient.Player.PlayerCharacter.Mithril);
			pak.WriteShort((ushort)m_gameClient.Player.PlayerCharacter.Platinum);
			SendTCP(pak);
		}

		public virtual void SendUpdateMaxSpeed()
		{
			//Speed is in % not a fixed value!
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MaxSpeed));
			pak.WriteShort((ushort)(m_gameClient.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED));
			pak.WriteByte((byte)(m_gameClient.Player.IsTurningDisabled ? 0x01 : 0x00));
			// water speed in % of land speed if its over 0 i think
			pak.WriteByte((byte)((m_gameClient.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED) * (m_gameClient.Player.GetModified(eProperty.WaterSpeed) * .01)));
			SendTCP(pak);
		}

		public virtual void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CombatAnimation));
			if (attacker != null)
				pak.WriteShort((ushort)attacker.ObjectID);
			else
				pak.WriteShort(0x00);
			if (defender != null)
				pak.WriteShort((ushort)defender.ObjectID);
			else
				pak.WriteShort(0x00);
			pak.WriteShort(weaponID);
			pak.WriteShort(shieldID);
			pak.WriteByte((byte)style);
			pak.WriteByte(stance);
			if (style > 0xFF)
				pak.WriteByte((byte)(result | 0x80));
			else
				pak.WriteByte(result);
			pak.WriteByte(targetHealthPercent);
			SendTCP(pak);
		}

		public virtual void SendStatusUpdate()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterStatusUpdate));
			pak.WriteByte(m_gameClient.Player.HealthPercent);
			pak.WriteByte(m_gameClient.Player.ManaPercent);
			pak.WriteShort((byte)(m_gameClient.Player.IsAlive ? 0x00 : 0x0f)); // 0x0F if dead
			pak.WriteByte((byte)(m_gameClient.Player.IsSitting ? 0x02 : 0x00));
			pak.WriteByte(m_gameClient.Player.EndurancePercent);
			pak.WriteByte(m_gameClient.Player.ConcentrationPercent);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SpellCastAnimation));
			pak.WriteShort((ushort)spellCaster.ObjectID);
			pak.WriteShort(spellID);
			pak.WriteShort(castingTime);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SpellEffectAnimation));
			pak.WriteShort((ushort)spellCaster.ObjectID);
			pak.WriteShort(spellid);
			pak.WriteShort((ushort)(spellTarget == null ? 0 : spellTarget.ObjectID));
			pak.WriteShort(boltTime);
			pak.WriteByte((byte)(noSound ? 1 : 0));
			pak.WriteByte(success);
			pak.WriteShort(0xFFBF);
			SendTCP(pak);
		}

		public virtual void SendRiding(GameObject rider, GameObject steed, bool dismount)
		{
			int slot = 0;
			if (steed is GameNPC && rider is GamePlayer && dismount == false)
			{
				slot = (steed as GameNPC).RiderSlot(rider as GamePlayer);
			}
			if (slot == -1)
				log.Error("SendRiding error, slot is -1 with rider " + rider.Name + " steed " + steed.Name);
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Riding));
			pak.WriteShort((ushort)rider.ObjectID);
			pak.WriteShort((ushort)steed.ObjectID);
			pak.WriteByte((byte)(dismount ? 0x00 : 0x01));
			pak.WriteByte((byte)slot);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendFindGroupWindowUpdate(GamePlayer[] list)
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.FindGroupUpdate));
			if (list != null)
			{
				pak.WriteByte((byte)list.Length);
				byte nbleader = 0;
				byte nbsolo = 0x1E;
				foreach (GamePlayer player in list)
				{
					if (player.Group != null)
					{
						pak.WriteByte(nbleader++);
					}
					else
					{
						pak.WriteByte(nbsolo++);
					}
					pak.WriteByte(player.Level);
					pak.WritePascalString(player.Name);
					pak.WriteString(player.CharacterClass.Name, 4);
					if (player.CurrentZone != null)
						pak.WriteByte((byte)player.CurrentZone.ID);
					else
						pak.WriteByte(255);
				}
			}
			else
			{
				pak.WriteShort(0x0000);
			}
			SendTCP(pak);
		}

		public virtual void SendGroupInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte(0x05);
			pak.WriteShort((ushort)invitingPlayer.Client.SessionID); //data1
			pak.Fill(0x00, 6); //data2&data3
			pak.WriteByte(0x01);
			pak.WriteByte(0x00);
			pak.WriteString(inviteMessage, inviteMessage.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendGuildInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte(0x03);
			pak.WriteShort((ushort)invitingPlayer.ObjectID); //data1
			pak.Fill(0x00, 6); //data2&data3
			pak.WriteByte(0x01);
			pak.WriteByte(0x00);
			pak.WriteString(inviteMessage, inviteMessage.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendGuildLeaveCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte(0x08);
			pak.WriteShort((ushort)invitingPlayer.ObjectID); //data1
			pak.Fill(0x00, 6); //data2&data3
			pak.WriteByte(0x01);
			pak.WriteByte(0x00);
			pak.WriteString(inviteMessage, inviteMessage.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
		}

		public virtual void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
		}

		protected virtual void SendQuestWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest,
			bool offer)
		{
		}

		// i'm reusing the questsubscribe command for quest abort since its 99% the same, only different event dets fired
		// data 3 defines wether it's subscribe or abort
		public virtual void SendQuestSubscribeCommand(GameNPC invitingNPC, ushort questid, string inviteMessage)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte(0x64);
			pak.WriteShort((ushort)questid); //questid, data1
			pak.WriteShort((ushort)invitingNPC.ObjectID); //data2
			pak.WriteShort(0x00); // 0x00 means subscribe data3
			pak.WriteShort(0x00);
			pak.WriteByte(0x01); // yes/no response
			pak.WriteByte(0x01); // autowrap message
			pak.WriteString(inviteMessage, inviteMessage.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		// i'm reusing the questsubscribe command for quest abort since its 99% the same, only different event dets fired
		// data 3 defines wether it's subscribe or abort
		public virtual void SendQuestAbortCommand(GameNPC abortingNPC, ushort questid, string abortMessage)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte(0x64);
			pak.WriteShort((ushort)questid); //questid, data1
			pak.WriteShort((ushort)abortingNPC.ObjectID); //data2
			pak.WriteShort(0x01); // 0x01 means abort data3
			pak.WriteShort(0x00);
			pak.WriteByte(0x01); // yes/no response
			pak.WriteByte(0x01); // autowrap message
			pak.WriteString(abortMessage, abortMessage.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWarpText, string message)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte((byte)code);
			pak.WriteShort((ushort)data1); //data1
			pak.WriteShort((ushort)data2); //data2
			pak.WriteShort((ushort)data3); //data3
			pak.WriteShort((ushort)data4); //data4
			pak.WriteByte((byte)type);
			pak.WriteByte((byte)(autoWarpText == true ? 0x01 : 0x00));
			pak.WriteString(message, message.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendCustomDialog(string msg, CustomDialogResponse callback)
		{
			if (m_gameClient.Player == null)
				return;
			lock (m_gameClient.Player)
			{
				if (m_gameClient.Player.CustomDialogCallback != null)
					m_gameClient.Player.CustomDialogCallback(m_gameClient.Player, 0x00);
				m_gameClient.Player.CustomDialogCallback = callback;
			}

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte((byte)eDialogCode.CustomDialog);
			pak.WriteShort((ushort)m_gameClient.SessionID); //data1
			pak.WriteShort(0x01); //custom dialog!	  //data2
			pak.WriteShort(0x00); //data3
			pak.WriteShort(0x00);
			pak.WriteByte((byte)(callback == null ? 0x00 : 0x01)); //ok or yes/no response
			pak.WriteByte(0x01); // autowrap text
			pak.WriteString(msg, msg.Length);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}


		public virtual void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback)
		{
			if (m_gameClient.Player == null)
				return;
			int TargetOID = (Target != null ? Target.ObjectID : 0);
			string key = string.Format("LOS C:0x{0} T:0x{1}", Checker.ObjectID, TargetOID);
			CheckLOSResponse old_callback = null;
			lock (m_gameClient.Player.TempProperties)
			{
				old_callback = (CheckLOSResponse)m_gameClient.Player.TempProperties.getObjectProperty(key, null);
				m_gameClient.Player.TempProperties.setProperty(key, callback);
			}
			if (old_callback != null)
				old_callback(m_gameClient.Player, 0, 0); // not sure for this,  i want targetOID there

			GSTCPPacketOut pak = new GSTCPPacketOut(0xD0);
			pak.WriteShort((ushort)Checker.ObjectID);
			pak.WriteShort((ushort)TargetOID);
			pak.WriteShort(0x00); // ?
			pak.WriteShort(0x00); // ?
			SendTCP(pak);
		}

		protected virtual void SendQuestPacket(AbstractQuest quest, int index)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));

			pak.WriteByte((byte)index);
			if (quest.Step <= 0)
			{
				pak.WriteByte(0);
				pak.WriteByte(0);
				pak.WriteByte(0);
			}
			else
			{
				string name = quest.Name;
				string desc = quest.Description;
				if (name.Length > byte.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": name is too long for 1.68+ clients (" + name.Length + ") '" + name + "'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > byte.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": description is too long for 1.68+ clients (" + desc.Length + ") '" + desc + "'");
					desc = desc.Substring(0, byte.MaxValue);
				}
				pak.WriteByte((byte)name.Length);
				pak.WriteByte((byte)desc.Length);
				pak.WriteByte(0);
				pak.WriteStringBytes(name); //Write Quest Name without trailing 0
				pak.WriteStringBytes(desc); //Write Quest Description without trailing 0
			}
			SendTCP(pak);
		}

		protected virtual void SendTaskInfo()
		{ }

		protected string BuildTaskString()
		{
			if (m_gameClient.Player == null)
				return "";
			AbstractTask task = m_gameClient.Player.Task;
			AbstractMission pMission = m_gameClient.Player.Mission;

			AbstractMission gMission = null;
			if (m_gameClient.Player.Group != null)
				gMission = m_gameClient.Player.Group.Mission;

			AbstractMission rMission = null;

			//all the task info is sent in name field

			string taskStr = "";
			if (task == null)
				taskStr = "You have no current personal task.\n";
			else taskStr = "[" + task.Name + "] " + task.Description + ".\n";

			string personalMission = "";
			if (pMission != null)
				personalMission = "[" + pMission.Name + "] " + pMission.Description + ".\n";

			string groupMission = "";
			if (gMission != null)
				groupMission = "[" + gMission.Name + "] " + gMission.Description + ".\n";

			string realmMission = "";
			if (rMission != null)
				realmMission = "[" + rMission.Name + "]" + " " + rMission.Description + ".\n";

			string name = taskStr + personalMission + groupMission + realmMission;

			if (name.Length > ushort.MaxValue)
			{
				if (log.IsWarnEnabled) log.Warn("Task packet name is too long for 1.71 clients (" + name.Length + ") '" + name + "'");
				name = name.Substring(0, byte.MaxValue);
			}
			if (name.Length > 2048 - 10)
			{
				name = name.Substring(0, 2048 - 10 - name.Length);
			}

			return name;
		}

		public virtual void SendQuestUpdate(AbstractQuest quest)
		{
			int questIndex = 0;
			lock (m_gameClient.Player.QuestList)
			{
				foreach (AbstractQuest q in m_gameClient.Player.QuestList)
				{
					if (q == quest)
					{
						SendQuestPacket(q, questIndex);
						break;
					}

					if (q.Step != -1)
						questIndex++;
				}
			}
		}

		public virtual void SendQuestListUpdate()
		{
			int questIndex = 0;
			lock (m_gameClient.Player.QuestList)
			{
				foreach (AbstractQuest quest in m_gameClient.Player.QuestList)
				{
					if (quest.Step != -1)
					{
						SendQuestPacket(quest, questIndex);
						questIndex++;
					}
				}
			}
		}

		public virtual void SendGroupWindowUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x06);

			Group group = m_gameClient.Player.Group;
			if (group == null)
			{
				pak.WriteByte(0x00);
			}
			else
			{
				pak.WriteByte((byte)group.MemberCount);
			}

			pak.WriteByte(0x01);
			pak.WriteByte(0x00);

			if (group != null)
			{
				lock (group)
				{
					foreach (GamePlayer updatePlayer in group)
					{
						bool sameRegion = updatePlayer.CurrentRegion == m_gameClient.Player.CurrentRegion;

						pak.WriteByte(updatePlayer.Level);
						if (sameRegion)
						{
							pak.WriteByte(updatePlayer.HealthPercent);
							pak.WriteByte(updatePlayer.ManaPercent);

							byte playerStatus = 0;
							if (!updatePlayer.IsAlive)
								playerStatus |= 0x01;
							if (updatePlayer.IsMezzed)
								playerStatus |= 0x02;
							if (updatePlayer.IsDiseased)
								playerStatus |= 0x04;
							if (SpellHandler.FindEffectOnTarget(updatePlayer, "DamageOverTime") != null)
								playerStatus |= 0x08;
							if (updatePlayer.Client.ClientState == GameClient.eClientState.Linkdead)
								playerStatus |= 0x10;
							if (updatePlayer.CurrentRegion != m_gameClient.Player.CurrentRegion)
								playerStatus |= 0x20;

							pak.WriteByte(playerStatus);
							// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
							// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

							pak.WriteShort((ushort)updatePlayer.ObjectID); //or session id?
						}
						else
						{
							pak.WriteInt(0x2000);
							pak.WriteByte(0);
						}
						pak.WritePascalString(updatePlayer.Name);
						pak.WritePascalString(updatePlayer.CharacterClass.Name); //classname
					}
				}
			}
			SendTCP(pak);
		}

		public void SendGroupMemberUpdate(bool updateIcons, GameLiving living)
		{
			if (m_gameClient.Player == null)
				return;
			Group group = m_gameClient.Player.Group;
			if (group == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.GroupMemberUpdate));
			lock (group)
			{ // make sure group is not modified before update is sent else player index could change _before_ update
				if (living.Group != group)
					return;
				WriteGroupMemberUpdate(pak, updateIcons, living);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public void SendGroupMembersUpdate(bool updateIcons)
		{
			if (m_gameClient.Player == null)
				return;

			Group group = m_gameClient.Player.Group;
			if (group == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.GroupMemberUpdate));
			lock (group)
			{ // make sure group is not modified before update is sent else player index could change _before_ update
				foreach (GameLiving living in group)
					WriteGroupMemberUpdate(pak, updateIcons, living);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		protected virtual void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GameLiving living)
		{
			pak.WriteByte((byte)(living.GroupIndex + 1)); // From 1 to 8
			bool sameRegion = living.CurrentRegion == m_gameClient.Player.CurrentRegion;
			if (sameRegion)
			{
				pak.WriteByte(living.HealthPercent);
				pak.WriteByte(living.ManaPercent);

				byte playerStatus = 0;
				if (!living.IsAlive)
					playerStatus |= 0x01;
				if (living.IsMezzed)
					playerStatus |= 0x02;
				if (living.IsDiseased)
					playerStatus |= 0x04;
				if (SpellHandler.FindEffectOnTarget(living, "DamageOverTime") != null)
					playerStatus |= 0x08;
				if (living is GamePlayer && ((GamePlayer)living).Client.ClientState == GameClient.eClientState.Linkdead)
					playerStatus |= 0x10;
				if (!sameRegion)
					playerStatus |= 0x20;

				pak.WriteByte(playerStatus);
				// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
				// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

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
								pak.WriteShort(effect.Icon);
							}
					}
				}
			}
			else
			{
				pak.WriteShort(0);
				pak.WriteByte(0x20);
				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | living.GroupIndex));
					pak.WriteByte(0);
				}
			}
		}

		protected virtual void SendInventorySlotsUpdateBase(ICollection slots, byte preAction)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			pak.WriteByte((byte)(slots == null ? 0 : slots.Count));
			pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte(m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(preAction); //preAction (0x00 - Do nothing)
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					pak.WriteByte((byte)updatedSlot);
					InventoryItem item = m_gameClient.Player.Inventory.GetItem((eInventorySlot)updatedSlot);

					if (item == null)
					{
						pak.Fill(0x00, 18);
						continue;
					}

					pak.WriteByte((byte)item.Level);

					int value1; // some object types use this field to display count
					int value2; // some object types use this field to display count
					switch (item.Object_Type)
					{
						case (int)eObjectType.Arrow:
						case (int)eObjectType.Bolt:
						case (int)eObjectType.Poison:
						case (int)eObjectType.GenericItem:
							value1 = item.Count;
							value2 = item.SPD_ABS;
							break;
						case (int)eObjectType.Thrown:
							value1 = item.DPS_AF;
							value2 = item.Count;
							break;
						case (int)eObjectType.Instrument:
							value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF); // 0x00 = Lute ; 0x01 = Drum ; 0x03 = Flute
							value2 = 0;
							break; // unused
						case (int)eObjectType.Shield:
							value1 = item.Type_Damage;
							value2 = item.DPS_AF;
							break;
						case (int)eObjectType.GardenObject:
							value1 = 0;
							value2 = item.SPD_ABS;
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
					pak.WriteShort((ushort)item.Weight);
					pak.WriteByte(item.ConditionPercent); // % of con
					pak.WriteByte(item.DurabilityPercent); // % of dur
					pak.WriteByte((byte)item.Quality); // % of qua
					pak.WriteByte((byte)item.Bonus); // % bonus
					pak.WriteShort((ushort)item.Model);
					if (item.Emblem != 0)
						pak.WriteShort((ushort)item.Emblem);
					else
						pak.WriteShort((ushort)item.Color);
					pak.WriteShort((ushort)item.Effect);
					if (item.Count > 1)
						pak.WritePascalString(item.Count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
			}
			SendTCP(pak);
		}

		public virtual void SendInventorySlotsUpdate(ICollection slots)
		{
			// slots contain ints

			if (m_gameClient.Player == null)
				return;

			// clients crash if too long packet is sent
			// so we send big updates in parts
			const int MAX_UPDATE = 32;
			if (slots == null || slots.Count <= MAX_UPDATE)
			{
				SendInventorySlotsUpdateBase(slots, 0);
			}
			else
			{
				ArrayList updateSlots = new ArrayList(MAX_UPDATE);
				foreach (int slot in slots)
				{
					updateSlots.Add(slot);
					if (updateSlots.Count >= MAX_UPDATE)
					{
						SendInventorySlotsUpdateBase(updateSlots, 0);
						updateSlots.Clear();
					}
				}
				if (updateSlots.Count > 0)
					SendInventorySlotsUpdateBase(updateSlots, 0);
			}
		}

		public virtual void SendInventoryItemsUpdate(IDictionary<int, InventoryItem> updateItems, byte windowType)
		{
		}

		public virtual void SendInventoryItemsPartialUpdate(IDictionary<int, InventoryItem> items, byte windowType)
		{
		}

		public virtual void SendInventoryItemsUpdate(ICollection itemsToUpdate)
		{
			SendInventoryItemsUpdate(0, itemsToUpdate);
		}

		public virtual void SendInventoryItemsUpdate(byte preAction, ICollection itemsToUpdate)
		{
			if (m_gameClient.Player == null)
				return;
			if (itemsToUpdate == null)
			{
				SendInventorySlotsUpdateBase(null, preAction);
				return;
			}

			// clients crash if too long packet is sent
			// so we send big updates in parts
			const int MAX_UPDATE = 32;
			ArrayList slotsToUpdate = new ArrayList(Math.Min(MAX_UPDATE, itemsToUpdate.Count));
			foreach (InventoryItem item in itemsToUpdate)
			{
				if (item == null)
					continue;

				slotsToUpdate.Add(item.SlotPosition);
				if (slotsToUpdate.Count >= MAX_UPDATE)
				{
					SendInventorySlotsUpdateBase(slotsToUpdate, preAction);
					slotsToUpdate.Clear();
					preAction = 0;
				}
			}
			if (slotsToUpdate.Count > 0)
			{
				SendInventorySlotsUpdateBase(slotsToUpdate, preAction);
			}
		}

		public virtual void SendInventoryItemsPartialUpdate(List<InventoryItem> items, byte windowType)
		{
		}

		public virtual void SendDoorState(IDoor door)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DoorState));
			pak.WriteInt((uint)door.DoorID);
			pak.WriteByte((byte)(door.State == eDoorState.Open ? 0x01 : 0x00));
			pak.WriteByte((byte)door.Flag);
			pak.Fill(0x0, 2);
			SendTCP(pak);
		}

		public virtual void SendMerchantWindow(MerchantTradeItems tradeItemsList, eMerchantWindowType windowType)
		{
			GSTCPPacketOut pak;
			if (tradeItemsList != null)
			{
				for (int page = 0; page < MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS; page++)
				{
					IDictionary itemsInPage = tradeItemsList.GetItemsInPage(page);
					if (itemsInPage == null || itemsInPage.Count == 0)
						continue;

					pak = new GSTCPPacketOut(GetPacketCode(ePackets.MerchantWindow));

					pak.WriteByte((byte)itemsInPage.Count); //Item count on this page
					pak.WriteByte((byte)windowType); //Unknown 0x00
					pak.WriteByte((byte)page); //Page number
					pak.WriteByte(0x00); //Unused

					for (int i = 0; i < MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS; i++)
					{
						if (!itemsInPage.Contains(i))
							continue;

						ItemTemplate item = (ItemTemplate)itemsInPage[i];//(ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate),((MerchantItem)tradeItemsList.ItemsList[page*MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS+i]).ItemTemplateID);
						if (item != null)
						{
							//DOLConsole.WriteLine("Item i="+itemIndex);
							pak.WriteByte((byte)i); //Item index on page
							pak.WriteByte((byte)item.Level);
							// some objects use this for count
							int value1;
							int value2;
							switch (item.Object_Type)
							{
								case (int)eObjectType.Arrow:
								case (int)eObjectType.Bolt:
								case (int)eObjectType.Poison:
								case (int)eObjectType.GenericItem:
									{
										value1 = item.PackSize;
										value2 = value1 * item.Weight;
										break;
									}
								case (int)eObjectType.Thrown:
									{
										value1 = item.DPS_AF;
										value2 = item.PackSize;
										break;
									}
								case (int)eObjectType.Shield:
									{
										value1 = item.Type_Damage;
										value2 = item.Weight;
										break;
									}
								case (int)eObjectType.GardenObject:
									{
										value1 = 0;
										value2 = item.Weight;
										break;
									}
								default:
									{
										value1 = item.DPS_AF;
										value2 = item.Weight;
										break;
									}
							}
							pak.WriteByte((byte)value1);
							pak.WriteByte((byte)item.SPD_ABS);
							if (item.Object_Type == (int)eObjectType.GardenObject)
								pak.WriteByte((byte)(item.DPS_AF));
							else
								pak.WriteByte((byte)(item.Hand << 6));
							pak.WriteByte((byte)((item.Type_Damage << 6) | item.Object_Type));
							//1 if item cannot be used by your class (greyed out)
							if (m_gameClient.Player.HasAbilityToUseItem(item))
								pak.WriteByte(0x00);
							else
								pak.WriteByte(0x01);
							pak.WriteShort((ushort)value2);
							//Item Price
							pak.WriteInt((uint)item.Value);
							pak.WriteShort((ushort)item.Model);
							pak.WritePascalString(item.Name);
						}
						else
						{
							if (log.IsErrorEnabled)
								log.Error("Merchant item template '" + ((MerchantItem)itemsInPage[page * MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS + i]).ItemTemplateID + "' not found, abort!!!");
							return;
						}
					}
					SendTCP(pak);
				}
			}
			else
			{
				pak = new GSTCPPacketOut(GetPacketCode(ePackets.MerchantWindow));
				pak.WriteByte((byte)0); //Item count on this page
				pak.WriteByte((byte)windowType); //Unknown 0x00
				pak.WriteByte((byte)0); //Page number
				pak.WriteByte(0x00); //Unused
				SendTCP(pak);
			}
		}

		public virtual void SendTradeWindow()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TradeWindow));
			lock (m_gameClient.Player.TradeWindow.Sync)
			{
				foreach (InventoryItem item in m_gameClient.Player.TradeWindow.TradeItems)
				{
					pak.WriteByte((byte)item.SlotPosition);
				}
				pak.Fill(0x00, 10 - m_gameClient.Player.TradeWindow.TradeItems.Count);

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort)Money.GetMithril(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort)Money.GetPlatinum(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort)Money.GetGold(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort)Money.GetSilver(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort)Money.GetCopper(m_gameClient.Player.TradeWindow.TradeMoney));

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort)Money.GetMithril(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort)Money.GetPlatinum(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort)Money.GetGold(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort)Money.GetSilver(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort)Money.GetCopper(m_gameClient.Player.TradeWindow.PartnerTradeMoney));

				pak.WriteShort(0x0000);
				ArrayList items = m_gameClient.Player.TradeWindow.PartnerTradeItems;
				if (items != null)
				{
					pak.WriteByte((byte)items.Count);
					pak.WriteByte(0x01);
				}
				else
				{
					pak.WriteShort(0x0000);
				}
				pak.WriteByte((byte)(m_gameClient.Player.TradeWindow.Repairing ? 0x01 : 0x00));
				pak.WriteByte((byte)(m_gameClient.Player.TradeWindow.Combine ? 0x01 : 0x00));
				if (items != null)
				{
					foreach (InventoryItem item in items)
					{
						pak.WriteByte((byte)item.SlotPosition);
						pak.WriteByte((byte)item.Level);
						pak.WriteByte((byte)item.DPS_AF); // dps_af
						pak.WriteByte((byte)item.SPD_ABS); //spd_abs
						pak.WriteByte((byte)(item.Hand << 6));
						pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
						pak.WriteShort((ushort)item.Weight); // weight
						pak.WriteByte(item.ConditionPercent); // con %
						pak.WriteByte(item.DurabilityPercent); // dur %
						pak.WriteByte((byte)item.Quality); // qua %
						pak.WriteByte((byte)item.Bonus); // bon %
						pak.WriteShort((ushort)item.Model); //model
						pak.WriteShort((ushort)item.Color); //color
						pak.WriteShort((ushort)item.Effect); //weaponproc
						if (item.Count > 1)
							pak.WritePascalString(item.Count + " " + item.Name);
						else
							pak.WritePascalString(item.Name); //size and name item
					}
				}
				if (m_gameClient.Player.TradeWindow is SelfCraftWindow)
					pak.WritePascalString("Combining for " + m_gameClient.Player.Name);
				else
					pak.WritePascalString("Trading with " + m_gameClient.Player.TradeWindow.Partner.Name); // transaction with ...
				SendTCP(pak);
			}
		}

		public virtual void SendCloseTradeWindow()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TradeWindow));
			pak.Fill(0x00, 40);
			SendTCP(pak);
		}

		public virtual void SendPlayerDied(GamePlayer killedPlayer, GameObject killer)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerDeath));

			pak.WriteShort((ushort)killedPlayer.ObjectID);
			if (killer != null)
				pak.WriteShort((ushort)killer.ObjectID);
			else
				pak.WriteShort(0x00);
			pak.Fill(0x0, 4);
			SendTCP(pak);
		}

		public virtual void SendPlayerRevive(GamePlayer revivedPlayer)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerRevive));

			pak.WriteShort((ushort)revivedPlayer.ObjectID);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendUpdatePlayer()
		{
			GamePlayer player = m_gameClient.Player;
			if (player == null)
				return;
			
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x03); //subcode
			pak.WriteByte(0x0d); //number of entry
			pak.WriteByte(0x00); //subtype
			pak.WriteByte(0x00); //unk
			//entry :

			pak.WriteByte(player.Level); //level
			pak.WritePascalString(player.Name); // player name
			pak.WriteByte((byte)(player.MaxHealth >> 8)); // maxhealth high byte ?
			pak.WritePascalString(player.CharacterClass.Name); // class name
			pak.WriteByte((byte)(player.MaxHealth & 0xFF)); // maxhealth low byte ?
			pak.WritePascalString( /*"The "+*/player.CharacterClass.Profession); // Profession
			pak.WriteByte(0x00); //unk
			pak.WritePascalString(player.CharacterClass.GetTitle(player.Level)); // player level

			//todo make function to calcule realm rank
			//client.Player.RealmPoints
			//todo i think it s realmpoint percent not realrank
			pak.WriteByte((byte)player.RealmLevel); //urealm rank
			pak.WritePascalString(player.RealmTitle); // Realm title
			pak.WriteByte((byte)player.RealmSpecialtyPoints); // realm skill points
			pak.WritePascalString(player.CharacterClass.BaseName); // base class
			pak.WriteByte((byte)(HouseMgr.GetHouseNumberByPlayer(player) >> 8)); // personal house high byte
			pak.WritePascalString(player.GuildName);  // Guild name
			pak.WriteByte((byte)(HouseMgr.GetHouseNumberByPlayer(player) & 0xFF)); // personal house low byte
			pak.WritePascalString(player.LastName); // Last name
			pak.WriteByte((byte)(player.MLLevel+1)); // ML Level (+1)
			pak.WritePascalString(player.RaceName); // Race name			
			pak.WriteByte(0x0);
			
			if (player.GuildRank != null)
				pak.WritePascalString(player.GuildRank.Title); // Guild title
			else
				pak.WritePascalString("");
			pak.WriteByte(0x0);

			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(player.CraftingPrimarySkill);
			if (skill != null)
				pak.WritePascalString(skill.Name); //crafter guilde: alchemist
			else
				pak.WritePascalString("None"); //no craft skill at start

			pak.WriteByte(0x0);
			pak.WritePascalString(player.CraftTitle); //crafter title: legendary alchemist
			pak.WriteByte(0x0);
			pak.WritePascalString(player.MLTitle); //ML title
			SendTCP(pak);
		}

		public virtual void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			maxSkills++;
		}

		public virtual void SendUpdatePlayerSkills()
		{
			if (m_gameClient.Player == null)
				return;
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			IList spelllines = m_gameClient.Player.GetSpellLines();
			Hashtable m_styleId = new Hashtable();
			int maxSkills = 0;
			int firstSkills = 0;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			bool flagSendHybrid = true;
			if (m_gameClient.Player.CharacterClass.ClassType == eClassType.ListCaster)
				flagSendHybrid = false;

			lock (skills.SyncRoot)
			{
				lock (styles.SyncRoot)
				{
					lock (specs.SyncRoot)
					{
						lock (spelllines.SyncRoot)
						{
							int skillCount = specs.Count + skills.Count + styles.Count;
							if (flagSendHybrid)
								skillCount += m_gameClient.Player.GetAmountOfSpell();

							pak.WriteByte(0x01); //subcode
							pak.WriteByte((byte)skillCount); //number of entry
							pak.WriteByte(0x03); //subtype
							pak.WriteByte((byte)firstSkills);

							foreach (Specialization spec in specs)
							{
								CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								pak.WriteByte((byte)spec.Level);
								pak.WriteByte((byte)eSkillPage.Specialization);
								pak.WriteShort(0);
								pak.WriteByte((byte)(m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
								pak.WriteShort(spec.ID);
								pak.WritePascalString(spec.Name);
							}

							int i = 0;
							foreach (Skill skill in skills)
							{
								i++;
								CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								pak.WriteByte(0);
								byte type = (byte)eSkillPage.Abilities;
								if (skill is RealmAbility)
								{
									type = (byte)eSkillPage.RealmAbilities;
								}
								pak.WriteByte(type);
								pak.WriteShort(0);
								pak.WriteByte(0);
								pak.WriteShort(skill.ID);
								string str = "";
								if (m_gameClient.Player.CharacterClass.ID == (int)eCharacterClass.Vampiir)
								{
									if (skill.Name == Abilities.VampiirConstitution ||
											skill.Name == Abilities.VampiirDexterity ||
											skill.Name == Abilities.VampiirStrength)
										str = " +" + ((m_gameClient.Player.Level - 5) * 3).ToString();
									else if (skill.Name == Abilities.VampiirQuickness)
										str = " +" + ((m_gameClient.Player.Level - 5) * 2).ToString();
								}
								pak.WritePascalString(skill.Name + str);
							}

							foreach (Style style in styles)
							{
								m_styleId[(int)style.ID] = i++;
								CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								//DOLConsole.WriteLine("style sended "+style.Name);
								pak.WriteByte(0); // no level for style
								pak.WriteByte((byte)eSkillPage.Styles);

								int pre = 0;
								switch (style.OpeningRequirementType)
								{
									case Style.eOpening.Offensive:
										pre = 0 + (int)style.AttackResultRequirement; // last result of our attack against enemy
										// hit, miss, target blocked, target parried, ...
										if (style.AttackResultRequirement == Style.eAttackResult.Style)
											pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
										break;
									case Style.eOpening.Defensive:
										pre = 100 + (int)style.AttackResultRequirement; // last result of enemies attack against us
										// hit, miss, you block, you parry, ...
										break;
									case Style.eOpening.Positional:
										pre = 200 + style.OpeningRequirementValue;
										break;
								}

								// style required?
								if (pre == 0)
								{
									pre = 0x100;
								}

								pak.WriteShort((ushort)pre);
								pak.WriteByte(0); // bonus
								pak.WriteShort((ushort)style.Icon);
								pak.WritePascalString(style.Name);
							}
							if (flagSendHybrid)
							{
								foreach (SpellLine spellline in spelllines)
								{
									int spec_index = specs.IndexOf(m_gameClient.Player.GetSpecialization(spellline.Spec));
									if (spec_index == -1)
										spec_index = 0xFE; // Nightshade special value
									IList spells = m_gameClient.Player.GetUsableSpellsOfLine(spellline);
									foreach (Spell spell in spells)
									{
										CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
										pak.WriteByte((byte)spell.Level);
										if (spell.InstrumentRequirement == 0)
										{
											pak.WriteByte((byte)eSkillPage.Spells);
											pak.WriteByte(0);
											pak.WriteByte((byte)spec_index);
										}
										else
										{
											pak.WriteByte((byte)eSkillPage.Songs);
											pak.WriteByte(0);
											pak.WriteByte(0xFF);
										}
										pak.WriteByte(0);
										pak.WriteShort(spell.Icon);
										pak.WritePascalString(spell.Name);
									}
								}
							}
						}
					}
				}
			}
			if (pak.Length > 7)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - firstSkills)); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)firstSkills);
				SendTCP(pak);
			}
			if (!flagSendHybrid)
				SendListCastersSpell();
		}

		public virtual void SendListCastersSpell()
		{
			GSTCPPacketOut pak;// = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			IList spelllines = m_gameClient.Player.GetSpellLines();
			byte linenumber = 0;
			lock (spelllines.SyncRoot)
			{
				foreach (SpellLine line in spelllines)
				{
					IList spells = SkillBase.GetSpellList(line.KeyName);
					int spellcount = 0;
					for (int i = 0; i < spells.Count; i++)
					{
						if (((Spell)spells[i]).Level <= line.Level)
						{
							spellcount++;
						}
					}
					pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
					pak.WriteByte(0x02); //subcode
					pak.WriteByte((byte)(spellcount + 1)); //number of entry
					pak.WriteByte(0x02); //subtype
					pak.WriteByte(linenumber++); //number of line
					pak.WriteByte(0); // level, not used when spell line
					pak.WriteShort(0); // icon, not used when spell line
					pak.WritePascalString(line.Name);
					foreach (Spell spell in spells)
					{
						if (spell.Level <= line.Level)
						{
							pak.WriteByte((byte)spell.Level);
							pak.WriteShort(spell.Icon);
							pak.WritePascalString(spell.Name);
						}
					}
					SendTCP(pak);
				}
			}
		}

		public virtual void SendUpdateCraftingSkills()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));

			pak.WriteByte(0x08); //subcode
			pak.WriteByte((byte)m_gameClient.Player.CraftingSkills.Count); //count
			pak.WriteByte(0x03); //subtype
			pak.WriteByte(0x00); //unk

			foreach (DictionaryEntry de in (Hashtable)m_gameClient.Player.CraftingSkills.Clone())
			{
				AbstractCraftingSkill curentCraftingSkill = CraftingMgr.getSkillbyEnum((eCraftingSkill)de.Key);
				pak.WriteShort(Convert.ToUInt16(de.Value)); //points
				pak.WriteByte(curentCraftingSkill.Icon); //icon
				pak.WriteInt(1);
				pak.WritePascalString(curentCraftingSkill.Name); //name
			}
			SendTCP(pak);
		}

		public virtual void SendUpdateWeaponAndArmorStats()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x05); //subcode
			pak.WriteByte(6); //number of entries
			pak.WriteByte(0x00); //subtype
			pak.WriteByte(0x00); //unk

			// weapondamage
			int wd = (int)(m_gameClient.Player.WeaponDamage(m_gameClient.Player.AttackWeapon) * 100.0);
			pak.WriteByte((byte)(wd / 100));
			pak.WritePascalString(" ");
			pak.WriteByte((byte)(wd % 100));
			pak.WritePascalString(" ");
			// weaponskill
			int ws = m_gameClient.Player.DisplayedWeaponSkill;
			pak.WriteByte((byte)(ws >> 8));
			pak.WritePascalString(" ");
			pak.WriteByte((byte)(ws & 0xff));
			pak.WritePascalString(" ");
			// overall EAF
			int eaf = m_gameClient.Player.EffectiveOverallAF;
			pak.WriteByte((byte)(eaf >> 8));
			pak.WritePascalString(" ");
			pak.WriteByte((byte)(eaf & 0xff));
			pak.WritePascalString(" ");

			SendTCP(pak);
		}

		public virtual void SendEncumberance()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Encumberance));
			pak.WriteShort((ushort)m_gameClient.Player.MaxEncumberance); // encumb total
			pak.WriteShort((ushort)m_gameClient.Player.Encumberance); // encumb used
			SendTCP(pak);
		}

		public virtual void SendCustomTextWindow(string caption, IList text)
		{
			if (text == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DetailWindow));

			pak.WritePascalString(caption); //window caption

			IEnumerator iter = text.GetEnumerator();
			byte line = 1;
			while (iter.MoveNext())
			{
				pak.WriteByte(line++);
				pak.WritePascalString((string)iter.Current);
			}

			//Trailing Zero!
			pak.WriteByte(0);
			SendTCP(pak);
		}

		public virtual void SendPlayerTitles()
		{
			IList text = GameServer.ServerRules.FormatPlayerStatistics(m_gameClient.Player);
			text.Add(" ");
			text.Add("Titles:");
			foreach (IPlayerTitle title in m_gameClient.Player.Titles)
				text.Add("- " + title.GetDescription(m_gameClient.Player));
			SendCustomTextWindow("Player Statistics", text);
		}

		public virtual void SendPlayerTitleUpdate(GamePlayer player)
		{
		}

		public virtual void SendAddFriends(string[] friendNames)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.AddFriend));
			foreach (string friend in friendNames)
				pak.WritePascalString(friend);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendRemoveFriends(string[] friendNames)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RemoveFriend));
			foreach (string friend in friendNames)
				pak.WritePascalString(friend);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendTimerWindow(string title, int seconds)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TimerWindow));
			pak.WriteShort((ushort)seconds);
			pak.WriteByte((byte)title.Length);
			pak.WriteByte(1);
			pak.WriteString(title);
			SendTCP(pak);
		}

		public virtual void SendCloseTimerWindow()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TimerWindow));
			pak.WriteShort(0);
			pak.WriteByte(0);
			pak.WriteByte(0);
			SendTCP(pak);
		}
		
        public virtual void SendChampionTrainerWindow(int type)
        {
            GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TrainerWindow));
            pak.WriteByte((byte)type);
            pak.WriteByte((byte)m_gameClient.Player.ChampionSpecialtyPoints);
            pak.WriteByte((byte)2);
            pak.WriteByte((byte)0);
            pak.WriteByte((byte)6);

            for (int skillindex = 1; skillindex < 7; skillindex++)
            {
                IList specs = ChampSpecMgr.GetAbilityForIndex(type, skillindex);
                pak.WriteByte((byte)skillindex);
                pak.WriteByte((byte)specs.Count);

                foreach (ChampSpec spec in specs)
                {
                    pak.WriteByte((byte)spec.Index);
                    Spell spell = SkillBase.GetSpellByID(spec.SpellID);
                    if (spell.SpellType == "StyleHandler")
                    {
                        pak.WriteByte(1);
                    }
                    else { pak.WriteByte(3); }
                    pak.WriteShortLowEndian(spell.Icon);
                    pak.WritePascalString(spell.Name);
                    if(m_gameClient.Player.HaveChampionSpell(spec.SpellID))
                    	pak.WriteByte((byte)1);
                    else if(m_gameClient.Player.IsCSAvailable(type, skillindex, spec.Index))
                    	pak.WriteByte((byte)2);
                    else
                    	pak.WriteByte((byte)0);
                    pak.WriteByte(0);
                }
            }
            SendTCP(pak);               
        }

		public virtual void SendTrainerWindow()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TrainerWindow));
			IList specs = m_gameClient.Player.GetSpecList();
			pak.WriteByte((byte)specs.Count);
			pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
			pak.WriteByte((byte)0);
			pak.WriteByte((byte)0);

			int i = 0;
			foreach (Specialization spec in specs)
			{
				pak.WriteByte((byte)i++);
				pak.WriteByte((byte)spec.Level);
				pak.WriteByte((byte)(spec.Level + 1));
				pak.WritePascalString(spec.Name);
			}
			SendTCP(pak);

			// realm abilities
			IList raList = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID);
			if (raList != null && raList.Count > 0)
			{

				ArrayList offeredRA = new ArrayList();
				foreach (RealmAbility ra in raList)
				{
					RealmAbility playerRA = (RealmAbility)m_gameClient.Player.GetAbility(ra.KeyName);
					if (playerRA != null)
					{
						if (playerRA.Level < playerRA.MaxLevel)
						{
							RealmAbility ab = SkillBase.GetAbility(playerRA.KeyName, playerRA.Level + 1) as RealmAbility;
							if (ab != null)
							{
								offeredRA.Add(ab);
							}
							else
							{
								log.Error("Ability " + ab.Name + " not found unexpectly");
							}
						}
					}
					else
					{
						if (ra.Level < ra.MaxLevel)
						{
							offeredRA.Add(ra);
						}
					}
				}

				pak = new GSTCPPacketOut(GetPacketCode(ePackets.TrainerWindow));
				pak.WriteByte((byte)offeredRA.Count);
				pak.WriteByte((byte)m_gameClient.Player.RealmSpecialtyPoints);
				pak.WriteByte((byte)1);
				pak.WriteByte((byte)0);

				i = 0;
				foreach (RealmAbility ra in offeredRA)
				{
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)ra.Level);
					pak.WriteByte((byte)ra.CostForUpgrade(ra.Level - 1));
					bool canBeUsed = ra.CheckRequirement(m_gameClient.Player);
					pak.WritePascalString((canBeUsed ? "" : "[") + ra.Name + (canBeUsed ? "" : "]"));
				}

				m_gameClient.Player.TempProperties.setProperty("OFFERED_RA", offeredRA);
				SendTCP(pak);
			}
		}

		public virtual void SendInterruptAnimation(GameLiving living)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InterruptSpellCast));
			pak.WriteShort((ushort)living.ObjectID);
			pak.WriteShort((ushort)1);
			SendTCP(pak);
		}

		public virtual void SendDisableSkill(Skill skill, int duration)
		{
			if (m_gameClient.Player == null)
				return;

			if (skill.SkillType == eSkillPage.Abilities || skill.SkillType == eSkillPage.RealmAbilities)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DisableSkills));
				pak.WriteShort((ushort)duration);
				int id = -1;
				IList skillList = m_gameClient.Player.GetNonTrainableSkillList();
				lock (skillList.SyncRoot)
				{
					foreach (Skill skl in skillList)
					{
						if (skl.SkillType == eSkillPage.Abilities || skl.SkillType == eSkillPage.RealmAbilities)
							id++;
						if (skl == skill)
							break;
					}
				}
				if (id < 0)
					return;
				pak.WriteByte((byte)id);
				pak.WriteByte(0); // not used?

				SendTCP(pak);
			}
			if (skill.SkillType == eSkillPage.Spells)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DisableSkills));
				if (m_gameClient.Player.CharacterClass.ClassType == eClassType.ListCaster)
				{
					pak.WriteShort((ushort)duration);
					pak.WriteByte(1); // count of spells
					pak.WriteByte(2); // code

					IList lines = m_gameClient.Player.GetSpellLines();
					lock (lines.SyncRoot)
					{
						int lineIndex = -1;
						int spellIndex = -1;
						bool found = false;
						foreach (SpellLine line in lines)
						{
							IList spells = SkillBase.GetSpellList(line.KeyName);
							spellIndex = 0;
							foreach (Spell spell in spells)
							{
								if (spell == skill)
								{
									found = true;
									//DOLConsole.LogLine("disable spell "+skill.Name+" in line "+line.Name);
									break;
								}
								spellIndex++;
							}
							lineIndex++;
							if (found)
								break;
						}
						if (!found)
							return;

						pak.WriteByte((byte)lineIndex);
						pak.WriteByte((byte)spellIndex);
					}
					SendTCP(pak);
				}
				else
				{
					int skillsCount = m_gameClient.Player.GetNonTrainableSkillList().Count + m_gameClient.Player.GetStyleList().Count;
					IList lines = m_gameClient.Player.GetSpellLines();
					int index = -1;
					lock (lines.SyncRoot)
					{
						int searchIndex = 0;
						foreach (SpellLine line in lines)
						{
							IList spells = m_gameClient.Player.GetUsableSpellsOfLine(line);
							foreach (Spell spell in spells)
							{
								if (spell == skill)
								{
									index = searchIndex;
									//DOLConsole.LogLine("disable spell "+skill.Name+" in line "+line.Name);
									break;
								}
								searchIndex++;
							}
							if (index >= 0)
								break;
						}
					}
					if (index < 0)
						return;
					pak.WriteShort(0);
					pak.WriteByte(1); // count of skills
					pak.WriteByte(1); // code
					pak.WriteShort((ushort)(index + skillsCount));
					pak.WriteShort((ushort)duration);
					SendTCP(pak);
				}
			}
		}

		public virtual void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			byte fxcount = 0;
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.UpdateIcons));

			lock (m_gameClient.Player.EffectList)
			{
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
						fxcount++;
				}

				pak.WriteByte(fxcount);
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown
				byte i = 0;
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						pak.WriteByte((effect is GameSpellEffect || effect.Icon > 5000) ? i++ : (byte)0xff);
						pak.WriteByte(0);
						pak.WriteShort(effect.Icon);
						pak.WriteShort((ushort)(effect.RemainingTime / 1000));
						pak.WriteShort(effect.InternalID); // reference for shift+i or cancel spell
						pak.WritePascalString(effect.Name);
					}
				}
			}
			SendTCP(pak);
		}

		public virtual void SendLevelUpSound()
		{
			// not sure what package this is, but it triggers the mob color update
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionSound));
			pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
			pak.WriteByte(1); //level up sounds
			pak.WriteByte((byte)m_gameClient.Player.Realm);
			SendTCP(pak);
		}

		public virtual void SendRegionEnterSound(byte soundId)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionSound));
			pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
			pak.WriteByte(2); //region enter sounds
			pak.WriteByte(soundId);
			SendTCP(pak);
		}

		public virtual void SendDebugMessage(string format, params object[] parameters)
		{
			try
			{
				SendMessage(String.Format("[DEBUG] " + format, parameters), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch
			{
				SendMessage("[DEBUG] Formatting the Debug Message: " + format + " caused an exception!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public virtual void SendDebugPopupMessage(string format, params object[] parameters)
		{
			try
			{
				SendMessage(String.Format("[DEBUG] " + format, parameters), eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
			catch
			{
				SendMessage("[DEBUG] Formatting the Debug Message: " + format + " caused an exception!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public virtual void SendEmblemDialogue()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EmblemDialogue));
			pak.Fill(0x00, 4);
			SendTCP(pak);
		}

		//FOR GM to test param and see min and max of each param
		public virtual void SendWeather(uint x, uint width, ushort speed, ushort fogdiffusion, ushort intensity)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Weather));

			pak.WriteInt(x);
			pak.WriteInt(width);
			pak.WriteShort(fogdiffusion);
			pak.WriteShort(speed);
			pak.WriteShort(intensity);
			pak.WriteShort(0); // 0x0508, 0xEB51, 0xFFBF
			SendTCP(pak);
		}

		public virtual void SendPlayerModelTypeChange(GamePlayer player, byte modelType)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerModelTypeChange));
			pak.WriteShort((ushort)player.ObjectID);
			pak.WriteByte(modelType);
			pak.WriteByte((byte)(modelType == 3 ? 0x08 : 0x00)); //unused?
			SendTCP(pak);
		}

		public virtual void SendObjectDelete(GameObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectDelete));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort(1); //TODO: unknown
			SendTCP(pak);
		}

		public virtual void SendConcentrationList()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ConcentrationList));
			lock (m_gameClient.Player.ConcentrationEffects)
			{
				pak.WriteByte((byte)(m_gameClient.Player.ConcentrationEffects.Count));
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown

				for (int i = 0; i < m_gameClient.Player.ConcentrationEffects.Count; i++)
				{
					IConcentrationEffect effect = m_gameClient.Player.ConcentrationEffects[i];
					pak.WriteByte((byte)i);
					pak.WriteByte(0); // unknown
					pak.WriteByte(effect.Concentration);
					pak.WriteShort(effect.Icon);
					pak.WritePascalString(effect.Name);
					pak.WritePascalString(effect.OwnerName);
				}
			}
			SendTCP(pak);
			SendStatusUpdate(); // send status update for convinience, mostly the conc has changed
		}

		public void SendChangeTarget(GameObject newTarget)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ChangeTarget));
			pak.WriteShort((ushort)(newTarget == null ? 0 : newTarget.ObjectID));
			pak.WriteShort(0); // unknown
			SendTCP(pak);
		}

		public void SendChangeGroundTarget(Point3D newTarget)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ChangeGroundTarget));
			pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.X));
			pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.Y));
			pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.Z));
			SendTCP(pak);
		}

		public virtual void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PetWindow));
			pak.WriteShort((ushort)(pet == null ? 0 : pet.ObjectID));
			pak.WriteByte(0x00); //unused
			pak.WriteByte(0x00); //unused
			switch (windowAction) //0-released, 1-normal, 2-just charmed? | Roach: 0-close window, 1-update window, 2-create window
			{
				case ePetWindowAction.Open: pak.WriteByte(2); break;
				case ePetWindowAction.Update: pak.WriteByte(1); break;
				default: pak.WriteByte(0); break;
			}
			switch (aggroState) //1-aggressive, 2-defensive, 3-passive
			{
				case eAggressionState.Aggressive: pak.WriteByte(1); break;
				case eAggressionState.Defensive: pak.WriteByte(2); break;
				case eAggressionState.Passive: pak.WriteByte(3); break;
				default: pak.WriteByte(0); break;
			}
			switch (walkState) //1-follow, 2-stay, 3-goto, 4-here
			{
				case eWalkState.Follow: pak.WriteByte(1); break;
				case eWalkState.Stay: pak.WriteByte(2); break;
				case eWalkState.GoTarget: pak.WriteByte(3); break;
				case eWalkState.ComeHere: pak.WriteByte(4); break;
				default: pak.WriteByte(0); break;
			}
			pak.WriteByte(0x00); //unused

			if (pet != null)
			{
				lock (pet.EffectList)
				{
					ArrayList icons = new ArrayList();
					foreach (IGameEffect effect in pet.EffectList)
					{
						if (effect.Icon == 0)
							continue;
						icons.Add(effect.Icon);
					}
					pak.WriteByte((byte)icons.Count); // effect count
					// 0x08 - null terminated - (byte) list of shorts - spell icons on pet
					foreach (ushort icon in icons)
					{
						if (icons.IndexOf(icon) >= 8)
							break;
						pak.WriteShort(icon);
					}
				}
			}

			pak.WriteByte(0x00); //null termination

			SendTCP(pak);
		}

		public virtual void SendKeepInfo(AbstractGameKeep keep)
		{
		}

		public virtual void SendKeepComponentInfo(GameKeepComponent keepComponent)
		{
		}
		public virtual void SendKeepComponentDetailUpdate(GameKeepComponent keepComponent)
		{
		}
		public virtual void SendWarmapUpdate(ICollection<AbstractGameKeep> list)
		{
		}
		public virtual void SendWarmapBonuses()
		{
		}

		public virtual void SendWarmapDetailUpdate(List<List<byte>> fights, List<List<byte>> groups)
		{
		}

		//housing
		public virtual void SendHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseCreate));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)house.Z);
			pak.WriteInt((uint)house.X);
			pak.WriteInt((uint)house.Y);
			pak.WriteShort((ushort)house.Heading);
			pak.WriteShort((ushort)house.PorchRoofColor);
			pak.WriteShort((ushort)house.GetPorchAndGuildEmblemFlags());
			pak.WriteShort((ushort)house.Emblem);
			pak.WriteByte((byte)house.Model);
			pak.WriteByte((byte)house.RoofMaterial);
			pak.WriteByte((byte)house.WallMaterial);
			pak.WriteByte((byte)house.DoorMaterial);
			pak.WriteByte((byte)house.TrussMaterial);
			pak.WriteByte((byte)house.PorchMaterial);
			pak.WriteByte((byte)house.WindowMaterial);
			pak.WriteByte(0x03);
			pak.WritePascalString(house.Name);

			SendTCP(pak);
		}

		public virtual void SendRemoveHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseCreate));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)house.Z);
			pak.WriteInt((uint)house.X);
			pak.WriteInt((uint)house.Y);
			pak.Fill(0x00, 15);
			pak.WriteByte(0x03);
			pak.WritePascalString("");

			SendTCP(pak);
		}

		public virtual void SendHousePermissions(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.HousingPersmissions);
			pak.WriteByte(10); // number of permissions ?
			pak.WriteByte(0x00); // ??
			pak.WriteShort((ushort)house.HouseNumber);
			byte i;
			for (i = 0; i < 10; i++)
			{
				pak.WriteByte(i);
				pak.WriteByte(house.HouseAccess[i].Enter);
				pak.WriteByte(house.HouseAccess[i].Vault1);
				pak.WriteByte(house.HouseAccess[i].Vault2);
				pak.WriteByte(house.HouseAccess[i].Vault3);
				pak.WriteByte(house.HouseAccess[i].Vault4);
				pak.WriteByte(house.HouseAccess[i].Appearance);
				pak.WriteByte(house.HouseAccess[i].Interior);
				pak.WriteByte(house.HouseAccess[i].Garden);
				pak.WriteByte(house.HouseAccess[i].Banish);
				pak.WriteByte(house.HouseAccess[i].UseMerchant);
				pak.WriteByte(house.HouseAccess[i].Tools);
				pak.WriteByte(house.HouseAccess[i].Bind);
				pak.WriteByte(house.HouseAccess[i].Merchant);
				pak.WriteByte(house.HouseAccess[i].PayRent);
				pak.WriteByte(0x00); // ??
			}
			SendTCP(pak);
		}

		public virtual void SendHousePayRentDialog(string title)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte((byte)eDialogCode.HousePayRent);
			pak.Fill(0x00, 8); // empty
			pak.WriteByte(0x02); // type
			pak.WriteByte(0x01); // wrap
			pak.WriteString(title); // title ??
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendGarden(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte((byte)house.OutdoorItems.Count);
			pak.WriteByte(0x80);
			foreach (DictionaryEntry entry in new SortedList(house.OutdoorItems))
			{
				OutdoorItem item = (OutdoorItem)entry.Value;
				pak.WriteByte((byte)((int)entry.Key));
				pak.WriteShort((ushort)item.Model);
				pak.WriteByte((byte)item.Position);
				pak.WriteByte((byte)item.Rotation);
			}
			SendTCP(pak);
		}

		public virtual void SendGarden(House house, int i)
		{
		}

		public virtual void SendRemoveGarden(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));

			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(0x00);
			pak.WriteByte(0x01); //dont know why 0x01 here?!

			SendTCP(pak);
		}

		public virtual void SendEnterHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseEnter));

			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)25000);         //constant!
			pak.WriteInt((uint)house.X);
			pak.WriteInt((uint)house.Y);
			pak.WriteShort((ushort)house.Heading); //useless/ignored by client.
			pak.WriteByte(0x00);
			pak.WriteByte((byte)house.GetGuildEmblemFlags()); //emblem style
			pak.WriteShort((ushort)house.Emblem);	//emblem
			pak.WriteByte(0x00);
			pak.WriteByte(0x00);
			pak.WriteByte((byte)house.Model);
			pak.WriteByte(0x00);
			pak.WriteByte(0x00);
			pak.WriteByte(0x00);
			pak.WriteByte((byte)house.Rug1Color);
			pak.WriteByte((byte)house.Rug2Color);
			pak.WriteByte((byte)house.Rug3Color);
			pak.WriteByte((byte)house.Rug4Color);
			pak.WriteByte(0x00);

			SendTCP(pak);
		}

		public virtual void SendToggleHousePoints(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseTogglePoints));
			
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(0x04);
			pak.WriteByte(0x00);

			SendTCP(pak);
		}

		public virtual void SendFurniture(House house)
		{

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HousingItem));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(Convert.ToByte(house.IndoorItems.Count));
			pak.WriteByte(0x80); //0x00 = update, 0x80 = complete package
			foreach (DictionaryEntry entry in new SortedList(house.IndoorItems))
			{
				IndoorItem item = (IndoorItem)entry.Value;
				pak.WriteByte((byte)((int)entry.Key));
				pak.WriteShort((ushort)item.Model);
				pak.WriteShort((ushort)item.Color);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				pak.WriteShort((ushort)item.X);
				pak.WriteShort((ushort)item.Y);
				pak.WriteShort((ushort)item.Rotation);
				pak.WriteByte((byte)item.Size);
				pak.WriteByte((byte)item.Position);
				pak.WriteByte((byte)(item.Placemode - 2));
			}
			SendTCP(pak);
		}

		public virtual void SendFurniture(House house, int i)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HousingItem));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(0x01); //cnt
			pak.WriteByte(0x00); //upd
			IndoorItem item = (IndoorItem)house.IndoorItems[i];
			pak.WriteByte((byte)i);
			pak.WriteShort((ushort)item.Model);
			pak.WriteShort((ushort)item.Color);
			pak.WriteByte(0x00);
			pak.WriteByte(0x00);
			pak.WriteShort((ushort)item.X);
			pak.WriteShort((ushort)item.Y);
			pak.WriteShort((ushort)item.Rotation);
			pak.WriteByte((byte)item.Size);
			pak.WriteByte((byte)item.Position);
			pak.WriteByte((byte)(item.Placemode - 2));
			SendTCP(pak);
		}

		public virtual void SendRentReminder(House house)
		{
			//0:00:58.047 S=>C 0xF7 show help window (topicIndex:106 houseLot?:4281)
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HelpWindow));
			pak.WriteShort(106);//short index
			pak.WriteShort((ushort)house.HouseNumber); //short lot
			SendTCP(pak);
		}

		public virtual void SendStarterHelp()
		{
			//* 0:00:57.984 S=>C 0xF7 show help window (topicIndex:1 houseLot?:0)
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HelpWindow));
			pak.WriteShort(1);//short index
			pak.WriteShort(0); //short lot
			SendTCP(pak);
		}

		public virtual void SendPlaySound(eSoundType soundType, ushort soundID)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlaySound));
			pak.WriteShort((ushort)soundType);
			pak.WriteShort(soundID);
			pak.Fill(0x00, 8);
			SendTCP(pak);
		}

		public virtual void SendKeepClaim(AbstractGameKeep keep, byte flag)
		{ }

		public virtual void SendKeepComponentUpdate(AbstractGameKeep keep, bool LevelUp)
		{ }

		public virtual void SendKeepComponentInteract(GameKeepComponent component)
		{ }

		public virtual void SendKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex)
		{ }

		public virtual void SendClearKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex)
		{ }

		public virtual void SendHookPointStore(GameKeepHookPoint hookPoint)
		{ }

		public virtual void SendMovingObjectCreate(GameMovingObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MovingObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort(0);
			pak.WriteShort(obj.Heading);
			pak.WriteShort((ushort)obj.Z);
			pak.WriteInt((uint)obj.X);
			pak.WriteInt((uint)obj.Y);
			pak.WriteShort(obj.Model);
			int flag = (obj.Type() | ((byte)obj.Realm == 3 ? 0x40 : (byte)obj.Realm << 4) | obj.Level << 9);
			pak.WriteShort((ushort)flag); //(0x0002-for Ship,0x7D42-for catapult,0x9602,0x9612,0x9622-for ballista)
			pak.WriteShort(obj.Emblem);//emblem
			pak.WriteShort(0);
			pak.WriteInt(0);
			pak.WritePascalString(obj.Name);
			pak.WriteByte(0); // trailing ?
			SendTCP(pak);
		}

		public virtual void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon, int time)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SiegeWeaponInterface));
			ushort flag = (ushort)((siegeWeapon.EnableToMove ? 1 : 0) | siegeWeapon.AmmoType << 8);
			pak.WriteShort(flag); //byte Ammo,  byte SiegeMoving(1/0)
			pak.WriteByte(0);
			pak.WriteByte(0); // Close interface(1/0)
			pak.WriteByte((byte)(time / 10));//time in 1000ms
			pak.WriteByte((byte)siegeWeapon.Ammo.Count); // external ammo count
			pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
			pak.WriteByte((byte)siegeWeapon.AmmoSlot);
			pak.WriteShort(siegeWeapon.Effect);
			pak.WriteShort((ushort)time); // time (?)
			pak.WriteInt((uint)siegeWeapon.ObjectID);

			pak.WritePascalString(siegeWeapon.Name + " (" + siegeWeapon.CurrentState.ToString() + ")");
			foreach (InventoryItem item in siegeWeapon.Ammo)
			{
				pak.WriteByte((byte)item.SlotPosition);
				if (item == null)
				{
					pak.Fill(0x00, 18);
					continue;
				}
				pak.WriteByte((byte)item.Level);
				pak.WriteByte((byte)item.DPS_AF);
				pak.WriteByte((byte)item.SPD_ABS);
				pak.WriteByte((byte)(item.Hand * 64));
				pak.WriteByte((byte)((item.Type_Damage * 64) + item.Object_Type));
				pak.WriteShort((ushort)item.Weight);
				pak.WriteByte(item.ConditionPercent); // % of con
				pak.WriteByte(item.DurabilityPercent); // % of dur
				pak.WriteByte((byte)item.Quality); // % of qua
				pak.WriteByte((byte)item.Bonus); // % bonus
				pak.WriteShort((ushort)item.Model);
				if (item.Emblem != 0)
					pak.WriteShort((ushort)item.Emblem);
				else
					pak.WriteShort((ushort)item.Color);
				pak.WriteShort((ushort)item.Effect);
				if (item.Count > 1)
					pak.WritePascalString(item.Count + " " + item.Name);
				else
					pak.WritePascalString(item.Name);
			}
			SendTCP(pak);
		}

		public virtual void SendSiegeWeaponCloseInterface()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SiegeWeaponInterface));
			pak.WriteShort(0);
			pak.WriteShort(1);
			pak.Fill(0, 13);
			SendTCP(pak);
		}

		public virtual void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon)
		{
			if (siegeWeapon == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SiegeWeaponAnimation));
			pak.WriteInt((uint)siegeWeapon.ObjectID);
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.X) : siegeWeapon.TargetObject.X));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.Y) : siegeWeapon.TargetObject.Y));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.Z) : siegeWeapon.TargetObject.Z));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
			pak.WriteShort(siegeWeapon.Effect);
			pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.TimeUntilElapsed / 100));
			pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
			pak.Fill(0, 3);
			SendTCP(pak);
		}

		public virtual void SendSiegeWeaponFireAnimation(GameSiegeWeapon siegeWeapon, int timer)
		{
			if (siegeWeapon == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SiegeWeaponAnimation));
			pak.WriteInt((uint)siegeWeapon.ObjectID);
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.X));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.Y));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.Z + 50));
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
			pak.WriteShort(siegeWeapon.Effect);
			pak.WriteShort((ushort)(timer / 100));
			pak.WriteByte((byte)SiegeTimer.eAction.Fire);
			pak.WriteByte((byte)0xAA);
			pak.WriteShort(0xFFBF);
			SendTCP(pak);
		}

		public virtual void SendNPCsQuestEffect(GameNPC npc, bool flag)
		{ }

		public virtual void SendHexEffect(GamePlayer player, byte effect1, byte effect2, byte effect3, byte effect4, byte effect5)
		{ }

		public virtual void SendLivingDataUpdate(GameLiving living, bool updateStrings)
		{ }

		public virtual void SendSoundEffect(ushort soundId, ushort zoneId, ushort x, ushort y, ushort z, ushort radius)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SoundEffect));
			pak.WriteShort(soundId);
			pak.WriteShort(zoneId);
			pak.WriteShort(x);
			pak.WriteShort(y);
			pak.WriteShort(z);
			pak.WriteShort(radius);
			SendTCP(pak);
		}
		public virtual void SendSetControlledHorse(GamePlayer player)
		{
		}
		public virtual void SendControlledHorse(GamePlayer player, bool flag)
		{
		}

		public virtual void SendCrash(string str)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)0x86);
			pak.WriteByte((byte)0xFF);
			pak.WritePascalString(str);
			SendTCP(pak);
		}

		public virtual void SendRvRGuildBanner(GamePlayer player, bool show)
		{ }

		public virtual void SendPlayerFreeLevelUpdate()
		{ }

		public virtual void SendXFireInfo(byte flag)
		{ }

		public virtual void SendMarketExplorerWindow()
		{
			/*
			 * 0:01:18.360 S=>C 0x1F market merchant (count:-1 page:0 pages:0 unk1:0x00)
			 * itemCount = byte
			 * page = byte
			 * maxPages = byte
			 * unk1 = byte
			 * 
			 * if (itemCount > 0)
			 * for each item
			 * item.index = byte
			 * item.level = ReadByte();
			 * item.value1 = ReadByte(); // DPS_AF
			 * item.value2 = ReadByte(); // SPD_ABS
			 * item.hand = ReadByte();
			 * item.damageAndObjectType = ReadByte(); // Hand + ObjectType ?
			 * item.canUse = ReadByte();
			 * item.weight = ReadShort();
			 * item.condition = ReadByte();
			 * item.durability = ReadByte();
			 * item.quality = ReadByte();
			 * item.bonus = ReadByte();
			 * item.model = ReadShort();
			 * item.color = ReadShort();
			 * item.effect = ReadShort();
			 * item.lot = ReadShort();
			 * item.price = ReadInt();
			 * item.name = ReadPascalString(); // max length = 0x2F ?
			 */
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.MarketExplorerWindow);
			pak.WriteByte(255);
			pak.Fill(0, 3);
			SendTCP(pak);
		}

		public virtual void SendMasterLevelWindow(byte ml)
		{
			// If required ML=0 then send current player ML data
			byte mlrequired = (ml==(byte)0 ? ((byte)m_gameClient.Player.MLLevel == (byte)0 ? (byte)1 : (byte)m_gameClient.Player.MLLevel) : ml);

			string description = "";
			double MLXPpercent = 0;
			
			if (m_gameClient.Player.MLLevel < 10)
				MLXPpercent = 100.0 * (double)m_gameClient.Player.MLExperience / (double)m_gameClient.Player.GetMLExperienceForLevel((int)(m_gameClient.Player.MLLevel+1));
			else MLXPpercent = 100.0; // ML10 has no MLXP, so always 100%
					
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.MasterLevelWindow);
			pak.WriteByte((byte)MLXPpercent); // MLXP (displayed in window)
			pak.WriteByte((byte)0x64);
			pak.WriteByte((byte)(m_gameClient.Player.MLLevel+1)); // ML level + 1
			pak.WriteByte((byte)0x00);
			pak.WriteByte(ml); // Required ML	
			if (mlrequired<10)
			{
				// ML level completition is displayed client side (Step 11)
				for (int i=1; i<11; i++)
				{
					if (!m_gameClient.Player.HasFinishedMLStep((int)mlrequired, i))
						description = i.ToString() + ". " + LanguageMgr.GetTranslation(m_gameClient, String.Format("SendMasterLevelWindow.Uncomplete.ML{0}.Step{1}",mlrequired,i));
					else
						description = i.ToString() + ". " + LanguageMgr.GetTranslation(m_gameClient, String.Format("SendMasterLevelWindow.Complete.ML{0}.Step{1}",mlrequired,i));						
					pak.WritePascalString(description);
				}
			}
			else pak.WriteByte((byte)0x00);
				
			pak.WriteByte((byte)0x00);
			SendTCP(pak);
		}
		
		public virtual void SendConsignmentMerchantMoney(ushort mithril, ushort plat, ushort gold, byte silver, byte copper)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.ConsignmentMerchantMoney);
			pak.WriteByte(copper);
			pak.WriteByte(silver);
			pak.WriteShort(gold);
			pak.WriteShort(mithril);
			pak.WriteShort(plat);
			SendTCP(pak);
		}

		/// <summary>
		/// The bow prepare animation
		/// </summary>
		public virtual int BowPrepare
		{
			get { return 0x01F4; }
		}

		/// <summary>
		/// The bow shoot animation
		/// </summary>
		public virtual int BowShoot
		{
			get { return 0x1F7; }
		}
	}
}
