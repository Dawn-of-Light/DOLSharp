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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Represents an in-game FaceCustomiser NPC
	/// </summary>
	public class FaceCustomiser : GameMob
	{
		/// <summary>
		/// Spell id of the magical effect
		/// </summary>
		public const int EFFECT_ID = 5924;

		/// <summary>
		/// The cast time delay in milliseconds
		/// </summary>
		public const int CAST_TIME = 2000;

		/// <summary>
		/// Constructor
		/// </summary>
		public FaceCustomiser () : base()
		{
		}

		/// <summary>
		/// Called when a player right clicks on the npc
		/// </summary>
		/// <param name="player">Player that interacting</param>
		/// <returns>True if succeeded</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 5000);

			if(player.PlayerCharacter.CustomisationStep == 2)
			{
				SayTo(player, eChatLoc.CL_PopupWindow, player.CharacterClass.Name +", I have discovered a secret spell that will allow you to change your appearance. I can cast this spell upon you if you wish. All you must do is say the word and I will [change your appearance].");
			}
			else if(player.PlayerCharacter.CustomisationStep == 3)
			{
				SayTo(player, eChatLoc.CL_PopupWindow, "You have already been granted the ability to change your appearance. You must leave this world to make the changes. (Log out to change your appearance.)");
			}

			return true;
		}

		/// <summary>
		/// This function is called when the Living receives a whispered text
		/// </summary>
		/// <param name="source">GameLiving that was whispering</param>
		/// <param name="str">string that was whispered</param>
		/// <returns>true if the string correctly processed</returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			
			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			if (player.PlayerCharacter.CustomisationStep == 2 && text == "change your appearance")
			{
				foreach(GamePlayer players in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE)) 
				{
					players.Out.SendSpellCastAnimation(this,EFFECT_ID,CAST_TIME);
				}
				new RegionTimer(player, new RegionTimerCallback(EndCastCallback), CAST_TIME);
			
				SayTo(player, eChatLoc.CL_PopupWindow, "There it is done! Now, you must leave this world for a short time for the magic to work. (You must log out to change your appearance.)");
				player.PlayerCharacter.CustomisationStep = 3;
			}
			return true;
		}

		/// <summary>
		/// This function is called to show the spell animation to players
		/// </summary>
		/// <param name="callingTimer"></param>
		/// <returns>new delay in milliseconds</returns>
		protected virtual int EndCastCallback(RegionTimer callingTimer)
		{
			foreach(GamePlayer players in this.GetPlayersInRadius( WorldMgr.VISIBILITY_DISTANCE)) 
			{
				players.Out.SendSpellEffectAnimation(this,this,EFFECT_ID,0,false,0x01);
			}
			return 0;
		}
	}
}