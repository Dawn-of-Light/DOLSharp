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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Handles power drain (conversion of target health to caster
    /// power).
    /// </summary>
    /// <author>Aredhel</author>
    [SpellHandler("PowerDrain")]
    public class PowerDrain : DirectDamageSpellHandler
    {
        /// <summary>
        /// Execute direct effect.
        /// </summary>
        /// <param name="target">Target that takes the damage.</param>
        /// <param name="effectiveness">Effectiveness of the spell (0..1, equalling 0-100%).</param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            // Calculate damage to the target.
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            SendDamageMessages(ad);
            DamageTarget(ad, true);
            DrainPower(ad);
            target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
        }

        /// <summary>
        /// Use a percentage of the damage to refill caster's power.
        /// </summary>
        /// <param name="ad">Attack data.</param>
        public virtual void DrainPower(AttackData ad)
        {
            if (ad == null || !Caster.IsAlive)
            {
                return;
            }

            GameLiving owner = Owner();
            if (owner == null)
            {
                return;
            }

            int powerGain = (ad.Damage + ad.CriticalDamage) * Spell.LifeDrainReturn / 100;
            powerGain = owner.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, powerGain);

            if (powerGain > 0)
            {
                MessageToOwner($"Your summon channels {powerGain} power to you!", eChatType.CT_Spell);
            }
            else
            {
                MessageToOwner("You cannot absorb any more power.", eChatType.CT_SpellResisted);
            }
        }

        /// <summary>
        /// The target of the drain. Generally the caster, except for necropet
        /// </summary>
        /// <returns></returns>
        protected virtual GameLiving Owner()
        {
            return Caster;
        }

        /// <summary>
        /// Send message to owner.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatType"></param>
        protected virtual void MessageToOwner(string message, eChatType chatType)
        {
            base.MessageToCaster(message, chatType);
        }

        /// <summary>
        /// Create a new handler for the power drain spell.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public PowerDrain(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    }
}
