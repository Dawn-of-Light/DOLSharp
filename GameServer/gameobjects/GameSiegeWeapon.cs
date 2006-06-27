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
using DOL.AI.Brain;
using DOL.GS.Database;
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
			SetOwnBrain(new SiegeWeaponBrain());
			this.Realm = 0;
			Level = 1;
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
		public static readonly ushort SIEGE_WEAPON_CONTROLE_DISTANCE = 2000;
		public static readonly int DECAYPERIOD = 240000; //ms
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
		public override Region Region
		{
			get
			{
				return base.Region;
			}
			set
			{
                base.Region = value;
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
		public int DecayedHp
		{
			get{return 3 * (this.MaxHealth / 10);}
		}
		public int DeductHp
		{
			get{return -this.MaxHealth / 10;}
		}
		private int m_timesrepaired;
		public int TimesRepaired
		{
			get { return m_timesrepaired; }
			set { m_timesrepaired = value; }
		}

		protected RegionTimer m_decayTimer;
		/// <summary>
		/// The lock object for lazy regen timers initialization
		/// </summary>
		protected readonly object m_decayTimerLock = new object();
		protected GameObject m_decayObject = null;


#endregion
		#region public methode
		public void TakeControl(GamePlayer player)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.Dying, new DOLEventHandler(ReleaseControlOnEvent));
			GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(ReleaseControlOnEvent));
			if (this.Owner != null)
			{
				player.Out.SendMessage(GetName(0, true) + " is allready under control of another player.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.SiegeWeapon != null)
			{
				player.Out.SendMessage("You allready have a siege weapon under control.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				return;
			}
			if (this.IsMoving)
			{
				player.Out.SendMessage("You can't take control of a siege weapon while it is moving.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				return;
			}
			this.Owner = player;
			player.SiegeWeapon = this;
			Arm();
			CurrentState &= GameSiegeWeapon.eState.Armed;
			this.Owner.Out.SendSiegeWeaponInterface(this);
			player.Out.SendMessage("You take control of "+GetName(0,false)+".",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
			GameEventMgr.AddHandler(player,GamePlayerEvent.InteractWith,new DOLEventHandler(SelectObjectTarget));

		}

		public static void ReleaseControlOnEvent(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;
			player.SiegeWeapon.ReleaseControl();
		}

		public void ReleaseControl()
		{
			this.Owner.Out.SendMessage("You are no longer controlling " + GetName(0, false) + ".",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
			this.Owner.Out.SendSiegeWeaponCloseInterface();
			GameEventMgr.RemoveHandler(this.Owner,GamePlayerEvent.InteractWith,new DOLEventHandler(SelectObjectTarget));
			GameEventMgr.RemoveHandler(GamePlayerEvent.Dying, new DOLEventHandler(ReleaseControlOnEvent));
			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(ReleaseControlOnEvent));
			this.Owner.SiegeWeapon = null;
			this.Owner = null;
		}
		protected virtual void SelectObjectTarget(DOLEvent e, object sender, EventArgs arguments)
		{
			InteractWithEventArgs arg = arguments as InteractWithEventArgs;
			if (!Position.CheckSquareDistance(Owner.Position, (uint) (WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE)))
			{
				this.Owner.Out.SendMessage(GetName(0,true)+ "'s target is too far away to reach!",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			GroundTarget = arg.Target.Position;
		}

		public void Aim()
		{
			if (!CanUse()) return;
			//The trebuchet isn't ready to be aimed yet!
			SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Aiming;
			this.Heading = Position.GetHeadingTo(GroundTarget);
			PreAction();
			if (Owner != null)
			{
				Owner.Out.SendMessage(GetName(0,true)+" is turning to your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public void Arm()
		{
			if (!CanUse()) return;
			SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Arming;
			PreAction();
			if (Owner != null)
			{//You prepare the cauldron of boiling oil for firing. (15.0s until armed)
				Owner.Out.SendMessage("You prepare "+GetName(0,false)+" for firing. ("+(GetActionDelay(SiegeTimer.eAction.Arming)/1000).ToString("N")+"s until armed)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

		}
		public void Move()
		{
			if (!CanUse()) return;
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
			if (!CanUse()) return;
			this.CurrentState |= eState.Aimed;
			if (Owner != null)
			{
				Owner.Out.SendMessage("Your "+this.Name+" is now aimed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
		public void Armed()
		{
			if (!CanUse()) return;
			this.CurrentState |= eState.Armed;
			if (Owner != null)
			{
				Owner.Out.SendMessage("Your "+this.Name+" is now armed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return;
		}
		public void Fire()
		{
			if (!CanUse()) return;
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
			if (TimesRepaired <= 3)
			{
				this.Owner.RepairSiegeWeapon(this);
					TimesRepaired = TimesRepaired + 1;
			}
			else
			{
				this.Owner.Out.SendMessage("The siegeweapon has decayed beyond repairs!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
			}
		}

		public void salvage()
		{
			if(this.Owner.GetCraftingSkillValue(eCraftingSkill.SiegeCrafting) == -1)
			{
				this.Owner.Out.SendMessage("You must be a Siege weapon crafter to repair it.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				return;
			}
			this.Owner.SalvageSiegeWeapon(this);
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
		private Boolean CanUse()
		{
            if (Health <= DecayedHp)
            {
                this.Owner.Out.SendMessage("The siegeweapon needs to be repaired!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return false;
            }
			if (!Position.CheckSquareDistance(Owner.Position, (uint) (WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE)))
			{
				Owner.Out.SendMessage("You are too far from your siege equipment to control it any longer!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
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
		public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			//todo check if bullet
			return base.ReceiveItem (source, item);
		}
		public override void TakeDamage(GameLiving source, eDamageType damageType, int damageAmount, int criticalAmount)
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
			if(!Position.CheckSquareDistance(player.Position, (uint)(WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE)))
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
		public override void LoadFromDatabase(object obj)
		{
			base.LoadFromDatabase(obj);
			if(!(obj is GenericItemTemplate)) return;
			GenericItemTemplate item = (GenericItemTemplate)obj;
			this.Name = item.Name;
			this.Model = (ushort) item.Model;
		}

		public bool EnableToMoove
		{
			set { m_enableToMove = value; }
		}
		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;
			StartDecay();
			TimesRepaired = 0;
			return true;
		}
		public override bool RemoveFromWorld()
		{
			if (!base.RemoveFromWorld()) return false;
			StopDecay();
			return true;
		}
		#endregion
		#region decay
		private void StartDecay()
		{
			if (ObjectState != eObjectState.Active)
				return;
			lock (m_decayTimerLock)
			{
				if (m_decayTimer == null)
				{
					m_decayTimer = new RegionTimer(this);
					m_decayTimer.Callback = new RegionTimerCallback(DecayTimerCallback);
				}
				else if (m_decayTimer.IsAlive)
					return;
				m_decayTimer.Start(DECAYPERIOD);
			}
		}

		private void StopDecay()
		{
			lock (m_decayTimerLock)
			{
				if (m_decayTimer == null)
					return;
				m_decayTimer.Stop();
				m_decayTimer = null;
			}
		}

		private int DecayTimerCallback(RegionTimer callingTimer)
		{
			if (Health <= 0)
			{
				StopDecay();
				Die(m_decayObject);
				return 0;
			}
			ChangeHealth(this, eHealthChangeType.Unknown, DeductHp);
			return DECAYPERIOD;
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
						SiegeWeapon.Armed();
					}break;
				case eAction.Aiming :
					{
						SiegeWeapon.Aimed();
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
Your target has moved out of range!
You put the ice ball into the field catapult.
That object isn't carryable...
*/