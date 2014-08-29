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
	/// Description of NecromancerAbominationPet.
	/// </summary>
	public class NecromancerAbominationPet : NecromancerPet
	{
		public NecromancerAbominationPet(INpcTemplate npcTemplate, int summonConBonus, int summonHitsBonus)
			: base(npcTemplate, summonConBonus, summonHitsBonus)
		{
			InventoryItem item;
			
			if (Inventory != null && (item = Inventory.GetItem(eInventorySlot.RightHandWeapon)) != null)
			{
				item.ProcSpellID = (int)Procs.Heat;
				item.ProcChance = 15;
			}
		}
		
		/// <summary>
		/// Base quickness. Higher Quickness for high 2H DPS
		/// </summary>
		public override short Quickness
		{
			get
			{
				return (short)(60 + Level);
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
			
			bool commandOk = false;
			
			foreach (string keyW in text.ContainsKey("arawn", "empower", "weapons", "fiery sword", "icy sword", "flaming mace", "frozen mace", "venomous mace"))
			{
				string keyWord = keyW;
				// switch for each keyword
				switch (keyWord)
				{	
					case "empower":
					{
						commandOk = true;
						Empower();
						break;
					}
					case "weapons":
					{
						if(source is GamePlayer)
							SayTo((GamePlayer)source, "What weapon do you command me to wield? A [fiery sword], [icy sword], [poisonous sword] or a [flaming mace], [frozen mace], [venomous mace]?");
						break;
					}
					case "fiery sword":
					case "icy sword":
					case "poisonous sword":
					case "flaming mace":
					case "frozen mace":
					case "venomous mace":
					{
						String templateID = String.Format("{0}_{1}", Name.ToLower(), keyWord.Replace(" ", "_"));
						
						if (LoadEquipmentTemplate(templateID))
							commandOk = true;
						break;
					}
				}
			}
			
			if (commandOk)
			{
				if (source is GamePlayer)
					ReplyCommandOK((GamePlayer)source);
			}
			
			return true;
		}
		
		protected override bool LoadEquipmentTemplate(String templateID)
		{
			if(base.LoadEquipmentTemplate(templateID))
			{
				InventoryItem item;
				if ((item = Inventory.GetItem(eInventorySlot.TwoHandWeapon)) != null)
				{
					switch (templateID.ToLower())
					{
						case "abomination_fiery_sword":
							MeleeDamageType = eDamageType.Slash;
							item.ProcSpellID = (int)Procs.Heat;
							break;
						case "abomination_flaming_mace":
							MeleeDamageType = eDamageType.Crush;
							item.ProcSpellID = (int)Procs.Heat;
							break;
						case "abomination_icy_sword":
							MeleeDamageType = eDamageType.Slash;
							item.ProcSpellID = (int)Procs.Cold;
							break;
						case "abomination_frozen_mace":
							MeleeDamageType = eDamageType.Crush;
							item.ProcSpellID = (int)Procs.Cold;
							break;
						case "abomination_poisonous_sword":
							MeleeDamageType = eDamageType.Slash;
							item.ProcSpellID = (int)Procs.Poison;
							break;
						case "abomination_venomous_mace":
							MeleeDamageType = eDamageType.Crush;
							item.ProcSpellID = (int)Procs.Poison;
							break;
					}
					item.ProcChance = 15;
				}
				
				return true;
			}
			
			return false;
		}
		
		protected override string ReceiveArawn()
		{
			return base.ReceiveArawn()+"\n\nI have a mighty arsenal of [weapons] at your disposal."
				+"\n\nYou may also [empower] me with just a word.";
		}
		
	}
}
