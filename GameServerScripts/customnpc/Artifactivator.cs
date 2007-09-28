using System;
using DOL;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    [NPCGuildScript("Activator")]
    public class Activator : GameNPC
    {
        public override bool AddToWorld()
        {
            Level = 255;
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            template.AddNPCEquipment(eInventorySlot.Cloak, 1727);
            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2226);
            template.AddNPCEquipment(eInventorySlot.LegsArmor, 2158);
            template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2873);
            template.AddNPCEquipment(eInventorySlot.HandsArmor, 2492);
            template.AddNPCEquipment(eInventorySlot.FeetArmor, 2875);
            Inventory = template.CloseTemplate();
            GuildName = "Artifact Activator";
            Flags = 16;
            base.AddToWorld();
            return true;
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            TurnTo(player, 100);

            player.Out.SendMessage("I can activate an artifact for you if you have the proper items. Just drop the book on me!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            return false;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer player = (GamePlayer)source;
            switch (str)
            {
                case "Bane of Battler 1H Blades (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_blades_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Blades (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_blades_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Blunt  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_blunt_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Blunt  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_blunt_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                    }
                    break;
                case "Bane of Battler 1H Crush  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_crush_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Crush  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_crush_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Hammer (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_hammer_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Hammer (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_hammer_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Slash  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_slash_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Slash  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_slash_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Sword  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_sword_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 1H Sword  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_1h_sword_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Crush  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_crush_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Crush  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_crush_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Hammer (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_hammer_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Hammer (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_hammer_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Slash  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_slash_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Slash  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_slash_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Sword  (+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_sword_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Sword  (+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_sword_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Crush  (LW/+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_lw_crush_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Crush  (LW/+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_lw_crush_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Slash  (LW/+Con)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_lw_slash_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Bane of Battler 2H Slash  (LW/+Dex)":
                    {
                        InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(battler);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bane_of_battler_2h_lw_slash_dex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bane of Battler has now been activated!");
                    }
                    break;
                case "Egg of Youth":
                    {
                        InventoryItem egg = player.Inventory.GetFirstItemByID("egg_of_youth", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        egg.IsActivated = true;
                        Reply(player, "Your Egg of Youth has now been activated!");
                    }
                    break;
                case "Eerie Darkness Lighting Stone":
                    {
                        InventoryItem eerie = player.Inventory.GetFirstItemByID("eerie_darkness", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        eerie.IsActivated = true;
                        Reply(player, "Your Eerie Darkness Lighting Stone has now been activated!");
                    }
                    break;
                case "Band of Stars":
                    {
                        InventoryItem band = player.Inventory.GetFirstItemByID("band_of_stars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        band.IsActivated = true;
                        Reply(player, "Your Band of Stars has now been activated!");
                    }
                    break;
                case "Ceremonial Bracers Con":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bracers);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ceremonial_bracers_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Ceremonial Bracers have now been activated!");
                    }
                    break;
                case "Ceremonial Bracers Qui":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bracers);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ceremonial_bracers_Qui");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Ceremonial Bracers have now been activated!");
                    }
                    break;
                case "Ceremonial Bracers Acu":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bracers);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ceremonial_bracers_acu");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Ceremonial Bracers have now been activated!");
                    }
                    break;
                case "Ceremonial Bracers Dex":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bracers);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ceremonial_bracers_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Ceremonial Bracers have now been activated!");
                    }
                    break;
                case "Ceremonial Bracers Str":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bracers);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ceremonial_bracers_con");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Ceremonial Bracers have now been activated!");
                    }
                    break;
                case "Bracers of Zo'arkat":
                    {
                        InventoryItem bracers = player.Inventory.GetFirstItemByID("bracers_of_zoarkat", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        bracers.IsActivated = true;
                        Reply(player, "Your Bracers of Zo'arkat have now been activated!");
                    }
                    break;
                case "Belt of the Moon":
                    {
                        InventoryItem belt = player.Inventory.GetFirstItemByID("belt_of_the_moon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        belt.IsActivated = true;
                        Reply(player, "Your Belt of the Moon has now been activated!");
                    }
                    break;
                case "Belt of the Sun":
                    {
                        InventoryItem belt = player.Inventory.GetFirstItemByID("belt_of_the_sun", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        belt.IsActivated = true;
                        Reply(player, "Your Belt of the Sun has now been activated!");
                    }
                    break;
                case "Atlantis Tablet":
                    {
                        InventoryItem tablet = player.Inventory.GetFirstItemByID("atlantis_tablet", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        tablet.IsActivated = true;
                        Reply(player, "Your Atlantis Tablet has now been activated!");
                    }
                    break;
                case "Shades of Mist Stealth":
                    {
                        InventoryItem cloak = player.Inventory.GetFirstItemByID("shade_of_mist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        cloak.IsActivated = true;
                        Reply(player, "Your Shades of Mist has now been activated!");
                    }
                    break;
                case "Shades of Mist Melee":
                    {
                        InventoryItem shade = player.Inventory.GetFirstItemByID("shade_of_mist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shade);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "shade_of_mist_melee");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Shades of Mist has now been activated!");
                    }
                    break;
                case "Alvarus Leggings Cloth":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_cloth");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Chain":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_chain");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Leather":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_leather");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Plate":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_plate");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Studded":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_studded");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Reinforced":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_reinforced");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Alvarus Leggings Scales":
                    {
                        InventoryItem alvarus = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(alvarus);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "alvarus_leggings_scales");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Alvarus Leggings have now been activated!");
                    }
                    break;
                case "Maddening Scalars Caster":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_caster");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Caster Chain":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_caster_chain");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Chain":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_chain");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Leather":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_leather");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Plate":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_plate");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Reinforced":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_reinforced");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Scales":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_scales");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Maddening Scalars Tank Studded":
                    {
                        InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(scalars);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "maddening_scalars_tank_studded");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Maddening Scalars have now been activated!");
                    }
                    break;
                case "Malice Axe 1H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_axe_1h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Axe 2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_axe_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Blades":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_blades");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Blunt":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_blunt");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Crush 1H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_crush_1h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Crush 2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_crush_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Hammer 1H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_hammer_1h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Hammer 2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_hammer_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice LW Crush":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_lw_crush");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice LW Slash":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_lw_slash");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Slash 1H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_slash_1h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Slash 2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_slash_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Sword 1H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_sword_1h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Malice Sword 2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("malice_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "malice_sword_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Malice Weapon has now been activated!");
                    }
                    break;
                case "Snakecharmer's Weapon Flex":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("snakecharmers_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "snakecharmers_weapon_flex");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Snakecharmer's Weapon has now been activated!");
                    }
                    break;
                case "Snakecharmer's Weapon H2H":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("snakecharmers_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "snakecharmers_weapon_h2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Snakecharmer's Weapon has now been activated!");
                    }
                    break;
                case "Snakecharmer's Weapon Scythe":
                    {
                        InventoryItem weapon = player.Inventory.GetFirstItemByID("snakecharmers_weapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(weapon);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "snakecharmers_weapon_scythe");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Snakecharmer's Weapon has now been activated!");
                    }
                    break;
                case "Arms of the Wind Chain":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_chain");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Plate":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_plate");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Studded":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_studded");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Cloth":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_cloth");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Reinforced":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_reinforced");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Scale":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_Scale");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Arms of the Wind Leather":
                    {
                        InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(arms);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arms_of_the_wind_leather");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Arms of the Wind have now been activated!");
                    }
                    break;
                case "Aten's Shield Small":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("atens_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "atens_shield_small");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Aten's Shield has now been activated!");
                    }
                    break;
                case "Aten's Shield Medium":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("atens_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "atens_shield_medium");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Aten's Shield has now been activated!");
                    }
                    break;
                case "Aten's Shield Large":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("atens_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "atens_shield_large");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Aten's Shield has now been activated!");
                    }
                    break;
                case "Gem of Lost Memories Melee":
                    {
                        InventoryItem golm = player.Inventory.GetFirstItemByID("golm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(golm);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "golm_melee");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Gem of Lost Memories has now been activated!");
                    }
                    break;
                case "Gem of Lost Memories Caster":
                    {
                        InventoryItem golm = player.Inventory.GetFirstItemByID("golm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(golm);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "golm_caster");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Gem of Lost Memories has now been activated!");
                    }
                    break;
                case "Gem of Lost Memories Archer":
                    {
                        InventoryItem golm = player.Inventory.GetFirstItemByID("golm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(golm);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "golm_archer");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Gem of Lost Memories has now been activated!");
                    }
                    break;
                case "Braggart's Longbow":
                    {
                        InventoryItem bow = player.Inventory.GetFirstItemByID("braggarts_bow", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bow);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "braggarts_longbow");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Braggart's Bow has now been activated!");
                    }
                    break;
                case "Braggart's Recurve Bow":
                    {
                        InventoryItem bow = player.Inventory.GetFirstItemByID("braggarts_bow", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bow);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "braggarts_recurvebow");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Braggart's Bow has now been activated!");
                    }
                    break;
                case "Braggart's Composite Bow":
                    {
                        InventoryItem bow = player.Inventory.GetFirstItemByID("braggarts_bow", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(bow);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "braggarts_compositebow");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Braggart's Bow has now been activated!");
                    }
                    break;
                case "Bruiser 1H":
                    {
                        InventoryItem hammer = player.Inventory.GetFirstItemByID("bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        hammer.IsActivated = true;
                        Reply(player, "Your Bruiser has now been activated!");
                    }
                    break;
                case "Bruiser 2H":
                    {
                        InventoryItem hammer = player.Inventory.GetFirstItemByID("bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(hammer);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "bruiser_2h");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Bruiser has now been activated!");
                    }
                    break;
                case "Cloudsong":
                    {
                        InventoryItem cloak = player.Inventory.GetFirstItemByID("cloudsong", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        cloak.IsActivated = true;
                        Reply(player, "Your Cloudsong has now been activated!");
                    }
                    break;
                case "Crocodile's Tears Ring":
                    {
                        InventoryItem ring = player.Inventory.GetFirstItemByID("croc_tears_ring", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        ring.IsActivated = true;
                        Reply(player, "Your Crocodile's Tears Ring has now been activated!");
                    }
                    break;
                case "Dream Sphere":
                    {
                        InventoryItem sphere = player.Inventory.GetFirstItemByID("dream_sphere", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        sphere.IsActivated = true;
                        Reply(player, "Your Dream Sphere has now been activated!");
                    }
                    break;
                case "Enyalio's Boots Chain":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("enyalios_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        boots.IsActivated = true;
                        Reply(player, "Your Enyalio's Boots have now been activated!");
                    }
                    break;
                case "Enyalio's Boots Plate":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("enyalios_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(boots);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "enyalios_boots_plate");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Enyalio's Boots have now been activated!");
                    }
                    break;
                case "Enyalio's Boots Reinforced":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("enyalios_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(boots);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "enyalios_boots_reinforced");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Enyalio's Boots have now been activated!");
                    }
                    break;
                case "Enyalio's Boots Studded":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("enyalios_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(boots);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "enyalios_boots_studded");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Enyalio's Boots have now been activated!");
                    }
                    break;
                case "Crocodile's Tooth Dagger Slashing":
                    {
                        InventoryItem dagger = player.Inventory.GetFirstItemByID("croc_tooth_dagger", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        dagger.IsActivated = true;
                        Reply(player, "Your Crocodile's Tooth Dagger has now been activated!");
                    }
                    break;
                case "Crocodile's Tooth Dagger Blades":
                    {
                        InventoryItem dagger = player.Inventory.GetFirstItemByID("croc_tooth_dagger", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(dagger);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "croc_tooth_dagger_blades");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Crocodile's Tooth Dagger has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Small Mid":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_smallm");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Small Hib":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_smallh");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Small Alb":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_smalla");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Medium Mid":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_medm");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Medium Hib":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_medh");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Cyclops Eye Shield Medium Alb":
                    {
                        InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(shield);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cyclops_eye_shield_meda");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Cyclops Eye Shield has now been activated!");
                    }
                    break;
                case "Erinys Charm":
                    {
                        InventoryItem erinys = player.Inventory.GetFirstItemByID("erinys_charm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        erinys.IsActivated = true;
                        Reply(player, "Your Erinys Charm has now been activated!");
                    }
                    break;
                case "Eternal Plant":
                    {
                        InventoryItem plant = player.Inventory.GetFirstItemByID("eternal_plant", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        plant.IsActivated = true;
                        Reply(player, "Your Eternal Plant has now been activated!");
                    }
                    break;
                case "Flamedancer's Boots Caster":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("flamedancers_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        boots.IsActivated = true;
                        Reply(player, "Your Flamedancer's Boots has now been activated!");
                    }
                    break;
                case "Flamedancer's Boots Friar":
                    {
                        InventoryItem boots = player.Inventory.GetFirstItemByID("flamedancers_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                        player.Inventory.RemoveItem(boots);
                        ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "flamedancers_boots_friar");
                        InventoryItem item = new InventoryItem(artifacttemplate);
                        item.IsActivated = true;
                        player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        Reply(player, "Your Flamedancer's Boots has now been activated!");
                    }
                    break;
                default: break;
            }
            return true;
        }
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (!(source is GamePlayer))
                return false;
            GamePlayer player = source as GamePlayer;
            switch (item.Id_nb)
            {
                case "book_of_shadeofmist":
                    {
                        if (PlayerHasArtifact(player, "shade_of_mist") && PlayerArtifactIsActivated(player, "shade_of_mist"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Shades of Mist cloak!");
                        else if (PlayerHasArtifact(player, "shade_of_mist") && !PlayerArtifactIsActivated(player, "shade_of_mist"))
                        {
                            //Rogue classes:
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger)
                            {
                                //Reply(player, "And with great honor, I bestow upon you the stealther version of this cape!");
                                Reply(player, "Congratulations on obtaining your Shades of Mist cloak, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_shadeofmist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem cloak = player.Inventory.GetFirstItemByID("shade_of_mist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, cloak);
                            }
                            //ARMS, MERC, PAL, REAV, BM, CHA, HERO, NS, RANG, VW, VAMP,BER, SAV, SKLD, THAN, WAR, VALK
                            else if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valewalker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Vampiir
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie)
                            {
                                //Reply(player, "Ahh, a fine new Shades of Mist cloak for you my friend, melee version of course!");
                                Reply(player, "Congratulations on obtaining your Shades of Mist cloak, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_shadeofmist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem cloak = player.Inventory.GetFirstItemByID("shade_of_mist", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, cloak);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Shades of Mist!");
                            }
                        }
                        break;
                    }

                case "book_of_eggofyouth":
                    {
                        if (PlayerHasArtifact(player, "egg_of_youth") && PlayerArtifactIsActivated(player, "egg_of_youth"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an egg of youth!");
                        else if (PlayerHasArtifact(player, "egg_of_youth") && !PlayerArtifactIsActivated(player, "egg_of_youth"))
                        {
                            switch ((eCharacterClass)player.CharacterClass.ID)
                            {
                                case eCharacterClass.Healer:
                                case eCharacterClass.Shaman:
                                case eCharacterClass.Cleric:
                                case eCharacterClass.Friar:
                                case eCharacterClass.Heretic:
                                case eCharacterClass.Druid:
                                case eCharacterClass.Warden:
                                case eCharacterClass.Bard:
                                    {
                                        Reply(player, "Worried about wrinkles? Don't be! It's the wonderful, incredible Egg of Youth! Go enjoy!");
                                        Reply(player, "Congratulations on obtaining your Egg of Youth, my friend!");
                                        InventoryItem book = player.Inventory.GetFirstItemByID("book_of_eggofyouth", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                        player.Inventory.RemoveItem(book);
                                        InventoryItem egg = player.Inventory.GetFirstItemByID("egg_of_youth", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                        ClaimArtifact(player, egg);
                                        break;
                                    }

                                default:
                                    {
                                        Reply(player, "Sorry friend, you don't meet the requirements for the Egg of Youth!");
                                        break;
                                    }

                            }
                        }
                        break;
                    }
                case "book_of_bandofstars":
                    {
                        if (PlayerHasArtifact(player, "band_of_stars") && PlayerArtifactIsActivated(player, "band_of_stars"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Band of Stars!");
                        else if (PlayerHasArtifact(player, "band_of_stars") && !PlayerArtifactIsActivated(player, "band_of_stars"))
                        {
                            Reply(player, "Congratulations on obtaining your Band of Stars, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_bandofstars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem band = player.Inventory.GetFirstItemByID("band_of_stars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, band);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the band of stars!");
                            break;

                        }
                        break;
                    }
                case "book_of_dreamsphere":
                    {
                        if (PlayerHasArtifact(player, "dream_sphere") && PlayerArtifactIsActivated(player, "dream_sphere"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Dream Sphere!");
                        else if (PlayerHasArtifact(player, "dream_sphere") && !PlayerArtifactIsActivated(player, "dream_sphere"))
                        {
                            Reply(player, "Congratulations on obtaining your Dream Sphere, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_dreamsphere", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem sphere = player.Inventory.GetFirstItemByID("dream_sphere", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, sphere);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Dream Sphere!");
                            break;

                        }
                        break;
                    }
                case "book_of_eeriedarkness":
                    {
                        if (PlayerHasArtifact(player, "eerie_darkness") && PlayerArtifactIsActivated(player, "eerie_darkness"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Eerie Darkness Lighting Stone!");
                        else if (PlayerHasArtifact(player, "eerie_darkness") && !PlayerArtifactIsActivated(player, "eerie_darkness"))
                        {
                            Reply(player, "Congratulations on obtaining your Eerie Darkness Lighting Stone, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_eeriedarkness", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem eerie = player.Inventory.GetFirstItemByID("eerie_darkness", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, eerie);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Eerie Darkness Lighting Stone!");
                            break;

                        }
                        break;
                    }
                case "book_of_golm":
                    {
                        if (PlayerHasArtifact(player, "golm") && PlayerArtifactIsActivated(player, "golm"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Gem of Lost Memories!");
                        else if (PlayerHasArtifact(player, "golm") && !PlayerArtifactIsActivated(player, "golm"))
                        {
                            Reply(player, "Congratulations on obtaining your Gem of Lost Memories, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_golm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem gem = player.Inventory.GetFirstItemByID("golm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, gem);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Gem of Lost Memories!");
                            break;

                        }
                        break;
                    }
                case "book_of_maddeningscalars":
                    {
                        if (PlayerHasArtifact(player, "maddening_scalars") && PlayerArtifactIsActivated(player, "maddening_scalars"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Maddening Scalars!");
                        else if (PlayerHasArtifact(player, "maddening_scalars") && !PlayerArtifactIsActivated(player, "maddening_scalars"))
                        {
                            Reply(player, "Congratulations on obtaining your Maddening Scalars, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_maddeningscalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem scalars = player.Inventory.GetFirstItemByID("maddening_scalars", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, scalars);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Maddening Scalars!");
                            break;

                        }
                        break;
                    }
                case "book_of_ceremonialbracers":
                    {
                        if (PlayerHasArtifact(player, "cerebracer") && PlayerArtifactIsActivated(player, "cerebracer"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Ceremonial Bracers!");
                        else if (PlayerHasArtifact(player, "cerebracer") && !PlayerArtifactIsActivated(player, "cerebracer"))
                        {
                            Reply(player, "Congratulations on obtaining your Ceremonial Bracers, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_ceremonialbracers", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem bracers = player.Inventory.GetFirstItemByID("cerebracer", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, bracers);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Ceremonial Bracers!");
                            break;

                        }
                        break;
                    }
                case "book_of_bracersofzoarkat":
                    {
                        if (PlayerHasArtifact(player, "bracers_of_zoarkat") && PlayerArtifactIsActivated(player, "bracers_of_zoarkat"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bracers of Zo'arkat!");
                        else if (PlayerHasArtifact(player, "bracers_of_zoarkat") && !PlayerArtifactIsActivated(player, "bracers_of_zoarkat"))
                        {
                            Reply(player, "Congratulations on obtaining your Bracers of Zo'arkat, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_bracersofzoarkat", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem bracers = player.Inventory.GetFirstItemByID("bracers_of_zoarkat", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, bracers);
                            break;

                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for the Bracers of Zoarkat!");
                            break;

                        }
                        break;
                    }
                case "book_of_alvarusleggings":
                    {
                        if (PlayerHasArtifact(player, "alvarus_leggings") && PlayerArtifactIsActivated(player, "alvarus_leggings"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Alvarus' Leggings!");
                        else if (PlayerHasArtifact(player, "alvarus_leggings") && !PlayerArtifactIsActivated(player, "alvarus_leggings"))
                        {
                            Reply(player, "Congratulations on obtaining your Alvarus Leggings, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_alvarusleggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem legs = player.Inventory.GetFirstItemByID("alvarus_leggings", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, legs);
                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for Alvarus Leggings!");
                        }
                        break;
                    }
                case "book_of_armsofthewind":
                    {
                        if (PlayerHasArtifact(player, "arms_of_the_wind") && PlayerArtifactIsActivated(player, "arms_of_the_wind"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Arms of the Wind!");
                        else if (PlayerHasArtifact(player, "arms_of_the_wind") && !PlayerArtifactIsActivated(player, "arms_of_the_wind"))
                        {
                            Reply(player, "Congratulations on obtaining your Arms of the Wind, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_armsofthewind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem arms = player.Inventory.GetFirstItemByID("arms_of_the_wind", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, arms);
                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for Arms of the Wind!");
                        }
                        break;
                    }
                case "book_of_enyaliosboots":
                    {
                        if (PlayerHasArtifact(player, "enyalios_boots") && PlayerArtifactIsActivated(player, "enyalios_boots"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Enyalio's Boots!");
                        else if (PlayerHasArtifact(player, "enyalios_boots") && !PlayerArtifactIsActivated(player, "enyalios_boots"))
                        {
                            //ARMS, CLER, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, RANG, WARD, BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior)
                            {
                                Reply(player, "Congratulations on obtaining your Enyalio's Boots, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_enyaliosboots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem boots = player.Inventory.GetFirstItemByID("enyalios_boots", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, boots);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for Enyalio's Boots!");
                            }
                        }
                        break;
                    }
                case "book_of_atlantistablet":
                    {
                        if (PlayerHasArtifact(player, "atlantis_tablet") && PlayerArtifactIsActivated(player, "atlantis_tablet"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Atlantis Tablet!");
                        else if (PlayerHasArtifact(player, "atlantis_tablet") && !PlayerArtifactIsActivated(player, "atlantis_tablet"))
                        {
                            Reply(player, "Congratulations on obtaining your Atlantis Tablet, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_atlantistablet", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem tablet = player.Inventory.GetFirstItemByID("atlantis_tablet", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, tablet);
                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for an Atlantis Tablet!");
                        }
                        break;
                    }
                case "book_of_maliceweapon":
                    {
                        if (PlayerHasArtifact(player, "malice_weapon") && PlayerArtifactIsActivated(player, "malice_weapon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Malice Weapon!");
                        else if (PlayerHasArtifact(player, "malice_weapon") && !PlayerArtifactIsActivated(player, "malice_weapon"))
                        {
                            //ARMS, CLER, HER, INF, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, NS, RANG, WARD, BER, HEAL, SAV, SB, SHA, SKLD, THAN, WAR
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage)
                            {
                                Reply(player, "Congratulations on obtaining your Malice Weapon, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_maliceweapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem weapon = player.Inventory.GetFirstItemByID("maliceweapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, weapon);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Malice Weapon!");
                            }
                        }
                        break;
                    }
                case "book_of_snakecharmersweapon":
                    {
                        if (PlayerHasArtifact(player, "snakecharmers_weapon") && PlayerArtifactIsActivated(player, "snakecharmers_weapon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Snakecharmer's Weapon!");
                        else if (PlayerHasArtifact(player, "snakecharmers_weapon") && !PlayerArtifactIsActivated(player, "snakecharmers_weapon"))
                        {
                            //HER, REAV, VW, SAV, MAUL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver ||(eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valewalker)
                            {
                                Reply(player, "Congratulations on obtaining your Snakecharmer's Weapon, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_snakecharmersweapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem snakec = player.Inventory.GetFirstItemByID("bane_of_snakecharmersweapon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, snakec);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Snakecharmer's Weapon!");
                            }
                        }
                        break;
                    }
                case "book_of_baneofbattler":
                    {
                        if (PlayerHasArtifact(player, "bane_of_battler") && PlayerArtifactIsActivated(player, "bane_of_battler"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bane of Battler!");
                        else if (PlayerHasArtifact(player, "bane_of_battler") && !PlayerArtifactIsActivated(player, "bane_of_battler"))
                        {
                            // INF, MINS, SCOU, ARMS, PAL, REAV, MERC, CLER, FRIAR, HER, NS, RANG, HERO, CHA, BM, DRU, WARD, BARD, SB, WAR, THAN, SKLD, BER, HEAL, SHA, SAV, HUNT, VALK
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie)
                            {
                                Reply(player, "Congratulations on obtaining your Bane of Battler, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_baneofbattler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem battler = player.Inventory.GetFirstItemByID("bane_of_battler", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, battler);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Bane of Battler!");
                            }
                        }
                        break;
                    }
                case "book_of_atensshield":
                    {
                        if (PlayerHasArtifact(player, "atens_shield") && PlayerArtifactIsActivated(player, "atens_shield"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Aten's Shield!");
                        else if (PlayerHasArtifact(player, "atens_shield") && !PlayerArtifactIsActivated(player, "atens_shield"))
                        {
                            // INF, HER, MINS, SCOU, ARMS, PAL, REAV, MERC, NS, RANG, DRU, BARD, WARD, HERO, CHA, BM, SB, SHA, WAR, THAN, SKLD, BER, VAL, CLER
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric)
                            {
                                Reply(player, "Congratulations on obtaining your Aten's Shield, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_atensshield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem shield = player.Inventory.GetFirstItemByID("atens_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, shield);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Aten's Shield!");
                            }
                        }
                        break;
                    }
                case "book_of_beltofthemoon":
                    {
                        if (PlayerHasArtifact(player, "belt_of_the_moon") && PlayerArtifactIsActivated(player, "belt_of_the_moon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Belt of the Moon!");
                        else if (PlayerHasArtifact(player, "belt_of_the_moon") && !PlayerArtifactIsActivated(player, "belt_of_the_moon"))
                        {
                            //WIZ, THEU, CAB, CLER, FRI, HER, SORC, NECR, ANI, BARD, BAN, DRU, ELD, ENCH, MENT, WARD BD, HEAL, RM, SHA, SM, WARL, MAUL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bainshee || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Enchanter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock)
                            {
                                Reply(player, "Congratulations on obtaining your Belt of the Moon, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_beltofthemoon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem belt = player.Inventory.GetFirstItemByID("belt_of_the_moon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, belt);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Belt of the Moon!");
                            }
                        }
                        break;
                    }
                case "book_of_beltofthesun":
                    {
                        if (PlayerHasArtifact(player, "belt_of_the_sun") && PlayerArtifactIsActivated(player, "belt_of_the_sun"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Belt of the Sun!");
                        else if (PlayerHasArtifact(player, "belt_of_the_sun") && !PlayerArtifactIsActivated(player, "belt_of_the_sun"))
                        {
                            // INF, MINS, SCOU, ARMS, PA, REAV, MERC, BM, CHA, HERO, NS, RANG, VW, VAMP, WARD BER, HUNT, SAV, SB, SKLD, THAN, VALK, WAR
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valewalker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Vampiir
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior)
                            {
                                Reply(player, "Congratulations on obtaining your Belt of the Sun, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_beltofthesun", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem belt = player.Inventory.GetFirstItemByID("belt_of_the_sun", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, belt);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Belt of the Sun!");
                            }
                        }
                        break;
                    }
                case "book_of_braggartsbow":
                    {
                        if (PlayerHasArtifact(player, "braggarts_bow") && PlayerArtifactIsActivated(player, "braggarts_bow"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Braggart's Bow!");
                        else if (PlayerHasArtifact(player, "braggarts_Bow") && !PlayerArtifactIsActivated(player, "braggarts_bow"))
                        {
                            // SCOU, RAN, HUN
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout)
                            {
                                Reply(player, "Congratulations on obtaining your Braggart's Bow, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_braggartsbow", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem bow = player.Inventory.GetFirstItemByID("braggarts_bow", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, bow);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Braggart's Bow!");
                            }
                        }
                        break;
                    }
                case "book_of_bruiser":
                    {
                        if (PlayerHasArtifact(player, "bruiser") && PlayerArtifactIsActivated(player, "bruiser"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bruiser!");
                        else if (PlayerHasArtifact(player, "bruiser") && !PlayerArtifactIsActivated(player, "bruiser"))
                        {
                            // ARMS, CLER, FRI, HER, MERC, PAL, REAV, BARD, BM, CHA, DRU, HERO, WARD, BER, HEAL, HUNT, SAV, SB, SHA, SKLD, THAN, WAR

                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior)
                            {
                                Reply(player, "Congratulations on obtaining your Bruiser, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem hammer = player.Inventory.GetFirstItemByID("bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, hammer);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Bruiser!");
                            }
                        }
                        break;
                    }
                case "book_of_cloudsong":
                    {
                        if (PlayerHasArtifact(player, "cloudsong") && PlayerArtifactIsActivated(player, "cloudsong"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Cloudsong!");
                        else if (PlayerHasArtifact(player, "cloudsong") && !PlayerArtifactIsActivated(player, "cloudsong"))
                        {
                            // CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BARD, DRU, ELD, ENCH, MENT, WARD, VW, BD, HEAL, RM, SHA, SM, WARL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Enchanter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valewalker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock)
                            {
                                Reply(player, "Congratulations on obtaining your Bruiser, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem hammer = player.Inventory.GetFirstItemByID("bruiser", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, hammer);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Bruiser!");
                            }
                        }
                        break;
                    }
                case "book_of_croctearsring":
                    {
                        if (PlayerHasArtifact(player, "croc_tears_ring") && PlayerArtifactIsActivated(player, "croc_tears_ring"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crocodile's Tears Ring!");
                        else if (PlayerHasArtifact(player, "croc_tears_ring") && !PlayerArtifactIsActivated(player, "croc_tears_ring"))
                        {
                            // MIN, SORC, THEU, BAIN, BARD, ENCH, WARD, RM, SKLD, CAB, CLER, FRI, HER, NECR, WIZ, ANI, DRU, ELD, MENT, BD, HEAL, SHA, SM, WARL

                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bainshee
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Enchanter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Skald || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Druid || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock)
                            {
                                Reply(player, "Congratulations on obtaining your Crodile's Tears Ring, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_croctearsring", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem ring = player.Inventory.GetFirstItemByID("croc_tears_ring", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, ring);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Crocodile's Tears Ring!");
                            }
                        }
                        break;
                    }
                case "book_of_croctoothdagger":
                    {
                        if (PlayerHasArtifact(player, "croc_tooth_dagger") && PlayerArtifactIsActivated(player, "croc_tooth_dagger"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crocodile's Tooth Dagger!");
                        else if (PlayerHasArtifact(player, "croc_tooth_dagger") && !PlayerArtifactIsActivated(player, "croc_tooth_dagger"))
                        {
                            // ARMS, INF, MERC, MINS PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, BER, HUNT, SAV, THAN, SAV, VALK, WAR
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior)
                            {
                                Reply(player, "Congratulations on obtaining your Crodile's Tooth Dagger, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_croctoothdagger", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem dagger = player.Inventory.GetFirstItemByID("croc_tooth_dagger", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, dagger);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Crocodile's Tooth Dagger!");
                            }
                        }
                        break;
                    }
                case "book_of_crownofzahur":
                    {
                        if (PlayerHasArtifact(player, "crown_of_zahur") && PlayerArtifactIsActivated(player, "crown_of_zahur"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crown of Zahur!");
                        else if (PlayerHasArtifact(player, "crown_of_zahur") && !PlayerArtifactIsActivated(player, "crown_of_zahur"))
                        {
                            Reply(player, "Congratulations on obtaining your Crown of Zahur, my friend!");
                            InventoryItem book = player.Inventory.GetFirstItemByID("book_of_crownofzahur", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(book);
                            InventoryItem crown = player.Inventory.GetFirstItemByID("crown_of_zahur", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            ClaimArtifact(player, crown);
                        }
                        else
                        {
                            Reply(player, "Sorry friend, you don't meet the requirements for a Crown of Zahur!");
                        }
                        break;
                    }
                case "book_of_cyclopseyeshield":
                    {
                        if (PlayerHasArtifact(player, "cyclops_eye_shield") && PlayerArtifactIsActivated(player, "cyclops_eye_shield"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Cyclops Eye Shield!");
                        else if (PlayerHasArtifact(player, "cyclops_eye_shield") && !PlayerArtifactIsActivated(player, "cyclops_eye_shield"))
                        {
                            // ARMS, INF, MERC, MINS PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, BER, HUNT, SAV, THAN, VALK, WAR
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Armsman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mercenary || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Minstrel
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Paladin || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Reaver
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Scout || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Blademaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Champion || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hero
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Ranger
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Berserker || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Hunter
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Savage || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Thane
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valkyrie || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warrior)
                            {
                                Reply(player, "Congratulations on obtaining your Cyclops Eye Shield, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_cyclopseyeshield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem shield = player.Inventory.GetFirstItemByID("cyclops_eye_shield", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, shield);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Cyclops Eye Shield!");
                            }
                        }
                        break;
                    }
                case "book_of_erinyscharm":
                    {
                        if (PlayerHasArtifact(player, "erinys_charm") && PlayerArtifactIsActivated(player, "erinys_charm"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Erinys Charm!");
                        else if (PlayerHasArtifact(player, "erinys_charm") && !PlayerArtifactIsActivated(player, "erinys_charm"))
                        {
                            // CAB, NECR, SORC, THEUR, WIZ, ANI, BAIN, ELD, ENCH, MENT, BD, RM, SM, WARL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bainshee || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Enchanter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock)
                            {
                                Reply(player, "Congratulations on obtaining your Erinys Charm, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_erinyscharm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem erinys = player.Inventory.GetFirstItemByID("erinys_charm", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, erinys);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Erinys Charm!");
                            }
                        }
                        break;
                    }
                case "book_of_eternalplant":
                    {
                        if (PlayerHasArtifact(player, "eternal_plant") && PlayerArtifactIsActivated(player, "eternal_plant"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Eternal Plant!");
                        else if (PlayerHasArtifact(player, "eternal_plant") && !PlayerArtifactIsActivated(player, "eternal_plant"))
                        {
                            // CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BAIN, BARD, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cleric
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bainshee
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bard || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Enchanter || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warden || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Healer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shaman || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock)
                            {
                                Reply(player, "Congratulations on obtaining your Eternal Plant, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_eternalplant", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem plant = player.Inventory.GetFirstItemByID("eternal_plant", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, plant);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Eternal Plant!");
                            }
                        }
                        break;
                    }
                case "book_of_flamedancersboots":
                    {
                        if (PlayerHasArtifact(player, "flamedancers_boots") && PlayerArtifactIsActivated(player, "flamedancers_boots"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Flamedancer's Boots!");
                        else if (PlayerHasArtifact(player, "flamedancers_boots") && !PlayerArtifactIsActivated(player, "flamedancers_boots"))
                        {
                            // CAB, FRI, HER, INF, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, NS, VW, VAMP BD, RM, SB, SM, WARL, MAUL
                            if ((eCharacterClass)player.CharacterClass.ID == eCharacterClass.Cabalist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Friar
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Heretic || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Infiltrator
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Necromancer || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Sorcerer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Theurgist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Wizard
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Animist || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bainshee
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Eldritch || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mentalist
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Nightshade || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Valewalker
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Vampiir || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Bonedancer
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Runemaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Shadowblade
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Spiritmaster || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Warlock
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mauler_Alb || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mauler_Hib
                                || (eCharacterClass)player.CharacterClass.ID == eCharacterClass.Mauler_Mid)
                            {
                                Reply(player, "Congratulations on obtaining your Eternal Plant, my friend!");
                                InventoryItem book = player.Inventory.GetFirstItemByID("book_of_eternalplant", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                player.Inventory.RemoveItem(book);
                                InventoryItem plant = player.Inventory.GetFirstItemByID("eternal_plant", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                ClaimArtifact(player, plant);
                            }
                            else
                            {
                                Reply(player, "Sorry friend, you don't meet the requirements for a Eternal Plant!");
                            }
                        }
                        break;
                    }
            }
            return base.ReceiveItem(source, item);
        }

        /// <summary>
        /// Checks all inventory slots on the player, to make sure he does not get double-artifacts.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="artifact">The artifact to check for.</param>
        /// <returns></returns>
        private bool PlayerHasArtifact(GamePlayer player, string str)
        {
            InventoryItem test = player.Inventory.GetFirstItemByID(str, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
            if (test != null)
                return true;
            return false;
        }
        private bool PlayerArtifactIsActivated(GamePlayer player, string str)
        {
            InventoryItem test = player.Inventory.GetFirstItemByID(str, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
            if (test != null && test.IsActivated) 
                return true;
            return false;
        }


        private void Reply(GamePlayer player, string str)
        {
            player.Out.SendMessage(str, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }

        private bool ClaimArtifact(GamePlayer player, InventoryItem iitem)
        {
            switch (iitem.Id_nb)
            {
                case "shade_of_mist":
                    {
                        player.Out.SendMessage("You can have a [Shades of Mist Stealth]\n" +
                            "You can have a [Shades of Mist Melee]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "egg_of_youth":
                    {
                        player.Out.SendMessage("You can have an [Egg of Youth]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "bracers_of_zoarkat":
                    {
                        player.Out.SendMessage("You can have a [Bracers of Zo'arkat]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "band_of_stars":
                    {
                        player.Out.SendMessage("You can have a [Band of Stars]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "atlantis_tablet":
                    {
                        player.Out.SendMessage("You can have an [Atlantis Tablet]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "belt_of_the_moon":
                    {
                        player.Out.SendMessage("You can have an [Belt of the Moon]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "belt_of_the_sun":
                    {
                        player.Out.SendMessage("You can have an [Belt of the Sun]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "cloudsong":
                    {
                        player.Out.SendMessage("You can have an [Cloudsong]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "croc_tears_ring":
                    {
                        player.Out.SendMessage("You can have an [Crocodile's Tears Ring]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "dream_sphere":
                    {
                        player.Out.SendMessage("You can have an [Dream Sphere]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "eerie_darkness":
                    {
                        player.Out.SendMessage("You can have an [Eerie Darkness Lighting Stone]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "erinys_charm":
                    {
                        player.Out.SendMessage("You can have an [Erinys Charm]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "eternal_plant":
                    {
                        player.Out.SendMessage("You can have an [Eternal Plant]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "flamedancers_boots":
                    {
                        player.Out.SendMessage("You can have [Flamedancer's Boots Caster]\n" +
                            "You can have [Flamedancer's Boots Friar", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "enyalios_boots":
                    {
                        player.Out.SendMessage("You can have a [Enyalio's Boots Chain]\n" +
                        "You can have a [Enyalio's Boots Plate]\n" +
                        "You can have a [Enyalio's Boots Reinforced]\n" +
                        "You can have a [Enyalio's Boots Studded]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "croc_tooth_dagger":
                    {
                        player.Out.SendMessage("You can have a [Crocodile's Tooth Dagger Slashing]\n" +
                            "You can have a [Crocodile's Tooth Dagger Blades]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "cerebracer":
                    {
                        player.Out.SendMessage("You can have a [Ceremonial Bracers Acu]\n" +
                            "You can have a [Ceremonial Bracers Con]\n" +
                            "You can have a [Ceremonial Bracers Qui]\n" +
                            "You can have a [Ceremonial Bracers Dex]\n" +
                            "You can have a [Ceremonial Bracers Str]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "bane_of_battler":
                    {
                        player.Out.SendMessage("You can have a [Bane of Battler 1H Blades (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Blades (+Dex)]\n" +
                        "You can have a [Bane of Battler 1H Blunt  (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Blunt  (+Dex)]\n" +
                        "You can have a [Bane of Battler 1H Crush  (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Crush  (+Dex)]\n" +
                        "You can have a [Bane of Battler 1H Hammer (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Hammer (+Dex)]\n" +
                        "You can have a [Bane of Battler 1H Slash  (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Slash  (+Dex)]\n" +
                        "You can have a [Bane of Battler 1H Sword  (+Con)]\n" +
                        "You can have a [Bane of Battler 1H Sword  (+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Crush  (+Con)]\n" +
                        "You can have a [Bane of Battler 2H Crush  (+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Hammer (+Con)]\n" +
                        "You can have a [Bane of Battler 2H Hammer (+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Slash  (+Con)]\n" +
                        "You can have a [Bane of Battler 2H Slash  (+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Sword  (+Con)]\n" +
                        "You can have a [Bane of Battler 2H Sword  (+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Crush  (LW/+Con)]\n" +
                        "You can have a [Bane of Battler 2H Crush  (LW/+Dex)]\n" +
                        "You can have a [Bane of Battler 2H Slash  (LW/+Con)]\n" +
                        "You can have a [Bane of Battler 2H Slash  (LW/+Dex)]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "alvarus_leggings":
                    {
                        player.Out.SendMessage("You can have a [Alvarus Leggings Cloth]\n" +
                        "You can have a [Alvarus Leggings Chain]\n" +
                        "You can have a [Alvarus Leggings Leather]\n" +
                        "You can have a [Alvarus Leggings Plate]\n" +
                        "You can have a [Alvarus Leggings Reinforced]\n" +
                        "You can have a [Alvarus Leggings Scales]\n" +
                        "You can have a [Alvarus Leggings Studded]\n", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "maddening_scalars":
                    {
                        player.Out.SendMessage("You can have a [Maddening Scalars Caster]\n" +
                        "You can have a [Maddening Scalars Caster Chain]\n" +
                        "You can have a [Maddening Scalars Tank Chain]\n" +
                        "You can have a [Maddening Scalars Tank Leather]\n" +
                        "You can have a [Maddening Scalars Tank Plate]\n" +
                        "You can have a [Maddening Scalars Tank Studded]\n" +
                        "You can have a [Maddening Scalars Tank Reinforced]\n" +
                        "You can have a [Maddening Scalars Tank Scales]\n", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "malice_weapon":
                    {
                        player.Out.SendMessage("You can have a [Malice Axe 1H]\n" +
                        "You can have a [Malice Axe 2H]\n" +
                        "You can have a [Malice Blades]\n" +
                        "You can have a [Malice Blunt]\n" +
                        "You can have a [Malice Crush 1H]\n" +
                        "You can have a [Malice Crush 2H]\n" +
                        "You can have a [Malice Hammer 1H]\n" +
                        "You can have a [Malice Hammer 2H]\n" +
                        "You can have a [Malice LW Crush]\n" +
                        "You can have a [Malice LW Slash]\n" +
                        "You can have a [Malice Slash 1H]\n" +
                        "You can have a [Malice Slash 2H]\n" +
                        "You can have a [Malice Sword 1H]\n" +
                        "You can have a [Malice Sword 2H]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "snakecharmers_weapon":
                    {
                        player.Out.SendMessage("You can have a [Snakecharmer's Weapon Flex]\n" +
                        "You can have a [Snakecharmer's Weapon H2H]\n" +
                        "You can have a [Snakecharmer's Weapon Scythe]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "golm":
                    {
                        player.Out.SendMessage("You can have a [Gem of Lost Memories Melee]\n" +
                        "You can have a [Gem of Lost Memories Caster]\n" +
                        "You can have a [Gem of Lost Memories Archer]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "arms_of_the_wind":
                    {
                        player.Out.SendMessage("You can have a [Arms of the Wind Chain]\n" +
                        "You can have a [Arms of the Wind Reinforced]\n" +
                        "You can have a [Arms of the Wind Cloth]\n" +
                        "You can have a [Arms of the Wind Plate]\n" +
                        "You can have a [Arms of the Wind Leather]\n" +
                        "You can have a [Arms of the Wind Studded]\n" +
                        "You can have a [Arms of the Wind Scale]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "atens_shield":
                    {
                        player.Out.SendMessage("You can have a [Aten's Shield Small]\n" +
                        "You can have a [Aten's Shield Medium]\n" +
                        "You can have a [Aten's Shield Large]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "braggarts_bow":
                    {
                        player.Out.SendMessage("You can have a [Braggart's Longbow]\n" +
                        "You can have a [Braggart's Recurve Bow]\n" +
                        "You can have a [Braggart's Composite Bow]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "bruiser":
                    {
                        player.Out.SendMessage("You can have a [Bruiser 2H]\n" +
                        "You can have a [Bruiser 1H]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "cyclops_eye_shield":
                    {
                        player.Out.SendMessage("You can have a [Cyclops Eye Shield Small Mid]\n" +
                        "You can have a [Cyclops Eye Shield Small Hib]\n" +
                        "You can have a [Cyclops Eye Shield Small Alb]\n" +
                        "You can have a [Cyclops Eye Shield Medium Mid]\n" +
                        "You can have a [Cyclops Eye Shield Medium Hib]\n" +
                        "You can have a [Cyclops Eye Shield Medium Alb]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                    break;
                default:
                    {
                        Reply(player, "That item is not yet implimented.");
                    }
                    break;
            }
            return true;
        }
    }
}