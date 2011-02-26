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
using System.Text;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.Spells;
using log4net;
using System.Reflection;
using DOL.GS.Quests.Catacombs.Obelisks;

namespace DOL.GS
{
	/// <summary>
	/// Hibernia teleporter.
	/// </summary>
	/// <author>Aredhel</author>
	public class HiberniaTeleporter : GameTeleporter
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

			String intro = String.Format("Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4} {5} {6}",
			                             "to far away lands. If you wish to fight in the Frontiers I can send you to [Cruachan Gorge] or to the",
			                             "border keeps [Druim Ligen] and [Druim Cain]. Maybe you wish to undertake the Trials of",
			                             "Atlantis in [Oceanus] haven or wish to visit the mysterious Grove of [Domnann] and the [Shrouded Isles]?",
			                             "You could explore the [Shannon Estuary] or perhaps you would prefer the comforts of the [housing] regions.",
			                             "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
			                             "the great city of [Tir na Nog] or the [Shar Labyrinth]?",
			                             "Or perhaps you are interested in porting to our training camp [Fintain]?");
			SayTo(player, intro);
			return true;
		}

		/// <summary>
		/// Player has picked a subselection.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="subSelection"></param>
		protected override void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
		{
			switch (subSelection.TeleportID.ToLower())
			{
				case "shrouded isles":
					{
						String reply = String.Format("The isles of Hy Brasil are an excellent choice. {0} {1}",
						                             "Would you prefer the grove of [Domnann] or perhaps one of the outlying towns",
						                             "like [Droighaid], [Aalid Feie], or [Necht]?");
						SayTo(player, reply);
						return;
					}
				case "housing":
					{
						String reply = String.Format("I can send you to your [personal] house. If you do {0} {1} {2} {3}",
						                             "not have a personal house or wish to be sent to the housing [entrance] then you will",
						                             "arrive just inside the housing area. I can also send you to your [guild] house. If your",
						                             "guild does not own a house then you will not be transported. You may go to your [Hearth] bind",
						                             "as well if you are bound inside a house.");
						SayTo(player, reply);
						return;
					}
			}
			base.OnSubSelectionPicked(player, subSelection);
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
				case "aalid feie":
					break;	// No text?
				case "battlegrounds":
					if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
					{
						SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
						return;
					}
					SayTo(player, "I will teleport you to the appropriate battleground for your level and Realm Rank. If you exceed the Realm Rank for a battleground, you will not teleport. Please gain more experience to go to the next battleground.");
					break;
				case "cruachan gorge":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					break;
				case "domnann":
					SayTo(player, "The Shrouded Isles await you.");
					break;
				case "droighaid":
					break;	// No text?
				case "druim cain":
					SayTo(player, "Druim Cain is what you seek, and Druim Cain is what you shall find.");
					break;
				case "druim ligen":
					SayTo(player, "Druim Ligen is what you seek, and Druim Ligen is what you shall find.");
					break;
				case "entrance":
					break;	// No text?
				case "necht":
					break;	// No text?
				case "oceanus":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					break;
				case "personal":
					break;
				case "hearth":
					break;
				case "shannon estuary":
					SayTo(player, "You shall soon arrive in the Shannon Estuary.");
					break;
				case "shar labyrinth":
					if (player.HasFinishedQuest(typeof(SharLabyrinth)) <= 0)
					{
						SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
						                            "city. Seek out the path to the city and in future times I will aid you in",
						                            "this journey."));
						return;
					}
					break;
				case "tir na nog":
					SayTo(player, "The great city awaits!");
					break;
				case "fintain":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
					{
						SayTo(player,"Sorry, this place is not available for now !");
						return;
					}
					if (player.Level > 15)
					{
						SayTo(player,"Sorry, you are far too experienced to enjoy this place !");
						return;
					}
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
