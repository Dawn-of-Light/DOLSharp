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
using System.Reflection;
using DOL.AI.Brain;
using log4net;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Sends updates only for changed effects
	/// when iterating over this effect list lock the list!
	/// </summary>
	public class GameEffectPlayerList : GameEffectList
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the list of changed effects
		/// </summary>
		protected readonly ArrayList m_changedEffects = new ArrayList();
		/// <summary>
		/// The count of effects on last update
		/// </summary>
		protected int m_lastUpdateEffectsCount;

		/// <summary>
		/// Constructs a new GameEffectPlayerList
		/// </summary>
		/// <param name="owner">The owner of effect list</param>
		public GameEffectPlayerList(GamePlayer owner) : base(owner)
		{
		}

		/// <summary>
		/// Called when an effect changed
		/// </summary>
		public override void OnEffectsChanged(IGameEffect changedEffect)
		{
			if (changedEffect.Icon == 0)
				return;
			lock (this)
			{
				if (!m_changedEffects.Contains(changedEffect))
					m_changedEffects.Add(changedEffect);
			}
			base.OnEffectsChanged(changedEffect);
		}

		/// <summary>
		/// add a new effect to the effectlist, it does not start the effect
		/// </summary>
		/// <param name="effect">The effect to add to the list</param>
		/// <returns>true if the effect was added</returns>
		public override bool Add(IGameEffect effect)
		{
			if (!base.Add(effect)) return false;

			// no timer has to be updated like with top icons so it only
			// makes sence to update group window icons on add/remove
			GamePlayer player = m_owner as GamePlayer;
			if (player != null && player.PlayerGroup != null)
			{
				player.PlayerGroup.UpdateMember(player, true, false);
			}

			return true;
		}

		/// <summary>
		/// remove effect
		/// </summary>
		/// <param name="effect">The effect to remove from the list</param>
		/// <returns>true if the effect was removed</returns>
		public override bool Remove(IGameEffect effect)
		{
			if (!base.Remove(effect)) return false;

			// no timer has to be updated like with top icons so it only
			// makes sence to update group window icons on add/remove
			GamePlayer player = m_owner as GamePlayer;
			if (player != null && player.PlayerGroup != null)
			{
				player.PlayerGroup.UpdateMember(player, true, false);
			}
			return true;
		}

		/// <summary>
		/// Updates changed effects to the owner.
		/// </summary>
		protected override void UpdateChangedEffects()
		{
			((GamePlayer)m_owner).Out.SendUpdateIcons(m_changedEffects, ref m_lastUpdateEffectsCount);
		}

	}
}
