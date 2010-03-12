using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using DOL.Events;
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.Effects;
using log4net;

namespace DOL.GS
{
    public sealed class MinotaurRelicManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// table of all relics, InternalID as key
        /// </summary>
        public static readonly Dictionary<string, MinotaurRelic> m_minotaurrelics = new Dictionary<string, MinotaurRelic>();

        /// <summary>
        /// Holds the maximum XP of Minotaur Relics
        /// </summary>
        public const double MAX_RELIC_EXP = 3750;
        /// <summary>
        /// Holds the minimum respawntime
        /// </summary>
        public const int MIN_RESPAWN_TIMER =300000;
        /// <summary>
        /// Holds the maximum respawntime
        /// </summary>
        public const int MAX_RESPAWN_TIMER = 1800000;
        /// <summary>
        /// Holds the Value which is removed from the XP per tick
        /// </summary>
        public const double XP_LOSS_PER_TICK = 10;
		
		[ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            if (ServerProperties.Properties.ENABLE_MINOTAUR_RELICS)
            {
                if (log.IsDebugEnabled)
                    log.Debug("Minotaur Relics manager initialized");

                Init();
            }
		}

        /// <summary>
        /// Inits the Minotaurrelics
        /// </summary>
        public static bool Init()
        {
            foreach (MinotaurRelic relic in m_minotaurrelics.Values)
            {
                relic.SaveIntoDatabase();
                relic.RemoveFromWorld();
            }

            m_minotaurrelics.Clear();

            try
            {
                var relics = GameServer.Database.SelectAllObjects<DBMinotaurRelic>();
                foreach (DBMinotaurRelic dbrelic in relics)
                {
                    if (WorldMgr.GetRegion((ushort)dbrelic.SpawnRegion) == null)
                    {
                        log.Warn("DBMinotaurRelic: Could not load " + dbrelic.ObjectId + ": Region missmatch.");
                        continue;
                    }

                    MinotaurRelic relic = new MinotaurRelic(dbrelic);

                    m_minotaurrelics.Add(relic.InternalID, relic);

                    relic.AddToWorld();
                }
                InitMapUpdate();
                log.Info("Minotaur Relics properly loaded");
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error loading Minotaur Relics", e);
                return false;
            }
        }

        static Timer m_mapUpdateTimer;
        public static void InitMapUpdate()
        {
            m_mapUpdateTimer = new Timer(new TimerCallback(MapUpdate), null, 0, 30 * 1000); //30sec Lifeflight change this to 15 seconds
        }
        public static void StopMapUpdate()
        {
            if (m_mapUpdateTimer != null)
                m_mapUpdateTimer.Dispose();
        }
        private static void MapUpdate(object nullValue)
        {
            Dictionary<ushort, IList<MinotaurRelic>> relics = new Dictionary<ushort, IList<MinotaurRelic>>();
            foreach (MinotaurRelic relic in MinotaurRelicManager.GetAllRelics())
            {
                if (!relics.ContainsKey(relic.CurrentRegionID))
                {
                    relics.Add(relic.CurrentRegionID, new List<MinotaurRelic>());
                }
                relics[relic.CurrentRegionID].Add(relic);
            }
            foreach (GameClient clt in WorldMgr.GetAllPlayingClients())
            {
                if (clt == null || clt.Player == null)
                    continue;

                if(relics.ContainsKey(clt.Player.CurrentRegionID))
                {
                    foreach(MinotaurRelic relic in relics[clt.Player.CurrentRegionID])
                    {
                        clt.Player.Out.SendMinotaurRelicMapUpdate((byte)relic.RelicID, relic.CurrentRegionID, relic.X, relic.Y, relic.Z);
                    }
                }
            }
        } 

        #region Helpers
        /// <summary>
        /// Adds a Relic to the Hashtable
        /// </summary>
        /// <param name="relic">The Relic you want to add</param>
        public static bool AddRelic(MinotaurRelic relic)
        {
            if (m_minotaurrelics.ContainsValue(relic)) return false;

            lock (m_minotaurrelics)
            {
                m_minotaurrelics.Add(relic.InternalID, relic);
            }

            return true;
        }

        //Lifeflight: Add
        /// <summary>
        /// Removes a Relic from the Hashtable
        /// </summary>
        /// <param name="relic">The Relic you want to remove</param>
        public static bool RemoveRelic(MinotaurRelic relic)
        {
            if (!m_minotaurrelics.ContainsValue(relic)) return false;

            lock (m_minotaurrelics)
            {
                m_minotaurrelics.Remove(relic.InternalID);
            }

            return true;
        }

        public static int GetRelicCount()
        {
            return m_minotaurrelics.Count;
        }

        public static IList<MinotaurRelic> GetAllRelics()
        {
            IList<MinotaurRelic> relics = new List<MinotaurRelic>();

            lock (m_minotaurrelics)
            {
                foreach (string id in m_minotaurrelics.Keys)
                    relics.Add(m_minotaurrelics[id]);
            }

            return relics;
        }

        /// <summary>
        /// Returns the Relic with the given ID
        /// </summary>
        /// <param name="ID">The Internal ID of the Relic</param>
        public static MinotaurRelic GetRelic(string ID)
        {
            lock (m_minotaurrelics)
            {
                if (!m_minotaurrelics.ContainsKey(ID))
                    return null;

                return m_minotaurrelics[ID] as MinotaurRelic;
            }
        }

        public static MinotaurRelic GetRelic(int ID)
        {
            lock (m_minotaurrelics)
            {
                foreach (MinotaurRelic relic in m_minotaurrelics.Values)
                {
                    if (relic.RelicID == ID)
                        return relic;
                }
            }
            return null;
        }
        #endregion
    }
}
