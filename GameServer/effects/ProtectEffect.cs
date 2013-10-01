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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.Effects
{

	/// <summary>
	/// The helper class for the protect ability
	/// </summary>
	public class ProtectEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// The player protecting the target
		/// </summary>
		GamePlayer m_protectSource;

		/// <summary>
		/// Gets the player protecting the target
		/// </summary>
		public GamePlayer ProtectSource
		{
			get { return m_protectSource; }
			set { m_protectSource = value; }
		}

		/// <summary>
		/// Reference to gameplayer that is protecting this player
		/// </summary>
		GamePlayer m_protectTarget = null;

		/// <summary>
		/// Gets the protected player
		/// </summary>
		public GamePlayer ProtectTarget
		{
			get { return m_protectTarget; }
			set { m_protectTarget = value; }
		}

		private Group m_playerGroup;

		/// <summary>
		/// Creates a new protect effect
		/// </summary>
		public ProtectEffect()
		{
		}

		/// <summary>
		/// Start the guarding on player
		/// </summary>
		public void Start(GamePlayer protectSource, GamePlayer protectTarget)
		{
			if (protectSource == null || protectTarget == null)
				return;

			m_owner = protectSource;
			m_playerGroup = protectSource.Group;

			if (m_playerGroup != protectTarget.Group)
				return;

			m_protectSource = protectSource;
			m_protectTarget = protectTarget;

			GameEventMgr.AddHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));

			m_protectSource.EffectList.Add(this);
			m_protectTarget.EffectList.Add(this);

			if (!protectSource.IsWithinRadius(protectTarget, ProtectAbilityHandler.PROTECT_DISTANCE))
			{
				protectSource.Out.SendMessage(LanguageMgr.GetTranslation(protectSource.Client, "Effects.ProtectEffect.YouProtectingYBut", protectTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				protectTarget.Out.SendMessage(LanguageMgr.GetTranslation(protectTarget.Client, "Effects.ProtectEffect.XProtectingYouBut", protectSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				protectSource.Out.SendMessage(LanguageMgr.GetTranslation(protectSource.Client, "Effects.ProtectEffect.YouProtectingY", protectTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				protectTarget.Out.SendMessage(LanguageMgr.GetTranslation(protectTarget.Client, "Effects.ProtectEffect.XProtectingYou", protectSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			if (eArgs.Member == ProtectTarget || eArgs.Member == ProtectSource)
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
			// intercept handling is done by the active part             
			m_protectSource.EffectList.Remove(this);
			m_protectTarget.EffectList.Remove(this);

			m_protectSource.Out.SendMessage(LanguageMgr.GetTranslation(m_protectSource.Client, "Effects.ProtectEffect.YouNoProtectY", m_protectTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			m_protectTarget.Out.SendMessage(LanguageMgr.GetTranslation(m_protectTarget.Client, "Effects.ProtectEffect.XNoProtectYou", m_protectSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_playerGroup = null;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{

			get
			{
				if (m_protectSource != null && m_protectTarget != null)
					return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.ProtectEffect.ProtectByName", m_protectTarget.GetName(0, false), m_protectSource.GetName(0, false));
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.ProtectEffect.Name");
			}
		}

		/// <summary>
		/// Remaining Time of the effect in seconds
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
			get { return 411; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>(4);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.ProtectEffect.InfoEffect"));
				delveInfoList.Add(" ");
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.ProtectEffect.XProtectingY", ProtectSource.GetName(0, true), ProtectTarget.GetName(0, false)));

				return delveInfoList;
			}
		}
	}
}
