/*
 * Niko for DOL
 *
 */
using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Utils;
using DOL.GS.Quests;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&dmgr",
		ePrivLevel.GM,
		"GMCommands.dmgr.Description",
		"GMCommands.dmgr.Name",
		"GMCommands.dmgr.Level",
		"GMCommands.dmgr.Realm",
		"GMCommands.dmgr.Guild",
		"GMCommands.dmgr.Info",
		"GMCommands.dmgr.heal",
		"GMCommands.dmgr.locked",
		"GMCommands.dmgr.Unlocked",
		"GMCommands.dmgr.Update",
		"GMCommands.dmgr.Delete"
		)]
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
				client.Out.SendMessage("Vous devez avoir une porte en selection", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
				
			if( client.Player.TargetObject != null && (client.Player.TargetObject is GameNPC || client.Player.TargetObject is GamePlayer ))
			{
				client.Out.SendMessage("Vous devez avoir une porte en selection", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
				client.Out.SendMessage("La porte est deja dans la base.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
						client.Player.Out.SendMessage("Added door ID:" + DoorID + "to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
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
						client.Player.Out.SendMessage("Added door " + DoorID + " to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
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
					client.Out.SendMessage("La porte est supprimée.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if( DOOR == null )
				{
					client.Out.SendMessage("La porte n'existe pas dans la base.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
				client.Out.SendMessage( "Door name changé en: " + targetDoor.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow );
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
                client.Out.SendMessage( "Door guild changed to: " + targetDoor.Guild, eChatType.CT_System, eChatLoc.CL_SystemWindow );
            }
            else
            {
                if ( targetDoor.Guild != "" )
                {
                    targetDoor.Guild = "";
                    targetDoor.SaveIntoDatabase();
                    client.Out.SendMessage( "door guild removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow );
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
                client.Out.SendMessage( "Door level changed to: " + targetDoor.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow );
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
                client.Out.SendMessage( "Door realm changed to: " + targetDoor.Realm, eChatType.CT_System, eChatLoc.CL_SystemWindow );
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
			if ((int)targetDoor.Realm == 0)
				Realmname = "None";
				
			if ((int)targetDoor.Realm == 1)
				Realmname = "Albion";
				
			if ((int)targetDoor.Realm == 2)
				Realmname = "Midgard";
				
			if ((int)targetDoor.Realm == 3)
				Realmname = "Hibernia";
				
			if ((int)targetDoor.Realm == 6)
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
            client.Out.SendCustomTextWindow( "DOOR INFO by Niko", info );
        }
		
		private void heal( GameClient client, GameDoor targetDoor)
        {
                targetDoor.Health = targetDoor.MaxHealth;
                targetDoor.SaveIntoDatabase();
                client.Out.SendMessage( "Door health full restored : " + targetDoor.Health, eChatType.CT_System, eChatLoc.CL_SystemWindow );
        }
		private void locked ( GameClient client, GameDoor targetDoor )
		{
			targetDoor.Locked = 1;
			targetDoor.SaveIntoDatabase( );
			client.Out.SendMessage("Door "+ targetDoor.Name +" Locked !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void unlocked ( GameClient client, GameDoor targetDoor )
		{
			targetDoor.Locked = 0;
			targetDoor.SaveIntoDatabase( );
			client.Out.SendMessage("Door " + targetDoor.Name + " UnLocked !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		

		private void kill ( GameClient client, GameDoor targetDoor, string[] args )
		{
			try
			{
				targetDoor.AddAttacker(client.Player);
				targetDoor.AddXPGainer(client.Player, targetDoor.Health);
				targetDoor.Die(client.Player);
				targetDoor.XPGainers.Clear( );
				client.Out.SendMessage("Door '" + targetDoor.Name + "' killed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch( Exception e )
			{
				client.Out.SendMessage(e.ToString( ), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}		
		private string CheckName( string name, GameClient client )
		{
			if (name.Length > 47)
				client.Out.SendMessage("Attention le nom ne doit pas depasser 47 caracteres.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
		
		private string CheckGuildName(string name, GameClient client)
        {
            if (name.Length > 47)
                client.Out.SendMessage("WARNING: guild name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return name;
        }
	}
}