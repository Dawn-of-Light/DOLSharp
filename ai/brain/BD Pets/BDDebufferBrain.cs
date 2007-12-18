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
	public class BDDebufferBrain : BDPetBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public BDDebufferBrain(GameLiving owner) : base(owner) { }

		/// <summary>
		/// The interval for thinking, 1.5 seconds
		/// </summary>
		public override int ThinkInterval
		{
			get { return 1500; }
		}

		#region AI

		/// <summary>
		/// Starts the brain thinking and resets the inactivity countdown
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			if (base.Start()) return false;

			GameEventMgr.AddHandler(((IControlledBrain)((GameNPC)Owner).Brain).Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));

			return true;
		}

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			base.Think();

			//Check for buffs, heals, etc
			CheckSpells(eCheckSpellType.Defensive);

			if (AggressionState == eAggressionState.Aggressive)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
				AttackMostWanted();
			}
		}

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public override void CheckAbilities() { }

		/// <summary>
		/// Checks the Positive Spells.  Handles buffs, heals, etc.
		/// </summary>
		protected override bool CheckDefensiveSpells(Spell spell) { return false; }

		/// <summary>
		/// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
		/// </summary>
		protected override bool CheckInstantSpells(Spell spell)
		{
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			switch (spell.SpellType)
			{
				case "CombatSpeedDebuff":
					Body.TargetObject = lastTarget;
					break;
			}

			if (Body.TargetObject != null)
			{
				if (LivingHasEffect((GameLiving)Body.TargetObject, spell))
					return false;
				Body.CastSpell(spell, m_mobSpellLine);
				Body.TargetObject = lastTarget;
				return true;
			}
			Body.TargetObject = lastTarget;
			return false;
		}

		#endregion
	}
}
