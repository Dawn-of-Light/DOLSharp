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
using System.Collections.Generic;
using log4net;

namespace DOL.GS.ServerRules
{
    /// <summary>
    /// Handles Adventure Wings (Catacombs) Instance Jump Point.
    /// Find available Instance, Create If Needed
    /// Clean up remaining instance before timer countdown
    /// </summary>
    public class AdventureWingJumpPoint : IJumpPointHandler
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Decides whether player can jump to the target point.
        /// All messages with reasons must be sent here.
        /// Can change destination too.
        /// </summary>
        /// <param name="targetPoint">The jump destination</param>
        /// <param name="player">The jumping player</param>
        /// <returns>True if allowed</returns>
        public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
        {

            // Handles zoning INTO an instance.
            AdventureWingInstance previousInstance = null;

            // Do we have a group ?
            if (player.Group != null)
            {
                // Check if there is an instance dedicated to this group
                foreach (Region region in WorldMgr.GetAllRegions())
                {
                    if ((region as AdventureWingInstance)?.Group != null && ((AdventureWingInstance)region).Group == player.Group)
                    {
                        // Our group has an instance !
                        previousInstance = (AdventureWingInstance)region;
                        break;
                    }

                    if ((region as AdventureWingInstance)?.Player != null && ((AdventureWingInstance)region).Player == player.Group.Leader)
                    {
                        // Our leader has an instance !
                        previousInstance = (AdventureWingInstance)region;
                        previousInstance.Group = player.Group;
                        break;
                    }
                }
            }
            else {
                // I am solo !
                // Check if there is an instance dedicated to me
                foreach (Region region in WorldMgr.GetAllRegions())
                {
                    if (region is AdventureWingInstance instance && instance.Player != null && instance.Player == player)
                    {
                        // I have an Instance !
                        previousInstance = instance;
                        previousInstance.Group = player.Group;
                        break;
                    }
                }
            }

            if (previousInstance != null)
            {
                // We should check if we can go in !
                if (previousInstance.Skin != targetPoint.TargetRegion)
                {
                    // we're trying to enter in an other instance and we still have one !
                    // check if previous one is empty
                    if (previousInstance.NumPlayers > 0)
                    {
                        // We can't jump !
                        player.Out.SendMessage($"You have another instance ({previousInstance.Description}) running with people in it !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    Log.Warn($"Player : {player.Name} requested new Instance, destroying instance {previousInstance.Description}, ID: {previousInstance.ID}, type={previousInstance.GetType()}.");
                    WorldMgr.RemoveInstance(previousInstance);
                    previousInstance = null;
                }
            }

           if (previousInstance == null)
           {
                // I have no instance to go to, create one !
                previousInstance = (AdventureWingInstance)WorldMgr.CreateInstance(targetPoint.TargetRegion, typeof(AdventureWingInstance));
                if (targetPoint.SourceRegion != 0 && targetPoint.SourceRegion == player.CurrentRegionID) {
                    // source loc seems legit...
                    previousInstance.SourceEntrance = new GameLocation("source", targetPoint.SourceRegion, targetPoint.SourceX, targetPoint.SourceY, targetPoint.SourceZ);
                }

                if (player.Group != null)
                {
                    previousInstance.Group = player.Group;
                    previousInstance.Player = player.Group.Leader;
                }
                else
                {
                    previousInstance.Group = null;
                    previousInstance.Player = player;
                }

                // get region data
                long mobs = 0;
                long merchants = 0;
                long items = 0;
                long bindpoints = 0;

                previousInstance.LoadFromDatabase(previousInstance.RegionData.Mobs, ref mobs, ref merchants, ref items, ref bindpoints);

                if (Log.IsInfoEnabled)
                {
                    Log.Info($"Total Mobs: {mobs}");
                    Log.Info($"Total Merchants: {merchants}");
                    Log.Info($"Total Items: {items}");
                }

                // Attach Loot Generator
                LootMgr.RegisterLootGenerator(new LootGeneratorAurulite(), null, null, null, previousInstance.ID);

                // Player created new instance
                // Destroy all other instance that should be...
                List<Region> toDelete = new List<Region>();
                foreach (Region region in WorldMgr.GetAllRegions())
                {
                    if (region is AdventureWingInstance toClean && toClean != previousInstance)
                    {
                        // Won't clean up populated Instance
                        if (toClean.NumPlayers == 0)
                        {
                            if (toClean.Group != null && player.Group != null && toClean.Group == player.Group)
                            {
                                // Got another instance for the same group... Destroy it !
                                toDelete.Add(toClean);
                            }
                            else if (toClean.Player != null && (toClean.Player == player || (player.Group != null && player.Group.Leader == toClean.Player)))
                            {
                                // Got another instance for the same player... Destroy it !
                                toDelete.Add(toClean);
                            }
                            else if (toClean.Group == null && toClean.Player == null)
                            {
                                // nobody owns this instance anymore
                                toDelete.Add(toClean);
                            }
                        }
                    }
                }

                // enumerate to_delete
                foreach (Region region in toDelete)
                {
                    Log.Warn($"Player : {player.Name} has provoked an instance cleanup - {region.Description}, ID: {region.ID}, type={region.GetType()}.");
                    WorldMgr.RemoveInstance((BaseInstance)region);
                }
            }

            // get loc of instance
            var loc = new GameLocation($"{previousInstance.Description} (instance)", previousInstance.ID, targetPoint.TargetX,  targetPoint.TargetY,  targetPoint.TargetZ,  targetPoint.TargetHeading);

            // Move Player, changing target destination is failing !!
            player.MoveTo(loc);
            return false;
        }
    }
}
