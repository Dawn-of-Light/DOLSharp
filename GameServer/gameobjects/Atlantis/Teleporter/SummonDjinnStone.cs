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
using log4net;
using System.Reflection;

namespace DOL.GS
{
	/// <summary>
	/// Djinn stone (summons ancient bound djinn).
	/// </summary>
	/// <author>Aredhel</author>
	public class SummonDjinnStone : DjinnStone
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Creates and summons the djinn if it isn't already up.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (Djinn == null)
			{
				try
				{
					Djinn = new SummonedDjinn(this);
				}
				catch (Exception e)
				{
					log.Warn(String.Format("Unable to create ancient bound djinn: {0}", e.Message));
					return false;
				}
			}

			if (!Djinn.IsSummoned)
				Djinn.Summon();

			return true;
		}
	}
}
