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
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

using log4net;

namespace DOL.GS.Scheduler
{
	/// <summary>
	/// Simple Timer Based Scheduler Running Tasks Concurrently.
	/// </summary>
	public class SimpleScheduler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Retrieve Current Timestamp used by Scheduler
		/// </summary>
		public static long Ticks { get { return Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000); } }
		
		/// <summary>
		/// Internal Timer Object
		/// </summary>
		private Timer Timer { get; set; }
		
		/// <summary>
		/// Timers Pending for Schedule.
		/// </summary>
		private ConcurrentBag<Tuple<long, ScheduledTask>> PendingTimers { get; set; }
		
		/// <summary>
		/// Scheduled Timers Ordered by Due Time
		/// </summary>
		private SortedDictionary<long, List<ScheduledTask>> ScheduledTimers { get; set; }
		
		/// <summary>
		/// UnScheduled Tasks Currently Running.
		/// </summary>
		private List<Tuple<long, Task>> RunningTimers { get; set; }
		
		/// <summary>
		/// Collection Locking Object
		/// </summary>
		private readonly object SchedulerLock = new object();
		
		/// <summary>
		/// Next Timer Tick Due Time
		/// </summary>
		private long NextDueTick = long.MaxValue;
		
		/// <summary>
		/// Create a new Instance of <see cref="SimpleScheduler"/>
		/// </summary>
		public SimpleScheduler()
		{
			PendingTimers = new ConcurrentBag<Tuple<long, ScheduledTask>>();
			ScheduledTimers = new SortedDictionary<long, List<ScheduledTask>>();
			RunningTimers = new List<Tuple<long, Task>>();
			
			Timer = new Timer();
			Timer.Interval = 1;
			Timer.AutoReset = false;
			Timer.Elapsed += OnTick;
			Timer.Enabled = true;
		}
		
		/// <summary>
		/// Timer Event Starting All Due Tasks
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnTick(object source, ElapsedEventArgs e)
		{
			System.Threading.Interlocked.Exchange(ref NextDueTick, long.MaxValue);
			
			var nextDueTime = long.MaxValue;
			lock (SchedulerLock)
			{
				ResolveRunningTimers();
				ResolvePendingTimers();
				
				ConsumePendingScheduledTasks();
				
				// Schedule Next Tick
				var nextTask = ScheduledTimers.FirstOrDefault();
				if (nextTask.Value != null)
					nextDueTime = nextTask.Key;
			}
			
			SignalNextDueTick(nextDueTime);
		}
		
		/// <summary>
		/// Start All Tasks
		/// </summary>
		private void ConsumePendingScheduledTasks()
		{			
			var StartedTicksIndex = new List<long>();
			foreach (KeyValuePair<long, List<ScheduledTask>> tasks in ScheduledTimers.Where(ent => ent.Key <= Ticks))
			{
				StartedTicksIndex.Add(tasks.Key);
				foreach (ScheduledTask taskEntry in tasks.Value)
				{
					// scope copy for thread safety
					var timerTask = taskEntry;
					var task = Task.Factory.StartNew(() => LaunchScheduledTask(timerTask));
					RunningTimers.Add(new Tuple<long, Task>(Ticks, task));
				}
			}
			
			foreach (var key in StartedTicksIndex)
				ScheduledTimers.Remove(key);
		}
		
		/// <summary>
		/// Start a Task Method
		/// </summary>
		/// <param name="task"></param>
		private void LaunchScheduledTask(ScheduledTask task)
		{
			int delay = 0;
			var start = Ticks;
			try
			{
				delay = task.Run();
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Exception In Simple Scheduler Task: ", ex);
			}
			
			if (log.IsWarnEnabled)
			{
				var runningTime = Ticks - start;
				if (runningTime > 500)
					log.WarnFormat("Simple Scheduler Task (interval:{1}) execution took {0}ms! ({2}.{3})", runningTime, delay, task.Method.GetMethodInfo().ReflectedType, task.Method.GetMethodInfo().Name);
			}
			
			if (delay > 0 && task.Active)
				Start(task, delay);
		}
		
		/// <summary>
		/// Resolve Pending Timer to be Inserted in Scheduler
		/// </summary>
		private void ResolvePendingTimers()
		{
			while (!PendingTimers.IsEmpty)
			{
				Tuple<long, ScheduledTask> task;
				if (PendingTimers.TryTake(out task))
				{
					List<ScheduledTask> scheduled;
					if (!ScheduledTimers.TryGetValue(task.Item1, out scheduled))
					{
						scheduled = new List<ScheduledTask>();
						ScheduledTimers.Add(task.Item1, scheduled);
					}
					
					scheduled.Add(task.Item2);
				}
			}
		}
		
		/// <summary>
		/// Resolve Running Tasks and Clean Up Timers
		/// </summary>
		private void ResolveRunningTimers()
		{
			foreach(var task in RunningTimers.ToArray())
			{
				if (task.Item2.IsFaulted)
				{
					if (log.IsErrorEnabled)
						log.Error("Exception in Simple Scheduler Faulted Task: ", task.Item2.Exception);
					
					RunningTimers.Remove(task);
				}
				else if (task.Item2.IsCompleted || task.Item2.IsCanceled)
				{
					RunningTimers.Remove(task);
				}
			}
				
		}
		
		/// <summary>
		/// Compare Next Due Time to Value and Update Timer Interval accordingly
		/// </summary>
		/// <param name="value"></param>
		private void SignalNextDueTick(long value)
		{
			long initialValue;
			do
			{
				initialValue = System.Threading.Interlocked.Read(ref NextDueTick);
				if (value >= initialValue) return;
			}
			while (System.Threading.Interlocked.CompareExchange(ref NextDueTick, value, initialValue) != initialValue);

			Timer.Interval = Math.Min(int.MaxValue, Math.Max(1, value - Ticks));
		}
		
		#region Scheduler API
		/// <summary>
		/// Trigger a Scheduled Method to be run withing given delay
		/// </summary>
		/// <param name="method">Method to be scheduled, return next interval or 0 for single run</param>
		/// <param name="delay">delay in ms for the Method to be started</param>
		/// <returns>The Scheduled Task Object</returns>
		public ScheduledTask Start(Func<int> method, int delay)
		{
			if (delay < 1)
				throw new ArgumentException("Task Delay should be greater than 0.", "delay");
			
			var task = new ScheduledTask(method);
			
			Start(task, delay);
			
			return task;
		}
		
		/// <summary>
		/// Add or Re Schedule Task Object
		/// </summary>
		/// <param name="task">Scheduled Task Object to Add</param>
		/// <param name="delay">Delay before Scheduled Task is triggered</param>
		private void Start(ScheduledTask task, int delay)
		{
			var dueTime = Ticks + delay;
			PendingTimers.Add(new Tuple<long, ScheduledTask>(dueTime, task));
			
			SignalNextDueTick(dueTime);
		}
		
		/// <summary>
		/// Shutdown the Scheduler
		/// </summary>
		public void Shutdown()
		{
			System.Threading.Interlocked.Exchange(ref NextDueTick, long.MinValue);
			Timer.Interval = int.MaxValue;
			Timer.Enabled = false;
			
			foreach (var task in RunningTimers)
				task.Item2.Wait();
		}
		#endregion
	}

	/// <summary>
	/// Reference Class for Holding a Task Method
	/// </summary>
	public sealed class ScheduledTask
	{
		/// <summary>
		/// Task's Method Reference
		/// </summary>
		internal Func<int> Method { get; private set; }
		
		/// <summary>
		/// Lock Object Synchronzing Stopping Task
		/// </summary>
		private readonly object LockObject = new object();
		
		/// <summary>
		/// Task Active Flag
		/// </summary>
		private bool m_active = true;
		
		/// <summary>
		/// Is This Task still Active ?
		/// </summary>
		public bool Active
		{
			get
			{
				lock (LockObject)
					return m_active;
			}
		}
		
		/// <summary>
		/// Create a new Instance of <see cref="ScheduledTask"/>
		/// </summary>
		/// <param name="Method"></param>
		internal ScheduledTask(Func<int> Method)
		{
			this.Method = Method;
		}
		
		/// <summary>
		/// Stop and Unschedule Task
		/// </summary>
		public void Stop()
		{
			lock(LockObject)
				m_active = false;
		}
		
		/// <summary>
		/// Run Task Synchronously Locked
		/// </summary>
		/// <returns></returns>
		internal int Run()
		{
			var result = 0;
			lock(LockObject)
			{
				try
				{
					if (Active)
						result = Method();
				}
				finally
				{
					m_active &= result > 0;
				}
			}
			
			return result;
		}
	}
}
