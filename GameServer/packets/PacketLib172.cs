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
#define  NOENCRYPTION

using System.Collections;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.Quests;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(172, GameClient.eClientVersion.Version172)]
	public class PacketLib172 : PacketLib171
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.72 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib172(GameClient client) : base(client)
		{
		}

		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PlayerCreate172));
			Region playerRegion = playerToCreate.Region;
			if (playerRegion == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerRegion == null");
				return;
			}
			Zone playerZone = playerToCreate.Region.GetZone(playerToCreate.Position);
			if (playerZone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerZone == null");
				return;
			}
			Point zonePos = playerZone.ToLocalPosition(playerToCreate.Position);

			pak.WriteShort((ushort) playerToCreate.Client.SessionID);
			pak.WriteShort((ushort) playerToCreate.ObjectID);
			pak.WriteShort((ushort) playerToCreate.Model);
			pak.WriteShort((ushort) zonePos.Z);
			pak.WriteShort((ushort) playerZone.ZoneID);
			pak.WriteShort((ushort) zonePos.X);
			pak.WriteShort((ushort) zonePos.Y);
			pak.WriteShort((ushort) playerToCreate.Heading);

			pak.WriteByte(playerToCreate.EyeSize); //1-4 = Eye Size / 5-8 = Nose Size
			pak.WriteByte(playerToCreate.LipSize); //1-4 = Ear size / 5-8 = Kin size
			pak.WriteByte(playerToCreate.EyeColor); //1-4 = Skin Color / 5-8 = Eye Color
			pak.WriteByte(playerToCreate.Level);
			pak.WriteByte(playerToCreate.HairColor); //Hair: 1-4 = Color / 5-8 = unknown
			pak.WriteByte(playerToCreate.FaceType); //1-4 = Unknown / 5-8 = Face type
			pak.WriteByte(playerToCreate.HairStyle); //1-4 = Unknown / 5-8 = Hair Style

			int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
			if (playerToCreate.Alive == false) flags |= 0x01;
			if (playerToCreate.IsSwimming) flags |= 0x02; //swimming
			if (playerToCreate.IsStealthed) flags |= 0x10;
			// 0x20 = wireframe
			pak.WriteByte((byte) flags);

			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			pak.WriteByte(0x00); //Trialing 0 ... needed!
			SendTCP(pak);

			if (GameServer.ServerRules.GetColorHandling(m_gameClient) == 1) // PvP
				SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server
		}

		protected override void SendInventorySlotsUpdateBase(ICollection slots, byte preAction)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			pak.WriteByte((byte) (slots == null ? 0 : slots.Count));
			pak.WriteByte((byte) ((m_gameClient.Player.Inventory.IsCloakHoodUp ? 0x01 : 0x00) | (int) m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte((byte) m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(preAction); //preAction (0x00 - Do nothing)
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					pak.WriteByte((byte) updatedSlot);
					
					GenericItem item = m_gameClient.Player.Inventory.GetItem((eInventorySlot) updatedSlot) as GenericItem;
					if (item == null)
					{
						pak.Fill(0x00, 19);
						continue;
					}

//					eObjectType.GardenObject
//					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)
//					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand 
//					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
//					
					int value1 = 0; // some object types use this field to display count
					int value2 = 0; // some object types use this field to display count
					int handNeeded = 0;
					int damageType = 0;
					int condition = 0;
					int durabiliy = 0; 
					int quality = 0; 
					int bonus = 0; 
					int modelExtension = 0;
					int color = 0;
					int effect = 0;
					int count = 0;

					if(item is StackableItem)
					{
						value1 = count = ((StackableItem)item).Count;
						if(item is Ammunition)
						{
							value2 = ((byte)((Ammunition)item).Damage) | (((byte)((Ammunition)item).Range) << 2) | (((byte)((Ammunition)item).Precision) << 4);//item.SPD_ABS;
						}
					}
					else if(item is Instrument)
					{
						value1 = (byte)((Instrument)item).Type;
					}
					else if (item is Shield)
					{
						value1 = (byte)((Shield)item).Size;
						value2 = ((Shield)item).DamagePerSecond;
					}
					else if(item is Weapon)
					{
						value1 = ((Weapon)item).DamagePerSecond;
						if(item is ThrownWeapon)
						{
							value2 = count = ((ThrownWeapon)item).Count;
						}
						else
						{
							value2 = (byte)(((Weapon)item).Speed / 100);
						}
					}
					else if(item is Armor)
					{
						value1 = ((Armor)item).ArmorFactor;
						value2 = ((Armor)item).Absorbtion;
					}
					
					if(item is VisibleEquipment)
					{
						color = ((VisibleEquipment)item).Color;
					}
					
					if(item is EquipableItem)
					{
						condition = (byte)((EquipableItem)item).Condition;
						durabiliy = ((EquipableItem)item).Durability; 
						quality = ((EquipableItem)item).Quality; 
						bonus = ((EquipableItem)item).Bonus;
					}
					else if(item is SpellCraftGem)
					{
						quality = ((SpellCraftGem)item).Quality;
					}
						
					if(item is Weapon)
					{
						handNeeded = (byte)((Weapon)item).HandNeeded;
						damageType = (byte)((Weapon)item).DamageType;
						effect = ((Weapon)item).GlowEffect;
					}
					else if(item is Armor) 
					{
						modelExtension = ((Armor)item).ModelExtension;
					}
					else if(item is Ammunition)
					{
						damageType = (byte)((Ammunition)item).DamageType;
					}

					pak.WriteByte(item.Level);
					pak.WriteByte((byte) value1);
					pak.WriteByte((byte) value2);
					pak.WriteByte((byte) (handNeeded << 6));
					pak.WriteByte((byte) ((damageType << 6) + (byte)item.ObjectType));
					pak.WriteShort((ushort) item.Weight);
					pak.WriteByte((byte) condition); // % of con
					pak.WriteByte((byte) durabiliy); // % of dur
					pak.WriteByte((byte) quality); // % of qua
					pak.WriteByte((byte) bonus); // % bonus
					pak.WriteShort((ushort) item.Model);
					pak.WriteByte((byte)modelExtension);
					pak.WriteShort((ushort) color);
					pak.WriteShort((ushort) effect);
					if (count > 1)
						pak.WritePascalString(count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
			}
			SendTCP(pak);
		}

		public override void SendTradeWindow()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.TradeWindow));
			lock (m_gameClient.Player.TradeWindow.Sync)
			{
				foreach (GenericItem item in  m_gameClient.Player.TradeWindow.TradeItems)
				{
					pak.WriteByte((byte) item.SlotPosition);
				}
				pak.Fill(0x00,10-m_gameClient.Player.TradeWindow.TradeItems.Count);

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort) Money.GetMithril(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetPlatinum(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetGold(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetSilver(m_gameClient.Player.TradeWindow.TradeMoney));
				pak.WriteShort((ushort) Money.GetCopper(m_gameClient.Player.TradeWindow.TradeMoney));

				pak.WriteShort(0x0000);
				pak.WriteShort((ushort) Money.GetMithril(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetPlatinum(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetGold(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetSilver(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
				pak.WriteShort((ushort) Money.GetCopper(m_gameClient.Player.TradeWindow.PartnerTradeMoney));

				pak.WriteShort(0x0000);
				ArrayList items = m_gameClient.Player.TradeWindow.PartnerTradeItems;
				if (items != null)
				{
					pak.WriteByte((byte) items.Count);
					pak.WriteByte(0x01);
				}
				else
				{
					pak.WriteShort(0x0000);
				}
				pak.WriteByte((byte) (m_gameClient.Player.TradeWindow.Repairing ? 0x01 : 0x00));
				pak.WriteByte((byte) (m_gameClient.Player.TradeWindow.Combine ? 0x01 : 0x00));
				if (items != null)
				{
					foreach (GenericItem item in items)
					{
						int value1 = 0; // some object types use this field to display count
						int value2 = 0; // some object types use this field to display count
						int handNeeded = 0;
						int damageType = 0;
						int condition = 0;
						int durabiliy = 0; 
						int quality = 0; 
						int bonus = 0; 
						int modelExtension = 0;
						int color = 0;
						int effect = 0;
						int count = 0;

						if(item is StackableItem)
						{
							value1 = count = ((StackableItem)item).Count;
							if(item is Ammunition)
							{
								value2 = ((byte)((Ammunition)item).Damage) | (((byte)((Ammunition)item).Range) << 2) | (((byte)((Ammunition)item).Precision) << 4);//item.SPD_ABS;
							}
						}
						else if(item is Instrument)
						{
							value1 = (byte)((Instrument)item).Type;
						}
						else if (item is Shield)
						{
							value1 = (byte)((Shield)item).Size;
							value2 = ((Shield)item).DamagePerSecond;
						}
						else if(item is Weapon)
						{
							value1 = ((Weapon)item).DamagePerSecond;
							if(item is ThrownWeapon)
							{
								value2 = count = ((ThrownWeapon)item).Count;
							}
							else
							{
								value2 = (byte)(((Weapon)item).Speed/100);
							}
						}
						else if(item is Armor)
						{
							value1 = ((Armor)item).ArmorFactor;
							value2 = ((Armor)item).Absorbtion;
						}

						if(item is VisibleEquipment)
						{
							color = ((VisibleEquipment)item).Color;
						}

						if(item is EquipableItem)
						{
							condition = (byte)((EquipableItem)item).Condition;
							durabiliy = ((EquipableItem)item).Durability; 
							quality = ((EquipableItem)item).Quality; 
							bonus = ((EquipableItem)item).Bonus;
						}
						else if(item is SpellCraftGem)
						{
							quality = ((SpellCraftGem)item).Quality; 
						}

						if(item is Weapon)
						{
							handNeeded = (byte)((Weapon)item).HandNeeded;
							damageType = (byte)((Weapon)item).DamageType;
							effect = ((Weapon)item).GlowEffect;
						}
						else if(item is Armor) 
						{
							modelExtension = ((Armor)item).ModelExtension;
						}
						else if(item is Ammunition)
						{
							damageType = (byte)((Ammunition)item).DamageType;
						}

						pak.WriteByte((byte) item.SlotPosition);
						pak.WriteByte(item.Level);
						pak.WriteByte((byte) value1);
						pak.WriteByte((byte) value2);
						pak.WriteByte((byte) (handNeeded << 6));
						pak.WriteByte((byte) ((damageType << 6) + (byte)item.ObjectType));
						pak.WriteShort((ushort) item.Weight);
						pak.WriteByte((byte) condition); // % of con
						pak.WriteByte((byte) durabiliy); // % of dur
						pak.WriteByte((byte) quality); // % of qua
						pak.WriteByte((byte) bonus); // % bonus
						pak.WriteShort((ushort) item.Model);
						pak.WriteByte((byte)modelExtension);
						pak.WriteShort((ushort) color);
						pak.WriteShort((ushort) effect);
						if (count > 1)
							pak.WritePascalString(count + " " + item.Name);
						else
							pak.WritePascalString(item.Name);
					}
				}
				if (m_gameClient.Player.TradeWindow.Partner != null)
					pak.WritePascalString("Trading with " + m_gameClient.Player.TradeWindow.Partner.Name); // transaction with ...
				else
					pak.WritePascalString("Selfcrafting"); // transaction with ...
				SendTCP(pak);
			}
		}
	}
}