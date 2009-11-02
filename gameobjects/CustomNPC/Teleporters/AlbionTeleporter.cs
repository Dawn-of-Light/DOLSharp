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
using DOL.GS.Housing;

namespace DOL.GS
{
	/// <summary>
	/// Albion teleporter.
	/// </summary>
	/// <author>Aredhel</author>
	public class AlbionTeleporter : GameTeleporter
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Add equipment to the teleporter.
		/// </summary>
		/// <returns></returns>
		public override bool AddToWorld()
		{
			GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.Cloak, 57, 66);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1005, 86);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 140, 6);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 141, 6);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 142, 6);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 143, 6);
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 1166);
			Inventory = template.CloseTemplate();

			SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			return base.AddToWorld();
		}

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
			                             "to far away lands. If you wish to fight in the Frontiers I can send you to [Forest Sauvage] or to the",
			                             "border keeps [Castle Sauvage] and [Snowdonia Fortress]. Maybe you wish to undertake the Trials of",
			                             "Atlantis in [Oceanus] haven or wish to visit the harbor of [Gothwaite] and the [Shrouded Isles]?",
			                             "You could explore the [Avalon Marsh] or perhaps you would prefer the comforts of the [housing] regions.",
			                             "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
			                             "the great city of [Camelot] or the [Inconnu Crypt]?",
			                             "Or perhaps you are interested in porting to our training camp [Holtham]?");
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
						String reply = String.Format("The isles of Avalon are an excellent choice. {0} {1}",
						                             "Would you prefer the harbor of [Gothwaite] or perhaps one of the outlying towns",
						                             "like [Wearyall] Village, Fort [Gwyntell], or Caer [Diogel]?");
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
				case "avalon marsh":
					SayTo(player, "You shall soon arrive in the Avalon Marsh.");
					break;
				case "battlegrounds":
					if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
					{
						SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
						return;
					}

					SayTo(player, "I will teleport you to the appropriate battleground for your level and Realm Rank. If you exceed the Realm Rank for a battleground, you will not teleport. Please gain more experience to go to the next battleground.");
					break;
				case "camelot":
					SayTo(player, "The great city awaits!");
					break;
				case "castle sauvage":
					SayTo(player, "Castle Sauvage is what you seek, and Castle Sauvage is what you shall find.");
					break;
				case "diogel":
					break;	// No text?
				case "entrance":
					break;	// No text?
				case "forest sauvage":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					break;
				case "gothwaite":
					SayTo(player, "The Shrouded Isles await you.");
					break;
				case "gwyntell":
					break;	// No text?
				case "inconnu crypt":
					if (player.HasFinishedQuest(typeof(InconnuCrypt)) <= 0)
					{
						SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
						                            "city. Seek out the path to the city and in future times I will aid you in",
						                            "this journey."));
						return;
					}
					break;
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
				case "snowdonia fortress":
					SayTo(player, "Snowdonia Fortress is what you seek, and Snowdonia Fortress is what you shall find.");
					break;
					// text for the following ?
				case "wearyall":
					break;
				case "holtham":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
					{
						SayTo(player,"Sorry, this place is not available for now !");
						return;
					}
					if (player.Level > 10)
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
