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
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.AI.Brain;
using System;
using log4net;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell to change up to 3 property bonuses at once
    /// in one their specific given bonus category
    /// </summary>

    public abstract class PropertyChangingSpell : SpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Execute property changing spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = Spell.Duration;
            if (HasPositiveEffect)
            {
                duration *= 1.0 + Caster.GetModified(eProperty.SpellDuration) * 0.01;
                if (Spell.InstrumentRequirement != 0)
                {
                    InventoryItem instrument = Caster.AttackWeapon;
                    if (instrument != null)
                    {
                        duration *= 1.0 + Math.Min(1.0, instrument.Level / (double)Caster.Level); // up to 200% duration for songs
                        duration *= instrument.Condition / (double)instrument.MaxCondition * instrument.Quality / 100;
                    }
                }

                if (duration < 1)
                {
                    duration = 1;
                }
                else if (duration > (Spell.Duration * 4))
                {
                    duration = Spell.Duration * 4;
                }

                return (int)duration;
            }

            duration = base.CalculateEffectDuration(target, effectiveness);
            return (int)duration;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // vampiir, they cannot be buffed except with resists/armor factor/ haste / power regen
            if (target is GamePlayer player)
            {
                if (HasPositiveEffect && player.CharacterClass.ID == (int)eCharacterClass.Vampiir && Caster != player)
                {
                    // restrictions
                    // if (this is PropertyChangingSpell
                    //    && this is ArmorFactorBuff == false
                    //    && this is CombatSpeedBuff == false
                    //    && this is AbstractResistBuff == false
                    //    && this is EnduranceRegenSpellHandler == false
                    //    && this is EvadeChanceBuff == false
                    //    && this is ParryChanceBuff == false)
                    // {
                    if (this is StrengthBuff || this is DexterityBuff || this is ConstitutionBuff || this is QuicknessBuff || this is StrengthConBuff || this is DexterityQuiBuff || this is AcuityBuff)
                    {
                        if (Caster is GamePlayer caster)
                        {
                            caster.Out.SendMessage("Your buff has no effect on the Vampiir!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }

                        player.Out.SendMessage("This buff has no effect on you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (this is ArmorFactorBuff)
                    {
                        if (FindEffectOnTarget(target, "ArmorFactorBuff") != null && SpellLine.IsBaseLine != true)
                        {
                            MessageToLiving(target, "You already have this effect!", eChatType.CT_SpellResisted);
                            return;
                        }
                    }
                }

                if (this is HeatColdMatterBuff || this is AllMagicResistsBuff)
                {
                    if (Spell.Frequency <= 0)
                    {
                        GameSpellEffect Matter = FindEffectOnTarget(player, "MatterResistBuff");
                        GameSpellEffect Cold = FindEffectOnTarget(player, "ColdResistBuff");
                        GameSpellEffect Heat = FindEffectOnTarget(player, "HeatResistBuff");
                        if (Matter != null || Cold != null || Heat != null)
                        {
                            MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
                            return;
                        }
                    }
                }

                if (this is BodySpiritEnergyBuff || this is AllMagicResistsBuff)
                {
                    if (Spell.Frequency <= 0)
                    {
                        GameSpellEffect Body = FindEffectOnTarget(player, "BodyResistBuff");
                        GameSpellEffect Spirit = FindEffectOnTarget(player, "SpiritResistBuff");
                        GameSpellEffect Energy = FindEffectOnTarget(player, "EnergyResistBuff");
                        if (Body != null || Spirit != null || Energy != null)
                        {
                            MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
                            return;
                        }
                    }
                }
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        /// <summary>
        /// start changing effect on target
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            ApplyBonus(effect.Owner , BonusCategory1, Property1, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner , BonusCategory2, Property2, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner , BonusCategory3, Property3, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner , BonusCategory4, Property4, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner , BonusCategory5, Property5, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner , BonusCategory6, Property6, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(Spell.Value * effect.Effectiveness), false);
            ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(Spell.Value * effect.Effectiveness), false);

            // Xali: buffs/debuffs are now efficient on pets

            if ((effect.Owner as GameNPC)?.Brain is ControlledNpcBrain)
            {
                // Increase Pet's ArmorAbsorb/MagicAbsorb with Buffs
                if (this is StrengthBuff)
                {
                    (effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeDamage] += (int)(((Spell.Value / 100) * Spell.Level) / 2);
                }
                else if (this is ConstitutionBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Body] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Energy] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Cold] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Heat] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Matter] += (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Spirit] += (int)(Spell.Value / 100 * Spell.Level / 4);
                }
                else if (this is ArmorFactorBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
                }
                else if (this is DexterityBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
                }
                else if (this is QuicknessBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeSpeed] += (int)(((Spell.Value / 100) * Spell.Level) / 6);
                }
                else if (this is StrengthConBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeDamage] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Body] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Energy] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Cold] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Heat] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Matter] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Spirit] += (int)(Spell.Value / 100 * Spell.Level / 8);
                }
                else if (this is DexterityQuiBuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeSpeed] += (int)(Spell.Value / 100 * Spell.Level / 6);
                }

                // Decrease Pet's ArmorAbsorb/MagicAbsorb with DeBuffs
                else if (this is StrengthDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeDamage] -= (int)(((Spell.Value / 100) * Spell.Level) / 2);
                }
                else if (this is ConstitutionDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Body] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Energy] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Cold] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Heat] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Matter] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Spirit] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                }
                else if (this is ArmorFactorDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                }
                else if (this is DexterityDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(Spell.Value / 100 * Spell.Level / 4);
                }
                else if (this is QuicknessDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeSpeed] -= (int)(Spell.Value / 100 * Spell.Level / 6);
                }
                else if (this is StrengthConDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeDamage] -= (int)((Spell.Value / 100 * Spell.Level) / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Body] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Energy] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Cold] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Heat] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Matter] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.Resist_Spirit] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                }
                else if (this is DexterityQuiDebuff)
                {
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(Spell.Value / 100 * Spell.Level / 8);
                    ((GameNPC) effect.Owner).AbilityBonus[(int)eProperty.MeleeSpeed] -= (int)(Spell.Value / 100 * Spell.Level / 6);
                }
            }

            SendUpdates(effect.Owner);

            eChatType toLiving = eChatType.CT_SpellPulse;
            eChatType toOther = eChatType.CT_SpellPulse;
            if (Spell.Pulse == 0 || !HasPositiveEffect)
            {
                toLiving = eChatType.CT_Spell;
                toOther = eChatType.CT_System;
                SendEffectAnimation(effect.Owner, 0, false, 1);
            }

            GameLiving player = null;

            if ((Caster as GameNPC)?.Brain is IControlledBrain)
            {
                player = (((GameNPC) Caster).Brain as IControlledBrain)?.Owner;
            }
            else if ((effect.Owner as GameNPC)?.Brain is IControlledBrain)
            {
                player = (((GameNPC) effect.Owner).Brain as IControlledBrain)?.Owner;
            }

            if (player != null)
            {
                // Controlled NPC. Show message in blue writing to owner...
                MessageToLiving(player, string.Format(Spell.Message2, effect.Owner.GetName(0, true)), toLiving);

                // ...and in white writing for everyone else.
                foreach (GamePlayer gamePlayer in effect.Owner.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (gamePlayer != player)
                    {
                        MessageToLiving(gamePlayer, string.Format(Spell.Message2, effect.Owner.GetName(0, true)), toOther);
                    }
                }
            }
            else
            {
                MessageToLiving(effect.Owner, Spell.Message1, toLiving);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
            }

            if (ServerProperties.Properties.BUFF_RANGE > 0 && effect.Spell.Concentration > 0 && effect.SpellHandler.HasPositiveEffect && effect.Owner != effect.SpellHandler.Caster)
            {
                m_buffCheckAction = new BuffCheckAction(effect.SpellHandler.Caster, effect.Owner, effect);
                m_buffCheckAction.Start(BuffCheckAction.BUFFCHECKINTERVAL);
            }
        }

        BuffCheckAction m_buffCheckAction;

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            ApplyBonus(effect.Owner , BonusCategory1, Property1, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner , BonusCategory2, Property2, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner , BonusCategory3, Property3, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner , BonusCategory4, Property4, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner , BonusCategory5, Property5, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner , BonusCategory6, Property6, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(Spell.Value * effect.Effectiveness), true);
            ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(Spell.Value * effect.Effectiveness), true);

            SendUpdates(effect.Owner);

            if (m_buffCheckAction != null)
            {
                m_buffCheckAction.Stop();
                m_buffCheckAction = null;
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        protected virtual void SendUpdates(GameLiving target)
        {
        }

        protected IPropertyIndexer GetBonusCategory(GameLiving target, eBuffBonusCategory categoryid)
        {
            IPropertyIndexer bonuscat = null;
            switch (categoryid)
            {
                case eBuffBonusCategory.BaseBuff:
                    bonuscat = target.BaseBuffBonusCategory;
                    break;
                case eBuffBonusCategory.SpecBuff:
                    bonuscat = target.SpecBuffBonusCategory;
                    break;
                case eBuffBonusCategory.Debuff:
                    bonuscat = target.DebuffCategory;
                    break;
                case eBuffBonusCategory.Other:
                    bonuscat = target.BuffBonusCategory4;
                    break;
                case eBuffBonusCategory.SpecDebuff:
                    bonuscat = target.SpecDebuffCategory;
                    break;
                case eBuffBonusCategory.AbilityBuff:
                    bonuscat = target.AbilityBonus;
                    break;
                default:
                    if (log.IsErrorEnabled)
                    {
                        log.Error($"BonusCategory not found {categoryid}!");
                    }

                    break;
            }

            return bonuscat;
        }

        /// <summary>
        /// Property 1 which bonus value has to be changed
        /// </summary>
        public abstract eProperty Property1 { get; }

        /// <summary>
        /// Property 2 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property2 => eProperty.Undefined;

        /// <summary>
        /// Property 3 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property3 => eProperty.Undefined;

        /// <summary>
        /// Property 4 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property4 => eProperty.Undefined;

        /// <summary>
        /// Property 5 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property5 => eProperty.Undefined;

        /// <summary>
        /// Property 6 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property6 => eProperty.Undefined;

        /// <summary>
        /// Property 7 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property7 => eProperty.Undefined;

        /// <summary>
        /// Property 8 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property8 => eProperty.Undefined;

        /// <summary>
        /// Property 9 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property9 => eProperty.Undefined;

        /// <summary>
        /// Property 10 which bonus value has to be changed
        /// </summary>
        public virtual eProperty Property10 => eProperty.Undefined;

        /// <summary>
        /// Bonus Category where to change the Property1
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory1 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property2
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory2 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property3
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory3 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property4
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory4 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property5
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory5 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property6
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory6 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property7
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory7 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property8
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory8 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property9
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory9 => eBuffBonusCategory.BaseBuff;

        /// <summary>
        /// Bonus Category where to change the Property10
        /// </summary>
        public virtual eBuffBonusCategory BonusCategory10 => eBuffBonusCategory.BaseBuff;

        public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
        {
            ApplyBonus(effect.Owner, BonusCategory1,Property1, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory2,Property2, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory3,Property3, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory4,Property4, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory5,Property5, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory6,Property6, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory7, Property7, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory8, Property8, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory9, Property9, vars[1], false);
            ApplyBonus(effect.Owner, BonusCategory10, Property10, vars[1], false);

            SendUpdates(effect.Owner);
        }

        public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
        {
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            ApplyBonus(effect.Owner, BonusCategory1,Property1, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory2,Property2, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory3,Property3, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory4,Property4, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory5,Property5, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory6,Property6, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory7, Property7, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory8, Property8, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory9, Property9, vars[1], true);
            ApplyBonus(effect.Owner, BonusCategory10, Property10, vars[1], true);

            SendUpdates(effect.Owner);
            return 0;
        }

        /// <summary>
        /// Method used to apply bonuses
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="BonusCat"></param>
        /// <param name="Property"></param>
        /// <param name="Value"></param>
        /// <param name="IsSubstracted"></param>
        protected void ApplyBonus(GameLiving owner,  eBuffBonusCategory BonusCat, eProperty Property, int Value, bool IsSubstracted)
        {
            if (Property != eProperty.Undefined)
            {
                var tblBonusCat = GetBonusCategory(owner, BonusCat);
                if (IsSubstracted)
                {
                    tblBonusCat[(int)Property] -= Value;
                }
                else
                {
                    tblBonusCat[(int)Property] += Value;
                }
            }
        }

        public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
        {
            PlayerXEffect eff = new PlayerXEffect();
            eff.Var1 = Spell.ID;
            eff.Duration = e.RemainingTime;
            eff.IsHandler = true;
            eff.Var2 = (int)(Spell.Value * e.Effectiveness);
            eff.SpellLine = SpellLine.KeyName;
            return eff;
        }

        // constructor
        public PropertyChangingSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
        {
        }
    }

    public class BuffCheckAction : RegionAction
    {
        public const int BUFFCHECKINTERVAL = 60000;// 60 seconds

        private readonly GameLiving m_caster;
        private readonly GameLiving m_owner;
        private readonly GameSpellEffect m_effect;

        public BuffCheckAction(GameLiving caster, GameLiving owner, GameSpellEffect effect)
            : base(caster)
        {
            m_caster = caster;
            m_owner = owner;
            m_effect = effect;
        }

        /// <summary>
        /// Called on every timer tick
        /// </summary>
        protected override void OnTick()
        {
            if (m_caster == null ||
                m_owner == null ||
                m_effect == null)
            {
                return;
            }

            if (!m_caster.IsWithinRadius(m_owner, ServerProperties.Properties.BUFF_RANGE))
            {
                m_effect.Cancel(false);
            }
            else
            {
                Start(BUFFCHECKINTERVAL);
            }
        }
    }
}
