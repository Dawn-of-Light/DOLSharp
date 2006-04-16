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

			if (args[1].ToLower() == "buy")
			{
				if (client.Player.RespecAmountAllSkill > 0 || client.Player.RespecAmountSingleSkill > 0)
				{
					client.Out.SendMessage("You already have a respec avalable!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				long cost = GamePlayer.CalculateRespecCost(client.Player.RespecBought + 1, client.Player.Level);
				if (client.Player.Money < cost)
				{
					// Message from live
					client.Out.SendMessage("You cannot afford the respec which costs "+ Money.GetString(cost) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				
				long nextPrice = GamePlayer.CalculateRespecCost(client.Player.RespecBought + 2, client.Player.Level);
				client.Out.SendDialogBox(eDialogCode.BuyRespec, 0x00, 0x00, 0x00, 0x00, eDialogType.YesNo, true, "Do you want to buy a respec single-line for " + Money.GetString(cost) + " ?\nThe next one will cost " + Money.GetString(nextPrice) + ".\n Respec lost on level up.");
				
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

				// Wipe skills and styles.
				IList specList = client.Player.GetSpecList();
				lock (specList.SyncRoot)
				{
					foreach (Specialization cspec in specList)
					{
						if (cspec.Level < 2)
							continue;
						specPoints += RespecSingleLine(client.Player, cspec);
					}
				}

				client.Player.RespecAmountAllSkill--; // Decriment players respecs available.
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
				
				client.Player.IsLevelRespecUsed = true;
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
			if (!player.UsedLevelCommand)
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