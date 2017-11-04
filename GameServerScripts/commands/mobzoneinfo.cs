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
 * This command was made to utilize the new 'zone' column in the 'mob' table in you database.
 * The command will export CurrectZone.Description info of existing mobs in your database and save it into the new column.
 */

using System;
using System.Reflection;
using System.Linq;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Commands
{
	[Cmd("&Mobzoneinfo",
		ePrivLevel.Admin,
		"Export mob zone info into database",
		"/mobzoneinfo load <from> <to>",
		"<from> and <to> must be the regionID numbers you wish to export",
		"This is done as a range, non-inclusive of <to> number.",
		"Currently the highest regionID is 499, so <to> should not exceed 500",
		"Example: /mobzoneinfo load 1 100 will export zone data of all of region 1 through to 99"
		)]
	public class MobzoneinfoCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private int From;
		private int To;
		
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 4 || !(args[1].ToLower().Equals("load")))
			{
				DisplaySyntax(client);
				return;
			}
			
			try
			{
				From = Convert.ToInt32(args[2]);
			}
			catch
			{
				client.Player.Out.SendMessage("The \"from\" value must be a valid number!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			try
			{
				To = Convert.ToInt32(args[3]);
			}
			catch
			{
				client.Player.Out.SendMessage("The \"to\" value must be a valid number!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				return;
			}
			if (From >= To)
			{
				client.Player.Out.SendMessage("The \"from\" value must be a number lower than the \"to\" number!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (From < 1)
			{
				client.Player.Out.SendMessage("The \"from\" value must be a valid number!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (To > 500)
			{
				client.Player.Out.SendMessage("The \"to\" value should not exceed 500! Check your database zone table for zone numbers!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (args[1].ToLower().Equals("load"))
			{					
				
				for (int i = From; i < To; i++)
				{
					client.Out.SendMessage("Exporting mob zone info for regionID " + i, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					MobZoneInfoExport((ushort)i);
				}
				client.Out.SendMessage("Exporting mob zone info complete!", eChatType.CT_System, eChatLoc.CL_SystemWindow);				
			}			
		}

		private void MobZoneInfoExport(ushort region)
		{
			foreach (GameNPC mob in WorldMgr.GetNPCsFromRegion(region))
			{
				if (!mob.LoadedFromScript)
				{					
					Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
					if (mobs != null)
					{
						mobs.Zone = mob.CurrentZone.Description;
						GameServer.Database.SaveObject(mobs);
					}										
				}
			}			
		}
	}	
}