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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Events;

namespace DOL.GS
{
	#region GameSiegeweapon
	/// <summary>
	/// Description résumée de GameSiegeWeapon.
	/// </summary>
	public class GameSiegeWeapon : GameMovingObject
	{
		public GameSiegeWeapon()
		{
			CurrentState = eState.Inactive;
			m_ammo = new ArrayList();

			m_ammoSlot =  0x14;
			ActionDelay
			= new int[]
				{
					0,//none
					10000,//aiming
					15000,//arming
					5000,//loading
					0//fireing
				};//en ms
			m_enableToMove = true;
		}
		public const int SIEGE_WEAPON_CONTROLE_DISTANCE = 2000;
		#region enum
		public enum eState : byte
		{
			Inactive = 0x0,
			Armed = 0x01,
			Aimed = 0x02,
			Ready = 0x03,//armed+aimed
		}

		public enum eCommand: byte
		{
			None = 0x00,
			PutAmmo = 0x01,
			Aim = 0x03,
			Fire = 0x04,
			Move = 0x05,
			Repair = 0x06,
			Salvage = 0x07,
			Release = 0x08,
		}
		#endregion
		#region properties
		public override Region CurrentRegion
		{
			get
			{
				return base.CurrentRegion;
			}
			set
			{
                base.CurrentRegion = value;
				this.SiegeWeaponTimer = new SiegeTimer(this);
			}
		}

		private ushort m_ammoType;
		public ushort AmmoType
		{
			get
			{return m_ammoType;
			}
			set { m_ammoType = value; }
		}

		private GamePlayer m_owner;
		public GamePlayer Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}
		private eState m_currentState;
		public eState CurrentState
		{
			get { return m_currentState; }
			set { m_currentState = value; }
		}
		
		public virtual ushort Effect
		{
			get {return m_effect;}
			set {m_effect = value;}
		}
		protected ArrayList m_ammo;
		public virtual ArrayList Ammo
		{
			get {return m_ammo;}
			set {m_ammo = value;}
		}
		protected SiegeTimer m_siegeTimer;
		public virtual SiegeTimer SiegeWeaponTimer
		{
			get {return m_siegeTimer;}
			set {m_siegeTimer = value;}
		}
		public override int MaxHealth
		{
			get
			{
				return 10000;
			}
		}
		private ushort m_ammoSlot;
		public ushort AmmoSlot
		{
			get { return m_ammoSlot; }
			set {m_ammoSlot = value;}
		}
		

#endregion
		#region public methode
		public void TakeControl(GamePlayer player)
		{
			if (this.Owner != null)
			{
				player.Out.SendMessage(GetName(0,true)+" is ever under control.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.SiegeWeapon != null)
			{
				player.Out.SendMessage("You have ever a siege weapon under control.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			if (this.IsMoving)
			{
				player.Out.SendMessage("You can take the control of a siege weapon while it is moving.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			this.Owner = player;
			player.SiegeWeapon = this;
			CurrentState &= GameSiegeWeapon.eState.Armed;
			this.Owner.Out.SendSiegeWeaponInterface(this);
			player.Out.SendMessage("You take control of "+GetName(0,false)+".",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
			GameEventMgr.AddHandler(player,GamePlayerEvent.InteractWith,new DOLEventHandler(SelectObjectTarget));

		}
		public void ReleaseControl()
		{
			this.Owner.Out.SendMessage("You are no longer controlling " + GetName(0, false) + ".",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
			this.Owner.Out.SendSiegeWeaponCloseInterface();
			GameEventMgr.RemoveHandler(this.Owner,GamePlayerEvent.InteractWith,new DOLEventHandler(SelectObjectTarget));
			this.Owner.SiegeWeapon = null;
			this.Owner = null;
		}
		protected virtual void SelectObjectTarget(DOLEvent e, object sender, EventArgs arguments)
		{
			InteractWithEventArgs arg = arguments as InteractWithEventArgs;
			SetGroundTarget(arg.Target.X,arg.Target.Y,arg.Target.Z);
		}

		public void Aim()
		{
			//The trebuchet isn't ready to be aimed yet!
			SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Aiming;
			this.Heading = GetHeadingToTarget(GroundTarget);
			PreAction();
			if (Owner != null)
			{
				Owner.Out.SendMessage(GetName(0,true)+" is turning to your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public void Arm()
		{
			SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Arming;
			PreAction();
			if (Owner != null)
			{//You prepare the cauldron of boiling oil for firing. (15.0s until armed)
				Owner.Out.SendMessage("You prepare "+GetName(0,false)+" for firing. ("+(GetActionDelay(SiegeTimer.eAction.Arming)/100).ToString("N")+"s until armed)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

		}
		public void Move()
		{
			if (!m_enableToMove)return;
			ReleaseControl();
			WalkTo(GroundTarget,100);
			//unarmed siege weapon
			this.CurrentState = (eState)(((byte)this.CurrentState) & (byte)eState.Armed ^ 0xFF);
		}

		public void Load(int ammo)
		{
			this.AmmoSlot = (ushort) ammo;
		}
		public void Aimed()
		{
			this.CurrentState |= eState.Aimed;
			if (Owner != null)
			{
				Owner.Out.SendMessage("Your "+this.Name+" is now aimed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
		public void Armed()
		{
			this.CurrentState |= eState.Armed;
			if (Owner != null)
			{
				Owner.Out.SendMessage("Your "+this.Name+" is now armed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return;
		}
		public void Fire()
		{
			if (this.CurrentState != eState.Ready){
				if (Owner != null)
				{
					Owner.Out.SendMessage("The "+this.Name+" is not ready to fire yet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return;
			}
			SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Fire;
			PreAction();
			if (Owner != null)
			{
				Owner.Out.SendMessage("You fire "+ GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			//unarmed but not unaimed
			this.CurrentState = (eState)(((byte)this.CurrentState) & (byte)eState.Armed ^ 0xFF);
		}

		public virtual void DoDamage()
		{
			GameLiving living = TargetObject as GameLiving;
			if (living == null) return;

			foreach(GamePlayer player in this.GetPlayersInRadius(500))
				player.Out.SendCombatAnimation(this.Owner,living,0x0000,0x0000,0x00,0x00,0x14,living.HealthPercent);
		}
		/*	slot:48 level:90 value1:0x00 value2:0x00 hand:0x00 damageType:0x00 objectType:0x29 weight:2    con:100 dur:0   qual:0   bonus:0  model:0x0A3C color:0x0000 effect:0x00 unk1_172:1 "2 greek fire"
			slot:49 level:90 value1:0x00 value2:0x00 hand:0x00 damageType:0x00 objectType:0x29 weight:2    con:100 dur:0   qual:0   bonus:0  model:0x0A3D color:0x0000 effect:0x00 unk1_172:1 "2 ice ball"
			index:0  unk1:0x5A00 unk2:0x0000 unk3:0x2900 unk4:0x0264 unk5:0x00 unk6:0x0000 model:0x0A3D unk7:0x0001 unk7:0x0000 name:"2 ice ball")
			index level value1  value2 hand  objecttype damagetype weight conc dur qual bonnus model color effect  
		*/

		public void Repair()
		{
			if(this.Owner.GetCraftingSkillValue(eCraftingSkill.SiegeCrafting) == -1)
			{
				this.Owner.Out.SendMessage("You must be a Siege weapon crafter to repair it.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			this.Owner.RepairSiegeWeapon(this);
		}

		public void salvage()
		{
			if(this.Owner.GetCraftingSkillValue(eCraftingSkill.SiegeCrafting) == -1)
			{
				this.Owner.Out.SendMessage("You must be a Siege weapon crafter to repair it.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			this.Owner.RepairSiegeWeapon(this);
		}
		#endregion
		#region private methode
		private void BroadcastAnimation()
		{
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSiegeWeaponAnimation(this);
				//				if (action == eAction.Fire)
				//					player.Out.SendEmoteAnimation(this, (eEmote)201); // on trebuchet fire external ammo ?
			}
		}
		protected int[] ActionDelay;
		private ushort m_effect;
		private bool m_enableToMove;

		/// <summary>
		/// delay to do action in Ms
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		private int GetActionDelay(SiegeTimer.eAction action)
		{
			return ActionDelay[(int)action];
		}
		
		private void PreAction()
		{
			if (SiegeWeaponTimer.IsAlive)
			{
				SiegeWeaponTimer.Stop();
				if (Owner != null)
					Owner.Out.SendSiegeWeaponCloseInterface();
			}
			SiegeWeaponTimer.Start(GetActionDelay(SiegeWeaponTimer.CurrentAction));
			if (Owner != null)
			{
				Owner.Out.SendSiegeWeaponInterface(this);
			}
			BroadcastAnimation();
		}
		#endregion
		#region override function
		public override bool ReceiveItem(GameLiving source, DOL.Database.InventoryItem item)
		{
			//todo check if bullet
			return base.ReceiveItem (source, item);
		}
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			if (source is GamePlayer)
			{
				damageAmount /= 30;
				criticalAmount /= 30;
			}
			base.TakeDamage(source,damageType,damageAmount,criticalAmount);
		}
		public override bool Interact(GamePlayer player)
		{
			if(!WorldMgr.CheckDistance(this, player, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far from siege equipment to control it!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			TakeControl(player);
			return true;
		}
		public override ushort Type()
		{
			//TODO
			return base.Type ();
		}
		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			if(!(obj is ItemTemplate)) return;
			ItemTemplate item = (ItemTemplate)obj;
			this.Name = item.Name;
			this.Model = (ushort) item.Model;
		}

		public bool EnableToMoove
		{
			set { m_enableToMove = value; }
		}

		#endregion
	}
	#endregion
    #region siegeTimer
	public class SiegeTimer : RegionAction
	{
		public enum eAction: byte
		{
			None = 0x00,
			Aiming = 0x01,
			Arming = 0x02,
			Loading = 0x03,
			Fire = 0x04,
		}

		/// <summary>
		/// Constructs a new UseSlotAction
		/// </summary>
		/// <param name="siegeWeapon">The siege weapon</param>
		public SiegeTimer(GameSiegeWeapon siegeWeapon) : base(siegeWeapon)
		{
			m_siegeWeapon = siegeWeapon;
		}

		private eAction m_currentAction;
		private GameSiegeWeapon m_siegeWeapon;

		public eAction CurrentAction
		{
			get { return m_currentAction; }
			set { m_currentAction = value; }
		}
		public GameSiegeWeapon SiegeWeapon
		{
			get { return m_siegeWeapon; }
			set { m_siegeWeapon = value; }
		}

		protected override void OnTick()
		{
			switch(CurrentAction)
			{
					case eAction.Arming:
					{
						SiegeWeapon.CurrentState &= GameSiegeWeapon.eState.Armed;
						SiegeWeapon.Owner.Out.SendMessage("Your " + SiegeWeapon.Name + " is now armed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}break;
				case eAction.Aiming :
					{
						SiegeWeapon.CurrentState &= GameSiegeWeapon.eState.Aimed;
					}break;
				case eAction.Loading :
					{
						//todo set ammo
					}break;
				case eAction.Fire :
					{
						SiegeWeapon.DoDamage();
					}break;
			}
			
			if (SiegeWeapon.Owner != null)
			{
				SiegeWeapon.Owner.Out.SendSiegeWeaponInterface(this.SiegeWeapon);
			}
		}
	}
	#endregion
}
/* messages:
You are too far from your siege equipment to control it any longer!
You can't salvage the trebuchet!
The trebuchet's target is too far away to reach!
Your target has moved out of range!
You put the ice ball into the field catapult.
The trebuchet is fully repaired!
That object isn't carryable...
*/