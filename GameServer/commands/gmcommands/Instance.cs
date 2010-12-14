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
        "/instance key <instancekey> (this sets the id of the instance you want to work with)",
        "/instance entry <ClassType> [<NpcTemplateID>] (add elements to the instance)",
        "/instance remove (please target the element to remove)",
		"/instance create <skin> (skin is the region id to copy)",
		"/instance test (enter a created instance)",
		"/instance close (delete the instance on the server)",
		"/instance exit")]
    public class InstanceCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public const string INSTANCE_KEY = "INSTANCE_KEY_TEMP";

        public void OnCommand(GameClient client, string[] args)
        {
			if (client.Player == null)
				return;

			GamePlayer player = client.Player;

			string key = GetInstanceKey(player);

			if (args.Length < 2)
            {
				if (key != "")
					SendMessage(client, "Current instance key is " + key);
					
				DisplaySyntax(client);
                return;
            }

            if (key == "" && args[1] != "key")
            {
                SendMessage(client, "You must first assign an instance to work with using /instance key <ID>.");
                return;
            }

            switch (args[1].ToLower())
            {
                #region SetInstanceID
                case "key":
					string newKey = string.Join(" ", args, 2, args.Length - 2);
                    client.Player.TempProperties.setProperty(INSTANCE_KEY, newKey);
					SendMessage(client, "Instance key set to " + newKey);
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

                        element.NPCTemplate = npctemplate.ToString();

                        //Save the element to database!
                        GameServer.Database.AddObject(element);
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
                            obj.GuildName = element.ObjectId.Substring(18);

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
				#endregion
				#region remove
				case "remove":
                    {
                        GameObject obj = client.Player.TargetObject;
                        if (obj == null)
                            return;
                        string ObjectId = obj.Name + obj.GuildName;
                        DataObject o = GameServer.Database.FindObjectByKey<DBInstanceXElement>(ObjectId);

                        if (o == null)
                        {
                            client.Out.SendMessage("Could not find the entry in the database! <key=" + ObjectId + ">", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        GameServer.Database.DeleteObject(o);
                        client.Out.SendMessage("Object removed!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);

                        //Remove object...
                        obj.RemoveFromWorld();
                        obj.Delete();
                        obj.DeleteFromDatabase();
                    }
                    break;
				#endregion
				#region create
				case "create":
					{
						if (player.CurrentRegion.IsInstance)
						{
							SendMessage(client, "You are already in an instance, use /instance exit to get out.");
							return;
						}

						try
						{
							if (args.Length < 3)
							{
								throw new Exception("You need to provide a skin id.  A skin is the ID of the region you want this instance to look like.");
							}

							Instance newInstance = player.TempProperties.getProperty<object>(key, null) as Instance;

							if (newInstance != null)
							{
								throw new Exception("You already have an instance '" + key + "' created, please close it before creating another.");
							}

							ushort skinID = Convert.ToUInt16(args[2]);

							newInstance = (Instance)WorldMgr.CreateInstance(skinID, typeof(Instance));

							if (newInstance == null)
							{
								SendMessage(client, "Instance creation failed.");
							}
							else
							{
								SendMessage(client, "Instance created, now loading elements for instance '" + key + "' from the DB.");
								newInstance.LoadFromDatabase(key);
								player.TempProperties.setProperty(key, newInstance);
							}
						}
						catch (Exception ex)
						{
							SendMessage(client, ex.Message);
							return;
						}
					}
					break;
				#endregion
				#region close
				case "close":
					{
						Instance newInstance = player.TempProperties.getProperty<object>(key, null) as Instance;

						if (newInstance == null)
						{
							SendMessage(client, "Can't find an instance to delete.");
						}
						else
						{
							player.TempProperties.removeProperty(key);
							newInstance.DestroyWhenEmpty = true;
							if (newInstance.NumPlayers == 0)
							{
								SendMessage(client, "Instance closed.");
							}
							else
							{
								SendMessage(client, "Instance will close once all players leave.");
							}
						}
					}
					break;
				#endregion
				#region test
				case "test":
					{
						if (player.CurrentRegion.IsInstance)
						{
							SendMessage(client, "You are already in an instance, use /instance exit to get out.");
							return;
						}

						Instance newInstance = player.TempProperties.getProperty<object>(key, null) as Instance;

						if (newInstance == null)
						{
							SendMessage(client, "Can't find an instance to test, you will need to create one first.");
						}
						else
						{
							// start with some generic coordinates that seem to work well in many instance zones
							int x = 32361;
							int y = 31744;
							int z = 16003;
							ushort heading = 1075;

							// If you're having trouble zoning into an instance then try adding an entrance element so it can be used here
							if (newInstance.InstanceEntranceLocation != null)
							{
								x = newInstance.InstanceEntranceLocation.X;
								y = newInstance.InstanceEntranceLocation.Y;
								z = newInstance.InstanceEntranceLocation.Z;
								heading = newInstance.InstanceEntranceLocation.Heading;
							}

							// save current position for use with /instance exit
							GameLocation saveLocation = new GameLocation(player.Name + "_exit", player.CurrentRegionID, player.X, player.Y, player.Z);
							player.TempProperties.setProperty(saveLocation.Name, saveLocation);

							bool success = true;

							if (!player.MoveTo(newInstance.ID, x, y, z, heading))
							{
								SendMessage(client, "MoveTo to entrance failed, now trying to move to current location inside the instance.");

								if (!player.MoveTo(newInstance.ID, player.X, player.Y, player.Z, player.Heading))
								{
									SendMessage(client, "That failed as well.  Either add an entrance to this instance or move in the world to a corresponding instance location.");
									success = false;
								}
							}

							if (success)
							{
								SendMessage(client, "Welcome to Instance ID " + newInstance.ID + ", Skin: " + newInstance.Skin + ", with " + newInstance.Zones.Count + " zones and " + newInstance.Objects.Length + " objects inside the region!");
								SendMessage(client, "Use '/instance exit' to leave if you get stuck.");
							}
						}
					}
					break;
				#endregion
				#region exit
				case "exit":
					{
						if (!player.CurrentRegion.IsInstance)
						{
							SendMessage(client, "You need to be in an instance to use this command.");
							return;
						}

						GameLocation saveLocation = player.TempProperties.getProperty<object>(player.Name + "_exit", null) as GameLocation;

						if (saveLocation == null)
						{
							ushort sourceRegion = (player.CurrentRegion as BaseInstance).Skin;

							if (!player.MoveTo(sourceRegion, player.X, player.Y, player.Z, player.Heading))
								player.MoveToBind();
						}
						else
						{
							player.MoveTo(saveLocation.RegionID, saveLocation.X, saveLocation.Y, saveLocation.Z, saveLocation.Heading);
						}
					}
					break;
				#endregion
			}

            return;
        }

        public string GetInstanceKey(GamePlayer p)
        {
            string str = "";
            try
            {
                str = (string)p.TempProperties.getProperty<object>(INSTANCE_KEY, "");
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
