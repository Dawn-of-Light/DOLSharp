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
using System.Collections.Generic;
using System.Reflection;

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
	              "'/object realm <0/1/2/3>' to set the targeted object realm",
	              "'/object name <newName>' to set the targeted object name to newName",
	              "'/object noname' to remove the targeted object name",
	              "'/object remove' to remove the targeted object",
	              "'/object copy' to copy the targeted object",
	              "'/object save' to save the object",
	              "'/object reload' to reload the object",
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
						List<string> info = new List<string>();
						
						string name = "(blank name)";
						if (!string.IsNullOrEmpty(targetObject.Name))
							name = targetObject.Name;
						
						info.Add(" OID: " + targetObject.ObjectID);
						info.Add (" Type: " + targetObject.GetType());
						info.Add(" ");
						info.Add(" Name: " + name);
						info.Add(" Model: " + targetObject.Model);
						info.Add(" Emblem: " + targetObject.Emblem);
						info.Add(" Realm: " + targetObject.Realm);
						if (targetObject.Owners.LongLength > 0)
						{
							info.Add(" ");
							info.Add(" Owner: " + targetObject.Owners[0].Name);
						}
						if (string.IsNullOrEmpty(targetObject.OwnerID) == false)
						{
							info.Add(" ");
							info.Add(" OwnerID: " + targetObject.OwnerID);
						}

						info.Add(" ");

						WorldInventoryItem invItem = targetObject as WorldInventoryItem;
						if( invItem != null )
						{
							info.Add (" Count: " + invItem.Item.Count);
						}

						info.Add(" ");
						info.Add(" Location: X= " + targetObject.X + " ,Y= " + targetObject.Y + " ,Z= " + targetObject.Z);
						
						client.Out.SendCustomTextWindow( "[ " + name + " ]", info );
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
				case "realm":
					{
						eRealm realm = eRealm.None;
						if (args[2] == "0") realm = eRealm.None;
						if (args[2] == "1") realm = eRealm.Albion;
						if (args[2] == "2") realm = eRealm.Midgard;
						if (args[2] == "3") realm = eRealm.Hibernia;
						targetObject.Realm = realm;
						targetObject.SaveIntoDatabase();
						DisplayMessage(client, "Object realm changed to: " + targetObject.Realm);
						
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
				case "noname":
					{
						targetObject.Name = "";
						targetObject.SaveIntoDatabase();
						DisplayMessage(client, "Object name removed");
						break;
					}
				case "copy":
					{
						GameStaticItem item = CreateItemInstance(client, targetObject.GetType().FullName);
						if (item == null)
						{
							ChatUtil.SendSystemMessage(client, "There was an error creating an instance of " + targetObject.GetType().FullName + "!");
							return;
						}
						item.X = client.Player.X;
						item.Y = client.Player.Y;
						item.Z = client.Player.Z;
						item.CurrentRegion = client.Player.CurrentRegion;
						item.Heading = client.Player.Heading;
						item.Level = targetObject.Level;
						item.Name = targetObject.Name;
						item.Model = targetObject.Model;
						item.Realm = targetObject.Realm;
						item.Emblem = targetObject.Emblem;
						item.LoadedFromScript = targetObject.LoadedFromScript;
						item.AddToWorld();
						item.SaveIntoDatabase();
						DisplayMessage(client, "Obj created: OID=" + item.ObjectID);
						break;
					}
				case "save":
					{
						targetObject.LoadedFromScript = false;
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
				case "reload":
					{
						targetObject.RemoveFromWorld(2);
						DisplayMessage(client, "Object reloading");
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

		GameStaticItem CreateItemInstance(GameClient client, string itemClassName)
		{
			GameStaticItem obj = null;
			ArrayList asms = new ArrayList(ScriptMgr.Scripts);
			asms.Add(typeof(GameServer).Assembly);

			foreach (Assembly script in asms)
			{
				try
				{
					client.Out.SendDebugMessage(script.FullName);
					obj = (GameStaticItem)script.CreateInstance(itemClassName, false);

					if (obj != null)
						break;
				}
				catch (Exception e)
				{
					DisplayMessage(client, e.ToString());
				}
			}
			return obj;
		}

		GameStaticItem CreateItem(GameClient client, string itemClassName)
		{
			GameStaticItem obj;

			if (!string.IsNullOrEmpty(itemClassName))
				obj = CreateItemInstance(client, itemClassName);
			else
				obj = new GameStaticItem();


			if (obj == null)
			{
				client.Out.SendMessage( "There was an error creating an instance of " + itemClassName + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow );
				return null;
			}

			//Fill the object variables
			obj.LoadedFromScript = false;
			obj.X = client.Player.X;
			obj.Y = client.Player.Y;
			obj.Z = client.Player.Z;
			obj.CurrentRegion = client.Player.CurrentRegion;
			obj.Heading = client.Player.Heading;
			obj.Name = "New Object";
			obj.Model = 100;
			obj.Emblem = 0;
			obj.Realm = 0;
			obj.AddToWorld();
			obj.SaveIntoDatabase();

			return obj;
		}
	}
}