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
	public abstract class SpawnGeneratorBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The unique id of this SpawnGenerator
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Returns the unique ID of this spawn generator
		/// </summary>
		public int SpawnGeneratorBaseID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The timer used to check for player proximity
		/// </summary>
		private RegionTimer m_spawnGeneratorTimer;

		/// <summary>
		/// The region of this spawn generator
		/// </summary>
		protected Region m_region;

		/// <summary>
		/// Holds all the spawn template of this generator
		/// </summary>
		protected Iesi.Collections.ISet m_spawnTemplates;

		/// <summary>
		/// Gets or sets the list of all the spawn template of this generator
		/// </summary>
		public Iesi.Collections.ISet SpawnTemplates
		{
			get
			{
				if(m_spawnTemplates == null) m_spawnTemplates = new Iesi.Collections.HybridSet();
				return m_spawnTemplates;
			}
			set	{ m_spawnTemplates = value; }
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public virtual Region Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		/// <summary>
		/// The interval at which the generator will fire
		/// </summary>
		public virtual int CheckInterval
		{
			get { return 5000; } // 5 sec
		}

		/// <summary>
		/// Returns weather this brain is active or not
		/// </summary>
		public virtual bool IsActive
		{
			get { return m_spawnGeneratorTimer != null && m_spawnGeneratorTimer.IsAlive; }
		}

		/// <summary>
		/// Holds all the mob instance created by this spawn generator
		/// </summary>
		protected IList m_generatedMobs = new ArrayList();

		/// <summary>
		/// Starts the spawn generator
		/// </summary>
		/// <returns>true if started</returns>
		public virtual bool Start()
		{
			if(SpawnTemplates.Count <= 0) return false;

			lock(this)
			{
				if(IsActive) return false;

				foreach(SpawnTemplateBase spawnTemplate in SpawnTemplates)
				{
					foreach(GameMob mob in spawnTemplate.GenerateMobInstances())
					{
						m_generatedMobs.Add(mob);

						mob.Region = Region;
						mob.Position = GetRandomLocation();
						mob.Heading = Util.Random(4096);

						mob.AddToWorld();
					}
				}

				if(m_generatedMobs.Count <= 0) return false;

				m_spawnGeneratorTimer = new RegionTimer(Region.TimeManager);
				m_spawnGeneratorTimer.Callback = new RegionTimerCallback(SpawnGeneratorTimerCallback);
				m_spawnGeneratorTimer.Start(CheckInterval);
			}
			return true;
		}

		/// <summary>
		/// Stops the spawn generator
		/// </summary>
		/// <returns>true if stopped</returns>
		public virtual bool Stop()
		{
			lock(this)
			{
				if(!IsActive) return false;

				foreach(GameMob mob in m_generatedMobs)
				{
					mob.RemoveFromWorld();
				}
				m_generatedMobs.Clear();

				m_spawnGeneratorTimer.Stop();
				m_spawnGeneratorTimer = null;
			}
			return true;
		}

		/// <summary>
		/// The callback timer for the generator ticks
		/// </summary>
		/// <param name="callingTimer">the calling timer</param>
		/// <returns>the new tick intervall</returns>
		protected virtual int SpawnGeneratorTimerCallback(RegionTimer callingTimer)
		{
			foreach(GameMob mob in m_generatedMobs)
			{
				 if(!mob.InCombat && !mob.IsMoving && Util.Chance(20))
				 {
					 mob.WalkTo(GetRandomLocation(), mob.MaxSpeedBase / 2);
				 }
			}

			return CheckInterval;
		}
		
		/// <summary>
		/// Get a random loc inside this spawn generator area
		/// </summary>
		/// <returns>the rand point</returns>
		public abstract Point GetRandomLocation();

		#region NHibernate Fix
		/// <summary>
		/// In the db we save the regionID but we must
		/// link the object with the region instance
		/// stored in the regionMgr
		///(it is only used internaly by NHibernate)
		/// </summary>
		/// <value>The region id</value>
		private int RegionID
		{
			get { return m_region.RegionID; }
			set { m_region = WorldMgr.GetRegion(value); }
		}
		#endregion
	}
}
