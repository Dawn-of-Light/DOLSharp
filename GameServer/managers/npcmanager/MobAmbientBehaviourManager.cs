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
using System.Linq;
using System.Collections.Generic;

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// MobAmbientBehaviourManager handles Mob Ambient Behaviour Lazy Loading
	/// </summary>
	public sealed class MobAmbientBehaviourManager
	{
		/// <summary>
		/// Sync Lock Object
		/// </summary>
		private readonly object LockObject = new object();
		
		/// <summary>
		/// Local Database Reference
		/// </summary>
		private IObjectDatabase Database { get; set; }
		
		/// <summary>
		/// Mob X Ambient Behaviour Cache indexed by Mob Name 
		/// </summary>
		private Dictionary<string, MobXAmbientBehaviour[]> AmbientBehaviour { get; set; }
		
		/// <summary>
		/// Retrieve MobXambiemtBehaviour Objects from Mob Name
		/// </summary>
		public MobXAmbientBehaviour[] this[string index]
		{
			get
			{
				if (string.IsNullOrEmpty(index))
					return new MobXAmbientBehaviour[0];
				
				var search = index.ToLower();
				
				MobXAmbientBehaviour[] matches;
				
				if (AmbientBehaviour.TryGetValue(search, out matches))
					return matches.Select(obj => obj.Clone()).Cast<MobXAmbientBehaviour>().ToArray();
				
				var records = Database.SelectObjects<MobXAmbientBehaviour>(string.Format("`Source` = '{0}'", GameServer.Database.Escape(search)));
				
				lock (LockObject)
				{
					if (!AmbientBehaviour.ContainsKey(search))
						AmbientBehaviour.Add(search, records.ToArray());
				}
				
				return records.Select(obj => obj.Clone()).Cast<MobXAmbientBehaviour>().ToArray();
			}
		}
		
		/// <summary>
		/// Create a new Instance of <see cref="MobAmbientBehaviourManager"/>
		/// </summary>
		public MobAmbientBehaviourManager(IObjectDatabase Database)
		{
			if (Database == null)
				throw new ArgumentNullException("Database");
			
			this.Database = Database;
			
			AmbientBehaviour = new Dictionary<string, MobXAmbientBehaviour[]>();
		}
	}
}
