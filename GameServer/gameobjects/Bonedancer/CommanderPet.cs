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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	public class CommanderPet : BDPet
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public CommanderPet(INpcTemplate npcTemplate)
			: base(npcTemplate)
		{
			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander"))
			{
				InitControlledBrainArray(0);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander"))
			{
				InitControlledBrainArray(1);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
			{
				InitControlledBrainArray(2);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
			{
				InitControlledBrainArray(3);
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
			if (player == null || player != (Brain as IControlledBrain).Owner)
				return false;

			string[] strargs = str.ToLower().Split(' ');

			for (int i = 0; i < strargs.Length; i++)
			{
				String curStr = strargs[i];

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Commander"))
				{
					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DreadGuardian", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DreadLich", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DreadArcher", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.XCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Combat"))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Combat", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Assist"))
				{
					//TODO: implement this - I have no idea how to do that...
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Assist.Text"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Taunt"))
				{
					bool found = false;
					foreach (Spell spell in Spells)
					{
						//If the taunt spell's ID is changed - this needs to be changed
						if (spell.ID == 60127)
						{
							Spells.Remove(spell);
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.CommNoTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							found = true;
							break;
						}
					}
					if (found) break;
					//TODO: change this so it isn't hardcoded
					Spell tauntspell = SkillBase.GetSpellByID(60127);
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.CommStartTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					if (tauntspell != null)
						Spells.Add(tauntspell);
					else
						Console.WriteLine("Couldn't find BD pet's taunt spell");
					break;
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Weapons"))
				{
					if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") &&
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") &&
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander") &&
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander") &&
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
					{
						break;
					}
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DiffCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Spells"))
				{
					if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
					{
						return false;
					}
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DreadLich2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Empower"))
				{
					if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
					{
						foreach (Spell spell in Spells)
						{
							if (spell.Name == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Spell.Empower"))
							{
								CastSpell(spell, null);
								break;
							}
						}
					}
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Snares"))
				{
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Debilitating"))
				{
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Damage"))
				{
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.One"))
				{
					i++;
					if (i + 1 >= strargs.Length)
						return false;
					CommanderSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, strargs[++i]);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Two"))
				{
					i++;
					if (i + 1 >= strargs.Length)
						return false;
					CommanderSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, strargs[++i]);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Harm"))
				{
					if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
					{
						return false;
					}
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.DreadGuardian2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Drain"))
				{
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client, "GameObjects.CommanderPet.WR.Const.Suppress"))
				{
				}
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
			//all weapons removed before
			InventoryItem item = Inventory.GetItem(eInventorySlot.RightHandWeapon);
			if (item != null) Inventory.RemoveItem(item);
			item = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			if (item != null) Inventory.RemoveItem(item);

			ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>(itemId);

			if (temp == null)
			{
				if (log.IsErrorEnabled)
					log.Error(string.Format("Unable to find Bonedancer item: {0}", itemId));
				return;
			}

			Inventory.AddItem(slot, GameInventoryItem.Create<ItemTemplate>(temp));
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
			if (i >= m_controlledBrain.Length)
				return false;
			m_controlledBrain[i] = controlledNpc;
			PetCount++;
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
					IControlledBrain tempBrain = m_controlledBrain[i];
					//Lets get rid of the brain asap
					m_controlledBrain[i] = null;

					//Only decrement, we just lost one pet
					PetCount--;

					return base.RemoveControlledNpc(controlledNpc);
				}
			}

			return found;
		}
	}
}