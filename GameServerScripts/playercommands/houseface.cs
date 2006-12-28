using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&houseface",
	  (int)ePrivLevel.Player,
	  "Points to the specified guildhouse of the guild noted, or the lot number noted in the command. /houseface alone will point to one's personal home.")]
	public class HouseFaceCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			int housenumber = 0;
			if (args.Length > 1)
			{
				try
				{
					housenumber = int.Parse(args[1]);
				}
				catch
				{
					DisplaySyntax(client);
					return 0;
				}
			}
			else HouseMgr.GetHouseNumberByPlayer(client.Player);

			if (housenumber == 0)
			{
				DisplayError(client, "No house found.", new object[] { });
				return 0;
			}

			House house = HouseMgr.GetHouse(housenumber);

			ushort direction = client.Player.GetHeadingToSpot(house.X, house.Y);
			client.Player.Heading = direction;
			client.Out.SendPlayerJump(true);
			return 0;
		}
	}
}