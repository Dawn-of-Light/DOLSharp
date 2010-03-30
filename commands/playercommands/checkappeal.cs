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

using System.Collections.Generic;
using DOL.GS.Appeal;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&checkappeal",
		ePrivLevel.Player,
		"Checks the status of your appeal or cancels it.",
		"Usage:",
		"/checkappeal view - View your appeal status.",
		"/checkappeal cancel - Cancel your appeal and remove it from the queue.",
		"Use /appeal to file an appeal.")]

	public class CheckAppealCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (ServerProperties.Properties.DISABLE_APPEALSYSTEM)
			{
				AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.SystemDisabled"));
				return;
			}

			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1])
			{

					#region checkappeal cancel
				case "remove":
				case "delete":
				case "cancel":
					{
						if (args.Length < 2)
						{
							DisplaySyntax(client);
							return;
						}
						//if your temporary properties says your a liar, don't even check the DB (prevent DB hammer abuse)
						bool HasPendingAppeal = client.Player.TempProperties.getProperty<bool>("HasPendingAppeal");
						if (!HasPendingAppeal)
						{
							AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoNotHaveAppeal"));
							return;
						}
						DBAppeal appeal = AppealMgr.GetAppealByPlayerName(client.Player.Name);
						if (appeal != null)
						{
							if (appeal.Status == "Being Helped")
							{
								AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.CantCancelWhile"));
								return;
							}
							else
							{
								AppealMgr.CancelAppeal(client.Player, appeal);
								break;
							}
						}
						AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoNotHaveAppeal"));
						break;
					}
					#endregion checkappeal cancel
					#region checkappeal view
				case "display":
				case "show":
				case "list":
				case "view":
					{
						if (args.Length < 2)
						{
							DisplaySyntax(client);
							return;
						}
						bool HasPendingAppeal = client.Player.TempProperties.getProperty<bool>("HasPendingAppeal");
						if (!HasPendingAppeal)
						{
							AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NoAppealToView"));
							return;
						}
						DBAppeal appeal = AppealMgr.GetAppealByPlayerName(client.Player.Name);
						if (appeal != null)
						{
							//Let's view it.
							List<string> msg = new List<string>();
							//note: we do not show the player his Appeals priority.
							msg.Add("[Player]: " + appeal.Name + ", [Status]: " + appeal.Status + ", [Issue]: " + appeal.Text + ", [Time]: " + appeal.Timestamp + ".\n");
							AppealMgr.GetAllAppeals(); //refresh the total number of appeals.
							msg.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.CurrentStaffAvailable", AppealMgr.StaffList.Count, AppealMgr.TotalAppeals) + "\n");
							msg.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.PleaseBePatient") + "\n");
							msg.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.IfYouLogOut") + "\n");
							msg.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.ToCancelYourAppeal"));
							client.Out.SendCustomTextWindow("Your Appeal", msg);
							return;
						}
						AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NoAppealToView"));
						break;

					}
				default:
					{
						DisplaySyntax(client);
						return;
					}
					#endregion checkappeal view

			}
		}
	}
}