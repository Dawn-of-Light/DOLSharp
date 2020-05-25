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
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Spells;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DOL.GS.PacketHandler
{
	[PacketLib(1125, GameClient.eClientVersion.Version1125)]
	public class PacketLib1125 : PacketLib1124
	{

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.125
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1125(GameClient client)
			: base(client)
		{
		}

		/// <summary>
		/// 1125 cryptkey
		/// </summary>
		public override void SendVersionAndCryptKey()
		{
			//Construct the new packet
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CryptKey)))
			{
				pak.WritePascalStringIntLE((((int)m_gameClient.Version) / 1000) + "." + (((int)m_gameClient.Version) - 1000) + m_gameClient.MinorRev);
				//// Same as the trailing two bytes sent in first client to server packet
				pak.WriteByte(m_gameClient.MajorBuild); // last seen : 0x2A 0x07
				pak.WriteByte(m_gameClient.MinorBuild);
				SendTCP(pak);
			}
		}

		/// <summary>
		/// 1125 login granted		
		/// </summary>        
		public override void SendLoginGranted(byte color)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginGranted)))
			{
				pak.WritePascalString(m_gameClient.Account.Name);
				pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
				pak.WriteByte(0x05); //Server ID, seems irrelevant
				pak.WriteByte(color); // 00 normal type?, 01 mordred type, 03 gaheris type, 07 ywain type
				pak.WriteByte(0x00); // Trial switch 0x00 - subbed, 0x01 - trial acc 
				SendTCP(pak);
			}
		}

		/// <summary>
		/// 1125 sendrealm
		/// </summary>        
		public override void SendRealm(eRealm realm)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Realm)))
			{
				pak.WriteByte((byte)realm);
				pak.Fill(0, 12);
				SendTCP(pak);
			}
		}

		/// <summary>
		/// 1125 char overview
		/// </summary>        
		public override void SendCharacterOverview(eRealm realm)
		{
			if (realm < eRealm._FirstPlayerRealm || realm > eRealm._LastPlayerRealm)
			{
				throw new Exception("CharacterOverview requested for unknown realm " + realm);
			}

			int firstSlot = (byte)realm * 100;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterOverview)))
			{
				//pak.Fillstring(GameClient.Account.Name, 24);
				pak.Fill(0, 8);
				if (m_gameClient.Account.Characters == null)
				{
					pak.Fill(0x0, 10);
				}
				else
				{
					Dictionary<int, DOLCharacters> charsBySlot = new Dictionary<int, DOLCharacters>();
					foreach (DOLCharacters c in m_gameClient.Account.Characters)
					{
						try
						{
							charsBySlot.Add(c.AccountSlot, c);
						}
						catch (Exception ex)
						{
							log.Error("SendCharacterOverview - Duplicate char in slot? Slot: " + c.AccountSlot + ", Account: " + c.AccountName, ex);
						}
					}
					var itemsByOwnerID = new Dictionary<string, Dictionary<eInventorySlot, InventoryItem>>();

					if (charsBySlot.Any())
					{
						var allItems = GameServer.Database.SelectObjects<InventoryItem>("`OwnerID` = @OwnerID AND `SlotPosition` >= @MinEquipable AND `SlotPosition` <= @MaxEquipable",
																						charsBySlot.Select(kv => new[] { new QueryParameter("@OwnerID", kv.Value.ObjectId), new QueryParameter("@MinEquipable", (int)eInventorySlot.MinEquipable), new QueryParameter("@MaxEquipable", (int)eInventorySlot.MaxEquipable) }))
							.SelectMany(objs => objs);

						foreach (InventoryItem item in allItems)
						{
							try
							{
								if (!itemsByOwnerID.ContainsKey(item.OwnerID))
								{
									itemsByOwnerID.Add(item.OwnerID, new Dictionary<eInventorySlot, InventoryItem>());
								}

								itemsByOwnerID[item.OwnerID].Add((eInventorySlot)item.SlotPosition, item);
							}
							catch (Exception ex)
							{
								log.Error("SendCharacterOverview - Duplicate item on character? OwnerID: " + item.OwnerID + ", SlotPosition: " + item.SlotPosition + ", Account: " + m_gameClient.Account.Name, ex);
							}
						}
					}

					for (int i = firstSlot; i < (firstSlot + 10); i++)
					{
						if (!charsBySlot.TryGetValue(i, out DOLCharacters c))
						{
							pak.WriteByte(0);
						}
						else
						{

							if (!itemsByOwnerID.TryGetValue(c.ObjectId, out Dictionary<eInventorySlot, InventoryItem> charItems))
							{
								charItems = new Dictionary<eInventorySlot, InventoryItem>();
							}

							byte extensionTorso = 0;
							byte extensionGloves = 0;
							byte extensionBoots = 0;


							if (charItems.TryGetValue(eInventorySlot.TorsoArmor, out InventoryItem item))
							{
								extensionTorso = item.Extension;
							}

							if (charItems.TryGetValue(eInventorySlot.HandsArmor, out item))
							{
								extensionGloves = item.Extension;
							}

							if (charItems.TryGetValue(eInventorySlot.FeetArmor, out item))
							{
								extensionBoots = item.Extension;
							}

							pak.WriteByte((byte)c.Level); // moved
							pak.WritePascalStringIntLE(c.Name);
							pak.WriteByte(0x18); // no idea
							pak.WriteInt(1); // no idea
							pak.WriteByte((byte)c.EyeSize);
							pak.WriteByte((byte)c.LipSize);
							pak.WriteByte((byte)c.EyeColor);
							pak.WriteByte((byte)c.HairColor);
							pak.WriteByte((byte)c.FaceType);
							pak.WriteByte((byte)c.HairStyle);
							pak.WriteByte((byte)((extensionBoots << 4) | extensionGloves));
							pak.WriteByte((byte)((extensionTorso << 4) | (c.IsCloakHoodUp ? 0x1 : 0x0)));
							pak.WriteByte((byte)c.CustomisationStep); //1 = auto generate config, 2= config ended by player, 3= enable config to player
							pak.WriteByte((byte)c.MoodType);
							pak.Fill(0x0, 13); //0 string

							string locationDescription = string.Empty;
							Region region = WorldMgr.GetRegion((ushort)c.Region);
							if (region != null)
							{
								locationDescription = region.GetTranslatedSpotDescription(m_gameClient, c.Xpos, c.Ypos, c.Zpos);
							}
							if (locationDescription.Length > 23) // location name over 23 chars has to be truncated eg. "The Great Pyramid of Stygia"
							{
								locationDescription = (locationDescription.Substring(0, 20)) + "...";
							}
							pak.WritePascalStringIntLE(locationDescription);

							string classname = "";
							if (c.Class != 0)
							{
								classname = ((eCharacterClass)c.Class).ToString();
							}
							pak.WritePascalStringIntLE(classname);

							string racename = m_gameClient.RaceToTranslatedName(c.Race, c.Gender);

							pak.WritePascalStringIntLE(racename);
							pak.WriteShortLowEndian((ushort)c.CurrentModel); // moved
																			 // something here
							pak.WriteByte((byte)c.Region);

							if (region == null || (int)m_gameClient.ClientType > region.Expansion)
							{
								pak.WriteByte(0x00);
							}
							else
							{
								pak.WriteByte((byte)(region.Expansion + 1)); //0x04-Cata zone, 0x05 - DR zone
							}

							charItems.TryGetValue(eInventorySlot.RightHandWeapon, out InventoryItem rightHandWeapon);
							charItems.TryGetValue(eInventorySlot.LeftHandWeapon, out InventoryItem leftHandWeapon);
							charItems.TryGetValue(eInventorySlot.TwoHandWeapon, out InventoryItem twoHandWeapon);
							charItems.TryGetValue(eInventorySlot.DistanceWeapon, out InventoryItem distanceWeapon);
							charItems.TryGetValue(eInventorySlot.HeadArmor, out InventoryItem helmet);
							charItems.TryGetValue(eInventorySlot.HandsArmor, out InventoryItem gloves);
							charItems.TryGetValue(eInventorySlot.FeetArmor, out InventoryItem boots);
							charItems.TryGetValue(eInventorySlot.TorsoArmor, out InventoryItem torso);
							charItems.TryGetValue(eInventorySlot.Cloak, out InventoryItem cloak);
							charItems.TryGetValue(eInventorySlot.LegsArmor, out InventoryItem legs);
							charItems.TryGetValue(eInventorySlot.ArmsArmor, out InventoryItem arms);

							pak.WriteShortLowEndian((ushort)(helmet != null ? helmet.Model : 0));
							pak.WriteShortLowEndian((ushort)(gloves != null ? gloves.Model : 0));
							pak.WriteShortLowEndian((ushort)(boots != null ? boots.Model : 0));

							ushort rightHandColor = 0;
							if (rightHandWeapon != null)
							{
								rightHandColor = (ushort)(rightHandWeapon.Emblem != 0 ? rightHandWeapon.Emblem : rightHandWeapon.Color);
							}
							pak.WriteShortLowEndian(rightHandColor);

							pak.WriteShortLowEndian((ushort)(torso != null ? torso.Model : 0));
							pak.WriteShortLowEndian((ushort)(cloak != null ? cloak.Model : 0));
							pak.WriteShortLowEndian((ushort)(legs != null ? legs.Model : 0));
							pak.WriteShortLowEndian((ushort)(arms != null ? arms.Model : 0));

							ushort helmetColor = 0;
							if (helmet != null)
							{
								helmetColor = (ushort)(helmet.Emblem != 0 ? helmet.Emblem : helmet.Color);
							}
							pak.WriteShortLowEndian(helmetColor);

							ushort glovesColor = 0;
							if (gloves != null)
							{
								glovesColor = (ushort)(gloves.Emblem != 0 ? gloves.Emblem : gloves.Color);
							}
							pak.WriteShortLowEndian(glovesColor);

							ushort bootsColor = 0;
							if (boots != null)
							{
								bootsColor = (ushort)(boots.Emblem != 0 ? boots.Emblem : boots.Color);
							}
							pak.WriteShortLowEndian(bootsColor);

							ushort leftHandWeaponColor = 0;
							if (leftHandWeapon != null)
							{
								leftHandWeaponColor = (ushort)(leftHandWeapon.Emblem != 0 ? leftHandWeapon.Emblem : leftHandWeapon.Color);
							}
							pak.WriteShortLowEndian(leftHandWeaponColor);

							ushort torsoColor = 0;
							if (torso != null)
							{
								torsoColor = (ushort)(torso.Emblem != 0 ? torso.Emblem : torso.Color);
							}
							pak.WriteShortLowEndian(torsoColor);

							ushort cloakColor = 0;
							if (cloak != null)
							{
								cloakColor = (ushort)(cloak.Emblem != 0 ? cloak.Emblem : cloak.Color);
							}
							pak.WriteShortLowEndian(cloakColor);

							ushort legsColor = 0;
							if (legs != null)
							{
								legsColor = (ushort)(legs.Emblem != 0 ? legs.Emblem : legs.Color);
							}
							pak.WriteShortLowEndian(legsColor);

							ushort armsColor = 0;
							if (arms != null)
							{
								armsColor = (ushort)(arms.Emblem != 0 ? arms.Emblem : arms.Color);
							}
							pak.WriteShortLowEndian(armsColor);

							//weapon models

							pak.WriteShortLowEndian((ushort)(rightHandWeapon != null ? rightHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(leftHandWeapon != null ? leftHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(twoHandWeapon != null ? twoHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(distanceWeapon != null ? distanceWeapon.Model : 0));

							//pak.WriteInt(0x0); // Internal database ID
							pak.WriteByte((byte)c.Strength);
							pak.WriteByte((byte)c.Dexterity);
							pak.WriteByte((byte)c.Constitution);
							pak.WriteByte((byte)c.Quickness);
							pak.WriteByte((byte)c.Intelligence);
							pak.WriteByte((byte)c.Piety);
							pak.WriteByte((byte)c.Empathy);
							pak.WriteByte((byte)c.Charisma);
							pak.WriteByte((byte)c.Class); // moved
							pak.WriteByte((byte)c.Realm); // moved                            
							pak.WriteByte((byte)((((c.Race & 0x10) << 2) + (c.Race & 0x0F)) | (c.Gender << 4)));

							if (c.ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.TwoHanded)
							{
								pak.WriteByte(0x02);
								pak.WriteByte(0x02);
							}
							else if (c.ActiveWeaponSlot == (byte)GameLiving.eActiveWeaponSlot.Distance)
							{
								pak.WriteByte(0x03);
								pak.WriteByte(0x03);
							}
							else
							{
								byte righthand = 0xFF;
								byte lefthand = 0xFF;

								if (rightHandWeapon != null)
								{
									righthand = 0x00;
								}

								if (leftHandWeapon != null)
								{
									lefthand = 0x01;
								}

								pak.WriteByte(righthand);
								pak.WriteByte(lefthand);
							}

							if (region == null || region.Expansion != 1)
							{
								pak.WriteByte(0x00);
							}
							else
							{
								pak.WriteByte(0x01); //0x01=char in SI zone, classic client can't "play"
							}

							pak.WriteByte((byte)c.Constitution);
						}
					}
				}

				SendTCP(pak);
			}
		}

		/// <summary>
		/// 1125 UDPinit reply
		/// </summary>
		public override void SendUDPInitReply()
		{
			using (var pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.UDPInitReply)))
			{

				if (!m_gameClient.Socket.Connected) // not using RC4, wont accept UDP packets anyway.
				{
					return;
				}

				ulong datetimenow = (ulong)DateTime.Now.Ticks >> 24; //shift 24 bits to match live value
				pak.WriteLongLowEndian(datetimenow);
				SendUDP(pak, true);
			}
		}

		public override void SendGroupWindowUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x06); // subcode - player group window
									 // a 06 00 packet is sent when logging in.
				var group = m_gameClient.Player.Group;
				if (group == null)
				{
					pak.WriteByte(0x00); // a 06 00 packet is sent when logging in.
				}
				else
				{
					pak.WriteByte(group.MemberCount);
					foreach (GameLiving living in group.GetMembersInTheGroup())
					{
						pak.WritePascalString(living.Name);
						pak.WritePascalString(living is GamePlayer ? ((GamePlayer)living).CharacterClass.Name : "NPC");
						pak.WriteShort((ushort)living.ObjectID); //or session id?
						pak.WriteByte(living.Level);
					}
				}

				SendTCP(pak);
			}
		}

		protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, bool updateMap, GameLiving living)
		{
			pak.WriteByte((byte)(0x20 | living.GroupIndex)); // From 1 to 8 // 0x20 is player status code
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

			pak.WriteByte(player?.CharacterClass?.HealthPercentGroupWindow ?? living.HealthPercent);
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

			if (updateMap)
				WriteGroupMemberMapUpdate(pak, living);

			if (updateIcons)
			{
				pak.WriteByte((byte)(0x80 | living.GroupIndex));
				lock (living.EffectList)
				{
					pak.WriteByte((byte)living.EffectList.OfType<GameSpellEffect>().Count());
					foreach (var effect in living.EffectList)
					{
						if (effect is GameSpellEffect)
						{
							pak.WriteByte(0);
							pak.WriteShort(effect.Icon);
						}
					}
				}
			}
		}

		/// <summary>
		/// 1125d+ Market Explorer
		/// </summary>
		public override void SendMarketExplorerWindow(IList<InventoryItem> items, byte page, byte maxpage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MarketExplorerWindow)))
			{
				pak.WriteByte((byte)items.Count);
				pak.WriteByte(page);
				pak.WriteByte(maxpage);
				pak.WriteByte(0);
				foreach (InventoryItem item in items)
				{
					pak.WriteByte((byte)items.IndexOf(item));
					pak.WriteByte((byte)item.Level);
					int value1; // some object types use this field to display count
					int value2; // some object types use this field to display count
					switch (item.Object_Type)
					{
						case (int)eObjectType.Arrow:
						case (int)eObjectType.Bolt:
						case (int)eObjectType.Poison:
						case (int)eObjectType.GenericItem:
							value1 = item.PackSize;
							value2 = item.SPD_ABS; break;
						case (int)eObjectType.Thrown:
							value1 = item.DPS_AF;
							value2 = item.PackSize; break;
						case (int)eObjectType.Instrument:
							value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF); // 0x00 = Lute ; 0x01 = Drum ; 0x03 = Flute
							value2 = 0; break; // unused
						case (int)eObjectType.Shield:
							value1 = item.Type_Damage;
							value2 = item.DPS_AF; break;
						case (int)eObjectType.GardenObject:
						case (int)eObjectType.HouseWallObject:
						case (int)eObjectType.HouseFloorObject:
							value1 = 0;
							value2 = item.SPD_ABS; break;
						default:
							value1 = item.DPS_AF;
							value2 = item.SPD_ABS; break;
					}
					pak.WriteByte((byte)value1);
					pak.WriteByte((byte)value2);
					if (item.Object_Type == (int)eObjectType.GardenObject)
					{
						pak.WriteByte((byte)(item.DPS_AF));
					}
					else
					{
						pak.WriteByte((byte)(item.Hand << 6));
					}

					pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
					pak.WriteByte((byte)(m_gameClient.Player.HasAbilityToUseItem(item.Template) ? 0 : 1));
					pak.WriteShortLowEndian((ushort)(item.PackSize > 1 ? item.Weight * item.PackSize : item.Weight)); // 

					pak.WriteByte((byte)item.ConditionPercent);
					pak.WriteByte((byte)item.DurabilityPercent);
					pak.WriteByte((byte)item.Quality);
					pak.WriteByte((byte)item.Bonus);
					pak.WriteShortLowEndian((ushort)item.Model);

					if (item.Emblem != 0)
					{
						pak.WriteShortLowEndian((ushort)item.Emblem); // untested for low endian but probabaly
					}
					else
					{
						pak.WriteShortLowEndian((ushort)item.Color); // untested for low endian but probabaly
					}

					pak.WriteShortLowEndian((byte)item.Effect); // untested for low endian but probabaly
					pak.WriteShortLowEndian(item.OwnerLot);//lot
					pak.WriteIntLowEndian((uint)item.SellPrice);

					if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					{
						string bpPrice = "";
						if (item.SellPrice > 0)
						{
							bpPrice = "[" + item.SellPrice.ToString() + " BP";
						}

						if (item.Count > 1)
						{
							pak.WritePascalStringIntLE(item.Count + " " + item.Name);
						}
						else if (item.PackSize > 1)
						{
							pak.WritePascalStringIntLE(item.PackSize + " " + item.Name + bpPrice);
						}
						else
						{
							pak.WritePascalStringIntLE(item.Name + bpPrice);
						}
					}
					else
					{
						if (item.Count > 1)
						{
							pak.WritePascalStringIntLE(item.Count + " " + item.Name);
						}
						else if (item.PackSize > 1)
						{
							pak.WritePascalStringIntLE(item.PackSize + " " + item.Name);
						}
						else
						{
							pak.WritePascalStringIntLE(item.Name);
						}
					}
				}

				SendTCP(pak);
			}
		}

		/// <summary>
		/// 1125d+ Merchant window
		/// </summary>  
		public override void SendMerchantWindow(MerchantTradeItems tradeItemsList, eMerchantWindowType windowType)
		{
			if (tradeItemsList != null)
			{
				for (byte page = 0; page < MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS; page++)
				{
					IDictionary itemsInPage = tradeItemsList.GetItemsInPage((int)page);
					if (itemsInPage == null || itemsInPage.Count == 0)
					{
						continue;
					}

					using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MerchantWindow)))
					{
						pak.WriteByte((byte)itemsInPage.Count); //Item count on this page
						pak.WriteByte((byte)windowType);
						pak.WriteByte((byte)page); //Page number
												   //pak.WriteByte(0x00); //Unused // testing

						for (ushort i = 0; i < MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS; i++)
						{
							if (!itemsInPage.Contains((int)i))
							{
								continue;
							}

							var item = (ItemTemplate)itemsInPage[(int)i];
							if (item != null)
							{
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
								{
									pak.WriteByte((byte)(item.DPS_AF));
								}
								else
								{
									pak.WriteByte((byte)(item.Hand << 6));
								}

								pak.WriteByte((byte)((item.Type_Damage << 6) | item.Object_Type));
								//1 if item cannot be used by your class (greyed out)
								if (m_gameClient.Player != null && m_gameClient.Player.HasAbilityToUseItem(item))
								{
									pak.WriteByte(0x01); // these maybe switched in 1125 earlier revs
								}
								else
								{
									pak.WriteByte(0x00); // these maybe switched in 1125 earlier revs
								}

								pak.WriteShortLowEndian((ushort)value2);
								pak.WriteIntLowEndian((uint)item.Price);
								pak.WriteShortLowEndian((ushort)item.Model);
								pak.WritePascalStringIntLE(item.Name);
							}
							else
							{
								if (log.IsErrorEnabled)
								{
									log.Error("Merchant item template '" +
											  ((MerchantItem)itemsInPage[page * MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS + i]).ItemTemplateID +
											  "' not found, abort!!!");
								}

								return;
							}
						}
						SendTCP(pak);
					}
				}
			}
			else
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MerchantWindow)))
				{
					pak.WriteByte(0); //Item count on this page
					pak.WriteByte((byte)windowType); //Unknown 0x00
					pak.WriteByte(0); //Page number
					pak.WriteByte(0x00); //Unused
					SendTCP(pak);
				}
			}
		}
	}
}
