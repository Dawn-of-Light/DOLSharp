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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Database2;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Styles;

namespace DOL.GS
{
	public class CommanderPet : BDPet
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public CommanderPet(INpcTemplate npcTemplate, GamePlayer owner)
			: base(npcTemplate)
		{
			switch (Name.ToLower())
			{
				case "returned commander":
				case "decayed commander":
					InitControlledNpc(0);
					break;
				case "skeletal commander":
					InitControlledNpc(1);
					break;
				case "bone commander":
					InitControlledNpc(2);
					break;
				case "dread commander":
				case "dread guardian":
				case "dread lich":
				case "dread archer":
					InitControlledNpc(3);
					break;
			}

			SetOwnBrain(new CommanderBrain(owner));
		}

		/// <summary>
		/// Address the master.
		/// </summary>
		public void HailMaster()
		{
			GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
			if (owner != null)
			{
				String message = String.Format("Hail, {0}. As my summoner, you may target me and say Commander to learn more about the abilities I possess.",
					(owner.PlayerCharacter.Gender == 0) ? "Master" : "Mistress");
				SayTo(owner, eChatLoc.CL_SystemWindow, message);
			}
		}

		/// <summary>
		/// Called when owner sends a whisper to the pet
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		public override bool WhisperReceive(GameLiving source, string str)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			string[] strargs = str.ToLower().Split(' ');

			for (int i = 0; i < strargs.Length; i++)
			{
				String curStr = strargs[i];
				string text = null;
				switch (curStr)
				{
					case "commander":
						switch (Name)
						{
							case "dread guardian":
								text = "The dread guardian says, \"As one of Bogdar's chosen guardians, I have mastered the ability to [harm] his enemies and [empower] myself with extra defenses.  I also have various [combat] tactics at your disposal.\"";
								break;
							case "dread lich":
								text = "The dread lich says, \"As one of Bogdar's mystics, I have mastered many dark [spells].  I also have the means to [empower] my spells to be more effective.  I also have various [combat] tactics at your disposal.\"";
								break;
							case "dread archer":
								text = "The dread archer says, \"As one of Bogdar's favored archers, you can [empower] me to be more effective with my bow.  I also have various [combat] tactics at your disposal.\"";
								break;
							case "dread commander":
							case "decayed commander":
							case "returned commander":
							case "skeletal commander":
							case "bone commander":
								text = "The " + Name + " says, \"As one of Bogdar's commanders, I have mastered many of the fossil [weapons] within his bone army.  Which weapon shall I wield for you?  I also have various [combat] tactics at your disposal.\"";
								break;
						}
						break;
					case "spells":
						if (Name != "dread lich")
							return false;
						text = "The dread lich says, \"My arsenal consists of [snare], [debilitating], and pure [damage] spells.  I cast debilitating spells when first summoned but you can command me to cast a particular type of spells if you so desire.\"";
						break;
					case "empower":
						if (Name == "dread guardian" || Name == "dread lich" || Name == "dread archer")
						{
							foreach (Spell spell in Spells)
							{
								if (spell.Name == "Empower")
								{
									CastSpell(spell, null);
									break;
								}
							}
						}
						break;
					case "combat":
						text = "The " + Name + " says, \"From the start I order the minions under my control to [assist] me with a target your choose for me to kill.  Issuing the command again will make them cease assisting.  I am also able to [taunt] your enemies so that they will focus on me instead of you.\"";
						break;
					case "snares":
						break;
					case "debilitating":
						break;
					case "damage":
						break;
					case "assist":
						//TODO: implement this - I have no idea how to do that...
						break;
					case "taunt":
						bool found = false;
						foreach (Spell spell in Spells)
						{
							//If the taunt spell's ID is changed - this needs to be changed
							if (spell.ID == 60127)
							{
								Spells.Remove(spell);
								player.Out.SendMessage("Your commander will no long taunt its enemies!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
								found = true;
								break;
							}
						}
						if (found) break;
						//TODO: change this so it isn't hardcoded
						Spell tauntspell = SkillBase.GetSpellByID(60127);
						player.Out.SendMessage("Your commander will start to taunt its enemies!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
						if (tauntspell != null)
							Spells.Add(tauntspell);
						else
							Console.WriteLine("Couldn't find BD pet's taunt spell");
						break;
					case "weapons":
						if (Name != "dread commander" && Name != "decayed commander" && Name != "returned commander" && Name != "skeletal commander" && Name != "bone commander") break;
						text = "The " + Name + " says, \"I have mastered the [one handed sword], the [two handed sword], the [one handed hammer], the [two handed hammer], the [one handed axe] and the [two handed axe].\"";
						break;
					case "one":
						i++;
						if (i + 1 >= strargs.Length)
							return false;
						CommanderSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, strargs[++i]);
						break;
					case "two":
						i++;
						if (i + 1 >= strargs.Length)
							return false;
						CommanderSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, strargs[++i]);
						break;
					case "harm":
						if (Name != "dread guardian")
							return false;
						text = "The dread guardian says, \"Bogdar has granted me with the ability to [drain] the life of our enemies or to [suppress] and hurt their spirit.  I cast spirit damaging spells when first summoned but you can command me to cast a particular type of spell if you so desire.\"";
						break;
					case "drain":
						break;
					case "suppress":
						break;
					default:
						return false;
				}
				if (text == null)
					return false;
				player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			return base.WhisperReceive(source, str);
		}

		/// <summary>
		/// Changes the commander's weapon to the specified type
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="aSlot"></param>
		/// <param name="weaponType"></param>
		protected void CommanderSwitchWeapon(eInventorySlot slot, eActiveWeaponSlot aSlot, string weaponType)
		{
			if (Inventory == null)
				return;
			
			string itemId = string.Format("BD_Commander_{0}_{1}", slot.ToString(), weaponType);
			InventoryItem item = Inventory.GetItem(slot);

			if (item != null)
				Inventory.RemoveItem(item);

			ItemTemplate temp = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), itemId);

			if (temp == null)
			{
				if (log.IsErrorEnabled)
					log.Error(string.Format("Unable to find Bonedancer item: {0}", itemId));
				return;
			}

			Inventory.AddItem(slot, new InventoryItem(temp));
			SwitchWeapon(aSlot);
			AddStatsToWeapon();
			UpdateNPCEquipmentAppearance();
		}

		/// <summary>
		/// Adds a pet to the current array of pets
		/// </summary>
		/// <param name="controlledNpc">The brain to add to the list</param>
		/// <returns>Whether the pet was added or not</returns>
		public override bool AddControlledNpc(IControlledBrain controlledNpc)
		{
			IControlledBrain[] brainlist = ControlledNpcList;
			if (brainlist == null) return false;
			foreach (IControlledBrain icb in brainlist)
			{
				if (icb == controlledNpc)
					return false;
			}

			if (controlledNpc.Owner != this)
				throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");

			//Find the next spot for this new pet
			int i = 0;
			for (; i < brainlist.Length; i++)
			{
				if (brainlist[i] == null)
					break;
			}
			//If we didn't find a spot return false
			if (i >= m_controlledNpc.Length)
				return false;
			m_controlledNpc[i] = controlledNpc;
			PetCounter++;
			return base.AddControlledNpc(controlledNpc);
		}

		/// <summary>
		/// Removes the brain from 
		/// </summary>
		/// <param name="controlledNpc">The brain to find and remove</param>
		/// <returns>Whether the pet was removed</returns>
		public override bool RemoveControlledNpc(IControlledBrain controlledNpc)
		{
			bool found = false;
			lock (ControlledNpcList)
			{
				if (controlledNpc == null) return false;
				IControlledBrain[] brainlist = ControlledNpcList;
				int i = 0;
				//Try to find the minion in the list
				for (; i < brainlist.Length; i++)
				{
					//Found it
					if (brainlist[i] == controlledNpc)
					{
						found = true;
						break;
					}
				}

				//Found it, lets remove it
				if (found)
				{
					//First lets store the brain to kill it
					IControlledBrain tempBrain = m_controlledNpc[i];
					//Lets get rid of the brain asap
					m_controlledNpc[i] = null;

					//Only decrement, we just lost one pet
					PetCounter--;

					return base.RemoveControlledNpc(controlledNpc);
				}
			}

			return found;
		}
	}
}