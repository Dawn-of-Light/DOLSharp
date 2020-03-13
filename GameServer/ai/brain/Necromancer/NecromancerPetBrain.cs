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
using System.Text;
using DOL.GS;
using System.Collections;
using System.Reflection;
using log4net;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain for the necromancer pets.
	/// </summary>
	/// <author>Aredhel</author>
	public class NecromancerPetBrain : ControlledNpcBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public NecromancerPetBrain(GameLiving owner) 
			: base(owner)
		{
		}


		public override int ThinkInterval
		{
			get { return 1000; }
		}


		public override int CastInterval
		{
			get { return 500; }
			set { }
		}


        /// <summary>
        /// Brain main loop.
        /// </summary>
		public override void Think()
		{
            CheckTether();

			// Necro pets need there own think as they may need to cast a spell in any state
			if (IsActive)
			{
				GamePlayer playerowner = GetPlayerOwner();

				if (playerowner != null && (GameTimer.GetTickCount() - playerowner.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(Body.CurrentRegionID, (ushort)Body.ObjectID)]) > ThinkInterval)
				{
					playerowner.Out.SendObjectUpdate(Body);
				}

				if (SpellsQueued)
					// if spells are queued then handle them first
					CheckSpellQueue();
				else if (AggressionState == eAggressionState.Aggressive)
				{
					CheckPlayerAggro();
					CheckNPCAggro();
				}

				AttackMostWanted();

				// Do not discover stealthed players
				if (Body.TargetObject != null)
				{
					if (Body.TargetObject is GamePlayer)
					{
						if (Body.IsAttacking && (Body.TargetObject as GamePlayer).IsStealthed)
						{
							Body.StopAttack();
							FollowOwner();
						}
					}
				}
			}
		}

		#region Events

		/// <summary>
		/// Process events.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOL.Events.DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			if (e == GameNPCEvent.PetSpell)
			{
				PetSpellEventArgs petSpell = (PetSpellEventArgs)args;
				bool hadQueuedSpells = false;

				if (SpellsQueued)
				{
					MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AI.Brain.Necromancer.CastSpellAfterAction", Body.Name), eChatType.CT_System);
					hadQueuedSpells = true;
				}

				// Instant spells bypass the queue
				if (petSpell.Spell.IsInstantCast)
					CastSpell(petSpell.Spell, petSpell.SpellLine, petSpell.Target);
				else // Necro pets can always try to cast in combat
					AddToSpellQueue(petSpell.Spell, petSpell.SpellLine, petSpell.Target);

				// Immediately cast if this was the first spell added
				if (hadQueuedSpells == false && !Body.IsCasting)
					CheckSpellQueue();
			}
            else if (e == GameLivingEvent.Dying)
            {
                // At necropet Die, we check DamageRvRMemory for transfer it to owner if necessary.
                GamePlayer playerowner = GetPlayerOwner();
                if (playerowner != null && Body.DamageRvRMemory > 0)
                {
                    playerowner.DamageRvRMemory = Body.DamageRvRMemory;
                }
                return;
            }
            else if (e == GameLivingEvent.CastFinished)
            {
				// Instant cast spells bypass the queue
				if (args is CastingEventArgs cArgs && !cArgs.SpellHandler.Spell.IsInstantCast)
				{
					// Remove the spell that has finished casting from the queue, if
					// there are more, keep casting.
					RemoveSpellFromQueue();
					AttackMostWanted();

					if (SpellsQueued)
					{
						DebugMessageToOwner("+ Cast finished, more spells to cast");
						CheckSpellQueue();
					}
					else
						DebugMessageToOwner("- Cast finished, no more spells to cast");
				}

				Owner.Notify(GamePlayerEvent.CastFinished, Owner, args);
			}
			else if (e == GameLivingEvent.CastFailed)
			{
				// Tell owner why cast has failed.
				switch ((args as CastFailedEventArgs).Reason)
				{
                    case CastFailedEventArgs.Reasons.TargetTooFarAway:

                        MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
                            "AI.Brain.Necromancer.ServantFarAwayToCast"), eChatType.CT_SpellResisted);
                        break;

                    case CastFailedEventArgs.Reasons.TargetNotInView:

                        MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
                            "AI.Brain.Necromancer.PetCantSeeTarget", Body.Name), eChatType.CT_SpellResisted);
                        break;

					case CastFailedEventArgs.Reasons.NotEnoughPower:

						RemoveSpellFromQueue();
						MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
							"AI.Brain.Necromancer.NoPower", Body.Name), eChatType.CT_SpellResisted);

						break;
                }
            }
            else if (e == GameLivingEvent.CastSucceeded)
            {
                // The spell will cast.

                PetSpellEventArgs spellArgs = args as PetSpellEventArgs;
                GameLiving target = spellArgs.Target;
                SpellLine spellLine = spellArgs.SpellLine;

				if (spellArgs != null && spellArgs.Spell != null)
					DebugMessageToOwner(String.Format("Now casting '{0}'", spellArgs.Spell.Name));


                // This message is for spells from the spell queue only, so suppress
                // it for insta cast buffs coming from the pet itself.

                if (spellLine.Name != (Body as NecromancerPet).PetInstaSpellLine)
                {
                    Owner.Notify(GameLivingEvent.CastStarting, Body, new CastingEventArgs(Body.CurrentSpellHandler));
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AI.Brain.Necromancer.PetCastingSpell", Body.Name), eChatType.CT_System);
                }

                // If pet is casting an offensive spell and is not set to
                // passive, put target on its aggro list; that way, even with
                // no attack directive from the owner it will start an attack
                // after the cast has finished.

                if (target != Body && spellArgs.Spell.Target == "Enemy")
                {
                    if (target != null)
                    {
                        if (!Body.AttackState && AggressionState != eAggressionState.Passive)
                        {
                            (Body as NecromancerPet).DrawWeapon();
                            AddToAggroList(target, 1);
                        }
                    }
                }
            }
            else if (e == GameNPCEvent.SwitchedTarget && sender == Body.TargetObject &&
                sender is GameNPC && !(sender as GameNPC).IsCrowdControlled)
            {
                // Target has started attacking someone else.

				if (Body.EffectList.GetOfType<TauntEffect>() != null)
                    (Body as NecromancerPet).Taunt();            
            }
            else if (e == GameNPCEvent.AttackFinished)
            {
                Owner.Notify(GamePlayerEvent.AttackFinished, Owner, args);
            }
            else if (e == GameNPCEvent.OutOfTetherRange)
            {
                // Pet past its tether, update effect icon (remaining time) and send 
                // warnings to owner at t = 10 seconds and t = 5 seconds.

                int secondsRemaining = (args as TetherEventArgs).Seconds;
                SetTetherTimer(secondsRemaining);

                if (secondsRemaining == 10)
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
                        "AI.Brain.Necromancer.PetTooFarBeLostSecIm", secondsRemaining), eChatType.CT_System);
                else if (secondsRemaining == 5)
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
                        "AI.Brain.Necromancer.PetTooFarBeLostSec", secondsRemaining), eChatType.CT_System);
            }
            else if (e == GameNPCEvent.PetLost)
            {
                // Pet despawn is imminent, notify owner.

                MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
                    "AI.Brain.Necromancer.HaveLostBondToPet"), eChatType.CT_System);
            }
		}

		/// <summary>
		/// Set the tether timer if pet gets out of range or comes back into range.
		/// </summary>
		/// <param name="seconds"></param>
		public void SetTetherTimer(int seconds)
		{
			NecromancerShadeEffect shadeEffect = Owner.EffectList.GetOfType<NecromancerShadeEffect>();

			if (shadeEffect != null)
			{
				lock (shadeEffect)
					shadeEffect.SetTetherTimer(seconds);
				ArrayList effectList = new ArrayList(1);
				effectList.Add(shadeEffect);
				int effectsCount = 1;
				if (Owner is GamePlayer)
				{
					(Owner as GamePlayer).Out.SendUpdateIcons(effectList, ref effectsCount);
				}
			}
		}



		#endregion

		#region Spell Queue
		/// <summary>
        /// See if there are any spells queued up and if so, get the first one
        /// and cast it.
        /// </summary>
		private void CheckSpellQueue()
		{
			SpellQueueEntry entry = GetSpellFromQueue();
			if (entry != null)
				if (!CastSpell(entry.Spell, entry.SpellLine, entry.Target))
					// If the spell can't be cast, remove it from the queue
					RemoveSpellFromQueue();
		}

		/// <summary>
		/// Try to cast a spell, returning true if the spell started to cast
		/// </summary>
		/// <returns>Whether or not the spell started to cast</returns>
		private bool CastSpell(Spell spell, SpellLine line, GameObject target)
		{
			GameLiving spellTarget = target as GameLiving;

			// Cast spell on the target, but don't automatically
			// make it our new target.

			// Target must be alive, or this is a self spell, or this is a pbaoe spell
			if ((spellTarget != null && spellTarget.IsAlive) || spell.Target.ToLower() == "self" || spell.Range == 0)
			{
				GameObject previousTarget = Body.TargetObject;
				Body.TargetObject = spellTarget;

				if (spellTarget != null && spellTarget != Body)
					Body.TurnTo(spellTarget);

				Body.CastSpell(spell, line);

				if (previousTarget != null)
					Body.TargetObject = previousTarget;

				return true;
			}
			else
			{
				DebugMessageToOwner("Invalid target for spell '" + spell.Name + "'");
				return false;
			}
		}

		/// <summary>
		/// This class holds a single entry for the spell queue.
		/// </summary>
		private class SpellQueueEntry
		{
			private Spell m_spell;
			private SpellLine m_spellLine;
			private GameLiving m_target;

			public SpellQueueEntry(Spell spell, SpellLine spellLine, GameLiving target)
			{
				m_spell = spell;
				m_spellLine = spellLine;
				m_target = target;
			}

			public SpellQueueEntry(SpellQueueEntry entry) : this(entry.Spell, entry.SpellLine, entry.Target)
			{
			}

			public Spell Spell
			{
				get { return m_spell; }
			}

			public SpellLine SpellLine
			{
				get { return m_spellLine; }
			}

			public GameLiving Target
			{
				get { return m_target; }
			}
		}

		private Queue<SpellQueueEntry> m_spellQueue = new Queue<SpellQueueEntry>(2);

		/// <summary>
		/// Clears the spell queue
		/// </summary>
		public void ClearSpellQueue()
		{
			lock (m_spellQueue)
				m_spellQueue.Clear();
		}

		/// <summary>
		/// Fetches a spell from the queue without removing it; the spell is
        /// removed *after* the spell has finished casting.
		/// </summary>
		/// <returns>The next spell or null, if no spell is in the queue.</returns>
		private SpellQueueEntry GetSpellFromQueue()
		{
			lock (m_spellQueue)
			{
                if (m_spellQueue.Count > 0)
                {
                    DebugMessageToOwner(String.Format("Grabbing spell '{0}' from the start of the queue in order to cast it", m_spellQueue.Peek().Spell.Name));
                    return m_spellQueue.Peek();
                }
			}
			return null;
		}

		/// <summary>
		/// Whether or not any spells are queued.
		/// </summary>
		public bool SpellsQueued
		{
			get
			{
				lock (m_spellQueue)
					return (m_spellQueue.Count > 0);
			}
		}

		/// <summary>
		/// Removes the spell that is first in the queue.
		/// </summary>
		private void RemoveSpellFromQueue()
		{
			lock (m_spellQueue)
			{
                if (m_spellQueue.Count > 0)
                {
                    DebugMessageToOwner(String.Format("Removing spell '{0}' from the start of the queue", m_spellQueue.Peek().Spell.Name));

                    m_spellQueue.Dequeue();
                }
			}
		}

		/// <summary>
		/// Add a spell to the queue. If there are already 2 spells in the
		/// queue, remove the spell that the pet would cast next.
		/// </summary>
		/// <param name="spell">The spell to add.</param>
		/// <param name="spellLine">The spell line the spell is in.</param>
		/// <param name="target">The target to cast the spell on.</param>
		private void AddToSpellQueue(Spell spell, SpellLine spellLine, GameLiving target)
		{
			lock (m_spellQueue)
			{
				if (m_spellQueue.Count >= 2)
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
                        "AI.Brain.Necromancer.SpellNoLongerInQueue", 
                        (m_spellQueue.Dequeue()).Spell.Name, Body.Name), 
                        eChatType.CT_Spell);

                DebugMessageToOwner(String.Format("Adding spell '{0}' to the end of the queue", spell.Name));
				m_spellQueue.Enqueue(new SpellQueueEntry(spell, spellLine, target));
			}
		}

		#endregion

		#region Tether

		private const int m_softTether = 2000;    // TODO: Check on Pendragon
        private const int m_hardTether = 2500;
        private TetherTimer m_tetherTimer = null;

        private void CheckTether()
        {
            // Check if pet is past hard tether range, if so, despawn it
            // right away.

            if (!Body.IsWithinRadius(Owner, m_hardTether))
            {
				if (m_tetherTimer != null)
					m_tetherTimer.Stop();
                (Body as NecromancerPet).CutTether();
                return;
            }

            // Check if pet is out of soft tether range.

            if (!Body.IsWithinRadius(Owner, m_softTether))
            {
                if (m_tetherTimer == null)
                {
                    // Pet just went out of range, start the timer.

                    m_tetherTimer = new TetherTimer(Body as NecromancerPet);
                    m_tetherTimer.Start(1);
                }
            }
            else
            {
                if (m_tetherTimer != null)
                {
                    // Pet is back in range, stop the timer.

                    m_tetherTimer.Stop();
                    m_tetherTimer = null;

                    SetTetherTimer(-1);
                }
            }
        }

        /// <summary>
        /// Timer for pet out of tether range.
        /// </summary>
        private class TetherTimer : GameTimer
        {
            private NecromancerPet m_pet;
            private int m_seconds = 10;

            public TetherTimer(NecromancerPet pet) 
                : base(pet.CurrentRegion.TimeManager) 
            {
                m_pet = pet;
            }

            protected override void OnTick()
            {
                this.Interval = 1000;

                if (m_seconds > 0)
                {
                    m_pet.Brain.Notify(GameNPCEvent.OutOfTetherRange, this, 
						new TetherEventArgs(m_seconds));
                    m_seconds -= 1;
                }
                else
                {
                    Stop();
                    m_pet.Brain.Notify(GameNPCEvent.PetLost, this, null);
                    m_pet.CutTether();
                }
            }
        }

		/// <summary>
		/// Send a message to the shade.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="chatType"></param>
		public void MessageToOwner(String message, eChatType chatType)
		{
			GamePlayer owner = Owner as GamePlayer;
			if ((owner != null) && (message.Length > 0))
				owner.Out.SendMessage(message, chatType, eChatLoc.CL_SystemWindow);
		}

        /// <summary>
        /// For debugging purposes only.
        /// </summary>
        /// <param name="message"></param>
        private void DebugMessageToOwner(String message)
        {
			if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG)
			{
				int tick = Environment.TickCount;
				int seconds = tick / 1000;
				int minutes = seconds / 60;

				MessageToOwner(String.Format("[{0:00}:{1:00}.{2:000}] {3}",	minutes % 60, seconds % 60, tick % 1000, message), eChatType.CT_Staff);
			}
        }

		#endregion
	}
}
