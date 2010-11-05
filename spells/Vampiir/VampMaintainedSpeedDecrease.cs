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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell handler for speed decreasing spells.  Special for vampiirs
    /// </summary>
    [SpellHandler("VampSpeedDecrease")]
    public class VampMaintainedSpeedDecrease : SpeedDecreaseSpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected GameLiving m_originalTarget = null;
		protected bool m_isPulsing = false;

        public VampMaintainedSpeedDecrease(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        /// <summary>
        /// Creates the corresponding spell effect for the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
			m_originalTarget = target;

			// this acts like a pulsing spell effect, but with 0 frequency.
            return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
        }

		/// <summary>
		/// This spell is a pulsing spell, not a pulsing effect, so we check spell pulse
		/// </summary>
		/// <param name="effect"></param>
		public override void OnSpellPulse(PulsingSpellEffect effect)
		{
			if (m_originalTarget == null || Caster.ObjectState != GameObject.eObjectState.Active || m_originalTarget.ObjectState != GameObject.eObjectState.Active)
			{
				MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
				effect.Cancel(false);
				return;
			}

			if (!Caster.IsAlive ||
				!m_originalTarget.IsAlive ||
				Caster.IsMezzed ||
				Caster.IsStunned ||
				Caster.IsSitting ||
				(Caster.TargetObject is GameLiving ? m_originalTarget != Caster.TargetObject as GameLiving : true))
			{
				MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
				effect.Cancel(false);
				return;
			}

			if (!Caster.IsWithinRadius(m_originalTarget, CalculateSpellRange()))
			{
				MessageToCaster("Your target is no longer in range.", eChatType.CT_SpellExpires);
				effect.Cancel(false);
				return;
			}

			if (!Caster.TargetInView)
			{
				MessageToCaster("Your target is no longer in view.", eChatType.CT_SpellExpires);
				effect.Cancel(false);
				return;
			}

			base.OnSpellPulse(effect);
		}


        protected override void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            // Spell can be used in combat, do nothing
        }

    }
}