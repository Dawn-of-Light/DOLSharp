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
using DOL.GS.PacketHandler;
using DOL.GS.Housing;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute("&bountyrent", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Pay house rent with bountypoints", //command description
        "Use /bountyrent personal/guild <amount> to pay.")]
	public class BountyRentCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
            long bpWorth = ServerProperties.Properties.RENT_BOUNTY_POINT_TO_GOLD;

			if (args.Length < 2)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.CmdUsage", bpWorth),
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return;
			}

            if (args.Length < 3)
            {
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.CorrectFormat"),
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            House house = client.Player.CurrentHouse;
			if (house == null)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.RangeOfAHouse"),
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return;
			}

            string numericAmount = "";
            foreach (char c in args[2]) //Assist player mistakes and remove letters / special characters
            {
                if (!Char.IsNumber(c))
                    continue;

                numericAmount += c;
            }

            if (String.IsNullOrEmpty(numericAmount))
            {
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.CorrectFormat"),
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            long BPsToAdd = 0;
            try
            {
                BPsToAdd = Int64.Parse(numericAmount);
            }
            catch
            {
                return;
            }

			switch (args[1].ToLower())
			{
				case "personal":
					{
                        if (!house.CanPayRent(client.Player))
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NoPayRentPerm"),
                                eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            return;
                        }

						if ((client.Player.BountyPoints -= BPsToAdd) < 0)
						{
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NotEnoughBp"),
                                eChatType.CT_System, eChatLoc.CL_SystemWindow);

							return;
						}

                        if (house.KeptMoney == (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS))
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.AlreadyMaxMoney"),
                                eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            return;
                        }

                        if ((house.KeptMoney + (BPsToAdd * bpWorth)) > (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS))
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.ToManyMoney"),
                                eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            return;
                        }

                        house.KeptMoney += (BPsToAdd * bpWorth);
                        house.SaveIntoDatabase();

                        client.Player.BountyPoints -= BPsToAdd;
                        client.Player.SaveIntoDatabase();

                        client.Out.SendUpdatePoints();
                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.YouSpend", BPsToAdd, ((BPsToAdd * bpWorth) / bpWorth)),
                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
					} break;
				case "guild":
					{
                        if (house.DatabaseItem.GuildHouse && client.Player.GuildName == house.DatabaseItem.GuildName)
                        {
                            if (house.CanPayRent(client.Player))
                            {
                                if ((client.Player.Guild.BountyPoints -= BPsToAdd) < 0)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NotEnoughGuildBp"),
                                        eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                if (house.KeptMoney == (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS))
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.AlreadyMaxMoney"),
                                        eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                    return;
                                }

                                if ((house.KeptMoney + (BPsToAdd * bpWorth)) > (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS))
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.ToManyMoney"),
                                        eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                    return;
                                }

                                house.KeptMoney += (BPsToAdd * bpWorth);
                                house.SaveIntoDatabase();

                                client.Player.Guild.BountyPoints -= BPsToAdd;
                                client.Player.Guild.SaveIntoDatabase();

                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.YouSpendGuild", BPsToAdd, ((BPsToAdd * bpWorth) / bpWorth)),
                                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                return;
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NoPayRentPerm"),
                                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                return;
                            }
                        }

                        DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NotAHouseGuildLeader"));
					} break;
				default:
					{
                        DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.CorrectFormat"));
					} break;
			}
		}
	}
}