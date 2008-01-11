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
	/// Necromancer trainer.
	/// </summary>	
	/// <author>Aredhel</author>
	[NPCGuildScript("Necromancer Trainer", eRealm.Albion)]
	public class NecromancerTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Necromancer; }
		}

		public const string WEAPON_ID = "necromancer_item";

		public NecromancerTrainer() 
			: base() { }

		/// <summary>
		/// Interact with trainer.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;
								
			// If the player is a necromancer, offer training, if it is a disciple,
			// offer a promotion. Otherwise send them somewhere else.

			if (player.CharacterClass.ID == (int) eCharacterClass.Necromancer) 
				OfferTraining(player);
            else 
            {
				if (CanPromotePlayer(player)) 
                {
                    String message = "Hail, young Disciple. Have you come seeking to imbue yourself with the power of ";
                    message += "Arawn and serve as one of his [Necromancers]?";
                    SayTo(player, message);
				} 
                else 
                    DismissPlayer(player);					
			}
			return true;
 		}

		/// <summary>
		/// Checks wether a player can be promoted or not.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level >= 5 && player.CharacterClass.ID == (int) eCharacterClass.Disciple 
                && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen 
                || player.Race == (int) eRace.Inconnu));
		}

		/// <summary>
		/// Talk to the trainer.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{				
			if (!base.WhisperReceive(source, text)) return false;			
			GamePlayer player = source as GamePlayer;			
	
			switch (text.ToLower()) 
            {
                case "necromancers":
                    String message = "The necromancer is a cloth wearing priest of Arawn, lord of the underworld. ";
                    message += "Due to their allegiance to the dark master, they are granted spellcasting and combat ";
                    message += "prowess that is unique in Albion. Unlike other casters, a Necromancer is powerless ";
                    message += "until it calls upon the dark magic of Arawn to transform itself into a fearsome ";
                    message += "servant, controlled by an ethereal \"shade\". As a shade, the Necro appears as a ";
                    message += "translucent shadowy, floating ghost that cannot be wounded by melee or spells while ";
                    message += "the servant carries out their dark commands. When the servant is slain or released ";
                    message += "in combat, the Necromancer returns to mortal form, alive, but with a very small ";
                    message += "amount of health. Necromancers have the power to wound their enemies and the pain ";
                    message += "they inflict brings them a surge of power or restores life to their servant. Do you ";
                    message += "wish to dedicate yourself to Lord Arawn and [join the Temple of Arawn]?";
                    SayTo(player, message);
                    break;

			case "join the temple of arawn":
				if (CanPromotePlayer(player)) 
                {
					PromotePlayer(player, (int)eCharacterClass.Necromancer, 
                        "Lord Arawn has accepted you into his Temple. Here is his gift to you. Use it well, Disciple.", null);
					player.ReceiveItem(this, WEAPON_ID);
				}
				break;
			}
			return true;		
		}
	}
}
