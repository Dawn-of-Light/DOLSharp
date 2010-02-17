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

using System.Collections;
using System;
using System.Collections.Generic;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Helper for charge realm ability
    /// </summary>
    public class AllureofDeathEffect : TimedEffect
    {
        private GameLiving owner;
        /// <summary>
        /// </summary>
        public AllureofDeathEffect() : base(60000) { }

        /// <summary>
        /// Start the effect on player
        /// </summary>
        /// <param name="target">The effect target</param>
        public override void Start(GameLiving target)
        {
            base.Start(target);
            owner = target;
            GamePlayer player = target as GamePlayer;
            if (player != null)
            {
                player.Model = 1669;
            }
        }

        public override void Stop()
        {
            base.Stop();
            GamePlayer player = owner as GamePlayer;
            if (player is GamePlayer)
            {
                player.Model = (ushort)player.PlayerCharacter.CreationModel;
            }
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name { get { return "Allure of Death"; } }

        /// <summary>
        /// Icon to show on players, can be id
        /// </summary>
        public override ushort Icon { get { return 3075; } }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("Changes your Skin for 30 seconds and grantz you 75% CC Imunity.");
                return list;
            }
        }
    }
}