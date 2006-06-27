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
// Respec script Version 1.0 by Echostorm
/* Ver 1.0 Notes:
 * With changes to the core the respec system adds a new (allowed null) field to the DOL character file called RespecAllSkill that contains an integer.
 * All characters with 1 or more in their RespecAllSkill field, who are targeting their trainer will be able to Respec, or reset their spec
 *		points to their full amount and return their specs to 1 clearing their style and spell lists.  One respec is deducted each time.
 * Characters recieve 1 respec upon creation, and 2 more at 20th and 40th levels.  Respecs are currently cumulative due to the high
 *		demand.
 * Respec stones have been added to default item template to prevent confustion with item databases.  They can be created via the /item command
 *		by typing /item create respec_stone.
 * Respec stones may be turned in to trainers for respecs.
 * 
 * TODO: include autotrains in the formula
 * TODO: realm respec
 */


using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&respec",
		(uint)ePrivLevel.Player,
		"Respecs the char",
		"/respec")]
	public class RespecCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				// Check for respecs.
				if (client.Player.RespecAmountAllSkill < 1
					&& client.Player.RespecAmountSingleSkill < 1)
				{
					client.Out.SendMessage("You don't seem to have any respecs available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				if (client.Player.RespecAmountAllSkill > 0)
				{
					client.Out.SendMessage("You have " + client.Player.RespecAmountAllSkill + " full skill respecs available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("Target any trainer and use /respec ALL", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				if (client.Player.RespecAmountSingleSkill > 0)
				{
					client.Out.SendMessage("You have " + client.Player.RespecAmountSingleSkill + " single-line respecs available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("Target any trainer and use /respec <line name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return 1;
			}

			// Player must be speaking with trainer to respec.  (Thus have trainer targeted.) Prevents losing points out in the wild.
			if (client.Player.TargetObject is GameTrainer == false)
			{
				client.Out.SendMessage("You must be speaking with your trainer to respec.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			//total specpoints returned
			int specPoints = 0;

			if (args[1].ToLower() == "all")
			{
				// Check for full respecs.
				if (client.Player.RespecAmountAllSkill < 1)
				{
					client.Out.SendMessage("You don't seem to have any full skill respecs available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				specPoints = RespecAllLines(client.Player); // Wipe skills and styles.

				client.Player.RespecAmountAllSkill--; // Decriment players respecs available.

				if (client.Player.Level == 5)
					client.Player.IsLevelRespecUsed = true;
			}
			else
			{
				// Check for single-line respecs.
				if (client.Player.RespecAmountSingleSkill < 1)
				{
					client.Out.SendMessage("You don't seem to have any single-line respecs available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				string lineName = string.Join(" ", args, 1, args.Length - 1);
				Specialization specLine = client.Player.GetSpecializationByName(lineName, false);
				if (specLine == null)
				{
					client.Out.SendMessage("No line with name '" + lineName + "' found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				if (specLine.Level < 2)
				{
					client.Out.SendMessage("Level of " + specLine.Name + " line is less than 2. ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				specPoints = RespecSingleLine(client.Player, specLine); // Wipe skills and styles.
				client.Player.RespecAmountSingleSkill--; // Decriment players respecs available.
				if (client.Player.Level == 20 || client.Player.Level == 40)
				{
					client.Player.IsLevelRespecUsed = true;
				}
			}

			// Assign full points returned
			client.Player.SkillSpecialtyPoints += specPoints;
			lock (client.Player.GetStyleList().SyncRoot)
			{
				client.Player.GetStyleList().Clear(); // Kill styles
			}
			client.Player.RefreshSpecDependendSkills(false);
			client.Player.UpdateSpellLineLevels(false);
			// Notify Player of points
			client.Out.SendMessage("You regain " + specPoints + " specialization points!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendUpdatePlayerSkills();
			client.Out.SendUpdatePoints();
			client.Out.SendTrainerWindow();
			client.Player.SaveIntoDatabase();

			return 1;
		}

		protected int RespecAllLines(GamePlayer player)
		{
			int specPoints = 0;
			IList specList = player.GetSpecList();
			lock (specList.SyncRoot)
			{
				foreach (Specialization cspec in specList)
				{
					if (cspec.Level < 2)
						continue;
					specPoints += RespecSingleLine(player, cspec);
				}
			}

			return specPoints;
		}

		/// <summary>
		/// Respec single line
		/// </summary>
		/// <param name="player">Player that is doing respec</param>
		/// <returns>Amount of points spent in that line</returns>
		protected int RespecSingleLine(GamePlayer player, Specialization specLine)
		{
			/*
			//Autotrain...
			//get total spec points
			int currentSpecPoints = (specLine.Level * specLine.Level + specLine.Level - 2) / 2;
			//get normal spec points
			int normalSpecPoints = 1;
			//calculate if there has been any autotraining
			int autotrainPool = currentSpecPoints - normalSpecPoints;
			if (autotrainPool != 0)
			{
				//calculate the level, and spec back up to the level
			}
			 */
			int specPoints = (specLine.Level*(specLine.Level + 1) - 2)/2;
			specLine.Level = 1;
			if (!player.PlayerCharacter.UsedLevelCommand)
			{
				foreach (string lineKey in player.CharacterClass.AutoTrainableSkills())
				{
					if (lineKey == specLine.KeyName)
					{
						specLine.Level = player.Level/4;
						specPoints -= (specLine.Level*(specLine.Level + 1) - 2)/2;
						break;
					}
				}
			}

			return specPoints;
		}
	}
}