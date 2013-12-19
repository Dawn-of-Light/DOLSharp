using System;
using System.Collections;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

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
		int m_delay = 0;
		protected int getCurrentPulse ()
		{return currentPulse;}
		
		public void CreateStatic(GamePlayer caster, Point3D gt, uint lifeTime, uint pulseFrequency, ushort radius, int delay) 
        {
			m_lifeTime = lifeTime;
			m_caster = caster;
			m_radius = radius;
			m_pulseFrequency = pulseFrequency;
			m_delay = delay;
			this.Name = GetStaticName();
			this.Model = GetStaticModel();
			this.X = gt.X;
			this.Y = gt.Y;
			this.Z = gt.Z;
			this.CurrentRegionID = m_caster.CurrentRegionID;
			this.Level = caster.Level;
            this.Realm = caster.Realm;
            this.AddToWorld();
		}
		public override bool AddToWorld() 
        {
			new RegionTimer(this, new RegionTimerCallback(PulseTimer), m_delay);
			GameEventMgr.AddHandler(m_caster, GamePlayerEvent.RemoveFromWorld, new DOLEventHandler(PlayerLeftWorld));
			return base.AddToWorld();
		}
		protected virtual int PulseTimer(RegionTimer timer)
        {
			if (currentTick >= m_lifeTime || m_caster == null) 
            {
				this.RemoveFromWorld();
				timer.Stop();
				timer=null;
				return 0;
			}
			if (currentTick%m_pulseFrequency==0){
				currentPulse++;
				foreach(GamePlayer target in this.GetPlayersInRadius(m_radius)) 
                {
					CastSpell(target);
					if (this is StaticTempestBase)
					{
						GameSpellAndImmunityEffect stun = SpellHandler.FindImmunityEffectOnTarget(target, typeof(StunSpellHandler));
						if (stun != null)
							stun.Remove();
					}
				}
				foreach (GameNPC npc in this.GetNPCsInRadius(m_radius))	
                {
					CastSpell(npc);
					if (this is StaticTempestBase)
					{
						GameSpellAndImmunityEffect stun = SpellHandler.FindImmunityEffectOnTarget(npc, typeof(StunSpellHandler));
						if (stun != null)
							stun.Remove();
					}
				}
			}
			
			currentTick++;
			return 1000;
		}
		protected virtual void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if (this.m_caster == player)
            {
				currentTick = (int)m_lifeTime;
			}
		}
		protected abstract void CastSpell (GameLiving target);		
	}
}

