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
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.Database;
using log4net;
using DOL.Events;
using DOL.GS.Effects;
using System.Collections;
using DOL.AI.Brain;

namespace DOL.GS
{
	/// <summary>
	/// The necromancer player class.
	/// </summary>
	/// <author>Aredhel</author>	
	class GameNecromancer : GamePlayer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Pet Control

		//private String m_petName = "";
		private int m_savedPetHealthPercent = 0;

		/// <summary>
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public override void SetControlledNpcBrain(IControlledBrain controlledNpcBrain)
		{
			m_savedPetHealthPercent = (ControlledNpcBrain != null)
				? (int)ControlledNpcBrain.Body.HealthPercent : 0;

			base.SetControlledNpcBrain(controlledNpcBrain);
			if (controlledNpcBrain == null)
			{
				OnPetReleased();
			}
		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		public override void CommandNpcRelease()
		{
			m_savedPetHealthPercent = (ControlledNpcBrain != null)
			? (int)ControlledNpcBrain.Body.HealthPercent : 0;

			base.CommandNpcRelease();
			OnPetReleased();
		}

		/// <summary>
		/// Invoked when pet is released.
		/// </summary>
		protected virtual void OnPetReleased()
		{
			if (IsShade)
				Shade(false);
		}

		#endregion

		/// <summary>
		/// Necromancer can only attack when it's not a shade.
		/// </summary>
		/// <param name="attackTarget"></param>
		public override void StartAttack(GameObject attackTarget)
		{
			if (!IsShade)
				base.StartAttack(attackTarget);
			else
				Out.SendMessage("You cannot enter combat while in shade form!",
				    eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// If the pet is up, show the pet's health in the group window.
		/// </summary>
		public override byte HealthPercentGroupWindow
		{
			get
			{
				if (ControlledNpcBrain == null)
					return base.HealthPercentGroupWindow;
				return ControlledNpcBrain.Body.HealthPercent;
			}
		}

		/// <summary>
		/// Set the tether timer if pet gets out of range or comes back into range.
		/// </summary>
		/// <param name="seconds"></param>
		public void SetTetherTimer(int seconds)
		{
			NecromancerShadeEffect shadeEffect =
				EffectList.GetOfType(typeof(NecromancerShadeEffect)) as NecromancerShadeEffect;

			if (shadeEffect != null)
			{
				lock (shadeEffect)
					shadeEffect.SetTetherTimer(seconds);
				ArrayList effectList = new ArrayList(1);
				effectList.Add(shadeEffect);
				int effectsCount = 1;
				Out.SendUpdateIcons(effectList, ref effectsCount);
			}
		}

		/// <summary>
		/// Create a necromancer shade effect for this player.
		/// </summary>
		/// <returns></returns>
		protected override ShadeEffect CreateShadeEffect()
		{
			return new NecromancerShadeEffect();
		}

		/// <summary>
		/// Changes shade state of the player
		/// </summary>
		/// <param name="state">The new state</param>
		public override void Shade(bool state)
		{
			bool wasShade = IsShade;
			base.Shade(state);

			if (wasShade == state)
				return;

			if (state)
			{
				// Necromancer has become a shade. Have any previous NPC 
				// attackers aggro on pet now, as they can't attack the 
				// necromancer any longer.

				if (ControlledNpcBrain != null && ControlledNpcBrain.Body != null)
				{
					GameNPC pet = ControlledNpcBrain.Body;
					ArrayList attackerList = (ArrayList)m_attackers.Clone();

					if (pet != null)
					{
						foreach (GameObject obj in attackerList)
						{
							if (obj is GameNPC)
							{
								GameNPC npc = (GameNPC)obj;
								if (npc.TargetObject == this && npc.AttackState)
								{
									IOldAggressiveBrain brain = npc.Brain as IOldAggressiveBrain;
									if (brain != null)
									{
										(npc).AddAttacker(pet);
										npc.StopAttack();
										brain.AddToAggroList(pet, (int)(brain.GetAggroAmountForLiving(this) + 1));
									}
								}
							}
						}
					}
				}
			}
			else
			{
				// Necromancer has lost shade form, release the pet if it
				// isn't dead already and update necromancer's current health.

				if (ControlledNpcBrain != null)
					(ControlledNpcBrain as ControlledNpcBrain).Stop();

				Health = Math.Min(Health, MaxHealth * Math.Max(10, m_savedPetHealthPercent) / 100);
			}
		}

		/// <summary>
		/// Called when player is removed from world.
		/// </summary>
		/// <returns></returns>
		public override bool RemoveFromWorld()
		{
			// Force caster form.

			if (IsShade)
				Shade(false);

			return base.RemoveFromWorld();
		}

        /// <summary>
        /// Drop shade first, this in turn will release the pet.
        /// </summary>
        /// <param name="killer"></param>
        public override void Die(GameObject killer)
        {
            Shade(false);

            base.Die(killer);
        }

		public GameNecromancer(GameClient client, Character theChar)
			: base(client, theChar)
		{
			// Force caster form when creating this player in the world.

			Model = (ushort)client.Account.Characters[m_client.ActiveCharIndex].CreationModel;
			Shade(false);
		}

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (ControlledNpcBrain != null)
            {
                GameNPC pet = ControlledNpcBrain.Body;

                if (pet != null && sender == pet && e == GameLivingEvent.CastStarting &&
                    args is CastStartingEventArgs)
                {
                    ISpellHandler spellHandler = (args as CastStartingEventArgs).SpellHandler;

                    if (spellHandler != null)
                    {
                        int powerCost = spellHandler.PowerCost(this);

                        if (powerCost > 0)
                            ChangeMana(this, eManaChangeType.Spell, -powerCost);
                    }

                    return;
                }
            }

            base.Notify(e, sender, args);
        }
	}
}
