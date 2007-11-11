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
using System.Collections.Generic;
using System.Text;
using log4net;
using System.Reflection;
using System.Collections;
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// This is a preliminary loot generator for artifact scrolls.
    /// Basically, any mob in ToA can drop scrolls, which scroll they
    /// drop, will depend on the level of the mob,
    /// level 45-50: 1 of 3
    /// level 50-55: 2 of 3
    /// level 55+: 3 of 3
    /// </summary>
    /// <author>Aredhel</author>
    class LootGeneratorScroll : LootGeneratorBase
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LootGeneratorScroll()
            : base() { }

        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList lootList = new LootList();
            
            if (mob.CurrentRegion.Description == "Atlantis" && mob.Level >= 45 && Util.Chance(25))
            {
                ArrayList artifacts = new ArrayList();
                switch (mob.CurrentZone.Description)
                {
                    case "Oceanus Hesperos":
                    case "Mesothalassa":
                        artifacts = ArtifactMgr.GetArtifactsFromZone("Oceanus");
                        break;
                    case "Oceanus Boreal":
                    case "Stygian Delta":
                    case "Land of Atum":
                        artifacts = ArtifactMgr.GetArtifactsFromZone("Stygia");
                        break;
                    case "Oceanus Notos":
                    case "Arbor Glen":
                    case "Green Glades":
                        artifacts = ArtifactMgr.GetArtifactsFromZone("Aerus");
                        break;
                    case "Oceanus Anatole":
                    case "Typhon's Reach":
                    case "Ashen Isles":
                        artifacts = ArtifactMgr.GetArtifactsFromZone("Volcanus");
                        break;
                }

                if (artifacts.Count > 0)
                {
                    String bookID = (artifacts[Util.Random(artifacts.Count-1)] as Artifact).BookID;
                    int pageNumber;

                    if (mob.Level >= 55)
                        pageNumber = 3;
                    else if (mob.Level >= 50)
                        pageNumber = 2;
                    else
                        pageNumber = 1;

                    ItemTemplate loot = new ItemTemplate();
                    loot.Model = 488;
                    loot.Name = "scroll|" + bookID + "|" + pageNumber;
                    loot.Level = 35;

                    lootList.AddFixed(loot, 1);
                }
            }

            return lootList;
        }
    }
}