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
 *///make by DeMAN
using System;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the stealth ability
	/// </summary>
	public class CamouflageEffect : IGameEffect
	{
		/// <summary>
		/// Information of effect
		/// </summary>
		protected const String delveString = "Available to the archer classes. This ability will make the archer undectable to the Mastery of Stealth Realm Ability for a period of time. Drawing a weapon or engaging in combat will cancel the effect and cause the player to be visible. This does not modify stealth skill.";
		/// <summary>
		/// The owner of the effect
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// The internal unique effect ID
		/// </summary>
		ushort m_id;

		/// <summary>
		/// Creates a new stealth effect
		/// </summary>
		public CamouflageEffect() { }

		/// <summary>
		/// Start the stealth on player
		/// </summary>
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
			m_player.EffectList.Remove(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			Stop();
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name { get { return "Camouflage"; } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public int RemainingTime { get { return 0; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon { get { return 476; } }

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID { get { return m_id; } set { m_id = value; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public IList DelveInfo
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
