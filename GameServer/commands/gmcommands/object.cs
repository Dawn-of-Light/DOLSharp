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
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&object", //command to handle
		 ePrivLevel.GM, //minimum privelege level
		"Various Object commands!", //command description
		//usage
		 "'/object info' to get information about the object",
		 "'/object create' to create a default object",
		 "'/object model <newModel>' to set the model to newModel",
		 "'/object emblem <newEmblem>' to set the emblem to newEmblem",
		 "'/object name <newName>' to set the targeted object name to newName",
		 "'/object remove' to remove the targeted object",
		 "'/object save' to save the object")]
	public class ObjectCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object create' to create an default", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object model newModel' to set the model to newModel", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object model newModel' to set the emblem to newEmblem", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object name newName' to set the name to newName", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object remove' to remove the Object", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object save' to save the Object", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string param = "";
			if (args.Length > 2)
				param = String.Join(" ", args, 2, args.Length - 2);

			GameStaticItem targetObject = client.Player.TargetObject as GameStaticItem;

			if (args[1] != "create" && targetObject == null)
			{
				client.Out.SendMessage("Type /object for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (args[1])
			{
				case "info":
					{
						client.Out.SendMessage("[ " + " " + targetObject.Name + " " + " ]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(" + Model: " + targetObject.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(" + Emblem: " + targetObject.Emblem, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(" + [X]: " + targetObject.X + " [Y]: " + targetObject.Y + " [Z]: " + targetObject.Z, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "movehere":
					{
						targetObject.X = client.Player.X;
						targetObject.Y = client.Player.Y;
						targetObject.Z = client.Player.Z;
						targetObject.Heading = client.Player.Heading;
						break;
					}
				case "create":
					{
						string theType = "DOL.GS.GameStaticItem";
						if (args.Length > 2)
							theType = args[2];

						//Create a new object
						GameStaticItem obj = new GameStaticItem();
						try
						{
							client.Out.SendDebugMessage(Assembly.GetAssembly(typeof(GameServer)).FullName);
							obj = (GameStaticItem)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(theType, false);
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
						if (obj == null)
						{
							try
							{
								client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
								obj = (GameStaticItem)Assembly.GetExecutingAssembly().CreateInstance(theType, false);
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						if (obj == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + theType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}

						//Fill the object variables
						obj.X = client.Player.X;
						obj.Y = client.Player.Y;
						obj.Z = client.Player.Z;
						obj.CurrentRegion = client.Player.CurrentRegion;
						obj.Heading = client.Player.Heading;
						obj.Name = "New Object";
						obj.Model = 100;
						obj.Emblem = 0;
						obj.AddToWorld();
						obj.SaveIntoDatabase();
						client.Out.SendMessage("Obj created: OID=" + obj.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "model":
					{
						ushort model;
						try
						{
							model = Convert.ToUInt16(args[2]);
							targetObject.Model = model;
							targetObject.SaveIntoDatabase();
							client.Out.SendMessage("Object model changed to: " + targetObject.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /object for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						break;
					}
				case "emblem":
					{
						int emblem;
						try
						{
							emblem = Convert.ToInt32(args[2]);
							targetObject.Emblem = emblem;
							targetObject.SaveIntoDatabase();
							client.Out.SendMessage("Object emblem changed to: " + targetObject.Emblem, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /object for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						break;
					}
				case "name":
					{
						if (param != "")
						{
							targetObject.Name = param;
							targetObject.SaveIntoDatabase();
							client.Out.SendMessage("Object name changed to: " + targetObject.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
					}
				case "save":
					{
						targetObject.SaveIntoDatabase();
						client.Out.SendMessage("Object saved to Database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "remove":
					{
						targetObject.DeleteFromDatabase();
						targetObject.Delete();
						client.Out.SendMessage("Object removed from Clients and Database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "target":
					{
						//todo targets the nearest object
						break;
					}
			}
			return 1;
		}
	}
}