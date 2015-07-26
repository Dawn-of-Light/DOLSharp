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

using DOL.Events;

namespace GameServerScripts.Titles
{
	#region keep
	/// <summary>
	/// "Frontier Challenger" title granted to everyone who captured 10+ keeps.
	/// </summary>
	public class FrontierChallengerTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.CapturedKeepsChanged; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Claim.Frontier.FrontierChallenger", "Titles.Claim.Frontier.FrontierChallenger"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.CapturedKeeps >= 10 && player.CapturedKeeps < 50; }}
	}
	/// <summary>
	/// "Frontier Vindicator" title granted to everyone who captured 50+ keeps.
	/// </summary>
	public class FrontierVindicatorTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.CapturedKeepsChanged; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Claim.Frontier.FrontierVindicator", "Titles.Claim.Frontier.FrontierVindicator"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.CapturedKeeps >= 50 && player.CapturedKeeps < 500; }}
	}
	/// <summary>
	/// "Frontier Challenger" title granted to everyone who captured 10+ keeps.
	/// </summary>
	public class FrontierProtectorTitle : NoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.CapturedKeepsChanged; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Frontier Protector", "Frontier Protector"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.CapturedKeeps >= 500; }}
	}
	#endregion
	#region tower
	/// <summary>
	/// "Stronghold Soldier" title granted to everyone who captured 100+ towers.
	/// </summary>
	public class StrongholdSoldierTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.CapturedTowersChanged; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Claim.Stronghold.StrongholdSoldier", "Titles.Claim.Stronghold.StrongholdSoldier"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.CapturedKeeps >= 100 && player.CapturedKeeps < 1000; }}
	}
	/// <summary>
	/// "Stronghold Chief" title granted to everyone who captured 1000+ towers.
	/// </summary>
	public class StrongholdChiefTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.CapturedTowersChanged; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Claim.Stronghold.StrongholdChief", "Titles.Claim.Stronghold.StrongholdChief"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.CapturedKeeps >= 1000; }}
	}
	#endregion
}
