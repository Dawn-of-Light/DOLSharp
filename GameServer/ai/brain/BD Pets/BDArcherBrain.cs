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
	public class BDArcherBrain : BDPetBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public BDArcherBrain(GameLiving owner) : base(owner) { }

		#region AI

		/// <summary>
		/// No Abilities or spells
		/// </summary>
		public override void CheckAbilities() { }
		protected override bool CheckDefensiveSpells(Spell spell) { return false; }
		protected override bool CheckOffensiveSpells(Spell spell) { return false; }
		protected override bool CheckInstantSpells(Spell spell) { return false; }

		public override void Attack(GameObject target)
		{
			Body.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
			base.Attack(target);
		}

		#endregion
	}
}
