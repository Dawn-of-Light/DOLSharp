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
	/// The Claim Num Server Property
	/// </summary>
	[ServerProperty(ServerPropertyConstants.CLAIM_NUM, "Players Needed For Claim - Edit this to change the amount of players required to claim a keep, towers are half this amount", "8")]
	public class ClaimNumServerProperty : AbstractServerProperty
	{
		/// <summary>
		/// The value of this property
		/// </summary>
		public static volatile byte Value;

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="a">The server property attribute</param>
		public ClaimNumServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		/// <summary>
		/// Load this property
		/// </summary>
		public override void Load()
		{
			base.Load();

			Value = byte.Parse(m_property.Value);
		}
	}
}
