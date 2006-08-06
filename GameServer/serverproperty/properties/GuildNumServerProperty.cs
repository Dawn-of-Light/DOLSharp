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
	[ServerProperty(ServerPropertyConstants.GUILD_NUM, "Players Needed For Guild Form - Edit this to change the amount of players required to form a guild", "8")]
	public class GuildNumServerProperty : AbstractServerProperty
	{
		public static volatile byte Value;

		public GuildNumServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		public override void Load()
		{
			base.Load();

			Value = byte.Parse(m_property.Value);
		}
	}
}
