/* 01/03/2005
   Written by Gavinius */

using System;
using DOL;
using DOL.GS;
using DOL.GS.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[NPCGuildScript("Name Registrar")]
	public class NameRegistrar : GameMob
	{
		private const string TOWARDSTR = " towards you.";

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(2);
			string AggroString = GetAggroLevelString(player, false);

			/* Adjust aggro string */
			if (AggroString.EndsWith(TOWARDSTR))
			{
					AggroString = AggroString.Remove(AggroString.Length - TOWARDSTR.Length, TOWARDSTR.Length);
			}

			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + AggroString + ".");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if(base.Interact(player))
			{
				/* Get primary crafting skill (if any) */
				int CraftSkill = 0;
				if (player.CraftingPrimarySkill != eCraftingSkill.NoCrafting)
					CraftSkill = player.GetCraftingSkillValue(player.CraftingPrimarySkill);

				/* Check if level and/or crafting skill let you have a lastname */
				if (player.Level < LastnameCommandHandler.LASTNAME_MIN_LEVEL && CraftSkill < LastnameCommandHandler.LASTNAME_MIN_CRAFTSKILL)
					SayTo(player, eChatLoc.CL_SystemWindow, "Return to me when you are " + LastnameCommandHandler.LASTNAME_MIN_LEVEL + "th level to choose a last name.");
				else
					SayTo(player, eChatLoc.CL_SystemWindow, "I can make you known by a last name of your choice. Use the /lastname <name> command.");
				return true;
			}
			return false;
		}

	}
}

