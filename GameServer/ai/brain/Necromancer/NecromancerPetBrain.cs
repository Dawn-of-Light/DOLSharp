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
	public class NecromancerPetBrain : ControlledNpc
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public NecromancerPetBrain(GameLiving owner) 
			: base(owner)
		{
		}

        /// <summary>
        /// Brain main loop.
        /// </summary>
		public override void Think()
		{
            CheckTether();
			if (IsActive)
				base.Think();
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
				AddToSpellQueue(petSpell.Spell, petSpell.SpellLine, petSpell.Target);

				// If we're not currently casting or meleeing, cast 
				// right away.

                if (!Body.IsCasting && Body.AttackState)
                {
                    DebugMessageToOwner(String.Format("Spell '{0}' relayed from shade, but pet in melee",
                        petSpell.Spell.Name));
                }

                if (!Body.IsCasting && !Body.AttackState)
                    CheckSpellQueue();
                else
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                        "AI.Brain.Necromancer.CastSpellAfterAction", Body.Name), eChatType.CT_System);
			}
            else if (e == GameLivingEvent.CastFinished)
            {
                // Remove the spell that has finished casting from the queue, if
                // there are more, keep casting.

                RemoveSpellFromQueue();
                if (SpellsQueued)
                {
                    DebugMessageToOwner("More spells to cast, grab the next one");
                    CheckSpellQueue();
                }
                else
                {
                    DebugMessageToOwner("No more spells to cast, start melee");
                    AttackMostWanted();
                }

                Owner.Notify(GamePlayerEvent.CastFinished, Owner, args);
            }
            else if (e == GameLivingEvent.CastFailed)
            {
                // Tell owner why cast has failed.

                DebugMessageToOwner(String.Format("Cast failed for '{0}')",
                    (args as CastFailedEventArgs).SpellHandler.Spell.Name));

                switch ((args as CastFailedEventArgs).Reason)
                {
                    case CastFailedEventArgs.Reasons.TargetTooFarAway:
                        MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                            "AI.Brain.Necromancer.ServantFarAwayToCast"), eChatType.CT_SpellResisted);
                        break;
                    case CastFailedEventArgs.Reasons.TargetNotInView:
                        MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                            "AI.Brain.Necromancer.PetCantSeeTarget", Body.Name), eChatType.CT_SpellResisted);
                        break;
                }
            }
            else if (e == GameLivingEvent.CastSucceeded)
            {
                // The spell will cast.

                PetSpellEventArgs spellArgs = args as PetSpellEventArgs;
                GameLiving target = spellArgs.Target;
                SpellLine spellLine = spellArgs.SpellLine;

                DebugMessageToOwner(String.Format("Now casting '{0}'", spellArgs.Spell.Name));

                // This message is for spells from the spell queue only, so suppress
                // it for insta cast buffs coming from the pet itself.

                if (spellLine.Name != (Body as NecromancerPet).PetInstaSpellLine)
                {
                    Owner.Notify(GameLivingEvent.CastStarting, Body,
                        new CastStartingEventArgs(Body.CurrentSpellHandler));
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                        "AI.Brain.Necromancer.PetCastingSpell", Body.Name), eChatType.CT_System);
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
            else if (e == GameNPCEvent.AttackFinished)
            {
                // If there are spells in the queue, hold the melee attack
                // and start casting.

                if (SpellsQueued)
                    CheckSpellQueue();
                else
                    AttackMostWanted();

                Owner.Notify(GamePlayerEvent.AttackFinished, Owner, args);
            }
            else if (e == GameNPCEvent.OutOfTetherRange)
            {
                // Pet past its tether, update effect icon (remaining time) and send 
                // warnings to owner at t = 10 seconds and t = 5 seconds.

                int secondsRemaining = (args as TetherEventArgs).Seconds;
                (Owner as GameNecromancer).SetTetherTimer(secondsRemaining);

                if (secondsRemaining == 10)
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                        "AI.Brain.Necromancer.PetTooFarBeLostSecIm", secondsRemaining), eChatType.CT_System);
                else if (secondsRemaining == 5)
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                        "AI.Brain.Necromancer.PetTooFarBeLostSec", secondsRemaining), eChatType.CT_System);
            }
            else if (e == GameNPCEvent.PetLost)
            {
                // Pet despawn is imminent, notify owner.

                MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
                    "AI.Brain.Necromancer.HaveLostBondToPet"), eChatType.CT_System);
            }
		}

		#endregion

		#region Taunt

		/// <summary>
		/// Whether or not taunt mode is activated.
		/// </summary>
		private bool TauntMode
		{
			get { return (Body.EffectList.GetOfType(typeof(TauntEffect)) != null); }
		}

		/// <summary>
		/// In addition to attacking the next target, taunt it if we
		/// we were told to do so and we don't have the attention of our
		/// target yet.
		/// </summary>
		protected override void AttackMostWanted()
		{
			base.AttackMostWanted();
			if (Body.TargetObject is GameNPC && TauntMode)
			{
                if ((Body.TargetObject as GameNPC).TargetObject != Body && ((Body.TargetObject as GameNPC).IsAlive))
                {
                    (Body as NecromancerPet).Taunt();
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
			SpellQueueEntry spellQueueEntry = GetSpellFromQueue();
			if (spellQueueEntry != null)
			{
				GameObject previousTarget = Body.TargetObject;
                GameLiving spellTarget = spellQueueEntry.Target;
                Spell spell = spellQueueEntry.Spell;

				// Cast spell on the target, but don't automatically
				// make it our new target.

                if ((spellTarget != null && spellTarget.IsAlive) || spell.Target.ToLower() == "self")
				{
					Body.TargetObject = spellTarget;

					if (spellTarget != Body)
						Body.TurnTo(spellTarget);

					Body.CastSpell(spell, spellQueueEntry.SpellLine);

					if (previousTarget != null)
						Body.TargetObject = previousTarget;
				}
				else
				{
                    DebugMessageToOwner(String.Format("Invalid target for spell '{0}', removing it...",
                        m_spellQueue.Peek().Spell.Name));

					RemoveSpellFromQueue();
				}
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
                    DebugMessageToOwner(String.Format("Grabbing spell '{0}' from the start of the queue in order to cast it",
                        m_spellQueue.Peek().Spell.Name));

                    return m_spellQueue.Peek();
                }
			}
			return null;
		}

		/// <summary>
		/// Whether or not any spells are queued.
		/// </summary>
		private bool SpellsQueued
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
                    DebugMessageToOwner(String.Format("Removing spell '{0}' from the start of the queue",
                        m_spellQueue.Peek().Spell.Name));

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
                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client, 
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

                    (Owner as GameNecromancer).SetTetherTimer(-1);
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
		private void MessageToOwner(String message, eChatType chatType)
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
#if DEBUG
            int tick = Environment.TickCount;
            int seconds = tick / 1000;
            int minutes = seconds / 60;

            MessageToOwner(String.Format("[{0:00}:{1:00}.{2:0000}] {3}", 
                minutes % 60, seconds % 60, tick % 1000, message), eChatType.CT_Guild);
#endif
        }

		#endregion
	}
}
