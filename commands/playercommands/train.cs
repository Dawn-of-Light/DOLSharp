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
using System.Text;
using DOL.Database;
using DOL.GS.Commands;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[Cmd(
		"&train",
		new string[] { "&trainline", "&trainskill" }, // new aliases to work around 1.105 client /train command
		ePrivLevel.Player,
		"Trains a line by the specified amount",
		"/train <line> <level>",
		"e.g. /train Dual Wield 50")]
	public class TrainCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const string CantTrainSpec = "You can't train in this specialization again this level!";
		private const string NotEnoughPointsLeft = "You don't have that many specialization points left for this level.";

		// Allow to automate this command: no checks for spam command
		private bool automated = false;
		public TrainCommandHandler() {}
		public TrainCommandHandler(bool automated)
		{
			this.automated = automated;
		}
		
		#region ICommandHandler Members

		public void OnCommand(GameClient client, string[] args)
		{
			if (!automated && IsSpammingCommand(client.Player, "train"))
			{
				return;
			}

			// no longer used since 1.105, except if we explicitely want
			if (client.Version >= GameClient.eClientVersion.Version1105)
			{
				if (!ServerProperties.Properties.CUSTOM_TRAIN)
				{
					client.Out.SendTrainerWindow();
					return;
				}
			}

			GameTrainer trainer = client.Player.TargetObject as GameTrainer;
			// Make sure the player is at a trainer.
			if (trainer == null || !trainer.CanTrain(client.Player))
			{
				client.Out.SendMessage("You have to be at your trainer to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// Make sure the user gave us atleast the specialization line and the level to train it to.
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			// Get the level to train the specialization line to.
			int level;
			if (!int.TryParse(args[args.Length - 1], out level))
			{
				DisplaySyntax(client);
				return;
			}

			// Get the specialization line.
			string line = string.Join(" ", args, 1, args.Length - 2);
			line = GameServer.Database.Escape(line);

			var dbSpec = GameServer.Database.SelectObject<DBSpecialization>(string.Format("KeyName LIKE '{0}%'", line));

			Specialization spec = null;

			if (dbSpec != null)
			{
				spec = client.Player.GetSpecialization(dbSpec.KeyName);
			}
			else
			{
				// if this is a custom line it might not be in the db so search for exact match on player
				spec = client.Player.GetSpecialization(line);
			}

			if (spec == null)
			{
				client.Out.SendMessage("The provided skill could not be found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return;
			}

			// Make sure the player can actually train the given specialization.
			int currentSpecLevel = spec.Level;

			if (currentSpecLevel >= client.Player.BaseLevel)
			{
				client.Out.SendMessage(CantTrainSpec, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return;
			}

			if (level <= currentSpecLevel)
			{
				client.Out.SendMessage("You have already trained the skill to this amount!", eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);

				return;
			}

			// Calculate the points to remove for training the specialization.
			level -= currentSpecLevel;
			ushort skillSpecialtyPoints = 0;
			int specLevel = 0;
			bool changed = false;
			bool canAutotrainSpec = client.Player.GetAutoTrainPoints(spec, 4) != 0;
			int autotrainPoints = client.Player.GetAutoTrainPoints(spec, 3);

			for (int i = 0; i < level; i++)
			{
				if (spec.Level + specLevel >= client.Player.BaseLevel)
				{
					client.Out.SendMessage(CantTrainSpec, eChatType.CT_System, eChatLoc.CL_SystemWindow);

					break;
				}

				// graveen: /train now match 1.87 autotrain rules
				if ((client.Player.SkillSpecialtyPoints + autotrainPoints) - skillSpecialtyPoints >= (spec.Level + specLevel) + 1)
				{
					changed = true;
					skillSpecialtyPoints += (ushort) ((spec.Level + specLevel) + 1);

					if (spec.Level + specLevel < client.Player.Level/4 && canAutotrainSpec)
					{
						skillSpecialtyPoints -= (ushort) ((spec.Level + specLevel) + 1);
					}

					specLevel++;
				}
				else
				{
					var sb = new StringBuilder();
					sb.AppendLine("That specialization costs " + (spec.Level + 1) + " specialization points!");
					sb.AppendLine(NotEnoughPointsLeft);

					client.Out.SendMessage(sb.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
				}
			}

			if (changed)
			{
				// tolakram - add some additional error checking to avoid overflow error
				if (client.Player.SkillSpecialtyPoints >= skillSpecialtyPoints)
				{
					client.Player.SkillSpecialtyPoints -= skillSpecialtyPoints;
					spec.Level += specLevel;

					client.Player.OnSkillTrained(spec);

					client.Out.SendUpdatePoints();
					client.Out.SendTrainerWindow();

					client.Out.SendMessage("Training complete!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					var sb = new StringBuilder();
					sb.AppendLine("That specialization costs " + (spec.Level + 1) + " specialization points!");
					sb.AppendLine(NotEnoughPointsLeft);

					client.Out.SendMessage(sb.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		#endregion
	}
}