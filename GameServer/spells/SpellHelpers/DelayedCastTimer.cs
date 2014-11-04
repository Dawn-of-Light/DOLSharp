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
using System.Text;

using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Casts a spell after the CastTime delay
	/// </summary>
	public class DelayedCastTimer : GameTimer
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		/// <summary>
		/// The spellhandler instance with callbacks
		/// </summary>
		private readonly SpellHandler m_handler;
		/// <summary>
		/// The target object at the moment of CastSpell call
		/// </summary>
		private readonly GameLiving m_target;
		private readonly GameLiving m_caster;
		private byte m_stage;
		private readonly int m_delay1;
		private readonly int m_delay2;

		/// <summary>
		/// Constructs a new DelayedSpellTimer
		/// </summary>
		/// <param name="actionSource">The caster</param>
		/// <param name="handler">The spell handler</param>
		/// <param name="target">The target object</param>
		public DelayedCastTimer(GameLiving actionSource, SpellHandler handler, GameLiving target, int delay1, int delay2)
			: base(actionSource.CurrentRegion.TimeManager)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			if (actionSource == null)
				throw new ArgumentNullException("actionSource");

			m_handler = handler;
			m_target = target;
			m_caster = actionSource;
			m_stage = 0;
			m_delay1 = delay1;
			m_delay2 = delay2;
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected override void OnTick()
		{
			try
			{
				if (m_stage == 0)
				{
					if (!m_handler.CheckAfterCast(m_target))
					{
						Interval = 0;
						m_handler.InterruptCasting();
						return;
					}
					m_stage = 1;
					m_handler.Stage = 1;
					Interval = m_delay1;
				}
				else if (m_stage == 1)
				{
					if (!m_handler.CheckDuringCast(m_target))
					{
						Interval = 0;
						m_handler.InterruptCasting();
						return;
					}
					m_stage = 2;
					m_handler.Stage = 2;
					Interval = m_delay2;
				}
				else if (m_stage == 2)
				{
					m_stage = 3;
					m_handler.Stage = 3;
					Interval = 100;

					if (m_handler.CheckEndCast(m_target))
					{
						m_handler.FinishSpellCast(m_target);
					}
				}
				else
				{
					m_stage = 4;
					m_handler.Stage = 4;
					Interval = 0;
					m_handler.OnAfterSpellCastSequence();
				}

				if (m_caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG && m_stage < 3)
				{
					(m_caster as GamePlayer).Out.SendMessage("[DEBUG] step = " + (m_handler.Stage + 1), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				return;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error(ToString(), e);
			}

			m_handler.OnAfterSpellCastSequence();
			Interval = 0;
		}

		/// <summary>
		/// Returns short information about the timer
		/// </summary>
		/// <returns>Short info about the timer</returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString(), 128)
				.Append(" spellhandler: (").Append(m_handler.ToString()).Append(')')
				.ToString();
		}
	}
}
