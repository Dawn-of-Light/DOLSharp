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
using System.Text;
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&house",
		ePrivLevel.Player,
		"Show various housing information"
		)]
	public class HouseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			try
			{
				if (client.Account.PrivLevel > (int)ePrivLevel.Player)
				{
					if (args.Length > 1)
					{
						HouseAdmin(client.Player, args);
						return;
					}

					if (client.Account.PrivLevel >= (int)ePrivLevel.GM)
					{
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.GMInfo"));
					}

					if (client.Account.PrivLevel == (int)ePrivLevel.Admin)
					{
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.AdminInfo1"));
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.AdminInfo2"));
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.AdminInfo3"));
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.AdminInfo4"));
					}
				}

				House house = HouseMgr.GetHouseByPlayer(client.Player);

				if (house != null)
				{
					if (client.Player.Guild != null)
					{
						// check to see if guild emblem is current
						if (house.Emblem != client.Player.Guild.Emblem)
						{
							house.Emblem = client.Player.Guild.Emblem;
							house.SaveIntoDatabase();
						}
					}

					if (house.RegionID == client.Player.CurrentRegionID && client.Player.InHouse == false)
					{
						// let's force update their house to make sure they can see it

						client.Out.SendHouse(house);
						client.Out.SendGarden(house);

						if (house.IsOccupied)
						{
							client.Out.SendHouseOccupied(house, true);
						}
					}

					// Send the house info dialog
					house.SendHouseInfo(client.Player);
				}
				else
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.House.NoHouse"));
				}

				// now check for a guild house and update emblem if needed, then force update

				if (client.Player.Guild != null && client.Player.Guild.GuildOwnsHouse && client.Player.Guild.GuildHouseNumber > 0)
				{
					House guildHouse = HouseMgr.GetHouse(client.Player.Guild.GuildHouseNumber);

					if (guildHouse != null)
					{
						if (guildHouse.Emblem != client.Player.Guild.Emblem)
						{
							guildHouse.Emblem = client.Player.Guild.Emblem;
							guildHouse.SaveIntoDatabase();
							guildHouse.SendUpdate(); // forces refresh
						}
						else if (guildHouse.RegionID == client.Player.CurrentRegionID)
						{
							guildHouse.SendUpdate(); // forces refresh
						}
					}
				}
			}
			catch
			{
				DisplaySyntax(client);
			}
		}

		public void HouseAdmin(GamePlayer player, string [] args)
		{
			if (player.Client.Account.PrivLevel == (int)ePrivLevel.Admin)
			{
				if (args[1].ToLower() == "restart")
				{
					HouseMgr.Start(player.Client);
					return;
				}

				if (args[1].ToLower() == "addhookpoints")
				{
					if (player.TempProperties.getProperty<bool>(HousingConstants.AllowAddHouseHookpoint, false))
					{
						player.TempProperties.removeProperty(HousingConstants.AllowAddHouseHookpoint);
						DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.HookPointOff"));
					}
					else
					{
						player.TempProperties.setProperty(HousingConstants.AllowAddHouseHookpoint, true);
						DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.HookPointOn"));
					}

					return;
				}
			}

			ArrayList houses = (ArrayList)HouseMgr.GetHousesCloseToSpot(player.Position, 700);
			if (houses.Count != 1)
			{
				DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.FarAway"));
				return;
			}

			if (args[1].ToLower() == "info")
			{
				(houses[0] as House).SendHouseInfo(player);
				return;
			}

			// The following commands are for Admins only

			if (player.Client.Account.PrivLevel != (int)ePrivLevel.Admin)
				return;

			if (args[1].ToLower() == "model")
			{
				int newModel = Convert.ToInt32(args[2]);

				if (newModel < 1 || newModel > 12)
				{
					DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.ModelInvalid"));
					return;
				}

				if (houses.Count == 1 && newModel != (houses[0] as House).Model)
				{
					HouseMgr.RemoveHouseItems(houses[0] as House);
					(houses[0] as House).Model = newModel;
					(houses[0] as House).SaveIntoDatabase();
					(houses[0] as House).SendUpdate();

					DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.ModelChanged", newModel));
					GameServer.Instance.LogGMAction(player.Name + " changed house #" + (houses[0] as House).HouseNumber + " model to " + newModel);
				}

				return;
			}

			if (args[1].ToLower() == "remove")
			{
				string confirm = "";

				if (args.Length > 2)
					confirm = args[2];

				if (confirm != "YES")
				{
					DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.ConfirmYES"));
					return;
				}

				if (houses.Count == 1)
				{
					HouseMgr.RemoveHouse(houses[0] as House);
					DisplayMessage(player.Client, LanguageMgr.GetTranslation(player.Client, "Scripts.Players.House.Removed"));
					GameServer.Instance.LogGMAction(player.Name + " removed house #" + (houses[0] as House).HouseNumber);
				}

				return;
			}
		}

	}
}