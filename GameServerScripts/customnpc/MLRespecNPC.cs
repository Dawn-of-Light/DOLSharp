/*
 * This NPC was originally written by crazys (kyle).
 * Updated 10-23-08 by BluRaven
 * I have only changed (added) the eMLLine enum to get it to compile
 * as a standalone script with the current SVN of DOL.  Place both scripts into your scripts folder and restart your server.
 */

using System;
using System.Collections;
using System.Reflection;
using System.Timers;

using log4net;

using DOL;
using DOL.GS;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.GS.GameEvents;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    public class MLRespecNPC : GameNPC
    {
        private static ItemTemplate mlrespectoken = null;

        byte mlchoicea = 0;
        byte mlchoiceb = 0;
        byte mlfinish = 0;
        
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	public MLRespecNPC()
		: base()
	{
		Flags |= GameNPC.eFlags.PEACE;
	}

	#region Add To World
	public override bool AddToWorld()
	{
		GuildName = "ML Respecialisation";
		Level = 50;
		base.AddToWorld();
		return true;
	}
	
	#endregion Add To World        
        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
			mlrespectoken = GameServer.Database.FindObjectByKey<ItemTemplate>("mlrespectoken");
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer t = source as GamePlayer;
            if (!IsWithinRadius(source, WorldMgr.INTERACT_DISTANCE))
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t.MLLine < 1)
            {
                t.Out.SendMessage("You have not completed any MLs and therefore cannot respec them!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (t != null && item != null && item.Id_nb == "mlrespectoken")
            {
                if (t.CharacterClass.ID == 1)//Paladin
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 2)//Armsman
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 3)//Scout
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Sojourner;
                }
                else if (t.CharacterClass.ID == 4)//Minstrel
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.Sojourner;
                }
                else if (t.CharacterClass.ID == 5)//Theurgist
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 6)//Cleric
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 7)//Wizard
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 8)//Sorcerer
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 9)//Infiltrator
                {
                    mlchoicea = (byte)eMLLine.Spymaster;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 10)//Frair
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 11)//Mercenary
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 12)//Necromancer
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 13)//Cabalist
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 19)//Reaver
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 21)//Thane
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 22)//Warrior
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 23)//Shadowblade
                {
                    mlchoicea = (byte)eMLLine.Spymaster;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 24)//Skald
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.Sojourner;
                }
                else if (t.CharacterClass.ID == 25)//Hunter
                {
                    mlchoicea = (byte)eMLLine.Sojourner;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 26)//Healer
                {
                    mlchoicea = (byte)eMLLine.Sojourner;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 27)//Spiritmaster
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 28)//Shaman
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 29)//Runemaster
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 30)//Bonedancer
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 31)//Berserker
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 32)//Savage
                {
                    mlchoicea = (byte)eMLLine.Warlord;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 33)//Heretic
                {
                    mlchoicea = (byte)eMLLine.Banelord;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 34)//Valkyrie
                {
                    mlchoicea = (byte)eMLLine.Stormlord;
                    mlchoiceb = (byte)eMLLine.Warlord;
                }
                else if (t.CharacterClass.ID == 39)//Bainshee
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 40)//Eldritch
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 41)//Enchanter
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 42)//Mentalist
                {
                    mlchoicea = (byte)eMLLine.Stormlord;
                    mlchoiceb = (byte)eMLLine.Warlord;
                }
                else if (t.CharacterClass.ID == 43)//Blademaster
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 44)//Hero
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Warlord;
                }
                else if (t.CharacterClass.ID == 45)//Champion
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Banelord;
                }
                else if (t.CharacterClass.ID == 46)//Warden
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 47)//Druid
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 48)//Bard
                {
                    mlchoicea = (byte)eMLLine.Sojourner;
                    mlchoiceb = (byte)eMLLine.Perfecter;
                }
                else if (t.CharacterClass.ID == 49)//Nightshade
                {
                    mlchoicea = (byte)eMLLine.Spymaster;
                    mlchoiceb = (byte)eMLLine.BattleMaster;
                }
                else if (t.CharacterClass.ID == 50)//Ranger
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Sojourner;
                }
                else if (t.CharacterClass.ID == 55)//Animist
                {
                    mlchoicea = (byte)eMLLine.Convoker;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 56)//Valewalker
                {
                    mlchoicea = (byte)eMLLine.BattleMaster;
                    mlchoiceb = (byte)eMLLine.Stormlord;
                }
                else if (t.CharacterClass.ID == 58)//Vampiir
                {
                    mlchoicea = (byte)eMLLine.Banelord;
                    mlchoiceb = (byte)eMLLine.Warlord;
                }
                else if (t.CharacterClass.ID == 59)//Warlock
                {
                    mlchoicea = (byte)eMLLine.Banelord;
                    mlchoiceb = (byte)eMLLine.Convoker;
                }

                int i = 0;
                switch (t.MLLine)
                {
                    case (byte)eMLLine.Banelord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Banelord");
                        }
                        break;
                    case (byte)eMLLine.BattleMaster:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Battlemaster");
                        }
                        break;
                    case (byte)eMLLine.Convoker:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Convoker");
                        }
                        break;
                    case (byte)eMLLine.Perfecter:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Perfecter");
                        }
                        break;
                    case (byte)eMLLine.Sojourner:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Sojourner");
                        }
                        break;
                    case (byte)eMLLine.Stormlord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Stormlord");
                        }
                        break;
                    case (byte)eMLLine.Spymaster:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Spymaster");
                        }
                        break;
                    case (byte)eMLLine.Warlord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.RemoveSpellLine("ML" + i + " Warlord");
                        }
                        break;
                    default:
                        break;
                }
                if (t.MLLine == mlchoicea)
                {
                    mlfinish = mlchoiceb;
                }
                else
                {
                    mlfinish = mlchoicea;
                }
                switch (mlfinish)
                {
                    case (byte)eMLLine.Banelord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Banelord"));
                        }
                        t.MLLine = (byte)eMLLine.Banelord;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.BattleMaster:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Battlemaster"));
                        }
                        t.MLLine = (byte)eMLLine.BattleMaster;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Convoker:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Convoker"));
                        }
                        t.MLLine = (byte)eMLLine.Convoker;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Perfecter:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Perfecter"));
                        }
                        t.MLLine = (byte)eMLLine.Perfecter;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Sojourner:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Sojourner"));
                        }
                        t.MLLine = (byte)eMLLine.Sojourner;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Stormlord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Stormlord"));
                        }
                        t.MLLine = (byte)eMLLine.Stormlord;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Spymaster:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Spymaster"));
                        }
                        t.MLLine = (byte)eMLLine.Spymaster;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    case (byte)eMLLine.Warlord:
                        for (i = 1; i < t.MLLevel + 1; i++)
                        {
                            t.AddSpellLine(SkillBase.GetSpellLine("ML" + i + " Warlord"));
                        }
                        t.MLLine = (byte)eMLLine.Warlord;
                        t.Inventory.RemoveItem(item); t.UpdateSpellLineLevels(true); t.Out.SendUpdatePlayerSkills(); t.Out.SendUpdatePlayer(); t.UpdatePlayerStatus();
                        t.SaveIntoDatabase();
                        break;
                    default:
                        break;
                }
                t.Out.SendPlayerQuit(false);
                t.Quit(true);
                t.SaveIntoDatabase();

            }
            return base.ReceiveItem(source, item);
        }

        protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
        {
            GiveItem(null, player, itemTemplate);
        }

        protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
        {
            InventoryItem item = GameInventoryItem.Create<ItemTemplate>(itemTemplate);
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

            player.Out.SendMessage("Hail! I have the power to convert your Master levels into the opposite line!  Hand me a ML respec token and I will change your path! Just be warned you cannot change back unless you give me another Token!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            player.Out.SendMessage("Additionally, I can give you an [mlrespectoken].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            

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
                    GiveItem(t, mlrespectoken);
                    break;
                default: break;
            }
            return true;
        }

    }

}