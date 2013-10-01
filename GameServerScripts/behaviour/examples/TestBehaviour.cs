using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Actions;
using DOL.GS.Behaviour.Triggers;
using log4net;
using System.Reflection;
using DOL.Events;
using DOL.GS;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Behaviour.Examples
{
    class TestBehaviour
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // list of all registered behaviours
        private static List<BaseBehaviour> behaviours = new List<BaseBehaviour>();

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_EXAMPLES)
                return;

            #region defineNPCs

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Quait", eRealm.Albion);
            npcs = WorldMgr.GetNPCsByName("Sir Quait", (eRealm)1);
            GameNPC SirQuait = null;
            if (npcs.Length == 0)
            {
                SirQuait = new DOL.GS.GameNPC();
                SirQuait.Model = 40;
                SirQuait.Name = "Sir Quait";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + SirQuait.Name + ", creating ...");                
                SirQuait.Realm = eRealm.Albion;
                SirQuait.CurrentRegionID = 1;
                SirQuait.Size = 50;
                SirQuait.Level = 10;
                SirQuait.MaxSpeedBase = 100;
                SirQuait.Faction = FactionMgr.GetFactionByID(0);
                SirQuait.X = 531971;
                SirQuait.Y = 478955;
                SirQuait.Z = 0;
                SirQuait.Heading = 3570;
                SirQuait.RespawnInterval = 0;
                SirQuait.BodyType = 0;


                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 0;
                SirQuait.SetOwnBrain(brain);
                
                SirQuait.AddToWorld();
            }
            else
            {
                SirQuait = npcs[0];
            }

            #endregion

            #region defineBehaviours

            BaseBehaviour b = new BaseBehaviour(SirQuait);
            MessageAction a = new MessageAction(SirQuait,"This is just a simple test bahaviour.",eTextType.Emote);
            b.AddAction(a);
            InteractTrigger t = new InteractTrigger(SirQuait,b.NotifyHandler,SirQuait);
            b.AddTrigger(t);

            // store the behaviour in a list so it won't be garbage collected
            behaviours.Add(b);

            #endregion

            log.Info("Simple Test Behaviour added");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            behaviours.Clear();
        }
    }
}
