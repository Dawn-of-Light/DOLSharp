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

using DOL.Database2;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&addhookpoint",
		 ePrivLevel.GM,
		 "add hookpoint on a keep component",
		 "'/addhookpoint <skin> <id>' to add a hookpoint (select the gamekeepcomponent)")]
	public class HookPointCommandHandler : AbstractCommandHandler,ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}
			int id = 0;
			int skin = 0;
			try
			{
				GameKeepComponent compo = client.Player.TargetObject as GameKeepComponent;
				if (compo == null)
				{
					DisplayMessage(client, "You are not targeting a GameKeepComponent!");
					return;
				}
				skin = Convert.ToInt32(args[1]);
				id = Convert.ToInt32(args[2]);
				DBKeepHookPoint dbkeephp = new DBKeepHookPoint();
				dbkeephp.HookPointID = id;
				dbkeephp.KeepComponentSkinID = skin;
				dbkeephp.X = client.Player.X - compo.X;
				dbkeephp.Y = client.Player.Y - compo.Y;
				dbkeephp.Z = client.Player.Z - compo.Z;
				dbkeephp.Heading = client.Player.Heading - compo.Heading;
			}
			catch(Exception e)
			{
				DisplayMessage(client,e.ToString());
			}
		}
	}
}
