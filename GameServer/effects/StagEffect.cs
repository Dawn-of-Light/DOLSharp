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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the stag ability
	/// </summary>
	public class StagEffect : TimedEffect, IGameEffect
	{
		/*
        1.42
        Hibernian Heroes have receieved a new ability - "Spirit of the Hunt".
        Whenever this ability is used, the Hero will shapeshift into a fearsome
        stag-headed Huntsman from Celtic Lore, and will receive bonus hit points.
        There are four different levels of of this ability: Initiate(15th level),
        Member(25th level), Leader(35th level), and Master(45th level).
        While in this form, the hero has increased hit points - +20% for the
        15th level ability up to +50% for the 45th level ability. The ability
        lasts for thirty seconds - at the end of this time, the hero's
        maximum hits will return to normal, but he keeps any hit point
        gain from the ability in his current hits (but he cannot exceed his
        pre-buffed maximum). The ability can be used once every 30 minutes
        played. Please note that there is only one Huntsman creature -
        male and female Heroes will both shapeshift into the same creature.
        */

		// some time after a lurikeen model was added for luri's

		/// <summary>
		/// The amount of max health gained
		/// </summary>
		protected int m_amount;

		protected ushort m_originalModel;

		protected int m_level;

		/// <summary>
		/// Creates a new stag effect
		/// </summary>
		public StagEffect(int level)
			: base(StagAbilityHandler.DURATION)
		{
			m_level = level;
		}

		/// <summary>
		/// Start the stag on player
		/// </summary>
		/// <param name="living">The living object the effect is being started on</param>
		public override void Start(GameLiving living)
		{
			base.Start(living);

			m_originalModel = living.Model;

			if (living is GamePlayer)
			{
				if ((living as GamePlayer).Race == (int)eRace.Lurikeen)
					living.Model = 859;
				else living.Model = 583;
			}			


			double m_amountPercent = (m_level + 0.5 + Util.RandomDouble()) / 10; //+-5% random
			if (living is GamePlayer)
				m_amount = (int)((living as GamePlayer).CalculateMaxHealth(living.Level, living.GetModified(eProperty.Constitution)) * m_amountPercent);
			else m_amount = (int)(living.MaxHealth * m_amountPercent);

			living.BaseBuffBonusCategory[(int)eProperty.MaxHealth] += m_amount;
			living.Health += (int)(living.GetModified(eProperty.MaxHealth) * m_amountPercent);
			if (living.Health > living.MaxHealth) living.Health = living.MaxHealth;

			living.Emote(eEmote.StagFrenzy);

			if (living is GamePlayer)
			{
				(living as GamePlayer).Out.SendUpdatePlayer();
				(living as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((living as GamePlayer).Client, "Effects.StagEffect.HuntsSpiritChannel"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.Model = m_originalModel;

			double m_amountPercent = m_amount / m_owner.GetModified(eProperty.MaxHealth);
			int playerHealthPercent = m_owner.HealthPercent;
			m_owner.BaseBuffBonusCategory[(int)eProperty.MaxHealth] -= m_amount;
			if (m_owner.IsAlive)
				m_owner.Health = (int)Math.Max(1, 0.01 * m_owner.MaxHealth * playerHealthPercent);

			if (m_owner is GamePlayer)
			{
				(m_owner as GamePlayer).Out.SendUpdatePlayer();
				// there is no animation on end of the effect
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.StagEffect.YourHuntsSpiritEnds"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.StagEffect.Name"); } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 480; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.StagEffect.InfoEffect"));

				int seconds = RemainingTime / 1000;
				if (seconds > 0)
				{
					delveInfoList.Add(" "); //empty line
					if (seconds > 60)
						delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.MinutesRemaining", (seconds / 60), (seconds % 60).ToString("00")));
					else
						delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.SecondsRemaining", seconds));
				}

				return delveInfoList;
			}
		}
	}
}
