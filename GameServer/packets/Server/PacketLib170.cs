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
using System.Collections;
using System.Collections.Generic;

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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepInfo));

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

		public override void SendKeepRealmUpdate(AbstractGameKeep keep)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepRealmUpdate));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);
			SendTCP(pak);
		}

		public override void SendKeepRemove(AbstractGameKeep keep)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepRemove));

			pak.WriteShort((ushort)keep.KeepID);
			SendTCP(pak);
		}

		public override void SendKeepComponentInfo(GameKeepComponent keepComponent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentInfo));

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
			if (keepComponent.IsRaized) // Only for towers
				flag |= 0x04;
			if (flag == 0x00 && keepComponent.Climbing)
				flag = 0x02;
			pak.WriteByte(flag);
			pak.WriteByte(0x00); //unk
			SendTCP(pak);
		}
		public override void SendKeepComponentDetailUpdate(GameKeepComponent keepComponent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentDetailUpdate));

			pak.WriteShort((ushort)keepComponent.Keep.KeepID);
			pak.WriteShort((ushort)keepComponent.ID);
			pak.WriteByte((byte)keepComponent.Height);
			pak.WriteByte(keepComponent.HealthPercent);
			byte flag = keepComponent.Status;
			if (keepComponent.IsRaized) // Only for towers
				flag |= 0x04;
			if (flag == 0x00 && keepComponent.Climbing)
				flag = 0x02;
			pak.WriteByte(flag);
			pak.WriteByte(0x00);//unk
			SendTCP(pak);
		}
		public override void SendKeepComponentUpdate(AbstractGameKeep keep, bool LevelUp)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentUpdate));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);
			pak.WriteByte((byte)keep.KeepComponents.Count);
			foreach (GameKeepComponent component in keep.KeepComponents)
			{
				byte m_flag = (byte)component.Height;
				if (component.Status == 0 && component.Climbing)
					m_flag |= 0x80;
				if (component.IsRaized) // Only for towers
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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepClaim));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteByte(flag);//0-Info,1-KeepTargetLevel,2-KeepLordType,4-Release
			pak.WriteByte((byte)1); //Keep Lord Type: always melee, type is no longer used
			pak.WriteByte((byte)ServerProperties.Properties.MAX_KEEP_LEVEL);
			pak.WriteByte((byte)keep.Level);
			SendTCP(pak);
		}

		public override void SendKeepComponentInteract(GameKeepComponent component)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentInteractResponse));

			pak.WriteShort((ushort)component.Keep.KeepID);
			pak.WriteByte((byte)component.Keep.Realm);
			pak.WriteByte(component.HealthPercent);

			pak.WriteByte(component.Keep.EffectiveLevel(component.Keep.Level));
			pak.WriteByte(component.Keep.EffectiveLevel((byte)ServerProperties.Properties.MAX_KEEP_LEVEL));
			//guild
			pak.WriteByte((byte)1); //Keep Type: always melee here, type is no longer used

			if (component.Keep.Guild != null)
			{
				pak.WriteString(component.Keep.Guild.Name);
			}
			pak.WriteByte(0);
			SendTCP(pak);
		}
		public override void SendKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointUpdate));
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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointUpdate));
			pak.WriteShort((ushort)component.Keep.KeepID);
			pak.WriteShort((ushort)component.ID);
			pak.WriteByte((byte)0);
			pak.WriteByte((byte)selectedHookPointIndex);
			SendTCP(pak);
		}

		public override void SendHookPointStore(GameKeepHookPoint hookPoint)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointStore));

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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry));

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

		public override void SendWarmapUpdate(ICollection<AbstractGameKeep> list)
		{
			if (m_gameClient.Player == null) return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarMapClaimedKeeps));
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
				int id = keep.KeepID & 0xFF;
				int tower = keep.KeepID >> 8;
				int map = (id - 25) / 25;
				int index = id - (map * 25 + 25);
				int flag = (byte)keep.Realm; // 3 bits
				Guild guild = keep.Guild;
				string name = "";
				pak.WriteByte((byte)((map << 6) | (index << 3) | tower));
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
					if (m_gameClient.Player.CurrentRegionID == KeepMgr.NEW_FRONTIERS && m_gameClient.Player.Realm == keep.Realm)
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

			foreach (AbstractGameKeep keep in KeepMgr.GetNFKeeps())
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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarmapBonuses));

			pak.WriteByte((byte)RealmKeeps);
			int magic = RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Magic);
			int strength = RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Strength);
			byte relics = (byte)(magic << 4 | strength);
			pak.WriteByte(relics);
			pak.WriteByte((byte)OwnerDF);
			SendTCP(pak);
		}

		public override void SendWarmapDetailUpdate(List<List<byte>> fights, List<List<byte>> groups)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarMapDetailUpdate));
			pak.WriteByte((byte)fights.Count);// count - Fights (Byte)
			pak.WriteByte((byte)groups.Count);// count - Groups (Byte)
			// order first fights after than groups

			// zoneid  - byte // zoneid from zones.xml
			//			- A7 - Mid	- left			-	10100111		Map: Midgard
			//			- A8 - Mid	- middle		-	10101000			| X	|
			//			- A9 - Mid	- right			-	10101001		|x	  x		x	|
			//			- AA - Mid	- middle  - top	-	10101010
			//
			//			- AB - Hib	- top			-	10101011	171		Map: Hibernia
			//			- AC - Hib	- middle		-	10101100				|X		|
			//			- AD - Hib	- middle -left	-	10101101			|x	 x		|
			//			- AE - Hib	- bottom		-	10101110				|x		|

			//			- AF - Alb	- bottom		-	10101111			Map: Albion
			//			- B0 - Alb	- middle -right	-	10110000			|X	|
			//			- B1 - Alb	- middle -left	-	10110001			|x	 x	|
			//			- B2 - Alb	- top			-	10110010	178		|X	|

			// position   x/y offset  x<<4,y

			foreach (List<byte> obj in fights)
			{
				pak.WriteByte(obj[0]);// zoneid
				pak.WriteByte((byte)((obj[1] << 4) | (obj[2] & 0x0f))); // position
				pak.WriteByte(obj[3]);// color - ( Fights:  0x00 - Grey , 0x01 - RedBlue , 0x02 - RedGreen , 0x03 - GreenBlue )
				pak.WriteByte(obj[4]);// type  - ( Fights:  Size 0x00 - small  0x01 - medium  0x02 - big 0x03 - huge )
			}

			foreach (List<byte> obj in groups)
			{
				pak.WriteByte(obj[0]);// zoneid
				pak.WriteByte((byte)(obj[1] << 4 | obj[2])); // position
				byte realm = obj[3];

				pak.WriteByte((byte)((realm == 3) ? 0x04 : (realm == 2) ? 0x02 : 0x01));//	color   ( Groups:  0x01 - Alb  , 0x02 - Mid , 0x04 - Hib
				switch ((eRealm)obj[3])
				{
					//	type    ( Groups:	Alb:	type	   0x03,0x02,0x01	& 0x03
					//						Mid:	type << 2  0x0C,0x08,0x04 	& 0x03
					//						Hib:	type << 4  0x30,0x20,0x10	& 0x03  )
					case eRealm.Albion:
					default:
						pak.WriteByte(obj[4]);
						break;
					case eRealm.Midgard:
						pak.WriteByte((byte)(obj[4] << 2));
						break;
					case eRealm.Hibernia:
						pak.WriteByte((byte)(obj[4] << 4));
						break;
				}
			}

			SendTCP(pak);
		}
	}
}
