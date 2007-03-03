//Designed by Overdriven for TFTR/DOL Test
//Multiple rewards - tested and working (if released)

using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Database;

namespace DOL.GS.Scripts
{
    public class RewardNPC : GameNPC
    {
        public override bool AddToWorld()
        {
            if (this.CurrentRegionID >= 1 || this.CurrentRegionID <= 800)
            {
                Name = "Lord Gebonan";
                Model = 52;
                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.Cloak, 1720);
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2871);
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 2872);
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2873);
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 2876);
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 2875);
                template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 649, 72);
                Inventory = template.CloseTemplate();
            }
            GuildName = "Aftermath Reward Master";
            Level = 50;
            SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
            base.AddToWorld();
            return true;
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            SendReply(player, "Hello, I guess you wish to collect some sort of [reward]?");
            return true;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            switch (str)
            {
                #region Base Interactions
                case "reward":

                    SendReply(t,
                        "You know, I can offer you many types of rewards but I don't [know] if I should.");
                    break;

                case "know":

                    SendReply(t,
                        "Okay, Okay, I suppose your [harassment] won't stop?");
                    break;

                case "harassment":

                    SendReply(t,
                        "Okay, I suggest you look at the rewards I've got [available] for you, remember; they can be based on different things!");
                    break;

                case "available":

                    SendReply(t,
                        "So, do you want a [Realm Rank] based reward, a [Experience] based reward, a [Solo kills] based reward or even a [Group kills] based reward, which would you like?");
                    break;

                #endregion

                #region Realm Rank Reward

                case "Realm Rank":
                    SendReply(t,
                        "Well, here's the reward I have for you [currently] I hope you're Realm Rank 5!");
                    break;

                case "currently":
                    {
                    if (t.Level != 50)
                        {
                            SendReply(t, "You need to be level 50 for this reward");
                            return false;
                        }
                        int rr5 = 513500;
                        if (t.RealmPoints >= rr5)
                        {
                                t.AddSpecialization(SkillBase.GetSpecialization("Morphs"));
                                t.AddSpellLine(SkillBase.GetSpellLine("Morphs"));
                                t.Out.SendUpdatePlayerSkills();             
                                SendReply(t, "Well, it seems you've earned some new skils... Log out then back in to see what they were");
                            }
                            else
                            {
                                if (t.RealmPoints < rr5)
                                    SendReply(t, "Sorry, but you need to be Realm Rank 5 for this reward, try again when you get more realm points");
                            }
                        }
                    break;

                #endregion Realm Rank Reward

                #region Experience Reward

                case "Experience":

                    if (t.Experience <= 210526009074)
                    {
                        SendReply(t, "I'm sorry but you've not gained the necessary experience needed for your reward!");
                        break;
                    }
                    else
                    {
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TFTRewardJewel");
                        generic0.CopyFrom(tgeneric0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        SendReply(t, "Here is your experience based reward!!");
                        break;
                    }

                #endregion Experience Reward

                #region Solo kills reward

                case "Solo kills":

                  if (t.KillsAlbionDeathBlows + t.KillsMidgardDeathBlows + t.KillsHiberniaDeathBlows >= 1000)
                    {
                    if ((t.CharacterClass.Name == "Cabalist") || (t.CharacterClass.Name == "Sorcerer") || (t.CharacterClass.Name == "Theurgist") || (t.CharacterClass.Name == "Wizard") || (t.CharacterClass.Name == "Necromancer")
                        || (t.CharacterClass.Name == "Enchanter") || (t.CharacterClass.Name == "Eldritch") || (t.CharacterClass.Name == "Mentalist") || (t.CharacterClass.Name == "Heretic") || (t.CharacterClass.Name == "Animist") || (t.CharacterClass.Name == "Bainshee")
                        || (t.CharacterClass.Name == "Runemaster") || (t.CharacterClass.Name == "Warlock") || (t.CharacterClass.Name == "Spiritmaster") || (t.CharacterClass.Name == "Bonedancer"))
                    {
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TFTKahnei");
                        generic0.CopyFrom(tgeneric0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        SendReply(t, "Enjoy your new necklace!");
                        break;
                    }
                         if ((t.CharacterClass.Name == "Healer") || (t.CharacterClass.Name == "Shaman") || (t.CharacterClass.Name == "Cleric") || (t.CharacterClass.Name == "Friar") || (t.CharacterClass.Name == "Heretic")
                         || (t.CharacterClass.Name == "Bard") || (t.CharacterClass.Name == "Druid") || (t.CharacterClass.Name == "Warden"))
                    {
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TFTJinoi");
                        generic0.CopyFrom(tgeneric0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        SendReply(t, "Enjoy your new necklace!");
                        break;
                    }
                         if ((t.CharacterClass.Name == "Shadowblade") || (t.CharacterClass.Name == "Hunter") || (t.CharacterClass.Name == "Nightshade") || (t.CharacterClass.Name == "Ranger") || (t.CharacterClass.Name == "Scout")
                         || (t.CharacterClass.Name == "Minstrel") || (t.CharacterClass.Name == "Infiltrator"))
                    {
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TFTHienia");
                        generic0.CopyFrom(tgeneric0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        SendReply(t, "Enjoy your new necklace!");
                        break;
                    }
                    if ((t.CharacterClass.Name == "Warrior") || (t.CharacterClass.Name == "Berserker") || (t.CharacterClass.Name == "Thane") || (t.CharacterClass.Name == "Savage") || (t.CharacterClass.Name == "Skald")
                    || (t.CharacterClass.Name == "Blademaster") || (t.CharacterClass.Name == "Hero") || (t.CharacterClass.Name == "Champion") || (t.CharacterClass.Name == "Warden") || (t.CharacterClass.Name == "Paladin") || (t.CharacterClass.Name == "Armsman")
                    || (t.CharacterClass.Name == "Mercenary") || (t.CharacterClass.Name == "Vampiir") || (t.CharacterClass.Name == "Valkyrie") || (t.CharacterClass.Name == "Reaver"))
                    {
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "TFTFalriim");
                        generic0.CopyFrom(tgeneric0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        SendReply(t, "Enjoy your new necklace!");
                        break;
                    }
                    else
                    {
                        if (t.KillsAlbionDeathBlows + t.KillsMidgardDeathBlows + t.KillsHiberniaDeathBlows <= 999)
                            SendReply(t, "Sorry, just no");
                    }
            }
                    break;

                #endregion Solo kills reward

                #region Group kills reward

                case "Group kills":

                    if (t.KillsAlbionDeathBlows + t.KillsMidgardDeathBlows + t.KillsHiberniaDeathBlows >= 500)
                    {
                        SendReply(t, "Heh, you've either been grouping alot or leeching alot but none the less, here's your reward");
                        InventoryItem generic0 = new InventoryItem();
                        ItemTemplate tgeneric0 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "foh_scroll");
                        InventoryItem generic1 = new InventoryItem();
                        ItemTemplate tgeneric1 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "foe_scroll");
                        InventoryItem generic2 = new InventoryItem();
                        ItemTemplate tgeneric2 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "fop_scroll");
                        generic0.CopyFrom(tgeneric0);
                        generic1.CopyFrom(tgeneric1);
                        generic2.CopyFrom(tgeneric2);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                    }
                    else
                    {
                        if (t.KillsAlbionDeathBlows + t.KillsMidgardDeathBlows + t.KillsHiberniaDeathBlows < 499)
                            SendReply(t, "You don't have enough kills! Come back when you do!");
                    }

                    break;
                #endregion Group kills reward
            }
                        
            return true;
        }
        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }
}