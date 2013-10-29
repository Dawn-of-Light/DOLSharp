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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using ThreadState = System.Threading.ThreadState;
using Amib.Threading;

namespace DOL.GS
{
	/// <summary>
	/// The GameTimer class invokes OnTick() method after
	/// certain intervals which are defined in milliseconds
	/// </summary>
	public abstract class GameTimer
	{
		#region static field	
		/// <summary>
		/// Get the tick count, this is needed because Environment.TickCount resets to 0
		/// when server has been up 48 days because it returned an int,
		/// this is a long
		/// This value is Synchronizing all Server Events !
		/// </summary>
		/// <returns></returns>
		public static long GetTickCount()
		{
			return DateTime.UtcNow.Ticks / 10000;
		}

		/// <summary>
		/// Gets the maximal allowed interval
		/// </summary>
		public static long MaxInterval
		{
			get { return long.MaxValue / 10000;; }
		}
		
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	
		#endregion
	
		#region locks
		/// <summary>
		/// Wether this timer is firing or not
		/// </summary>
		private volatile bool m_firing = false;
		
		/// <summary>
		/// Wether this timer is started or not
		/// </summary>
		private bool m_started = false;
		
		/// <summary>
		/// Locking Object
		/// </summary>
		private readonly object m_timerLock = new object();
		
		#endregion
		
		#region Timer Members		
		/// <summary>
		/// Stores the timer intervals
		/// </summary>
		private long m_interval = 0;
		
		/// <summary>
		/// Stores the time where the timer should run
		/// </summary>
		private long m_targetTime = MaxInterval;
		
		/// <summary>
		/// Stores the time manager used for this timer
		/// </summary>
		private readonly GameScheduler m_scheduler;

		/// <summary>
		/// Gets the time left until this timer fires, in milliseconds.
		/// </summary>
		public int TimeUntilElapsed
		{
			get
			{
				
				if(m_firing)
					return 0;

				lock(m_timerLock)
				{
					if(!m_started)
					return -1;
					
					if (m_targetTime < GetTickCount())
						return -1;
				
					return (int)(m_targetTime - GetTickCount());
				}
			}
		}

		/// <summary>
		/// Gets or sets the timer intervals in milliseconds
		/// </summary>
		public virtual long Interval
		{
			get 
			{
				lock(m_timerLock)
					return m_interval;
			}
			set
			{
				if (value < 0 || value > MaxInterval)
					throw new ArgumentOutOfRangeException("value", value, "Interval value must be in 1 .. "+MaxInterval.ToString()+" range.");
				
				lock(m_timerLock)
					m_interval = value;
			}
		}

		/// <summary>
		/// Checks whether this timer is disabled
		/// </summary>
		public bool IsAlive
		{
			get 
			{
				lock(m_timerLock)
					return m_started;
			}
		}

		#endregion

		/// <summary>
		/// Constructs a new GameTimer
		/// </summary>
		/// <param name="time">The time manager for this timer</param>
		public GameTimer(GameScheduler sched)
		{
				if (sched == null)
					throw new ArgumentNullException("GameScheduler");
				m_scheduler = sched;
		}
		
		#region methods		
		/// <summary>
		/// Returns short information about the timer
		/// </summary>
		/// <returns>Short info about the timer</returns>
		public override string ToString()
		{
			return string.Format("{0} targetTime:0x{1:X8} interval:{2} manager:'{3}'", GetType().FullName, m_targetTime, m_interval, m_scheduler.Name);
		}
		
		/// <summary>
		/// Starts the timer with defined initial delay
		/// </summary>
		/// <param name="initialDelay">The initial timer delay. Must be more than 0 and less than MaxInterval</param>
		public virtual void Start(long initialDelay)
		{
			if (initialDelay > MaxInterval || initialDelay < 1)
				throw new ArgumentOutOfRangeException("offsetTick", initialDelay.ToString(), "Offset must be in range from 1 to "+MaxInterval);
			
			if(!IsAlive) 
			{
				m_started = true;
				
				m_targetTime = GetTickCount()+initialDelay;
				m_scheduler.InsertTimer(this, m_targetTime);
			}
		}

		/// <summary>
		/// Stops the timer
		/// </summary>
		public virtual void Stop()
		{
			if(IsAlive)
			{
				m_started = false;
				
				lock(m_timerLock) 
				{
					// Make sure it finished

					m_scheduler.RemoveTimer(this);
					m_targetTime = MaxInterval;
				}
			}
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected abstract void OnTick();
		
		#endregion

		#region TimeManager

		/// <summary>
		/// This class manages all the GameTimers. It is started from
		/// within the GameServer.Start() method and stopped from
		/// within the GameServer.Stop() method. It runs an own thread
		/// when it is started, that cylces through all GameTimers and
		/// executes them at the right moment.
		/// </summary>
		public sealed class GameScheduler
		{
			#region static members
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			
			private const int MAX_SCHEDULER_SLEEP = 4500;
			#endregion
			
			#region lock
			/// <summary>
			/// Lock Object to prevent concurrent access
			/// </summary>
			private readonly object m_lockObject = new object();

			/// <summary>
			/// The timer is running while this flag is true
			/// </summary>
			private volatile bool m_running = false;
			
			/// <summary>
			/// True if manager is active
			/// </summary>
			public bool Running
			{
				get { return m_running; }
			}
			
			#endregion

			#region member
			/// <summary>
			/// Thread Pool Handling this Time Manager
			/// </summary>
			//private SmartThreadPool m_threadPool;
			
			/// <summary>
			/// Defines a sorted dict containing timer to fire.
			/// </summary>
			private readonly SortedList<long, List<GameTimer>> m_scheduleQueue = new SortedList<long, List<GameTimer>>();
			
			/// <summary>
			/// Pulse Monitor to notify scheduler of a timer.
			/// </summary>
			private readonly EventWaitHandle m_pulseMonitor = new EventWaitHandle(false, EventResetMode.AutoReset);
			
			/// <summary>
			/// The scheduler thread
			/// </summary>
			private Thread m_timeThread;
			
			/// <summary>
			/// The time thread name
			/// </summary>
			private readonly string m_name;
			
			/// <summary>
			/// The Next event time.
			/// </summary>			
			public long NextTick 
			{
				get
				{
					lock(m_lockObject)
					{
						if(m_scheduleQueue.Count > 0)
							return m_scheduleQueue.Keys[0];
						
						return MaxInterval;
					}
				}
			}
			
			/// <summary>
			/// The current manager time in milliseconds
			/// </summary>
			private long m_time;

			/// <summary>
			/// Gets the current manager time in milliseconds
			/// </summary>
			public long CurrentTime
			{
				get 
				{
					long num;
					lock(m_lockObject)
						num = m_time;
					
					return num;
				}
				
				set 
				{
					lock(m_lockObject)
						m_time = value; 
				}
			}

			/// <summary>
			/// Gets the manager name
			/// </summary>
			public string Name
			{
				get { return m_name; }
			}
			
			#endregion
			
			/// <summary>
			/// Constructs a new time manager
			/// </summary>
			/// <param name="name">Thread name</param>
			public GameScheduler(string name)
			{
				if (name == null)
					throw new ArgumentNullException("name");
				m_name = name;
			}
			
			#region statistics and debug

			/// <summary>
			/// Returns short description of the time manager
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return string.Format("time manager:'{0}' running:{1} currentTime:{2}", m_name, m_running, m_time);
			}
			
			/// <summary>
			/// Sync Message frequency
			/// </summary>
			private const long DELAY_WARNING_SYNC = 10000;
			
			/// <summary>
			/// Sync Message Delay
			/// </summary>
			private long OutOfSyncWarn = long.MinValue / 10000;
			private long OutOfTimeWarn = long.MinValue / 10000;

			private int m_activeTimer = 0;
			/// <summary>
			/// Gets the current count of active timers
			/// </summary>
			public int ActiveTimers
			{
				get 
				{
					int num;
					lock(m_lockObject)
						num = m_activeTimer;
					
					return num;
				}
			}

			private long m_invokedCount = 0;
			/// <summary>
			/// Gets the invoked timers count
			/// </summary>
			public long InvokedCount
			{
				get 
				{
					long num;
					lock(m_lockObject)
						num = m_invokedCount;
					
					return num;
				}
			}

			private long m_threadLoop = 0;
			/// <summary>
			/// Gets the invoked timers count
			/// </summary>
			public long ThreadLoop
			{
				get 
				{
					long num;
					lock(m_lockObject)
						num = m_threadLoop;
					
					return num;
				}
			}

			/// <summary>
			/// Gets the time thread stacktrace
			/// </summary>
			/// <returns>stacktrace</returns>
			public StackTrace GetStacktrace()
			{
				if (m_timeThread == null)
					return null;
				
				StackTrace result;
				lock (m_lockObject)
				{
					result = Util.GetThreadStack(m_timeThread);
				}
				
				return result;
			}

			#endregion 
			
			#region Start/Stop
			/// <summary>
			/// Starts the time manager if not started already
			/// </summary>
			/// <returns>success</returns>
			public bool Start()
			{
				if(m_running)
					return false;
				
				m_running = true;
				
				// if it's first start it shouldn't lock
				lock(m_lockObject)
				{
					m_time = GetTickCount();
					
					// Init Scheduler Thread
					m_timeThread = new Thread(new ThreadStart(TimeThread));
					m_timeThread.Name = m_name;
					m_timeThread.Priority = ThreadPriority.AboveNormal;
					m_timeThread.IsBackground = true;
									
					m_timeThread.Start();
					//m_threadPool.Start();
					
					return true;
				}
			}

			/// <summary>
			/// Stops the time manager if not stopped already
			/// </summary>
			/// <returns>success</returns>
			public bool Stop()
			{

				if(!m_running)
					return false;
				
				// Tell the thread loop to stop, this is volatile
				m_running = false;
				
				// Force Thread to finish Loop
				m_pulseMonitor.Set();
				
				// this should lock during shutdown, 
				// if lock cannot be acquired, there is something wrong going in scheduler
				lock(m_lockObject)
				{
					// make sure it stops
					m_pulseMonitor.Set();
				}
				
				lock(m_lockObject)
				{
					if (!m_timeThread.Join(3000))
					{
						if (log.IsErrorEnabled)
						{
							ThreadState state = m_timeThread.ThreadState;
							StackTrace trace = Util.GetThreadStack(m_timeThread);
							log.ErrorFormat("failed to stop the time thread \"{0}\" in 3 seconds (thread state={1}); thread stacktrace:\n", m_name, state);
							log.ErrorFormat(Util.FormatStackTrace(trace));
							log.ErrorFormat("aborting the thread.\n");
						}
						
						try 
						{
							m_timeThread.Abort();
							
							if (m_timeThread.Join(3000))
							{
								m_scheduleQueue.Clear();
								return true;
							}
							else
							{
								log.ErrorFormat("Couldn't gracefully abort thread {0}.\n", m_name);
								return false;
							}
							
						}
						catch
						{
							log.ErrorFormat("Couldn't abort thread {0}.\n", m_name);
						}
						finally
						{
							m_scheduleQueue.Clear();
							m_timeThread = null;
						}
					}

				}
				
				return true;
			}
			#endregion

			#region Scheduling
			internal void InsertTimer(GameTimer t, long adujstTick)
			{
				
				bool pulse = false;
				
				// inserting need the Scheduler to be available
				lock(m_lockObject)
				{
					if(adujstTick < NextTick)
					{
						pulse = true;
					}
					
					if(!m_scheduleQueue.ContainsKey(adujstTick))
					{
						m_scheduleQueue.Add(adujstTick, new List<GameTimer>());
					}
					
					m_scheduleQueue[adujstTick].Add(t);
					m_activeTimer++;
				}
				
				if(pulse)
					m_pulseMonitor.Set();
					
			}
			

			/// <summary>
			/// Removes the timer from the table.
			/// </summary>
			/// <param name="timer">The timer to remove</param>
			internal void RemoveTimer(GameTimer timer)
			{
				// removing need the Scheduler to be available				
				lock(m_lockObject)
				{
					while(m_scheduleQueue.ContainsKey(timer.m_targetTime) && m_scheduleQueue[timer.m_targetTime].Contains(timer)) 
					{
						m_scheduleQueue[timer.m_targetTime].Remove(timer);
						m_activeTimer--;
					}
				}
			}
			
			#endregion

			#region scheduler Loop
			/// <summary>
			/// The time thread loop
			/// </summary>
			private void TimeThread()
			{
				log.InfoFormat("started timer thread {0} (ID:{1})", m_name, Thread.CurrentThread.ManagedThreadId);

				long workEnd, previousTime;
				m_time = workEnd = previousTime = GetTickCount();
				int count,listcount,index,wait;
				Queue<GameTimer> firingQueue = new Queue<GameTimer>();
				
				ushort trim = 0;
				
				while (m_running)
				{
					
					try
					{
						// Set Current Time
						m_time = GetTickCount();
						
						// Lock the Scheduler
						lock(m_lockObject)
						{
							if(trim > 1000) 
							{
								//Trim lists for performances
								m_scheduleQueue.TrimExcess();
								trim = 0;
							}
							else 
							{
								trim++;
							}

							
							// Get all event needing firing
							
							// Try Index
							if(m_scheduleQueue.ContainsKey(m_time)) 
							{
								index = m_scheduleQueue.IndexOfKey(m_time)+1;
							}
							else
							{
								// Lookup index
								index = 0;
								for(count = 0 ; count < m_scheduleQueue.Count ; count++) 
								{
									index = count;
									if(m_scheduleQueue.Keys[count] > m_time)
									{
										break;
									}
								}
							}
							
							// Fire them
							for(count = 0 ; count < index ; count++) 
							{
								for(listcount = 0 ; listcount < m_scheduleQueue.Values[count].Count ; listcount++)
								{
									if(!m_scheduleQueue.Values[count][listcount].m_firing) 
									{
										m_scheduleQueue.Values[count][listcount].m_firing = true;
										firingQueue.Enqueue(m_scheduleQueue.Values[count][listcount]);										
										m_invokedCount++;
										m_activeTimer--;
									}
									else
									{
										log.Warn("Duplicate Job in "+m_name+" - "+m_scheduleQueue.Values[count][listcount].ToString());
									}
								}
							}
							
							// Unschedule all runned ticks
							for(count = 0 ; count < index ; count++) 
							{
								m_scheduleQueue.RemoveAt(0);
							}
						}
						
						while(firingQueue.Count > 0) 
						{
							// inline Timer
							FireTimer(firingQueue.Dequeue());
						}
						
						lock(m_lockObject)
						{
							// Loop again If needed and listen for signal !
							wait = (int)Math.Max(0, Math.Min(MAX_SCHEDULER_SLEEP, NextTick - GetTickCount()));
						}
						
						//Check for Desync
						workEnd = GetTickCount();
					
						if(OutOfSyncWarn < workEnd && (workEnd - m_time) > 150)
						{
							// we're out of Sync !
							log.Warn("Game Scheduler "+m_name+" Out of Sync !! - "+(workEnd - m_time)+"ms");
							OutOfSyncWarn = workEnd + DELAY_WARNING_SYNC;
						}
						
						while(m_running) 
						{
							if(m_pulseMonitor.WaitOne(wait)) 
							{
								// make sure it stops !
								if(!m_running)
									break;
								
								// got a reinsert signal !
								lock(m_lockObject) 
								{
									wait = (int)Math.Max(0, Math.Min(MAX_SCHEDULER_SLEEP, NextTick - GetTickCount()));
								}
								
								// exit if we're needed immediatly
								if(wait == 0)
									break;
							}
							else
							{
								break;
							}
						}

					}
					catch (ThreadAbortException e)
					{
						if (log.IsWarnEnabled)
							log.Warn("Time manager thread \"" + m_name + "\" was aborted (Time : "+GetTickCount()+") (Next : "+NextTick+")", e);
						
						m_running = false;
						break;
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Exception in time manager \"" + m_name + "\"!", e);
					}
					
					m_threadLoop++;
				}
				
				log.InfoFormat("stopped timer thread {0} (ID:{1})", m_name, Thread.CurrentThread.ManagedThreadId);
			}
			
			#endregion
			
			/// <summary>
			/// Fire Timer Callback used in Thread Pool
			/// </summary>
			/// <param name="gt"></param>
			/// <returns></returns>
			private void FireTimer(Object obj)
			{
				
				GameTimer gt = (GameTimer)obj;
				
				// Time the Callback
				long start = GetTickCount();
				
				lock(gt.m_timerLock)
				{
					if(!gt.m_firing)
						log.Warn("Timer isn't scheduled - "+gt.ToString());
					
					if(gt.m_started)
						gt.OnTick();
					
					gt.m_firing = false;
					
					if(gt.m_interval > 0 && gt.m_started)
					{
						// if interval, reschedule
						gt.m_targetTime += gt.m_interval;
						
						if(OutOfTimeWarn > GetTickCount() && gt.m_targetTime < GetTickCount()) 
						{
							log.Warn("Timer is reinserting in Past (Current Time : "+GetTickCount()+" ) (TargetTime : "+gt.m_targetTime+") - "+gt.ToString());
							OutOfTimeWarn=GetTickCount()+(DELAY_WARNING_SYNC >> 1);
						}
						
						gt.m_scheduler.InsertTimer(gt, gt.m_targetTime);
					}
					else
					{
						// unschedule
						gt.m_started = false;
						gt.m_targetTime = MaxInterval;
					}
				}
				
				long elapsed = start - GetTickCount();

				if(elapsed > 250) 
				{
					if (log.IsWarnEnabled)
					{
						string curStr;
						try 
						{
							curStr = gt.ToString(); 
						}
						catch(Exception ee) 
						{ 
							curStr = "error in timer.ToString(): " + gt.GetType().FullName + "; " + ee.ToString(); 
						}
						
						log.Warn("callback took "+(elapsed)+" ms! "+curStr);
					}
				}
			}
		}

		#endregion
	}
}
