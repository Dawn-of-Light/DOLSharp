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

namespace DOL.GS.Effects
{
    /// <summary>
    /// Dummy effect for testing purposes (identifying buff icons and such).
    /// </summary>
    /// <author>Aredhel</author>
    public class DummyEffect : TimedEffect, IGameEffect
    {
        /// <summary>
        /// Create a new DummyEffect for the given effect ID.
        /// </summary>
        /// <param name="effectId"></param>
        public DummyEffect(ushort effectId) : base(60000)
        {
            EffectId = effectId;
        }

        /// <summary>
        /// The ID associated with this effect.
        /// </summary>
        public ushort EffectId { get; protected set; }

		/// <summary>
		/// The effect owner.
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// Start the dummy effect on a player.
		/// </summary>
		/// <param name="living">The effect target</param>
		public override void Start(GameLiving living)
		{
			GamePlayer player = living as GamePlayer;

			if (player != null)
			{
				m_player = player;

				player.EffectList.Add(this);
                player.Out.SendUpdatePlayer();       
			}
		}

		/// <summary>
		/// Stop the effect.
		/// </summary>
		public override void Stop()
		{
            if (m_player != null)
            {
                m_player.EffectList.Remove(this);
                m_player.Out.SendUpdatePlayer();
            }
		}

		/// <summary>
		/// Name of the effect.
		/// </summary>
		public override string Name 
        {
            get { return String.Format("Effect #{0}", EffectId); } 
        }	

		/// <summary>
		/// Icon to show on players.
		/// </summary>
		public override ushort Icon 
        { 
            get { return EffectId; } 
        }
		
		/// <summary>
		/// Delve information.
		/// </summary>
		public override IList<string> DelveInfo 
        {
            get 
			{
				string[] delve = new string[1];
				delve[0] = "Dummy Effect Icon #" + Icon;
				return delve;
			} 
        }
    }
}
