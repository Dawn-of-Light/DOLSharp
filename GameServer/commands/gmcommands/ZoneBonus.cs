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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.GS.Utils;
using DOL.Database;
using log4net;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&zonebonus",
        ePrivLevel.GM,
        "/zonebonus <zoneID|current> <xpBonus> <rpBonus> <bpBonus> <coinBonus> <Save? (true/false)>")]
    public class ZoneBonus : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 5)
            {
                DisplaySyntax(client);
                return;
            }

            //make sure that only numbers are used to avoid errors.
            foreach (char c in string.Join(" ", args, 2, 4))
            {
                if (char.IsLetter(c))
                {
                    DisplaySyntax(client);
                    return;
                }
            }

            Zone zone;

            switch (args[1].ToString().ToLower())
            {
                case "c":
                case "cu":
                case "cur":
                case "curr":
                case "curre":
                case "current":
                    {
                        zone = WorldMgr.GetZone(client.Player.CurrentZone.ID);
                    }
                    break;
                default:
                    {
                        //make sure that its a number again.
                        foreach (char c in args[1])
                        {
                            if (!(char.IsNumber(c)))
                            {
                                DisplaySyntax(client);
                                return;
                            }
                        }

                        if (WorldMgr.GetZone(ushort.Parse(args[1])) == null)
                        {
                            DisplayMessage(client, "No Zone with that ID was found!");
                            return;
                        }
                            zone = WorldMgr.GetZone(ushort.Parse(args[1]));
                    }
                    break;
            }

            zone.BonusExperience = int.Parse(args[2]);
            zone.BonusRealmpoints = int.Parse(args[3]);
            zone.BonusBountypoints = int.Parse(args[4]);
            zone.BonusCoin = int.Parse(args[5]);

            if (args[6].ToLower().StartsWith("t"))
            {
                client.Player.TempProperties.setProperty("ZONE_BONUS_SAVE", zone);
                client.Player.Out.SendCustomDialog(string.Format("Are you sure you wan't to over write {0} in the database?", zone.Description), new CustomDialogResponse(AreYouSure));
            }
            else
            {
                client.Player.Out.SendCustomDialog(string.Format("The zone settings for {0} will be reverted back to database settings on server restart.", zone.Description), null);
            }
        }
        public static void AreYouSure(GamePlayer player, byte response)
        {
            //here we get the zones new info.
            Zone zone = player.TempProperties.getProperty<Zone>("ZONE_BONUS_SAVE");

            if (response != 0x01)
            {
                player.Out.SendCustomDialog(string.Format("{0}'s bonuses will not be saved to the database!", zone.Description), null);
                player.TempProperties.removeProperty("ZONE_BONUS_SAVE");
                return;
            }

            //find the zone.
            Zones dbZone = GameServer.Database.SelectObject<Zones>("`ZoneID` = '" + zone.ID + "' AND `RegionID` = '" + zone.ZoneRegion.ID + "'");
            //update the zone bonuses.
            dbZone.Bountypoints = zone.BonusBountypoints;
            dbZone.Realmpoints = zone.BonusRealmpoints;
            dbZone.Coin = zone.BonusCoin;
            dbZone.Experience = zone.BonusExperience;
            GameServer.Database.SaveObject(dbZone);

            player.Out.SendCustomDialog(string.Format("{0}'s new zone bonuses have been updated to the database and changes have already taken effect!", zone.Description), null);
            
            //remove the property.
            player.TempProperties.removeProperty("ZONE_BONUS_SAVE");
        }
    }
}
