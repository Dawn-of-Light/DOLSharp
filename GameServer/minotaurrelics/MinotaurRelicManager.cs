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
using System.Reflection;
using System.Threading;
using DOL.Events;
using DOL.Database;
using log4net;

namespace DOL.GS
{
    public sealed class MinotaurRelicManager
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// table of all relics, InternalID as key
        /// </summary>
        public static readonly Dictionary<string, MinotaurRelic> Minotaurrelics = new Dictionary<string, MinotaurRelic>();

        /// <summary>
        /// Holds the maximum XP of Minotaur Relics
        /// </summary>
        public const double MaxRelicExp = 3750;
        /// <summary>
        /// Holds the minimum respawntime
        /// </summary>
        public const int MinRespawnTimer = 300000;
        /// <summary>
        /// Holds the maximum respawntime
        /// </summary>
        public const int MaxRespawnTimer = 1800000;
        /// <summary>
        /// Holds the Value which is removed from the XP per tick
        /// </summary>
        public const double XpLossPerTick = 10;

        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            if (ServerProperties.Properties.ENABLE_MINOTAUR_RELICS)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Minotaur Relics manager initialized");
                }

                Init();
            }
        }

        /// <summary>
        /// Inits the Minotaurrelics
        /// </summary>
        public static bool Init()
        {
            foreach (MinotaurRelic relic in Minotaurrelics.Values)
            {
                relic.SaveIntoDatabase();
                relic.RemoveFromWorld();
            }

            Minotaurrelics.Clear();

            try
            {
                var relics = GameServer.Database.SelectAllObjects<DBMinotaurRelic>();
                foreach (DBMinotaurRelic dbrelic in relics)
                {
                    if (WorldMgr.GetRegion((ushort)dbrelic.SpawnRegion) == null)
                    {
                        Log.Warn($"DBMinotaurRelic: Could not load {dbrelic.ObjectId}: Region missmatch.");
                        continue;
                    }

                    MinotaurRelic relic = new MinotaurRelic(dbrelic);

                    Minotaurrelics.Add(relic.InternalID, relic);

                    relic.AddToWorld();
                }

                InitMapUpdate();
                Log.Info("Minotaur Relics properly loaded");
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error loading Minotaur Relics", e);
                return false;
            }
        }

        private static Timer _mapUpdateTimer;

        public static void InitMapUpdate()
        {
            _mapUpdateTimer = new Timer(MapUpdate, null, 0, 30 * 1000); // 30sec Lifeflight change this to 15 seconds
        }

        public static void StopMapUpdate()
        {
            _mapUpdateTimer?.Dispose();
        }

        private static void MapUpdate(object nullValue)
        {
            Dictionary<ushort, IList<MinotaurRelic>> relics = new Dictionary<ushort, IList<MinotaurRelic>>();
            foreach (MinotaurRelic relic in GetAllRelics())
            {
                if (!relics.ContainsKey(relic.CurrentRegionID))
                {
                    relics.Add(relic.CurrentRegionID, new List<MinotaurRelic>());
                }

                relics[relic.CurrentRegionID].Add(relic);
            }

            foreach (GameClient clt in WorldMgr.GetAllPlayingClients())
            {
                if (clt?.Player == null)
                {
                    continue;
                }

                if (relics.ContainsKey(clt.Player.CurrentRegionID))
                {
                    foreach (MinotaurRelic relic in relics[clt.Player.CurrentRegionID])
                    {
                        clt.Player.Out.SendMinotaurRelicMapUpdate((byte)relic.RelicId, relic.CurrentRegionID, relic.X, relic.Y, relic.Z);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a Relic to the Hashtable
        /// </summary>
        /// <param name="relic">The Relic you want to add</param>
        public static bool AddRelic(MinotaurRelic relic)
        {
            if (Minotaurrelics.ContainsValue(relic))
            {
                return false;
            }

            lock (Minotaurrelics)
            {
                Minotaurrelics.Add(relic.InternalID, relic);
            }

            return true;
        }

        // Lifeflight: Add
        /// <summary>
        /// Removes a Relic from the Hashtable
        /// </summary>
        /// <param name="relic">The Relic you want to remove</param>
        public static bool RemoveRelic(MinotaurRelic relic)
        {
            if (!Minotaurrelics.ContainsValue(relic))
            {
                return false;
            }

            lock (Minotaurrelics)
            {
                Minotaurrelics.Remove(relic.InternalID);
            }

            return true;
        }

        public static int GetRelicCount()
        {
            return Minotaurrelics.Count;
        }

        public static IList<MinotaurRelic> GetAllRelics()
        {
            IList<MinotaurRelic> relics = new List<MinotaurRelic>();

            lock (Minotaurrelics)
            {
                foreach (string id in Minotaurrelics.Keys)
                {
                    relics.Add(Minotaurrelics[id]);
                }
            }

            return relics;
        }

        /// <summary>
        /// Returns the Relic with the given ID
        /// </summary>
        /// <param name="id">The Internal ID of the Relic</param>
        public static MinotaurRelic GetRelic(string id)
        {
            lock (Minotaurrelics)
            {
                if (!Minotaurrelics.ContainsKey(id))
                {
                    return null;
                }

                return Minotaurrelics[id];
            }
        }

        public static MinotaurRelic GetRelic(int id)
        {
            lock (Minotaurrelics)
            {
                foreach (MinotaurRelic relic in Minotaurrelics.Values)
                {
                    if (relic.RelicId == id)
                    {
                        return relic;
                    }
                }
            }

            return null;
        }
    }
}
