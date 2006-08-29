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
using System.Reflection;
using System.Collections;

using DOL.GS.Keeps;
using DOL.GS.Quests;

using log4net;


namespace DOL.GS.PacketHandler
{
	[PacketLib(170, GameClient.eClientVersion.Version170)]
	public class PacketLib170 : PacketLib169
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.70+ clients, contains NF Keep stuff.
		///
		/// --ShadowCode
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib170(GameClient client)
			: base(client)
		{
		}

		public override void SendKeepInfo(AbstractGameKeep keep)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepInfo));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteShort(0);//zone id not sure
			pak.WriteInt((uint)keep.X);
			pak.WriteInt((uint)keep.Y);
			pak.WriteShort((ushort)keep.Heading);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);//level(not sure)
			pak.WriteShort(0);//unk
			pak.WriteByte(0x57);//model= 5-8Bit =lvl 1-4bit = Keep Type //uncertain
			pak.WriteByte(0xB7);//unk
			SendTCP(pak);
		}

		public override void SendKeepComponentInfo(GameKeepComponent keepComponent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentInfo));

			pak.WriteShort((ushort)keepComponent.Keep.KeepID);
			pak.WriteShort((ushort)keepComponent.ID);
			pak.WriteInt((uint)keepComponent.ObjectID);
			pak.WriteByte((byte)keepComponent.Skin);
			pak.WriteByte((byte)(keepComponent.ComponentX));//relative to keep
			pak.WriteByte((byte)(keepComponent.ComponentY));//relative to keep
			pak.WriteByte((byte)keepComponent.ComponentHeading);
			pak.WriteByte((byte)keepComponent.Height);
			pak.WriteByte(keepComponent.HealthPercent);
			byte flag = keepComponent.Status;
			if (flag == 0x00 && keepComponent.Climbing)
				flag = 0x02;
			pak.WriteByte(flag);
			pak.WriteByte(0x00); //unk
			SendTCP(pak);
		}
		public override void SendKeepComponentDetailUpdate(GameKeepComponent keepComponent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentDetailUpdate));

			pak.WriteShort((ushort)keepComponent.Keep.KeepID);
			pak.WriteShort((ushort)keepComponent.ID);
			pak.WriteByte((byte)keepComponent.Height);
			pak.WriteByte(keepComponent.HealthPercent);
			byte flag = keepComponent.Status;
			if (flag == 0x00 && keepComponent.Climbing)
				flag = 0x02;
			pak.WriteByte(flag);
			pak.WriteByte(0x00);//unk
			SendTCP(pak);
		}
		public override void SendKeepComponentUpdate(AbstractGameKeep keep, bool LevelUp)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentUpdate));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);
			pak.WriteByte((byte)keep.KeepComponents.Count);
			foreach (GameKeepComponent component in keep.KeepComponents)
			{
				byte m_flag = (byte)component.Height;
				if (component.Status == 0 && component.Climbing)
					m_flag |= 0x80;
				if (component.Rized) // Only for towers
					m_flag |= 0x10;
				if (LevelUp)
					m_flag |= 0x20;
				if (!component.IsAlive)
					m_flag |= 0x40;
				pak.WriteByte(m_flag);
			}
			pak.WriteByte((byte)0);//unk
			SendTCP(pak);
		}

		public override void SendKeepClaim(AbstractGameKeep keep, byte flag)
		{
			if (m_gameClient.Player == null || keep == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepClaim));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteByte(flag);//0-Info,1-KeepTargetLevel,2-KeepLordType,4-Release
			pak.WriteByte((byte)keep.KeepType);//Keep Lord Type: 1-Melee,2-Magic,4-Stealth
			pak.WriteByte((byte)keep.TargetLevel);//target level not suported for moment
			pak.WriteByte((byte)keep.Level);
			SendTCP(pak);
		}

		public override void SendKeepComponentInteract(GameKeepComponent component)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentInteractResponse));

			pak.WriteShort((ushort)component.Keep.KeepID);
			pak.WriteByte((byte)component.Keep.Realm);
			pak.WriteByte(component.HealthPercent);
			pak.WriteByte((byte)component.Keep.Level);
			pak.WriteByte((byte)component.Keep.TargetLevel);
			//guild
			pak.WriteByte((byte)component.Keep.KeepType);

			if (component.Keep.Guild != null)
			{
				pak.WriteString(component.Keep.Guild.Name);
			}
			pak.WriteByte(0);
			SendTCP(pak);
		}
		public override void SendKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentHookpointUpdate));
			pak.WriteShort((ushort)component.Keep.KeepID);
			pak.WriteShort((ushort)component.ID);
			ArrayList freeHookpoints = new ArrayList();
			foreach (GameKeepHookPoint hookPt in component.HookPoints.Values)
			{
				if (hookPt.IsFree) freeHookpoints.Add(hookPt);
			}
			pak.WriteByte((byte)freeHookpoints.Count);
			pak.WriteByte((byte)selectedHookPointIndex);
			foreach (GameKeepHookPoint hookPt in freeHookpoints)//have to sort by index?
			{
				pak.WriteByte((byte)hookPt.ID);
			}
			SendTCP(pak);
		}
		public override void SendClearKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentHookpointUpdate));
			pak.WriteShort((ushort)component.Keep.KeepID);
			pak.WriteShort((ushort)component.ID);
			pak.WriteByte((byte)0);
			pak.WriteByte((byte)selectedHookPointIndex);
			SendTCP(pak);
		}

		public override void SendHookPointStore(GameKeepHookPoint hookPoint)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepComponentHookpointStore));

			pak.WriteShort((ushort)hookPoint.Component.Keep.KeepID);
			pak.WriteShort((ushort)hookPoint.Component.ID);
			pak.WriteShort((ushort)hookPoint.ID);
			pak.Fill(0x01, 3);
			HookPointInventory inventory;
			if (hookPoint.ID > 0x80) inventory = HookPointInventory.YellowHPInventory; //oil
			else if (hookPoint.ID > 0x60) inventory = HookPointInventory.GreenHPInventory;//big siege
			else if (hookPoint.ID > 0x40) inventory = HookPointInventory.LightGreenHPInventory; //small siege
			else if (hookPoint.ID > 0x20) inventory = HookPointInventory.BlueHPInventory;//npc
			else inventory = HookPointInventory.RedHPInventory;//guard

			pak.WriteByte((byte)inventory.GetAllItems().Count);//count
			pak.WriteShort(0);
			int i = 0;
			foreach (HookPointItem item in inventory.GetAllItems())
			{
				//TODO : must be quite like the merchant item.
				//the problem is to change how it is done maybe make the hookpoint item inherit from an interface in common with itemtemplate. have to think to that.
				pak.WriteByte((byte)i);
				i++;
				if (item.GameObjectType == "GameKeepGuard")//TODO: hack wrong must think how design thing to have merchante of gameobject(living or item)
					pak.WriteShort(0);
				else
					pak.WriteShort(item.Flag);
				pak.WriteShort(0);
				pak.WriteShort(0);
				pak.WriteShort(0);
				pak.WriteInt((uint)item.Gold);
				pak.WriteShort(item.Icon);
				pak.WritePascalString(item.Name);//item sell
			}
			SendTCP(pak);
		}

		protected override void SendQuestPacket(AbstractQuest quest, int index)
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
				if (desc.Length > ushort.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": description is too long for 1.68+ clients (" + desc.Length + ") '" + desc + "'");
					desc = desc.Substring(0, ushort.MaxValue);
				}

				pak.WriteByte((byte)name.Length);
				pak.WriteShort((ushort)desc.Length);
				pak.WriteStringBytes(name); //Write Quest Name without trailing 0
				pak.WriteStringBytes(desc); //Write Quest Description without trailing 0
			}
			SendTCP(pak);
		}
		public override void SendWarmapUpdate(IList list)
		{
			if (m_gameClient.Player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.WarMapClaimedKeeps));
			int KeepCount = 0;
			int TowerCount = 0;
			foreach (AbstractGameKeep keep in list)
			{
				if (keep is GameKeep)
					KeepCount++;
				else
					TowerCount++;
			}
			pak.WriteShort(0);
			pak.WriteByte((byte)KeepCount);
			pak.WriteByte((byte)TowerCount);
			pak.WriteShort(0); // ?
			pak.WriteShort(0); // ?
			pak.WriteShort(0); // ?
			foreach (AbstractGameKeep keep in list)
			{
				int id = keep.KeepID & 0xFF;
				int tower = keep.KeepID >> 8;
				int map = (id - 25) / 25;
				int index = id - (map * 25 + 25);
				int flag = keep.Realm; // 3 bits
				Guild guild = keep.Guild;
				string name = "";
				pak.WriteByte((byte)((map << 6) | (index << 3) | tower));
				if (guild != null)
				{
					flag |= 0x04; // claimed
					name = guild.Name;
				}
				//Teleport
				//gms with debug mode on can see every keep
				if (m_gameClient.Account.PrivLevel > 1 && m_gameClient.Player.TempProperties.getObjectProperty(GamePlayer.DEBUG_MODE_PROPERTY, null) != null)
					flag |= 0x10;
				else
				{
					//lets let players only teleport from the stones
					if (m_gameClient.Player.Realm == keep.Realm)
					{
						bool good = true;
						GameKeep theKeep = keep as GameKeep;
						if (theKeep == null)
							good = false;
						else
						{
							foreach (GameKeepTower t in theKeep.Towers)
							{
								if (t.Realm != theKeep.Realm)
								{
									good = false;
									break;
								}
							}
						}
						if (good)
							flag |= 0x10;
					}
					if (keep.InCombat)
						flag |= 0x08;
				}
				pak.WriteByte((byte)flag);
				pak.WritePascalString(name);
			}
			SendTCP(pak);
		}
		public override void SendWarmapBonuses()
		{
			if (m_gameClient.Player == null) return;
			int AlbTowers = 0;
			int MidTowers = 0;
			int HibTowers = 0;
			int AlbKeeps = 0;
			int MidKeeps = 0;
			int HibKeeps = 0;
			int OwnerDFTowers = 0;
			eRealm OwnerDF = eRealm.None;

			foreach (AbstractGameKeep keep in KeepMgr.getNFKeeps())
			{
				if (keep is GameKeep)
				{
					switch ((eRealm)keep.Realm)
					{
						case eRealm.Albion: AlbKeeps++; break;
						case eRealm.Midgard: MidKeeps++; break;
						case eRealm.Hibernia: HibKeeps++; break;
						default:
							break;
					}
				}
				else
				{
					switch ((eRealm)keep.Realm)
					{
						case eRealm.Albion: AlbTowers++; break;
						case eRealm.Midgard: MidTowers++; break;
						case eRealm.Hibernia: HibTowers++; break;
						default:
							break;
					}
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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.WarmapBonuses));

			pak.WriteByte((byte)RealmKeeps);
			int magic = RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Magic);
			int strength = RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Strength);
			byte relics = (byte)(magic << 4 | strength);
			pak.WriteByte(relics);
			pak.WriteByte((byte)OwnerDF);
			SendTCP(pak);
		}
	}
}
