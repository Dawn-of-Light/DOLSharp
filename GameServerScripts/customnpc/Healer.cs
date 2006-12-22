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
//							Written by Doulbousiouf (27/11/2004)								//
//																					//
//																					//
//																					//


using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Represents an in-game GameHealer NPC
	/// </summary>
	[NPCGuildScript("Healer")]
	public class GameHealer : GameNPC
	{
		private const string CURRED_SPELL_TYPE = "PveResurrectionIllness";

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
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a healer.");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 5000);

			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(player, CURRED_SPELL_TYPE);
			if (effect != null)
			{
				effect.Cancel(false);
				player.Out.SendMessage(GetName(0, false) + " cure your resurrection sickness.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			if (player.TotalConstitutionLostAtDeath > 0)
			{
				int oneConCost = GamePlayer.prcRestore[player.Level < GamePlayer.prcRestore.Length ? player.Level : GamePlayer.prcRestore.Length - 1];
				player.TempProperties.setProperty(COST_BY_PTS, (long)oneConCost);
				player.Out.SendCustomDialog("It will cost " + Money.GetString(player.TotalConstitutionLostAtDeath * (long)oneConCost) + " to have your constitution restored. Do you accept?", new CustomDialogResponse(HealerDialogResponse));
			}
			else
			{
				player.Out.SendMessage("Your constitution is already fully restored!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return true;
		}

		protected void HealerDialogResponse(GamePlayer player, byte response)
		{
			if (!WorldMgr.CheckDistance(this, player, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to speak with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01) return; //declined

			long cost = player.TempProperties.getLongProperty(COST_BY_PTS, 0);
			player.TempProperties.removeProperty(COST_BY_PTS);
			int restorePoints = (int)Math.Min(player.TotalConstitutionLostAtDeath, player.GetCurrentMoney() / cost);
			if (restorePoints < 1)
				restorePoints = 1; // at least one
			long totalCost = restorePoints * cost;
			if (player.RemoveMoney(totalCost))
			{
				player.Out.SendMessage("You give " + player.TargetObject.Name + " a donation of " + Money.GetString(totalCost), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.TotalConstitutionLostAtDeath -= restorePoints;
				player.Out.SendCharStatsUpdate();
			}
			else
			{
				player.Out.SendMessage("Need " + Money.GetString(totalCost) + " to restore " + restorePoints + " constitution points.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return;
		}
		#endregion Examine/Interact Message
	}
}