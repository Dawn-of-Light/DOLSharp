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
		 "'/object movehere' to move object to your location",
		 "'/object create [ObjectClassName]' to create a default object",
		 "'/object fastcreate [name] [modelID]' to create the specified object",
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

			if ( targetObject == null && args[1] != "create" && args[1] != "fastcreate" && args[1] != "target" )
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
						client.Out.SendMessage( " + Class: " + client.Player.TargetObject.GetType(), eChatType.CT_System, eChatLoc.CL_SystemWindow );

						GameInventoryItem invItem = client.Player.TargetObject as GameInventoryItem;
						if( invItem != null )
						{
							client.Out.SendMessage( " + Count: " + invItem.Item.Count, eChatType.CT_System, eChatLoc.CL_SystemWindow );
						}

						client.Out.SendMessage(" + [X]: " + targetObject.X + " [Y]: " + targetObject.Y + " [Z]: " + targetObject.Z, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "movehere":
					{
						targetObject.X = client.Player.X;
						targetObject.Y = client.Player.Y;
						targetObject.Z = client.Player.Z;
						targetObject.Heading = client.Player.Heading;
						targetObject.SaveIntoDatabase();
						break;
					}
				case "create":
					{
						string theType = "DOL.GS.GameStaticItem";
						if (args.Length > 2)
							theType = args[2];

						GameStaticItem obj = CreateItem( client, theType );

						if( obj != null )
							DisplayMessage(client, "Obj created: OID=" + obj.ObjectID);

						break;
					}
				case "fastcreate":
					{
						string objName = "new object";
						ushort modelID = 100;

						if ( args.Length > 2 )
							objName = args[2];

						if ( args.Length > 3 )
							ushort.TryParse( args[3], out modelID );

						GameStaticItem obj = CreateItem( client, null );

						if ( obj != null )
						{
							obj.Name = objName;
							obj.Model = modelID;
							DisplayMessage( client, "Object created: OID = " + obj.ObjectID );
						}

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
						DisplayMessage( client, "Object removed from Clients and Database" );
						break;
					}
				case "target":
					{
						foreach ( GameStaticItem item in client.Player.GetItemsInRadius( 1000 ) )
						{
							client.Player.TargetObject = item;
							DisplayMessage( client, "Target set to nearest object!" );
							return;
						}

						DisplayMessage( client, "No objects in 1000 unit range!" );
						break;
					}
			}
		}

		GameStaticItem CreateItem( GameClient client, string itemClassName )
		{
			GameStaticItem obj;

			if ( itemClassName != null && itemClassName.Length > 0 )
			{
				obj = null;
				ArrayList asms = new ArrayList( ScriptMgr.Scripts );
				asms.Add( typeof( GameServer ).Assembly );

				foreach ( Assembly script in asms )
				{
					try
					{
						client.Out.SendDebugMessage( script.FullName );
						obj = (GameStaticItem)script.CreateInstance( itemClassName, false );

						if ( obj != null )
							break;
					}
					catch ( Exception e )
					{
						DisplayMessage( client, e.ToString() );
					}
				}
			}
			else
			{
				obj = new GameStaticItem();
			}


			if ( obj == null )
			{
				client.Out.SendMessage( "There was an error creating an instance of " + itemClassName + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow );
				return null;
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

			return obj;
		}
	}
}