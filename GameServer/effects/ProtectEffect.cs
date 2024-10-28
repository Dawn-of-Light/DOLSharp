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
		/// The individual protecting the target
		/// </summary>
		public GameLiving ProtectSource { get; private set; }

		/// <summary>
		/// The individual being protected
		/// </summary>
		public GameLiving ProtectTarget { get; private set; }

		private Group m_playerGroup = null;

		/// <summary>
		/// Creates a new protect effect
		/// </summary>
		public ProtectEffect()
		{
		}

		/// <summary>
		/// Start the guarding on player
		/// </summary>
		public void Start(GameLiving source, GameLiving target)
		{
			if (source == null || target == null)
				return;

			m_owner = source;
			ProtectSource = source;
			ProtectTarget = target;

			if (target.Group != null && target.Group == source.Group)
			{
				m_playerGroup = source.Group;
				GameEventMgr.AddHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
			}

			source.EffectList.Add(this);
			target.EffectList.Add(this);

			if (!source.IsWithinRadius(target, ProtectAbilityHandler.PROTECT_DISTANCE))
			{
				if (source is GamePlayer playerSource)
					playerSource.Out.SendMessage(LanguageMgr.GetTranslation(playerSource.Client, "Effects.ProtectEffect.YouProtectingYBut", target.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (target is GamePlayer playerTarget)
					playerTarget.Out.SendMessage(LanguageMgr.GetTranslation(playerTarget.Client, "Effects.ProtectEffect.XProtectingYouBut", source.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (source is GamePlayer playerSource)
					playerSource.Out.SendMessage(LanguageMgr.GetTranslation(playerSource.Client, "Effects.ProtectEffect.YouProtectingY", target.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (target is GamePlayer playerTarget)
					playerTarget.Out.SendMessage(LanguageMgr.GetTranslation(playerTarget.Client, "Effects.ProtectEffect.XProtectingYou", source.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			if (args is MemberDisbandedEventArgs eArgs && (eArgs.Member == ProtectTarget || eArgs.Member == ProtectSource))
				Cancel(false);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			if (m_playerGroup != null)
				GameEventMgr.RemoveHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
			
			// intercept handling is done by the active part             
			ProtectSource.EffectList.Remove(this);
			ProtectTarget.EffectList.Remove(this);

			if (ProtectSource is GamePlayer playerSource)
				playerSource.Out.SendMessage(LanguageMgr.GetTranslation(playerSource.Client, "Effects.ProtectEffect.YouNoProtectY", ProtectTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if (ProtectTarget is GamePlayer playerTarget)
				playerTarget.Out.SendMessage(LanguageMgr.GetTranslation(playerTarget.Client, "Effects.ProtectEffect.XNoProtectYou", ProtectSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				string language;
				if (ProtectTarget is GamePlayer playerTarget)
					language = playerTarget.Client.Account.Language;
				else if (ProtectSource is GamePlayer playerSource)
					language = playerSource.Client.Account.Language;
				else
					language = LanguageMgr.DefaultLanguage;
				return LanguageMgr.GetTranslation(language, "Effects.ProtectEffect.ProtectByName", ProtectTarget.GetName(0, false), ProtectSource.GetName(0, false));
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
				string language;
				if (ProtectTarget is GamePlayer playerTarget)
					language = playerTarget.Client.Account.Language;
				else if (ProtectSource is GamePlayer playerSource)
					language = playerSource.Client.Account.Language;
				else
					language = LanguageMgr.DefaultLanguage;

				return new List<string>(4)
				{
					LanguageMgr.GetTranslation(language, "Effects.ProtectEffect.InfoEffect"),
					" ",
					LanguageMgr.GetTranslation(language, "Effects.ProtectEffect.XProtectingY", ProtectSource.GetName(0, true), ProtectTarget.GetName(0, false))
				};
			}
		}
	}
}
