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
using System.Reflection;
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&area",
		ePrivLevel.GM,
		"GMCommands.Area.Description",
		"GMCommands.Area.Usage.Create")]
	public class AreaCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				#region Create
				case "create":
					{
						if (args.Length != 7)
						{
							DisplaySyntax(client);
							return;
						}

						DBArea area = new DBArea();
						area.Description = args[2];

						switch (args[3].ToLower())
						{
							case "circle": area.ClassType = "DOL.GS.Area+Circle"; break;
							case "square": area.ClassType = "DOL.GS.Area+Square"; break;
							case "safe":
							case "safearea": area.ClassType = "DOL.GS.Area+SafeArea"; break;
							case "bind":
							case "bindarea": area.ClassType = "DOL.GS.Area+BindArea"; break;
							default:
								{
									DisplaySyntax(client);
									return;
								}
						}

						area.Radius = Convert.ToInt16(args[4]);
						switch (args[5].ToLower())
						{
							case "y": { area.CanBroadcast = true; break; }
							case "n": { area.CanBroadcast = false; break; }
							default: { DisplaySyntax(client); return; }
						}
						area.Sound = byte.Parse(args[6]);
						area.Region = client.Player.CurrentRegionID;
						area.X = client.Player.X;
						area.Y = client.Player.Y;
						area.Z = client.Player.Z;

						Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
						AbstractArea newArea = (AbstractArea)gasm.CreateInstance(area.ClassType, false);
						newArea.LoadFromDatabase(area);

						newArea.Sound = area.Sound;
						newArea.CanBroadcast = area.CanBroadcast;
						WorldMgr.GetRegion(client.Player.CurrentRegionID).AddArea(newArea);
						GameServer.Database.AddNewObject(area);
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Area.AreaCreated", area.Description, area.X, area.Z, area.Radius, area.CanBroadcast.ToString(), area.Sound));
						break;
					}
				#endregion Create
				#region Default
				default:
					{
						DisplaySyntax(client);
						break;
					}
				#endregion Default
			}
		}
	}
}