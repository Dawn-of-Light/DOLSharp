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
	[ServerProperty(ServerPropertyConstants.STARTING_MONEY, "Starting Money - Edit this to change the amount in copper of money new characters start the game with, max 214 plat", "0")]
	public class StartingMoneyServerProperty : AbstractServerProperty
	{
		public static volatile int Value;

		public StartingMoneyServerProperty(ServerPropertyAttribute a)
			: base(a)
		{
		}

		public override void Load()
		{
			base.Load();

			Value = int.Parse(m_property.Value);
		}
	}
}
