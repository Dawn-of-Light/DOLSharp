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
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the engage ability
	/// </summary>
	public class EngageEffect : StaticEffect, IGameEffect
	{
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
		/// Start the berserk on player
		/// </summary>
		public override void Start(GameLiving engageSource)
		{
			base.Start(engageSource);

			m_engageTarget = engageSource.TargetObject as GameLiving;
			engageSource.IsEngaging = true;

			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.EngageEffect.ConcOnBlockingX", m_engageTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			// only emulate attack mode so it works more like on live servers
			// entering real attack mode while engaging someone stops engage
			// other players will see attack mode after pos update packet is sent
			if (!m_owner.AttackState)
			{
				m_owner.StartAttack(m_engageTarget);
				if (m_owner is GamePlayer)
					(m_owner as GamePlayer).Out.SendAttackMode(true);
				//m_engageSource.Out.SendMessage("You enter combat mode to engage your target!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				//m_engageSource.Out.SendMessage("You enter combat mode and target ["+engageTarget.GetName(0, false)+"]", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			base.Cancel(playerCancel);
			if (m_owner is GamePlayer)
			{
				if (playerCancel)
					(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.EngageEffect.YouNoConcOnBlock"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				else
					(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.EngageEffect.YouNoAttemptToEngageT"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.IsEngaging = false;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				if (m_engageTarget != null)
					return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.EngageEffect.EngageName", m_engageTarget.GetName(0, false));
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.EngageEffect.Name");
			}
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 421; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(3);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.EngageEffect.InfoEffect"));
				delveInfoList.Add(" ");
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.EngageEffect.XEngagesY", m_owner.GetName(0, true), m_engageTarget.GetName(0, false)));
				return delveInfoList;
			}
		}
	}
}
