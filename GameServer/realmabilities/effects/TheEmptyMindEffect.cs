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
using DOL.GS.SkillHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The Empty Mind Effect
	/// </summary>
	public class TheEmptyMindEffect : TimedEffect, IGameEffect
	{
		/// <summary>
		/// Constructs a new Empty Mind Effect
		/// </summary>
		public TheEmptyMindEffect()
			: base(RealmAbilities.TheEmptyMindAbility.m_duration)
		{
		}

		/// <summary>
		/// Starts the effect on the living
		/// </summary>
		/// <param name="living"></param>
		public override void Start(GameLiving living)
		{
			foreach (GamePlayer visiblePlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visiblePlayer.Out.SendSpellEffectAnimation(living, living, 1197, 0, false, 1);
			}

			base.Start(living);
		}
		
		public override void Stop()
		{
			base.Stop();
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage("Your clearheaded state leaves you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon
		{
			get { return 7008; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add("Grants the user 45 seconds of increased resistances to all magical damage by the percentage listed.");
				foreach (string str in base.DelveInfo)
					delveInfoList.Add(str);

				return delveInfoList;
			}
		}
	}
}
