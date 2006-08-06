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
	[ServerProperty(ServerPropertyConstants.PVP_DAMAGE, "The PvP Damage Modifier - Edit this to change the amountof damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", "1.0")]
	public class PvPDamageServerProperty : AbstractServerProperty
	{
		public static volatile float Value;

		public PvPDamageServerProperty(ServerPropertyAttribute a)
			: base(a)
		{ }

		public override void Load()
		{
			base.Load();

			Value = float.Parse(m_property.Value);
		}
	}
}
