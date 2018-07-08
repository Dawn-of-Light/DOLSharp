using System;
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
    public class ScoutMission : AbstractMission
    {
        private readonly AbstractGameKeep _keep;

        public ScoutMission(object owner)
            : base(owner)
        {
            eRealm realm = 0;
            if (owner is Group group)
            {
                realm = group.Leader.Realm;
            }
            else if (owner is GamePlayer player)
            {
                realm = player.Realm;
            }

            ArrayList list = new ArrayList();

            ICollection<AbstractGameKeep> keeps;
            if (owner is Group group1)
            {
                keeps = GameServer.KeepManager.GetKeepsOfRegion(group1.Leader.CurrentRegionID);
            }
            else if (owner is GamePlayer gOwner)
            {
                keeps = GameServer.KeepManager.GetKeepsOfRegion(gOwner.CurrentRegionID);
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

                if (keep.Realm != realm)
                {
                    list.Add(keep);
                }
            }

            if (list.Count > 0)
            {
                _keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;
            }

            GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
            GameEventMgr.AddHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (e == AreaEvent.PlayerEnter)
            {
                if (!(args is AreaEventArgs kargs))
                {
                    return;
                }

                if (Owner is GamePlayer && kargs.GameObject != Owner)
                {
                    return;
                }

                foreach (var area1 in kargs.GameObject.CurrentAreas)
                {
                    var area = (AbstractArea) area1;
                    if (area is KeepArea && (area as KeepArea).Keep == _keep)
                    {
                        FinishMission();
                        break;
                    }
                }
            }
            else if (e == KeepEvent.KeepTaken)
            {
                if (!(args is KeepEventArgs kargs))
                {
                    return;
                }

                if (kargs.Keep != _keep)
                {
                    return;
                }

                ExpireMission();
            }
        }

        public override void FinishMission()
        {
            base.FinishMission();
            GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
            GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
        }

        public override void ExpireMission()
        {
            base.ExpireMission();
            GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
            GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
        }

        public override string Description
        {
            get
            {
                if (_keep == null)
                {
                    return "Keep is null when trying to send the description";
                }

                return $"Scout the area around {_keep.Name}";
            }
        }

        public override long RewardRealmPoints => 250;
    }
}