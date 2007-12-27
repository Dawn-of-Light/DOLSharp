using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	  "&housepoints",
	  ePrivLevel.Player,
	   "Toggles display of housepoints",
		 "Useage: /housepoints toggle")]
	public class HousePointsCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			House house = HouseMgr.GetHouseByPlayer(client.Player);
			if (house == null)
			{
				DisplayMessage(client, "You don't own a house!");
				return;
			}

			if (!client.Player.InHouse || client.Player.CurrentHouse != house)
			{
				DisplayMessage(client, "You have to be in your house to use this command!");
				return;
			}

			client.Player.Out.SendToggleHousePoints(house);
		}
	}
}