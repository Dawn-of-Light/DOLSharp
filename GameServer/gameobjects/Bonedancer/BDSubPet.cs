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
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Database;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Styles;

namespace DOL.GS
{
	public class BDSubPet : BDPet
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the different subpet ids
		/// </summary>
		public enum SubPetType : byte
		{
			Melee = 0,
			Healer = 1,
			Caster = 2,
			Debuffer = 3,
			Buffer = 4,
			Archer = 5
		}

		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public BDSubPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override short MaxSpeed
		{
			get
			{
				return (Brain as IControlledBrain).Owner.MaxSpeed;
			}
		}
	}
}