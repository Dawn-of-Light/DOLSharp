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
    public class StatsCommandHandler : ICommandHandler
    {
        public int OnCommand(GameClient client, string[] args)
        {
            GamePlayer player = client.Player;

            player.Out.SendMessage(PlayerStatistic.GetStatsMessage(player), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            
            return 1;
        }


    }
}