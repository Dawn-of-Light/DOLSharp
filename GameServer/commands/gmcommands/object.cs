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
using DOL.Database2;
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
		 "'/object save' to save the object",
		 "'/object target' to automatically target the nearest object")]
	public class ObjectCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}
			string param = "";
			if (args.Length > 2)
				param = String.Join(" ", args, 2, args.Length - 2);

			GameStaticItem targetObject = client.Player.TargetObject as GameStaticItem;

			if (args[1] != "create" && targetObject == null)
			{
				client.Out.SendMessage("Type /object for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
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

						//Create a new mob
						GameStaticItem obj = null;
						ArrayList asms = new ArrayList(ScriptMgr.Scripts);
						asms.Add(typeof(GameServer).Assembly);
						foreach (Assembly script in asms)
						{
							try
							{
								client.Out.SendDebugMessage(script.FullName);
								obj = (GameStaticItem)script.CreateInstance(theType, false);
								if (obj != null)
									break;
							}
							catch (Exception e)
							{
								DisplayMessage(client, e.ToString());
							}
						}
						if (obj == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + theType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
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
						DisplayMessage(client, "Obj created: OID=" + obj.ObjectID);
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
							DisplayMessage(client, "Object model changed to: " + targetObject.Model);
						}
						catch (Exception)
						{
							DisplayMessage(client, "Type /object for command overview");
							return;
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
							DisplayMessage(client, "Object emblem changed to: " + targetObject.Emblem);
						}
						catch (Exception)
						{
							DisplayMessage(client, "Type /object for command overview");
							return;
						}
						break;
					}
				case "name":
					{
						if (param != "")
						{
							targetObject.Name = param;
							targetObject.SaveIntoDatabase();
							DisplayMessage(client, "Object name changed to: " + targetObject.Name);
						}
						break;
					}
				case "save":
					{
						targetObject.SaveIntoDatabase();
						DisplayMessage(client, "Object saved to Database");
						break;
					}
				case "remove":
					{
						targetObject.DeleteFromDatabase();
						targetObject.Delete();
						DisplayMessage(client, "Object removed from Clients and Database");
						break;
					}
				case "target":
					{
						foreach (GameStaticItem item in client.Player.GetItemsInRadius(1000))
						{
							client.Player.TargetObject = item;
							DisplayMessage(client, "Target set to nearest object!");
							return;
						}
						DisplayMessage(client, "No objects in 1000 unit range!");
						break;
					}
			}
		}
	}
}