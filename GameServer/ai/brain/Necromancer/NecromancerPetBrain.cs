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
using System.Reflection;
using System.Collections.Generic;
using log4net;

using DOL.GS;
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
	public class NecromancerPetBrain : CasterNotifiedPetBrain
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

                if (playerowner != null && !playerowner.CurrentUpdateArray[Body.ObjectID - 1])
				{
					playerowner.Out.SendObjectUpdate(Body);
					playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
				}

				if (SpellsQueued)
				{
					// if spells are queued then handle them first

					if (!Body.IsCasting)
					{
						CheckSpellQueue();
					}
				}
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

			if(sender == Body)
			{
	            if (e == GameLivingEvent.Dying)
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
	                // Removing the spell is done in subclass
	
					AttackMostWanted();
	
	                if (SpellsQueued)
	                {
	                    DebugMessageToOwner("+ Cast finished, more spells to cast");
					}
	                else
	                {
	                    DebugMessageToOwner("- Cast finished, no more spells to cast");
	                }
	
					if (SpellsQueued && Body.CurrentRegion.Time - Body.LastAttackedByEnemyTick > 5 * 1000)
					{
						// there are more, keep casting.
						CheckSpellQueue();
					}
	
	            }
	            else if (e == GameLivingEvent.CastStarting)
	            {
	                // The spell will cast.
	
	                CastingEventArgs spellArgs = args as CastingEventArgs;
	                
	                if(spellArgs == null)
	                	return;
	                
	                GameLiving target = spellArgs.Target;
	
					if (spellArgs != null && spellArgs.SpellHandler.Spell != null)
						DebugMessageToOwner(String.Format("Now casting '{0}'", spellArgs.SpellHandler.Spell.Name));
	
	                // If pet is casting an offensive spell and is not set to
	                // passive, put target on its aggro list; that way, even with
	                // no attack directive from the owner it will start an attack
	                // after the cast has finished.
	
	                if (target != Body && spellArgs.SpellHandler.Spell.Target.ToLower() == "enemy")
	                {
	                    if (target != null)
	                    {
	                        if (!Body.AttackState && AggressionState != eAggressionState.Passive)
	                        {
	                            Body.DrawWeapon();
	                            AddToAggroList(target, 1);
	                        }
	                    }
	                }
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
	                        "AI.Brain.Necromancer.PetTooFarBeLostSecIm", secondsRemaining), eChatType.CT_SpellPulse);
	                else if (secondsRemaining == 5)
	                    MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
	                        "AI.Brain.Necromancer.PetTooFarBeLostSec", secondsRemaining), eChatType.CT_Important);
	            }
	            else if (e == GameNPCEvent.PetLost)
	            {
	                // Pet despawn is imminent, notify owner.
	
	                MessageToOwner(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language,
	                    "AI.Brain.Necromancer.HaveLostBondToPet"), eChatType.CT_SpellExpires);
	            }
			}
            
            if (e == GameNPCEvent.SwitchedTarget && sender == Body.TargetObject &&
                sender is GameNPC && !(sender as GameNPC).IsCrowdControlled)
            {
                // Target has started attacking someone else.

				if (Body.EffectList.GetOfType<TauntEffect>() != null)
                    (Body as NecromancerPet).Taunt();            
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
				List<IGameEffect> effectList = new List<IGameEffect>(1);
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
		protected override byte MaxSpellQueueCount
		{
			get
			{
				return 2;
			}
			set
			{
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
                    m_tetherTimer.Interval = 1000;
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
                    m_pet.Brain.Notify(GameNPCEvent.OutOfTetherRange, m_pet, 
						new TetherEventArgs(m_seconds));
                    m_seconds -= 1;
                }
                else
                {
                    Stop();
                    m_pet.Brain.Notify(GameNPCEvent.PetLost, m_pet, null);
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
