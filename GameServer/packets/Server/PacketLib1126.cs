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
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DOL.GS.PacketHandler
{
	[PacketLib(1126, GameClient.eClientVersion.Version1126)]
	public class PacketLib1126 : PacketLib1125
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.125
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1126(GameClient client)
			: base(client)
		{
		}

		/// <summary>
		/// Reply on Server Opening to Client Encryption Request
		/// Actually forces Encryption Off to work with Portal.
		/// </summary>
		public override void SendVersionAndCryptKey()
		{
			//Construct the new packet
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CryptKey)))
			{
				//Disable encryption (1110+ always encrypt)
				pak.WriteIntLowEndian(0);
				// pak.Write(key, 0, key.Length);

				// From now on we expect RSA!
				// _gameClient.PacketProcessor.Encoding.EncryptionState = eEncryptionState.PseudoRC4Encrypted; // disabled by the launcher

				// Reply with current version
				pak.WriteString((((int)m_gameClient.Version) / 1000) + "." + (((int)m_gameClient.Version) - 1000), 5);

				// revision, last seen (c) 0x63
				pak.WriteByte(0x00);

				// Build number
				pak.WriteByte(0x00); // last seen : 0x44 0x05
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public override void SendCharacterOverview(eRealm realm)
		{
			if (realm < eRealm._FirstPlayerRealm || realm > eRealm._LastPlayerRealm)
				throw new Exception($"CharacterOverview requested for unknown realm {realm}");

			int firstSlot = (byte)realm * 100;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterOverview1126)))
			{
				pak.WriteIntLowEndian(0); // 0x01 & 0x02 are flags
				pak.WriteIntLowEndian(0);
				pak.WriteIntLowEndian(0);
				pak.WriteIntLowEndian(0);
				if (m_gameClient.Account.Characters == null || m_gameClient.Account.Characters.Length == 0)
				{
					SendTCP(pak);
					return;
				}

				Dictionary<int, DOLCharacters> charsBySlot = new Dictionary<int, DOLCharacters>();
				foreach (DOLCharacters c in m_gameClient.Account.Characters)
				{
					try
					{
						charsBySlot.Add(c.AccountSlot, c);
					}
					catch (Exception ex)
					{
						log.Error($"SendCharacterOverview - Duplicate char in slot? Slot: {c.AccountSlot}, Account: {c.AccountName}", ex);
					}
				}
				var itemsByOwnerID = new Dictionary<string, Dictionary<eInventorySlot, InventoryItem>>();

				if (charsBySlot.Any())
				{
					var allItems = GameServer.Database.SelectObjects<InventoryItem>(
							"`OwnerID` = @OwnerID AND `SlotPosition` >= @MinEquipable AND `SlotPosition` <= @MaxEquipable",
							charsBySlot.Select(kv => new[] {
								new QueryParameter("@OwnerID", kv.Value.ObjectId),
								new QueryParameter("@MinEquipable", (int)eInventorySlot.MinEquipable),
								new QueryParameter("@MaxEquipable", (int)eInventorySlot.MaxEquipable)
							})
						)
						.SelectMany(objs => objs);

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
							log.Error($"SendCharacterOverview - Duplicate item on character? OwnerID: {item.OwnerID}, SlotPosition: {item.SlotPosition}, Account: {m_gameClient.Account.Name}", ex);
						}
					}
				}

				// send each characters
				for (int i = firstSlot; i < (firstSlot + 10); i++)
				{
					DOLCharacters c = null;
					if (!charsBySlot.TryGetValue(i, out c))
					{
						pak.WriteByte(0);
						continue;
					}

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

					string locationDescription = string.Empty;
					Region region = WorldMgr.GetRegion((ushort)c.Region);
					if (region != null)
						locationDescription = m_gameClient.GetTranslatedSpotDescription(region, c.Xpos, c.Ypos, c.Zpos);
					string classname = "";
					if (c.Class != 0)
						classname = ((eCharacterClass)c.Class).ToString();
					string racename = m_gameClient.RaceToTranslatedName(c.Race, c.Gender);

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

					ushort rightHandColor = 0;
					if (rightHandWeapon != null)
						rightHandColor = (ushort)(rightHandWeapon.Emblem != 0 ? rightHandWeapon.Emblem : rightHandWeapon.Color);
					ushort helmetColor = 0;
					if (helmet != null)
						helmetColor = (ushort)(helmet.Emblem != 0 ? helmet.Emblem : helmet.Color);
					ushort glovesColor = 0;
					if (gloves != null)
						glovesColor = (ushort)(gloves.Emblem != 0 ? gloves.Emblem : gloves.Color);
					ushort bootsColor = 0;
					if (boots != null)
						bootsColor = (ushort)(boots.Emblem != 0 ? boots.Emblem : boots.Color);
					ushort leftHandWeaponColor = 0;
					if (leftHandWeapon != null)
						leftHandWeaponColor = (ushort)(leftHandWeapon.Emblem != 0 ? leftHandWeapon.Emblem : leftHandWeapon.Color);
					ushort torsoColor = 0;
					if (torso != null)
						torsoColor = (ushort)(torso.Emblem != 0 ? torso.Emblem : torso.Color);
					ushort cloakColor = 0;
					if (cloak != null)
						cloakColor = (ushort)(cloak.Emblem != 0 ? cloak.Emblem : cloak.Color);
					ushort legsColor = 0;
					if (legs != null)
						legsColor = (ushort)(legs.Emblem != 0 ? legs.Emblem : legs.Color);
					ushort armsColor = 0;
					if (arms != null)
						armsColor = (ushort)(arms.Emblem != 0 ? arms.Emblem : arms.Color);

					pak.WriteByte((byte)c.Level);
					pak.WritePascalStringIntLE(c.Name);
					pak.WriteIntLowEndian(0x18);
					pak.WriteByte(1); // always 1 ?
					pak.WriteByte(c.EyeSize); // seems to be : 0xF0 = eyes, 0x0F = nose
					pak.WriteByte(c.LipSize); // seems to be : 0xF0 = lips, 0xF = jaw
					pak.WriteByte(c.EyeColor); // seems to be : 0xF0 = eye color, 0x0F = skin tone
					pak.WriteByte(c.HairColor);
					pak.WriteByte(c.FaceType); // seems to be : 0xF0 = face
					pak.WriteByte(c.HairStyle); // seems to be : 0xF0 = hair
					pak.WriteByte((byte)((extensionBoots << 4) | extensionGloves));
					pak.WriteByte((byte)((extensionTorso << 4) | (c.IsCloakHoodUp ? 0x1 : 0x0)));
					pak.WriteByte(c.CustomisationStep); //1 = auto generate config, 2= config ended by player, 3= enable config to player
					pak.WriteByte(c.MoodType);
					pak.Fill(0x0, 13);
					pak.WritePascalStringIntLE(locationDescription);
					pak.WritePascalStringIntLE(classname);
					pak.WritePascalStringIntLE(racename);
					pak.WriteShortLowEndian((ushort)c.CurrentModel);

					pak.WriteByte((byte)c.Region);
					if (region == null || (int)m_gameClient.ClientType > region.Expansion)
						pak.WriteByte(0x00);
					else
						pak.WriteByte((byte)(region.Expansion + 1)); //0x04-Cata zone, 0x05 - DR zone

					pak.WriteShortLowEndian((ushort)(helmet != null ? helmet.Model : 0));
					pak.WriteShortLowEndian((ushort)(gloves != null ? gloves.Model : 0));
					pak.WriteShortLowEndian((ushort)(boots != null ? boots.Model : 0));
					pak.WriteShortLowEndian(rightHandColor);
					pak.WriteShortLowEndian((ushort)(torso != null ? torso.Model : 0));
					pak.WriteShortLowEndian((ushort)(cloak != null ? cloak.Model : 0));
					pak.WriteShortLowEndian((ushort)(legs != null ? legs.Model : 0));
					pak.WriteShortLowEndian((ushort)(arms != null ? arms.Model : 0));

					pak.WriteShortLowEndian(helmetColor);
					pak.WriteShortLowEndian(glovesColor);
					pak.WriteShortLowEndian(bootsColor);
					pak.WriteShortLowEndian(leftHandWeaponColor);
					pak.WriteShortLowEndian(torsoColor);
					pak.WriteShortLowEndian(cloakColor);
					pak.WriteShortLowEndian(legsColor);
					pak.WriteShortLowEndian(armsColor);

					//weapon models
					pak.WriteShortLowEndian((ushort)(rightHandWeapon != null ? rightHandWeapon.Model : 0));
					pak.WriteShortLowEndian((ushort)(leftHandWeapon != null ? leftHandWeapon.Model : 0));
					pak.WriteShortLowEndian((ushort)(twoHandWeapon != null ? twoHandWeapon.Model : 0));
					pak.WriteShortLowEndian((ushort)(distanceWeapon != null ? distanceWeapon.Model : 0));

					pak.WriteByte((byte)c.Strength);
					pak.WriteByte((byte)c.Quickness);
					pak.WriteByte((byte)c.Constitution);
					pak.WriteByte((byte)c.Dexterity);
					pak.WriteByte((byte)c.Intelligence);
					pak.WriteByte((byte)c.Piety);
					pak.WriteByte((byte)c.Empathy); // ?
					pak.WriteByte((byte)c.Charisma); // ?

					pak.WriteByte((byte)c.Class);
					pak.WriteByte((byte)c.Realm); // ok?
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
						pak.WriteByte((byte)(rightHandWeapon != null ? 0x00 : 0xFF));
						pak.WriteByte((byte)(leftHandWeapon != null ? 0x01 : 0xFF));
					}
					pak.WriteByte(0); // SI = 1, Classic = 0
					pak.WriteByte((byte)c.Constitution); // ok
					pak.WriteByte(0); // unknown
				}

				SendTCP(pak);
			}
		}

		public override void SendRegions()
		{
			if (!m_gameClient.Socket.Connected || m_gameClient.Player == null)
				return;

			Region region = WorldMgr.GetRegion(m_gameClient.Player.CurrentRegionID);
			if (region == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ClientRegion)))
			{
				var ip = region.ServerIP;
				if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.") || ip.StartsWith("192.168."))
					ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
				pak.WritePascalStringIntLE(ip);
				pak.WriteIntLowEndian(region.ServerPort);
				pak.WriteIntLowEndian(region.ServerPort);
				SendTCP(pak);
			}
		}

		/// <summary>
		/// This packet may have been updated anywhere from 1125b-1126a - not sure
		/// </summary>
		public override void SendUpdateWeaponAndArmorStats()
		{
			if (m_gameClient.Player == null)
			{
				return;
			}

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x05); //subcode
				pak.WriteByte(6); //number of entries
				pak.WriteByte(0x00); //subtype
				pak.WriteByte(0x00); //unk

				// weapondamage
				var wd = (int)(m_gameClient.Player.WeaponDamage(m_gameClient.Player.AttackWeapon) * 100.0);
				pak.WriteByte((byte)(wd / 100));
				pak.WriteByte(0x00);
				pak.WriteByte((byte)(wd % 100));
				pak.WriteByte(0x00);
				// weaponskill
				int ws = m_gameClient.Player.DisplayedWeaponSkill;
				pak.WriteByte((byte)(ws >> 8));
				pak.WriteByte(0x00);
				pak.WriteByte((byte)(ws & 0xff));
				pak.WriteByte(0x00);
				// overall EAF
				int eaf = m_gameClient.Player.EffectiveOverallAF;
				pak.WriteByte((byte)(eaf >> 8));
				pak.WriteByte(0x00);
				pak.WriteByte((byte)(eaf & 0xff));
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
	}
}
