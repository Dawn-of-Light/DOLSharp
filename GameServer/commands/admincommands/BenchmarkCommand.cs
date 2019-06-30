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
using System.Linq;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&benchmark",
		ePrivLevel.Admin,
		"Benchmark some aspects of DOL Server.",
		"/benchmark listskills|listspells|styles|respawns|deaths|tooltips")]
	public class BenchmarkCommand : AbstractCommandHandler, ICommandHandler
	{		
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2 || client == null || client.Player == null)
			{
				DisplaySyntax(client);
				return;
			}
			
			long start,spent;
			switch(args[1])
			{
			
				case "listskills":
					start = GameTimer.GetTickCount();
					Util.ForEach(Enumerable.Range(0, 1000).AsParallel(), i =>
					{
						var tmp = client.Player.GetAllUsableSkills(true);
					}
					                                              );
					
					spent = GameTimer.GetTickCount() - start;
					
					client.Player.Out.SendMessage(string.Format("Skills Benchmark took {0}ms for 1000 iterations...", spent), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				break;
				case "listspells":
					start = GameTimer.GetTickCount();
					Util.ForEach(Enumerable.Range(0, 1000).AsParallel(), i =>
					{
						var tmp = client.Player.GetAllUsableListSpells(true);
					}
					                                              );
					
					spent = GameTimer.GetTickCount() - start;
					
					client.Player.Out.SendMessage(string.Format("Spells Benchmark took {0}ms for 1000 iterations...", spent), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				break;
				
			}
	
		}

	}
}
