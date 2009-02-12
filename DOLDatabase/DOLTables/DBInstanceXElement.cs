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
    /// 
    /// </summary>
    [DataTable(TableName = "InstanceXElement")]
    public class DBInstanceXElement : DataObject
    {
        public DBInstanceXElement()
        {
        }

        /// <summary>
        /// The unqiue name of this instance. Eg 'TaskDungeon001' or 'Quest_FlayingTheLol'
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public String InstanceID
        {
            get { return m_instanceID; }
            set { m_instanceID = value; }
        }

        protected string m_instanceID;

        /// <summary>
        /// The ClassType of this element!
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public String ClassType
        {
            get { return m_classType; }
            set { m_classType = value; }
        }

        protected string m_classType;

        protected int m_X, m_Y, m_Z;
        protected ushort m_Heading;

        [DataElement(AllowDbNull = false)]
        public int X
        { get { return m_X; } set { m_X = value; } }

        [DataElement(AllowDbNull = false)]
        public int Y
        { get { return m_Y; } set { m_Y = value; } }

        [DataElement(AllowDbNull = false)]
        public int Z
        { get { return m_Z; } set { m_Z = value; } }

        [DataElement(AllowDbNull = false)]
        public ushort Heading
        { get { return m_Heading; } set { m_Heading = value; } }

        private int m_NPCTemplate;

        /// <summary>
        /// Where applicable, the npc template to create this mob from.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int NPCTemplate
        { get { return m_NPCTemplate; } set { m_NPCTemplate = value; } }

        override public bool AutoSave
        {
            get { return true; }
            set { }
        }

        [Relation(LocalField = "PathID", RemoteField = "PathID", AutoLoad = true, AutoDelete = true)]
        public DBPathPoint[] PathPoints;
    }
}
