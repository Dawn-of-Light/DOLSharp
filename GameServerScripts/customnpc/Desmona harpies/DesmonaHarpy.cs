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
using System.Linq;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;

namespace DOL.GS.Scripts
{
    public class DesmonaHarpy : AtlantisVisualModelNPC
    {
        public override ushort GetClientVisualModel(GameClient client)
        {
            //Battlegroup Support.
            BattleGroup battleGroup = client.Player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);
            if (battleGroup != null)
            {
                foreach (GamePlayer player in battleGroup.Members.Keys)
                {
                    foreach (GameInventoryItem item in player.Inventory.AllItems)
                    {
                        if (item is DesmonaCoin)
                            return Model; //Livelike models for this mob are 990-992.
                    }
                }

            }

            //Group Support.
            if (client.Player.Group != null)
            {
                foreach (GamePlayer player in client.Player.Group.GetPlayersInTheGroup())
                {
                    foreach (GameInventoryItem item in player.Inventory.AllItems)
                    {
                        if (item is DesmonaCoin)
                            return Model; //Livelike models for this mob are 990-992.
                    }
                }
            }

            //Solo Player Support.
            foreach (GameInventoryItem item in client.Player.Inventory.AllItems)
            {
                if (item is DesmonaCoin)
                    return Model; //Livelike models for this mob are 990-992.
            }

            //GMs should always be able to target/see mobs
            //so we make it invisible still, but targetable with its name.
            if (client.Account.PrivLevel >= (int)ePrivLevel.GM)
                return 665;

            return 666; //Invisible Model.
        }
        protected override AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
        {
            if (target is GamePlayer)
            {
                if (Util.Chance(10))
                {
                    StealToken(target as GamePlayer);
                }
            }

            return base.MakeAttack(target, weapon, style, effectiveness, interruptDuration, dualWield);
        }
        public void StealToken(GamePlayer player)
        {
            foreach (GameInventoryItem item in player.Inventory.AllItems)
            {
                if (item is DesmonaCoin)
                {
                    player.Inventory.RemoveItem(item);
                    player.Out.SendMessage(Name + " stole your " + item.Name + "!", eChatType.CT_Emote, eChatLoc.CL_ChatWindow);
                    return;
                }
            }
        }
    }
}
