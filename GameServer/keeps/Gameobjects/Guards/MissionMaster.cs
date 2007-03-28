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

using DOL.GS.Quests;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Represents a mission master
	/// </summary>
	public class MissionMaster : GameKeepGuard
	{
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (Component == null)
				SayTo(player, "Greetings, " + player.Name + ". We have put out the call far and wide for heroes such as yourself to aid us in our ongoing struggle. It warms my heart good to to see a great " + player.CharacterClass.Name + " such as yourself willing to lay their life on the line in defence of the [realm].");
			else SayTo(player, "Hail and well met, " + player.Name + "! As the leader of our forces, I am calling upon our finest warriors to aid in the vanquishing of our enemies. Do you wish to do your duty in defence of our [realm]?");
			return true;
		}

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			switch (str.ToLower())
			{
				case "realm":
					{
						if (Component == null)
							SayTo(player, "We all must do our part. How would you like to assist the cause? I have [personal missions], [group missions], and [guild missions] available.");
						else SayTo(player, "Excellent! We all must do our part. How would you like to assist the cause? I have [personal missions] and [group missions] available."); 
						break;
					}
				case "personal missions":
					{
						SayTo(player, "We have several personal missions from which to choose. Would you like to claim the bounty on some [realm guards], or claim the bounties on some [enemies of the realm]? Perhaps a frontal assault isn't your style? If so, we also have missions that require you to [reconnoiter] an enemy realm, or elimate the thread of an impending [assassination]?");
						break;
					}
				case "realm guards":
					{
						if (player.Mission != null)
							player.Mission.ExpireMission();
						player.Mission = new KillMission(typeof(GameKeepGuard), 15, "enemy realm guards", player);
						break;
					}
				case "enemies of the realm":
					{
						if (player.Mission != null)
							player.Mission.ExpireMission();
						player.Mission = new KillMission(typeof(GamePlayer), 5, "enemy players", player);
						break;
					}
				case "reconnoiter":
					{
						if (player.Mission != null)
							player.Mission.ExpireMission();
						player.Mission = new ScoutMission(player);
						break;
					}
				case "assassination":
					{
						SayTo(player, "This type of mission is not yet implemented");
						break;
					}
				case "group missions":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}

						SayTo(player, "Would your group like to help with a [tower capture], a [keep capture], or a [caravan] raid? Should those choices fail to appeal to you, I also have bounty missions on [enemy guards] and [realm enemies] if that is your preference.");
						break;
					}
				case "tower raize":
					{
						break;
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						player.PlayerGroup.Mission = new RaizeMission(player.PlayerGroup);
						break;
					}
				case "tower capture":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						if (player.PlayerGroup.Mission != null)
							player.PlayerGroup.Mission.ExpireMission();
						player.PlayerGroup.Mission = new CaptureMission(CaptureMission.eCaptureType.Tower, player.PlayerGroup);
						break;
					}
				case "keep capture":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						if (player.PlayerGroup.Mission != null)
							player.PlayerGroup.Mission.ExpireMission();
						player.PlayerGroup.Mission = new CaptureMission(CaptureMission.eCaptureType.Keep, player.PlayerGroup);
						break;
					}
				case "caravan":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						SayTo(player, "This type of mission is not yet implemented");
						break;
					}
				case "enemy guards":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						if (player.PlayerGroup.Mission != null)
							player.PlayerGroup.Mission.ExpireMission();
						player.PlayerGroup.Mission = new KillMission(typeof(GameKeepGuard), 25, "enemy realm guards", player.PlayerGroup);
						break;
					}
				case "realm enemies":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You are not in a group!");
							break;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							break;
						}
						if (player.PlayerGroup.Mission != null)
							player.PlayerGroup.Mission.ExpireMission();
						player.PlayerGroup.Mission = new KillMission(typeof(GamePlayer), 15, "enemy players", player.PlayerGroup);
						break;
					}
				case "guild missions":
					{
						if (Component != null)
							break;
						if (player.Guild == null)
						{
							SayTo(player, "You have no guild!");
							return false;
						}

						if (!player.Guild.GotAccess(player, eGuildRank.OcSpeak))
						{
							SayTo(player, "You are not high enough rank in your guild!");
							return false;
						}
						SayTo(player, "This type of mission is not yet implemented");
						break;
						SayTo(player, "Outstanding, we can always use help from organized guilds. Would you like to press the attack on the realm of [Albion] or the realm of [Hibernia].");
						break;
					}
			}

			if (player.Mission != null)
				SayTo(player, player.Mission.Description);

			if (player.PlayerGroup != null && player.PlayerGroup.Mission != null)
				SayTo(player, player.PlayerGroup.Mission.Description);

			return true;
		}
	}

	/*
	 * Champion Commander
	 * Captain Commander
	 * Hersir Commander
	 * 
	 * Hail and well met, PLAYERNAME! As the leader of our forces, I am calling upon our finest warriors to aid in the vanquishing of our enemies. Do you wish to do your duty in defence of our [realm]?
	 * Excellent! We all must do our part. How would you like to assist the cause? I have [personal missions] and [group missions] available.
	 * 
	 * General
	 * 
	 * Greetings, PLAYERNAME. We have put out the call far and wide for heroes such as yourself to aid us in our ongoing struggle. It warms my heart good to to see a great CLASSNAME such as yourself willing to lay their life on the line in defence of the [realm].
	 * We all must do our part. How would you like to assist the cause? I have [personal missions], [group missions], and [guild missions] available.
	 * We have several personal missions from which to choose. Would you like to claim the bounty on some [realm guards], or claim the bounties on some [enemies of the realm]? Perhaps a frontal assault isn't your style? If so, we also have missions that require you to [reconnoiter] an enemy realm, or elimate the thread of an impending [assassination]?
	 * Would your group like to help with a [tower capture], a [keep capture], or a [caravan] raid? Should those choices fail to appeal to you, I also have bounty missions on [enemy guards] and [realm enemies] if that is your preference.
	 * Outstanding, we can always use help from organized guilds. Would you like to press the attack on the realm of [Albion] or the realm of [Hibernia].
	 */
}