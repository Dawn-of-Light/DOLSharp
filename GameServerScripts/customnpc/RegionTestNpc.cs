using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DOL.GS;
using log4net;
using DOL.GS.Instances;
namespace DOL.GS
{
    public class RegionTestNpc : GameNPC
    {
        public override bool SayReceive(GameLiving source, string str)
        {
            if (!base.SayReceive(source, str))
                return false;

            string[] split = str.Split(' ');
            switch (split[0])
            {
                case "create":
                    ushort id = 286;

                    if (split.Length > 1)
                    {
                        try { id = ushort.Parse(split[1]); }
                        catch { }
                    }

                    m_instance = (Instance)WorldMgr.CreateInstance(id, typeof(Instance));
                    if (m_instance != null)
                        Say("Success, instance created.");
                    else
                        Say("Instance creation found errors.");

                    //Try and load a template from the db...
                    if (split.Length > 2)
                    {
                        string load = split[2];
                        m_instance.LoadFromDatabase(load);
                    }
                   
                    break;
                case "test":
                    if (m_instance == null)
                        Say("Instance is currently null.");
                    else
                    {
                        int x = 32361;
                            int y = 31744;
                        int z = 16003;
                        ushort heading = 1075;

                        if (m_instance.InstanceEntranceLocation != null)
                        {
                            x = m_instance.InstanceEntranceLocation.X;
                            y = m_instance.InstanceEntranceLocation.Y;
                            z = m_instance.InstanceEntranceLocation.Z;
                            heading = m_instance.InstanceEntranceLocation.Heading;
                        }

                        Say("Instance ID " + m_instance + ", Skin: " + m_instance.Skin + ", with " + m_instance.Zones.Count + " zones inside the region.");
                        if (!source.MoveTo(m_instance.ID, x, y, z, heading))
                            Say("Source could not be moved to the target location; MoveTo returned false.");
                        
                    }
                    break;
                case "storm":
                    StormInstance instance = (StormInstance)WorldMgr.CreateInstance(415, typeof(StormInstance));

                    instance.LoadFromDatabase("stormTest");
                    m_instance = instance;

                    if (m_instance == null)
                        Say("Failed.");
                    else
                        Say("Success!");
                    break;
            }
            return true;
        }

        private Instance m_instance;
    }
}

namespace DOL.GS.Commands
{
    [CmdAttribute("&npcdebug", //command to handle
        ePrivLevel.Admin, //minimum privelege level
        "Writes information of all npcs in a region to the console", //command description
        "/npcdebug")] //usage
    public class NpcDebugHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            int index = 0;
            foreach (GameObject npc in client.Player.CurrentRegion.Objects)
            {
                if (npc == null)
                    continue;

                log.Warn(index + ": Object name=" + npc.Name + ", {X/Y/Z} H =  { " + npc.X + " / " + npc.Y + " / " + npc.Z + " }  " + npc.Heading + ".");
                log.Warn("Zone, zoneID " + npc.CurrentZone.ID + " (" + npc.CurrentZone.Description + "), zoneSkinID = " + npc.CurrentZone.ZoneSkinID + ".");
            }
        }
            
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
