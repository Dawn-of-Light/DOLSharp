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

using DOL.GS;
using DOL.AI;

namespace DOL.GS
{
	/// <summary>
	/// This class is the template class for all GameSummonedPet
	/// </summary>
	public class GameSummonedPetTemplate : GameNPCTemplate
	{
		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GameObject CreateInstance()
		{
			GameSummonedPet obj = new GameSummonedPet();
			obj.Name = m_Name;
			obj.Realm = m_Realm;
			obj.Model = m_Model;
			obj.Level = m_Level;
			obj.GuildName = m_guildName;
			obj.MaxSpeedBase = m_maxSpeedBase;
			obj.Size = m_size;
			obj.Flags = m_flags;
			obj.MeleeDamageType = m_meleeDamageType;
			obj.EvadeChance = m_evadeChance;
			obj.BlockChance = m_blockChance;
			obj.ParryChance = m_parryChance;
			obj.LeftHandSwingChance = m_leftHandSwingChance;
			obj.Inventory = NPCInventoryMgr.GetNPCInventory(m_inventoryID);
			if (obj.Inventory != null)
			{
				if (obj.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
					obj.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
				else if (obj.Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
					obj.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			}
			obj.OwnBrain = m_ownBrainTemplate.CreateInstance();
			return obj;
		}
	}
}
