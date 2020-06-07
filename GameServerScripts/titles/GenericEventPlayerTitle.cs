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

using DOL.GS;
using DOL.Events;
using DOL.GS.PlayerTitles;
using DOL.Language;

namespace GameServerScripts.Titles
{
	/// <summary>
	/// GenericEventPlayerTitle Allow to Implement easily custom event based player title
	/// </summary>
	public abstract class GenericEventPlayerTitle : EventPlayerTitle
	{
		/// <summary>
		/// Tuple of String Description / Name / Female Description / Female Name 
		/// </summary>
		protected abstract Tuple<string, string, string, string> GenericNames { get; }
		
		/// <summary>
		/// Suitable Lamba Method
		/// </summary>
		protected abstract Func<GamePlayer, bool> SuitableMethod { get; }
		
		/// <summary>
		/// Should this Title go through Translator
		/// </summary>
		protected abstract bool Translate { get; }
		
		/// <summary>
		/// Get Description for this Title
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override string GetDescription(GamePlayer player)
		{
			string description = GenericNames.Item1;
			
			if (player.Gender == eGender.Female && !string.IsNullOrEmpty(GenericNames.Item3))
				description = GenericNames.Item3;
			
			return Translate ? TryTranslate(description, player) : description;
		}
		
		/// <summary>
		/// Get Value for this Title
		/// </summary>
		/// <param name="source">The player looking.</param>
		/// <param name="player"></param>
		/// <returns></returns>
		public override string GetValue(GamePlayer source, GamePlayer player)
		{
			string titleValue = GenericNames.Item2;
			
			if (player.Gender == eGender.Female && !string.IsNullOrEmpty(GenericNames.Item4))
				titleValue = GenericNames.Item4;
						
			return Translate ? TryTranslate(titleValue, source) : titleValue;
		}
		
		protected static string TryTranslate(string value, GamePlayer source)
		{
			return LanguageMgr.TryTranslateOrDefault(source, string.Format("!{0}!", value), value);
		}
		
		/// <summary>
		/// Return True if this Title Suit the Targeted Player
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool IsSuitable(GamePlayer player)
		{
			if (SuitableMethod != null)
				return SuitableMethod(player);
			
			return false;
		}
	}

	public abstract class NoGenderGenericEventPlayerTitle : GenericEventPlayerTitle
	{
		/// <summary>
		/// Tuple of String Description / Name / Female Description / Female Name 
		/// </summary>
		protected override Tuple<string, string, string, string> GenericNames
		{
			get
			{
				return new Tuple<string, string, string, string>(DescriptionValue.Item1, DescriptionValue.Item2, null, null);
			}
		}
		
		/// <summary>
		/// Tuple of String Descrition / Name Value
		/// </summary>
		protected abstract Tuple<string, string> DescriptionValue { get; }
		
		/// <summary>
		/// Should this Title go through Translator
		/// </summary>
		protected override bool Translate { get { return false; }}
	}

	public abstract class TranslatedGenericEventPlayerTitle : GenericEventPlayerTitle
	{
		/// <summary>
		/// Should this Title go through Translator
		/// </summary>
		protected override bool Translate { get { return true; }}
	}
	
	public abstract class TranslatedNoGenderGenericEventPlayerTitle : NoGenderGenericEventPlayerTitle
	{
		/// <summary>
		/// Should this Title go through Translator
		/// </summary>
		protected override bool Translate { get { return true; }}
	}

}
