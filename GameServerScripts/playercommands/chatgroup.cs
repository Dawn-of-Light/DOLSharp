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
using System.Text;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&chat",
		new string[] {"&c"},
		(uint) ePrivLevel.Player,
		"Chat group command",
		"/c <text>")]
	public class ChatGroupCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
			if (mychatgroup == null)
			{
				client.Player.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (mychatgroup.Listen == true && (((bool) mychatgroup.Members[client.Player]) == false))
			{
				client.Player.Out.SendMessage("Only moderator can talk on this chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (args.Length < 2)
			{
				client.Player.Out.SendMessage("Usage: /c <text>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			StringBuilder text = new StringBuilder(7 + 3 + client.Player.Name.Length + (args.Length - 1)*8);
			text.Append("[Chat] ");
			text.Append(client.Player.Name);
			text.Append(": \"");
			text.Append(args[1]);
			for (int i = 2; i < args.Length; i++)
			{
				text.Append(" ");
				text.Append(args[i]);
			}
			text.Append("\"");
			string message = text.ToString();
			foreach (GamePlayer ply in mychatgroup.Members.Keys)
			{
				ply.Out.SendMessage(message, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
			}
			return 1;
		}
	}

	[CmdAttribute(
		"&chatgroup",
		new string[] {"&cg"},
		(uint) ePrivLevel.Player,
		"Chat group command",
		"/cg <option>")]
	public class ChatGroupSetupCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				PrintHelp(client);
				return 1;
			}
			switch (args[1].ToLower())
			{
				case "help":
					{
						PrintHelp(client);
					}
					break;
				case "invite":
					{
						if (args.Length < 3)
						{
							client.Out.SendMessage("Usage: /cg invite <playername>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						GameClient inviteeclient = WorldMgr.GetClientByPlayerName(args[2], false);
						if (inviteeclient == null || !GameServer.ServerRules.IsSameRealm(inviteeclient.Player, client.Player, true)) // allow priv level>1 to invite anyone
						{
							client.Out.SendMessage("There is no player with this name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (client == inviteeclient)
						{
							client.Out.SendMessage("You cannot invite yourself to a chatgroup", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						ChatGroup oldchatgroup = (ChatGroup) inviteeclient.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (oldchatgroup != null)
						{
							client.Out.SendMessage(inviteeclient.Player.Name + " is already in a chatgroup.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							mychatgroup = new ChatGroup();
							mychatgroup.AddPlayer(client.Player, true);
						}
						else if (((bool) mychatgroup.Members[client.Player]) == false)
						{
							client.Out.SendMessage("You must be the leader to invite a player to join the chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						inviteeclient.Player.TempProperties.setProperty(JOIN_CHATGROUP_PROPERTY, mychatgroup);
						inviteeclient.Player.Out.SendCustomDialog("Do you want to join " + client.Player.Name + "'s chatgroup?", new CustomDialogResponse(JoinChatGroup));
					}
					break;
				case "who":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						int i = 0;
						StringBuilder text = new StringBuilder(64);
						foreach (GamePlayer player in mychatgroup.Members.Keys)
						{
							i++;
							text.Length = 0;
							text.Append(i);
							text.Append(") ");
							text.Append(player.Name);
							if (player.Guild != null)
							{
								text.Append(" <");
								text.Append(player.GuildName);
								text.Append(">");
							}
							client.Out.SendMessage(text.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							//TODO: make function formatstring
						}
					}
					break;
				case "remove":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (args.Length < 3)
						{
							PrintHelp(client);
						}
						GameClient inviteeclient = WorldMgr.GetClientByPlayerName(args[2], false);
						if (inviteeclient == null)
						{
							client.Out.SendMessage("There is no player with this name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.RemovePlayer(inviteeclient.Player);
					}
					break;
				case "leave":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.RemovePlayer(client.Player);
					}
					break;
				case "listen":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[client.Player] == false)
						{
							client.Out.SendMessage("You must be a leader of the chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.Listen = !mychatgroup.Listen;
						string message = "The listen mode of chatgroup is switch " + (mychatgroup.Listen ? "on." : "off.");
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(message, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}					
					}
					break;
				case "leader":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[client.Player] == false)
						{
							client.Out.SendMessage("You must be a leader of the chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (args.Length < 3)
						{
							PrintHelp(client);
						}
						string invitename = String.Join(" ", args, 2, args.Length - 2);
						GameClient inviteeclient = WorldMgr.GetClientByPlayerName(invitename, false);
						if (inviteeclient == null)
						{
							client.Out.SendMessage("There is no player with this name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.Members[inviteeclient.Player] = true;
						string message = inviteeclient.Player.Name + " becomes a moderator.";
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(message, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}					
					}
					break;
				case "public":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[client.Player] == false)
						{
							client.Out.SendMessage("You must be a leader of the chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.IsPublic = true;
						string message = "The chatgroup is now public";
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(message, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}
					}
					break;
				case "private":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[client.Player] == false)
						{
							client.Out.SendMessage("You must be a leader of the chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.IsPublic = false;
						string message = "The chatgroup is now private";
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(message, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}
					}
					break;
				case "join":
					{
						if (args.Length < 3)
						{
							PrintHelp(client);
						}
						GameClient inviteeclient = WorldMgr.GetClientByPlayerName(args[2], false);
						if (inviteeclient == null || !GameServer.ServerRules.IsSameRealm(client.Player, inviteeclient.Player, true)) // allow priv level>1 to join anywhere
						{
							client.Out.SendMessage("There is no player with this name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (client == inviteeclient)
						{
							client.Out.SendMessage("You cannot join your own chatgroup", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						ChatGroup mychatgroup = (ChatGroup) inviteeclient.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("This player is not a member of a chatgroup.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[inviteeclient.Player] == false)
						{
							client.Out.SendMessage("This player is not a leader of a chatgroup.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (!mychatgroup.IsPublic)
						{
							if (args.Length == 4)
							{
								if (args[3] == mychatgroup.Password)
								{
									mychatgroup.AddPlayer(client.Player, false);
								}
							}
							client.Player.Out.SendMessage("This chatgroup is not public.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						mychatgroup.AddPlayer(client.Player, false);
					}
					break;
				case "password":
					{
						ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Out.SendMessage("You must be in a chat group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if ((bool) mychatgroup.Members[client.Player] == false)
						{
							client.Out.SendMessage("You must be a leader of a chatgroup to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (args.Length < 3)
						{
							client.Out.SendMessage("Password of Chatgroup: " + mychatgroup.Password, eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (args[2] == "clear")
						{
							mychatgroup.Password = "";
							return 1;
						}
						mychatgroup.Password = args[2];
						client.Out.SendMessage("You have change the password of Chatgroup to: " + mychatgroup.Password, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				default:
					{
					}
					break;
			}

			return 1;
		}

		public void PrintHelp(GameClient client)
		{
			client.Out.SendMessage("/cg usage :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg help - Displays all chat group commands", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg invite [playername] - Invites the specified player to the chat group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg who - Lists all members of the chat group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg remove [playername] - Removes the specified player from the chat group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg leave - Remove oneself from the chat group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg listen - Puts the chat group on listen mode; only the moderator and leaders can speak", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg leader <name> - Declare another member of the chat group as leader; This player can invite other players into the chat group and speak when the chat group is on listen mode.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg public - The chat group is public and anyone can join by typing /cg join", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg private - The chat group is invite or password-only", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg join [moderator name] - Join a public chat group by name of the moderator", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg join [moderator name] [password] - Join a private chat group which has a password set", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg password - Display the current password for the chat group (moderator only)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg password clear - Clears the current password (moderator only)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/cg password [new password] ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		protected const string JOIN_CHATGROUP_PROPERTY = "JOIN_CHATGROUP_PROPERTY";

		public static void JoinChatGroup(GamePlayer player, byte response)
		{
			ChatGroup mychatgroup = (ChatGroup)player.TempProperties.getObjectProperty(JOIN_CHATGROUP_PROPERTY, null);
			if (mychatgroup == null) return;
			lock (mychatgroup)
			{
				if (mychatgroup.Members.Count < 1)
				{
					player.Out.SendMessage("Chat group doesn't exist anymore.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if (response == 0x01)
				{
					mychatgroup.AddPlayer(player, false);
				}
				player.TempProperties.removeProperty(JOIN_CHATGROUP_PROPERTY);
			}
		}
	}
}