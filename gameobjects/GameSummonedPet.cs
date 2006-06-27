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
using DOL.GS;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Represents a summoned game pet
	/// </summary>
	public class GameSummonedPet : GameMob
	{	
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new GameSummonedPet based on NPC template
		/// </summary>
		/// <param name="template"></param>
		public GameSummonedPet(INpcTemplate template) : base(template)
		{

		}

		/// <summary>
		/// Tests if this pet should give XP and loot based on the XPGainers
		/// </summary>
		/// <returns>always false</returns>
		public override bool IsWorthReward
		{
			get { return false; }
			set { }
		}

		/// <summary>
		/// Gets/sets the maximum amount of mana
		/// </summary>
		public override int MaxMana
		{
			get { return 1000; }
		}

		/// <summary>
		/// Starts the Respawn Timer
		/// </summary>
		public override void StartRespawn()
		{
		}
	}
}
