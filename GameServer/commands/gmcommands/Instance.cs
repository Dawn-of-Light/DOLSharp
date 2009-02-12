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
using DOL.GS.PacketHandler;
using System.Reflection;
using System.Collections;
using DOL.Database;

//By dinberg - so its him who you blame ;)
namespace DOL.GS.Commands
{
    [Cmd(
        "&instance",
        ePrivLevel.GM,
        "This command assists in the creation of instances.",
        "/instance",
        "/instance key <instancekey>",
        "/instance entry <ClassType> [<NpcTemplateID>]",
        "/instance delete (please target the element to remove)")]
    public class InstanceCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public const string INSTANCE_KEY = "INSTANCE_KEY_TEMP";

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            string key = GetInstanceKey(client);
            if (key == "" && args[1] != "key")
            {
                SendMessage(client, "You must first assign an instance to work with using /instance key <ID>.");
                return;
            }

            switch (args[1])
            {
                #region SetInstanceID
                case "key":
                    client.Player.TempProperties.setProperty(INSTANCE_KEY, args[2]);
                    break;
                #endregion
                #region Create Entry
                case "entry":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        //Create the database entry...
                        DBInstanceXElement element = new DBInstanceXElement();
                        element.Heading = client.Player.Heading;
                        element.X = client.Player.X;
                        element.Y = client.Player.Y;
                        element.Z = client.Player.Z;
                        element.InstanceID = key;
                        element.ClassType = args[2];

                        int npctemplate = 0;

                        try { npctemplate = int.Parse(args[3]); }
                        catch { }

                        element.NPCTemplate = npctemplate;

                        //Save the element to database!
                        GameServer.Database.AddNewObject(element);
                        GameServer.Database.SaveObject(element);

                        //Dinberg: place a marker at this spot!
                        string theType = args[2];

                        SendMessage(client, "Created an element here! Use your memory for now, I sure as hell dont have anything else to show you where it is ^^");

                        //Only create ones that have namespaces (signified by '.')
                        if (theType.Contains("."))
                        {
                            SendMessage(client, "theType suspected to be a ClassType - attempting to invoke a marker of this class.");
                            GameObject obj = null;

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

                            if (args.Length == 4)
                            {
                                int templateID = 0;
                                try { templateID = int.Parse(args[3]); }
                                catch { }
                                //If its an npc, load from the npc template about now.
                                //By default, we ignore npctemplate if its set to 0.
                                if ((GameNPC)obj != null && templateID != 0)
                                {
                                    INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(templateID);
                                    //we only want to load the template if one actually exists, or there could be trouble!
                                    if (npcTemplate != null)
                                    {
                                        ((GameNPC)obj).LoadTemplate(npcTemplate);
                                    }
                                }
                            }

                            //Add to world...
                            obj.Name = element.ObjectId.Substring(0, 18);
                            obj.Guild = element.ObjectId.Substring(18);

                            obj.X = element.X;
                            obj.Y = element.Y;
                            obj.Z = element.Z;
                            obj.Heading = element.Heading;

                            obj.CurrentRegion = client.Player.CurrentRegion;

                            if (!obj.AddToWorld())
                                client.Out.SendMessage("Object not added to world correctly.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            else
                                client.Out.SendMessage("Object added!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                    }
                    break;
                case "delete":
                    {
                        GameObject obj = client.Player.TargetObject;
                        if (obj == null)
                            return;
                        string ObjectId = obj.Name + obj.Guild;
                        DataObject o = GameServer.Database.FindObjectByKey(typeof(DBInstanceXElement), ObjectId);

                        if (o == null)
                        {
                            client.Out.SendMessage("Could not find the entry in the database! <key=" + ObjectId + ">", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        GameServer.Database.DeleteObject(o);
                        client.Out.SendMessage("Object delected!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);

                        //Remove object...
                        obj.RemoveFromWorld();
                        obj.Delete();
                        obj.DeleteFromDatabase();
                    }
                    break;
                #endregion
            }

            return;
        }

        public string GetInstanceKey(GameClient c)
        {
            string str = "";
            try
            {
                str = (string)c.Player.TempProperties.getObjectProperty(INSTANCE_KEY, "");
            }
            catch { return ""; }
            return str;
        }

        public void SendMessage(GameClient c, string str)
        {
                            c.Out.SendMessage(str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
    }
}
