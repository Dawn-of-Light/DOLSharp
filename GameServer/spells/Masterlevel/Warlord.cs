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
using System.Reflection;
using log4net;
using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.Events;
using System.Collections.Specialized;

namespace DOL.GS.Spells
{
    #region Warlord-1
    //Gamesiegeweapon - getactiondelay
    #endregion

    #region Warlord-2/8
    [SpellHandlerAttribute("PBAEHeal")]
    public class PBAEHealHandler : MasterlevelHandling
    {
        public override void FinishSpellCast(GameLiving target)
        {
            switch (Spell.DamageType)
            {
                case (eDamageType)((byte)1):
                    {
                        int value = (int)Spell.Value;
                        int life;
                        life = (m_caster.Health * value) / 100;
                        m_caster.Health -= life;
                    }
                    break;
            }
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            GamePlayer player = target as GamePlayer;

            if (target is GamePlayer)
            {
                switch (Spell.DamageType)
                {
                    //Warlord ML 2
                    case (eDamageType)((byte)0):
                        {
                            int mana;
                            int health;
                            int end;
                            int value = (int)Spell.Value;
                            mana = (target.MaxMana * value) / 100;
                            end = (target.MaxEndurance * value) / 100;
                            health = (target.MaxHealth * value) / 100;

                            if (target.Health + health > target.MaxHealth)
                                target.Health = target.MaxHealth;
                            else
                                target.Health += health;

                            if (target.Mana + mana > target.MaxMana)
                                target.Mana = target.MaxMana;
                            else
                                target.Mana += mana;

                            if (target.Endurance + end > target.MaxEndurance)
                                target.Endurance = target.MaxEndurance;
                            else
                                target.Endurance += end;

                            SendEffectAnimation(target, 0, false, 1);
                        }
                        break;
                    //warlord ML8
                    case (eDamageType)((byte)1):
                        {
                            int value = (int)Spell.Value;
                            int health;
                            health = (player.MaxHealth * value) / 100;
                            if (player.Health + health > player.MaxHealth)
                                player.Health = player.MaxHealth;
                            else
                                player.Health += health;

                            if (player != null)
                            {
                                player.Out.SendCharStatsUpdate();
                                player.UpdatePlayerStatus();
                                player.Out.SendCharResistsUpdate();
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
    #endregion

    #region Warlord-3
    [SpellHandlerAttribute("CoweringBellow")]
    public class CoweringBellowSpellHandler : FearSpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList();
            GameLiving target = Caster;
            foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius))
            {
                if (npc is GameNPC && npc.Brain is ControlledNpc)//!(npc is NecromancerPet))
                    list.Add(npc);
            }
            return list;
        }

        public CoweringBellowSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Warlord-5
    [SpellHandlerAttribute("Critical")]
    public class CriticalDamageBuff : MasterlevelDualBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.CriticalSpellHitChance; } }
        public override eProperty Property2 { get { return eProperty.CriticalMeleeHitChance; } }

        public CriticalDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion  
        
    #region Warlord-7
    [SpellHandlerAttribute("CleansingAura")]
    public class CleansingAurauraSpellHandler : SpellHandler
    {
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return true;
        }

        public CleansingAurauraSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Warlord-9
    [SpellHandlerAttribute("EffectivenessBuff")]
    public class EffectivenessBuff : MasterlevelHandling
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.PlayerEffectiveness += Spell.Value * 0.01;
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
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.PlayerEffectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
            return 0;
        }

        public EffectivenessBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Warlord-10
    [SpellHandlerAttribute("MLABSBuff")]
    public class MLABSBuff : MasterlevelBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.ToHitBonus; } }

        public MLABSBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}
