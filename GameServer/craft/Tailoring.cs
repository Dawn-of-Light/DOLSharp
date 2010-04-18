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
using DOL.Language;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS
{
    public class Tailoring : AbstractProfession
    {
        public Tailoring()
        {
            Icon = 0x0B;
            Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Tailoring");
            eSkill = eCraftingSkill.Tailoring;
        }

        protected override String Profession
        {
            get
            {
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,
                    "CraftersProfession.Tailor");
            }
        }

        /// <summary>
        /// Check if  the player own has all the needed tools.
        /// </summary>
        /// <param name="player">The crafting player</param>
        /// <param name="craftItemData">The object in construction</param>
        /// <returns>True if the player hold all needed tools</returns>
		protected override bool CheckForTools(GamePlayer player, DBCraftedItem craftItemData)
        {
            bool needForge = false;

            foreach (DBCraftedXItem rawmaterial in craftItemData.RawMaterials)
            {
                if (rawmaterial.ItemTemplate.Model == 519) // metal bar
                {
                    needForge = true;
                    break;
                }
            }

            if (needForge)
            {
                foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
                {
                    if (item.Name == "forge" || item.Model == 478) // Forge
                        return true;
                }

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, 
                    "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, 
                    "Crafting.CheckTool.FindForge"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return false;
            }
            return true;

        }

        /// <summary>
        /// Calculate the minumum needed secondary crafting skill level to make the item
        /// </summary>
        public override int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem item)
        {
            switch (item.ItemTemplate.Object_Type)
            {
                case (int)eObjectType.Cloth:
                case (int)eObjectType.Leather:
                case (int)eObjectType.Studded:
                    return item.CraftingLevel - 30;
            }

            return base.GetSecondaryCraftingSkillMinimumLevel(item);
        }

        /// <summary>
        /// Select craft to gain a point in and increase it.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
        {
            if (Util.Chance(CalculateChanceToGainPoint(player, item)))
            {
                player.GainCraftingSkill(eCraftingSkill.Tailoring, 1);
                base.GainCraftingSkillPoints(player, item);
                player.Out.SendUpdateCraftingSkills();
            }
        }
    }
}
