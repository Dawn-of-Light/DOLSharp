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
using System.Reflection;
using System.Threading;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game merchant
	/// </summary>
	public class GameMerchant : GameMob, IGameMerchant
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region GetExamineMessages / Interact / Whisp

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(4);
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a merchant.");
			list.Add("[Right click to display a shop window]");
			return list;
		}

		/// <summary>
		/// Called when a player right clicks on the merchant
		/// </summary>
		/// <param name="player">Player that interacted with the merchant</param>
		/// <returns>True if succeeded</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			TurnTo(player, 10000);
			ThreadPool.QueueUserWorkItem(new WaitCallback(SendMerchantWindowCallback), player);
			return true;
		}

		/// <summary>
		/// Sends merchant window from threadpool thread
		/// </summary>
		/// <param name="state">The game player to send to</param>
		protected virtual void SendMerchantWindowCallback(object state)
		{
			((GamePlayer)state).Out.SendMerchantWindow(this);
		}

		/// <summary>
		/// Whispers a reply
		/// </summary>
		/// <param name="source">Source of the trade</param>
		/// <param name="str">Type of interaction requested</param>
		/// <returns>True if succeeded</returns>
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;
			if (source == null || !(source is GamePlayer))
				return false;
			GamePlayer t = (GamePlayer) source;
			switch (str)
			{
				case "trade":
					t.Out.SendMessage("You can buy the items I have available. I'm not really interested in your items, but I'll purchase them for an average price!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					break;
				default:
					break;
			}
			return true;
		}

		#endregion

		#region WindowsID/OnBuy/OnSell/OnApparaise

		/// <summary>
		/// Store the windows id of this merchant
		/// </summary>
		private int m_merchantWindowID;

		/// <summary>
		/// Gets or set the windows id of this merchant
		/// </summary>
		public int MerchantWindowID
		{
			get { return m_merchantWindowID; }
			set { m_merchantWindowID = value; }
		}

		/// <summary>
		/// Called when a player buys an item
		/// </summary>
		/// <param name="player">The player making the purchase</param>
		/// <param name="item_slot">slot of the item to be bought</param>
		/// <param name="number">Number to be bought</param>
		/// <returns>true if buying is allowed, false if buying should be prevented</returns>
		public virtual bool OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			return true; // buy from everybody
		}

		/// <summary>
		/// Called when a player sells something
		/// </summary>
		/// <param name="player">Player making the sale</param>
		/// <param name="item">The InventoryItem to be sold</param>
		/// <returns>true if selling is allowed, false if it should be prevented</returns>
		public virtual bool OnPlayerSell(GamePlayer player, GenericItem item)
		{
			return true; // sell to everybody
		}

		/// <summary>
		/// Called to appraise the value of an item
		/// </summary>
		/// <param name="player">The player whose item needs appraising</param>
		/// <param name="item">The item to be appraised</param>
		public virtual bool OnPlayerAppraise(GamePlayer player, GenericItem item)
		{
			return true; // apparaise to everybody
		}

		#endregion
	}
}