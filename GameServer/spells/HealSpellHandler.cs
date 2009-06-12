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
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("Heal")]
    public class HealSpellHandler : SpellHandler
    {
        // constructor
        public HealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        /// <summary>
        /// Execute heal spell
        /// </summary>
        /// <param name="target"></param>
        public override void StartSpell(GameLiving target)
        {
            IList targets = SelectTargets(target);
            if (targets.Count <= 0) return;

            bool healed = false;
            int minHeal;
            int maxHeal;
            CalculateHealVariance(out minHeal, out maxHeal);

            foreach (GameLiving healTarget in targets)
            {
                int heal = Util.Random(minHeal, maxHeal);
                if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
                    heal = maxHeal;
                if (healTarget.IsDiseased)
                {
                    MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
                    heal >>= 1;
                }
                healed |= HealTarget(healTarget, heal);
            }

            // group heals seem to use full power even if no heals
            if (!healed && Spell.Target.ToLower() == "realm")
                m_caster.Mana -= PowerCost(target) >> 1; // only 1/2 power if no heal
            else
                m_caster.Mana -= PowerCost(target);

            // send animation for non pulsing spells only
            if (Spell.Pulse == 0)
            {
                // show resisted effect if not healed
				foreach (GameLiving healTarget in targets)
					if(healTarget.IsAlive)
						SendEffectAnimation(healTarget, 0, false, healed ? (byte)1 : (byte)0);
            }

            if (!healed && Spell.CastTime == 0) m_startReuseTimer = false;
        }

        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>true if heal was done</returns>
        public virtual bool HealTarget(GameLiving target, int amount)
        {
            if (target == null || target.ObjectState != GameLiving.eObjectState.Active) return false;

            if (!target.IsAlive)
            {
                //"You cannot heal the dead!" sshot550.tga
                MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
                return false;
            }

            if (target is GamePlayer && (target as GamePlayer).NoHelp && Caster is GamePlayer)
            {
                //player not grouped, anyone else
                //player grouped, different group
                if ((target as GamePlayer).Group == null ||
                    (Caster as GamePlayer).Group == null ||
                    (Caster as GamePlayer).Group != (target as GamePlayer).Group)
                {
                    MessageToCaster("That player does not want assistance", eChatType.CT_SpellResisted);
                    return false;
                }
            }

            amount = (int)(amount * 1.00);
            //moc heal decrease
            double mocFactor = 1.0;
            Effects.MasteryofConcentrationEffect moc = (Effects.MasteryofConcentrationEffect)Caster.EffectList.GetOfType(typeof(Effects.MasteryofConcentrationEffect));
            if (moc != null)
            {
                GamePlayer playerCaster = Caster as GamePlayer;
                RealmAbility ra = playerCaster.GetAbility(typeof(MasteryofConcentrationAbility)) as RealmAbility;
                if (ra != null)
                    mocFactor = System.Math.Round((double)ra.Level * 25 / 100, 2);
                amount = (int)Math.Round(amount * mocFactor);
            }

            int criticalvalue = 0;
            int criticalchance = Caster.GetModified(eProperty.CriticalHealHitChance);
            double effectiveness = 0;
            if (Caster is GamePlayer)
                effectiveness = ((GamePlayer)Caster).Effectiveness + Caster.GetModified(eProperty.HealingEffectiveness) * 0.01;
            if (Caster is GameNPC)
                effectiveness = 1.0;

            //USE DOUBLE !
            double cache = (double)amount * effectiveness;

            amount = (int)cache;

            if (Util.Chance(criticalchance))
                criticalvalue = Util.Random(amount / 10, amount / 2 + 1);

            amount += criticalvalue;

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, amount);

            if (heal == 0)
            {
                if (Spell.Pulse == 0)
                {
                    if (target == m_caster) 
                        MessageToCaster("You are fully healed.", eChatType.CT_SpellResisted);
                    else 
                        MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_SpellResisted);
                }
                return false;
            }

            if (m_caster == target)
            {
                MessageToCaster("You heal yourself for " + heal + " hit points.", eChatType.CT_Spell);
                if (heal < amount)
                    MessageToCaster("You are fully healed.", eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " hit points!", eChatType.CT_Spell);
                MessageToLiving(target, "You are healed by " + m_caster.GetName(0, false) + " for " + heal + " hit points.", eChatType.CT_Spell);
                if (heal < amount)
                    MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_Spell);
                if (heal > 0 && criticalvalue > 0)
                    MessageToCaster("Your heal criticals for an extra " + criticalvalue + " amount of hit points!", eChatType.CT_Spell);
            }

            return true;
        }

        /// <summary>
        /// Calculates heal variance based on spec
        /// </summary>
        /// <param name="min">store min variance here</param>
        /// <param name="max">store max variance here</param>
        public virtual void CalculateHealVariance(out int min, out int max)
        {
            double spellValue = m_spell.Value;
            GamePlayer casterPlayer = m_caster as GamePlayer;

            // percents if less than zero
            if (spellValue < 0)
            {
                if (casterPlayer != null)
                    spellValue = (spellValue / -100.0) * casterPlayer.CalculateMaxHealth(casterPlayer.Level, casterPlayer.GetBaseStat(eStat.CON));
                else
                    spellValue = (spellValue / -100.0) * m_caster.MaxHealth;

                min = max = (int)spellValue;
                return;
            }

            int upperLimit = (int)(spellValue * 1.25);
            if (upperLimit < 1)
            {
                upperLimit = 1;
            }

            double eff = 1.25;
            if (Caster is GamePlayer)
            {
                double lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
                if (lineSpec < 1)
                    lineSpec = 1;
                eff = 0.25;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level;
                    if (eff > 1.25)
                        eff = 1.25;
                }
            }

            int lowerLimit = (int)(spellValue * eff);
            if (lowerLimit < 1)
            {
                lowerLimit = 1;
            }
            if (lowerLimit > upperLimit)
            {
                lowerLimit = upperLimit;
            }

            min = lowerLimit;
            max = upperLimit;
            return;
        }
    }
}
