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
	/// Summary description for a Potion
	/// </summary> 
	public class Potion : MagicalItem
	{
		#region Function
        
        /// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public override bool OnItemUsed(byte type)
		{
			if(! base.OnItemUsed(type)) return false;
			
			SpellLine potionEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
			if (potionEffectLine != null)
			{
				IList spells = SkillBase.GetSpellList(potionEffectLine.KeyName);
				if (spells != null)
				{
					foreach (Spell spell in spells)
					{
						if (spell.ID == SpellID)
						{
							if(spell.Level <= Level)
							{
								if(spell.CastTime > 0 && Owner.InCombat)
								{
									Owner.Out.SendMessage("You can't use this item in combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spell, potionEffectLine);
									if (spellHandler != null)
									{
										Owner.Emote(eEmote.Drink);
										spellHandler.StartSpell(Owner.TargetObject as GameLiving);
										Charge--;
                                        if (Charge < 1) Owner.Inventory.RemoveItem(this);
										Owner.Out.SendMessage(Name+" has been used.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
									else
									{
										Owner.Out.SendMessage("Potion effect ID " + spell.ID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
								}
							}
							break;
						}
					}
				}
			}

			return true;
		}

		#endregion
		
	}
}	