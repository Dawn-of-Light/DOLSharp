/*
*Broadcast - Etaew - Fallen Realms v1
*
*ToDo: Sort areas for broadcast? not zones
*/
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&broadcast",
	  new string[] {"&b"},
	  (uint)ePrivLevel.Player,
		"Broadcast something to other players in the same zone",
		 "/b <message>")]
	public class BroadcastCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if(args.Length<2)
			{
				client.Out.SendMessage("You must broadcast something...",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 1;
			}

			bool broadcastDone = false;
			string message = string.Join(" ", args, 1, args.Length - 1);
			foreach (AbstractArea currentArea in client.Player.CurrentAreas)
			{
				if(currentArea.IsBroadcastEnabled == true)
				{
					foreach(GamePlayer player in currentArea.PlayersInArea)
					{
						if(GameServer.ServerRules.IsAllowedToUnderstand(client.Player, player))
						{
							player.Out.SendMessage("[Broadcast] " + client.Player.Name + ": " + message, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
						}
					}
					broadcastDone = true;
				}
			}
			
			if(broadcastDone == false)
			{
				client.Out.SendMessage("You cannot broadcast here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			
			return -1;
		}
	}
}
