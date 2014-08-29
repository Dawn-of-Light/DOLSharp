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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// EffectRangeCheckHelper is meant to provide standard timers for Spell Effect that need range checks
	/// Should Disable Spell Effect changes on failed range check without removing it (displaying as fading)
	/// Should Restore Spell Effect changes when range check succeed again.
	/// </summary>
	public class EffectRangeCheckHelper
	{
		/// <summary>
		/// SpellHandler to Maintain
		/// </summary>
		private ISpellHandler m_spellHandler;
		
		/// <summary>
		/// SpellHandler to Maintain
		/// </summary>
		public ISpellHandler SpellHandler
		{
			get { return m_spellHandler; }
		}
		
		/// <summary>
		/// Timer Holding Range check
		/// </summary>
		private RangeCheckAction m_actionTimer;
		
		/// <summary>
		/// Dictionary containing all effect handled by this helper.
		/// </summary>
		private ConcurrentDictionary<GameSpellEffect, bool> m_effectList = new ConcurrentDictionary<GameSpellEffect, bool>();
		
		/// <summary>
		/// Dictionary containing all effect handled by this helper.
		/// </summary>
		public ConcurrentDictionary<GameSpellEffect, bool> EffectList
		{
			get { return m_effectList; }
		}
		
		/// <summary>
		/// Range check interval in ms (default 5000ms)
		/// </summary>
		private int m_rangeCheckInterval;
		
		/// <summary>
		/// Max Range check for this effect.
		/// </summary>
		private int m_rangeMax;
		
		/// <summary>
		/// Max Range check for this effect.
		/// </summary>
		public int RangeMax
		{
			get { return m_rangeMax; }
			set { m_rangeMax = value; }
		}
		
		/// <summary>
		/// Called when Effect Start.
		/// </summary>
		/// <param name="effect"></param>
		public virtual void OnEffectStart(GameSpellEffect effect)
		{
			// doesn't handle self effect
			if (effect.Owner == null || effect.Owner == SpellHandler.Caster)
				return;
			
			// Check if this effect is already handled.
			if (!m_effectList.ContainsKey(effect))
			{
				m_effectList.TryAdd(effect, true);
			}
			
			if (!m_actionTimer.IsAlive)
			{
				m_actionTimer.Start(m_rangeCheckInterval);
			}
		}
		
		/// <summary>
		/// Called to check if the effect need to be expired, or if it was already done by range.
		/// </summary>
		/// <param name="effect"></param>
		/// <returns></returns>
		public virtual bool IsAlreadyExpired(GameSpellEffect effect) 
		{
			// self buff are not checked, so the handler should expires them !
			if (effect == null || effect.Owner == SpellHandler.Caster)
				return false;
			
			// returns true, if the effect is already disabled...
			if (m_effectList.ContainsKey(effect))
			{
				return !m_effectList[effect];
			}
			
			return false;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="spellHandler"></param>
		/// <param name="range"></param>
		public EffectRangeCheckHelper(ISpellHandler spellHandler, int range, int interval)
		{
			m_spellHandler = spellHandler;
			m_rangeMax = range;
			m_actionTimer = new RangeCheckAction(spellHandler.Caster, this);
			m_rangeCheckInterval = interval;
			m_actionTimer.Interval = m_rangeCheckInterval;
		}
		
		public EffectRangeCheckHelper(ISpellHandler spellHandler, int range)
			: this(spellHandler, range, 5000)
		{
		}
		
		/// <summary>
		/// Checks effect owner distance and cancels the effect if too far
		/// </summary>
		private sealed class RangeCheckAction : RegionAction
		{
			/// <summary>
			/// The helper owning this Timer.
			/// </summary>
			private EffectRangeCheckHelper m_rangeHelper;
			
			/// <summary>
			/// Constructs a new RangeCheckAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="handler">The spell handler</param>
			public RangeCheckAction(GameLiving actionSource, EffectRangeCheckHelper helper)
				: base(actionSource)
			{
				m_rangeHelper = helper;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				List<GameSpellEffect> toRemove = new List<GameSpellEffect>();
				
				foreach (KeyValuePair<GameSpellEffect, bool> pair in m_rangeHelper.EffectList)
				{
					GameSpellEffect effect = pair.Key;
					bool active = pair.Value;
					bool changed = false;
					
					// Effect should not be taken care of anymore.
					if (effect != null && (effect.Owner == null || effect.IsExpired))
					{
						toRemove.Add(effect);
						continue;
					}
					
					// Check Range
					if (effect.Owner.IsWithinRadius(m_rangeHelper.SpellHandler.Caster, m_rangeHelper.RangeMax))
					{
						// In Range - Should be active
						if (!active)
						{
							m_rangeHelper.SpellHandler.OnEffectStart(effect);
							m_rangeHelper.EffectList[effect] = true;
							effect.IsFading = false;
							changed = true;
						}
					}
					else
					{
						// Out of Range - Should de-activate
						if (active)
						{
							m_rangeHelper.SpellHandler.OnEffectExpires(effect, true);
							m_rangeHelper.EffectList[effect] = false;
							effect.IsFading = true;
							changed = true;
						}
					}
					
					// Effect state changed !
					if (changed)
					{
						if (effect.Owner is GamePlayer)
						{
							List<IGameEffect> list = new List<IGameEffect>(1);
							list.Add(effect);
							int changeCount = 1;
							((GamePlayer)effect.Owner).Out.SendUpdateIcons(list, ref changeCount);
						}
					}
				}
				
				// Remove effect that should be off.
				foreach (GameSpellEffect eff in toRemove)
				{
					GameSpellEffect effect = eff;
					bool dummy;
					m_rangeHelper.EffectList.TryRemove(effect, out dummy);
				}
				
				// If we have no more effect to handle we should stop.
				if (m_rangeHelper.EffectList.Count < 1)
				{
					Stop();
					return;
				}
			}
		}
	}
}
