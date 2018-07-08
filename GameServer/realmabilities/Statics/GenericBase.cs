using System;
using DOL.Events;

namespace DOL.GS.RealmAbilities.Statics
{
    public abstract class GenericBase : GameStaticItem
    {
        protected abstract string GetStaticName();

        protected abstract ushort GetStaticModel();

        protected abstract ushort GetStaticEffect();

        private uint _lifeTime;
        private ushort _radius;
        private uint _pulseFrequency;
        private int _currentTick;

        protected GamePlayer Caster { get; private set; }
        protected int CurrentPulse { get; private set; }

        public void CreateStatic(GamePlayer caster, Point3D gt, uint lifeTime, uint pulseFrequency, ushort radius)
        {
            _lifeTime = lifeTime;
            Caster = caster;
            _radius = radius;
            _pulseFrequency = pulseFrequency;
            Name = GetStaticName();
            Model = GetStaticModel();
            X = gt.X;
            Y = gt.Y;
            Z = gt.Z;
            CurrentRegionID = Caster.CurrentRegionID;
            Level = caster.Level;
            Realm = caster.Realm;
            AddToWorld();
        }

        public override bool AddToWorld()
        {
            new RegionTimer(this, new RegionTimerCallback(PulseTimer),1000);
            GameEventMgr.AddHandler(Caster, GameObjectEvent.RemoveFromWorld, new DOLEventHandler(PlayerLeftWorld));
            return base.AddToWorld();
        }

        protected virtual int PulseTimer(RegionTimer timer)
        {
            if (_currentTick >= _lifeTime || Caster == null)
            {
                RemoveFromWorld();
                timer.Stop();
                return 0;
            }

            if (_currentTick % _pulseFrequency == 0) {
                CurrentPulse++;
                foreach (GamePlayer target in GetPlayersInRadius(_radius))
                {
                    CastSpell(target);
                }

                foreach (GameNPC npc in GetNPCsInRadius(_radius))
                {
                    CastSpell(npc);
                }
            }

            _currentTick++;
            return 1000;
        }

        protected virtual void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (Caster == player)
            {
                _currentTick = (int)_lifeTime;
            }
        }

        protected abstract void CastSpell(GameLiving target);
    }
}

