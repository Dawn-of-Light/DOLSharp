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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.Language;

using log4net;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Spell Effect assists SpellHandler with duration spells
	/// </summary>
	public class GameSpellEffect : IGameEffect, IConcentrationEffect
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Lock object for Thread Syncing.
		/// </summary>
		private readonly object m_LockObject = new object();
		
		/// <summary>
		/// The spell handler of this effect
		/// </summary>
		protected ISpellHandler m_handler;

		/// <summary>
		/// The owner of this effect
		/// </summary>
		protected GameLiving m_owner;

		/// <summary>
		/// The internal unique ID of this effect
		/// </summary>
		protected ushort m_id;

		/// <summary>
		/// The effect duration in milliseconds
		/// </summary>
		protected int m_duration;

		/// <summary>
		/// The effect frequency in milliseconds
		/// </summary>
		protected int m_pulseFreq;

		/// <summary>
		/// The effectiveness of this effect
		/// </summary>
		protected double m_effectiveness;

		/// <summary>
		/// The flag indicating that this effect has expired
		/// </summary>
		protected volatile bool m_expired;

		/// <summary>
		/// The timer for pulsing effects
		/// </summary>
		protected PulsingEffectTimer m_timer;

		/// <summary>
        /// Is it a Minotaur Relic Effect?
        /// </summary>
        protected bool m_minotaur = false;

        /// <summary>
        /// Is this effect fading regardless of remaining time ?
        /// </summary>
        protected volatile bool m_isFading = false;


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
				if (m_owner != null)
					return m_owner.Name;
				
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
				if (m_handler != null && m_handler.Spell != null)
					return m_handler.Spell.ClientEffect;
				else
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
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The living to that this effect is applied to
		/// </summary>
		public GameLiving Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Effectiveness of the spell effect 0..1
		/// </summary>
		public double Effectiveness
		{
			get { return m_effectiveness; }
		}

		/// <summary>
		/// Duration of the spell effect in milliseconds
		/// </summary>
		public int Duration
		{
			get { return m_duration; }
		}

		/// <summary>
		/// Effect frequency
		/// </summary>
		public int PulseFreq
		{
			get { return m_pulseFreq; }
		}

		/// <summary>
		/// associated Spell handler
		/// </summary>
		public ISpellHandler SpellHandler
		{
			get { return m_handler; }
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
		/// Is this effect Expired, and shouldn't be in effect.
		/// </summary>
		public bool IsExpired
		{
			get { return m_expired; }
		}
		
		/// <summary>
		/// Is this ImmunityState.
		/// </summary>
		public bool ImmunityState
		{
			get { return m_expired && m_timer != null && m_timer.IsAlive; }
		}
		
		/// <summary>
		/// Set Fading effect state, regardless of remaining time
		/// Useful for range limited spell that should fade with distance from caster.
		/// </summary>
		public bool IsFading
		{
			get { return m_isFading; }
			set { m_isFading = value; }
		}
		
		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler">the spell handler</param>
		/// <param name="duration">the spell duration in milliseconds</param>
		/// <param name="pulseFreq">the pulse frequency in milliseconds</param>
		public GameSpellEffect(ISpellHandler handler, int duration, int pulseFreq)
			: this(handler, duration, pulseFreq, 1)
		{
		}

		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler">the spell handler</param>
		/// <param name="duration">the spell duration in milliseconds</param>
		/// <param name="pulsefreq">the pulse frequency in milliseconds</param>
		/// <param name="mino">is this effect a minotaur effect</param>
		public GameSpellEffect(ISpellHandler handler, int duration, int pulsefreq, bool mino)
            : this(handler, duration, pulsefreq, 1)
        {
            m_minotaur = mino;
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
			// not started = expired
			m_expired = true;
		}

		/// <summary>
		/// Starts the effect
		/// </summary>
		/// <param name="target">the target</param>
		public virtual void Start(GameLiving target)
		{
			// Check if effect is not enable.
			if (!IsExpired)
			{
				if (log.IsErrorEnabled)
					log.Error("Tried to start non-expired effect.\n" + Environment.StackTrace);
				
				// already started
				return;
			}

			if (target == null)
			{
				throw new ArgumentNullException("Target");
			}
			
			lock (m_LockObject)
			{
				// Set Effect Owner to target
				m_owner = target;
			}
				
			try
			{
				// Add effect to owner list.
				Owner.EffectList.BeginChanges();

				if (!Owner.EffectList.Add(this))
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("{0}: effect was not added to the effects list, not starting it either. (effect class:{1} spell type:{2} spell name:'{3}')", target.Name, GetType().FullName, Spell.SpellType, Name);
					
					return;
				}
				
				// Add effect to concentration list if needed.
				if (Concentration > 0) 
				{
					SpellHandler.Caster.ConcentrationEffects.Add(this);
				}

				// Set effect active
				m_expired = false;
				
				lock (m_LockObject)
				{
					// Start it.
					StartTimers();

					// Trigger Spellhandler Effect Start/Pulse.
					if (RestoredEffect)
						SpellHandler.OnEffectRestored(this, RestoreVars);
					else
						SpellHandler.OnEffectStart(this);
					
					SpellHandler.OnEffectPulse(this);
				}
			}
			finally
			{
				// Close Living Effect list Changes.
				Owner.EffectList.CommitChanges();
			}
		}

		/// <summary>
		/// Overwrites the effect
		/// concentration based effects should never be overwritten
		/// </summary>
		/// <param name="effect">the new effect</param>
		public void Overwrite(GameSpellEffect effect)
		{
			if (Spell.Concentration > 0) 
			{

				if (log.IsWarnEnabled)
					log.Warn(effect.Name + " (" + effect.Spell.Name + ") is trying to overwrite " + Spell.Name + " which has concentration " + Spell.Concentration);
				return;
			}

			lock (m_LockObject)
			{
				// Stop effect
				StopTimers();

				// immunity effects in immunity state are already expired
				if (!IsExpired)
				{
					// Only trigger Expires if not already Expired
					m_expired = true;
					m_handler.OnEffectExpires(this, true);
				}
				
				// Replace Effect Handlers
				m_handler = effect.SpellHandler;
				m_duration = effect.Duration;
				m_pulseFreq = effect.PulseFreq;
				m_effectiveness = effect.Effectiveness;

				// Add to concentration List if needed.
				if (Spell.Concentration > 0)
				{
					SpellHandler.Caster.ConcentrationEffects.Add(this);
				}
				
				m_expired = false;
				
				StartTimers();
				
				SpellHandler.OnEffectStart(this);
				SpellHandler.OnEffectPulse(this);
			}

			// Notify Effect Change.
			Owner.EffectList.OnEffectsChanged(this);
		}
		
		/// <summary>
		/// Cancel the Effect
		/// </summary>
		/// <param name="playerCanceled">true if canceled by the player</param>
		public void Cancel(bool playerCanceled)
		{
			Cancel(playerCanceled, false);
		}
		
		/// <summary>
		/// Cancels the effect
		/// </summary>
		/// <param name="playerCanceled">true if canceled by the player</param>
		/// <param name="force">true if cancel effect disregarding rules</param>
		public virtual void Cancel(bool playerCanceled, bool force)
		{

			if (playerCanceled && !(SpellHandler.HasPositiveEffect && !IsExpired))
			{
				if (Owner is GamePlayer)
				{
					((GamePlayer)Owner).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, "Effects.GameSpellEffect.CantRemoveEffect"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				return;
			}
			
			lock (m_LockObject)
			{
				StopTimers();
			}
			
			if (!IsExpired)
			{
				m_expired = true;
				
				int immunityDuration = 0;
				
				lock (m_LockObject)
				{
					// Expires Effect to Get Immunity Duration.
					if (RestoredEffect)
					{
						immunityDuration = SpellHandler.OnRestoredEffectExpires(this, RestoreVars, false);
					}
					else
					{
						immunityDuration = SpellHandler.OnEffectExpires(this, false);
					}
								
					// If immunity is needed start timers.
					if (immunityDuration > 0)
					{
						this.m_duration = immunityDuration;
						StartExpiredTimers(immunityDuration);
						Owner.EffectList.OnEffectsChanged(this);
					}
					else
					{
						// Remove if no Immunity
						RemoveEffect();
					}
				}
			}
			else if (!Owner.IsAlive || force)
			{
				lock (m_LockObject)
				{
					// Remove Immunity/Expired only if owner is dead...
					RemoveEffect();
				}
			}
		}

		/// <summary>
		/// Remove Effect from owner...
		/// </summary>
		protected void RemoveEffect()
		{
			// Prevent errors
			if (Owner != null)
			{
				// Remove effect
				Owner.EffectList.Remove(this);
				
				// If Concentration remove from caster concentration list.
				if (Concentration > 0) 
				{
					SpellHandler.Caster.ConcentrationEffects.Remove(this);
				}
			}
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();
			// 0 = endless until explicit stop
			if (Duration > 0 || PulseFreq > 0)
			{
				m_timer = new PulsingEffectTimer(this);
				m_timer.Interval = PulseFreq;
				m_timer.Start(PulseFreq == 0 ? Duration : PulseFreq);
			}
		}

		/// <summary>
		/// Start Expiration Timer.
		/// </summary>
		/// <param name="duration"></param>
		protected virtual void StartExpiredTimers(int duration)
		{
			StopTimers();
			if (duration > 0)
			{
				m_timer = new PulsingEffectTimer(this);
				m_timer.Interval = 0;
				m_timer.Start(duration);
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
			// Immunity state
			if (IsExpired)
			{
				StopTimers();
				RemoveEffect();
			}
			else
			{
				Cancel(false);
			}
		}

		/// <summary>
		/// Pulse callback
		/// </summary>
		protected virtual void PulseCallback()
		{
			if (SpellHandler != null)
				SpellHandler.OnEffectPulse(this);
		}
		
		/// <summary>
		/// Restore Vars for effect imported from Database.
		/// </summary>
		public int[] RestoreVars = new int[] { };
		
		/// <summary>
		/// Flag for Restored Effect. 
		/// </summary>
		public bool RestoredEffect = false;

		/// <summary>
		/// Return a Saved Effect for Database Storage.
		/// </summary>
		/// <returns></returns>
		public PlayerXEffect getSavedEffect()
		{
			// If this is already a restored effect get current values.
			if (this.RestoredEffect)
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
				
				if (SpellHandler != null && SpellHandler.SpellLine != null)
					eff.SpellLine = this.SpellHandler.SpellLine.KeyName;
				
				return eff;
			}
			
			// Build a restored effected if Spellhandler is available.
			if (SpellHandler != null)
			{
				PlayerXEffect eff = SpellHandler.GetSavedEffect(this);
				return eff;
			}
			
			return null;
		}
		
		/// <summary>
		/// Delve information
		/// </summary>
		public IList<string> DelveInfo
		{
			get
			{
				IList<string> list;
				
				if (SpellHandler != null)
				{
					 list = SpellHandler.DelveInfo;
				}
				else
				{
					list = new List<string>();
				}

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

		/// <summary>
		/// Returns the string representation of the GameSpellEffect
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(64)
				.Append("Duration=").Append(Duration)
				.Append(", Owner.Name=").Append(Owner==null?"(null)":Owner.Name)
				.Append(", PulseFreq=").Append(PulseFreq)
				.Append(", RemainingTime=").Append(RemainingTime)
				.Append(", Effectiveness=").Append(Effectiveness)
				.Append(", m_expired=").Append(m_expired)
				.Append("\nSpellHandler info: ")/*.Append(SpellHandler==null?"(null)":SpellHandler.ToString())*/
				.ToString();
		}
		
		/// <summary>
		/// Handles effect pulses and effect time
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
					
					if (!m_effect.IsExpired)
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
				return new StringBuilder(base.ToString(), 128)
					.Append(" effect: (").Append(m_effect.ToString()).Append(')')
					.ToString();
			}
		}
	}
}