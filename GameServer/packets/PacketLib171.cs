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

using DOL.AI.Brain;
using DOL.GS.Keeps;
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
			pak.WriteShort((ushort)m_gameClient.Player.Z);
			pak.WriteInt((uint)m_gameClient.Player.X);
			pak.WriteInt((uint)m_gameClient.Player.Y);
			pak.WriteShort(m_gameClient.Player.Heading);

			int flags = 0;
			if (m_gameClient.Player.CurrentRegion.DivingEnabled)
				flags = 0x80 | (m_gameClient.Player.IsUnderwater?0x01:0x00);
			pak.WriteByte((byte)(flags));

			pak.WriteByte(0x00);	//TODO Unknown
			Zone zone = m_gameClient.Player.CurrentZone;
			if (zone == null) return;
			pak.WriteShort((ushort)(zone.XOffset/0x2000));
			pak.WriteShort((ushort)(zone.YOffset/0x2000));
			pak.WriteShort(m_gameClient.Player.CurrentRegionID);
			pak.WriteShort(0x00); //TODO: unknown, new in 1.71
			SendTCP(pak);
		}

		public override void SendObjectCreate(GameObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			if (obj is GameStaticItem)
				pak.WriteShort((ushort)(obj as GameStaticItem).Emblem);
			else pak.WriteShort(0);
			pak.WriteShort(obj.Heading);
			pak.WriteShort((ushort)obj.Z);
			pak.WriteInt((uint)obj.X);
			pak.WriteInt((uint)obj.Y);
			pak.WriteShort(obj.Model);
			int flag = (obj.Realm & 3) << 4;
			if (obj is GameKeepBanner)
				flag |= 0x08;
			if (obj is GameStaticItemTimed && m_gameClient.Player!=null && ((GameStaticItemTimed)obj).IsOwner(m_gameClient.Player))
				flag |= 0x04;
			pak.WriteShort((ushort)flag);
			pak.WriteInt(0x0); //TODO: unknown, new in 1.71
			pak.WritePascalString(obj.Name);
			if (obj is IDoor)
			{
				pak.WriteByte((byte)(obj as IDoor).Flag);
				pak.WriteInt((uint)(obj as IDoor).DoorID);
			}
			else pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public override void SendNPCCreate(GameNPC npc)
		{
			if(npc is GameMovingObject) { SendMovingObjectCreate(npc as GameMovingObject); return; }

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.NPCCreate));
			int speed = 0;
			ushort speedZ = 0;
			if (npc == null)
				return;
			if(!npc.IsOnTarget())
			{
				speed = npc.CurrentSpeed;
				speedZ = (ushort)npc.ZAddition;
			}
			pak.WriteShort((ushort)npc.ObjectID);
			pak.WriteShort((ushort)(speed));
			pak.WriteShort(npc.Heading);
			pak.WriteShort((ushort)npc.Z);
			pak.WriteInt((uint)npc.X);
			pak.WriteInt((uint)npc.Y);
			pak.WriteShort(speedZ);
			pak.WriteShort(npc.Model);
			pak.WriteByte(npc.Size);
			pak.WriteByte(npc.Level);

			byte flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);
			if((npc.Flags & (uint)GameNPC.eFlags.TRANSPARENT) != 0) flags |= 0x01;
			if(npc.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
			if((npc.Flags & (uint)GameNPC.eFlags.PEACE) != 0) flags |= 0x10;
			if((npc.Flags & (uint)GameNPC.eFlags.FLYING) != 0) flags |= 0x20;


			pak.WriteByte(flags);
			pak.WriteByte(0x20); //TODO this is the default maxstick distance

			string add = "";
			byte flags2 = 0x00;
			if((npc.Flags & (uint)GameNPC.eFlags.CANTTARGET) != 0)
				if (m_gameClient.Account.PrivLevel > 1) add += "-DOR"; // indicates DOR flag for GMs
				else flags2 |= 0x01;
			if ((npc.Flags & (uint) GameNPC.eFlags.DONTSHOWNAME) != 0)
				if (m_gameClient.Account.PrivLevel > 1) add += "-NON"; // indicates NON flag for GMs
				else flags2 |= 0x02;
			if((npc.Flags & (uint)GameNPC.eFlags.TRANSPARENT) != 0) flags2 |= 0x04;
			if(npc.CanGiveOneQuest(m_gameClient.Player)) flags2 |= 0x08;

			pak.WriteByte(flags2); // 4 high bits seems unused (new in 1.71)
			pak.WriteShort(0x00); // new in 1.71
			pak.WriteByte(0x00); // new in 1.71 (region instance ID from StoC_0x20)

			string name = npc.Name;
			if (name.Length+add.Length+2 > 47) // clients crash with too long names
				name = name.Substring(0, 47-add.Length-2);
			if (add.Length > 0)
				name = string.Format("[{0}]{1}", name, add);

			pak.WritePascalString(name);

			if (npc.GuildName.Length > 47)
				pak.WritePascalString(npc.GuildName.Substring(0, 47));
			else pak.WritePascalString(npc.GuildName);

			pak.WriteByte(0x00);
			SendTCP(pak);

			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
			{
				IControlledBrain brain = npc.Brain as IControlledBrain;
				if (brain != null)
					SendObjectGuildID(npc, brain.Owner.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
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
					if(player.CurrentZone != null)
						pak.WriteByte((byte)player.CurrentZone.ID);
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

		protected override void SendQuestPacket(AbstractQuest quest, int index)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));

			pak.WriteByte((byte) index);
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
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": name is too long for 1.71 clients ("+name.Length+") '"+name+"'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > ushort.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": description is too long for 1.71 clients ("+desc.Length+") '"+desc+"'");
					desc = desc.Substring(0, ushort.MaxValue);
				}
				if (name.Length + desc.Length > 2048-10)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": name + description length is too long and would have crashed the client.\nName ("+name.Length+"): '"+name+"'\nDesc ("+desc.Length+"): '"+desc+"'");
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
			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, player));
				pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, player));
			}
			else if (!updateStrings)
			{
				pak.WriteByte(0xFF);
			}
			else
			{
				pak.WritePascalString(living.GuildName);
				pak.WritePascalString(living.Name);
			}
			SendTCP(pak);
		}
	}
}
