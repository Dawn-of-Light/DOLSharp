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

using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Default class for spell handler
	/// should be used as a base class for spell handler
	/// </summary>
	public class SpellHandler : ISpellHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
		/// Has the spell been interrupted
		/// </summary>
		protected bool m_interrupted = false;
		/// <summary>
		/// Shall we start the reuse timer
		/// </summary>
		protected bool m_startReuseTimer = true;

		/// <summary>
		/// Ability that casts a spell
		/// </summary>
		protected SpellCastingAbilityHandler m_ability = null;

		/// <summary>
		/// Stores the current delve info depth
		/// </summary>
		private byte m_delveInfoDepth;

		/// <summary>
		/// The property key for the interrupt timeout
		/// </summary>
		public const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";
		/// <summary>
		/// The duration for the spell interrupt duration
		/// </summary>
		protected const int SPELL_INTERRUPT_DURATION = 3000; //3 sec for all spells!

		/// <summary>
		/// The property key for focus spells
		/// </summary>
		protected const string FOCUS_SPELL = "FOCUSING_A_SPELL";

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

			if (Caster.Mana > Spell.PulsePower)
			{
				Caster.Mana -= Spell.PulsePower;
				if (Spell.InstrumentRequirement != 0 || !HasPositiveEffect)
					SendEffectAnimation(Caster, 0, true, 1); // pulsing auras or songs
				StartSpell(Caster.TargetObject as GameLiving);
			}
			else
			{
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
			if (instrument == null || instrument.Object_Type != (int)eObjectType.Instrument || (instrument.DPS_AF != 4 && instrument.DPS_AF != m_spell.InstrumentRequirement))
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
			lock (living.ConcentrationEffects)
			{
				for (int i = 0; i < living.ConcentrationEffects.Count; i++)
				{
					PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
					if (effect == null)
						continue;
					effect.Cancel(false);
				}
			}
		}

		#endregion

		/// <summary>
		/// called whenever the player clicks on a spell icon
		/// or a GameLiving wants to cast a spell
		/// </summary>
		public virtual bool CastSpell()
		{
			m_caster.Notify(GameLivingEvent.CastSpell, m_caster, new CastSpellEventArgs(this));
			// nightshade is unstealthed even if no target, target is same realm, target is too far
			if (Caster is GamePlayer && Spell.SpellType != "Archery")
				((GamePlayer)Caster).Stealth(false);

			// cancel engage effect if exist
			if (m_caster.IsEngaging)
			{
				EngageEffect effect = (EngageEffect)Caster.EffectList.GetOfType(typeof(EngageEffect));
				if (effect != null)
					effect.Cancel(false);
			}

			m_interrupted = false;
			GameLiving target = Caster.TargetObject as GameLiving;

			if (Spell.Target.ToLower() == "pet")
			{
				// Pet is the target, check if the caster is the pet.

				if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
					target = Caster;
			}
			else if (Spell.Target.ToLower() == "controlled")
			{
				// Can only be issued by the owner of a pet and the target
				// is always the pet then.

				if (Caster is GamePlayer && Caster.ControlledNpc != null)
					target = Caster.ControlledNpc.Body;
				else
					target = null;
			}

			if (Spell.Pulse != 0 && CancelPulsingSpell(Caster, Spell.SpellType))
			{
				// is done even if caster is sitting
				if (Spell.InstrumentRequirement == 0)
					MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
				else
					MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
			}
			else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, target, Spell, m_spellLine) 
				&& CheckBeginCast(target))
			{
				if (m_caster is GamePlayer && (m_caster as GamePlayer).IsOnHorse && !HasPositiveEffect)
				{
					(m_caster as GamePlayer).IsOnHorse = false;
				}

				if (Spell.CastTime > 0)
				{
					// no instant cast
					m_interrupted = false;
					SendSpellMessages();

					//set the time when casting to can not quickcast during a minimum time
					//if (m_caster is GamePlayer && ((GamePlayer)m_caster).IsQuickCasting)
					//	((GamePlayer)m_caster).TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, GameTimer.CurrentTick);

					m_castTimer = new DelayedCastTimer(Caster, this, target);
					m_castTimer.Start(1 + CalculateCastingTime());
					SendCastAnimation();

					if (m_caster is GamePlayer && ((GamePlayer)m_caster).IsStrafing)
					{
						CasterMoves();
					}

					if (m_caster.IsMoving)
					{
						CasterMoves();
					}
				}
				else
				{
                   // instant cast
                    bool sendcast = true;
		        	if (m_caster.ControlledNpc!=null && m_caster.ControlledNpc.Body!=null && m_caster.ControlledNpc.Body is NecromancerPet)
		        		sendcast = false;
                    
                    if(sendcast) SendCastAnimation(0);
					FinishSpellCast(target);
				}
			}
			if (!IsCasting)
			{
				OnAfterSpellCastSequence();
			}
			return true;
		}

		/// <summary>
		/// Is called when the caster moves
		/// </summary>
		public virtual void CasterMoves()
		{
			if (Spell.InstrumentRequirement != 0)
				return; // song can be played while moving
			if (Spell.MoveCast)
				return;
			MessageToCaster("You move and interrupt your spellcast!", eChatType.CT_System);
			InterruptCasting();
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
			if (Spell.Uninterruptible)
				return false;
			if (Caster.EffectList.GetOfType(typeof(QuickCastEffect)) != null)
				return false;
			if (Caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) != null)
				return false;
			if (Caster.EffectList.GetOfType(typeof(FacilitatePainworkingEffect)) != null)
				return false;
			if (IsCasting)
			{
				double mod = Caster.GetConLevel(attacker);
				double chance = 65;
				chance += mod * 10;
				chance = Math.Max(1, chance);
				chance = Math.Min(99, chance);
				if(attacker is GamePlayer) chance=99;
				if (Util.Chance((int)chance))
				{
					Caster.TempProperties.setProperty(INTERRUPT_TIMEOUT_PROPERTY, Caster.CurrentRegion.Time + SPELL_INTERRUPT_DURATION);
					MessageToLiving(Caster, attacker.GetName(0, true) + " attacks you and your spell is interrupted!", eChatType.CT_SpellResisted);
					InterruptCasting(); // always interrupt at the moment
					return true;
				}
			}
			return false;
		}

		#region begin & end cast check

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public virtual bool CheckBeginCast(GameLiving selectedTarget)
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

			GameSpellEffect Phaseshift = SpellHandler.FindEffectOnTarget(Caster, "Phaseshift");
			if (Phaseshift != null && (Spell.InstrumentRequirement == 0 || Spell.SpellType == "Mesmerize"))
			{
				MessageToCaster("You're phaseshifted and can't cast a spell", eChatType.CT_System);
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
						if (m_caster is GamePlayer)
							((GamePlayer)m_caster).Out.SendMessage(string.Format("{0} is invisible to you!", selectedTarget.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					
						return false;
					}
				}
			}

            if (selectedTarget!=null && selectedTarget.HasAbility("DamageImmunity") && Spell.SpellType == "DirectDamage" && Spell.Radius == 0)
            {
                MessageToCaster(selectedTarget.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return false;
            }
			
			if (m_spell.InstrumentRequirement != 0)
			{
				if (!CheckInstrument())
				{
					MessageToCaster("You are not wielding the right type of instrument!", 
						eChatType.CT_SpellResisted);
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

			if (m_caster.AttackState && m_spell.CastTime != 0)
			{
				if (m_caster is GamePlayer)
				{
					GamePlayer player = m_caster as GamePlayer;
					if (!(player.CharacterClass is PlayerClass.ClassVampiir) 
					    && !(player.CharacterClass is PlayerClass.ClassMaulerAlb)
					    && !(player.CharacterClass is PlayerClass.ClassMaulerMid)
					    && !(player.CharacterClass is PlayerClass.ClassMaulerHib))
					{
						m_caster.StopAttack();
						return false;
					}
				}
			}

			if (!m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer && m_caster.EffectList.GetOfType(typeof(QuickCastEffect)) == null)
			{
				long leftseconds = Math.Max(
				    Caster.TempProperties.getLongProperty(INTERRUPT_TIMEOUT_PROPERTY, 0) - Caster.CurrentRegion.Time,
				    Caster.SwingTimeLeft);
				if (leftseconds > 0)
				{
					MessageToCaster("You must wait " + (leftseconds / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_System);
					return false;
				}
				Caster.TempProperties.removeProperty(INTERRUPT_TIMEOUT_PROPERTY);
			}

			if (m_spell.RecastDelay > 0)
			{
				int left = m_caster.GetSkillDisabledDuration(m_spell);
				if (left > 0)
				{
					MessageToCaster("You must wait " + (left / 1000 + 1).ToString() + " seconds to use this spell!", eChatType.CT_System);
					return false;
				}
			}

			String targetType = m_spell.Target.ToLower();
			if (targetType == "area")
			{
				if (!WorldMgr.CheckDistance(m_caster, m_caster.GroundTarget, CalculateSpellRange()))
				{
					MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
				//				if (!Caster.GroundTargetInView)
				//				{
				//					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
				//					return false;
				//				}
			}
			else if (targetType != "self" && targetType != "group" && targetType != "pet"
				&& targetType != "controlled" && targetType != "cone" && m_spell.Range > 0)
			{
				// All spells that need a target.

				if (selectedTarget == null || selectedTarget.ObjectState != GameLiving.eObjectState.Active)
				{
					MessageToCaster("You must select a target for this spell!",
						eChatType.CT_SpellResisted);
					return false;
				}

				if (!WorldMgr.CheckDistance(m_caster, selectedTarget, CalculateSpellRange()))
				{
					if(Caster is GamePlayer) MessageToCaster("That target is too far away!",
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
							MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
							return false;
						}

						if (FindStaticEffectOnTarget(selectedTarget, typeof(NecromancerShadeEffect)) != null)
						{
							MessageToCaster("Invalid target.", eChatType.CT_System);
							return false;
						}

						//enemys have to be in front and in view for targeted spells
						if (!(m_caster.IsObjectInFront(selectedTarget, 180) && m_caster.TargetInView))
						{
							if(m_spell.SpellType=="Charm" && m_spell.CastTime==0 && m_spell.Pulse!=0) break;
							MessageToCaster("Your target is not in view!", eChatType.CT_System);
							Caster.Notify(GameLivingEvent.CastFailed,
								new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
							return false;
						}

						if (!GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, false))
						{
							return false;
						}
						break;

					case "corpse":
						if (selectedTarget.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, true))
						{
							MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "realm":
						if (!GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, false))
						{
							return false;
						}
						break;
				}

				//heals/buffs/rez need LOS only to start casting
				if (!m_caster.TargetInView && m_spell.Target.ToLower() != "pet")
				{
					MessageToCaster("Your target is not in view!", eChatType.CT_System);
					Caster.Notify(GameLivingEvent.CastFailed,
								new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
					return false;
				}

				if (m_spell.Target.ToLower() != "corpse" && !selectedTarget.IsAlive)
				{
					MessageToCaster(selectedTarget.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
					return false;
				}
			}

			//Ryan: don't want mobs to have reductions in mana
			if (Spell.Power>0 && m_caster is GamePlayer && (m_caster as GamePlayer).CharacterClass.ID != (int)eCharacterClass.Savage && m_caster.Mana < CalculateNeededPower(selectedTarget) && Spell.SpellType != "Archery")
			{
				MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Concentration > 0)
			{
				if (m_caster.Concentration < m_spell.Concentration)
				{
					MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
					return false;
				}

				if (m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
				{
					MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			// We have a mauler - Take power when a spell is used
			//if (Caster is GamePlayer)
			//{
			//    if ((Caster as GamePlayer).CharacterClass.ID > 59 && (Caster as GamePlayer).CharacterClass.ID < 63)
			//    {
			//        (Caster as GamePlayer).ChangeMana((Caster as GamePlayer), GameLiving.eManaChangeType.Spell, -m_spell.Power);
			//    }
			//}

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
				Caster.Notify(GameLivingEvent.CastSucceeded, this,
					new PetSpellEventArgs(Spell, SpellLine, selectedTarget));
			return true;
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
		/// Check the Line of Sight from you to your target
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">The result</param>
		/// <param name="targetOID">The target OID</param>
		public virtual void CheckLOSYouToTarget(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100) // In view ?
				return;
			MessageToLiving(player, "You can't see your target!", eChatType.CT_SpellResisted);
			InterruptCasting(); // break;
		}

		/// <summary>
		/// Check the Line of Sight from your pet to your target
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">The result</param>
		/// <param name="targetOID">The target OID</param>
		public virtual void CheckLOSPetToTarget(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100) // In view ?

				return;
			MessageToLiving(player, "Your pet can't see the target!", eChatType.CT_SpellResisted);
			InterruptCasting(); // break;
		}

		/// <summary>
		/// Checks after casting before spell is executed
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CheckEndCast(GameLiving target)
		{
			if (m_interrupted)
			{
				return false;
			}

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
				if (!WorldMgr.CheckDistance(m_caster, m_caster.GroundTarget, CalculateSpellRange()))
				{
					MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
				//				if (!Caster.GroundTargetInView)
				//				{
				//					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
				//					return false;
				//				}
			}
			else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
			{
				//all spells that need a target

				if (m_spell.Target.ToLower() == "pet" && m_caster is GamePlayer &&
				    ((GamePlayer)m_caster).ControlledNpc != null)
				{
					if (((GamePlayer)m_caster).ControlledNpc.Body != null)
						target = ((GamePlayer)Caster).ControlledNpc.Body;
				}

				if (target == null || target.ObjectState != GameLiving.eObjectState.Active)
				{
					MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
					return false;
				}

				if (!WorldMgr.CheckDistance(m_caster, target, CalculateSpellRange()))
				{
					if(Caster is GamePlayer) MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return false;
				}

				switch (m_spell.Target)
				{
					case "Enemy":
						//enemys have to be in front and in view for targeted spells
						if (!(m_caster.IsObjectInFront(target, 180) && m_caster.TargetInView))
						{
							MessageToCaster("Your target is not in view.  The spell fails.", eChatType.CT_SpellResisted);
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
							MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "Realm":
						if (!GameServer.ServerRules.IsSameRealm(Caster, target, false))
						{
							return false;
						}
						break;

					case "Pet":
						if (Caster is GamePlayer)
						{
							GamePlayer casterPlayer = (GamePlayer)Caster;
							if (casterPlayer.ControlledNpc == null)
							{
								// TODO: correct message?
								MessageToCaster("Not controlling anything.", eChatType.CT_SpellResisted);
								return false;
							}
						}
						break;
				}
			}

			if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
			{
				MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			if (Spell.Power > 0 && m_caster.Mana < CalculateNeededPower(target) && Spell.SpellType != "Archery")
			{
				MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster.Concentration < m_spell.Concentration)
			{
				MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
			{
				MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
				return false;
			}

			return true;
		}

		#endregion

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual int CalculateNeededPower(GameLiving target)
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
			int range = Math.Max(32, (int)(Spell.Range * Caster.GetModified((Spell.SpellType != "Archery" ? eProperty.SpellRange : eProperty.ArcheryRange)) * 0.01));
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

			if (IsCasting)
			{
				foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendInterruptAnimation(m_caster);
				}
			}

			m_interrupted = true;
			if (m_castTimer != null)
			{
				m_castTimer.Stop();
				m_castTimer = null;
				if (m_caster is GamePlayer)
					((GamePlayer)m_caster).ClearSpellQueue();
				//This was called in OnAfterSpellCastSequence anyways for GameNPCs
				//else if (m_caster is GameNPC)
				//    ((GameNPC)m_caster).StopSpellAttack();
			}
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

			/// <summary>
			/// Constructs a new DelayedSpellTimer
			/// </summary>
			/// <param name="actionSource">The caster</param>
			/// <param name="handler">The spell handler</param>
			/// <param name="target">The target object</param>
			public DelayedCastTimer(GameLiving actionSource, SpellHandler handler, GameLiving target)
				: base(actionSource.CurrentRegion.TimeManager)
			{
				if (handler == null)
					throw new ArgumentNullException("handler");
				m_handler = handler;
				m_target = target;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving target = m_target;

				try
				{
					if (m_handler.CheckEndCast(target))
					{
						m_handler.FinishSpellCast(target);
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(ToString(), e);
				}
				m_handler.OnAfterSpellCastSequence();
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
			int ticks = m_spell.CastTime;
			if (Spell.InstrumentRequirement != 0 || SpellLine.KeyName == "Item Spells")
			{
				return ticks;
			}
			GamePlayer player = m_caster as GamePlayer;
			if (player != null)
			{
				//Fix for Warlock castingtime
				if (player.CharacterClass.ID == (int)eCharacterClass.Warlock)
				{
					if (Spell.SpellType == "Chamber")
						return ticks;

					if ((m_spellLine.KeyName == "Cursing"
					    || m_spellLine.KeyName == "Cursing Spec")
					    && (Spell.SpellType != "ArmorFactorBuff"
					    && Spell.SpellType != "Bladeturn"
					    && Spell.SpellType != "ArmorAbsorbtionBuff"))
						return ticks;
				}
			}
			double percent = 1.0;
			int dex = m_caster.GetModified(eProperty.Dexterity);		
			if(SpellLine.KeyName.Length>=18 && SpellLine.KeyName.Substring(0,18)=="Champion Abilities" ) dex=100; //Vico: No casting time diminution for CL Spells
			
			if (m_caster.EffectList.GetOfType(typeof(QuickCastEffect)) != null)
			{
				return 2000; //always 2 sec
			}
			//http://daoc.nisrv.com/modules.php?name=DD_DMG_Calculator
			//Q: Would you please give more detail as to how dex affects a caster?
			//For instance, I understand that when I have my dex maxed I will cast 25% faster.
			//How does this work incrementally? And will a lurikeen be able to cast faster in the end than another race?
			//A: From a dex of 50 to a dex of 250, the formula lets you cast 1% faster for each ten points.
			//From a dex of 250 to the maximum possible (which as you know depends on your starting total),
			//your speed increases 1% for every twenty points.

			if (dex < 60)
			{
				//do nothing.  THis may prove inaccurate for level 10 trolls
			}
			else if (dex < 250)
			{
				percent = 1.0 - (dex - 60) * 0.15 * 0.01;
			}
			else
			{
				percent = 1.0 - ((dex - 60) * 0.15 + (dex - 250) * 0.05) * 0.01;
			}
			if (player != null)
				percent *= 1.0 - m_caster.GetModified((Spell.SpellType != "Archery" ? eProperty.CastingSpeed : eProperty.ArcherySpeed)) * 0.01;

			ticks = (int)(ticks * Math.Max(0.4, percent));
			if (ticks < 1)
				ticks = 1; // at least 1 tick
			return ticks;
		}

		/// <summary>
		/// The casting time reduction based on dexterity bonus.
		/// </summary>
		public virtual double DexterityCastTimeReduction
		{
			get
			{
				int dex = Caster.GetModified(eProperty.Dexterity);
				if (dex < 60) return 1.0;
				else if (dex < 250) return 1.0 - (dex - 60) * 0.15 * 0.01;
				else return 1.0 - ((dex - 60) * 0.15 + (dex - 250) * 0.05) * 0.01;
			}
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
			if (m_spell.SubSpellID > 0 && Spell.SpellType != "Archery" && Spell.SpellType != "Bomber" && Spell.SpellType != "Turret" && Spell.SpellType != "Grapple")
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
					foreach (Spell sp in SkillBase.GetSpellList(m_spellLine.KeyName))
					{
						if (sp.SpellType == m_spell.SpellType && sp.RecastDelay == m_spell.RecastDelay && sp.Group == m_spell.Group)
						{
							m_caster.DisableSkill(sp, sp.RecastDelay);
						}
					}
				}
				else if (m_caster is GameNPC)
					m_caster.DisableSkill(m_spell, m_spell.RecastDelay);
			}

			GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastSpellEventArgs(this, target));
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
			string NewTarget = Spell.Target.ToLower();
			ushort NewRadius = (ushort)Spell.Radius;
			int newtarget = 0;

			GameSpellEffect TargetMod = SpellHandler.FindEffectOnTarget(m_caster, "TargetModifier");
			if (TargetMod != null)
			{
				if (NewTarget == "enemy" || NewTarget == "realm" || NewTarget == "group")
				{
					newtarget = (int)TargetMod.Spell.Value;

					switch (newtarget)
					{
						case 0: // Apply on heal single
							if (m_spell.SpellType.ToLower() == "heal" && NewTarget == "realm")
							{
								NewTarget = "group";
								targetchanged = true;
							}
							break;
						case 1: // Apply on heal group
							if (m_spell.SpellType.ToLower() == "heal" && NewTarget == "group")
							{
								NewTarget = "realm";
								NewRadius = (ushort)m_spell.Range;
								targetchanged = true;
							}
							break;
						case 2: // apply on enemy
							if (NewTarget == "enemy")
							{
								if (m_spell.Radius == 0)
									NewRadius = 450;
								if (m_spell.Radius != 0)
									NewRadius += 300;
								targetchanged = true;
							}
							break;
						case 3: // Apply on buff
							if (m_spell.Target.ToLower() == "group"
							   && m_spell.Pulse != 0)
							{
								NewTarget = "realm";
								NewRadius = (ushort)m_spell.Range;
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
			switch (NewTarget)
			{
				#region GTAoE
				// GTAoE
				case "area":
                    //Dinberg - fix for animists turrets, where before a radius of zero meant that no targets were ever 
                    //selected!
                    if		(Spell.SpellType == "Turret")
                        list.Add(Caster);
                    else
                        if (NewRadius > 0)
                        {
                            foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, NewRadius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                                {
                                    // Apply Mentalist RA5L
									SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
									if (SelectiveBlindness != null)
									{
										GameLiving EffectOwner = SelectiveBlindness.EffectSource;
										if(EffectOwner==player)
										{
											if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
										}
										else list.Add(player);
									}
									else list.Add(player);
                                }
                            }
                            foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, NewRadius))
                            {
                                if (npc is GameStorm)
                                    list.Add(npc);
                                else if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                                {
                                    if(!npc.HasAbility("DamageImmunity")) list.Add(npc);
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

					ControlledNpc brain = Caster.ControlledNpc as ControlledNpc;
					GameNPC petBody = brain == null ? null : brain.Body;

					//Make sure we actually have a pet
					if (petBody != null)
					{
						//Single target pet buff has radius 0
						if (Spell.Radius == 0)
						{
							//If we have our p
							if (petBody != castTarget)
							{
								//Make sure we have some subpets
								if (petBody.ControlledNpcList != null)
								{
									//Go through each subpet and see if we have it targeted
									foreach (IControlledBrain icb in petBody.ControlledNpcList)
									{
										if (icb != null && icb.Body == castTarget)
										{
											list.Add(icb.Body);
											break;
										}
									}
								}
							}
							
							//If we didn't find any of our subpets as our target then we add our main pet
							if (list.Count == 0)
								list.Add(petBody);
						}
						//Our buff affects every pet in the area (our pets)
						else
						{
							//Obviously, add our main pet
							if (WorldMgr.CheckDistance(m_caster, petBody, Spell.Radius))
								list.Add(petBody);

							//Make sure we have some subpets
							if (petBody.ControlledNpcList != null)
							{
								//Go through each subpet and make sure they're in our radius, then add if so
								foreach (IControlledBrain icb in petBody.ControlledNpcList)
								{
									if (icb != null && WorldMgr.CheckDistance(m_caster, icb.Body, Spell.Radius))
									{
										list.Add(icb.Body);
									}
								}
							}
						}
					}

					break;
				#endregion
				#region Enemy
				case "enemy":
					if (NewRadius > 0)
					{
						if (target == null || Spell.Range == 0)
							target = Caster;
						foreach (GamePlayer player in target.GetPlayersInRadius(NewRadius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
							{
								SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
								if (SelectiveBlindness != null)
								{
									GameLiving EffectOwner = SelectiveBlindness.EffectSource;
									if(EffectOwner==player)
									{
										if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
									}
									else list.Add(player);
								}
								else list.Add(player);
							}
						}
						foreach (GameNPC npc in target.GetNPCsInRadius(NewRadius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
							{
								if(!npc.HasAbility("DamageImmunity")) list.Add(npc);
							}
						}
					}
					else
					{
						if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
						{
							// Apply Mentalist RA5L
							if(Spell.Range>0)
							{
							SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
								if (SelectiveBlindness != null)
								{
									GameLiving EffectOwner = SelectiveBlindness.EffectSource;
									if(EffectOwner==target)
									{
										if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
									}
									else if(!target.HasAbility("DamageImmunity")) list.Add(target);
								}
								else if(!target.HasAbility("DamageImmunity")) list.Add(target);
							} else if(!target.HasAbility("DamageImmunity")) list.Add(target);
						}
					}
					break;
				#endregion
				#region Realm
				case "realm":
					if (NewRadius > 0)
					{
						if (target == null || Spell.Range == 0)
							target = Caster;
						foreach (GamePlayer player in target.GetPlayersInRadius(NewRadius))
						{
							if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
							{
								list.Add(player);
							}
						}
						foreach (GameNPC npc in target.GetNPCsInRadius(NewRadius))
						{
							if (GameServer.ServerRules.IsSameRealm(Caster, npc, true))
							{
								list.Add(npc);
							}
						}
					}
					else
					{
						if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
							list.Add(target);
					}
					break;
				#endregion
				#region Self
				case "self":
					{
						if (NewRadius > 0)
						{
							if (target == null || Spell.Range == 0)
								target = Caster;
							foreach (GamePlayer player in target.GetPlayersInRadius(NewRadius))
							{
								if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
								{
									list.Add(player);
								}
							}
							foreach (GameNPC npc in target.GetNPCsInRadius(NewRadius))
							{
								if (GameServer.ServerRules.IsSameRealm(Caster, npc, true))
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
							spellRange = NewRadius;

						//Just add ourself
						if (group == null)
						{
							list.Add(m_caster);

							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								//Add our first pet
								GameNPC petBody2 = npc.Body;
								if (WorldMgr.CheckDistance(m_caster, petBody2, spellRange))
									list.Add(petBody2);

								//Now lets add any subpets!
								if (petBody2 != null && petBody2.ControlledNpcList != null)
								{
									foreach (IControlledBrain icb in petBody2.ControlledNpcList)
									{
										if (icb != null && WorldMgr.CheckDistance(m_caster, icb.Body, spellRange))
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
								if (WorldMgr.CheckDistance(m_caster, living, spellRange))
								{
									list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										//Add our first pet
										GameNPC petBody2 = npc.Body;
										if (WorldMgr.CheckDistance(m_caster, petBody2, spellRange))
											list.Add(petBody2);

										//Now lets add any subpets!
										if (petBody2 != null && petBody2.ControlledNpcList != null)
										{
											foreach (IControlledBrain icb in petBody2.ControlledNpcList)
											{
												if (icb != null && WorldMgr.CheckDistance(m_caster, icb.Body, spellRange))
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

							if(!npc.HasAbility("DamageImmunity")) list.Add(npc);

						}
						break;
					}
				#endregion
			}
			#endregion
			return list;
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		/// <param name="target">The current target object</param>
		public virtual void StartSpell(GameLiving target)
		{
			GamePlayer player = Caster as GamePlayer;
			if (target == null || (Spell.Radius > 0 && Spell.Range == 0))
				target = Caster;
			IList targets = SelectTargets(target);

			double effectiveness = 1.0;

			if (Caster is GamePlayer)
				effectiveness = player.Effectiveness;

			if (Caster.EffectList.GetOfType(typeof(MasteryofConcentrationEffect)) != null)
			{
				RealmAbility ra = Caster.GetAbility(typeof(MasteryofConcentrationAbility)) as RealmAbility;
				if (ra != null)
					effectiveness = System.Math.Round((double)ra.Level * 25 / 100, 2);
			}
			// warlock - fixed by Dinberg, thx
			if (Caster is GamePlayer && (Caster as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Warlock && m_spell.IsSecondary)
			{
				GameSpellEffect affect = SpellHandler.FindEffectOnTarget(Caster, "Uninterruptable");
				if (affect != null)
				{
					int nerf = (int)(affect.Spell.Value);
					effectiveness *= (1 - (nerf * 0.01));
				}
			}

			foreach (GameLiving t in targets)
			{
				// Aggressive NPCs will aggro on every target they hit
				// with an AoE spell, whether it landed or was resisted.

				if (Spell.Radius > 0 && Spell.Target.ToLower() == "enemy" 
					&& Caster is GameNPC && (Caster as GameNPC).Brain is IAggressiveBrain)
						((Caster as GameNPC).Brain as IAggressiveBrain).AddToAggroList(t, 1);

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
					int dist = WorldMgr.GetDistance(t, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(dist, Spell.Radius)));
					}
				}
				else if (Spell.Target.ToLower() == "cone")
				{
					int dist = WorldMgr.GetDistance(Caster, t);
					if (dist >= 0)
					{
						//Cone spells use the range for their variance!
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(dist, Spell.Range)));
					}
				}
				else
				{
					int dist = WorldMgr.GetDistance(target, t);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(dist, Spell.Radius)));
					}
				}
			}
		}

		/// <summary>
		/// Calculate the variance due to the radius of the spell
		/// </summary>
		/// <param name="distance">The distance away from center of the spell</param>
		/// <param name="radius">The radius of the spell</param>
		/// <returns></returns>
		protected virtual double CalculateAreaVariance(int distance, int radius)
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

			if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent) && Spell.SpellType!="SiegeArrow")
			{
				MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
				return;
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
									if (immunity.ImmunityState && immunity.Owner is GamePlayer)
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
			//				if (log.IsWarnEnabled)
			//					log.Warn("Spell effect compare with different types " + oldspell.SpellType + " <=> " + newspell.SpellType + "\n" + Environment.StackTrace);
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
				GameEventMgr.AddHandler(Caster, GameLivingEvent.CastSpell, new DOLEventHandler(FocusSpellAction));
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
			if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
				return 0;
			if (HasPositiveEffect)
				return 0;
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
					target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
			}*/
			if(!(Spell.SpellType.ToLower().IndexOf("debuff")>=0 && Spell.CastTime==0))
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);


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
				(Caster as GamePlayer).Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
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

			GameSpellEffect currentEffect = (GameSpellEffect)living.TempProperties.getObjectProperty(FOCUS_SPELL, null);
			if (currentEffect == null) return;

			if (args is CastSpellEventArgs)
			{
				if ((args as CastSpellEventArgs).SpellHandler.Caster != Caster)
					return;
				if ((args as CastSpellEventArgs).SpellHandler.Spell.SpellType == currentEffect.Spell.SpellType)
					return;
			}

			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastSpell, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(currentEffect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			Caster.TempProperties.removeProperty(FOCUS_SPELL);

			if (currentEffect.Spell.Pulse != 0) CancelPulsingSpell(Caster, currentEffect.Spell.SpellType);
			else currentEffect.Cancel(false);

			MessageToCaster(String.Format("You lose your focus on your {0} spell.", currentEffect.Spell.Name), eChatType.CT_SpellExpires);
			if (e == GameLivingEvent.Moving)
				MessageToCaster("You move and interrupt your focus!", eChatType.CT_System);
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
				if (m_spell.Target != "Enemy" && m_spell.Target != "Cone")
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
		public virtual IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				//list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				//list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add("Instrument require: " + GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement));
				if (Spell.Damage != 0)
					list.Add("Damage: " + Spell.Damage.ToString("0.###;0.###'%'"));
				if (Spell.LifeDrainReturn != 0)
					list.Add("Health returned: " + Spell.LifeDrainReturn + "% of damage dealt");
				else if (Spell.Value != 0)
					list.Add("Value: " + Spell.Value.ToString("0.###;0.###'%'"));
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));
				if (Spell.Power != 0)
					list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
					list.Add("Concentration cost: " + Spell.Concentration);
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);
				if (Spell.DamageType != eDamageType.Natural)
					list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));
				if (Spell.IsFocus)
					list.Add("This is a focus spell. Cancels if you do any action.");

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
			return 0.085; // 8.5% diff per level distance
		}

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public virtual void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects || m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || m_spellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
			{
				min = max = 1.0;
				return;
			}

			int speclevel = 1;

			if (m_caster is GameNPC && ((GameNPC)m_caster).Brain is IControlledBrain)
			{
				#warning This needs to be changed when GamePet is made
				IControlledBrain brain = (m_caster as GameNPC).Brain as IControlledBrain;
				speclevel = brain.GetPlayerOwner().GetModifiedSpecLevel(m_spellLine.Spec);
			}
			else if (m_caster is GamePlayer)
			{
				int a = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
				if (m_spellLine.Name == "Archery")
				{
					if (a >= 45)
						speclevel = 45;
					else
						speclevel = a;
				}
				else
					speclevel = a;
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
					min += GetLevelModFactor() * (owner.Level - owner.Level);
					max += GetLevelModFactor() * (owner.Level - owner.Level);
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
		/// Calculates the base 100% spell damage which is then modified by damage variance factors
		/// </summary>
		/// <returns></returns>
		public virtual double CalculateDamageBase()
		{
			double spellDamage = Spell.Damage;
			GamePlayer player = Caster as GamePlayer;

			// For pets the stats of the owner have to be taken into account.

			if (Caster is GameNPC && ((Caster as GameNPC).Brain) is IControlledBrain)
				player = (((Caster as GameNPC).Brain) as IControlledBrain).Owner as GamePlayer;

			if (player != null)
			{
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
				    && player.CharacterClass.ID != (int)eCharacterClass.Mauler_Alb
				    && player.CharacterClass.ID != (int)eCharacterClass.Mauler_Mid
				    && player.CharacterClass.ID != (int)eCharacterClass.Mauler_Hib
				    && player.CharacterClass.ID != (int)eCharacterClass.Vampiir)
				{
					int manaStatValue = player.GetModified((eProperty)player.CharacterClass.ManaStat);
					spellDamage *= (manaStatValue + 200) / 275.0;
				}
			}

			if (Spell.SpellType == "Archery")
				spellDamage /= 1.5;

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
			if (m_caster is GameNPC && (m_caster as GameNPC).Brain is ControlledNpc)
				caster = ((ControlledNpc)((GameNPC)m_caster).Brain).Owner;
			else caster = m_caster;
			int spellbonus = caster.GetModified(eProperty.SpellLevel);
			spellLevel += spellbonus;
			//Cap on lvl 50 for spell level
			if (spellLevel > 50)
				spellLevel = 50;

			//Andraste
			if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect
				|| (m_spellLine.KeyName.Length>=18 && SpellLine.KeyName.Substring(0,18)=="Champion Abilities" ))
					spellLevel=50;

			int speclevel = 1;
			int manastat = 0;
			int bonustohit = m_caster.GetModified(eProperty.ToHitBonus);
			if (caster is GamePlayer)
			{
				GamePlayer player = caster as GamePlayer;
				speclevel = player.GetBaseSpecLevel(m_spellLine.Spec);
				if (player.CharacterClass.ManaStat != eStat.UNDEFINED)
					manastat = player.GetModified((eProperty)player.CharacterClass.ManaStat);
				bonustohit += (int)(speclevel * 0.1 + manastat * 0.01);
			}
			//Piercing Magic affects to-hit bonus too
			GameSpellEffect resPierce = SpellHandler.FindEffectOnTarget(m_caster, "PenetrateResists");
			if (resPierce != null)
				bonustohit += (int)resPierce.Spell.Value;
			int hitchance = 85 + ((spellLevel - target.Level) >> 1) + bonustohit;
			if (!(caster is GamePlayer && target is GamePlayer))
			{
				// level mod
				hitchance -= (int)(m_caster.GetConLevel(target) * 10);
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
			double spellDamage = CalculateDamageBase();

			if (m_caster is GamePlayer)
				effectiveness += m_caster.GetModified((Spell.SpellType != "Archery" ? eProperty.SpellDamage : eProperty.RangedDamage)) * 0.01;

			spellDamage *= m_caster.Effectiveness;
			int finalDamage = Util.Random((int)(minVariance * spellDamage), (int)(maxVariance * spellDamage));

			int hitChance = CalculateToHitChance(ad.Target);
			if (hitChance < 55)
			{
				finalDamage += (int)(finalDamage * (hitChance - 55) * 0.01);
				hitChance = 55;

			}
			else if (hitChance > 100)
			{
				finalDamage += (int)(finalDamage * (hitChance - 100) * 0.01);
				hitChance = 100;

			}

			// apply effectiveness
			finalDamage = (int)(finalDamage * effectiveness);
			if ((m_caster is GamePlayer || (m_caster is GameNPC && (m_caster as GameNPC).Brain is IControlledBrain && m_caster.Realm != 0)))
			{
				if (target is GamePlayer)
					finalDamage = (int)((double)finalDamage * ServerProperties.Properties.PVP_DAMAGE);
				else if (target is GameNPC)
					finalDamage = (int)((double)finalDamage * ServerProperties.Properties.PVE_DAMAGE);
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

			#region Resists
			eProperty property = target.GetResistTypeForDamage(Spell.DamageType);
			// The Daoc resistsystem is since 1.65 a 2category system.
			// - First category are Item/Race/Buff/RvrBanners resists that are displayed in the characteroverview.
			// - Second category are resists that are given through RAs like avoidance of magic, brilliance aura of deflection.
			//   Those resist affect ONLY the spelldamage. Not the duration, not the effectiveness of debuffs.
			// so calculation is (finaldamage * Category1Modification) * Category2Modification
			// -> Remark for the future: VampirResistBuff is Category2 too.
			// - avi

			#region Primary Resists
			int primaryResistModifier = ad.Target.GetResist(Spell.DamageType);

			/* Resist Pierce	
			 * Resipierce is a special bonus which has been introduced with ToA.
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


			// cap to +200% of base damage
			if (finalDamage > Spell.Damage * 3 * effectiveness)
			{
				finalDamage = (int)(Spell.Damage * 3 * effectiveness);
			}

			if (finalDamage < 0)
				finalDamage = 0;

			int criticalchance = (Spell.SpellType != "Archery" ? m_caster.SpellCriticalChance : m_caster.GetModified(eProperty.CriticalArcheryHitChance));
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
			ad.DamageType = Spell.DamageType;
			ad.Modifier = resistModifier;

			return ad;
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
			foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);
			}
			// send animation before dealing damage else dead livings show no animation
			ad.Target.OnAttackedByEnemy(ad);
			ad.Attacker.DealDamage(ad);
			if (ad.Damage == 0 && ad.Target is GameNPC)
			{
				IAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
		}

		#endregion

		#region saved effects
		public virtual PlayerXEffect getSavedEffect(GameSpellEffect effect)
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
