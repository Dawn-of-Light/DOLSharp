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
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&plvl",
		ePrivLevel.Admin,
		"AdminCommands.plvl.Description",
		"AdminCommands.plvl.Usage",
		"AdminCommands.plvl.Usage.Single",
		"AdminCommands.plvl.Usage.SingleAccount",
		"AdminCommands.plvl.Usage.Remove")]
	public class PlvlCommand : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			GamePlayer target = client.Player;

			switch (args[1])
			{
				#region Single
				case "single":
					{
						if( args.Length < 3 )
						{
							DisplaySyntax( client );
							return;
						}

						if( args.Length > 3 )
						{
							GameClient targetClient = WorldMgr.GetClientByPlayerName( args[3], true, true );

							if( targetClient != null )
								target = targetClient.Player;
						}

						SinglePermission.setPermission(target, args[2]);
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.AddedSinglePermission", target.Name, args[2]));

						break;
					}
				#endregion Single

				#region Single Account
				case "singleaccount":
					{
						if( args.Length < 3 )
						{
							DisplaySyntax( client );
							return;
						}

						if( args.Length > 3 )
						{
							GameClient targetClient = WorldMgr.GetClientByPlayerName( args[3], true, true );

							if( targetClient != null )
								target = targetClient.Player;
						}

						SinglePermission.setPermissionAccount( target, args[2] );
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.AddedSingleAccountPermission", target.Client.Account.Name, args[2]));

						break;
					}
				#endregion

				#region Remove
				case "remove":
					{
						if( args.Length < 2 )
						{
							DisplaySyntax( client );
							return;
						}

						if( args.Length > 3 )
						{
							GameClient targetClient = WorldMgr.GetClientByPlayerName( args[3], true, true );

							if( targetClient != null )
								target = targetClient.Player;
						}

						if( SinglePermission.removePermission( target, args[2] ) )
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.RemoveSinglePermission", target.Name, args[2]));
						else
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.NoPermissionForCommand", target.Name, args[2]));

						break;
					}
				#endregion Remove

				#region Default
				default:
					{
						uint plvl = 1;

						if( !UInt32.TryParse( args[1], out plvl ) )
						{
							DisplaySyntax( client );
							return;
						}

						if( args.Length > 2 )
							{
							GameClient targetClient = WorldMgr.GetClientByPlayerName( args[2], true, true );

							if( targetClient != null )
								target = targetClient.Player;
							}

									target.Client.Account.PrivLevel = plvl;
						GameServer.Database.SaveObject( target.Client.Account );

						foreach( GameNPC npc in client.Player.GetNPCsInRadius( WorldMgr.VISIBILITY_DISTANCE ) )
									{
							if( ( npc.Flags & (int)GameNPC.eFlags.CANTTARGET ) != 0 || ( npc.Flags & (int)GameNPC.eFlags.DONTSHOWNAME ) != 0 )
										{
								client.Out.SendNPCCreate( npc );
										}
									}

						target.Client.Out.SendMessage( LanguageMgr.GetTranslation( client, "AdminCommands.plvl.YourPlvlHasBeenSetted", plvl.ToString() ), eChatType.CT_Important, eChatLoc.CL_SystemWindow );

						if( target != client.Player )
							client.Out.SendMessage( LanguageMgr.GetTranslation( client, "AdminCommands.plvl.PlayerPlvlHasBeenSetted", target.Name, plvl.ToString() ), eChatType.CT_Important, eChatLoc.CL_SystemWindow );

						break;
					}
				#endregion Default
			}
		}
	}
}