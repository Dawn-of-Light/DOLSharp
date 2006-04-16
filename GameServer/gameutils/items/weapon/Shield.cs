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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a Shield
	/// </summary> 
	public class Shield : Weapon
	{
		#region Declaraction
		/// <summary>
		/// The size of the shield
		/// </summary>
		private eShieldSize m_size;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the size of the template
		/// </summary>
		public eShieldSize Size
		{
			get { return m_size; }
			set	{ m_size = value; }
		}

		#endregion

		#region Method
		/// <summary>
		/// Called when the item hit something or is hitted by something
		/// </summary>
		public override void OnItemHit(GameLivingBase target, bool isHitted)
		{
			base.OnItemHit(target, isHitted);

			if(isHitted && ProcEffectType == eMagicalEffectType.ReactiveEffect) // when hitted start reactive proc
			{
				//Shield only proc reactive effect again players
				if(!(target is GamePlayer)) return;

				// random chance
				if (!Util.Chance(10))
					return;

				SpellLine reactiveEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
				if (reactiveEffectLine != null)
				{
					IList spells = SkillBase.GetSpellList(reactiveEffectLine.KeyName);
					if (spells != null)
					{
						foreach (Spell spell in spells)
						{
							if (spell.ID == ProcSpellID)
							{
								if(spell.Level <= Level)
								{
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spell, reactiveEffectLine);
									if (spellHandler != null)
									{
										spellHandler.StartSpell(target);
									}
									else
									{
										Owner.Out.SendMessage("Reactive effect ID " + ProcSpellID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
								}
								break;
							}
						}
					}
				}
			}
		}
		#endregion
	
		/// <summary>
		/// Gets or sets the damage type of this weapon
		/// </summary>
		public override eDamageType DamageType 
		{
			get
			{
				return m_damageType;
			}
			set
			{
				// Shield can make damage of type : 
				// - legendary weapon : No legendary
				// - classic weapon : Crush
				m_damageType = DamageType;
					
			}
		}

		/// <summary>
		/// Gets how much hands are needed to use this weapon
		/// </summary>
		public override eHandNeeded HandNeeded 
		{
			// Shields can only be left hand
			get	{ return m_handNeeded; }
			set { m_handNeeded = eHandNeeded.LeftHand; }
		}

		/// <summary>
		/// Gets all inventory slots where the item can be equipped
		/// </summary>
		public override eInventorySlot[] EquipableSlot 
		{
			get  // Shield can only be equipped in left hand slot
			{
				return new eInventorySlot[] {eInventorySlot.LeftHandWeapon};
			}
		}

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Shield; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = (ArrayList) base.DelveInfo;

				list.RemoveRange(list.IndexOf("Damage Modifiers:"), list.Count - 1); // remove all classic weapon infos

				double itemDPS = DamagePerSecond/10.0;
				double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * Owner.Level);
				double itemSPD = Speed/1000.0;

				list.Add(" ");
				list.Add(" ");
				list.Add("Damage Modifiers (when used with shield styles):");
				list.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				list.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
				list.Add("- " + itemSPD.ToString("0.0") + " Shield Speed");
				list.Add(" ");

				switch (Size)
				{
					case eShieldSize.Small: list.Add("- Shield Size : Small"); break;
					case eShieldSize.Medium: list.Add("- Shield Size : Medium"); break;
					case eShieldSize.Large: list.Add("- Shield Size : Large"); break;
				}
				
				return list;
			}
		}
	}
}	
