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

using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&assist", ePrivLevel.Player, "Assist your target", "/assist [playerName]")]
	public class AssistCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "assist"))
				return;

            if (args.Length > 1)
            {
                //This makes absolutely no sense, but it's one of the weird features of the live servers
                if (args[1].ToLower() == client.Player.Name.ToLower()) //ToLower() is correct, don't change it!
                {
                    //We cannot assist our target when it has no target.
                    if (!HasTarget(client, client.Player))
                        return;

                    YouAssist(client, client.Player.Name, client.Player.TargetObject);
                    return;
                }

                GamePlayer assistPlayer = null;
                
                //Should be faster then WorldMgr.GetClientByPlayerName
                foreach(GamePlayer plr in client.Player.GetPlayersInRadius(2048))
                {
                    //ToLower() is correct, don't change it!
                    if(plr.Name.ToLower() != args[1].ToLower())
                        continue;

                    assistPlayer = plr;
                    break;
                }

                if(assistPlayer != null)
                {
                    //Each server type handles the assist command on it's own way.
                    switch(GameServer.Instance.Configuration.ServerType)
                    {
                            #region Normal rules
                        case eGameServerType.GST_Normal:
                            {
                                //We cannot assist players of an enemy realm.
                                if (!SameRealm(client, assistPlayer, false))
                                    return;

                                //We cannot assist our target when it has no target.
                                if (!HasTarget(client, assistPlayer))
                                    return;

                                YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                return;
                            }
                            #endregion
                            #region PvE rules
                        case eGameServerType.GST_PvE:
                            {
                                //We cannot assist our target when it has no target.
                                if (!HasTarget(client, assistPlayer))
                                    return;

                                YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                return;
                            }
                            #endregion
                            #region PvP rules
                        case eGameServerType.GST_PvP:
                            {
                                //Note:
                                //I absolutely don't have experience with pvp servers - change it when something is wrong.

                                //Lets check if the client and it's targeted player are in the same alliance.
                                if(client.Player.Guild.alliance.Contains(assistPlayer.Guild))
                                {
                                    //We cannot assist our target when it has no target.
                                    if (!HasTarget(client, assistPlayer))
                                        return;

                                    YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                    return;
                                }
                                            
                                //They are no alliance members, maybe guild members?
                                if (client.Player.Guild.GetMemberByName(assistPlayer.Name) != null)
                                {
                                    //We cannot assist our target when it has no target.
                                    if (!HasTarget(client, assistPlayer))
                                        return;

                                    YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                    return;
                                }

                                //They are no alliance or guild members - maybe group members?
                                if(client.Player.Group.IsInTheGroup(assistPlayer))
                                {
                                    //We cannot assist our target when it has no target.
                                    if (!HasTarget(client, assistPlayer))
                                        return;

                                    YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                    return;
                                }

                                //Ok, they are not in the same alliance, guild or group - maybe in the same battle group?
                                BattleGroup clientBattleGroup = (BattleGroup)client.Player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                                if(clientBattleGroup != null)
                                {
                                    if(clientBattleGroup.Members.Contains(assistPlayer))
                                    {
                                        //We cannot assist our target when it has no target.
                                        if (!HasTarget(client, assistPlayer))
                                            return;

                                        YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                        return;
                                    }
                                }
                                            
                                //Ok, they are not in the same alliance, guild, group or battle group - maybe in the same chat group?
                                ChatGroup clientChatGroup = (ChatGroup)client.Player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
                                if(clientChatGroup != null)
                                {
                                    if(clientChatGroup.Members.Contains(assistPlayer))
                                    {
                                        //We cannot assist our target when it has no target.
                                        if (!HasTarget(client, assistPlayer))
                                            return;

                                        YouAssist(client, assistPlayer.Name, assistPlayer.TargetObject);
                                        return;
                                    }
                                }

                                //They are not in the same alliance, guild, group, battle group or chat group. And now? Well, they are enemies!
                                NoValidTarget(client, assistPlayer);
                                return;
                            }
                            #endregion
                    }
                }

                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.MemberNotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            
            if (client.Player.TargetObject != null)
            {
                //This makes absolutely no sense, but it's one of the weird features of the live servers
                if (client.Player.TargetObject == client.Player)
                {
                    YouAssist(client, client.Player.Name, client.Player.TargetObject);
                    return;
                }

                //Only assist npc's or players!
                if(client.Player.TargetObject is GameNPC || client.Player.TargetObject is GamePlayer)
                {
                    //We cannot assist game objects!
                    if (client.Player.TargetObject is GameMovingObject)
                    {
                        NoValidTarget(client, client.Player.TargetObject as GameLiving);
                        return;
                    }

                    //Each server type handles the assist command on it's own way.
                    switch(GameServer.Instance.Configuration.ServerType)
                    {
                        #region Normal rules
                        case eGameServerType.GST_Normal:
                            {
                                //We cannot assist npc's or players of an enemy realm.
                                if (!SameRealm(client, client.Player.TargetObject as GameLiving, false))
                                    return;

                                //We cannot assist our target when it has no target.
                                if (!HasTarget(client, client.Player.TargetObject as GameLiving))
                                    return;

                                YouAssist(client, client.Player.TargetObject.GetName(0, true), (client.Player.TargetObject as GameLiving).TargetObject);
                                return;
                            }
                        #endregion
                        #region PvE rules
                        case eGameServerType.GST_PvE:
                            {
                                if(client.Player.TargetObject is GamePlayer)
                                {
                                    //We cannot assist our target when it has no target.
                                    if (!HasTarget(client, (client.Player.TargetObject as GameLiving)))
                                        return;

                                    YouAssist(client, client.Player.TargetObject.Name, (client.Player.TargetObject as GameLiving).TargetObject);
                                    return;
                                }
                                        
                                if (client.Player.TargetObject is GameNPC)
                                {
                                    if (!SameRealm(client, (client.Player.TargetObject as GameNPC), true))
                                        return;
                                    else
                                    {
                                        //We cannot assist our target when it has no target.
                                        if (!HasTarget(client, (client.Player.TargetObject as GameNPC)))
                                            return;

                                        YouAssist(client, client.Player.TargetObject.GetName(0, true), (client.Player.TargetObject as GameLiving).TargetObject);
                                        return;
                                    }
                                }
                            } break;
                        #endregion
                        #region PvP rules
                        case eGameServerType.GST_PvP:
                            {
                                //Note:
                                //I absolutely don't have experience with pvp servers - change it when something is wrong.

                                if(client.Player.TargetObject is GamePlayer)
                                {
                                    GamePlayer targetPlayer = client.Player.TargetObject as GamePlayer;

                                    //Lets check if the client and it's targeted player are in the same alliance.
                                    if(client.Player.Guild.alliance.Contains(targetPlayer.Guild))
                                    {
                                        //We cannot assist our target when it has no target
                                        if (!HasTarget(client, targetPlayer))
                                            return;

                                        YouAssist(client, targetPlayer.Name, targetPlayer.TargetObject);
                                        return;
                                    }
                                            
                                    //They are no alliance members, maybe guild members?
                                    if (client.Player.Guild.GetMemberByName(targetPlayer.Name) != null)
                                    {
                                        //We cannot assist our target when it has no target
                                        if (!HasTarget(client, targetPlayer))
                                            return;

                                        YouAssist(client, targetPlayer.Name, targetPlayer.TargetObject);
                                        return;
                                    }

                                    //They are no alliance or guild members - maybe group members?
                                    if(client.Player.Group.IsInTheGroup(targetPlayer))
                                    {
                                        //We cannot assist our target when it has no target
                                        if (!HasTarget(client, targetPlayer))
                                            return;

                                        YouAssist(client, targetPlayer.Name, targetPlayer.TargetObject);
                                        return;
                                    }

                                    //Ok, they are not in the same alliance, guild or group - maybe in the same battle group?
                                    BattleGroup clientBattleGroup = (BattleGroup)client.Player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                                    if(clientBattleGroup != null)
                                    {
                                        if(clientBattleGroup.Members.Contains(targetPlayer))
                                        {
                                            //We cannot assist our target when it has no target
                                            if (!HasTarget(client, targetPlayer))
                                                return;

                                            YouAssist(client, targetPlayer.Name, targetPlayer.TargetObject);
                                            return;
                                        }
                                    }
                                            
                                    //Ok, they are not in the same alliance, guild, group or battle group - maybe in the same chat group?
                                    ChatGroup clientChatGroup = (ChatGroup)client.Player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
                                    if(clientChatGroup != null)
                                    {
                                        if(clientChatGroup.Members.Contains(targetPlayer))
                                        {
                                            //We cannot assist our target when it has no target
                                            if (!HasTarget(client, targetPlayer))
                                                return;

                                            YouAssist(client, targetPlayer.Name, targetPlayer.TargetObject);
                                            return;
                                        }
                                    }

                                    //They are not in the same alliance, guild, group, battle group or chat group. And now? Well, they are enemies!
                                    NoValidTarget(client, targetPlayer);
                                    return;
                                }
                                        
                                if(client.Player.TargetObject is GameNPC)
                                {
                                    if(client.Player.TargetObject is GamePet)
                                    {
                                        GamePet targetPet = client.Player.TargetObject as GamePet;

                                        if(targetPet.Owner is GamePlayer)
                                        {
                                            GamePlayer targetPlayer = targetPet.Owner as GamePlayer;

                                            //Lets check if the client and it's targeted pets owner are in the same alliance.
                                            if(client.Player.Guild.alliance.Contains(targetPlayer.Guild))
                                            {
                                                //We cannot assist our target when it has no target
                                                if (!HasTarget(client, targetPet))
                                                    return;

                                                YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                return;
                                            }
                                            
                                            //They are no alliance members, maybe guild members?
                                            if (client.Player.Guild.GetMemberByName(targetPlayer.Name) != null)
                                            {
                                                //We cannot assist our target when it has no target
                                                if (!HasTarget(client, targetPet))
                                                    return;

                                                YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                return;
                                            }

                                            //They are no alliance or guild members - maybe group members?
                                            if(client.Player.Group.IsInTheGroup(targetPlayer))
                                            {
                                                //We cannot assist our target when it has no target
                                                if (!HasTarget(client, targetPet))
                                                    return;

                                                YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                return;
                                            }

                                            //Ok, they are not in the same alliance, guild or group - maybe in the same battle group?
                                            BattleGroup clientBattleGroup = (BattleGroup)client.Player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                                            if(clientBattleGroup != null)
                                            {
                                                if(clientBattleGroup.Members.Contains(targetPlayer))
                                                {
                                                    //We cannot assist our target when it has no target
                                                    if (!HasTarget(client, targetPet))
                                                        return;

                                                    YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                    return;
                                                }
                                            }
                                            
                                            //Ok, they are not in the same alliance, guild, group or battle group - maybe in the same chat group?
                                            ChatGroup clientChatGroup = (ChatGroup)client.Player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
                                            if(clientChatGroup != null)
                                            {
                                                if(clientChatGroup.Members.Contains(targetPlayer))
                                                {
                                                    //We cannot assist our target when it has no target
                                                    if (!HasTarget(client, targetPet))
                                                        return;

                                                    YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                    return;
                                                }
                                            }

                                            //They are not in the same alliance, guild, group, battle group or chat group. And now? Well, they are enemies!
                                            NoValidTarget(client, targetPet);
                                            return;
                                        }

                                        if(targetPet.Owner is GameNPC)
                                        {
                                            if (!SameRealm(client, (targetPet.Owner as GameNPC), true))
                                                return;
                                            else
                                            {
                                                //We cannot assist our target when it has no target
                                                if (!HasTarget(client, targetPet))
                                                    return;

                                                YouAssist(client, targetPet.GetName(0, false), targetPet.TargetObject);
                                                return;
                                            }
                                        }
                                    }

                                    if(client.Player.TargetObject is GameKeepGuard)
                                    {
                                        //Note:
                                        //We do not check if the targeted guard is attacking us, because this can be a bug!

                                        GameKeepGuard targetGuard = client.Player.TargetObject as GameKeepGuard;
                                        Guild targetedGuardGuild = GuildMgr.GetGuildByName(targetGuard.GuildName);

                                        //We can assist guards of an unclaimed keep!
                                        if(targetedGuardGuild == null)
                                        {
                                            //We cannot assist our target when it has no target
                                            if (!HasTarget(client, targetGuard))
                                                return;

                                            YouAssist(client, targetGuard.GetName(0, false), targetGuard.TargetObject);
                                            return;
                                        }

                                        //Is the guard of our guild?
                                        if(client.Player.Guild == targetedGuardGuild)
                                        {
                                            //We cannot assist our target when it has no target
                                            if (!HasTarget(client, targetGuard))
                                                return;

                                            YouAssist(client, targetGuard.GetName(0, false), targetGuard.TargetObject);
                                            return;
                                        }

                                        //Is the guard of one of our alliance guilds?
                                        if(client.Player.Guild.alliance.Contains(targetedGuardGuild))
                                        {
                                            //We cannot assist our target when it has no target
                                            if (!HasTarget(client, targetGuard))
                                                return;

                                            YouAssist(client, targetGuard.GetName(0, false), targetGuard.TargetObject);
                                            return;
                                        }

                                        //The guard is not of one of our alliance guilds and our guild. And now? Well, he is an enemy and we cannot assist enemies!
                                        NoValidTarget(client, targetGuard);
                                        return;
                                    }
                                }
                            } break;
                            #endregion
                    }
                }
            }

            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.SelectMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return;
		}

        #region OnCommand member methods

        private bool HasTarget(GameClient client, GameLiving livingToCheck)
        {
            if (livingToCheck.TargetObject != null)
                return true;

            //We cannot assist our target when it has no target.
            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.DoesntHaveTarget", livingToCheck.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return false;
        }

        private void NoValidTarget(GameClient client, GameLiving livingToAssist)
        {
            //Original live text: {0} is not a member of your realm!
            //
            //The original text sounds stupid if we use it for rams or other things: The battle ram is not a member of your realm!
            //
            //But the text is also used for rams that are a member of our realm, so we don't use it.
            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.NotValid", livingToAssist.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return;
        }

        private bool SameRealm(GameClient client, GameLiving livingToCheck, bool usePvEPvPRule)
        {
            if (usePvEPvPRule)
            {
                if (livingToCheck.Realm != 0)
                    return true;
            }
            else
            {
                if (livingToCheck.Realm == client.Player.Realm)
                    return true;
            }

            //We cannot assist livings of an enemy realm.
            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.NoRealmMember", livingToCheck.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return false;
        }

        private void YouAssist(GameClient client, string targetName, GameObject assistTarget)
        {
            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Assist.YouAssist", targetName), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            client.Out.SendChangeTarget(assistTarget);
            return;
        }

        #endregion
    }
}