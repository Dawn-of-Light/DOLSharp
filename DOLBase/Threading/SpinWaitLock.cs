using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DOL
{
	/// <summary>
	/// An efficient spin-wait lock implementation.
	/// </summary>
	/// <remarks>
	/// <para>This is a value type so it works very efficiently when used as a field in a class.</para>
	/// <para>Avoid boxing or you will lose thread safety.</para>
	/// <para>This structure is based on Jeffrey Richter's article "Concurrent Affairs" in the October 2005 issue of MSDN Magazine.</para>
	/// </remarks>
	public struct SpinWaitLock
	{
		private const int LockFree = 0;
		private const int LockOwned = 1;
		private static readonly bool IsSingleCpuMachine = (Environment.ProcessorCount == 1);
		private int _lockState; // Defaults to 0=LockFree

		public void Enter()
		{
			Thread.BeginCriticalRegion();

			while (true)
			{
				// If resource available, set it to in-use and return
				if (Interlocked.Exchange(ref _lockState, LockOwned) == LockFree)
					return;

				// Efficiently spin, until the resource looks like it might 
				// be free. NOTE: Just reading here (as compared to repeatedly 
				// calling Exchange) improves performance because writing 
				// forces all CPUs to update this value
				while (Thread.VolatileRead(ref _lockState) == LockOwned)
				{
					StallThread();
				}
			}
		}

		public void Exit()
		{
			// Mark the resource as available
			Interlocked.Exchange(ref _lockState, LockFree);
			Thread.EndCriticalRegion();
		}

#if LINUX
        private static void StallThread()
        {
            //Linux doesn't support SwitchToThread()
            Thread.SpinWait(1);
        }
#else
		private static void StallThread()
		{
			if (IsSingleCpuMachine)
			{
				// On a single-CPU system, spinning does no good
				SwitchToThread();
			}
			else
			{
				// Multi-CPU system might be hyper-threaded, let other thread run
				Thread.SpinWait(1);
			}
		}

		[DllImport("kernel32", ExactSpelling = true)]
		private static extern void SwitchToThread();
#endif
	}
}