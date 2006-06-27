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
using System.Collections;
using System.Text;
using DOL.GS;
using DOL.GS.SpawnGenerators;
using DOL.Events;
using DOL.AI.Brain;
using DOL.AI;
using log4net;
using System.Reflection;

namespace DOL.GS.SpawnGenerators
{
    public class StandardMobSpawner : IMobSpawner
    {
        #region variables
        private INpcTemplate m_template;
        private ISpawnGenerator m_spawnGenerator;
        private int m_minLevel = 0;
        private int m_maxLevel;
        private int m_minCount = 0;
        private int m_maxCount;
        private int m_minTime = 0;
        private int m_maxTime = 24;
        private int m_respawnTime;
        private int m_chance = 100;
        protected Type m_brain = null;
        protected ArrayList m_mobs = new ArrayList(1);

        /// <summary>
        /// A timer that will show/hide mobs
        /// </summary>
        private TimeTimer m_timeTimer;

        /// <summary>
        /// A timer that will respawn mobs
        /// </summary>
        private RespawnTimer m_respawnTimer;

        /// <summary>
        /// The sync object for respawn timer modifications
        /// </summary>
        private readonly object m_respawnTimerLock = new object();

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        #region Properties
        public int RespawnTime
        {
            get
            {
                if (m_respawnTime >= 0)
                    return m_respawnTime;

                if (((m_maxLevel + m_minLevel) >> 1) <= 65)
                {
                    return Util.Random(5 * 60000) + 3 * 60000;
                }
                else
                {
                    int minutes = ((m_maxLevel + m_minLevel) >> 1) - 65 + 15;// /2
                    return minutes * 60000;
                }
            }
        }
        public ArrayList Mobs
        {
            get { return m_mobs; }
        
        }
        public ISpawnGenerator SpawnGenerator
        {
            get { return m_spawnGenerator; }
            set { m_spawnGenerator = value; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:StandardMobSpawner"/> class.
        /// </summary>
        public StandardMobSpawner()
        {
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < m_minCount; i++)
            {
                CreateMob();
            }
            StartRespawn();
            if ((m_minTime != 0) || (m_maxTime != 24))
            {
                m_timeTimer = new TimeTimer(this);
                m_timeTimer.Start(m_minTime * 60 * 54 * 1000);//the time start at 0 on server
            }
        }

        /// <summary>
        /// Mods the dying.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="arguments">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private void ModDying(DOLEvent e, object sender, EventArgs arguments)
        {
            GameMob mob = sender as GameMob;
            m_mobs.Remove(mob);
            GameEventMgr.RemoveHandler(mob, GameLivingEvent.Dying, new DOLEventHandler(ModDying));
            if (m_mobs.Count < m_maxCount)
                StartRespawn();
        }

        /// <summary>
        /// Create mob in area
        /// </summary>
        public virtual void CreateMob()
        {
            GameMob mob = new GameMob(m_template);
            mob.Region = m_spawnGenerator.Area.Region;
            Point point3D = m_spawnGenerator.Area.GetRandomLocation();
            mob.Position = point3D;
            mob.Level = (byte)Util.Random(m_minLevel, m_maxLevel);
            mob.Heading = (ushort)Util.Random(4095);//max heading make it const
            mob.RespawnInterval = 0;//delete mob when dying but not respawn
            mob.AddToWorld();
            if (m_brain != null)
            {
                ABrain mybrain = Activator.CreateInstance(m_brain) as ABrain;
                mob.SetOwnBrain(mybrain);
            }
            GameEventMgr.AddHandler(mob, GameLivingEvent.Dying, new DOLEventHandler(ModDying));
            //not save in DB
            m_mobs.Add(mob);
        }

        //TODO : make respawn when player come near and not auto when die
        #region respawn

        /// <summary>
        /// Starts the respawn.
        /// </summary>
        public void StartRespawn()
        {
            int respawnInt = RespawnTime;
            if (respawnInt > 0)
            {
                lock (m_respawnTimerLock)
                {
                    if (m_respawnTimer == null)
                    {
                        m_respawnTimer = new RespawnTimer(this);
                    }
                    else if (m_respawnTimer.IsAlive)
                    {
                        m_respawnTimer.Stop();
                    }
                    m_respawnTimer.Start(respawnInt);
                }
            }
        }

        /// <summary>
        /// Stops the respawn.
        /// </summary>
        public void StopRespawn()
        {
            lock (m_respawnTimerLock)
            {
                if (m_respawnTimer == null)
                {
                    return;
                }
                else if (m_respawnTimer.IsAlive)
                {
                    m_respawnTimer.Stop();
                }
            }
        }

        /// <summary>
        /// the respawn timer which tick to make respawn mob
        /// </summary>
        private class RespawnTimer : GameTimer
        {

            private StandardMobSpawner m_template;
            /// <summary>
            /// Constructs a new respawn timer
            /// </summary>
            /// <param name="timerOwner">The game object that is starting the timer</param>
            public RespawnTimer(StandardMobSpawner template)
                : base(template.SpawnGenerator.Area.Region.TimeManager)
            {
                if (template == null)
                    throw new ArgumentNullException("template");
                m_template = template;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                if (m_template.m_mobs.Count >= m_template.m_maxCount)
                {
                    Stop();
                    return;
                }
                this.Interval = m_template.RespawnTime;
                if (m_template.m_chance == 100 || Util.Chance(m_template.m_chance))
                    m_template.CreateMob();
            }
        }
        #endregion

        #region params

        /// <summary>
        /// parse parameters of db at start to load all in property
        /// </summary>
        /// <param name="strparams"></param>
        public void ParseParams(string strparams)
        {
            string[] strparam = strparams.Split(',');
            foreach (string str in strparam)
            {
                if (str == null || str == "")
                    continue;
                LoadParam(str);
            }
        }

        /// <summary>
        /// Loads each param.
        /// </summary>
        /// <param name="str">The STR.</param>
        private void LoadParam(string str)
        {
            str = str.Replace(" ", "");//remove space
            string[] param = str.Split('=');
            switch (param[0])
            {
                case "template":
                    {
                        try
                        {
                            int templateid = Convert.ToInt32(param[1]);
                            m_template = NpcTemplateMgr.GetTemplate(templateid);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get template.");
                        }
                    } break;
                case "lvlrange":
                case "levelrange":
                    {
                        try
                        {
                            string[] levelparam = param[1].Split('-');
                            m_minLevel = Convert.ToInt32(levelparam[0]);
                            m_maxLevel = Convert.ToInt32(levelparam[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get level range.");
                        }
                    } break;
                case "minlvl":
                case "minlevel":
                    {
                        try
                        {
                            m_minLevel = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get min level.");
                        }
                    } break;
                case "maxlvl":
                case "maxlevel":
                    {
                        try
                        {
                            m_maxLevel = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get max level.");
                        }
                    } break;
                case "min":
                    {
                        try
                        {
                            m_minCount = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get min.");
                        }
                    } break;
                case "max":
                    {
                        try
                        {
                            m_maxCount = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get max.");
                        }
                    } break;
                case "time":
                    {
                        try
                        {
                            string[] timeparam = param[1].Split('-');
                            m_minTime = Convert.ToInt32(timeparam[0]);
                            m_maxTime = Convert.ToInt32(timeparam[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get time.");
                        }
                    } break;
                case "respawntime":
                    {
                        try
                        {
                            m_respawnTime = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get respawntime.");
                        }
                    } break;
                case "chance":
                    {
                        try
                        {
                            m_chance = Convert.ToInt32(param[1]);
                            if (m_chance > 100 || m_chance < 0)
                                throw new OverflowException("chance or respawn in spawn area must be between 0 and 100");
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get chance.");
                        }
                    } break;
                case "brain":
                    {
                        try
                        {
                            m_brain = Scripts.ScriptMgr.GetType(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not get chance.");
                        }
                    } break;
                default:
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Could not recognize param :" + param[0] + ".");
                    } break;
            }
        }

        #endregion

        #region Time

        private bool m_visible = true;

        private class TimeTimer : GameTimer
        {
            private StandardMobSpawner m_template;
            /// <summary>
            /// Constructs a new respawn timer
            /// </summary>
            /// <param name="timerOwner">The game object that is starting the timer</param>
            public TimeTimer(StandardMobSpawner template)
                : base(template.SpawnGenerator.Area.Region.TimeManager)
            {
                if (template == null)
                    throw new ArgumentNullException("template");
                m_template = template;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                if (m_template.m_mobs.Count >= m_template.m_maxCount)
                {
                    Stop();
                    return;
                }
                //if (m_template.maxTime < WorldMgr.GetCurrentDayTime()/1000/60/54)//special mythic minute = 54 secondes
                if (m_template.m_visible)//toggle
                {
                    this.Interval = ((m_template.m_maxTime - m_template.m_minTime) % 24) * 60 * 54 * 1000;
                    m_template.Hide();
                    m_template.m_visible = false;
                }
                else
                {
                    this.Interval = ((m_template.m_minTime - m_template.m_maxTime) % 24) * 60 * 54 * 1000;
                    m_template.Show();
                    m_template.m_visible = true;
                }
            }
        }

        /// <summary>
        /// Hides the mobs.
        /// </summary>
        public void Hide()
        {
            foreach (GameMob mob in m_mobs)
                mob.RemoveFromWorld();
        }

        /// <summary>
        /// Shows the mobs.
        /// </summary>
        public void Show()
        {
            foreach (GameMob mob in m_mobs)
                mob.AddToWorld();
        }
        #endregion
    }

}
