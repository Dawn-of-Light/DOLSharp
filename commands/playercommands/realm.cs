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
using DOL.GS.PacketHandler;
using DOL.GS.Commands;
using DOL.GS.Effects;
using DOL.GS;
using DOL.Database;
using DOL.GS.Keeps;

namespace DOL.GS.Commands
{
    [CmdAttribute("&realm", //command to handle
        ePrivLevel.Player, //minimum privelege level
       "Displays the realm status.", //command description
       "/realm")] //usage
    public class RealmCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
        	// TODO: complete and make livelike
            ArrayList info = new ArrayList();
            DBKeep keep1 = (DBKeep)GameServer.Database.SelectObject(typeof(DBKeep), "Name = 'Dun Crauchon'");
           
            info.Add("Realm Info");
            info.Add(" ");
            info.Add("----------");
            info.Add("Keepname: Dun Crauchon");
            info.Add("Keeplevel: " + keep1.Level);
            info.Add("Owner: " + (eRealm)keep1.Realm);
            if (keep1.ClaimedGuildName != null)
                info.Add("Claimed Guild: " + keep1.ClaimedGuildName);

            client.Out.SendCustomTextWindow("Realm Info (snapshot)", info);
        }
    }
}
