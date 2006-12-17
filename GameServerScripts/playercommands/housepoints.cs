using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&housepoints",
	  (int)ePrivLevel.Player,
	   "Toggles display of housepoints",
		 "Useage: /housepoints toggle")]
	public class HousePointsCommandHandler : AbstractCommandHandler, ICommandHandler
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

			client.Player.Out.SendToggleHousePoints(house);
			return 0;
		}
	}
}