using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using DOL.GS.Geometry;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// Path calculator component that can be added to any NPC to calculate paths
    /// @author mlinder
    /// </summary>
    public sealed class PathCalculator
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Minimum distance that a target has to be away before we start plotting paths instead of directly
        /// walking towards the target. Includes Z.
        /// </summary>
        public const int MIN_PATHING_DISTANCE = 80;

        /// <summary>
        /// Minimum distance difference required before a path is being replotted
        /// </summary>
        public const int MIN_TARGET_DIFF_REPLOT_DISTANCE = 80;

        /// <summary>
        /// Distance at which we consider a pathing node reached
        /// </summary>
        public const int NODE_REACHED_DISTANCE = 24;

        /// <summary>
        /// Distance to search for doors when computing NextDoor.
        /// </summary>
        private const int DOOR_SEARCH_DISTANCE = 512;

        /// <summary>
        /// True if this calculator can be used for the specified NPC.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsSupported(GameNPC o)
        {
            return o?.CurrentZone != null && o.CurrentZone.IsPathingEnabled;
        }

        /// <summary>
        /// Owner to which this calculator belongs to. Used for calculating position offsets
        /// </summary>
        public GameNPC Owner { get; private set; }

        /// <summary>
        /// If set, contains the next door on the NPCs path
        /// </summary>
        public GameDoor NextDoor { get; private set; }

        private LinePath path = new LinePath();
        private Coordinate _lastTarget = Coordinate.Nowhere;

        /// <summary>
        /// Forces the path to be replot on the next CalculateNextTarget(...)
        /// </summary>
        public bool ForceReplot { get; set; }

        /// <summary>
        /// True if a path to the target was plotted. False if resorting back to air/direct route
        /// </summary>
        public bool DidFindPath { get; private set; }

        /// <summary>
        /// Creates a path calculator for the given NPC
        /// </summary>
        /// <param name="owner"></param>
        public PathCalculator(GameNPC owner)
        {
            ForceReplot = true;
            Owner = owner;
        }

        /// <summary>
        /// True if we should path towards the target point
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool ShouldPath(Coordinate target)
        {
            return ShouldPath(Owner, target);
        }

        /// <summary>
        /// True if we should path towards the target point
        /// </summary>
        public static bool ShouldPath(GameNPC owner, Coordinate destination)
        {
            if (owner.Coordinate.DistanceTo(destination) < MIN_PATHING_DISTANCE)
                return false; // too close to path
            if (owner.IsFlying)
                return false;
            if (owner.Position.Z <= 0)
                return false; // this will probably result in some really awkward paths otherwise
            var zone = owner.CurrentZone;
            if (zone == null || !zone.IsPathingEnabled)
                return false; // we're in nirvana
            if (owner.CurrentRegion.GetZone(destination) != zone)
                return false; // target is in a different zone (TODO: implement this maybe? not sure if really required)
            return true;
        }

        //private readonly Meter noPathFoundMetric = Metric.Meter("NoPathFound", Unit.Calls);

        /// <summary>
        /// Semaphore to prevent multiple replots
        /// </summary>
        private int isReplottingPath = IDLE;
        const int IDLE = 0, REPLOTTING = 1;

        private void ReplotPath(Coordinate destination)
        {
            // Try acquiring a pathing lock
            if (Interlocked.CompareExchange(ref isReplottingPath, REPLOTTING, IDLE) != IDLE)
            {
                // Computation is already in progress. ReplotPathAsync will be called again automatically by .PathTo every few ms
                return;
            }

            // we make a blocking call here because we are already in a worker thread and inside a lock
            try
            {
                var currentZone = Owner.CurrentZone;
                var pathingResult = PathingMgr.Instance.GetPathStraightAsync(currentZone, Owner.Coordinate, destination);

                if (pathingResult.Error != PathingError.NoPathFound && pathingResult.Error != PathingError.NavmeshUnavailable &&
                    !pathingResult.Path.Start.Equals(Coordinate.Nowhere))
                {
                    path = pathingResult.Path;
                }
                _lastTarget = destination;
                ForceReplot = false;
            }
            finally
            {
                if (Interlocked.Exchange(ref isReplottingPath, IDLE) != REPLOTTING)
                {
                    log.Warn("PathCalc semaphore was in IDLE state even though we were replotting. This should never happen");
                }
            }
        }

        public Coordinate CalculateNextLineSegment(Coordinate destination)
        {
            if (!ShouldPath(destination))
            {
                return Coordinate.Nowhere;
            }

            // Check if we can reuse our path. We assume that we ourselves never "suddenly" warp to a completely
            // different position.
            if (ForceReplot || _lastTarget.DistanceTo(destination) > MIN_TARGET_DIFF_REPLOT_DISTANCE)
            {
                ReplotPath(destination);
            }

            while (path.PointCount > 0 && Owner.Coordinate.DistanceTo(path.CurrentWayPoint) <= NODE_REACHED_DISTANCE)
            {
                path.SelectNextWayPoint();
            }

            // Find the next node in the path to the target, but skip points that are too close
            var nextWayPoint = path.CurrentWayPoint;

            if (path.PointCount == 0) return Coordinate.Nowhere; // no more nodes (or no path)

            return nextWayPoint;
        }

        public override string ToString()
            => $"PathCalc[Target={_lastTarget}, Nodes={path.PointCount}, NextNode={(path.PointCount > 0 ? path.CurrentWayPoint.ToString() : null)}, NextDoor={NextDoor}]";
    }
}
