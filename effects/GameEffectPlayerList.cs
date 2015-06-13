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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		/// Lock object for Change Update
		/// </summary>
		private readonly object m_changedLock = new object();		
		/// <summary>
		/// Holds the list of changed effects
		/// </summary>
		protected readonly HashSet<IGameEffect> m_changedEffects = new HashSet<IGameEffect>();
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
			
			lock (m_changedLock) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (!m_changedEffects.Contains(changedEffect))
					m_changedEffects.Add(changedEffect);
			}
			
			base.OnEffectsChanged(changedEffect);
		}


		/// <summary>
		/// Updates changed effects to the owner.
		/// </summary>
		protected override void UpdateChangedEffects()
		{
			var player = m_owner as GamePlayer;
			if (player != null)
			{
				// Send Modified Effects and Clear
				player.Out.SendUpdateIcons(m_changedEffects.ToList(), ref m_lastUpdateEffectsCount);
				m_changedEffects.Clear();
				
				if (player.Group != null)
				{
					player.Group.UpdateMember(player, true, false);
				}
			}
		}

	}
}
