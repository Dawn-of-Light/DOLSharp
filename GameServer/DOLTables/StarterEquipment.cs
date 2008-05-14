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

using DOL.Database2;


namespace DOL.Database2
{
	/// <summary>
	/// Holds starter equipment
	/// </summary>
	[Serializable]//TableName = "StarterEquipment")]
	public class StarterEquipment : DatabaseObject
	{
		private byte m_class;
		private string m_templateID;

		private static bool m_autoSave;
        public StarterEquipment() : base() { }
		/// <summary>
		/// The class this item should be given to, 0 for all
		/// </summary>
		
		public byte Class
		{
			get
			{
				return m_class;
			}
			set
			{
				m_Dirty = true;
				m_class = value;
			}
		}

		/// <summary>
		/// The template ID of free item
		/// </summary>
		
		public string TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				m_templateID = value;
				m_Dirty = true;
			}
		}

		/// <summary>
		/// The ItemTemplate for this record
		/// </summary>
		//[Relation(LocalField = "TemplateID", RemoteField = "Id_nb", AutoLoad = true, AutoDelete = false)]
		public ItemTemplate Template;
        public override void FillObjectRelations()
        {
            Template = (ItemTemplate)DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), TemplateID);
            base.FillObjectRelations();
        }
	}
}
