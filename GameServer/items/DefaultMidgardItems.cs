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
using System.Reflection;
using DOL.Events;
using DOL.Database;
using log4net;

namespace DOL.GS.Items
{
	/// <summary>
	/// Creates needed items when they doesnt exist yet
	/// </summary>
	public class DefaultMidgardItems
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args) 
		{
			ItemTemplate practice_sword_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "practice_sword");
			if (practice_sword_template==null)
			{
				practice_sword_template = new ItemTemplate();
				practice_sword_template.Name = "practice sword";
				practice_sword_template.Level = 0;
				practice_sword_template.Durability=100;
				practice_sword_template.MaxDurability=100;
				practice_sword_template.Condition = 50000;
				practice_sword_template.MaxCondition = 50000;
				practice_sword_template.Quality = 90;
				practice_sword_template.MaxQuality = 90;
				practice_sword_template.DPS_AF = 12;
				practice_sword_template.SPD_ABS = 25;
				practice_sword_template.Hand = 0;
				practice_sword_template.Type_Damage = 2;
				practice_sword_template.Object_Type = 3;
				practice_sword_template.Item_Type = 10;
				practice_sword_template.Weight = 10;
				practice_sword_template.Model = 3;
				practice_sword_template.IsPickable = true; 
				practice_sword_template.IsDropable = false; 
				practice_sword_template.Id_nb = "practice_sword";
				GameServer.Database.AddNewObject(practice_sword_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + practice_sword_template.Id_nb);
			}
		}
	}
}