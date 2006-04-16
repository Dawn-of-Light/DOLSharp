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
using System.Collections;
using System.Reflection;
using log4net;
using DOL.GS;
using DOL.Events;

namespace DOL.GS.SpawnGenerators
{
	/// <summary>
	/// The base classe for all spawn generator
	/// </summary>
	public abstract class SpawnTemplateBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The unique id of this SpawnTemplate
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Returns the unique ID of this spawn template
		/// </summary>
		public int SpawnTemplateBaseID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The mob template to spawn
		/// </summary>
		private GameMobTemplate m_mobTemplate;

		/// <summary>
		/// Gets or Sets the mob template to spawn
		/// </summary>
		public virtual GameMobTemplate MobTemplate
		{
			get { return m_mobTemplate; }
			set { m_mobTemplate = value; }
		}

		/// <summary>
		/// Get the list of all mob instances to create using the SpawnTemplate
		/// </summary>
		/// <returns>the list of GameMob</returns>
		public abstract IList GenerateMobInstances();
	}
}
