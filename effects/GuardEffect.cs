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
using DOL.Events;
using DOL.GS.SkillHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the guard ability
	/// </summary>
	public class GuardEffect : IGameEffect
	{
		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "Ability that if successful will guard an attack meant for the ability's target. You will block in the target's place.";

		/// <summary>
		/// Holds guarder
		/// </summary>
		private GamePlayer m_guardSource;

		/// <summary>
		/// Gets guarder
		/// </summary>
		public GamePlayer GuardSource
		{
			get { return m_guardSource; }
		}

		/// <summary>
		/// Holds guarded player
		/// </summary>
		private GamePlayer m_guardTarget;

		/// <summary>
		/// Gets guarded player
		/// </summary>
		public GamePlayer GuardTarget
		{
			get { return m_guardTarget; }
		}

		/// <summary>
		/// Holds player group
		/// </summary>
		private PlayerGroup m_playerGroup;

		/// <summary>
		/// Creates a new guard effect
		/// </summary>
		public GuardEffect()
		{
		}

		/// <summary>
		/// Start the guarding on player
		/// </summary>
		/// <param name="guardSource">The guarder</param>
		/// <param name="guardTarget">The player guarded by guarder</param>
		public void Start(GamePlayer guardSource, GamePlayer guardTarget)
		{
			if (guardSource == null || guardTarget == null)
				return;

			m_playerGroup = guardSource.PlayerGroup;

			if (m_playerGroup != guardTarget.PlayerGroup)
				return;

			m_guardSource = guardSource;
			m_guardTarget = guardTarget;

			GameEventMgr.AddHandler(m_playerGroup, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(GroupDisbandCallback));

			m_guardSource.EffectList.Add(this);
			m_guardTarget.EffectList.Add(this);

			if (!WorldMgr.CheckDistance(guardSource, guardTarget, GuardAbilityHandler.GUARD_DISTANCE))
			{
				guardSource.Out.SendMessage(string.Format("You are now guarding {0}, but you must stand closer.", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				guardTarget.Out.SendMessage(string.Format("{0} is now guarding you, but you must stand closer.", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				guardSource.Out.SendMessage(string.Format("You are now guarding {0}.", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				guardTarget.Out.SendMessage(string.Format("{0} is now guarding you.", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Cancels guard if one of players disbands
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">The group</param>
		/// <param name="args"></param>
		protected void GroupDisbandCallback(DOLEvent e, object sender, EventArgs args)
		{
			PlayerDisbandedEventArgs eArgs = args as PlayerDisbandedEventArgs;
			if (eArgs == null) return;
			if (eArgs.Player == GuardTarget || eArgs.Player == GuardSource)
			{
				Cancel(false);
			}
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			GameEventMgr.RemoveHandler(m_playerGroup, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(GroupDisbandCallback));
			m_guardSource.EffectList.Remove(this);
			m_guardTarget.EffectList.Remove(this);

			m_guardSource.Out.SendMessage(string.Format("You are no longer guarding {0}.", m_guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_guardTarget.Out.SendMessage(string.Format("{0} is no longer guarding you.", m_guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_playerGroup = null;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get
			{
				if (m_guardSource != null && m_guardTarget != null)
					return m_guardTarget.GetName(0, false) + " guarded by " + m_guardSource.GetName(0, false);
				return "Guard";
			}
		}

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public int RemainingTime
		{
			get { return 0; }
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon
		{
			get { return 412; }
		}

		/// <summary>
		/// The internal unique effect ID
		/// </summary>
		private ushort m_id;

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

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
				delveInfoList.Add(GuardSource.GetName(0, true) + " is guarding " + GuardTarget.GetName(0, false));
				return delveInfoList;
			}
		}
	}
}
