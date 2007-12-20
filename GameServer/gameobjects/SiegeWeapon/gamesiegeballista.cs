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
using DOL.GS.Spells;
using System.Collections;
/*1,Ballista,1,ammo,0.46,1
2,Catapult,2,ammo,0.39,1
3,Trebuchet,2,ammo,1.03,1
4,Scorpion,1,ammo,0.22,1
5,Oil,3,ammo,0,1*/
/*ID,Textual Name,Load start,Load End,Fire Start,Fire End,Release Point
1,Ballista,30,60,0,30,12
2,Ram,30,60,0,30,12
3,Mangonel,30,200,0,30,12
4,trebuchet,97,180,0,96,
5,Ballista Low,11,30,0,10,
6,Ballista High,20,90,0,19,
7,Catapult W,40,111,0,40,
8,Catapult G,40,111,0,40,
9,Ram High,0,12,13,80,
10,Ram Mid,0,12,13,80,
11,Ram Low,0,12,13,80,*/
namespace DOL.GS
{
	/// <summary>
	/// GameMovingObject is a base class for boats and siege weapons.
	/// </summary>
	public class GameSiegeBallista : GameSiegeWeapon
	{
		public GameSiegeBallista()
			: base()
		{
			MeleeDamageType = eDamageType.Thrust;
			Name = "field ballista";
			AmmoType = 0x18;
			this.Model = 0x0A55;
			this.Effect = 0x089A;
			ActionDelay = new int[]{
				0,//none
				5000,//aiming
				10000,//arming
				0,//loading
				1100//fireing
			};//en ms
			/*SpellLine siegeWeaponSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.SiegeWeapon_Spells);
			IList spells = SkillBase.GetSpellList(siegeWeaponSpellLine.KeyName);
			if (spells != null)
			{
				foreach (Spell spell in spells)
				{
					if (spell.ID == 2430) //TODO good id for balista
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
			//todo remove ammo + spell in db and uncomment
			//m_spellHandler.StartSpell(player);
			base.DoDamage();//anim mut be called after damage
		}
		public override bool ReceiveItem(GameLiving source, DOL.Database.InventoryItem item)
		{
			//todo check if bullet
			return base.ReceiveItem(source, item);
		}

	}
}
