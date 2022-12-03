/* 01/03/2005
   Written by Gavinius */

using System;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Finance;
using DOL.Language;
using System.Numerics;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&lastname",
		 ePrivLevel.Player,
		 "Set/change your lastname.", "/lastname <name>")]

	public class LastnameCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const string LASTNAME_WEAK = "new lastname";
		/* lastname cost: 10 gold pieces*/
		private const int LASTNAME_FEE = 10;

		/* Max chars in lastname */
		private const int LASTNAME_MAXLENGTH = 23;

		/* Min levels required to have a lastname: 10th level or 200 in a crafting skill */
		public const int LASTNAME_MIN_LEVEL = 10;
		public const int LASTNAME_MIN_CRAFTSKILL = 200;

		public void OnCommand(GameClient client, string[] args)
		{
			/* Get primary crafting skill (if any) */
			int CraftSkill = 0;
			if (client.Player.CraftingPrimarySkill != eCraftingSkill.NoCrafting)
				CraftSkill = client.Player.GetCraftingSkillValue(client.Player.CraftingPrimarySkill);

			/* Check if level and/or crafting skill let you have a lastname */
			if (client.Player.Level < LASTNAME_MIN_LEVEL && CraftSkill < LASTNAME_MIN_CRAFTSKILL)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.Level", LASTNAME_MIN_LEVEL, LASTNAME_MIN_CRAFTSKILL), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* When you don't have a lastname, change is for free, otherwise you need money */
			if (client.Player.LastName != "" && client.Player.CopperBalance < Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.Costs", Money.GetString(Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check if you selected a Name Registrar NPC */
			if (client.Player.TargetObject is NameRegistrar == false)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.NoRegistrar"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Chek if you are near enough to NPC*/
			if ( !client.Player.IsWithinRadius( client.Player.TargetObject, WorldMgr.INTERACT_DISTANCE ) )
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.TooFar", client.Player.TargetObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* If you type /lastname with no other arguments, clear your actual lastname */
			if (args.Length < 2)
			{
				client.Player.TempProperties.setProperty(LASTNAME_WEAK, "");
				client.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.Clear"), new CustomDialogResponse(LastNameDialogResponse));
				return;
			}

			/* Get the name */
			string NewLastname = args[1];
			/* Check to ensure that lastnames do not exeed maximum length */
			if (NewLastname.Length > LASTNAME_MAXLENGTH)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.TooLong", LASTNAME_MAXLENGTH), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* First char of lastname must be uppercase */
			//if (!Char.IsUpper(NewLastname, 0)) /* IsUpper() use unicode characters, it doesn't catch all accented uppercase letters like �, �, �, ecc.. that are invalid! */
			if (NewLastname[0] < 'A' || NewLastname[0] > 'Z')
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.InvalidUcase"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Only permits letters, with no spaces or symbols */
			if (args.Length > 2 || LastnameIsInvalid(NewLastname))
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.Invalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check if lastname is legal and is not contained in invalidnames.txt */
			if (GameServer.Instance.PlayerManager.InvalidNames[NewLastname])
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.NotLegal", NewLastname), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			client.Player.TempProperties.setProperty(LASTNAME_WEAK, NewLastname);
			client.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Players.Lastname.Set", "\n", NewLastname), new CustomDialogResponse(LastNameDialogResponse));

			return;
		}

		/* Validate lastnames: they must contain only letters (either lowercase or uppercase) */
		protected bool LastnameIsInvalid(string name)
		{
			foreach (Char c in name)
			{
				//if (!Char.IsLetter(c)) /* IsLetter() use unicode characters, it doesn't catch all accented letters like �, �, �, �, ecc.. that are invalid! */
				if (c < 'A' || (c > 'Z' && c < 'a') || c > 'z')
					return true;
			}
			return false;
		}

		protected void LastNameDialogResponse(GamePlayer player, byte response)
		{
			string NewLastName =
				(string)player.TempProperties.getProperty<object>(
					LASTNAME_WEAK,
					String.Empty
				);
            string message = string.Empty;
            player.TempProperties.removeProperty(LASTNAME_WEAK);

			if (!(player.TargetObject is NameRegistrar))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.NoRegistrar"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if ( !player.IsWithinRadius( player.TargetObject, WorldMgr.INTERACT_DISTANCE ) )
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.TooFar", player.TargetObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check money only if your lastname is not blank */
			if (player.LastName != "" && player.CopperBalance < Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.Costs", Money.GetString(Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
			{
				if (string.IsNullOrEmpty(NewLastName))
				{
					message = LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.DeclineClear");
                }
				else
				{
                    message = LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.DeclineName", NewLastName);
                }
				player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Remove money only if your lastname is not blank and is different from the previous one */
            if (player.LastName != "" && player.LastName != NewLastName)
            {
                player.RemoveMoney(Currency.Copper.Mint(LASTNAME_FEE * 100 * 100));
                InventoryLogging.LogInventoryAction(player, player.TargetObject, eInventoryActionType.Merchant, LASTNAME_FEE * 10000);
            }

		    /* Set the new lastname */
			player.LastName = NewLastName;
            if (string.IsNullOrEmpty(NewLastName))
            {
                message = LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.OkClear");
            }
            else
            {
                message = LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Lastname.OkName", NewLastName);
            }
            player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}