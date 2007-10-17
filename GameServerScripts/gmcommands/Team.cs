using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	"&team",
	new string[] {"&te"},
   (uint)ePrivLevel.GM,
	"Server broadcast message for administrators",
	"/team <message>")]
	
	public class TeamCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if(args.Length < 2)
			{
				client.Out.SendMessage("Use: /st <message>",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}
			
			string msg = string.Join(" ",args,1,args.Length-1);
			
			foreach (GameClient player in WorldMgr.GetAllPlayingClients())
			{
				if(player.Account.PrivLevel >= 1)
				{
					player.Out.SendMessage("[Staff -Information]:\n " + msg,eChatType.CT_Staff,eChatLoc.CL_ChatWindow);
				}
			}
		return 0;
		}
	}
}