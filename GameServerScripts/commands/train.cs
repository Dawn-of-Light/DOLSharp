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
/* Original from Etaew
 * Updates: Timx, Daeli
 */  
using System;
using System.Collections;
using System.Text;
using DOL.GS.Commands;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts.Commands
{
    [CmdAttribute(
        "&train",
        ePrivLevel.Player,
        "Trains a line by the specified amount.",
        "/train <line> <level>",
        "e.g. /train Dual Wield 50")]
    public class TrainCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (client.Player.TargetObject is GameTrainer == false)
            {
                client.Out.SendMessage("You have to be at your trainer to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if(args.Length < 3)
            {
                DisplaySyntax(client);
                return;
            }

            int level;
            if(Int32.TryParse(args[args.Length - 1], out level) == false)
            {
                DisplaySyntax(client);
                return;
            }

            string line = string.Join(" ", args, 1, args.Length - 2);

            var dbSpec = GameServer.Database.SelectObject<DBSpecialization>(string.Format("KeyName LIKE \"{0}%\"", line));

			Specialization spec = null;

			if (dbSpec != null)
				spec = client.Player.GetSpecialization(dbSpec.KeyName);

            if(spec == null)
            {
                client.Out.SendMessage("The provided skill could not be found.", 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            int currentSpecLevel = spec.Level;

            if (currentSpecLevel >= client.Player.Level)
            {
                client.Out.SendMessage("You can't train in this specialization again this level!", 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (level <= currentSpecLevel)
            {
                client.Out.SendMessage("You have already trained the skill to this amount!", 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            level -= currentSpecLevel;
            ushort skillspecialtypoints = 0;
            int speclevel = 0;
            bool changed = false;

            for (int i = 0; i < level; i++)
            {
                if (spec.Level + speclevel >= client.Player.Level)
                {
                    client.Out.SendMessage("You can't train in this specialization again this level!", 
                        eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    break;
                }

                // graveen: /train now match 1.87 autotrain rules
                if ((client.Player.SkillSpecialtyPoints + client.Player.GetAutoTrainPoints(spec, 3)) - skillspecialtypoints >= (spec.Level + speclevel) + 1)
                {
                    changed = true;
                    skillspecialtypoints += (ushort)((spec.Level + speclevel) + 1);
                    if (spec.Level + speclevel < client.Player.Level / 4 && client.Player.GetAutoTrainPoints(spec, 4) != 0)
                        skillspecialtypoints -= (ushort)((spec.Level + speclevel) + 1);

                    speclevel++;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("That specialization costs " + (spec.Level + 1) + " specialization points!");
                    sb.AppendLine("You don't have that many specialization points left for this level.");

                    client.Out.SendMessage(sb.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                }
            }

            if (changed == true)
            {
				// tolakram - add some additional error checking to avoid overflow error
				if (client.Player.SkillSpecialtyPoints >= skillspecialtypoints)
				{
					client.Player.SkillSpecialtyPoints -= skillspecialtypoints;
					spec.Level += speclevel;
					client.Player.OnSkillTrained(spec);
					client.Out.SendUpdatePoints();
					client.Out.SendTrainerWindow();
					client.Out.SendMessage("Training complete!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("That specialization costs " + (spec.Level + 1) + " specialization points!");
					sb.AppendLine("You don't have that many specialization points left for this level.");

					client.Out.SendMessage(sb.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
            }
        }
    }
}