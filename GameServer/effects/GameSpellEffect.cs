/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Reflection;

using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.Language;

using log4net;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Spell Effect assists SpellHandler with duration spells
	/// </summary>
	public class GameSpellEffect : IGameEffect, IConcentrationEffect
	{
		#region private internal
		
		/// <summary>
		/// Lock object for thread access
		/// </summary>
		private readonly object m_LockObject = new object(); // dummy object for thread sync - Mannen

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		#endregion

		#region Protected / Getters

		/// <summary>
		/// The spell handler of this effect
		/// </summary>
		protected ISpellHandler m_handler;
		/// <summary>
		/// associated Spell handler
		/// </summary>
		public ISpellHandler SpellHandler
		{
			get { return m_handler; }
			protected set { RestoredEffect = false; m_handler = value; }
		}

		/// <summary>
		/// The owner of this effect
		/// </summary>
		protected GameLiving m_owner;
		/// <summary>
		/// The living to that this effect is applied to
		/// </summary>
		public GameLiving Owner
		{
			get { return m_owner; }
			protected set { m_owner = value; }
		}
		
		/// <summary>
		/// The internal unique ID of this effect
		/// </summary>
		protected ushort m_id;
		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The effect duration in milliseconds
		/// </summary>
		protected int m_duration;
		/// <summary>
		/// Duration of the spell effect in milliseconds
		/// </summary>
		public int Duration
		{
			get { return m_duration; }
			protected set { m_duration = value; }
		}
		
		/// <summary>
		/// The effect frequency in milliseconds
		/// </summary>
		protected int m_pulseFreq;
		/// <summary>
		/// Effect frequency
		/// </summary>
		public int PulseFreq
		{
			get { return m_pulseFreq; }
			protected set { m_pulseFreq = value; }
		}
		
		/// <summary>
		/// The effectiveness of this effect
		/// </summary>
		protected double m_effectiveness;
		/// <summary>
		/// Effectiveness of the spell effect 0..1
		/// </summary>
		public double Effectiveness
		{
			get { return m_effectiveness; }
			protected set { m_effectiveness = value; }
		}
		
		protected bool m_disabled;
		
		public bool IsDisabled
		{
			get { return m_disabled; }
			protected set { m_disabled = value; }
		}

		/// <summary>
		/// The flag indicating that this effect has expired
		/// </summary>
		protected bool m_expired;
		
		public bool IsExpired
		{
			get { return m_expired; }
			protected set { m_expired = value; }
		}

		protected int m_immunityDuration = 0;
		
		public int ImmunityDuration
		{
			get { return m_immunityDuration; }
			protected set { m_immunityDuration = value; }
		}
		
		/// <summary>
		/// The timer for pulsing effects
		/// </summary>
		protected PulsingEffectTimer m_timer;
		#endregion

		#region public Getters
       
		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get 
			{
				if (Spell != null)
					return Spell.Name;
				
				return string.Empty;
			}
		}

		/// <summary>
		/// The name of the owner
		/// </summary>
		public string OwnerName
		{
			get
			{
				if (Owner != null)
					return Owner.Name;
				
				return string.Empty;
			}
		}

		/// <summary>
		/// Amount of concentration used by effect
		/// </summary>
		public byte Concentration
		{
			get 
			{
				if (Spell != null)
					return Spell.Concentration;
				
				return 0;
			}
		}

		/// <summary>
		/// Icon to show on players Effects bar
		/// </summary>
		public ushort Icon
		{
			get
			{
				if (Spell != null)
					return Spell.Icon;

				return 0;
			}
		}

		/// <summary>
		/// Remaining Effect duration in milliseconds
		/// </summary>
		public int RemainingTime
		{
			get
			{
				if (Duration == 0)
					return 0;

				if (m_timer == null || !m_timer.IsAlive)
					return 0;
				
				return Duration - m_timer.TimeSinceStart;
			}
		}

		/// <summary>
		/// Spell thats used
		/// </summary>
		public Spell Spell
		{
			get 
			{
				if (SpellHandler != null)
					return SpellHandler.Spell;
				
				return null;				
			}
		}
		
		/// <summary>
		/// True if effect is in immunity state
		/// </summary>
		public bool ImmunityState
		{
			get { return IsExpired && m_timer != null && m_timer.IsAlive; }
		}
		
		#endregion
		
		#region Constructor
		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler">the spell handler</param>
		/// <param name="duration">the spell duration in milliseconds</param>
		/// <param name="pulseFreq">the pulse frequency in milliseconds</param>
		public GameSpellEffect(ISpellHandler handler, int duration, int pulseFreq) : this(handler, duration, pulseFreq, 1)
		{
		}

		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler">the spell handler</param>
		/// <param name="duration">the spell duration in milliseconds</param>
		/// <param name="pulseFreq">the pulse frequency in milliseconds</param>
		/// <param name="effectiveness">the effectiveness</param>
		public GameSpellEffect(ISpellHandler handler, int duration, int pulseFreq, double effectiveness)
		{
			m_handler = handler;
			m_duration = duration;
			m_pulseFreq = pulseFreq;
			m_effectiveness = effectiveness;
			m_expired = true; // not started = expired
			m_disabled = true; // not enabled
		}
		#endregion
		
		#region effect enable/disable
		
		/// <summary>
		/// Enable Effect on Target Without Adding
		/// </summary>
		public virtual void EnableEffect()
		{
			// Check if need enabling
			bool canEnable = false;
			lock (m_LockObject)
			{
				if (IsDisabled && !IsExpired)
				{
					canEnable = true;
					IsDisabled = false;
				}
			}
			
			if (canEnable)
			{
				if (RestoredEffect)
					SpellHandler.OnEffectRestored(this, RestoreVars);
				else
					SpellHandler.OnEffectStart(this);

				UpdateEffect();
			}
		}
		
		/// <summary>
		/// Disable Effect on Target without Removing
		/// </summary>
		/// <param name="noMessages"></param>
		public virtual void DisableEffect(bool noMessages)
		{
			// Check if need disabling.
			bool canDisable = false;
			lock (m_LockObject)
			{
				if (!IsDisabled)
				{
					canDisable = true;
					IsDisabled = true;
				}
			}
						
			if (canDisable)
			{
				int immunityDuration = 0;
				if (RestoredEffect)
					immunityDuration = SpellHandler.OnRestoredEffectExpires(this, RestoreVars, noMessages);
				else
					immunityDuration = SpellHandler.OnEffectExpires(this, noMessages);
				
				UpdateEffect();

				// Save Immunity Duration returned.
				lock (m_LockObject)
				{
					ImmunityDuration = immunityDuration;
				}
			}
		}
		
		/// <summary>
		/// Remove effect Completely from Owner
		/// </summary>
		/// <param name="noMessages"></param>
		protected virtual void RemoveEffect(bool noMessages)
		{
			lock (m_LockObject)
			{
				StopTimers();
				
				// Expire Effect
				IsExpired = true;
				
				// Remove concentration Effect from Caster List.
				if (Concentration > 0 && SpellHandler != null && SpellHandler.Caster != null && SpellHandler.Caster.ConcentrationEffects != null) 
					SpellHandler.Caster.ConcentrationEffects.Remove(this);
				
				// Remove effect from Owner list
				if(Owner != null && Owner.EffectList != null) 
					Owner.EffectList.Remove(this);
			}
			
			// Try disabling Effect
			DisableEffect(false);
		}
		
		/// <summary>
		/// Add Effect and Enable when First Starting on Target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="enable"></param>
		protected virtual void AddEffect(GameLiving target, bool enable)
		{
			bool commitChange = false;
			try
			{
				lock (m_LockObject)
				{
					// already started ?
					if (!IsExpired)
					{
						if (log.IsErrorEnabled)
							log.ErrorFormat("Tried to start non-expired effect ({0})).\n{1}", this, Environment.StackTrace);
						return; 
					}
					
					// already added ?
					if (Owner != null)
					{
						if (log.IsErrorEnabled)
							log.ErrorFormat("Tried to start an already owned effect ({0})).\n{1}", this, Environment.StackTrace);
						return; 
					}
					
					//Enable Effect
					Owner = target;
					IsExpired = false;
					try
					{
						Owner.EffectList.BeginChanges();
						commitChange = true;
					}
					catch (Exception ex)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Effect ({0}) Could not Begin Change in living - {1} - Spell Effect List, {2}", this, Owner, ex);
						commitChange = false;
					}
					
					// Insert into Owner Effect List
					if (!Owner.EffectList.Add(this))
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("{0}: effect was not added to the effects list, not starting it either. (effect class:{1} spell type:{2} spell name:'{3}')", Owner.Name, GetType().FullName, Spell.SpellType, Name);
						return;
					}
					
					// Add concentration Effect To Caster List.
					if (Concentration > 0 && SpellHandler != null && SpellHandler.Caster != null && SpellHandler.Caster.ConcentrationEffects != null)
						SpellHandler.Caster.ConcentrationEffects.Add(this);
					
					StartTimers();
				}
				
				// Try Enabling Effect
				if (enable)
					EnableEffect();
				
				SpellHandler.OnEffectAdd(this);
			}
			finally
			{
				if (commitChange)
					Owner.EffectList.CommitChanges();
			}
			
			// Start first pulse.
			PulseCallback();
		}
		
		/// <summary>
		/// Update Effect in Owner List
		/// </summary>
		protected virtual void UpdateEffect()
		{
			// Update effect in player display.
			if (Owner != null && Owner.EffectList != null)
			{
				Owner.EffectList.BeginChanges();
				try
				{
					Owner.EffectList.OnEffectsChanged(this);
				}
				finally
				{
					Owner.EffectList.CommitChanges();
				}
			}
		}
		#endregion
		
		#region effects methods

		/// <summary>
		/// Starts the effect
		/// </summary>
		/// <param name="target">the target</param>
		public virtual void Start(GameLiving target)
		{
			AddEffect(target, true);
		}

		/// <summary>
		/// Starts the effect without enabling it.
		/// </summary>
		/// <param name="target">the target</param>
		public virtual void StartDisabled(GameLiving target)
		{
			AddEffect(target, false);
		}

		/// <summary>
		/// Cancels the effect
		/// </summary>
		/// <param name="playerCanceled">true if canceled by the player</param>
		public virtual void Cancel(bool playerCanceled)
		{
			lock (m_LockObject)
			{
				// Player can't remove negative effect or Effect in Immunity State
				if (playerCanceled && ((SpellHandler != null && !SpellHandler.HasPositiveEffect) || ImmunityState))
				{
					GamePlayer player = Owner as GamePlayer;
					if (player != null)
						player.Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, "Effects.GameSpellEffect.CantRemoveEffect"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
	
					return;
				}
				
				// Can't Cancel Immunity Effect from Alive Living
				if (ImmunityState && Owner != null && Owner.IsAlive)
					return;
				
				// Expire Effect
				IsExpired = true;
			}
			
			// Check if Immunity needed.
			Owner.EffectList.BeginChanges();
			try
			{
				DisableEffect(false);
				SpellHandler.OnEffectRemove(this, false);
				lock (m_LockObject)
				{
					if (m_immunityDuration > 0)
					{
						Duration = m_immunityDuration;
						StartTimers();
						UpdateEffect();
						return;
					}
				}
				
				RemoveEffect(false);
			}
			finally
			{
				Owner.EffectList.CommitChanges();
			}
		}

		/// <summary>
		/// Overwrites the effect
		/// concentration based effects should never be overwritten
		/// </summary>
		/// <param name="effect">the new effect</param>
		/// <param name="enable">Start new Effect or not</param>
		protected virtual void ReplaceEffect(GameSpellEffect effect, bool enable)
		{
			if (Concentration > 0) 
			{

				if (log.IsWarnEnabled)
					log.WarnFormat("{0} is trying to overwrite {1},  which has concentration {2}", effect.Name, Name, Concentration);
				return;
			}
						
			// Prevent further change to this effect.
			lock (m_LockObject)
			{
				StopTimers();
				IsExpired = true;
			}

			DisableEffect(true);
			SpellHandler.OnEffectRemove(this, true);
			
			lock (m_LockObject)
			{
				if (!IsDisabled)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("{0} is trying to overwrite an enabled effect {1}", effect.Name, Name);
					return;
				}

				SpellHandler = effect.SpellHandler;
				Duration = effect.Duration;
				PulseFreq = effect.PulseFreq;
				Effectiveness = effect.Effectiveness;

				// Add concentration Effect To Caster List.
				if (Concentration > 0 && SpellHandler != null && SpellHandler.Caster != null && SpellHandler.Caster.ConcentrationEffects != null)
					SpellHandler.Caster.ConcentrationEffects.Add(this);

				// Restart Effect
				IsExpired = false;
				StartTimers();
			}
			
			// Try Enabling Effect
			if (enable)
				EnableEffect();
			else
				UpdateEffect();
			
			SpellHandler.OnEffectAdd(this);
			
			PulseCallback();
		}
		
		/// <summary>
		/// Overwrite existing Effect and Enable new One.
		/// </summary>
		/// <param name="effect"></param>
		public virtual void Overwrite(GameSpellEffect effect)
		{
			ReplaceEffect(effect, true);
		}

		/// <summary>
		/// Overwrite existing Effect without Starting.
		/// </summary>
		/// <param name="effect"></param>
		public virtual void OverwriteDisabled(GameSpellEffect effect)
		{
			ReplaceEffect(effect, false);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();
			// Duration => 0 = endless until explicit stop
			if (Duration > 0 || PulseFreq > 0)
			{
				m_timer = new PulsingEffectTimer(this);
				m_timer.Interval = PulseFreq;
				m_timer.Start(PulseFreq == 0 ? Duration : PulseFreq);
			}
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		protected virtual void StopTimers()
		{
			if (m_timer != null)
			{
				m_timer.Stop();
				m_timer = null;
			}
		}

		/// <summary>
		/// The callback method when the effect expires
		/// </summary>
		protected virtual void ExpiredCallback()
		{
			bool removeEffect = false;
			lock (m_LockObject)
			{
				StopTimers();
				removeEffect = IsExpired;
			}
			
			if (removeEffect)
				RemoveEffect(false);
			else
				Cancel(false);
		}

		/// <summary>
		/// Pulse callback
		/// </summary>
		protected virtual void PulseCallback()
		{
			bool canPulse = false;
			lock (m_LockObject)
			{
				if (!IsDisabled && !IsExpired && PulseFreq > 0)
					canPulse = true;
			}
			
			if (canPulse)
				SpellHandler.OnEffectPulse(this);
		}
		#endregion

		/// <summary>
		/// Delve information
		/// </summary>
		public virtual IList<string> DelveInfo
		{
			get
			{
				IList<string> list = m_handler.DelveInfo;

				int seconds = RemainingTime / 1000;
				if (seconds > 0)
				{
					list.Add(" "); //empty line
					if (seconds > 60)
						list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.MinutesRemaining", (seconds / 60), (seconds % 60).ToString("00")));
					else
						list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.SecondsRemaining", seconds));
				}

				return list;
			}
		}

		public int[] RestoreVars = new int[] { };
		public bool RestoredEffect = false;

		public PlayerXEffect getSavedEffect()
		{
			if (this.RestoredEffect && this.RemainingTime > 5000)
			{
				PlayerXEffect eff = new PlayerXEffect();
				eff.Duration = this.RemainingTime;
				eff.IsHandler = true;
				eff.Var1 = this.RestoreVars[0];
				eff.Var2 = this.RestoreVars[1];
				eff.Var3 = this.RestoreVars[2];
				eff.Var4 = this.RestoreVars[3];
				eff.Var5 = this.RestoreVars[4];
				eff.Var6 = this.RestoreVars[5];
				eff.SpellLine = this.SpellHandler.SpellLine.KeyName;
				return eff;
			}
			if (m_handler != null)
			{
				PlayerXEffect eff = m_handler.GetSavedEffect(this);
				return eff;
			}
			return null;
		}

		/// <summary>
		/// Returns the string representation of the GameSpellEffect
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Duration={0}, Owner.Name={1}, PulseFreq={2}, RemainingTime={3}, Effectiveness={4}, m_expired={5}\nSpellHandler info: {6}",
			                     Duration, Owner == null ? "(null)" : Owner.Name, PulseFreq, RemainingTime, Effectiveness, m_expired, SpellHandler == null ? "(null)" : SpellHandler.ToString());
		}
		
		/// <summary>
		/// Restart Timers after a Region Froze
		/// </summary>
		public virtual void RestartTimers()
		{
			lock (m_LockObject)
			{
				if (m_timer != null)
				{
					Duration = (int)Math.Max(1, Duration - (GameTimer.GetTickCount() - m_timer.LastStartedTick));
					StartTimers();
				}
			}
			
			UpdateEffect();
		}
		
		#region timer subclass		
		/// <summary>
		/// Handles effect pulses
		/// </summary>
		protected sealed class PulsingEffectTimer : GameTimer
		{
			/// <summary>
			/// The pulsing effect
			/// </summary>
			private readonly GameSpellEffect m_effect;

			/// <summary>
			/// The time in milliseconds since timer start
			/// </summary>
			private int m_timeSinceStart;

			/// <summary>
			/// Gets the effect remaining time, decreased every interval
			/// </summary>
			public int TimeSinceStart
			{
				get { return IsAlive ? m_timeSinceStart - TimeUntilElapsed : 0; }
			}
			
			/// <summary>
			/// Get tick when effect Last Started.
			/// </summary>
			public long LastStartedTick
			{
				get;
				private set;
			}

			/// <summary>
			/// Constructs a new pulsing timer
			/// </summary>
			/// <param name="effect">The pulsing effect</param>
			public PulsingEffectTimer(GameSpellEffect effect) : base(effect.m_owner.CurrentRegion.TimeManager)
			{
				if (effect == null)
					throw new ArgumentNullException("effect");
				m_effect = effect;
			}

			/// <summary>
			/// Starts the timer with defined initial delay
			/// </summary>
			/// <param name="initialDelay">The initial timer delay. Must be more than 0 and less than MaxInterval</param>
			public override void Start(int initialDelay)
			{
				base.Start(initialDelay);
				m_timeSinceStart = initialDelay;
				LastStartedTick = GameTimer.GetTickCount();
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				if (m_effect.Concentration > 0 || m_timeSinceStart < m_effect.Duration)
				{
					m_timeSinceStart += Interval;
					if (m_timeSinceStart > m_effect.Duration)
						Interval = m_timeSinceStart - m_effect.Duration;
					m_effect.PulseCallback();
				}
				else
				{
					Stop();
					m_effect.ExpiredCallback();
				}
			}

			/// <summary>
			/// Returns short information about the timer
			/// </summary>
			/// <returns>Short info about the timer</returns>
			public override string ToString()
			{
				return string.Format(" effect: ({0})", m_effect);
			}
		}
		#endregion
	}
}