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
 *
 */

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&npc",
		ePrivLevel.GM,
		"Various npc commands",
		"/npc say <text>",
		"/npc yell <text>",
		"/npc action <text>",
		"/npc emote <emote>",
		"/npc face <name>",
		"/npc follow <name>",
		"/npc stopfollow",
		"/npc walkto <name> [speed]",
		"/npc target <name>",
		"/npc weapon <slot>",
		"/npc cast <spellLine> <spellID>")]

	public class NPCCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			if (!(client.Player.TargetObject is GameNPC))
			{
				client.Out.SendMessage("You must target an NPC.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			GameNPC npc = client.Player.TargetObject as GameNPC;

			switch (args[1].ToLower())
			{
				case "say":
					{
						if (args.Length < 3)
						{
							client.Player.Out.SendMessage("Usage: /npc say <message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						string message = string.Join(" ", args, 2, args.Length - 2);
						npc.Say(message);
						break;
					}
				case "yell":
					{
						if (args.Length < 3)
						{
							client.Player.Out.SendMessage("Usage: /npc yell <message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						string message = string.Join(" ", args, 2, args.Length - 2);
						npc.Yell(message);
						break;
					}
				case "action":
					{
						if (args.Length < 3)
						{
							client.Player.Out.SendMessage("Usage: /npc action <action message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						string action = string.Join(" ", args, 2, args.Length - 2);
						action = "<" + npc.Name + " " + action + " >";
						foreach (GamePlayer player in npc.GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
						{
							player.Out.SendMessage(action, eChatType.CT_Emote, eChatLoc.CL_ChatWindow);
						}
						break;
					}
				case "emote":
					{
						if (args.Length != 3)
						{
							client.Player.Out.SendMessage("Usage: /npc emote <emote>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						eEmote emoteID;
						switch (args[2].ToLower())
						{
							case "bang": emoteID = eEmote.BangOnShield; break;
							case "beckon": emoteID = eEmote.Beckon; break;
							case "beg": emoteID = eEmote.Beg; break;
							case "blush": emoteID = eEmote.Blush; break;
							case "bow": emoteID = eEmote.Bow; break;
							case "charge": emoteID = eEmote.LetsGo; break;
							case "cheer": emoteID = eEmote.Cheer; break;
							case "clap": emoteID = eEmote.Clap; break;
							case "cry": emoteID = eEmote.Cry; break;
							case "curtsey": emoteID = eEmote.Curtsey; break;
							case "dance": emoteID = eEmote.Dance; break;
							case "dismiss": emoteID = eEmote.Distract; break;
							case "flex": emoteID = eEmote.Flex; break;
							case "hug": emoteID = eEmote.Hug; break;
							case "induct": emoteID = eEmote.Induct; break;
							case "kiss": emoteID = eEmote.BlowKiss; break;
							case "laugh": emoteID = eEmote.Laugh; break;
							case "military": emoteID = eEmote.Military; break;
							case "no": emoteID = eEmote.No; break;
							case "point": emoteID = eEmote.Point; break;
							case "ponder": emoteID = eEmote.Ponder; break;
							case "present": emoteID = eEmote.Present; break;
							case "raise": emoteID = eEmote.Raise; break;
							case "rude": emoteID = eEmote.Rude; break;
							case "salute": emoteID = eEmote.Salute; break;
							case "shrug": emoteID = eEmote.Shrug; break;
							case "slap": emoteID = eEmote.Slap; break;
							case "slit": emoteID = eEmote.Slit; break;
							case "surrender": emoteID = eEmote.Surrender; break;
							case "taunt": emoteID = eEmote.Taunt; break;
							case "victory": emoteID = eEmote.Victory; break;
							case "wave": emoteID = eEmote.Wave; break;
							case "yes": emoteID = eEmote.Yes; break;
							default: return;
						}

						npc.Emote(emoteID);
						break;
					}
				case "walkto":
					{
						if (args.Length < 3 || args.Length > 4)
						{
							client.Out.SendMessage("Usage: /npc walkto <targetname> [speed]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						int speed = 200;
						if (args.Length == 4)
						{
							speed = Convert.ToInt32(args[3]);
						}

						int X = 0;
						int Y = 0;
						int Z = 0;
						switch (args[2].ToLower())
						{
							case "me":
								{
									X = client.Player.X;
									Y = client.Player.Y;
									Z = client.Player.Z;
									break;
								}

							default:
								{
									//Check for players by name in visibility distance
									foreach (GamePlayer targetplayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetplayer.Name.ToLower() == args[2].ToLower())
										{
											X = targetplayer.X;
											Y = targetplayer.Y;
											Z = targetplayer.Z;
											break;
										}
									}
									//Check for NPCs by name in visibility distance
									foreach (GameNPC target in npc.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (target.Name.ToLower() == args[2].ToLower())
										{
											X = target.X;
											Y = target.Y;
											Z = target.Z;
											break;
										}
									}
									break;
								}
						}

						if (X == 0 && Y == 0 && Z == 0)
						{
							client.Out.SendMessage("Can't find name " + args[2].ToLower() + " near your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						npc.WalkTo(X, Y, Z, speed);
						client.Out.SendMessage("Your target is walking to your location!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "face":
					{
						if (args.Length != 3)
						{
							client.Player.Out.SendMessage("Usage: /npc face <targetname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						GameLiving target = null;
						switch (args[2].ToLower())
						{
							case "me":
								{
									target = client.Player;
									break;
								}

							default:
								{
									//Check for players by name in visibility distance
									foreach (GamePlayer targetplayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetplayer.Name.ToLower() == args[2].ToLower())
										{
											target = targetplayer;
											break;
										}
									}
									//Check for NPCs by name in visibility distance
									foreach (GameNPC targetNpc in npc.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetNpc.Name.ToLower() == args[2].ToLower())
										{
											target = targetNpc;
											break;
										}
									}
									break;
								}
						}

						if (target == null)
						{
							client.Out.SendMessage("Can't find name " + args[2].ToLower() + " near your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						npc.TurnTo(target);
						break;
					}
				case "follow":
					{
						if (args.Length != 3)
						{
							client.Player.Out.SendMessage("Usage: /npc follow <targetname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						GameLiving target = null;
						switch (args[2].ToLower())
						{
							case "me":
								{
									target = client.Player;
									break;
								}

							default:
								{
									//Check for players by name in visibility distance
									foreach (GamePlayer targetplayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetplayer.Name.ToLower() == args[2].ToLower())
										{
											target = targetplayer;
											break;
										}
									}
									//Check for NPCs by name in visibility distance
									foreach (GameNPC targetNpc in npc.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetNpc.Name.ToLower() == args[2].ToLower())
										{
											target = targetNpc;
											break;
										}
									}
									break;
								}
						}

						if (target == null)
						{
							client.Out.SendMessage("Can't find name " + args[2].ToLower() + " near your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						npc.Follow(target, 128, short.MaxValue);
						break;
					}
				case "stopfollow":
					{
						if (args.Length != 2)
						{
							client.Player.Out.SendMessage("Usage: /npc stopfollow", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						npc.StopFollowing();
						break;
					}
				case "target":
					{
						if (args.Length != 3)
						{
							client.Player.Out.SendMessage("Usage: /npc target <targetName>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						GameLiving target = null;
						switch (args[2].ToLower())
						{
							case "self":
								{
									target = npc;
								}
								break;

							case "me":
								{
									target = client.Player;
								}
								break;

							default:
								{
									//Check for players by name in visibility distance
									foreach (GamePlayer targetplayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetplayer.Name.ToLower() == args[2].ToLower())
										{
											target = targetplayer;
											break;
										}
									}
									//Check for NPCs by name in visibility distance
									foreach (GameNPC targetNpc in npc.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if (targetNpc.Name.ToLower() == args[2].ToLower())
										{
											target = targetNpc;
											break;
										}
									}
									break;
								}
						}

						if (target == null)
						{
							client.Out.SendMessage("Can't find name " + args[2].ToLower() + " near your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						npc.TargetObject = target;
						client.Out.SendMessage(npc.Name + " now target " + target.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}


				case "cast":
					{
						if (args.Length != 4)
						{
							client.Player.Out.SendMessage("Usage: /npc cast <spellLine> <spellID>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Player.Out.SendMessage("(Be sure the npc target something to be able to cast)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						SpellLine line = SkillBase.GetSpellLine(args[2]);
						List<Spell> spells = SkillBase.GetSpellList(line.KeyName);
						if (spells.Count <= 0)
						{
							client.Out.SendMessage("No spells found in line " + args[2] + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == Convert.ToInt32(args[3]))
								{
									npc.CastSpell(spl, line);
									return;
								}
							}
						}

						client.Out.SendMessage("Spell with id " + Convert.ToInt16(args[3]) + " not found in db!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

						break;
					}
				case "weapon":
					{
						if (args.Length != 3)
						{
							client.Player.Out.SendMessage("Usage: /npc weapon <activeWeaponSlot>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						if (Convert.ToInt16(args[2]) < 0 || Convert.ToInt16(args[2]) > 2)
						{
							client.Player.Out.SendMessage("The activeWeaponSlot must be between 0 and 2.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}

						GameLiving.eActiveWeaponSlot slot = (GameLiving.eActiveWeaponSlot)Convert.ToInt16(args[2]);
						npc.SwitchWeapon(slot);
						client.Player.Out.SendMessage(npc.Name + " will now use its " + Enum.GetName(typeof(GameLiving.eActiveWeaponSlot), slot) + " weapon to attack.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

						break;
					}
				default:
					{
						client.Out.SendMessage("Type /npc for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
			}
		}
	}
}