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

using DOL.GS.Commands;
using DOL.Database;

namespace DOL.GS.GameEvents
{
    /// <summary>
    /// StartupLocationTest Command allows to trigger some virtual startup location query with different parameters.
    /// </summary>
    [Cmd(
        "&startuploctest",
        ePrivLevel.GM,
        "Test a given Client configuration for starting Location Match.",
        "/startuploctest Version RegionRequestID RealmID ClassID RaceId")]
    public class StartupLocationTest : AbstractCommandHandler, ICommandHandler
    {
        /// <summary>
        /// Command Handling Tests.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        public void OnCommand(GameClient client, string[] args)
        {
            // Needs all arguments
            if (args.Length < 6)
            {
                DisplaySyntax(client);
                return;
            }

            // Parse Arguments
            int version;
            int regionId;
            int realmId;
            int classId;
            int raceId;

            if (!int.TryParse(args[1], out version))
            {
                DisplaySyntax(client);
                return;
            }

            if (!int.TryParse(args[2], out regionId))
            {
                DisplaySyntax(client);
                return;
            }

            if (!int.TryParse(args[3], out realmId))
            {
                DisplaySyntax(client);
                return;
            }

            if (!int.TryParse(args[4], out classId))
            {
                DisplaySyntax(client);
                return;
            }

            if (!int.TryParse(args[5], out raceId))
            {
                DisplaySyntax(client);
                return;
            }

            // Build a temp character with given params.
            var chtmp = new DOLCharacters();
            chtmp.AllowAdd = false;
            chtmp.Region = regionId;
            chtmp.Realm = realmId;
            chtmp.Class = classId;
            chtmp.Race = raceId;

            // Test params againt database value and return results.
            var locs = StartupLocations.GetAllStartupLocationForCharacter(chtmp, (GameClient.eClientVersion)version);

            if (locs.Count < 1)
            {
                DisplayMessage(client, "No configuration for startup locations found with these constraints !");
            }
            else
            {
                foreach (StartupLocation l in locs)
                {
                    DisplayMessage(client, string.Format("--- Loc Id:{0}, X:{1}, Y:{2}, Z:{3}, Head:{4}, Region:{5}", l.StartupLoc_ID, l.XPos, l.YPos, l.ZPos, l.Heading, l.Region));
                }
            }
        }
    }
}
