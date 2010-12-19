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
using System.Collections.Generic;
using System.Reflection;
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
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Maximum number of sub-spells to get delve info for.
		/// </summary>
		protected static readonly byte MAX_DELVE_RECURSION = 5;

		protected DelayedCastTimer m_castTimer;
		/// <summary>
		/// The spell that we want to handle
		/// </summary>
		protected Spell m_spell;
		/// <summary>
		/// The spell line the spell belongs to
		/// </summary>
		protected SpellLine m_spellLine;
		/// <summary>
		/// The caster of the spell
		/// </summary>
		protected GameLiving m_caster;
		/// <summary>
		/// The target for this spell
		/// </summary>
		protected GameLiving m_spellTarget = null;
		/// <summary>
		/// Has the spell been interrupted
		/// </summary>
		protected bool m_interrupted = false;
		/// <summary>
		/// Delayedcast Stage
		/// </summary>
		public int Stage
		{
			get { return m_stage; }
			set { m_stage = value; }
		}
		protected int m_stage = 0;
		/// <summary>
		/// Use to store Time when the delayedcast started
		/// </summary>
		protected long m_started = 0;
		/// <summary>
		/// Shall we start the reuse timer
		/// </summary>
		protected bool m_startReuseTimer = true;

		public bool StartReuseTimer
		{
			get { return m_startReuseTimer; }
		}

		/// <summary>
		/// Can this spell be queued with other spells?
		/// </summary>
		public virtual bool CanQueue
		{
			get { return true; }
		}


		protected InventoryItem m_spellItem = null;

		/// <summary>
		/// Ability that casts a spell
		/// </summary>
		protected SpellCastingAbilityHandler m_ability = null;

		/// <summary>
		/// Stores the current delve info depth
		/// </summary>
		private byte m_delveInfoDepth;

		/// <summary>
		/// AttackData result for this spell, if any
		/// </summary>
		protected AttackData m_lastAttackData = null;
		/// <summary>
		/// AttackData result for this spell, if any
		/// </summary>
		public AttackData LastAttackData
		{
			get { return m_lastAttackData; }
		}

		/// <summary>
		/// The property key for the interrupt timeout
		/// </summary>
		public const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";
		/// <summary>
		/// The property key for focus spells
		/// </summary>
		protected const string FOCUS_SPELL = "FOCUSING_A_SPELL";

		protected bool m_ignoreDamageCap = false;

		/// <summary>
		/// Does this spell ignore any damage cap?
		/// </summary>
		public bool IgnoreDamageCap
		{
			get { return m_ignoreDamageCap; }
			set { m_ignoreDamageCap = value; }
		}

		protected bool m_useMinVariance = false;

		/// <summary>
		/// Should this spell use the minimum variance for the type?
		/// Followup style effects, for example, always use the minimum variance
		/// </summary>
		public bool UseMinVariance
		{
			get { return m_useMinVariance; }
			set { m_useMinVariance = value; }
		}


		/// <summary>
		/// The CastingCompleteEvent
		/// </summary>
		public event CastingCompleteCallback CastingCompleteEvent;
		//public event SpellEndsCallback SpellEndsEvent;

		/// <summary>
		/// spell handler constructor
		/// <param name="caster">living that is casting that spell</param>
		/// <param name="spell">the spell to cast</param>
		/// <param name="spellLine">the spell line that spell belongs to</param>
		/// </summary>
		public SpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
		{
			m_caster = caster;
			m_spell = spell;
			m_spellLine = spellLine;
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
				.Append("\nSpell: ").Append(Spell == null ? "(null)" : Spell.ToString())
				.Append("\nSpellLine: ").Append(SpellLine == null ? "(null)" : SpellLine.ToString())
				.ToString();
		}

		#region Pulsing Spells

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
				return;
			if (Caster.IsStunned || Caster.IsMezzed)
				return;

			// no instrument anymore = stop the song
			if (m_spell.InstrumentRequirement != 0 && !CheckInstrument())
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

				if (m_spell.SubSpellID > 0 && Spell.SpellType != "Archery" && Spell.SpellType != "Bomber" && Spell.SpellType != "SummonAnimistFnF" && Spell.SpellType != "SummonAnimistPet" && Spell.SpellType != "Grapple")
				{
					Spell spell = SkillBase.GetSpellByID(m_spell.SubSpellID);
					//we need subspell ID to be 0, we don't want spells linking off the subspell
					if (spell != null && spell.SubSpellID == 0)
					{
						ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
						spellhandler.StartSpell(m_spellTarget);
					}
				}

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
			if (instrument == null || instrument.Object_Type != (int)eObjectType.Instrument ) // || (instrument.DPS_AF != 4 && instrument.DPS_AF != m_spell.InstrumentRequirement))
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
						continue;
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
						continue;

					if ( player != null && player.CharacterClass.MaxPulsingSpells > 1 )
						pulsingSpells.Add( effect );
					else
						effect.Cancel(false);
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
			if ( pulsingSpells.Count > 1 )
			{
				pulsingSpells[pulsingSpells.Count - 1].Cancel( false );
			}
		}

		#endregion

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
			bool success = true;

			m_spellTarget = targetObject;

			Caster.Notify(GameLivingEvent.CastStarting, m_caster, new CastingEventArgs(this));

			if (Caster is GamePlayer && Spell.SpellType != "Archery")
				((GamePlayer)Caster).Stealth(false);

			if (Caster.IsEngaging)
			{
				EngageEffect effect = (EngageEffect)Caster.EffectList.GetOfType(typeof(EngageEffect));

				if (effect != null)
					effect.Cancel(false);
			}

			m_interrupted = false;

			if (Spell.Target.ToLower() == "pet")
			{
				// Pet is the target, check if the caster is the pet.

				if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
					m_spellTarget = Caster;

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
					m_spellTarget = Caster.ControlledBrain.Body;
				else
					m_spellTarget = null;
			}

			if (Spell.Pulse != 0 && CancelPulsingSpell(Caster, Spell.SpellType))
			{
				if (Spell.InstrumentRequirement == 0)
					MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
				else
					MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
			}
			else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, m_spellTarget, Spell, m_spellLine))
			{
				if (CheckBeginCast(m_spellTarget))
				{
					if (m_caster is GamePlayer && (m_caster as GamePlayer).IsOnHorse && !HasPositiveEffect)
					{
						(m_caster as GamePlayer).IsOnHorse = false;
					}

					if (!Spell.IsInstantCast)
					{
						StartCastTimer(m_spellTarget);

						if ((Caster is GamePlayer && (Caster as GamePlayer).IsStrafing) || Caster.IsMoving)
							CasterMoves();
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
				OnAfterSpellCastSequence();

			return success;
		}


		public virtual void StartCastTimer(GameLiving target)
		{
			m_interrupted = false;
			SendSpellMessages();

			int time = CalculateCastingTime();

			int step1 = time / 3;
			if (step1 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
				step1 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
			if (step1 < 1)
				step1 = 1;

			int step3 = time / 3;
			if (step3 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
				step3 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
			if (step3 < 1)
				step3 = 1;

			int step2 = time - step1 - step3;
			if (step2 < 1)
				step2 = 1;

			if (Caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG)
			{
				(Caster as GamePlayer).Out.SendMessage("[DEBUG] spell time = " + time + ", step1 = " + step1 + ", step2 = " + step2 + ", step3 = " + step3, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			m_castTimer = new DelayedCastTimer(Caster, this, target, step2, step3);
			m_castTimer.Start(step1);
			m_started = Caster.CurrentRegion.Time;
			SendCastAnimation();

			if (m_caster.IsMoving || m_caster.IsStrafing)
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
				return;

			if (Spell.MoveCast)
				return;

			InterruptCasting();
			MessageToCaster("You move and interrupt your spellcast!", eChatType.CT_Important);
		}

		/// <summary>
		/// This sends the spell messages to the player/target.
		///</summary>
		public virtual void SendSpellMessages()
		{
			if (Spell.InstrumentRequirement == 0)
				MessageToCaster("You begin casting a " + Spell.Name + " spell!", eChatType.CT_Spell);
			else
				MessageToCaster("You begin playing " + Spell.Name + "!", eChatType.CT_Spell);
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
            //[StephenxPimentel] Check if the necro has MoC effect before interrupting.
            if (Caster is NecromancerPet)
            {
                if ((Caster as NecromancerPet).Owner.EffectList.GetOfType(typeof (MasteryofConcentrationEffect)) != null)
                {
                    return false;
                }
			}
			if (Spell.Uninterruptible)
				return false;
			if (Caster.EffectList.GetOfType(typeof(QuickCastEffect)) != null)
				return false;
			if (Caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) != null)
				return false;
			if (Caster.EffectList.GetOfType(typeof(FacilitatePainworkingEffect)) != null)
				return false;
			if (IsCasting && Stage < 2)
			{
				double mod = Caster.GetConLevel(attacker);
				double chance = Caster.BaseInterruptChance;
				chance += mod * 10;
				chance = Math.Max(1, chance);
				chance = Math.Min(99, chance);
				if(attacker is GamePlayer) chance=99;

				if (Util.Chance((int)chance))
				{
					Caster.LastInterruptMessage = attacker.GetName(0, true) + " attacks you and your spell is interrupted!";
					MessageToLiving(Caster, Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
					InterruptCasting(); // always interrupt at the moment
					return true;
				}
			}
			return false;
		}

		#region begin & end cast check

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
			if (m_caster.ObjectState != GameLiving.eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.IsAlive)
			{
				if(!quiet) MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}


            if (m_caster is GamePlayer)
            {
                long nextSpellAvailTime = m_caster.TempProperties.getProperty<long>(GamePlayer.NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION);

                if (nextSpellAvailTime > m_caster.CurrentRegion.Time)
                {
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client, "GamePlayer.CastSpell.MustWaitBeforeCast", (nextSpellAvailTime - m_caster.CurrentRegion.Time) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                if (((GamePlayer)m_caster).Steed != null && ((GamePlayer)m_caster).Steed is GameSiegeRam)
                {
                    if (!quiet) MessageToCaster("You can't cast in a siegeram!.", eChatType.CT_System);
                    return false;
                }
            }

			GameSpellEffect Phaseshift = FindEffectOnTarget(Caster, "Phaseshift");
			if (Phaseshift != null && (Spell.InstrumentRequirement == 0 || Spell.SpellType == "Mesmerize"))
			{
				if (!quiet) MessageToCaster("You're phaseshifted and can't cast a spell", eChatType.CT_System);
				return false;
			}

			// Apply Mentalist RA5L
			if (Spell.Range>0)
			{
				SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
				if (SelectiveBlindness != null)
				{
					GameLiving EffectOwner = SelectiveBlindness.EffectSource;
					if(EffectOwner==selectedTarget)
					{
						if (m_caster is GamePlayer && !quiet)
							((GamePlayer)m_caster).Out.SendMessage(string.Format("{0} is invisible to you!", selectedTarget.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

						return false;
					}
				}
			}

			if (selectedTarget!=null && selectedTarget.HasAbility("DamageImmunity") && Spell.SpellType == "DirectDamage" && Spell.Radius == 0)
			{
				if (!quiet) MessageToCaster(selectedTarget.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				if (!CheckInstrument())
				{
					if (!quiet) MessageToCaster("You are not wielding the right type of instrument!",
					                            eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.IsSitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				if (!quiet) MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster.AttackState && m_spell.CastTime != 0)
			{
				if (m_caster.CanCastInCombat(Spell) == false)
				{
					m_caster.StopAttack();
					return false;
				}
			}

			if (!m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer && m_caster.EffectList.GetOfType(typeof(QuickCastEffect)) == null && m_caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) == null)
			{
				if (Caster.InterruptAction > 0 && Caster.InterruptAction + Caster.SpellInterruptRecastTime > Caster.CurrentRegion.Time)
				{
					if (!quiet) MessageToCaster("You must wait " + (((Caster.InterruptAction + Caster.SpellInterruptRecastTime) - Caster.CurrentRegion.Time) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
					return false;
				}
			}

			if (m_spell.RecastDelay > 0)
			{
				int left = m_caster.GetSkillDisabledDuration(m_spell);
				if (left > 0)
				{
					if (m_caster is NecromancerPet && ((m_caster as NecromancerPet).Owner as GamePlayer).Client.Account.PrivLevel > (int)ePrivLevel.Player)
					{
						// Ignore Recast Timer
					}
					else
					{
						if (!quiet) MessageToCaster("You must wait " + (left / 1000 + 1).ToString() + " seconds to use this spell!", eChatType.CT_System);
						return false;
					}
				}
			}

			String targetType = m_spell.Target.ToLower();

			//[Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
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
						if (!quiet) MessageToCaster("You must cast this spell on a creature you are controlling.",
						                            eChatType.CT_System);
						return false;
					}
				}
			}
			if (targetType == "area")
			{
				if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
				{
					if (!quiet) MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
				if (!Caster.GroundTargetInView)
				{
					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (targetType != "self" && targetType != "group" && targetType != "pet"
			         && targetType != "controlled" && targetType != "cone" && m_spell.Range > 0)
			{
				// All spells that need a target.

				if (selectedTarget == null || selectedTarget.ObjectState != GameLiving.eObjectState.Active)
				{
					if (!quiet) MessageToCaster("You must select a target for this spell!",
					                            eChatType.CT_SpellResisted);
					return false;
				}

				if (!m_caster.IsWithinRadius(selectedTarget, CalculateSpellRange()))
				{
					if(Caster is GamePlayer && !quiet) MessageToCaster("That target is too far away!",
					                                                   eChatType.CT_SpellResisted);
					Caster.Notify(GameLivingEvent.CastFailed,
					              new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
					return false;
				}

				switch (m_spell.Target.ToLower())
				{
					case "enemy":
						if (selectedTarget == m_caster)
						{
							if (!quiet) MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
							return false;
						}

						if (FindStaticEffectOnTarget(selectedTarget, typeof(NecromancerShadeEffect)) != null)
						{
							if (!quiet) MessageToCaster("Invalid target.", eChatType.CT_System);
							return false;
						}

						if (m_spell.SpellType == "Charm" && m_spell.CastTime == 0 && m_spell.Pulse != 0)
							break;

						if (m_caster.IsObjectInFront(selectedTarget, 180) == false)
						{
							if (!quiet) MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
							Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
							return false;
						}

						if (m_caster.TargetInView == false)
						{
							if (!quiet) MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
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
							if (!quiet) MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
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

				//heals/buffs/rez need LOS only to start casting
				if (!m_caster.TargetInView && m_spell.Target.ToLower() != "pet")
				{
					if (!quiet) MessageToCaster("Your target is not in visible!", eChatType.CT_SpellResisted);
					Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
					return false;
				}

				if (m_spell.Target.ToLower() != "corpse" && !selectedTarget.IsAlive)
				{
					if (!quiet) MessageToCaster(selectedTarget.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
					return false;
				}
			}

			//Ryan: don't want mobs to have reductions in mana
			if (Spell.Power != 0 && m_caster is GamePlayer && (m_caster as GamePlayer).CharacterClass.ID != (int)eCharacterClass.Savage && m_caster.Mana < PowerCost(selectedTarget) && Spell.SpellType != "Archery")
			{
				if (!quiet) MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_spell.Concentration > 0)
			{
				if (m_caster.Concentration < m_spell.Concentration)
				{
					if (!quiet) MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
					return false;
				}

				if (m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
				{
					if (!quiet) MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
					return false;
				}
			}

			// Cancel engage if user starts attack
			if (m_caster.IsEngaging)
			{
				EngageEffect engage = (EngageEffect)m_caster.EffectList.GetOfType(typeof(EngageEffect));
				if (engage != null)
				{
					engage.Cancel(false);
				}
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
				return;
			if ((response & 0x100) == 0x100) // In view ?
				return;
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
				return;

			if ((response & 0x100) == 0x100) // In view?
				return;

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
				return;

			if ((response & 0x100) == 0x100) // In view?
				return;

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
			if (m_caster.ObjectState != GameLiving.eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.IsAlive)
			{
				MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				if (!CheckInstrument())
				{
					MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.IsSitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Target.ToLower() == "area")
			{
				if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
				{
					MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
			{
				if (m_spell.Target.ToLower() != "pet")
				{
					//all other spells that need a target
					if (target == null || target.ObjectState != GameObject.eObjectState.Active)
					{
						if (Caster is GamePlayer)
							MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
						return false;
					}

					if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
					{
						if (Caster is GamePlayer)
							MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
						return false;
					}
				}

				switch (m_spell.Target)
				{
					case "Enemy":
						//enemys have to be in front and in view for targeted spells
						if (!m_caster.IsObjectInFront(target, 180))
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
							MessageToCaster("This spell only works on dead members of your realm!",
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
						//Now check distance for own pet
						if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
						{
							MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
							return false;
						}
						break;
				}
			}

			if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
			{
				MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
			{
				MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.Concentration < m_spell.Concentration)
			{
				MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
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

			if (!m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer && m_caster.EffectList.GetOfType(typeof(QuickCastEffect)) == null && m_caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) == null)
			{
				if(Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
				{
					if (!quiet)
					{
						if (Caster.LastInterruptMessage != "") MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
						else MessageToCaster("You are interrupted and must wait " + ((Caster.InterruptTime - m_started) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
					}
					return false;
				}
			}

			if (m_caster.ObjectState != GameLiving.eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.IsAlive)
			{
				if(!quiet) MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				if (!CheckInstrument())
				{
					if (!quiet) MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.IsSitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				if (!quiet) MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Target.ToLower() == "area")
			{
				if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
				{
					if (!quiet) MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
			{
				if (m_spell.Target.ToLower() != "pet")
				{
					//all other spells that need a target
					if (target == null || target.ObjectState != GameObject.eObjectState.Active)
					{
						if (Caster is GamePlayer && !quiet)
							MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
						return false;
					}

					if (Caster is GamePlayer && !m_caster.IsWithinRadius(target, CalculateSpellRange()))
					{
						if (!quiet) MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
						return false;
					}
				}

				switch (m_spell.Target.ToLower())
				{
					case "enemy":
						//enemys have to be in front and in view for targeted spells
						if (Caster is GamePlayer && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
						{
							if (!quiet) MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
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
							if (!quiet) MessageToCaster("This spell only works on dead members of your realm!",
							                            eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "realm":
						if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
						{
							return false;
						}
						break;

					case "pet":
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
								if (!quiet) MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
								return false;
							}
						}
						//Now check distance for own pet
						if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
						{
							if (!quiet) MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
							return false;
						}
						break;
				}
			}

			if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
			{
				if (!quiet) MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
			{
				if (!quiet) MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_caster.Concentration < m_spell.Concentration)
			{
				if (!quiet) MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
			{
				if (!quiet) MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
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

			if (!m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer && m_caster.EffectList.GetOfType(typeof(QuickCastEffect)) == null && m_caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) == null)
			{
				if (Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
				{
					if (!quiet)
					{
						if(Caster.LastInterruptMessage != "") MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
						else MessageToCaster("You are interrupted and must wait " + ((Caster.InterruptTime - m_started) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
					}
					Caster.InterruptAction = Caster.CurrentRegion.Time - Caster.SpellInterruptRecastAgain;
					return false;
				}
			}

			if (m_caster.ObjectState != GameLiving.eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.IsAlive)
			{
				if (!quiet) MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				if (!CheckInstrument())
				{
					if (!quiet) MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.IsSitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				if (!quiet) MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Target.ToLower() == "area")
			{
				if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
				{
					if (!quiet) MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
				if (!Caster.GroundTargetInView)
				{
					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
			{
				if (m_spell.Target.ToLower() != "pet")
				{
					//all other spells that need a target
					if (target == null || target.ObjectState != GameObject.eObjectState.Active)
					{
						if (Caster is GamePlayer && !quiet)
							MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
						return false;
					}

					if (Caster is GamePlayer && !m_caster.IsWithinRadius(target, CalculateSpellRange()))
					{
						if (!quiet) MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
						return false;
					}
				}

				switch (m_spell.Target)
				{
					case "Enemy":
						//enemys have to be in front and in view for targeted spells
						if (Caster is GamePlayer && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
						{
							if (!quiet) MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
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
							if (!quiet) MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "Realm":
						if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
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
								if (!quiet) MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
								return false;
							}
						}
						//Now check distance for own pet
						if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
						{
							if (!quiet) MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
							return false;
						}
						break;
				}
			}

			if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
			{
				if (!quiet) MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
			{
				if (!quiet) MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_caster.Concentration < m_spell.Concentration)
			{
				if (!quiet) MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
			{
				if (!quiet) MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
				return false;
			}

			/*if (m_caster.Concentration < m_spell.Concentration)
{
MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
return false;
}*/

			/*if (m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
{
MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
return false;
}*/

			return true;
		}


		#endregion

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual int PowerCost(GameLiving target)
		{
			// warlock
			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Powerless");
			if (effect != null && !m_spell.IsPrimary)
				return 0;

			// Apply Valkyrie RA5L effect
			ValhallasBlessingEffect ValhallasBlessing = (ValhallasBlessingEffect)m_caster.EffectList.GetOfType(typeof(ValhallasBlessingEffect));
			if (ValhallasBlessing != null && Util.Chance(10))
				return 0;

			// Apply Animist RA5L effect
			FungalUnionEffect FungalUnion = (FungalUnionEffect)m_caster.EffectList.GetOfType(typeof(FungalUnionEffect));
			{
				if (FungalUnion != null && Util.Chance(10))
					return 0;
			}

			#region [Freya] Nidel: Arcane Syphon chance
			int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
			if (syphon > 0)
			{
				if(Util.Chance(syphon))
				{
					return 0;
				}
			}
			#endregion

			double basepower = m_spell.Power; //<== defined a basevar first then modified this base-var to tell %-costs from absolut-costs

			// percent of maxPower if less than zero
			if (basepower < 0)
				if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				GamePlayer player = Caster as GamePlayer;
				basepower = player.CalculateMaxMana(player.Level, player.GetBaseStat(player.CharacterClass.ManaStat)) * basepower * -0.01;
			}
			else
				basepower = Caster.MaxMana * basepower * -0.01;

			double power = basepower * 1.2; //<==NOW holding basepower*1.2 within 'power'

			eProperty focusProp = SkillBase.SpecToFocus(SpellLine.Spec);
			if (focusProp != eProperty.Undefined)
			{
				double focusBonus = Caster.GetModified(focusProp) * 0.4;
				if (Spell.Level > 0)
					focusBonus /= Spell.Level;
				if (focusBonus > 0.4)
					focusBonus = 0.4;
				else if (focusBonus < 0)
					focusBonus = 0;
				power -= basepower * focusBonus; //<== So i can finally use 'basepower' for both calculations: % and absolut
			}
			else if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ClassType == eClassType.Hybrid)
			{
				double specBonus = 0;
				if (Spell.Level != 0) specBonus = (((GamePlayer)Caster).GetBaseSpecLevel(SpellLine.Spec) * 0.4 / Spell.Level);

				if (specBonus > 0.4)
					specBonus = 0.4;
				else if (specBonus < 0)
					specBonus = 0;
				power -= basepower * specBonus;
			}
			// doubled power usage if quickcasting
			if (Caster.EffectList.GetOfType(typeof(QuickCastEffect)) != null && Spell.CastTime > 0)
				power *= 2;
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
			//Dinberg: add for warlock range primer
		}

		/// <summary>
		/// Called whenever the casters casting sequence is to interrupt immediately
		/// </summary>
		public virtual void InterruptCasting()
		{
			if (m_interrupted || !IsCasting)
				return;

			m_interrupted = true;

			if (IsCasting)
			{
				foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendInterruptAnimation(m_caster);
				}
			}

			if (m_castTimer != null)
			{
				m_castTimer.Stop();
				m_castTimer = null;

				if (m_caster is GamePlayer)
				{
					((GamePlayer)m_caster).ClearSpellQueue();
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
				if (handler == null)
					throw new ArgumentNullException("handler");

				if (actionSource == null)
					throw new ArgumentNullException("actionSource");

				m_handler = handler;
				m_target = target;
				m_caster = actionSource;
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
						(m_caster as GamePlayer).Out.SendMessage("[DEBUG] step = " + (m_handler.Stage + 1), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}

					return;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(ToString(), e);
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
					.Append(" spellhandler: (").Append(m_handler.ToString()).Append(')')
					.ToString();
			}
		}

		/// <summary>
		/// Calculates the effective casting time
		/// </summary>
		/// <returns>effective casting time in milliseconds</returns>
		public virtual int CalculateCastingTime()
		{
			return m_caster.CalculateCastingTime(m_spellLine, m_spell);
		}


		#region animations

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
			foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null)
					continue;
				player.Out.SendSpellCastAnimation(m_caster, m_spell.ClientEffect, castTime);
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
				target = m_caster;

			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, boltDuration, noSound, success);
			}
		}

		/// <summary>
		/// Send the Interrupt Cast Animation
		/// </summary>
		public virtual void SendInterruptCastAnimation()
		{
			foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendInterruptAnimation(m_caster);
			}
		}
		public virtual void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
		{
			if (target == null)
				target = m_caster;

			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, clientEffect, boltDuration, noSound, success);
			}
		}
		#endregion

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public virtual void FinishSpellCast(GameLiving target)
		{
			//turn into wraith
			if (m_caster is GamePlayer
			    && ((m_caster as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Bainshee)
			    && (m_caster as GamePlayer).InWraithForm == false
			    && !HasPositiveEffect)
				(m_caster as GamePlayer).InWraithForm = true; ;

			if (Caster is GamePlayer && ((GamePlayer)Caster).IsOnHorse && !HasPositiveEffect)
				((GamePlayer)Caster).IsOnHorse = false;

			if (Caster is GamePlayer)
				((GamePlayer)Caster).Stealth(false);

            if (Caster is GamePlayer && !HasPositiveEffect)
            {
                if (Caster.AttackWeapon != null && Caster.AttackWeapon is GameInventoryItem)
                {
                    (Caster.AttackWeapon as GameInventoryItem).OnStrikeTargetSpell(Caster, target);
                }
            }

			// messages
			if (Spell.InstrumentRequirement == 0 && Spell.ClientEffect != 0 && Spell.CastTime > 0)
			{
				MessageToCaster("You cast a " + m_spell.Name + " spell!", eChatType.CT_Spell);
				foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					if (player != m_caster)
						player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			if (m_spell.Pulse != 0 && m_spell.Frequency > 0)
			{
				CancelAllPulsingSpells(Caster);
				PulsingSpellEffect pulseeffect = new PulsingSpellEffect(this);
				pulseeffect.Start();
				// show animation on caster for positive spells, negative shows on every StartSpell
				if (m_spell.Target == "Self" || m_spell.Target == "Group" || m_spell.Target == "Pet")
					SendEffectAnimation(Caster, 0, false, 1);
			}

			StartSpell(target); // and action

			//Dinberg: This is where I moved the warlock part (previously found in gameplayer) to prevent
			//cancelling before the spell was fired.
			if (m_spell.SpellType != "Powerless" && m_spell.SpellType != "Range" && m_spell.SpellType != "Uninterruptable")
			{
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Powerless");
				if (effect == null)
					effect = SpellHandler.FindEffectOnTarget(m_caster, "Range");
				if (effect == null)
					effect = SpellHandler.FindEffectOnTarget(m_caster, "Uninterruptable");

				//if we found an effect, cancel it!
				if (effect != null)
					effect.Cancel(false);
			}


			//Subspells
			if (m_spell.SubSpellID > 0 && Spell.SpellType != "Archery" && Spell.SpellType != "Bomber" && Spell.SpellType != "SummonAnimistFnF" && Spell.SpellType != "SummonAnimistPet" && Spell.SpellType != "Grapple")
			{
				Spell spell = SkillBase.GetSpellByID(m_spell.SubSpellID);
				//we need subspell ID to be 0, we don't want spells linking off the subspell
				if (target != null && spell != null && spell.SubSpellID == 0)
				{
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
					spellhandler.StartSpell(target);
				}
			}

			//the quick cast is unallowed whenever you miss the spell
			//set the time when casting to can not quickcast during a minimum time
			if (m_caster is GamePlayer)
			{
				QuickCastEffect quickcast = (QuickCastEffect)m_caster.EffectList.GetOfType(typeof(QuickCastEffect));
				if (quickcast != null && Spell.CastTime > 0)
				{
					m_caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, m_caster.CurrentRegion.Time);
					((GamePlayer)m_caster).DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
					quickcast.Cancel(false);
				}
			}


			if (m_ability != null)
				m_caster.DisableSkill(m_ability.Ability, (m_spell.RecastDelay == 0 ? 3 : m_spell.RecastDelay));

			// disable spells with recasttimer (Disables group of same type with same delay)
			if (m_spell.RecastDelay > 0 && m_startReuseTimer)
			{
				if (m_caster is GamePlayer)
				{
					GamePlayer gp_caster = m_caster as GamePlayer;
					foreach (SpellLine spellline in gp_caster.GetSpellLines())
						foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
							if (sp == m_spell || (sp.SharedTimerGroup != 0 && (sp.SharedTimerGroup == m_spell.SharedTimerGroup)))
								m_caster.DisableSkill(sp, sp.RecastDelay);
				}
				else if (m_caster is GameNPC)
					m_caster.DisableSkill(m_spell, m_spell.RecastDelay);
			}

			GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
		}

		/// <summary>
		/// Select all targets for this spell
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public virtual IList SelectTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList(8);
			GameLiving target = castTarget as GameLiving;
			bool targetchanged = false;
			string modifiedTarget = Spell.Target.ToLower();
			ushort modifiedRadius = (ushort)Spell.Radius;
			int newtarget = 0;

			GameSpellEffect TargetMod = SpellHandler.FindEffectOnTarget(m_caster, "TargetModifier");
			if (TargetMod != null)
			{
				if (modifiedTarget == "enemy" || modifiedTarget == "realm" || modifiedTarget == "group")
				{
					newtarget = (int)TargetMod.Spell.Value;

					switch (newtarget)
					{
						case 0: // Apply on heal single
							if (m_spell.SpellType.ToLower() == "heal" && modifiedTarget == "realm")
							{
								modifiedTarget = "group";
								targetchanged = true;
							}
							break;
						case 1: // Apply on heal group
							if (m_spell.SpellType.ToLower() == "heal" && modifiedTarget == "group")
							{
								modifiedTarget = "realm";
								modifiedRadius = (ushort)m_spell.Range;
								targetchanged = true;
							}
							break;
						case 2: // apply on enemy
							if (modifiedTarget == "enemy")
							{
								if (m_spell.Radius == 0)
									modifiedRadius = 450;
								if (m_spell.Radius != 0)
									modifiedRadius += 300;
								targetchanged = true;
							}
							break;
						case 3: // Apply on buff
							if (m_spell.Target.ToLower() == "group"
							    && m_spell.Pulse != 0)
							{
								modifiedTarget = "realm";
								modifiedRadius = (ushort)m_spell.Range;
								targetchanged = true;
							}
							break;
					}
				}
				if (targetchanged)
				{
					if (TargetMod.Duration < 65535)
						TargetMod.Cancel(false);
				}
			}

			if (modifiedTarget == "pet" && Spell.Damage > 0 && Spell.Radius > 0)
			{
				modifiedTarget = "enemy";
				//[Ganrod] Nidel: can cast TurretPBAoE on selected Pet/Turret
				if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower())
				{
					target = Caster.ControlledBrain.Body;
				}
			}

			#region Process the targets
			switch (modifiedTarget)
			{
				#region GTAoE
				// GTAoE
				case "area":
					//Dinberg - fix for animists turrets, where before a radius of zero meant that no targets were ever
					//selected!
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
									SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
									if (SelectiveBlindness != null)
									{
										GameLiving EffectOwner = SelectiveBlindness.EffectSource;
										if (EffectOwner == player)
										{
											if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
										}
										else list.Add(player);
									}
									else list.Add(player);
								}
							}
							foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
							{
								if (npc is GameStorm)
									list.Add(npc);
								else if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
								{
									if (!npc.HasAbility("DamageImmunity")) list.Add(npc);
								}
							}
						}
					break;
				#endregion
				#region Corpse
				case "corpse":
					if (target != null && !target.IsAlive)
						list.Add(target);
					break;
				#endregion
				#region Pet
				case "pet":
					{
						//Start-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
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
						//check controllednpc if target isn't pet (our pet)
						if (list.Count < 1 && Caster.ControlledBrain != null)
						{
							petBody = Caster.ControlledBrain.Body;
							if (petBody != null && Caster.IsWithinRadius(petBody, Spell.Range))
							{
								list.Add(petBody);
							}
						}

						//Single spell buff/heal...
						if (Spell.Radius == 0)
						{
							return list;
						}
						//Our buff affects every pet in the area of targetted pet (our pets)
						if (Spell.Radius > 0 && petBody != null)
						{
							foreach (GameNPC pet in petBody.GetNPCsInRadius(modifiedRadius))
							{
								//ignore target or our main pet already added
								if (pet == petBody || !Caster.IsControlledNPC(pet))
								{
									continue;
								}
								list.Add(pet);
							}
						}
					}
					//End-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
					break;
				#endregion
				#region Enemy
				case "enemy":
					if (modifiedRadius > 0)
					{
						if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower() && (target == null || Spell.Range == 0))
							target = Caster;
						if (target == null) return null;
						foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
							{
								SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
								if (SelectiveBlindness != null)
								{
									GameLiving EffectOwner = SelectiveBlindness.EffectSource;
									if (EffectOwner == player)
									{
										if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
									}
									else list.Add(player);
								}
								else list.Add(player);
							}
						}
						foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
							{
								if (!npc.HasAbility("DamageImmunity")) list.Add(npc);
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
								SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
								if (SelectiveBlindness != null)
								{
									GameLiving EffectOwner = SelectiveBlindness.EffectSource;
									if (EffectOwner == target)
									{
										if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
									}
									else if (!target.HasAbility("DamageImmunity")) list.Add(target);
								}
								else if (!target.HasAbility("DamageImmunity")) list.Add(target);
							}
							else if (!target.HasAbility("DamageImmunity")) list.Add(target);
						}
					}
					break;
				#endregion
				#region Realm
				case "realm":
					if (modifiedRadius > 0)
					{
						if (target == null || Spell.Range == 0)
							target = Caster;

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
							list.Add(target);
					}
					break;
				#endregion
				#region Self
				case "self":
					{
						if (modifiedRadius > 0)
						{
							if (target == null || Spell.Range == 0)
								target = Caster;
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
				#endregion
				#region Group
				case "group":
					{
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = modifiedRadius;

						//Just add ourself
						if (group == null)
						{
							list.Add(m_caster);

							IControlledBrain npc = m_caster.ControlledBrain;
							if (npc != null)
							{
								//Add our first pet
								GameNPC petBody2 = npc.Body;
								if (m_caster.IsWithinRadius(petBody2, spellRange))
									list.Add(petBody2);

								//Now lets add any subpets!
								if (petBody2 != null && petBody2.ControlledNpcList != null)
								{
									foreach (IControlledBrain icb in petBody2.ControlledNpcList)
									{
										if (icb != null && m_caster.IsWithinRadius(icb.Body, spellRange))
											list.Add(icb.Body);
									}
								}
							}

						}
						//We need to add the entire group
						else
						{
							foreach (GameLiving living in group.GetMembersInTheGroup())
							{
								// only players in range
								if (m_caster.IsWithinRadius(living, spellRange))
								{
									list.Add(living);

									IControlledBrain npc = living.ControlledBrain;
									if (npc != null)
									{
										//Add our first pet
										GameNPC petBody2 = npc.Body;
										if (m_caster.IsWithinRadius(petBody2, spellRange))
											list.Add(petBody2);

										//Now lets add any subpets!
										if (petBody2 != null && petBody2.ControlledNpcList != null)
										{
											foreach (IControlledBrain icb in petBody2.ControlledNpcList)
											{
												if (icb != null && m_caster.IsWithinRadius(icb.Body, spellRange))
													list.Add(icb.Body);
											}
										}
									}
								}
							}
						}

						break;
					}
				#endregion
				#region Cone AoE
				case "cone":
					{
						target = Caster;
						foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Range))
						{
							if (player == Caster)
								continue;

							if (!m_caster.IsObjectInFront(player, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
								continue;

							if (!GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
								continue;

							list.Add(player);
						}

						foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Range))
						{
							if (npc == Caster)
								continue;

							if (!m_caster.IsObjectInFront(npc, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
								continue;

							if (!GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
								continue;

							if (!npc.HasAbility("DamageImmunity")) list.Add(npc);

						}
						break;
					}
				#endregion
			}
			#endregion
			return list;
		}

		/// <summary>
		/// Cast all subspell recursively
		/// </summary>
		/// <param name="target"></param>
		public void CastSubSpells(GameLiving target, SpellLine line)
		{
			if (Spell.SubSpellID > 0 && Spell.SpellType != "Archery" && Spell.SpellType != "Bomber" && Spell.SpellType != "SummonAnimistFnF" && Spell.SpellType != "SummonAnimistPet" && Spell.SpellType != "Grapple")
			{
				List<Spell> spells = SkillBase.GetSpellList(SpellLine.KeyName);
				foreach (Spell subSpell in spells)
				{
					if (subSpell.ID == Spell.SubSpellID)
					{
						SpellHandler subSpellHandler = ScriptMgr.CreateSpellHandler(Caster, subSpell, line) as SpellHandler;
						if (subSpellHandler != null)
						{
							subSpellHandler.StartSpell(target);
							subSpellHandler.CastSubSpells(target, line);
						}
						break;
					}
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
				m_spellTarget = target;

			if (m_spellTarget == null) return false;

			IList targets = SelectTargets(m_spellTarget);

			double effectiveness = Caster.Effectiveness;

			if (Caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) != null)
			{
				RealmAbility ra = Caster.GetAbility(typeof(MasteryofConcentrationAbility)) as RealmAbility;
				if (ra != null && ra.Level > 0)
				{
					effectiveness *= System.Math.Round((double)ra.Level * 25 / 100, 2);
				}
			}

            //[StephenxPimentel] Reduce Damage if necro is using MoC
            if (Caster is NecromancerPet)
            {
                if ((Caster as NecromancerPet).Owner.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) != null)
                {
                    RealmAbility necroRA = (Caster as NecromancerPet).Owner.GetAbility(typeof(MasteryofConcentrationAbility)) as RealmAbility;
					if (necroRA != null && necroRA.Level > 0)
					{
						effectiveness *= System.Math.Round((double)necroRA.Level * 25 / 100, 2);
					}
                }
            }

			if (Caster is GamePlayer && (Caster as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Warlock && m_spell.IsSecondary)
			{
                Spell uninterruptibleSpell = Caster.TempProperties.getProperty<Spell>(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);

                if (uninterruptibleSpell != null && uninterruptibleSpell.Value > 0)
                {
                    double nerf = uninterruptibleSpell.Value;
                    effectiveness *= (1 - (nerf * 0.01));
                    Caster.TempProperties.removeProperty(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);
                }
			}

			foreach (GameLiving t in targets)
			{
				// Aggressive NPCs will aggro on every target they hit
				// with an AoE spell, whether it landed or was resisted.

				if (Spell.Radius > 0 && Spell.Target.ToLower() == "enemy"
				    && Caster is GameNPC && (Caster as GameNPC).Brain is IOldAggressiveBrain)
					((Caster as GameNPC).Brain as IOldAggressiveBrain).AddToAggroList(t, 1);

				if (Util.Chance(CalculateSpellResistChance(t)))
				{
					OnSpellResisted(t);
					continue;
				}

				if (Spell.Radius == 0)
				{
					ApplyEffectOnTarget(t, effectiveness);
				}
				else if (Spell.Target.ToLower() == "area")
				{
					int dist = t.GetDistanceTo(Caster.GroundTarget);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Radius)));
					}
				}
				else if (Spell.Target.ToLower() == "cone")
				{
					int dist = t.GetDistanceTo(Caster);
					if (dist >= 0)
					{
						//Cone spells use the range for their variance!
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Range)));
					}
				}
				else
				{
					int dist = t.GetDistanceTo(target);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Radius)));
					}
				}
			}

			if (Spell.Target.ToLower() == "ground")
			{
				ApplyEffectOnTarget(null, 1);
			}

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
			return ((double)distance / (double)radius);
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
			duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
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
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
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
			int freq = Spell != null ? Spell.Frequency : 0;
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
				GameSpellEffect effect1;
				effect1 = SpellHandler.FindEffectOnTarget(target, "Phaseshift");
				if ((effect1 != null && (Spell.SpellType != "SpreadHeal" || Spell.SpellType != "Heal" || Spell.SpellType != "SpeedEnhancement")))
				{
					MessageToCaster(target.Name + " is Phaseshifted and can't be effected by this Spell!", eChatType.CT_SpellResisted);
					return;
				}
			}

			if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent))
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
						isAllowed = true;
					else
						isSilent = true;
				}

				if (!isAllowed)
				{
					if (!isSilent)
					{
						MessageToCaster(String.Format("Your spell has no effect on the {0}!", target.Name), eChatType.CT_SpellResisted);
					}

					return;
				}
			}
			if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects || m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || m_spellLine.KeyName == GlobalSpellsLines.Potions_Effects || m_spellLine.KeyName == Specs.Savagery || m_spellLine.KeyName == GlobalSpellsLines.Character_Abilities || m_spellLine.KeyName == "OffensiveProc")
				effectiveness = 1.0; // TODO player.PlayerEffectiveness
			if (effectiveness <= 0)
				return; // no effect

			if ((Spell.Duration > 0 && Spell.Target.ToLower() != "area") || Spell.Concentration > 0)
			{
				if (!target.IsAlive)
					return;
				eChatType noOverwrite = (Spell.Pulse == 0) ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
				GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
				GameSpellEffect overwriteEffect = null;
				bool foundInList = false;
				lock (target.EffectList)
				{
					foreach (IGameEffect effect in target.EffectList)
					{
						if (effect is GameSpellEffect)
						{
							GameSpellEffect gsp = (GameSpellEffect)effect;
							if (gsp.SpellHandler.IsOverwritable(neweffect))
							{
								foundInList = true;
								if (gsp is GameSpellAndImmunityEffect)
								{
									GameSpellAndImmunityEffect immunity = (GameSpellAndImmunityEffect)gsp;
									if (immunity.ImmunityState && (immunity.Owner is GamePlayer || immunity.Owner is GamePet))
									{
										SendEffectAnimation(target, 0, false, 0); //resisted effect
										MessageToCaster(immunity.Owner.GetName(0, true) + " can't have that effect again yet!", noOverwrite);
										break;
									}
								}
								if (IsNewEffectBetter(gsp, neweffect))
								{
									overwriteEffect = gsp;
								}
								else
								{
									if (target == m_caster)
									{
										MessageToCaster("You already have that effect. Wait until it expires.  Spell failed.", noOverwrite);
									}
									else
									{
										MessageToCaster(target.GetName(0, true) + " already has that effect.", noOverwrite);
										MessageToCaster("Wait until it expires.  Spell Failed.", noOverwrite);
									}
									// show resisted effect if spell failed
									if (Spell.Pulse == 0)
										SendEffectAnimation(target, 0, false, 0);
								}
								break;
							}
						}
					}
				}

				if (!foundInList)
				{
					neweffect.Start(target);
				}
				else if (overwriteEffect != null)
				{
					overwriteEffect.Overwrite(neweffect);
				}
			}
			else
			{
				OnDirectEffect(target, effectiveness);
			}

			if (!HasPositiveEffect)
			{
				AttackData ad = new AttackData();
				ad.Attacker = Caster;
				ad.Target = target;
				ad.AttackType = AttackData.eAttackType.Spell;
				ad.SpellHandler = this;
				ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
				ad.IsSpellResisted = false;

				m_lastAttackData = ad;
			}
		}

		/// <summary>
		/// Called when cast sequence is complete
		/// </summary>
		public virtual void OnAfterSpellCastSequence()
		{
			if (CastingCompleteEvent != null)
			{
				CastingCompleteEvent(this);
			}
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
//			if (oldspell.SpellType != newspell.SpellType)
//			{
//				if (Log.IsWarnEnabled)
//					Log.Warn("Spell effect compare with different types " + oldspell.SpellType + " <=> " + newspell.SpellType + "\n" + Environment.StackTrace);
//				return false;
//			}
			if (oldspell.Concentration > 0)
				return false;
			if (newspell.Damage < oldspell.Damage)
				return false;
			if (newspell.Value < oldspell.Value)
				return false;
			//makes problems for immunity effects
			if (oldeffect is GameSpellAndImmunityEffect == false || ((GameSpellAndImmunityEffect)oldeffect).ImmunityState == false)
			{
				if (neweffect.Duration <= oldeffect.RemainingTime)
					return false;
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
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (compare.Spell.SpellType != Spell.SpellType)
				return false;
			return true;
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void OnDirectEffect(GameLiving target, double effectiveness)
		{
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public virtual void OnEffectStart(GameSpellEffect effect)
		{
			if (Spell.Pulse == 0)
				SendEffectAnimation(effect.Owner, 0, false, 1);
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
			if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || HasPositiveEffect)
			{
				return 0;
			}

			if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects && m_spellItem != null)
			{
				GamePlayer playerCaster = Caster as GamePlayer;
				if (playerCaster != null)
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
			SendEffectAnimation(target, 0, false, 0);

			// Deliver message to the target, if the target is a pet, to its
			// owner instead.

			if (target is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
				if (brain != null)
				{
					GamePlayer owner = brain.GetPlayerOwner();
					if (owner != null)
					{
						MessageToLiving(owner, "Your " + target.Name + " resists the effect!", eChatType.CT_SpellResisted);
					}
				}
			}
			else
			{
				MessageToLiving(target, "You resist the effect!", eChatType.CT_SpellResisted);
			}

			// Deliver message to the caster as well.

			MessageToCaster(target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);

			// Report resisted spell attack data to any type of living object, no need
			// to decide here what to do. For example, NPCs will use their brain.
			// "Just the facts, ma'am, just the facts."

			AttackData ad = new AttackData();
			ad.Attacker = Caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.SpellHandler = this;
			ad.AttackResult = GameLiving.eAttackResult.Missed;
			ad.IsSpellResisted = true;
			target.OnAttackedByEnemy(ad);

			// Spells that would have caused damage or are not instant will still
			// interrupt a casting player.

			/*if (target is GamePlayer)
{
if (target.IsCasting && (Spell.Damage > 0 || Spell.CastTime > 0))
target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
}*/
			if(!(Spell.SpellType.ToLower().IndexOf("debuff")>=0 && Spell.CastTime==0))
				target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);


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

		#region messages

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
				(Caster as GamePlayer).Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
			}
			else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain
			         && (type == eChatType.CT_YouHit || type == eChatType.CT_SpellResisted))
			{
				GamePlayer owner = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
				if (owner != null)
					owner.Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
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
			if (living is GamePlayer && message != null && message.Length > 0)
			{
				((GamePlayer)living).Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
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
			GameLiving living = sender as GameLiving;
			if (living == null) return;

			GameSpellEffect currentEffect = (GameSpellEffect)living.TempProperties.getProperty<object>(FOCUS_SPELL, null);
			if (currentEffect == null)
				return;

			if (args is CastingEventArgs)
			{
				if ((args as CastingEventArgs).SpellHandler.Caster != Caster)
					return;
				if ((args as CastingEventArgs).SpellHandler.Spell.SpellType == currentEffect.Spell.SpellType)
					return;
			}

			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(currentEffect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			Caster.TempProperties.removeProperty(FOCUS_SPELL);

			if (currentEffect.Spell.Pulse != 0)
				CancelPulsingSpell(Caster, currentEffect.Spell.SpellType);
			else
				currentEffect.Cancel(false);

			MessageToCaster(String.Format("You lose your focus on your {0} spell.", currentEffect.Spell.Name), eChatType.CT_SpellExpires);

			if (e == GameLivingEvent.Moving)
				MessageToCaster("You move and interrupt your focus!", eChatType.CT_Important);
		}
		#endregion

		/// <summary>
		/// Ability to cast a spell
		/// </summary>
		public SpellCastingAbilityHandler Ability
		{
			get { return m_ability; }
			set { m_ability = value; }
		}
		/// <summary>
		/// The Spell
		/// </summary>
		public Spell Spell
		{
			get { return m_spell; }
		}

		/// <summary>
		/// The Spell Line
		/// </summary>
		public SpellLine SpellLine
		{
			get { return m_spellLine; }
		}

		/// <summary>
		/// The Caster
		/// </summary>
		public GameLiving Caster
		{
			get { return m_caster; }
		}

		/// <summary>
		/// Is the spell being cast?
		/// </summary>
		public bool IsCasting
		{
			get { return m_castTimer != null && m_castTimer.IsAlive; }
		}

		/// <summary>
		/// Does the spell have a positive effect?
		/// </summary>
		public virtual bool HasPositiveEffect
		{
			get
			{
				if (m_spell.Target.ToLower() != "enemy" && m_spell.Target.ToLower() != "cone" && m_spell.Target.ToLower() != "area")
					return true;

				return false;
			}
		}

		/// <summary>
		/// Is this Spell purgeable
		/// </summary>
		public virtual bool IsUnPurgeAble
		{
			get { return false; }
		}

		/// <summary>
		/// Current depth of delve info
		/// </summary>
		public byte DelveInfoDepth
		{
			get { return m_delveInfoDepth; }
			set { m_delveInfoDepth = value; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public virtual IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>(32);
				//list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				//list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
				if (Spell.Damage != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
				if (Spell.LifeDrainReturn != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.HealthReturned", Spell.LifeDrainReturn));
				else if (Spell.Value != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")));
				list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
				if (Spell.Range != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " " + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
				else if (Spell.Duration != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
				if (Spell.Power != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
				list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				if (Spell.RecastDelay > 60000)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.ConcentrationCost", Spell.Concentration));
				if (Spell.Radius != 0)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
				if (Spell.DamageType != eDamageType.Natural)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
				if (Spell.IsFocus)
					list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Focus"));

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
						continue;
					GameSpellEffect effect = (GameSpellEffect)fx;
					if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
						continue; // ignore immunity effects

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
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect fx in target.EffectList)
				{
					if (!(fx is GameSpellEffect))
						continue;
					GameSpellEffect effect = (GameSpellEffect)fx;
					if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
						continue; // ignore immunity effects
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
						continue;
					if (gsp.SpellHandler != spellHandler)
						continue;
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue; // ignore immunity effects
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
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					if (gsp == null)
						continue;
					if (gsp.SpellHandler.GetType().IsInstanceOfType(spellHandler) == false)
						continue;
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue; // ignore immunity effects
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
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
					if (effect.GetType() == effectType)
						return effect;
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
					if (pulsingSpell == null) continue;
					if (pulsingSpell.SpellHandler == handler)
						return pulsingSpell;
				}
				return null;
			}
		}

		#region various helpers

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
			if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects)
			{
				min = 1.0;
				max = 1.25;
				return;
			}

			if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
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

			if (m_spellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
			{
				min = max = 1.0;
				return;
			}

			int speclevel = 1;

			if (m_caster is GamePet)
			{
				IControlledBrain brain = (m_caster as GameNPC).Brain as IControlledBrain;
				speclevel = brain.GetPlayerOwner().Level;
			}
			else if (m_caster is GamePlayer)
			{
				speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
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
			if (m_caster is GamePlayer)
			{
				min += GetLevelModFactor() * (m_caster.Level - target.Level);
				max += GetLevelModFactor() * (m_caster.Level - target.Level);
			}
			else if (m_caster is GameNPC && ((GameNPC)m_caster).Brain is IControlledBrain)
			{
				//Get the root owner
				GamePlayer owner = ((IControlledBrain)((GameNPC)m_caster).Brain).GetPlayerOwner();
				if (owner != null)
				{
					min += GetLevelModFactor() * (owner.Level - target.Level);
					max += GetLevelModFactor() * (owner.Level - target.Level);
				}
			}

			if (max < 0.25)
				max = 0.25;
			if (min > max)
				min = max;
			if (min < 0)
				min = 0;
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

			if (Caster is GameNPC && ((Caster as GameNPC).Brain) is IControlledBrain)
			{
				player = (((Caster as GameNPC).Brain) as IControlledBrain).Owner as GamePlayer;
			}

			if (player != null)
			{
				if (Caster is GamePet)
				{
					spellDamage = CapPetSpellDamage(spellDamage, player);
				}

				if (this.SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
				{
					double WeaponSkill = player.GetWeaponSkill(player.AttackWeapon);
					WeaponSkill /= 5;
					spellDamage *= (WeaponSkill + 200) / 275.0;
				}

				if (player.CharacterClass.ManaStat != eStat.UNDEFINED
				    && this.SpellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect
				    && this.m_spellLine.KeyName != GlobalSpellsLines.Mundane_Poisons
				    && this.SpellLine.KeyName != GlobalSpellsLines.Item_Effects
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
				spellDamage = CapNPCSpellDamage(spellDamage, Caster as GameNPC);
			}

			if (spellDamage < 0)
				spellDamage = 0;

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
			if (m_caster is GameNPC && (m_caster as GameNPC).Brain is ControlledNpcBrain)
			{
				caster = ((ControlledNpcBrain)((GameNPC)m_caster).Brain).Owner;
			}
			else
			{
				caster = m_caster;
			}

			int spellbonus = caster.GetModified(eProperty.SpellLevel);
			spellLevel += spellbonus;

			GamePlayer playerCaster = caster as GamePlayer;

			if (playerCaster != null)
			{
				if (spellLevel > playerCaster.MaxLevel)
				{
					spellLevel = playerCaster.MaxLevel;
				}
			}

			GameSpellEffect effect = FindEffectOnTarget(m_caster, "HereticPiercingMagic");
			if (effect != null)
			{
				spellLevel += (int)effect.Spell.Value;
			}

			if (playerCaster != null && (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || m_spellLine.KeyName == GlobalSpellsLines.Champion_Spells))
			{
				spellLevel = Math.Min(playerCaster.MaxLevel, target.Level);
			}

			int bonustohit = m_caster.GetModified(eProperty.ToHitBonus);

			//Piercing Magic affects to-hit bonus too
			GameSpellEffect resPierce = SpellHandler.FindEffectOnTarget(m_caster, "PenetrateResists");
			if (resPierce != null)
				bonustohit += (int)resPierce.Spell.Value;

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
				hitchance -= (int)(m_caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
				hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
			}

			// [Freya] Nidel: Harpy Cloak : They have less chance of landing melee attacks, and spells have a greater chance of affecting them.
			if((target is GamePlayer))
			{
				GameSpellEffect harpyCloak = FindEffectOnTarget(target, "HarpyFeatherCloak");
				if(harpyCloak != null)
				{
					hitchance += (int) ((hitchance*harpyCloak.Spell.Value)*0.01);
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
			AttackData ad = new AttackData();
			ad.Attacker = m_caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.SpellHandler = this;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;

			double minVariance;
			double maxVariance;

			CalculateDamageVariance(target, out minVariance, out maxVariance);
			double spellDamage = CalculateDamageBase(target);

			if (m_caster is GamePlayer)
			{
				effectiveness += m_caster.GetModified(eProperty.SpellDamage) * 0.01;

				// Relic bonus applied to damage, does not alter effectiveness or increase cap
				spellDamage *= (1.0 + RelicMgr.GetRelicBonusModifier(m_caster.Realm, eRelicType.Magic));

			}

			// Apply casters effectiveness
			spellDamage *= m_caster.Effectiveness;

			int finalDamage = Util.Random((int)(minVariance * spellDamage), (int)(maxVariance * spellDamage));

			// Live testing done Summer 2009 by Bluraven, Tolakram  Levels 40, 45, 50, 55, 60, 65, 70
			// Damage reduced by chance < 55, no extra damage increase noted with hitchance > 100
			int hitChance = CalculateToHitChance(ad.Target);
			finalDamage = AdjustDamageForHitChance(finalDamage, hitChance);

			// apply spell effectiveness
			finalDamage = (int)(finalDamage * effectiveness);

			if ((m_caster is GamePlayer || (m_caster is GameNPC && (m_caster as GameNPC).Brain is IControlledBrain && m_caster.Realm != 0)))
			{
				if (target is GamePlayer)
					finalDamage = (int)((double)finalDamage * ServerProperties.Properties.PVP_SPELL_DAMAGE);
				else if (target is GameNPC)
					finalDamage = (int)((double)finalDamage * ServerProperties.Properties.PVE_SPELL_DAMAGE);
			}

			// Well the PenetrateResistBuff is NOT ResistPierce
			GameSpellEffect penPierce = SpellHandler.FindEffectOnTarget(m_caster, "PenetrateResists");
			if (penPierce != null)
			{
				finalDamage = (int)(finalDamage * (1.0 + penPierce.Spell.Value / 100.0));
			}

			int cdamage = 0;
			if (finalDamage < 0)
				finalDamage = 0;

			eDamageType damageType = DetermineSpellDamageType();

			#region Resists
			eProperty property = target.GetResistTypeForDamage(damageType);
			// The Daoc resistsystem is since 1.65 a 2category system.
			// - First category are Item/Race/Buff/RvrBanners resists that are displayed in the characteroverview.
			// - Second category are resists that are given through RAs like avoidance of magic, brilliance aura of deflection.
			//   Those resist affect ONLY the spelldamage. Not the duration, not the effectiveness of debuffs.
			// so calculation is (finaldamage * Category1Modification) * Category2Modification
			// -> Remark for the future: VampirResistBuff is Category2 too.
			// - avi

			#region Primary Resists
			int primaryResistModifier = ad.Target.GetResist(damageType);

			/* Resist Pierce
			 * Resipierce is a special bonus which has been introduced with TrialsOfAtlantis.
			 * At the calculation of SpellDamage, it reduces the resistance that the victim recives
			 * through ITEMBONUSES for the specified percentage.
			 * http://de.daocpedia.eu/index.php/Resistenz_durchdringen (translated)
			 */
			int resiPierce = Caster.GetModified(eProperty.ResistPierce);
			GamePlayer ply = Caster as GamePlayer;
			if (resiPierce > 0 && Spell.SpellType != "Archery")
			{
				//substract max ItemBonus of property of target, but atleast 0.
				primaryResistModifier -= Math.Max(0, Math.Min(ad.Target.ItemBonus[(int)property], resiPierce));
			}
			#endregion

			#region Secondary Resists
			//Using the resist BuffBonusCategory2 - its unused in ResistCalculator
			int secondaryResistModifier = target.SpecBuffBonusCategory[(int)property];

			/*Variance by Memories of War
			 * - Memories of War: Upon reaching level 41, the Hero, Warrior and Armsman
			 * will begin to gain more magic resistance (spell damage reduction only)
			 * as they progress towards level 50. At each level beyond 41 they gain
			 * 2%-3% extra resistance per level. At level 50, they will have the full 15% benefit.
			 * from http://www.camelotherald.com/article.php?id=208
			 *
			 * - assume that "spell damage reduction only" indicates resistcategory 2
			 */

			if (ad.Target is GamePlayer && (ad.Target as GamePlayer).HasAbility(Abilities.MemoriesOfWar) && ad.Target.Level >= 40)
			{
				int levelbonus = Math.Min(target.Level - 40, 10);
				secondaryResistModifier += (int)((levelbonus * 0.1 * 15));
			}

			if (secondaryResistModifier > 80)
				secondaryResistModifier = 80;
			#endregion

			int resistModifier = 0;
			//primary resists
			resistModifier += (int)(finalDamage * (double)primaryResistModifier * -0.01);
			//secondary resists
			resistModifier += (int)((finalDamage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);
			//apply resists
			finalDamage += resistModifier;

			#endregion

			// Apply damage cap (this can be raised by effectiveness)
			if (finalDamage > DamageCap(effectiveness))
			{
				finalDamage = (int)DamageCap(effectiveness);
			}

			if (finalDamage < 0)
				finalDamage = 0;

			int criticalchance = (m_caster.SpellCriticalChance);
			if (Util.Chance(Math.Min(50, criticalchance)) && (finalDamage >= 1))
			{
				int critmax = (ad.Target is GamePlayer) ? finalDamage / 2 : finalDamage;
				cdamage = Util.Random(finalDamage / 10, critmax); //think min crit is 10% of damage
			}
			//Andraste
			if(ad.Target is GamePlayer && ad.Target.GetModified(eProperty.Conversion)>0)
			{
				int manaconversion=(int)Math.Round(((double)ad.Damage+(double)ad.CriticalDamage)*(double)ad.Target.GetModified(eProperty.Conversion)/200);
				//int enduconversion=(int)Math.Round((double)manaconversion*(double)ad.Target.MaxEndurance/(double)ad.Target.MaxMana);
				int enduconversion=(int)Math.Round(((double)ad.Damage+(double)ad.CriticalDamage)*(double)ad.Target.GetModified(eProperty.Conversion)/200);
				if(ad.Target.Mana+manaconversion>ad.Target.MaxMana) manaconversion=ad.Target.MaxMana-ad.Target.Mana;
				if(ad.Target.Endurance+enduconversion>ad.Target.MaxEndurance) enduconversion=ad.Target.MaxEndurance-ad.Target.Endurance;
				if(manaconversion<1) manaconversion=0;
				if(enduconversion<1) enduconversion=0;
				if(manaconversion>=1) (ad.Target as GamePlayer).Out.SendMessage("You gain "+manaconversion+" power points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if(enduconversion>=1) (ad.Target as GamePlayer).Out.SendMessage("You gain "+enduconversion+" endurance points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				ad.Target.Endurance+=enduconversion; if(ad.Target.Endurance>ad.Target.MaxEndurance) ad.Target.Endurance=ad.Target.MaxEndurance;
				ad.Target.Mana+=manaconversion; if(ad.Target.Mana>ad.Target.MaxMana) ad.Target.Mana=ad.Target.MaxMana;
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
			string modmessage = "";
			if (ad.Modifier > 0)
				modmessage = " (+" + ad.Modifier + ")";
			if (ad.Modifier < 0)
				modmessage = " (" + ad.Modifier + ")";
			if (Caster is GamePlayer || Caster is NecromancerPet)
				MessageToCaster(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);
			else if (Caster is GameNPC)
				MessageToCaster(string.Format("Your " + Caster.Name + " hits {0} for {1}{2} damage!",
				                              ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);
			if (ad.CriticalDamage > 0)
				MessageToCaster("You critically hit for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit);
		}

		/// <summary>
		/// Make damage to target and send spell effect but no messages
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		public virtual void DamageTarget(AttackData ad, bool showEffectAnimation)
		{
			DamageTarget(ad, showEffectAnimation, 0x14); //spell damage attack result
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
				foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);

			// send animation before dealing damage else dead livings show no animation
			ad.Target.OnAttackedByEnemy(ad);
			ad.Attacker.DealDamage(ad);
			if (ad.Damage == 0 && ad.Target is GameNPC)
			{
				IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}

			m_lastAttackData = ad;
		}

		#endregion

		#region saved effects
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
		#endregion

	}
}
