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
	/// The helper class for the intercept ability
	/// </summary>
	public class InterceptEffect : IGameEffect
	{
		protected const String delveString = "Ability that if successful will intercept an attack meant for the ability's target. You will take damage in the target's place.";

		/// <summary>
		/// Holds the interceptor
		/// </summary>
		private GamePlayer m_interceptSource;

		/// <summary>
		/// Reference to gameplayer that is protecting this player with intercept
		/// </summary>
		private GamePlayer m_interceptTarget;

		/// <summary>
		/// Holds the interceptor/intercepted group
		/// </summary>
		private PlayerGroup m_group;

		/// <summary>
		/// Gets the interceptor
		/// </summary>
		public GamePlayer InterceptSource
		{
			get { return m_interceptSource; }
		}

		/// <summary>
		/// Gets the intercepted
		/// </summary>
		public GamePlayer InterceptTarget
		{
			get { return m_interceptTarget; }
		}

		/// <summary>
		/// Creates a new intercept effect
		/// </summary>
		public InterceptEffect()
		{
		}

		/// <summary>
		/// Start the intercepting on player
		/// </summary>
		/// <param name="interceptor">The interceptor</param>
		/// <param name="intercepted">The intercepted</param>
		public void Start(GamePlayer interceptor, GamePlayer intercepted)
		{
			m_group = interceptor.PlayerGroup;
			if (m_group == null) return;

			m_interceptSource = interceptor;
			m_interceptTarget = intercepted;

			GameEventMgr.AddHandler(m_group, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(GroupDisbandCallback));

			if (!interceptor.Position.CheckSquareDistance(intercepted.Position, (uint) (InterceptAbilityHandler.INTERCEPT_DISTANCE*InterceptAbilityHandler.INTERCEPT_DISTANCE)))
			{
				interceptor.Out.SendMessage(string.Format("You are now attempting to intercept an attack for {0}, but you must stand closer.", intercepted.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				intercepted.Out.SendMessage(string.Format("{0} is now attempting to intercept an attack for you, but you must stand closer.", interceptor.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				interceptor.Out.SendMessage(string.Format("You are now attempting to intercept an attack for {0}.", intercepted.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				intercepted.Out.SendMessage(string.Format("{0} is now attempting to intercept an attack for you.", interceptor.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			interceptor.EffectList.Add(this);
			intercepted.EffectList.Add(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			GameEventMgr.RemoveHandler(m_group, PlayerGroupEvent.PlayerDisbanded, new DOLEventHandler(GroupDisbandCallback));
			InterceptSource.EffectList.Remove(this);
			InterceptTarget.EffectList.Remove(this);
			if (playerCancel)
			{
				InterceptSource.Out.SendMessage("You are no longer attempting to intercept an attack for " + InterceptTarget.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				InterceptTarget.Out.SendMessage(InterceptSource.GetName(0, true) + " is no longer attempting to intercept an attack for you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			m_group = null;
		}

		/// <summary>
		/// Cancels effect if interceptor or intercepted leaves the group
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">The group</param>
		/// <param name="args"></param>
		protected void GroupDisbandCallback(DOLEvent e, object sender, EventArgs args)
		{
			PlayerDisbandedEventArgs eArgs = args as PlayerDisbandedEventArgs;
			if (eArgs == null) return;
			if (eArgs.Player == InterceptSource || eArgs.Player == InterceptTarget)
			{
				Cancel(false);
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get
			{
				if (m_interceptSource != null && m_interceptTarget != null)
					return m_interceptSource.GetName(0, false) + " is Intercepting " + m_interceptTarget.GetName(0, false);
				return "Intercept";
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
			get { return 410; }
		}

		/// <summary>
		/// Holds unique id for identification in effect list
		/// </summary>
		private ushort m_id;

		/// <summary>
		/// Gets or Sets unique id for identification in effect list
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
				delveInfoList.Add(InterceptSource.GetName(0, true) + " is intercepting for " + InterceptTarget.GetName(0, false));
				return delveInfoList;
			}
		}
	}
}
