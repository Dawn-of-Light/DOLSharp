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
using DOL.Database;
using DOL.AI.Brain;
using DOL.Events;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&keepguard", //command to handle
		(uint) ePrivLevel.GM, //minimum privelege level
		"Various keep guard creation commands!", //command description
		"'/keepguard fastcreate <type>' to create a guard for the keep with base template",
		"'/keepguard fastcreate ' to show all template available in fast create",
		"'/keepguard create ' to create a guard for the closed keep ",
		"'/keepguard create lord' to create the lord for the closed keep ",
		"'/keepguard keep <keepID>' to assign guard to keep",
		"'/keepguard keep ' to assign guard to the nearest keep",
		"'/keepguard equipment <equipmentid>' to put equipment on guard",
		"'/keepguard level <level>' to change base level of guard",
		"'/keepguard save' to save guard into DB",
		"Use '/mob' command if you want to change other param of guard")] //usage
	public class KeepGuardCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public enum eGuardType : int
		{
			None = 0,//maybe for random later
			Archer = 1,
			Warior = 2,
			Wizard = 3,
			Healer = 4,
			Enter_Gatekeeper = 5,
			Exit_Gatekeeper = 6,
			Enter_PosternKeeper = 7,
			Exit_PosternKeeper = 8,
			Lord = 9,
		};
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}
			GameKeepGuard guardTarget = client.Player.TargetObject as GameKeepGuard;
			switch (args[1])
			{
				case "fastcreate":
				{
					if (args.Length == 2)
					{
						client.Out.SendMessage("type :",eChatType.CT_System, eChatLoc.CL_SystemWindow);
						int index = 0;
						foreach(string str in Enum.GetNames(typeof(eGuardType)))
						{
							client.Out.SendMessage(index + " : "+ str,eChatType.CT_System, eChatLoc.CL_SystemWindow);
							index++;
						}
						return 1;
					}
					//TODO : create guard for mid and hib too (not only albion)
					//todo : template for mid and hib
					GameNpcInventoryTemplate clericTemplate = new GameNpcInventoryTemplate();
					clericTemplate.AddNPCEquipment(eInventorySlot.RightHandWeapon, 14);
					clericTemplate.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);//emblem
					clericTemplate.AddNPCEquipment(eInventorySlot.HandsArmor, 184);
					clericTemplate.AddNPCEquipment(eInventorySlot.FeetArmor, 185);
					clericTemplate.AddNPCEquipment(eInventorySlot.TorsoArmor, 181);
					clericTemplate.AddNPCEquipment(eInventorySlot.Cloak, 164);//emblem
					clericTemplate.AddNPCEquipment(eInventorySlot.LegsArmor, 182);
					clericTemplate.AddNPCEquipment(eInventorySlot.ArmsArmor, 183);

					GameNpcInventoryTemplate ArmsmanTemplate = new GameNpcInventoryTemplate();
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.RightHandWeapon, 310);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.HeadArmor, 95);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.HandsArmor, 153);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.FeetArmor, 154);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.TorsoArmor, 150);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.Cloak, 164);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.LegsArmor, 151);
					ArmsmanTemplate.AddNPCEquipment(eInventorySlot.ArmsArmor, 152);

					GameNpcInventoryTemplate scoutTemplate = new GameNpcInventoryTemplate();
					scoutTemplate.AddNPCEquipment(eInventorySlot.DistanceWeapon, 58);
					scoutTemplate.AddNPCEquipment(eInventorySlot.HeadArmor, 312);
					scoutTemplate.AddNPCEquipment(eInventorySlot.HandsArmor, 85);
					scoutTemplate.AddNPCEquipment(eInventorySlot.FeetArmor, 84);
					scoutTemplate.AddNPCEquipment(eInventorySlot.TorsoArmor, 81);
					scoutTemplate.AddNPCEquipment(eInventorySlot.Cloak, 164);
					scoutTemplate.AddNPCEquipment(eInventorySlot.LegsArmor, 82);
					scoutTemplate.AddNPCEquipment(eInventorySlot.ArmsArmor, 83);

					GameNpcInventoryTemplate wizardTemplate = new GameNpcInventoryTemplate();
					wizardTemplate.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 327);
					wizardTemplate.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
					wizardTemplate.AddNPCEquipment(eInventorySlot.Cloak, 164);

					GameNpcInventoryTemplate lordTemplate = new GameNpcInventoryTemplate();
					lordTemplate.AddNPCEquipment(eInventorySlot.RightHandWeapon, 412);
					lordTemplate.AddNPCEquipment(eInventorySlot.DistanceWeapon, 58);
					lordTemplate.AddNPCEquipment(eInventorySlot.HeadArmor, 94);
					lordTemplate.AddNPCEquipment(eInventorySlot.HandsArmor, 153);
					lordTemplate.AddNPCEquipment(eInventorySlot.FeetArmor, 154);
					lordTemplate.AddNPCEquipment(eInventorySlot.TorsoArmor, 150);
					lordTemplate.AddNPCEquipment(eInventorySlot.Cloak, 164);
					lordTemplate.AddNPCEquipment(eInventorySlot.LegsArmor, 151);
					//todo make constant for radius
					AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot( client.Player.CurrentRegionID , client.Player, 10000);
					GameKeepGuard m_guard = new GameKeepGuard(keep);
					m_guard.Realm = 1;//alb only for moment
					m_guard.Level = 50;
					m_guard.CurrentRegion = client.Player.CurrentRegion;
					m_guard.X = client.Player.X;
					m_guard.Y = client.Player.Y;
					m_guard.Z = client.Player.Z;
					m_guard.Heading = client.Player.Heading;
					m_guard.CurrentSpeed = 0;
					m_guard.MaxSpeedBase = 200;
					m_guard.GuildName = "";
					m_guard.Size = 50;
					eGuardType classtype = eGuardType.Archer;
					try
					{
						classtype = (eGuardType)Convert.ToInt32(args[2]);
					}
					catch
					{
						client.Out.SendMessage("Wrong type of keep guard : "+args[2]+"hit /keepguard fastcreate to know all type.",eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					switch(classtype)
					{
						case eGuardType.Archer :
						{
							m_guard.Name = "Scout";
							m_guard.Inventory = scoutTemplate.CloseTemplate();
							m_guard.Model = 486;
							m_guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
						}break;
						case eGuardType.Healer :
						{
							m_guard.Name = "Cleric";
							m_guard.Inventory = clericTemplate.CloseTemplate();
							m_guard.Model = 492;
							m_guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						}break;
						case eGuardType.Lord :
						{
							m_guard = new GameKeepLord(m_guard);
							m_guard.Name = "Lord ";//+keep.LordName
							m_guard.Inventory = lordTemplate.CloseTemplate();
							m_guard.Model = 726;
							m_guard.Level = 74;//todo : find level for lord with formulat
							m_guard.AddToKeep(keep);
						}break;
						case eGuardType.Wizard :
						{
							m_guard.Name = "Wizard";
							m_guard.Inventory = wizardTemplate.CloseTemplate();
							m_guard.Model = 477;
							m_guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						}break;
						case eGuardType.Warior :
						{
								m_guard.Name = "Armswoman";
								m_guard.Model = 477;
							//todo gender
							/*
								m_guard.Name = "Armswoman";
								m_guard.Model = 486;
							*/
							m_guard.Inventory = ArmsmanTemplate.CloseTemplate();
							m_guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						}break;
						case eGuardType.Enter_Gatekeeper :
						{
							m_guard.Name = "Albion Enter Gatekeeper";
							m_guard.Model = 666;
						}break;
						case eGuardType.Exit_Gatekeeper :
						{
							m_guard.Name = "Albion Exit Gatekeeper";
							m_guard.Model = 666;
						}break;
						case eGuardType.Enter_PosternKeeper :
						{
							m_guard.Name = "Albion Enter PosternKeeper";
							m_guard.Model = 666;
						}break;
						case eGuardType.Exit_PosternKeeper :
						{
							m_guard.Name = "Albion Exit PosternKeeper";
							m_guard.Model = 666;
						}break;
					}
					if (keep.Lord != m_guard)
						keep.Guards.Add(m_guard);
					m_guard.SaveIntoDatabase();
					m_guard.AddToWorld();
					client.Out.SendMessage("You have created keep guard",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "create":
				{
					AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot( client.Player.CurrentRegionID , client.Player,10000);
					GameKeepGuard m_guard = new GameKeepGuard(keep);
					if (args.Length > 2)
					{
						if (args[2] == "lord")
							m_guard = new GameKeepLord(keep);
					}
					//todo make constant for radius
					m_guard.Name = CheckName("Blank guard", client);

					//Fill the object variables
					m_guard.X = client.Player.X;
					m_guard.Y = client.Player.Y;
					m_guard.Z = client.Player.Z;
					m_guard.CurrentRegion = client.Player.CurrentRegion;
					m_guard.Heading = client.Player.Heading;
					m_guard.Level = 50;
					m_guard.Realm = 1;
					m_guard.Name = "blank guard";
					m_guard.Model = 486;
					//Fill the living variables
					m_guard.CurrentSpeed = 0;
					m_guard.MaxSpeedBase = 200;
					m_guard.GuildName = "";
					m_guard.Size = 50;
					m_guard.AddToWorld();
					client.Out.SendMessage("You have created keep guard",eChatType.CT_System,eChatLoc.CL_SystemWindow);

				}break;
				case "keep":
				{
					if (guardTarget == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					if (!(guardTarget.Brain is KeepGuardBrain))
					{
						DisplaySyntax(client);
						return 1;
					}
					AbstractGameKeep mykeep = null;
					if (args.Length < 3)
					{
						mykeep = KeepMgr.getKeepCloseToSpot(client.Player.CurrentRegionID, client.Player, WorldMgr.VISIBILITY_DISTANCE);
					}
					else
					{
						try
						{
							int keepid = Convert.ToInt32(args[2]);
							mykeep = KeepMgr.getKeepByID(keepid);
						}
						catch
						{
							DisplayError(client,"keep id must be a number");
							return 1;
						}
					}
					(guardTarget.Brain as KeepGuardBrain).Keep = mykeep;

					client.Out.SendMessage("You have change the keep of guard to "+mykeep.KeepID ,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "equipment":
				{
					if (args.Length < 3)
					{
						DisplaySyntax(client);
						return 1;
					}
					if (guardTarget == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					guardTarget.EquipmentTemplateID = args[2];
					client.Out.SendMessage("You have put a equipment on a guard" ,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "level":
				{
					if (guardTarget == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					try
					{
						byte level = Convert.ToByte(args[2]);
						guardTarget.Level = level;
					}
					catch
					{
						DisplayError(client,"keep id must be a number");
						return 1;
					}
					client.Out.SendMessage("You have change the level of guard to " + args[2] ,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "save":
				{
					if (guardTarget == null)
					{
						DisplaySyntax(client);
						return 1;
					}
					guardTarget.SaveIntoDatabase();
					client.Out.SendMessage("You have save the current guard" ,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				default :
				{
					DisplaySyntax(client);
					return 1;
				}
			}
			return 1;
		}
		private string CheckName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}

		private string CheckGuildName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: guild name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
	}
}