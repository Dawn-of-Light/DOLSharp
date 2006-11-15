//#define MonitorCallbacks
//#define CollectStatistic
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using ThreadState = System.Threading.ThreadState;

namespace DOL.GS
{
	/// <summary>
	/// The GameTimer class invokes OnTick() method after
	/// certain intervals which are defined in milliseconds
	/// </summary>
	public abstract class GameTimer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Stores the reference to the next timer in the chain
		/// </summary>
		private GameTimer m_nextTimer;
		/// <summary>
		/// Stores the next execution tick and flags
		/// </summary>
		private long m_tick = TIMER_DISABLED;
		/// <summary>
		/// Stores the timer intervals
		/// </summary>
		private int m_interval;
		/// <summary>
		/// Stores the time where the timer was inserted
		/// </summary>
		private long m_targetTime = -1;
		/// <summary>
		/// Stores the time manager used for this timer
		/// </summary>
		private readonly TimeManager m_time;

		/// <summary>
		/// Flags the timer as disabled
		/// </summary>
		public static readonly long TIMER_DISABLED = long.MinValue;
		/// <summary>
		/// Flags the current tick timers as rescheduled
		/// </summary>
		public static readonly long TIMER_RESCHEDULED = 0x40000000;

		/// <summary>
		/// Constructs a new GameTimer
		/// </summary>
		/// <param name="time">The time manager for this timer</param>
		public GameTimer(TimeManager time)
		{
			if (time == null)
				throw new ArgumentNullException("time");
			m_time = time;
		}

		/// <summary>
		/// Gets the time left until this timer fires, in milliseconds.
		/// </summary>
		public int TimeUntilElapsed
		{
			get
			{
				long ins = m_targetTime;
				if (ins < 0)
					return -1;
				return (int)((ulong)ins - (ulong)m_time.CurrentTime);
			}
		}

		/// <summary>
		/// Gets or sets the timer intervals in milliseconds
		/// </summary>
		public virtual int Interval
		{
			get { return m_interval; }
			set
			{
				if (value < 0 || value > MaxInterval)
					throw new ArgumentOutOfRangeException("value", value, "Interval value must be in 1 .. "+MaxInterval.ToString()+" range.");
				m_interval = value;
			}
		}

		/// <summary>
		/// Gets the maximal allowed interval
		/// </summary>
		public long MaxInterval
		{
			get { return m_time.MaxInterval; }
		}

		/// <summary>
		/// Checks whether this timer is disabled
		/// </summary>
		public bool IsAlive
		{
			get { return (m_tick & TIMER_DISABLED) == 0; }
		}

		/// <summary>
		/// Returns short information about the timer
		/// </summary>
		/// <returns>Short info about the timer</returns>
		public override string ToString()
		{
			return string.Format("{0} tick:0x{1:X8} interval:{2} manager:'{3}'", GetType().FullName, m_tick, m_interval, m_time.Name);
		}

		/// <summary>
		/// Starts the timer with defined initial delay
		/// </summary>
		/// <param name="initialDelay">The initial timer delay. Must be more than 0 and less than MaxInterval</param>
		public virtual void Start(int initialDelay)
		{
			m_time.InsertTimer(this, initialDelay);
		}

//		/// <summary>
//		/// Starts the timer using current interval
//		/// </summary>
//		public virtual void Start()
//		{ not clear what it does, imo
//			m_time.InsertTimer(this, Interval);
//		}

		/// <summary>
		/// Stops the timer
		/// </summary>
		public virtual void Stop()
		{
			m_time.RemoveTimer(this);
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected abstract void OnTick();

		private static long StopwatchFrequencyMilliseconds = Stopwatch.Frequency / 1000;
		/// <summary>
		/// Get the tick count, this is needed because Environment.TickCount resets to 0
		/// when server has been up 48 days because it returned an int,
		/// this is a long
		/// </summary>
		/// <returns></returns>
		public static long GetTickCount()
		{
			return Stopwatch.GetTimestamp() / StopwatchFrequencyMilliseconds;
		}

		#region TimeManager

		/// <summary>
		/// This class manages all the GameTimers. It is started from
		/// within the GameServer.Start() method and stopped from
		/// within the GameServer.Stop() method. It runs an own thread
		/// when it is started, that cylces through all GameTimers and
		/// executes them at the right moment.
		/// </summary>
		public sealed class TimeManager
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// The size of cache array with 1ms granularity, in bits
			/// Must be more than or equal to one bucket size
			/// </summary>
			public static readonly int CACHE_BITS = 14;
			/// <summary>
			/// The bucket size in bits. All timers with same high bits get into same bucket.
			/// </summary>
			public static readonly int BUCKET_BITS = 4;
			/// <summary>
			/// The table arrays size in bits.
			/// Table size in milliseconds is (1 << BUCKET_BITS + TABLE_BITS).
			/// </summary>
			public static readonly int TABLE_BITS = 17;
			/// <summary>
			/// Defines amount of bits that don't fit the fixed time table.
			/// Two highest bits are reserved for flags.
			/// </summary>
			public static readonly int LONGTERM_BITS = 30 - BUCKET_BITS - TABLE_BITS;

			/// <summary>
			/// Defines the bitmask used to get timer cache bucket index
			/// </summary>
			public static readonly int CACHE_MASK = (1 << CACHE_BITS) - 1;
			/// <summary>
			/// Defines the bitmask used for a check if new bucket should be sorted
			/// </summary>
			public static readonly int BUCKET_MASK = (1 << BUCKET_BITS) - 1;
			/// <summary>
			/// Defines the bitmask used to get timer bucket index
			/// (after shifting it by BUCKET_BITS!)
			/// </summary>
			public static readonly int TABLE_MASK = (1 << TABLE_BITS) - 1;
			/// <summary>
			/// Defines the bitmask used to check if timer should be delayed for another table loop
			/// </summary>
			public static readonly int LONGTERM_MASK = ((1 << LONGTERM_BITS) - 1) << BUCKET_BITS + TABLE_BITS;
			/// <summary>
			/// Defines the bitmask used to "overflow" the current millisecond
			/// so that time table starts from 0 again.
			/// </summary>
			public static readonly int TICK_MASK = (1 << TABLE_BITS + BUCKET_BITS) - 1;


			/// <summary>
			/// The time thread
			/// </summary>
			private Thread m_timeThread;
			/// <summary>
			/// The time thread name
			/// </summary>
			private readonly string m_name;
			/// <summary>
			/// The timer is running while this flag is true
			/// </summary>
			private volatile bool m_running;
			/// <summary>
			/// The current virtual millisecond, overflows when it reach the time table end
			/// </summary>
			private int m_tick;
			/// <summary>
			/// The current manager time in milliseconds
			/// </summary>
			private long m_time;
			/// <summary>
			/// The cached bucket with 1ms granularity.
			/// All intervals that fit this array don't need sorting.
			/// </summary>
			private readonly CacheBucket[] m_cachedBucket = new CacheBucket[1 << CACHE_BITS];
			/// <summary>
			/// Stores all timers. Array index = (TimerTick >> BUCKET_BITS) & TABLE_MASK
			/// </summary>
			private readonly GameTimer[] m_buckets = new GameTimer[1 << TABLE_BITS];
			/// <summary>
			/// The count of active timers in the manager
			/// </summary>
			private int m_activeTimers;
			/// <summary>
			/// The count of invoked timers
			/// </summary>
			private long m_invokedCount;

			/// <summary>
			/// Holds the first and the last timers in the chain
			/// </summary>
			private struct CacheBucket
			{
				/// <summary>
				/// The first timer in the chain
				/// </summary>
				public GameTimer FirstTimer;
				/// <summary>
				/// The last timer in the chain
				/// </summary>
				public GameTimer LastTimer;
				/// <summary>
				/// The empty bucket
				/// </summary>
				public static readonly CacheBucket EmptyBucket = new CacheBucket();
			}

			/// <summary>
			/// Constructs a new time manager
			/// </summary>
			/// <param name="name">Thread name</param>
			public TimeManager(string name)
			{
				if (name == null)
					throw new ArgumentNullException("name");
				m_name = name;
#if MonitorCallbacks
				FileStream stream = new FileStream("logs\\delays-" + m_name + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
				m_delayLog = new StreamWriter(stream);
				m_delayLog.WriteLine("\n\n\n\n\n===============================");
				m_delayLog.WriteLine("=== new log "+DateTime.Now.ToString());
				m_delayLog.WriteLine("===============================\n\n\n");
#endif
			}

			/// <summary>
			/// Gets the maximal allowed interval
			/// </summary>
			public int MaxInterval
			{
				get { return (1 << LONGTERM_BITS + TABLE_BITS + BUCKET_BITS) - (1 << TABLE_BITS + BUCKET_BITS); }
			}

			/// <summary>
			/// Gets the current virtual millisecond which is reset after TICK_MASK ticks
			/// </summary>
			internal int CurrentTick
			{
				get { return m_tick; }
			}

			/// <summary>
			/// Gets the current manager time in milliseconds
			/// </summary>
			public long CurrentTime
			{
				get { return m_time; }
			}

			/// <summary>
			/// True if manager is active
			/// </summary>
			public bool Running
			{
				get { return m_running; }
			}

			/// <summary>
			/// Gets the manager name
			/// </summary>
			public string Name
			{
				get { return m_name; }
			}

			/// <summary>
			/// Gets the current count of active timers
			/// </summary>
			public int ActiveTimers
			{
				get { return m_activeTimers; }
			}

			/// <summary>
			/// Gets the invoked timers count
			/// </summary>
			public long InvokedCount
			{
				get { return m_invokedCount; }
			}

			/// <summary>
			/// Returns short description of the time manager
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return string.Format("time manager:'{0}' running:{1} currentTime:{2}", m_name, m_running, m_time);
			}

			#region debug

#if CollectStatistic
			/// <summary>
			/// Holds callback statistics
			/// </summary>
			private readonly Hashtable m_timerCallbackStatistic = new Hashtable();
			/// <summary>
			/// Get a list of active counts of different callbacks
			/// </summary>
			/// <returns></returns>
			public IList GetUsedCallbacks()
			{
				Hashtable table;
				lock (m_timerCallbackStatistic) {
					table = (Hashtable)m_timerCallbackStatistic.Clone();
				}

				// sort it
				ArrayList sorted = new ArrayList();
				foreach (DictionaryEntry entry in table) {						
					int count = (int)entry.Value;
					int i;
					for (i=0; i<sorted.Count; i++) {
						if ( ((int)((DictionaryEntry)sorted[i]).Value) < count ) break;
					}
					sorted.Insert(i, entry);
				}

				// make strings
				for (int i=0; i<sorted.Count; i++) {
					DictionaryEntry entry = (DictionaryEntry)sorted[i];
					sorted[i] = entry.Value+": "+entry.Key;
				}
				return sorted;
			}
#endif

			/// <summary>
			/// Counts all timers in the table
			/// </summary>
			/// <returns></returns>
			public int CountTimers()
			{
				lock (m_buckets)
				{
					int res = 0;
					GameTimer t;
					for (int i = 0; i < m_cachedBucket.Length; i++)
					{
						t = m_cachedBucket[i].FirstTimer;
						res += CheckChain(t, "cached bucket");
					}
					for (int i = 0; i < m_buckets.Length; i++)
					{
						t = m_buckets[i];
						res += CheckChain(t, "big buckets");
					}
					return res;
				}
			}

			/// <summary>
			/// Checks one timers chain
			/// </summary>
			/// <param name="t"></param>
			/// <param name="tableName"></param>
			/// <returns></returns>
			private static int CheckChain(GameTimer t, string tableName)
			{
				int res = 0;
				while (t != null)
				{
					if (res++ > 500000)
					{
						log.WarnFormat("possible circular chain (500000 timers in one bucket)");
						break;
					}
					t = t.m_nextTimer;
				}
				return res;
			}
			
#if MonitorCallbacks
			
			private StreamWriter m_delayLog;
			private volatile GameTimer m_currentTimer;
			private volatile int m_timerTickStart;
			
			private void SlowTimerCallback(object state)
			{
				try
				{
					int start = m_timerTickStart;
					GameTimer timer = m_currentTimer;
					if (timer == null) return;

					StringBuilder str = new StringBuilder("=== Timer already took ", 1024);
					str.Append(GetTickCount() - start).Append("ms\n");
					str.Append(timer.ToString());
					str.Append("\n\n");
					str.Append("Timer thread:\n");
					str.Append(Util.FormatStackTrace(Util.GetThreadStack(m_timeThread)));

					string packetStacks = PacketHandler.PacketProcessor.GetConnectionThreadpoolStacks();
					if (packetStacks.Length > 0)
					{
						str.Append("\n\nPackethandler threads:\n");
						str.Append(packetStacks);
					}

					str.Append("\n\nNPC update thread:\n");
					str.Append(Util.FormatStackTrace(WorldMgr.GetNpcUpdateStacktrace()));

					str.Append("\n\nRelocation thread:\n");
					str.Append(Util.FormatStackTrace(WorldMgr.GetRelocateRegionsStacktrace()));

					str.Append("\n\n");

					lock (m_delayLog)
					{
						m_delayLog.Write(str.ToString());
						m_delayLog.Flush();
					}
				}
				catch (Exception e)
				{
					if (log.IsWarnEnabled)
						log.Warn("collecting/writing timer delays", e);
				}
			}
			
#endif

			/// <summary>
			/// Gets the time thread stacktrace
			/// </summary>
			/// <returns>stacktrace</returns>
			public StackTrace GetStacktrace()
			{
				if (m_timeThread == null)
					return null;
				
				lock (m_timeThread)
				{
					return Util.GetThreadStack(m_timeThread);
				}
			}
			
			#endregion

			/// <summary>
			/// Starts the time manager if not started already
			/// </summary>
			/// <returns>success</returns>
			public bool Start()
			{
				if (m_timeThread != null)
					return false;

				lock (m_timeThread)
				{
					m_running = true;
					m_timeThread = new Thread(new ThreadStart(TimeThread));
					m_timeThread.Name = m_name;
					m_timeThread.Priority = ThreadPriority.AboveNormal;
					m_timeThread.IsBackground = true;
					m_timeThread.Start();
					return true;
				}
			}

			/// <summary>
			/// Stops the time manager if not stopped already
			/// </summary>
			/// <returns>success</returns>
			public bool Stop()
			{
				if (m_timeThread == null)
					return false;

				lock (m_timeThread)
				{
					m_running = false;

					if (!m_timeThread.Join(10000))
					{
						if (log.IsErrorEnabled)
						{
							ThreadState state = m_timeThread.ThreadState;
							StackTrace trace = Util.GetThreadStack(m_timeThread);
							log.ErrorFormat("failed to stop the time thread \"{0}\" in 10 seconds (thread state={1}); thread stacktrace:\n", m_name, state);
							log.ErrorFormat(Util.FormatStackTrace(trace));
							log.ErrorFormat("aborting the thread.\n");
						}
						m_timeThread.Abort();
					}
					
					m_timeThread = null;

					Array.Clear(m_buckets, 0, m_buckets.Length);
					Array.Clear(m_cachedBucket, 0, m_cachedBucket.Length);
					
#if MonitorCallbacks
					try
					{
						m_delayLog.Flush();
						m_delayLog.Close();
					}
					catch (Exception e)
					{
						log.Error("Closing delays log while stop() "+m_name, e);
					}
#endif
					return true;
				}
			}

			/// <summary>
			/// Inserts the timer into the table.
			/// </summary>
			/// <param name="t">The timer to insert</param>
			/// <param name="offsetTick">The offset from current tick. min value=1, max value&lt;MaxInterval</param>
			internal void InsertTimer(GameTimer t, int offsetTick)
			{
				if (offsetTick > MaxInterval || offsetTick < 1)
					throw new ArgumentOutOfRangeException("offsetTick", offsetTick.ToString(), "Offset must be in range from 1 to "+MaxInterval);

				GameTimer timer = t;

				lock (m_buckets)
				{
					long timerTick = timer.m_tick;
					long targetTick = m_tick + offsetTick;
					
					if (timerTick == m_tick || (timerTick & TIMER_RESCHEDULED) != 0)
					{
						timer.m_targetTime = (int) (CurrentTime + offsetTick);
						timer.m_tick = targetTick | TIMER_RESCHEDULED;
						return;
					}

					if ((timerTick & TIMER_DISABLED) == 0)
					{
						RemoveTimerUnsafe(timer);
					}

					timer.m_targetTime = (int) (CurrentTime + offsetTick);
					m_activeTimers++;

					if (offsetTick <= CACHE_MASK + 1)
					{
						timer.m_tick = targetTick & TICK_MASK;
						targetTick &= CACHE_MASK;
						CacheBucket bucket = m_cachedBucket[targetTick];
						GameTimer prev = bucket.LastTimer;
						if (prev != null)
						{
							prev.m_nextTimer = timer;
							m_cachedBucket[targetTick].LastTimer = timer;
						}
						else
						{
							bucket.FirstTimer = timer;
							bucket.LastTimer = timer;
							m_cachedBucket[targetTick] = bucket;
						}
					}
					else
					{
						if ((targetTick & TICK_MASK) > (m_tick & ~BUCKET_MASK) + BUCKET_MASK)
							targetTick += TICK_MASK + 1; // extra pass if the timer is ahead of current tick
						timer.m_tick = targetTick;
                        targetTick = (targetTick >> BUCKET_BITS) & TABLE_MASK;
						GameTimer next = m_buckets[targetTick];
						m_buckets[targetTick] = timer;
						if (next != null)
						{
							timer.m_nextTimer = next;
						}
					}
				}
			}

			/// <summary>
			/// Removes the timer from the table.
			/// </summary>
			/// <param name="timer">The timer to remove</param>
			internal void RemoveTimer(GameTimer timer)
			{
				lock (m_buckets)
				{
					RemoveTimerUnsafe(timer);
				}
			}

			/// <summary>
			/// Removes the timer from the table without locking the table
			/// </summary>
			/// <param name="timer">The timer to remove</param>
			private void RemoveTimerUnsafe(GameTimer timer)
			{
				GameTimer t = timer;
				long tick = t.m_tick;
				if ((tick & TIMER_DISABLED) != 0)
					return;

				timer.m_targetTime = -1;
				// never change the active chain
				if (tick == m_tick || (tick & TIMER_RESCHEDULED) != 0)
				{
					t.m_tick = TIMER_DISABLED | TIMER_RESCHEDULED;
					return;
				}

				m_activeTimers--;

				// check the cache first
				long cachedIndex = tick & CACHE_MASK;
				CacheBucket bucket = m_cachedBucket[cachedIndex];
				if (bucket.FirstTimer == t)
				{
					t.m_tick = TIMER_DISABLED;
					bucket.FirstTimer = t.m_nextTimer;
					if (bucket.LastTimer == t)
						bucket.LastTimer = t.m_nextTimer;
					t.m_nextTimer = null;
					m_cachedBucket[cachedIndex] = bucket;
					return;
				}

				GameTimer timerChain = bucket.FirstTimer;
				GameTimer prev;
				while (timerChain != null)
				{
					prev = timerChain;
					timerChain = timerChain.m_nextTimer;
					if (timerChain == t)
					{
						prev.m_nextTimer = t.m_nextTimer;
						t.m_nextTimer = null;
						t.m_tick = TIMER_DISABLED;
						if (bucket.LastTimer == t)
						{
							bucket.LastTimer = prev;
							m_cachedBucket[cachedIndex] = bucket;
						}
						return;
					}
				}

				// check the buckets
				tick = (tick >> BUCKET_BITS) & TABLE_MASK;
				timerChain = m_buckets[tick];
				if (timerChain == t)
				{
					timerChain = timerChain.m_nextTimer;
					m_buckets[tick] = timerChain;
					t.m_nextTimer = null;
					t.m_tick = TIMER_DISABLED;
					return;
				}

				while (timerChain != null)
				{
					prev = timerChain;
					timerChain = timerChain.m_nextTimer;
					if (timerChain == t)
					{
						prev.m_nextTimer = t.m_nextTimer;
						break;
					}
				}
				t.m_nextTimer = null;
				t.m_tick = TIMER_DISABLED;
			}

			/// <summary>
			/// The time thread loop
			/// </summary>
			private void TimeThread()
			{
				log.InfoFormat("started timer thread {0} (ID:{1})", m_name, AppDomain.GetCurrentThreadId());

				int timeBalance = 0;
				uint workStart, workEnd;
				GameTimer chain, next, bucketTimer;

				workStart = workEnd = (uint)GetTickCount();
				
#if MonitorCallbacks
				Timer t = new Timer(new TimerCallback(SlowTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
#endif

				while (m_running)
				{
					try
					{
						// fire timers
						lock (m_buckets)
						{
							m_time++;
							int newTick = m_tick = (m_tick + 1) & TICK_MASK;
							if ((newTick & BUCKET_MASK) == 0)
							{
								// cache next bucket
								int index = newTick >> BUCKET_BITS;
								next = m_buckets[index];
								if (next != null)
								{
									m_buckets[index] = null;
									// sort the new cached bucket
									do
									{
										GameTimer timer = next;
										next = next.m_nextTimer;
										long index2 = timer.m_tick;
										if ((index2 & LONGTERM_MASK) != 0
											&& ((index2 -= (1 << TABLE_BITS + BUCKET_BITS)) & LONGTERM_MASK) != 0)
										{
											// reinsert longterm timers back
											timer.m_tick = index2;
											bucketTimer = m_buckets[index];
											m_buckets[index] = timer;
										}
										else
										{
											timer.m_tick = index2;
											index2 &= CACHE_MASK;
											bucketTimer = m_cachedBucket[index2].FirstTimer;
											m_cachedBucket[index2].FirstTimer = timer;
											if (m_cachedBucket[index2].LastTimer == null)
												m_cachedBucket[index2].LastTimer = timer;
										}

										if (bucketTimer == null)
										{
											timer.m_nextTimer = null;
										}
										else
										{
											timer.m_nextTimer = bucketTimer;
										}
									}
									while (next != null);
								}
							}

							int cacheIndex = m_tick & CACHE_MASK;
							chain = m_cachedBucket[cacheIndex].FirstTimer;
							if (chain != null)
							{
								m_cachedBucket[cacheIndex] = CacheBucket.EmptyBucket;
							}
						}

						GameTimer current = chain;
						int curTick = m_tick;
						int currentBucketMax = (curTick & ~BUCKET_MASK) + BUCKET_MASK;
						while (current != null)
						{
							if (current.m_tick == curTick)
							{
								try
								{
									long callbackStart = GetTickCount();
									
#if MonitorCallbacks
									m_currentTimer = current;
									m_timerTickStart = callbackStart;
									t.Change(200, 100);
#endif

									current.OnTick();

#if MonitorCallbacks
									m_currentTimer = null;
									t.Change(Timeout.Infinite, Timeout.Infinite);
									if (GetTickCount() - callbackStart > 200)
									{
										lock (m_delayLog)
										{
											m_delayLog.Write("\n========================================================\n\n");
										}
									}
#endif

									if (GetTickCount() - callbackStart > 100) 
									{
										if (log.IsWarnEnabled)
										{
											string curStr;
											try { curStr = current.ToString(); }
											catch(Exception ee) { curStr = "error in timer.ToString(): " + current.GetType().FullName + "; " + ee.ToString(); }
											string warning = "callback took "+(GetTickCount() - callbackStart)+"ms! "+curStr;
											log.Warn(warning);
										}
									}

#if CollectStatistic
									// statistic
									int start = GetTickCount();
									string callback;
									if (current is RegionTimer)
									{
										callback = ((RegionTimer)current).Callback.Method.ToString();
									}
									else
									{
										callback = current.GetType().FullName;
									}
									lock (m_timerCallbackStatistic) 
									{
										object obj = m_timerCallbackStatistic[callback];
										if (obj == null) 
										{
											m_timerCallbackStatistic[callback] = 1;	
										} 
										else 
										{
											m_timerCallbackStatistic[callback] = ((int)obj) + 1;
										}
									}
									if (GetTickCount()-start > 500) 
									{
										if (log.IsWarnEnabled)
											log.Warn("Ticker statistic "+callback+" took more than 500ms!");
									}
#endif
								}
								catch (Exception e)
								{
									string curStr;
									try { curStr = current.ToString(); }
									catch(Exception ee) { curStr = "error in timer.ToString(): " + current.GetType().FullName + "; " + ee.ToString(); }
									if (log.IsErrorEnabled)
										log.Error("Timer callback (" + curStr + ")", e);
									current.m_tick = TIMER_DISABLED | TIMER_RESCHEDULED;
								}
								
								m_invokedCount++;
							}
							else if ((current.m_tick & TIMER_RESCHEDULED) == 0)
							{
								log.ErrorFormat("timer tick != current tick (0x{0:X4}), fired anyway: {1}", curTick, current);
								try
								{
									current.OnTick();
								}
								catch (Exception e)
								{
									log.Error("timer error", e);
									current.m_tick = TIMER_DISABLED | TIMER_RESCHEDULED;
								}
							}

							lock (m_buckets)
							{
								next = current.m_nextTimer;
								long tick = current.m_tick;
								long interval = current.m_interval;

								if ((tick & TIMER_DISABLED) != 0 || (interval == 0 && (tick & TIMER_RESCHEDULED) == 0))
								{
									m_activeTimers--;
									current.m_nextTimer = null;
									current.m_tick = TIMER_DISABLED;
									current.m_targetTime = -1;
								}
								else
								{
									///// REINSERT all including rescheduled timers
									if ((tick & TIMER_RESCHEDULED) != 0)
									{
										current.m_tick = tick &= ~TIMER_RESCHEDULED;
									}
									else
									{
										current.m_targetTime = (int) (CurrentTime + interval);
										current.m_tick = tick = curTick + interval;
									}

									if (tick - curTick <= CACHE_MASK + 1)
									{
										tick &= CACHE_MASK;
										current.m_tick &= TICK_MASK;
										CacheBucket bucket = m_cachedBucket[tick];
										GameTimer prev = bucket.LastTimer;
										current.m_nextTimer = null;
										if (prev != null)
										{
											prev.m_nextTimer = current;
											bucket.LastTimer = current;
										}
										else
										{
											bucket.FirstTimer = current;
											bucket.LastTimer = current;
										}
										m_cachedBucket[tick] = bucket;
									}
									else
									{
										if ((tick & TICK_MASK) > currentBucketMax)
											current.m_tick = tick += TICK_MASK + 1; // extra pass if the timer is ahead of current tick
										tick = (tick >> BUCKET_BITS) & TABLE_MASK;
										bucketTimer = m_buckets[tick];
										if (bucketTimer == null)
										{
											current.m_nextTimer = null;
										}
										else
										{
											current.m_nextTimer = bucketTimer;
										}
										m_buckets[tick] = current;
									}

									/////
								}
							}

							current = next;
						}

						bucketTimer = null;




						workEnd = (uint)GetTickCount();
						timeBalance += 1 - (int)(workEnd - workStart);

						if (timeBalance > 0)
						{
							Thread.Sleep(timeBalance);
							workStart = (uint)GetTickCount();
							timeBalance -= (int)(workStart - workEnd);
						}
						else
						{
							if (timeBalance < -1000)
							{
								//We can not increase forever if we get out of
								//sync. At some point we have to print out a warning
								//and catch up some time!
								if (log.IsWarnEnabled)
									log.Warn(Name + " out of sync! 1000ms lost! " + timeBalance.ToString());
								timeBalance += 1000;
							}
							workStart = workEnd;
						}
					}
					catch (ThreadAbortException e)
					{
						if (log.IsWarnEnabled)
							log.Warn("Time manager thread \"" + m_name + "\" was aborted", e);
						m_running = false;
						break;
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Exception in time manager \"" + m_name + "\"!", e);
					}
				}

				log.InfoFormat("stopped timer thread {0} (ID:{1})", m_name, AppDomain.GetCurrentThreadId());
			}
		}

		#endregion
	}
}
