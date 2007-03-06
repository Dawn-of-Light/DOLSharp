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

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Heretic Trainer
	/// </summary>	
	[NPCGuildScript("Heretic Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Heretic Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class HereticTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Heretic; }
		}

		public const string WEAPON_ID1 = "chrush_sword_item";

		public HereticTrainer()
			: base()
		{
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			// check if class matches.				
			if (player.CharacterClass.ID == (int)eCharacterClass.Heretic)
			{

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			}
			else
			{
				// perhaps player can be promoted
				if (CanPromotePlayer(player))
				{
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Temple of Arawn] and defend our realm as a Heretic?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
				else
				{
					player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
				}
			}
			return true;
		}

		/// <summary>
		/// checks wether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level >= 5 && player.CharacterClass.ID == (int)eCharacterClass.Acolyte && (player.Race == (int)eRace.Briton || player.Race == (int)eRace.Avalonian
				|| player.Race == (int)eRace.Inconnu || player.Race == (int)eRace.AlbionMinotaur));
		}

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;
			GamePlayer player = source as GamePlayer;

			switch (text)
			{
				case "join the Temple of Arawn":
					// promote player to other class
					if (CanPromotePlayer(player))
					{
						PromotePlayer(player, (int)eCharacterClass.Heretic, "Welcome to the Temple of Arawn, " + player.Name + ".", null);
					}
					break;
			}
			return true;
		}

		public override bool AddToWorld()
		{
			if (ServerProperties.Properties.DISABLE_CATACOMBS_CLASSES)
				return false;
			return base.AddToWorld();
		}
	}
}
