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

// made by DeMAN
using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    [SpellHandler("StyleDmgAbs")]
    public class StyleDmgAbsSpellHandler : SpellHandler
    {

        // This spell should be a buffer that absorbs 50% of the style damage
        // there is no cap to how much is absorbs, just simply the duration of the spell
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

            eChatType toLiving = Spell.Pulse == 0 ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
            eChatType toOther = Spell.Pulse == 0 ? eChatType.CT_System : eChatType.CT_SpellPulse;
            MessageToLiving(effect.Owner, Spell.Message1, toLiving);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return 0;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            // Log.DebugFormat("sender:{0} res:{1} IsMelee:{2} Type:{3}", living.Name, ad.AttackResult, ad.IsMeleeAttack, ad.AttackType);
            if (ad == null || ad.AttackResult != GameLiving.eAttackResult.HitStyle && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled)
            {
                return;
            }

            if (!ad.IsMeleeAttack && ad.AttackType != AttackData.eAttackType.Ranged)
            {
                return;
            }

            // default is 50%
            double absorbPercent = 50;

            // But if the buff has a specified spell damage, use that as the percent.
            if (Spell.Damage > 0)
            {
                absorbPercent = Spell.Damage;
            }

            if (absorbPercent > 100)
            {
                absorbPercent = 100;
            }

            // This only absorbs style damage.
            int damageAbsorbed = (int)(0.01 * absorbPercent * ad.StyleDamage);

            ad.Damage -= damageAbsorbed;

            OnDamageAbsorbed(ad, damageAbsorbed);

            // TODO correct messages
            MessageToLiving(ad.Target, $"Your style absorbtion absorbs {damageAbsorbed} damage!", eChatType.CT_Spell);
            MessageToLiving(ad.Attacker, $"A barrier absorbs {damageAbsorbed} damage of your attack!", eChatType.CT_Spell);
        }

        protected virtual void OnDamageAbsorbed(AttackData ad, int DamageAmount)
        {
        }

        public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
        {
            if ( // VaNaTiC-> this cannot work, cause PulsingSpellEffect is derived from object and only implements IConcEffect
                // e is PulsingSpellEffect ||
                // VaNaTiC<-
                Spell.Pulse != 0 || Spell.Concentration != 0 || e.RemainingTime < 1)
            {
                return null;
            }

            PlayerXEffect eff = new PlayerXEffect
            {
                Var1 = Spell.ID,
                Duration = e.RemainingTime,
                IsHandler = true,
                Var2 = (int) Spell.Value,
                SpellLine = SpellLine.KeyName
            };

            return eff;
        }

        public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
        {
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
        }

        public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return 0;
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    $"Name: {Spell.Name}",
                    string.Empty,
                    $"Description: {Spell.Description}",
                    string.Empty,
                    $"Target: {Spell.Target}",
                    "Type: Style Absorption"
                };

                if (Spell.Damage != 0)
                {
                    list.Add($"Absorption: {Spell.Damage}%");
                }

                if (Spell.Damage > 100)
                {
                    list.Add("Absorption: 100%");
                }

                if (Spell.Damage == 0)
                {
                    list.Add("Absorption: 50%");
                }

                // Value
                if (Spell.Value != 0)
                {
                    list.Add($"Value: {Spell.Value}");
                }

                // Cast
                if (Spell.CastTime < 0.1)
                {
                    list.Add("Casting time: Instant");
                }
                else if (Spell.CastTime > 0)
                {
                    list.Add($"Casting time: {(Spell.CastTime * 0.001):0.0## sec;-0.0## sec;'instant'}");
                }

                // Duration
                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min");
                }
                else if (Spell.Duration != 0)
                {

                    // Range
                    if (Spell.Range != 0)
                    {
                        list.Add($"Range: {Spell.Range}");
                    }
                }

                // Radius
                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                // Cost
                list.Add($"Power cost: {Spell.Power:0;0'%'}");

                // Frequency
                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                // DamageType
                if (Spell.DamageType != 0)
                {
                    list.Add($"Damage Type: {Spell.DamageType}");
                }

                return list;
            }
        }

        public StyleDmgAbsSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
