using System;
using DOL.Events;

namespace DOL.GS.RealmAbilities.Statics
{
    public abstract class GenericBase : GameStaticItem 
    {
		protected abstract string GetStaticName();
		protected abstract ushort GetStaticModel();
		protected abstract ushort GetStaticEffect();
		protected GamePlayer m_caster;
		protected uint m_lifeTime;
		protected ushort m_radius;
		protected uint m_pulseFrequency;
		int currentTick = 0;
		int currentPulse = 0;
		protected int getCurrentPulse ()
		{return currentPulse;}
		
		public void CreateStatic(GamePlayer caster, Point3D gt, uint lifeTime, uint pulseFrequency, ushort radius) 
        {
			m_lifeTime = lifeTime;
			m_caster = caster;
			m_radius = radius;
			m_pulseFrequency = pulseFrequency;
            Name = GetStaticName();
            Model = GetStaticModel();
            X = gt.X;
            Y = gt.Y;
            Z = gt.Z;
            CurrentRegionID = m_caster.CurrentRegionID;
            Level = caster.Level;
            Realm = caster.Realm;
            AddToWorld();
		}
		public override bool AddToWorld() 
        {
			new RegionTimer(this, new RegionTimerCallback(PulseTimer),1000);
			GameEventMgr.AddHandler(m_caster, GamePlayerEvent.RemoveFromWorld, new DOLEventHandler(PlayerLeftWorld));
			return base.AddToWorld();
		}
		protected virtual int PulseTimer(RegionTimer timer)
        {
			if (currentTick >= m_lifeTime || m_caster == null) 
            {
                RemoveFromWorld();
				timer.Stop();
				timer=null;
				return 0;
			}
			if (currentTick%m_pulseFrequency==0){
				currentPulse++;
				foreach(GamePlayer target in GetPlayersInRadius(m_radius)) 
                {
					CastSpell(target);
				}
				foreach (GameNPC npc in GetNPCsInRadius(m_radius))	
                {
					CastSpell(npc);
				}
			}
			
			currentTick++;
			return 1000;
		}
		protected virtual void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if (m_caster == player)
            {
				currentTick = (int)m_lifeTime;
			}
		}
		protected abstract void CastSpell (GameLiving target);		
	}
}

