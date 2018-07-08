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
using System.Collections.Generic;
using System.Linq;

using System.Text;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using DOL.Language;

using log4net;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Default class for spell handler
    /// should be used as a base class for spell handler
    /// </summary>
    public class SpellHandler : ISpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Maximum number of sub-spells to get delve info for.
        /// </summary>
        protected static readonly byte MAX_DELVE_RECURSION = 5;

        protected DelayedCastTimer m_castTimer;

        /// <summary>
        /// The target for this spell
        /// </summary>
        protected GameLiving m_spellTarget;
        /// <summary>
        /// Has the spell been interrupted
        /// </summary>
        protected bool m_interrupted;
        /// <summary>
        /// Delayedcast Stage
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// Use to store Time when the delayedcast started
        /// </summary>
        protected long m_started;
        /// <summary>
        /// Shall we start the reuse timer
        /// </summary>
        protected bool m_startReuseTimer = true;

        public bool StartReuseTimer => m_startReuseTimer;

        /// <summary>
        /// Can this spell be queued with other spells?
        /// </summary>
        public virtual bool CanQueue => true;

        /// <summary>
        /// Does this spell break stealth on start of cast?
        /// </summary>
        public virtual bool UnstealthCasterOnStart => true;

        /// <summary>
        /// Does this spell break stealth on Finish of cast?
        /// </summary>
        public virtual bool UnstealthCasterOnFinish => true;

        protected InventoryItem m_spellItem;

        /// <summary>
        /// AttackData result for this spell, if any
        /// </summary>
        protected AttackData m_lastAttackData;
        /// <summary>
        /// AttackData result for this spell, if any
        /// </summary>
        public AttackData LastAttackData => m_lastAttackData;

        /// <summary>
        /// The property key for the interrupt timeout
        /// </summary>
        public const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";
        /// <summary>
        /// The property key for focus spells
        /// </summary>
        protected const string FOCUS_SPELL = "FOCUSING_A_SPELL";

        /// <summary>
        /// Does this spell ignore any damage cap?
        /// </summary>
        public bool IgnoreDamageCap { get; set; }

        /// <summary>
        /// Should this spell use the minimum variance for the type?
        /// Followup style effects, for example, always use the minimum variance
        /// </summary>
        public bool UseMinVariance { get; set; } = false;

        /// <summary>
        /// Can this SpellHandler Coexist with other Overwritable Spell Effect
        /// </summary>
        public virtual bool AllowCoexisting => Spell.AllowCoexisting;

        /// <summary>
        /// The CastingCompleteEvent
        /// </summary>
        public event CastingCompleteCallback CastingCompleteEvent;

        // public event SpellEndsCallback SpellEndsEvent;

        /// <summary>
        /// spell handler constructor
        /// <param name="caster">living that is casting that spell</param>
        /// <param name="spell">the spell to cast</param>
        /// <param name="spellLine">the spell line that spell belongs to</param>
        /// </summary>
        public SpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
        {
            Caster = caster;
            Spell = spell;
            SpellLine = spellLine;
        }

        /// <summary>
        /// Returns the string representation of the SpellHandler
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(128)
                .Append("Caster=").Append(Caster == null ? "(null)" : Caster.Name)
                .Append(", IsCasting=").Append(IsCasting)
                .Append(", m_interrupted=").Append(m_interrupted)
                .Append("\nSpell: ").Append(Spell?.ToString() ?? "(null)")
                .Append("\nSpellLine: ").Append(SpellLine?.ToString() ?? "(null)")
                .ToString();
        }

        /// <summary>
        /// When spell pulses
        /// </summary>
        public virtual void OnSpellPulse(PulsingSpellEffect effect)
        {
            if (Caster.IsMoving && Spell.IsFocus)
            {
                MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                effect.Cancel(false);
                return;
            }

            if (Caster.IsAlive == false)
            {
                effect.Cancel(false);
                return;
            }

            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (Caster.IsStunned || Caster.IsMezzed)
            {
                return;
            }

            // no instrument anymore = stop the song
            if (Spell.InstrumentRequirement != 0 && !CheckInstrument())
            {
                MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
                effect.Cancel(false);
                return;
            }

            if (Caster.Mana >= Spell.PulsePower)
            {
                Caster.Mana -= Spell.PulsePower;
                if (Spell.InstrumentRequirement != 0 || !HasPositiveEffect)
                {
                    SendEffectAnimation(Caster, 0, true, 1); // pulsing auras or songs
                }

                StartSpell(m_spellTarget);
            }
            else
            {
                MessageToCaster("You do not have enough mana and your spell was cancelled.", eChatType.CT_SpellExpires);
                effect.Cancel(false);
            }
        }

        /// <summary>
        /// Checks if caster holds the right instrument for this spell
        /// </summary>
        /// <returns>true if right instrument</returns>
        protected bool CheckInstrument()
        {
            InventoryItem instrument = Caster.AttackWeapon;

            // From patch 1.97:  Flutes, Lutes, and Drums will now be able to play any song type, and will no longer be limited to specific songs.
            if (instrument == null || instrument.Object_Type != (int)eObjectType.Instrument) // || (instrument.DPS_AF != 4 && instrument.DPS_AF != m_spell.InstrumentRequirement))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cancels first pulsing spell of type
        /// </summary>
        /// <param name="living">owner of pulsing spell</param>
        /// <param name="spellType">type of spell to cancel</param>
        /// <returns>true if any spells were canceled</returns>
        public virtual bool CancelPulsingSpell(GameLiving living, string spellType)
        {
            lock (living.ConcentrationEffects)
            {
                for (int i = 0; i < living.ConcentrationEffects.Count; i++)
                {
                    PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                    {
                        continue;
                    }

                    if (effect.SpellHandler.Spell.SpellType == spellType)
                    {
                        effect.Cancel(false);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Cancels all pulsing spells
        /// </summary>
        /// <param name="living"></param>
        public static void CancelAllPulsingSpells(GameLiving living)
        {
            List<IConcentrationEffect> pulsingSpells = new List<IConcentrationEffect>();

            GamePlayer player = living as GamePlayer;

            lock (living.ConcentrationEffects)
            {
                for (int i = 0; i < living.ConcentrationEffects.Count; i++)
                {
                    PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                    {
                        continue;
                    }

                    if (player != null && player.CharacterClass.MaxPulsingSpells > 1)
                    {
                        pulsingSpells.Add(effect);
                    }
                    else
                    {
                        effect.Cancel(false);
                    }
                }
            }

            // Non-concentration spells are grouped at the end of GameLiving.ConcentrationEffects.
            // The first one is added at the very end; successive additions are inserted just before the last element
            // which results in the following ordering:
            // Assume pulsing spells A, B, C, and D were added in that order; X, Y, and Z represent other spells
            // ConcentrationEffects = { X, Y, Z, ..., B, C, D, A }
            // If there are only ever 2 or less pulsing spells active, then the oldest one will always be at the end.
            // However, if an update or modification allows more than 2 to be active, the goofy ordering of the spells
            // will prevent us from knowing which spell is the oldest and should be canceled - we can go ahead and simply
            // cancel the last spell in the list (which will result in inconsistent behavior) or change the code that adds
            // spells to ConcentrationEffects so that it enforces predictable ordering.
            if (pulsingSpells.Count > 1)
            {
                pulsingSpells[pulsingSpells.Count - 1].Cancel(false);
            }
        }

        /// <summary>
        /// Cast a spell by using an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CastSpell(InventoryItem item)
        {
            m_spellItem = item;
            return CastSpell(Caster.TargetObject as GameLiving);
        }

        /// <summary>
        /// Cast a spell by using an Item
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CastSpell(GameLiving targetObject, InventoryItem item)
        {
            m_spellItem = item;
            return CastSpell(targetObject);
        }

        /// <summary>
        /// called whenever the player clicks on a spell icon
        /// or a GameLiving wants to cast a spell
        /// </summary>
        public virtual bool CastSpell()
        {
            return CastSpell(Caster.TargetObject as GameLiving);
        }

        public virtual bool CastSpell(GameLiving targetObject)
        {
            // Scale spells that are cast by pets
            if (Caster is GamePet && !(Caster is NecromancerPet) && ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
            {
                switch (Spell.SpellType.ToLower())
                {
                    // Scale Damage
                    case "damageovertime":
                    case "damageshield":
                    case "damageadd":
                    case "directdamage":
                    case "directdamagewithdebuff":
                    case "lifedrain":
                    case "damagespeeddecrease":
                    case "StyleBleeding": // Style Effect
                        Spell.Damage = Spell.Damage * Caster.Level / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL;
                        break;

                    // Scale Value
                    case "enduranceregenbuff":
                    case "combatspeedbuff":
                    case "hastebuff":
                    case "celeritybuff":
                    case "combatspeeddebuff":
                    case "hastedebuff":
                    case "heal":
                    case "combatheal":
                    case "healthregenbuff":
                    case "constitutionbuff":
                    case "dexteritybuff":
                    case "strengthbuff":
                    case "constitutiondebuff":
                    case "dexteritydebuff":
                    case "strengthdebuff":
                    case "armorfactordebuff":
                    case "armorfactorbuff":
                    case "armorabsorptionbuff":
                    case "armorabsorptiondebuff":
                    case "dexterityquicknessbuff":
                    case "strengthconstitutionbuff":
                    case "dexterityquicknessdebuff":
                    case "strengthconstitutiondebuff":
                    case "taunt":
                    case "unbreakablespeeddecrease":
                    case "SpeedDecrease":
                    case "stylecombatspeeddebuff": // Style Effect
                    case "stylespeeddecrease": // Style Effect
                    // case "styletaunt":  Taunt styles already scale with damage, leave their values alone.
                        Spell.Value = Spell.Value * Caster.Level / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL;
                        break;

                    // Scale Duration
                    case "disease":
                    case "stun":
                    case "Mesmerize":
                    case "stylestun": // Style Effect
                        Spell.Duration = (int)Math.Round(Spell.Duration * (double)Caster.Level / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL); ;
                        break;
                    default: break; // Don't mess with types we don't know
                } // switch (m_spell.SpellType.ToString().ToLower())
            } // if (Caster is GamePet)

            bool success = true;

            m_spellTarget = targetObject;

            Caster.Notify(GameLivingEvent.CastStarting, Caster, new CastingEventArgs(this));

            // [Stryve]: Do not break stealth if spell can be cast without breaking stealth.
            if (Caster is GamePlayer && UnstealthCasterOnStart)
            {
                ((GamePlayer)Caster).Stealth(false);
            }

            if (Caster.IsEngaging)
            {
                EngageEffect effect = Caster.EffectList.GetOfType<EngageEffect>();

                effect?.Cancel(false);
            }

            m_interrupted = false;

            if (Spell.Target.ToLower() == "pet")
            {
                // Pet is the target, check if the caster is the pet.
                if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
                {
                    m_spellTarget = Caster;
                }

                if (Caster is GamePlayer && Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                {
                    if (m_spellTarget == null || !Caster.IsControlledNPC(m_spellTarget as GameNPC))
                    {
                        m_spellTarget = Caster.ControlledBrain.Body;
                    }
                }
            }
            else if (Spell.Target.ToLower() == "controlled")
            {
                // Can only be issued by the owner of a pet and the target
                // is always the pet then.
                if (Caster is GamePlayer && Caster.ControlledBrain != null)
                {
                    m_spellTarget = Caster.ControlledBrain.Body;
                }
                else
                {
                    m_spellTarget = null;
                }
            }

            if (Spell.Pulse != 0 && CancelPulsingSpell(Caster, Spell.SpellType))
            {
                MessageToCaster(
                    Spell.InstrumentRequirement == 0 ? "You cancel your effect." : "You stop playing your song.",
                    eChatType.CT_Spell);
            }
            else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, m_spellTarget, Spell, SpellLine))
            {
                if (CheckBeginCast(m_spellTarget))
                {
                    if (Caster is GamePlayer && (Caster as GamePlayer).IsOnHorse && !HasPositiveEffect)
                    {
                        (Caster as GamePlayer).IsOnHorse = false;
                    }

                    if (!Spell.IsInstantCast)
                    {
                        StartCastTimer(m_spellTarget);

                        if ((Caster is GamePlayer && (Caster as GamePlayer).IsStrafing) || Caster.IsMoving)
                        {
                            CasterMoves();
                        }
                    }
                    else
                    {
                        if (Caster.ControlledBrain == null || Caster.ControlledBrain.Body == null || !(Caster.ControlledBrain.Body is NecromancerPet))
                        {
                            SendCastAnimation(0);
                        }

                        FinishSpellCast(m_spellTarget);
                    }
                }
                else
                {
                    success = false;
                }
            }

            // This is critical to restore the casters state and allow them to cast another spell
            if (!IsCasting)
            {
                OnAfterSpellCastSequence();
            }

            return success;
        }

        public virtual void StartCastTimer(GameLiving target)
        {
            m_interrupted = false;
            SendSpellMessages();

            int time = CalculateCastingTime();

            int step1 = time / 3;
            if (step1 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
            {
                step1 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
            }

            if (step1 < 1)
            {
                step1 = 1;
            }

            int step3 = time / 3;
            if (step3 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
            {
                step3 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
            }

            if (step3 < 1)
            {
                step3 = 1;
            }

            int step2 = time - step1 - step3;
            if (step2 < 1)
            {
                step2 = 1;
            }

            if (Caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG)
            {
                ((GamePlayer) Caster).Out.SendMessage("[DEBUG] spell time = " + time + ", step1 = " + step1 + ", step2 = " + step2 + ", step3 = " + step3, eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            m_castTimer = new DelayedCastTimer(Caster, this, target, step2, step3);
            m_castTimer.Start(step1);
            m_started = Caster.CurrentRegion.Time;
            SendCastAnimation();

            if (Caster.IsMoving || Caster.IsStrafing)
            {
                CasterMoves();
            }
        }

        /// <summary>
        /// Is called when the caster moves
        /// </summary>
        public virtual void CasterMoves()
        {
            if (Spell.InstrumentRequirement != 0)
            {
                return;
            }

            if (Spell.MoveCast)
            {
                return;
            }

            InterruptCasting();
            (Caster as GamePlayer)?.Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SpellHandler.CasterMove"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// This sends the spell messages to the player/target.
        ///</summary>
        public virtual void SendSpellMessages()
        {
            if (Spell.InstrumentRequirement == 0)
            {
                MessageToCaster("You begin casting a " + Spell.Name + " spell!", eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You begin playing " + Spell.Name + "!", eChatType.CT_Spell);
            }
        }

        /// <summary>
        /// casting sequence has a chance for interrupt through attack from enemy
        /// the final decision and the interrupt is done here
        /// TODO: con level dependend
        /// </summary>
        /// <param name="attacker">attacker that interrupts the cast sequence</param>
        /// <returns>true if casting was interrupted</returns>
        public virtual bool CasterIsAttacked(GameLiving attacker)
        {
            // [StephenxPimentel] Check if the necro has MoC effect before interrupting.
            if ((Caster as NecromancerPet)?.Owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
            {
                return false;
            }

            if (Spell.Uninterruptible)
            {
                return false;
            }

            if (Caster.EffectList.CountOfType(typeof(QuickCastEffect), typeof(MasteryofConcentrationEffect), typeof(FacilitatePainworkingEffect)) > 0)
            {
                return false;
            }

            if (IsCasting && Stage < 2)
            {
                if (Caster.ChanceSpellInterrupt(attacker))
                {
                    Caster.LastInterruptMessage = attacker.GetName(0, true) + " attacks you and your spell is interrupted!";
                    MessageToLiving(Caster, Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                    InterruptCasting(); // always interrupt at the moment
                    return true;
                }
            }

            return false;
        }

        public virtual bool CheckBeginCast(GameLiving selectedTarget)
        {
            return CheckBeginCast(selectedTarget, false);
        }

        /// <summary>
        /// All checks before any casting begins
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public virtual bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
        {
            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            if (!Caster.IsAlive)
            {
                if (!quiet)
                {
                    MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                }

                return false;
            }

            if (Caster is GamePlayer player)
            {
                long nextSpellAvailTime = Caster.TempProperties.getProperty<long>(GamePlayer.NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION);

                if (nextSpellAvailTime > Caster.CurrentRegion.Time)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.CastSpell.MustWaitBeforeCast", (nextSpellAvailTime - Caster.CurrentRegion.Time) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }

                if (player.Steed is GameSiegeRam)
                {
                    if (!quiet)
                    {
                        MessageToCaster("You can't cast in a siegeram!.", eChatType.CT_System);
                    }

                    return false;
                }

                GameSpellEffect naturesWomb = FindEffectOnTarget(Caster, typeof(NaturesWombEffect));
                if (naturesWomb != null)
                {
                    // [StephenxPimentel]
                    // Get Correct Message for 1.108 update.
                    MessageToCaster("You are silenced and cannot cast a spell right now.", eChatType.CT_SpellResisted);
                    return false;
                }
            }

            GameSpellEffect Phaseshift = FindEffectOnTarget(Caster, "Phaseshift");
            if (Phaseshift != null && (Spell.InstrumentRequirement == 0 || Spell.SpellType == "Mesmerize"))
            {
                if (!quiet)
                {
                    MessageToCaster("You're phaseshifted and can't cast a spell", eChatType.CT_System);
                }

                return false;
            }

            // Apply Mentalist RA5L
            if (Spell.Range > 0)
            {
                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                if (SelectiveBlindness != null)
                {
                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                    if (EffectOwner == selectedTarget)
                    {
                        if (Caster is GamePlayer && !quiet)
                        {
                            ((GamePlayer)Caster).Out.SendMessage($"{selectedTarget.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        }

                        return false;
                    }
                }
            }

            if (selectedTarget != null && selectedTarget.HasAbility("DamageImmunity") && Spell.SpellType == "DirectDamage" && Spell.Radius == 0)
            {
                if (!quiet)
                {
                    MessageToCaster(selectedTarget.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                    {
                        MessageToCaster(
                            "You are not wielding the right type of instrument!",
                                                eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }
            else if (Caster.IsSitting) // songs can be played if sitting
            {
                // Purge can be cast while sitting but only if player has negative effect that
                // don't allow standing up (like stun or mez)
                if (!quiet)
                {
                    MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster.AttackState && Spell.CastTime != 0)
            {
                if (Caster.CanCastInCombat(Spell) == false)
                {
                    Caster.StopAttack();
                    return false;
                }
            }

            if (!Spell.Uninterruptible && Spell.CastTime > 0 && Caster is GamePlayer &&
                Caster.EffectList.GetOfType<QuickCastEffect>() == null && Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null)
            {
                if (Caster.InterruptAction > 0 && Caster.InterruptAction + Caster.SpellInterruptRecastTime > Caster.CurrentRegion.Time)
                {
                    if (!quiet)
                    {
                        MessageToCaster($"You must wait {(Caster.InterruptAction + Caster.SpellInterruptRecastTime - Caster.CurrentRegion.Time) / 1000 + 1} seconds to cast a spell!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }

            if (Spell.RecastDelay > 0)
            {
                int left = Caster.GetSkillDisabledDuration(Spell);
                if (left > 0)
                {
                    if (Caster is NecromancerPet && ((Caster as NecromancerPet).Owner as GamePlayer).Client.Account.PrivLevel > (int)ePrivLevel.Player)
                    {
                        // Ignore Recast Timer
                    }
                    else
                    {
                        if (!quiet)
                        {
                            MessageToCaster($"You must wait {(left / 1000 + 1)} seconds to use this spell!", eChatType.CT_System);
                        }

                        return false;
                    }
                }
            }

            string targetType = Spell.Target.ToLower();

            // [Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
            if (targetType.Equals("pet"))
            {
                if (selectedTarget == null || !Caster.IsControlledNPC(selectedTarget as GameNPC))
                {
                    if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                    {
                        selectedTarget = Caster.ControlledBrain.Body;
                    }
                    else
                    {
                        if (!quiet)
                        {
                            MessageToCaster(
                                "You must cast this spell on a creature you are controlling.",
                                                    eChatType.CT_System);
                        }

                        return false;
                    }
                }
            }

            if (targetType == "area")
            {
                if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                    {
                        MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                    }

                    return false;
                }

                if (!Caster.GroundTargetInView)
                {
                    MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (targetType != "self" && targetType != "group" && targetType != "pet"
                     && targetType != "controlled" && targetType != "cone" && Spell.Range > 0)
            {
                // All spells that need a target.
                if (selectedTarget == null || selectedTarget.ObjectState != GameObject.eObjectState.Active)
                {
                    if (!quiet)
                    {
                        MessageToCaster(
                            "You must select a target for this spell!",
                                                eChatType.CT_SpellResisted);
                    }

                    return false;
                }

                if (!Caster.IsWithinRadius(selectedTarget, CalculateSpellRange()))
                {
                    if (Caster is GamePlayer && !quiet)
                    {
                        MessageToCaster(
                            "That target is too far away!",
                                                                       eChatType.CT_SpellResisted);
                    }

                    Caster.Notify(
                        GameLivingEvent.CastFailed,
                                  new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
                    return false;
                }

                switch (Spell.Target.ToLower())
                {
                    case "enemy":
                        if (selectedTarget == Caster)
                        {
                            if (!quiet)
                            {
                                MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
                            }

                            return false;
                        }

                        if (FindStaticEffectOnTarget(selectedTarget, typeof(NecromancerShadeEffect)) != null)
                        {
                            if (!quiet)
                            {
                                MessageToCaster("Invalid target.", eChatType.CT_System);
                            }

                            return false;
                        }

                        if (Spell.SpellType == "Charm" && Spell.CastTime == 0 && Spell.Pulse != 0)
                        {
                            break;
                        }

                        if (Caster.IsObjectInFront(selectedTarget, 180) == false)
                        {
                            if (!quiet)
                            {
                                MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                            }

                            Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                            return false;
                        }

                        if (Caster.TargetInView == false)
                        {
                            if (!quiet)
                            {
                                MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
                            }

                            Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                            return false;
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, quiet))
                        {
                            return false;
                        }

                        break;

                    case "corpse":
                        if (selectedTarget.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, true))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        break;

                    case "realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, true))
                        {
                            return false;
                        }

                        break;
                }

                // heals/buffs/rez need LOS only to start casting
                if (!Caster.TargetInView && Spell.Target.ToLower() != "pet")
                {
                    if (!quiet)
                    {
                        MessageToCaster("Your target is not in visible!", eChatType.CT_SpellResisted);
                    }

                    Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                    return false;
                }

                if (Spell.Target.ToLower() != "corpse" && !selectedTarget.IsAlive)
                {
                    if (!quiet)
                    {
                        MessageToCaster(selectedTarget.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }

            // Ryan: don't want mobs to have reductions in mana
            if (Spell.Power != 0 && Caster is GamePlayer && (Caster as GamePlayer).CharacterClass.ID != (int)eCharacterClass.Savage && Caster.Mana < PowerCost(selectedTarget) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                {
                    MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0)
            {
                if (Caster.Concentration < Spell.Concentration)
                {
                    if (!quiet)
                    {
                        MessageToCaster("This spell requires " + Spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }

                if (Caster.ConcentrationEffects.ConcSpellsCount >= 50)
                {
                    if (!quiet)
                    {
                        MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }

            // Cancel engage if user starts attack
            if (Caster.IsEngaging)
            {
                EngageEffect engage = Caster.EffectList.GetOfType<EngageEffect>();
                engage?.Cancel(false);
            }

            if (!(Caster is GamePlayer))
            {
                Caster.Notify(GameLivingEvent.CastSucceeded, this, new PetSpellEventArgs(Spell, SpellLine, selectedTarget));
            }

            return true;
        }

        /// <summary>
        /// Does the area we are in force an LoS check on everything?
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected bool MustCheckLOS(GameLiving living)
        {
            foreach (AbstractArea area in living.CurrentAreas)
            {
                if (area.CheckLOS)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check the Line of Sight from you to your pet
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSYouToPet(GamePlayer player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
            {
                return;
            }

            if ((response & 0x100) == 0x100) // In view ?
            {
                return;
            }

            MessageToLiving(player, "Your pet not in view.", eChatType.CT_SpellResisted);
            InterruptCasting(); // break;
        }

        /// <summary>
        /// Check the Line of Sight from a player to a target
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSPlayerToTarget(GamePlayer player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
            {
                return;
            }

            if ((response & 0x100) == 0x100) // In view?
            {
                return;
            }

            if (ServerProperties.Properties.ENABLE_DEBUG)
            {
                MessageToCaster("LoS Interrupt in CheckLOSPlayerToTarget", eChatType.CT_System);
                log.Debug("LoS Interrupt in CheckLOSPlayerToTarget");
            }

            if (Caster is GamePlayer)
            {
                MessageToCaster("You can't see your target from here!", eChatType.CT_SpellResisted);
            }

            InterruptCasting();
        }

        /// <summary>
        /// Check the Line of Sight from an npc to a target
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSNPCToTarget(GamePlayer player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
            {
                return;
            }

            if ((response & 0x100) == 0x100) // In view?
            {
                return;
            }

            if (ServerProperties.Properties.ENABLE_DEBUG)
            {
                MessageToCaster("LoS Interrupt in CheckLOSNPCToTarget", eChatType.CT_System);
                log.Debug("LoS Interrupt in CheckLOSNPCToTarget");
            }

            InterruptCasting();
        }

        /// <summary>
        /// Checks after casting before spell is executed
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool CheckEndCast(GameLiving target)
        {
            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            if (!Caster.IsAlive)
            {
                MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                return false;
            }

            if (Spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (Caster.IsSitting) // songs can be played if sitting
            {
                // Purge can be cast while sitting but only if player has negative effect that
                // don't allow standing up (like stun or mez)
                MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Spell.Target.ToLower() == "area")
            {
                if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
                {
                    MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (Spell.Target.ToLower() != "self" && Spell.Target.ToLower() != "group" && Spell.Target.ToLower() != "cone" && Spell.Range > 0)
            {
                if (Spell.Target.ToLower() != "pet")
                {
                    // all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (Caster is GamePlayer)
                        {
                            MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }

                    if (!Caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        if (Caster is GamePlayer)
                        {
                            MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }
                }

                switch (Spell.Target)
                {
                    case "Enemy":
                        // enemys have to be in front and in view for targeted spells
                        if (!Caster.IsObjectInFront(target, 180))
                        {
                            MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            return false;
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, false))
                        {
                            return false;
                        }

                        break;

                    case "Corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, true))
                        {
                            MessageToCaster(
                                "This spell only works on dead members of your realm!",
                                            eChatType.CT_SpellResisted);
                            return false;
                        }

                        break;

                    case "Realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }

                        break;

                    case "Pet":
                        /*
                         * [Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                return false;
                            }
                        }

                        // Now check distance for own pet
                        if (!Caster.IsWithinRadius(target, CalculateSpellRange()))
                        {
                            MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                            return false;
                        }

                        break;
                }
            }

            if (Caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Spell.Power != 0 && Caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.Concentration < Spell.Concentration)
            {
                MessageToCaster("This spell requires " + Spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                return false;
            }

            return true;
        }

        public virtual bool CheckDuringCast(GameLiving target)
        {
            return CheckDuringCast(target, false);
        }

        public virtual bool CheckDuringCast(GameLiving target, bool quiet)
        {
            if (m_interrupted)
            {
                return false;
            }

            if (!Spell.Uninterruptible && Spell.CastTime > 0 && Caster is GamePlayer &&
                Caster.EffectList.GetOfType<QuickCastEffect>() == null && Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null)
            {
                if (Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
                {
                    if (!quiet)
                    {
                        if (Caster.LastInterruptMessage != string.Empty)
                        {
                            MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                        }
                        else
                        {
                            MessageToCaster($"You are interrupted and must wait {((Caster.InterruptTime - m_started) / 1000 + 1)} seconds to cast a spell!", eChatType.CT_SpellResisted);
                        }
                    }

                    return false;
                }
            }

            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            if (!Caster.IsAlive)
            {
                if (!quiet)
                {
                    MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                }

                return false;
            }

            if (Spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                    {
                        MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }
            else if (Caster.IsSitting) // songs can be played if sitting
            {
                // Purge can be cast while sitting but only if player has negative effect that
                // don't allow standing up (like stun or mez)
                if (!quiet)
                {
                    MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Spell.Target.ToLower() == "area")
            {
                if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                    {
                        MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }
            else if (Spell.Target.ToLower() != "self" && Spell.Target.ToLower() != "group" && Spell.Target.ToLower() != "cone" && Spell.Range > 0)
            {
                if (Spell.Target.ToLower() != "pet")
                {
                    // all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (Caster is GamePlayer && !quiet)
                        {
                            MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }

                    if (Caster is GamePlayer && !Caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        if (!quiet)
                        {
                            MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }
                }

                switch (Spell.Target.ToLower())
                {
                    case "enemy":
                        // enemys have to be in front and in view for targeted spells
                        if (Caster is GamePlayer && !Caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        if (ServerProperties.Properties.CHECK_LOS_DURING_CAST)
                        {
                            GamePlayer playerChecker = null;

                            if (target is GamePlayer)
                            {
                                playerChecker = target as GamePlayer;
                            }
                            else if (Caster is GamePlayer)
                            {
                                playerChecker = Caster as GamePlayer;
                            }
                            else if (Caster is GameNPC && (Caster as GameNPC).Brain != null && (Caster as GameNPC).Brain is IControlledBrain)
                            {
                                playerChecker = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                            }

                            if (playerChecker != null)
                            {
                                // If the area forces an LoS check then we do it, otherwise we only check
                                // if caster or target is a player
                                // This will generate an interrupt if LOS check fails
                                if (Caster is GamePlayer)
                                {
                                    playerChecker.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckLOSPlayerToTarget));
                                }
                                else if (target is GamePlayer || MustCheckLOS(Caster))
                                {
                                    playerChecker.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckLOSNPCToTarget));
                                }
                            }
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
                        {
                            return false;
                        }

                        break;

                    case "corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, quiet))
                        {
                            if (!quiet)
                            {
                                MessageToCaster(
                                    "This spell only works on dead members of your realm!",
                                                        eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        break;

                    case "realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }

                        break;

                    case "pet":
                        /*
                         * Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                if (!quiet)
                                {
                                    MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                }

                                return false;
                            }
                        }

                        // Now check distance for own pet
                        if (!Caster.IsWithinRadius(target, CalculateSpellRange()))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        break;
                }
            }

            if (Caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                if (!quiet)
                {
                    MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Spell.Power != 0 && Caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                {
                    MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.Concentration < Spell.Concentration)
            {
                if (!quiet)
                {
                    MessageToCaster("This spell requires " + Spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                if (!quiet)
                {
                    MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            return true;
        }

        public virtual bool CheckAfterCast(GameLiving target)
        {
            return CheckAfterCast(target, false);
        }

        public virtual bool CheckAfterCast(GameLiving target, bool quiet)
        {
            if (m_interrupted)
            {
                return false;
            }

            if (!Spell.Uninterruptible && Spell.CastTime > 0 && Caster is GamePlayer &&
                Caster.EffectList.GetOfType<QuickCastEffect>() == null && Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null)
            {
                if (Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
                {
                    if (!quiet)
                    {
                        if (Caster.LastInterruptMessage != string.Empty)
                        {
                            MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                        }
                        else
                        {
                            MessageToCaster($"You are interrupted and must wait {(Caster.InterruptTime - m_started) / 1000 + 1} seconds to cast a spell!", eChatType.CT_SpellResisted);
                        }
                    }

                    Caster.InterruptAction = Caster.CurrentRegion.Time - Caster.SpellInterruptRecastAgain;
                    return false;
                }
            }

            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            if (!Caster.IsAlive)
            {
                if (!quiet)
                {
                    MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                }

                return false;
            }

            if (Spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                    {
                        MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    }

                    return false;
                }
            }
            else if (Caster.IsSitting) // songs can be played if sitting
            {
                // Purge can be cast while sitting but only if player has negative effect that
                // don't allow standing up (like stun or mez)
                if (!quiet)
                {
                    MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Spell.Target.ToLower() == "area")
            {
                if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                    {
                        MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                    }

                    return false;
                }

                if (!Caster.GroundTargetInView)
                {
                    MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (Spell.Target.ToLower() != "self" && Spell.Target.ToLower() != "group" && Spell.Target.ToLower() != "cone" && Spell.Range > 0)
            {
                if (Spell.Target.ToLower() != "pet")
                {
                    // all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (Caster is GamePlayer && !quiet)
                        {
                            MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }

                    if (Caster is GamePlayer && !Caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        if (!quiet)
                        {
                            MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        }

                        return false;
                    }
                }

                switch (Spell.Target)
                {
                    case "Enemy":
                        // enemys have to be in front and in view for targeted spells
                        if (Caster is GamePlayer && !Caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
                        {
                            return false;
                        }

                        break;

                    case "Corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, quiet))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        break;

                    case "Realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }

                        break;

                    case "Pet":
                        /*
                         * [Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                if (!quiet)
                                {
                                    MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                }

                                return false;
                            }
                        }

                        // Now check distance for own pet
                        if (!Caster.IsWithinRadius(target, CalculateSpellRange()))
                        {
                            if (!quiet)
                            {
                                MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                            }

                            return false;
                        }

                        break;
                }
            }

            if (Caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                if (!quiet)
                {
                    MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Spell.Power != 0 && Caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                {
                    MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.Concentration < Spell.Concentration)
            {
                if (!quiet)
                {
                    MessageToCaster($"This spell requires {Spell.Concentration} concentration points to cast!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            if (Caster is GamePlayer && Spell.Concentration > 0 && Caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                if (!quiet)
                {
                    MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the power to cast the spell
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int PowerCost(GameLiving target)
        {
            // warlock
            GameSpellEffect effect = FindEffectOnTarget(Caster, "Powerless");
            if (effect != null && !Spell.IsPrimary)
            {
                return 0;
            }

            // 1.108 - Valhallas Blessing now has a 75% chance to not use power.
            ValhallasBlessingEffect ValhallasBlessing = Caster.EffectList.GetOfType<ValhallasBlessingEffect>();
            if (ValhallasBlessing != null && Util.Chance(75))
            {
                return 0;
            }

            // patch 1.108 increases the chance to not use power to 50%.
            FungalUnionEffect FungalUnion = Caster.EffectList.GetOfType<FungalUnionEffect>();
            {
                if (FungalUnion != null && Util.Chance(50))
                {
                    return 0;
                }
            }

            // Arcane Syphon chance
            int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
            if (syphon > 0)
            {
                if (Util.Chance(syphon))
                {
                    return 0;
                }
            }

            double basepower = Spell.Power; // <== defined a basevar first then modified this base-var to tell %-costs from absolut-costs

            // percent of maxPower if less than zero
            if (basepower < 0)
            {
                if (Caster is GamePlayer player && player.CharacterClass.ManaStat != eStat.UNDEFINED)
                {
                    basepower = player.CalculateMaxMana(player.Level, player.GetBaseStat(player.CharacterClass.ManaStat)) * basepower * -0.01;
                }
                else
                {
                    basepower = Caster.MaxMana * basepower * -0.01;
                }
            }

            double power = basepower * 1.2; // <==NOW holding basepower*1.2 within 'power'

            eProperty focusProp = SkillBase.SpecToFocus(SpellLine.Spec);
            if (focusProp != eProperty.Undefined)
            {
                double focusBonus = Caster.GetModified(focusProp) * 0.4;
                if (Spell.Level > 0)
                {
                    focusBonus /= Spell.Level;
                }

                if (focusBonus > 0.4)
                {
                    focusBonus = 0.4;
                }
                else if (focusBonus < 0)
                {
                    focusBonus = 0;
                }

                power -= basepower * focusBonus; // <== So i can finally use 'basepower' for both calculations: % and absolut
            }
            else if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ClassType == eClassType.Hybrid)
            {
                double specBonus = 0;
                if (Spell.Level != 0)
                {
                    specBonus = ((GamePlayer)Caster).GetBaseSpecLevel(SpellLine.Spec) * 0.4 / Spell.Level;
                }

                if (specBonus > 0.4)
                {
                    specBonus = 0.4;
                }
                else if (specBonus < 0)
                {
                    specBonus = 0;
                }

                power -= basepower * specBonus;
            }

            // doubled power usage if quickcasting
            if (Caster.EffectList.GetOfType<QuickCastEffect>() != null && Spell.CastTime > 0)
            {
                power *= 2;
            }

            return (int)power;
        }

        /// <summary>
        /// Calculates the enduance cost of the spell
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateEnduranceCost()
        {
            return 5;
        }

        /// <summary>
        /// Calculates the range to target needed to cast the spell
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateSpellRange()
        {
            int range = Math.Max(32, (int)(Spell.Range * Caster.GetModified(eProperty.SpellRange) * 0.01));
            return range;

            // Dinberg: add for warlock range primer
        }

        /// <summary>
        /// Called whenever the casters casting sequence is to interrupt immediately
        /// </summary>
        public virtual void InterruptCasting()
        {
            if (m_interrupted || !IsCasting)
            {
                return;
            }

            m_interrupted = true;

            if (IsCasting)
            {
                foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendInterruptAnimation(Caster);
                }
            }

            if (m_castTimer != null)
            {
                m_castTimer.Stop();
                m_castTimer = null;

                if (Caster is GamePlayer)
                {
                    ((GamePlayer)Caster).ClearSpellQueue();
                }
            }

            m_startReuseTimer = false;
            OnAfterSpellCastSequence();
        }

        /// <summary>
        /// Casts a spell after the CastTime delay
        /// </summary>
        protected class DelayedCastTimer : GameTimer
        {
            /// <summary>
            /// The spellhandler instance with callbacks
            /// </summary>
            private readonly SpellHandler m_handler;
            /// <summary>
            /// The target object at the moment of CastSpell call
            /// </summary>
            private readonly GameLiving m_target;
            private readonly GameLiving m_caster;
            private byte m_stage;
            private readonly int m_delay1;
            private readonly int m_delay2;

            /// <summary>
            /// Constructs a new DelayedSpellTimer
            /// </summary>
            /// <param name="actionSource">The caster</param>
            /// <param name="handler">The spell handler</param>
            /// <param name="target">The target object</param>
            public DelayedCastTimer(GameLiving actionSource, SpellHandler handler, GameLiving target, int delay1, int delay2)
                : base(actionSource.CurrentRegion.TimeManager)
            {
                m_handler = handler ?? throw new ArgumentNullException(nameof(handler));
                m_target = target;
                m_caster = actionSource ?? throw new ArgumentNullException(nameof(actionSource));
                m_stage = 0;
                m_delay1 = delay1;
                m_delay2 = delay2;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                try
                {
                    if (m_stage == 0)
                    {
                        if (!m_handler.CheckAfterCast(m_target))
                        {
                            Interval = 0;
                            m_handler.InterruptCasting();
                            m_handler.OnAfterSpellCastSequence();
                            return;
                        }

                        m_stage = 1;
                        m_handler.Stage = 1;
                        Interval = m_delay1;
                    }
                    else if (m_stage == 1)
                    {
                        if (!m_handler.CheckDuringCast(m_target))
                        {
                            Interval = 0;
                            m_handler.InterruptCasting();
                            m_handler.OnAfterSpellCastSequence();
                            return;
                        }

                        m_stage = 2;
                        m_handler.Stage = 2;
                        Interval = m_delay2;
                    }
                    else if (m_stage == 2)
                    {
                        m_stage = 3;
                        m_handler.Stage = 3;
                        Interval = 100;

                        if (m_handler.CheckEndCast(m_target))
                        {
                            m_handler.FinishSpellCast(m_target);
                        }
                    }
                    else
                    {
                        m_stage = 4;
                        m_handler.Stage = 4;
                        Interval = 0;
                        m_handler.OnAfterSpellCastSequence();
                    }

                    if (m_caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG && m_stage < 3)
                    {
                        (m_caster as GamePlayer).Out.SendMessage($"[DEBUG] step = {m_handler.Stage + 1}", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }

                    return;
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error(ToString(), e);
                    }
                }

                m_handler.OnAfterSpellCastSequence();
                Interval = 0;
            }

            /// <summary>
            /// Returns short information about the timer
            /// </summary>
            /// <returns>Short info about the timer</returns>
            public override string ToString()
            {
                return new StringBuilder(base.ToString(), 128)
                    .Append(" spellhandler: (").Append(m_handler).Append(')')
                    .ToString();
            }
        }

        /// <summary>
        /// Calculates the effective casting time
        /// </summary>
        /// <returns>effective casting time in milliseconds</returns>
        public virtual int CalculateCastingTime()
        {
            return Caster.CalculateCastingTime(SpellLine, Spell);
        }

        /// <summary>
        /// Sends the cast animation
        /// </summary>
        public virtual void SendCastAnimation()
        {
            ushort castTime = (ushort)(CalculateCastingTime() / 100);
            SendCastAnimation(castTime);
        }

        /// <summary>
        /// Sends the cast animation
        /// </summary>
        /// <param name="castTime">The cast time</param>
        public virtual void SendCastAnimation(ushort castTime)
        {
            foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player?.Out.SendSpellCastAnimation(Caster, Spell.ClientEffect, castTime);
            }
        }

        /// <summary>
        /// Send the Effect Animation
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="boltDuration">The duration of a bolt</param>
        /// <param name="noSound">sound?</param>
        /// <param name="success">spell success?</param>
        public virtual void SendEffectAnimation(GameObject target, ushort boltDuration, bool noSound, byte success)
        {
            if (target == null)
            {
                target = Caster;
            }

            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(Caster, target, Spell.ClientEffect, boltDuration, noSound, success);
            }
        }

        /// <summary>
        /// Send the Interrupt Cast Animation
        /// </summary>
        public virtual void SendInterruptCastAnimation()
        {
            foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendInterruptAnimation(Caster);
            }
        }

        public virtual void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
        {
            if (target == null)
            {
                target = Caster;
            }

            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(Caster, target, clientEffect, boltDuration, noSound, success);
            }
        }

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public virtual void FinishSpellCast(GameLiving target)
        {
            if (Caster is GamePlayer && ((GamePlayer)Caster).IsOnHorse && !HasPositiveEffect)
            {
                ((GamePlayer)Caster).IsOnHorse = false;
            }

            // [Stryve]: Do not break stealth if spell never breaks stealth.
            if ((Caster is GamePlayer) && UnstealthCasterOnFinish)
            {
                ((GamePlayer)Caster).Stealth(false);
            }

            if (Caster is GamePlayer && !HasPositiveEffect)
            {
                (Caster.AttackWeapon as GameInventoryItem)?.OnSpellCast(Caster, target, Spell);
            }

            // messages
            if (Spell.InstrumentRequirement == 0 && Spell.ClientEffect != 0 && Spell.CastTime > 0)
            {
                MessageToCaster("You cast a " + Spell.Name + " spell!", eChatType.CT_Spell);
                foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (player != Caster)
                    {
                        player.MessageFromArea(Caster, Caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            if (Spell.Pulse != 0 && Spell.Frequency > 0)
            {
                CancelAllPulsingSpells(Caster);
                PulsingSpellEffect pulseeffect = new PulsingSpellEffect(this);
                pulseeffect.Start();

                // show animation on caster for positive spells, negative shows on every StartSpell
                if (Spell.Target == "Self" || Spell.Target == "Group")
                {
                    SendEffectAnimation(Caster, 0, false, 1);
                }

                if (Spell.Target == "Pet")
                {
                    SendEffectAnimation(target, 0, false,1);
                }
            }

            StartSpell(target); // and action

            // Dinberg: This is where I moved the warlock part (previously found in gameplayer) to prevent
            // cancelling before the spell was fired.
            if (Spell.SpellType != "Powerless" && Spell.SpellType != "Range" && Spell.SpellType != "Uninterruptable")
            {
                GameSpellEffect effect = FindEffectOnTarget(Caster, "Powerless");
                if (effect == null)
                {
                    effect = FindEffectOnTarget(Caster, "Range");
                }

                if (effect == null)
                {
                    effect = FindEffectOnTarget(Caster, "Uninterruptable");
                }

                // if we found an effect, cancel it!
                effect?.Cancel(false);
            }

            // the quick cast is unallowed whenever you miss the spell
            // set the time when casting to can not quickcast during a minimum time
            if (Caster is GamePlayer)
            {
                QuickCastEffect quickcast = Caster.EffectList.GetOfType<QuickCastEffect>();
                if (quickcast != null && Spell.CastTime > 0)
                {
                    Caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, Caster.CurrentRegion.Time);
                    ((GamePlayer)Caster).DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
                    quickcast.Cancel(false);
                }
            }

            if (Ability != null)
            {
                Caster.DisableSkill(Ability.Ability, Spell.RecastDelay == 0 ? 3000 : Spell.RecastDelay);
            }

            // disable spells with recasttimer (Disables group of same type with same delay)
            if (Spell.RecastDelay > 0 && m_startReuseTimer)
            {
                if (Caster is GamePlayer)
                {
                    ICollection<Tuple<Skill, int>> toDisable = new List<Tuple<Skill, int>>();

                    GamePlayer gp_caster = Caster as GamePlayer;
                    foreach (var skills in gp_caster.GetAllUsableSkills())
                    {
                        if (skills.Item1 is Spell &&
                            (((Spell)skills.Item1).ID == Spell.ID || (((Spell)skills.Item1).SharedTimerGroup != 0 && (((Spell)skills.Item1).SharedTimerGroup == Spell.SharedTimerGroup))))
                        {
                            toDisable.Add(new Tuple<Skill, int>((Spell)skills.Item1, Spell.RecastDelay));
                        }
                    }

                    foreach (var sl in gp_caster.GetAllUsableListSpells())
                    {
                        foreach (var sp in sl.Item2)
                        {
                            if (sp is Spell &&
                                (((Spell)sp).ID == Spell.ID || (((Spell)sp).SharedTimerGroup != 0 && (((Spell)sp).SharedTimerGroup == Spell.SharedTimerGroup))))
                            {
                                toDisable.Add(new Tuple<Skill, int>((Spell)sp, Spell.RecastDelay));
                            }
                        }
                    }

                    Caster.DisableSkill(toDisable);
                }
                else if (Caster is GameNPC)
                {
                    Caster.DisableSkill(Spell, Spell.RecastDelay);
                }
            }

            GameEventMgr.Notify(GameLivingEvent.CastFinished, Caster, new CastingEventArgs(this, target, m_lastAttackData));
        }

        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public virtual IList<GameLiving> SelectTargets(GameObject castTarget)
        {
            var list = new List<GameLiving>(8);
            GameLiving target = castTarget as GameLiving;
            bool targetchanged = false;
            string modifiedTarget = Spell.Target.ToLower();
            ushort modifiedRadius = (ushort)Spell.Radius;

            GameSpellEffect TargetMod = FindEffectOnTarget(Caster, "TargetModifier");
            if (TargetMod != null)
            {
                if (modifiedTarget == "enemy" || modifiedTarget == "realm" || modifiedTarget == "group")
                {
                    var newtarget = (int)TargetMod.Spell.Value;

                    switch (newtarget)
                    {
                        case 0: // Apply on heal single
                            if (Spell.SpellType.ToLower() == "heal" && modifiedTarget == "realm")
                            {
                                modifiedTarget = "group";
                                targetchanged = true;
                            }

                            break;
                        case 1: // Apply on heal group
                            if (Spell.SpellType.ToLower() == "heal" && modifiedTarget == "group")
                            {
                                modifiedTarget = "realm";
                                modifiedRadius = (ushort)Spell.Range;
                                targetchanged = true;
                            }

                            break;
                        case 2: // apply on enemy
                            if (modifiedTarget == "enemy")
                            {
                                if (Spell.Radius == 0)
                                {
                                    modifiedRadius = 450;
                                }

                                if (Spell.Radius != 0)
                                {
                                    modifiedRadius += 300;
                                }

                                targetchanged = true;
                            }

                            break;
                        case 3: // Apply on buff
                            if (Spell.Target.ToLower() == "group"
                                && Spell.Pulse != 0)
                            {
                                modifiedTarget = "realm";
                                modifiedRadius = (ushort)Spell.Range;
                                targetchanged = true;
                            }

                            break;
                    }
                }

                if (targetchanged)
                {
                    if (TargetMod.Duration < 65535)
                    {
                        TargetMod.Cancel(false);
                    }
                }
            }

            if (modifiedTarget == "pet" && !HasPositiveEffect)
            {
                modifiedTarget = "enemy";

                // [Ganrod] Nidel: can cast TurretPBAoE on selected Pet/Turret
                if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower())
                {
                    target = Caster.ControlledBrain.Body;
                }
            }

            switch (modifiedTarget)
            {
                // GTAoE
                case "area":
                    // Dinberg - fix for animists turrets, where before a radius of zero meant that no targets were ever
                    // selected!
                    if (Spell.SpellType == "SummonAnimistPet" || Spell.SpellType == "SummonAnimistFnF")
                    {
                        list.Add(Caster);
                    }
                    else
                        if (modifiedRadius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                // Apply Mentalist RA5L
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == player)
                                    {
                                        if (Caster is GamePlayer)
                                        {
                                            ((GamePlayer)Caster).Out.SendMessage($"{player.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                    else
                                    {
                                        list.Add(player);
                                    }
                                }
                                else
                                {
                                    list.Add(player);
                                }
                            }
                        }

                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
                        {
                            if (npc is GameStorm)
                            {
                                list.Add(npc);
                            }
                            else if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                if (!npc.HasAbility("DamageImmunity"))
                                {
                                    list.Add(npc);
                                }
                            }
                        }
                    }

                    break;

                case "corpse":
                    if (target != null && !target.IsAlive)
                    {
                        list.Add(target);
                    }

                    break;

                case "pet":
                    {
                        // Start-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
                        // awesome, Pbaoe with target pet spell ?^_^
                        if (modifiedRadius > 0 && Spell.Range == 0)
                        {
                            foreach (GameNPC pet in Caster.GetNPCsInRadius(modifiedRadius))
                            {
                                if (Caster.IsControlledNPC(pet))
                                {
                                    list.Add(pet);
                                }
                            }

                            return list;
                        }

                        if (target == null)
                        {
                            break;
                        }

                        GameNPC petBody = target as GameNPC;

                        // check target
                        if (petBody != null && Caster.IsWithinRadius(petBody, Spell.Range))
                        {
                            if (Caster.IsControlledNPC(petBody))
                            {
                                list.Add(petBody);
                            }
                        }

                        // check controllednpc if target isn't pet (our pet)
                        if (list.Count < 1 && Caster.ControlledBrain != null)
                        {
                            petBody = Caster.ControlledBrain.Body;
                            if (petBody != null && Caster.IsWithinRadius(petBody, Spell.Range))
                            {
                                list.Add(petBody);
                            }
                        }

                        // Single spell buff/heal...
                        if (Spell.Radius == 0)
                        {
                            return list;
                        }

                        // Our buff affects every pet in the area of targetted pet (our pets)
                        if (Spell.Radius > 0 && petBody != null)
                        {
                            foreach (GameNPC pet in petBody.GetNPCsInRadius(modifiedRadius))
                            {
                                // ignore target or our main pet already added
                                if (pet == petBody || !Caster.IsControlledNPC(pet))
                                {
                                    continue;
                                }

                                list.Add(pet);
                            }
                        }
                    }

                    // End-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
                    break;

                case "enemy":
                    if (modifiedRadius > 0)
                    {
                        if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower() && (target == null || Spell.Range == 0))
                        {
                            target = Caster;
                        }

                        if (target == null)
                        {
                            return null;
                        }

                        foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == player)
                                    {
                                        if (Caster is GamePlayer)
                                        {
                                            ((GamePlayer)Caster).Out.SendMessage($"{player.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                    else
                                    {
                                        list.Add(player);
                                    }
                                }
                                else
                                {
                                    list.Add(player);
                                }
                            }
                        }

                        foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                if (!npc.HasAbility("DamageImmunity"))
                                {
                                    list.Add(npc);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            // Apply Mentalist RA5L
                            if (Spell.Range > 0)
                            {
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == target)
                                    {
                                        if (Caster is GamePlayer)
                                        {
                                            ((GamePlayer)Caster).Out.SendMessage($"{target.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                    else if (!target.HasAbility("DamageImmunity"))
                                    {
                                        list.Add(target);
                                    }
                                }
                                else if (!target.HasAbility("DamageImmunity"))
                                {
                                    list.Add(target);
                                }
                            }
                            else if (!target.HasAbility("DamageImmunity"))
                            {
                                list.Add(target);
                            }
                        }
                    }

                    break;

                case "realm":
                    if (modifiedRadius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                        {
                            target = Caster;
                        }

                        foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true) == false)
                            {
                                list.Add(player);
                            }
                        }

                        foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) == false)
                            {
                                list.Add(npc);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) == false)
                        {
                            list.Add(target);
                        }
                    }

                    break;

                case "self":
                    {
                        if (modifiedRadius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                            {
                                target = Caster;
                            }

                            foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true) == false)
                                {
                                    list.Add(player);
                                }
                            }

                            foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) == false)
                                {
                                    list.Add(npc);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }

                        break;
                    }

                case "group":
                    {
                        Group group = Caster.Group;
                        int spellRange = CalculateSpellRange();
                        if (spellRange == 0)
                        {
                            spellRange = modifiedRadius;
                        }

                        // Just add ourself
                        if (group == null)
                        {
                            list.Add(Caster);

                            IControlledBrain npc = Caster.ControlledBrain;
                            if (npc != null)
                            {
                                // Add our first pet
                                GameNPC petBody2 = npc.Body;
                                if (Caster.IsWithinRadius(petBody2, spellRange))
                                {
                                    list.Add(petBody2);
                                }

                                // Now lets add any subpets!
                                if (petBody2 != null && petBody2.ControlledNpcList != null)
                                {
                                    foreach (IControlledBrain icb in petBody2.ControlledNpcList)
                                    {
                                        if (icb != null && Caster.IsWithinRadius(icb.Body, spellRange))
                                        {
                                            list.Add(icb.Body);
                                        }
                                    }
                                }
                            }
                        }

                        // We need to add the entire group
                        else
                        {
                            foreach (GameLiving living in group.GetMembersInTheGroup())
                            {
                                // only players in range
                                if (Caster.IsWithinRadius(living, spellRange))
                                {
                                    list.Add(living);

                                    IControlledBrain npc = living.ControlledBrain;
                                    if (npc != null)
                                    {
                                        // Add our first pet
                                        GameNPC petBody2 = npc.Body;
                                        if (Caster.IsWithinRadius(petBody2, spellRange))
                                        {
                                            list.Add(petBody2);
                                        }

                                        // Now lets add any subpets!
                                        if (petBody2 != null && petBody2.ControlledNpcList != null)
                                        {
                                            foreach (IControlledBrain icb in petBody2.ControlledNpcList)
                                            {
                                                if (icb != null && Caster.IsWithinRadius(icb.Body, spellRange))
                                                {
                                                    list.Add(icb.Body);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }

                case "cone":
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Range))
                        {
                            if (player == Caster)
                            {
                                continue;
                            }

                            if (!Caster.IsObjectInFront(player, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
                            {
                                continue;
                            }

                            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                continue;
                            }

                            list.Add(player);
                        }

                        foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Range))
                        {
                            if (npc == Caster)
                            {
                                continue;
                            }

                            if (!Caster.IsObjectInFront(npc, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
                            {
                                continue;
                            }

                            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                continue;
                            }

                            if (!npc.HasAbility("DamageImmunity"))
                            {
                                list.Add(npc);
                            }
                        }

                        break;
                    }
            }
            return list;
        }

        /// <summary>
        /// Cast all subspell recursively
        /// </summary>
        /// <param name="target"></param>
        public virtual void CastSubSpells(GameLiving target)
        {
            List<int> subSpellList = new List<int>();
            if (Spell.SubSpellId > 0)
            {
                subSpellList.Add(Spell.SubSpellId);
            }

            foreach (int spellID in subSpellList.Union(Spell.MultipleSubSpells))
            {
                Spell spell = SkillBase.GetSpellByID(spellID);

                // we need subspell ID to be 0, we don't want spells linking off the subspell
                if (target != null && spell != null && spell.SubSpellId == 0)
                {
                    ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    spellhandler.StartSpell(target);
                }
            }
        }

        /// <summary>
        /// Tries to start a spell attached to an item (/use with at least 1 charge)
        /// Override this to do a CheckBeginCast if needed, otherwise spell will always cast and item will be used.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="item"></param>
        public virtual bool StartSpell(GameLiving target, InventoryItem item)
        {
            m_spellItem = item;
            return StartSpell(target);
        }

        /// <summary>
        /// Called when spell effect has to be started and applied to targets
        /// This is typically called after calling CheckBeginCast
        /// </summary>
        /// <param name="target">The current target object</param>
        public virtual bool StartSpell(GameLiving target)
        {
            // For PBAOE spells always set the target to the caster
            if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower() && (target == null || (Spell.Radius > 0 && Spell.Range == 0)))
            {
                target = Caster;
            }

            if (m_spellTarget == null)
            {
                m_spellTarget = target;
            }

            if (m_spellTarget == null)
            {
                return false;
            }

            var targets = SelectTargets(m_spellTarget);

            double effectiveness = Caster.Effectiveness;

            if (Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
            {
                MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
                if (ra != null && ra.Level > 0)
                {
                    effectiveness *= Math.Round((double)ra.GetAmountForLevel(ra.Level) / 100, 2);
                }
            }

            // [StephenxPimentel] Reduce Damage if necro is using MoC
            if ((Caster as NecromancerPet)?.Owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
            {
                MasteryofConcentrationAbility necroRA = (Caster as NecromancerPet).Owner.GetAbility<MasteryofConcentrationAbility>();
                if (necroRA != null && necroRA.Level > 0)
                {
                    effectiveness *= Math.Round((double)necroRA.GetAmountForLevel(necroRA.Level) / 100, 2);
                }
            }

            if (Caster is GamePlayer && (Caster as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Warlock && Spell.IsSecondary)
            {
                Spell uninterruptibleSpell = Caster.TempProperties.getProperty<Spell>(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);

                if (uninterruptibleSpell != null && uninterruptibleSpell.Value > 0)
                {
                    double nerf = uninterruptibleSpell.Value;
                    effectiveness *= 1 - nerf * 0.01;
                    Caster.TempProperties.removeProperty(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);
                }
            }

            foreach (GameLiving t in targets)
            {
                // Aggressive NPCs will aggro on every target they hit
                // with an AoE spell, whether it landed or was resisted.
                if (Spell.Radius > 0 && Spell.Target.ToLower() == "enemy")
                {
                    ((Caster as GameNPC)?.Brain as IOldAggressiveBrain)?.AddToAggroList(t, 1);
                }

                if (Util.Chance(CalculateSpellResistChance(t)))
                {
                    OnSpellResisted(t);
                    continue;
                }

                if (Spell.Radius == 0 || HasPositiveEffect)
                {
                    ApplyEffectOnTarget(t, effectiveness);
                }
                else if (Spell.Target.ToLower() == "area")
                {
                    int dist = t.GetDistanceTo(Caster.GroundTarget);
                    if (dist >= 0)
                    {
                        ApplyEffectOnTarget(t, effectiveness - CalculateAreaVariance(t, dist, Spell.Radius));
                    }
                }
                else if (Spell.Target.ToLower() == "cone")
                {
                    int dist = t.GetDistanceTo(Caster);

                    // Cone spells use the range for their variance!
                    if (dist >= 0)
                    {
                        ApplyEffectOnTarget(t, effectiveness - CalculateAreaVariance(t, dist, Spell.Range));
                    }
                }
                else
                {
                    int dist = t.GetDistanceTo(target);
                    if (dist >= 0)
                    {
                        ApplyEffectOnTarget(t, effectiveness - CalculateAreaVariance(t, dist, Spell.Radius));
                    }
                }
            }

            if (Spell.Target.ToLower() == "ground")
            {
                ApplyEffectOnTarget(null, 1);
            }

            CastSubSpells(target);
            return true;
        }

        /// <summary>
        /// Calculate the variance due to the radius of the spell
        /// </summary>
        /// <param name="distance">The distance away from center of the spell</param>
        /// <param name="radius">The radius of the spell</param>
        /// <returns></returns>
        protected virtual double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            return (double)distance / radius;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected virtual int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = Spell.Duration;
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

            duration *= effectiveness;
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

        /// <summary>
        /// Creates the corresponding spell effect for the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        protected virtual GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            int freq = Spell?.Frequency ?? 0;
            return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), freq, effectiveness);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public virtual void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target is GamePlayer)
            {
                var effect1 = FindEffectOnTarget(target, "Phaseshift");
                if (effect1 != null && (Spell.SpellType != "SpreadHeal" || Spell.SpellType != "Heal" || Spell.SpellType != "SpeedEnhancement"))
                {
                    MessageToCaster(target.Name + " is Phaseshifted and can't be effected by this Spell!", eChatType.CT_SpellResisted);
                    return;
                }
            }

            if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
            {
                bool isAllowed = false;
                bool isSilent = false;

                if (Spell.Radius == 0)
                {
                    switch (Spell.SpellType.ToLower())
                    {
                        case "archery":
                        case "bolt":
                        case "bomber":
                        case "damagespeeddecrease":
                        case "directdamage":
                        case "magicalstrike":
                        case "siegearrow":
                        case "summontheurgistpet":
                        case "directdamagewithdebuff":
                            isAllowed = true;
                            break;
                    }
                }

                if (Spell.Radius > 0)
                {
                    // pbaoe is allowed, otherwise door is in range of a AOE so don't spam caster with a message
                    if (Spell.Range == 0)
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        isSilent = true;
                    }
                }

                if (!isAllowed)
                {
                    if (!isSilent)
                    {
                        MessageToCaster($"Your spell has no effect on the {target.Name}!", eChatType.CT_SpellResisted);
                    }

                    return;
                }
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects || SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || SpellLine.KeyName == GlobalSpellsLines.Potions_Effects || SpellLine.KeyName == Specs.Savagery || SpellLine.KeyName == GlobalSpellsLines.Character_Abilities || SpellLine.KeyName == "OffensiveProc")
            {
                effectiveness = 1.0; // TODO player.PlayerEffectiveness
            }

            if (effectiveness <= 0)
            {
                return; // no effect
            }

            // Apply effect for Duration Spell.
            if (Spell.Duration > 0 && Spell.Target.ToLower() != "area" || Spell.Concentration > 0)
            {
                OnDurationEffectApply(target, effectiveness);
            }
            else
            {
                OnDirectEffect(target, effectiveness);
            }

            if (!HasPositiveEffect)
            {
                AttackData ad = new AttackData
                {
                    Attacker = Caster,
                    Target = target,
                    AttackType = AttackData.eAttackType.Spell,
                    SpellHandler = this,
                    AttackResult = GameLiving.eAttackResult.HitUnstyled,
                    IsSpellResisted = false
                };

                m_lastAttackData = ad;
            }
        }

        /// <summary>
        /// Called when cast sequence is complete
        /// </summary>
        public virtual void OnAfterSpellCastSequence()
        {
            CastingCompleteEvent?.Invoke(this);
        }

        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns>true if this spell is better version than compare spell</returns>
        public virtual bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            Spell oldspell = oldeffect.Spell;
            Spell newspell = neweffect.Spell;

            if (oldspell.IsConcentration)
            {
                return false;
            }

            if (newspell.Damage < oldspell.Damage)
            {
                return false;
            }

            if (newspell.Value < oldspell.Value)
            {
                return false;
            }

            // makes problems for immunity effects
            if (!oldeffect.ImmunityState && !newspell.IsConcentration)
            {
                if (neweffect.Duration <= oldeffect.RemainingTime)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public virtual bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0 || compare.Spell.EffectGroup != 0)
            {
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            }

            if (compare.Spell.SpellType != Spell.SpellType)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines wether this spell can be disabled
        /// by better versions spells that stacks without overwriting
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public virtual bool IsCancellable(GameSpellEffect compare)
        {
            if (compare.SpellHandler != null)
            {
                if ((compare.SpellHandler.AllowCoexisting || AllowCoexisting)
                    && (!compare.SpellHandler.SpellLine.KeyName.Equals(SpellLine.KeyName, StringComparison.OrdinalIgnoreCase)
                        || compare.SpellHandler.Spell.IsInstantCast != Spell.IsInstantCast))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines wether new spell is better than old spell and should disable it
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns></returns>
        public virtual bool IsCancellableEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            if (neweffect.SpellHandler.Spell.Value >= oldeffect.SpellHandler.Spell.Value)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Execute Duration Spell Effect on Target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public virtual void OnDurationEffectApply(GameLiving target, double effectiveness)
        {
            if (!target.IsAlive || target.EffectList == null)
            {
                return;
            }

            eChatType noOverwrite = (Spell.Pulse == 0) ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);

            // Iterate through Overwritable Effect
            var overwritenEffects = target.EffectList.OfType<GameSpellEffect>().Where(effect => effect.SpellHandler != null && effect.SpellHandler.IsOverwritable(neweffect));

            // Store Overwritable or Cancellable
            var enable = true;
            var cancellableEffects = new List<GameSpellEffect>(1);
            GameSpellEffect overwriteEffect = null;

            foreach (var ovEffect in overwritenEffects)
            {
                // If we can cancel spell effect we don't need to overwrite it
                if (ovEffect.SpellHandler.IsCancellable(neweffect))
                {
                    // Spell is better than existing "Cancellable" or it should start disabled
                    if (IsCancellableEffectBetter(ovEffect, neweffect))
                    {
                        cancellableEffects.Add(ovEffect);
                    }
                    else
                    {
                        enable = false;
                    }
                }
                else
                {
                    // Check for Overwriting.
                    if (IsNewEffectBetter(ovEffect, neweffect))
                    {
                        // New Spell is overwriting this one.
                        overwriteEffect = ovEffect;
                    }
                    else
                    {
                        // Old Spell is Better than new one
                        SendSpellResistAnimation(target);
                        if (target == Caster)
                        {
                            if (ovEffect.ImmunityState)
                            {
                                MessageToCaster("You can't have that effect again yet!", noOverwrite);
                            }
                            else
                            {
                                MessageToCaster("You already have that effect. Wait until it expires. Spell failed.", noOverwrite);
                            }
                        }
                        else
                        {
                            if (ovEffect.ImmunityState)
                            {
                                this.MessageToCaster(noOverwrite, "{0} can't have that effect again yet!", ovEffect.Owner != null ? ovEffect.Owner.GetName(0, true) : "(null)");
                            }
                            else
                            {
                                this.MessageToCaster(noOverwrite, "{0} already has that effect.", target.GetName(0, true));
                                MessageToCaster("Wait until it expires. Spell Failed.", noOverwrite);
                            }
                        }

                        // Prevent Adding.
                        return;
                    }
                }
            }

            // Register Effect list Changes
            target.EffectList.BeginChanges();
            try
            {
                // Check for disabled effect
                foreach (var disableEffect in cancellableEffects)
                {
                    disableEffect.DisableEffect(false);
                }

                if (overwriteEffect != null)
                {
                    if (enable)
                    {
                        overwriteEffect.Overwrite(neweffect);
                    }
                    else
                    {
                        overwriteEffect.OverwriteDisabled(neweffect);
                    }
                }
                else
                {
                    if (enable)
                    {
                        neweffect.Start(target);
                    }
                    else
                    {
                        neweffect.StartDisabled(target);
                    }
                }
            }
            finally
            {
                target.EffectList.CommitChanges();
            }
        }

        /// <summary>
        /// Called when Effect is Added to target Effect List
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectAdd(GameSpellEffect effect)
        {
        }

        /// <summary>
        /// Check for Spell Effect Removed to Enable Best Cancellable
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="overwrite"></param>
        public virtual void OnEffectRemove(GameSpellEffect effect, bool overwrite)
        {
            if (!overwrite)
            {
                // Re-Enable Cancellable Effects.
                var enableEffect = effect.Owner.EffectList.OfType<GameSpellEffect>()
                    .Where(eff => eff != effect && eff.SpellHandler != null && eff.SpellHandler.IsOverwritable(effect) && eff.SpellHandler.IsCancellable(effect));

                // Find Best Remaining Effect
                GameSpellEffect best = null;
                foreach (var eff in enableEffect)
                {
                    if (best == null)
                    {
                        best = eff;
                    }
                    else if (best.SpellHandler.IsCancellableEffectBetter(best, eff))
                    {
                        best = eff;
                    }
                }

                if (best != null)
                {
                    effect.Owner.EffectList.BeginChanges();
                    try
                    {
                        // Enable Best Effect
                        best.EnableEffect();
                    }
                    finally
                    {
                        effect.Owner.EffectList.CommitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public virtual void OnDirectEffect(GameLiving target, double effectiveness)
        { }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectStart(GameSpellEffect effect)
        {
            if (Spell.Pulse == 0)
            {
                SendEffectAnimation(effect.Owner, 0, false, 1);
            }

            if (Spell.IsFocus) // Add Event handlers for focus spell
            {
                Caster.TempProperties.setProperty(FOCUS_SPELL, effect);
                GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            }
        }

        /// <summary>
        /// When an applied effect pulses
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectPulse(GameSpellEffect effect)
        {
            if (effect.Owner.IsAlive == false)
            {
                effect.Cancel(false);
            }
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public virtual int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            return 0;
        }

        /// <summary>
        /// Calculates chance of spell getting resisted
        /// </summary>
        /// <param name="target">the target of the spell</param>
        /// <returns>chance that spell will be resisted for specific target</returns>
        public virtual int CalculateSpellResistChance(GameLiving target)
        {
            if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || HasPositiveEffect)
            {
                return 0;
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects && m_spellItem != null)
            {
                if (Caster is GamePlayer playerCaster)
                {
                    int itemSpellLevel = m_spellItem.Template.LevelRequirement > 0 ? m_spellItem.Template.LevelRequirement : Math.Min(playerCaster.MaxLevel, m_spellItem.Level);
                    return 100 - (85 + ((itemSpellLevel - target.Level) / 2));
                }
            }

            return 100 - CalculateToHitChance(target);
        }

        /// <summary>
        /// When spell was resisted
        /// </summary>
        /// <param name="target">the target that resisted the spell</param>
        protected virtual void OnSpellResisted(GameLiving target)
        {
            SendSpellResistAnimation(target);
            SendSpellResistMessages(target);
            SendSpellResistNotification(target);
            StartSpellResistInterruptTimer(target);
            StartSpellResistLastAttackTimer(target);
        }

        /// <summary>
        /// Send Spell Resisted Animation
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistAnimation(GameLiving target)
        {
            if (Spell.Pulse == 0 || !HasPositiveEffect)
            {
                SendEffectAnimation(target, 0, false, 0);
            }
        }

        /// <summary>
        /// Send Spell Resist Messages to Caster and Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistMessages(GameLiving target)
        {
            // Deliver message to the target, if the target is a pet, to its
            // owner instead.
            if (target is GameNPC npc)
            {
                if (npc.Brain is IControlledBrain brain)
                {
                    GamePlayer owner = brain.GetPlayerOwner();
                    if (owner != null)
                    {
                        this.MessageToLiving(owner, eChatType.CT_SpellResisted, "Your {0} resists the effect!", target.Name);
                    }
                }
            }
            else
            {
                MessageToLiving(target, "You resist the effect!", eChatType.CT_SpellResisted);
            }

            // Deliver message to the caster as well.
            this.MessageToCaster(eChatType.CT_SpellResisted, "{0} resists the effect!", target.GetName(0, true));
        }

        /// <summary>
        /// Send Spell Attack Data Notification to Target when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistNotification(GameLiving target)
        {
            // Report resisted spell attack data to any type of living object, no need
            // to decide here what to do. For example, NPCs will use their brain.
            // "Just the facts, ma'am, just the facts."
            AttackData ad = new AttackData
            {
                Attacker = Caster,
                Target = target,
                AttackType = AttackData.eAttackType.Spell,
                SpellHandler = this,
                AttackResult = GameLiving.eAttackResult.Missed,
                IsSpellResisted = true
            };

            target.OnAttackedByEnemy(ad);
        }

        /// <summary>
        /// Start Spell Interrupt Timer when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void StartSpellResistInterruptTimer(GameLiving target)
        {
            // Spells that would have caused damage or are not instant will still
            // interrupt a casting player.
            if (!(Spell.SpellType.IndexOf("debuff", StringComparison.OrdinalIgnoreCase) >= 0 && Spell.CastTime == 0))
            {
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            }
        }

        /// <summary>
        /// Start Last Attack Timer when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void StartSpellResistLastAttackTimer(GameLiving target)
        {
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }
        }

        /// <summary>
        /// Sends a message to the caster, if the caster is a controlled
        /// creature, to the player instead (only spell hit and resisted
        /// messages).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void MessageToCaster(string message, eChatType type)
        {
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).MessageToSelf(message, type);
            }
            else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain
                     && (type == eChatType.CT_YouHit || type == eChatType.CT_SpellResisted))
            {
                GamePlayer owner = (((GameNPC) Caster).Brain as IControlledBrain)?.GetPlayerOwner();
                owner?.MessageFromControlled(message, type);
            }
        }

        /// <summary>
        /// sends a message to a living
        /// </summary>
        /// <param name="living"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void MessageToLiving(GameLiving living, string message, eChatType type)
        {
            if (!string.IsNullOrEmpty(message))
            {
                living.MessageToSelf(message, type);
            }
        }

        /// <summary>
        /// Hold events for focus spells
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void FocusSpellAction(DOLEvent e, object sender, EventArgs args)
        {
            if (!(sender is GameLiving living))
            {
                return;
            }

            GameSpellEffect currentEffect = (GameSpellEffect)living.TempProperties.getProperty<object>(FOCUS_SPELL, null);
            if (currentEffect == null)
            {
                return;
            }

            if (args is CastingEventArgs)
            {
                if ((args as CastingEventArgs).SpellHandler.Caster != Caster)
                {
                    return;
                }

                if ((args as CastingEventArgs).SpellHandler.Spell.SpellType == currentEffect.Spell.SpellType)
                {
                    return;
                }
            }

            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(currentEffect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            Caster.TempProperties.removeProperty(FOCUS_SPELL);

            if (currentEffect.Spell.Pulse != 0)
            {
                CancelPulsingSpell(Caster, currentEffect.Spell.SpellType);
            }
            else
            {
                currentEffect.Cancel(false);
            }

            MessageToCaster($"You lose your focus on your {currentEffect.Spell.Name} spell.", eChatType.CT_SpellExpires);

            if (e == GameLivingEvent.Moving)
            {
                MessageToCaster("You move and interrupt your focus!", eChatType.CT_Important);
            }
        }

        /// <summary>
        /// Ability to cast a spell
        /// </summary>
        public ISpellCastingAbilityHandler Ability { get; set; }

        /// <summary>
        /// The Spell
        /// </summary>
        public Spell Spell { get; }

        /// <summary>
        /// The Spell Line
        /// </summary>
        public SpellLine SpellLine { get; }

        /// <summary>
        /// The Caster
        /// </summary>
        public GameLiving Caster { get; }

        /// <summary>
        /// Is the spell being cast?
        /// </summary>
        public bool IsCasting => m_castTimer != null && m_castTimer.IsAlive;

        /// <summary>
        /// Does the spell have a positive effect?
        /// </summary>
        public virtual bool HasPositiveEffect
        {
            get
            {
                if (Spell.Target.ToLower() != "enemy" && Spell.Target.ToLower() != "cone" && Spell.Target.ToLower() != "area")
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Is this Spell purgeable
        /// </summary>
        public virtual bool IsUnPurgeAble => false;

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public byte DelveInfoDepth { get; set; }

        /// <summary>
        /// Delve Info
        /// </summary>
        public virtual IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(32);

                // list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                // list.Add(" "); //empty line
                GamePlayer p = null;

                if (Caster is GamePlayer || ((Caster as GameNPC)?.Brain as IControlledBrain)?.GetPlayerOwner() != null)
                {
                    p = Caster is GamePlayer player ? player : (((GameNPC) Caster).Brain as IControlledBrain)?.GetPlayerOwner();
                }

                list.Add(Spell.Description);
                list.Add(" "); // empty line
                if (Spell.InstrumentRequirement != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
                }

                if (Spell.Damage != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
                }

                if (Spell.LifeDrainReturn != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.HealthReturned", Spell.LifeDrainReturn) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.HealthReturned", Spell.LifeDrainReturn));
                }
                else if (Spell.Value != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")));
                }

                list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Target", Spell.Target) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
                if (Spell.Range != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Range", Spell.Range) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
                }

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add(p != null ? $"{LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration")} Permanent." : $"{LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration")} Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add(p != null ? $"{LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration")} {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min"
                        : $"{LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration")} {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add(p != null ? $"{LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration")} {(Spell.Duration / 1000):0' sec';'Permanent.';'Permanent.'}"
                        : $"{LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration")} {(Spell.Duration / 1000):0' sec';'Permanent.';'Permanent.'}");
                }

                if (Spell.Frequency != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                }

                if (Spell.Power != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                }

                list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                if (Spell.RecastDelay > 60000)
                {
                    list.Add(p != null ? $"{LanguageMgr.GetTranslation(p.Client, "DelveInfo.RecastTime")} {(Spell.RecastDelay / 60000)}:{(Spell.RecastDelay % 60000 / 1000):00} min"
                        : $"{LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime")} {(Spell.RecastDelay / 60000)}:{(Spell.RecastDelay % 60000 / 1000):00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add(p != null ? $"{LanguageMgr.GetTranslation(p.Client, "DelveInfo.RecastTime")} {(Spell.RecastDelay / 1000)} sec"
                        : $"{LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime")} {(Spell.RecastDelay / 1000)} sec");
                }

                if (Spell.Concentration != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.ConcentrationCost", Spell.Concentration) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.ConcentrationCost", Spell.Concentration));
                }

                if (Spell.Radius != 0)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Radius", Spell.Radius) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
                }

                if (Spell.DamageType != eDamageType.Natural)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
                }

                if (Spell.IsFocus)
                {
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Focus") : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Focus"));
                }

                return list;
            }
        }

        // warlock add
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType, string spellName)
        {
            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                    {
                        continue;
                    }

                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect immunityEffect && immunityEffect.ImmunityState)
                    {
                        continue; // ignore immunity effects
                    }

                    if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType) && (effect.SpellHandler.Spell.Name == spellName))
                    {
                        return effect;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find effect by spell type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType)
        {
            if (target == null)
            {
                return null;
            }

            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                    {
                        continue;
                    }

                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
                    {
                        continue; // ignore immunity effects
                    }

                    if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
                    {
                        return effect;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find effect by spell handler
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellHandler"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, ISpellHandler spellHandler)
        {
            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;
                    if (gsp == null)
                    {
                        continue;
                    }

                    if (gsp.SpellHandler != spellHandler)
                    {
                        continue;
                    }

                    if (gsp is GameSpellAndImmunityEffect immunityEffect && immunityEffect.ImmunityState)
                    {
                        continue; // ignore immunity effects
                    }

                    return gsp;
                }
            }

            return null;
        }

        /// <summary>
        /// Find effect by spell handler
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellHandler"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, Type spellHandler)
        {
            if (spellHandler.IsInstanceOfType(typeof(SpellHandler)) == false)
            {
                return null;
            }

            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;
                    if (gsp == null)
                    {
                        continue;
                    }

                    if (gsp.SpellHandler.GetType().IsInstanceOfType(spellHandler) == false)
                    {
                        continue;
                    }

                    if (gsp is GameSpellAndImmunityEffect immunityEffect && immunityEffect.ImmunityState)
                    {
                        continue; // ignore immunity effects
                    }

                    return gsp;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the target has the given static effect, false
        /// otherwise.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public static IGameEffect FindStaticEffectOnTarget(GameLiving target, Type effectType)
        {
            if (target == null)
            {
                return null;
            }

            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    if (effect.GetType() == effectType)
                    {
                        return effect;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find pulsing spell by spell handler
        /// </summary>
        /// <param name="living"></param>
        /// <param name="handler"></param>
        /// <returns>first occurance of spellhandler in targets' conc list or null</returns>
        public static PulsingSpellEffect FindPulsingSpellOnTarget(GameLiving living, ISpellHandler handler)
        {
            lock (living.ConcentrationEffects)
            {
                foreach (IConcentrationEffect concEffect in living.ConcentrationEffects)
                {
                    PulsingSpellEffect pulsingSpell = concEffect as PulsingSpellEffect;
                    if (pulsingSpell == null)
                    {
                        continue;
                    }

                    if (pulsingSpell.SpellHandler == handler)
                    {
                        return pulsingSpell;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Level mod for effect between target and caster if there is any
        /// </summary>
        /// <returns></returns>
        public virtual double GetLevelModFactor()
        {
            return 0.02;  // Live testing done Summer 2009 by Bluraven, Tolakram  Levels 40, 45, 50, 55, 60, 65, 70
        }

        /// <summary>
        /// Calculates min damage variance %
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="min">returns min variance</param>
        /// <param name="max">returns max variance</param>
        public virtual void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
            {
                min = 1.0;
                max = 1.25;
                return;
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
            {
                if (UseMinVariance)
                {
                    min = 1.50;
                }
                else
                {
                    min = 1.00;
                }

                max = 1.50;

                return;
            }

            if (SpellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
            {
                min = max = 1.0;
                return;
            }

            int speclevel = 1;

            if (Caster is GamePet)
            {
                IControlledBrain brain = ((GameNPC) Caster).Brain as IControlledBrain;
                speclevel = brain.GetLivingOwner().Level;
            }
            else if (Caster is GamePlayer)
            {
                speclevel = ((GamePlayer)Caster).GetModifiedSpecLevel(SpellLine.Spec);
            }

            min = 1.25;
            max = 1.25;

            if (target.Level > 0)
            {
                min = 0.25 + (speclevel - 1) / (double)target.Level;
            }

            if (speclevel - 1 > target.Level)
            {
                double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
                min += overspecBonus;
                max += overspecBonus;
            }

            // add level mod
            if (Caster is GamePlayer)
            {
                min += GetLevelModFactor() * (Caster.Level - target.Level);
                max += GetLevelModFactor() * (Caster.Level - target.Level);
            }
            else if (Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
            {
                // Get the root owner
                GameLiving owner = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
                if (owner != null)
                {
                    min += GetLevelModFactor() * (owner.Level - target.Level);
                    max += GetLevelModFactor() * (owner.Level - target.Level);
                }
            }

            if (max < 0.25)
            {
                max = 0.25;
            }

            if (min > max)
            {
                min = max;
            }

            if (min < 0)
            {
                min = 0;
            }
        }

        /// <summary>
        /// Player pet damage cap
        /// This simulates a player casting a baseline nuke with the capped damage near (but not exactly) that of the equivilent spell of the players level.
        /// This cap is not applied if the player is level 50
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual double CapPetSpellDamage(double damage, GamePlayer player)
        {
            double cappedDamage = damage;

            if (player.Level < 13)
            {
                cappedDamage = 4.1 * player.Level;
            }

            if (player.Level < 50)
            {
                cappedDamage = 3.8 * player.Level;
            }

            return Math.Min(damage, cappedDamage);
        }

        /// <summary>
        /// Put a calculated cap on NPC damage to solve a problem where an npc is given a high level spell but needs damage
        /// capped to the npc level.  This uses player spec nukes to calculate damage cap.
        /// NPC's level 50 and above are not capped
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual double CapNPCSpellDamage(double damage, GameNPC npc)
        {
            if (npc.Level < 50)
            {
                return Math.Min(damage, 4.7 * npc.Level);
            }

            return damage;
        }

        /// <summary>
        /// Calculates the base 100% spell damage which is then modified by damage variance factors
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateDamageBase(GameLiving target)
        {
            double spellDamage = Spell.Damage;
            GamePlayer player = Caster as GamePlayer;

            // For pets the stats of the owner have to be taken into account.
            if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
            {
                player = ((Caster as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
            }

            if (player != null)
            {
                if (Caster is GamePet)
                {
                    spellDamage = CapPetSpellDamage(spellDamage, player);
                }

                if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
                {
                    double WeaponSkill = player.GetWeaponSkill(player.AttackWeapon);
                    WeaponSkill /= 5;
                    spellDamage *= (WeaponSkill + 200) / 275.0;
                }

                if (player.CharacterClass.ManaStat != eStat.UNDEFINED
                    && SpellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect
                    && SpellLine.KeyName != GlobalSpellsLines.Mundane_Poisons
                    && SpellLine.KeyName != GlobalSpellsLines.Item_Effects
                    && player.CharacterClass.ID != (int)eCharacterClass.MaulerAlb
                    && player.CharacterClass.ID != (int)eCharacterClass.MaulerMid
                    && player.CharacterClass.ID != (int)eCharacterClass.MaulerHib
                    && player.CharacterClass.ID != (int)eCharacterClass.Vampiir)
                {
                    int manaStatValue = player.GetModified((eProperty)player.CharacterClass.ManaStat);
                    spellDamage *= (manaStatValue + 200) / 275.0;
                }
            }
            else if (Caster is GameNPC)
            {
                var npc = (GameNPC)Caster;
                int manaStatValue = npc.GetModified(eProperty.Intelligence);
                spellDamage = CapNPCSpellDamage(spellDamage, npc) * (manaStatValue + 200) / 275.0;
            }

            if (spellDamage < 0)
            {
                spellDamage = 0;
            }

            return spellDamage;
        }

        /// <summary>
        /// Calculates the chance that the spell lands on target
        /// can be negative or above 100%
        /// </summary>
        /// <param name="target">spell target</param>
        /// <returns>chance that the spell lands on target</returns>
        public virtual int CalculateToHitChance(GameLiving target)
        {
            int spellLevel = Spell.Level;

            GameLiving caster = null;
            if (Caster is GameNPC npc && npc.Brain is ControlledNpcBrain)
            {
                caster = ((ControlledNpcBrain)npc.Brain).Owner;
            }
            else
            {
                caster = Caster;
            }

            int spellbonus = caster.GetModified(eProperty.SpellLevel);
            spellLevel += spellbonus;

            GamePlayer playerCaster = caster as GamePlayer;

            if (spellLevel > playerCaster?.MaxLevel)
            {
                spellLevel = playerCaster.MaxLevel;
            }

            GameSpellEffect effect = FindEffectOnTarget(Caster, "HereticPiercingMagic");
            if (effect != null)
            {
                spellLevel += (int)effect.Spell.Value;
            }

            if (playerCaster != null && (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || SpellLine.KeyName.StartsWith(GlobalSpellsLines.Champion_Lines_StartWith)))
            {
                spellLevel = Math.Min(playerCaster.MaxLevel, target.Level);
            }

            int bonustohit = Caster.GetModified(eProperty.ToHitBonus);

            // Piercing Magic affects to-hit bonus too
            GameSpellEffect resPierce = FindEffectOnTarget(Caster, "PenetrateResists");
            if (resPierce != null)
            {
                bonustohit += (int)resPierce.Spell.Value;
            }

            /*
            http://www.camelotherald.com/news/news_article.php?storyid=704

            Q: Spell resists. Can you give me more details as to how the system works?

            A: Here's the answer, straight from the desk of the spell designer:

            "Spells have a factor of (spell level / 2) added to their chance to hit. (Spell level defined as the level the spell is awarded, chance to hit defined as
            the chance of avoiding the "Your target resists the spell!" message.) Subtracted from the modified to-hit chance is the target's (level / 2).
            So a L50 caster casting a L30 spell at a L50 monster or player, they have a base chance of 85% to hit, plus 15%, minus 25% for a net chance to hit of 75%.
            If the chance to hit goes over 100% damage or duration is increased, and if it goes below 55%, you still have a 55% chance to hit but your damage
            or duration is penalized. If the chance to hit goes below 0, you cannot hit at all. Once the spell hits, damage and duration are further modified
            by resistances.

            Note:  The last section about maintaining a chance to hit of 55% has been proven incorrect with live testing.  The code below is very close to live like.
            - Tolakram
             */

            int hitchance = 85 + ((spellLevel - target.Level) / 2) + bonustohit;

            if (!(caster is GamePlayer && target is GamePlayer))
            {
                hitchance -= (int)(Caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
                hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
            }

            // [Freya] Nidel: Harpy Cloak : They have less chance of landing melee attacks, and spells have a greater chance of affecting them.
            if (target is GamePlayer)
            {
                GameSpellEffect harpyCloak = FindEffectOnTarget(target, "HarpyFeatherCloak");
                if (harpyCloak != null)
                {
                    hitchance += (int)((hitchance * harpyCloak.Spell.Value) * 0.01);
                }
            }

            return hitchance;
        }

        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <returns>attack data</returns>
        public AttackData CalculateDamageToTarget(GameLiving target)
        {
            return CalculateDamageToTarget(target, 1);
        }

        /// <summary>
        /// Adjust damage based on chance to hit.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="hitChance"></param>
        /// <returns></returns>
        public virtual int AdjustDamageForHitChance(int damage, int hitChance)
        {
            int adjustedDamage = damage;

            if (hitChance < 55)
            {
                adjustedDamage += (int)(adjustedDamage * (hitChance - 55) * ServerProperties.Properties.SPELL_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER * 0.01);
            }

            return Math.Max(adjustedDamage, 1);
        }

        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="effectiveness">value from 0..1 to modify damage</param>
        /// <returns>attack data</returns>
        public virtual AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = new AttackData
            {
                Attacker = Caster,
                Target = target,
                AttackType = AttackData.eAttackType.Spell,
                SpellHandler = this,
                AttackResult = GameLiving.eAttackResult.HitUnstyled
            };

            CalculateDamageVariance(target, out var minVariance, out var maxVariance);
            double spellDamage = CalculateDamageBase(target);

            if (Caster is GamePlayer)
            {
                effectiveness += Caster.GetModified(eProperty.SpellDamage) * 0.01;

                // Relic bonus applied to damage, does not alter effectiveness or increase cap
                spellDamage *= 1.0 + RelicMgr.GetRelicBonusModifier(Caster.Realm, eRelicType.Magic);
            }

            // Apply casters effectiveness
            spellDamage *= Caster.Effectiveness;

            int finalDamage = Util.Random((int)(minVariance * spellDamage), (int)(maxVariance * spellDamage));

            // Live testing done Summer 2009 by Bluraven, Tolakram  Levels 40, 45, 50, 55, 60, 65, 70
            // Damage reduced by chance < 55, no extra damage increase noted with hitchance > 100
            int hitChance = CalculateToHitChance(ad.Target);
            finalDamage = AdjustDamageForHitChance(finalDamage, hitChance);

            // apply spell effectiveness
            finalDamage = (int)(finalDamage * effectiveness);

            if (Caster is GamePlayer || (Caster as GameNPC)?.Brain is IControlledBrain && Caster.Realm != 0)
            {
                if (target is GamePlayer)
                {
                    finalDamage = (int)(finalDamage * ServerProperties.Properties.PVP_SPELL_DAMAGE);
                }
                else if (target is GameNPC)
                {
                    finalDamage = (int)(finalDamage * ServerProperties.Properties.PVE_SPELL_DAMAGE);
                }
            }

            // Well the PenetrateResistBuff is NOT ResistPierce
            GameSpellEffect penPierce = FindEffectOnTarget(Caster, "PenetrateResists");
            if (penPierce != null)
            {
                finalDamage = (int)(finalDamage * (1.0 + penPierce.Spell.Value / 100.0));
            }

            int cdamage = 0;
            if (finalDamage < 0)
            {
                finalDamage = 0;
            }

            eDamageType damageType = DetermineSpellDamageType();

            eProperty property = target.GetResistTypeForDamage(damageType);

            // The Daoc resistsystem is since 1.65 a 2category system.
            // - First category are Item/Race/Buff/RvrBanners resists that are displayed in the characteroverview.
            // - Second category are resists that are given through RAs like avoidance of magic, brilliance aura of deflection.
            //   Those resist affect ONLY the spelldamage. Not the duration, not the effectiveness of debuffs.
            // so calculation is (finaldamage * Category1Modification) * Category2Modification
            // -> Remark for the future: VampirResistBuff is Category2 too.
            // - avi
            int primaryResistModifier = ad.Target.GetResist(damageType);

            /* Resist Pierce
             * Resipierce is a special bonus which has been introduced with TrialsOfAtlantis.
             * At the calculation of SpellDamage, it reduces the resistance that the victim recives
             * through ITEMBONUSES for the specified percentage.
             * http://de.daocpedia.eu/index.php/Resistenz_durchdringen (translated)
             */
            int resiPierce = Caster.GetModified(eProperty.ResistPierce);
            if (resiPierce > 0 && Spell.SpellType != "Archery")
            {
                // substract max ItemBonus of property of target, but atleast 0.
                primaryResistModifier -= Math.Max(0, Math.Min(ad.Target.ItemBonus[(int)property], resiPierce));
            }

            // Using the resist BuffBonusCategory2 - its unused in ResistCalculator
            int secondaryResistModifier = target.SpecBuffBonusCategory[(int)property];

            if (secondaryResistModifier > 80)
            {
                secondaryResistModifier = 80;
            }

            int resistModifier = 0;

            // primary resists
            resistModifier += (int)(finalDamage * (double)primaryResistModifier * -0.01);

            // secondary resists
            resistModifier += (int)((finalDamage + (double)resistModifier) * secondaryResistModifier * -0.01);

            // apply resists
            finalDamage += resistModifier;

            // Apply damage cap (this can be raised by effectiveness)
            if (finalDamage > DamageCap(effectiveness))
            {
                finalDamage = (int)DamageCap(effectiveness);
            }

            if (finalDamage < 0)
            {
                finalDamage = 0;
            }

            int criticalchance = Caster.SpellCriticalChance;
            if (Util.Chance(Math.Min(50, criticalchance)) && (finalDamage >= 1))
            {
                int critmax = ad.Target is GamePlayer ? finalDamage / 2 : finalDamage;
                cdamage = Util.Random(finalDamage / 10, critmax); // think min crit is 10% of damage
            }

            // Andraste
            int manaconversion = (int)Math.Round((ad.Damage + (double)ad.CriticalDamage) * ad.Target.GetModified(eProperty.Conversion) / 200);
            if (ad.Target is GamePlayer && ad.Target.GetModified(eProperty.Conversion) > 0)
            {
                // int enduconversion=(int)Math.Round((double)manaconversion*(double)ad.Target.MaxEndurance/(double)ad.Target.MaxMana);
                int enduconversion = (int)Math.Round((ad.Damage + (double)ad.CriticalDamage) * ad.Target.GetModified(eProperty.Conversion) / 200);
                if (ad.Target.Mana + manaconversion > ad.Target.MaxMana)
                {
                    manaconversion = ad.Target.MaxMana - ad.Target.Mana;
                }

                if (ad.Target.Endurance + enduconversion > ad.Target.MaxEndurance)
                {
                    enduconversion = ad.Target.MaxEndurance - ad.Target.Endurance;
                }

                if (manaconversion < 1)
                {
                    manaconversion = 0;
                }

                if (enduconversion < 1)
                {
                    enduconversion = 0;
                }

                if (manaconversion >= 1)
                {
                    (ad.Target as GamePlayer).Out.SendMessage($"You gain {manaconversion} power points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                if (enduconversion >= 1)
                {
                    (ad.Target as GamePlayer).Out.SendMessage($"You gain {enduconversion} endurance points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                ad.Target.Endurance += enduconversion; if (ad.Target.Endurance > ad.Target.MaxEndurance)
                {
                    ad.Target.Endurance = ad.Target.MaxEndurance;
                }

                ad.Target.Mana += manaconversion; if (ad.Target.Mana > ad.Target.MaxMana)
                {
                    ad.Target.Mana = ad.Target.MaxMana;
                }
            }

            ad.Damage = finalDamage;
            ad.CriticalDamage = cdamage;
            ad.DamageType = damageType;
            ad.Modifier = resistModifier;

            // Attacked living may modify the attack data.  Primarily used for keep doors and components.
            ad.Target.ModifyAttack(ad);

            m_lastAttackData = ad;
            return ad;
        }

        public virtual double DamageCap(double effectiveness)
        {
            return Spell.Damage * 3.0 * effectiveness;
        }

        /// <summary>
        /// What damage type to use.  Overriden by archery
        /// </summary>
        /// <returns></returns>
        public virtual eDamageType DetermineSpellDamageType()
        {
            return Spell.DamageType;
        }

        /// <summary>
        /// Sends damage text messages but makes no damage
        /// </summary>
        /// <param name="ad"></param>
        public virtual void SendDamageMessages(AttackData ad)
        {
            string modmessage = string.Empty;
            if (ad.Modifier > 0)
            {
                modmessage = $" (+{ad.Modifier})";
            }

            if (ad.Modifier < 0)
            {
                modmessage = $" ({ad.Modifier})";
            }

            if (Caster is GamePlayer || Caster is NecromancerPet)
            {
                MessageToCaster($"You hit {ad.Target.GetName(0, false)} for {ad.Damage}{modmessage} damage!", eChatType.CT_YouHit);
            }
            else if (Caster is GameNPC)
            {
                MessageToCaster($"Your {Caster.Name} hits {ad.Target.GetName(0, false)} for {ad.Damage}{modmessage} damage!", eChatType.CT_YouHit);
            }

            if (ad.CriticalDamage > 0)
            {
                MessageToCaster($"You critically hit for an additional {ad.CriticalDamage} damage!", eChatType.CT_YouHit);
            }
        }

        /// <summary>
        /// Make damage to target and send spell effect but no messages
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="showEffectAnimation"></param>
        public virtual void DamageTarget(AttackData ad, bool showEffectAnimation)
        {
            DamageTarget(ad, showEffectAnimation, 0x14); // spell damage attack result
        }

        /// <summary>
        /// Make damage to target and send spell effect but no messages
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="showEffectAnimation"></param>
        /// <param name="attackResult"></param>
        public virtual void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
        {
            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            if (showEffectAnimation)
            {
                SendEffectAnimation(ad.Target, 0, false, 1);
            }

            if (ad.Damage > 0)
            {
                foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);
                }
            }

            // send animation before dealing damage else dead livings show no animation
            ad.Target.OnAttackedByEnemy(ad);
            ad.Attacker.DealDamage(ad);
            if (ad.Damage == 0 && ad.Target is GameNPC)
            {
                if (((GameNPC)ad.Target).Brain is IOldAggressiveBrain aggroBrain)
                {
                    aggroBrain.AddToAggroList(Caster, 1);
                }
            }

            m_lastAttackData = ad;
        }

        public virtual PlayerXEffect GetSavedEffect(GameSpellEffect effect)
        {
            return null;
        }

        public virtual void OnEffectRestored(GameSpellEffect effect, int[] vars)
        { }

        public virtual int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
        {
            return 0;
        }

        /// <summary>
        /// Return the given Delve Writer with added keyvalue pairs.
        /// </summary>
        /// <param name="dw"></param>
        /// <param name="id"></param>
        public virtual void TooltipDelve(ref MiniDelveWriter dw, int id)
        {
            if (dw == null)
            {
                return;
            }

            dw.AddKeyValuePair("Function", "light"); // Function of type "light" allows custom description to show with no hardcoded text.  Temporary Fix - tolakram
            // .Value("Function", spellHandler.FunctionName ?? "0")
            dw.AddKeyValuePair("Index", unchecked((short)id));
            dw.AddKeyValuePair("Name", Spell.Name);

            if (Spell.CastTime > 2000)
            {
                dw.AddKeyValuePair("cast_timer", Spell.CastTime - 2000); // minus 2 seconds (why mythic?)
            }
            else if (!Spell.IsInstantCast)
            {
                dw.AddKeyValuePair("cast_timer", 0); // minus 2 seconds (why mythic?)
            }

            if (Spell.IsInstantCast)
            {
                dw.AddKeyValuePair("instant","1");
            }

            // .Value("damage", spellHandler.GetDelveValueDamage, spellHandler.GetDelveValueDamage != 0)
            if ((int)Spell.DamageType > 0)
            {
                dw.AddKeyValuePair("damage_type", (int)Spell.DamageType + 1); // Damagetype not the same as dol
            }

            // .Value("type1", spellHandler.GetDelveValueType1, spellHandler.GetDelveValueType1 > 0)
            if (Spell.Level > 0)
            {
                dw.AddKeyValuePair("level", Spell.Level);
            }

            if (Spell.CostPower)
            {
                dw.AddKeyValuePair("power_cost", Spell.Power);
            }

            // .Value("round_cost",spellHandler.GetDelveValueRoundCost,spellHandler.GetDelveValueRoundCost!=0)
            // .Value("power_level", spellHandler.GetDelveValuePowerLevel,spellHandler.GetDelveValuePowerLevel!=0)
            if (Spell.Range > 0)
            {
                dw.AddKeyValuePair("range", Spell.Range);
            }

            if (Spell.Duration > 0)
            {
                dw.AddKeyValuePair("duration", Spell.Duration / 1000); // seconds
            }

            if (GetDurationType() > 0)
            {
                dw.AddKeyValuePair("dur_type", GetDurationType());
            }

            // .Value("parm",spellHandler.GetDelveValueParm,spellHandler.GetDelveValueParm>0)
            if (Spell.HasRecastDelay)
            {
                dw.AddKeyValuePair("timer_value", Spell.RecastDelay / 1000);
            }

            // .Value("bonus", spellHandler.GetDelveValueBonus, spellHandler.GetDelveValueBonus > 0)
            // .Value("no_combat"," ",Util.Chance(50))//TODO
            // .Value("link",14000)
            // .Value("ability",4) // ??
            // .Value("use_timer",4)
            if (GetSpellTargetType() > 0)
            {
                dw.AddKeyValuePair("target", GetSpellTargetType());
            }

            // .Value("frequency", spellHandler.GetDelveValueFrequency, spellHandler.GetDelveValueFrequency != 0)
            if (!string.IsNullOrEmpty(Spell.Description))
            {
                dw.AddKeyValuePair("description_string", Spell.Description);
            }

            if (Spell.IsAoE)
            {
                dw.AddKeyValuePair("radius", Spell.Radius);
            }

            if (Spell.IsConcentration)
            {
                dw.AddKeyValuePair("concentration_points", Spell.Concentration);
            }

            // .Value("num_targets", spellHandler.GetDelveValueNumTargets, spellHandler.GetDelveValueNumTargets>0)
            // .Value("no_interrupt", spell.Interruptable ? (char)0 : (char)1) //Buggy?
            // log.Info(dw.ToString());
        }

        /// <summary>
        /// Returns delve code for target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual int GetSpellTargetType()
        {
            switch (Spell.Target)
            {
                case "Realm":
                    return 7;
                case "Self":
                    return 0;
                case "Enemy":
                    return 1;
                case "Pet":
                    return 6;
                case "Group":
                    return 3;
                case "Area":
                    return 0; // TODO
                default:
                    return 0;
            }
        }

        protected virtual int GetDurationType()
        {
            // 2-seconds,4-conc,5-focus
            if (Spell.Duration > 0)
            {
                return 2;
            }

            if (Spell.IsConcentration)
            {
                return 4;
            }

            return 0;
        }
    }
}
