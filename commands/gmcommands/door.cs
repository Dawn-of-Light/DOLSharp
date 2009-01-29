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
 
 /*
  * New system by Niko jan 2009
  */

using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Utils;
using DOL.GS.Quests;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&door",
		ePrivLevel.GM,
		"GMCommands.door.Description",
		"GMCommands.door.Add",
		"GMCommands.door.Update",
		"GMCommands.door.Delete",
		"GMCommands.door.Name",
		"GMCommands.door.Level",
		"GMCommands.door.Realm",
		"GMCommands.door.Guild",
		"GMCommands.door.Info",
		"GMCommands.door.Heal",
		"GMCommands.door.Locked",
		"GMCommands.door.Unlocked")]
	public class NewDoorCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private int DoorID;
		private int DoorIDint;
		private int doorType;
		
		public void OnCommand(GameClient client, string[] args)
		{
			GameDoor targetDoor = null;
			
			if ( client.Player.TargetObject == null )
			{
				client.Out.SendMessage("You must target a door", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
				
			if( client.Player.TargetObject != null && (client.Player.TargetObject is GameNPC || client.Player.TargetObject is GamePlayer ))
			{
				client.Out.SendMessage("You must target a door", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
				
            if ( client.Player.TargetObject != null && client.Player.TargetObject is GameDoor )
			{
                targetDoor = (GameDoor)client.Player.TargetObject;
				DoorID = targetDoor.DoorID;
				doorType = targetDoor.DoorID / 100000000;
			}
			
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
				
			switch (args[1])
            {
                case "name": name( client, targetDoor, args ); break;
				case "guild": guild( client, targetDoor, args ); break;
				case "level": level( client, targetDoor, args ); break;
				case "realm": realm( client, targetDoor, args ); break;
				case "info": info( client, targetDoor); break;
				case "heal": heal( client, targetDoor); break;
				case "locked": locked(client, targetDoor); break;
				case "unlocked": unlocked(client, targetDoor); break;
				case "kill": kill(client, targetDoor, args); break;
				case "delete": delete(client, targetDoor); break;
				case "add": add(client, targetDoor); break;
				case "update": update(client, targetDoor); break;
				   
                default:
                    DisplaySyntax( client );
                    return;
            }
		}

		private void add( GameClient client, GameDoor targetDoor)
		{

			DBDoor DOOR = (DBDoor)GameServer.Database.SelectObject(typeof(DBDoor), "InternalID = '" + DoorID + "'");

			if (DOOR != null)
			{
				client.Out.SendMessage("The door is already in the database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
				if (DOOR == null)
				{
					if(doorType != 7 && doorType != 9)
					{
						DBDoor door = new DBDoor( );
						door.ObjectId = null;
						door.InternalID = DoorID;						
						door.Name = "door";
						door.Type = DoorID / 100000000;
						door.Level = 20;
						door.Realm = 6;
						door.X = targetDoor.X;
						door.Y = targetDoor.Y;
						door.Z = targetDoor.Z;
						door.Heading = targetDoor.Heading;
						door.Health = 2545;
						GameServer.Database.AddNewObject(door);
						((GameLiving)targetDoor).AddToWorld( );
						client.Player.Out.SendMessage("Added door ID:" + DoorID + "to the database", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						//DoorMgr.Init( );
						return;
					}
				}  
			}
			private void update ( GameClient client, GameDoor targetDoor )
			{
				delete(client, targetDoor);
				
				if( targetDoor != null)
				{
					if( doorType != 7 && doorType != 9 )
					{
						DBDoor door = new DBDoor( );
						door.ObjectId = null;
						door.InternalID = DoorID;
						door.Name = "door";
						door.Type = DoorID / 100000000;
						door.Level = targetDoor.Level;
						door.Realm = (byte)targetDoor.Realm;
						door.Health = targetDoor.Health;
						door.Locked = targetDoor.Locked;
						door.X = client.Player.X;
						door.Y = client.Player.Y;
						door.Z = client.Player.Z;
						door.Heading = client.Player.Heading;
						GameServer.Database.AddNewObject(door);
						((GameLiving)targetDoor).AddToWorld( );
						client.Player.Out.SendMessage("Added door " + DoorID + " to the database", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						return;
					}
				}
			}
				
			private void delete ( GameClient client, GameDoor targetDoor)
			{

				DBDoor DOOR = (DBDoor)GameServer.Database.SelectObject(typeof(DBDoor), "InternalID = '" + DoorID + "'");

				if( DOOR != null )
				{
					GameServer.Database.DeleteObject(DOOR);
					client.Out.SendMessage("Door removed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if( DOOR == null )
				{
					client.Out.SendMessage("This door doesn't exist in the database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
		
		
		
		private void name( GameClient client, GameDoor targetDoor, string[] args )
		{
			string doorName = "";

			if ( args.Length > 2 )
				doorName = String.Join( " ", args, 2, args.Length - 2 );

			if ( doorName != "" )
			{
				targetDoor.Name = CheckName( doorName, client );
				targetDoor.SaveIntoDatabase();
				client.Out.SendMessage( "You changed the door name to " + targetDoor.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow );
			}
			else
			{
				DisplaySyntax( client, args[1] );
			}
		}
		
		private void guild( GameClient client, GameDoor targetDoor, string[] args )
        {
            string guildName = "";

            if ( args.Length > 2 )
                guildName = String.Join( " ", args, 2, args.Length - 2 );

            if ( guildName != "" )
            {
                targetDoor.Guild = CheckGuildName( guildName, client );
                targetDoor.SaveIntoDatabase();
                client.Out.SendMessage( "You changed the door guild to " + targetDoor.Guild, eChatType.CT_System, eChatLoc.CL_SystemWindow );
            }
            else
            {
                if ( targetDoor.Guild != "" )
                {
                    targetDoor.Guild = "";
                    targetDoor.SaveIntoDatabase();
                    client.Out.SendMessage( "Door guild removed", eChatType.CT_System, eChatLoc.CL_SystemWindow );
                }
                else
                    DisplaySyntax( client, args[1] );
            }
        }
		
		private void level( GameClient client, GameDoor targetDoor, string[] args )
        {
            byte level;

            try
            {
                level = Convert.ToByte( args[2] );
                targetDoor.Level = level;
                targetDoor.Health = targetDoor.MaxHealth;
                targetDoor.SaveIntoDatabase();
                client.Out.SendMessage( "You changed the door level to " + targetDoor.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow );
            }
            catch ( Exception )
            {
                DisplaySyntax( client, args[1] );
            }
        }
		
		private void realm( GameClient client, GameDoor targetDoor, string[] args )
        {
            byte realm;

            try
            {
                realm = Convert.ToByte( args[2] );
                targetDoor.Realm = (eRealm)realm;
                targetDoor.SaveIntoDatabase();
                client.Out.SendMessage( "You changed the door realm to " + targetDoor.Realm, eChatType.CT_System, eChatLoc.CL_SystemWindow );
            }
            catch ( Exception )
            {
                DisplaySyntax( client, args[1] );
            }
        }
		private string Realmname;
		private string statut;
		
		private void info( GameClient client, GameDoor targetDoor)
        {
			if (targetDoor.Realm == eRealm.None)
				Realmname = "None";
				
			if (targetDoor.Realm == eRealm.Albion)
				Realmname = "Albion";
				
			if (targetDoor.Realm == eRealm.Midgard)
				Realmname = "Midgard";
				
			if (targetDoor.Realm == eRealm.Hibernia)
				Realmname = "Hibernia";
				
			if (targetDoor.Realm == eRealm.Door)
				Realmname = "All";
				
			if (targetDoor.Locked == 1)
				statut = " Locked";
				
			if( targetDoor.Locked == 0 )
				statut = " Unlocked";

			int doorType = DoorRequestHandler.DoorIDhandler / 100000000;
				
            ArrayList info = new ArrayList();

            info.Add(" + Info sur la porte :  " + targetDoor.Name);
			info.Add( "  " );
			info.Add( " + Name : " + targetDoor.Name );
			info.Add(" + ID : " + DoorID);
            info.Add( " + Realm : " + (int)targetDoor.Realm + " : " +Realmname );
            info.Add( " + Level : " + targetDoor.Level );
            info.Add( " + Guild : " + targetDoor.Guild );
			info.Add( " + Health : " + targetDoor.Health +" / "+ targetDoor.MaxHealth);
			info.Add(" + Statut : " + statut);
			info.Add(" + Type : " + doorType);
			info.Add(" + X : " + targetDoor.X);  
			info.Add(" + Y : " + targetDoor.Y);
			info.Add(" + Z : " + targetDoor.Z);
			info.Add(" + Heading : " + targetDoor.Heading);
            client.Out.SendCustomTextWindow( "Door Information", info );
        }
		
		private void heal( GameClient client, GameDoor targetDoor)
        {
                targetDoor.Health = targetDoor.MaxHealth;
                targetDoor.SaveIntoDatabase();
                client.Out.SendMessage( "You change the door health to " + targetDoor.Health, eChatType.CT_System, eChatLoc.CL_SystemWindow );
        }
		private void locked ( GameClient client, GameDoor targetDoor )
		{
			targetDoor.Locked = 1;
			targetDoor.SaveIntoDatabase( );
			client.Out.SendMessage("Door "+ targetDoor.Name +" is locked", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void unlocked ( GameClient client, GameDoor targetDoor )
		{
			targetDoor.Locked = 0;
			targetDoor.SaveIntoDatabase( );
			client.Out.SendMessage("Door " + targetDoor.Name + " is unlocked", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		

		private void kill ( GameClient client, GameDoor targetDoor, string[] args )
		{
			try
			{
				targetDoor.AddAttacker(client.Player);
				targetDoor.AddXPGainer(client.Player, targetDoor.Health);
				targetDoor.Die(client.Player);
				targetDoor.XPGainers.Clear( );
				client.Out.SendMessage("Door " + targetDoor.Name + " health reaches 0", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch( Exception e )
			{
				client.Out.SendMessage(e.ToString( ), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}		
		private string CheckName( string name, GameClient client )
		{
			if (name.Length > 47)
				client.Out.SendMessage("The door name must not be longer than 47 bytes", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
		
		private string CheckGuildName(string name, GameClient client)
        {
            if (name.Length > 47)
                client.Out.SendMessage("The guild name is " + name.Length + ", but only 47 bytes 'll be displayed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return name;
        }
	}
}