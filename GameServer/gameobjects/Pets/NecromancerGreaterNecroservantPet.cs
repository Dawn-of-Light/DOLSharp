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

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Description of NecromancerGreaterNecroservantPet.
	/// </summary>
	public class NecromancerGreaterNecroservantPet : NecromancerPet
	{
		public NecromancerGreaterNecroservantPet(INpcTemplate npcTemplate, int summonConBonus, int summonHitsBonus)
			: base(npcTemplate, summonConBonus, summonHitsBonus)
		{
			InventoryItem item;
			
			if (Inventory != null && (item = Inventory.GetItem(eInventorySlot.RightHandWeapon)) != null)
			{
				item.ProcSpellID = (int)Procs.Poison;
				item.ProcChance = 10;
			}
		}
		
		/// <summary>
		/// Base dexterity. Make greater necroservant slightly more dextrous than
		/// all the other pets.
		/// </summary>
		public override short Dexterity
		{
			get
			{
				return (short)(60 + Level / 3);
			}
		}
		
		/// Actions to be taken when the pet receives a whisper.
		/// </summary>
		/// <param name="source">Source of the whisper.</param>
		/// <param name="text">"Text that was whispered</param>
		/// <returns>True if whisper was handled, false otherwise.</returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if(!base.WhisperReceive(source, text))
				return false;
			
			InventoryItem item = Inventory.GetItem(eInventorySlot.RightHandWeapon);
			
			switch (text.ToLower())
			{
				case "disease":
					if (Inventory != null && item != null)
					{
						item.ProcSpellID = (int)Procs.Disease;
						if(source is GamePlayer)
							ReplyCommandOK((GamePlayer)source);
					}
					return true;
				
				case "poison":
					if (Inventory != null && item != null)
					{
						item.ProcSpellID = (int)Procs.Poison;
						if(source is GamePlayer)
							ReplyCommandOK((GamePlayer)source);
					}
					return true;
				
				case "empower":
					if(source is GamePlayer)
						ReplyCommandOK((GamePlayer)source);
					Empower();
					return true;
			}
						
			return true;
		}
		
		protected override string ReceiveArawn()
		{
			return base.ReceiveArawn()+"\nI can also inflict [poison] or [disease] on your enemies."
				+"\nYou may also [empower] me with just a word.";
		}
		
	}
}
