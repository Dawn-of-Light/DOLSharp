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
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&harm",
		ePrivLevel.GM,
		"GMCommands.Harm.Description",
		"GMCommands.Harm.Usage")]
	public class HarmCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			int amount;

			try
			{
				amount = Convert.ToInt16(args[1]);
				GameLiving living = client.Player.TargetObject as GameLiving;
				if (living != null)
					living.TakeDamage(client.Player, eDamageType.GM, amount, 0);
				else
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Harm.InvalidTarget"));
			}
			catch (Exception ex)
			{
				List<string> list = new List<string>();
				list.Add(ex.ToString());
				client.Out.SendCustomTextWindow("Exception", list);
			}
		}
	}
}