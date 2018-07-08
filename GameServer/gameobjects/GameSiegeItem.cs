/* DAWN OF LIGHT - The first free open source DAoC server emulator
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
*/
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// Items of this class will proc on GameKeepComponent and GameKeepDoors, checked for in GameLiving-CheckWeaponMagicalEffect
    /// Used for Bruiser, or any other item that can fire a proc on keep components.  Itemtemplates must be set to DOL.GS.GameSiegeItem
    /// in the classtype field
    /// </summary>
    public class GameSiegeItem : GameInventoryItem
        {
                private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

                private GameSiegeItem() { }

                public GameSiegeItem(ItemTemplate template)
                        : base(template)
                {
                }

                public GameSiegeItem(ItemUnique template)
                        : base(template)
                {
                }

                public GameSiegeItem(InventoryItem item)
                        : base(item)
                {
                }
        }
}
