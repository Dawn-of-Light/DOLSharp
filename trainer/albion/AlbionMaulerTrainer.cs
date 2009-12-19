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
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Database;

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Mauler Trainer
    /// </summary>	
    [NPCGuildScript("Mauler Trainer", eRealm.Albion)]
    public class AlbionMaulerTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.Mauler_Alb; }
        }

        /// <summary>
        /// Interact with trainer
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            // check if class matches.				
            if (player.CharacterClass.ID > 59 && player.CharacterClass.ID < 63)
            {
                // popup the training window
                player.Out.SendTrainerWindow();
                //player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Temple of the Iron Fist] and fight for the glorious realm of Albion?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					if (!player.IsLevelRespecUsed)
					{
						OfferRespecialize(player);
					}
				}
                else
                {
					DismissPlayer(player);
				}
            }
            return true;
        }

        /// <summary>
        /// checks wether a player can be promoted or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanPromotePlayer(GamePlayer player)
        {
            return (player.Level >= 5 && player.CharacterClass.ID == (int)eCharacterClass.Fighter && (player.Race == (int)eRace.Briton
                || player.Race == (int)eRace.AlbionMinotaur));
        }

        /// <summary>
        /// Talk to trainer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text)) return false;
            GamePlayer player = source as GamePlayer;

            switch (text)
            {
                case "join the Temple of the Iron Fist":
                    // promote player to other class
                    if (CanPromotePlayer(player))
                    {
                        // loose all spec lines
                        player.RemoveAllSkills();
                        player.RemoveAllStyles();

                        // Mauler_Alb = 60
                        PromotePlayer(player, (int)eCharacterClass.Mauler_Alb, "Welcome young Mauler. May your time in Albion be rewarding.", null);

                        // drop any equiped-non usable item, in inventory or on the ground if full
                        lock (player.Inventory)
                        {
                            foreach (InventoryItem item in player.Inventory.EquippedItems)
                            {
                                if (!player.HasAbilityToUseItem(item))
                                    if (player.Inventory.IsSlotsFree(item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == true)
                                        player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item.Count);
                                    else
                                        player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, eInventorySlot.Ground, item.Count);
                            }
                        }
                    }
                    break;
            }
            return true;
        }
    }
}
