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
using System.Net;
using System.Reflection;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using DOL.GS.Spells;
using DOL.GS.Styles;
using NHibernate.Expression;
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
		public PacketLib168(GameClient client) : base(client)
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
			int version = (int) m_gameClient.Version;
			pak.WriteByte((byte) (version/100));
			pak.WriteByte((byte) ((version%100)/10));
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

		public virtual void SendLoginDenied(eLoginError et)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.LoginDenied));
			pak.WriteByte((byte) et); // Error Code
			/*
			if(is_si)
				pak.WriteByte(0x32);
			else
				pak.WriteByte(0x31);
			*/
			pak.WriteByte(0x01);
			int version = (int) m_gameClient.Version;
			pak.WriteByte((byte) (version/100));
			pak.WriteByte((byte) ((version%100)/10));
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
			int version = (int) m_gameClient.Version;
			pak.WriteByte((byte) (version/100));
			pak.WriteByte((byte) ((version%100)/10));
			//pak.WriteByte(build);
			pak.WriteByte(0x00);
			pak.WritePascalString(m_gameClient.Account.AccountName);
			pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
			pak.WriteByte(0x0C); //Server ID
			pak.WriteByte(GameServer.ServerRules.GetColorHandling(m_gameClient));
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendSessionID()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SessionID));
			pak.WriteShortLowEndian((ushort) m_gameClient.SessionID);
			SendTCP(pak);
		}

		public virtual void SendPingReply(ulong timestamp, ushort sequence)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PingReply));
			pak.WriteInt((uint) timestamp);
			pak.Fill(0x00, 4);
			pak.WriteShort((ushort) (sequence + 1));
			pak.Fill(0x00, 6);
			SendTCP(pak);
		}

		public virtual void SendRealm(eRealm realm)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Realm));

			pak.WriteByte((byte) realm);
			SendTCP(pak);
		}

		public virtual void SendCharacterOverview(eRealm realm)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterOverview));
			pak.FillString(m_gameClient.Account.AccountName, 24);
			
			m_gameClient.Account.CharactersInSelectedRealm = GameServer.Database.SelectObjects(typeof(GamePlayer), Expression.And(Expression.Eq("AccountID", m_gameClient.Account.AccountID),Expression.Eq("Realm", (byte)realm)), Order.Asc("SlotPosition"));
			if (m_gameClient.Account.CharactersInSelectedRealm.Count < 1)
			{
				pak.Fill(0x0, 1848);
			}
			else
			{
				GamePlayer currentChar = null;
				IEnumerator iter = m_gameClient.Account.CharactersInSelectedRealm.GetEnumerator();
				if(iter.MoveNext()) currentChar = (GamePlayer) iter.Current;
				
				for(int i = 0 ; i < 8 ; i++)
				{
					if(currentChar != null && currentChar.SlotPosition == i)
					{
						pak.FillString(currentChar.Name, 24);
						pak.Fill(0x0, 24); //0 String

						Region reg = currentChar.Region;
						Zone zon = null;
						if (reg != null)
							zon = reg.GetZone(currentChar.Position);
						if (zon != null)
							pak.FillString(zon.Description, 24);
						else
							pak.Fill(0x0, 24); //No known location

						pak.FillString("", 24); //Class name

						pak.FillString(GlobalConstants.RaceToName((eRace)currentChar.Race), 24);
						pak.WriteByte((byte) currentChar.Level);
						pak.WriteByte((byte) currentChar.CharacterClassID);
						pak.WriteByte((byte) currentChar.Realm);
						pak.WriteByte((byte)(((((byte)currentChar.Race & 0xF0) << 2)+((byte)currentChar.Race & 0x0F)) | (currentChar.Gender << 4)));
						pak.WriteShortLowEndian((ushort) currentChar.Model);
						pak.WriteByte((byte) currentChar.Region.RegionID);
						pak.WriteByte(0x0); //second byte of region, currently unused
						pak.WriteInt(0x0); //Unknown, last used?
						pak.WriteByte((byte) currentChar.BaseStrength);
						pak.WriteByte((byte) currentChar.BaseDexterity);
						pak.WriteByte((byte) currentChar.BaseConstitution);
						pak.WriteByte((byte) currentChar.BaseQuickness);
						pak.WriteByte((byte) currentChar.BaseIntelligence);
						pak.WriteByte((byte) currentChar.BasePiety);
						pak.WriteByte((byte) currentChar.BaseEmpathy);
						pak.WriteByte((byte) currentChar.BaseCharisma);
			
						VisibleEquipment currentItem;
						for(int slot = (int)eInventorySlot.HeadArmor ; slot <= (int)eInventorySlot.ArmsArmor ; slot++)
						{
							currentItem = (VisibleEquipment)currentChar.Inventory.GetItem((eInventorySlot)slot);
							if(currentItem == null)
							{
								pak.WriteShort(0x00);
							}
							else
							{
								pak.WriteShortLowEndian((ushort) currentItem.Model); // all armors models
							}
						}

						for(int slot = (int)eInventorySlot.HeadArmor ; slot <= (int)eInventorySlot.ArmsArmor ; slot++)
						{
							if(slot == (int)eInventorySlot.Jewellery) currentItem = (VisibleEquipment)currentChar.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
							else currentItem = (VisibleEquipment)currentChar.Inventory.GetItem((eInventorySlot)slot);
						
							if(currentItem == null)
							{
								pak.WriteShort(0x00);
							}
							else
							{
								pak.WriteShortLowEndian((ushort) currentItem.Color); // all armors color (+shield)
							}
						}

						for(int slot = (int)eInventorySlot.RightHandWeapon ; slot <= (int)eInventorySlot.DistanceWeapon ; slot++)
						{
							currentItem = (VisibleEquipment)currentChar.Inventory.GetItem((eInventorySlot)slot);
							if(currentItem == null)
							{
								pak.WriteShort(0x00);
							}
							else
							{
								pak.WriteShortLowEndian((ushort) currentItem.Model); // all weapon models
							}
						}
					
						if (currentChar.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.TwoHanded)
						{
							pak.WriteByte(0x02);
							pak.WriteByte(0x02);
						}
						else if (currentChar.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
						{
							pak.WriteByte(0x03);
							pak.WriteByte(0x03);
						}
						else
						{
							if(currentChar.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null) pak.WriteByte(0x00); else pak.WriteByte(0xFF);
							if(currentChar.Inventory.GetItem(eInventorySlot.LeftHandWeapon) != null) pak.WriteByte(0x01); else pak.WriteByte(0xFF);
						}
	
						pak.WriteByte(0x00); //0x01=char in SI zone, classic client can't "play"
						pak.WriteByte(0x00);
					
						currentChar = iter.MoveNext() ? (GamePlayer)iter.Current : null;
					}
					else
					{
						pak.Fill(0x0, 184); // no char in this slot
					}
				}
				pak.Fill(0x0, 0x68); //Don't know why so many trailing 0's | Corillian: Cuz they're stupid like that ;)
			}
			SendTCP(pak);
		}

		public virtual void SendDupNameCheckReply(string name, bool nameExists)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DupNameCheckReply));
			pak.FillString(name, 30);
			pak.FillString(m_gameClient.Account.AccountName, 20);
			pak.WriteByte((byte) (nameExists ? 0x1 : 0x0));
			pak.Fill(0x0, 3);
			SendTCP(pak);
		}

		public virtual void SendBadNameCheckReply(string name, bool bad)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.BadNameCheckReply));
			pak.FillString(name, 30);
			pak.FillString(m_gameClient.Account.AccountName, 20);
			pak.WriteByte((byte) (bad ? 0x0 : 0x1));
			pak.Fill(0x0, 3);
			SendTCP(pak);
		}

		public virtual void SendAttackMode(bool attackState)
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.AttackMode));
			pak.WriteByte((byte) (attackState ? 0x01 : 0x00));
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
			pak.WriteShort((ushort) m_gameClient.Player.BaseStrength);
			pak.WriteShort((ushort) m_gameClient.Player.BaseDexterity);
			pak.WriteShort((ushort) m_gameClient.Player.BaseConstitution);
			pak.WriteShort((ushort) m_gameClient.Player.BaseQuickness);
			pak.WriteShort((ushort) m_gameClient.Player.BaseIntelligence);
			pak.WriteShort((ushort) m_gameClient.Player.BasePiety);
			pak.WriteShort((ushort) m_gameClient.Player.BaseEmpathy);
			pak.WriteShort((ushort) m_gameClient.Player.BaseCharisma);
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Strength) - m_gameClient.Player.BaseStrength));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Dexterity) - m_gameClient.Player.BaseDexterity));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Constitution) - m_gameClient.Player.BaseConstitution));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Quickness) - m_gameClient.Player.BaseQuickness));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Intelligence) - m_gameClient.Player.BaseIntelligence));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Piety) - m_gameClient.Player.BasePiety));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Empathy) - m_gameClient.Player.BaseEmpathy));
			pak.WriteShort((ushort) (m_gameClient.Player.GetModified(eProperty.Charisma) - m_gameClient.Player.BaseCharisma));
			pak.WriteShort((ushort) m_gameClient.Player.MaxHealth);
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
					while (index < count && (entries[index].id > byte.MaxValue || m_gameClient.ClientType <= entries[index].expansion))
					{ //skip high ID regions added with catacombs
						index++;
					}

					if (index >= count)
					{ //If we have no more entries
						pak.Fill(0x0, 52);
					}
					else
					{
						pak.WriteByte((byte) (++num));
						pak.WriteByte((byte) entries[index].id);
						pak.FillString(entries[index].name, 20);
						pak.FillString(entries[index].fromPort, 5);
						pak.FillString(entries[index].toPort, 5);

						//Try to fix the region ip so UDP is enabled!
						string ip = entries[index].ip;
						if(ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
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
			pak.WriteShort((ushort) m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
			Point pos = m_gameClient.Player.Position;
			pak.WriteShort((ushort) pos.Z);
			pak.WriteInt((uint) pos.X);
			pak.WriteInt((uint) pos.Y);
			pak.WriteShort((ushort) m_gameClient.Player.Heading);

			int flags = 0;
			if (m_gameClient.Player.Region.IsDivingEnabled)
				flags = 0x80 | (m_gameClient.Player.IsDiving ? 0x01 : 0x00);
			pak.WriteByte((byte) (flags));

			pak.WriteByte(0x00); //TODO Unknown
			SendTCP(pak);
		}

		public virtual void SendPlayerJump(bool headingOnly)
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterJump));
			Point pos = m_gameClient.Player.Position;
			pak.WriteInt((uint) (headingOnly ? 0 : pos.X));
			pak.WriteInt((uint) (headingOnly ? 0 : pos.Y));
			pak.WriteShort((ushort) m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
			pak.WriteShort((ushort) (headingOnly ? 0 : pos.Z));
			pak.WriteShort((ushort) m_gameClient.Player.Heading);
			//if (m_gameClient.Player.CurrentHouse == null)
			{
				pak.WriteShort(0);
			}
			/*else
			{
				pak.WriteShort((ushort) m_gameClient.Player.CurrentHouse.HouseNumber);
			}*/
			SendTCP(pak);
		}

		public virtual void SendPlayerInitFinished()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterInitFinished));
			pak.WriteByte(0x00); // Mobs sended
			SendTCP(pak);
		}

		public virtual void SendUDPInitReply()
		{
			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.UDPInitReply));
			pak.Fill(0x0, 0x18); // TODO normaly contains the host
			// address, but its unused by the client
			SendUDP(pak);
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
			pak.WriteShort((ushort) m_gameClient.SessionID);
			pak.WriteShort(0x00);
			pak.WriteByte((byte) type);
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
			Region playerRegion = playerToCreate.Region;
			if (playerRegion == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerRegion == null");
				return;
			}
			Zone playerZone = playerToCreate.Region.GetZone(playerToCreate.Position);
			if (playerZone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerZone == null");
				return;
			}
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerCreate));
			pak.WriteShort((ushort) playerToCreate.Client.SessionID);
			pak.WriteShort((ushort) playerToCreate.ObjectID);
			
			Point playerPosition = playerToCreate.Position;
			Point zonePosition = playerZone.ToLocalPosition(playerPosition);
			pak.WriteShort((ushort) zonePosition.X);
			pak.WriteShort((ushort) zonePosition.Y);
			pak.WriteShort((ushort) playerZone.ZoneID);
			pak.WriteShort((ushort) zonePosition.Z);
			pak.WriteShort((ushort) playerToCreate.Heading);
			pak.WriteShort((ushort) playerToCreate.Model);
			//DOLConsole.WriteLine("send created player "+target.Player.Name+" to "+client.Player.Name+" alive="+target.Player.Alive);
			pak.WriteByte((byte) (playerToCreate.Alive ? 0x1 : 0x0));
			pak.WriteByte(0x00);
			pak.WriteByte(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate));
			pak.WriteByte(playerToCreate.Level);
			pak.WriteByte((byte) (playerToCreate.IsStealthed ? 0x01 : 0x00));
			pak.WriteByte(0x00); //Unused (??)
			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			pak.WriteByte(0x00); //Trialing 0 ... needed!
			SendTCP(pak);

			if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
				SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
		}

		public virtual void SendObjectGuildID(GameObject obj, Guild guild)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectGuildID));
			pak.WriteShort((ushort) obj.ObjectID);
			if (guild == null)
			{
				pak.WriteShort(0x00);
				pak.WriteShort(0x00);
			}
			else
			{
				pak.WriteShort((ushort)guild.GuildID);
				pak.WriteShort((ushort)guild.GuildID);
			}
			pak.WriteShort(0x00); //seems random, not used by the client
			SendTCP(pak);
		}

		public virtual void SendPlayerQuit(bool totalOut)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Quit));
			pak.WriteByte((byte) (totalOut ? 0x01 : 0x00));
			if (m_gameClient.Player == null)
				pak.WriteByte(0);
			else				
				pak.WriteByte(m_gameClient.Player.Level);
			SendTCP(pak);
		}

		public virtual void SendRemoveObject(GameObject obj, eRemoveType type)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RemoveObject));
			pak.WriteShort((ushort) obj.ObjectID);
			pak.WriteShort((ushort) type);
			SendTCP(pak);
		}

		public virtual void SendItemCreate(GameObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ItemCreate));
			pak.WriteShort((ushort) obj.ObjectID);
//			if(obj is GameKeepBanner)  // TODO : uncomment when GameKeepBanner are done
//				pak.WriteShort(((GameKeepBanner)obj).Emblem);
//			else
			pak.WriteShort(0);
			pak.WriteShort((ushort)obj.Heading);
			Point pos = obj.Position;
			pak.WriteShort((ushort) pos.Z);
			pak.WriteInt((uint) pos.X);
			pak.WriteInt((uint) pos.Y);
			pak.WriteShort((ushort)obj.Model); // Doors model is 0xFFFF
			pak.WriteByte(0x00); // RVR object type (for keep/boat static siege weapon 0, 0x3E-cauldron, 0x64-trebuchet/palinstone)
			byte flag = 0;
			if(obj is GameInventoryItem)
			{
				int itemRealm = (int)((GameInventoryItem)obj).Item.Realm;
				flag |= (byte)((itemRealm & 3) << 4);
			}
			else
			{
				flag |= (byte)((obj.Realm & 3) << 4); // Realm = 0 for all non RVR doors, non RVR banner, lathe, forge, ..., campfire, grounds portal stone ect ...
			}

			if(obj is GameObjectTimed && ((GameObjectTimed)obj).IsOwner(m_gameClient.Player))
			{
				flag |= 0x04;
			}
			else if (obj is GameStaticItem) // || obj is GameKeepBanner) // all non moving and non targetable objects
			{
				flag |= 0x08;
			}
			
			pak.WriteByte((byte)flag);
			pak.WritePascalString(obj.Name);
			IDoor door = obj as IDoor;
			if(door != null)
			{
				pak.WriteByte(0x04);
				pak.WriteInt((uint) door.DoorID);
			}
			else
			{
				pak.WriteByte(0x0);
			}
			SendTCP(pak);
		}

		public virtual void SendDebugMode(bool on)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DebugMode));
			pak.WriteByte((byte) (on ? 0x01 : 0x00));
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendModelChange(GameObject obj, ushort newModel)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ModelChange));
			pak.WriteShort((ushort) obj.ObjectID);
			pak.WriteShort(newModel);
			pak.WriteInt(0x00);
			SendTCP(pak);
		}

		public virtual void SendEmoteAnimation(GameObject obj, eEmote emote)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EmoteAnimation));
			pak.WriteShort((ushort) obj.ObjectID);
			pak.WriteByte((byte) emote);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendNPCCreate(GameObject obj)
		{
			//if(npc is GameMovingObject) { SendMovingObjectCreate(npc as GameMovingObject); return; }

			IMovingGameObject movingObj = obj as IMovingGameObject;
			GameNPC npcObj = obj as GameNPC;
			GameSteed steedObj = obj as GameSteed;
			MovingGameStaticItem movingStaticObj = obj as MovingGameStaticItem;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.NPCCreate));
			pak.WriteShort((ushort) obj.ObjectID);
			if(movingObj != null)
			{
				pak.WriteShort((ushort) movingObj.CurrentSpeed);
			}
			else
			{
				pak.WriteShort((ushort) 0);
			}
			pak.WriteShort((ushort) obj.Heading);
			Point pos = obj.Position;
			pak.WriteShort((ushort) pos.Z);
			pak.WriteInt((uint) pos.X);
			pak.WriteInt((uint) pos.Y);
			if(movingObj != null)
			{
				//pak.WriteShort((ushort) (obj.ZAddition * 1000.0)); // TODO : Z movment handling ...
				pak.WriteShort((ushort)0);
			}
			else
			{
				pak.WriteShort((ushort)0);
			}
			pak.WriteShort((ushort) obj.Model);

			byte size = 0;
			byte level = 0;
			if(npcObj != null)
			{
				size = npcObj.Size;
				level = npcObj.Level;
			}
			else if(steedObj != null)
			{
				size = steedObj.Size;
				level = steedObj.Level;
			}
			else if(movingStaticObj != null)
			{
				size = movingStaticObj.Size;
				level = movingStaticObj.Level;
			}

			pak.WriteByte(size);
			pak.WriteByte(level);

			byte flags = 0;
			if(npcObj != null)
			{
				flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npcObj) << 6);
				if ((npcObj.Flags & (uint) GameNPC.eFlags.GHOST) != 0) flags |= 0x01;
				if (npcObj.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
				if (npcObj.Brain is PeaceBrain) flags |= 0x10; // not in nearest enemy even if not same realm
				if ((npcObj.Flags & (uint) GameNPC.eFlags.FLYING) != 0) flags |= 0x20;
			
			}
			else if(steedObj != null)
			{
				flags = (byte)(steedObj.Realm << 6);
				flags |= 0x10;
			}
			else if(movingStaticObj != null)
			{
				flags = (byte)(movingStaticObj.Realm << 6);
				flags |= 0x10;
			}

			pak.WriteByte(flags);
			pak.WriteByte(0x20); //TODO this is the default maxstick distance (47 for horse and 32 for mob)

			string add = "";
			if (npcObj != null && m_gameClient.Account.PrivLevel > ePrivLevel.Player)
			{
				if ((npcObj.Flags & (uint) GameNPC.eFlags.CANTTARGET) != 0)
					add += "-DOR"; // indicates DOR flag for GMs
				if ((npcObj.Flags & (uint) GameNPC.eFlags.DONTSHOWNAME) != 0)
					add += "-NON"; // indicates NON flag for GMs
			}

			string name = obj.Name;
			if (name.Length+add.Length+2 > 47) // clients crash with too long names
				name = name.Substring(0, 47-add.Length-2);
			if (add.Length > 0)
				name = string.Format("[{0}]{1}", name, add);

			pak.WritePascalString(name);

			string guildName = "";
			if(npcObj != null)
			{
				guildName = npcObj.GuildName;
				if(guildName.Length > 47) guildName = guildName.Substring(0, 47);
			}

			pak.WritePascalString(guildName);

			pak.WriteByte(0x00);
			SendTCP(pak);

			if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
			{
				if(npcObj != null)
				{
					IControlledBrain brain = npcObj.Brain as IControlledBrain;
					if (brain != null) SendObjectGuildID(npcObj, brain.Owner.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
				}
			}
		}

		public virtual void SendNPCUpdate(GameObject obj)
		{
			IMovingGameObject movingObj = obj as IMovingGameObject;
			GameNPC npcObj = obj as GameNPC;
			GameLiving livingObj = obj as GameLiving;
			GameSteed steedObj = obj as GameSteed;
			MovingGameStaticItem movingStaticObj = obj as MovingGameStaticItem;

			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.NPCUpdate));

			int speed = 0;
			if(movingObj != null)
			{
				speed = movingObj.CurrentSpeed;
				if (speed > 0x0FFF)
				{
					if (log.IsErrorEnabled)
						log.Error("Too high NPC speed. ("+speed+")"+obj.Name);
					speed = 0x0FFF;
				}
				else if (speed < 0)
				{
					if (log.IsErrorEnabled)
						log.Error("NPC speed can't be negative. ("+speed+")"+obj.Name);
					speed = 0;
				}
			}

			pak.WriteShort((ushort) speed);
			pak.WriteShort((ushort) obj.Heading); //|((npc.m_flyTest>>3)<<13)));

			Point currentPosition = obj.Position;
			Zone currentZone = obj.Region.GetZone(currentPosition);
			Point curZonePos = currentZone.ToLocalPosition(currentPosition);
			byte targetZoneID = 0;
			Point targetZonePos = Point.Zero;
			
			if(movingObj != null)
			{
				Point targetPoint = movingObj.TargetPosition;
				if (targetPoint != Point.Zero)
				{
					Zone targetZone = movingObj.Region.GetZone(targetPoint);
					if (targetZone != null)
					{
						targetZonePos = targetZone.ToLocalPosition(targetPoint);
						targetZoneID = (byte) targetZone.ZoneID;
					}
				}
			}

			pak.WriteShort((ushort) curZonePos.X);
			pak.WriteShort((ushort) targetZonePos.X);
			pak.WriteShort((ushort) curZonePos.Y);
			pak.WriteShort((ushort) targetZonePos.Y);
			pak.WriteShort((ushort) curZonePos.Z);
			pak.WriteShort((ushort) targetZonePos.Z);
			pak.WriteShort((ushort) obj.ObjectID);

			if(npcObj != null 
			&& npcObj.AttackState 
			&& npcObj.TargetObject != null 
			&& npcObj.TargetObject.ObjectState == eObjectState.Active
			&& !npcObj.IsTurningDisabled)
			{
				pak.WriteShort((ushort) npcObj.TargetObject.ObjectID);
			}
			else
			{
				pak.WriteShort(0x00);
			}

			if(livingObj != null)
			{
				pak.WriteByte(livingObj.HealthPercent);
			}
			else
			{
				pak.WriteByte(0);
			}

			byte flags = 0;
			if(npcObj != null)
			{
				flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npcObj) << 6);
				if (m_gameClient.Account.PrivLevel == ePrivLevel.Player)
				{
					if ((npcObj.Flags & (uint) GameNPC.eFlags.CANTTARGET) != 0) flags |= 0x01;
					if ((npcObj.Flags & (uint) GameNPC.eFlags.DONTSHOWNAME) != 0) flags |= 0x02;
				}
				if (npcObj.IsDiving) flags |= 0x10;
				if ((npcObj.Flags & (uint) GameNPC.eFlags.FLYING) != 0) flags |= 0x20;
			}
			else if(steedObj != null)
			{
				flags = (byte)(steedObj.Realm << 6);
			}
			else if(movingStaticObj != null)
			{
				flags = (byte)(movingStaticObj.Realm << 6);
				flags |= 0x03;
			}

			pak.WriteByte(flags);
			pak.WriteByte((byte) currentZone.ZoneID);
			pak.WriteByte(targetZoneID);
			SendUDP(pak);

			if(npcObj != null) npcObj.NPCUpdatedCallback();
		}

		public virtual void SendLivingEquipementUpdate(GameLiving living)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EquipmentUpdate));

			pak.WriteShort((ushort) living.ObjectID);
			pak.WriteByte((byte) (living.Inventory != null && living.Inventory.IsCloakHoodUp ? 0x01 : 0x00)); //bit0 is hood up
			pak.WriteByte(living.VisibleActiveWeaponSlots);

			if (living.Inventory != null)
			{
				ICollection items = living.Inventory.VisibleItems;
				pak.WriteByte((byte) items.Count);
				foreach (GenericItemBase item in items)
				{
					pak.WriteByte((byte) item.SlotPosition);
					
					int color = 0;
					int glowEffect = 0;
					if(item is VisibleEquipment)
					{
						color = ((VisibleEquipment)item).Color;
						if (item is Weapon) glowEffect = ((Weapon)item).GlowEffect;
					}
					else if(item is NPCEquipment)
					{
						color = ((NPCEquipment)item).Color;
						if(item is NPCWeapon) glowEffect = ((NPCWeapon)item).GlowEffect;
					}

					ushort model = (ushort) (item.Model & 0x1FFF);
					if ((color & ~0xFF) != 0)
						model |= 0x8000;
					else if ((color & 0xFF) != 0)
						model |= 0x4000;
					if (glowEffect != 0)
						model |= 0x2000;

					pak.WriteShort(model);

					if ((color & ~0xFF) != 0)
						pak.WriteShort((ushort) color);
					else if ((color & 0xFF) != 0)
						pak.WriteByte((byte) color);
					if (glowEffect != 0)
						pak.WriteShort((ushort) glowEffect);
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
			pak.WriteShort((ushort) m_gameClient.Player.Region.RegionID);
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
			pak.WriteShort((ushort) m_gameClient.Player.SkillSpecialtyPoints);
			pak.WriteInt((uint)m_gameClient.Player.BountyPoints);
			pak.WriteShort((ushort) m_gameClient.Player.RealmSpecialtyPoints);
			pak.WriteShort(0);
			SendTCP(pak);
		}

		public virtual void SendUpdateMoney()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MoneyUpdate));
			pak.WriteByte((byte) Money.GetCopper(m_gameClient.Player.Money));
			pak.WriteByte((byte) Money.GetSilver(m_gameClient.Player.Money));
			pak.WriteShort((ushort) Money.GetGold(m_gameClient.Player.Money));
			pak.WriteShort((ushort) Money.GetMithril(m_gameClient.Player.Money));
			pak.WriteShort((ushort) Money.GetPlatinum(m_gameClient.Player.Money));
			SendTCP(pak);
		}

		public virtual void SendUpdateMaxSpeed()
		{
			//Speed is in % not a fixed value!
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MaxSpeed));
			pak.WriteShort((ushort) (m_gameClient.Player.MaxSpeed*100/GamePlayer.PLAYER_BASE_SPEED));
			pak.WriteByte((byte) (m_gameClient.Player.IsTurningDisabled ? 0x01 : 0x00));
			pak.WriteByte(0);
			SendTCP(pak);
		}

		public virtual void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CombatAnimation));
			if (attacker != null)
				pak.WriteShort((ushort) attacker.ObjectID);
			else
				pak.WriteShort(0x00);
			if (defender != null)
				pak.WriteShort((ushort) defender.ObjectID);
			else
				pak.WriteShort(0x00);
			pak.WriteShort(weaponID);
			pak.WriteShort(shieldID);
			pak.WriteByte((byte) style);
			pak.WriteByte(stance);
			if (style > 0xFF)
				pak.WriteByte((byte) (result | 0x80));
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
			pak.WriteShort((byte) (m_gameClient.Player.Alive ? 0x00 : 0x0f)); // 0x0F if dead
			pak.WriteByte((byte) (m_gameClient.Player.Sitting ? 0x02 : 0x00));
			pak.WriteByte(m_gameClient.Player.EndurancePercent);
			pak.WriteByte(m_gameClient.Player.ConcentrationPercent);
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public virtual void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SpellCastAnimation));
			pak.WriteShort((ushort) spellCaster.ObjectID);
			pak.WriteShort(spellID);
			pak.WriteShort(castingTime);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendSpellEffectAnimation(GameLiving spellCaster, GameLiving spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SpellEffectAnimation));
			pak.WriteShort((ushort) spellCaster.ObjectID);
			pak.WriteShort(spellid);
			pak.WriteShort((ushort) (spellTarget == null ? 0 : spellTarget.ObjectID));
			pak.WriteShort(boltTime);
			pak.WriteByte((byte) (noSound ? 1 : 0));
			pak.WriteByte(success);
			pak.WriteShort(0xFFBF);
			SendTCP(pak);
		}

		public virtual void SendRiding(GameObject rider, GameObject steed, bool dismount)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Riding));
			pak.WriteShort((ushort) rider.ObjectID);
			pak.WriteShort((ushort) steed.ObjectID);
			pak.WriteByte((byte) (dismount ? 0x00 : 0x01));
			pak.WriteByte(0x00);
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
				pak.WriteByte((byte) list.Length);
				byte nbleader = 0;
				byte nbsolo = 0x1E;
				foreach (GamePlayer player in list)
				{
					if (player.PlayerGroup != null)
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
					Zone currentZone = player.Region.GetZone(player.Position);
					if (currentZone != null)
						pak.WriteByte((byte) currentZone.ZoneID);
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

		public virtual void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWarpText, string message)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
			pak.WriteByte(0x00);
			pak.WriteByte((byte) code);
			pak.WriteShort((ushort)data1); //data1
			pak.WriteShort((ushort)data2); //data2
			pak.WriteShort((ushort)data3); //data3
			pak.WriteShort((ushort)data4); //data4
			pak.WriteByte((byte) type);
			pak.WriteByte((byte) (autoWarpText == true ? 0x01 : 0x00));
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
			pak.WriteByte((byte) eDialogCode.CustomDialog);
			pak.WriteShort((ushort) m_gameClient.SessionID); //data1
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
			string key = string.Format("LOS C:0x{0} T:0x{1}",Checker.ObjectID,TargetOID);
			CheckLOSResponse old_callback = null;
			lock (m_gameClient.Player.TempProperties)
			{
				old_callback = (CheckLOSResponse)m_gameClient.Player.TempProperties.getObjectProperty(key, null);
				m_gameClient.Player.TempProperties.setProperty(key, callback);
			}
			if (old_callback != null)
				old_callback(m_gameClient.Player, 0); // not responded  = not in view

			GSTCPPacketOut pak = new GSTCPPacketOut(0xD0);
			pak.WriteShort((ushort) Checker.ObjectID);
			pak.WriteShort((ushort) TargetOID);
			pak.WriteShort(0x00); // ?
			pak.WriteShort(0x00); // ?
			SendTCP(pak);
		}

        protected virtual void SendQuestPacket(PlayerJournalEntry entry, int index)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));

			pak.WriteByte((byte) index);
            if (entry == null)
			{
				pak.WriteByte(0);
				pak.WriteByte(0);
				pak.WriteByte(0);
			}
			else
			{
                string name = entry.Name;
                string desc = entry.Description;
				if (name.Length > byte.MaxValue)
				{
                    if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": name is too long for 1.68+ clients (" + name.Length + ") '" + name + "'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > byte.MaxValue)
				{
                    if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": description is too long for 1.68+ clients (" + desc.Length + ") '" + desc + "'");
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

        public virtual void SendTaskUpdate()
        {
            string description = "You have no current personal task.";
            if (m_gameClient.Player.Task != null) description = m_gameClient.Player.Task.Description;

            SendQuestPacket(new PlayerJournalEntry(description, ""), 0);	
        }
	    
		public virtual void SendQuestUpdate(AbstractQuest quest)
		{
			int questIndex = 0;
			lock (m_gameClient.Player.ActiveQuests)
			{
				foreach (AbstractQuest q in m_gameClient.Player.ActiveQuests)
				{
					if (q == quest)
					{
                        SendQuestPacket(new PlayerJournalEntry(quest.Name, quest.Description), questIndex);
						break;
					}
					if (q.Step != 0) questIndex++;
				}
			}
		}

		public virtual void SendQuestListUpdate()
		{
			int questIndex = 0;
			lock (m_gameClient.Player.ActiveQuests)
			{
				int questToClear = 0;
				foreach (AbstractQuest quest in m_gameClient.Player.ActiveQuests)
				{
					if(quest.Step <= 0)
					{
                        questToClear++;
					}
					else
					{
						SendQuestPacket(new PlayerJournalEntry(quest.Name, quest.Description), questIndex);
						questIndex++;
					}
				}

				for(int i = 0 ; i < questToClear ; i++)
				{
					SendQuestPacket(null, questIndex);
					questIndex++;
				}
			}
		}

		public virtual void SendGroupWindowUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x06);

			PlayerGroup group = m_gameClient.Player.PlayerGroup;
			if (group == null)
			{
				pak.WriteByte(0x00);
			}
			else
			{
				pak.WriteByte((byte) group.PlayerCount);
			}

			pak.WriteByte(0x01);
			pak.WriteByte(0x00);

			if (group != null)
			{
				lock (group)
				{
					for (int i = 0; i < group.PlayerCount; ++i)
					{
						GamePlayer updatePlayer = group[i];
						bool sameRegion = updatePlayer.Region == m_gameClient.Player.Region;

						pak.WriteByte(updatePlayer.Level);
						if (sameRegion)
						{
							pak.WriteByte(updatePlayer.HealthPercent);
							pak.WriteByte(updatePlayer.ManaPercent);

							byte playerStatus = 0;
							if (!updatePlayer.Alive)
								playerStatus |= 0x01;
							if (updatePlayer.Mez)
								playerStatus |= 0x02;
							if (updatePlayer.IsDiseased)
								playerStatus |= 0x04;
							if (SpellHandler.FindEffectOnTarget(updatePlayer, "DamageOverTime") != null)
								playerStatus |= 0x08;
							if (updatePlayer.Client.ClientState == GameClient.eClientState.Linkdead)
								playerStatus |= 0x10;
							if (updatePlayer.Region != m_gameClient.Player.Region)
								playerStatus |= 0x20;

							pak.WriteByte(playerStatus);
							// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
							// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

							pak.WriteShort((ushort) updatePlayer.ObjectID); //or session id?
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

		public void SendGroupMemberUpdate(bool updateIcons, GamePlayer player)
		{
			if (m_gameClient.Player == null)
				return;
			PlayerGroup group = m_gameClient.Player.PlayerGroup;
			if (group == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.GroupMemberUpdate));
			lock (group)
			{ // make sure group is not modified before update is sent else player index could change _before_ update
				if (player.PlayerGroup != group)
					return;
				WriteGroupMemberUpdate(pak, updateIcons, player);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public void SendGroupMembersUpdate(bool updateIcons)
		{
			if (m_gameClient.Player == null)
				return;

			PlayerGroup group = m_gameClient.Player.PlayerGroup;
			if (group == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.GroupMemberUpdate));
			lock (group)
			{ // make sure group is not modified before update is sent else player index could change _before_ update
				foreach (GamePlayer player in group)
					WriteGroupMemberUpdate(pak, updateIcons, player);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		protected virtual void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GamePlayer player)
		{
			pak.WriteByte((byte) (player.PlayerGroupIndex + 1)); // From 1 to 8
			bool sameRegion = player.Region == m_gameClient.Player.Region;
			if (sameRegion)
			{
				pak.WriteByte(player.HealthPercent);
				pak.WriteByte(player.ManaPercent);

				byte playerStatus = 0;
				if (!player.Alive)
					playerStatus |= 0x01;
				if (player.Mez)
					playerStatus |= 0x02;
				if (player.IsDiseased)
					playerStatus |= 0x04;
				if (SpellHandler.FindEffectOnTarget(player, "DamageOverTime") != null)
					playerStatus |= 0x08;
				if (player.Client.ClientState == GameClient.eClientState.Linkdead)
					playerStatus |= 0x10;
				if (!sameRegion)
					playerStatus |= 0x20;

				pak.WriteByte(playerStatus);
				// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
				// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | player.PlayerGroupIndex));
					lock (player.EffectList)
					{
						byte i=0;
						foreach (IGameEffect effect in player.EffectList)
							if(effect is GameSpellEffect)
								i++;
						pak.WriteByte(i);
						foreach (IGameEffect effect in player.EffectList)
							if(effect is GameSpellEffect)
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
					pak.WriteByte((byte)(0x80 | player.PlayerGroupIndex));
					pak.WriteByte(0);
				}
			}
		}

		protected virtual void SendInventorySlotsUpdateBase(ICollection slots, byte preAction)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			pak.WriteByte((byte) (slots == null ? 0 : slots.Count));
			pak.WriteByte((byte) ((m_gameClient.Player.Inventory.IsCloakHoodUp ? 0x01 : 0x00) | (int) m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte(m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(preAction); //preAction (0x00 - Do nothing)
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					pak.WriteByte((byte) updatedSlot);
					
					GenericItem item = m_gameClient.Player.Inventory.GetItem((eInventorySlot) updatedSlot) as GenericItem;
					if (item == null)
					{
						pak.Fill(0x00, 18);
						continue;
					}

					int value1 = 0; // some object types use this field to display count
					int value2 = 0; // some object types use this field to display count
					int handNeeded = 0;
					int damageType = 0;
					int condition = 0;
					int durabiliy = 0; 
					int quality = 0; 
					int bonus = 0; 
					int color = 0;
					int effect = 0;
					int count = 0;

					if(item is StackableItem)
					{
						value1 = count = ((StackableItem)item).Count;
						if(item is Ammunition)
						{
							value2 = ((byte)((Ammunition)item).Damage) | (((byte)((Ammunition)item).Range) << 2) | (((byte)((Ammunition)item).Precision) << 4);
							damageType = (byte)((Ammunition)item).DamageType;
						}
					}
					else if(item is Instrument)
					{
						value1 = (byte)((Instrument)item).Type;
					}
					else if (item is Shield)
					{
						value1 = (byte)((Shield)item).Size;
						value2 = ((Shield)item).DamagePerSecond;
					}
					else if(item is Weapon)
					{
						value1 = ((Weapon)item).DamagePerSecond;
						if(item is ThrownWeapon)
						{
							value2 = count = ((ThrownWeapon)item).Count;
						}
						else
						{
							value2 = (byte)(((Weapon)item).Speed / 100);
						}
					}
					else if(item is Armor)
					{
						value1 = ((Armor)item).ArmorFactor;
						value2 = ((Armor)item).Absorbtion;
					}

					if(item is VisibleEquipment)
					{
						color = ((VisibleEquipment)item).Color;
					}

					if(item is Weapon)
					{
						handNeeded = (byte)((Weapon)item).HandNeeded;
						damageType = (byte)((Weapon)item).DamageType;
						effect = ((Weapon)item).GlowEffect;
					}

					if(item is EquipableItem)
					{
						condition = (byte)((EquipableItem)item).Condition;
						durabiliy = ((EquipableItem)item).Durability; 
						quality = ((EquipableItem)item).Quality; 
						bonus = ((EquipableItem)item).Bonus;
					}
					else if(item is SpellCraftGem)
					{
						quality = ((SpellCraftGem)item).Quality; 
					}
					

					pak.WriteByte(item.Level);
					pak.WriteByte((byte) value1);
					pak.WriteByte((byte) value2);
					pak.WriteByte((byte) (handNeeded << 6));
					pak.WriteByte((byte) ((damageType << 6) + (byte)item.ObjectType));
					pak.WriteShort((ushort) item.Weight);
					pak.WriteByte((byte) condition); // % of con
					pak.WriteByte((byte) durabiliy); // % of dur
					pak.WriteByte((byte) quality); // % of qua
					pak.WriteByte((byte) bonus); // % bonus
					pak.WriteShort((ushort) item.Model);
					pak.WriteShort((ushort) color);
					pak.WriteShort((ushort) effect);
					if (count > 1)
						pak.WritePascalString(count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
			}
			SendTCP(pak);
		}

		public virtual void SendInventorySlotsUpdate(byte preAction, ICollection slots)
		{
			// slots contain ints
			if (m_gameClient.Player == null)
				return;

			// clients crash if too long packet is sent
			// so we send big updates in parts
			const int MAX_UPDATE = 32;
			if (slots == null || slots.Count <= MAX_UPDATE)
			{
				SendInventorySlotsUpdateBase(slots, preAction);
			}
			else
			{
				ArrayList updateSlots = new ArrayList(MAX_UPDATE);
				foreach (int slot in slots)
				{
					updateSlots.Add(slot);
					if (updateSlots.Count >= MAX_UPDATE)
					{
						SendInventorySlotsUpdateBase(updateSlots, preAction);
						updateSlots.Clear();
					}
				}
				if (updateSlots.Count > 0)
					SendInventorySlotsUpdateBase(updateSlots, preAction);
			}
		}

		public virtual void SendInventorySlotsUpdate(ICollection slots)
		{
			SendInventorySlotsUpdate(0, slots);
		}

		public virtual void SendDoorState(IDoor door)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DoorState));
			pak.WriteInt((uint)door.DoorID);
			pak.WriteByte((byte)door.State);
			pak.WriteByte((byte)door.Flag); // It seems to be the type of the door (in costwold all door are 1, keep door are 4)
			pak.Fill(0x0, 2);
			SendTCP(pak);
		}

		public virtual void SendMerchantWindow(IGameMerchant merchant)
		{
			MerchantWindow window = GameServer.Database.FindObjectByKey(typeof(MerchantWindow), merchant.MerchantWindowID) as MerchantWindow;	
			if (window != null)
			{
				for (int pageNumber = 0; pageNumber < MerchantWindow.MAX_PAGES_IN_MERCHANTWINDOW; pageNumber++)
				{
					MerchantPage page = window.MerchantPages[pageNumber] as MerchantPage;
					if(page == null || page.MerchantItems == null)
						continue;

					GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MerchantWindow));

					pak.WriteByte((byte) page.MerchantItems.Count); //Item count on this page
					pak.WriteByte((byte) page.Currency);
					pak.WriteByte((byte) pageNumber); //Page number
					pak.WriteByte(0x00); //Unused

					for (int i = 0; i < MerchantPage.MAX_ITEMS_IN_MERCHANTPAGE; i++)
					{
						MerchantItem merchantItem = page.MerchantItems[i] as MerchantItem;
						if (merchantItem == null)
							continue;

						GenericItemTemplate item = merchantItem.ItemTemplate;
						if (item!=null)
						{
							//DOLConsole.WriteLine("Item i="+itemIndex);
							pak.WriteByte((byte) i); //Item index on page
							
							int value1 = 0; // some object types use this field to display count
							int value2 = 0; // some object types use this field to display count
							int weight = item.Weight;
							int handNeeded = 0;
							int damageType = 0;
							
							if(item is StackableItemTemplate)
							{
								value1 = ((StackableItemTemplate)item).PackSize;
								if(item is AmmunitionTemplate)
								{
									damageType = (byte)((AmmunitionTemplate)item).DamageType;
								}
							}
							else if(item is InstrumentTemplate)
							{
								value1 = (byte)((InstrumentTemplate)item).Type;
							}
							else if (item is ShieldTemplate)
							{
								value1 = (byte)((ShieldTemplate)item).Size;
							}
							else if(item is WeaponTemplate)
							{
								value1 = ((WeaponTemplate)item).DamagePerSecond;
								
								if(item is ThrownWeaponTemplate)
								{
									value2 = ((ThrownWeaponTemplate)item).PackSize;
								}
								else
								{
									value2 = (byte)(((WeaponTemplate)item).Speed/100);
								}

								handNeeded = (byte)((WeaponTemplate)item).HandNeeded;
								damageType = (byte)((WeaponTemplate)item).DamageType;
							}
							else if(item is ArmorTemplate)
							{
								value1 = ((ArmorTemplate)item).ArmorFactor;
								value2 = ((ArmorTemplate)item).Absorbtion;
							}
							
							pak.WriteByte(item.Level);
							pak.WriteByte((byte) value1);
							pak.WriteByte((byte) value2);
							pak.WriteByte((byte) (handNeeded << 6));
							pak.WriteByte((byte) ((damageType << 6) + (byte)item.ObjectType));
							//1 if item cannot be used by your class (greyed out)
							if ((item is EquipableItemTemplate && ! m_gameClient.Player.HasAbilityToUseItem((EquipableItem)item.CreateInstance()))
							 || (item is PoisonTemplate && m_gameClient.Player.GetModifiedSpecLevel(Specs.Envenom) < 1)
							 || (item is BoltTemplate && ! m_gameClient.Player.HasAbility(Abilities.Weapon_Crossbow))
							 || (item is ArrowTemplate && ! m_gameClient.Player.HasAbility(Abilities.Weapon_CompositeBows) && ! m_gameClient.Player.HasAbility(Abilities.Weapon_Longbows) && ! m_gameClient.Player.HasAbility(Abilities.Weapon_RecurvedBows) && ! m_gameClient.Player.HasAbility(Abilities.Weapon_Shortbows)))
								pak.WriteByte(0x01);
							else
								pak.WriteByte(0x00);
							pak.WriteShort((ushort) weight);
							//Item Price
							pak.WriteInt((uint) item.Value);
							pak.WriteShort((ushort) item.Model);
							pak.WritePascalString(item.Name);
						}
						else
						{
							if (log.IsErrorEnabled)
								log.Error("MerchantItem '"+ merchantItem.MerchantItemID + "' .ItemTemplate not found, abort!!!");
							return;
						}
					}
					SendTCP(pak);
				}
			}
			else
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MerchantWindow));
				pak.Fill(0x00, 4);
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
				foreach (GenericItem item in m_gameClient.Player.TradeWindow.TradeItems)
				{
					pak.WriteByte((byte) item.SlotPosition);
				}
				pak.Fill(0x00,10-m_gameClient.Player.TradeWindow.TradeItems.Count);

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort) Money.GetMithril(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetPlatinum(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetGold(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetSilver(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetCopper(m_gameClient.Player.TradeWindow.TradeMoney));

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort) Money.GetMithril(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetPlatinum(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetGold(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetSilver(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetCopper(m_gameClient.Player.TradeWindow.PartnerTradeMoney));

				pak.WriteShort(0x0000);
				ArrayList items = m_gameClient.Player.TradeWindow.PartnerTradeItems;
				if (items != null)
				{
					pak.WriteByte((byte) items.Count);
					pak.WriteByte(0x01);
				}
				else
				{
					pak.WriteShort(0x0000);
				}
				pak.WriteByte((byte) (m_gameClient.Player.TradeWindow.Repairing ? 0x01 : 0x00));
				pak.WriteByte((byte) (m_gameClient.Player.TradeWindow.Combine ? 0x01 : 0x00));
				if (items != null)
				{
					foreach (GenericItem item in items)
					{
						int value1 = 0; // some object types use this field to display count
						int value2 = 0; // some object types use this field to display count
						int handNeeded = 0;
						int damageType = 0;
						int condition = 0;
						int durabiliy = 0; 
						int quality = 0; 
						int bonus = 0; 
						int color = 0;
						int effect = 0;
						int count = 0;

						if(item is StackableItem)
						{
							value1 = count = ((StackableItem)item).Count;
							if(item is Ammunition)
							{
								value2 = ((byte)((Ammunition)item).Damage) | (((byte)((Ammunition)item).Range) << 2) | (((byte)((Ammunition)item).Precision) << 4);//item.SPD_ABS;
							}
						}
						else if(item is Instrument)
						{
							value1 = (byte)((Instrument)item).Type;
						}
						else if (item is Shield)
						{
							value1 = (byte)((Shield)item).Size;
							value2 = ((Shield)item).DamagePerSecond;
						}
						else if(item is Weapon)
						{
							value1 = ((Weapon)item).DamagePerSecond;
							if(item is ThrownWeapon)
							{
								value2 = count = ((ThrownWeapon)item).Count;
							}
							else
							{
								value2 = (byte)(((Weapon)item).Speed/100);
							}
                        }
						else if(item is Armor)
						{
							value1 = ((Armor)item).ArmorFactor;
							value2 = ((Armor)item).Absorbtion;
						}

						if(item is Weapon)
						{
							handNeeded = (byte)((Weapon)item).HandNeeded;
							damageType = (byte)((Weapon)item).DamageType;
							effect = ((Weapon)item).GlowEffect;
						}
						else if(item is Ammunition)
						{
							damageType = (byte)((Ammunition)item).DamageType;
						}

						if(item is EquipableItem)
						{
							condition = (byte)((EquipableItem)item).Condition;
							durabiliy = ((EquipableItem)item).Durability; 
							quality = ((EquipableItem)item).Quality; 
							bonus = ((EquipableItem)item).Bonus;
						}
						else if(item is SpellCraftGem)
						{
							quality = ((SpellCraftGem)item).Quality; 
						}

						if(item is VisibleEquipment)
						{
							color = ((VisibleEquipment)item).Color;
						}

						pak.WriteByte((byte) item.SlotPosition);
						pak.WriteByte(item.Level);
						pak.WriteByte((byte) value1);
						pak.WriteByte((byte) value2);
						pak.WriteByte((byte) (handNeeded << 6));
						pak.WriteByte((byte) ((damageType << 6) + (byte)item.ObjectType));
						pak.WriteShort((ushort) item.Weight);
						pak.WriteByte((byte) condition); // % of con
						pak.WriteByte((byte) durabiliy); // % of dur
						pak.WriteByte((byte) quality); // % of qua
						pak.WriteByte((byte) bonus); // % bonus
						pak.WriteShort((ushort) item.Model);
						pak.WriteShort((ushort) color);
						pak.WriteShort((ushort) effect);
						if (count > 1)
							pak.WritePascalString(count + " " + item.Name);
						else
							pak.WritePascalString(item.Name);
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

			pak.WriteShort((ushort) killedPlayer.ObjectID);
			if (killer != null)
				pak.WriteShort((ushort) killer.ObjectID);
			else
				pak.WriteShort(0x00);
			pak.Fill(0x0, 4);
			SendTCP(pak);
		}

		public virtual void SendPlayerRevive(GamePlayer revivedPlayer)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerRevive));

			pak.WriteShort((ushort) revivedPlayer.ObjectID);
			pak.WriteShort(0x00);
			SendTCP(pak);
		}

		public virtual void SendUpdatePlayer()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x03); //subcode
			pak.WriteByte(0x0d); //number of entry
			pak.WriteByte(0x00); //subtype
			pak.WriteByte(0x00); //unk
			//entry :

			pak.WriteByte(m_gameClient.Player.Level); //level
			pak.WritePascalString(m_gameClient.Player.Name);

			pak.WriteByte((byte) (m_gameClient.Player.MaxHealth >> 8)); // maxhealth high byte ?
			pak.WritePascalString(m_gameClient.Player.CharacterClass.Name); // class name
			pak.WriteByte((byte) (m_gameClient.Player.MaxHealth & 0xFF)); // maxhealth low byte ?

			pak.WritePascalString( /*"The "+*/m_gameClient.Player.CharacterClass.Profession); // Profession

			pak.WriteByte(0x00); //unk

			pak.WritePascalString(m_gameClient.Player.CharacterClass.GetTitle(m_gameClient.Player.Level));

			//todo make function to calcule realm rank
			//client.Player.RealmPoints
			//todo i think it s realmpoint percent not realrank
			pak.WriteByte((byte) m_gameClient.Player.RealmLevel); //urealm rank
			pak.WritePascalString(m_gameClient.Player.RealmTitle);
			pak.WriteByte((byte) m_gameClient.Player.RealmSpecialtyPoints); // realm skill points

			pak.WritePascalString(m_gameClient.Player.CharacterClass.BaseName); // base class

			pak.WriteByte((byte) (m_gameClient.Player.LotNumber >> 8)); // personal house high byte
			if (m_gameClient.Player.Guild != null)	pak.WritePascalString(m_gameClient.Player.Guild.GuildName);
			else pak.WriteByte(0x0);
			pak.WriteByte((byte) (m_gameClient.Player.LotNumber & 0xFF)); // personal house low byte

			pak.WritePascalString(m_gameClient.Player.LastName);

			pak.WriteByte(0x0); // ML Level
			pak.WritePascalString(GlobalConstants.RaceToName((eRace)m_gameClient.Player.Race));

			pak.WriteByte(0x0);
			if (m_gameClient.Player.Guild != null) pak.WritePascalString(m_gameClient.Player.Guild.GuildRanks[m_gameClient.Player.GuildRank].Title);
			else pak.WriteByte(0x0);
			pak.WriteByte(0x0);

			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_gameClient.Player.CraftingPrimarySkill);
			if (skill != null)
				pak.WritePascalString(skill.Name); //crafter guilde: alchemist
			else
				pak.WritePascalString("None"); //no craft skill at start

			pak.WriteByte(0x0);
			pak.WritePascalString(m_gameClient.Player.CraftTitle); //crafter title: legendary alchemist

			pak.WriteByte(0x0);
			pak.WritePascalString("None"); //ML title
			SendTCP(pak);
		}

		public virtual void SendUpdatePlayerSkills()
		{
			switch (m_gameClient.Player.CharacterClass.ClassType)
			{
				case eClassType.PureTank:
					SendUpdatePureTankSkills();
					break;

				case eClassType.ListCaster:
					SendUpdateListCasterSkills();
					break;

				case eClassType.Hybrid:
					SendUpdateHybridSkills();
					break;
			}
		}

		public virtual void SendUpdateHybridSkills()
		{
			if (m_gameClient.Player == null)
				return;
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			IList spelllines = m_gameClient.Player.GetSpellLines();
			Hashtable m_styleId = new Hashtable();

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			
			lock (skills.SyncRoot)
				lock (styles.SyncRoot)
					lock (specs.SyncRoot)
						lock (spelllines.SyncRoot)
						{
							int spellscount = m_gameClient.Player.GetAmountOfSpell();

							pak.WriteByte(0x01); //subcode
							pak.WriteByte((byte) (specs.Count + skills.Count + styles.Count + spellscount)); //number of entry
							pak.WriteByte(0x03); //subtype
							pak.WriteByte(0x00); //unk

							foreach (Specialization spec in specs)
							{
								pak.WriteByte((byte) spec.Level);
								pak.WriteByte((byte) eSkillPage.Specialization);
								pak.WriteShort(0);
								pak.WriteByte((byte) (m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
								pak.WriteShort(spec.ID);
								pak.WritePascalString(spec.Name);
							}

							int i=0;
							foreach (Skill skill in skills)
							{
								i++;
								pak.WriteByte((byte) 0);
								if(skill.ID < 500) pak.WriteByte((byte) eSkillPage.Abilities);
								else pak.WriteByte((byte) eSkillPage.AbilitiesSpell);
								pak.WriteShort(0);
								pak.WriteByte(0);
								pak.WriteShort(skill.ID);
								pak.WritePascalString(skill.Name);
							}

							foreach (Style style in styles)
							{
								m_styleId[(int)style.ID] = i++;
								//DOLConsole.WriteLine("style sended "+style.Name);
								pak.WriteByte(0); // no level for style
								pak.WriteByte((byte) eSkillPage.Styles);

								int pre = 0;
								switch (style.OpeningRequirementType)
								{
									case Style.eOpening.Offensive:
										pre = 0 + (int) style.AttackResultRequirement; // last result of our attack against enemy
										// hit, miss, target blocked, target parried, ...
										if (style.AttackResultRequirement==Style.eAttackResult.Style)
											pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
										break;
									case Style.eOpening.Defensive:
										pre = 100 + (int) style.AttackResultRequirement; // last result of enemies attack against us
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

								pak.WriteShort((ushort) pre);
								pak.WriteByte(0); // bonus
								pak.WriteShort(style.ID);
								pak.WritePascalString(style.Name);
							}
							foreach (SpellLine spellline in spelllines)
							{
								int spec_index = specs.IndexOf(m_gameClient.Player.GetSpecialization(spellline.Spec));
								if (spec_index == -1)
									spec_index = 0xFE; // Nightshade special value
								IList spells = m_gameClient.Player.GetUsableSpellsOfLine(spellline);
								foreach (Spell spell in spells)
								{
									pak.WriteByte((byte) spell.Level);
									if (spell.InstrumentRequirement == 0)
									{
										pak.WriteByte((byte) eSkillPage.Spells);
										pak.WriteByte(0);
										pak.WriteByte((byte) spec_index);
									}
									else
									{
										pak.WriteByte((byte) eSkillPage.Songs);
										pak.WriteByte(0);
										pak.WriteByte(0xFF);
									}
									pak.WriteByte(0);
									pak.WriteShort(spell.Icon);
									pak.WritePascalString(spell.Name);
								}
							}
						}
			SendTCP(pak);
		}

		public virtual void SendUpdateListCasterSkills()
		{
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			IList spelllines = m_gameClient.Player.GetSpellLines();
			Hashtable m_styleId = new Hashtable();

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			
			lock (skills.SyncRoot)
				lock (styles.SyncRoot)
					lock (specs.SyncRoot)
					{
						pak.WriteByte(0x01);//subcode
						pak.WriteByte((byte)(specs.Count+skills.Count+styles.Count));//number of entry
						pak.WriteByte(0x03);//subtype
						pak.WriteByte(0);//unk

						foreach (Specialization spec in specs)
						{
							pak.WriteByte((byte)spec.Level);
							pak.WriteByte((byte)eSkillPage.Specialization);
							pak.WriteShort(0);
							pak.WriteByte((byte)(m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName)-spec.Level)); // bonus
							pak.WriteShort(spec.ID);
							pak.WritePascalString(spec.Name);
						}

						int i=0;
						foreach (Skill skill in skills)
						{
							i++;
							pak.WriteByte(0);
							if(skill.ID < 500) pak.WriteByte((byte) eSkillPage.Abilities);
							else pak.WriteByte((byte) eSkillPage.AbilitiesSpell);
							pak.WriteShort(0);
							pak.WriteByte(0);
							pak.WriteShort(skill.ID);
							pak.WritePascalString(skill.Name);
						}

						foreach (Style style in styles)
						{
							m_styleId[(int)style.ID] = i++;
							//DOLConsole.WriteLine("style sent "+style.Name);
							pak.WriteByte(0); // no level for style
							pak.WriteByte((byte)eSkillPage.Styles);

							int pre = 0;
							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = 0 + (int)style.AttackResultRequirement; // last result of our attack against enemy
									// hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement==Style.eAttackResult.Style)
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
							pak.WriteShort(style.ID);
							pak.WritePascalString(style.Name);
						}
					}

			SendTCP(pak);

			byte linenumber = 0;

			lock (spelllines.SyncRoot)
			{
				foreach (SpellLine line in spelllines)
				{
					IList spells = SkillBase.GetSpellList(line.KeyName);
					int spellcount = 0;
					for (int i = 0; i < spells.Count; i++)
					{
						if (((Spell) spells[i]).Level <= line.Level)
						{
							spellcount++;
						}
					}
					pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
					pak.WriteByte(0x02); //subcode
					pak.WriteByte((byte) (spellcount + 1)); //number of entry
					pak.WriteByte(0x02); //subtype
					pak.WriteByte(linenumber++); //number of line
					pak.WriteByte(0); // level, not used when spell line
					pak.WriteShort(0); // icon, not used when spell line
					pak.WritePascalString(line.Name);
					foreach (Spell spell in spells)
					{
						if (spell.Level <= line.Level)
						{
							pak.WriteByte((byte) spell.Level);
							pak.WriteShort(spell.Icon);
							pak.WritePascalString(spell.Name);
						}
					}
					SendTCP(pak);
				}
			}
		}

		public virtual void SendUpdatePureTankSkills()
		{
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			Hashtable m_styleId = new Hashtable();

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			
			lock (skills.SyncRoot)
				lock (styles.SyncRoot)
					lock (specs.SyncRoot)
					{
						pak.WriteByte(0x01); //subcode
						pak.WriteByte((byte) (specs.Count + skills.Count + styles.Count)); //number of entry
						pak.WriteByte(0x03); //subtype
						pak.WriteByte(0x00); //unk

						foreach (Specialization spec in specs)
						{
							pak.WriteByte((byte) spec.Level);
							pak.WriteByte((byte) eSkillPage.Specialization);
							pak.WriteShort(0);
							pak.WriteByte((byte) (m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
							pak.WriteShort(spec.ID);
							pak.WritePascalString(spec.Name);
						}

						int i=0;
						foreach (Skill skill in skills)
						{
							i++;
							pak.WriteByte((byte) 0);
							if(skill.ID < 500) pak.WriteByte((byte) eSkillPage.Abilities);
							else pak.WriteByte((byte) eSkillPage.AbilitiesSpell);
							pak.WriteShort(0);
							pak.WriteByte(0);
							pak.WriteShort(skill.ID);
							pak.WritePascalString(skill.Name);
						}

						foreach (Style style in styles)
						{
							m_styleId[(int)style.ID] = i++;
							//DOLConsole.WriteLine("style sended "+style.Name);
							pak.WriteByte(0); // no level for style
							pak.WriteByte((byte) eSkillPage.Styles);

							int pre = 0;
							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = 0 + (int) style.AttackResultRequirement; // last result of our attack against enemy
									// hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement==Style.eAttackResult.Style)
										pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
									break;
								case Style.eOpening.Defensive:
									pre = 100 + (int) style.AttackResultRequirement; // last result of enemies attack against us
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

							pak.WriteShort((ushort) pre);
							pak.WriteByte(0); // bonus
							pak.WriteShort(style.ID);
							pak.WritePascalString(style.Name);
						}
					}
			SendTCP(pak);
		}

		public virtual void SendUpdateCraftingSkills()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));

			pak.WriteByte(0x08); //subcode
			pak.WriteByte((byte) m_gameClient.Player.CraftingSkills.Count); //count
			pak.WriteByte(0x03); //subtype
			pak.WriteByte(0x00); //unk

			foreach (DictionaryEntry de in (Hashtable) m_gameClient.Player.CraftingSkills.Clone())
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
			int wd = (int)(m_gameClient.Player.WeaponDamage(m_gameClient.Player.AttackWeapon)*100.0);
			pak.WriteByte((byte) (wd/100));
			pak.WritePascalString(" ");
			pak.WriteByte((byte) (wd%100));
			pak.WritePascalString(" ");
			// weaponskill
			int ws = m_gameClient.Player.DisplayedWeaponSkill;
			pak.WriteByte((byte) (ws >> 8));
			pak.WritePascalString(" ");
			pak.WriteByte((byte) (ws & 0xff));
			pak.WritePascalString(" ");
			// overall EAF
			int eaf = m_gameClient.Player.EffectiveOverallAF;
			pak.WriteByte((byte) (eaf >> 8));
			pak.WritePascalString(" ");
			pak.WriteByte((byte) (eaf & 0xff));
			pak.WritePascalString(" ");

			SendTCP(pak);
		}

		public virtual void SendEncumberance()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Encumberance));
			pak.WriteShort((ushort) m_gameClient.Player.MaxEncumberance); // encumb total
			pak.WriteShort((ushort) m_gameClient.Player.Encumberance); // encumb used
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
				pak.WritePascalString((string) iter.Current);
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
			pak.WriteShort((ushort) seconds);
			pak.WriteByte((byte) title.Length);
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

		public virtual void SendTrainerWindow()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TrainerWindow));
			IList specs = m_gameClient.Player.GetSpecList();
			pak.WriteByte((byte) specs.Count);
			pak.WriteByte((byte) m_gameClient.Player.SkillSpecialtyPoints);
			pak.WriteByte((byte) 0);
			pak.WriteByte((byte) 0);

			int i = 0;
			foreach (Specialization spec in specs)
			{
				pak.WriteByte((byte) i++);
				pak.WriteByte((byte) spec.Level);
				pak.WriteByte((byte) (spec.Level + 1));
				pak.WritePascalString(spec.Name);
			}
			SendTCP(pak);
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

			if (skill.SkillType == eSkillPage.Abilities)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DisableSkills));
				pak.WriteShort((ushort) duration);
				int id = -1;
				IList skillList = m_gameClient.Player.GetNonTrainableSkillList();
				lock (skillList.SyncRoot)
				{
					foreach (Skill skl in skillList)
					{
						if (skl.SkillType == eSkillPage.Abilities)
							id++;
						if (skl == skill)
							break;
					}
				}
				if (id < 0)
					return;
				pak.WriteByte((byte) id);
				pak.WriteByte(0); // not used?

				SendTCP(pak);
			}
			if (skill.SkillType == eSkillPage.Spells)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DisableSkills));
				if (m_gameClient.Player.CharacterClass.ClassType==eClassType.ListCaster)
				{
					pak.WriteShort((ushort) duration);
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

						pak.WriteByte((byte) lineIndex);
						pak.WriteByte((byte) spellIndex);
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
					pak.WriteShort((ushort) (index + skillsCount));
					pak.WriteShort((ushort) duration);
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
						pak.WriteByte((effect is GameSpellEffect) ? i++ : (byte)0xff);
						pak.WriteByte(0);
						pak.WriteShort(effect.Icon);
						pak.WriteShort((ushort)(effect.RemainingTime/1000));
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
			pak.WriteShort((ushort) m_gameClient.Player.ObjectID);
			pak.WriteByte(1); //level up sounds
			pak.WriteByte(m_gameClient.Player.Realm);
			SendTCP(pak);
		}

		public virtual void SendRegionEnterSound(byte soundId)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionSound));
			pak.WriteShort((ushort) m_gameClient.Player.ObjectID);
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
			pak.WriteShort((ushort) player.ObjectID);
			pak.WriteByte(modelType);
			pak.WriteByte((byte) (modelType == 3 ? 0x08 : 0x00)); //unused?
			SendTCP(pak);
		}

		public virtual void SendObjectDelete(GameObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectDelete));
			pak.WriteShort((ushort) obj.ObjectID);
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
				pak.WriteByte((byte) (m_gameClient.Player.ConcentrationEffects.Count));
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown
				pak.WriteByte(0); // unknown

				for (int i = 0; i < m_gameClient.Player.ConcentrationEffects.Count; i++)
				{
					IConcentrationEffect effect = m_gameClient.Player.ConcentrationEffects[i];
					pak.WriteByte((byte) i);
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
			pak.WriteShort((ushort) (newTarget == null ? 0 : newTarget.ObjectID));
			pak.WriteShort(0); // unknown
			SendTCP(pak);
		}

		public virtual void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PetWindow));
			pak.WriteShort((ushort)(pet==null ? 0 : pet.ObjectID));
			pak.WriteByte(0x00); //unused
			pak.WriteByte(0x00); //unused
			switch (windowAction) //0-released, 1-normal, 2-just charmed? | Roach: 0-close window, 1-update window, 2-create window
			{
				case ePetWindowAction.Open  : pak.WriteByte(2); break;
				case ePetWindowAction.Update: pak.WriteByte(1); break;
				default: pak.WriteByte(0); break;
			}
			switch (aggroState) //1-aggressive, 2-defensive, 3-passive
			{
				case eAggressionState.Aggressive: pak.WriteByte(1); break;
				case eAggressionState.Defensive : pak.WriteByte(2); break;
				case eAggressionState.Passive   : pak.WriteByte(3); break;
				default: pak.WriteByte(0); break;
			}
			switch (walkState) //1-follow, 2-stay, 3-goto, 4-here
			{
				case eWalkState.Follow  : pak.WriteByte(1); break;
				case eWalkState.Stay    : pak.WriteByte(2); break;
				case eWalkState.GoTarget: pak.WriteByte(3); break;
				case eWalkState.ComeHere: pak.WriteByte(4); break;
				default: pak.WriteByte(0); break;
			}
			pak.WriteByte(0x00); //unused

			if (pet != null)
			{
				lock (pet.EffectList)
				{
					int count = 0;
					foreach (IGameEffect effect in pet.EffectList)
					{
						pak.WriteShort(effect.Icon); // 0x08 - null terminated - (byte) list of shorts - spell icons on pet
						if (++count > 8) break;
					}
				}
			}

			pak.WriteByte(0x00); //null termination

			SendTCP(pak);
		}

		//housing
		/*public virtual void SendHouse (House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseCreate));
			pak.WriteShort((ushort)house.HouseNumber);
			Point pos = house.Position;
			pak.WriteShort((ushort)pos.Z);
			pak.WriteInt((uint)pos.X);
			pak.WriteInt((uint)pos.Y);
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

		public virtual void SendGarden(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));
			pak.WriteShort((ushort) house.HouseNumber);
			pak.WriteByte((byte) house.OutdoorItems.Count);
			pak.WriteByte(0x80);
			for (int i = 0; i < house.OutdoorItems.Count; i++)
			{
				pak.WriteByte((byte) i);
				pak.WriteShort((ushort) ((OutdoorItem) house.OutdoorItems[i]).Model);
				pak.WriteByte((byte) ((OutdoorItem) house.OutdoorItems[i]).Position);
				pak.WriteByte((byte) ((OutdoorItem) house.OutdoorItems[i]).Rotation);
			}

			SendTCP(pak);
		}

		public virtual void SendRemoveGarden(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));

			pak.WriteShort((ushort) house.HouseNumber);
			pak.WriteByte(0x00);
			pak.WriteByte(0x01); //dont know why 0x01 here?!

			SendTCP(pak);
		}

		public virtual void SendEnterHouse (House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseEnter));

			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)25000);         //constant!
			Point pos = house.Position;
			pak.WriteInt((uint)pos.X);
			pak.WriteInt((uint)pos.Y);
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

		public void SendToggleHousePoints(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseTogglePoints));

			pak.WriteShort((ushort) house.HouseNumber);
			pak.WriteByte(0x04);
			pak.WriteByte(0x00);

			SendTCP(pak);
		}

		public void SendFurniture (House house)
		{

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HousingItem));

			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(Convert.ToByte(house.IndoorItems.Count));
			pak.WriteByte(0x80); //0x00 = update, 0x80 = complete package
			for(int i = 0; i < house.IndoorItems.Count; i++)
			{
				IndoorItem item = (IndoorItem)house.IndoorItems[i];
				pak.WriteByte((byte)i);
				pak.WriteShort((ushort)item.Model);
				pak.WriteShort((ushort)item.Color);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				pak.WriteShort((ushort)item.X);
				pak.WriteShort((ushort)item.Y);
				pak.WriteShort((ushort)item.Rotation);
				pak.WriteByte(Convert.ToByte(item.Size));
				pak.WriteByte(Convert.ToByte(item.Position));
				pak.WriteByte(Convert.ToByte(item.Placemode-2));
			}
			SendTCP(pak);
		}*/

		public virtual void SendPlaySound(eSoundType soundType, ushort soundID)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlaySound));
			pak.WriteShort((ushort)soundType);
			pak.WriteShort(soundID);
			pak.Fill(0x00, 8);
			SendTCP(pak);
		}

		/*
		public virtual void SendKeepInfo(AbstractGameKeep keep)
		{
		}

		public virtual void SendKeepComponentInfo(GameKeepComponent keepComponent)
		{
		}
		public virtual void SendKeepComponentDetailUpdate(GameKeepComponent keepComponent)
		{
		}
		public virtual void SendWarmapUpdate(IList list)
		{
		}
		public virtual void SendWarmapBonuses()
		{
		}
		 
		public virtual void SendKeepClaim(AbstractGameKeep keep)
		{}

		public virtual void SendKeepComponentUpdate(AbstractGameKeep keep,bool LevelUp)
		{}

		public virtual void SendKeepComponentInteract(GameKeepComponent component)
		{}

		public virtual void SendKeepComponentHookPoint(GameKeepComponent component,int selectedHookPointIndex)
		{}

		public virtual void SendClearKeepComponentHookPoint(GameKeepComponent component,int selectedHookPointIndex)
		{}

		public virtual void SendHookPointStore(GameKeepHookPoint hookPoint)
		{}

		public virtual void SendMovingObjectCreate(GameMovingObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.MovingObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			pak.WriteShort(0);
			pak.WriteShort((ushort) obj.Heading);
			Point pos = obj.Position;
			pak.WriteShort((ushort)pos.Z);
			pak.WriteInt((uint)pos.X);
			pak.WriteInt((uint)pos.Y);
			pak.WriteShort((ushort) obj.Model);
			pak.WriteShort(obj.Type());
			pak.WriteInt(0);//(0x0002-for Ship,0x7D42-for catapult,0x9602,0x9612,0x9622-for ballista)
			pak.WriteInt(0);
			pak.WritePascalString(obj.Name);
			pak.WriteByte(0); // trailing ?
			SendTCP(pak);
		}

		public virtual void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.SiegeWeaponInterface));
			pak.WriteShort(siegeWeapon.AmmoType);
			pak.WriteByte(0);
			pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.Interval/100));//time in 100ms

			pak.WriteByte((byte)siegeWeapon.Ammo.Count); // external ammo count
			pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
			pak.WriteByte((byte)siegeWeapon.AmmoSlot);
			pak.WriteShort(siegeWeapon.Effect);
			pak.WriteShort(0); // ?
			pak.WriteInt((uint)siegeWeapon.ObjectID);

			pak.WritePascalString(siegeWeapon.Name+" ("+siegeWeapon.CurrentState.ToString()+")");
			foreach (InventoryItem item in siegeWeapon.Ammo)
			{
				pak.WriteByte((byte) item.SlotPosition);
				if (item == null)
				{
					pak.Fill(0x00, 18);
					continue;
				}
				pak.WriteByte((byte) item.Level);
				pak.WriteByte((byte) item.DPS_AF);
				pak.WriteByte((byte) item.SPD_ABS);
				pak.WriteByte((byte) (item.Hand*64));
				pak.WriteByte((byte) ((item.Type_Damage*64) + item.Object_Type));
				pak.WriteShort((ushort) item.Weight);
				pak.WriteByte(item.ConditionPercent); // % of con
				pak.WriteByte(item.DurabilityPercent); // % of dur
				pak.WriteByte((byte) item.Quality); // % of qua
				pak.WriteByte((byte) item.Bonus); // % bonus
				pak.WriteShort((ushort) item.Model);
				if (item.Emblem != 0)
					pak.WriteShort((ushort) item.Emblem);
				else
					pak.WriteShort((ushort) item.Color);
				pak.WriteShort((ushort) item.Effect);
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
			Point pos = siegeWeapon.TargetPosition;
			pak.WriteInt((uint) pos.X);
			pak.WriteInt((uint) pos.Y);
			pak.WriteInt((uint) pos.Z);
			pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
			pak.WriteShort(siegeWeapon.Effect);
			pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.Interval/100));
			pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
			if (siegeWeapon.SiegeWeaponTimer.CurrentAction == SiegeTimer.eAction.Fire)
			{
				pak.WriteByte((byte)0xAA);
				pak.WriteShort(0xFFBF);
			}
			else
				pak.Fill(0,3);
			SendTCP(pak);
		}
		public virtual void SendComponentUpdate(GameKeepComponent keepcomponent)
		{
			Zone z = keepcomponent.CurrentZone;
			if (z == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendComponentUpdate: component zone == null. component ID:"+keepcomponent.InternalID);
				return;
			}
			Point pos = keepcomponent.Position;
			ushort XOffsetInZone = (ushort) (pos.X - z.XOffset);
			ushort YOffsetInZone = (ushort) (pos.Y - z.YOffset);

			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.NPCUpdate));
			pak.WriteShort(0);
			pak.WriteShort((ushort) keepcomponent.Heading);
			pak.WriteShort(XOffsetInZone);
			pak.WriteShort(0);
			pak.WriteShort(YOffsetInZone);
			pak.WriteShort(0);
			pak.WriteShort((ushort) pos.Z);
			pak.WriteShort(0);
			pak.WriteShort((ushort) keepcomponent.ObjectID);
			pak.WriteShort(0x00);
			pak.WriteByte(keepcomponent.HealthPercent);
			pak.WriteByte(0);
			pak.WriteByte((byte) z.ZoneID);
			pak.WriteByte(0);
			SendUDP(pak);
		}

		public virtual void SendKeepDoorUpdate(GameKeepDoor door)
		{
			Zone z = door.CurrentZone;
			if (z == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendDoorUpdate: Door zone == null:"+door.InternalID);
				return;
			}
			Point pos = door.Position;
			ushort XOffsetInZone = (ushort) (pos.X - z.XOffset);
			ushort YOffsetInZone = (ushort) (pos.Y - z.YOffset);
			GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(ePackets.NPCUpdate));
			pak.WriteShort(0);
			pak.WriteShort((ushort) door.Heading);
			pak.WriteShort(XOffsetInZone);
			pak.WriteShort(0);
			pak.WriteShort(YOffsetInZone);
			pak.WriteShort(0);
			pak.WriteShort((ushort) pos.Z);
			pak.WriteShort(0);
			pak.WriteShort((ushort) door.ObjectID);
			pak.WriteShort(0x00);
			pak.WriteByte(door.HealthPercent);
			pak.WriteByte(0);
			pak.WriteByte((byte) z.ZoneID);
			pak.WriteByte(0);
			SendUDP(pak);
		}*/

		public virtual void SendNPCsQuestEffect(GameNPC npc, bool flag)
		{}

		public virtual void SendLivingDataUpdate(GameLiving living, bool updateStrings)
		{}

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
		
	}
}