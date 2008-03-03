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
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for quckcast ability
	/// </summary>
	public class QuickCastEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "This allows them to cast a single spell with a quickened casting time, but costs twice the power. It cannot be interrupted by melee or spells. The only exception to this is the Necromancer form of Quickcast (Facilitate Painworking). It allows the Necromancer's summoned undead pet to have six seconds of uninterrupted casting time.";

		/// <summary>
		/// Start the quickcast on player
		/// </summary>
		public override void Start(GameLiving living)
		{
			base.Start(living);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage("You have activated quickcast.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_owner.TempProperties.removeProperty(Spells.SpellHandler.INTERRUPT_TIMEOUT_PROPERTY);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			base.Cancel(playerCancel);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage("Your next spell will not be quickcasted.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return "QuickCast"; } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime { get { return 0; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 0x0190; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(1);
				delveInfoList.Add(delveString);
				return delveInfoList;
			}
		}
	}
}
