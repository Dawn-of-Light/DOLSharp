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
	/// <summary>
	/// "Veteran" title granted to all chars that play for at least 180 days.
	/// </summary>
	public class VeteranTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.GameEntered; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Time.Character.Veteran", "Titles.Time.Character.Veteran"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => DateTime.Now.Subtract(player.DBCharacter.CreationDate).TotalDays >= 178; }} // ~half year
	}
	
	/// <summary>
	/// "Elder" title granted to all chars on all accounts that created for at least one year.
	/// </summary>
	public class ElderTitle : TranslatedNoGenderGenericEventPlayerTitle
	{
		public override DOLEvent Event { get { return GamePlayerEvent.GameEntered; }}
		protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Time.Account.ElderTitle", "Titles.Time.Account.ElderTitle"); }}
		protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => DateTime.Now.Subtract(player.Client.Account.CreationDate).TotalDays >= 365; }} // a year
	}
}
