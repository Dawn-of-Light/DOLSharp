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

namespace DOL.GS.Database
{
    public class DBNPC
    {
        private int m_id;
        private string m_NPCtype;
        private string m_name;//can override npc template name for npc
        private int m_x;
        private int m_y;
        private int m_z;
        private int m_heading;
        private int m_region;
        private int m_templateID;
        private string m_NPCTypeParameters;//exemple item list for merchant
        private string m_brainClass;
        private string m_brainParams;

        public DBNPC()
        {
            m_NPCtype = "DOL.GS.GameMerchant";
            m_NPCTypeParameters = "";
            m_brainClass = ""; // the default is take in Template
        }

        public int NPCID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        public string ClassType
        {
            get
            {
                return m_NPCtype;
            }
            set
            {
                m_NPCtype = value;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public int X
        {
            get
            {
                return m_x;
            }
            set
            {
                m_x = value;
            }
        }

        public int Y
        {
            get
            {
                return m_y;
            }
            set
            {
                m_y = value;
            }
        }

        public int Z
        {
            get
            {
                return m_z;
            }
            set
            {
                m_z = value;
            }
        }

        public int Heading
        {
            get
            {
                return m_heading;
            }
            set
            {
                m_heading = value;
            }
        }

        public int Region
        {
            get
            {
                return m_region;
            }
            set
            {
                m_region = value;
            }
        }
        
        public int TemplateID
        {
            get
            {
                return m_templateID;
            }
            set
            {
                m_templateID = value;
            }
        }

        public string NPCTypeParameters
        {
            get
            {
                return m_NPCTypeParameters;
            }
            set
            {
                m_NPCTypeParameters = value;
            }
        }

        public string BrainClass
        {
            get
            {
                return m_brainClass;
            }
            set
            {
                m_brainClass = value;
            }
        }

        public string BrainParams
        {
            get
            {
                return m_brainParams;
            }
            set
            {
                m_brainParams = value;
            }
        }
    }
}

