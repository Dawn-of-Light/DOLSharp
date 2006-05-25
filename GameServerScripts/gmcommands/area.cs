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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&area",
		(uint) ePrivLevel.GM,
		"various commands to help you with areas",
		"/area create <type> <name> <radius> <broadcast(y/n)> <soundid>")]
	public class AreaCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				ShowSyntax(client.Player);
				return 1;
			}
			switch (args[1].ToLower())
			{
				case "create":
					{
						if (args.Length != 7)
						{
							ShowSyntax(client.Player);
							return 1;
						}

						try
						{
							AbstractArea newArea = Assembly.GetAssembly(typeof (GameServer)).CreateInstance(args[2], false) as AbstractArea;
							if(newArea == null)
							{
								client.Out.SendMessage("Area type are only DOL.GS.Square or DOL.GS.Circle !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return -1;
							}

							Point centerSpot = client.Player.Position;
							newArea.Description = args[3];
							newArea.RegionID = client.Player.Region.RegionID;
							if (args[5].ToLower() == "y")
								newArea.IsBroadcastEnabled = true;
							else 
								newArea.IsBroadcastEnabled = false;

							newArea.Sound = byte.Parse(args[6]);

							ushort radius = Convert.ToUInt16(args[4]);
							if (newArea is Square)
							{
								((Square)newArea).X = centerSpot.X - radius / 2;
								((Square)newArea).Y = centerSpot.Y - radius / 2;
								((Square)newArea).Width = radius;
								((Square)newArea).Height = radius;
							}
							else if (newArea is Circle)
							{
								((Circle)newArea).X = centerSpot.X ;
								((Circle)newArea).Y = centerSpot.Y;
								((Circle)newArea).Radius = radius;
							}

							if(AreaMgr.RegisterArea(newArea))
							{
								GameServer.Database.AddNewObject(newArea);

								client.Player.Out.SendMessage("Area created - Type : "+newArea.GetType(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								
								client.Player.Out.SendMessage("Details :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								client.Player.Out.SendMessage("- Description = "+ newArea.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								client.Player.Out.SendMessage("- IsBroacastEnabled = "+ newArea.IsBroadcastEnabled, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								client.Player.Out.SendMessage("- Sound = "+ newArea.Sound, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								client.Player.Out.SendMessage("- Region = "+ newArea.RegionID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								
								if(newArea is Square)
								{
									client.Player.Out.SendMessage("X = "+((Square)newArea).X, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									client.Player.Out.SendMessage("Y = "+((Square)newArea).Y, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									client.Player.Out.SendMessage("Width = "+((Square)newArea).Width, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									client.Player.Out.SendMessage("Height = "+((Square)newArea).Height, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else if(newArea is Circle)
								{
									client.Player.Out.SendMessage("X = "+((Circle)newArea).X, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									client.Player.Out.SendMessage("Y = "+((Circle)newArea).Y, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									client.Player.Out.SendMessage("Radius = "+((Circle)newArea).Radius, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}	
							}
							else
							{
								client.Out.SendMessage("This area can't be registered.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("Type /area for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}

						break;
					}	
			}
			return 1;
		}
		public void ShowSyntax(GamePlayer player)
		{
			player.Out.SendMessage("Usage: /area", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			player.Out.SendMessage("/area create <type> <name> <radius> <broadcast(y/n)> <soundid>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}