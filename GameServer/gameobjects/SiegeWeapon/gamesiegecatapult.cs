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
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// GameMovingObject is a base class for boats and siege weapons.
	/// </summary>
	public  class GameSiegeCatapult : GameSiegeWeapon
	{
		public GameSiegeCatapult() : base()
		{
			MeleeDamageType = eDamageType.Crush;
			Name = "catapult";
			AmmoType = 0x1301;
			this.Effect = 0x89C;
			this.Model = 0xA26;
			ActionDelay = new int[]
			{
				0,//none
				5000,//aiming
				10000,//arming
				0,//loading
				2700//fireing
			};//en ms
			/*SpellLine siegeWeaponSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.SiegeWeapon_Spells);
			IList spells = SkillBase.GetSpellList(siegeWeaponSpellLine.KeyName);
			if (spells != null)
			{
				foreach (Spell spell in spells) 
				{
					if (spell.ID == 2430) //TODO good id for catapult
					{
						if(spell.Level <= Level)
						{
							m_spellHandler = ScriptMgr.CreateSpellHandler(this, spell, siegeWeaponSpellLine);
						}
						break;
					}
				}
			}*/
		}
		public override void DoDamage()
		{
			InventoryItem ammo = this.Ammo[AmmoSlot] as InventoryItem;
			//todo remove ammo + spell in db and uncomment
			//m_spellHandler.StartSpell(player);
			base.DoDamage();//anim mut be called after damage
		}
		public override bool ReceiveItem(GameLiving source, DOL.Database.InventoryItem item)
		{
			//todo check if bullet
			return base.ReceiveItem (source, item);
		}
	}
}
