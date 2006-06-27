/*
*Broadcast - Etaew - Fallen Realms v1
*
*ToDo: Sort areas for broadcast? not zones
*/
using DOL.GS;
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

			AbstractArea targetArea = CheckArea(client.Player);

			if (targetArea == null)
			{
				client.Out.SendMessage("You cannot broadcast here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			string message = string.Join(" ", args, 1, args.Length - 1);

			Broadcast(client.Player.Name, message, targetArea, client.Player.CurrentRegionID);

			return 1;
		}

		private AbstractArea CheckArea(GamePlayer player)
		{
			foreach (AbstractArea thisArea in player.CurrentAreas)
			{
				if (AreaMgr.BroadcastableAreas.Contains(thisArea)) return thisArea;
			}
			return null;
		}

		private void Broadcast(string name, string message, AbstractArea area, ushort regionID)
		{
			foreach (GameClient thisClient in WorldMgr.GetClientsOfRegion(regionID))
			{
				if (thisClient.Player.CurrentAreas.Contains(area))
					thisClient.Player.Out.SendMessage("[Broadcast] " + name + ": " + message, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
			}
		}
	}
}
