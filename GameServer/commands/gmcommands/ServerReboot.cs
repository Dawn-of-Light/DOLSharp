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
 //Created by Loki2020


using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Commands
{
    [Cmd("&ServerReboot",ePrivLevel.GM,"Restarts the server instantly!!")]
    public class RestartCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void OnCommand(GameClient client, string[] args)
        {
            client.Out.SendCustomDialog(string.Format("Do you wish to reboot the server instantly!!"), new CustomDialogResponse(RebootResponse));
        }
		protected void RebootResponse(GamePlayer player, byte response)
		{		
			if (response != 0x01)
			{			
				return;
			}

            new Thread(new ThreadStart(ShutDownServer)).Start();
            log.Info("Server Rebooted by " + player.Name + "");           
        }
        public static void ShutDownServer()
        {
            if (GameServer.Instance.IsRunning)
            {
                GameServer.Instance.Stop();  
                Thread.Sleep(2000);
                Process.Start(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "DOLServer.exe"));
                Environment.Exit(0);
            }
        }
    }
}
