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
using DOL.GS.RealmAbilities;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    using Effects;

    /// <summary>
    ///
    /// </summary>
    [SpellHandler("Heal")]
    public class HealSpellHandler : SpellHandler
    {
        // constructor
        public HealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        /// <summary>
        /// Execute heal spell
        /// </summary>
        /// <param name="target"></param>
        public override bool StartSpell(GameLiving target)
        {
            var targets = SelectTargets(target);
            if (targets.Count <= 0)
            {
                return false;
            }

            bool healed = false;

            CalculateHealVariance(out var minHeal, out var maxHeal);

            foreach (GameLiving healTarget in targets)
            {
                int heal = Util.Random(minHeal, maxHeal);

                if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
                {
                    heal = maxHeal;
                }

                if (healTarget.IsDiseased)
                {
                    MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
                    heal >>= 1;
                }

                if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
                {
                    healed |= ProcHeal(healTarget, heal);
                }
                else
                {
                    healed |= HealTarget(healTarget, heal);
                }
            }

            // group heals seem to use full power even if no heals
            if (!healed && Spell.Target.ToLower() == "realm")
            {
                Caster.Mana -= PowerCost(target) >> 1; // only 1/2 power if no heal
            }
            else
            {
                Caster.Mana -= PowerCost(target);
            }

            // send animation for non pulsing spells only
            if (Spell.Pulse == 0)
            {
                // show resisted effect if not healed
                foreach (GameLiving healTarget in targets)
                {
                    if (healTarget.IsAlive)
                    {
                        SendEffectAnimation(healTarget, 0, false, healed ? (byte)1 : (byte)0);
                    }
                }
            }

            if (!healed && Spell.CastTime == 0)
            {
                m_startReuseTimer = false;
            }

            return true;
        }

        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>true if heal was done</returns>
        public virtual bool HealTarget(GameLiving target, int amount)
        {
            return HealTarget(target, (double)amount);
        }

        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>true if heal was done</returns>
        public virtual bool HealTarget(GameLiving target, double amount)
        {
            if (target == null || target.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            // we can't heal people we can attack
            if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                return false;
            }

            // no healing of keep components
            if (target is Keeps.GameKeepComponent || target is Keeps.GameKeepDoor)
            {
                return false;
            }

            if (!target.IsAlive)
            {
                // "You cannot heal the dead!" sshot550.tga
                MessageToCaster($"{target.GetName(0, true)} is dead!", eChatType.CT_SpellResisted);
                return false;
            }

            if (target is GamePlayer gamePlayer && gamePlayer.NoHelp && Caster is GamePlayer)
            {
                // player not grouped, anyone else
                // player grouped, different group
                if (gamePlayer.Group == null ||
                    ((GamePlayer) Caster).Group == null ||
                    ((GamePlayer) Caster).Group != gamePlayer.Group)
                {
                    MessageToCaster("That player does not want assistance", eChatType.CT_SpellResisted);
                    return false;
                }
            }

            // moc heal decrease
            double mocFactor = 1.0;
            MasteryofConcentrationEffect moc = Caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
            if (moc != null)
            {
                if (Caster is GamePlayer playerCaster)
                {
                    MasteryofConcentrationAbility ra = playerCaster.GetAbility<MasteryofConcentrationAbility>();
                    if (ra != null)
                    {
                        mocFactor = ra.GetAmountForLevel(ra.Level) / 100.0;
                    }
                }

                amount = amount * mocFactor;
            }

            double criticalvalue = 0;
            int criticalchance = Caster.GetModified(eProperty.CriticalHealHitChance);
            double effectiveness = 0;
            if (Caster is GamePlayer player1)
            {
                effectiveness = player1.Effectiveness + Caster.GetModified(eProperty.HealingEffectiveness) * 0.01;
            }

            if (Caster is GameNPC)
            {
                effectiveness = 1.0;
            }

            // USE DOUBLE !
            double cache = amount * effectiveness;

            amount = cache;

            if (Util.Chance(criticalchance))
            {
                double minValue = amount / 10;
                double maxValue = amount / 2 + 1;
                criticalvalue = Util.RandomDouble() * (maxValue - minValue) + minValue;
            }

            amount += criticalvalue;

            if (target is GamePlayer playerTarget)
            {
                GameSpellEffect HealEffect = FindEffectOnTarget(playerTarget, "EfficientHealing");
                if (HealEffect != null)
                {
                    double HealBonus = amount * ((int)HealEffect.Spell.Value * 0.01);
                    amount += (int)HealBonus;
                    playerTarget.Out.SendMessage($"Your Efficient Healing buff grants you a additional{HealBonus} in the Heal!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
                }

                GameSpellEffect EndEffect = FindEffectOnTarget(playerTarget, "EfficientEndurance");
                if (EndEffect != null)
                {
                    double EndBonus = amount * ((int)EndEffect.Spell.Value * 0.01);

                    // 600 / 10 = 60end
                    playerTarget.Endurance += (int)EndBonus;
                    playerTarget.Out.SendMessage($"Your Efficient Endurance buff grants you {EndBonus} Endurance from the Heal!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
                }
            }

            GameSpellEffect flaskHeal = FindEffectOnTarget(target, "HealFlask");
            if (flaskHeal != null)
            {
                amount += (int)((amount * flaskHeal.Spell.Value) * 0.01);
            }

            // Scale spells that are cast by pets
            if (Caster is GamePet && !(Caster is NecromancerPet) && ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
            {
                amount = ScalePetHeal(amount);
            }

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, (int)Math.Round(amount));

            long healedrp = 0;

            if (target.DamageRvRMemory > 0 && (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer))
            {
                healedrp = Math.Max(heal, 0);
                target.DamageRvRMemory -= healedrp;
            }

            if (heal == 0)
            {
                if (Spell.Pulse == 0)
                {
                    if (target == Caster)
                    {
                        MessageToCaster("You are fully healed.", eChatType.CT_SpellResisted);
                    }
                    else
                    {
                        MessageToCaster($"{target.GetName(0, true)} is fully healed.", eChatType.CT_SpellResisted);
                    }
                }

                return false;
            }

            if (Caster is GamePlayer && ((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer && healedrp > 0)
            {
                int POURCENTAGE_SOIN_RP = ServerProperties.Properties.HEAL_PVP_DAMAGE_VALUE_RP; // ...% de bonus RP pour les soins effectués

                if (Spell.Pulse == 0 && Caster.CurrentRegionID != 242 && // On Exclu zone COOP
                    Spell.SpellType.ToLower() != "spreadheal" && target != Caster &&
                    SpellLine.KeyName != GlobalSpellsLines.Item_Spells &&
                    SpellLine.KeyName != GlobalSpellsLines.Potions_Effects &&
                    SpellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect &&
                    SpellLine.KeyName != GlobalSpellsLines.Reserved_Spells)
                {
                    if (Caster is GamePlayer player)
                    {
                        long Bonus_RP_Soin = Convert.ToInt64((double)healedrp * POURCENTAGE_SOIN_RP / 100.0);

                        if (Bonus_RP_Soin >= 1)
                        {
                            if (player.Statistics is PlayerStatistics stats)
                            {
                                stats.RPEarnedFromHitPointsHealed += (uint)Bonus_RP_Soin;
                                stats.HitPointsHealed += (uint)healedrp;
                            }

                            player.GainRealmPoints(Bonus_RP_Soin, false);
                            player.Out.SendMessage($"You earn {Bonus_RP_Soin} Realm Points for healing a member of your realm.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                    }
                }
            }

            if (Caster == target)
            {
                MessageToCaster($"You heal yourself for {heal} hit points.", eChatType.CT_Spell);
                if (heal < amount)
                {
                    if (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                        {
                            target.DamageRvRMemory = 0; // Remise a zéro compteur dommages/heal rps
                        }
                    }

                    MessageToCaster("You are fully healed.", eChatType.CT_Spell);
                }
            }
            else
            {
                MessageToCaster($"You heal {target.GetName(0, false)} for {heal} hit points!", eChatType.CT_Spell);
                MessageToLiving(target, $"You are healed by {Caster.GetName(0, false)} for {heal} hit points.", eChatType.CT_Spell);
                if (heal < amount)
                {
                    if (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                        {
                            target.DamageRvRMemory = 0; // Remise a zéro compteur dommages/heal rps
                        }
                    }

                    MessageToCaster($"{target.GetName(0, true)} is fully healed.", eChatType.CT_Spell);
                }

                if (heal > 0 && criticalvalue > 0)
                {
                    MessageToCaster($"Your heal criticals for an extra {criticalvalue} amount of hit points!", eChatType.CT_Spell);
                }
            }

            return true;
        }

        /// <summary>
        /// A heal generated by an item proc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool ProcHeal(GameLiving target, int amount)
        {
            return ProcHeal(target, (double)amount);
        }

        /// <summary>
        /// A heal generated by an item proc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool ProcHeal(GameLiving target, double amount)
        {
            if (target == null || target.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            // we can't heal people we can attack
            if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                return false;
            }

            if (!target.IsAlive)
            {
                return false;
            }

            // no healing of keep components
            if (target is Keeps.GameKeepComponent || target is Keeps.GameKeepDoor)
            {
                return false;
            }

            // Scale spells that are cast by pets
            if (Caster is GamePet && !(Caster is NecromancerPet) && ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
            {
                amount = ScalePetHeal(amount);
            }

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, (int)Math.Round(amount));

            if (Caster == target && heal > 0)
            {
                MessageToCaster($"You heal yourself for {heal} hit points.", eChatType.CT_Spell);

                if (heal < amount)
                {
                    MessageToCaster("You are fully healed.", eChatType.CT_Spell);

                    if (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                        {
                            target.DamageRvRMemory = 0; // Remise a zéro compteur dommages/heal rps
                        }
                    }
                }
            }
            else if (heal > 0)
            {
                MessageToCaster($"You heal {target.GetName(0, false)} for {heal} hit points!", eChatType.CT_Spell);
                MessageToLiving(target, $"You are healed by {Caster.GetName(0, false)} for {heal} hit points.", eChatType.CT_Spell);

                if (heal < amount)
                {
                    if (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                        {
                            target.DamageRvRMemory = 0; // Remise a zéro compteur dommages/heal rps
                        }
                    }
                }
                else
                {
                    if (((target as NecromancerPet)?.Brain as IControlledBrain)?.GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                        {
                            target.DamageRvRMemory -= Math.Max(heal, 0);
                        }
                    }
                }
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
            double spellValue = Spell.Value;

            if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
            {
                if (Spell.Value > 0)
                {
                    min = (int)(spellValue * 0.75);
                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Potions_Effects)
            {
                if (Spell.Value > 0)
                {
                    min = (int)(spellValue * 1.00);
                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
            {
                if (Spell.Value > 0)
                {
                    if (UseMinVariance)
                    {
                        min = (int)(spellValue * 1.25);
                    }
                    else
                    {
                        min = (int)(spellValue * 0.75);
                    }

                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
            {
                min = max = (int)spellValue;
                return;
            }

            // percents if less than zero
            if (spellValue < 0)
            {
                spellValue = spellValue / -100.0 * Caster.MaxHealth;

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
                double lineSpec = Caster.GetModifiedSpecLevel(SpellLine.Spec);
                if (lineSpec < 1)
                {
                    lineSpec = 1;
                }

                eff = 0.25;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level;
                    if (eff > 1.25)
                    {
                        eff = 1.25;
                    }
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
        }

        private double ScalePetHeal(double amount)
        {
            return amount * Caster.Level / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL;
        }
    }
}
