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
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain that can be controlled
	/// </summary>
	public class BDBufferBrain : BDPetBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public BDBufferBrain(GameLiving owner) : base(owner) { }

		#region AI

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public override void CheckAbilities() { }

		/// <summary>
		/// Checks the Positive Spells.  Handles buffs, heals, etc.
		/// </summary>
		protected override bool CheckDefensiveSpells(Spell spell)
		{
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			GamePlayer player = null;
			GameLiving owner = null;
			switch (spell.SpellType)
			{
				#region Buffs
				case "CombatSpeedBuff":
				case "DamageShield":
				case "Bladeturn":
					{
						if (!Body.IsAttacking)
						{
							//Buff self
							if (!LivingHasEffect(Body, spell))
							{
								Body.TargetObject = Body;
								break;
							}

							if (spell.Target != "Self")
							{

								owner = (this as IControlledBrain).Owner;

								//Buff owner
								if (owner != null)
								{
									if (!LivingHasEffect(owner, spell))
									{
										Body.TargetObject = owner;
										break;
									}

									//Buff other minions
									foreach (IControlledBrain icb in ((GameNPC)owner).ControlledNpcList)
									{
										if (icb == null)
											continue;
										if (!LivingHasEffect(icb.Body, spell))
										{
											Body.TargetObject = icb.Body;
											break;
										}
									}

									player = GetPlayerOwner();

									//Buff player
									if (player != null)
									{
										if (!LivingHasEffect(player, spell))
										{
											Body.TargetObject = player;
											break;
										}
									}
								}
							}
						}
						break;
					}
				#endregion
			}
			if (Body.TargetObject != null)
			{
				if (Body.IsMoving)
					Body.StopFollow();
				Body.TurnTo(Body.TargetObject);
				Body.CastSpell(spell, m_mobSpellLine);
				Body.TargetObject = lastTarget;
				return true;
			}
			Body.TargetObject = lastTarget;
			return base.CheckDefensiveSpells(spell); ;
		}

		/// <summary>
		/// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
		/// </summary>
		protected override bool CheckInstantSpells(Spell spell) { return false; }

		#endregion
	}
}
