using System;

namespace DOL.GS
{
    public interface IPlayerStatistics
    {
        /// <summary>
        /// Return a string formatted for the system display
        /// </summary>
        /// <returns></returns>
        string GetStatisticsMessage();

        /// <summary>
        /// Display server wide statistics to the client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command"></param>
        void DisplayServerStatistics(GameClient client, string command, string playerName);
    }
}
