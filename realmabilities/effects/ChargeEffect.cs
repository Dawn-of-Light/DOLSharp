using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Events;

namespace DOL.GS.Effects
{

	public class ChargeEffect : IGameEffect
	{
        protected const String delveString = "Grants unbreakable speed 3 for 15 second duration. Grants immunity to roots, stun, snare and mesmerize spells. Target will still take damage from snare/root spells that do damage.";
        GamePlayer m_player;
		protected long m_startTick;		
		protected RegionTimer m_expireTimer;
        private UInt16 m_id;

		public void Start(GamePlayer player)
		{
            m_player = player;
            m_player.Out.SendMessage("You begin to charge wildly!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            

			m_startTick = player.CurrentRegion.Time;
            foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
					t_player.Out.SendSpellEffectAnimation(player, player, 7035, 0, false, 1);
            }
			
            //sets player into combat mode
            player.LastAttackTick = player.CurrentRegion.Time;
            ArrayList speedSpells = new ArrayList();
            foreach (GameSpellEffect effect in player.EffectList) 
            {
            
                if (effect.Spell.SpellType == "SpeedEnhancement")
                    speedSpells.Add(effect);                    
            }
            foreach (GameSpellEffect spell in speedSpells)
                spell.Cancel(false);
            m_player.MaxSpeedBase = 174+191;
            m_player.Out.SendUpdateMaxSpeed();
			m_player.TempProperties.setProperty("Charging", true);
			StartTimers();
            m_player.EffectList.Add(this);
		}
		
		public void Cancel(bool playerCancel) 
        {
			m_player.TempProperties.removeProperty("Charging");
			m_player.EffectList.Remove(this);
			m_player.MaxSpeedBase = 191;
			m_player.Out.SendUpdateMaxSpeed();
            m_player.Out.SendMessage("You no longer seem so crazy!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			StopTimers();
		}

		protected virtual void StartTimers()
		{
			StopTimers();
            m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), RealmAbilities.ChargeAbility.DURATION * 1000);
		}

		
		// Stops the timers for this effect
		protected virtual void StopTimers()
		{			
			if(m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}

		
		// The callback method when the effect expires
		protected virtual int ExpiredCallback(RegionTimer timer)
		{						
			Cancel(false);
			return 0;
		}		
	
		
		// Name of the effect
		public string Name { get { return "Charge"; } }


        /// <summary>
        /// Remaining time of the effect in milliseconds
        /// </summary>
        public Int32 RemainingTime
        {
            get
            {
                RegionTimer timer = m_expireTimer;
                if (timer == null || !timer.IsAlive)
                    return 0;
                return timer.TimeUntilElapsed;
            }
        }


		// Icon to show on players, can be id
		public ushort Icon { get { return 457; } }

		
		// unique id for identification in effect list
		public ushort InternalID	{	get { return m_id; }	set { m_id = value; } }

		
		// Delve Info
		public IList DelveInfo
		{
			get 
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add(delveString);

                int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" "); //empty line
					if(seconds > 60)
						delveInfoList.Add("- " + seconds/60 + ":" + (seconds%60).ToString("00") + " minutes remaining.");
					else
						delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList; 
			}
		}
	}
}
