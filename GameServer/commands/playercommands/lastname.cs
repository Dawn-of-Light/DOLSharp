/* 01/03/2005
   Written by Gavinius */

using System;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;


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
				client.Out.SendMessage("You must be " + LASTNAME_MIN_LEVEL + "th level or " + LASTNAME_MIN_CRAFTSKILL + " in your primary trade skill to register a last name!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* When you don't have a lastname, change is for free, otherwise you need money */
			if (client.Player.LastName != "" && client.Player.GetCurrentMoney() < Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))
			{
				client.Out.SendMessage("Changing your last name costs " + Money.GetString(Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0)) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check if you selected a Name Registrar NPC */
			if (client.Player.TargetObject is NameRegistrar == false)
			{
				client.Out.SendMessage("You must select a name registrar to set your last name with!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Chek if you are near enough to NPC*/
			if ( !client.Player.IsWithinRadius( client.Player.TargetObject, WorldMgr.INTERACT_DISTANCE ) )
			{
				client.Out.SendMessage("You are too far away to interact with " + client.Player.TargetObject.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* If you type /lastname with no other arguments, clear your actual lastname */
			if (args.Length < 2)
			{
				client.Player.TempProperties.setProperty(LASTNAME_WEAK, "");
				client.Out.SendCustomDialog("Would you like to clear your last name?", new CustomDialogResponse(LastNameDialogResponse));
				return;
			}

			/* Get the name */
			string NewLastname = args[1];
			/* Check to ensure that lastnames do not exeed maximum length */
			if (NewLastname.Length > LASTNAME_MAXLENGTH)
			{
				client.Out.SendMessage("Last names can be no longer than " + LASTNAME_MAXLENGTH + " characters!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* First char of lastname must be uppercase */
			//if (!Char.IsUpper(NewLastname, 0)) /* IsUpper() use unicode characters, it doesn't catch all accented uppercase letters like �, �, �, ecc.. that are invalid! */
			if (NewLastname[0] < 'A' || NewLastname[0] > 'Z')
			{
				client.Out.SendMessage("Your lastname must start with a valid, uppercase character!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Only permits letters, with no spaces or symbols */
			if (args.Length > 2 || LastnameIsInvalid(NewLastname))
			{
				client.Out.SendMessage("Your lastname must consist of valid characters!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check if lastname is legal and is not contained in invalidnames.txt */
			foreach (string invalid in GameServer.Instance.InvalidNames)
			{
				if (NewLastname.ToLower().IndexOf(invalid) != -1)
				{
					client.Out.SendMessage(NewLastname + " is not a legal last name! Choose another.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			client.Player.TempProperties.setProperty(LASTNAME_WEAK, NewLastname);
			client.Out.SendCustomDialog("Would you like to set your last name to \x000a" + NewLastname + "?", new CustomDialogResponse(LastNameDialogResponse));

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
			player.TempProperties.removeProperty(LASTNAME_WEAK);

			if (!(player.TargetObject is NameRegistrar))
			{
				player.Out.SendMessage("You must select a name registrar to set your last name with!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if ( !player.IsWithinRadius( player.TargetObject, WorldMgr.INTERACT_DISTANCE ) )
			{
				player.Out.SendMessage("You are too far away to interact with " + player.TargetObject.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Check money only if your lastname is not blank */
			if (player.LastName != "" && player.GetCurrentMoney() < Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0))
			{
				player.Out.SendMessage("Changing your last name costs " + Money.GetString(Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0)) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
			{
				player.Out.SendMessage("You decline to " + (NewLastName != "" ? ("take " + NewLastName + " as") : "clear") + " your last name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			/* Remove money only if your lastname is not blank and is different from the previous one */
            if (player.LastName != "" && player.LastName != NewLastName)
            {
                player.RemoveMoney(Money.GetMoney(0, 0, LASTNAME_FEE, 0, 0), null);
                InventoryLogging.LogInventoryAction(player, player.TargetObject, eInventoryActionType.Merchant, LASTNAME_FEE * 10000);
            }

		    /* Set the new lastname */
			player.LastName = NewLastName;
			player.Out.SendMessage("Your last name has been " + (NewLastName != "" ? ("set to " + NewLastName) : "cleared") + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}