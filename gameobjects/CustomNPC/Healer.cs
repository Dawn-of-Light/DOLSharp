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
*/
/*				
            Written by Doulbousiouf (27/11/2004)
*/

using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game GameHealer NPC
	/// </summary>
	[NPCGuildScript("Healer")]
	public class GameHealer : GameNPC
	{
		private const string CURED_SPELL_TYPE = GlobalSpells.PvERessurectionIllnessSpellType;

		private const string COST_BY_PTS = "cost";

		/// <summary>
		/// Constructor
		/// </summary>
		public GameHealer()
			: base()
		{
		}

		#region Examine/Interact Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
        {
			IList list = new ArrayList();
            list.Add(LanguageMgr.GetTranslation(player.Client, "Healer.GetExamineMessages.Text1", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
            return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 5000);

			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(player, CURED_SPELL_TYPE);
			if (effect != null)
			{
				effect.Cancel(false);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.Interact.Text1", GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

			if (player.TotalConstitutionLostAtDeath > 0)
			{
				int oneConCost = GamePlayer.prcRestore[player.Level < GamePlayer.prcRestore.Length ? player.Level : GamePlayer.prcRestore.Length - 1];
				player.TempProperties.setProperty(COST_BY_PTS, (long)oneConCost);
                player.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client, "Healer.Interact.Text2", Money.GetString(player.TotalConstitutionLostAtDeath * (long)oneConCost)), new CustomDialogResponse(HealerDialogResponse));
            }
			else
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.Interact.Text3"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
			return true;
		}

		protected void HealerDialogResponse(GamePlayer player, byte response)
        {
            if (!this.IsWithinRadius(player, WorldMgr.INTERACT_DISTANCE))
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.HealerDialogResponse.Text1", GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (response != 0x01) //declined
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.HealerDialogResponse.Text2"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            long cost = player.TempProperties.getProperty<long>(COST_BY_PTS);
            player.TempProperties.removeProperty(COST_BY_PTS);
            int restorePoints = (int)Math.Min(player.TotalConstitutionLostAtDeath, player.GetCurrentMoney() / cost);
            if (restorePoints < 1)
                restorePoints = 1; // at least one
            long totalCost = restorePoints * cost;
            if (player.RemoveMoney(totalCost))
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.HealerDialogResponse.Text3", this.Name, Money.GetString(totalCost)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.TotalConstitutionLostAtDeath -= restorePoints;
                player.Out.SendCharStatsUpdate();
            }
            else
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Healer.HealerDialogResponse.Text4", Money.GetString(totalCost), restorePoints), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            return;
        }
		#endregion Examine/Interact Message
	}
}