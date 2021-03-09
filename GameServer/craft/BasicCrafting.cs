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

using DOL.Database;
using DOL.Language;

namespace DOL.GS
{
	public class BasicCrafting : AbstractProfession
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BasicCrafting()
		{
			Icon = 0x0F;
            Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.BasicCrafting");
            eSkill = eCraftingSkill.BasicCrafting;
		}

        protected override String Profession
        {
            get
            {
                return "CraftersProfession.BasicCrafter";
            }
        }

        public override string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "Crafter's";
            }
		}

		public override void GainCraftingSkillPoints(GamePlayer player, Recipe recipe)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, recipe.Level)))
			{
				player.GainCraftingSkill(eCraftingSkill.BasicCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
