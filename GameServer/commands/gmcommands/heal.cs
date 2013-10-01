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

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&heal",
		ePrivLevel.GM,
		"GMCommands.Heal.Description",
		"GMCommands.Heal.Usage",
		"/heal me - heals self")]
	public class HealCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			try
			{
				GameLiving target = client.Player.TargetObject as GameLiving;
				
				if (target == null || (args.Length > 1 && args[1].ToLower() == "me"))
					target = (GameLiving)client.Player;

				target.Health = target.MaxHealth;
				target.Endurance = target.MaxEndurance;
				target.Mana = target.MaxMana;
			}
			catch (Exception)
			{
				DisplaySyntax(client);
			}
		}
	}
}