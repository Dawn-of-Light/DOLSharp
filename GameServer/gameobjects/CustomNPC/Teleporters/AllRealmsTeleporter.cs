using DOL.Database;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS
{
	/// <summary>
	/// Albion teleporter.
	/// </summary>
	/// <author>Aredhel</author>
	public class AllRealmsTeleporter : GameTeleporter
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Display teleport destinations for passed realm
		/// </summary>
		/// <param name="PlayerRealm">Realm to display destinations for</param>
		public String DisplayTeleportDestinations(eRealm DestinationRealm)
		{
			String sRet = null;	

			switch (DestinationRealm)
			{
				case eRealm.Albion:
					sRet = String.Format("Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4} {5} {6} {7}",
										 "to far away lands. If you wish to fight in the Frontiers I can send you to [Forest Sauvage] or to the",
										 "border keeps [Castle Sauvage] and [Snowdonia Fortress]. Maybe you wish to undertake the Trials of",
										 "Atlantis in [Albion Oceanus] haven or wish to visit the harbor of [Gothwaite] and the [Albion Shrouded Isles]?",
										 "You could explore the [Avalon Marsh] or perhaps you would prefer the comforts of the [housing] regions.",
										 "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
										 "the great city of [Camelot] or the [Inconnu Crypt]?",
										 "Or perhaps you are interested in porting to our training camp [Holtham]?",
										 "Or would you prefer to teleport to [Hibernia] or [Midgard]?");
					break;
				case eRealm.Midgard:
					sRet = String.Format("Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4} {5} {6} {7}",
										 "to far away lands. If you wish to fight in the Frontiers I can send you to [Uppland] or to the",
										 "border keeps [Svasud Faste] and [Vindsaul Faste]. Maybe you wish to undertake the Trials of",
										 "Atlantis in [Midgard Oceanus] haven or wish to visit the [City of Aegirhamn] and the [Midgard Shrouded Isles]?",
										 "You could explore the [Gotar] or perhaps you would prefer the comforts of the [housing] regions.",
										 "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
										 "the great city of [Jordheim] or the [Kobold Undercity]?",
										 "Or perhaps you are interested in porting to our training camp [Hafheim]?",
										 "Or would you prefer to teleport to [Albion] or [Hibernia]?");
					break;
				case eRealm.Hibernia:
					sRet = String.Format("Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4} {5} {6} {7}",
										 "to far away lands. If you wish to fight in the Frontiers I can send you to [Cruachan Gorge] or to the",
										 "border keeps [Druim Ligen] and [Druim Cain]. Maybe you wish to undertake the Trials of",
										 "Atlantis in [Hibernia Oceanus] haven or wish to visit the mysterious Grove of [Domnann] and the [Hibernia Shrouded Isles]?",
										 "You could explore the [Shannon Estuary] or perhaps you would prefer the comforts of the [housing] regions.",
										 "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
										 "the great city of [Tir na Nog] or the [Shar Labyrinth]?",
										 "Or perhaps you are interested in porting to our training camp [Fintain]?",
										 "Or would you prefer to teleport to [Albion] or [Midgard]?");
					break;
				default:
					log.Warn(String.Format("DisplayTeleportDestinations does not handle player realm [{0}]",DestinationRealm.ToString()));
					break;
			}

			return sRet;
		}

		/// <summary>
		/// Player right-clicked the teleporter.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			String intro = DisplayTeleportDestinations(player.Realm);
			if (intro != null)
				SayTo(player, intro);

			if (!base.Interact(player))
				return false;

			return true;
		}

		/// <summary>
		/// Talk to the teleporter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			eRealm realmTarget;

			switch (text.ToLower())
			{
				// Realm specific menus
				case "albion":
					SayTo(player, DisplayTeleportDestinations(eRealm.Albion));
					return true;
				case "midgard":
					SayTo(player, DisplayTeleportDestinations(eRealm.Midgard));
					return true;
				case "hibernia":
					SayTo(player, DisplayTeleportDestinations(eRealm.Hibernia));
					return true;
				case "albion shrouded isles":
					String sReplyASI = String.Format("The isles of Avalon are an excellent choice. {0} {1}",
						"Would you prefer the harbor of [Gothwaite] or perhaps one of the outlying towns",
						"like [Wearyall] Village, Fort [Gwyntell], or Caer [Diogel]?");
					SayTo(player, sReplyASI);
					return true;
				case "midgard shrouded isles":
					String sReplyMSI = String.Format("The isles of Aegir are an excellent choice. {0} {1}",
						"Would you prefer the city of [Aegirhamn] or perhaps one of the outlying towns",
						"like [Bjarken], [Hagall], or [Knarr]?");
					SayTo(player, sReplyMSI);
					return true;
				case "hibernia shrouded isles":
					String sReplyHSI = String.Format("The isles of Hy Brasil are an excellent choice. {0} {1}",
						"Would you prefer the grove of [Domnann] or perhaps one of the outlying towns",
						"like [Droighaid], [Aalid Feie], or [Necht]?");
					SayTo(player, sReplyHSI);
					return true;
				case "housing":
					String sReplyH = String.Format("I can send you to your [personal] house. If you do {0} {1} {2} {3}",
						"not have a personal house or wish to be sent to the housing [entrance] then you will",
						"arrive just inside the housing area. I can also send you to your [guild] house. If your",
						"guild does not own a house then you will not be transported. You may go to your [Hearth] bind",
						"as well if you are bound inside a house.");
					SayTo(player, sReplyH);
					return true;

				// Albion destinations
				case "albion oceanus":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Albion;
					text = "Oceanus";
					break;
				case "avalon marsh":
					SayTo(player, "You shall soon arrive in the Avalon Marsh.");
					realmTarget = eRealm.Albion;
					break;
				case "camelot":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Albion;
					break;
				case "castle sauvage":
					SayTo(player, "Castle Sauvage is what you seek, and Castle Sauvage is what you shall find.");
					realmTarget = eRealm.Albion;
					break;
				case "diogel":
					realmTarget = eRealm.Albion;
					break;  // No text?
				case "forest sauvage":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Albion;
					break;
				case "gothwaite":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Albion;
					break;
				case "gwyntell":
					realmTarget = eRealm.Albion;
					break;  // No text?
				case "inconnu crypt":
					//if (player.HasFinishedQuest(typeof(InconnuCrypt)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Albion;
					break;
				case "snowdonia fortress":
					SayTo(player, "Snowdonia Fortress is what you seek, and Snowdonia Fortress is what you shall find.");
					realmTarget = eRealm.Albion;
					break;
				// text for the following ?
				case "wearyall":
					realmTarget = eRealm.Albion;
					break;
				case "holtham":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
					{
						SayTo(player, "Sorry, this place is not available for now !");
						return true;
					}
					if (player.Level > 15)
					{
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
						return true;
					}
					realmTarget = eRealm.Albion;
					break;

				// Midgard
				case "midgard oceanus":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Midgard;
					text = "Oceanus";
					break;
				case "bjarken":
					realmTarget = eRealm.Midgard;
					break;  // No text?
				case "city of aegirhamn":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Midgard;
					break;
				case "gotar":
					SayTo(player, "You shall soon arrive in the Gotar.");
					realmTarget = eRealm.Midgard;
					break;
				case "hagall":
					realmTarget = eRealm.Midgard;
					break;  // No text?
				case "jordheim":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Midgard;
					break;
				case "knarr":
					realmTarget = eRealm.Midgard;
					break;  // No text?
				case "kobold undercity":
					//if (player.HasFinishedQuest(typeof(KoboldUndercity)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Midgard;
					break;
				case "svasud faste":
					SayTo(player, "Svasud Faste is what you seek, and Svasud Faste is what you shall find.");
					realmTarget = eRealm.Midgard;
					break;
				case "uppland":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Midgard;
					break;
				case "vindsaul faste":
					SayTo(player, "Vindsaul Faste is what you seek, and Vindsaul Faste is what you shall find.");
					realmTarget = eRealm.Midgard;
					break;
				case "hafheim":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
					{
						SayTo(player, "Sorry, this place is not available for now !");
						return true;
					}
					if (player.Level > 15)
					{
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
						return true;
					}
					realmTarget = eRealm.Midgard;
					break;
				// Hibernia
				case "hibernia oceanus":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Hibernia;
					text = "Oceanus";
					break;
				case "aalid feie":
					realmTarget = eRealm.Hibernia;
					break;  // No text?
				case "cruachan gorge":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Hibernia;
					break;
				case "domnann":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Hibernia;
					break;
				case "droighaid":
					realmTarget = eRealm.Hibernia;
					break;  // No text?
				case "druim cain":
					SayTo(player, "Druim Cain is what you seek, and Druim Cain is what you shall find.");
					realmTarget = eRealm.Hibernia;
					break;
				case "druim ligen":
					SayTo(player, "Druim Ligen is what you seek, and Druim Ligen is what you shall find.");
					realmTarget = eRealm.Hibernia;
					break;
				case "necht":
					realmTarget = eRealm.Hibernia;
					break;  // No text?
				case "shannon estuary":
					SayTo(player, "You shall soon arrive in the Shannon Estuary.");
					realmTarget = eRealm.Hibernia;
					break;
				case "shar labyrinth":
					//if (player.HasFinishedQuest(typeof(SharLabyrinth)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Hibernia;
					break;
				case "tir na nog":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Hibernia;
					break;
				case "fintain":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
					{
						SayTo(player, "Sorry, this place is not available for now !");
						return true;
					}
					if (player.Level > 15)
					{
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
						return true;
					}
					realmTarget = eRealm.Hibernia;
					break;
				// All realms
				case "battlegrounds":
					if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
					{
						SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
						return true;
					}
					SayTo(player, "I will teleport you to the appropriate battleground for your level and Realm Rank. If you exceed the Realm Rank for a battleground, you will not teleport. Please gain more experience to go to the next battleground.");
					realmTarget = eRealm.Hibernia;
					break;
				case "entrance":
				case "personal":
				case "hearth":
					realmTarget = player.Realm;
					break;
				default:
					SayTo(player, "This destination is not yet supported.");
					return true;
			} // switch (text.ToLower())

			// Find the teleport location in the database.
			Teleport port = WorldMgr.GetTeleportLocation(realmTarget, String.Format(":{0}",text));
			if (port != null)
			{
				OnTeleportSpell(player, port);
				return true;
			}

			return GetTeleportLocation(player, text);
		}
	}
}
