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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Keeps;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// LosCheckMgr is a class designed to handle LoS Checks and cache them.
	/// If all in-game LoS check go through this class they'll obey serverproperties rules.
	/// </summary>
	public class LosCheckMgr
	{
		/// <summary>
		/// For Debug purposes
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		#region members
		
		/// <summary>
		/// Dictionary to keep players and last CheckLos Ticks.
		/// </summary>
		private readonly Dictionary<GamePlayer, long> m_clientChecks = new Dictionary<GamePlayer, long>();
		
		/// <summary>
		/// Dictionary to keep players and last CheckLos Ticks.
		/// </summary>
		private Dictionary<GamePlayer, long> ClientChecks
		{
			get
			{
				return m_clientChecks;
			}
		}

		/// <summary>
		/// Dictionary to keep clients LoS stats Tuple(Number of checks, average reply time, last reply time)
		/// </summary>
		private readonly Dictionary<GamePlayer, Tuple<int, int, int>> m_clientStats = new Dictionary<GamePlayer, Tuple<int, int, int>>();

		/// <summary>
		/// Dictionary to keep clients LoS stats Tuple(Number of checks, average reply time, last reply time)
		/// </summary>
		private Dictionary<GamePlayer, Tuple<int, int, int>> ClientStats
		{
			get
			{
				return m_clientStats;
			}
		}		
		
		/// <summary>
		/// Dictionary to keep Target to Target LoSChecks and cache them with updated time.
		/// </summary>
		private readonly Dictionary<Tuple<GameObject, GameObject>, Tuple<long, bool>> m_responsesCache = new Dictionary<Tuple<GameObject, GameObject>, Tuple<long, bool>>();
		
		/// <summary>
		/// Dictionary to keep Target to Target LoSChecks and cache them with updated time.
		/// </summary>
		private Dictionary<Tuple<GameObject, GameObject>, Tuple<long, bool>> ResponsesCache
		{
			get
			{
				return m_responsesCache;
			}
		}
		
		/// <summary>
		/// Dictionary to keep Pending LoSChecks
		/// </summary>
		private readonly Dictionary<Tuple<GamePlayer, ushort, ushort>, long> m_pendingChecks = new Dictionary<Tuple<GamePlayer, ushort, ushort>, long>();
		
		/// <summary>
		/// Dictionary to keep Pending LoSChecks
		/// </summary>
		private Dictionary<Tuple<GamePlayer, ushort, ushort>, long> PendingChecks  
		{
			get
			{
				return m_pendingChecks;
			}
		}
		
		/// <summary>
		/// Dictionary holding a list of object needing Notify
		/// </summary>
		private Dictionary<Tuple<GameObject, GameObject>, List<IDOLEventHandler>> m_registeredLosEvents = new Dictionary<Tuple<GameObject, GameObject>, List<IDOLEventHandler>>();
		
		/// <summary>
		/// Dictionary holding a list of object needing Notify
		/// </summary>
		private Dictionary<Tuple<GameObject, GameObject>, List<IDOLEventHandler>> RegisteredLosEvents
		{
			get
			{
				return m_registeredLosEvents;
			}
		}
		
		/// <summary>
		/// Timer for cleanup routines
		/// </summary>
		private System.Timers.Timer m_timerCleanup;
		
		/// <summary>
		/// Timer for pendings routines
		/// </summary>
		private System.Timers.Timer m_timerPending;
		
		#endregion
		
		#region params
		
		/// <summary>
		/// Default LoS Check Timeout in ms
		/// </summary>
		public static int LOSMGR_QUERY_TIMEOUT
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_QUERY_TIMEOUT;
			}
		}
		
		/// <summary>
		/// Default PvP LoS Cache Timeout in ms
		/// </summary>		
		public static int LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT;
			}
		}
		
		/// <summary>
		/// Default PvE LoS Cache Timeout in ms
		/// </summary>		
		public static int LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT;
			}
		}
		
		/// <summary>
		/// Default EvE LoS Cache Timeout in ms
		/// </summary>		
		public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT 
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT;
			}
		}
		
		/// <summary>
		/// Player Los Frequency in ms 
		/// </summary>				
		public static int LOSMGR_PLAYER_CHECK_FREQUENCY
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_CHECK_FREQUENCY;
			}
		}

		/// <summary>
		/// PvP Los range Threshold (should be short)
		/// </summary>
		public static int LOSMGR_PLAYER_VS_PLAYER_RANGE_THRESHOLD
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_VS_PLAYER_RANGE_THRESHOLD;
			}
		}

		/// <summary>
		/// PvE Los range Threshold
		/// </summary>
		public static int LOSMGR_PLAYER_VS_ENVIRONMENT_RANGE_THRESHOLD
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_VS_ENVIRONMENT_RANGE_THRESHOLD;
			}
		}

		/// <summary>
		/// EvE Los range Threshold should be largest
		/// </summary>
		public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_RANGE_THRESHOLD
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_RANGE_THRESHOLD;
			}
		}
		
		/// <summary>
		/// Contamination Radius used for EvE and max range checks.
		/// </summary>
		public static int LOSMGR_MAX_CONTAMINATION_RADIUS
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_MAX_CONTAMINATION_RADIUS;
			}
		}
		
		/// <summary>
		/// Contamination radius in which NPC receive LoS Checks (should be under MAX)
		/// </summary>
		public static int LOSMGR_NPC_CONTAMINATION_RADIUS
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_NPC_CONTAMINATION_RADIUS;
			}
		}
		
		/// <summary>
		/// Contamination radius in which Player Pets receive LoS Checks
		/// </summary>
		public static int LOSMGR_PET_CONTAMINATION_RADIUS
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PET_CONTAMINATION_RADIUS;
			}
		}
		
		/// <summary>
		/// Contamination radius in which Players receive LoS Checks
		/// </summary>
		public static int LOSMGR_PLAYER_CONTAMINATION_RADIUS
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_PLAYER_CONTAMINATION_RADIUS;
			}
		}
		
		/// <summary>
		/// Contamination radius in which Guard receive LoS Checks (should be under npc)
		/// </summary>
		public static int LOSMGR_GUARD_CONTAMINATION_RADIUS
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_GUARD_CONTAMINATION_RADIUS;
			}
		}
		
		/// <summary>
		/// Contamination radius Z-factor
		/// </summary>
		public static double LOSMGR_CONTAMINATION_ZFACTOR
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_CONTAMINATION_ZFACTOR;
			}
		}

				/// <summary>
		/// Max Cleanup Entries, to prevent locking the Dictionaries for too long
		/// </summary>				
		public static int MAX_CLEANUP_ENTRIES
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_CLEANUP_ENTRIES;
			}
		}
		
		/// <summary>
		/// Cleanup Frequency in ms, too long cleanup can be intensive and won't clean all up, too fast and it could clean useful data
		/// </summary>				
		public static int LOSMGR_CLEANUP_FREQUENCY
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_CLEANUP_FREQUENCY;
			}
		}

		/// <summary>
		/// Get the Debug level of this Los Check Manager
		/// </summary>
		public static int LOSMGR_DEBUG_LEVEL
		{
			get
			{
				return ServerProperties.Properties.LOSMGR_DEBUG_LEVEL;
			}
		}
		
		/// <summary>
		/// Info level Debug display
		/// </summary>
		public const ushort LOSMGR_DEBUG_INFO = 1;
		/// <summary>
		/// Warn level Debug display
		/// </summary>
		public const ushort LOSMGR_DEBUG_WARN = 2;
		/// <summary>
		/// Debug level Debug display
		/// </summary>
		public const ushort LOSMGR_DEBUG_DEBUG = 3;
		
		#endregion
		
		#region constructor
		
		public LosCheckMgr()
		{
			int rndRange = LOSMGR_CLEANUP_FREQUENCY >> 3;
			m_timerCleanup = new System.Timers.Timer(Util.Random(LOSMGR_CLEANUP_FREQUENCY-rndRange, LOSMGR_CLEANUP_FREQUENCY+rndRange));
			m_timerCleanup.AutoReset = true;
			m_timerCleanup.Elapsed += new System.Timers.ElapsedEventHandler(CleanUp);
			m_timerCleanup.Start();

			if(LOSMGR_QUERY_TIMEOUT > 0)
			{
				m_timerPending = new System.Timers.Timer(LOSMGR_QUERY_TIMEOUT);
				m_timerPending.AutoReset = true;
				m_timerPending.Elapsed += new System.Timers.ElapsedEventHandler(PendingLosCheck);
				m_timerPending.Start();
			}
		}
		
		/// <summary>
		/// Stop Timer on object dispose.
		/// </summary>
		~LosCheckMgr()
		{
			if(m_timerCleanup != null) 
			{
				m_timerCleanup.Stop();
				m_timerCleanup.Close();
			}
			
			if(m_timerPending != null) 
			{
				m_timerPending.Stop();
				m_timerPending.Close();
			}
		}
		
		#endregion
		
		#region non-blocking LoS Checker
	

		/// <summary>
		/// Check LoS and wait for a reply before returning
		/// </summary>
		/// <param name="player">Client used to make the LoS check</param>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <param name="notifier">GameObject to Notify when Check is made</param>
		/// <param name="cached">Use a cached result</param>
		/// <param name="timeout">Cache Timeout, 0 = default</param>
		public void LosCheck(GamePlayer player, GameObject source, GameObject target, LosMgrResponse notifier, bool cached = true , int timeout = 0)
		{
			LosCheck(player, source, target, new LosMgrResponseHandler(notifier), cached, timeout);
		}
		
		/// <summary>
		/// Check LoS and wait for a reply before returning
		/// </summary>
		/// <param name="player">Client used to make the LoS check</param>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <param name="notifier">GameObject to Notify when Check is made</param>
		/// <param name="cached">Use a cached result</param>
		/// <param name="timeout">Cache Timeout, 0 = default</param>
		public void LosCheck(GamePlayer player, GameObject source, GameObject target, IDOLEventHandler notifier, bool cached = true, int timeout = 0)
		{			
			if(player == null || source == null || target == null)
				throw new LosUnavailableException();
			
			// FIXME debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
				log.Warn("LOSMGR_W : Starting Los Check with - Player : "+player.Name+" Source : "+source.Name+" Target : "+target.Name+".");

			if(timeout <= 0)
				timeout = GetDefaultTimeouts(source, target);
			
			// check Threshold first
			if(source.IsWithinRadius(target, GetDefaultThreshold(source, target), false))
			{
				// FIXME debug
				if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
					log.Warn("LOSMGR_D : Threshold hitted ("+GetDefaultThreshold(source, target)+") with - Player : "+player.Name+" Source : "+source.Name+" Target : "+target.Name+".");

				notifier.Notify(GameObjectEvent.FinishedLosCheck, player, new LosCheckData(source, target, GameTimer.GetTickCount(), true));
				return;
			}
			
			// check in cache then !
			if(cached)
			{	
				try
				{
					bool los = GetLosCheckFromCache(source, target, timeout);
					notifier.Notify(GameObjectEvent.FinishedLosCheck, player, new LosCheckData(source, target, GameTimer.GetTickCount(), los));
					return;
					
				}
				catch (LosUnavailableException)
				{
					// we have no cache
				}
			}

			// Check if a LoS is pending, or this player is already LoS Checking...
			Tuple<GameObject, GameObject> cacheKey = new Tuple<GameObject, GameObject>(source, target);
						
			// We need to lock the Pending list during the checks
			lock(((ICollection)PendingChecks).SyncRoot)
			{
				IEnumerable<Tuple<GamePlayer, ushort, ushort>> pendings = (from pending in PendingChecks where (pending.Key.Item2 == source.ObjectID && pending.Key.Item3 == target.ObjectID) || (pending.Key.Item2 == target.ObjectID && pending.Key.Item3 == source.ObjectID) select pending.Key).Take(1);

				// We have pending data not too old, Register this event handler
				foreach(Tuple<GamePlayer, ushort, ushort> pend in pendings)
				{
					// FIXME debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
						log.Warn("LOSMGR_D : Registered to an other LoS with - Player : "+player.Name+" Source : "+source.Name+" Target : "+target.Name+".");

					AddRegisteredEvent(cacheKey, notifier);
					return;
				}
			}
			
			// Throttle
			if(IsPvpLosCheck(source, target) || IsAvailableForLosCheck(player, source, target)) {
				// Not pending, Not in Cache, let's work
				AddRegisteredEvent(cacheKey, notifier);
				EventNotifierLosCheck(player, source, target);
			}
			else
			{
				// Get best checker !
				GamePlayer checker = GetBestLosChecker(source, target);
				
				if(checker != null) 
				{
					// FIXME Debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
						log.Warn("LOSMGR_D : Deferred LoSCheck to "+checker.Name+" From - Player : "+player.Name+" Source : "+source.Name+" Target : "+target.Name+".");

					AddRegisteredEvent(cacheKey, notifier);
					EventNotifierLosCheck(checker, source, target);
				}
				else
				{
					// the Player checker is unavailable to make this LosCheck
					throw new LosUnavailableException();
				}
			}
		}
		
		/// <summary>
		/// LoscheckVincinity can be used when source player isn't available to check Los
		/// </summary>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <param name="notifier">GameObject to Notify when Check is made</param>
		/// <param name="cached">Use a cached result</param>
		/// <param name="timeout">Cache Timeout, 0 = default</param>
		public void LosCheckVincinity(GameObject source, GameObject target, LosMgrResponse notifier, bool cached = true, int timeout = 0) 
		{
			LosCheckVincinity(source, target, new LosMgrResponseHandler(notifier), true, 0);
		}
		
		/// <summary>
		/// LoscheckVincinity can be used when source player isn't available to check Los
		/// </summary>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <param name="notifier">GameObject to Notify when Check is made</param>
		/// <param name="cached">Use a cached result</param>
		/// <param name="timeout">Cache Timeout, 0 = default</param>
		public void LosCheckVincinity(GameObject source, GameObject target, IDOLEventHandler notifier, bool cached = true, int timeout = 0) 
		{			
			// FIXME debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
				log.Warn("LOSMGR_W : Starting Vincinity Los Check Between - Source : "+source.Name+" Target : "+target.Name+".");
			
			if(timeout == 0)
				timeout = GetDefaultTimeouts(source, target);
			
			// check Threshold first
			if(source.IsWithinRadius(target, GetDefaultThreshold(source, target), false))
			{
				//we need an arbitrary player
				foreach(GamePlayer player in source.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if(player.ObjectState == GameNPC.eObjectState.Active) 
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Vincinity Hitted Treshold ("+GetDefaultThreshold(source, target)+") - Source : "+source.Name+" Target : "+target.Name+".");
						
						notifier.Notify(GameObjectEvent.FinishedLosCheck, player, new LosCheckData(source, target, GameTimer.GetTickCount(), true));
						return;
					}
				}
			}
			
			// check cache then !
			if(cached)
			{			
				try
				{
					bool los = GetLosCheckFromCache(source, target, timeout);
					//we need an arbitrary player
					foreach(GamePlayer player in source.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if(player.ObjectState == GameNPC.eObjectState.Active) 
						{
							notifier.Notify(GameObjectEvent.FinishedLosCheck, player, new LosCheckData(source, target, GameTimer.GetTickCount(), los));
							return;
						}
						
					}
				}
				catch (LosUnavailableException)
				{
					// we have no cache
				}
			}
			
			// check pending
			lock(((ICollection)PendingChecks).SyncRoot)
			{
				IEnumerable<Tuple<GamePlayer, ushort, ushort>> pendings = (from pending in PendingChecks where (pending.Key.Item2 == source.ObjectID && pending.Key.Item3 == target.ObjectID) || (pending.Key.Item2 == target.ObjectID && pending.Key.Item3 == source.ObjectID) select pending.Key).Take(1);
				foreach(Tuple<GamePlayer, ushort, ushort> pend in pendings)
				{
					// FIXME debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
						log.Warn("LOSMGR_I : Vincinity Registered to an other LoS - Source : "+source.Name+" Target : "+target.Name+".");
					
					AddRegisteredEvent(new Tuple<GameObject, GameObject>(source, target), notifier);
					return;
				}
			}
						
			// check if source is player and available for Los check
			if(IsAvailableForLosCheck(source, source, target))
			{
				LosCheck((GamePlayer)source, source, target, notifier, cached, timeout);
				return;
			}
			
			// check if target is player and available for Los check
			if(IsAvailableForLosCheck(target, source, target))
			{
				LosCheck((GamePlayer)target, source, target, notifier, cached, timeout);
				return;
			}
			
			// check if source has an available owner
			if(source is GameNPC && ((GameNPC)source).Brain != null && ((GameNPC)source).Brain is IControlledBrain)
			{
				GamePlayer owner = ((IControlledBrain)((GameNPC)source).Brain).GetPlayerOwner();
				if(owner != null && IsAvailableForLosCheck(owner, source, target)) {
					LosCheck(owner, source, target, notifier, cached, timeout);
					return;
				}	
			}
			
			// check if target has an available owner
			if(target is GameNPC && ((GameNPC)target).Brain != null && ((GameNPC)target).Brain is IControlledBrain)
			{
				GamePlayer tgtowner = ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner();
				if(tgtowner != null && IsAvailableForLosCheck(tgtowner, source, target)) {
					LosCheck(tgtowner, source, target, notifier, cached, timeout);
					return;
				}	
			}
			
			GamePlayer checker = GetBestLosChecker(source, target);
			
			if(checker != null)
			{
				// FIXME debug
				if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
					log.Warn("LOSMGR_I : Vincinity found best checker "+checker.Name+" - Source : "+source.Name+" Target : "+target.Name+".");
				
				LosCheck(checker, source, target, notifier, cached, timeout);
				return;
			}
			
			throw new LosUnavailableException();
		}
		
		/// <summary>
		/// This must be called after all checks have been done
		/// </summary>
		/// <param name="player">Player to use for Los check</param>
		/// <param name="source">Source for Los check</param>
		/// <param name="target">Target of Los check</param>
		/// <param name="notifier">The object notified</param>
		private void EventNotifierLosCheck(GamePlayer player, GameObject source, GameObject target)
		{
			// FIXME debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
				log.Warn("LOSMGR_W : LoS Sent - Player : "+player.Name+", Source : "+source.Name+", Target : "+target.Name+".");
			
			// pending key to store this lookup
			Tuple<GamePlayer, ushort, ushort> checkerKey = new Tuple<GamePlayer, ushort, ushort>(player, (ushort)source.ObjectID, (ushort)target.ObjectID);
			
			long time = GameTimer.GetTickCount();
			
			// insert into pendings
			lock(((ICollection)PendingChecks).SyncRoot)
			{
				PendingChecks[checkerKey] = time;
				ClientChecks[player] = time;
			}

			// Everything start here
			player.Out.SendCheckLOS(source, target, new CheckLOSMgrResponse(LosResponseHandler));
			
			// Then wait for notify...
		}
		
		#endregion

		#region response handler
		
		/// <summary>
		/// LoS Responses are handled through this method
		/// </summary>
		/// <param name="player">Player replying</param>
		/// <param name="response">Client response</param>
		/// <param name="targetOID">Target OID to which the Check was made</param>
		private void LosResponseHandler(GamePlayer player, ushort response, ushort sourceOID, ushort targetOID) 
		{
			if(player == null || sourceOID == 0 || targetOID == 0)
				return;
			
			// get time
			long sent = GameTimer.GetTickCount();
			long time = sent;
			
			// Check result
			bool losOK = (response & 0x100) == 0x100;
			
			// key for pending
			Tuple<GamePlayer, ushort, ushort> checkerKey = new Tuple<GamePlayer, ushort, ushort>(player, sourceOID, targetOID);			
			
			// Object from OID
			GameObject source = player.CurrentRegion.GetObject(sourceOID);
			GameObject target = player.CurrentRegion.GetObject(targetOID);
			
			// FIXME remove debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
				log.Warn("LOSMGR_W : Los Check Finished - Player : "+player.Name+" Source : "+(source != null ? source.Name : "null")+"("+sourceOID+") Target : "+(target != null ? target.Name : "null")+"("+targetOID+") LoS : " + (losOK ? "OK" : "KO"));
			
			// we have a response, it should be in pending
			lock(((ICollection)PendingChecks).SyncRoot)
			{			
				// We have some data into pending cache
				if(PendingChecks.ContainsKey(checkerKey)) 
				{
					//We should remove it
					sent = PendingChecks[checkerKey];
					PendingChecks.Remove(checkerKey);
					// Update Client Stats

					if(ClientStats.ContainsKey(player)) 
					{
						long total = ClientStats[player].Item1 * ClientStats[player].Item2;
						total += (time - sent);
						int count = ClientStats[player].Item1+1;
						ClientStats[player] = new Tuple<int, int, int>(count, (int)total/count, (int)(time - sent));
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Clients Stats Update : "+player.Name+", count : "+count+", average : "+((int)total/count)+", instant : "+((int)(time - sent)));
					}
					else
					{
						ClientStats[player] = new Tuple<int, int, int>(1, (int)(time - sent), (int)(time - sent));
					}
			
				}
				else
				{
					//No Pending Object, it's a wild one !
					// FIXME Display debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
						log.Warn("LOSMGR_I : Los Check Without pending objects - Player : "+player.Name+" Source : "+(source != null ? source.Name : "null")+"("+sourceOID+") Target : "+(target != null ? target.Name : "null")+"("+targetOID+") LoS : " + (losOK ? "OK" : "KO") + " !");
				}
			}
			
			// Update all caches
			if(source != null && target != null) 
			{
				int timeout = GetDefaultTimeouts(source, target);
				lock(((ICollection)ResponsesCache).SyncRoot)
				{
					UpdateVincinityLosCache(source, target, losOK, time);
					UpdateVincinityLosCache(target, source, losOK, time);
					UpdateLosCacheItem(source, target, losOK, timeout, time);
				}
			}
			
			// key for registered
			Tuple<GameObject, GameObject> cachekey = new Tuple<GameObject, GameObject>(source, target);
			Tuple<GameObject, GameObject> rcachekey = new Tuple<GameObject, GameObject>(target, source);
			
			// We should Notify objects asking for this Check or the reverse one.
			lock(((ICollection)RegisteredLosEvents).SyncRoot)
			{
				LosCheckData losData = new LosCheckData(source, target, sent, losOK);
				if(RegisteredLosEvents.ContainsKey(cachekey))
				{
					NotifyObjects(RegisteredLosEvents[cachekey], player, losData);
				}
				if(RegisteredLosEvents.ContainsKey(rcachekey))
				{
					NotifyObjects(RegisteredLosEvents[rcachekey], player, losData);
				}
			}		
		}
		
		/// <summary>
		/// Distribute Events to Registered objects
		/// </summary>
		/// <param name="notifiers">Enumerable of Registered Object</param>
		/// <param name="player">player who made the check</param>
		/// <param name="data">Check data for Args</param>
		private static void NotifyObjects(IList<IDOLEventHandler> notifiers, GamePlayer player, LosCheckData data) 
		{
			// Lock notifiers list.
			lock(((ICollection)notifiers).SyncRoot)
			{
				foreach(IDOLEventHandler notifier in notifiers)
				{
					notifier.Notify(GameObjectEvent.FinishedLosCheck, player, data);
				}
				
				// Clear at end, so a new list can be made for next call
				notifiers.Clear();
			}
		}
		
		/// <summary>
		/// Register an event handler for the given key
		/// </summary>
		/// <param name="key">Key to Register to</param>
		/// <param name="notifier">Event handler Object</param>
		private void AddRegisteredEvent(Tuple<GameObject, GameObject> key, IDOLEventHandler notifier)
		{
			lock(((ICollection)RegisteredLosEvents).SyncRoot)
			{
				if(!RegisteredLosEvents.ContainsKey(key) || RegisteredLosEvents[key] == null) 
				{
					RegisteredLosEvents[key] = new List<IDOLEventHandler>();
				}

				RegisteredLosEvents[key].Add(notifier);
				return;
			}
		}

		/// <summary>
		/// Update nearest object with LoS results
		/// </summary>
		/// <param name="source">source of LoS</param>
		/// <param name="target">target of Los</param>
		/// <param name="losOK">is LoS ok ?</param>
		/// <param name="time">current time</param>
		private void UpdateVincinityLosCache(GameObject source, GameObject target, bool losOK, long time)
		{			
			// Take NPCs in the largest Radius, should be EvE
			foreach(GameNPC contamined in source.GetNPCsInRadius((ushort)LOSMGR_MAX_CONTAMINATION_RADIUS))
			{
				if(contamined == source || contamined == target)
					continue;
				
				// don't give LoS results to Peace NPC
				if(contamined.IsPeaceful)
					continue;
					
				// player pet to player should use the special PET radius
				if((isObjectFromPlayer(target) || isObjectFromPlayer(source))  && isObjectFromPlayer(contamined))
				{
					// Update using PvP cache Timeout
					if(LOSMGR_PET_CONTAMINATION_RADIUS > 0 && source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_PET_CONTAMINATION_RADIUS) 
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Contamination Los Check (Pet PvP) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));

						UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT, time);
					}
					
					continue;
				}
				
				// guard to anything should use guard radius
				if(contamined is GameKeepGuard)
				{
					// Update using PvE cache Timeout
					if(LOSMGR_GUARD_CONTAMINATION_RADIUS > 0 && source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_GUARD_CONTAMINATION_RADIUS)
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Contamination Los Check (Grd PvE) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));

						UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT, time);
					}
					
					continue;
				}
				
				// if this NPC is targetting a player it's PvE, or I'm a pet not targeting a player it's PvE too
				if((isObjectFromPlayer(target) || isObjectFromPlayer(source)) || isObjectFromPlayer(contamined))
				{
					// Update using PvE cache Timeout
					if(LOSMGR_NPC_CONTAMINATION_RADIUS > 0 && source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_NPC_CONTAMINATION_RADIUS)
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Contamination Los Check (Mob PvE) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));

						UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT, time);
					}
					
					continue;
				}
				

				// else it's EvE radius
				if(source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_MAX_CONTAMINATION_RADIUS)
				{
					// FIXME debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
						log.Warn("LOSMGR_D : Contamination Los Check (Mob EvE) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));
					
					UpdateLosCacheItem(source, target, losOK, LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT, time);
				}
			}
			
			// Take Player in the largest Radius, should be PvE for players
			foreach(GamePlayer contamined in source.GetPlayersInRadius((ushort)LOSMGR_NPC_CONTAMINATION_RADIUS))
			{
				if(contamined == source || contamined == target)
					continue;
				
				// P v P
				if(target is GamePlayer || source is GamePlayer)
				{
					if(LOSMGR_PLAYER_CONTAMINATION_RADIUS > 0 && source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_PLAYER_CONTAMINATION_RADIUS)
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Contamination Los Check (Pl PvP) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));

						UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT, time);
					}
					
					continue;
				}
				
				// Player to Pet
				if(isObjectFromPlayer(target) || isObjectFromPlayer(source)) 
				{
					if(LOSMGR_PET_CONTAMINATION_RADIUS > 0 && source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_PET_CONTAMINATION_RADIUS)
					{
						// FIXME debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
							log.Warn("LOSMGR_D : Contamination Los Check (Pl PvPets) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));

						UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT, time);
					}
					
					continue;
				}
				
				if(source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR) <= LOSMGR_NPC_CONTAMINATION_RADIUS)
				{
					// FIXME debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
						log.Warn("LOSMGR_D : Contamination Los Check (Pl PvE) of "+source.Name+" and "+target.Name+" Contaminated "+contamined.Name+", range : "+source.GetDistanceTo(contamined.Position, LOSMGR_CONTAMINATION_ZFACTOR));
	
					// else we're in PvE, a player can't be in EvE
					UpdateLosCacheItem(source, target, losOK, LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT, time);
				}
			}
			
		}
		
		/// <summary>
		/// Update a cache item bi-directionnal
		/// </summary>
		/// <param name="source">source object</param>
		/// <param name="target">target object</param>
		/// <param name="losOK">is los OK</param>
		/// <param name="timeout">cache timeout expected</param>
		/// <param name="time">current time</param>
		private void UpdateLosCacheItem(GameObject source, GameObject target, bool losOK, int timeout, long time)
		{
			// Should be used in an already locked environment
			
			Tuple<long, bool> cacheValue = new Tuple<long, bool>(time, losOK);
			
			ResponsesCache[new Tuple<GameObject, GameObject>(source, target)] = cacheValue;
			ResponsesCache[new Tuple<GameObject, GameObject>(target, source)] = cacheValue;	
		}
		
		#endregion

		#region utils
		
		/// <summary>
		/// Check if Object is from Gameplayer
		/// Depending on Serverproperties ALWAYS_CHECK_PET_LOS, Player Pets will be considered Mob or Players.
		/// </summary>
		/// <param name="obj">Object to check</param>
		/// <returns>True is this is related to a Player</returns>
		private bool isObjectFromPlayer(GameObject obj)
		{
			return (obj is GameNPC && ((GameNPC)obj).Brain is IControlledBrain && ServerProperties.Properties.ALWAYS_CHECK_PET_LOS && ((IControlledBrain)((GameNPC)obj).Brain).GetPlayerOwner() != null)
			   || obj is GamePlayer;
		}
		
		/// <summary>
		/// Check if source and target are considered in PvP fight (need better latency)
		/// </summary>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <returns>true if it's a PvP fight</returns>
		private bool IsPvpLosCheck(GameObject source, GameObject target)
		{	
			return isObjectFromPlayer(source) && isObjectFromPlayer(target);
		}
		
		/// <summary>
		/// Get default timeout depending on targets
		/// </summary>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <returns>Cache Timeout</returns>
		private int GetDefaultTimeouts(GameObject source, GameObject target)
		{
			// P v P
			if(IsPvpLosCheck(source, target))
				return LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT;
			
			// P v E
			if(isObjectFromPlayer(source) || isObjectFromPlayer(target))
				return LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT;
			
			// E v E
			return LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT;	
		}
		
		/// <summary>
		/// Get default range Threshold depending on targets
		/// </summary>
		/// <param name="source">GameObject from witch LoS check start</param>
		/// <param name="target">GameObject to check LoS to</param>
		/// <returns>Range in game unit</returns>
		private int GetDefaultThreshold(GameObject source, GameObject target)
		{
			// P v P
			if(IsPvpLosCheck(source, target))
				return LOSMGR_PLAYER_VS_PLAYER_RANGE_THRESHOLD;
			
			// P v E
			if(isObjectFromPlayer(source) || isObjectFromPlayer(target))
				return LOSMGR_PLAYER_VS_ENVIRONMENT_RANGE_THRESHOLD;
			
			// E v E
			return LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_RANGE_THRESHOLD;
		}
		
		/// <summary>
		/// Check if the GameObject is available for Los Check
		/// </summary>
		/// <param name="checker">The source object for Los Check</param>
		/// <returns>true if it can do a Los Check, false if it's throttled or not a GamePlayer</returns>
		public bool IsAvailableForLosCheck(GameObject checker, GameObject source, GameObject target) 
		{
			if(checker is GamePlayer && target != null && source != null)
			{
				// Check if it can do a new LoS Check
				lock(((ICollection)ClientChecks).SyncRoot)
				{
					if(ClientChecks.ContainsKey((GamePlayer)checker) && GetElapsedTicks(ClientChecks[(GamePlayer)checker]) < LOSMGR_PLAYER_CHECK_FREQUENCY)
					{
						// FIXME Debug
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_WARN)
							log.Warn("LOSMGR_W : Player Throttled - "+checker.Name);
						
						return false;
					}
				}
				
				// we're not doing anything
				return true;
			}
			
			return false;
		}
		
		private GamePlayer GetBestLosChecker(GameObject source, GameObject target) 
		{
			// Stealthed Player can't be third-party los Checked
			// FIXME could use player who can detect the Stealther (Realm mate) for a third party check
			if(source is GamePlayer && ((GamePlayer)source).IsStealthed)
			{
				// FIXME Debug
				if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
					log.Warn("LOSMGR_D : Vincinity Player choosen because source is stealthed - "+source.Name);
				
				return (GamePlayer)source;
			}
			
			if(target is GamePlayer && ((GamePlayer)target).IsStealthed)
			{
				// FIXME Debug
				if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
					log.Warn("LOSMGR_D : Vincinity Player choosen because target is stealthed - "+target.Name);
				
				return (GamePlayer)target;
			}
			
			
			// Get all players around able to check LoS
			IEnumerable<GamePlayer> playersQuery = (from GamePlayer player in source.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE) select player);
			
			List<GamePlayer> players = new List<GamePlayer>();
			
			foreach(GamePlayer test in playersQuery)
			{
				if(target.IsWithinRadius(test, WorldMgr.VISIBILITY_DISTANCE))
				   players.Add(test);
			}
			
			// first get all players that haven't done anything
			lock(((ICollection)ClientChecks).SyncRoot)
			{
				IEnumerable<GamePlayer> checkers = (from checker in players where !ClientChecks.ContainsKey(checker) || !ClientStats.ContainsKey(checker)
				                                    select checker).Take(1);
			
				// Pick one that isn't in cache
				foreach(GamePlayer choosed in checkers)
				{
					// FIXME Debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
						log.Warn("LOSMGR_D : Vincinity Player choosen because inactive - "+choosed.Name);
					
					return choosed;
				}
			}
			
			lock(((ICollection)ClientStats).SyncRoot)
			{
				long currentTime = GameTimer.GetTickCount();
				// We don't have player that aren't in cache try selecting available ones (lowest count && best instant checker)
				IEnumerable<GamePlayer> bestavails = (from best in ClientStats.Keys where players.Contains(best) && 
				                                      (ClientChecks.ContainsKey(best) && (currentTime-ClientChecks[best]) > LOSMGR_PLAYER_CHECK_FREQUENCY)
				            orderby ClientStats[best].Item1, ClientStats[best].Item3 descending
				            select best).Take(1);
				
				// Pick the best
				foreach(GamePlayer choosed in bestavails)
				{
					// FIXME Debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
						log.Warn("LOSMGR_D : Vincinity Player choosen because available and best checker - "+choosed.Name);
					
					return choosed;
				}
				
				// All of them are busy pick the fastest instant checker
				IEnumerable<GamePlayer> betters = (from best in ClientStats.Keys where players.Contains(best)
				            orderby ClientStats[best].Item3 descending, ClientStats[best].Item2 descending
				            select best).Take(1);
				
				// Pick the best
				foreach(GamePlayer choosed in betters)
				{
					// FIXME Debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
						log.Warn("LOSMGR_D : Vincinity Player choosen because best checker even if not available - "+choosed.Name);
					
					return choosed;
				}
			}
		
			// Return any default players
			foreach(GamePlayer choosed in players)
			{
				// FIXME Debug
				if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
					log.Warn("LOSMGR_D : Vincinity Player choosen because there was no one else - "+choosed.Name);
				
				return choosed;
			}
			
			// FIXME Debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG) 
			{
				string concat = "";
				foreach(GamePlayer choosed in players)
					concat += ", "+choosed.Name;
				
				log.Warn("LOSMGR_D : Vincinity Player could not be choosen amoung"+concat);
			}
			
			return null;
		}
		
		/// <summary>
		/// Returns the time elapsed since a given tickcount in millisecond
		/// Doesn't handle future...
		/// </summary>
		/// <param name="time">tick count</param>
		/// <returns></returns>
		public static long GetElapsedTicks(long time) 
		{
			long elapsed = ((long)GameTimer.GetTickCount())-time;
			return elapsed <= 0 ? 0 : elapsed;
		}

		/// <summary>
		/// Get the last LoS check from Cache
		/// </summary>
		/// <param name="source">source object for check</param>
		/// <param name="target">target object for check</param>
		/// <param name="timeout">oldest cache to use</param>
		/// <returns>Target is in LoS</returns>
		public bool GetLosCheckFromCache(GameObject source, GameObject target, int timeout = 0)
		{
			
			if(timeout == 0)
			{
				timeout = GetDefaultTimeouts(source, target);
			}
			
			Tuple<GameObject, GameObject> key = new Tuple<GameObject, GameObject>(source, target);
			
			lock(((ICollection)ResponsesCache).SyncRoot)
			{
				// KO LoS check have halved cache timeout
				if(ResponsesCache.ContainsKey(key) && 
				   ((ResponsesCache[key].Item2 && GetElapsedTicks(ResponsesCache[key].Item1) < timeout) || 
				   (!ResponsesCache[key].Item2 && GetElapsedTicks(ResponsesCache[key].Item1) < (timeout >> 1))))
				{
					// FIXME Display debug
					if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
						log.Warn("LOSMGR_I : Cached LoS returned, source : "+(source != null ? source.Name : "null")+", target : "+(target != null ? target.Name : "null")+", Los : "+(ResponsesCache[key].Item2 ? "OK" : "KO")+", timeout : "+timeout);
					
					return ResponsesCache[key].Item2;
				}
			}

			// FIXME Display debug
			if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_DEBUG)
				log.Warn("LOSMGR_D : Cached LoS Missed, timeout : "+timeout+", source : "+(source != null ? source.Name : "null")+", target : "+(target != null ? target.Name : "null"));
			
			// we don't have valid content
			throw new LosUnavailableException();
		}
		#endregion
		
		#region Cleanups

		/// <summary>
		/// Cleanup Routine called by timer.
		/// </summary>
		/// <param name="sender">Object sending Event</param>
		/// <param name="args">Event Args Object</param>
		private static void CleanUp(object sender, System.Timers.ElapsedEventArgs args) {
			if(sender != null && sender is LosCheckMgr)
				((LosCheckMgr)sender).CleanUp();
		}
		/// <summary>
		/// Clean up Dictionary, must be called from time to time
		/// </summary>
		public void CleanUp()
		{
			CleanUpCache();
			CleanUpClients();
			CleanUpPending();
			CleanUpNotifiers();
		}

		/// <summary>
		/// Clean up Los Check Cache
		/// </summary>
		private void CleanUpCache()
		{
			long obsoleteTime = GameTimer.GetTickCount() - (LOSMGR_CLEANUP_FREQUENCY);
			
			lock(((ICollection)ResponsesCache).SyncRoot)
			{
				foreach(Tuple<GameObject, GameObject> toClean in (from responses in ResponsesCache where responses.Value.Item1 < obsoleteTime select responses.Key).Take(MAX_CLEANUP_ENTRIES))
				{
					ResponsesCache.Remove(toClean);
				}
			}
		}

		/// <summary>
		/// Clean up Los Pending List
		/// </summary>				
		private void CleanUpPending()
		{
			long obsoleteTime = GameTimer.GetTickCount() - (LOSMGR_CLEANUP_FREQUENCY);
			
			lock(((ICollection)PendingChecks).SyncRoot)
			{
				foreach(Tuple<GamePlayer, ushort, ushort> toClean in (from pendings in PendingChecks where pendings.Value < obsoleteTime select pendings.Key).Take(MAX_CLEANUP_ENTRIES))
				{
					PendingChecks.Remove(toClean);
				}
			}
		}
		
		/// <summary>
		/// Clean up Los Clients Cache
		/// </summary>
		private void CleanUpClients()
		{
			long obsoleteTime = GameTimer.GetTickCount() - (LOSMGR_CLEANUP_FREQUENCY);
			
			lock(((ICollection)ClientChecks).SyncRoot)
			{
				foreach(GamePlayer toClean in (from clients in ClientChecks where clients.Value < obsoleteTime select clients.Key).Take(MAX_CLEANUP_ENTRIES))
				{
					ClientChecks.Remove(toClean);
					
					if(ClientStats.ContainsKey(toClean))
						ClientStats.Remove(toClean);
				}
			}
		}

		/// <summary>
		/// Clean up Notifier Cache
		/// </summary>
		private void CleanUpNotifiers()
		{
			lock(((ICollection)RegisteredLosEvents).SyncRoot)
			{
				foreach(Tuple<GameObject, GameObject> toClean in (from notifiers in RegisteredLosEvents where (notifiers.Value != null && notifiers.Value.Count < 1) || (notifiers.Key.Item1 == null || notifiers.Key.Item1.ObjectState != GameObject.eObjectState.Active) select notifiers.Key).Take(MAX_CLEANUP_ENTRIES))
				{
					RegisteredLosEvents.Remove(toClean);
				}
			}				
		}
		
		#endregion
		
		#region Pending Timed Checks
		
		/// <summary>
		/// Restart Pending Queries
		/// </summary>
		/// <param name="sender">Object sending Event</param>
		/// <param name="args">Event Args Object</param>
		private static void PendingLosCheck(object sender, System.Timers.ElapsedEventArgs args)
		{
			if(!(sender is LosCheckMgr))
			   return;
			
			LosCheckMgr chk = sender as LosCheckMgr;
			
			long currenTime = GameTimer.GetTickCount();
			// Get Timing out Pending Los Check
			IEnumerable<Tuple<GamePlayer, ushort, ushort>> pendingTimeout;
			lock(((ICollection)chk.PendingChecks).SyncRoot)
			{
				pendingTimeout = (from pending in chk.PendingChecks where currenTime-pending.Value >= LOSMGR_QUERY_TIMEOUT select pending.Key);
			}
			// Launch them to vincinity
			foreach(Tuple<GamePlayer, ushort, ushort> toRetry in pendingTimeout) 
			{
				GameObject source = toRetry.Item1.CurrentRegion.GetObject(toRetry.Item2);
				GameObject target = toRetry.Item1.CurrentRegion.GetObject(toRetry.Item3);
				
				if(source != null && target != null)
				{
					GamePlayer checker = chk.GetBestLosChecker(source, target);
					
					if(checker != null) 
					{
						if(LOSMGR_DEBUG_LEVEL >= LOSMGR_DEBUG_INFO)
							log.Warn("LOSMGR_I : Packet Resent, Player : "+checker.Name+", Source : "+source.Name+", Target : "+target.Name);
							
						chk.EventNotifierLosCheck(checker, source, target);
					}
				}
			}
			
		}
		
		#endregion
	}
	
	#region Los Data Subclass

	/// <summary>
	/// Los Data container.
	/// </summary>
	public class LosCheckData : EventArgs
	{
		/// <summary>
		/// The LoS Result
		/// </summary>
		private bool m_losOK = false;
		
		public bool LosOK
		{
			get
			{
				return m_losOK;
			}
			set
			{
				m_losOK = value;
			}
		}
		
		private readonly GameObject m_sourceObject;
		
		public GameObject SourceObject
		{
			get
			{
				return m_sourceObject;
			}
		}
		
		private readonly GameObject m_targetObject;
		
		public GameObject TargetObject
		{
			get
			{
				return m_targetObject;
			}
		}

		private readonly long m_sent;			

		public long Sent
		{
			get
			{
				return m_sent;
			}
		}
		
		public LosCheckData(GameObject source, GameObject target, long sent)
		{
			m_sourceObject = source;
			m_targetObject = target;
			m_sent = sent;
		}
		
		public LosCheckData(GameObject source, GameObject target, long sent, bool losOK)
		{
			m_sourceObject = source;
			m_targetObject = target;
			m_sent = sent;
			m_losOK = losOK;
		}
	}
	
	#endregion
	
	#region ExceptionHandlers
	/// <summary>
	/// Object thrown when something went wrong in los checks.
	/// </summary>
	public class LosUnavailableException : Exception
	{
	}
	#endregion

	#region losResponse handler
	
	/// <summary>
	/// Default Class implementing IDOLEventHandler with a callback to replace old Los Check callbacks.
	/// </summary>
	public class LosMgrResponseHandler : IDOLEventHandler
	{
		private readonly LosMgrResponse m_delegate;
		
		/// <summary>
		/// temp properties
		/// </summary>
		private readonly PropertyCollection m_tempProps = new PropertyCollection();		
		
		/// <summary>
		/// use it to store temporary properties on this living
		/// beware to use unique keys so they do not interfere
		/// </summary>
		public PropertyCollection TempProperties
		{
			get { return m_tempProps; }
		}
		
		public LosMgrResponseHandler(LosMgrResponse deleg) {
			m_delegate = deleg;
		}
		
		public void Notify(DOLEvent e, object sender, EventArgs args)
		{
			m_delegate((GamePlayer)sender, ((LosCheckData)args).SourceObject, ((LosCheckData)args).TargetObject, ((LosCheckData)args).LosOK, args, TempProperties);
		}
	}
	
	public delegate void LosMgrResponse(GamePlayer checker, GameObject source, GameObject target, bool losOK, EventArgs args, PropertyCollection tempProperties);
	
	#endregion
}
