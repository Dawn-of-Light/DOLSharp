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
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
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
		protected bool m_expired;
		/// <summary>
		/// The timer for pulsing effects
		/// </summary>
		protected PulsingEffectTimer m_timer;

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
				.Append("\nSpellHandler info: ").Append(SpellHandler==null?"(null)":SpellHandler.ToString())
				.ToString();
		}

		/// <summary>
		/// Starts the effect
		/// </summary>
		/// <param name="target">the target</param>
		public void Start(GameLiving target)
		{
			lock (this)
			{
				if (!m_expired)
				{
					if (log.IsErrorEnabled)
						log.Error("Tried to start non-expired effect.\n" + Environment.StackTrace);
					return; // already started?
				}

				m_owner = target;
				try
				{
					m_owner.EffectList.BeginChanges();

					if (!m_owner.EffectList.Add(this))
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("{0}: effect was not added to the effects list, not starting it either. (effect class:{1} spell type:{2} spell name:'{3}')", target.Name, GetType().FullName, Spell.SpellType, Name);
						return;
					}

					m_expired = false;
					StartTimers();

					if (Spell.Concentration > 0) 
					{
						SpellHandler.Caster.ConcentrationEffects.Add(this);
					}
					m_handler.OnEffectStart(this);
					m_handler.OnEffectPulse(this);
				}
				finally
				{
					m_owner.EffectList.CommitChanges();
				}
			}
		}

		/// <summary>
		/// Cancels the effect
		/// </summary>
		/// <param name="playerCanceled">true if canceled by the player</param>
		public virtual void Cancel(bool playerCanceled)
		{
			if (log.IsDebugEnabled)
				log.Debug(Owner.Name+": CancelEffect playerCanceled="+playerCanceled+"  SpellType="+Spell.SpellType);
			if (playerCanceled && !m_handler.HasPositiveEffect) {
				if (Owner is GamePlayer)
					((GamePlayer) Owner).Out.SendMessage("You can't remove this effect!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			lock(this)
			{				
				if (m_expired)
					return;

				m_expired = true;
				StopTimers();
				if(m_owner != null) {
//					if (log.IsDebugEnabled)
//						log.Debug("Effect, remove from effect list");
					m_owner.EffectList.Remove(this);
//					if (log.IsDebugEnabled)
//						log.Debug("Effect, removed");
					if (Spell.Concentration > 0) 
					{
						SpellHandler.Caster.ConcentrationEffects.Remove(this);
					}
					m_handler.OnEffectExpires(this, false);
				}
			}
			//DOLConsole.WriteLine("done cancel effect on "+Owner.Name);
		}

		/// <summary>
		/// Overwrites the effect
		/// concentration based effects should never be overwritten
		/// </summary>
		/// <param name="effect">the new effect</param>
		public void Overwrite(GameSpellEffect effect)
		{
			//DOLConsole.WriteLine("overwrite effect on "+Owner.Name+" effect "+effect.Spell.Name);
			if (Spell.Concentration > 0) 
			{
				if (log.IsWarnEnabled)
					log.Warn("Tried to Overwrite conc based effect! effectowner:"+effect.Owner+" effect:"+effect.Spell.Name+" caster:"+SpellHandler.Caster);
				return;
			}

			lock (this)
			{
				// immunity effects in immunity state are already expired
				if (!m_expired)
				{
					m_expired = true;
					m_handler.OnEffectExpires(this, true);
				}
				StopTimers();
				m_handler = effect.m_handler;
				m_duration = effect.m_duration;
				m_pulseFreq = effect.m_pulseFreq;
				m_effectiveness = effect.m_effectiveness;
				StartTimers();
				if (Spell.Concentration > 0) 
				{
					SpellHandler.Caster.ConcentrationEffects.Add(this);
				}
				m_owner.EffectList.OnEffectsChanged(this);
				m_handler.OnEffectStart(this);
				m_handler.OnEffectPulse(this);
				//DOLConsole.WriteLine("done overwrite effect on "+Owner.Name);
				m_expired = false;
			}
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();
			if (m_duration > 0 || m_pulseFreq > 0)
			{ // 0 = endless until explicit stop
				m_timer = new PulsingEffectTimer(this);
				m_timer.Interval = m_pulseFreq;
				m_timer.Start(m_pulseFreq == 0 ? m_duration : m_pulseFreq);
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
			lock (this)
			{
				if (m_expired)
					return;
				m_expired = true;
				StopTimers();
			}
			m_owner.EffectList.Remove(this);
			m_handler.OnEffectExpires(this, false);
		}

		/// <summary>
		/// Pulse callback
		/// </summary>
		protected virtual void PulseCallback()
		{
			m_handler.OnEffectPulse(this);
		}

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
				if (m_effect.Spell.Concentration > 0 || m_timeSinceStart < m_effect.Duration)
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
				return new StringBuilder(base.ToString(), 128)
					.Append(" effect: (").Append(m_effect.ToString()).Append(')')
					.ToString();
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get { return m_handler.Spell.Name; }
		}

		/// <summary>
		/// The name of the owner
		/// </summary>
		public string OwnerName
		{
			get { return m_owner.Name; }
		}

		/// <summary>
		/// Amount of concentration used by effect
		/// </summary>
		public byte Concentration
		{
			get { return Spell.Concentration; }
		}

		/// <summary>
		/// Icon to show on players Effects bar
		/// </summary>
		public ushort Icon
		{
			get { return m_handler.Spell.ClientEffect; }
		}

		/// <summary>
		/// Remaining Effect duration in milliseconds
		/// </summary>
		public int RemainingTime
		{
			get
			{
				if (m_duration == 0)
					return 0;
				PulsingEffectTimer timer = m_timer;
				if (timer == null || !timer.IsAlive)
					return 0;
				return m_duration - timer.TimeSinceStart;
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
			get { return m_handler.Spell; }
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public IList DelveInfo
		{
			get
			{
				IList list = m_handler.DelveInfo;

				int seconds = RemainingTime / 1000;
				if (seconds > 0)
				{
					list.Add(" "); //empty line
					if(seconds > 60)
						list.Add("- " + seconds/60 + ":" + (seconds%60).ToString("00") + " minutes remaining.");
					else
						list.Add("- " + seconds + " seconds remaining.");
				}

				return list;
			}
		}
	}
}