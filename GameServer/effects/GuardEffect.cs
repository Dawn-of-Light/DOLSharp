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
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the guard ability
	/// </summary>
	public class GuardEffect : StaticEffect, IGameEffect
	{
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
		private Group m_playerGroup;

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

			m_playerGroup = guardSource.Group;

			if (m_playerGroup != guardTarget.Group)
				return;

			m_guardSource = guardSource;
			m_guardTarget = guardTarget;

			GameEventMgr.AddHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));

			m_guardSource.EffectList.Add(this);
			m_guardTarget.EffectList.Add(this);

			if (!WorldMgr.CheckDistance(guardSource, guardTarget, GuardAbilityHandler.GUARD_DISTANCE))
			{
				guardSource.Out.SendMessage(LanguageMgr.GetTranslation(guardSource.Client, "Effects.GuardEffect.YouAreNowGuardingYBut", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				guardTarget.Out.SendMessage(LanguageMgr.GetTranslation(guardTarget.Client, "Effects.GuardEffect.XIsNowGuardingYouBut", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				guardSource.Out.SendMessage(LanguageMgr.GetTranslation(guardSource.Client, "Effects.GuardEffect.YouAreNowGuardingY", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				guardTarget.Out.SendMessage(LanguageMgr.GetTranslation(guardTarget.Client, "Effects.GuardEffect.XIsNowGuardingYou", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			MemberDisbandedEventArgs eArgs = args as MemberDisbandedEventArgs;
			if (eArgs == null) return;
			if (eArgs.Member == GuardTarget || eArgs.Member == GuardSource)
			{
				Cancel(false);
			}
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			GameEventMgr.RemoveHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
			m_guardSource.EffectList.Remove(this);
			m_guardTarget.EffectList.Remove(this);

			m_guardSource.Out.SendMessage(LanguageMgr.GetTranslation(m_guardSource.Client, "Effects.GuardEffect.YourNoLongerGuardingY", m_guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_guardTarget.Out.SendMessage(LanguageMgr.GetTranslation(m_guardTarget.Client, "Effects.GuardEffect.XNoLongerGuardingYoy", m_guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_playerGroup = null;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				if (m_guardSource != null && m_guardTarget != null)
					return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.GuardEffect.GuardedByName", m_guardTarget.GetName(0, false), m_guardSource.GetName(0, false));
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.GuardEffect.Name");
			}
		}

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime
		{
			get { return 0; }
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon
		{
			get { return 412; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(3);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.GuardEffect.InfoEffect"));
				delveInfoList.Add(" ");
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.GuardEffect.XIsGuardingY", GuardSource.GetName(0, true), GuardTarget.GetName(0, false)));
				return delveInfoList;
			}
		}
	}
}
