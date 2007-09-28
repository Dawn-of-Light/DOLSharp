using System;
using DOL;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    [NPCGuildScript("Scholar")]
    public class Scholar : GameNPC
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
            GuildName = "Artifact Scholar";
            Flags = 16;
            base.AddToWorld();
            return true;
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem iitem)
        {
            if (!(source is GamePlayer))
                return false;
            GamePlayer player = source as GamePlayer;
            switch (iitem.Id_nb)
            {
                case "shadeofmist_scrolls,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "shade_of_mist") && PlayerArtifactIsActivated(player, "shade_of_mist"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Shades of Mist cloak!");
                        else if (PlayerHasArtifact(player, "shade_of_mist") && !PlayerArtifactIsActivated(player, "shade_of_mist")
                            && PlayerHasArtifact(player, "shadeofmist_scrolls,_1_of_3") && PlayerHasArtifact(player, "shadeofmist_scrolls,_2_of_3") &&
                            PlayerHasArtifact(player, "shadeofmist_scrolls,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_shadeofmist");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("shadeofmist_scrolls,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("shadeofmist_scrolls,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("shadeofmist_scrolls,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Egg_of_Youth,_Scroll_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "egg_of_youth") && PlayerArtifactIsActivated(player, "egg_of_youth"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Egg of Youth!");
                        else if (PlayerHasArtifact(player, "egg_of_youth") && !PlayerArtifactIsActivated(player, "egg_of_youth")
                            && PlayerHasArtifact(player, "Egg_of_Youth,_Scroll_1_of_3") && PlayerHasArtifact(player, "Egg_of_Youth,_Scroll_2_of_3") &&
                            PlayerHasArtifact(player, "Egg_of_Youth,_Scroll_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_eggofyouth");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Egg_of_Youth,_Scroll_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Egg_of_Youth,_Scroll_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Egg_of_Youth,_Scroll_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Bane_of_Battler,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "bane_of_battler") && PlayerArtifactIsActivated(player, "bane_of_battler"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bane of Battler!");
                        else if (PlayerHasArtifact(player, "bane_of_battler") && !PlayerArtifactIsActivated(player, "bane_of_battler")
                            && PlayerHasArtifact(player, "Bane_of_Battler,_1_of_3") && PlayerHasArtifact(player, "Bane_of_Battler,_2_of_3") &&
                            PlayerHasArtifact(player, "Bane_of_Battler,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_baneofbattler");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Bane_of_Battler,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Bane_of_Battler,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Bane_of_Battler,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Alvarus'_Letter,_part_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "alvarus_leggings") && PlayerArtifactIsActivated(player, "alvarus_leggings"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Alvarus' Leggings!");
                        else if (PlayerHasArtifact(player, "alvarus_leggings") && !PlayerArtifactIsActivated(player, "alvarus_leggings")
                            && PlayerHasArtifact(player, "Alvarus'_Letter,_part_1_of_3") && PlayerHasArtifact(player, "Alvarus'_Letter,_part_2_of_3") &&
                            PlayerHasArtifact(player, "Alvarus'_Letter,_part_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_alvarusleggings");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Alvarus'_Letter,_part_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Alvarus'_Letter,_part_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Alvarus'_Letter,_part_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "King's_Vase,_piece_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "band_of_stars") && PlayerArtifactIsActivated(player, "band_of_stars"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Band of Stars!");
                        else if (PlayerHasArtifact(player, "band_of_stars") && !PlayerArtifactIsActivated(player, "band_of_stars")
                            && PlayerHasArtifact(player, "King's_Vase,_piece_1_of_3") && PlayerHasArtifact(player, "King's_Vase,_piece_2_of_3") &&
                            PlayerHasArtifact(player, "King's_Vase,_piece_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_bandofstars");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("King's_Vase,_piece_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("King's_Vase,_piece_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("King's_Vase,_piece_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Apprentice_Notes,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "bracers_of_zoarkat") && PlayerArtifactIsActivated(player, "bracers_of_zoarkat"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bracers of Zoarkat!");
                        else if (PlayerHasArtifact(player, "bracers_of_zoarkat") && !PlayerArtifactIsActivated(player, "bracers_of_zoarkat")
                            && PlayerHasArtifact(player, "Apprentice_Notes,_1_of_3") && PlayerHasArtifact(player, "Apprentice_Notes,_2_of_3") &&
                            PlayerHasArtifact(player, "Apprentice_Notes,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_bracersofzoarkat");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Apprentice_Notes,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Apprentice_Notes,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Apprentice_Notes,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Arbitors_Paper,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "cerebracer") && PlayerArtifactIsActivated(player, "cerebracer"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Ceremonial Bracers!");
                        else if (PlayerHasArtifact(player, "bracers_of_zoarkat") && !PlayerArtifactIsActivated(player, "cerebracer")
                            && PlayerHasArtifact(player, "Arbitors_Paper,_1_of_3") && PlayerHasArtifact(player, "Arbitors_Paper,_2_of_3") &&
                            PlayerHasArtifact(player, "Arbitors_Paper,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_ceremonialbracers");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Arbitors_Paper,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Arbitors_Paper,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Arbitors_Paper,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Mad_Tales,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "maddening_scalars") && PlayerArtifactIsActivated(player, "maddening_scalars"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Maddening Scalars!");
                        else if (PlayerHasArtifact(player, "maddening_scalars") && !PlayerArtifactIsActivated(player, "maddening_scalars")
                            && PlayerHasArtifact(player, "Mad_Tales,_1_of_3") && PlayerHasArtifact(player, "Mad_Tales,_2_of_3") &&
                            PlayerHasArtifact(player, "Mad_Tales,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_maddeningscalars");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Mad_Tales,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Mad_Tales,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Mad_Tales,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Julea's_Story,_part_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "snakecharmers_weapon") && PlayerArtifactIsActivated(player, "snakecharmers_weapon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Snakecharmer's weapon!");
                        else if (PlayerHasArtifact(player, "snakecharmers_weapon") && !PlayerArtifactIsActivated(player, "snakecharmers_weapon")
                            && PlayerHasArtifact(player, "Julea's_Story,_part_1_of_3") && PlayerHasArtifact(player, "Julea's_Story,_part_2_of_3") &&
                            PlayerHasArtifact(player, "Julea's_Story,_part_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_snakecharmersweapon");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Julea's_Story,_part_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Julea's_Story,_part_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Julea's_Story,_part_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Story_of_Malice,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "malice_weapon") && PlayerArtifactIsActivated(player, "malice_weapon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Malice Weapon!");
                        else if (PlayerHasArtifact(player, "malice_weapon") && !PlayerArtifactIsActivated(player, "malice_weapon")
                            && PlayerHasArtifact(player, "Story_of_Malice,_1_of_3") && PlayerHasArtifact(player, "Story_of_Malice,_2_of_3") &&
                            PlayerHasArtifact(player, "Story_of_Malice,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_maliceweapon");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Story_of_Malice,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Story_of_Malice,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Story_of_Malice,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Gem_of_Lost_Memories_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "golm") && PlayerArtifactIsActivated(player, "golm"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Gem of Lost Memories!");
                        else if (PlayerHasArtifact(player, "golm") && !PlayerArtifactIsActivated(player, "golm")
                            && PlayerHasArtifact(player, "Gem_of_Lost_Memories_1_of_3") && PlayerHasArtifact(player, "Gem_of_Lost_Memories_2_of_3") &&
                            PlayerHasArtifact(player, "Gem_of_Lost_Memories_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_golm");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Gem_of_Lost_Memories_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Gem_of_Lost_Memories_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Gem_of_Lost_Memories_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Silvery_Fish_Scale,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "arms_of_the_wind") && PlayerArtifactIsActivated(player, "arms_of_the_wind"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Arms of the Wind!");
                        else if (PlayerHasArtifact(player, "arms_of_the_wind") && !PlayerArtifactIsActivated(player, "arms_of_the_wind")
                            && PlayerHasArtifact(player, "Silvery_Fish_Scale,_1_of_3") && PlayerHasArtifact(player, "Bronze_Fish_Scale,_2_of_3") &&
                            PlayerHasArtifact(player, "Gold_Fish_Scale,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_armsofthewind");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Silvery_Fish_Scale,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Bronze_Fish_Scale,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Gold_Fish_Scale,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Champion's_Notes,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "atens_shield") && PlayerArtifactIsActivated(player, "atens_shield"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have Aten's Shield!");
                        else if (PlayerHasArtifact(player, "atens_shield") && !PlayerArtifactIsActivated(player, "atens_shield")
                            && PlayerHasArtifact(player, "Champion's_Notes,_1_of_3") && PlayerHasArtifact(player, "Champion's_Notes,_2_of_3") &&
                            PlayerHasArtifact(player, "Champion's_Notes,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_atensshield");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Champion's_Notes,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Champion's_Notes,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Champion's_Notes,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Atlantis_Tablet,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "atlantis_tablet") && PlayerArtifactIsActivated(player, "atlantis_tablet"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Atlantis Tablet!");
                        else if (PlayerHasArtifact(player, "atlantis_tablet") && !PlayerArtifactIsActivated(player, "atlantis_tablet")
                            && PlayerHasArtifact(player, "Atlantis_Tablet,_1_of_3") && PlayerHasArtifact(player, "Atlantis_Tablet,_2_of_3") &&
                            PlayerHasArtifact(player, "Atlantis_Tablet,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_atlantistablet");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Atlantis_Tablet,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Atlantis_Tablet,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Atlantis_Tablet,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Belt_of_moon_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "belt_of_the_moon") && PlayerArtifactIsActivated(player, "belt_of_the_moon"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Belt of the Moon!");
                        else if (PlayerHasArtifact(player, "belt_of_the_moon") && !PlayerArtifactIsActivated(player, "belt_of_the_moon")
                            && PlayerHasArtifact(player, "Belt_of_moon_1_of_3") && PlayerHasArtifact(player, "Belt_of_moon_2_of_3") &&
                            PlayerHasArtifact(player, "Belt_of_moon_2_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_beltofthemoon");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Belt_of_moon_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Belt_of_moon_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Belt_of_moon_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Scholar's_Notes,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "belt_of_the_sun") && PlayerArtifactIsActivated(player, "belt_of_the_sun"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Belt of the Sun!");
                        else if (PlayerHasArtifact(player, "belt_of_the_sun") && !PlayerArtifactIsActivated(player, "belt_of_the_sun")
                            && PlayerHasArtifact(player, "Scholar's_Notes,_1_of_3") && PlayerHasArtifact(player, "Scholar's_Notes,_2_of_3") &&
                            PlayerHasArtifact(player, "Scholar's_Notes,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_beltofthesun");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Scholar's_Notes,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Scholar's_Notes,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Scholar's_Notes,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Carved_Tablet,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "braggarts_bow") && PlayerArtifactIsActivated(player, "braggarts_bow"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Braggart's Bow!");
                        else if (PlayerHasArtifact(player, "braggarts_bow") && !PlayerArtifactIsActivated(player, "braggarts_bow")
                            && PlayerHasArtifact(player, "Carved_Tablet,_1_of_3") && PlayerHasArtifact(player, "Carved_Tablet,_2_of_3") &&
                            PlayerHasArtifact(player, "Carved_Tablet,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_braggartsbow");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Carved_Tablet,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Carved_Tablet,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Carved_Tablet,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "tale_of_bruiser,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "bruiser") && PlayerArtifactIsActivated(player, "bruiser"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Bruiser!");
                        else if (PlayerHasArtifact(player, "bruiser") && !PlayerArtifactIsActivated(player, "bruiser")
                            && PlayerHasArtifact(player, "tale_of_bruiser,_1_of_3") && PlayerHasArtifact(player, "tale_of_bruiser,_2_of_3") &&
                            PlayerHasArtifact(player, "tale_of_bruiser,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_bruiser");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("tale_of_bruiser,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("tale_of_bruiser,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("tale_of_bruiser,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Cloudsong,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "cloudsong") && PlayerArtifactIsActivated(player, "cloudsong"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Cloudsong!");
                        else if (PlayerHasArtifact(player, "cloudsong") && !PlayerArtifactIsActivated(player, "cloudsong")
                            && PlayerHasArtifact(player, "Cloudsong,_1_of_3") && PlayerHasArtifact(player, "Cloudsong,_2_of_3") &&
                            PlayerHasArtifact(player, "Cloudsong,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_cloudsong");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Cloudsong,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Cloudsong,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Cloudsong,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Tyrus's_Episc_Poem,_part_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "croc_tears_ring") && PlayerArtifactIsActivated(player, "croc_tears_ring"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crocodile's Tears Ring!");
                        else if (PlayerHasArtifact(player, "croc_tears_ring") && !PlayerArtifactIsActivated(player, "croc_tears_ring")
                            && PlayerHasArtifact(player, "Tyrus's_Episc_Poem,_part_1_of_3") && PlayerHasArtifact(player, "Tyrus's_Episc_Poem,_part_2_of_3") &&
                            PlayerHasArtifact(player, "Tyrus's_Episc_Poem,_part_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_croctearsring");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Tyrus's_Episc_Poem,_part_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Tyrus's_Episc_Poem,_part_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Tyrus's_Episc_Poem,_part_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Marricus'_Journal,_part_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "croc_tooth_dagger") && PlayerArtifactIsActivated(player, "croc_tooth_dagger"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crocodile's Tooth Dagger!");
                        else if (PlayerHasArtifact(player, "croc_tooth_dagger") && !PlayerArtifactIsActivated(player, "croc_tooth_dagger")
                            && PlayerHasArtifact(player, "Marricus'_Journal,_part_1_of_3") && PlayerHasArtifact(player, "Marricus'_Journal,_part_2_of_3") &&
                            PlayerHasArtifact(player, "Marricus'_Journal,_part_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_croctoothdagger");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Marricus'_Journal,_part_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Marricus'_Journal,_part_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Marricus'_Journal,_part_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Advisor's_Log,_part_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "crown_of_zahur") && PlayerArtifactIsActivated(player, "crown_of_zahur"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Crown of Zahur!");
                        else if (PlayerHasArtifact(player, "crown_of_zahur") && !PlayerArtifactIsActivated(player, "crown_of_zahur")
                            && PlayerHasArtifact(player, "Advisor's_Log,_part_1_of_3") && PlayerHasArtifact(player, "Advisor's_Log,_part_2_of_3") &&
                            PlayerHasArtifact(player, "Advisor's_Log,_part_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_crownofzahur");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Advisor's_Log,_part_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Advisor's_Log,_part_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Advisor's_Log,_part_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Damyon's_Journal,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "cyclops_eye_shield") && PlayerArtifactIsActivated(player, "cyclops_eye_shield"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Cyclops Eye Shield!");
                        else if (PlayerHasArtifact(player, "cyclops_eye_shield") && !PlayerArtifactIsActivated(player, "cyclops_eye_shield")
                            && PlayerHasArtifact(player, "Damyon's_Journal,_1_of_3") && PlayerHasArtifact(player, "Damyon's_Journal,_2_of_3") &&
                            PlayerHasArtifact(player, "Damyon's_Journal,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_cyclopseyeshield");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Damyon's_Journal,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Damyon's_Journal,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Damyon's_Journal,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Loukas'_Journal,_volume_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "dream_sphere") && PlayerArtifactIsActivated(player, "dream_sphere"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Dream Sphere!");
                        else if (PlayerHasArtifact(player, "dream_sphere") && !PlayerArtifactIsActivated(player, "dream_sphere")
                            && PlayerHasArtifact(player, "Loukas'_Journal,_volume_1_of_3") && PlayerHasArtifact(player, "Loukas'_Journal,_volume_2_of_3") &&
                            PlayerHasArtifact(player, "Loukas'_Journal,_volume_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_cyclopseyeshield");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Loukas'_Journal,_volume_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Loukas'_Journal,_volume_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Loukas'_Journal,_volume_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Crafter's_Pages,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "eerie_darkness") && PlayerArtifactIsActivated(player, "eerie_darkness"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Eerie Darkness Lighting Stone!");
                        else if (PlayerHasArtifact(player, "eerie_darkness") && !PlayerArtifactIsActivated(player, "eerie_darkness")
                            && PlayerHasArtifact(player, "Crafter's_Pages,_1_of_3") && PlayerHasArtifact(player, "Crafter's_Pages,_2_of_3") &&
                            PlayerHasArtifact(player, "Crafter's_Pages,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_eeriedarkness");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Crafter's_Pages,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Crafter's_Pages,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Crafter's_Pages,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Enyalios'_Boots,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "enyalios_boots") && PlayerArtifactIsActivated(player, "enyalios_boots"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Enyalio's Boots!");
                        else if (PlayerHasArtifact(player, "enyalios_boots") && !PlayerArtifactIsActivated(player, "enyalios_boots")
                            && PlayerHasArtifact(player, "Enyalios'_Boots,_1_of_3") && PlayerHasArtifact(player, "Enyalios'_Boots,_2_of_3") &&
                            PlayerHasArtifact(player, "Enyalios'_Boots,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_enyaliosboots");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Enyalios'_Boots,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Enyalios'_Boots,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Enyalios'_Boots,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Song_of_Erinys,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "erinys_charm") && PlayerArtifactIsActivated(player, "erinys_charm"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have a Erinys Charm!");
                        else if (PlayerHasArtifact(player, "erinys_charm") && !PlayerArtifactIsActivated(player, "erinys_charm")
                            && PlayerHasArtifact(player, "Song_of_Erinys,_1_of_3") && PlayerHasArtifact(player, "Song_of_Erinys,_2_of_3") &&
                            PlayerHasArtifact(player, "Song_of_Erinys,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_erinyscharm");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Song_of_Erinys,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Song_of_Erinys,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Song_of_Erinys,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "Healer's_Notes,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "eternal_plant") && PlayerArtifactIsActivated(player, "eternal_plant"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Eternal Plant!");
                        else if (PlayerHasArtifact(player, "eternal_plant") && !PlayerArtifactIsActivated(player, "eternal_plant")
                            && PlayerHasArtifact(player, "Healer's_Notes,_1_of_3") && PlayerHasArtifact(player, "Healer's_Notes,_2_of_3") &&
                            PlayerHasArtifact(player, "Healer's_Notes,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_erinyscharm");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("Healer's_Notes,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("Healer's_Notes,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("Healer's_Notes,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                case "King_Kiron's_Note,_1_of_3":
                    {
                        if (PlayerHasArtifact(player, "flamedancers_boots") && PlayerArtifactIsActivated(player, "flamedancers_boots"))
                            Reply(player, "I am afraid to tell you this, but it appears from our records that you already have an Flamedancer's Boots!");
                        else if (PlayerHasArtifact(player, "flamedancers_boots") && !PlayerArtifactIsActivated(player, "flamedancers_boots")
                            && PlayerHasArtifact(player, "King_Kiron's_Note,_1_of_3") && PlayerHasArtifact(player, "King_Kiron's_Note,_2_of_3") &&
                            PlayerHasArtifact(player, "King_Kiron's_Note,_3_of_3"))
                        {
                            ItemTemplate artifacttemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "book_of_flamedancersboots");
                            InventoryItem item = new InventoryItem(artifacttemplate);

                            Reply(player, "I have studied the scrolls, and transcribed them into this book.");
                            InventoryItem scroll1 = player.Inventory.GetFirstItemByID("King_Kiron's_Note,_1_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll2 = player.Inventory.GetFirstItemByID("King_Kiron's_Note,_2_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            InventoryItem scroll3 = player.Inventory.GetFirstItemByID("King_Kiron's_Note,_3_of_3", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                            player.Inventory.RemoveItem(scroll1);
                            player.Inventory.RemoveItem(scroll2);
                            player.Inventory.RemoveItem(scroll3);
                            player.Inventory.AddItem(player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item);
                        }
                        else
                            Reply(player, "You do not have the required materials for me to transcribe.");
                        break;
                    }
                default:
                    {
                        Reply(player, "That does not seem to be a valid artifact item.");
                        break;
                    }
            }
            return base.ReceiveItem(source, iitem);
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
    }
}
