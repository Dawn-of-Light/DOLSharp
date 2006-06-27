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
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a VisibleEquipment
	/// </summary> 
	public abstract class VisibleEquipment : EquipableItem
	{
		#region Declaraction
		/// <summary>
		/// The color/item of this item
		/// </summary>
		private int m_color;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the color/emblem of this item
		/// </summary>
		public int Color
		{
			get { return m_color; }
			set	{ m_color = value; }
		}

		#endregion

		#region Function
		/// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public override bool OnItemUsed(byte type)
		{
			if(! base.OnItemUsed(type)) return false;
			
			if(type == 0)
			{
				switch(SlotPosition)
				{
					case (int)eInventorySlot.RightHandWeapon :
					case (int)eInventorySlot.LeftHandWeapon : 
						Owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
						break;

					case (int)eInventorySlot.TwoHandWeapon : 
						Owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
						break;		

					case (int)eInventorySlot.DistanceWeapon : 
					{
						bool newAttack = false;
						if(Owner.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
						{
							Owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
						}
						else if (!Owner.AttackState)
						{
							Owner.StartAttack(Owner.TargetObject);
							newAttack = true;
						}

						//Clean up range attack state/type if we are not in combat mode
						//anymore
						if(!Owner.AttackState)
						{
							Owner.RangeAttackState=GameLiving.eRangeAttackState.None;
							Owner.RangeAttackType=GameLiving.eRangeAttackType.Normal;
						}
						if(!newAttack && Owner.RangeAttackState != GameLiving.eRangeAttackState.None)
						{
							if(Owner.RangeAttackState == GameLiving.eRangeAttackState.ReadyToFire)
							{
								Owner.RangeAttackState = GameLiving.eRangeAttackState.Fire;
							}
							else if(Owner.RangeAttackState == GameLiving.eRangeAttackState.Aim)
							{
								if(!Owner.TargetInView)
								{
									// Don't store last target if it's not visible
									Owner.Out.SendMessage("You can't see your target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									if(Owner.RangeAttackTarget != Owner.TargetObject)
									{
										//set new target only if there was no target before
										Owner.RangeAttackTarget = Owner.TargetObject;
									}

									Owner.RangeAttackState = GameLiving.eRangeAttackState.AimFire;
									Owner.Out.SendMessage("You will now automatically release your shot.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
								}
							}
							else if(Owner.RangeAttackState == GameLiving.eRangeAttackState.AimFire)
							{
								Owner.RangeAttackState = GameLiving.eRangeAttackState.AimFireReload;
								Owner.Out.SendMessage("You will now automatically release your shot and reload.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							}
							else if(Owner.RangeAttackState == GameLiving.eRangeAttackState.AimFireReload)
							{
								Owner.RangeAttackState = GameLiving.eRangeAttackState.Aim;
								Owner.Out.SendMessage("You will no longer automatically release your shot or reload.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							}
						}
						break;
					}
				}
			}

			return true;
		}
		#endregion
	}
}	