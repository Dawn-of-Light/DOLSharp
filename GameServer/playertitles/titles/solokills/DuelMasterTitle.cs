/*
 * Suncheck: [19.06.2007]
 *   - Corrected
 *   - Sorted
 *   - Added missing (+language support)
 */

using System;
using DOL.Language;
using DOL.Events;
using DOL.GS.PlayerTitles;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Example...
	/// </summary>
	public class DuelMasterTitle : EventPlayerTitle
	{
		/// <summary>
		/// The title description, shown in "Titles" window.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title description.</returns>
		public override string GetDescription(GamePlayer player)
		{
			return LanguageMgr.GetTranslation(player.Client, "Titles.Solokills.DuelMaster");
		}

		/// <summary>
		/// The title value, shown over player's head.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title value.</returns>
		public override string GetValue(GamePlayer player)
		{
			return LanguageMgr.GetTranslation(player.Client, "Titles.Solokills.DuelMaster");
		}

		/// <summary>
		/// The event to hook.
		/// </summary>
		public override DOLEvent Event
		{
			get { return GamePlayerEvent.KillsTotalSoloChanged; }
		}
		/// <summary>
		/// Verify whether the player is suitable for this title.
		/// </summary>
		/// <param name="player">The player to check.</param>
		/// <returns>true if the player is suitable for this title.</returns>
		public override bool IsSuitable(GamePlayer player)
		{
			//25000+
			return (player.KillsAlbionSolo + player.KillsHiberniaSolo + player.KillsMidgardSolo) >= 25000;
		}
		
		/// <summary>
		/// The event callback.
		/// </summary>
		/// <param name="e">The event fired.</param>
		/// <param name="sender">The event sender.</param>
		/// <param name="arguments">The event arguments.</param>
		protected override void EventCallback(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer p = sender as GamePlayer;
			if (p != null && p.Titles.Contains(this))
			{
				p.UpdateCurrentTitle();
				return;
			}
			base.EventCallback(e, sender, arguments);
		}
	}
}
