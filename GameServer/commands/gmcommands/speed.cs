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

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&speed",
		ePrivLevel.GM,
		"Change base speed of target (no parameter to see current speed)",
		"/speed [newSpeed]")]
	public class SpeedCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
            GamePlayer player = client.Player;
            GameLiving target = player.TargetObject as GameLiving;

            if ( target == null )
            {
                DisplayMessage( client, "You have not selected a valid target" );
                return;
            }

			if (args.Length == 1)
			{
                DisplayMessage( player, ( player == target ? "Your" : target.Name ) + " maximum speed is " + target.MaxSpeedBase );
				return;
			}

            short speed;

            if ( short.TryParse( args[1], out speed ) )
            {
                target.MaxSpeedBase = speed;

                GameNPC npc = target as GameNPC;

                if ( npc == null )
                {
                    GamePlayer targetPlayer = target as GamePlayer;

                    if ( targetPlayer != null )
                    {
                        targetPlayer.Out.SendUpdateMaxSpeed();
                    }
                }
                else
                {
                    if ( npc.LoadedFromScript == false )
                    {
                        npc.SaveIntoDatabase();
                    }
                }

                DisplayMessage( player, ( player == target ? "Your" : target.Name ) + " maximum speed is now " + target.MaxSpeedBase );
            }
            else
            {
                DisplaySyntax( client );
            }
        }
	}
}