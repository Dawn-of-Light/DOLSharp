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
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.Language;
using log4net;


namespace DOL.GS.PacketHandler
{
	[PacketLib(1104, GameClient.eClientVersion.Version1104)]
	public class PacketLib1104 : PacketLib1103
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.104
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1104(GameClient client)
			: base(client)
		{
		}

		public override void SendCharacterOverview(eRealm realm)
		{
			if (realm < eRealm.Albion || realm > eRealm.Hibernia)
			{
				throw new Exception("CharacterOverview requested for unknown realm " + realm);
			}

			int firstSlot = (byte)realm * 100;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterOverview));
			pak.FillString(m_gameClient.Account.Name, 28); //extra 4 in 1.104

			if (m_gameClient.Account.Characters == null)
			{
				pak.Fill(0x0, 1880);
			}
			else
			{
				Dictionary<int, DOLCharacters> charsBySlot = new Dictionary<int, DOLCharacters>();
				string itemQuery = "(";
				foreach (DOLCharacters c in m_gameClient.Account.Characters)
				{
					try
					{
						charsBySlot.Add(c.AccountSlot, c);
						itemQuery += "OwnerID = '" + c.ObjectId + "' OR ";
					}
					catch (Exception ex)
					{
						log.Error("SendCharacterOverview - Duplicate char in slot? Slot: " + c.AccountSlot + ", Account: " + c.AccountName, ex);
					}
				}
				itemQuery = itemQuery.Substring(0, itemQuery.Length - 4); //remove last OR
				itemQuery += ") AND SlotPosition >= " + ((int)eInventorySlot.MinEquipable) + " AND SlotPosition <= " + ((int)eInventorySlot.MaxEquipable);

				var itemsByOwnerID = new Dictionary<string, Dictionary<eInventorySlot, InventoryItem>>();
				var allItems = (InventoryItem[])GameServer.Database.SelectObjects<InventoryItem>(itemQuery);
				foreach (InventoryItem item in allItems)
				{
                    try
                    {
                        if (!itemsByOwnerID.ContainsKey(item.OwnerID))
                            itemsByOwnerID.Add(item.OwnerID, new Dictionary<eInventorySlot, InventoryItem>());

                        itemsByOwnerID[item.OwnerID].Add((eInventorySlot)item.SlotPosition, item);
                    }
                    catch (Exception ex)
                    {
                        log.Error("SendCharacterOverview - Duplicate item on character? OwnerID: " + item.OwnerID + ", SlotPosition: " + item.SlotPosition + ", Account: " + m_gameClient.Account.Name, ex);
                    }
				}

				for (int i = firstSlot; i < (firstSlot + 10); i++)
				{
					DOLCharacters c = null;
					if (!charsBySlot.TryGetValue(i, out c))
					{
						pak.Fill(0x0, 188);
					}
					else
					{
						Dictionary<eInventorySlot, InventoryItem> charItems = null;

						if (!itemsByOwnerID.TryGetValue(c.ObjectId, out charItems))
							charItems = new Dictionary<eInventorySlot, InventoryItem>();

						byte extensionTorso = 0;
						byte extensionGloves = 0;
						byte extensionBoots = 0;

						InventoryItem item = null;

						if (charItems.TryGetValue(eInventorySlot.TorsoArmor, out item))
							extensionTorso = item.Extension;

						if (charItems.TryGetValue(eInventorySlot.HandsArmor, out item))
							extensionGloves = item.Extension;

						if (charItems.TryGetValue(eInventorySlot.FeetArmor, out item))
							extensionBoots = item.Extension;

						pak.FillString(c.Name, 24);
						pak.WriteByte(0x01);
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
						pak.Fill(0x0, 13); //0 String

						string locationDescription = "";
						Region region = WorldMgr.GetRegion((ushort)c.Region);
						if (region != null)
						{
							Zone zone = null;
							if ((zone = region.GetZone(c.Xpos, c.Ypos)) != null)
							{
								IList areas = zone.GetAreasOfSpot(c.Xpos, c.Ypos, c.Zpos);

								foreach (AbstractArea area in areas)
								{
									if (!area.DisplayMessage)
										continue;

									locationDescription = area.Description;
									break;
								}

								if (locationDescription == "")
								{
									locationDescription = zone.Description;
								}
							}
						}
						pak.FillString(locationDescription, 24);

						string classname = "";
						if (c.Class != 0)
							classname = ((eCharacterClass)c.Class).ToString();
						pak.FillString(classname, 24);

						string racename = GamePlayer.RACENAMES(m_gameClient, c.Race, c.Gender);
						pak.FillString(racename, 24);

						pak.WriteByte((byte)c.Level);
						pak.WriteByte((byte)c.Class);
						pak.WriteByte((byte)c.Realm);
						pak.WriteByte((byte)((((c.Race & 0x10) << 2) + (c.Race & 0x0F)) | (c.Gender << 4))); // race max value can be 0x1F
						pak.WriteShortLowEndian((ushort)c.CurrentModel);
						pak.WriteByte((byte)c.Region);
						if (region == null || (int)m_gameClient.ClientType > region.Expansion)
							pak.WriteByte(0x00);
						else
							pak.WriteByte((byte)(region.Expansion + 1)); //0x04-Cata zone, 0x05 - DR zone
						pak.WriteInt(0x0); // Internal database ID
						pak.WriteByte((byte)c.Strength);
						pak.WriteByte((byte)c.Dexterity);
						pak.WriteByte((byte)c.Constitution);
						pak.WriteByte((byte)c.Quickness);
						pak.WriteByte((byte)c.Intelligence);
						pak.WriteByte((byte)c.Piety);
						pak.WriteByte((byte)c.Empathy);
						pak.WriteByte((byte)c.Charisma);

						InventoryItem rightHandWeapon = null;
						charItems.TryGetValue(eInventorySlot.RightHandWeapon, out rightHandWeapon);
						InventoryItem leftHandWeapon = null;
						charItems.TryGetValue(eInventorySlot.LeftHandWeapon, out leftHandWeapon);
						InventoryItem twoHandWeapon = null;
						charItems.TryGetValue(eInventorySlot.TwoHandWeapon, out twoHandWeapon);
						InventoryItem distanceWeapon = null;
						charItems.TryGetValue(eInventorySlot.DistanceWeapon, out distanceWeapon);

						InventoryItem helmet = null;
						charItems.TryGetValue(eInventorySlot.HeadArmor, out helmet);
						InventoryItem gloves = null;
						charItems.TryGetValue(eInventorySlot.HandsArmor, out gloves);
						InventoryItem boots = null;
						charItems.TryGetValue(eInventorySlot.FeetArmor, out boots);
						InventoryItem torso = null;
						charItems.TryGetValue(eInventorySlot.TorsoArmor, out torso);
						InventoryItem cloak = null;
						charItems.TryGetValue(eInventorySlot.Cloak, out cloak);
						InventoryItem legs = null;
						charItems.TryGetValue(eInventorySlot.LegsArmor, out legs);
						InventoryItem arms = null;
						charItems.TryGetValue(eInventorySlot.ArmsArmor, out arms);

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

						if (c.ActiveWeaponSlot == (byte)DOL.GS.GameLiving.eActiveWeaponSlot.TwoHanded)
						{
							pak.WriteByte(0x02);
							pak.WriteByte(0x02);
						}
						else if (c.ActiveWeaponSlot == (byte)DOL.GS.GameLiving.eActiveWeaponSlot.Distance)
						{
							pak.WriteByte(0x03);
							pak.WriteByte(0x03);
						}
						else
						{
							byte righthand = 0xFF;
							byte lefthand = 0xFF;

							if (rightHandWeapon != null)
								righthand = 0x00;

							if (leftHandWeapon != null)
								lefthand = 0x01;

							pak.WriteByte(righthand);
							pak.WriteByte(lefthand);
						}

						if (region == null || region.Expansion != 1)
							pak.WriteByte(0x00);
						else
							pak.WriteByte(0x01); //0x01=char in SI zone, classic client can't "play"

						pak.WriteByte((byte)c.Constitution);
						pak.Fill(0x00, 4);//new trailing bytes in 1.99
					}

				}
			}
			pak.Fill(0x0, 90);
			SendTCP(pak);
		}
		public override void SendDupNameCheckReply(string name, bool nameExists)
		{
			if (m_gameClient == null || m_gameClient.Account == null)
				return;

			// This presents the user with Name Not Allowed which may not be correct but at least it prevents duplicate char creation
			// - tolakram
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DupNameCheckReply)))
			{
				pak.FillString(name, 30);
				pak.FillString(m_gameClient.Account.Name, 24);
				pak.WriteByte((byte)(nameExists ? 0x1 : 0x0));
				pak.Fill(0x0, 3);
				SendTCP(pak);
			}
		}

	}
}
