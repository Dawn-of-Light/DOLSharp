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
            House house = client.Player.CurrentHouse;
			if (!client.Player.InHouse || house == null)
			{
                DisplayMessage(client, "You need to be in a House to use this command!");
				return;
			}

            if (!house.HasOwnerPermissions(client.Player))
            {
                DisplayMessage(client, "You do not have permissions to do that!");
                return;
            }

			client.Player.Out.SendToggleHousePoints(house);
		}
	}
}