using System;
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
    public class CaptureMission : AbstractMission
    {
        private readonly AbstractGameKeep _keep;

        public enum eCaptureType
        {
            Tower = 1,
            Keep = 2,
        }

        public CaptureMission(eCaptureType type, object owner, string hint)
            : base(owner)
        {
            eRealm realm = eRealm.None;
            if (owner is Group group)
            {
                realm = group.Leader.Realm;
            }
            else if (owner is GamePlayer)
            {
                realm = ((GamePlayer) owner).Realm;
            }

            ArrayList list = new ArrayList();

            switch (type)
            {
                case eCaptureType.Tower:
                    {
                        ICollection<AbstractGameKeep> keeps;
                        if (owner is Group group1)
                        {
                            keeps = GameServer.KeepManager.GetKeepsOfRegion(group1.Leader.CurrentRegionID);
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

                        break;
                    }

                case eCaptureType.Keep:
                    {
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

                            if (keep is GameKeep && keep.Realm != realm)
                            {
                                list.Add(keep);
                            }
                        }

                        break;
                    }
            }

            if (list.Count > 0)
            {
                if (hint != string.Empty)
                {
                    foreach (AbstractGameKeep keep in list)
                    {
                        if (keep.Name.ToLower().Contains(hint))
                        {
                            _keep = keep;
                            break;
                        }
                    }
                }

                if (_keep == null)
                {
                    _keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;
                }
            }

            GameEventMgr.AddHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (e != KeepEvent.KeepTaken)
            {
                return;
            }

            if (args is KeepEventArgs kargs && kargs.Keep != _keep)
            {
                return;
            }

            GamePlayer testPlayer = null;
            if (Owner is GamePlayer player)
            {
                testPlayer = player;
            }
            else if (Owner is Group gOwner)
            {
                testPlayer = gOwner.Leader;
            }

            if (testPlayer != null)
            {
                foreach (var area1 in testPlayer.CurrentAreas)
                {
                    var area = (AbstractArea) area1;
                    if (area is KeepArea && (area as KeepArea).Keep == _keep)
                    {
                        FinishMission();
                    }
                }
            }

            ExpireMission();
        }

        public override void FinishMission()
        {
            base.FinishMission();
            GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
        }

        public override void ExpireMission()
        {
            base.ExpireMission();
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

                return $"Capture {_keep.Name}";
            }
        }

        public override long RewardRealmPoints
        {
            get
            {
                if (_keep is GameKeep)
                {
                    return 1500;
                }

                if (_keep is GameKeepTower)
                {
                    return 250 + _keep.Level * 50;
                }

                return 0;
            }
        }
    }
}