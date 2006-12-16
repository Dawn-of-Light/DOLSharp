using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&boot",
	  (int)ePrivLevel.Player,
	   "Kicks a player out of your house",
		 "Useage: /boot [playername]")]
	public class BootCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			int num = HouseMgr.GetHouseNumberByPlayer(client.Player);
			if (num == 0)
			{
				DisplayError(client, "You don't own a house!", new object[] { });
				return 0;
			}

			if (!client.Player.InHouse || client.Player.CurrentHouse.HouseNumber != num)
			{
				DisplayError(client, "You have to be in your house to use this command!", new object[] { });
				return 0;
			}

			House house = HouseMgr.GetHouse(num);

			foreach (GamePlayer player in house.GetAllPlayersInHouse())
			{
				if (player != client.Player && player.Name.IndexOf(args[1]) > -1)
				{
					string message = "You have been removed from the house by " + client.Player.Name;
					player.Out.SendMessage(message + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.LeaveHouse();
					return 1;
				}
			}
			
			client.Out.SendMessage("No one with that name is online to Kick!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
			return 0;
		}
	}
}