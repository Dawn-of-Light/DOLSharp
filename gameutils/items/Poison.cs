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
using DOL.GS.Scripts;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a Poison
	/// </summary> 
	public class Poison : MagicalStackableItem
    {
        #region Function
        /// <summary>
        /// Checks if the object can stack with the param
        /// </summary>
        public override bool CanStackWith(IStackableItem item)
        {
            Poison poison = item as Poison;
            if (poison == null) return false;

            return (base.CanStackWith(item));
        }

		/// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public override bool OnItemUsed(byte type)
		{
			if(! base.OnItemUsed(type)) return false;
			
			Weapon toItem = Owner.AttackWeapon;
			if(toItem == null)
			{
				toItem = Owner.Inventory.GetItem(eInventorySlot.LeftHandWeapon) as Weapon;
				if(toItem == null)
				{
					Owner.Out.SendMessage("Poisons can be applied only to weapons.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;		
				}
			}
							
			Owner.ApplyPoison(this, toItem);				
							
			return true;
		}

		#endregion
	
		/// <summary>
		/// Gets the object type of the item (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Poison; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = (ArrayList) base.DelveInfo;

				if (SpellID != 0)
				{
					SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
					if (poisonLine != null)
					{
						IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
						if (spells != null)
						{
							foreach(Spell spl in spells)
							{
								if(spl.ID == SpellID)
								{
									list.Add(" ");
									list.Add("Level Requirement:");
									list.Add("- " + spl.Level + " Level");
									list.Add(" ");
									list.Add("Offensive Proc Ability:");
									//removed due to poisons don't have charges
									//list.Add("- " + item.Charge+ " Charges");
									//list.Add("- " + item.MaxCharge+ " Max");
									list.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spl, poisonLine);
									if(spellHandler != null)
									{
										list.AddRange(spellHandler.DelveInfo);
									}
									else
									{
										list.Add("-"+ spl.Name +" (Not implemented yet)");
									}
									break;
								}
							}
						}
					}
				}
				
				return list;
			}
		}
	}
}	