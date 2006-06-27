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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&heal",
		(uint) ePrivLevel.GM,
		"Heals your target (health,endu,mana)",
		"/heal")]
	public class HealCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /heal",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}

			try
			{
				GameLiving living = client.Player.TargetObject as GameLiving;
				if (living != null)
				{
					living.Health = living.MaxHealth;
					living.Endurance = living.MaxEndurance;
					living.Mana = living.MaxMana;
				}
				else
				{
					client.Player.Health = client.Player.MaxHealth;
					client.Player.Endurance = client.Player.MaxEndurance;
					client.Player.Mana = client.Player.MaxMana;
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage("Usage: /heal",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}