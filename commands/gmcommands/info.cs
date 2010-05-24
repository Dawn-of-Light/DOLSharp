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
using System.Collections.Generic;
using System.Reflection;

using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using DOL.GS.Quests;
using DOL.GS.Utils;
using DOL.Language;

namespace DOL.GS.Commands
{
	[Cmd("&info", ePrivLevel.GM, "Various Information", "'/info (select a target)")]
	
	public class InfoCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length > 1)
			{
				DisplaySyntax(client);
				return;
			}
			
			uint hour = WorldMgr.GetCurrentDayTime() / 1000 / 60 / 60;
			uint minute = WorldMgr.GetCurrentDayTime() / 1000 / 60 % 60;
			uint seconde = WorldMgr.GetCurrentDayTime() / 1000 % 60;
				
			string name = "(NoName)";
			var info = new List<string>();
			info.Add( "          Votre Region : " + client.Player.CurrentRegionID );
			info.Add(" ");
			
			if (client.Player.TargetObject != null)
			{
				/********************* MOB ************************/
				if (client.Player.TargetObject is GameNPC)
				{
					var target = client.Player.TargetObject as GameNPC;
					name = target.Name;
					
					
					info.Add(" + Class: " + target.GetType().ToString());
					info.Add(" + Brain: " + (target.Brain == null ? "(null)" : target.Brain.GetType().ToString()));
					if (target.LoadedFromScript)
						info.Add(" + Loaded: from Script");
					else
						info.Add(" + Loaded: from Database");
					info.Add(" ");
					if (client.Player.TargetObject is GameMerchant)
					{
						var targetM = client.Player.TargetObject as GameMerchant;
						
                        info.Add(" + Is Merchant " + targetM.TradeItems.ItemsListID);
						if (targetM.TradeItems != null)
						{
                            info.Add(" + Sell List: \n   " + targetM.TradeItems.ItemsListID);
						}
						else 
							info.Add(" + Sell List:  Not Present !\n");
						info.Add(" ");
					}
					if (client.Player.TargetObject is GamePet)
					{
						var targetP = client.Player.TargetObject as GamePet;
                        info.Add(" + Is Pet ");
						info.Add(" + Pet Owner: \n   " + targetP.Owner);
						info.Add(" ");
					}
					
					if (client.Player.TargetObject is GameMovingObject)
					{
						var targetM = client.Player.TargetObject as GameMovingObject;
                        info.Add(" + Is GameMovingObject  ");
                        info.Add(" + ( Boats - Siege weapons - Custom Object");
						info.Add(" + Emblem: \n   " + targetM.Emblem);
						info.Add(" ");
					}
					
					info.Add(" + Name: " + name);
					if (target.GuildName != null && target.GuildName.Length > 0)
						info.Add(" + Guild: " + target.GuildName);
					info.Add(" + Level: " + target.Level);
					info.Add(" + Realm: " + GlobalConstants.RealmToName(target.Realm));
					info.Add(" + Model:  " + target.Model);
					info.Add(" + Size " + target.Size);
					info.Add(" ");
					
					info.Add(" + Speed(current/max): " + target.CurrentSpeed + "/" + target.MaxSpeedBase);
					info.Add(" + Health: " + target.Health + "/" + target.MaxHealth);
					
					IOldAggressiveBrain aggroBrain = target.Brain as IOldAggressiveBrain;
					if (aggroBrain != null)
					{
						info.Add(" + Aggro level: " + aggroBrain.AggroLevel);
						info.Add(" + Aggro range: " + aggroBrain.AggroRange);

						if (target.MaxDistance < 0)
							info.Add(" + MaxDistance: " + -target.MaxDistance * aggroBrain.AggroRange / 100);
						else
							info.Add(" + MaxDistance: " + target.MaxDistance);
					}
					else
						info.Add(" + Not aggressive brain");
						
					if (target.NPCTemplate != null)
						info.Add(" + NPCTemplate: " + "[" + target.NPCTemplate.TemplateId + "] " + target.NPCTemplate.Name);

					info.Add(" + Roaming Range: " + target.RoamingRange);

					TimeSpan respawn = TimeSpan.FromMilliseconds(target.RespawnInterval);
					if (target.RespawnInterval <= 0)
						info.Add(" + Respawn: NPC will not respawn");
					else
					{
						string days = "";
						string hours = "";
						if (respawn.Days > 0)
							days = respawn.Days + " days ";
						if (respawn.Hours > 0)
							hours = respawn.Hours + " hours ";
						info.Add(" + Respawn: " + days + hours + respawn.Minutes + " minutes " + respawn.Seconds + " seconds");
						info.Add(" + SpawnPoint:  " + target.SpawnPoint.X + ", " + target.SpawnPoint.Y + ", " + target.SpawnPoint.Z);
					}
					
					info.Add(string.Format(" + Flags: {0} (0x{1})", ((GameNPC.eFlags)target.Flags).ToString("G"), target.Flags.ToString("X")));
					
					if (target.QuestListToGive.Count > 0)
						info.Add(" + Quests to give:  " + target.QuestListToGive.Count);
						
					if (target.PathID != null && target.PathID.Length > 0)
						info.Add(" + Path: " + target.PathID);
						
					if (target.BoatOwnerID != null && target.BoatOwnerID.Length > 0)
						info.Add(" + Boat OwnerID: " + target.BoatOwnerID);
						
					info.Add(" ");
					info.Add(" + STR  /  CON  /  DEX  /  QUI");
					info.Add(" + " + target.Strength + "  /  " + target.Constitution + "  /  " + target.Dexterity + "  /  " + target.Quickness);
					info.Add(" + INT  /  EMP  /  PIE  /  CHR");
					info.Add(" + " + target.Intelligence + "  /  " + target.Empathy + "  /  " + target.Piety + "  /  " + target.Charisma);
					info.Add(" + Block / Parry / Evade %:  " + target.BlockChance + " / " + target.ParryChance + " / " + target.EvadeChance);
					info.Add(" + Damage type: " + target.MeleeDamageType);
					if (target.LeftHandSwingChance > 0)
						info.Add(" + Left Swing %: " + target.LeftHandSwingChance);
						
					if (target.Abilities != null && target.Abilities.Count > 0)
						info.Add(" + Abilities: " + target.Abilities.Count);
						
					if (target.Spells != null && target.Spells.Count > 0)
						info.Add(" + Spells: " + target.Spells.Count);
						
					if (target.Styles != null && target.Styles.Count > 0)
						info.Add(" + Styles: " + target.Styles.Count);
						
					info.Add(" ");
					if (target.Race > 0)
						info.Add(" + Race:  " + target.Race);
						
					if (target.BodyType > 0)
						info.Add(" + Body Type:  " + target.BodyType);
						
					info.Add(" + Resist Crush/Slash/Thrust:  " + target.GetDamageResist(eProperty.Resist_Crush)
							 + " / " + target.GetDamageResist(eProperty.Resist_Slash)
							 + " / " + target.GetDamageResist(eProperty.Resist_Thrust));
					info.Add(" +  -- Heat/Cold/Matter/Natural:  " + target.GetDamageResist(eProperty.Resist_Heat)
							 + " / " + target.GetDamageResist(eProperty.Resist_Cold)
							 + " / " + target.GetDamageResist(eProperty.Resist_Matter)
							 + " / " + target.GetDamageResist(eProperty.Resist_Natural));
					info.Add(" +  -- Body/Spirit/Energy:  " + target.GetDamageResist(eProperty.Resist_Body)
							 + " / " + target.GetDamageResist(eProperty.Resist_Spirit)
							 + " / " + target.GetDamageResist(eProperty.Resist_Energy));
					info.Add(" + Active weapon slot: " + target.ActiveWeaponSlot);
					info.Add(" + Visible weapon slot: " + target.VisibleActiveWeaponSlots);
					
					if (target.EquipmentTemplateID != null && target.EquipmentTemplateID.Length > 0)
						info.Add(" + Equipment Template ID: " + target.EquipmentTemplateID);
						
					if (target.Inventory != null)
						info.Add(" + Inventory: " + target.Inventory.AllItems.Count + " items");
						
					info.Add(" ");
					info.Add(" + Mob_ID:  " + target.InternalID);
					info.Add(" + Position:  " + target.X + ", " + target.Y + ", " + target.Z + ", " + target.Heading);
					info.Add(" + OID: " + target.ObjectID);
					info.Add(" + Package ID:  " + target.PackageID);
										
					info.Add("");
					info.Add(" + Loot:");
					info.Add("");

					var template = GameServer.Database.SelectObjects<LootTemplate>("TemplateName = '" + GameServer.Database.Escape(target.Name) + "'");
					foreach (LootTemplate loot in template)
					{
						ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(loot.ItemTemplateID);

						string message = "";
						if (drop == null)
						{
							message += loot.ItemTemplateID + " (Template Not Found)";
						}
						else
						{
							message += drop.Name + " (" + drop.Id_nb + ")";
						}

						message += " Chance: " + loot.Chance.ToString();
						info.Add("- " + message);
					}
				}
				
				/********************* PLAYER ************************/
				if (client.Player.TargetObject is GamePlayer)
				{
					var target = client.Player.TargetObject as GamePlayer;
					
					name = target.Name;
					
					info.Add("PLAYER INFORMATION (Client # " + target.Client.SessionID + ")");
					info.Add("  - Name : " + target.Name);
					info.Add("  - Lastname : " + target.LastName);
					info.Add("  - Realm : " + GlobalConstants.RealmToName(target.Realm));
					info.Add("  - Level : " + target.Level);
					info.Add("  - Class : " + target.CharacterClass.Name);
					info.Add("  - Guild : " + target.GuildName);
					info.Add(" ");
					info.Add("  - Account Name : " + name);
					info.Add("  - IP : " + target.Client.Account.LastLoginIP);
					info.Add("  - Priv. Level : " + target.Client.Account.PrivLevel);
					info.Add("  - Client Version: " + target.Client.Account.LastClientVersion);
					info.Add(" ");
					info.Add("  - Craftingskill : " + target.CraftingPrimarySkill + "");
					info.Add("  - Model ID : " + target.Model);
					info.Add("  - AFK Message: " + target.TempProperties.getProperty<string>(GamePlayer.AFK_MESSAGE) + "");
					info.Add(" ");
                    info.Add("  - Money : " + Money.GetString(target.GetCurrentMoney()) + "\n");
					info.Add("  - Speed : " + target.MaxSpeedBase);
					info.Add("  - XPs : " + target.Experience);
					info.Add("  - RPs : " + target.RealmPoints);
					info.Add("  - BPs : " + target.BountyPoints);

					String sCurrent = "";
					String sTitle = "";
					int cnt = 0;
								
					info.Add(" ");
					info.Add("SPECCING INFORMATIONS ");
					info.Add("  - Remaining spec. points : " + target.SkillSpecialtyPoints);
					sTitle = "  - Player specialisations / level: \n";
					sCurrent = "";
                    foreach (Specialization spec in target.GetSpecList())
					{
						sCurrent += "  - " +spec.Name + " = " + spec.Level + " \n";
					}
					info.Add(sTitle + sCurrent);
					
					sCurrent = "";
					sTitle = "";

					info.Add(" ");
					info.Add("CHARACTER STATS ");
					info.Add("  - Maximum Health : " + target.MaxHealth);
					info.Add("  - Current AF : " + target.GetModified(eProperty.ArmorFactor));
					info.Add("  - Current ABS : " + target.GetModified(eProperty.ArmorAbsorption));

					for (eProperty stat = eProperty.Stat_First; stat <= eProperty.Stat_Last; stat++, cnt++)
					{
						sTitle += GlobalConstants.PropertyToName(stat);
                        sCurrent += target.GetModified(stat);
						
						info.Add("  - " + sTitle + " : " + sCurrent);
						sCurrent = "";
						sTitle = "";
					}

					sCurrent = "";
					sTitle = "";
					cnt = 0;
					for (eProperty res = eProperty.Resist_First; res <= eProperty.Resist_Last; res++, cnt++)
					{
						sTitle += GlobalConstants.PropertyToName(res);
                        sCurrent += target.GetModified(res);
						info.Add("  - " + sTitle + " : " + sCurrent);
						sCurrent = "";
						sTitle = "";
					}

					info.Add(" ");
					info.Add(" ");
					info.Add("  - Respecs dol : " + target.RespecAmountDOL);
					info.Add("  - Respecs single : " + target.RespecAmountSingleSkill);
					info.Add("  - Respecs full : " + target.RespecAmountAllSkill);
					
					info.Add(" ");
					info.Add(" ");
					info.Add("  --------------------------------------");
					info.Add("  -----  Inventory Equiped -----");
					info.Add("  --------------------------------------");
					////////////// Inventaire /////////////
					info.Add("  ----- Money:");
					info.Add(Money.GetShortString(target.GetCurrentMoney()));
					info.Add(" ");

					info.Add("  ----- Wearing:");
					foreach (InventoryItem item in target.Inventory.EquippedItems)
						info.Add(" [" + GlobalConstants.SlotToName(item.Item_Type) + "] " + item.Name);
					info.Add(" ");
				}
				
				/********************* OBJECT ************************/
				if (client.Player.TargetObject is GameStaticItem)
				{
					var target = client.Player.TargetObject as GameStaticItem;
					
					if (!string.IsNullOrEmpty(target.Name))
						name = target.Name;
					info.Add("  ------- OBJECT ------\n");
					info.Add(" Name: " + name);
					info.Add(" Model: " + target.Model);
					info.Add(" Emblem: " + target.Emblem);
					info.Add(" Realm: " + target.Realm);
					if (target.Owners.LongLength > 0)
					{
						info.Add(" ");
						info.Add(" Owner Name: " + target.Owners[0].Name);
					}
					info.Add(" ");
					info.Add(" OID: " + target.ObjectID);
					info.Add (" Type: " + target.GetType());

					GameInventoryItem invItem = target as GameInventoryItem;
					if( invItem != null )
					{
						info.Add (" Count: " + invItem.Item.Count);
					}

					info.Add(" ");
					info.Add(" Location: X= " + target.X + " ,Y= " + target.Y + " ,Z= " + target.Z);
				}
				
				/********************* DOOR ************************/
				if (client.Player.TargetObject is GameDoor)
				{
					var target = client.Player.TargetObject as GameDoor;
					
					string Realmname = "";
					string statut = "";
					
					name = target.Name;
					
					if (target.Realm == eRealm.None)
						Realmname = "None";
				
					if (target.Realm == eRealm.Albion)
						Realmname = "Albion";
						
					if (target.Realm == eRealm.Midgard)
						Realmname = "Midgard";
						
					if (target.Realm == eRealm.Hibernia)
						Realmname = "Hibernia";
						
					if (target.Realm == eRealm.Door)
						Realmname = "All";
						
					if (target.Locked == 1)
						statut = " Locked";
						
					if( target.Locked == 0 )
						statut = " Unlocked";

					info.Add("  ------- DOOR ------\n");
					info.Add(" ");
					info.Add( " + Name : " + target.Name );
					info.Add(" + ID : " + target.DoorID);
					info.Add( " + Realm : " + (int)target.Realm + " : " +Realmname );
					info.Add( " + Level : " + target.Level );
					info.Add( " + Guild : " + target.Guild );
					info.Add( " + Health : " + target.Health +" / "+ target.MaxHealth);
					info.Add(" + Statut : " + statut);
					info.Add(" + Type : " + DoorRequestHandler.DoorIDhandler / 100000000);
					info.Add(" ");
					info.Add(" + X : " + target.X);  
					info.Add(" + Y : " + target.Y);
					info.Add(" + Z : " + target.Z);
					info.Add(" + Heading : " + target.Heading);
				}
				
				/********************* KEEP ************************/
				if (client.Player.TargetObject is GameKeepComponent)
				{
					var target = client.Player.TargetObject as GameKeepComponent;
					
					name = target.Name;
					
					string realm = " other realm";
					if((byte)target.Realm == 0)
						realm = " Monster";
					if((byte)target.Realm == 1)
						realm = " Albion";
					if((byte)target.Realm == 2)
						realm = " Midgard";
					if((byte)target.Realm == 3)
						realm = " Hibernia";
						
					info.Add("  ------- KEEP ------\n");
					info.Add( " + Name : " + target.Name);
					info.Add( " + Level : " + target.Level);
					info.Add( " + Realm : " + realm);
					info.Add( " + Health : " + target.Health);
					info.Add( " + Height : " + target.Height);
					info.Add( " ");
					info.Add( " + Model : " + target.Model);
					info.Add( " + Skin : " + target.Skin);
					info.Add( " + ID : " + target.ID);
					info.Add( " ");
					info.Add( " + Climbing : " + target.Climbing);
					info.Add( " + IsRaized : " + target.IsRaized);
					info.Add( " + Status : " + target.Status);
					info.Add( " ");
					info.Add( " + ComponentX : " + target.ComponentX);
					info.Add( " + ComponentY : " + target.ComponentY);
					info.Add( " + ComponentHeading : " + target.ComponentHeading);
					info.Add( " ");
					info.Add( " + HookPoints : " + target.HookPoints.Count);
					info.Add( " + Positions : " + target.Positions.Count);
					info.Add( " ");
					info.Add( " + RealmPointsValue : " + target.RealmPointsValue);
					info.Add( " + ExperienceValue : " + target.ExperienceValue);
					info.Add( " + AttackRange : " + target.AttackRange);
				}
				
				client.Out.SendCustomTextWindow("[ " + name + " ]", info);
			}
			
			if (client.Player.TargetObject == null)
			{
			
				/*********************** HOUSE *************************/
				if (client.Player.InHouse)
				{
						
					House house = client.Player.CurrentHouse as House;
					
					name = house.Name;
		
					int level = house.Model - ((house.Model - 1)/4)*4;
					TimeSpan due = (house.LastPaid.AddDays(7).AddHours(1) - DateTime.Now);
					
					info.Add("  ------- HOUSE ------\n");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Owner", name));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Lotnum", house.HouseNumber));
					info.Add("Unique ID: "+house.UniqueID);
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Level", level));
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Porch"));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.PorchEnabled", (house.Porch ? " Present" : " Not Present")));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.PorchRoofColor",  Color(house.PorchRoofColor)));
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.ExteriorMaterials"));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.RoofMaterial", MaterialWall(house.RoofMaterial)));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.WallMaterial", MaterialWall(house.WallMaterial)));
					
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.DoorMaterial", MaterialDoor(house.DoorMaterial)));
					
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.TrussMaterial", MaterialTruss(house.TrussMaterial)));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.PorchMaterial", MaterialTruss(house.PorchMaterial)));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.WindowMaterial", MaterialTruss(house.WindowMaterial)));
					
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.ExteriorUpgrades"));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.OutdoorGuildBanner", ((house.OutdoorGuildBanner) ? " Present" : " Not Present")));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.OutdoorGuildShield", ((house.OutdoorGuildShield) ? " Present" : " Not Present")));
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.InteriorUpgrades"));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.IndoorGuildBanner", ((house.IndoorGuildBanner) ? " Present" : " Not Present")));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.IndoorGuildShield",((house.IndoorGuildShield) ? " Present" : " Not Present")));
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.InteriorCarpets"));
					if (house.Rug1Color != 0)
						info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Rug1Color", Color(house.Rug1Color)));
					if (house.Rug2Color != 0)
						info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Rug2Color", Color(house.Rug2Color)));
					if (house.Rug3Color != 0)
						info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Rug3Color", Color(house.Rug3Color)));
					if (house.Rug4Color != 0)
						info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Rug4Color", Color(house.Rug4Color)));
					info.Add(" ");
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.Lockbox", Money.GetString(house.KeptMoney)));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.RentalPrice", Money.GetString(HouseMgr.GetRentByModel(house.Model))));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.MaxLockbox", Money.GetString(HouseMgr.GetRentByModel(house.Model)*4)));
					info.Add(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.RentDueIn", due.Days, due.Hours));

					client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "House.SendHouseInfo.HouseOwner", name), info);
				}
				
				if (!client.Player.InHouse)
				{
					string realm = " other realm";
					if((byte)client.Player.CurrentZone.GetRealm() == 1)
						realm = " Albion";
					if((byte)client.Player.CurrentZone.GetRealm() == 2)
						realm = " Midgard";
					if((byte)client.Player.CurrentZone.GetRealm() == 3)
						realm = " Hibernia";
					
					info.Add(" Time In Game: \t"+ hour.ToString() + ":" + minute.ToString());
                    info.Add(" ");
                    info.Add(" Server player: " + WorldMgr.GetAllPlayingClientsCount());
                    info.Add(" ");
                    info.Add(" PLAYER IN REGION:");
                    info.Add(" ");
                    info.Add(" All Region player: " + WorldMgr.GetClientsOfRegionCount(client.Player.CurrentRegion.ID));
                    info.Add(" ");
                    info.Add(" Alb player: " + WorldMgr.GetClientsOfRegionCount(client.Player.CurrentRegion.ID, eRealm.Albion));
                    info.Add(" Hib player: " + WorldMgr.GetClientsOfRegionCount(client.Player.CurrentRegion.ID, eRealm.Hibernia));
                    info.Add(" Mid player: " + WorldMgr.GetClientsOfRegionCount(client.Player.CurrentRegion.ID, eRealm.Midgard));

                    info.Add(" ");
                    info.Add(" NPC in zone:");
                    info.Add(" Alb : " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Albion).Count);
                    info.Add(" Hib : " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Hibernia).Count);
                    info.Add(" Mid: " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.Midgard).Count);
                    info.Add(" None : " + client.Player.CurrentZone.GetNPCsOfZone(eRealm.None).Count);
                    info.Add(" ");
                    info.Add(" Objects in zone: " + client.Player.CurrentZone.TotalNumberOfObjects);
                    info.Add(" ");
					info.Add(" Zone Description: "+ client.Player.CurrentZone.Description);
					info.Add(" Zone Realm: "+ realm);
					info.Add(" Zone ID: "+ client.Player.CurrentZone.ID);
					info.Add(" Zone IsDungeon: "+ client.Player.CurrentZone.IsDungeon);
					info.Add(" Zone SkinID: "+ client.Player.CurrentZone.ZoneSkinID);
					info.Add(" Zone X: "+ client.Player.CurrentZone.XOffset);
					info.Add(" Zone Y: "+ client.Player.CurrentZone.YOffset);
					info.Add(" Zone Width: "+ client.Player.CurrentZone.Width);
					info.Add(" Zone Height: "+ client.Player.CurrentZone.Height);
					info.Add(" Zone Waterlevel: "+ client.Player.CurrentZone.Waterlevel);
					info.Add(" ");
					info.Add(" Region Name: "+ client.Player.CurrentRegion.Name);
                    info.Add(" Region Description: " + client.Player.CurrentRegion.Description);
                    info.Add(" Region Skin: " + client.Player.CurrentRegion.Skin);
					info.Add(" Region ID: "+ client.Player.CurrentRegion.ID);
                    info.Add(" Region Expansion: " + client.Player.CurrentRegion.Expansion);
					info.Add(" Region IsRvR: "+ client.Player.CurrentRegion.IsRvR);
					info.Add(" Region IsDungeon: "+ client.Player.CurrentRegion.IsDungeon);
                    info.Add(" Zone in Region: " + client.Player.CurrentRegion.Zones.Count);
                    info.Add(" Region WaterLevel: " + client.Player.CurrentRegion.WaterLevel);
                    info.Add(" Region DivingEnabled: " + client.Player.CurrentRegion.DivingEnabled);
                    info.Add(" Region HousingEnabled: " + client.Player.CurrentRegion.HousingEnabled);
                    info.Add(" Region IsDisabled: " + client.Player.CurrentRegion.IsDisabled);
					info.Add(" ");
                    info.Add(" Region ServerIP: " + client.Player.CurrentRegion.ServerIP);
                    info.Add(" Region ServerPort: " + client.Player.CurrentRegion.ServerPort);
					
                    client.Out.SendCustomTextWindow("[ " + client.Player.CurrentRegion.Description + " ]", info);
				}
			}
		}

		private string Color(int color)
		{
			if (color == 0) return " White";
			if (color == 53) return " Royal Blue";
			if (color == 54) return " Dark Blue";
			if (color == 57) return " Royal Turquoise";
			if (color == 60) return " Royal Teal";
			if (color == 66) return " Royal Red";
			if (color == 84) return " Violet";
			if (color == 69) return " Green";
			if (color == 70) return " Royal Green";
			if (color == 62) return " Brown ";
			if (color == 72) return " Dark Grey";
			if (color == 74) return " Black";
			if (color == 77) return " Royal Orange";
			if (color == 83) return " Royal Yellow";

            return null;
		}
		
		private string MaterialWall(int material)
		{
			if (material == 0) return " Commoner";
			if (material == 1) return " Burgess";
			if (material == 2) return " Noble";
			
			return null;
		}
		
		private string MaterialDoor(int material)
		{
			if (material == 0) return " Wooden Double";
			if (material == 1) return " Wooden with Chain";
			if (material == 2) return " Iron";
			if (material == 3) return " Aged Wood";
			if (material == 4) return " New Wood";
			if (material == 5) return " Four Panel";
			if (material == 6) return " Iron with Knocker";
			if (material == 7) return " Fine Wooden";
			if (material == 8) return " Fine Paneled";
			if (material == 9) return " Embossed Iron";
			
			return null;
		}
		
		private string MaterialTruss(int material)
		{
			if (material == 0) return " Sand";
			if (material == 1) return " River Stone";
			if (material == 2) return " Driftwood";
			if (material == 3) return " Charcoal Grey";
			if (material == 4) return " Pearl Grey";
			if (material == 5) return " Aged Beige";
			if (material == 6) return " Winter Moss";
			if (material == 7) return " Northern Ivy";
			if (material == 8) return " White Oak";
			if (material == 9) return " Onyx";
			
			return null;
		}
	}
}
