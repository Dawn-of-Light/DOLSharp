/*Written: 6/9/07
 *Written by: Crazys
 * This NPC controls the respecing of MLs
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
    public class MLRepecNPC : GameNPC
    {
        private static ItemTemplate item = null;
        private static ItemTemplate mlrespectoken = null;

        string mlchoicea = "";
        string mlchoiceb = "";
        string mlstart = "";
        string mlfinish = "";
        int mllevel = 0;
        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {

            item = new ItemTemplate();
            item.Id_nb = "mlrespectoken";
            item.Name = "ML Respec Token";
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
            mlrespectoken = item;
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer t = source as GamePlayer;
            if (WorldMgr.GetDistance(this, source) > WorldMgr.INTERACT_DISTANCE)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t != null && item != null && item.Id_nb == "mlrespectoken")
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
                
                if (t.GetSpellLine("Banelord01") == null &&
                    t.GetSpellLine("Battlemaster01") == null &&
                    t.GetSpellLine("Convoker01") == null &&
                    t.GetSpellLine("Perfecter01") == null &&
                    t.GetSpellLine("Sojourner01") == null &&
                    t.GetSpellLine("Stormlord01") == null &&
                    t.GetSpellLine("Spymaster01") == null &&
                    t.GetSpellLine("Warlord01") == null)
                {
                    t.Out.SendMessage("You have not completed any MLs and therefore cannot respec them!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    //gets starting ml line
                    if (t.GetSpellLine("Banelord01") != null)            {mlstart = "[Banelord]";}
                    else if (t.GetSpellLine("Battlemaster01") != null)   {mlstart = "[Battlemaster]";}
                    else if (t.GetSpellLine("Convoker01") != null)       {mlstart = "[Convoker]";}
                    else if (t.GetSpellLine("Perfecter01") != null)      {mlstart = "[Perfecter]";}
                    else if (t.GetSpellLine("Sojourner01") != null)      {mlstart = "[Sojourner]";}
                    else if (t.GetSpellLine("Stormlord01") != null)      {mlstart = "[Stormlord]";}
                    else if (t.GetSpellLine("Spymaster01") != null)      {mlstart = "[Spymaster]";}
                    else if (t.GetSpellLine("Warlord01") != null)        {mlstart = "[Warlord]";}
                    
                    //gets finishing ml line
                    if (mlstart == mlchoicea)
                    {
                        mlfinish = mlchoiceb;
                    }
                    else
                    {
                        mlfinish = mlchoicea;
                    }

                    //gest ml level
                    if (t.GetSpellLine("Banelord10") != null ||
                    t.GetSpellLine("Battlemaster10") != null ||
                    t.GetSpellLine("Convoker10") != null ||
                    t.GetSpellLine("Perfecter10") != null ||
                    t.GetSpellLine("Sojourner10") != null ||
                    t.GetSpellLine("Stormlord10") != null ||
                    t.GetSpellLine("Spymaster10") != null ||
                    t.GetSpellLine("Warlord10") != null)
                    {mllevel = 10;}
                    else if (t.GetSpellLine("Banelord09") != null ||
                    t.GetSpellLine("Battlemaster09") != null ||
                    t.GetSpellLine("Convoker09") != null ||
                    t.GetSpellLine("Perfecter09") != null ||
                    t.GetSpellLine("Sojourner09") != null ||
                    t.GetSpellLine("Stormlord09") != null ||
                    t.GetSpellLine("Spymaster09") != null ||
                    t.GetSpellLine("Warlord09") != null)
                    {mllevel = 9;}
                    else if (t.GetSpellLine("Banelord08") != null ||
                    t.GetSpellLine("Battlemaster08") != null ||
                    t.GetSpellLine("Convoker08") != null ||
                    t.GetSpellLine("Perfecter08") != null ||
                    t.GetSpellLine("Sojourner08") != null ||
                    t.GetSpellLine("Stormlord08") != null ||
                    t.GetSpellLine("Spymaster08") != null ||
                    t.GetSpellLine("Warlord08") != null)
                    {mllevel = 8;}
                    else if (t.GetSpellLine("Banelord07") != null ||
                    t.GetSpellLine("Battlemaster07") != null ||
                    t.GetSpellLine("Convoker07") != null ||
                    t.GetSpellLine("Perfecter07") != null ||
                    t.GetSpellLine("Sojourner07") != null ||
                    t.GetSpellLine("Stormlord07") != null ||
                    t.GetSpellLine("Spymaster07") != null ||
                    t.GetSpellLine("Warlord07") != null)
                    {mllevel = 7;}
                    else if (t.GetSpellLine("Banelord06") != null ||
                    t.GetSpellLine("Battlemaster06") != null ||
                    t.GetSpellLine("Convoker06") != null ||
                    t.GetSpellLine("Perfecter06") != null ||
                    t.GetSpellLine("Sojourner06") != null ||
                    t.GetSpellLine("Stormlord06") != null ||
                    t.GetSpellLine("Spymaster06") != null ||
                    t.GetSpellLine("Warlord06") != null)
                    {mllevel = 6;}
                    else if (t.GetSpellLine("Banelord05") != null ||
                    t.GetSpellLine("Battlemaster05") != null ||
                    t.GetSpellLine("Convoker05") != null ||
                    t.GetSpellLine("Perfecter05") != null ||
                    t.GetSpellLine("Sojourner05") != null ||
                    t.GetSpellLine("Stormlord05") != null ||
                    t.GetSpellLine("Spymaster05") != null ||
                    t.GetSpellLine("Warlord05") != null)
                    {mllevel = 5;}
                    else if (t.GetSpellLine("Banelord04") != null ||
                    t.GetSpellLine("Battlemaster04") != null ||
                    t.GetSpellLine("Convoker04") != null ||
                    t.GetSpellLine("Perfecter04") != null ||
                    t.GetSpellLine("Sojourner04") != null ||
                    t.GetSpellLine("Stormlord04") != null ||
                    t.GetSpellLine("Spymaster04") != null ||
                    t.GetSpellLine("Warlord04") != null)
                    {mllevel = 4;}
                    else if (t.GetSpellLine("Banelord03") != null ||
                    t.GetSpellLine("Battlemaster03") != null ||
                    t.GetSpellLine("Convoker03") != null ||
                    t.GetSpellLine("Perfecter03") != null ||
                    t.GetSpellLine("Sojourner03") != null ||
                    t.GetSpellLine("Stormlord03") != null ||
                    t.GetSpellLine("Spymaster03") != null ||
                    t.GetSpellLine("Warlord03") != null)
                    {mllevel = 3;}
                    else if (t.GetSpellLine("Banelord02") != null ||
                    t.GetSpellLine("Battlemaster02") != null ||
                    t.GetSpellLine("Convoker02") != null ||
                    t.GetSpellLine("Perfecter02") != null ||
                    t.GetSpellLine("Sojourner02") != null ||
                    t.GetSpellLine("Stormlord02") != null ||
                    t.GetSpellLine("Spymaster02") != null ||
                    t.GetSpellLine("Warlord02") != null)
                    {mllevel = 2;}
                    else if (t.GetSpellLine("Banelord01") != null ||
                    t.GetSpellLine("Battlemaster01") != null ||
                    t.GetSpellLine("Convoker01") != null ||
                    t.GetSpellLine("Perfecter01") != null ||
                    t.GetSpellLine("Sojourner01") != null ||
                    t.GetSpellLine("Stormlord01") != null ||
                    t.GetSpellLine("Spymaster01") != null ||
                    t.GetSpellLine("Warlord01") != null)
                    {mllevel = 1;}
                }
                if (mlstart == "[Banelord]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Banelord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Battlemaster]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Battlemaster10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Convoker]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Convoker10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Perfecter]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Perfecter10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Sojourner]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Sojourner10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Stormlord]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Stormlord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Spymaster]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Spymaster10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlstart == "[Warlord]")
                {
                    t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord01"));
                    if (mllevel >= 2)
                    {
                        t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord02"));
                        if (mllevel >= 3)
                        {
                            t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord03"));
                            if (mllevel >= 4)
                            {
                                t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord04"));
                                if (mllevel >= 5)
                                {
                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.RemoveSpellLine(SkillBase.GetSpellLine("Warlord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (mlfinish == "[Banelord]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Banelord01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Banelord03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Banelord04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Banelord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Banelord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Banelord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Banelord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Banelord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Battlemaster]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Battlemaster10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Convoker]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Convoker01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Convoker03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Convoker04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Convoker05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Convoker07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Convoker08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Convoker09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Convoker10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Perfecter]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Perfecter01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Perfecter03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Perfecter04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Perfecter05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Perfecter07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Perfecter08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Perfecter09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Perfecter10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Sojourner]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Sojourner01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Sojourner03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Sojourner04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Sojourner05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Sojourner07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Sojourner08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Sojourner09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Sojourner10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Stormlord]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Stormlord01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Stormlord03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Stormlord04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Stormlord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Stormlord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Stormlord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Stormlord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Stormlord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Spymaster]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Spymaster01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Spymaster03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Spymaster04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Spymaster05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Spymaster07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Spymaster08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Spymaster09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Spymaster10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (mlfinish == "[Warlord]")
                {
                    t.AddSpellLine(SkillBase.GetSpellLine("Warlord01"));
                    if (mllevel >= 2)
                    {
                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord02"));
                        if (mllevel >= 3)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("Warlord03"));
                            if (mllevel >= 4)
                            {
                                t.AddSpellLine(SkillBase.GetSpellLine("Warlord04"));
                                if (mllevel >= 5)
                                {
                                    t.AddSpellLine(SkillBase.GetSpellLine("Warlord05"));
                                    if (mllevel >= 6)
                                    {
                                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord06"));
                                        if (mllevel >= 7)
                                        {
                                            t.AddSpellLine(SkillBase.GetSpellLine("Warlord07"));
                                            if (mllevel >= 8)
                                            {
                                                t.AddSpellLine(SkillBase.GetSpellLine("Warlord08"));
                                                if (mllevel >= 9)
                                                {
                                                    t.AddSpellLine(SkillBase.GetSpellLine("Warlord09"));
                                                    if (mllevel == 10)
                                                    {
                                                        t.AddSpellLine(SkillBase.GetSpellLine("Warlord10"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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

            player.Out.SendMessage("Hail! I have the power to convert your MLs into the opposite line!  Hand me a ML respec token and I will change your path! Just be warned you cannot change back unless you give me another Token!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            if (player.Client.Account.PrivLevel > 2)
            {
                player.Out.SendMessage("Additionally, as an Admin, I can give you a [mlrespectoken].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "mlrespectoken":
                    if (t.Client.Account.PrivLevel < 3)
                    {
                        break;
                    }
                    GiveItem(t, mlrespectoken);
                    break;
                default: break;
            }
            return true;
        }

    }

}