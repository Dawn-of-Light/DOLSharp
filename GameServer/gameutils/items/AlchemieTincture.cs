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
	/// Summary description for a AlchemieTincture
	/// </summary> 
	public abstract class AlchemieTincture : GenericItem
	{
		#region Declaraction
		/// <summary>
		/// The spellID used by the object
		/// </summary>
		private int m_spellID;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the spellId used by this tincture
		/// </summary>
		public int SpellID
		{
			get { return m_spellID; }
			set	{ m_spellID = value; }
		}

		#endregion

		/// <summary>
		/// Gets the object type of the tincture (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.AlchemyTincture; }
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
					SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (spellLine != null)
					{
						IList spells = SkillBase.GetSpellList(spellLine.KeyName);
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

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spl, spellLine);
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