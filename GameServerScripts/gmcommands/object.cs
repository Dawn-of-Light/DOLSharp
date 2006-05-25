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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&object", //command to handle
		 (uint)ePrivLevel.GM, //minimum privelege level
		"Various Object commands!", //command description
		 //usage
		 "'/object create' to create a default object",
		 "'/object model <newModel>' to set the model to newModel",
		 "'/object emblem <newEmblem>' to set the emblem to newEmblem",
		 "'/object name <newName>' set the targeted object name to newName",
		 "'/object remove' to remove the targeted object",
		 "'/object save' to save the object")]
	public class ObjectCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if(args.Length==1)
			{
				client.Out.SendMessage("Usage:",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object create' to create an default",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object model newModel' to set the model to newModel",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object model newModel' to set the emblem to newEmblem",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object name newName' to set the name to newName",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object remove' to remove the Object",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/object save' to save the Object",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 1;
			}
			string param="";
			if(args.Length>2)
				param=String.Join(" ",args,2,args.Length-2);

			GameStaticItem targetObject = client.Player.TargetObject as GameStaticItem;

			if(args[1]!="create" && targetObject==null)
			{
				client.Out.SendMessage("Type /object for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch(args[1])
			{
				case "info":
				{
					client.Out.SendMessage("[ "+" "+targetObject.Name+" "+" ]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage(" + Model: "+targetObject.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage(" + Realm: "+targetObject.Realm, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage(" + pos: "+targetObject.Position.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
				}
				case "movehere":
				{
					targetObject.MoveTo(client.Player.Region, client.Player.Position, (ushort)client.Player.Heading);
					break;
				}
				case "create":
				{
					//Create a new object
					GameStaticItem obj = new GameStaticItem();
					//Fill the object variables
					obj.Position = client.Player.Position;
					obj.Region=client.Player.Region;
					obj.Heading=client.Player.Heading;
					obj.Name="New Object";
					obj.Model=100;
					obj.AddToWorld();
					client.Out.SendMessage("Obj created: OID="+obj.ObjectID,eChatType.CT_System,eChatLoc.CL_SystemWindow);
					break;
				}
				case "model":
				{
					ushort model;
					try
					{
						model = Convert.ToUInt16(args[2]);
						targetObject.Model=model;
						client.Out.SendMessage("Object model changed to: "+targetObject.Model,eChatType.CT_System,eChatLoc.CL_SystemWindow);
					} 
					catch(Exception)
					{
						client.Out.SendMessage("Type /object for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					break;
				}
				case "name":
				{
					if(param!="")
					{
						targetObject.Name=param;
						client.Out.SendMessage("Object name changed to: "+targetObject.Name,eChatType.CT_System,eChatLoc.CL_SystemWindow);
  				}
  				break;
				}
				case "save":
				{
					if(targetObject.PersistantGameObjectID != 0) GameServer.Database.SaveObject(targetObject);
					else GameServer.Database.AddNewObject(targetObject);
					client.Out.SendMessage("Object saved to Database",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					break;
				}
				case "remove":
				{
					targetObject.RemoveFromWorld();
					if(targetObject.PersistantGameObjectID != 0) GameServer.Database.DeleteObject(targetObject);
					client.Out.SendMessage("Object removed from Clients and Database",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					break;
				}
			}
			return 1;
		}
	}
}