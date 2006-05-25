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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the engage ability
	/// </summary>
	public class EngageEffect : IGameEffect
	{
		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "Endurance is drained in return for a significantly increased blocking rate. Engage can only be used at range, or while closing to attack. Once you attack, it is disabled.";

		/// <summary>
		/// The player that defends the target
		/// </summary>
		GamePlayer m_engageSource;

		/// <summary>
		/// Gets the player that defends the target
		/// </summary>
		public GamePlayer EngageSource
		{
			get { return m_engageSource; }
			set { m_engageSource = value; }
		}

		/// <summary>
		/// The player that is defended by the engage source
		/// </summary>
		GameLiving m_engageTarget;

		/// <summary>
		/// Gets the defended player
		/// </summary>
		public GameLiving EngageTarget
		{
			get { return m_engageTarget; }
			set { m_engageTarget = value; }
		}

		/// <summary>
		/// Creates a new engage effect
		/// </summary>
		public EngageEffect()
		{
		}

		/// <summary>
		/// Start the berserk on player
		/// </summary>
		public void Start(GamePlayer engageSource, GameLiving engageTarget)
		{
			m_engageSource = engageSource;
			m_engageSource.EffectList.Add(this);

			m_engageTarget = engageTarget;

			m_engageSource.Out.SendMessage("You concentrate on blocking the blows of "+m_engageTarget.GetName(0,false)+"!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			// if player isn't already attacking attack.
//			if (!m_engageSource.AttackState)
//				m_engageSource.StartAttack(m_engageTarget);

			// only emulate attack mode so it works more like on live servers
			// entering real attack mode while engaging someone stops engage
			// other players will see attack mode after pos update packet is sent
			if (!m_engageSource.AttackState)
			{
				m_engageSource.Out.SendAttackMode(true);
				m_engageSource.Out.SendMessage("You enter combat mode to engage your target!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				m_engageSource.Out.SendMessage("You enter combat mode and target ["+engageTarget.GetName(0, false)+"]", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			if(playerCancel)
				m_engageSource.Out.SendMessage("You no longer concentrate on blocking the blows!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				m_engageSource.Out.SendMessage("You are no longer attempting to engage a target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_engageSource.EffectList.Remove(this);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get
			{
				if (m_engageTarget != null)
					return "Engage: " + m_engageTarget.GetName(0, false);
				return "Engage";
			}
		}

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public int RemainingTime { get { return 0; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon { get { return 421; } }

		/// <summary>
		/// Stores the internal effect ID
		/// </summary>
		ushort m_id;

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID { get { return m_id; } set { m_id = value; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(3);
				delveInfoList.Add(delveString);
				delveInfoList.Add(" ");
				delveInfoList.Add(m_engageSource.GetName(0, true) + " engages " + m_engageTarget.GetName(0, false));
				return delveInfoList;
			}
		}
	}
}
