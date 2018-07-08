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
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Commands
{
    [Cmd(
        "&Reload",
        ePrivLevel.Admin,
        "Reload various elements",
        "/reload mob|object|specs|spells")]
    public class ReloadCommandHandler : ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void SendSystemMessageBase(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage("\n  ===== [[[ Command Reload ]]] ===== \n", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" Reload given element.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private static void SendSystemMessageMob(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage(" /reload mob ' reload all mob in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload mob ' realm <0/1/2/3>' reload all mob with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload mob ' name <name_you_want>' reload all mob with specifique name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload mob ' model <model_ID>' reload all mob with specifique model in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private static void SendSystemMessageObject(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage(" /reload object ' reload all static object in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload object ' realm <0/1/2/3>' reload all static object with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload object ' name <name_you_want>' reload all static object with specifique name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" /reload object ' model <model_ID>' reload all static object with specifique model in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private static void SendSystemMessageRealm(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage("\n /reload <object/mob> realm <0/1/2/3>' reload all element with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(" can use 0/1/2/3 or n/a/m/h or no/alb/mid/hib....", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private static void SendSystemMessageName(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage("\n /reload <object/mob>  name <name_you_want>' reload all element with specified name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private static void SendSystemMessageModel(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage("\n /reload <object/mob>  model <model_ID>' reload all element with specified model_ID in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        public void OnCommand(GameClient client, string[] args)
        {
            ushort region = 0;
            if (client.Player != null)
            {
                region = client.Player.CurrentRegionID;
            }

            string arg = string.Empty;
            int argLength = args.Length - 1;

            if (argLength < 1)
            {
                if (client.Player != null)
                {
                    SendSystemMessageBase(client);
                    SendSystemMessageMob(client);
                    SendSystemMessageObject(client);
                    client.Out.SendMessage(" /reload specs - reload all specializations.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage(" /reload spells - reload a spells and spelllines, checking db for changed and new spells.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                log.Info("/reload command failed, review parameters.");
                return;
            }
            else if (argLength > 1)
            {
                if (args[2] == "realm" || args[2] == "Realm")
                {
                    if (argLength == 2)
                    {
                        SendSystemMessageRealm(client);
                        return;
                    }

                    if (args[3] == "0" || args[3] == "None" || args[3] == "none" || args[3] == "no" || args[3] == "n")
                    {
                        arg = "None";
                    }
                    else if (args[3] == "1" || args[3] == "a" || args[3] == "alb" || args[3] == "Alb" || args[3] == "albion" || args[3] == "Albion")
                    {
                        arg = "Albion";
                    }
                    else if (args[3] == "2" || args[3] == "m" || args[3] == "mid" || args[3] == "Mid" || args[3] == "midgard" || args[3] == "Midgard")
                    {
                        arg = "Midgard";
                    }
                    else if (args[3] == "3" || args[3] == "h" || args[3] == "hib" || args[3] == "Hib" || args[3] == "hibernia" || args[3] == "Hibernia")
                    {
                        arg = "Hibernia";
                    }
                    else
                    {
                        SendSystemMessageRealm(client);
                        return;
                    }
                }
                else if (args[2] == "name" || args[2] == "Name")
                {
                    if (argLength == 2)
                    {
                        SendSystemMessageName(client);
                        return;
                    }

                    arg = string.Join(" ", args, 3, args.Length - 3);
                }
                else if (args[2] == "model" || args[2] == "Model")
                {
                    if (argLength == 2)
                    {
                        SendSystemMessageModel(client);
                        return;
                    }

                    arg = args[3];
                }
            }

            if (args[1] == "mob" || args[1] == "Mob")
            {

                if (argLength == 1)
                {
                    arg = "all";
                    ReloadMobs(client.Player, region, arg, arg);
                }

                if (argLength > 1)
                {
                    ReloadMobs(client.Player, region, args[2], arg);
                }
            }

            if (args[1] == "object" || args[1] == "Object")
            {
                if (argLength == 1)
                {
                    arg = "all";
                    ReloadStaticItem(region, arg, arg);
                }

                if (argLength > 1)
                {
                    ReloadStaticItem(region, args[2], arg);
                }
            }

            if (args[1].ToLower() == "spells")
            {
                SkillBase.ReloadDBSpells();
                int loaded = SkillBase.ReloadSpellLines();
                if (client != null)
                {
                    ChatUtil.SendSystemMessage(client, string.Format("Reloaded db spells and {0} spells for all lines !", loaded));
                }

                log.Info(string.Format("Reloaded db spells and {0} spells for all spell lines !", loaded));
                return;
            }

            if (args[1].ToLower() == "specs")
            {
                int count = SkillBase.LoadSpecializations();
                if (client != null)
                {
                    client.Out.SendMessage(string.Format("{0} specializations loaded.", count), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }

                log.Info(string.Format("{0} specializations loaded.", count));
                return;
            }

            return;
        }

        private void ReloadMobs(GamePlayer player, ushort region, string arg1, string arg2)
        {
            if (region == 0)
            {
                log.Info("Region reload not supported from console.");
                return;
            }

            ChatUtil.SendSystemMessage(player, "Reloading Mobs:  " + arg1 + ", " + arg2 + " ...");

            int count = 0;

            foreach (GameNPC mob in WorldMgr.GetobjectsFromRegion<GameNPC>(region))
            {
                if (!mob.LoadedFromScript)
                {
                    if (arg1 == "all")
                    {
                        mob.RemoveFromWorld();

                        Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
                        if (mobs != null)
                        {
                            mob.LoadFromDatabase(mobs);
                            mob.AddToWorld();
                            count++;
                        }
                    }

                    if (arg1 == "realm")
                    {
                        eRealm realm = eRealm.None;
                        if (arg2 == "None")
                        {
                            realm = eRealm.None;
                        }

                        if (arg2 == "Albion")
                        {
                            realm = eRealm.Albion;
                        }

                        if (arg2 == "Midgard")
                        {
                            realm = eRealm.Midgard;
                        }

                        if (arg2 == "Hibernia")
                        {
                            realm = eRealm.Hibernia;
                        }

                        if (mob.Realm == realm)
                        {
                            mob.RemoveFromWorld();

                            Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
                            if (mobs != null)
                            {
                                mob.LoadFromDatabase(mobs);
                                mob.AddToWorld();
                                count++;
                            }
                        }
                    }

                    if (arg1 == "name")
                    {
                        if (mob.Name == arg2)
                        {
                            mob.RemoveFromWorld();

                            Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
                            if (mobs != null)
                            {
                                mob.LoadFromDatabase(mobs);
                                mob.AddToWorld();
                                count++;
                            }
                        }
                    }

                    if (arg1 == "model")
                    {
                        if (mob.Model == Convert.ToUInt16(arg2))
                        {
                            mob.RemoveFromWorld();

                            WorldObject mobs = GameServer.Database.FindObjectByKey<WorldObject>(mob.InternalID);
                            if (mobs != null)
                            {
                                mob.LoadFromDatabase(mobs);
                                mob.AddToWorld();
                                count++;
                            }
                        }
                    }
                }
            }

            ChatUtil.SendSystemMessage(player, count + " mobs reloaded!");
        }

        private void ReloadStaticItem(ushort region, string arg1, string arg2)
        {
            if (region == 0)
            {
                log.Info("Region reload not supported from console.");
                return;
            }

            foreach (GameStaticItem staticItem in WorldMgr.GetobjectsFromRegion<GameStaticItem>(region))
            {
                if (!staticItem.LoadedFromScript)
                {
                    if (arg1 == "all")
                    {
                        staticItem.RemoveFromWorld();

                        WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(staticItem.InternalID);
                        if (obj != null)
                        {
                            staticItem.LoadFromDatabase(obj);
                            staticItem.AddToWorld();
                        }
                    }

                    if (arg1 == "realm")
                    {
                        eRealm realm = eRealm.None;
                        if (arg2 == "None")
                        {
                            realm = eRealm.None;
                        }

                        if (arg2 == "Albion")
                        {
                            realm = eRealm.Albion;
                        }

                        if (arg2 == "Midgard")
                        {
                            realm = eRealm.Midgard;
                        }

                        if (arg2 == "Hibernia")
                        {
                            realm = eRealm.Hibernia;
                        }

                        if (staticItem.Realm == realm)
                        {
                            staticItem.RemoveFromWorld();

                            WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(staticItem.InternalID);
                            if (obj != null)
                            {
                                staticItem.LoadFromDatabase(obj);
                                staticItem.AddToWorld();
                            }
                        }
                    }

                    if (arg1 == "name")
                    {
                        if (staticItem.Name == arg2)
                        {
                            staticItem.RemoveFromWorld();

                            WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(staticItem.InternalID);
                            if (obj != null)
                            {
                                staticItem.LoadFromDatabase(obj);
                                staticItem.AddToWorld();
                            }
                        }
                    }

                    if (arg1 == "model")
                    {
                        if (staticItem.Model == Convert.ToUInt16(arg2))
                        {
                            staticItem.RemoveFromWorld();

                            WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(staticItem.InternalID);
                            if (obj != null)
                            {
                                staticItem.LoadFromDatabase(obj);
                                staticItem.AddToWorld();
                            }
                        }
                    }
                }
            }
        }
    }
}
