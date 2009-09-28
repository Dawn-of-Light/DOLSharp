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
using DOL.GS.SkillHandler;
using DOL.Events;
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the berserk ability
	/// </summary>
	public class BerserkEffect : TimedEffect, IGameEffect
	{
		protected ushort m_startModel = 0;
		/// <summary>
		/// Creates a new berserk effect
		/// </summary>
		public BerserkEffect()
			: base(BerserkAbilityHandler.DURATION)
		{
		}

		/// <summary>
		/// Start the berserk on a living
		/// </summary>
		public override void Start(GameLiving living)
		{
			base.Start(living);
			m_startModel = living.Model;

			if(living is GamePlayer)
			{
				(living as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((living as GamePlayer).Client, "Effects.BerserkEffect.GoBerserkerFrenzy"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				living.Emote(eEmote.MidgardFrenzy);
				if((living as GamePlayer).Race == (int)eRace.Dwarf)
				{
					living.Model = 2032;
				}
				else
				{
					living.Model = 582;
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.Model = m_startModel;

			// there is no animation on end of the effect
			if(m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.BerserkEffect.BerserkerFrenzyEnds"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BerserkEffect.Name");
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BerserkEffect.InfoEffect"));

				int seconds = RemainingTime / 1000;
				if(seconds > 0)
				{
					delveInfoList.Add(" "); //empty line
					if(seconds > 60)
						delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.MinutesRemaining", (seconds / 60), ((seconds % 60).ToString("00"))));
					else
						delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.SecondsRemaining", seconds));
				}

				return delveInfoList;
			}
		}
	}
}
