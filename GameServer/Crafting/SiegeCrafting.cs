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

namespace DOL.GS
{
	/// <summary>
	/// SiegeCrafting is the crafting skill to make siege weapon like catapult, balista,...
	/// </summary>
	public class SiegeCrafting : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SiegeCrafting()
			: base()
		{
			Icon = 0x03;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.Siegecraft");
			eSkill = eCraftingSkill.SiegeCrafting;
		}
		public override string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "Siegecrafter";
			}
		}

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, DBCraftedItem craftItemData)
		{
            return true;
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, item)))
			{
				player.GainCraftingSkill(eCraftingSkill.SiegeCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}
		protected override void BuildCraftedItem(GamePlayer player, DBCraftedItem craftItemData)
		{
			GameSiegeWeapon siegeweapon = null;
			switch ((eObjectType)craftItemData.ItemTemplate.Object_Type)
			{
				case eObjectType.SiegeBalista:
					{
						siegeweapon = new GameSiegeBallista();
					}
					break;
				case eObjectType.SiegeCatapult:
					{
						siegeweapon = new GameSiegeCatapult();
					}
					break;
				case eObjectType.SiegeCauldron:
					{
						siegeweapon = new GameSiegeCauldron();
					}
					break;
				case eObjectType.SiegeRam:
					{
						siegeweapon = new GameSiegeRam();
					}
					break;
				case eObjectType.SiegeTrebuchet:
					{
						siegeweapon = new GameSiegeTrebuchet();
					}
					break;
				default:
					{
						base.BuildCraftedItem(player, craftItemData);
						return;
					}
			}

			//actually stores the Id_nb of the siegeweapon
			siegeweapon.ItemId = craftItemData.ItemTemplate.Id_nb;

			siegeweapon.LoadFromDatabase(craftItemData.ItemTemplate);
			siegeweapon.CurrentRegion = player.CurrentRegion;
			siegeweapon.Heading = player.Heading;
			siegeweapon.X = player.X;
			siegeweapon.Y = player.Y;
			siegeweapon.Z = player.Z;
			siegeweapon.Realm = player.Realm;
			siegeweapon.AddToWorld();
		}
	}
}
