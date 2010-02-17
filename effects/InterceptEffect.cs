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
using System.Collections.Generic;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the intercept ability
	/// </summary>
	public class InterceptEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// Holds the interceptor
		/// </summary>
		private GameLiving m_interceptSource;

		/// <summary>
		/// Reference to gameplayer that is protecting this player with intercept
		/// </summary>
		private GameLiving m_interceptTarget;

		/// <summary>
		/// Holds the interceptor/intercepted group
		/// </summary>
		private Group m_group;

		/// <summary>
		/// Gets the interceptor
		/// </summary>
		public GameLiving InterceptSource
		{
			get { return m_interceptSource; }
		}

		/// <summary>
		/// Gets the intercepted
		/// </summary>
		public GameLiving InterceptTarget
		{
			get { return m_interceptTarget; }
		}

		/// <summary>
		/// chance to intercept
		/// </summary>
		public int InterceptChance
		{
			get
			{
				if (InterceptSource.Name.ToLower().Contains("brittle guard"))
					return 100;
				else
					return 50;
			}
		}

		/// <summary>
		/// Start the intercepting on player
		/// </summary>
		/// <param name="interceptor">The interceptor</param>
		/// <param name="intercepted">The intercepted</param>
		public void Start(GameLiving interceptor, GameLiving intercepted)
		{
			if (interceptor is GamePlayer && intercepted is GamePlayer)
			{
				m_group = ((GamePlayer)interceptor).Group;
				if (m_group == null) return;
				GameEventMgr.AddHandler(m_group, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
			}
			m_interceptSource = interceptor;
			m_owner = m_interceptSource;
			m_interceptTarget = intercepted;

			if (!interceptor.IsWithinRadius(intercepted, InterceptAbilityHandler.INTERCEPT_DISTANCE))
			{
				if (interceptor is GamePlayer)
					((GamePlayer)interceptor).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)interceptor).Client, "Effects.InterceptEffect.YouAttemtInterceptYBut", intercepted.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (intercepted is GamePlayer && interceptor is GamePlayer)
					((GamePlayer)intercepted).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)intercepted).Client, "Effects.InterceptEffect.XAttemtInterceptYouBut", interceptor.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (interceptor is GamePlayer)
					((GamePlayer)interceptor).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)interceptor).Client, "Effects.InterceptEffect.YouAttemtInterceptY", intercepted.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (intercepted is GamePlayer && interceptor is GamePlayer)
					((GamePlayer)intercepted).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)intercepted).Client, "Effects.InterceptEffect.XAttemptInterceptYou", interceptor.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			interceptor.EffectList.Add(this);
			intercepted.EffectList.Add(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			if (InterceptSource is GamePlayer && InterceptTarget is GamePlayer)
			{
				GameEventMgr.RemoveHandler(m_group, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
				m_group = null;
			}
			InterceptSource.EffectList.Remove(this);
			InterceptTarget.EffectList.Remove(this);
			if (playerCancel)
			{
				if (InterceptSource is GamePlayer)
					((GamePlayer)InterceptSource).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)InterceptSource).Client, "Effects.InterceptEffect.YouNoAttemtInterceptY", InterceptTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (InterceptTarget is GamePlayer && InterceptSource is GamePlayer)
					((GamePlayer)InterceptTarget).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)InterceptTarget).Client, "Effects.InterceptEffect.XNoAttemptInterceptYou", InterceptSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Cancels effect if interceptor or intercepted leaves the group
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">The group</param>
		/// <param name="args"></param>
		protected void GroupDisbandCallback(DOLEvent e, object sender, EventArgs args)
		{
			MemberDisbandedEventArgs eArgs = args as MemberDisbandedEventArgs;
			if (eArgs == null) return;
			if (eArgs.Member == InterceptSource || eArgs.Member == InterceptTarget)
			{
				Cancel(false);
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				if (Owner is GamePlayer)
				{
					if (m_interceptSource != null && m_interceptTarget != null)
						return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.InterceptEffect.InterceptedByName", m_interceptTarget.GetName(0, false), m_interceptSource.GetName(0, false));
					return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.InterceptEffect.Name");
				}
				return "";
			}
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon
		{
			get
			{
				//let's not display this icon on NPC's because i use this for spiritmasters
				if (m_owner is GameNPC)
					return 7249;
				return 410;
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>(4);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.InterceptEffect.InfoEffect"));
				delveInfoList.Add(" ");
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.InterceptEffect.XInterceptingY", InterceptSource.GetName(0, true), InterceptTarget.GetName(0, false)));

				return delveInfoList;
			}
		}
	}
}
