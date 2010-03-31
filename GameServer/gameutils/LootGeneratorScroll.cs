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
    /// level 51-55: 2 of 3
    /// level 56+: 3 of 3
	/// 
	/// Note: this generates a placeholder scroll ... GameNPC DropLoot generates the actual scroll
	/// 
    /// </summary>
    /// <author>Aredhel</author>
    class LootGeneratorScroll : LootGeneratorBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LootGeneratorScroll()
            : base() { }

        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
			LootList lootList = new LootList();

			// check to see if we are in ToA
			if (mob.CurrentRegion.Expansion == (int)eExpansion.ToA && killer.CurrentRegion.Expansion == (int)eExpansion.ToA)
			{
				if (mob.Level >= 45 && Util.Chance(ServerProperties.Properties.SCROLL_DROP_RATE))
				{
					List<Artifact> artifacts = new List<Artifact>();

					if (mob.CurrentRegion.IsDungeon && Util.Chance((int)(ServerProperties.Properties.SCROLL_DROP_RATE * 1.5)))
					{
						artifacts = ArtifactMgr.GetArtifacts();
					}
					else
					{
						switch (mob.CurrentZone.Description)
						{
							case "Oceanus Hesperos":
							case "Mesothalassa":
							case "Oceanus Notos":
							case "Oceanus Boreal":
							case "Oceanus Anatole":
								artifacts = ArtifactMgr.GetArtifacts("Oceanus");
								break;
							case "Stygian Delta":
							case "Land of Atum":
								artifacts = ArtifactMgr.GetArtifacts("Stygia");
								break;
							case "Arbor Glen":
							case "Green Glades":
								artifacts = ArtifactMgr.GetArtifacts("Aerus");
								break;
							case "Typhon's Reach":
							case "Ashen Isles":
								artifacts = ArtifactMgr.GetArtifacts("Volcanus");
								break;
						}
					}

					if (artifacts.Count > 0)
					{
						String artifactID = (artifacts[Util.Random(artifacts.Count - 1)]).ArtifactID;
						int pageNumber;

						ItemTemplate loot = new ItemTemplate();

						if (mob.Level > 55)
						{
							pageNumber = 3;
						}
						else if (mob.Level >= 51)
						{
							pageNumber = 2;
						}
						else
						{
							pageNumber = 1;
						}

						loot.Model = 488;
						loot.Name = "scroll|" + artifactID + "|" + pageNumber;
						loot.Level = 35;

						loot.IsPickable = true;
						loot.IsDropable = true;
						loot.IsTradable = true;

						lootList.AddFixed(loot, 1);
					}
				}
			}

            return lootList;
        }
    }
}