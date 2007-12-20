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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&keepcomponent", //command to handle
		 ePrivLevel.GM, //minimum privelege level
		 "Various keep component creation commands!", //command description
		 "'/keepcomponent create <type> <keepid>' to create a keep component",
		 "'/keepcomponent create <type>' to create a keep component assign to nearest keep",
		 "'/keepcomponent skin <skin>' to change the skin",
		 "'/keepcomponent delete' to delete the component")]
	public class KeepComponentCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private readonly ushort INVISIBLE_MODEL = 150;

		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}
			AbstractGameKeep myKeep = KeepMgr.getKeepCloseToSpot(client.Player.CurrentRegionID,client.Player, WorldMgr.OBJ_UPDATE_DISTANCE );
			switch (args[1])
			{
				case "create":
				{
					if (args.Length < 3)
					{
						int i =0;
						foreach(string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
						{
							client.Out.SendMessage("#" + i +" : "+ str,eChatType.CT_System,eChatLoc.CL_SystemWindow);
							i++;
						}
						DisplaySyntax(client);
						return 1;
					}
					if (myKeep == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					int skin = 0;
					try
					{
						skin = Convert.ToInt32(args[2]);
					}
					catch
					{
						int i =0;
						foreach(string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
						{
							client.Out.SendMessage("#" + i +" : "+ str,eChatType.CT_System,eChatLoc.CL_SystemWindow);
							i++;
						}
						DisplaySyntax(client);
						return 1;
					}
					if (args.Length >= 4)
					{
						int keepid = 0;
						try
						{
							keepid = Convert.ToInt32(args[2]);
							myKeep = KeepMgr.getKeepByID(keepid);
						}
						catch
						{
							DisplaySyntax(client);
							return 1;
						}
					}
					GameKeepComponent component = new GameKeepComponent();
					component.X = client.Player.X;
					component.Y = client.Player.Y;
					component.Z = client.Player.Z;
					component.ComponentHeading = (client.Player.Heading-myKeep.Heading)/1024;
					component.Heading = (ushort) (component.ComponentHeading * 1024 + myKeep.Heading);
					component.Keep = myKeep;
					//todo good formula
					component.ComponentX = (component.X-myKeep.X)/148;
					component.ComponentY = (component.Y-myKeep.Y)/148;
					/*
					x = (component.X-myKeep.X)/148 = a*cos(t) - b*sin(t)
					y = (component.Y-myKeep.Y)/148 = a*sin(t) + b*cos(t)
					a = sqrt((x+b*sin(t))^2 + (y-b*cos(t))^2)
					a = sqrt(x²+y²+b² +2*x*b*sin(t)-2*y*b*cos(t))
					b = sqrt((x-a*cos(t))^2 + (y-a*sin(t))^2)
					b = sqrt(x²+y²+a²-2*x*a*cos(t)-2*y*a*sin(t))
					0 = 2x²+2y²-2*x*a*cos(t)-2*y*a*sin(t)+2*x*sqrt(x²+y²+a²-2*x*a*cos(t)-2*y*a*sin(t))*sin(t)-2*y*sqrt(x²+y²+a²-2*x*a*cos(t)-2*y*a*sin(t))*cos(t)
					pfff
					so must find an other way to find it....
					*/
					component.Name = myKeep.Name;
					component.Model = INVISIBLE_MODEL;
					component.Skin = skin;
					component.Level = (byte)myKeep.Level;
					component.Health = 100;
					component.CurrentRegion = client.Player.CurrentRegion;
					component.ID = myKeep.KeepComponents.Count;
					component.SaveInDB = true;
					component.AddToWorld();
					client.Out.SendMessage("You have created a keep component",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "skin":
				{
					if (args.Length < 3)
					{
						int i =0;
						foreach(string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
						{
							client.Out.SendMessage("#" + i +" : "+ str,eChatType.CT_System,eChatLoc.CL_SystemWindow);
							i++;
						}
						DisplaySyntax(client);
						return 1;
					}
					int skin = 0;
					try
					{
						skin = Convert.ToInt32(args[2]);
					}
					catch
					{
						DisplaySyntax(client);
						return 1;
					}
					GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
					if (component == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					component.Skin = skin;
					//todo update view of player
					component.SaveInDB = true;
					client.Out.SendMessage("You change the skin of current keep component",eChatType.CT_System,eChatLoc.CL_SystemWindow);

				}break;
				case "delete":
				{
					GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
					if (component == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					component.RemoveFromWorld();
					component.Delete();
					client.Out.SendMessage("You delete the current component",eChatType.CT_System,eChatLoc.CL_SystemWindow);

				}break;
				default :
				{
					DisplaySyntax(client);
					return 1;
				}
			}
			return 1;
		}
	}
}