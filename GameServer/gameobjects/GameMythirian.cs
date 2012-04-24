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
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
        public class GameMythirian : GameInventoryItem
        {
                private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

                private GameMythirian() { }

                public GameMythirian(ItemTemplate template)
                        : base(template)
                {
                }

                public GameMythirian(ItemUnique template)
                        : base(template)
                {
                }

                public GameMythirian(InventoryItem item)
                        : base(item)
                {
                }

                public override bool CanEquip(GamePlayer player)
                {
                        if (base.CanEquip(player))
                        {
                                if (Type_Damage <= player.ChampionLevel)
                                {
                                        return true;
                                }

                                // maybe display some message to player here?
                        }

                        return false;
                }

      }
}
 