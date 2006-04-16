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
	/// Summary description for a PoisonTemplate
	/// </summary> 
	public class PoisonTemplate : MagicalStackableItemTemplate
    {
        /// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Poison; }
		}

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			Poison item = new Poison();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight;
			item.Value = m_value;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
            item.Count = m_packSize;
			item.MaxCount = m_maxCount;
			item.SpellID = m_spellID;
			return item;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList GetDelveInfo(GamePlayer player)
		{
			ArrayList list = (ArrayList) base.GetDelveInfo(player);

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

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(player, spl, poisonLine);
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