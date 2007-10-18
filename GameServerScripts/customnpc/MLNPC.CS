/*Written: 6/9/07
 *Written by: Crazys
 * This NPC controls the giving of MLs
 * The ml line choices where matched with camelotherald.com
 * Problems: The player must log out and in... to see changes...
 * (never spent the time to try anything else)
*/
using System;
using System.Collections;
using System.Timers;
using DOL;
using DOL.GS;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.GS.GameEvents;
using DOL.GS.PacketHandler;
using System.Reflection;
using System.Collections;

namespace DOL.GS.Scripts
{
    public class MLNPC : GameNPC
    {
        private static ItemTemplate item = null;
        private static ItemTemplate ml1token = null;
        private static ItemTemplate ml2token = null;
        private static ItemTemplate ml3token = null;
        private static ItemTemplate ml4token = null;
        private static ItemTemplate ml5token = null;
        private static ItemTemplate ml6token = null;
        private static ItemTemplate ml7token = null;
        private static ItemTemplate ml8token = null;
        private static ItemTemplate ml9token = null;
        private static ItemTemplate ml10token = null;

        string mlchoicea = "";
        string mlchoiceb = "";

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {

            item = new ItemTemplate();
            item.Id_nb = "ml1token";
            item.Name = "ML1 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml1token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml2token";
            item.Name = "ML2 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml2token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml3token";
            item.Name = "ML3 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml3token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml4token";
            item.Name = "ML4 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml4token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml5token";
            item.Name = "ML5 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml5token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml6token";
            item.Name = "ML6 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml6token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml7token";
            item.Name = "ML7 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml7token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml8token";
            item.Name = "ML8 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml8token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml9token";
            item.Name = "ML9 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml9token = item;

            item = new ItemTemplate();
            item.Id_nb = "ml10token";
            item.Name = "ML10 Credit";
            item.Level = 50;
            item.Item_Type = 40;
            item.Model = 485;
            item.IsTradable = true;
            item.Object_Type = 0;
            item.Quality = 100;
            item.Weight = 1;
            item.MaxCondition = 100;
            item.MaxDurability = 100;
            item.Condition = 100;
            item.Durability = 100;
            ml10token = item;

        }




        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer t = source as GamePlayer;
            if (WorldMgr.GetDistance(this, source) > WorldMgr.INTERACT_DISTANCE)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t != null && item != null && item.Id_nb == "ml1token")
            {
                if (t.CharacterClass.ID == 1)//Paladin
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 2)//Armsman
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 3)//Scout
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Sojourner]";
                }
                else if (t.CharacterClass.ID == 4)//Minstrel
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Sojourner]";
                }
                else if (t.CharacterClass.ID == 5)//Theurgist
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 6)//Cleric
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 7)//Wizard
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 8)//Sorcerer
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 9)//Infiltrator
                {
                    mlchoicea = "[Spymaster]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 10)//Frair
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 11)//Mercenary
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Banelord]";
                }
                else if (t.CharacterClass.ID == 12)//Necromancer
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 13)//Cabalist
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Battlemaster]";
                }
                //else if (t.CharacterClass.ID == 14)//Fighter
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 15)//Elementalist
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 16)//Acolyte
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 17)//AlbionRogue
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 18)//Mage
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                else if (t.CharacterClass.ID == 19)//Reaver
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Banelord]";
                }
                //else if (t.CharacterClass.ID == 20)//Disciple
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                else if (t.CharacterClass.ID == 21)//Thane
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 22)//Warrior
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 23)//Shadowblade
                {
                    mlchoicea = "[Spymaster]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 24)//Skald
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Sojourner]";
                }
                else if (t.CharacterClass.ID == 25)//Hunter
                {
                    mlchoicea = "[Sojourner]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 26)//Healer
                {
                    mlchoicea = "[Sojourner]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 27)//Spiritmaster
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 28)//Shaman
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 29)//Runemaster
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 30)//Bonedancer
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Banelord]";
                }
                else if (t.CharacterClass.ID == 31)//Berserker
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Banelord]";
                }
                else if (t.CharacterClass.ID == 32)//Savage
                {
                    mlchoicea = "[Warlord]";
                    mlchoiceb = "[Battlemaster]";
                }
                else if (t.CharacterClass.ID == 33)//Heretic
                {
                    mlchoicea = "[Banelord]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 34)//Valkyrie
                {
                    mlchoicea = "[Stormlord]";
                    mlchoiceb = "[Warlord]";
                }
                //else if (t.CharacterClass.ID == 35)//Viking
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 36)//Mystic
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 37)//Seer
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 38)//MidgardRogue
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                else if (t.CharacterClass.ID == 39)//Bainshee
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 40)//Eldritch
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 41)//Enchanter
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 42)//Mentalist
                {
                    mlchoicea = "[Stormlord]";
                    mlchoiceb = "[Warlord]";
                }
                else if (t.CharacterClass.ID == 43)//Blademaster
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Banelord]";
                }
                else if (t.CharacterClass.ID == 44)//Hero
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Warlord]";
                }
                else if (t.CharacterClass.ID == 45)//Champion
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Banelord]";
                }
                else if (t.CharacterClass.ID == 46)//Warden
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 47)//Druid
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 48)//Bard
                {
                    mlchoicea = "[Sojourner]";
                    mlchoiceb = "[Perfecter]";
                }
                else if (t.CharacterClass.ID == 49)//Nightshade
                {
                    mlchoicea = "[Spymaster]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 50)//Ranger
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Sojourner]";
                }
                //else if (t.CharacterClass.ID == 51)//Magicican
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 52)//Guirdian
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 53)//Naturalist
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                //else if (t.CharacterClass.ID == 54)//Stalker
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                else if (t.CharacterClass.ID == 55)//Animist
                {
                    mlchoicea = "[Convoker]";
                    mlchoiceb = "[Stormlord]";
                }
                else if (t.CharacterClass.ID == 56)//Valewalker
                {
                    mlchoicea = "[Battlemaster]";
                    mlchoiceb = "[Stormlord]";
                }
                //else if (t.CharacterClass.ID == 57)//Forester
                //{
                //    mlchoicea = "[]";
                //    mlchoiceb = "[]";
                //}
                else if (t.CharacterClass.ID == 58)//Vampiir
                {
                    mlchoicea = "[Banelord]";
                    mlchoiceb = "[Warlord]";
                }
                else if (t.CharacterClass.ID == 59)//Warlock
                {
                    mlchoicea = "[Banelord]";
                    mlchoiceb = "[Convoker]";
                }
                if (t.GetSpellLine("ML_Banelord") == null &&
                    t.GetSpellLine("ML_Battlemaster") == null &&
                    t.GetSpellLine("ML_Convoker") == null &&
                    t.GetSpellLine("ML_Perfecter") == null &&
                    t.GetSpellLine("ML_Sojourner") == null &&
                    t.GetSpellLine("ML_Stormlord") == null &&
                    t.GetSpellLine("ML_Spymaster") == null &&
                    t.GetSpellLine("ML_Warlord") == null)
                {
                    if (mlchoicea != "")
                    {
                        t.Out.SendMessage("I can grant you access to " + mlchoicea + " or " + mlchoiceb, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);
                        mlchoiceb = "RECIVED";
                    }
                    else
                    {
                        t.Out.SendMessage("You are not experienced enough to have MLs", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                return true;
            }
            else if (t != null && item != null && item.Id_nb == "ml2token")
            {
                if (t.GetSpellLine("ML_Banelord") == null &&
                    t.GetSpellLine("ML_Battlemaster") == null &&
                    t.GetSpellLine("ML_Convoker") == null &&
                    t.GetSpellLine("ML_Perfecter") == null &&
                    t.GetSpellLine("ML_Sojourner") == null &&
                    t.GetSpellLine("ML_Stormlord") == null &&
                    t.GetSpellLine("ML_Spymaster") == null &&
                    t.GetSpellLine("ML_Warlord") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Banelord"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);   t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster21"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Convoker"));
                        t.Out.SendMessage("You have gained Convoker ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Perfecter"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Sojourner"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Spymaster"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Stormlord"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Warlord"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
            else if (t != null && item != null && item.Id_nb == "ml3token")
            {
                if (t.GetSpellLine("ML_Banelord") == null &&
                    t.GetSpellLine("ML_Battlemaster") == null &&
                    t.GetSpellLine("ML_Convoker") == null &&
                    t.GetSpellLine("ML_Perfecter") == null &&
                    t.GetSpellLine("ML_Sojourner") == null &&
                    t.GetSpellLine("ML_Stormlord") == null &&
                    t.GetSpellLine("ML_Spymaster") == null &&
                    t.GetSpellLine("ML_Warlord") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Banelord"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Battlemaster"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Convoker"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Perfecter"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Sojourner"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Spymaster"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Stormlord"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("ML_Warlord"));
                        t.Out.SendMessage("You have gained ML3.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
            else if (t != null && item != null && item.Id_nb == "ml4token")
            {
                if (t.GetSpellLine("Banelord04") == null &&
                    t.GetSpellLine("Battlemaster04") == null &&
                    t.GetSpellLine("Convoker04") == null &&
                    t.GetSpellLine("Perfecter04") == null &&
                    t.GetSpellLine("Sojourner04") == null &&
                    t.GetSpellLine("Stormlord04") == null &&
                    t.GetSpellLine("Spymaster04") == null &&
                    t.GetSpellLine("Warlord04") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord04"));
                        t.Out.SendMessage("You have gained ML4.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

            }
            else if (t != null && item != null && item.Id_nb == "ml5token")
            {
                if (t.GetSpellLine("Banelord05") == null &&
                    t.GetSpellLine("Battlemaster05") == null &&
                    t.GetSpellLine("Convoker05") == null &&
                    t.GetSpellLine("Perfecter05") == null &&
                    t.GetSpellLine("Sojourner05") == null &&
                    t.GetSpellLine("Stormlord05") == null &&
                    t.GetSpellLine("Spymaster05") == null &&
                    t.GetSpellLine("Warlord05") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord05"));
                        t.Out.SendMessage("You have gained ML5.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

            }
            else if (t != null && item != null && item.Id_nb == "ml6token")
            {
                if (t.GetSpellLine("Banelord06") == null &&
                    t.GetSpellLine("Battlemaster06") == null &&
                    t.GetSpellLine("Convoker06") == null &&
                    t.GetSpellLine("Perfecter06") == null &&
                    t.GetSpellLine("Sojourner06") == null &&
                    t.GetSpellLine("Stormlord06") == null &&
                    t.GetSpellLine("Spymaster06") == null &&
                    t.GetSpellLine("Warlord06") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null && t.GetSpellLine("Banelord05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null && t.GetSpellLine("Battlemaster05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null && t.GetSpellLine("Convoker05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null && t.GetSpellLine("Perfecter05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null && t.GetSpellLine("Sojourner05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null && t.GetSpellLine("Spymaster05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null && t.GetSpellLine("Stormlord05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null && t.GetSpellLine("Warlord05") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord06"));
                        t.Out.SendMessage("You have gained ML6.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

            }
            else if (t != null && item != null && item.Id_nb == "ml7token")
            {
                if (t.GetSpellLine("Banelord07") == null &&
                    t.GetSpellLine("Battlemaster07") == null &&
                    t.GetSpellLine("Convoker07") == null &&
                    t.GetSpellLine("Perfecter07") == null &&
                    t.GetSpellLine("Sojourner07") == null &&
                    t.GetSpellLine("Stormlord07") == null &&
                    t.GetSpellLine("Spymaster07") == null &&
                    t.GetSpellLine("Warlord07") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null && t.GetSpellLine("Banelord05") != null && t.GetSpellLine("Banelord06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null && t.GetSpellLine("Battlemaster05") != null && t.GetSpellLine("Battlemaster06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null && t.GetSpellLine("Convoker05") != null && t.GetSpellLine("Convoker06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null && t.GetSpellLine("Perfecter05") != null && t.GetSpellLine("Perfecter06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null && t.GetSpellLine("Sojourner05") != null && t.GetSpellLine("Sojourner06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null && t.GetSpellLine("Spymaster05") != null && t.GetSpellLine("Spymaster06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null && t.GetSpellLine("Stormlord05") != null && t.GetSpellLine("Stormlord06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null && t.GetSpellLine("Warlord05") != null && t.GetSpellLine("Warlord06") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord07"));
                        t.Out.SendMessage("You have gained ML7.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

            }
            else if (t != null && item != null && item.Id_nb == "ml8token")
            {
                if (t.GetSpellLine("Banelord08") == null &&
                    t.GetSpellLine("Battlemaster08") == null &&
                    t.GetSpellLine("Convoker08") == null &&
                    t.GetSpellLine("Perfecter08") == null &&
                    t.GetSpellLine("Sojourner08") == null &&
                    t.GetSpellLine("Stormlord08") == null &&
                    t.GetSpellLine("Spymaster08") == null &&
                    t.GetSpellLine("Warlord08") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null && t.GetSpellLine("Banelord05") != null && t.GetSpellLine("Banelord06") != null && t.GetSpellLine("Banelord07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null && t.GetSpellLine("Battlemaster05") != null && t.GetSpellLine("Battlemaster06") != null && t.GetSpellLine("Battlemaster07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null && t.GetSpellLine("Convoker05") != null && t.GetSpellLine("Convoker06") != null && t.GetSpellLine("Convoker07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null && t.GetSpellLine("Perfecter05") != null && t.GetSpellLine("Perfecter06") != null && t.GetSpellLine("Perfecter07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null && t.GetSpellLine("Sojourner05") != null && t.GetSpellLine("Sojourner06") != null && t.GetSpellLine("Sojourner07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null && t.GetSpellLine("Spymaster05") != null && t.GetSpellLine("Spymaster06") != null && t.GetSpellLine("Spymaster07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null && t.GetSpellLine("Stormlord05") != null && t.GetSpellLine("Stormlord06") != null && t.GetSpellLine("Stormlord07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null && t.GetSpellLine("Warlord05") != null && t.GetSpellLine("Warlord06") != null && t.GetSpellLine("Warlord07") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord08"));
                        t.Out.SendMessage("You have gained ML8.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
            else if (t != null && item != null && item.Id_nb == "ml9token")
            {
                if (t.GetSpellLine("Banelord09") == null &&
                    t.GetSpellLine("Battlemaster09") == null &&
                    t.GetSpellLine("Convoker09") == null &&
                    t.GetSpellLine("Perfecter09") == null &&
                    t.GetSpellLine("Sojourner09") == null &&
                    t.GetSpellLine("Stormlord09") == null &&
                    t.GetSpellLine("Spymaster09") == null &&
                    t.GetSpellLine("Warlord09") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null && t.GetSpellLine("Banelord05") != null && t.GetSpellLine("Banelord06") != null && t.GetSpellLine("Banelord07") != null && t.GetSpellLine("Banelord08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null && t.GetSpellLine("Battlemaster05") != null && t.GetSpellLine("Battlemaster06") != null && t.GetSpellLine("Battlemaster07") != null && t.GetSpellLine("Battlemaster08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null && t.GetSpellLine("Convoker05") != null && t.GetSpellLine("Convoker06") != null && t.GetSpellLine("Convoker07") != null && t.GetSpellLine("Convoker08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null && t.GetSpellLine("Perfecter05") != null && t.GetSpellLine("Perfecter06") != null && t.GetSpellLine("Perfecter07") != null && t.GetSpellLine("Perfecter08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null && t.GetSpellLine("Sojourner05") != null && t.GetSpellLine("Sojourner06") != null && t.GetSpellLine("Sojourner07") != null && t.GetSpellLine("Sojourner08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null && t.GetSpellLine("Spymaster05") != null && t.GetSpellLine("Spymaster06") != null && t.GetSpellLine("Spymaster07") != null && t.GetSpellLine("Spymaster08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null && t.GetSpellLine("Stormlord05") != null && t.GetSpellLine("Stormlord06") != null && t.GetSpellLine("Stormlord07") != null && t.GetSpellLine("Stormlord08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null && t.GetSpellLine("Warlord05") != null && t.GetSpellLine("Warlord06") != null && t.GetSpellLine("Warlord07") != null && t.GetSpellLine("Warlord08") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord09"));
                        t.Out.SendMessage("You have gained ML9.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
            else if (t != null && item != null && item.Id_nb == "ml10token")
            {
                if (t.GetSpellLine("Banelord10") == null &&
                    t.GetSpellLine("Battlemaster10") == null &&
                    t.GetSpellLine("Convoker10") == null &&
                    t.GetSpellLine("Perfecter10") == null &&
                    t.GetSpellLine("Sojourner10") == null &&
                    t.GetSpellLine("Stormlord10") == null &&
                    t.GetSpellLine("Spymaster10") == null &&
                    t.GetSpellLine("Warlord10") == null)
                {
                    if (t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("ML_Banelord") != null && t.GetSpellLine("Banelord04") != null && t.GetSpellLine("Banelord05") != null && t.GetSpellLine("Banelord06") != null && t.GetSpellLine("Banelord07") != null && t.GetSpellLine("Banelord08") != null && t.GetSpellLine("Banelord09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("ML_Battlemaster") != null && t.GetSpellLine("Battlemaster04") != null && t.GetSpellLine("Battlemaster05") != null && t.GetSpellLine("Battlemaster06") != null && t.GetSpellLine("Battlemaster07") != null && t.GetSpellLine("Battlemaster08") != null && t.GetSpellLine("Battlemaster09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("ML_Convoker") != null && t.GetSpellLine("Convoker04") != null && t.GetSpellLine("Convoker05") != null && t.GetSpellLine("Convoker06") != null && t.GetSpellLine("Convoker07") != null && t.GetSpellLine("Convoker08") != null && t.GetSpellLine("Convoker09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("ML_Perfecter") != null && t.GetSpellLine("Perfecter04") != null && t.GetSpellLine("Perfecter05") != null && t.GetSpellLine("Perfecter06") != null && t.GetSpellLine("Perfecter07") != null && t.GetSpellLine("Perfecter08") != null && t.GetSpellLine("Perfecter09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("ML_Sojourner") != null && t.GetSpellLine("Sojourner04") != null && t.GetSpellLine("Sojourner05") != null && t.GetSpellLine("Sojourner06") != null && t.GetSpellLine("Sojourner07") != null && t.GetSpellLine("Sojourner08") != null && t.GetSpellLine("Sojourner09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("ML_Spymaster") != null && t.GetSpellLine("Spymaster04") != null && t.GetSpellLine("Spymaster05") != null && t.GetSpellLine("Spymaster06") != null && t.GetSpellLine("Spymaster07") != null && t.GetSpellLine("Spymaster08") != null && t.GetSpellLine("Spymaster09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("ML_Stormlord") != null && t.GetSpellLine("Stormlord04") != null && t.GetSpellLine("Stormlord05") != null && t.GetSpellLine("Stormlord06") != null && t.GetSpellLine("Stormlord07") != null && t.GetSpellLine("Stormlord08") != null && t.GetSpellLine("Stormlord09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else if (t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("ML_Warlord") != null && t.GetSpellLine("Warlord04") != null && t.GetSpellLine("Warlord05") != null && t.GetSpellLine("Warlord06") != null && t.GetSpellLine("Warlord07") != null && t.GetSpellLine("Warlord08") != null && t.GetSpellLine("Warlord09") != null)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord10"));
                        t.Out.SendMessage("You have gained ML2.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        t.Inventory.RemoveItem(item);     t.Out.SendUpdatePlayerSkills();     t.Out.SendUpdatePlayer();
                    }
                    else
                    {
                        t.Out.SendMessage("You must complete all previous MLs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else
                {
                    t.Out.SendMessage("You already have this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }

            return base.ReceiveItem(source, item);
        }

        protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
        {
            GiveItem(null, player, itemTemplate);
        }

        protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
        {
            InventoryItem item = new InventoryItem(itemTemplate);
            if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
            {
                if (source == null)
                {
                    player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                player.CreateItemOnTheGround(item);
                player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
            }
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            player.Out.SendMessage("Hail! I am the Master of the MLs.  Hand me the appropirate ML credit and I will advance your ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            if (player.Client.Account.PrivLevel > 2)
            {
                player.Out.SendMessage("Additionally, as an Admin, I can give you a [ml1],[ml2],[ml3],[ml4],[ml5],[ml6],[ml7],[ml8],[ml9],[ml10],tokken.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }

            return true;
        }


        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            TurnTo(t.X, t.Y);
            switch (str)
            {
                case "ml1":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml1token);
                    break;
                case "ml2":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml2token);
                    break;
                case "ml3":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml3token);
                    break;
                case "ml4":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml4token);
                    break;
                case "ml5":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml5token);
                    break;
                case "ml6":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml6token);
                    break;
                case "ml7":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml7token);
                    break;
                case "ml8":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml8token);
                    break;
                case "ml9":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml9token);
                    break;
                case "ml10":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, ml10token);
                    break;
                case "Banelord"://11,19,31,33,43,45,58,59,
                        if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 11 ||
                            t.CharacterClass.ID == 19 || t.CharacterClass.ID == 31 ||
                            t.CharacterClass.ID == 33 || t.CharacterClass.ID == 43 ||
                            t.CharacterClass.ID == 45 || t.CharacterClass.ID == 58 ||
                            t.CharacterClass.ID == 59))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Banelord"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Battlemaster"://2,1,3,9,10,11,19,21,22,23,25,31,32,43,44,45,46,50,56,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 2 ||
                            t.CharacterClass.ID == 1 ||
                            t.CharacterClass.ID == 3 ||
                            t.CharacterClass.ID == 9 ||
                            t.CharacterClass.ID == 10 ||
                            t.CharacterClass.ID == 11 ||
                            t.CharacterClass.ID == 19 ||
                            t.CharacterClass.ID == 21 ||
                            t.CharacterClass.ID == 22 ||
                            t.CharacterClass.ID == 23 ||
                            t.CharacterClass.ID == 25 ||
                            t.CharacterClass.ID == 31 ||
                            t.CharacterClass.ID == 32 ||
                            t.CharacterClass.ID == 43 ||
                            t.CharacterClass.ID == 44 ||
                            t.CharacterClass.ID == 45 ||
                            t.CharacterClass.ID == 46 ||
                            t.CharacterClass.ID == 50 ||
                            t.CharacterClass.ID == 56))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Battlemaster"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Convoker"://5,7,8,12,13,27,28,29,30,39,40,41,47,55,59
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 5 ||
                            t.CharacterClass.ID == 7 ||
                            t.CharacterClass.ID == 8 ||
                            t.CharacterClass.ID == 12 ||
                            t.CharacterClass.ID == 13 ||
                            t.CharacterClass.ID == 27 ||
                            t.CharacterClass.ID == 28 ||
                            t.CharacterClass.ID == 29 ||
                            t.CharacterClass.ID == 30 ||
                            t.CharacterClass.ID == 39 ||
                            t.CharacterClass.ID == 40 ||
                            t.CharacterClass.ID == 41 ||
                            t.CharacterClass.ID == 47 ||
                            t.CharacterClass.ID == 55 ||
                            t.CharacterClass.ID == 59))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Convoker"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Perfecter"://6,10,26,28,33,46,47,48,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 6 ||
                            t.CharacterClass.ID == 10 || t.CharacterClass.ID == 26 ||
                            t.CharacterClass.ID == 28 || t.CharacterClass.ID == 33 ||
                            t.CharacterClass.ID == 46 || t.CharacterClass.ID == 47 ||
                            t.CharacterClass.ID == 48))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Perfecter"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Sojourner"://3,4,24,25,26,48,50,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 3 ||
                            t.CharacterClass.ID == 4 || t.CharacterClass.ID == 24 ||
                            t.CharacterClass.ID == 25 || t.CharacterClass.ID == 26 ||
                            t.CharacterClass.ID == 48 || t.CharacterClass.ID == 50))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Sojourner"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Spymaster"://9,23,49,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 9 ||
                            t.CharacterClass.ID == 23 || t.CharacterClass.ID == 49))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Spymaster"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Stormlord"://5,7,8,12,13,21,27,29,30,34,39,40,41,42,49,55,56,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 5 ||
                            t.CharacterClass.ID == 7 ||
                            t.CharacterClass.ID == 8 ||
                            t.CharacterClass.ID == 12 ||
                            t.CharacterClass.ID == 13 ||
                            t.CharacterClass.ID == 21 ||
                            t.CharacterClass.ID == 27 ||
                            t.CharacterClass.ID == 29 ||
                            t.CharacterClass.ID == 30 ||
                            t.CharacterClass.ID == 34 ||
                            t.CharacterClass.ID == 39 ||
                            t.CharacterClass.ID == 40 ||
                            t.CharacterClass.ID == 41 ||
                            t.CharacterClass.ID == 42 ||
                            t.CharacterClass.ID == 49 ||
                            t.CharacterClass.ID == 55 ||
                            t.CharacterClass.ID == 56))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Stormlord"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;
                case "Warlord"://2,1,4,6,22,24,32,34,42,44,58,
                    if (mlchoiceb == "RECIVED" && (t.CharacterClass.ID == 2 ||
                            t.CharacterClass.ID == 1 ||
                            t.CharacterClass.ID == 4 ||
                            t.CharacterClass.ID == 6 ||
                            t.CharacterClass.ID == 22 ||
                            t.CharacterClass.ID == 24 ||
                            t.CharacterClass.ID == 32 ||
                            t.CharacterClass.ID == 34 ||
                            t.CharacterClass.ID == 42 ||
                            t.CharacterClass.ID == 44 ||
                            t.CharacterClass.ID == 58))
                        {
                            mlchoiceb = "Completed";
                            t.AddSpellLine(SkillBase.GetSpellLine("ML_Warlord"));
                            t.Out.SendMessage("You have gained ML1.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            t.Out.SendUpdatePlayerSkills();
                            t.Out.SendUpdatePlayer();
                        }
                    else
                    {
                        t.Out.SendMessage("Your class can not become this ML.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;


                default: break;
            }
            return true;
        }

    }

}