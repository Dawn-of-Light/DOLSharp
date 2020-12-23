/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /// <summary>
    /// LootGeneratorDreadedSeal
    /// Adds Glowing Dreaded Seal to loot
    /// </summary>
    public class DreadedSealCollector : GameNPC
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static List<Tuple<int, int>> m_levelMultipliers = ParseMultipliers();
        protected static Dictionary<string, float> m_BPMultipliers = ParseValues(ServerProperties.Properties.DREADEDSEALS_BP_VALUES);
        protected static Dictionary<string, float> m_RPMultipliers = ParseValues(ServerProperties.Properties.DREADEDSEALS_RP_VALUES);

        /// <summary>
        /// Parse a server property string level multiplier values
        /// </summary>
        /// <param name="serverProperty"></param>
        /// <returns></returns>
        protected static List<Tuple<int, int>> ParseMultipliers()
        {
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();

            foreach (string entry in ServerProperties.Properties.DREADEDSEALS_LEVEL_MULTIPLIER.Split(';'))
            {
                string[] asVal = entry.Split('|');

                if (asVal.Length > 1 && int.TryParse(asVal[0], out int level) && int.TryParse(asVal[1], out int multiplier))
                    list.Add(Tuple.Create(level, multiplier));
            } // foreach

            if (list.Count > 0)
                list.Sort();
            else
                log.Error("ParseMultipliers: Could not parse any level multipliers; DreadedSealCollector disabled.");

            return list;
        }

        /// <summary>
        /// Parse a server property string BP or RP values
        /// </summary>
        /// <param name="serverProperty"></param>
        /// <returns></returns>
        protected static Dictionary<string, float> ParseValues(string serverProperty)
        {
            Dictionary<string, float> dict = new Dictionary<string, float>();

            foreach (string entry in serverProperty.Split(';'))
            {
                string[] asVal = entry.Split('|');

                if (asVal.Length > 1 && float.TryParse(asVal[1], out float value))
                    dict[asVal[0]] = value;
            } // foreach

            return dict;
        }

        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_ChatWindow);
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            string response;

            if (m_levelMultipliers.Count <= 0)
                response = "Sorry, no level multipliers are defined so I cannot accept seals at this time.";
            else if (m_BPMultipliers.Count > 0 && m_RPMultipliers.Count > 0)
                response = "Hand me Dreaded Seals and I'll give you Bounty and Realm points!";
            else if (m_BPMultipliers.Count > 0)
                response = "Hand me Dreaded Seals and I'll give you Bounty points!";
            else if (m_RPMultipliers.Count > 0)
                response = "Hand me Dreaded Seals and I'll give you Realm points!";
            else
                response = "Sorry, no dreaded seal types are defined so I cannot accept seals at this time.";
            player.Out.SendMessage(response, eChatType.CT_Say, eChatLoc.CL_ChatWindow);

            return true;
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (source is GamePlayer player && item != null)
            {
                if (GetDistanceTo(player) > WorldMgr.INTERACT_DISTANCE)
                {
                    ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to me "
                    + player.Name + ". Come a little closer.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    return false;
                }

                if (m_levelMultipliers.Count < 1)
                {
                    ((GamePlayer)source).Out.SendMessage("Sorry, no level multipliers are defined so I cannot accept seals at this time.",
                        eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    return false;
                }

                float bpMultiplier = 0;
                if (m_BPMultipliers.ContainsKey(item.Id_nb))
                    bpMultiplier = m_BPMultipliers[item.Id_nb];

                float rpMultiplier = 0;
                if (m_RPMultipliers.ContainsKey(item.Id_nb))
                    rpMultiplier = m_RPMultipliers[item.Id_nb];

                if (bpMultiplier < 1 && rpMultiplier < 1)
                {
                    ((GamePlayer)source).Out.SendMessage("Sorry, I cannot accept items of that type.",
                        eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    return false;
                }
                else
                {
                    int levelMultiplier = 0;
                    int nextLevel = 0;

                    foreach (Tuple<int, int> tup in m_levelMultipliers)
                    {
                        if (player.Level >= tup.Item1)
                            levelMultiplier = tup.Item2;
                        else
                        {
                            nextLevel = tup.Item1;
                            break;
                        }
                    }

                    if (levelMultiplier <= 0)
                    {
                        ((GamePlayer)source).Out.SendMessage("You are too young yet to make use of these items "
                        + player.Name + ". Come back in " + (nextLevel - player.Level) + " levels.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);

                        return false;
                    }

                    if (bpMultiplier > 0)
                        player.GainBountyPoints((int)(item.Count * levelMultiplier * bpMultiplier));

                    if (rpMultiplier > 0)
                        player.GainRealmPoints((int)(item.Count * levelMultiplier * rpMultiplier));

                    player.Inventory.RemoveItem(item);
                    player.Out.SendUpdatePoints();

                    return true;
                }
            }

            return base.ReceiveItem(source, item);
        }
    }
}
