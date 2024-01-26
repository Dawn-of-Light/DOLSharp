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

using NUnit.Framework;

using DOL.GS.Scheduler;

namespace DOL.Integration.Utils
{
	/// <summary>
	/// DOL Simple Scheduler Unit Test.
	/// </summary>
	[TestFixture]
	public class SchedulerTest
	{
		public SchedulerTest()
		{
		}
		
		[OneTimeSetUp]
		public void SetUp()
		{
			log4net.Config.BasicConfigurator.Configure(
				new log4net.Appender.ConsoleAppender {
					Layout = new log4net.Layout.SimpleLayout()});
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			log4net.LogManager.Shutdown();
		}
		
		[Test]
		public void Scheduler_AddTimer_RetrieveTask()
		{
			var scheduler = new SimpleScheduler();
			
			var task = scheduler.Start(() => 0, 10);
			
			Assert.That(task.Active, Is.True, "Task should be active after Scheduler Insertion...");
		}
		
		[Test]
		public void Scheduler_StopTimer_TaskInactive()
		{
			var scheduler = new SimpleScheduler();
			
			var task = scheduler.Start(() => 1, 1);
			
			task.Stop();
			
			Assert.That(task.Active, Is.False, "Task should be inactive after Scheduler Stop...");
		}
		
		[Test]
		public void Scheduler_StopTimerLatency_TaskNotRun()
		{
			var scheduler = new SimpleScheduler();
			
			var run = false;
			var task = scheduler.Start(() => { run = true; return 0; }, 100);
			
			System.Threading.Thread.Sleep(50);
			
			task.Stop();
			
			System.Threading.Thread.Sleep(100);
			
			Assert.That(run, Is.False, "Task should not have run when Stopping before delay...");
		}
		
		[Test]
		public void Scheduler_StopTimerLatency_TaskRun()
		{
			var scheduler = new SimpleScheduler();
			
			var run = false;
			var task = scheduler.Start(() => { run = true; return 0; }, 100);
			
			System.Threading.Thread.Sleep(150);
			
			task.Stop();
			
			Assert.That(run, Is.True, "Task should have run when Stopping after delay...");
		}
		
		[Test]
		public void Scheduler_StartIntervalTimer_TaskRunMultipleTime()
		{
			var scheduler = new SimpleScheduler();
			
			int count = 0;
			var task = scheduler.Start(() => { count++; return 1; }, 1);
			
			System.Threading.Thread.Sleep(100);
			
			var intermediate = count;
			Assert.That(intermediate, Is.GreaterThan(0), "Task Should have been Scheduled multiple time with an Interval of 1ms...");
			
			System.Threading.Thread.Sleep(100);
			task.Stop();

			Assert.That(count, Is.GreaterThan(intermediate), "Task should have been scheduled again before stopping...");
		}
		
		[Test]
		public void Scheduler_StartTimerOnce_TaskRunOnce()
		{
			var scheduler = new SimpleScheduler();
			
			int count = 0;
			var task = scheduler.Start(() => { count++; return 0; }, 1);
			
			System.Threading.Thread.Sleep(100);
			
			Assert.That(1, Is.EqualTo(count), "Task Should have been Scheduled once with no Interval...");
			
			Assert.That(task.Active, Is.False, "Task Should be inactive after one Scheduling...");
		}
		
		[Test]
		public void Scheduler_IntervalTimerFixedCount_CountRun()
		{
			var scheduler = new SimpleScheduler();
			
			int count = 0;
			var task = scheduler.Start(() => { count++; return count == 10 ? 0 : 1; }, 1);
			
			System.Threading.Thread.Sleep(200);
			
			Assert.That(count, Is.EqualTo(10), "Task Should have been Scheduled 10 times...");
			Assert.That(task.Active, Is.False, "Task should be Inactive after Fixed Schedule count...");
		}
		
		[Test]
		public void Scheduler_TimerStartLatency_LowerThan()
		{
			var scheduler = new SimpleScheduler();
			
			var start = SimpleScheduler.Ticks;
			var finished = long.MaxValue;
			var task = scheduler.Start(() => { finished = SimpleScheduler.Ticks; return 0; }, 1);
			
			System.Threading.Thread.Sleep(100);
			
			Assert.That(finished - start, Is.LessThan(100), "Scheduler Task Latency is higher than 100ms!");
			Assert.That(task.Active, Is.False, "Task Should be inactive after Scheduling...");
		}
		
		[Test, Explicit]
		public void Scheduler_MultiInsertStopTest_AllTasksFinished()
		{
			var scheduler = new SimpleScheduler();
			
			// Long Interval Tasks
			var longTasks = Enumerable.Range(0, 10).Select(i => scheduler.Start(() => 0, 1000)).ToArray();

			System.Threading.Thread.Sleep(100);
			
			// Short Interval Tasks
			var shortTasks = Enumerable.Range(0, 10).Select(i => scheduler.Start(() => 0, 1)).ToArray();
			
			System.Threading.Thread.Sleep(100);
			
			Assert.That(shortTasks.Select(t => t.Active), Is.EqualTo(Enumerable.Range(0, 10).Select(i => false)));
			
			System.Threading.Thread.Sleep(900);
			
			Assert.That(longTasks.Select(t => t.Active), Is.EqualTo(Enumerable.Range(0, 10).Select(i => false)));
		}
		
		[Test, Explicit]
		public void Scheduler_MultiInsertInterval_FixedCountRun()
		{
			var scheduler = new SimpleScheduler();
			
			// Long Interval Tasks
			var longCount = Enumerable.Range(0, 10).Select(i => 0).ToArray();
			var longTasks = Enumerable.Range(0, 10).Select(i => scheduler.Start(() => { longCount[i]++; return longCount[i] == 5 ? 0 : 500; }, 500)).ToArray();

			System.Threading.Thread.Sleep(100);
			
			// Short Interval Tasks
			var shortCount = Enumerable.Range(0, 10).Select(i => 0).ToArray();
			var shortTasks = Enumerable.Range(0, 10).Select(i => scheduler.Start(() => { shortCount[i]++; return shortCount[i] == 10 ? 0 : 100; }, 1)).ToArray();
			
			System.Threading.Thread.Sleep(1100);
			
			Assert.That(shortCount, Is.EqualTo(Enumerable.Range(0, 10).Select(i => 10)));
			Assert.That(shortTasks.Select(t => t.Active), Is.EqualTo(Enumerable.Range(0, 10).Select(i => false)));
			
			System.Threading.Thread.Sleep(1900);
			
			Assert.That(longCount, Is.EqualTo(Enumerable.Range(0, 10).Select(i => 5)));
			Assert.That(longTasks.Select(t => t.Active), Is.EqualTo(Enumerable.Range(0, 10).Select(i => false)));
		}

		[Test, Explicit]
		public void Scheduler_TimerLongRuntime_LogDisplay()
		{
			var scheduler = new SimpleScheduler();
			
			var task = scheduler.Start(() => { System.Threading.Thread.Sleep(600); return 0; }, 1);
			
			System.Threading.Thread.Sleep(1000);
			
			Assert.That(task.Active, Is.False, "Task should have been scheduled to Test Exception...");
		}
		
		[Test]
		public void Scheduler_TimerException_LogDisplay()
		{
			var scheduler = new SimpleScheduler();
			
			var task = scheduler.Start(() => { throw new Exception(); }, 1);
			
			System.Threading.Thread.Sleep(100);
			
			Assert.That(task.Active, Is.False, "Task should have been scheduled to Test Exception...");
		}
	}
}
