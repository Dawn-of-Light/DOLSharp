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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&sell",
		ePrivLevel.Player,
		"Sell items to a targeted merchant.  Specify a single item, or a first and last item to sell multiple items.",
		"Use: /sell 4 to sell 4th item\n    /sell 9 16 to sell all items in bag 2")]
	public class SellCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			int firstItem = 0, lastItem = 0;

			switch (args.Length)
            {
				case 2:
					if (int.TryParse(args[1], out firstItem))
						lastItem = firstItem;
					break;
				case 3:
					if (int.TryParse(args[1], out firstItem) && int.TryParse(args[2], out lastItem))
                    {
						if (lastItem < firstItem)
                        {
							int temp = firstItem;
							firstItem = lastItem;
							lastItem = temp;
                        }
                    }
					break;
				default:
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Sell.Usage"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
			}

			if (firstItem < 1 || firstItem > 40 || lastItem < 1 || lastItem > 40)
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Sell.Invalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else if (client.Player is GamePlayer player && player.Inventory != null)
            {
				if (player.TargetObject is GameMerchant merchant)
                {
					firstItem += (int)eInventorySlot.FirstBackpack - 1;
					lastItem += (int)eInventorySlot.FirstBackpack - 1;

					for (int i = firstItem; i <= lastItem; i++)
                    {
						InventoryItem item = player.Inventory.GetItem((eInventorySlot)i);
						if (item != null)
							merchant.OnPlayerSell(player, item);
                    }
				}
				else
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Sell.Merchant"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
	}
}