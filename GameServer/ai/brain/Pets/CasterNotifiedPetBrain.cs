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
	/// Pet brain that can Handle Pet Spell casting through Event Notification
	/// Will use a queue for requested casted spells.
	/// </summary>
	public class CasterNotifiedPetBrain : ControlledNpcBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public CasterNotifiedPetBrain(GameLiving owner) 
			: base(owner)
		{
		}
		
		/// <summary>
		/// Spell Queue for this Brain
		/// Should support 2 spell enqueued by defaut
		/// </summary>
		protected ConcurrentQueue<Tuple<Spell, SpellLine, GameObject>> m_spellQueue = new ConcurrentQueue<Tuple<Spell, SpellLine, GameObject>>();
		
		/// <summary>
		/// Are spells Queued
		/// </summary>
		protected virtual bool SpellsQueued
		{
			get
			{
				return !m_spellQueue.IsEmpty;
			}
			
			set
			{
			}
		}
		
		protected virtual byte MaxSpellQueueCount
		{
			get
			{
				return 5;
			}
			set
			{
			}
		}
		
		/// <summary>
		/// Check Spell queue and cast any spell in there
		/// </summary>
		protected virtual void CheckSpellQueue()
		{
			Tuple<Spell, SpellLine, GameObject> spellQueueEntry = PeekSpellQueue();
			if (spellQueueEntry != null)
			{
				GameObject previousTarget = Body.TargetObject;
                GameObject spellTarget = spellQueueEntry.Item3;
                Spell spell = spellQueueEntry.Item1;
                SpellLine line = spellQueueEntry.Item2;

				// Cast spell on the target, but don't automatically
				// make it our new target.

				// Target must be alive, or this is a self spell, or this is a pbaoe spell
				if (spell.Target.ToLower() == "self" || spell.Range == 0 || (spellTarget != null && (spellTarget is GameLiving && ((GameLiving)spellTarget).IsAlive)))
				{
					Body.TargetObject = spellTarget;

					Body.CastSpell(spell, line);

					if (previousTarget != null)
						Body.TargetObject = previousTarget;
				}
				else
				{
					DequeueSpell();
				}
			}
		}
		
		/// <summary>
		/// Add Spell to the queue
		/// </summary>
		protected virtual void AddSpellToQueue(Spell spellToCast, SpellLine lineToCast, GameObject target)
		{
			if(m_spellQueue.Count >= MaxSpellQueueCount) 
			{
				Tuple<Spell, SpellLine, GameObject> spellDequeued;
				if(m_spellQueue.TryDequeue(out spellDequeued))
					if(Owner is GamePlayer)
						Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
                    	    "AI.Brain.Necromancer.SpellNoLongerInQueue", spellDequeued.Item1.Name, Body.Name), eChatType.CT_Spell);
			}
			
			m_spellQueue.Enqueue(new Tuple<Spell, SpellLine, GameObject>(spellToCast, lineToCast, target));
		}
		
		/// <summary>
		/// Remove the last Spell from Queue. (Dequeue)
		/// </summary>
		/// <returns></returns>
		protected virtual bool DequeueSpell()
		{
			if(SpellsQueued)
			{
				Tuple<Spell, SpellLine, GameObject> queuedSpell;
				return m_spellQueue.TryDequeue(out queuedSpell);
			}
			
			return false;
		}
		
		protected virtual Tuple<Spell, SpellLine, GameObject> PeekSpellQueue()
		{
			Tuple<Spell, SpellLine, GameObject> peekSpell;
			if(SpellsQueued && m_spellQueue.TryPeek(out peekSpell))
				return peekSpell;
			
			return null;
		}
				
		
		/// <summary>
		/// Override Think to empty the spell Queue
		/// </summary>
		public override void Think()
		{
			if(IsActive)
			{
				// if spells are queued then handle them first
				if (SpellsQueued)
				{
					if (!Body.IsCasting)
					{
						CheckSpellQueue();
					}
				}
			}
			
			base.Think();
		}
		
		
		/// <summary>
		/// Handle PetSpell Event Notifications
		/// </summary>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			
			// Receive a Casting Order from Owner
			if (sender == Owner && e == GameNPCEvent.PetSpell)
			{
				// Pet cast Required !
				PetSpellEventArgs petSpell = (PetSpellEventArgs)args;
				bool hadQueuedSpells = false;

				if (SpellsQueued)
				{
					if(Owner is GamePlayer)
						Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AI.Brain.Necromancer.CastSpellAfterAction", Body.Name), eChatType.CT_System);
					
					hadQueuedSpells = true;
				}
				
				AddSpellToQueue(petSpell.Spell, petSpell.SpellLine, petSpell.Target);

				// immediate casts are ok if we're not doing anything else or if it's instant and queue is empty.
				if (!hadQueuedSpells && ((!Body.AttackState && !Body.IsCasting) || petSpell.Spell.IsInstantCast))
				{
					CheckSpellQueue();
				}
			}
			
			// Received Notification from Body
			if (sender == Body)
			{
	            if (e == GameLivingEvent.CastStarting)
	            {
	                // The spell will cast.
	
	                CastingEventArgs spellArgs = args as CastingEventArgs;
	                
	                if(spellArgs == null || Owner == null || spellArgs.SpellHandler == null)
	                	return;
	                
	                GameLiving target = spellArgs.Target;
	                SpellLine spellLine = spellArgs.SpellHandler.SpellLine;
	
					// Match Owner SpellLine, means it's a notified spell.
					if(Owner is GamePlayer)
					{
						if(((GamePlayer)Owner).GetSpellLines().Contains(spellLine))
						{
							// Consume Mana
							int powerCost = spellArgs.SpellHandler.PowerCost(Owner, true);
                       		if (powerCost > 0)
								Owner.ChangeMana(Body, GameLiving.eManaChangeType.Spell, -powerCost);
                       		
							// Notify and Message
							Owner.Notify(GameLivingEvent.CastStarting, Body, new CastingEventArgs(spellArgs.SpellHandler));
							
	       					if(Owner is GamePlayer)
								Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AI.Brain.Necromancer.PetCastingSpell", Body.Name), eChatType.CT_System);
	
						}
					}
					else
					{
						// Notify Only
						Owner.Notify(GameLivingEvent.CastStarting, Body, new CastingEventArgs(Body.CurrentSpellHandler));
					}
	            }
				else if (e == GameLivingEvent.CastFinished)
	            {
					Owner.Notify(GamePlayerEvent.CastFinished, Owner, args);
					// Pet Cast Finished
					DequeueSpell();
				}
	            else if (e == GameLivingEvent.CastFailed)
	            {
  	               	// Pet Cast did not finish
					DequeueSpell();
					
					if(args == null)
						return;
	
	                switch ((args as CastFailedEventArgs).Reason)
	                {
	                    case CastFailedEventArgs.Reasons.TargetTooFarAway:
	                		if(Owner is GamePlayer)
	                        	Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
	                            	"AI.Brain.Necromancer.ServantFarAwayToCast"), eChatType.CT_SpellResisted);
	                        
	                		break;
	
	                    case CastFailedEventArgs.Reasons.TargetNotInView:
	                		if(Owner is GamePlayer)
		                        Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, 
	                            "AI.Brain.Necromancer.PetCantSeeTarget", Body.Name), eChatType.CT_SpellResisted);
	                        
	                		break;
	
						case CastFailedEventArgs.Reasons.NotEnoughPower:	
	                		if(Owner is GamePlayer)
								Owner.MessageFromControlled(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
								"AI.Brain.Necromancer.NoPower", Body.Name), eChatType.CT_SpellResisted);
							break;
	                }
	            }			
				else if (e == GameLivingEvent.PetReleased)
				{
					// Pet release, empty the queue
					m_spellQueue = new ConcurrentQueue<Tuple<Spell, SpellLine, GameObject>>();
				}
			}
		}
	}
}
