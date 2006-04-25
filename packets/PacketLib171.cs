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
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Quests;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(171, GameClient.eClientVersion.Version171)]
	public class PacketLib171 : PacketLib170
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.71 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib171(GameClient client):base(client)
		{
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
			Region currentRegion = m_gameClient.Player.Region;
			Zone zone = currentRegion.GetZone(m_gameClient.Player.Position);
			if (zone == null) return;
			pak.WriteShort((ushort)(zone.XOffset/0x2000));
			pak.WriteShort((ushort)(zone.YOffset/0x2000));
			pak.WriteShort((ushort) currentRegion.RegionID);
			pak.WriteShort(0x00); //TODO: unknown, new in 1.71
			SendTCP(pak);
		}

		public override void SendItemCreate(GameObject obj)
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
			pak.WriteShort((ushort)obj.Model); // All doors model is 0xFFFF
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
			pak.WriteInt(0x0); //TODO: unknown, new in 1.71
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

		public override void SendNPCCreate(GameObject obj)
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
				if (npcObj.Brain is PeaceBrain) flags |= 0x10;
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
			byte flags2 = 0x00;

			if(npcObj != null)
			{
				if((npcObj.Flags & (uint)GameNPC.eFlags.CANTTARGET) != 0)
					if (m_gameClient.Account.PrivLevel > ePrivLevel.Player) add += "-DOR"; // indicates DOR flag for GMs
					else flags2 |= 0x01;

				if ((npcObj.Flags & (uint) GameNPC.eFlags.DONTSHOWNAME) != 0)
					if (m_gameClient.Account.PrivLevel > ePrivLevel.Player) add += "-NON"; // indicates NON flag for GMs
					else flags2 |= 0x02;

				if((npcObj.Flags & (uint)GameNPC.eFlags.STEALTH) != 0) flags2 |= 0x04;
				if(QuestMgr.CanGiveOneNewQuest(npcObj, m_gameClient.Player)) flags2 |= 0x08;
			}
			else if(movingStaticObj != null)
			{
				flags2 |= 0x03;
			}

			pak.WriteByte(flags2); // 4 high bits seems unused (new in 1.71)
			pak.WriteByte(0x00); // new in 1.71
			pak.WriteByte(0x00); // new in 1.71
			pak.WriteByte(0x00); // new in 1.71

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

			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
			{
				if(npcObj != null)
				{
					IControlledBrain brain = npcObj.Brain as IControlledBrain;
					if (brain != null)
						SendObjectGuildID(npcObj, brain.Owner.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
				}
			}
		}

		public override void SendFindGroupWindowUpdate(GamePlayer[] list)
		{
			if (m_gameClient.Player==null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.FindGroupUpdate));
			if (list!=null)
			{
				pak.WriteByte((byte)list.Length);
				byte nbleader=0;
				byte nbsolo=0x1E;
				foreach(GamePlayer player in list)
				{
					if (player.PlayerGroup!=null)
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
					if(currentZone != null)
						pak.WriteByte((byte)currentZone.ZoneID);
					else
						pak.WriteByte(255);
					pak.WriteByte(0); // duration
					pak.WriteByte(0); // objective
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte((byte) (player.PlayerGroup!=null ? 1 : 0));
					pak.WriteByte(0);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
		}

        protected override void SendQuestPacket(PlayerJournalEntry entry, int index)
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
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": name is too long for 1.71 clients ("+name.Length+") '"+name+"'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > ushort.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": description is too long for 1.71 clients ("+desc.Length+") '"+desc+"'");
					desc = desc.Substring(0, ushort.MaxValue);
				}
				if (name.Length + desc.Length > 2048-10)
				{
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": name + description length is too long and would have crashed the client.\nName ("+name.Length+"): '"+name+"'\nDesc ("+desc.Length+"): '"+desc+"'");
					name = name.Substring(0, 32);
					desc = desc.Substring(0, 2048-10 - name.Length); // all that's left
				}
				pak.WriteByte((byte)name.Length);
				pak.WriteShort((ushort)desc.Length);
				pak.WriteStringBytes(name); //Write Quest Name without trailing 0
				pak.WriteStringBytes(desc); //Write Quest Description without trailing 0
			}
			SendTCP(pak);
		}

		public override void SendLivingDataUpdate(GameLiving living, bool updateStrings)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectDataUpdate));
			pak.WriteShort((ushort)living.ObjectID);
			pak.WriteByte(0);
			pak.WriteByte(living.Level);
			if (updateStrings)
			{
				GamePlayer player = living as GamePlayer;
				if (player != null)
				{
					pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, player));
					pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, player));
				}
				else
				{
					pak.WritePascalString(living.GuildName);
					pak.WritePascalString(living.Name);
				}
			}
			else
			{
				pak.WriteByte(0xFF);
			}
			SendTCP(pak);
		}
	}
}
