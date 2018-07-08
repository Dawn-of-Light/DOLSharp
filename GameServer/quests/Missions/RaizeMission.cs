using System;
using System.Collections;
using System.Collections.Generic;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
    public class RaizeMission : AbstractMission
    {
        private readonly AbstractGameKeep _keep;

        public RaizeMission(object owner)
            : base(owner)
        {
            eRealm realm = 0;
            if (owner is Group group1)
            {
                realm = group1.Leader.Realm;
            }
            else if (owner is GamePlayer)
            {
                realm = ((GamePlayer) owner).Realm;
            }

            ArrayList list = new ArrayList();

            ICollection<AbstractGameKeep> keeps;
            if (owner is Group group)
            {
                keeps = GameServer.KeepManager.GetKeepsOfRegion(group.Leader.CurrentRegionID);
            }
            else if (owner is GamePlayer)
            {
                keeps = GameServer.KeepManager.GetKeepsOfRegion(((GamePlayer) owner).CurrentRegionID);
            }
            else
            {
                keeps = new List<AbstractGameKeep>();
            }

            foreach (AbstractGameKeep keep in keeps)
            {
                if (keep.IsPortalKeep)
                {
                    continue;
                }

                if (keep is GameKeepTower && keep.Realm != realm)
                {
                    list.Add(keep);
                }
            }

            if (list.Count > 0)
            {
                _keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;
            }

            GameEventMgr.AddHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (e != KeepEvent.TowerRaized)
            {
                return;
            }

            if (!(args is KeepEventArgs kargs))
            {
                return;
            }

            if (kargs.Keep != _keep)
            {
                return;
            }

            FinishMission();
        }

        public override void FinishMission()
        {
            base.FinishMission();
            GameEventMgr.RemoveHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
        }

        public override void ExpireMission()
        {
            base.ExpireMission();
            GameEventMgr.RemoveHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
        }

        public override string Description
        {
            get
            {
                if (_keep == null)
                {
                    return "Keep is null when trying to send the description";
                }

                return $"Raize {_keep.Name}";
            }
        }
    }
}