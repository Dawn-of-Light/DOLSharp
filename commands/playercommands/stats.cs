using System;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Commands;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&stats",
        ePrivLevel.Player,
        "Displays player statistics",//TODO correct message
        "/stats")]
    public class StatsCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
			DisplayMessage(client, PlayerStatistic.GetStatsMessage(client.Player));
        }
    }
}