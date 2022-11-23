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

namespace GameServerScripts.Titles
{
	#region base class
	/// <summary>
	/// Generic Realm Rank Title Base Class
	/// </summary>
	public abstract class RealmGenericEventPlayerTitle : GenericEventPlayerTitle
	{
		/// <summary>
		/// Define Realm Level
		/// </summary>
		protected abstract int RRLevel { get; }
		
		/// <summary>
		/// Define Realm
		/// </summary>
		protected abstract eRealm Realm { get; }
		
		/// <summary>
		/// Tuple of String Description / Name / Female Description / Female Name 
		/// </summary>
		protected override Tuple<string, string, string, string> GenericNames
		{
			get
			{
				string realm = string.Empty;
				if (Realm == eRealm.Albion)
					realm = "Albion";
				else if (Realm == eRealm.Midgard)
					realm = "Midgard";
				else
					realm = "Hibernia";

				string male = string.Format("GamePlayer.RealmTitle.{0}.RR{1}.Male", realm, RRLevel);
				string female = string.Format("GamePlayer.RealmTitle.{0}.RR{1}.Female", realm, RRLevel);;
				return new Tuple<string, string, string, string>(null, male, null, female);
			}
		}
		
		/// <summary>
		/// Suitable Lamba Method
		/// </summary>
		protected override Func<GamePlayer, bool> SuitableMethod
		{
			get
			{
				return p => p.Realm == Realm && (p.RealmLevel / 10 + 1) == RRLevel;
			}
		}
		
		/// <summary>
		/// Should this Title go through Translator
		/// </summary>
		protected override bool Translate { get { return true; }}
		
		/// <summary>
		/// The event to hook.
		/// </summary>
		public override DOLEvent Event { get { return GamePlayerEvent.RRLevelUp; }}

		/// <summary>
		/// Get Description for this Title
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override string GetDescription(GamePlayer player)
		{
			return "Realm Rank";
		}
	}
	#endregion
	
	#region Albion
	
	public abstract class AlbionRealmGenericEventPlayerTitle : RealmGenericEventPlayerTitle
	{
		protected override eRealm Realm { get { return eRealm.Albion; }}
	}

	public class AlbionRR4Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 4; }}
	}
	public class AlbionRR5Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 5; }}
	}
	public class AlbionRR6Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 6; }}
	}
	public class AlbionRR7Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 7; }}
	}
	public class AlbionRR8Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 8; }}
	}
	public class AlbionRR9Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 9; }}
	}
	public class AlbionRR10Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 10; }}
	}
	public class AlbionRR11Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 11; }}
	}
	public class AlbionRR12Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 12; }}
	}
	public class AlbionRR13Title : AlbionRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 13; }}
	}
	#endregion
	
	#region Midgard
	public abstract class MidgardRealmGenericEventPlayerTitle : RealmGenericEventPlayerTitle
	{
		protected override eRealm Realm { get { return eRealm.Midgard; }}
	}

	public class MidgardRR4Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 4; }}
	}
	public class MidgardRR5Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 5; }}
	}
	public class MidgardRR6Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 6; }}
	}
	public class MidgardRR7Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 7; }}
	}
	public class MidgardRR8Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 8; }}
	}
	public class MidgardRR9Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 9; }}
	}
	public class MidgardRR10Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 10; }}
	}
	public class MidgardRR11Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 11; }}
	}
	public class MidgardRR12Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 12; }}
	}
	public class MidgardRR13Title : MidgardRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 13; }}
	}
	#endregion
	
	#region Hibernia
	public abstract class HiberniaRealmGenericEventPlayerTitle : RealmGenericEventPlayerTitle
	{
		protected override eRealm Realm { get { return eRealm.Hibernia; }}
	}

	public class HiberniaRR4Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 4; }}
	}
	public class HiberniaRR5Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 5; }}
	}
	public class HiberniaRR6Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 6; }}
	}
	public class HiberniaRR7Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 7; }}
	}
	public class HiberniaRR8Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 8; }}
	}
	public class HiberniaRR9Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 9; }}
	}
	public class HiberniaRR10Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 10; }}
	}
	public class HiberniaRR11Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 11; }}
	}
	public class HiberniaRR12Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 12; }}
	}
	public class HiberniaRR13Title : HiberniaRealmGenericEventPlayerTitle
	{
		protected override int RRLevel { get { return 13; }}
	}
	#endregion

}
