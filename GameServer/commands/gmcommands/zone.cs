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
        "&zone",
        ePrivLevel.GM,
		"/zone info",
		"/zone divingflag <0 = use region, 1 = on, 2 = off>",
		"/zone waterlevel <#>",
		"/zone bonus <zoneID|current> <xpBonus> <rpBonus> <bpBonus> <coinBonus> <Save? (true/false)>")]
    public class ZoneCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
			try
			{
				Zone zone;

				if (args[1].ToLower() == "info")
				{
					var info = new List<string>();
					info.Add(" ");
					info.Add(" NPCs in zone:");
					info.Add(" Alb: " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Albion).Count);
					info.Add(" Hib: " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Hibernia).Count);
					info.Add(" Mid: " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Midgard).Count);
					info.Add(" None: " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.None).Count);
					info.Add(" ");
					info.Add(string.Format(" Objects in zone: {0}, Total allowed for region: {1}", client.Player.CurrentZone.TotalNumberOfObjects, ServerProperties.Properties.REGION_MAX_OBJECTS));
					info.Add(" ");
					info.Add(" Zone Description: " + client.Player.CurrentZone.Description);
					info.Add(" Zone Realm: " + GlobalConstants.RealmToName(client.Player.CurrentZone.GetRealm()));
					info.Add(" Zone ID: " + client.Player.CurrentZone.ID);
					info.Add(" Zone IsDungeon: " + client.Player.CurrentZone.IsDungeon);
					info.Add(" Zone SkinID: " + client.Player.CurrentZone.ZoneSkinID);
					info.Add(" Zone X: " + client.Player.CurrentZone.XOffset);
					info.Add(" Zone Y: " + client.Player.CurrentZone.YOffset);
					info.Add(" Zone Width: " + client.Player.CurrentZone.Width);
					info.Add(" Zone Height: " + client.Player.CurrentZone.Height);
					info.Add(" Zone DivingEnabled: " + client.Player.CurrentZone.IsDivingEnabled);
					info.Add(" Zone Waterlevel: " + client.Player.CurrentZone.Waterlevel);

					zone = WorldMgr.GetZone(client.Player.CurrentZone.ID);
					Zones dbZone = GameServer.Database.SelectObject<Zones>("`ZoneID` = '" + zone.ID + "' AND `RegionID` = '" + zone.ZoneRegion.ID + "'");

					if (dbZone != null)
					{
						string dflag = "Use Region";
						if (dbZone.DivingFlag == 1)
							dflag = "Always Yes";
						else if (dbZone.DivingFlag == 2)
							dflag = "Always No";

						info.Add(" Zone DivingFlag: " + dbZone.DivingFlag + " (" + dflag + ")");
					}

					client.Out.SendCustomTextWindow("[ " + client.Player.CurrentZone.Description + " ]", info);
				}

				if (args[1].ToLower() == "divingflag")
				{
					zone = WorldMgr.GetZone(client.Player.CurrentZone.ID);
					byte divingFlag = Convert.ToByte(args[2]);
					if (divingFlag > 2)
					{
						DisplaySyntax(client);
						return;
					}

					if (divingFlag == 0)
						zone.IsDivingEnabled = client.Player.CurrentRegion.IsRegionDivingEnabled;
					else if (divingFlag == 1)
						zone.IsDivingEnabled = true;
					else
						zone.IsDivingEnabled = false;

					Zones dbZone = GameServer.Database.SelectObject<Zones>("`ZoneID` = '" + zone.ID + "' AND `RegionID` = '" + zone.ZoneRegion.ID + "'");
					dbZone.DivingFlag = divingFlag;
					GameServer.Database.SaveObject(dbZone);

					// Update water level and diving flag for the new zone
					client.Out.SendPlayerPositionAndObjectID();

					string dflag = "Use Region";
					if (dbZone.DivingFlag == 1)
						dflag = "Always Yes";
					else if (dbZone.DivingFlag == 2)
						dflag = "Always No";

					DisplayMessage(client, string.Format("Diving Flag for {0}:{1} changed to {2} ({3}).", zone.ID, zone.Description, divingFlag, dflag));
					return;
				}

				if (args[1].ToLower() == "waterlevel")
				{
					zone = WorldMgr.GetZone(client.Player.CurrentZone.ID);
					int waterlevel = Convert.ToInt32(args[2]);
					zone.Waterlevel = waterlevel;

					Zones dbZone = GameServer.Database.SelectObject<Zones>("`ZoneID` = '" + zone.ID + "' AND `RegionID` = '" + zone.ZoneRegion.ID + "'");
					dbZone.WaterLevel = waterlevel;
					GameServer.Database.SaveObject(dbZone);

					// Update water level and diving flag for the new zone
					client.Out.SendPlayerPositionAndObjectID();

					client.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z + 1, client.Player.Heading);

					DisplayMessage(client, string.Format("Waterlevel for {0}:{1} changed to {2}.", zone.ID, zone.Description, waterlevel));
					return;
				}

				// otherwise set zone bonuses

				//make sure that only numbers are used to avoid errors.
				foreach (char c in string.Join(" ", args, 2, 4))
				{
					if (char.IsLetter(c))
					{
						DisplaySyntax(client);
						return;
					}
				}

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
			catch
			{
				DisplaySyntax(client);
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
