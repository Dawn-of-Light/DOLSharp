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
using System.Collections.Concurrent;
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
	public class SpellHandler : SpellHandlerBase, ISpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Maximum number of sub-spells to get delve info for.
		/// </summary>
		protected static readonly byte MAX_DELVE_RECURSION = 5;

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
		/// The target for this spell
		/// </summary>
		public GameLiving SpellCastTarget
		{
			get { return m_spellTarget; }
		}
		
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

		public virtual bool StartReuseTimer
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

		/// <summary>
		/// Does this spell break stealth on start of cast?
		/// </summary>
		public virtual bool UnstealthCasterOnStart
		{
			get { return true; }
		}
		
		/// <summary>
		/// Does this spell break stealth on Finish of cast?
		/// </summary>
		public virtual bool UnstealthCasterOnFinish
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
		protected ConcurrentDictionary<GameLiving, AttackData> m_lastAttackData = new ConcurrentDictionary<GameLiving, AttackData>();

		/// <summary>
		/// The property key for the interrupt timeout
		/// </summary>
		public const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";

		/// <summary>
		/// The property key for focus spells
		/// </summary>
		protected const string FOCUS_SPELL = "FOCUSING_A_SPELL";
		
		/// <summary>
		/// Property key for focus spells Events
		/// </summary>
		protected const string FOCUS_SPELL_EVENTS = "EVENTS_FOCUSING_A_SPELL";

		/// <summary>
		/// Does this spell ignore any damage cap?
		/// </summary>		
		protected bool m_ignoreDamageCap = false;

		/// <summary>
		/// Does this spell ignore any damage cap?
		/// </summary>
		public bool IgnoreDamageCap
		{
			get { return m_ignoreDamageCap; }
			set { m_ignoreDamageCap = value; }
		}

		/// <summary>
		/// Should this spell use the minimum variance for the type?
		/// Followup style effects, for example, always use the minimum variance
		/// </summary>
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
		/// Does this spell handler should start effect regardless of duration
		/// needed for unlimited spell effect.
		/// </summary>
		public virtual bool ForceStartEffect
		{
			get { return false; }
		}
		
		/// <summary>
		/// The CastingCompleteEvent
		/// </summary>
		public event CastingCompleteCallback CastingCompleteEvent;

		/// <summary>
		/// spell handler constructor
		/// <param name="caster">living that is casting that spell</param>
		/// <param name="spell">the spell to cast</param>
		/// <param name="spellLine">the spell line that spell belongs to</param>
		/// </summary>
		public SpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base()
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
				.Append("\nHashcode: ").Append(this.GetHashCode())
				.Append("\nSpell: ").Append(Spell == null ? "(null)" : Spell.ToString())
				.Append("\nSpellLine: ").Append(SpellLine == null ? "(null)" : SpellLine.ToString())
				.Append("\nTarget: ").Append(SpellCastTarget == null ? "(null)" : SpellCastTarget.ToString())
				.ToString();
		}

		#region Pulsing Spells

		/// <summary>
		/// When spell pulses
		/// </summary>
		public virtual void OnSpellPulse(PulsingSpellEffect effect)
		{
			if(Spell.IsFocus)
			{
				// Focus Duraction Check
				if(Spell.Duration > 0 && effect.StartTime+Spell.Duration <= Caster.CurrentRegion.Time)
				{
					effect.Cancel(false);
					MessageToCaster("Your focus spell is ending.", eChatType.CT_SpellExpires);
					FocusSpellAction(null, Caster, null);
					return;
				}
				
				// Focus spell Check Handling
				if(!CheckFocusPulse(SpellCastTarget))
				{
					// Focus can't continue !
					effect.Cancel(false);
					FocusSpellAction(GameLivingEvent.CastFailed, Caster, null);
					return;
				}
				
				if(!CheckFocusPulseApply(SpellCastTarget))
				{
					//Pulse of this Focus won't apply this turn. (or will be apply/breaked after LoS check...)
					return;
				}
			}
			else
			{
				// It's a Song/Chant Pulse
				if(!CheckSpellPulse(SpellCastTarget))
				{
					// Pulsing Song/Chant can't continue !
					effect.Cancel(false);
					if(Spell.NeedInstrument)
						MessageToCaster("You stop playing your song.", eChatType.CT_SpellExpires);
					else
						MessageToCaster("You stop pulsing your effect.", eChatType.CT_SpellExpires);
					
					return;
				}
				
				if(!CheckSpellPulseApply(SpellCastTarget))
				{
					// Pulse of this Song/Chant won't apply this turn. (or will be apply after LoS check...)
					return;
				}
			}
			
			//Consume mana and start spell !
			//If there are LoS check pending the spell will be started in the according LoS Event Handler
			SpellPulseStart();

		}

		/// <summary>
		/// Signal For Pulsing Effect Canceling (force, or player)
		/// </summary>
		/// <param name="effect">Pulsing Effect Signaling</param>
		/// <param name="playerCanceled">True if Canceled by Player</param>
		public virtual void OnSpellPulseCancel(PulsingSpellEffect effect, bool playerCanceled)
		{
			// Canceling by player message
			if(playerCanceled)
			{
				if(Spell.IsFocus)
				{
					FocusSpellAction(null, Caster, null);						
					CancelPulseMessage();
				}
			}
		}
		
		/// <summary>
		/// Start Pulse on Pulsing Spell.
		/// </summary>
		public virtual void SpellPulseStart()
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * Spell.PulsePower);
			StartSpell(SpellCastTarget);
			SpellPulseSubSpell(SpellCastTarget);
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
		
		public virtual bool CancelPulsingSpell(GameLiving living, ISpellHandler spellHandler)
		{
			PulsingSpellEffect effect = SpellHelper.FindPulsingSpellOnTarget(living, spellHandler);
			
			if(effect != null)
			{
				effect.Cancel(false);
				return true;
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

		/// <summary>
		/// Send Default Message for Canceling Pulsing Spell
		/// </summary>
		public virtual void CancelPulseMessage()
		{
			if (!Spell.NeedInstrument)
				MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
			else
				MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
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

			//[Stryve]: Do not break stealth if spell can be cast without breaking stealth.
			if (Caster is GamePlayer && UnstealthCasterOnStart)
				((GamePlayer)Caster).Stealth(false);

			if (Caster.IsEngaging)
			{
				EngageEffect effect = Caster.EffectList.GetOfType<EngageEffect>();

				if (effect != null)
					effect.Cancel(false);
			}

			m_interrupted = false;

			if (Spell.IsPulsing && CancelPulsingSpell(Caster, Spell.SpellType))
			{
				if(Spell.IsFocus)
				{
					FocusSpellAction(null, Caster, null);
				}
				
				CancelPulseMessage();
			}
			else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, SpellCastTarget, Spell, SpellLine))
			{
				if (CheckBeginCast(SpellCastTarget))
				{
					if (Caster is GamePlayer && (Caster as GamePlayer).IsOnHorse && !HasPositiveEffect)
					{
						(Caster as GamePlayer).IsOnHorse = false;
					}

					if (!Spell.IsInstantCast)
					{
						// Notify that casting is starting
						Caster.Notify(GameLivingEvent.CastStarting, Caster, new CastingEventArgs(this, SpellCastTarget));
						
						StartCastTimer(SpellCastTarget);
					}
					else
					{

						if(CheckEndCast(SpellCastTarget))
						{							
							// Notify that casting is starting
							Caster.Notify(GameLivingEvent.CastStarting, Caster, new CastingEventArgs(this, SpellCastTarget));
							
							if (Caster.ControlledBrain == null || !(Caster.ControlledBrain is CasterNotifiedPetBrain))
							{
								SendCastAnimation(0);
							}

							FinishSpellCast(SpellCastTarget);
						}
						else
						{
							success = false;
						}
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
			if (Spell.NeedInstrument)
				return;

			if (Spell.MoveCast)
				return;

			if(InterruptCasting())
			{
            	if (Caster is GamePlayer)
                	(Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SpellHandler.CasterMove"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// This sends the spell messages to the player/target.
		///</summary>
		public virtual void SendSpellMessages()
		{
			if (!Spell.NeedInstrument)
				MessageToCaster("You begin casting a " + Spell.Name + " spell!", eChatType.CT_Spell);
			else
				MessageToCaster("You begin playing " + Spell.Name + "!", eChatType.CT_Spell);
		}

		/// <summary>
		/// casting sequence has a chance for interrupt through attack from enemy
		/// the final decision and the interrupt is done here
		/// </summary>
		/// <param name="attacker">attacker that interrupts the cast sequence</param>
		/// <returns>true if casting was interrupted</returns>
		public virtual bool CasterIsAttacked(GameLiving attacker)
		{
			// Check if the spell can be interrupted
			if(CheckInterrupted(attacker, true))
				return false;
			

			if (IsCasting && Stage < 3)
			{
				if (Caster.ChanceSpellInterrupt(attacker))
				{
					InterruptCasting();
					Caster.LastInterruptMessage = GetInterruptMessage(attacker);
					MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Get Spell Interrupt Message
		/// </summary>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public virtual string GetInterruptMessage(GameLiving attacker)
		{
			return attacker.GetName(0, true) + " attacks you and your spell is interrupted!";
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
			if (!CheckCasterState(selectedTarget, quiet))
				return false;

			// Potion Check
			if (!CheckCasterPotionUse(selectedTarget, quiet))
				return false;
			
			// Ram Check
			if (!CheckCasterSiegeRam(selectedTarget, quiet))
				return false;
			
			// Special Effect Check
			if (!CheckCasterSpecialEffect(selectedTarget, quiet))
				return false;

			if (!CheckInstrument(selectedTarget, quiet))
				return false;
			
			if (!CheckCastSitting(selectedTarget, quiet))
				return false;

			// Check casting in combat.
			if (Caster.AttackState && Spell.CastTime != 0)
			{
				if (Caster.CanCastInCombat(Spell) == false)
				{
					Caster.StopAttack();
					return false;
				}
			}

			if (!CheckInterrupted(selectedTarget, quiet))
				return false;
			
			// Check Recast Timer
			if (Spell.RecastDelay > 0)
			{
				int timeLeft = Caster.GetSkillDisabledDuration(Spell);
				if (timeLeft > 0)
				{
					if (!quiet)
						MessageToCaster("You must wait " + (timeLeft / 1000 + 1).ToString() + " seconds to use this spell!", eChatType.CT_System);
					
					return false;
				}
			}
			
			if (!CheckCastingTarget(ref selectedTarget, quiet, true, true))
				return false;

			if (!CheckPower(selectedTarget, quiet))
				return false;

			if (!CheckConcentration(selectedTarget, quiet))
				return false;

			// Cancel engage if user starts attack
			if (Caster.IsEngaging)
			{
				EngageEffect engage = m_caster.EffectList.GetOfType<EngageEffect>();
				if (engage != null)
				{
					engage.Cancel(false);
				}
			}

			m_spellTarget = selectedTarget;
			return true;
		}

		/// <summary>
		/// Checks run During Cast Sequence Started
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <returns>True if the cast succeed</returns>
		public virtual bool CheckDuringCast(GameLiving target)
		{
			return CheckDuringCast(target, false);
		}

		/// <summary>
		/// Checks run During the Cast Sequence
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Messages ?</param>
		/// <returns>True if the cast succeed</returns>
		public virtual bool CheckDuringCast(GameLiving target, bool quiet)
		{
			if (!CheckCasterState(target, quiet))
				return false;

			// Special Effect Check
			if(!CheckCasterSpecialEffect(target, quiet))
				return false;
			
			if (!CheckInstrument(target, quiet))
				return false;

			if (!CheckCastSitting(target, quiet))
				return false;
			
			// View on Ennemy will be check in LoS Check
			if (!CheckCastingTarget(ref target, quiet, false, !Spell.IsChant))
				return false;
			
			// Los Check During Cast (if server property forbid it it will notify with a null check, area target can't be checked with LoS)
			if(!HasPositiveEffect && !Spell.IsChant && ServerProperties.Properties.CHECK_LOS_DURING_CAST && !Spell.Target.ToLower().Equals("area"))
			{
				// Event will interrupt cast on KO Los check.
				GameEventMgr.AddHandlerUnique(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(InterruptDuringCastOnLoSCheck));
				if(!CheckTargetLoS(target))
				{
					// check los not issued
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(InterruptDuringCastOnLoSCheck));
				}
			}
			
			if (!CheckPower(target, quiet))
				return false;

			if (!CheckConcentration(target, quiet))
				return false;

			return true;
		}

		/// <summary>
		/// Checks run right after the Cast Sequence Started
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <returns>True if the cast succeed</returns>
		public virtual bool CheckAfterCast(GameLiving target)
		{
			return CheckAfterCast(target, false);
		}

		/// <summary>
		/// Checks run right after the Cast Sequence Started
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Messages ?</param>
		/// <returns>True if the cast succeed</returns>
		public virtual bool CheckAfterCast(GameLiving target, bool quiet)
		{

			if (!CheckCasterState(target, quiet))
				return false;

			// Special Effect Check
			if(!CheckCasterSpecialEffect(target, quiet))
				return false;
			
			if (!CheckInstrument(target, quiet))
				return false;

			if (!CheckCastSitting(target, quiet))
				return false;

			if (!CheckCastingTarget(ref target, quiet, false, !Spell.IsChant))
				return false;
			
			if (!CheckPower(target, quiet))
				return false;

			if (!CheckConcentration(target, quiet))
				return false;

			return true;
		}

		/// <summary>
		/// Checks right after casting before spell is started
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <returns>True if the cast Succeed</returns>
		public virtual bool CheckEndCast(GameLiving target)
		{
			return CheckEndCast(target, false);
		}
		
		/// <summary>
		/// Checks right after casting before spell is started
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Messages ?</param>
		/// <returns>True if the cast Succeed</returns>
		public virtual bool CheckEndCast(GameLiving target, bool quiet)
		{
			if (!CheckCasterState(target, quiet))
				return false;

			// Special Effect Check
			if(!CheckCasterSpecialEffect(target, quiet))
				return false;

			if (!CheckInstrument(target, quiet))
				return false;
			
			if (!CheckCastSitting(target, quiet))
				return false;

			if (!CheckCastingTarget(ref target, quiet, false, !Spell.IsChant))
				return false;

			if (!CheckPower(target, quiet))
				return false;

			if (!CheckConcentration(target, quiet))
				return false;

			return true;
		}

		/// <summary>
		/// Check During Focus Pulsing Spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns>False if the focus spell must be stopped</returns>
		public virtual bool CheckFocusPulse(GameLiving target)
		{
			return CheckFocusPulse(target, false);
		}
		
		/// <summary>
		/// Check During Focus Pulsing Spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns>False if the focus spell must be stopped</returns>
		public virtual bool CheckFocusPulse(GameLiving target, bool quiet)
		{
			if (!CheckCasterState(target, true))
				return false;

			// Special Effect Check
			if(!CheckCasterSpecialEffect(target, quiet))
				return false;
			
			// Check Pulse Power
			if(!CheckPulsePower(target, quiet))
				return false;
			
			// Check Instrument
			if(!CheckInstrument(target, quiet))
				return false;
			
			// Check if the target is still valid/in range/in view, LOS check should be run in Pulse Apply !
			if (!CheckCastingTarget(ref target, quiet, false, true))
				return false;
			
			return true;
		}

		/// <summary>
		/// Check if Focus Pulse Should Apply (LoS check on ennemy)
		/// This won't break focus on returning false.
		/// </summary>
		/// <param name="target">True if the Focus Should Apply and Continue, False if Los Check is Pending.</param>
		/// <returns></returns>
		public virtual bool CheckFocusPulseApply(GameLiving target)
		{
			return CheckFocusPulseApply(target, false);
		}

		/// <summary>
		/// Check if Focus Pulse Should Apply (LoS check on ennemy)
		/// This won't break focus on returning false.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns>True if the Focus Should Apply and Continue, False if Los Check is Pending.</returns>
		public virtual bool CheckFocusPulseApply(GameLiving target, bool quiet)
		{
			// Purely Needed for LoS check
			if(!HasPositiveEffect)
			{
				// Try a Los Check
				GameEventMgr.AddHandlerUnique(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusPulseLOS));
				if(!CheckTargetLoS(target))
				{
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusPulseLOS));
					// Continue with focus if no LoS checks...
					return true;
				}

				// Let focus event apply pulse (or cancel focus).
				return false;
			}
			
			// continue with focus pulse.
			return true;
		}
		
		/// <summary>
		/// Check during Spell Pulsing (Chant/Song)
		/// </summary>
		/// <param name="target"></param>
		/// <returns>False if the song/chant must be stopped</returns>
		public virtual bool CheckSpellPulse(GameLiving target)
		{
			return CheckSpellPulse(target, false);
		}
		
		/// <summary>
		/// Check during Spell Pulsing (Chant/Song)
		/// Pulse Chant/Song will stop on these check, other check must be made to check if Effect should be apply but song not stopped.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns>False if the song/chant must be stopped</returns>
		public virtual bool CheckSpellPulse(GameLiving target, bool quiet)
		{
			if (!CheckCasterState(target, true))
				return false;

			// Special Effect Check
			if(!CheckCasterSpecialEffect(target, quiet))
				return false;
			
			// Check Pulse Power
			if (!CheckPulsePower(target, quiet))
				return false;
			
			// Check Instrument
			if (!CheckInstrument(target, quiet))
				return false;
			
			// We don't check target here, but if target is dead (Caster excluded) we should stop the song !
			if (target != Caster && !target.IsAlive)
			{
				if (!quiet)
					MessageToCaster("Your target is dead, you stop your song.", eChatType.CT_SpellExpires);
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// Check if the spell Pulsing (Chant/Song) should'nt apply effect, but don't stop the pulse.
		/// CheckSpellPulse() should have been checked before !
		/// </summary>
		/// <param name="target"></param>
		/// <returns>True if the effect shouldn't be applied. (False anyway if a LoS Check is needed)</returns>
		public virtual bool CheckSpellPulseApply(GameLiving target)
		{
			return CheckSpellPulseApply(target, false);
		}

		/// <summary>
		/// Check if the spell Pulsing (Chant/Song) should'nt apply effect, but don't stop the pulse.
		/// CheckSpellPulse() should have been checked before !
		/// </summary>
		/// <param name="target"></param>
		/// <returns>False if the effect shouldn't be applied. (False anyway if a LoS Check is needed)</returns>		
		public virtual bool CheckSpellPulseApply(GameLiving target, bool quiet)
		{
			// If caster is CC and cannot cast
			if (Caster.IsMezzed || Caster.IsStunned)
				return false;
			
			// If target is not in range/visible/etc, do not notify the spell will not fail
			if (!CheckCastingTarget(ref target, quiet, false, true, false))
				return false;
						
			// If caster Target is out of LoS
			if(!HasPositiveEffect)
			{
				// Run Los Check.
				GameEventMgr.AddHandlerUnique(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckSpellPulseApplyLOS));
				if(!CheckTargetLoS(target))
				{
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckSpellPulseApplyLOS));
					// no los check issued continue with effect.
					return true;
				}
				
				// Leave the LoS check finish the spell casting.
				return false;
			}
			
			// pulse should apply
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
		/// Los Check on Pulsing Spell Chant, Start the Spell if Pulse can Apply ! (do nothing otherwise)
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void CheckSpellPulseApplyLOS(DOLEvent e, object sender, EventArgs args)
		{
			if(e == GameLivingEvent.FinishedLosCheck && sender == Caster && args is LosCheckData)
			{
				if(!((LosCheckData)args).LosOK)
				{
					MessageToCaster("You can't see your target and your song has no effect!", eChatType.CT_SpellResisted);
				}
				else
				{
					// Apply Pulse Spell - Consume mana and start spell !
					SpellPulseStart();
				}
			}
			
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckSpellPulseApplyLOS));
		}
		
		/// <summary>
		/// LoS check on Focus Pulse when spell has not positive effects
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void CheckFocusPulseLOS(DOLEvent e, object sender, EventArgs args)
		{
			if(e == GameLivingEvent.FinishedLosCheck && sender == Caster && args is LosCheckData)
			{
				if(!((LosCheckData)args).LosOK)
				{
					MessageToCaster("You can't focus on your target without seeing it!", eChatType.CT_SpellResisted);
					
					// Focus without LoS need canceling...
					FocusSpellAction(GameLivingEvent.CastFailed, Caster, null);
				}
				else
				{
					// Apply Pulse Spell - Consume mana and start spell !
					SpellPulseStart();
				}
			}
			
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusPulseLOS));
		}
		
		/// <summary>
		/// Event Handling on LoS Check During Cast
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void InterruptDuringCastOnLoSCheck(DOLEvent e, object sender, EventArgs args)
		{
			if(e == GameLivingEvent.FinishedLosCheck && args is LosCheckData && sender == Caster)
			{
				if(!((LosCheckData)args).LosOK)
				{
					MessageToCaster("You can't see your target from here!", eChatType.CT_SpellResisted);

					InterruptCasting();
				}
			}
			
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(InterruptDuringCastOnLoSCheck));
		}
		
		/// <summary>
		/// Checks if caster holds the right instrument for this spell
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send message ?</param> 
		/// <returns>true if right instrument or no instrument requirement</returns>
		protected virtual bool CheckInstrument(GameLiving target, bool quiet = false)
		{
			// If the spell does not require instrument this is always true
			if(!Spell.NeedInstrument)
				return true;
			
			InventoryItem instrument = Caster.AttackWeapon;
			// From patch 1.97:  Flutes, Lutes, and Drums will now be able to play any song type, and will no longer be limited to specific songs.
			if (instrument == null || instrument.Object_Type != (int)eObjectType.Instrument)
			{
				if (!quiet)
					MessageToCaster("You are not wielding an instrument!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Checks if caster has enough concentration to cast this spell
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send message ?</param>
		/// <returns>True if Caster has enough concentration</returns>
		protected virtual bool CheckConcentration(GameLiving target, bool quiet = false)
		{
			if (Caster is GamePlayer && Spell.IsConcentration && Caster.Concentration < Spell.Concentration)
			{
				if (!quiet)
					MessageToCaster("This spell requires " + Spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && Spell.IsConcentration && Caster.ConcentrationEffects.ConcSpellsCount >= 50)
			{
				if (!quiet)
					MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if Caster has enough Power to cast the spell
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster has enough Power.</returns>
		protected virtual bool CheckPower(GameLiving target, bool quiet = false)
		{
			if (Caster.Mana <= 0 && Spell.UsePower)
			{
				if (!quiet)
					MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			
			if (Spell.UsePower && Caster.Mana < PowerCost(target, false))
			{
				if (!quiet)
					MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// Check if Caster has enough Power for next Pulse.
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster has enough Power.</returns>
		protected virtual bool CheckPulsePower(GameLiving target, bool quiet = false)
		{
			if (Spell.UsePulsePower && Caster.Mana < Spell.PulsePower)
			{
				MessageToCaster("You do not have enough mana and your spell was cancelled.", eChatType.CT_SpellExpires);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if Caster is Interrupted or can still cast
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		protected virtual bool CheckInterrupted(GameLiving target, bool quiet = false)
		{
			// if we are in interrupt state and the spellhandler has not been noticed before.
			if(Caster.IsBeingInterrupted)
			{
				// Check if Necro Pet has owner with MoC
				if (Caster is GameNPC && ((GameNPC)Caster).Brain is CasterNotifiedPetBrain)
				{
					GameLiving owner = ((CasterNotifiedPetBrain)((GameNPC)Caster).Brain).GetLivingOwner();
					if(owner != null && owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
						return true;
				}
				
				// Check if spell can be interrupted
				if (!Spell.Uninterruptible && !Spell.IsInstantCast &&
			    Caster.EffectList.GetOfType<QuickCastEffect>() == null && 
			    Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null && 
				Caster.EffectList.GetOfType<FacilitatePainworkingEffect>() == null)
				{
					if (!quiet)
						MessageToCaster("You are interrupted and must wait " + (((Caster.InterruptTime) - Caster.CurrentRegion.Time) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Check if can Cast while Sitting
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		public virtual bool CheckCastSitting(GameLiving target, bool quiet = false)
		{
			if (Caster.IsSitting && !Spell.NeedInstrument) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				if (!quiet)
					MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if Caster is in state to Cast (mostly object state and Alive)
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		public virtual bool CheckCasterState(GameLiving target, bool quiet = false)
		{
			if (Caster.ObjectState != GameLiving.eObjectState.Active)
			{
				return false;
			}

			if (!Caster.IsAlive)
			{
				if (!quiet)
					MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if Caster used a postion recently
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		public virtual bool CheckCasterPotionUse(GameLiving target, bool quiet = false)
		{
			if(Caster is GamePlayer)
			{
				long nextSpellAvailTime = Caster.TempProperties.getProperty<long>(GamePlayer.NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION, 0);

				if (nextSpellAvailTime > Caster.CurrentRegion.Time)
				{
					if(!quiet)
						((GamePlayer)Caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client, "GamePlayer.CastSpell.MustWaitBeforeCast", (nextSpellAvailTime - Caster.CurrentRegion.Time) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					
					return false;
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if Caster is on a Ram
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		public virtual bool CheckCasterSiegeRam(GameLiving target, bool quiet = false)
		{
			if(Caster is GamePlayer)
			{
				if (((GamePlayer)Caster).Steed != null && ((GamePlayer)Caster).Steed is GameSiegeRam)
				{
					if (!quiet)
						MessageToCaster("You can't cast in a siegeram!.", eChatType.CT_System);
					
					return false;
				}
			}
			
			return true;
		}

		/// <summary>
		/// Check if Caster has any special effect preventing him from casting.
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Message ?</param>
		/// <returns>True if the caster can cast</returns>
		public virtual bool CheckCasterSpecialEffect(GameLiving target, bool quiet = false)
		{
			// Nature's Womb silences
			IGameEffect naturesWomb = SpellHelper.FindStaticEffectOnTarget(Caster, typeof(NaturesWombEffect));
			if (naturesWomb != null)
			{
				if (!quiet)
					MessageToCaster("You are silenced and cannot cast a spell right now!", eChatType.CT_SpellResisted);
				return false;
			}
			
			// Phase Shifted
			GameSpellEffect Phaseshift = SpellHelper.FindEffectOnTarget(Caster, typeof(PhaseshiftHandler));
			if (Phaseshift != null && (!Spell.NeedInstrument || !HasPositiveEffect))
			{
				if (!quiet)
					MessageToCaster("You're phaseshifted and can't cast a spell!", eChatType.CT_System);
				return false;
			}

			// Apply Mentalist RA5L
			if (Spell.Range > 0)
			{
				SelectiveBlindnessEffect selectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
				if (selectiveBlindness != null)
				{
					if(selectiveBlindness.EffectSource == target)
					{
						if (m_caster is GamePlayer && !quiet)
							MessageToCaster(string.Format("{0} is invisible to you!", target.GetName(0, true)), eChatType.CT_Missed);

						return false;
					}
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Check if the Target is valid for casting.
		/// </summary>
		/// <param name="target">Spell Target</param>
		/// <param name="quiet">Send Messages ?</param>
		/// <param name="needViewRealm">Target "Realm" needs to be in View ?</param>
		/// <param name="needViewOffensive">Target "Enemy" needs to be in View ?</param>
		/// <param name="notify">Notify event handler ?</param>
		/// <returns>True if the cast succeed</returns>
		public virtual bool CheckCastingTarget(ref GameLiving target, bool quiet = false, bool needViewRealm = false, bool needViewOffensive = true, bool notify = true)
		{
			String targetType = Spell.Target.ToLower();
		
			// spell target that defaults to Caster
			if (targetType.Equals("self") || targetType.Equals("group") || targetType.Equals("cone") || Spell.IsPBAoE)
			{
				target = Caster;
				return true;
			}
			
			// Can cast pet spell on all Pet/Turret/Minion (our pet)
			if (targetType.Equals("pet") || targetType.Equals("controlled"))
			{
				// If no target or target isn't a controlled Pet try to auto select Pet
				if (target == null || !Caster.IsControlledNPC(target as GameNPC))
				{
					if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
					{
						target = Caster.ControlledBrain.Body;
					}
					else
					{
						if (!quiet)
							MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_SpellResisted);
						
						return false;
					}
				}
			}
			
			// Check GT Area spell
			if (targetType.Equals("area"))
			{
				target = Caster;
				
				if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
				{
					if (!quiet)
						MessageToCaster("Your ground target is out of range. Select a closer target.", eChatType.CT_SpellResisted);
					
					if (notify)
						Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
					
					return false;
				}
				
				if (!Caster.GroundTargetInView)
				{
					if (!quiet)
						MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
					
					if (notify)
						Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
					
					return false;
				}
			}
			
			// Check Spell that need a Target (Corpse, KeepComponent, Enemy, Realm)
			if ((targetType.Equals("corpse") || targetType.Equals("enemy") || targetType.Equals("realm") || targetType.Equals("keepcomponent")
			     || targetType.Equals("pet") || targetType.Equals("controlled")) && !Spell.IsPBAoE)
			{
				// All spells that need a target.
				if (target == null || target.ObjectState != GameLiving.eObjectState.Active)
				{
					if (!quiet)
						MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
					
					return false;
				}

				// All spells that aren't meant for dead bodies !
				if (!targetType.Equals("corpse") && !target.IsAlive)
				{
					if (!quiet)
						MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
					
					return false;
				}
				
				if (Spell.Range > 0 && !Caster.IsWithinRadius(target, CalculateSpellRange()))
				{
					if (!quiet)
						MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					
					if (notify)
						Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
					
					return false;
				}
				
				switch (targetType)
				{
					case "enemy":
					case "keepcomponent":
						if (target == Caster)
						{
							if (!quiet)
								MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
							return false;
						}

						if (SpellHelper.FindStaticEffectOnTarget(target, typeof(NecromancerShadeEffect)) != null)
						{
							if (!quiet)
								MessageToCaster("This target can't be attacked in this state.", eChatType.CT_System);
							return false;
						}

						if (!Caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
						{
							if (!quiet)
								MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
							
							if (notify)
								Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
							
							return false;
						}
						
						if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) && GameServer.ServerRules.IsSameRealm(Caster, target, true))
						{
							if (!quiet)
								MessageToCaster("You can't attack a member of your realm!", eChatType.CT_SpellResisted);
							
							return false;
						}
						else if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
						{
							if (!quiet)
								MessageToCaster("Your can't attack this target!", eChatType.CT_SpellResisted);
							
							return false;
						}
						
					break;

					case "corpse":
						if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, true))
						{
							if (!quiet)
								MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							
							return false;
						}
						
					break;

					case "realm":
						if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) || !GameServer.ServerRules.IsSameRealm(Caster, target, true))
						{
							if (!quiet)
								MessageToCaster("You must use this spell on a member of your realm!", eChatType.CT_SpellResisted);
							
							return false;
						}
						
					break;
				}
				
				//heals/buffs/rez need LOS only to start casting
				// we can't check los on non-selected pet...
				if ((needViewRealm && Caster.TargetObject == target && !Caster.TargetInView && !targetType.Equals("enemy") && !targetType.Equals("keepcomponent") && !targetType.Equals("pet") && !targetType.Equals("controlled"))
				   || (needViewOffensive && Caster.TargetObject == target && !Caster.TargetInView && (targetType.Equals("enemy") || targetType.Equals("keepcomponent"))))
				{
					if (!quiet)
						MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
					
					if (notify)
						Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
					
					return false;
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Check LoS Caster to Target
		/// Launch a Event Notify on result
		/// </summary>
		/// <param name="target"></param>
		/// <returns>False if the LoS check couldn't be issued</returns>
		public virtual bool CheckTargetLoS(GameLiving target)
		{
			// don't issue los check !			
			if(Caster == target)
				return false;
			
			//issue Los Check

			GamePlayer playerChecker = null;

			if (Caster is GamePlayer)
			{
				playerChecker = Caster as GamePlayer;
			}
			else if (target is GamePlayer)
			{
				playerChecker = target as GamePlayer;
			}
			else if (Caster is GameNPC && (Caster as GameNPC).Brain != null && (Caster as GameNPC).Brain is IControlledBrain)
			{
				playerChecker = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
			}
			else if (target is GameNPC && (target as GameNPC).Brain != null && (target as GameNPC).Brain is IControlledBrain)
			{
				playerChecker = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
			}

			if (playerChecker != null)
			{
				// This will generate a LoS Notify
				playerChecker.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckLOSCasterToTarget));
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Handle the Line of Sight Result from a player to a target
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">The result</param>
		/// <param name="targetOID">The target OID</param>
		public virtual void CheckLOSCasterToTarget(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null) // Hmm
			{
				// Notify los failure for event handling
				Caster.Notify(GameLivingEvent.FinishedLosCheck, Caster);
				return;
			}

			if (ServerProperties.Properties.ENABLE_DEBUG)
			{
				MessageToCaster("LoS Check in CheckLOSPlayerToTarget", eChatType.CT_System);
				if (log.IsDebugEnabled)
					log.Debug("LoS Check in CheckLOSCasterToTarget");
			}
			
			if ((response & 0x100) == 0x100) // In view?
			{
				Caster.Notify(GameLivingEvent.FinishedLosCheck, Caster, new LosCheckData(Caster, player.CurrentRegion.GetObject(targetOID), GameTimer.GetTickCount(), true));
				return;
			}
			
			Caster.Notify(GameLivingEvent.FinishedLosCheck, Caster, new LosCheckData(Caster, player.CurrentRegion.GetObject(targetOID), GameTimer.GetTickCount(), false));
		}
		#endregion

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target">Target of the Spell</param>
		/// <param name="consume">If false compute spell cost only.</param>
		/// <returns>Spell casting Power Cost</returns>
		public virtual int PowerCost(GameLiving target, bool consume)
		{
			// warlock
			GameSpellEffect effect = SpellHelper.FindEffectOnTarget(Caster, "Powerless");
			if (effect != null && !m_spell.IsPrimary)
				return 0;
			
			if(consume)
			{
				//1.108 - Valhallas Blessing now has a 75% chance to not use power.
				ValhallasBlessingEffect ValhallasBlessing = Caster.EffectList.GetOfType<ValhallasBlessingEffect>();
				if (ValhallasBlessing != null && Util.Chance(75))
					return 0;
	
				//patch 1.108 increases the chance to not use power to 50%.
				FungalUnionEffect FungalUnion = Caster.EffectList.GetOfType<FungalUnionEffect>();
				{
					if (FungalUnion != null && Util.Chance(50))
						return 0;
				}
	
				// Arcane Syphon chance
				int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
				if (syphon > 0)
				{
					if(Util.Chance(syphon))
					{
						return 0;
					}
				}
			}

			double basepower = Spell.Power; //<== defined a basevar first then modified this base-var to tell %-costs from absolut-costs

			// percent of maxPower if less than zero
			if (basepower < 0)
			{
				if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ManaStat != eStat.UNDEFINED)
				{
					GamePlayer player = Caster as GamePlayer;
					basepower = player.CalculateMaxMana(player.Level, player.GetBaseStat(player.CharacterClass.ManaStat)) * basepower * -0.01;
				}
				else
				{
					basepower = Caster.MaxMana * basepower * -0.01;
				}
			}

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
			
			if(consume)
			{
				// doubled power usage if quickcasting
				if (Caster.EffectList.GetOfType<QuickCastEffect>() != null && !Spell.IsInstantCast)
					power *= 2;
			}
			
			return (int)power;
		}

		/// <summary>
		/// Calculates the enduance cost of the spell
		/// </summary>
		/// <returns></returns>
		public virtual int CalculateEnduranceCost(bool consume)
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
		public virtual bool InterruptCasting()
		{
			if (m_interrupted || !IsCasting)
				return false;

			m_interrupted = true;

			if (m_castTimer != null)
			{
				m_castTimer.Stop();
				m_castTimer = null;

				if (Caster is GamePlayer)
				{
					((GamePlayer)Caster).ClearSpellQueue();
				}
			}

			SendInterruptCastAnimation();
			
			// don't start reuse timer
			m_startReuseTimer = false;
			OnAfterSpellCastSequence();
			
			return true;
		}

		/// <summary>
		/// Calculates the effective casting time
		/// </summary>
		/// <returns>effective casting time in milliseconds</returns>
		public virtual int CalculateCastingTime()
		{
			return Caster.CalculateCastingTime(SpellLine, Spell);
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
			SendCastAnimation(castTime, Spell.ClientEffect);
		}
		
		/// <summary>
		/// Sends the cast animation
		/// </summary>
		/// <param name="castTime">The cast time</param>
		/// <param name="clientEffect">Cast Animation</param>
		public virtual void SendCastAnimation(ushort castTime, ushort clientEffect)
		{
			foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				GamePlayer playAnimation = player;
				
				if (playAnimation != null)
					playAnimation.Out.SendSpellCastAnimation(Caster, clientEffect, castTime);
			}
		}

		/// <summary>
		/// Send the Effect Animation
		/// </summary>
		/// <param name="target">The target object</param>
		/// <param name="boltDuration">The duration of a bolt</param>
		/// <param name="noSound">sound?</param>
		/// <param name="success">spell success?</param>
		public void SendEffectAnimation(GameObject target, ushort boltDuration, bool noSound, byte success)
		{
			SendEffectAnimation(target, Spell.ClientEffect, boltDuration, noSound, success);
		}

		/// <summary>
		/// Send the Effect Animation
		/// </summary>
		/// <param name="target">The target object</param>
		/// <param name="clientEffect">Spell Animation Effect ID</param>
		/// <param name="boltDuration">The duration of a bolt</param>
		/// <param name="noSound">sound?</param>
		/// <param name="success">spell success?</param>
		public virtual void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
		{
			if (target == null)
				target = Caster;

			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				GamePlayer playAnimation = player;
				if (playAnimation != null)
					player.Out.SendSpellEffectAnimation(Caster, target, clientEffect, boltDuration, noSound, success);
			}
		}
		
		/// <summary>
		/// Send the Interrupt Cast Animation
		/// </summary>
		public virtual void SendInterruptCastAnimation()
		{
			foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				GamePlayer playAnimation = player;
				if (playAnimation != null)
					player.Out.SendInterruptAnimation(Caster);
			}
		}
		#endregion

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public virtual void FinishSpellCast(GameLiving target)
		{			
			//turn into wraith
			if (Caster is GamePlayer
			    && ((Caster as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Bainshee)
			    && (Caster as GamePlayer).InWraithForm == false
			    && !HasPositiveEffect)
			{
				(Caster as GamePlayer).InWraithForm = true;
			}

			if (Caster is GamePlayer && ((GamePlayer)Caster).IsOnHorse && !HasPositiveEffect)
				((GamePlayer)Caster).IsOnHorse = false;
			
			//[Stryve]: Do not break stealth if spell never breaks stealth.
			if ((Caster is GamePlayer) && UnstealthCasterOnFinish )
				((GamePlayer)Caster).Stealth(false);

			if (Caster is GamePlayer && !HasPositiveEffect)
			{
				if (Caster.AttackWeapon != null && Caster.AttackWeapon is GameInventoryItem)
				{
					(Caster.AttackWeapon as GameInventoryItem).OnSpellCast(Caster, target, Spell);
				}
			}

			// messages
			if (!Spell.NeedInstrument && Spell.ClientEffect != 0 && !Spell.IsInstantCast)
			{
				MessageToCaster("You cast a " + m_spell.Name + " spell!", eChatType.CT_Spell);
				foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					if (player != Caster)
						player.MessageFromArea(Caster, Caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			// Handle Focus spell
			if(Spell.IsFocus)
			{
				AddFocusEventHandler(target);
			}
			
			if (Spell.IsPulsing && Spell.Frequency > 0)
			{
				CancelAllPulsingSpells(Caster);
				PulsingSpellEffect pulseeffect = new PulsingSpellEffect(this);
				pulseeffect.Start();
				// show animation on caster for positive spells, negative shows on every StartSpell
				if (Spell.Target.ToLower().Equals("self") || Spell.Target.ToLower().Equals("group"))
					SendEffectAnimation(Caster, 0, false, 1);
				if (Spell.Target.ToLower().Equals("pet"))
					SendEffectAnimation(target, 0, false, 1);
				
				OnSpellPulse(pulseeffect);

			}
			else
			{
				StartSpell(target); // and action
				//Subspells
				FinishCastSubSpell(target);				
			}

			
			
			//Dinberg: This is where I moved the warlock part (previously found in gameplayer) to prevent
			//cancelling before the spell was fired.
			if (Spell.SpellType != "Powerless" && Spell.SpellType != "Range" && Spell.SpellType != "Uninterruptable")
			{
				GameSpellEffect effect = SpellHelper.FindEffectOnTarget(Caster, "Powerless");
				if (effect == null)
					effect = SpellHelper.FindEffectOnTarget(Caster, "Range");
				if (effect == null)
					effect = SpellHelper.FindEffectOnTarget(Caster, "Uninterruptable");

				//if we found an effect, cancel it!
				if (effect != null)
					effect.Cancel(false);
			}

			//the quick cast is unallowed whenever you miss the spell
			//set the time when casting to can not quickcast during a minimum time
			if (Caster is GamePlayer)
			{
				QuickCastEffect quickcast = m_caster.EffectList.GetOfType<QuickCastEffect>();
				if (quickcast != null && !Spell.IsInstantCast)
				{
					Caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, Caster.CurrentRegion.Time);
					((GamePlayer)Caster).DisableSkill(Caster.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
					quickcast.Cancel(false);
					// Disable interruption after a quickcast spell
					Caster.InterruptTime = 0;
				}
			}


			if (Ability != null)
				Caster.DisableSkill(Ability.Ability, (!Spell.HasRecastDelay ? 3 : Spell.RecastDelay));

			// disable spells with recasttimer (Disables group of same type with same delay)
			if (Spell.HasRecastDelay && StartReuseTimer)
			{
				// TODO Update this to be handled by GetUsableSpells, then adapt it for GameLiving ?
				if (Caster is GamePlayer)
				{
					GamePlayer gp_caster = Caster as GamePlayer;
					foreach (SpellLine spellline in gp_caster.GetSpellLines())
						foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
							if (sp.Level <= spellline.Level && (sp == Spell || (sp.SharedTimerGroup != 0 && (sp.SharedTimerGroup == Spell.SharedTimerGroup))))
								Caster.DisableSkill(sp, sp.RecastDelay);
				}
				else if (Caster is GameNPC)
					Caster.DisableSkill(Spell, Spell.RecastDelay);
			}

			GameEventMgr.Notify(GameLivingEvent.CastFinished, Caster, new CastingEventArgs(this, target, GetLastAttackData(target)));
		}

		/// <summary>
		/// Sub Spell Started when main Spell Finish Casting
		/// </summary>
		public virtual void FinishCastSubSpell(GameLiving target)
		{
			if (Spell.HasSubSpell)
			{
				Spell spell = SkillBase.GetSpellByID(Spell.SubSpellID);
				spell.Level = Spell.Level;
				//we need subspell ID to be 0, we don't want spells linking off the subspell
				if (target != null && spell != null && !spell.HasSubSpell)
				{
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, spell, SpellLine);
					spellhandler.StartSpell(target);
				}
			}
		}
		
		/// <summary>
		/// Sub Spell Started when main Spell Pulse
		/// </summary>
		public virtual void SpellPulseSubSpell(GameLiving target)
		{
			if (Spell.HasSubSpell)
			{
				Spell spell = SkillBase.GetSpellByID(Spell.SubSpellID);
				spell.Level = Spell.Level;
				//we need subspell ID to be 0, we don't want spells linking off the subspell
				if (target != null && spell != null && !spell.HasSubSpell)
				{
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, spell, SpellLine);
					spellhandler.StartSpell(target);
				}
			}
		}
		
		/// <summary>
		/// Select all targets for this spell
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public virtual IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			List<GameLiving> list = new List<GameLiving>(8);
			GameLiving target = castTarget as GameLiving;
			bool targetchanged = false;
			string modifiedTarget = Spell.Target.ToLower();
			ushort modifiedRadius = (ushort)Spell.Radius;
			int newtarget = 0;

			GameSpellEffect TargetMod = SpellHelper.FindEffectOnTarget(Caster, "TargetModifier");
			if (TargetMod != null)
			{
				if (modifiedTarget == "enemy" || modifiedTarget == "realm" || modifiedTarget == "group")
				{
					newtarget = (int)TargetMod.Spell.Value;

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
								modifiedRadius = (ushort)m_spell.Range;
								targetchanged = true;
							}
							break;
						case 2: // apply on enemy
							if (modifiedTarget == "enemy")
							{
								if (!Spell.IsAoE)
								{
									modifiedRadius = 450;
								}
								else
								{
									modifiedRadius += 300;
								}
								targetchanged = true;
							}
							break;
						case 3: // Apply on buff
							if (Spell.Target.ToLower() == "group"
							    && Spell.IsPulsing)
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
						TargetMod.Cancel(false);
				}
			}

			#region Process the targets
			switch (modifiedTarget)
			{
					#region GTAoE
					// GTAoE
				case "area":
					if (modifiedRadius > 0)
					{
						foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
							{
								list.Add(player);
							}
						}
						foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
						{
							if (npc is GameStorm)
								list.Add(npc);
							
							else if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
							{
								list.Add(npc);
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
						if (target == null) 
							return null;
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
								list.Add(npc);
							}
						}
						
						// Add the target in last position for AoE.
						if(list.Contains(target))
						{
							list.Remove(target);
							list.Add(target);
						}
					}
					else
					{
						if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
						{
							list.Add(target);
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

							list.Add(npc);
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
		/// This is typically called after calling CheckBeginCast & FinishSpellCast & OnSpellPulse
		/// </summary>
		/// <param name="target">The current target object</param>
		public virtual bool StartSpell(GameLiving target)
		{
			// FIXME if we don't have a correct target try to use args
			if (SpellCastTarget == null)
			{
				if (target == null)
				    return false;
				
				m_spellTarget = target;
			}
			
			IList<GameLiving> targets = SelectTargets(SpellCastTarget);

			// cancel effect from focused target not anymore in target list
			if(Spell.IsFocus)
			{
				ConcurrentDictionary<GameLiving, GameSpellEffect> focusedEffects = Caster.TempProperties.getProperty<ConcurrentDictionary<GameLiving, GameSpellEffect>>(FOCUS_SPELL);
				if(focusedEffects != null)
				{
					foreach(KeyValuePair<GameLiving, GameSpellEffect> focusIterate in focusedEffects.ToArray())
					{
						KeyValuePair<GameLiving, GameSpellEffect> focusEffect = focusIterate;
						if(focusEffect.Key != null && !targets.Contains(focusEffect.Key))
						{
							GameSpellEffect dummy;
							// effect owner not in target list
							focusEffect.Value.Cancel(false);
							focusedEffects.TryRemove(focusEffect.Key, out dummy);
						}
					}
					
				}
			}
			
			double effectiveness = Caster.Effectiveness;
			if (log.IsDebugEnabled)
				log.DebugFormat("StartSpell - Caster Effectiveness {0} ({1})", effectiveness, Caster.GetModified(eProperty.LivingEffectiveness));

			if (Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
			{
				MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
				if (ra != null && ra.Level > 0)
				{
					effectiveness *= System.Math.Round((double)ra.GetAmountForLevel(ra.Level) * 0.01, 2);
				}
			}

			//[StephenxPimentel] Reduce Damage if necro is using MoC
			if (Caster is NecromancerPet)
			{
				if ((Caster as NecromancerPet).Owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
				{
					MasteryofConcentrationAbility necroRA = (Caster as NecromancerPet).Owner.GetAbility<MasteryofConcentrationAbility>();
					if (necroRA != null && necroRA.Level > 0)
					{
						effectiveness *= System.Math.Round((double)necroRA.GetAmountForLevel(necroRA.Level) * 0.01, 2);
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

				if (Spell.IsAoE && Spell.Target.ToLower() == "enemy"
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
			return ((double)distance / (double)radius) / 2.0;
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		public virtual int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			// explicit unlimited.
			if (Spell.Duration == 0)
				return 0;
			
			double duration = Spell.Duration;
			
			duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
			
			if (Spell.NeedInstrument)
			{
				InventoryItem instrument = Caster.AttackWeapon;
				if (instrument != null)
				{
					duration *= 1.0 + Math.Min(1.0, instrument.Level / (double)Caster.Level); // up to 200% duration for songs
					duration *= instrument.Condition / (double)instrument.MaxCondition * instrument.Quality * 0.01;
				}
			}
			
			if(!HasPositiveEffect)
			{
				// if it's a song/chant without instrument effectiveness shouldn't reduce the pulse
				duration *= effectiveness;	
			}

			// Cap duration between 1 msec and Duration * 4 
			duration = Math.Min(Math.Max(duration, 1), Spell.Duration * 4);
			
			return (int)duration;
		}

		/// <summary>
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public virtual GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			int freq = 0;
			int duration = 0;

			if (Spell.IsChant)
			{
				// "Song", no frequency it's the spell pulsing !
				duration = CalculateEffectDuration(target, effectiveness);
			}
			else if (!Spell.IsFocus)
			{
				// standard "over time" spells
				duration = CalculateEffectDuration(target, effectiveness);
				freq = Spell.Frequency;
			}		
			
			return new GameSpellEffect(this, duration, freq, effectiveness);
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
				effect1 = SpellHelper.FindEffectOnTarget(target, "Phaseshift");
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

			if (effectiveness <= 0)
				return; // no effect

			if ((Spell.Duration > 0 && Spell.Target.ToLower() != "area") || Spell.Concentration > 0 || Spell.IsFocus || ForceStartEffect)
			{
				if (!target.IsAlive)
					return;
				eChatType noOverwrite = (!Spell.IsPulsing) ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
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

								if (IsNewEffectBetter(gsp, neweffect))
								{
									overwriteEffect = gsp;
								}
								else
								{
									OnFailedApplyEffectOnTarget(target, gsp, neweffect);
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
				ad.AttackResult = eAttackResult.HitUnstyled;
				ad.IsSpellResisted = false;

				AppendAttackData(ad);
			}
		}
		
		/// <summary>
		/// Message and Animation for Failed Apply Effect (no overwrite)
		/// </summary>
		/// <param name="target">Target for the apply</param>
		/// <param name="oldEffect">Old Effect than can't be overwrite</param>
		/// <param name="newEffect">New Effect that tried overwrite</param>
		public virtual void OnFailedApplyEffectOnTarget(GameLiving target, GameSpellEffect oldEffect, GameSpellEffect newEffect)
		{
			eChatType noOverwrite = !Spell.IsPulsing ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
			
			if (target == Caster)
			{
				MessageToCaster("You already have that effect. Wait until it expires. Spell failed.", noOverwrite);
			}
			else
			{
				MessageToCaster(target.GetName(0, true) + " already has that effect.", noOverwrite);
				MessageToCaster("Wait until it expires. Spell Failed.", noOverwrite);
			}
			// show resisted effect if spell failed
			if (!(Spell.IsPulsing && HasPositiveEffect))
				SendEffectAnimation(target, 0, false, 0);
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
			if (oldeffect.Spell.IsConcentration)
				return false;
			
			if ((neweffect.Spell.Damage * neweffect.Effectiveness) < (oldeffect.Spell.Damage * oldeffect.Effectiveness))
				return false;
			
			if ((neweffect.Spell.Value * neweffect.Effectiveness) < (oldeffect.Spell.Value * oldeffect.Effectiveness))
				return false;
			
			//makes problems for immunity effects
			if (oldeffect.ImmunityState == false)
			{
				if ((oldeffect.Spell.Duration == 0 || (neweffect.Spell.Duration != 0 && neweffect.Duration < oldeffect.RemainingTime)) && !Spell.IsFocus)
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
			if (Spell.SpellType != compare.Spell.SpellType)
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
			if (!(Spell.IsPulsing && HasPositiveEffect))
				SendEffectAnimation(effect.Owner, 0, false, 1);
			
			
			if (Spell.IsFocus)
			{
				// Adding targets to the focus effect list
				ConcurrentDictionary<GameLiving, GameSpellEffect> focusEffects = Caster.TempProperties.getProperty<ConcurrentDictionary<GameLiving, GameSpellEffect>>(FOCUS_SPELL);
				if(focusEffects != null)
				{
					GameSpellEffect dummy;
					if(focusEffects.ContainsKey(effect.Owner))
						focusEffects.TryRemove(effect.Owner, out dummy);
					
					focusEffects.TryAdd(effect.Owner, effect);
				}

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
			// Special Cases
			if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || HasPositiveEffect)
			{
				return 0;
			}

			if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects || m_spellItem != null)
			{
				int itemSpellLevel = m_spellItem.Template.LevelRequirement > 0 ? m_spellItem.Template.LevelRequirement : Math.Min(Caster.EffectiveLevel, m_spellItem.Level);
				return 100 - (85 + ((itemSpellLevel - target.EffectiveLevel) >> 1));
			}
			
			// Base resist chance based on hit chances
			int resistChance = 100 - CalculateToHitChance(target);

			// Add target bonus to be missed (Apply MissHit Bonus/Malus)
			resistChance += (int)(target.ChanceToBeMissed * 100);
			
			// Ratio Bonus to player targeted (mob/player vs (player)) defaut to 1.
			if (target is GamePlayer && resistChance < 100)
			{
				resistChance = (int)(resistChance*ServerProperties.Properties.SPELL_RESISTCHANCE_PLAYER_BONUS);
			}
			else if (resistChance < 100)
			{
				resistChance = (int)(resistChance*ServerProperties.Properties.SPELL_RESISTCHANCE_LIVING_BONUS);
			}
			
			// Clamp resistChance
			if (resistChance > 70 && resistChance < 100)
				resistChance = 70;
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler - Calculate Resist Chance : {0}", resistChance);
			return resistChance;
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
			ad.AttackResult = eAttackResult.Missed;
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
				Caster.MessageToSelf(message, type);
			}
			else if (type == eChatType.CT_YouHit || type == eChatType.CT_SpellResisted)
			{
				// Will send to owner or discard
				Caster.MessageToSelf(message, type);
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
		/// Hold events for focus spells, should cancel on any actions
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void FocusSpellAction(DOLEvent e, object sender, EventArgs args)
		{
			//The Caster is trying to cast an other spell !
			if(e == GameLivingEvent.CastStarting)
			{
				// we have casting args
				if(args is CastingEventArgs)
				{
					// if it's the same spell let the spell handler cancel the pulse
					if(((CastingEventArgs)args).SpellHandler != null && Spell == ((CastingEventArgs)args).SpellHandler.Spell)
						return;
				}
			}
			
			if(e == GameLivingEvent.AttackedByEnemy)
			{				   
				MessageToCaster("You are being atttacked and lose your focus!", eChatType.CT_SpellExpires);
			}
			
			//Get target list to cancel their effects...
			RemoveEffectFromFocusTarget();
			
			RemoveFocusEventHandler();
			
			if(e != null)
			{
				CancelPulsingSpell(Caster, Spell.SpellType);
				MessageToCaster(String.Format("You lose your focus on your {0} spell.", Spell.Name), eChatType.CT_SpellExpires);
			}
			
			SendInterruptCastAnimation();
			
			if (e == GameLivingEvent.Moving)
				MessageToCaster("You move and interrupt your focus!", eChatType.CT_Important);
		}
		
		protected void RemoveEffectFromFocusTarget()
		{
			//Get target list to cancel their effects...
			ConcurrentDictionary<GameLiving, GameSpellEffect> focusedEffects = Caster.TempProperties.getProperty<ConcurrentDictionary<GameLiving, GameSpellEffect>>(FOCUS_SPELL);
			
			if(focusedEffects != null)
			{
				foreach(GameSpellEffect effect in focusedEffects.Values)
				{
					GameSpellEffect currentEffect = effect;
					
					if (currentEffect != null)
						currentEffect.Cancel(false);
				}
				
				focusedEffects.Clear();
			}
		}
		
		/// <summary>
		/// Add Events Handler for Focusing Spells
		/// </summary>
		/// <param name="target"></param>
		public virtual void AddFocusEventHandler(GameLiving target)
		{
			DOLEventHandler focusEventHandler = new DOLEventHandler(FocusSpellAction);
			
			// set event data
			Caster.TempProperties.setProperty(FOCUS_SPELL_EVENTS, new Tuple<DOLEventHandler, GameLiving>(focusEventHandler, target));
			Caster.TempProperties.setProperty(FOCUS_SPELL, new ConcurrentDictionary<GameLiving, GameSpellEffect>());
			
			if(!Spell.Uninterruptible)
			{
				GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, focusEventHandler);
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, focusEventHandler);
				GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, focusEventHandler);
			}

			GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, focusEventHandler);
			GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, focusEventHandler);
			GameEventMgr.AddHandler(Caster, GameLivingEvent.CrowdControlled, focusEventHandler);
			GameEventMgr.AddHandler(Caster, GameLivingEvent.Delete, focusEventHandler);
			GameEventMgr.AddHandler(Caster, GameLivingEvent.RemoveFromWorld, focusEventHandler);
			
			if(Caster is GamePlayer)
			{
				GameEventMgr.AddHandler(Caster, GamePlayerEvent.Linkdeath, focusEventHandler);
				GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, focusEventHandler);
				GameEventMgr.AddHandler(Caster, GamePlayerEvent.RegionChanging, focusEventHandler);
			}

			
			// we don't want to listen to all world event
			if(target != null && target != Caster)
			{
				GameEventMgr.AddHandler(target, GameLivingEvent.Dying, focusEventHandler);
				GameEventMgr.AddHandler(target, GameLivingEvent.Delete, focusEventHandler);
				GameEventMgr.AddHandler(target, GameLivingEvent.RemoveFromWorld, focusEventHandler);
				
				if(target is GamePlayer)
				{
					GameEventMgr.AddHandler(target, GamePlayerEvent.Linkdeath, focusEventHandler);
					GameEventMgr.AddHandler(target, GamePlayerEvent.Quit, focusEventHandler);
					GameEventMgr.AddHandler(target, GamePlayerEvent.RegionChanging, focusEventHandler);
				}
			}
		}
		
		/// <summary>
		/// Removes the Event Handler for Focusing Spells
		/// </summary>
		public virtual void RemoveFocusEventHandler()
		{
			Caster.TempProperties.removeProperty(FOCUS_SPELL);
			
			// retrieve event data
			Tuple<DOLEventHandler, GameLiving> focusEvents = Caster.TempProperties.getProperty<Tuple<DOLEventHandler, GameLiving>>(FOCUS_SPELL_EVENTS);
			
			if(focusEvents != null)
			{
				GameLiving target = focusEvents.Item2;
				DOLEventHandler eventHandler = focusEvents.Item1;
				
				if(!Spell.Uninterruptible)
				{
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, eventHandler);
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, eventHandler);
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, eventHandler);
				}
				
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, eventHandler);
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, eventHandler);
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CrowdControlled, eventHandler);
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Delete, eventHandler);
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.RemoveFromWorld, eventHandler);
				
				if(Caster is GamePlayer)
				{
					GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.Linkdeath, eventHandler);
					GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.Quit, eventHandler);
					GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.RegionChanging, eventHandler);
				}

				if(target != null)
				{
					GameEventMgr.RemoveHandler(target, GameLivingEvent.Dying, eventHandler);
					GameEventMgr.RemoveHandler(target, GameLivingEvent.Delete, eventHandler);
					GameEventMgr.RemoveHandler(target, GameLivingEvent.RemoveFromWorld, eventHandler);
					
					if(target is GamePlayer)
					{
						GameEventMgr.RemoveHandler(target, GamePlayerEvent.Linkdeath, eventHandler);
						GameEventMgr.RemoveHandler(target, GamePlayerEvent.Quit, eventHandler);
						GameEventMgr.RemoveHandler(target, GamePlayerEvent.RegionChanging, eventHandler);
					}
				}

			}
			
			Caster.TempProperties.removeProperty(FOCUS_SPELL_EVENTS);
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
                GamePlayer p = null;

                if (Caster is GamePlayer || Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain &&
    			((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null)
                {
                    p = Caster is GamePlayer ? (Caster as GamePlayer) : ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                }
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
				if (Spell.Damage != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
				if (Spell.LifeDrainReturn != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.HealthReturned", Spell.LifeDrainReturn) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.HealthReturned", Spell.LifeDrainReturn));
				else if (Spell.Value != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")));
                list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Target", Spell.Target) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
				if (Spell.Range != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Range", Spell.Range) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
				if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration") + " Permanent." : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " Permanent.");
				else if (Spell.Duration > 60000)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration") + " " + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min" : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " " + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min");
				else if (Spell.Duration != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'") : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
				if (Spell.Power != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				if (Spell.RecastDelay > 60000)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min" : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 1000).ToString() + " sec" : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.ConcentrationCost", Spell.Concentration) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.ConcentrationCost", Spell.Concentration));
				if (Spell.Radius != 0)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Radius", Spell.Radius) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
				if (Spell.DamageType != eDamageType.Natural)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
				if (Spell.IsFocus)
                    list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Focus") : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Focus"));

				return list;
			}
		}

		#region various helpers

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public virtual void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			// Basically Damage Variance is 25% - 125%
			min = 0.25;
			max = 1.25;
			
			// Item Effect
			if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects || m_spellItem != null)
			{
				min = 1.0;
				return;
			}

			// Combat Style Effect
			if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
			{
				if (UseMinVariance)
				{
					min = max;
				}
				else
				{
					min = 1.00;
				}

				return;
			}

			// Reserved Spells.
			if (SpellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
			{
				min = max;
				return;
			}

			// For anything else, it needs spec Levels of Caster/Pet. (or specifics for mobs as they have no spec !)
			// Spec level at 0 value means the caster don't have this line !
			int speclevel = 0;
			GameLiving realCaster = Caster;

			// If this is a Controlled NPC
			if (Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
			{
				GameLiving owner = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
				
				if (owner != null)
					realCaster = owner;
			}
			
			if (realCaster is GamePlayer)
			{
				// For Gameplayer just grab spec
				speclevel = realCaster.GetModifiedSpecLevel(SpellLine.Spec);
			}
			
			// If we couldn't grab a spec, spec is 2/3 of level
			if (speclevel == 0 && Caster is GameNPC)
			{
				speclevel = ((int)(2.0/3.0*realCaster.Level))+1;
			}

			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcDmgVar - Spec Before overspec {0} (min {1}, max {2})", speclevel, min, max);
			
			// Compute overspec. overspec before Level gives 0.005, overspec after Level gives 0.004
			double overspecBonus = Math.Max(0.0, (Math.Min(realCaster.Level, speclevel)-Spell.Level) * 0.005) + Math.Max(0.0, (speclevel-realCaster.Level) * 0.004);
			
			// Add overspec.
			max += overspecBonus;
			min += overspecBonus;
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcDmgVar - Variance After overspec {0} (min {1}, spell Level {2})", max, min, Spell.Level);
			
			if (!SpellLine.IsBaseLine)
			{
				// Specline damage have no variance
				min = max;
			}
			else
			{
				// base variance, full 0.5(+0.25 base = 75%) bonus at 2/3 speclevel (1.5*0.5=0.75)
				double baseVariance = Math.Min(0.5, Math.Max(0, speclevel - 1) / Math.Max(1.0, Spell.Level) * 0.75);
				// bonus variance spec level over target level. (bonus when spec > 2/3*target level)
				double levelVariance = Math.Max(0.0, Math.Max(0, speclevel - 1) / Math.Max(1.0, target.EffectiveLevel) * 1.25 - 0.75);

				// add up and cap to min/max
				min = Math.Min(max, Math.Max(min, min + baseVariance + levelVariance));
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
		public virtual double CapPetSpellDamage(double damage, GameLiving owner)
		{
			double cappedDamage = damage;

			if (owner.Level < 13)
			{
				cappedDamage = 4.1 * owner.Level;
			}

			if (owner.Level < 50)
			{
				cappedDamage = 3.8 * owner.Level;
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
		public virtual double CapNPCSpellDamage(double damage, GameLiving npc)
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
			GameLiving realCaster = Caster;

			// For pets the stats of the owner have to be taken into account.
			if (Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
			{
				realCaster = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
			}

			// compute involving player
			if (realCaster is GamePlayer)
			{
				// GamePet Cap Damage. // TODO review this
				if (Caster is GamePet)
				{
					spellDamage = CapPetSpellDamage(spellDamage, realCaster);
				}

				if (((GamePlayer)realCaster).CharacterClass.ManaStat != eStat.UNDEFINED
				    && ((GamePlayer)realCaster).CharacterClass.ManaStat != eStat.STR
				    && ((GamePlayer)realCaster).CharacterClass.ManaStat != eStat.QUI
				    && ((GamePlayer)realCaster).CharacterClass.ManaStat != eStat.CON				    
				    && SpellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect
				    && SpellLine.KeyName != GlobalSpellsLines.Mundane_Poisons
				    && SpellLine.KeyName != GlobalSpellsLines.Item_Effects)
				{
					int manaStatValue = realCaster.GetModified((eProperty)((GamePlayer)realCaster).CharacterClass.ManaStat);
					spellDamage *= (manaStatValue + 200.0) / 275.0;
				}
				else if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
				{
					// Combat Styles Effect Damage Calc
					double WeaponSkill = realCaster.GetWeaponSkill(realCaster.AttackWeapon)/5;
					spellDamage *= (WeaponSkill + 200.0) / 275.0;
				}

			}
			else if (realCaster is GameNPC)
			{
				int manaStatValue = realCaster.GetModified(eProperty.Intelligence);
				spellDamage = CapNPCSpellDamage(spellDamage, realCaster) * (manaStatValue + 200.0) / 275.0;
			}
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcDmgBase - Spell Damage Base : {0}", spellDamage);

			return Math.Max(1, spellDamage);
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

			GameLiving realCaster = Caster;
			
			if (Caster is GameNPC && (Caster as GameNPC).Brain is ControlledNpcBrain)
			{
				realCaster = ((ControlledNpcBrain)((GameNPC)Caster).Brain).GetLivingOwner();
			}

			int spellbonus = realCaster.GetModified(eProperty.SpellLevel);
			spellLevel += spellbonus;

			// Clamp spell bonus level to living Level.
			if (spellLevel > realCaster.EffectiveLevel)
			{
				spellLevel = ((GamePlayer)realCaster).EffectiveLevel;
			}

			// Clamp Champion Spell and Style Effect to Target Level and Caster Level, prevent to-hit bonuses
			if ((SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || SpellLine.KeyName == GlobalSpellsLines.Champion_Spells))
			{
				spellLevel = Math.Min(realCaster.EffectiveLevel, target.EffectiveLevel);
			}

			// Apply ToHitBonus
			int bonustohit = realCaster.GetModified(eProperty.ToHitBonus);

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

			int hitchance = 85 + ((spellLevel - target.EffectiveLevel) >> 1) + bonustohit;

			// PvE Only
			if (target is GameNPC && !(((GameNPC)target).Brain is IControlledBrain && ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner() != null))
			{
				hitchance -= (int)(Caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
				hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
			}
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcToHitChc - Spell Hit Chance : {0} (bonusToHit : {1}) (AttackerCount : {2})", hitchance, bonustohit, target.Attackers.Count);

			return hitchance;
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
				// With 2.2 ratio 1 damage @ 10 hit chance.
				adjustedDamage += (int)(adjustedDamage * (hitChance - 55) * ServerProperties.Properties.SPELL_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER * 0.01);
			}
			else if (hitChance > 100)
			{
				// 0.50% damage bonus for point over hitChance
				adjustedDamage += (int)(adjustedDamage * Math.Min(100, hitChance - 100) * ServerProperties.Properties.SPELL_HITCHANCE_DAMAGE_RAISE_MULTIPLIER * 0.01);
			}

			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler AdjDmgHitChc - Spell Adjusted Damage for Hit Chance : {0}", adjustedDamage);

			return Math.Max(1, adjustedDamage);
		}

		
		/// <summary>
		/// Calculate base damage to target before resists and defense modifiers
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public virtual int CalculateBaseDamageToTarget(GameLiving target, double effectiveness)
		{
			// Get Variance
			double minVariance;
			double maxVariance;

			CalculateDamageVariance(target, out minVariance, out maxVariance);
			
			// Get Damage Base
			int spellDamage = (int)CalculateDamageBase(target);
			
			// Adjust Damage base to hit chance
			int totalDamage = AdjustDamageForHitChance(spellDamage, CalculateToHitChance(target));
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcBaseDmgToTgt - Spell Base Damage with hit chance : {0}", totalDamage);
			
			int finalDamage;
			// Run Variance random
			if (minVariance != maxVariance)
			{
				finalDamage = Util.Random((int)(minVariance * totalDamage), (int)(maxVariance * totalDamage));
			}
			else
			{
				finalDamage = (int)(maxVariance * totalDamage);
			}
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcBaseDmgToTgt - Spell Base Damage with variance : {0} (min : {1}, max : {2})", totalDamage, minVariance, maxVariance);
			
			// Caster Bonus Damage modifier
			double damageModifier = Caster.GetModified(eProperty.SpellDamage) * 0.001;
			
			// Add Relic bonus modifier
			damageModifier *= 1.0 + RelicMgr.GetRelicBonusModifier(Caster.Realm, eRelicType.Magic);
			
			// Compute damage.
			finalDamage = (int)(finalDamage * damageModifier * effectiveness);
			
			// Server Properties Modifier
			if (Caster.Realm != eRealm.None && target.Realm != eRealm.None)
			{
				finalDamage = (int)(finalDamage * ServerProperties.Properties.PVP_SPELL_DAMAGE);
			}
			else
			{
				finalDamage = (int)(finalDamage * ServerProperties.Properties.PVE_SPELL_DAMAGE);
			}
			
			// Apply Magic Absorption (mostly for mobs)
			double magicAbsorption = 1.0 - target.GetModified(eProperty.MagicAbsorption) * 0.01;
			finalDamage = (int)(finalDamage * magicAbsorption);
			
			if (log.IsDebugEnabled)
				log.DebugFormat("SpellHandler CalcBaseDmgToTgt - Spell Base Damage after bonus and absorb : {0} (abs : {1})", finalDamage, magicAbsorption);
			
			// Base damage can't overcap
			return Math.Max(1, Math.Min((int)DamageCap(target, effectiveness), finalDamage));
		}

		/// <summary>
		/// Calculate Resist Modifier of target based on given damage.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <param name="baseDamage"></param>
		/// <returns></returns>
		public virtual int CalculateResistDamageToTarget(GameLiving target, double effectiveness, int baseDamage)
		{
			eDamageType damageType = DetermineSpellDamageType();

			eProperty property = target.GetResistTypeForDamage(damageType);
			
			// The Daoc resistsystem is since 1.65 a 2category system.
			// - First category are Item/Race/Buff/RvrBanners resists that are displayed in the characteroverview.
			// - Second category are resists that are given through RAs like avoidance of magic, brilliance aura of deflection.
			//   Those resist affect ONLY the spelldamage. Not the duration, not the effectiveness of debuffs.
			// so calculation is (finaldamage * Category1Modification) * Category2Modification
			// -> Remark : VampirResistBuff is Category2 too.

			int primaryResistModifier = target.GetResistBase(property);
			int secondaryResistModifier = target.GetResist(property)-primaryResistModifier;

			/* Resist Pierce is a special bonus which has been introduced with TrialsOfAtlantis.
			 * At the calculation of SpellDamage, it reduces the resistance that the victim receives
			 * through ITEMBONUSES for the specified percentage. */

			//substract max ItemBonus of property of target, but atleast 0.
			primaryResistModifier -= Math.Max(0, Math.Min(target.ItemBonus[property], Math.Max(0, Caster.GetModified(eProperty.ResistPierce))));

			double resistModifier = 0;
			//primary resists
			resistModifier += baseDamage * primaryResistModifier * -0.01;
			
			// Display at least -1+1 from any resists
			if (resistModifier > 0 && resistModifier < 1)
			{
				resistModifier = 1;
			}
			else if (resistModifier < 0 && resistModifier > -1)
			{
				resistModifier = -1;
			}
			
			//secondary resists
			resistModifier += Math.Max(0, baseDamage + (int)resistModifier) * secondaryResistModifier * -0.01;
			
			// Display at least -1+1 from any resists
			if (resistModifier > 0 && resistModifier < 1)
			{
				resistModifier = 1;
			}
			else if (resistModifier < 0 && resistModifier > -1)
			{
				resistModifier = -1;
			}
			
			// Return the modifier.
			return (int)resistModifier;
		}
		
		/// <summary>
		/// Calculate spell critical (with chances) based on given damage.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <param name="finalDamage"></param>
		/// <returns></returns>
		public virtual int CalculateCriticalDamageToTarget(GameLiving target, double effectiveness, int finalDamage)
		{
			if (Util.Chance(Math.Min(50, Caster.SpellCriticalChance)) && (finalDamage >= 1))
			{
				//think min crit is 10% of damage
				return Util.Random(finalDamage / 10, target is GamePlayer ? finalDamage >> 1 : finalDamage);
			}
			
			return 0;
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
		/// Calculates damage to target with resist chance and stores it in ad
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="effectiveness">value from 0..1 to modify damage</param>
		/// <returns>attack data</returns>
		public virtual AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = new AttackData();
			ad.Attacker = Caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.SpellHandler = this;
			ad.AttackResult = eAttackResult.HitUnstyled;
			
			// Get Damage Cap
			int damageCap = (int)DamageCap(target, effectiveness);
			
			// Get base damage (should be capped)
			int finalDamage = CalculateBaseDamageToTarget(target, effectiveness);

			// Apply Resists
			int resistModifier = CalculateResistDamageToTarget(target, effectiveness, finalDamage);

			// Cap resist modifier
			if ((finalDamage + resistModifier) > damageCap)
			{
				resistModifier -= finalDamage + resistModifier - damageCap; 
			}
			
			finalDamage += resistModifier;
			
			// Clamp
			finalDamage = Math.Max(1, finalDamage);
			
			// Get Critical value
			int cdamage = CalculateCriticalDamageToTarget(target, effectiveness, finalDamage);

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
			ad.DamageType = DetermineSpellDamageType();
			ad.Modifier = resistModifier;

			// TODO this must be done by the function caller
			AppendAttackData(ad);
			return ad;
		}

		/// <summary>
		/// Spell Damage Cap based on Delve * 3 * Toa * MoM
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public virtual double DamageCap(GameLiving target, double effectiveness)
		{
			// should be (int)((int)((int)((int)(damage*3))*toa)*Mom), using spell damage as base +-1000 for this
			return Spell.Damage * 3.0 * (Caster.GetModified(eProperty.SpellDamage) * 0.001) * effectiveness;
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
			// resists Bonus damages
			if (ad.Modifier > 0)
				modmessage = " (+" + ad.Modifier + ")";
			// resists Malus damages
			if (ad.Modifier < 0)
				modmessage = " (" + ad.Modifier + ")";
			
			// spell hitting message
			if (Caster is GamePlayer || Caster is NecromancerPet)
			{
				MessageToCaster(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);
				// spell critical message
				if (ad.CriticalDamage > 0)
					MessageToCaster("You critically hit for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit);
			}
			else if (Caster is GameNPC)
			{
				MessageToCaster(string.Format("Your " + Caster.Name + " hits {0} for {1}{2} damage!",
				                              ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);
				if (ad.CriticalDamage > 0)
					MessageToCaster("Your " + Caster.Name + " critically hits for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit);
			}

		}

		/// <summary>
		/// Make damage to target and send spell effect but no messages
		/// </summary>
		/// <param name="ad">Attack Data for the given Damages</param>
		/// <param name="showEffectAnimation">Display Effect Animation ?</param>
		public void DamageTarget(AttackData ad, bool showEffectAnimation)
		{
			DamageTarget(ad, showEffectAnimation, 0x14); //spell damage attack result
		}

		/// <summary>
		/// Make damage to target and send spell effect but no messages
		/// </summary>
		/// <param name="ad">Attack Data for the given Damages</param>
		/// <param name="showEffectAnimation">Display Effect Animation ?</param>
		/// <param name="attackResult">Attack result to display.</param>
		public virtual void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			if (ad == null || ad.Target == null)
				return;
			
			// Attacked living may modify the attack data.  Primarily used for keep doors and components.
			ad.Target.ModifyAttack(ad);
			
			// Attacked living may take action based on notification, and modify the Attack Data Object !
			ad.Target.OnAttackedByEnemy(ad);
			
			// Check if the Attack is a success to take action (could be modified by OnAttackedByEnemy)
			if (ad.IsSpellResisted)
			{
				// If we get a notification here that spell is resisted we should trigger a spell resist and exit
				// Other spell resist/miss rules should be handled in casting sequence, this only applies to special effects modifying the AD 
				OnSpellResisted(ad.Target);
				return;
			}
			
			// If we're gonna Damage the Target with a Spell, the attack result is HitUnstyled !
			ad.AttackResult = eAttackResult.HitUnstyled;
			
			// save Attack data.
			AppendAttackData(ad);

			// Send Damage Message after event notification
			SendDamageMessages(ad);
			
			// send animation before dealing damage else dead livings show no animation
			if (showEffectAnimation)
			{
				SendEffectAnimation(ad.Target, 0, false, 1);
			}
			
			// Effectively DealDamage
			ad.Attacker.DealDamage(ad);
			
			if (ad.Damage > 0)
			{
				// Send animation after DealDamage to have the correct Health Percent.
				
				foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer playCombat = player;
					if(playCombat != null)
						playCombat.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);
				}
			}
		}
		
		/// <summary>
		/// Append Last Attack Data for future Use.
		/// </summary>
		/// <param name="ad"></param>
		public virtual void AppendAttackData(AttackData ad)
		{
			if (ad.Target == null)
				return;
			
			if (m_lastAttackData.ContainsKey(ad.Target))
			{
				AttackData dummy;
				m_lastAttackData.TryRemove(ad.Target, out dummy);
			}
			
			m_lastAttackData.TryAdd(ad.Target, ad);
		}
		
		/// <summary>
		/// Retrieve Last Attack Data for a Target.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual AttackData GetLastAttackData(GameLiving target)
		{
			if (target != null && m_lastAttackData.ContainsKey(target))
			{
				AttackData result;
				if (m_lastAttackData.TryGetValue(target, out result))
					return result;
			}
			
			return null;
		}

		/// <summary>
		/// Resist Ratio for all Duration based spell that don't take Ability or other bonuses into account.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public virtual double GetResistBaseRatio(GameLiving target, double effectiveness)
		{
			eProperty property = target.GetResistTypeForDamage(DetermineSpellDamageType());

			int primaryResistModifier = target.GetResistBase(property);

			// Piercing substract max ItemBonus of property of target, but atleast 0.
			primaryResistModifier -= Math.Max(0, Math.Min(target.ItemBonus[property], Math.Max(0, Caster.GetModified(eProperty.ResistPierce))));
			
			return 1.0-(primaryResistModifier*0.01);
		}
		
		#endregion

		#region saved effects
		public virtual PlayerXEffect GetSavedEffect(GameSpellEffect effect)
		{
			return null;
		}

		public virtual void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
		}

		public virtual int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return 0;
		}
		#endregion

	}
}
