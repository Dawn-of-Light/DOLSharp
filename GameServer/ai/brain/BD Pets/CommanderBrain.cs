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

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain for the commanders
	/// </summary>
	public class CommanderBrain : ControlledNpcBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public CommanderBrain(GameLiving owner)
			: base(owner)
		{
		}

		public bool MinionsAssisting
		{
			get { return Body is CommanderPet commander && commander.MinionsAssisting; }
		}

		/// <summary>
		/// Determines if a given controlled brain is part of the commanders subpets
		/// </summary>
		/// <param name="brain">The brain to check</param>
		/// <returns>True if found, else false</returns>
		public bool FindPet(IControlledBrain brain)
		{
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (IControlledBrain icb in Body.ControlledNpcList)
						if (brain == icb)
							return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Attack the target on command
		/// </summary>
		/// <param name="target">The target to attack</param>
		public override void Attack(GameObject target)
		{
			base.Attack(target);
			//Check for any abilities
			CheckAbilities();

			if (MinionsAssisting && Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (BDPetBrain icb in Body.ControlledNpcList)
						if (icb != null)
							icb.Attack(target);
				}
			}
		}
		
		/// <summary>
		/// Defend a minion that is being attacked
		/// </summary>
		/// <param name="ad"></param>
		public void DefendMinion(GameLiving attacker)
		{
			AddToAggroList(attacker, 1);
			AttackMostWanted();
		}

		/// <summary>
		/// Make sure the subpets are following the commander
		/// </summary>
		/// <param name="target"></param>
		public override void Follow(GameObject target)
		{
			base.Follow(target);
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		private void SubpetFollow()
		{
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (BDPetBrain icb in Body.ControlledNpcList)
						if (icb != null)
							icb.FollowOwner();
				}
			}
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		public override void Stay()
		{
			base.Stay();
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		public override void ComeHere()
		{
			base.ComeHere();
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		/// <param name="target"></param>
		public override void Goto(GameObject target)
		{
			base.Goto(target);
			SubpetFollow();
		}

		public override void SetAggressionState(eAggressionState state)
		{
			base.SetAggressionState(state);
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (BDPetBrain icb in Body.ControlledNpcList)
						if (icb != null)
							icb.SetAggressionState(state);
				}
			}
		}

		/// <summary>
		/// Checks if any spells need casting
		/// </summary>
		/// <param name="type">Which type should we go through and check for?</param>
		/// <returns></returns>
		public override bool CheckSpells(eCheckSpellType type)
		{
			bool casted = false;

			if (type == eCheckSpellType.Offensive && Body is CommanderPet pet
				&& pet.PreferredSpell != CommanderPet.eCommanderPreferredSpell.None
				&& !pet.IsCasting && !pet.IsBeingInterrupted && pet.TargetObject is GameLiving living && living.IsAlive)

			{
				Spell spellDamage = pet.CommSpellDamage;
				Spell spellDamageDebuff = pet.CommSpellDamageDebuff;
				Spell spellDot = pet.CommSpellDot;
				Spell spellDebuff = pet.CommSpellDebuff;
				Spell spellOther = pet.CommSpellOther;

				Spell cast = null;
				switch (pet.PreferredSpell)
				{
					case CommanderPet.eCommanderPreferredSpell.Debuff:
						if (spellDebuff != null && !living.HasEffect(spellDebuff))
							cast = spellDebuff;
						break;
					case CommanderPet.eCommanderPreferredSpell.Other:
						cast = spellOther;
						break;
				}

				if (cast == null)
				{
					// Pick a damage spell
					if (spellDot != null && !living.HasEffect(spellDot))
						cast = spellDot;
					else if (spellDamageDebuff != null && (!living.HasEffect(spellDamageDebuff) || spellDamage == null))
						cast = spellDamageDebuff;
					else if (spellDamage != null)
						cast = spellDamage;
				}

				if (cast != null)
					casted = CheckOffensiveSpells(cast);
			}

			if(casted)
			{
				// Check instant spells, but only cast one to prevent spamming
				if (Body.CanCastInstantHarmfulSpells)
					foreach (Spell spell in Body.InstantHarmfulSpells)
						if (CheckOffensiveSpells(spell))
							break;
			}
			else
				// Only call base method if we didn't cast anything, 
				//	otherwise it tries to cast a second offensive spell
				casted = base.CheckSpells(type);

			return casted;
		}
	}
}
