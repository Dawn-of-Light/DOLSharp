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
using DOL.GS.Database;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Effects;
using DOL.GS.Quests;
using NHibernate.Expression;
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
				
				for(int i = 0 ; i < 10 ; i++)
				{
					if(currentChar != null && currentChar.SlotPosition == i)
					{
						pak.FillString(currentChar.Name, 24);

						pak.WriteByte(0x01);
						pak.WriteByte((byte) currentChar.EyeSize);
						pak.WriteByte((byte) currentChar.LipSize);
						pak.WriteByte((byte) currentChar.EyeColor);
						pak.WriteByte((byte) currentChar.HairColor);
						pak.WriteByte((byte) currentChar.FaceType);
						pak.WriteByte((byte) currentChar.HairStyle);

						byte extensionFeet = 0;
						byte extensionHands = 0;
						byte extensionTorso = 0;
 
						VisibleEquipment currentItem = (VisibleEquipment)currentChar.Inventory.GetItem(eInventorySlot.FeetArmor);
						if(currentItem != null && currentItem is Armor) extensionFeet = ((Armor)currentItem).ModelExtension;
						currentItem = (Armor)currentChar.Inventory.GetItem(eInventorySlot.HandsArmor);
						if(currentItem != null && currentItem is Armor) extensionHands = ((Armor)currentItem).ModelExtension;
						currentItem = (Armor)currentChar.Inventory.GetItem(eInventorySlot.TorsoArmor);
						if(currentItem != null && currentItem is Armor) extensionTorso = ((Armor)currentItem).ModelExtension;

						pak.WriteByte((byte) ((extensionFeet << 4) | extensionHands));	
						pak.WriteByte((byte) ((extensionTorso << 4) | (currentChar.Inventory.IsCloakHoodUp ? 0x1 : 0x0)));
						pak.WriteByte((byte) currentChar.CustomisationStep); //1 = auto generate config, 2= config ended by player, 3= enable config to player
						pak.WriteByte((byte) currentChar.MoodType);
						pak.Fill(0x0, 13); //0 String

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
				pak.Fill(0x0, 0x82); //Don't know why so many trailing 0's | Corillian: Cuz they're stupid like that ;)
			}
			SendTCP(pak);
		}

		/*public override void SendKeepInfo(AbstractGameKeep keep)
		{
			if (m_gameClient.Player==null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.KeepInfo));

			pak.WriteShort((ushort)keep.KeepID);
			pak.WriteShort(0);
			Point pos = keep.Position;
			pak.WriteInt((uint)pos.X);
			pak.WriteInt((uint)pos.Y);
			pak.WriteShort((ushort)keep.Heading);
			pak.WriteByte((byte)keep.Realm);
			pak.WriteByte((byte)keep.Level);//level
			pak.WriteShort(0);//unk
			pak.WriteByte(0x52);//model
			pak.WriteByte(0);//unk

			SendTCP(pak);
		}*/

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
			lock (m_gameClient.Player.ActiveQuests)
			{
				foreach (AbstractQuest q in m_gameClient.Player.ActiveQuests)
				{
					if (q == quest)
					{
						SendQuestPacket(q, questIndex);
						break;
					}
					if (q.Step != 0) questIndex++;
				}
			}
		}

		public override void SendQuestListUpdate()
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));
			pak.WriteInt(0);
			SendTCP(pak);

			int questIndex = 1;
			lock (m_gameClient.Player.ActiveQuests)
			{
				IList questToClear = null;
				foreach (AbstractQuest quest in m_gameClient.Player.ActiveQuests)
				{
					if(quest.Step <= 0)
					{
						if(questToClear == null) questToClear = new ArrayList(1);
						questToClear.Add(quest);
					}
					else
					{
						SendQuestPacket(quest, questIndex);
						questIndex++;
					}
				}

				if(questToClear != null)
				{
					foreach(AbstractQuest quest in questToClear)
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
			pak.WriteShort((ushort) m_gameClient.Player.Region.RegionID);
			pak.WriteShort(0x00); // Zone ID?
			pak.WriteShort(0x00); // ?
			pak.WriteShort(0x01); // cause region change ?
			SendTCP(pak);
		}
	}
}
