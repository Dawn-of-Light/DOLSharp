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
	/// The necromancer character class.
	/// </summary>
	public class CharacterClassNecromancer : CharacterClassBase
	{
		public override void Init(GamePlayer player)
		{
			base.Init(player);

			// Force caster form when creating this player in the world.
			player.Model = (ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;
			player.Shade(false);
		}

		
		//private String m_petName = "";
		private int m_savedPetHealthPercent = 0;

		/// <summary>
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public override void SetControlledBrain(IControlledBrain controlledNpcBrain)
		{
			m_savedPetHealthPercent = (Player.ControlledBrain != null)
				? (int)Player.ControlledBrain.Body.HealthPercent : 0;

			base.SetControlledBrain(controlledNpcBrain);

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
			m_savedPetHealthPercent = (Player.ControlledBrain != null) ? (int)Player.ControlledBrain.Body.HealthPercent : 0;

			base.CommandNpcRelease();
			OnPetReleased();
		}

		/// <summary>
		/// Invoked when pet is released.
		/// </summary>
		public override void OnPetReleased()
		{
			if (Player.IsShade)
				Player.Shade(false);

			Player.InitControlledBrainArray(0);
		}

		/// <summary>
		/// Necromancer can only attack when it's not a shade.
		/// </summary>
		/// <param name="attackTarget"></param>
		public override bool StartAttack(GameObject attackTarget)
		{
			if (!Player.IsShade)
			{
				return true;
			}
			else
			{
				Player.Out.SendMessage("You cannot enter combat while in shade form!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return false;
			}
		}

		/// <summary>
		/// If the pet is up, show the pet's health in the group window.
		/// </summary>
		public override byte HealthPercentGroupWindow
		{
			get
			{
				if (Player.ControlledBrain == null) 
					return Player.HealthPercentGroupWindow;

				return Player.ControlledBrain.Body.HealthPercent;
			}
		}

		/// <summary>
		/// Create a necromancer shade effect for this player.
		/// </summary>
		/// <returns></returns>
		public override ShadeEffect CreateShadeEffect()
		{
			return new NecromancerShadeEffect();
		}

		/// <summary>
		/// Changes shade state of the player
		/// </summary>
		/// <param name="state">The new state</param>
		public override void Shade(bool state)
		{
			bool wasShade = Player.IsShade;
			base.Shade(state);

			if (wasShade == state)
				return;

			if (state)
			{
				// Necromancer has become a shade. Have any previous NPC 
				// attackers aggro on pet now, as they can't attack the 
				// necromancer any longer.

				if (Player.ControlledBrain != null && Player.ControlledBrain.Body != null)
				{
					GameNPC pet = Player.ControlledBrain.Body;
					ArrayList attackerList = (ArrayList)((ArrayList)Player.Attackers).Clone();

					if (pet != null)
					{
						foreach (GameObject obj in attackerList)
						{
							if (obj is GameNPC)
							{
								GameNPC npc = (GameNPC)obj;
								if (npc.TargetObject == Player && npc.AttackState)
								{
									IOldAggressiveBrain brain = npc.Brain as IOldAggressiveBrain;
									if (brain != null)
									{
										(npc).AddAttacker(pet);
										npc.StopAttack();
										brain.AddToAggroList(pet, (int)(brain.GetAggroAmountForLiving(Player) + 1));
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

				if (Player.ControlledBrain != null)
					(Player.ControlledBrain as ControlledNpcBrain).Stop();

				Player.Health = Math.Min(Player.Health, Player.MaxHealth * Math.Max(10, m_savedPetHealthPercent) / 100);
			}
		}

		/// <summary>
		/// Called when player is removed from world.
		/// </summary>
		/// <returns></returns>
		public override bool RemoveFromWorld()
		{
			// Force caster form.

			if (Player.IsShade)
				Player.Shade(false);

			return base.RemoveFromWorld();
		}

        /// <summary>
        /// Drop shade first, this in turn will release the pet.
        /// </summary>
        /// <param name="killer"></param>
        public override void Die(GameObject killer)
        {
            Player.Shade(false);

            base.Die(killer);
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (Player.ControlledBrain != null)
            {
				GameNPC pet = Player.ControlledBrain.Body;

                if (pet != null && sender == pet && e == GameLivingEvent.CastStarting && args is CastingEventArgs)
                {
                    ISpellHandler spellHandler = (args as CastingEventArgs).SpellHandler;

                    if (spellHandler != null)
                    {
                        int powerCost = spellHandler.PowerCost(Player);

                        if (powerCost > 0)
							Player.ChangeMana(Player, GameLiving.eManaChangeType.Spell, -powerCost);
                    }

                    return;
                }
            }

            base.Notify(e, sender, args);
        }
	}
}
