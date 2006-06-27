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
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// Summary description for a Poison
    /// </summary> 
    public class CatapultAmmunition : MagicalStackableItem
    {
        #region Function
        /// <summary>
        /// Checks if the object can stack with the param
        /// </summary>
        public override bool CanStackWith(StackableItem item)
        {
            CatapultAmmunition ammo = item as CatapultAmmunition;
            if (ammo == null) return false;
            return base.CanStackWith(item);
        }

        /// <summary>
        /// Called when the item is used
        /// </summary>
        /// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
        public override bool OnItemUsed(byte type)
        {
            if (!base.OnItemUsed(type)) return false;

            Owner.Out.SendMessage("You can't use catapult ammunition this way.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            return false;

            //TODO: Check if this method is supposed to remain as is or handle some stuff with gamesiegecatapult.cs
        }

        #endregion

        /// <summary>
        /// Gets the object type of the item (for test use class type instead of this propriety)
        /// </summary>
        public override eObjectType ObjectType
        {
            get { return eObjectType.SiegeAmmunition; }
        }
    }
}	
