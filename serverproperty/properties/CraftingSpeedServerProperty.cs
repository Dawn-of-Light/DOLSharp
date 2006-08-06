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
	/// The class for the crafting speed server property
	/// </summary>
	[ServerProperty(ServerPropertyConstants.CRAFTING_SPEED, "Crafting Speed Modifier - Edit this to change the speed at which you craft e.g 1.5 is 50% faster 2.0 is twice as fast (100%) 0.5 is half the speed (50%)", "1.0")]
	public class CraftingSpeedServerProperty : AbstractServerProperty
	{
		/// <summary>
		/// The value of this property
		/// </summary>
		public static volatile float Value;

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="a">The server property attribute</param>
		public CraftingSpeedServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		/// <summary>
		/// Load this property
		/// </summary>
		public override void Load()
		{
			base.Load();

			Value = float.Parse(m_property.Value);
		}
	}
}
