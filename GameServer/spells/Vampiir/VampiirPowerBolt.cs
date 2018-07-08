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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandler("VampiirBolt")]
    public class VampiirBoltSpellHandler : SpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.InCombat)
            {
                MessageToCaster("You cannot cast this spell in combat!", eChatType.CT_SpellResisted);
                return false;
            }

            return base.CheckBeginCast(selectedTarget);
        }

        public override bool StartSpell(GameLiving target)
        {
            foreach (GameLiving targ in SelectTargets(target))
            {
                DealDamage(targ);
            }

            return true;
        }

        private void DealDamage(GameLiving target)
        {
            int ticksToTarget = Caster.GetDistanceTo(target) * 100 / 85; // 85 units per 1/10s
            int delay = 1 + ticksToTarget / 100;
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(Caster, target, Spell.ClientEffect, (ushort)delay, false, 1);
            }

            BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
            bolt.Start(1 + ticksToTarget);
        }

        public override void FinishSpellCast(GameLiving target)
        {
            if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
            {
                MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
                return;
            }

            base.FinishSpellCast(target);
        }

        protected class BoltOnTargetAction : RegionAction
        {
            protected readonly GameLiving m_boltTarget;
            protected readonly VampiirBoltSpellHandler m_handler;

            public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, VampiirBoltSpellHandler spellHandler)
                : base(actionSource)
            {
                m_boltTarget = boltTarget ?? throw new ArgumentNullException(nameof(boltTarget));
                m_handler = spellHandler ?? throw new ArgumentNullException(nameof(spellHandler));
            }

            protected override void OnTick()
            {
                GameLiving target = m_boltTarget;
                GameLiving caster = (GameLiving)m_actionSource;
                if (target == null || target.CurrentRegionID != caster.CurrentRegionID || target.ObjectState != GameObject.eObjectState.Active || !target.IsAlive)
                {
                    return;
                }

                if (target is GameNPC || target.Mana > 0)
                {
                    int power;
                    if (target is GameNPC)
                    {
                        power = (int)Math.Round((target.Level * m_handler.Spell.Value * 2) / 100);
                    }
                    else
                    {
                        power = (int)Math.Round(target.MaxMana * (m_handler.Spell.Value / 250));
                    }

                    if (target.Mana < power)
                    {
                        power = target.Mana;
                    }

                    caster.Mana += power;

                    if (target is GamePlayer gamePlayer)
                    {
                        target.Mana -= power;
                        gamePlayer.Out.SendMessage($"{caster.Name} takes {power} power!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                    }

                    if (caster is GamePlayer player)
                    {
                        player.Out.SendMessage($"You receive {power} power from {target.Name}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
                else
                {
                    ((GamePlayer)caster).Out.SendMessage($"You did not receive any power from {target.Name}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                // Place the caster in combat
                if (target is GamePlayer)
                {
                    caster.LastAttackTickPvP = caster.CurrentRegion.Time;
                }
                else
                {
                    caster.LastAttackTickPvE = caster.CurrentRegion.Time;
                }

                // create the attack data for the bolt
                AttackData ad = new AttackData
                {
                    Attacker = caster,
                    Target = target,
                    DamageType = eDamageType.Heat,
                    AttackType = AttackData.eAttackType.Spell,
                    AttackResult = GameLiving.eAttackResult.HitUnstyled,
                    SpellHandler = m_handler
                };

                target.OnAttackedByEnemy(ad);

                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, caster);
            }
        }

        public VampiirBoltSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}