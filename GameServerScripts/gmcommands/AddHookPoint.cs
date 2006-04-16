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
/*using System;
using DOL.GS.PacketHandler;
using DOL.GS.Database;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		 "&addhookpoint",
		 (uint) ePrivLevel.GM,
		 "add hookpoint on a keep component",
		 "'/addhookpoint <skin> <id>' to add a hookpoint (select the gamekeepcomponent)")]
	public class HookPointCommandHandler : AbstractCommandHandler,ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				return 1;
			}
			int id = 0;
			int skin = 0;
			try
			{
				GameKeepComponent compo = client.Player.TargetObject as GameKeepComponent;
				if (compo == null)
				{
					this.DisplaySyntax(client);
					return 1;
				}
				skin = Convert.ToInt32(args[1]);
				id = Convert.ToInt32(args[2]);
				DBKeepHookPoint dbkeephp = new DBKeepHookPoint();
				dbkeephp.HookPointID = id;
				dbkeephp.KeepComponentSkinID = skin;
				Point p = client.Player.Position;
				Point c = compo.Position;
				dbkeephp.X = p.X - c.X;
				dbkeephp.Y = p.Y - c.Y;
				dbkeephp.Z = p.Z - c.Z;
				dbkeephp.Heading = compo.Heading;
				GameServer.Database.AddNewObject(dbkeephp);
			}
			catch(Exception e)
			{
				this.DisplayError(client,e.ToString());
				return 1;
			}
			return 1;
		}
	}
}*/
