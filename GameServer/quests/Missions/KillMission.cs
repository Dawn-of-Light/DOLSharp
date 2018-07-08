using System;

using DOL.Events;

namespace DOL.GS.Quests
{
    public class KillMission : AbstractMission
    {
        private readonly Type _targetType;
        private readonly int _total;
        private readonly string _desc;
        private int _current;

        public KillMission(Type targetType, int total, string desc, object owner)
            : base(owner)
        {
            _targetType = targetType;
            _total = total;
            _desc = desc;
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (e != GameLivingEvent.EnemyKilled)
            {
                return;
            }

            if (!(args is EnemyKilledEventArgs eargs))
            {
                return;
            }

            // we don't want mission masters to be considered realm guards because they are insta respawn
            // in addition do not count realm 0 guards
            if (eargs.Target is Keeps.MissionMaster || eargs.Target.Realm == eRealm.None)
            {
                return;
            }

            if (_targetType.IsInstanceOfType(eargs.Target) == false)
            {
                return;
            }

            // we dont allow events triggered by non group leaders
            if (MissionType == eMissionType.Group && sender is GamePlayer)
            {
                GamePlayer player = (GamePlayer) sender;

                if (player.Group == null)
                {
                    return;
                }

                if (player.Group.Leader != player)
                {
                    return;
                }
            }

            // we don't want group events to trigger personal mission updates
            if (MissionType == eMissionType.Personal && sender is GamePlayer)
            {
                GamePlayer player = (GamePlayer) sender;

                if (player.Group != null)
                {
                    return;
                }
            }

            _current++;
            UpdateMission();
            if (_current == _total)
            {
                FinishMission();
            }
        }

        public override string Description => $"Kill {_total} {_desc}, you have killed {_current}.";

        public override long RewardRealmPoints
        {
            get
            {
                if (_targetType == typeof(Keeps.GameKeepGuard))
                {
                    return 500;
                }

                if (_targetType == typeof(GamePlayer))
                {
                    return 750;
                }

                return 0;
            }
        }
    }
}