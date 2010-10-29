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

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, 0x1C, "Withdraw GameConsignmentMerchant Merchant Money")]
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
            GameConsignmentMerchant con = client.Player.ActiveConMerchant;
            if (con == null)
                return;

			// current house is null, return
            House house = HouseMgr.GetHouse(con.HouseNumber);
            if (house == null)
                return;

			// make sure player has permissions to withdraw from the consignment merchant
            if (!house.CanUseConsignmentMerchant(client.Player, ConsignmentPermissions.Withdraw))
            {
                client.Player.Out.SendMessage("You don't have permission to withdraw money from this merchant!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                return;
            }

        	var totalConMoney = con.TotalMoney;

            if (totalConMoney > 0)
            {
                if (ConsignmentMoney.UseBP)
                {
					client.Player.Out.SendMessage("You withdraw " + totalConMoney.ToString() + " BountyPoints from your Merchant.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					client.Player.BountyPoints += totalConMoney;
                    client.Player.Out.SendUpdatePoints();
                }
                else
                {
					ChatUtil.SendMerchantMessage(client, "GameMerchant.OnPlayerWithdraw", Money.GetString(totalConMoney));
					client.Player.AddMoney(totalConMoney);
                }

				con.TotalMoney -= totalConMoney;
            }
        }
    }
}