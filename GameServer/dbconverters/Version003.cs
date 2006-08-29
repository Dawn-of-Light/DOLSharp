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
using System.Collections;
using log4net;
using DOL.Database;

namespace DOL.GS.DatabaseConverters
{
	/// <summary>
	/// Converts the database format to the version 3
	/// </summary>
	[DatabaseConverter(3)]
	public class Version003 : IDatabaseConverter
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// we need to make use of the new poison fields
		/// </summary>
		public void ConvertDatabase()
		{
			log.Info("Database Version 3 Convert Started");

			ItemTemplate[] templates = (ItemTemplate[])GameServer.Database.SelectObjects(typeof(ItemTemplate), "SpellID != 0");

			int count = 0;
			foreach (ItemTemplate template in templates)
			{
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
					if (spells != null)
					{
						foreach (Spell spl in spells)
						{
							if (spl.ID == template.SpellID)
							{
								template.PoisonSpellID = template.SpellID;
								template.SpellID = 0;
								template.PoisonCharges = template.Charges;
								template.Charges = 0;
								template.PoisonMaxCharges = template.MaxCharges;
								template.MaxCharges = 0;
								GameServer.Database.SaveObject(template);
								count++;
								break;
							}
						}
					}
				}
			}

			log.Info("Converted " + count + " templates");

			InventoryItem[] items = (InventoryItem[])GameServer.Database.SelectObjects(typeof(InventoryItem), "SpellID != 0");
			count = 0;
			foreach (InventoryItem item in items)
			{
				foreach (ItemTemplate template in templates)
				{
					SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
					if (poisonLine != null)
					{
						IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == template.SpellID)
								{
									template.PoisonSpellID = template.SpellID;
									template.SpellID = 0;
									template.PoisonCharges = template.Charges;
									template.Charges = 0;
									template.PoisonMaxCharges = template.MaxCharges;
									template.MaxCharges = 0;
									GameServer.Database.SaveObject(template);
									count++;
									break;
								}
							}
						}
					}
				}
			}

			log.Info("Converted " + count + " items");

			log.Info("Database Version 3 Convert Finished");
		}
	}
}
