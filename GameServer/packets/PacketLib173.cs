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
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Effects;
using DOL.GS.Quests;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(173, GameClient.eClientVersion.Version173)]
	public class PacketLib173 : PacketLib172
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.73 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib173(GameClient client) : base(client)
		{
		}

		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.UpdateIcons));
			long initPos = pak.Position;

			int fxcount = 0;
			int entriesCount = 0;
			lock (m_gameClient.Player.EffectList)
			{
				pak.WriteByte(0);	// effects count set in the end
				pak.WriteByte(0);	// unknown
				pak.WriteByte(0);	// unknown
				pak.WriteByte(0);	// unknown
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
							continue;
//						log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
						pak.WriteByte((byte)(fxcount-1)); // icon index
						pak.WriteByte((effect is GameSpellEffect) ? (byte)(fxcount-1) : (byte)0xff);
						byte ImmunByte = 0;
						if (effect is GameSpellAndImmunityEffect)
  						{
    						GameSpellAndImmunityEffect immunity = (GameSpellAndImmunityEffect)effect;
     						if (immunity.ImmunityState) ImmunByte = 1;
						}
						pak.WriteByte(ImmunByte); // new in 1.73; if non zero says "protected by" on right click
						// bit 0x08 adds "more..." to right click info
						pak.WriteShort(effect.Icon);
						pak.WriteShort((ushort)(effect.RemainingTime/1000));
						pak.WriteShort(effect.InternalID);      // reference for shift+i or cancel spell
						pak.WritePascalString(effect.Name);
						entriesCount++;
					}
				}

				int oldCount = lastUpdateEffectsCount;
				lastUpdateEffectsCount = fxcount;
				while (oldCount > fxcount)
				{
					pak.WriteByte((byte)(fxcount++));
					pak.Fill(0, 9);
					entriesCount++;
//					log.DebugFormat("adding [{0}] (empty)", fxcount-1);
				}

				if (changedEffects != null)
					changedEffects.Clear();

				if (entriesCount == 0)
					return; // nothing changed - no update is needed

				pak.Position = initPos;
				pak.WriteByte((byte)entriesCount);
				pak.Seek(0, SeekOrigin.End);

				SendTCP(pak);
//				log.Debug("packet sent.");
			}
			return;
		}

		public override void SendRegions()
		{
			RegionEntry[] entries = WorldMgr.GetRegionList();

			if(entries==null) return;
			int index = 0;
			int num = 0;
			int count = entries.Length;
			while(entries!=null && count > index)
			{
				GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ClientRegions));
				for(int i=0;i<4;i++)
				{
					while (index < count && m_gameClient.ClientType <= entries[index].expansion)
					{
						index++;
					}

					if(index >= count)
					{	//If we have no more entries
						pak.Fill(0x0,52);
					}
					else
					{
						pak.WriteByte((byte)(++num));
						pak.WriteByte((byte)entries[index].id);
						pak.FillString(entries[index].name,20);
						pak.FillString(entries[index].fromPort,5);
						pak.FillString(entries[index].toPort,5);
						//Try to fix the region ip so UDP is enabled!
						string ip = entries[index].ip;
						if(ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
							ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
						pak.FillString(ip, 20);

//						DOLConsole.WriteLine(string.Format(" ip={3}; fromPort={1}; toPort={2}; num={4}; id={0}; region name={5}", entries[index].id, entries[index].fromPort, entries[index].toPort, entries[index].ip, num, entries[index].name));
						index++;
					}
				}
				SendTCP(pak);
			}
		}


		public override void SendCharacterOverview(eRealm realm)
		{
			int firstAccountSlot;
			switch (realm)
			{
				case eRealm.Albion  : firstAccountSlot = 100; break;
				case eRealm.Midgard : firstAccountSlot = 200; break;
				case eRealm.Hibernia: firstAccountSlot = 300; break;
				default: throw new Exception("CharacterOverview requested for unknown realm "+realm);
			}

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterOverview));
			pak.FillString(m_gameClient.Account.Name,24);
			InventoryItem[] items;
			Character[] characters = m_gameClient.Account.Characters;
			if(characters==null)
			{
				pak.Fill(0x0,1840);
			}
			else
			{
				for(int i=firstAccountSlot;i<firstAccountSlot+10;i++)
				{
					bool written = false;
					for(int j = 0 ; j < characters.Length && written==false ; j++)
						if(characters[j].AccountSlot==i)
						{
							pak.FillString(characters[j].Name,24);
							items = (InventoryItem[]) GameServer.Database.SelectObjects(typeof(InventoryItem),"OwnerID = '"+characters[j].ObjectId+"'");
							byte ExtensionTorso = 0;
							byte ExtensionGloves = 0;
							byte ExtensionBoots = 0;
							foreach(InventoryItem item in items)
							{
								switch (item.SlotPosition)
								{
									case 22:
										ExtensionGloves = item.Extension;
										break;
									case 23:
										ExtensionBoots = item.Extension;
										break;
									case 25:
										ExtensionTorso = item.Extension;
										break;
									default:
										break;
								}
							}

							pak.WriteByte(0x01);
							pak.WriteByte((byte) characters[j].EyeSize);
							pak.WriteByte((byte) characters[j].LipSize);
							pak.WriteByte((byte) characters[j].EyeColor);
							pak.WriteByte((byte) characters[j].HairColor);
							pak.WriteByte((byte) characters[j].FaceType);
							pak.WriteByte((byte) characters[j].HairStyle);
							pak.WriteByte((byte) ((ExtensionBoots << 4) | ExtensionGloves));
							pak.WriteByte((byte) ((ExtensionTorso << 4) | (characters[j].IsCloakHoodUp ? 0x1 : 0x0)));
							pak.WriteByte((byte) characters[j].CustomisationStep); //1 = auto generate config, 2= config ended by player, 3= enable config to player
							pak.Fill(0x0, 14); //0 String


							Region reg = WorldMgr.GetRegion((ushort)characters[j].Region);
							Zone zon = null;
							if(reg!=null) zon = reg.GetZone(characters[j].Xpos,characters[j].Ypos);
							if(zon!=null)
								pak.FillString(zon.Description,24);
							else
								pak.Fill(0x0,24); //No known location

							if (characters[j].Class == 0)
								pak.FillString("" ,24); //Class name
							else
								pak.FillString(((eCharacterClass)characters[j].Class).ToString(),24); //Class name

    						pak.FillString(GamePlayer.RACENAMES[characters[j].Race],24);
							pak.WriteByte((byte)characters[j].Level);
							pak.WriteByte((byte)characters[j].Class);
							pak.WriteByte((byte)characters[j].Realm);
							pak.WriteByte((byte)((((characters[j].Race & 0xF0) << 2)+(characters[j].Race & 0x0F)) | (characters[j].Gender << 4)));
							pak.WriteShortLowEndian((ushort)characters[j].CurrentModel);
							pak.WriteByte((byte)characters[j].Region);
							pak.WriteByte(0x0); //second byte of region, currently unused
							pak.WriteInt(0x0);  //Unknown, last used?
							pak.WriteByte((byte)characters[j].Strength);
							pak.WriteByte((byte)characters[j].Dexterity);
							pak.WriteByte((byte)characters[j].Constitution);
							pak.WriteByte((byte)characters[j].Quickness);
							pak.WriteByte((byte)characters[j].Intelligence);
							pak.WriteByte((byte)characters[j].Piety);
							pak.WriteByte((byte)characters[j].Empathy);
							pak.WriteByte((byte)characters[j].Charisma);

							int found=0;
							//16 bytes: armor model
							for (int k=0x15;k<0x1D;k++)
							{
								found=0;
								foreach(InventoryItem item in items)
								{
									if (item.SlotPosition==k && found==0)
									{
										pak.WriteShortLowEndian((ushort)item.Model);
										found=1;
									}
								}
								if (found==0)
									pak.WriteShort(0x00);
							}
							//16 bytes: armor color
							for (int k=0x15;k<0x1D;k++)
							{
								int l;
								if (k==0x15+3)
									//shield emblem
									l=(int)eInventorySlot.LeftHandWeapon;
								else
									l=k;

								found=0;
								foreach(InventoryItem item in items)
								{
									if (item.SlotPosition==l && found==0)
									{
										if(item.Emblem != 0)
											pak.WriteShortLowEndian((ushort)item.Emblem);
										else
											pak.WriteShortLowEndian((ushort)item.Color);
										found=1;
									}
								}
								if (found==0)
									pak.WriteShort(0x00);
							}
							//8 bytes: weapon model
							for (int k=0x0A;k<0x0E;k++)
							{
								found=0;
								foreach(InventoryItem item in items)
								{
									if (item.SlotPosition==k && found==0)
									{
										pak.WriteShortLowEndian((ushort)item.Model);
										found=1;
									}
								}
								if (found==0)
									pak.WriteShort(0x00);
							}
							if(characters[j].ActiveWeaponSlot==(byte)GameLiving.eActiveWeaponSlot.TwoHanded)
							{
								pak.WriteByte(0x02);
								pak.WriteByte(0x02);
							}
							else if(characters[j].ActiveWeaponSlot==(byte)GameLiving.eActiveWeaponSlot.Distance)
							{
								pak.WriteByte(0x03);
								pak.WriteByte(0x03);
							}
							else
							{
								byte righthand = 0xFF;
								byte lefthand = 0xFF;
								foreach(InventoryItem item in items)
								{
									if (item.SlotPosition==(int)eInventorySlot.RightHandWeapon)
										righthand=0x00;
									if (item.SlotPosition==(int)eInventorySlot.LeftHandWeapon)
										lefthand=0x01;
								}
								if (righthand==lefthand)
								{
									if(characters[j].ActiveWeaponSlot==(byte)GameLiving.eActiveWeaponSlot.TwoHanded)
										righthand=lefthand=0x02;
									else if(characters[j].ActiveWeaponSlot==(byte)GameLiving.eActiveWeaponSlot.Distance)
										righthand=lefthand=0x03;
								}
								pak.WriteByte(righthand);
								pak.WriteByte(lefthand);
							}
							pak.WriteByte(0x00); //0x01=char in SI zone, classic client can't "play"
							pak.WriteByte(0x00);
							//pak.Fill(0x00,2);
							written=true;
						}
					if(written==false)
						pak.Fill(0x0,184);
				}
//				pak.Fill(0x0,184); //Slot 9
//				pak.Fill(0x0,184); //Slot 10
			}
			pak.Fill(0x0,0x82); //Don't know why so many trailing 0's | Corillian: Cuz they're stupid like that ;)

			SendTCP(pak);
		}
		public override void SendKeepInfo(AbstractGameKeep keep)
		{
			if (m_gameClient.Player==null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepInfo));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteShort(0);
			pak.WriteInt((uint)keep.X);
			pak.WriteInt((uint)keep.Y);
			pak.WriteShort((ushort)keep.Heading);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);//level
			pak.WriteShort(0);//unk
			pak.WriteByte(0x52);//model
			pak.WriteByte(0);//unk

			SendTCP(pak);
		}

		public override void SendNPCsQuestEffect(GameNPC npc, bool flag)
		{
			if (m_gameClient.Player == null || npc == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VisualEffect));

			pak.WriteShort((ushort)npc.ObjectID);
			pak.WriteByte(0x7); // Quest visual effect
			pak.WriteByte((byte)(flag ? 1 : 0));
			pak.WriteInt(0);

			SendTCP(pak);
		}

		public override void SendMessage(string msg, eChatType type, eChatLoc loc)
		{
			if (m_gameClient.ClientState == GameClient.eClientState.CharScreen)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Message));
			pak.WriteShort(0xFFFF);
			pak.WriteShort((ushort) m_gameClient.SessionID);
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

		public override void SendQuestUpdate(AbstractQuest quest)
		{
			int questIndex = 1;
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

		public override void SendQuestListUpdate()
		{
			int questIndex = 0;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));
			pak.WriteInt(0);
			SendTCP(pak);
			questIndex++;
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

		public override void SendRegionChanged()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.RegionChanged));
			pak.WriteShort(m_gameClient.Player.CurrentRegionID);
			pak.WriteShort(0x00); // Zone ID?
			pak.WriteShort(0x00); // ?
			pak.WriteShort(0x01); // cause region change ?
			SendTCP(pak);
		}
	}
}
