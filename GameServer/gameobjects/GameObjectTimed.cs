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

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de GameObjectTimed.
	/// </summary>
	public abstract class GameObjectTimed : GameObject
	{
		#region AddToWorld / RemoveFromWorld / Update to players

		/// <summary>
		/// Called to create an item in the world
		/// </summary>
		/// <returns>true when created</returns>
		public override bool AddToWorld()
		{
			if(!base.AddToWorld()) return false;
			if (m_removeItemAction == null) m_removeItemAction = new RemoveItemAction(this);
			m_removeItemAction.Start(RemoveDelay);
			return true;
		}

		/// <summary>
		/// Called to remove the item from the world
		/// </summary>
		/// <returns>true if removed</returns>
		public override bool RemoveFromWorld()
		{
			if(!base.RemoveFromWorld()) return false;
			if (m_removeItemAction != null)
			{
				m_removeItemAction.Stop();
				m_removeItemAction = null;
			}
			return true;
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState != eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendItemCreate(this);
			}
		}

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRemoveObject(this, eRemoveType.Disappear);
			}
		}

		#endregion

		#region ExamineMessage

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Insert(0, "You select "+ GetName(0, false) +".");
			return list;
		}

		#endregion

		#region Owners

		/// <summary>
		/// Holds the owners of this item, can be more than 1 person
		/// </summary>
		private readonly ArrayList m_owners = new ArrayList(1);

		/// <summary>
		/// Adds an owner to this item
		/// </summary>
		/// <param name="player">the object that is an owner</param>
		public void AddOwner(GamePlayer player)
		{
			lock(m_owners.SyncRoot)
			{
				foreach(WeakReference weak in m_owners)
				{
					if(weak.Target == player) return;
				}
				m_owners.Add(new WeakRef(player));
			}
		}
		/// <summary>
		/// Tests if a specific gameobject owns this item
		/// </summary>
		/// <param name="testOwner">the owner to test for</param>
		/// <returns>true if this object owns this item</returns>
		public bool IsOwner(GamePlayer testOwner)
		{
			//No owner ... return true
			if(m_owners.Count == 0) return true;

			lock(m_owners.SyncRoot)
			{
				foreach(WeakReference weak in m_owners)
				{
					if(weak.Target==testOwner) return true;
				}
				return false;
			}
		}
		#endregion

		#region RemoveDelay / RemoveAction

		/// <summary>
		/// Gets the delay in gameticks after which this object is removed
		/// </summary>
		public abstract int RemoveDelay
		{
			get;
		}

		/// <summary>
		/// The timer that will remove this object from the world after a delay
		/// </summary>
		protected RemoveItemAction m_removeItemAction;

		/// <summary>
		/// The callback function that will remove this bag after some time
		/// </summary>
		protected class RemoveItemAction : RegionAction
		{
			/// <summary>
			/// Constructs a new remove action
			/// </summary>
			/// <param name="item"></param>
			public RemoveItemAction(GameObjectTimed item) : base(item)
			{
			}

			/// <summary>
			/// The callback function that will remove this bag after some time
			/// </summary>
			protected override void OnTick()
			{
				m_actionSource.RemoveFromWorld();
			}
		}
		#endregion
	}
}
