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

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// Class for the Alliance Max Server Property
	/// </summary>
	[ServerProperty(ServerPropertyConstants.ALLIANCE_MAX, "Max Guilds In Alliance - Edit this to change the maximum number of guilds in an alliance -1 = unlimited, 0=disable alliances", "-1")]
	public class AllianceMaxServerProperty : AbstractServerProperty
	{
		/// <summary>
		/// The value of this property
		/// </summary>
		public static volatile int Value;

		/// <summary>
		/// The value for alliances disabled
		/// </summary>
		private const int DISABLE_ALLIANCES = 0;
		/// <summary>
		/// The value for unlimited guilds
		/// </summary>
		private const int UNLIMITED_GUILDS = -1;

		/// <summary>
		/// Are alliances disabled
		/// </summary>
		public static bool AlliancesDisabled
		{
			get { return Value == DISABLE_ALLIANCES; }
		}

		/// <summary>
		/// Are unlimited guilds allowed in the alliance
		/// </summary>
		public static bool UnlimitedGuilds
		{
			get { return Value == UNLIMITED_GUILDS; }
		}

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="a">The property attribute</param>
		public AllianceMaxServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		/// <summary>
		/// Load the property
		/// </summary>
		public override void Load()
		{
			base.Load();

			Value = int.Parse(m_property.Value);
		}
	}
}
