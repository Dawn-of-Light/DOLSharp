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
using System.Reflection;
using DOL.Database;
using DOL.GS.Housing;
using log4net;
using DOL.Language;
using DOL.GS.Finance;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.WithDrawMerchantMoney, "Withdraw GameConsignmentMerchant Merchant Money", eClientStatus.PlayerInGame)]
    public class PlayerWithdrawMerchantMoney : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
			// player is null, return
            if (client.Player == null)
                return;

			// active consignment merchant is null, return
            GameConsignmentMerchant conMerchant = client.Player.ActiveInventoryObject as GameConsignmentMerchant;
            if (conMerchant == null)
                return;

			// current house is null, return
            House house = HouseMgr.GetHouse(conMerchant.HouseNumber);
            if (house == null)
                return;

			// make sure player has permissions to withdraw from the consignment merchant
            if (!house.CanUseConsignmentMerchant(client.Player, ConsignmentPermissions.Withdraw))
            {
                client.Player.Out.SendMessage("You don't have permission to withdraw money from this merchant!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                return;
            }

			lock (conMerchant.LockObject())
			{
				long totalConMoney = conMerchant.TotalMoney;

				if (totalConMoney > 0)
				{
					if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					{
						client.Player.Out.SendMessage("You withdraw " + totalConMoney.ToString() + " BountyPoints from your Merchant.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
						client.Player.AddMoney(Currency.BountyPoints.Mint(totalConMoney));
					}
					else
					{
						ChatUtil.SendMerchantMessage(client, "GameMerchant.OnPlayerWithdraw", Money.GetString(totalConMoney));
						client.Player.AddMoney(Currency.Copper.Mint(totalConMoney));
						InventoryLogging.LogInventoryAction(conMerchant, client.Player, eInventoryActionType.Merchant, totalConMoney);
					}

					conMerchant.TotalMoney -= totalConMoney;

					if (ServerProperties.Properties.MARKET_ENABLE_LOG)
					{
						log.DebugFormat("CM: [{0}:{1}] withdraws {2} from CM on lot {3}.", client.Player.Name, client.Account.Name, totalConMoney, conMerchant.HouseNumber);
					}

					client.Out.SendConsignmentMerchantMoney(conMerchant.TotalMoney);
				}
			}
        }
    }
}