/* HACK: Convert to EventPlayerTitle!
 */

using System;
using DOL.Language;
using DOL.Events;
using DOL.GS.PlayerTitles;

namespace DOL.GS.Scripts
{
	public class RealmRank : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player)
		{
			return player.RealmRankTitle(player.Client.Account.Language);
		}
		public override string GetValue(GamePlayer player)
		{
			return player.RealmRankTitle(player.Client.Account.Language);
		}
		public override string GetValue(GamePlayer source, GamePlayer target)
		{
			return target.RealmRankTitle(source.Client.Account.Language);
		}
		public override bool IsSuitable(GamePlayer player)
		{
			return true;
		}
	}
}