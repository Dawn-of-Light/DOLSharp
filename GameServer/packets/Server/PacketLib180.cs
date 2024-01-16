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
using System.Reflection;

using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(180, GameClient.eClientVersion.Version180)]
	public class PacketLib180 : PacketLib179
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.80 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib180(GameClient client):base(client)
		{
		}

		public override void SendSetControlledHorse(GamePlayer player)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse)))
			{

				if (player.HasHorse)
				{
					pak.WriteShort(0); // for set self horse OID must be zero
					pak.WriteByte(player.ActiveHorse.ID);
					if (player.ActiveHorse.BardingColor == 0 && player.ActiveHorse.Barding != 0 && player.Guild != null)
					{
						int newGuildBitMask = (player.Guild.Emblem & 0x010000) >> 9;
						pak.WriteByte((byte)(player.ActiveHorse.Barding | newGuildBitMask));
						pak.WriteShort((ushort)player.Guild.Emblem);
					}
					else
					{
						pak.WriteByte(player.ActiveHorse.Barding);
						pak.WriteShort(player.ActiveHorse.BardingColor);
					}
					pak.WriteByte(player.ActiveHorse.Saddle);
					pak.WriteByte(player.ActiveHorse.SaddleColor);
					pak.WriteByte(player.ActiveHorse.Slots);
					pak.WriteByte(player.ActiveHorse.Armor);
					pak.WritePascalString(player.ActiveHorse.Name == null ? "" : player.ActiveHorse.Name);
				}
				else
				{
					pak.Fill(0x00, 8);
				}
				SendTCP(pak);
			}
		}

		public override void SendControlledHorse(GamePlayer player, bool flag)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse)))
			{
				if (!flag || !player.HasHorse)
				{
					pak.WriteShort((ushort)player.ObjectID);
					pak.Fill(0x00, 6);
				}
				else
				{
					pak.WriteShort((ushort)player.ObjectID);
					pak.WriteByte(player.ActiveHorse.ID);
					if (player.ActiveHorse.BardingColor == 0 && player.ActiveHorse.Barding != 0 && player.Guild != null)
					{
						int newGuildBitMask = (player.Guild.Emblem & 0x010000) >> 9;
						pak.WriteByte((byte)(player.ActiveHorse.Barding | newGuildBitMask));
						pak.WriteShort((ushort)player.Guild.Emblem);
					}
					else
					{
						pak.WriteByte(player.ActiveHorse.Barding);
						pak.WriteShort(player.ActiveHorse.BardingColor);
					}
	
					pak.WriteByte(player.ActiveHorse.Saddle);
					pak.WriteByte(player.ActiveHorse.SaddleColor);
				}
				SendTCP(pak);
			}
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
					log.Error("SendPlayerCreate: m_gameClient.Player == null");
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

				pak.WriteShort((ushort)playerToCreate.Client.SessionID);
				pak.WriteShort((ushort)playerToCreate.ObjectID);
				pak.WriteShort(playerToCreate.Model);
				pak.WriteShort((ushort)playerToCreate.Position.Z);
				//Dinberg:Instances - send out the 'fake' zone ID to the client for positioning purposes.
				pak.WriteShort(playerZone.ZoneSkinID);
				pak.WriteShort(GetXOffsetInZone(playerToCreate));
				pak.WriteShort(GetYOffsetInZone(playerToCreate));
				pak.WriteShort(playerToCreate.Orientation.InHeading);
	
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeSize)); //1-4 = Eye Size / 5-8 = Nose Size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.LipSize)); //1-4 = Ear size / 5-8 = Kin size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.MoodType)); //1-4 = Ear size / 5-8 = Kin size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeColor)); //1-4 = Skin Color / 5-8 = Eye Color
				pak.WriteByte(playerToCreate.GetDisplayLevel(m_gameClient.Player));
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairColor)); //Hair: 1-4 = Color / 5-8 = unknown
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.FaceType)); //1-4 = Unknown / 5-8 = Face type
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairStyle)); //1-4 = Unknown / 5-8 = Hair Style
	
				int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
				if (playerToCreate.IsAlive == false) flags |= 0x01;
				if (playerToCreate.IsUnderwater) flags |= 0x02; //swimming
				if (playerToCreate.IsStealthed) flags |= 0x10;
				if (playerToCreate.IsWireframe) flags |= 0x20;
				if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Vampiir) flags |= 0x40; //Vamp fly
				pak.WriteByte((byte)flags);
				pak.WriteByte(0x00); // new in 1.74
	
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
		}

		public override void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			if(pak.Length > 1500)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - first));
				pak.WriteByte( (byte)( first == 0 ? 99 : 0x03 ) ); //subtype
				pak.WriteByte((byte)first);
				SendTCP(pak);
				pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
				pak.WriteByte(0x01); //subcode
				pak.WriteByte((byte)maxSkills); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)first);
				first = maxSkills;
			}
			maxSkills++;
		}

		
		public override void SendUpdatePlayerSkills()
		{
			if (m_gameClient.Player == null)
				return;
			
			// Get Skills as "Usable Skills" which are in network order ! (with forced update)
			List<Tuple<Skill, Skill>> usableSkills = m_gameClient.Player.GetAllUsableSkills(true);
						
			bool sent = false; // set to true once we can't send packet anymore !
			int index = 0; // index of our position in the list !
			int total = usableSkills.Count; // cache List count.
			int packetCount = 0; // Number of packet sent for the entire list
			while (!sent)
			{
				int packetEntry = 0; // needed to tell client how much skill we send
				// using pak
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
				{
					// Write header
					pak.WriteByte(0x01); //subcode for skill
					pak.WriteByte((byte)0); //packet entries, can't know it for now...
					pak.WriteByte((byte)0x03); //subtype for following pages
					pak.WriteByte((byte)index); // packet first entry

					// getting pak filled
					while(index < total)
					{
						// this item will break the limit, send the packet before, keep index as is to continue !
						if ((index >= byte.MaxValue) || ((pak.Length + 8 + usableSkills[index].Item1.Name.Length) > 1400))
						{
							break;
						}
						
						// Enter Packet Values !! Format Level - Type - SpecialField - Bonus - Icon - Name
						Skill skill = usableSkills[index].Item1;
						Skill skillrelated = usableSkills[index].Item2;

						if (skill is Specialization)
						{
							Specialization spec = (Specialization)skill;
							pak.WriteByte((byte)Math.Max(1, spec.Level));
							pak.WriteByte((byte)spec.SkillType);
							pak.WriteShort(0);
							pak.WriteByte((byte)(m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
							pak.WriteShort((ushort)spec.Icon);
							pak.WritePascalString(spec.Name);
						}
						else if (skill is Ability)
						{							
							Ability ab = (Ability)skill;
							
							pak.WriteByte((byte)0);
							pak.WriteByte((byte)ab.SkillType);
							pak.WriteShort(0);
							pak.WriteByte((byte)0);
							pak.WriteShort((ushort)ab.Icon);
							pak.WritePascalString(ab.Name);							
						}
						else if (skill is Spell)
						{
							Spell spell = (Spell)skill;
							pak.WriteByte((byte)spell.Level);
							pak.WriteByte((byte)spell.SkillType);
							
							// spec index for this Spell - Special for Song and Unknown Indexes...
							int spin = 0;							
							if (spell.SkillType == eSkillPage.Songs)
							{
								spin = 0xFF;
							}
							else
							{
								// find this line Specialization index !
								if (skillrelated is SpellLine && !Util.IsEmpty(((SpellLine)skillrelated).Spec))
								{
									spin = usableSkills.FindIndex(sk => (sk.Item1 is Specialization) && ((Specialization)sk.Item1).KeyName == ((SpellLine)skillrelated).Spec);
									
									if (spin == -1)
										spin = 0xFE;
								}
								else
								{
									spin = 0xFE;
								}
							}
							
							pak.WriteShort((ushort)spin); // special index for spellline
							pak.WriteByte(0); // bonus
							pak.WriteShort(spell.InternalIconID > 0 ? spell.InternalIconID : spell.Icon); // icon
							pak.WritePascalString(spell.Name);
						}
						else if (skill is Style)
						{
							Style style = (Style)skill;
							pak.WriteByte((byte)style.SpecLevelRequirement);
							pak.WriteByte((byte)style.SkillType);
							
							// Special pre-requisite (First byte is Pre-requisite Icon / second Byte is prerequisite code...)
							int pre = 0;
				
							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = (int)style.AttackResultRequirement; // last result of our attack against enemy hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
									{
										// get style requirement value... find prerequisite style index from specs beginning...
										int styleindex = Math.Max(0, usableSkills.FindIndex(it => (it.Item1 is Style) && it.Item1.ID == style.OpeningRequirementValue));										
										int speccount = Math.Max(0, usableSkills.FindIndex(it => (it.Item1 is Specialization) == false));										
										pre |= ((byte)(100 + styleindex - speccount)) << 8;
									}
									break;
								case Style.eOpening.Defensive:
									pre = 100 + (int)style.AttackResultRequirement; // last result of enemies attack against us hit, miss, you block, you parry, ...
									break;
								case Style.eOpening.Positional:
									pre = 200 + style.OpeningRequirementValue;
									break;
							}
							
							// style required?
							if (pre == 0)
								pre = 0x100;
	
							pak.WriteShort((ushort)pre);
							pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // index specialization in bonus...
							pak.WriteShort((ushort)style.Icon);
							pak.WritePascalString(style.Name);
						}						
						
						packetEntry++;
						index++;
					}

					// test if we finished sending packets
					if (index >= total || index >= byte.MaxValue)
						sent = true;
					
					// rewrite header for count.
					pak.Position = 4;
					pak.WriteByte((byte)packetEntry);
					
					if (!sent)
						pak.WriteByte((byte)99);
					
					SendTCP(pak);
					
				}
				
				packetCount++;
			}
			
			// Send List Cast Spells...
			SendNonHybridSpellLines();
			// reset trainer cache
			m_gameClient.TrainerSkillCache = null;
		}
	}
}
