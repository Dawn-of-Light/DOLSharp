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
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper effect for sure shot
	/// </summary>
	public class RapidFireEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// Start the effect on player
		/// </summary>
		/// <param name="living">The effect target</param>
		public override void Start(GameLiving living)
		{
			base.Start(living);
			if (living is GamePlayer)
				(living as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((living as GamePlayer).Client, "Effects.RapidFireEffect.YouSwitchRFMode"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.RapidFireEffect.Name"); } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 484; } }
	}
}