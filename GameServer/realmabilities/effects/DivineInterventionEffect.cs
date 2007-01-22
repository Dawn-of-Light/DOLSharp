using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Events;
//using DOL.GS.GameEvents;

namespace DOL.GS.Effects
{
	/// <summary>
	/// 
	/// </summary>
	public class DivineInterventionEffect : StaticEffect, IGameEffect
	{
		public DivineInterventionEffect(int value)
			: base()
		{
			poolValue = value;
		}

		protected const String delveString = "This ability creates a pool of healing on the user, instantly healing any member of the caster's group when they go below 75% hp, until it is used up.";
		GamePlayer m_player;
		public int poolValue = 0;
		protected long m_startTick;
		protected RegionTimer m_expireTimer;
		ArrayList affected = new ArrayList();
		public void Start(GamePlayer player)
		{
			m_player = player;

            if (player.PlayerGroup == null)
                return;
            m_player.Out.SendMessage("Your group is protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            m_startTick = player.CurrentRegion.Time;
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            GameEventMgr.AddHandler(m_player.PlayerGroup, PlayerGroupEvent.PlayerJoined, new DOLEventHandler(PlayerJoinedGroup));
            GameEventMgr.AddHandler(m_player.PlayerGroup, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(PlayerDisbandedGroup));

			foreach (GamePlayer g_player in player.PlayerGroup.GetPlayersInTheGroup())
			{
				foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					t_player.Out.SendSpellEffectAnimation(player, g_player, 7036, 0, false, 1);
				}
				if (g_player == player) continue;
				g_player.Out.SendMessage("Your are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				affected.Add(g_player);
				GameEventMgr.AddHandler(g_player,GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
			}
            StartTimers();
            player.EffectList.Add(this);  
		}
        protected void PlayerJoinedGroup(DOLEvent e, object sender, EventArgs args)
        {
            PlayerJoinedEventArgs pjargs = args as PlayerJoinedEventArgs;
            if (pjargs == null) return;
            affected.Add(pjargs.Player);
            GameEventMgr.AddHandler(pjargs.Player, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
            pjargs.Player.Out.SendMessage("Your are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
        }
        protected void PlayerDisbandedGroup(DOLEvent e, object sender, EventArgs args)
        {
            PlayerDisbandedEventArgs pdargs = args as PlayerDisbandedEventArgs;
            if (pdargs == null) return;
            affected.Remove(pdargs.Player);
            GameEventMgr.RemoveHandler(pdargs.Player, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
            pdargs.Player.Out.SendMessage("Your are not longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
        }
		protected void TakeDamage(DOLEvent e, object sender, EventArgs args)
		{
			TakeDamageEventArgs targs = args as TakeDamageEventArgs;
			GamePlayer player = sender as GamePlayer;	
            if (!WorldMgr.CheckDistance(player,m_player,2300))
                return;
            if (targs.DamageType == eDamageType.Falling) return;
            if (!player.IsAlive) return;
            int dmgamount = player.MaxHealth-player.Health;  
            if (dmgamount <= 0 || player.HealthPercent >= 75) return;
            int healamount = 0;
                if (poolValue <= 0)
                    Cancel(false);
                if (!(player.IsAlive))
                    return;
                if (poolValue - dmgamount > 0)
                {
                    healamount = dmgamount;
                }
                else
                {
                    healamount = dmgamount - poolValue;
                }
                foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    t_player.Out.SendSpellEffectAnimation(m_player, player, 8051, 0, false, 1);
                }
                player.Out.SendMessage("You are healed by the pool of healing for " + healamount + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                player.ChangeHealth(m_player, GameLiving.eHealthChangeType.Spell, healamount);
                poolValue -= dmgamount;
                if (poolValue <= 0)
                    Cancel(false);          
		}
		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			DivineInterventionEffect DIEffect = (DivineInterventionEffect)player.EffectList.GetOfType(typeof(DivineInterventionEffect));
			if (DIEffect != null)
			{
				DIEffect.Cancel(false);
			}
		}
		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			StopTimers();
			m_player.EffectList.Remove(this);
            GameEventMgr.RemoveHandler(m_player.PlayerGroup, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(PlayerDisbandedGroup));
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            GameEventMgr.RemoveHandler(m_player.PlayerGroup, PlayerGroupEvent.PlayerJoined, new DOLEventHandler(PlayerJoinedGroup));
            foreach (GamePlayer pl in affected)
            {
                pl.Out.SendMessage("Your are no longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                GameEventMgr.RemoveHandler(pl, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
            }
            affected.Clear();
            m_player.EffectList.Remove(this);
		}
		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();
            m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), RealmAbilities.DivineInterventionAbility.poolDuration * 1000);
		}
		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		protected virtual void StopTimers()
		{
			if (m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}
		/// <summary>
		/// The callback method when the effect expires
		/// </summary>
		/// <param name="timer">the gametimer of the effect</param>
		/// <returns>the new intervall (0) </returns>
		protected virtual int ExpiredCallback(RegionTimer timer)
		{
			Cancel(false);
			return 0;
		}
		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name { get { return "Divine Intervention"; } }

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
		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon { get { return 3035; } }
		ushort m_id;
		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID { get { return m_id; } set { m_id = value; } }
		/// <summary>
		/// Delve Info
		/// </summary>
		public IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(10);
				delveInfoList.Add(delveString);
				delveInfoList.Add(" ");
				delveInfoList.Add("Target: Group");
				delveInfoList.Add("Duration: 15:00 min");
				delveInfoList.Add(" ");
				delveInfoList.Add("Value: "+poolValue);
                int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					if (seconds > 60)
						delveInfoList.Add("- " + seconds / 60 + ":" + (seconds % 60).ToString("00") + " minutes remaining.");
					else
						delveInfoList.Add("- " + seconds + " seconds remaining.");
				}
				return delveInfoList;
			}
		}
	}
}
