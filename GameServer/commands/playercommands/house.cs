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
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&house",
		ePrivLevel.Player,
		"Show various housing information"
		)]
	public class HouseCommanHandler : AbstractCommandHandler, ICommandHandler
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
						DisplayMessage(client, "GM: info - Display house info for a nearby house");
					}

					if (client.Account.PrivLevel == (int)ePrivLevel.Admin)
					{
						DisplayMessage(client, "Admin: model <1 - 12> - change house model");
						DisplayMessage(client, "Admin: restart - restart the housing manager");
						DisplayMessage(client, "Admin: addhookpoints - allow adding of missing hookpoints");
						DisplayMessage(client, "Admin: remove <YES> - remove this house!");
					}
				}

				House house = HouseMgr.GetHouseByPlayer(client.Player);
				if (house == null)
				{
					DisplayMessage(client, "You do not own a house.");
					return;
				}

				house.SendHouseInfo(client.Player);
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
						DisplayMessage(player.Client, "Add hookpoints turned off!");
					}
					else
					{
						player.TempProperties.setProperty(HousingConstants.AllowAddHouseHookpoint, true);
						DisplayMessage(player.Client, "Add hookpoints turned on!");
					}

					return;
				}
			}

			ArrayList houses = (ArrayList)HouseMgr.GetHousesCloseToSpot(player.CurrentRegionID, player.X, player.Y, 700);
			if (houses.Count != 1)
			{
				DisplayMessage(player.Client, "You need to stand closer to a house!");
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
					DisplayMessage(player.Client, "Valid house models are 1 - 12!");
					return;
				}

				if (houses.Count == 1 && newModel != (houses[0] as House).Model)
				{
					HouseMgr.RemoveHouseItems(houses[0] as House);
					(houses[0] as House).Model = newModel;
					(houses[0] as House).SaveIntoDatabase();

					foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						p.Out.SendHouse(houses[0] as House);
						p.Out.SendGarden(houses[0] as House);
					}

					DisplayMessage(player.Client, "House model changed to " + newModel + "!");
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
					DisplayMessage(player.Client, "You must confirm this removal with 'YES'");
					return;
				}

				if (houses.Count == 1)
				{
					HouseMgr.RemoveHouse(houses[0] as House);
					DisplayMessage(player.Client, "House removed!");
					GameServer.Instance.LogGMAction(player.Name + " removed house #" + (houses[0] as House).HouseNumber);
				}

				return;
			}
		}

	}
}