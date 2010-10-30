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
 *		by typing /item create respec_full | respec_single | respec_realm.
 * Respec stones may be turned in to trainers for respecs.
 * 
 * TODO: include autotrains in the formula
 * TODO: realm respec
 * 
 * Suncheck:
 * 	Added: /respec buy
 */


using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&respec",
		ePrivLevel.Player,
		"Respecs the char",
		"/respec")]
	public class RespecCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		const string RA_RESPEC = "realm_respec";
		const string ALL_RESPEC = "all_respec";
		const string LINE_RESPEC = "line_respec";
		const string DOL_RESPEC = "dol_respec";
		const string BUY_RESPEC = "buy_respec";
		
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				// Check for respecs.
				if (client.Player.RespecAmountAllSkill < 1
					&& client.Player.RespecAmountSingleSkill < 1
					&& client.Player.RespecAmountDOL <1
					&& client.Player.RespecAmountRealmSkill < 1)
				{
					DisplayMessage(client, "You don't seem to have any respecs available.");
					DisplayMessage(client, "Use /respec buy to buy an single-line respec.");
					return;
				}

				if (client.Player.RespecAmountAllSkill > 0)
				{
					DisplayMessage(client, "You have " + client.Player.RespecAmountAllSkill + " full skill respecs available.");
					DisplayMessage(client, "Target any trainer and use /respec ALL");
				}
				if (client.Player.RespecAmountSingleSkill > 0)
				{
					DisplayMessage(client, "You have " + client.Player.RespecAmountSingleSkill + " single-line respecs available.");
					DisplayMessage(client, "Target any trainer and use /respec <line name>");
				}
				if (client.Player.RespecAmountRealmSkill > 0)
				{
					DisplayMessage(client, "You have " + client.Player.RespecAmountRealmSkill + " realm skill respecs available.");
					DisplayMessage(client, "Target any trainer and use /respec REALM");
				}
				if (client.Player.RespecAmountDOL > 0)
				{
					DisplayMessage(client, "You have " + client.Player.RespecAmountDOL + " DOL ( full skill ) respecs available.");
					DisplayMessage(client, "Target any trainer and use /respec DOL");
				}
				DisplayMessage(client, "Use /respec buy to buy an single-line respec.");
				return;
			}			

			// Player must be speaking with trainer to respec.  (Thus have trainer targeted.) Prevents losing points out in the wild.
			if (client.Player.TargetObject is GameTrainer == false && args[1].ToLower() != "buy")
			{
				DisplayMessage(client, "You must be speaking with your trainer to respec.");
				return;
			}

			switch (args[1].ToLower())
			{
				case "buy":
					{
						// Buy respec
						if (client.Player.CanBuyRespec == false || client.Player.RespecCost < 0)
						{
							DisplayMessage(client, "You can't buy a respec on this level again.");
							return;
						}

						long mgold = client.Player.RespecCost;
						if ((client.Player.DBCharacter.Gold + 1000 * client.Player.DBCharacter.Platinum) < mgold)
						{
							DisplayMessage(client, "You don't have enough money! You need " + mgold + " gold!");
							return;
						}
						client.Out.SendCustomDialog("It costs " + mgold + " gold. Want you really buy?", new CustomDialogResponse(RespecDialogResponse));
						client.Player.TempProperties.setProperty(BUY_RESPEC, true);
						break;
					}
				case "all":
					{
						// Check for full respecs.
						if (client.Player.RespecAmountAllSkill < 1)
						{
							DisplayMessage(client, "You don't seem to have any full skill respecs available.");
							return;
						}

						client.Out.SendCustomDialog("CAUTION: All respec changes are final with no second chance. Proceed carefully!", new CustomDialogResponse(RespecDialogResponse));
						client.Player.TempProperties.setProperty(ALL_RESPEC, true);
						break;
					}
				case "dol":
					{
						// Check for DOL respecs.
						if (client.Player.RespecAmountDOL < 1)
						{
							DisplayMessage(client, "You don't seem to have any DOL respecs available.");
							return;
						}

						client.Out.SendCustomDialog("CAUTION: All respec changes are final with no second chance. Proceed carefully!", new CustomDialogResponse(RespecDialogResponse));
						client.Player.TempProperties.setProperty(DOL_RESPEC, true);
						break;
					}
				case "realm":
					{
						if (client.Player.RespecAmountRealmSkill < 1)
						{
							DisplayMessage(client, "You don't seem to have any realm skill respecs available.");
							return;
						}

						client.Out.SendCustomDialog("CAUTION: All respec changes are final with no second chance. Proceed carefully!", new CustomDialogResponse(RespecDialogResponse));
						client.Player.TempProperties.setProperty(RA_RESPEC, true);
						break;
					}
				default:
					{
						// Check for single-line respecs.
						if (client.Player.RespecAmountSingleSkill < 1)
						{
							DisplayMessage(client, "You don't seem to have any single-line respecs available.");
							return;
						}

						string lineName = string.Join(" ", args, 1, args.Length - 1);
						Specialization specLine = client.Player.GetSpecializationByName(lineName, false);
						if (specLine == null)
						{
							DisplayMessage(client, "No line with name '" + lineName + "' found.");
							return;
						}
						if (specLine.Level < 2)
						{
							DisplayMessage(client, "Level of " + specLine.Name + " line is less than 2. ");
							return;
						}

						client.Out.SendCustomDialog("CAUTION: All respec changes are final with no second chance. Proceed carefully!", new CustomDialogResponse(RespecDialogResponse));
						client.Player.TempProperties.setProperty(LINE_RESPEC, specLine);
						break;
					}
			}
		}
		

		protected void RespecDialogResponse(GamePlayer player, byte response)
		{

			if (response != 0x01) return; //declined

			int specPoints = 0;
			int realmSpecPoints = 0;

			if (player.TempProperties.getProperty(ALL_RESPEC, false))
			{
				specPoints = player.RespecAll();
				player.TempProperties.removeProperty(ALL_RESPEC);
			}
			if (player.TempProperties.getProperty(DOL_RESPEC, false))
			{
				specPoints = player.RespecDOL();
				player.TempProperties.removeProperty(DOL_RESPEC);
			}
			if (player.TempProperties.getProperty(RA_RESPEC, false))
			{
				realmSpecPoints = player.RespecRealm();
				player.TempProperties.removeProperty(RA_RESPEC);
			}
			if (player.TempProperties.getProperty<object>(LINE_RESPEC, null) != null)
			{
				Specialization specLine = (Specialization)player.TempProperties.getProperty<object>(LINE_RESPEC, null);
				specPoints = player.RespecSingle(specLine);
				player.TempProperties.removeProperty(LINE_RESPEC);
			}
			if (player.TempProperties.getProperty(BUY_RESPEC, false))
			{
				player.TempProperties.removeProperty(BUY_RESPEC);
				if (player.RespecCost >= 0 && player.RemoveMoney(player.RespecCost * 10000))
				{
					player.RespecAmountSingleSkill++;
					player.RespecBought++;
					DisplayMessage(player, "You bought a single line respec!");
				}
				player.Out.SendUpdateMoney();
			}			
			// Assign full points returned
			if (specPoints > 0)
			{
				player.SkillSpecialtyPoints += specPoints;
				lock (player.GetStyleList().SyncRoot)
				{
					player.GetStyleList().Clear(); // Kill styles
				}
				player.UpdateSpellLineLevels(false);
				DisplayMessage(player, "You regain " + specPoints + " specialization points!");
			}
			if (realmSpecPoints > 0)
			{
				player.RealmSpecialtyPoints += realmSpecPoints;
				DisplayMessage(player, "You regain " + realmSpecPoints + " realm specialization points!");
			}
			player.RefreshSpecDependantSkills(false);
			// Notify Player of points
			player.Out.SendUpdatePlayerSkills();
			player.Out.SendUpdatePoints();
			player.Out.SendUpdatePlayer();
			player.Out.SendTrainerWindow();
			player.SaveIntoDatabase();
		}
	}
}
