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
//Author (aka who to blame): Dinberg if its good, Graveen if something goes wrong ^^
using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// This table represents instances, with an entry for each element (instance type, objects, mobs, entrances, etc) in an instance.
	/// </summary>
	[DataTable(TableName = "InstanceXElement")]
	public class DBInstanceXElement : DataObject
	{
	    public DBInstanceXElement()
		{
		}

		/// <summary>
		/// The unique name of this instance. Eg 'My Task Dungeon'
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public string InstanceID { get; set; }

	    [DataElement(AllowDbNull = true)]
		public string ClassType { get; set; }

	    [DataElement(AllowDbNull = false)]
		public int X { get; set; }

	    [DataElement(AllowDbNull = false)]
		public int Y { get; set; }

	    [DataElement(AllowDbNull = false)]
		public int Z { get; set; }

	    [DataElement(AllowDbNull = false)]
		public ushort Heading { get; set; }

	    /// <summary>
		/// Where applicable, the npc template to create this mob from.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 255)]
		public string NPCTemplate { get; set; }

	    /// <summary>
		/// Convert the NPCTemplate to/from int, assuming a single ID
		/// </summary>
		public int NPCTemplateID
		{
			get
			{
				int i = 0;
				int.TryParse(NPCTemplate, out i);
				return i;
			}

			set
			{
				NPCTemplate = value.ToString();
			}
		}

	}
}
