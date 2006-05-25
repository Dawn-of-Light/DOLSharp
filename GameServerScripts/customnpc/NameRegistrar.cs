/* 01/03/2005
   Written by Gavinius */

using System.Collections;
using DOL.GS.PacketHandler;
using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Represents an in-game NameRegistrar NPC
	/// </summary>
	[Subclass(NameType=typeof(NameRegistrar), ExtendsType=typeof(GameMob))] 
	public class NameRegistrar : GameMob
	{
		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + ".");
			return list;
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if(!base.Interact(player)) return false;
			
			TurnTo(player, 10000);
			
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

	}
}

