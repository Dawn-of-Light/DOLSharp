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
using System.Text;
using System.Reflection;
using DOL.Database;
using DOL.GS.Quests;

namespace DOL.GS.Behaviour
{
    public class BehaviourBuilder
    {
        
        //private MethodInfo addActionMethod;

        public BehaviourBuilder()
        {            
            //this.addActionMethod = questType.GetMethod("AddBehaviour", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);            
        }                

        public void AddBehaviour(QuestBehaviour questPart)
        {            
            //addActionMethod.Invoke(null, new object[] { questPart });
        }        

        public BaseBehaviour CreateBehaviour(GameNPC npc)
        {
            BaseBehaviour behaviour =  new BaseBehaviour(npc);            
            return behaviour;
        }
        
    }
}
