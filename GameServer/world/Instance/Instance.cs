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

//Instance devised by Dinberg 
//     - there will probably be questions, direct them to dinberg_darktouch@hotmail.co.uk ;)
using System;
using System.Collections;
using System.Text;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// The Instance is an implementation of BaseInstance that contains additional functionality to load
    /// a template from InstanceXElement.
    /// </summary>
    public class Instance : BaseInstance
    {
        /// <summary>
        /// Creates an instance object. This shouldn't be used directly - Please use WorldMgr.CreateInstance
        /// to create an instance.
        /// </summary>
        public Instance(ushort ID, GameTimer.TimeManager time, RegionData data) :base(ID, time, data)
        {

        }

        #region Entrance

        protected GameLocation m_entranceLocation = null;

        /// <summary>
        /// Returns the entrance location into this instance.
        /// </summary>
        public GameLocation InstanceEntranceLocation
        { get { return m_entranceLocation; } }

        #endregion

        #region LoadFromDatabase

        /// <summary>
        /// Loads elements relating to the given instance keyname from the database and populates the instance.
        /// </summary>
        /// <param name="instanceName"></param>
        public virtual void LoadFromDatabase(string instanceName)
        {
            DBInstanceXElement[] objects = (DBInstanceXElement[])GameServer.Database.SelectObjects(typeof(DBInstanceXElement), "`InstanceID` = '" + instanceName + "'");

            if (objects == null || objects.Length == 0)
                return;

            int count = 0;

            //Now we have a list of DBElements, lets create the various entries 
            //associated with them and populate the instance.
            foreach (DBInstanceXElement entry in objects)
            {
                if (entry == null)
                    continue; //an odd error, but experience knows best.

                GameObject obj = null;
                string theType = "DOL.GS.GameNPC";

                //Switch the classtype to see what we are making.
                switch (entry.ClassType)
                {
                    case "entrance":
                        {
                            //create the entrance, then move to the next.
                            m_entranceLocation = new GameLocation(instanceName + "entranceRegion" + ID, ID, entry.X, entry.Y, entry.Z, entry.Heading);
                            //move to the next entry, nothing more to do here...
                            continue;
                        }
                    case "DOL.GS.GameNPC": break;
                    default: theType = entry.ClassType; break;
                }

                //Now we have the classtype to create, create it thus!
                ArrayList asms = new ArrayList();
                asms.Add(typeof(GameServer).Assembly);
                asms.AddRange(ScriptMgr.Scripts);

                //This is required to ensure we check scripts for the space aswell, such as quests!
                foreach (Assembly asm in asms)
                {
                    obj = (GameObject)(asm.CreateInstance(theType, false));
                    if (obj != null)
                        break;
                }

                
                if (obj == null)
                    continue;


                //We now have an object that isnt null. Lets place it at the location, in this region.

                obj.X = entry.X;
                obj.Y = entry.Y;
                obj.Z = entry.Z;
                obj.Heading = entry.Heading;
                obj.CurrentRegionID = ID;

                //If its an npc, load from the npc template about now.
                //By default, we ignore npctemplate if its set to 0.
                if ((GameNPC)obj != null && entry.NPCTemplate != 0)
                {
                    INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(entry.NPCTemplate);
                    //we only want to load the template if one actually exists, or there could be trouble!
                    if (npcTemplate != null)
                    {
                        ((GameNPC)obj).LoadTemplate(npcTemplate);
                    }
                }
                //Finally, add it to the world!
                obj.AddToWorld();

                //Keep track of numbers.
                count++;
            }

            log.Info("Successfully loaded a db entry to " + Description + " - Region ID " + ID + ". Loaded Entities: " + count);
        }

        #endregion


        /// <summary>
        /// This method returns an int representative of an average level for the instance.
        /// Instances do not scale with level by default, but specific instances like TaskDungeonMission can
        /// use this for an accurate representation of level.
        /// </summary>
        /// <returns></returns>
        public int GetInstanceLevel()
        {
            double level = 0;
            double count = 0;
            foreach (GameObject obj in Objects)
            {
                if (obj == null)
                    continue;

                GamePlayer player = obj as GamePlayer;
                if (player == null)
                    continue;

                //Dinberg: Guess work for now!
                //I'll guess an appropriate formulae.
                //100 + 7 times number of players divided by 100, multiplied by E(level)

                //Where E(level) = average level.
                count++;
                level += player.Level;
            }
            level = Math.Max(1,(level / count)); //double needed needed for lower levels...

            level *= ((100 + 7 * count) / 100);
            return (int)level;
        }
    }
}
