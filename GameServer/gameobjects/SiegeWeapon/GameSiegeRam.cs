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
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GameMovingObject is a base class for boats and siege weapons.
	/// </summary>
	public  class GameSiegeRam : GameSiegeWeapon
	{
		public GameSiegeRam() : base()
		{
			MeleeDamageType = eDamageType.Crush;
			Name = "siege ram";

			//AmmoType = 0x3B00;
			AmmoType = 0x2601;
			//this.Effect = 0x8A1;
			this.Model = 0xA2A;//0xA28
			//TODO find all value for ram
			ActionDelay = new int[]
			{
				0,//none
				5000,//aiming
				10000,//arming
				0,//loading
				1100//fireing
			};//en ms
		}
		protected override void SelectObjectTarget(DOLEvent e, object sender, EventArgs arguments)
		{
			InteractWithEventArgs arg = arguments as InteractWithEventArgs;
			if (this.TargetObject is GameKeepDoor)
			{
				if (Position.CheckSquareDistance(TargetObject.Position, (uint) (WorldMgr.PICKUP_DISTANCE*WorldMgr.PICKUP_DISTANCE)))
					TargetObject = arg.Target;

				Owner.Out.SendMessage("The Ram is to far away from your target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

		}
		public override void DoDamage()
		{
			GameLiving target = (TargetObject as GameLiving);
			if (target == null)
			{
				//todo msg
				return;
			}
			//todo good  distance check

			int damageAmount = 0;
			switch(this.Level)
			{
				case 1:damageAmount=300;
					break;
				case 2:damageAmount=450;
					break;
				case 3:damageAmount=750;
					break;
			}
			
			target.TakeDamage(this,eDamageType.Crush,damageAmount,0);
			Owner.Out.SendMessage("The Ram hits " + target.Name + " for " + damageAmount + " dmg!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
			base.DoDamage();
		}
	}
}
