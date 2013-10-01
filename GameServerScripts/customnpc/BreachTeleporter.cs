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
using DOL.Database;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Demons Breach teleporter.
	/// </summary>
	/// <author>Argoras</author>
	public class BreachTeleporter : GameTeleporter
	{
        /// <summary>
		/// Player right-clicked the teleporter.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

            if (player.Level < 5)
            {
                SayTo(player, "Sorry, you are not experienced enough to enter this place !");
                return false;
            }

            if (player.Level > 10)
            {
                SayTo(player, "Sorry, you are far too experienced to enter this place !");
                return false;
            }

            String intro = String.Format("Greetings. I can channel the energies of this place to send you {0}",
			                             "to Demons Breach. Do you wish to travel to [Balban's Breach]?");
			SayTo(player, intro);
			return true;
		}

		/// <summary>
		/// Player has picked a destination.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
		{
			switch (destination.TeleportID.ToLower())
            {
                case "balban's breach":
                    SayTo(player, "For the glory of the Realm, Balban's Breach it will be.");
                    break;
                default:
					SayTo(player, "This destination is not yet supported.");
					return;
			}
			base.OnDestinationPicked(player, destination);
		}

		/// <summary>
		/// Teleport the player to the designated coordinates.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnTeleport(GamePlayer player, Teleport destination)
		{
			OnTeleportSpell(player, destination);
		}
	}
}