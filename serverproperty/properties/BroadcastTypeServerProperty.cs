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
	/// Class for the Broadcast Type Server Property
	/// </summary>
	[ServerProperty(ServerPropertyConstants.BROADCAST_TYPE, "Broadcast Type - Edit this to change what /b does, values 0 = disabled, 1 = area, 2 = visibility distance, 3 = zone, 4 = region, 5 = realm, 6 = server", "1")]
	public class BroadcastTypeServerProperty : AbstractServerProperty
	{
		/// <summary>
		/// The value of this property
		/// </summary>
		public static volatile byte Value;

		/// <summary>
		/// The type of broadcast
		/// </summary>
		public enum eBroadcastType : byte
		{ 
			Disabled = 0,
			Area = 1,
			Visible = 2,
			Zone = 3,
			Region = 4,
			Realm = 5,
			Server = 6,
		}

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="a">The property attribute</param>
		public BroadcastTypeServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		/// <summary>
		/// Load the property
		/// </summary>
		public override void Load()
		{
			base.Load();

			Value = byte.Parse(m_property.Value);
		}
	}
}
