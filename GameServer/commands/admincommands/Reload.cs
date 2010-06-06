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
	[Cmd("&Reload",
        ePrivLevel.Admin,
		"Recharge l'element donné."
		)]
	public class ReloadCommandHandler :ICommandHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        private static void SendSystemMessageBase(GameClient client)
        {			
			client.Out.SendMessage("\n  ===== [[[ Command Reload ]]] ===== \n", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" Reload given element.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
        private static void SendSystemMessageMob(GameClient client)
        {	
			client.Out.SendMessage(" /reload mob ' reload all mob in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload mob ' realm <0/1/2/3>' reload all mob with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload mob ' name <name_you_want>' reload all mob with specifique name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload mob ' model <model_ID>' reload all mob with specifique model in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
        private static void SendSystemMessageObject(GameClient client)
        {	
			client.Out.SendMessage(" /reload object ' reload all static object in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload object ' realm <0/1/2/3>' reload all static object with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload object ' name <name_you_want>' reload all static object with specifique name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" /reload object ' model <model_ID>' reload all static object with specifique model in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
		private static void SendSystemMessageRealm(GameClient client)
        {
			client.Out.SendMessage("\n /reload <object/mob> realm <0/1/2/3>' reload all element with specifique realm in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(" can use 0/1/2/3 or n/a/m/h or no/alb/mid/hib....", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
        private static void SendSystemMessageName(GameClient client)
        {	
			client.Out.SendMessage("\n /reload <object/mob>  name <name_you_want>' reload all element with specified name in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        private static void SendSystemMessageModel(GameClient client)
        {	
			client.Out.SendMessage("\n /reload <object/mob>  model <model_ID>' reload all element with specified model_ID in region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

		public void OnCommand(GameClient client, string[] args)
		{
			ushort region = client.Player.CurrentRegionID;
			string arg = "";
            int argLength = args.Length-1;

			if (argLength < 1)
            {
				SendSystemMessageBase(client);
				SendSystemMessageMob(client);
				SendSystemMessageObject(client);
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
						arg = "None";
					else if (args[3] == "1" || args[3] == "a" || args[3] == "alb" || args[3] == "Alb" || args[3] == "albion" || args[3] == "Albion")
						arg = "Albion";
					else if (args[3] == "2" || args[3] == "m" || args[3] == "mid" || args[3] == "Mid" || args[3] == "midgard" || args[3] == "Midgard")
						arg = "Midgard";
					else if (args[3] == "3" || args[3] == "h" || args[3] == "hib" || args[3] == "Hib" || args[3] == "hibernia" || args[3] == "Hibernia")
						arg = "Hibernia";
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
					arg = String.Join(" ", args, 3, args.Length - 3);
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
					ReloadNPC (region , arg, arg);
				}
				
				if (argLength > 1)
				{
					ReloadNPC (region , args[2], arg);
				}
			}
			
            if (args[1] == "object" || args[1] == "Object")
            {
                if (argLength == 1)
				{
					arg = "all";
					ReloadStaticItem (region , arg, arg);
				}

				if (argLength > 1)
				{
					ReloadStaticItem (region , args[2], arg);
				}
			}
			return;
		}
		
		
		private void ReloadNPC (ushort region , string arg1, string arg2)
		{
			foreach (GameNPC mob in WorldMgr.GetNPCsFromRegion(region))
			{
				if (!mob.LoadedFromScript)
				{	
					if(arg1 == "all")
					{
						mob.RemoveFromWorld();
						
						Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
						if (mobs != null)
						{
							mob.LoadFromDatabase(mobs);
							mob.AddToWorld();
						}
					}
					
					if(arg1 == "realm")
					{
						eRealm realm = eRealm.None;
						if (arg2 == "None") realm = eRealm.None;
						if (arg2 == "Albion") realm = eRealm.Albion;
						if (arg2 == "Midgard") realm = eRealm.Midgard;
						if (arg2 == "Hibernia") realm = eRealm.Hibernia;
						
						if (mob.Realm == realm)
						{
							mob.RemoveFromWorld();
							
							Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
							if (mobs != null)
							{
								mob.LoadFromDatabase(mobs);
								mob.AddToWorld();
							}
						}
					}
					
					if(arg1 == "name")
					{
						if (mob.Name == arg2)
						{
							mob.RemoveFromWorld();
							
							Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
							if (mobs != null)
							{
								mob.LoadFromDatabase(mobs);
								mob.AddToWorld();
							}
						}
					}

					if(arg1 == "model")
					{
						if (mob.Model == Convert.ToUInt16(arg2))
						{
                            mob.RemoveFromWorld();

                            WorldObject mobs = GameServer.Database.FindObjectByKey<WorldObject>(mob.InternalID);
							if (mobs != null)
							{
                                mob.LoadFromDatabase(mobs);
                                mob.AddToWorld();
							}
						}
					}
				}
			}
		}
		
		private void ReloadStaticItem (ushort region , string arg1, string arg2)
		{
			foreach (GameStaticItem objet in WorldMgr.GetStaticItemFromRegion(region))
			{
				if (!objet.LoadedFromScript)
				{	
					if(arg1 == "all")
					{
						objet.RemoveFromWorld();

                        WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(objet.InternalID);
						if (obj != null)
						{
							objet.LoadFromDatabase(obj);
							objet.AddToWorld();
						}
					}
					
					if(arg1 == "realm")
					{
						eRealm realm = eRealm.None;
						if (arg2 == "None") realm = eRealm.None;
						if (arg2 == "Albion") realm = eRealm.Albion;
						if (arg2 == "Midgard") realm = eRealm.Midgard;
						if (arg2 == "Hibernia") realm = eRealm.Hibernia;
						
						if (objet.Realm == realm)
						{
							objet.RemoveFromWorld();
							
							WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(objet.InternalID);
							if (obj != null)
							{
								objet.LoadFromDatabase(obj);
								objet.AddToWorld();
							}
						}
					}
					
					if(arg1 == "name")
					{
						if (objet.Name == arg2)
						{
							objet.RemoveFromWorld();
							
							WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(objet.InternalID);
							if (obj != null)
							{
								objet.LoadFromDatabase(obj);
								objet.AddToWorld();
							}
						}
					}

					if(arg1 == "model")
					{
						if (objet.Model == Convert.ToUInt16(arg2))
						{
							objet.RemoveFromWorld();
							
							WorldObject obj = GameServer.Database.FindObjectByKey<WorldObject>(objet.InternalID);
							if (obj != null)
							{
								objet.LoadFromDatabase(obj);
								objet.AddToWorld();
							}
						}
					}
				}
			}
		}
	}
}
