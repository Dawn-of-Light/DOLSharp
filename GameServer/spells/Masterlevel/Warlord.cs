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
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    // http://www.camelotherald.com/masterlevels/ma.php?ml=Warlord
    // Gamesiegeweapon - getactiondelay

    // shared timer 1 for 2 - shared timer 4 for 8
    [SpellHandler("PBAEHeal")]
    public class PBAEHealHandler : MasterlevelHandling
    {
        public override void FinishSpellCast(GameLiving target)
        {
            switch (Spell.DamageType)
            {
                case (eDamageType)1:
                    {
                        int value = (int)Spell.Value;
                        var life = Caster.Health * value / 100;
                        Caster.Health -= life;
                    }

                    break;
            }

            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

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

            GamePlayer player = target as GamePlayer;

            if (target is GamePlayer)
            {
                switch (Spell.DamageType)
                {
                    // Warlord ML 2
                    case 0:
                        {
                            int value = (int)Spell.Value;
                            var mana = target.MaxMana * value / 100;
                            var end = target.MaxEndurance * value / 100;
                            var health = target.MaxHealth * value / 100;

                            if (target.Health + health > target.MaxHealth)
                            {
                                target.Health = target.MaxHealth;
                            }
                            else
                            {
                                target.Health += health;
                            }

                            if (target.Mana + mana > target.MaxMana)
                            {
                                target.Mana = target.MaxMana;
                            }
                            else
                            {
                                target.Mana += mana;
                            }

                            if (target.Endurance + end > target.MaxEndurance)
                            {
                                target.Endurance = target.MaxEndurance;
                            }
                            else
                            {
                                target.Endurance += end;
                            }

                            SendEffectAnimation(target, 0, false, 1);
                        }

                        break;

                    // warlord ML8
                    case (eDamageType)1:
                        {
                            int healvalue = (int)Spell.Value;
                            int heal;
                                if (target.IsAlive && !GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                                {
                                    heal = target.ChangeHealth(target, GameLiving.eHealthChangeType.Spell, healvalue);
                                    if (heal != 0)
                                {
                                    player.Out.SendMessage($"{Caster.Name} heal you for {heal} hit point!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                                }
                            }

                            heal = Caster.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, (int)(-Caster.Health * 90 / 100));
                            if (heal != 0)
                            {
                                MessageToCaster($"You lose {heal} hit point{(heal == 1 ? "." : "s.")}", eChatType.CT_Spell);
                            }

                            SendEffectAnimation(target, 0, false, 1);
                        }

                        break;
                }
            }
        }

        // constructor
        public PBAEHealHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 2
    [SpellHandler("CoweringBellow")]
    public class CoweringBellowSpellHandler : FearSpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override IList<GameLiving> SelectTargets(GameObject castTarget)
        {
            var list = new List<GameLiving>();
            GameLiving target = Caster;
            foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius))
            {
                if (npc?.Brain is ControlledNpcBrain) // !(npc is NecromancerPet))
                {
                    list.Add(npc);
                }
            }

            return list;
        }

        public CoweringBellowSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // ML4~     //shared timer 3

    // shared timer 3
    [SpellHandler("Critical")]
    public class CriticalDamageBuff : MasterlevelDualBuffHandling
    {
        public override eProperty Property1 => eProperty.CriticalSpellHitChance;

        public override eProperty Property2 => eProperty.CriticalMeleeHitChance;

        public CriticalDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // ML6~     //shared timer 4

    // shared timer 3
    [SpellHandler("CleansingAura")]
    public class CleansingAurauraSpellHandler : SpellHandler
    {
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return true;
        }

        public CleansingAurauraSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 5
    [SpellHandler("EffectivenessBuff")]
    public class EffectivenessBuff : MasterlevelHandling
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override bool HasPositiveEffect => true;

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (effect.Owner is GamePlayer player)
            {
                player.Effectiveness += Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
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
            if (effect.Owner is GamePlayer player)
            {
                player.Effectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }

            return 0;
        }

        public EffectivenessBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 5
    [SpellHandler("MLABSBuff")]
    public class MLABSBuff : MasterlevelBuffHandling
    {
        public override eProperty Property1 => eProperty.ArmorAbsorption;

        public MLABSBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
