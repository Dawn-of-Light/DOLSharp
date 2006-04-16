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

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper effect for player shade mode
	/// </summary>
	public class ShadeEffect : IGameEffect
	{
		/// <summary>
		/// The effect owner
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// Creates a new stealth effect
		/// </summary>
		public ShadeEffect()
		{
		}

		/// <summary>
		/// Start the shade effect on player
		/// </summary>
		/// <param name="player">The effect target</param>
		public void Start(GamePlayer player)
		{
			m_player = player;
			player.EffectList.Add(this);
		}

		/// <summary>
		/// Stop the effect on target
		/// </summary>
		public void Stop()
		{
//			m_player.Out.SendMessage("You Stop effect.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_player.EffectList.Remove(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel) {
//			m_player.Out.SendMessage("You Cancel effect.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_player.Shade(false);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name { get { return "Shade"; } }

		/// <summary>
		/// Remaining Time of the effect in seconds
		/// </summary>
		public int RemainingTime { get { return 0; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon { get { return 0x193; } }

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		private ushort m_id;

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public IList DelveInfo { get { return new ArrayList(0); } }
	}
}
