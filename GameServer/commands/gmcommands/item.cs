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
 */
using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Commands
{
    [Cmd("&item",
        ePrivLevel.GM,
        "GMCommands.Item.Description",
        "GMCommands.Item.Information",
        "GMCommands.Item.Usage.Blank",
        "GMCommands.Item.Usage.Info",
        "GMCommands.Item.Usage.Create",
        "GMCommands.Item.Usage.Count",
        "GMCommands.Item.Usage.MaxCount",
        "GMCommands.Item.Usage.PackSize",
        "GMCommands.Item.Usage.Model",
        "GMCommands.Item.Usage.Extension",
        "GMCommands.Item.Usage.Color",
        "GMCommands.Item.Usage.Effect",
        "GMCommands.Item.Usage.Name",
        "GMCommands.Item.Usage.CrafterName",
        "GMCommands.Item.Usage.Type",
        "GMCommands.Item.Usage.Object",
        "GMCommands.Item.Usage.Hand",
        "GMCommands.Item.Usage.DamageType",
        "GMCommands.Item.Usage.Emblem",
        "GMCommands.Item.Usage.Price",
        "GMCommands.Item.Usage.Condition",
        "GMCommands.Item.Usage.Quality",
        "GMCommands.Item.Usage.Durability",
        "GMCommands.Item.Usage.isPickable",
        "GMCommands.Item.Usage.isDropable",
        "GMCommands.Item.Usage.IsTradable",
        "GMCommands.Item.Usage.IsStackable",
        "GMCommands.Item.Usage.CanDropAsLoot",
        "GMCommands.Item.Usage.Bonus",
        "GMCommands.Item.Usage.mBonus",
        "GMCommands.Item.Usage.Weight",
        "GMCommands.Item.Usage.DPS_AF",
        "GMCommands.Item.Usage.SPD_ABS",
        "GMCommands.Item.Usage.Material",
        "GMCommands.Item.Usage.Scroll",
        "GMCommands.Item.Usage.Spell",
        "GMCommands.Item.Usage.Spell1",
        "GMCommands.Item.Usage.Proc",
        "GMCommands.Item.Usage.Proc1",
        "GMCommands.Item.Usage.Poison",
        "GMCommands.Item.Usage.Realm",
        "GMCommands.Item.Usage.SaveTemplate",
        "GMCommands.Item.Usage.TemplateID",
        "GMCommands.Item.Usage.FindID",
        "GMCommands.Item.Usage.FindName")]
    public class ItemCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            try
            {
                switch (args[1].ToLower())
                {
                    #region Blank
                    case "blank":
                        {
                            InventoryItem item = new InventoryItem();
                            Random rand = new Random();
                            item.Id_nb = "blankItem" + rand.Next().ToString();
                            item.Name = "a blank item";
                            if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Blank.ItemCreated"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Blank.CreationError"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    #endregion Blank
                    #region Scroll
                    case "scroll":
                        {
                            GameInventoryItem scroll = ArtifactMgr.CreateScroll(args[2], Convert.ToInt16(args[3]));
                            if (scroll == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Scroll.NotFound", args[3], args[2]), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, scroll.Item))
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Scroll.Created", scroll.Item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    #endregion Scroll
                    #region Create
                    case "create":
                        {
                            ItemTemplate template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), args[2]);
                            if (template == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Create.NotFound", args[2]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            else
                            {
                                int count = 1;
                                if (args.Length >= 4)
                                {
                                    try
                                    {
                                        count = Convert.ToInt32(args[3]);
                                        if (count < 1)
                                            count = 1;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }

                                InventoryItem item = new InventoryItem();
                                item.CopyFrom(template);
                                if (item.IsStackable)
                                {
                                    item.Count = count;
                                    item.Weight = item.Count * item.Weight;
                                }
                                if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Create.Created", item.Level, item.GetName(0, false), count), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            break;
                        }
                    #endregion Create
                    #region Count
                    case "count":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;

                            if (args.Length >= 4)
                            {
                                slot = Convert.ToInt32(args[3]);
                            }

                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!item.IsStackable)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NotStackable", item.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (Convert.ToInt32(args[2]) < 1)
                            {
                                item.Weight = item.Weight / item.Count;
                                item.Count = 1;
                            }
                            else
                            {
                                item.Weight = Convert.ToInt32(args[2]) * item.Weight / item.Count;
                                item.Count = Convert.ToInt32(args[2]);
                            }
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            client.Player.UpdateEncumberance();
                            break;
                        }
                    #endregion Count
                    #region MaxCount
                    case "maxcount":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;

                            if (args.Length >= 4)
                            {
                                slot = Convert.ToInt32(args[3]);
                            }

                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.MaxCount = Convert.ToInt32(args[2]);
                            if (item.MaxCount < 1)
                                item.MaxCount = 1;
                            break;
                        }
                    #endregion MaxCount
                    #region PackSize
                    case "packsize":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;

                            if (args.Length >= 4)
                            {
                                slot = Convert.ToInt32(args[3]);
                            }

                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.PackSize = Convert.ToInt32(args[2]);
                            if (item.PackSize < 1)
                                item.PackSize = 1;
                            break;
                        }
                    #endregion PackSize
                    #region Info
                    case "info":
                        {
                            ItemTemplate obj = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), args[2]);
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Info.ItemTemplateUnknown", args[2]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            DetailDisplayHandler itemhandler = new DetailDisplayHandler();
                            ArrayList objectInfo = new ArrayList();
                            itemhandler.WriteTechnicalInfo(objectInfo, obj);
                            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "GMCommands.Item.Info.Informations", obj.Id_nb), objectInfo);
                            break;
                        }
                    #endregion Info
                    #region Model
                    case "model":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Model = Convert.ToUInt16(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                client.Player.UpdateEquipmentAppearance();
                            break;
                        }
                    #endregion Model
                    #region Extension
                    case "extension":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Extension = Convert.ToByte(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                client.Player.UpdateEquipmentAppearance();
                            break;
                        }
                    #endregion Extension
                    #region Color
                    case "color":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Color = Convert.ToUInt16(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                client.Player.UpdateEquipmentAppearance();
                            break;
                        }
                    #endregion Color
                    #region Effect
                    case "effect":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Effect = Convert.ToUInt16(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                client.Player.UpdateEquipmentAppearance();
                            break;
                        }
                    #endregion Effect
                    #region Type
                    case "type":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Item_Type = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Type
                    #region Object
                    case "object":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Object_Type = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Object
                    #region Hand
                    case "hand":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Hand = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Hand
                    #region DamageType
                    case "damagetype":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Type_Damage = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion DamageType
                    #region Name
                    case "name":
                        {
							string name = args[2];
                            int slot = (int)eInventorySlot.LastBackpack;

							if ( int.TryParse( args[args.Length - 1], out slot ) )
							{
								name = string.Join( " ", args, 2, args.Length - 3 );
							}
							else
							{
								name = string.Join( " ", args, 2, args.Length - 2 );
								slot = (int)eInventorySlot.LastBackpack;
							}

                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            item.Name = name;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Name
                    #region CrafterName
                    case "craftername":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.CrafterName = args[2];
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion CrafterName
                    #region Emblem
                    case "emblem":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Emblem = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                client.Player.UpdateEquipmentAppearance();
                            break;
                        }
                    #endregion Emblem
                    #region Level
                    case "level":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Level = Convert.ToUInt16(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Level
                    #region Price
                    case "price":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 7)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[6]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Platinum = (short)(Convert.ToInt16(args[2]) % 1000);
                            item.Gold = (short)(Convert.ToInt16(args[3]) % 1000);
                            item.Silver = (byte)(Convert.ToByte(args[4]) % 100);
                            item.Copper = (byte)(Convert.ToByte(args[5]) % 100);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Price
                    #region Condition
                    case "condition":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 5)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[4]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int con = Convert.ToInt32(args[2]);
                            int maxcon = Convert.ToInt32(args[3]);
                            item.Condition = con;
                            item.MaxCondition = maxcon;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Condition
                    #region Durability
                    case "durability":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 5)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[4]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Dur = Convert.ToInt32(args[2]);
                            int MaxDur = Convert.ToInt32(args[3]);
                            item.Durability = Dur;
                            item.MaxDurability = MaxDur;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Durability
                    #region Quality
                    case "quality":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Qua = Convert.ToInt32(args[2]);
                            item.Quality = Qua;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Quality
                    #region Bonus
                    case "bonus":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Bonus = Convert.ToInt32(args[2]);
                            item.Bonus = Bonus;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Bonus
                    #region mBonus
                    case "mbonus":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            int num = 0;
                            int bonusType = 0;
                            int bonusValue = 0;
                            if (args.Length >= 6)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[5]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            try
                            {
                                num = Convert.ToInt32(args[2]);
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.mBonus.NonSetBonusNumber"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            try
                            {
                                bonusType = Convert.ToInt32(args[3]);
                                if (bonusType < 0 || bonusType >= (int)eProperty.MaxProperty)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.mBonus.TypeShouldBeInRange", (int)(eProperty.MaxProperty - 1)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.mBonus.NonSetBonusType"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            try
                            {
                                bonusValue = Convert.ToInt32(args[4]);
                                switch (num)
                                {
                                    case 0:
                                        {
                                            item.ExtraBonus = bonusValue;
                                            item.ExtraBonusType = bonusType;
                                            break;
                                        }
                                    case 1:
                                        {
                                            item.Bonus1 = bonusValue;
                                            item.Bonus1Type = bonusType;
                                            break;
                                        }
                                    case 2:
                                        {
                                            item.Bonus2 = bonusValue;
                                            item.Bonus2Type = bonusType;
                                            break;
                                        }
                                    case 3:
                                        {
                                            item.Bonus3 = bonusValue;
                                            item.Bonus3Type = bonusType;
                                            break;
                                        }
                                    case 4:
                                        {
                                            item.Bonus4 = bonusValue;
                                            item.Bonus4Type = bonusType;
                                            break;
                                        }
                                    case 5:
                                        {
                                            item.Bonus5 = bonusValue;
                                            item.Bonus5Type = bonusType;
                                            break;
                                        }
                                    case 6:
                                        {
                                            item.Bonus6 = bonusValue;
                                            item.Bonus6Type = bonusType;
                                            break;
                                        }
                                    case 7:
                                        {
                                            item.Bonus7 = bonusValue;
                                            item.Bonus7Type = bonusType;
                                            break;
                                        }
                                    case 8:
                                        {
                                            item.Bonus8 = bonusValue;
                                            item.Bonus8Type = bonusType;
                                            break;
                                        }
                                    case 9:
                                        {
                                            item.Bonus9 = bonusValue;
                                            item.Bonus9Type = bonusType;
                                            break;
                                        }
                                    case 10:
                                        {
                                            item.Bonus10 = bonusValue;
                                            item.Bonus10Type = bonusType;
                                            break;
                                        }
                                    default:
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.mBonus.UnknownBonusNumber", num), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return;
                                }
                                if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
                                {
                                    client.Out.SendCharStatsUpdate();
                                    client.Out.SendCharResistsUpdate();
                                }
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.mBonus.NotSetBonusValue"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    #endregion mBonus
                    #region Weight
                    case "weight":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Weight = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Weight
                    #region DPS_AF - DPS - AF
                    case "dps_af":
                    case "dps":
                    case "af":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.DPS_AF = Convert.ToByte(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion DPS_AF - DPS - AF
                    #region SPD_ABS - SPD - ABS
                    case "spd_abs":
                    case "spd":
                    case "abs":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.SPD_ABS = Convert.ToByte(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion SPD_ABS - SPD - ABS
                    #region IsDropable
                    case "isdropable":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.IsDropable = Convert.ToBoolean(args[2]);
                            break;
                        }
                    #endregion IsDropable
                    #region IsPickable
                    case "ispickable":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.IsPickable = Convert.ToBoolean(args[2]);
                            break;
                        }
                    #endregion IsPickable
                    #region IsTradable
                    case "istradable":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.IsTradable = Convert.ToBoolean(args[2]);
                            break;
                        }
                    #endregion IsTradable
                    #region CanDropAsLoot
                    case "candropasloot":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.CanDropAsLoot = Convert.ToBoolean(args[2]);
                            break;
                        }
                    #endregion CanDropAsLoot
                    #region Spell
                    case "spell":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 6)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[5]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Charges = Convert.ToInt32(args[2]);
                            int MaxCharges = Convert.ToInt32(args[3]);
                            int SpellID = Convert.ToInt32(args[4]);
                            item.Charges = Charges;
                            item.MaxCharges = MaxCharges;
                            item.SpellID = SpellID;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Spell
                    #region Spell1
                    case "spell1":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 6)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[5]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Charges = Convert.ToInt32(args[2]);
                            int MaxCharges = Convert.ToInt32(args[3]);
                            int SpellID1 = Convert.ToInt32(args[4]);
                            item.Charges1 = Charges;
                            item.MaxCharges1 = MaxCharges;
                            item.SpellID1 = SpellID1;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Spell1
                    #region Proc
                    case "proc":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.ProcSpellID = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Proc
                    #region Proc1
                    case "proc1":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.ProcSpellID1 = Convert.ToInt32(args[2]);
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Proc1
                    #region Poison
                    case "poison":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 6)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[5]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int Charges = Convert.ToInt32(args[2]);
                            int MaxCharges = Convert.ToInt32(args[3]);
                            int SpellID = Convert.ToInt32(args[4]);
                            item.PoisonCharges = Charges;
                            item.PoisonMaxCharges = MaxCharges;
                            item.PoisonSpellID = SpellID;
                            client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                            break;
                        }
                    #endregion Poison
                    #region Realm
                    case "realm":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Realm = int.Parse(args[2]);
                            break;
                        }
                    #endregion Realm
                    #region TemplateID
                    case "templateid":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            item.Id_nb = args[2];
                            break;
                        }
                    #endregion TemplateID
                    #region SaveTemplate
                    case "savetemplate":
                        {
                            int slot = (int)eInventorySlot.LastBackpack;
                            string name = "";
                            if (args.Length >= 4)
                            {
                                try
                                {
                                    slot = Convert.ToInt32(args[3]);
                                }
                                catch
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                                if (slot > (int)eInventorySlot.LastBackpack)
                                {
                                    slot = (int)eInventorySlot.LastBackpack;
                                }
                                if (slot < 0)
                                {
                                    slot = 0;
                                }
                                name = args[2];
                            }
                            else if (args.Length >= 3)
                            {
                                name = args[2];
                            }
                            else
                            {
                                DisplaySyntax(client);
                                return;
                            }
                            InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);
                            if (item == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.Count.NoItemInSlot", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            ItemTemplate temp = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), name);
                            bool add = false;
                            if (temp == null)
                            {
                                add = true;
                                temp = new ItemTemplate();
                            }
                            item.Id_nb = name;
                            temp.Bonus = item.Bonus;
                            temp.Bonus1 = item.Bonus1;
                            temp.Bonus2 = item.Bonus2;
                            temp.Bonus3 = item.Bonus3;
                            temp.Bonus4 = item.Bonus4;
                            temp.Bonus5 = item.Bonus5;
                            temp.Bonus6 = item.Bonus6;
                            temp.Bonus7 = item.Bonus7;
                            temp.Bonus8 = item.Bonus8;
                            temp.Bonus9 = item.Bonus9;
                            temp.Bonus10 = item.Bonus10;
                            temp.Bonus1Type = item.Bonus1Type;
                            temp.Bonus2Type = item.Bonus2Type;
                            temp.Bonus3Type = item.Bonus3Type;
                            temp.Bonus4Type = item.Bonus4Type;
                            temp.Bonus5Type = item.Bonus5Type;
                            temp.Bonus6Type = item.Bonus6Type;
                            temp.Bonus7Type = item.Bonus7Type;
                            temp.Bonus8Type = item.Bonus8Type;
                            temp.Bonus9Type = item.Bonus9Type;
                            temp.Bonus10Type = item.Bonus10Type;
                            temp.Platinum = item.Platinum;
                            temp.Gold = item.Gold;
                            temp.Silver = item.Silver;
                            temp.Copper = item.Copper;
                            temp.Color = item.Color;
                            temp.Condition = item.Condition;
                            temp.DPS_AF = item.DPS_AF;
                            temp.Durability = item.Durability;
                            temp.Effect = item.Effect;
                            temp.Emblem = item.Emblem;
                            temp.ExtraBonus = item.ExtraBonus;
                            temp.ExtraBonusType = item.ExtraBonusType;
                            temp.Hand = item.Hand;
                            temp.Id_nb = item.Id_nb;
                            temp.IsDropable = item.IsDropable;
                            temp.IsPickable = item.IsPickable;
                            temp.IsTradable = item.IsTradable;
                            temp.Item_Type = item.Item_Type;
                            temp.Level = item.Level;
                            temp.MaxCondition = item.MaxCondition;
                            temp.MaxDurability = item.MaxDurability;
                            temp.Model = item.Model;
                            temp.Extension = item.Extension;
                            temp.Name = item.Name;
                            temp.Object_Type = item.Object_Type;
                            temp.Quality = item.Quality;
                            temp.SPD_ABS = item.SPD_ABS;
                            temp.Type_Damage = item.Type_Damage;
                            temp.Weight = item.Weight;
                            temp.MaxCount = item.MaxCount;
                            temp.PackSize = item.PackSize;
                            temp.Charges = item.Charges;
                            temp.MaxCharges = item.MaxCharges;
                            temp.SpellID = item.SpellID;
                            temp.SpellID1 = item.SpellID1;
                            temp.ProcSpellID = item.ProcSpellID;
                            temp.ProcSpellID1 = item.ProcSpellID1;
                            temp.Realm = item.Realm;
                            temp.PoisonCharges = item.PoisonCharges;
                            temp.PoisonMaxCharges = item.PoisonMaxCharges;
                            temp.PoisonSpellID = item.PoisonSpellID;
                            if (add)
                            {
                                GameServer.Database.AddNewObject(temp);
                            }
                            else
                            {
                                GameServer.Database.SaveObject(temp);
                            }
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Item.SaveTemplate.ItemSaved", name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    #endregion SaveTemplate
                    #region FindID
                    case "findid":
                        {
                            string name = string.Join(" ", args, 2, args.Length - 2);
							if(name != "")
							{
								ItemTemplate[] items = (ItemTemplate[])GameServer.Database.SelectObjects(typeof(ItemTemplate), "id_nb like '%" + GameServer.Database.Escape(name) + "%'");
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Item.FindID.MatchingIDsForX", name, items.Length), new object[] { });
								foreach (ItemTemplate item in items)
								{
									DisplayMessage(client, item.Id_nb + " (" + item.Name + ")", new object[] { });
								}
							}
                            break;
                        }
                    #endregion FindID
                    #region FindName
                    case "findname":
                        {
                            string name = string.Join(" ", args, 2, args.Length - 2);
							if(name != "")
							{
								ItemTemplate[] items = (ItemTemplate[])GameServer.Database.SelectObjects(typeof(ItemTemplate), "name like '%" + GameServer.Database.Escape(name) + "%'");
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Item.FindName.MatchingNamesForX", name, items.Length), new object[] { });
								foreach (ItemTemplate item in items)
								{
									DisplayMessage(client, item.Name + "  (" + item.Id_nb + ")", new object[] { });
								}
							}
                            break;
                        }
                    #endregion FindName
                }
            }
            catch
            {
                DisplaySyntax(client);
            }
        }
    }
}