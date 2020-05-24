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

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	///SiegeWeaponActionHandler handler the command of player to control siege weapon
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0xf5, "Handles Siege command Request")]
	public class SiegeWeaponActionHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			packet.ReadShort(); // unk
			int action = packet.ReadByte();
			int ammo = packet.ReadByte(); // (ammo type if command = 'select ammo' ?)
			if (client.Player.SiegeWeapon == null)
				return;
			if (client.Player.IsStealthed)
			{
				client.Out.SendMessage("You can't control a siege weapon while hidden!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (client.Player.IsSitting)
			{
				client.Out.SendMessage("You can't fire a siege weapon while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!client.Player.IsAlive || client.Player.IsMezzed || client.Player.IsStunned)
			{
				client.Out.SendMessage("You can't control a siege weapon now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            if( !client.Player.IsWithinRadius( client.Player.SiegeWeapon, GameSiegeWeapon.SIEGE_WEAPON_CONTROLE_DISTANCE ) )
			{
				client.Out.SendMessage(client.Player.SiegeWeapon.GetName(0, true) + " is too far away for you to control!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (action)
			{
				case 1: { client.Player.SiegeWeapon.Load(ammo); } break;//select ammo need Log to know how sent
				case 2: { client.Player.SiegeWeapon.Arm(); } break;//arm
				case 3: { client.Player.SiegeWeapon.Aim(); } break;//aim
				case 4: { client.Player.SiegeWeapon.Fire(); } break;//fire
				case 5: { client.Player.SiegeWeapon.Move(); } break;//move
				case 6: { client.Player.SiegeWeapon.Repair(); } break;//repair
				case 7: { client.Player.SiegeWeapon.salvage(); } break;//salvage
				case 8: { client.Player.SiegeWeapon.ReleaseControl(); } break;//release
				case 9: { client.Player.SiegeWeapon.StopMove(); } break;//stop
				case 10: { client.Player.SiegeWeapon.Fire(); } break;//swing
				default:
					{
						client.Player.Out.SendMessage("Unhandled action ID: " + action, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
			}
		}
	}
}
