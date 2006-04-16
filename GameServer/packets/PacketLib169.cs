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
using DOL.GS.Database;
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
			if (m_gameClient.Player==null) return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x06);

			PlayerGroup group = m_gameClient.Player.PlayerGroup;
			if(group==null)
			{
				pak.WriteByte(0x00);
			}
			else
			{
				pak.WriteByte((byte) group.PlayerCount);
			}

			pak.WriteByte(0x01);
			pak.WriteByte(0x00);

			if(group != null)
			{
				lock (group)
				{
					for(int i = 0; i < group.PlayerCount; ++i)
					{
						GamePlayer updatePlayer = group[i];
						bool sameRegion = updatePlayer.Region == m_gameClient.Player.Region;

						pak.WriteByte(updatePlayer.Level);
						if (sameRegion)
						{
							pak.WriteByte(updatePlayer.HealthPercent);					
							pak.WriteByte(updatePlayer.ManaPercent);		
							pak.WriteByte(updatePlayer.EndurancePercent); //new in 1.69

							byte playerStatus = 0;
							if (!updatePlayer.Alive)
								playerStatus |= 0x01;
							if (updatePlayer.Mez)
								playerStatus |= 0x02;
							if (updatePlayer.IsDiseased)
								playerStatus |= 0x04;
							if (SpellHandler.FindEffectOnTarget(updatePlayer, "DamageOverTime") != null)
								playerStatus |= 0x08;
							if (updatePlayer.Client.ClientState == GameClient.eClientState.Linkdead)
								playerStatus |= 0x10;
							if (updatePlayer.Region != m_gameClient.Player.Region)
								playerStatus |= 0x20;

							pak.WriteByte(playerStatus);
							// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased , 
							// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

							pak.WriteShort((ushort)updatePlayer.ObjectID);//or session id?
						}
						else
						{
							pak.WriteInt(0x20);
							pak.WriteShort(0);
						}
						pak.WritePascalString(updatePlayer.Name);
						pak.WritePascalString(updatePlayer.CharacterClass.Name);//classname
					}
				}
			}
			SendTCP(pak);
		}

		protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GamePlayer player)
		{
			pak.WriteByte((byte)(player.PlayerGroupIndex+1)); // From 1 to 8
			bool sameRegion = player.Region == m_gameClient.Player.Region;
			if (sameRegion)
			{
				pak.WriteByte(player.HealthPercent);
				pak.WriteByte(player.ManaPercent);
				pak.WriteByte(player.EndurancePercent); // new in 1.69

				byte playerStatus = 0;
				if (!player.Alive)
					playerStatus |= 0x01;
				if (player.Mez)
					playerStatus |= 0x02;
				if (player.IsDiseased)
					playerStatus |= 0x04;
				if (SpellHandler.FindEffectOnTarget(player, "DamageOverTime") != null)
					playerStatus |= 0x08;
				if (player.Client.ClientState == GameClient.eClientState.Linkdead)
					playerStatus |= 0x10;
				if (!sameRegion)
					playerStatus |= 0x20;

				pak.WriteByte(playerStatus);
				// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased , 
				// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | player.PlayerGroupIndex));
					lock (player.EffectList)
					{
						byte i=0;
						foreach (IGameEffect effect in player.EffectList)
							if(effect is GameSpellEffect)
								i++;
						pak.WriteByte(i);
						foreach (IGameEffect effect in player.EffectList)
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
					pak.WriteByte((byte)(0x80 | player.PlayerGroupIndex));
					pak.WriteByte(0);
				}
			}
		}
	}
}
