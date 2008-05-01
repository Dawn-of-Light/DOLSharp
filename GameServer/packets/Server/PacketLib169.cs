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
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Effects;

namespace DOL.GS.PacketHandler
{
	[PacketLib(169, GameClient.eClientVersion.Version169)]
	public class PacketLib169 : PacketLib168
	{
		/// <summary>
		/// Constructs a new PacketLib for Version 1.69 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib169(GameClient client) : base(client)
		{
		}

		public override void SendGroupWindowUpdate()
		{
			if (m_gameClient.Player == null) return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x06);

			Group group = m_gameClient.Player.Group;
			if (group == null)
			{
				pak.WriteByte(0x00);
			}
			else
			{
				pak.WriteByte((byte)group.MemberCount);
			}

			pak.WriteByte(0x01);
			pak.WriteByte(0x00);

			if (group != null)
			{
				foreach (GameLiving living in group.GetMembersInTheGroup())
				{
					bool sameRegion = living.CurrentRegion == m_gameClient.Player.CurrentRegion;

					pak.WriteByte(living.Level);
					if (sameRegion)
					{
						pak.WriteByte(living.HealthPercentGroupWindow);
						pak.WriteByte(living.ManaPercent);
						pak.WriteByte(living.EndurancePercent); //new in 1.69

						byte playerStatus = 0;
						if (!living.IsAlive)
							playerStatus |= 0x01;
						if (living.IsMezzed)
							playerStatus |= 0x02;
						if (living.IsDiseased)
							playerStatus |= 0x04;
						if (SpellHandler.FindEffectOnTarget(living, "DamageOverTime") != null)
							playerStatus |= 0x08;
						if (living is GamePlayer && ((GamePlayer)living).Client.ClientState == GameClient.eClientState.Linkdead)
							playerStatus |= 0x10;
						if (living.CurrentRegion != m_gameClient.Player.CurrentRegion)
							playerStatus |= 0x20;

						pak.WriteByte(playerStatus);
						// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased , 
						// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

						pak.WriteShort((ushort)living.ObjectID);//or session id?
					}
					else
					{
						pak.WriteInt(0x20);
						pak.WriteShort(0);
					}
					pak.WritePascalString(living.Name);
					pak.WritePascalString(living is GamePlayer ? ((GamePlayer)living).CharacterClass.Name : "NPC");//classname
				}
			}
			SendTCP(pak);
		}

		protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GameLiving living)
		{
			pak.WriteByte((byte)(living.GroupIndex+1)); // From 1 to 8
			bool sameRegion = living.CurrentRegion == m_gameClient.Player.CurrentRegion;
			if (sameRegion)
			{
				pak.WriteByte(living.HealthPercent);
				pak.WriteByte(living.ManaPercent);
				pak.WriteByte(living.EndurancePercent); // new in 1.69

				byte playerStatus = 0;
				if (!living.IsAlive)
					playerStatus |= 0x01;
				if (living.IsMezzed)
					playerStatus |= 0x02;
				if (living.IsDiseased)
					playerStatus |= 0x04;
				if (SpellHandler.FindEffectOnTarget(living, "DamageOverTime") != null)
					playerStatus |= 0x08;
				if (living is GamePlayer && ((GamePlayer)living).Client.ClientState == GameClient.eClientState.Linkdead)
					playerStatus |= 0x10;
				if (!sameRegion)
					playerStatus |= 0x20;

				pak.WriteByte(playerStatus);
				// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased , 
				// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | living.GroupIndex));
					lock (living.EffectList)
					{
						byte i=0;
						foreach (IGameEffect effect in living.EffectList)
							if(effect is GameSpellEffect)
								i++;
						pak.WriteByte(i);
						foreach (IGameEffect effect in living.EffectList)
							if(effect is GameSpellEffect)
							{
								pak.WriteByte(0);
								pak.WriteShort(effect.Icon);
							}
					}
				}
			}
			else
			{
				pak.WriteInt(0x20);
				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | living.GroupIndex));
					pak.WriteByte(0);
				}
			}
		}
	}
}
